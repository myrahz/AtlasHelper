using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.FilesInMemory;
using ExileCore.PoEMemory.FilesInMemory.Atlas;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using GameOffsets;
using GameOffsets.Components;
using ImGuiNET;
using SharpDX;
using Map = ExileCore.PoEMemory.Components.Map;

namespace AtlasHelper
{
    public class AtlasHelper : BaseSettingsPlugin<AtlasHelperSettings>
    {
        

        private List<String> linesIgnoreMaps = new List<String>();
        private Color[] SelectColors;

        private List<(WorldArea, int, bool)> BonusAreasBaseTiers = new List<(WorldArea, int, bool)>();

        private List<AtlasMap> ListAtlasMaps = new List<AtlasMap>();
        private List<AtlasMap> ListAtlasMapsCompleteable = new List<AtlasMap>();
        private int highestCompletedTier = 0;

        private IList<WorldArea> bonusComp;
        

        private List<(AtlasMap,int, RectangleF)> finalMapsToRun = new List<(AtlasMap,int,RectangleF)>();
        private bool willGiveCompletion = false;

        public override bool Initialise()
        {
            #region Colors

            SelectColors = new[]
            {
                Color.Aqua,
                Color.Blue,
                Color.BlueViolet,
                Color.Brown,
                Color.BurlyWood,
                Color.CadetBlue,
                Color.Chartreuse,
                Color.Chocolate,
                Color.Coral,
                Color.CornflowerBlue,
                Color.Cornsilk,
                Color.Crimson,
                Color.Cyan,
                Color.DarkBlue,
                Color.DarkCyan,
                Color.DarkGoldenrod,
                Color.DarkGray,
                Color.DarkGreen,
                Color.DarkKhaki,
                Color.DarkMagenta,
                Color.DarkOliveGreen,
                Color.DarkOrange,
                Color.DarkOrchid,
                Color.DarkRed,
                Color.DarkSalmon,
                Color.DarkSeaGreen,
                Color.DarkSlateBlue,
                Color.DarkSlateGray,
                Color.DarkTurquoise,
                Color.DarkViolet,
                Color.DeepPink,
                Color.DeepSkyBlue,
                Color.DimGray,
                Color.DodgerBlue,
                Color.Firebrick,
                Color.FloralWhite,
                Color.ForestGreen,
                Color.Fuchsia,
                Color.Gainsboro,
                Color.GhostWhite,
                Color.Gold,
                Color.Goldenrod,
                Color.Gray,
                Color.Green,
                Color.GreenYellow,
                Color.Honeydew,
                Color.HotPink,
                Color.IndianRed,
                Color.Indigo,
                Color.Ivory,
                Color.Khaki,
                Color.Lavender,
                Color.LavenderBlush,
                Color.LawnGreen,
                Color.LemonChiffon,
                Color.LightBlue,
                Color.LightCoral,
                Color.LightCyan,
                Color.LightGoldenrodYellow,
                Color.LightGray,
                Color.LightGreen,
                Color.LightPink,
                Color.LightSalmon,
                Color.LightSeaGreen,
                Color.LightSkyBlue,
                Color.LightSlateGray,
                Color.LightSteelBlue,
                Color.LightYellow,
                Color.Lime,
                Color.LimeGreen,
                Color.Linen,
                Color.Magenta,
                Color.Maroon,
                Color.MediumAquamarine,
                Color.MediumBlue,
                Color.MediumOrchid,
                Color.MediumPurple,
                Color.MediumSeaGreen,
                Color.MediumSlateBlue,
                Color.MediumSpringGreen,
                Color.MediumTurquoise,
                Color.MediumVioletRed,
                Color.MidnightBlue,
                Color.MintCream,
                Color.MistyRose,
                Color.Moccasin,
                Color.NavajoWhite,
                Color.Navy,
                Color.OldLace,
                Color.Olive,
                Color.OliveDrab,
                Color.Orange,
                Color.OrangeRed,
                Color.Orchid,
                Color.PaleGoldenrod,
                Color.PaleGreen,
                Color.PaleTurquoise,
                Color.PaleVioletRed,
                Color.PapayaWhip,
                Color.PeachPuff,
                Color.Peru,
                Color.Pink,
                Color.Plum,
                Color.PowderBlue,
                Color.Purple,
                Color.Red,
                Color.RosyBrown,
                Color.RoyalBlue,
                Color.SaddleBrown,
                Color.Salmon,
                Color.SandyBrown,
                Color.SeaGreen,
                Color.SeaShell,
                Color.Sienna,
                Color.Silver,
                Color.SkyBlue,
                Color.SlateBlue,
                Color.SlateGray,
                Color.Snow,
                Color.SpringGreen,
                Color.SteelBlue,
                Color.Tan,
                Color.Teal,
                Color.Thistle,
                Color.Tomato,
                Color.Transparent,
                Color.Turquoise,
                Color.Violet,
                Color.Wheat,
                Color.White,
                Color.WhiteSmoke,
                Color.Yellow,
                Color.YellowGreen
            };

            #endregion

            var initImage = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "ImagesAtlas.png"), false);
            var initImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "AtlasMapCross.png"), false);
            var vaalCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "vaal.png"), false);
            var transmuteImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "transmute.png"), false);
            var augmentImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "augment.png"), false);
            var alcImageCross = Graphics.InitImage(Path.Combine(DirectoryFullName, "images", "alc.png"), false);            
            linesIgnoreMaps = File.ReadAllLines(Path.Combine(DirectoryFullName, "images", "ignoreMaps.txt")).ToList();
            
            GetAtlasNodes();
            if (!initImage)
                return false;

            Input.RegisterKey(Keys.LControlKey);

            return true;
        }
        static bool IsPointInsideRectangle(RectangleF rectangle, double x, double y)
        {
            return x >= rectangle.Left && x <= rectangle.Right &&
                   y >= rectangle.Top && y <= rectangle.Bottom;
        }

        public override void AreaChange(AreaInstance area)
        {
            GetAtlasNodes();
            //refreshCompletedAreas();


        }


        public override void Render()
        {

            
            
            // test highest completed tier
            if (Settings.Debug)
            {
                LogMessage($"Highest completed tier: {highestCompletedTier}", 5, Color.LightBlue);
                LogMessage($"Number of atlas maps that award completion: {ListAtlasMaps.Count(atlasMap => atlasMap.AdjacentMaps.Count > 0)}", 5, Color.LightBlue);
            }
                
            checkMapIgnore();
            DrawPlayerInvMaps();
            DrawNpcInvMaps();
            //DrawNpcInvMaps();
            //DrawNpcInvMapsAlt();
            DrawDiagonalProgression();
            
        }

        private void DrawDiagonalProgression()
        {

            var ingameState = GameController.Game.IngameState;
            finalMapsToRun.Clear();

            if (ingameState.IngameUi.InventoryPanel.IsVisible)
            {

                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;

                if (ingameState.IngameUi.StashElement.IsVisible && ingameState.IngameUi.StashElement.VisibleStash != null   )
                {

                    foreach (var normalStashItem in ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems )
                    {
                        inventoryZone.Insert(inventoryZone.Count, normalStashItem);
                    }
                }

                

                var mapsThatGiveCompletion = new List<(AtlasMap,int, RectangleF)>();
                var mapsThatWontGiveCompletion = new List<(AtlasMap,int, RectangleF)>();

                

                // if highest tier < 3          

                foreach (var item in inventoryZone)
                {
                    var entity = item?.Item;

                    if (entity == null) continue;
                    var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);
                        
                    if (bit == null) continue;
                    if (bit.ClassName != "Map" && bit.ClassName != "Maps") continue;
                    var mapComponent = entity.GetComponent<Map>();
                    var modsComponent = entity.GetComponent<Mods>();
                    var baseComponent = entity.GetComponent<Base>();

                    var rarity = modsComponent.ItemRarity;
                    var corrupted = baseComponent.isCorrupted;
                    var drawRect = item.GetClientRect();
                    var uniqueName = modsComponent.UniqueName;
                    var area = mapComponent.Area;
                    var tier = mapComponent.Tier;
                    if ((!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == uniqueName) && rarity == ItemRarity.Unique) || (!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == area.Name) && rarity < ItemRarity.Unique))
                    {
                        continue;
                    }



                    drawRect.Top += 20;
                    drawRect.Bottom -= 0;
                    drawRect.Right -= 20;
                    drawRect.Left += 0;





         
                    
                    var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == area.Name).BaseTier;

                    //if (comp.Contains(area))
                    //    completed++;

                    if( naturalTier == 17)
                    {
                        continue;
                    }
                    if (linesIgnoreMaps.Contains(area.ToString()))
                    {

                        continue;

                    }

                    if (mapCanGiveCompletion(item))
                    {
                        if(rarity != ItemRarity.Unique)
                        {
                            mapsThatGiveCompletion.Add((ListAtlasMaps.Where(x => x.AdjacentMaps.Count() > 0).FirstOrDefault(x => x.WorldArea.Name == area.Name), tier, drawRect));
                        }
                        else
                        {
                            mapsThatGiveCompletion.Add((ListAtlasMaps.Where(x => x.AdjacentMaps.Count() > 0).FirstOrDefault(x => x.WorldArea.Name == uniqueName), tier, drawRect));
                        }

                        
                        
                    }
                    else
                    {
                        
                        if (rarity != ItemRarity.Unique)
                        {
                            mapsThatWontGiveCompletion.Add((ListAtlasMaps.Where(x => x.AdjacentMaps.Count() > 0).FirstOrDefault(x => x.WorldArea.Name == area.Name), tier, drawRect));
                        }
                        else
                        {
                            mapsThatWontGiveCompletion.Add((ListAtlasMaps.Where(x => x.AdjacentMaps.Count() > 0).FirstOrDefault(x => x.WorldArea.Name == uniqueName), tier, drawRect));
                        }
                    }
                    

                }

                // iterar maps que dão completion e os que não dão

                if (Settings.Debug)
                {
                    foreach (var aux in mapsThatWontGiveCompletion)
                    {
                        LogMessage("Wont give completion: " + aux.Item1.WorldArea.Name + " of tier : " + aux.Item2, 5, Settings.IgnoredMaps);
                    }

                    foreach (var aux in mapsThatGiveCompletion)
                    {
                        LogMessage("WILL GIVE completion: " + aux.Item1.WorldArea.Name + " of tier : " + aux.Item2, 5, Settings.UncompletedMaps);
                    }
                }

                
                var mapsToRunCompletion = mapsThatGiveCompletion.Where(x => x.Item2 <= highestCompletedTier - 2).OrderByDescending(x => x.Item2);
                var mapsToRunNoCompletion = mapsThatWontGiveCompletion.Where(x => x.Item2 <= highestCompletedTier - 2).OrderByDescending(x => x.Item2);
          

                if (highestCompletedTier >= 3)
                {

                    if (mapsToRunCompletion.Count() > 0) // best scenario, we have uncompleted maps that are at least 2 tiers below
                    {

                        // first criteria, highest tier of map, so filter everythign by the highest tier inside that list
                        // second criteria, do the map that has the highest sum of uncompleted map tiers

                        int highestTier = mapsToRunCompletion.Max(map => map.Item2);
                        var highestTierMaps = mapsToRunCompletion.Where(map => map.Item2 == highestTier).ToList();

                        int maxSum = highestTierMaps.Max(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps());
                        finalMapsToRun = highestTierMaps.Where(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps() == maxSum).ToList();
                        willGiveCompletion = true;

                    }
                    else if (mapsThatGiveCompletion.Count() > 0)  // second best scenario, we have uncompleted maps 
                    {

                        int highestTier = mapsThatGiveCompletion.Max(map => map.Item2);
                        var highestTierMaps = mapsThatGiveCompletion.Where(map => map.Item2 == highestTier).ToList();

                        int maxSum = highestTierMaps.Max(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps());
                        finalMapsToRun = highestTierMaps.Where(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps() == maxSum).ToList();
                        willGiveCompletion = true;

                    } 
                    else if (mapsThatWontGiveCompletion.Count() > 0) // fourth best scenario, we have completed maps run the highest that have the most uncompleted maps adjacent
                    {


                        int highestTier = mapsThatWontGiveCompletion.Max(map => map.Item2);
                        var highestTierMaps = mapsThatWontGiveCompletion.Where(map => map.Item2 == highestTier).ToList();

                        int maxSum = highestTierMaps.Max(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps());
                        finalMapsToRun = highestTierMaps.Where(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps() == maxSum).ToList();
                        willGiveCompletion = false;
                    }
                }
                else
                {
                    if (mapsThatGiveCompletion.Count() > 0)
                    {

                        int highestBaseTier = mapsThatGiveCompletion.Max(map => map.Item1.BaseTier);
                        var highestBaseTierMaps = mapsThatGiveCompletion.Where(map => map.Item1.BaseTier == highestBaseTier).ToList();

                        int maxSum = highestBaseTierMaps.Max(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps());
                        finalMapsToRun = highestBaseTierMaps.Where(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps() == maxSum).ToList();
                        willGiveCompletion = true;
                    }
                    else if (mapsThatWontGiveCompletion.Count() > 0)
                    {

                        int highestBaseTier = mapsThatWontGiveCompletion.Max(map => map.Item1.BaseTier);
                        var highestBaseTierMaps = mapsThatWontGiveCompletion.Where(map => map.Item1.BaseTier == highestBaseTier).ToList();

                        int maxSum = highestBaseTierMaps.Max(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps());
                        finalMapsToRun = highestBaseTierMaps.Where(map => map.Item1.SumOfBaseTiersOfUncompletedAdjacentMaps() == maxSum).ToList();
                        willGiveCompletion = false;
                    }

                }


                


            }
        }

        private bool mapCanGiveCompletion(NormalInventoryItem mapNormalInventoryItem)
        {
            var entity = mapNormalInventoryItem?.Item;
            var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);
            

            if (bit == null) return false; ;
            if (bit.ClassName != "Map" && bit.ClassName != "Maps") return false; ;
            var mapComponent = entity.GetComponent<Map>();
            var modsComponent = entity.GetComponent<Mods>();
            var baseComponent = entity.GetComponent<Base>();
            var rarity = modsComponent.ItemRarity;
            var corrupted = baseComponent.isCorrupted;
            var uniqueName = modsComponent.UniqueName;
            var area = mapComponent.Area;
            var tier = mapComponent.Tier;
            var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == area.Name).BaseTier;
            var neighbours = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == area.Name).AdjacentMaps.Count();

            if ((!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == uniqueName) && rarity == ItemRarity.Unique) || (!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == area.Name) && rarity < ItemRarity.Unique))

            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + uniqueName + "|" + area + " | " + rarity + " wont give completion because it isn't even present on the atlas", 5, Settings.IgnoredMaps);
                }

                return false;

            }
            else if (bonusComp.Contains(area) && rarity != ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because it is already present on the completed areas", 5, Settings.IgnoredMaps);
                }

                return false;

            }
            else if (bonusComp.Any(x => x.Name == uniqueName) && rarity == ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Unique Map " + uniqueName + "|" + area + " rarity " + rarity + " wont give completion because it is already present on the completed areas", 5, Settings.IgnoredMaps);
                }

                return false;

            }
            else if (naturalTier > 5 && corrupted && rarity != ItemRarity.Rare)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because it is already corrupted, it is not rare and its natural tier is bigger than 10", 5, Settings.IgnoredMaps);
                }
                return false;
            }
            else if (corrupted && (rarity != ItemRarity.Rare && rarity != ItemRarity.Magic))
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because it is already corrupted, it is not rare/magic", 5, Settings.IgnoredMaps);
                }
                return false;
            }
            else if (!bonusComp.Any(x => x.Name == area.Name) && rarity < ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + uniqueName + "|" + area + " | " + rarity + " will give completion ", 5, Settings.UncompletedMaps);
                }
                return true;
            }
            else if (!bonusComp.Any(x => x.Name == uniqueName) && rarity == ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("UNIQUE Map " + uniqueName + " | " + area + " | " + rarity + " will give completion ", 5, Settings.UncompletedMaps);
                }
                return true;
            }
            else
            {
                if (Settings.Debug)
                {
                    LogMessage("Couldnt capture " + uniqueName + "|" + area + " | " + rarity + "  ", 5, Color.Orange);
                }
                return false;
            }


        }

        private bool npcInvMapCanGiveCompletion(ServerInventory.InventSlotItem npcInventSlotItem)
        {
            var entity = npcInventSlotItem?.Item;
            var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);


            if (bit == null) return false; ;
            if (bit.ClassName != "Map" && bit.ClassName != "Maps") return false; ;
            var mapComponent = entity.GetComponent<Map>();
            var modsComponent = entity.GetComponent<Mods>();
            var baseComponent = entity.GetComponent<Base>();
            var rarity = modsComponent.ItemRarity;
            var corrupted = baseComponent.isCorrupted;
            var area = mapComponent.Area;
            var uniqueName = modsComponent.UniqueName;
            var tier = mapComponent.Tier;
            var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == area.Name).BaseTier;

            if ((!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == uniqueName) && rarity == ItemRarity.Unique) || (!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == area.Name) && rarity < ItemRarity.Unique))

            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + uniqueName + "|" + area + " | " + rarity + " wont give completion because it isn't even present on the atlas", 5, Settings.IgnoredMaps);
                }

                return false;

            }
            else if (bonusComp.Contains(area) && rarity != ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because it is already present on the completed areas", 5, Settings.IgnoredMaps);
                }

                return false;

            }
            else if (bonusComp.Any(x => x.Name == uniqueName) && rarity == ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Unique Map " + uniqueName + "|" + area + " rarity " + rarity + " wont give completion because it is already present on the completed areas", 5, Settings.IgnoredMaps);
                }

                return false;

            }
            else if (naturalTier > 5 && corrupted && rarity != ItemRarity.Rare)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because it is already corrupted, it is not rare and its natural tier is bigger than 10", 5, Settings.IgnoredMaps);
                }
                return false;
            }
            else if (corrupted && (rarity != ItemRarity.Rare && rarity != ItemRarity.Magic))
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because it is already corrupted, it is not rare/magic", 5, Settings.IgnoredMaps);
                }
                return false;
            }
            else if (!corrupted && naturalTier > 10 && rarity < ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because its not corrupted and its natural tier is bigger than 10", 5, Settings.IgnoredMaps);
                }
                return false;
            }
            else if (!bonusComp.Any(x => x.Name == area.Name) && rarity < ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + uniqueName + "|" + area + " | " + rarity + " will give completion ", 5, Settings.UncompletedMaps);
                }
                return true;
            }
            else if (!bonusComp.Any(x => x.Name == uniqueName) && rarity == ItemRarity.Unique)
            {
                if (Settings.Debug)
                {
                    LogMessage("UNIQUE Map " + uniqueName + " | " + area + " | " + rarity + " will give completion ", 5, Settings.UncompletedMaps);
                }
                return true;
            }
            else
            {
                if (Settings.Debug)
                {
                    LogMessage("Couldnt capture " + uniqueName + "|" + area + " | " + rarity + "  ", 5, Color.Orange);
                }
                return false;
            }


        }

        private List<long> ReadDatPtr2(DatArrayStruct array, IMemory memory)
        {
            return (from x in array.ReadDat<(long, int)>(memory, 8)
                    select x.Item1).ToList();
        }
        private void GetAtlasNodes()
        {
            var ingameState = GameController.Game.IngameState;
            var atlasNodes = ingameState.TheGame.Files.AtlasNodes.EntriesList;
            var serverData = ingameState.ServerData;
            bonusComp = serverData.BonusCompletedAreas;
            ListAtlasMaps.Clear();
            BonusAreasBaseTiers.Clear();
            //bonusCompWithoutUniques = serverData.BonusCompletedAreas.Where(x => !x.Id.Contains("Unique"));



            foreach (var node in atlasNodes)
            {

                var baseAtlasNodeAddress = node.Address;
                //var tier0 = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x51);
                var tier0 = node.TierProgression[0];
                

                var AtlasNodeKeys = ingameState.M.Read<DatArrayStruct>(baseAtlasNodeAddress + 0x41);
               
                //var numberOfNeighbours = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x41);
                var numberOfNeighbours = node.Connections.Count();
                var isNormalMap = numberOfNeighbours > 1;

                
                BonusAreasBaseTiers.Add((node.Area, tier0,isNormalMap));
                var AtlasNodeKeysStruct = ReadDatPtr2(AtlasNodeKeys, ingameState.M);

                //LogMessage(node.Area.Name + " " + node.Area.Index + " : " + AtlasNodeKeysStruct.Count(), 5, Color.Yellow);
                List<AtlasMap> adjacentMaps = new List<AtlasMap>();
                foreach (var aux in AtlasNodeKeysStruct)
                {
                    //LogMessage(ingameState.TheGame.Files.AtlasNodes.GetByAddress(aux).Area.Name, 5, Color.Yellow);
                    var auxArea = ingameState.TheGame.Files.AtlasNodes.GetByAddress(aux).Area;
                    bool isCompletedAux = bonusComp.Contains(auxArea);
                    var baseAtlasNodeAddressAux = atlasNodes.FirstOrDefault(x => x.Area.Name == auxArea.Name).Address;
                    var tier0Aux = ingameState.M.Read<int>(baseAtlasNodeAddressAux + 0x51);
                    adjacentMaps.Add(new AtlasMap(auxArea, tier0Aux, adjacentMaps, isCompletedAux));
                }

                bool isCompleted = bonusComp.Contains(node.Area);
                //if (numberOfNeighbours > 0)
                //{
                    ListAtlasMaps.Add(new AtlasMap(node.Area, tier0, adjacentMaps, isCompleted));
                    
                //}
                    



            }

            ListAtlasMapsCompleteable = ListAtlasMaps.Where(atlasMap => atlasMap.AdjacentMaps.Count > 0).ToList();



            var filteredBonus = BonusAreasBaseTiers.Where(x => x.Item3 == true && bonusComp.Contains(x.Item1)).ToList(); // item 3 indicates if a map is normal, which means if it isnt a unique or a special map like Shaper or T17
            
            if (Settings.SimulateHighestCompletedTier)
            {
                highestCompletedTier = Settings.HighestCompletedTierSim;
            }
            else
            {
                if(filteredBonus.Count() > 0)
                highestCompletedTier = filteredBonus.Max(x => x.Item2);
            }
        }
        private void checkMapIgnore()
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;

            //if (linesIgnoreMaps.Contains(GameController.IngameState.Data.CurrentArea.Name))
            if (linesIgnoreMaps.Contains(GameController.IngameState.Data.CurrentArea.Name) && !bonusComp.Any(x => x.Name == GameController.IngameState.Data.CurrentArea.Name))
            {
                Vector2 newInfoPanel2 = new Vector2(822, 722);
                var drawBox2 = new RectangleF(newInfoPanel2.X, newInfoPanel2.Y, 350, 60);
                if (Settings.WarnInsideMapToNotComplete) { 
                Graphics.DrawBox(drawBox2, Color.Red, 5);
                Graphics.DrawText("DONT FINISH MAP", newInfoPanel2, Color.White, 30);
                }
            }
        }

        private void DrawPlayerInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            if (ingameState.IngameUi.InventoryPanel.IsVisible)
            {
                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
                //var inventoryZone = ingameState.IngameUi.InventoryPanel[18]. ;
                
                if (ingameState.IngameUi.StashElement.IsVisible && ingameState.IngameUi.StashElement.VisibleStash != null) // stash is open
                {

                    foreach (var normalStashItem in ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems)
                    {
                        inventoryZone.Insert(inventoryZone.Count, normalStashItem);
                    }
                }
                if (ingameState.IngameUi.StashElement.IsVisible && ingameState.IngameUi.StashElement.VisibleStash != null) // mini map stash
                {

                    foreach (var normalStashItem in ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems)
                    {
                        inventoryZone.Insert(inventoryZone.Count, normalStashItem);
                    }
                }
                HiglightAllMaps(inventoryZone);
            }
        }

        private List<string> GetFilteredCompletableItems(IList<NormalInventoryItem> items, Func<NormalInventoryItem, bool> condition)
        {

            return items.Where(condition).Select(item => item.Item.GetComponent<Map>().Area.Name).ToList();
        }

        private void DrawNpcInvMapsOld()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
            var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            List<string> MapAreasInBag = GetFilteredCompletableItems(inventoryZone, mapCanGiveCompletion);



            foreach (var inv in npcInv)
            {

                if (inv.Inventory.Rows == 1)
                {//kirac mission++
                    if (GameController.Game.IngameState.IngameUi.ZanaMissionChoice.IsVisible)
                    {

                        var KiracPanel = GameController.Game.IngameState.IngameUi.ZanaMissionChoice;
                        
                        //var inventory = KiracPanel.GetChildFromIndices(0, 3);
                        var inventory = KiracPanel.GetChildFromIndices(0, 3, 0, 0);
                        //LogMessage("Children indice 0 : " + KiracPanel.GetChildFromIndices(0).ChildCount.ToString());
                        //LogMessage("Children indice 0,3 : " + inventory.ChildCount.ToString());

                        var auxMap = KiracPanel?.GetChildFromIndices(0, 3);
                        var auxMap2 = KiracPanel?.GetChildFromIndices(0, 3, 0);
                        var auxMap3 = KiracPanel?.GetChildFromIndices(0, 3, 0, 0);

                        
                        var zanaMisionAux = auxMap.GetChildrenAs<ExileCore.PoEMemory.Element>().ToList() ?? new List<ExileCore.PoEMemory.Element>();
          
                        var firstVisible = zanaMisionAux.FirstOrDefault(x => x.IsVisible);
                        if (firstVisible == null)
                            continue;
                        var lastVisible = zanaMisionAux.Last(x => x.IsVisible);
                        var firstVisibleIndex = firstVisible.IndexInParent;
                        var lastVisibleIndex = lastVisible.IndexInParent;
                        
                        // get the list of maps in bag, check if they are completaable
                        
                        foreach (var item in inv.Inventory.InventorySlotItems)
                        {

                            var mapComponent = item.Item.GetComponent<Map>();
                            var mapGridX = item.PosX;
         
                            if (mapComponent == null)
                                continue;

                            var drawRect = item.GetClientRect();
                            var mapArea = mapComponent.Area;
                            var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == mapArea.Name).BaseTier;


                            var mapRarity = item.Item.GetComponent<Mods>().ItemRarity;


                            if (MapAreasInBag.Contains(mapArea.Name))
                            {
                                if (Settings.Debug)
                                {
                                    LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                                }
                                continue;
                            }

                            if (!npcInvMapCanGiveCompletion(item))
                            {
                                continue;
                            }

                            if (mapRarity != ItemRarity.Unique)
                            {

                                if (bonusComp.Contains(mapArea)) continue; // check item quality
                            }
                            else

                            {
                                var mapUniqueName = item.Item.GetComponent<Mods>().UniqueName;

                                if (bonusComp.Any(r => r.Name == mapUniqueName)) continue;

                            }

                            ///-

                            var color = Color.White;

                            if (mapComponent.Tier > 10)
                            {
                                color = Color.Red;
                            }
                            else if (mapComponent.Tier > 5)
                            {
                                color = Color.Yellow;
                            }

                            var ignoreCompletion = false;

                            if (linesIgnoreMaps.Contains(mapArea.ToString()))
                            {
                                ignoreCompletion = true;

                            }

                            var auxindex = (int)item.InventoryPosition.X;
                            var drawRect2 = KiracPanel.GetChildFromIndices(0, 3).GetChildAtIndex(auxindex).GetClientRect();

                            var stringtoDraw = mapArea.Name;
                            if (ignoreCompletion)
                                
                                stringtoDraw += " --- IGNORED MAP";

                            Graphics.DrawText(stringtoDraw, drawListPos, color, 20);
                            drawListPos.Y += 20;
                            if (mapGridX < firstVisibleIndex || mapGridX > lastVisibleIndex) continue;
                            //LogMessage("map " + mapArea.ToString() + drawRect2.ToString(), 5, Color.Red);
                            if (!ignoreCompletion)
                            {
                                Graphics.DrawFrame(drawRect2, Settings.UncompletedMaps, 5);
                                if(Settings.DrawKiracMapsNaturalTier)
                                    Graphics.DrawText(naturalTier.ToString(), new Vector2(drawRect2.X + drawRect2.Width - 4 - naturalTier.ToString().Count() * 6, drawRect2.Y + drawRect2.Height - 14), Color.GreenYellow);

                            }
                            else
                            {
                                if(Settings.HighlightIgnoredMaps)
                                    Graphics.DrawImage("AtlasMapCross.png", drawRect2, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                            }
                                
                            
                                

                           

                        }

                    }
                }
                else // kirac shop
                {
                    foreach (var item in inv.Inventory.InventorySlotItems)
                    {
                        var mapComponent = item.Item.GetComponent<Map>();

                        if (mapComponent == null)
                            continue;

                        var drawRect = item.GetClientRect();
                        drawRect.X = drawRect.X - 961.0f;
                        drawRect.Y = drawRect.Y - 325.0f;

                        var mapArea = mapComponent.Area;

                        //var shaper = shapered.Contains(mapArea);
                        if (MapAreasInBag.Contains(mapArea.Name))
                        {
                            if (Settings.Debug)
                            {
                                LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                            }
                            continue;
                        }

                        if (bonusComp.Contains(mapArea)) continue;

                        var color = Color.White;

                        if (mapComponent.Tier > 10)
                        {
                            color = Color.Red;
                        }
                        else if (mapComponent.Tier > 5)
                        {
                            color = Color.Yellow;
                        }
                        //LogMessage("Map: " + mapArea);

                        //LogMessage("Map: " + mapArea + " indice x:" + item.InventoryPosition.X + " indice y:" + item.InventoryPosition.Y);
                        var ignoreCompletion = false;

                        if (linesIgnoreMaps.Contains(mapArea.ToString()))
                        {
                            ignoreCompletion = true;

                        }
                        var stringtoDraw = mapArea.Name;
                        if (ignoreCompletion)
                            stringtoDraw += " --- IGNORED MAP";
                        Graphics.DrawText(mapArea.Name, drawListPos, color, 20);
                        Graphics.DrawFrame(drawRect, Color.Red, 5);
                        if (ignoreCompletion)
                            Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                        drawListPos.Y += 20;
                    }
                }
            }

          
        }   private void DrawNpcInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
            var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            List<string> MapAreasInBag = GetFilteredCompletableItems(inventoryZone, mapCanGiveCompletion);



            foreach (var inv in npcInv)
            {

                if (inv.Inventory.Rows == 1)
                {//kirac mission++
                    if (GameController.Game.IngameState.IngameUi.ZanaMissionChoice.IsVisible)
                    {

                        var KiracPanel = GameController.Game.IngameState.IngameUi.ZanaMissionChoice;
                        
                        //var inventory = KiracPanel.GetChildFromIndices(0, 3);
                        var inventory = KiracPanel.GetChildFromIndices(0, 3, 0, 0);
                        //LogMessage("Children indice 0 : " + KiracPanel.GetChildFromIndices(0).ChildCount.ToString());
                        //LogMessage("Children indice 0,3 : " + inventory.ChildCount.ToString());

                        var auxMap = KiracPanel?.GetChildFromIndices(0, 3);
                        var auxMap2 = KiracPanel?.GetChildFromIndices(0, 3, 0);
                        var auxMap3 = KiracPanel?.GetChildFromIndices(0, 3, 0, 0);

                        
                        var zanaMisionAux = auxMap.GetChildrenAs<ExileCore.PoEMemory.Element>().ToList() ?? new List<ExileCore.PoEMemory.Element>();
          
                        var firstVisible = zanaMisionAux.FirstOrDefault(x => x.IsVisible);
                        if (firstVisible == null)
                            continue;
                        var lastVisible = zanaMisionAux.Last(x => x.IsVisible);
                        var firstVisibleIndex = firstVisible.IndexInParent;
                        var lastVisibleIndex = lastVisible.IndexInParent;
                        
                        // get the list of maps in bag, check if they are completaable
                        
                        foreach (var item in inv.Inventory.InventorySlotItems)
                        {

                            var mapComponent = item.Item.GetComponent<Map>();
                            var mapGridX = item.PosX;
         
                            if (mapComponent == null)
                                continue;

                            var drawRect = item.GetClientRect();
                            var mapArea = mapComponent.Area;
                            var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == mapArea.Name).BaseTier;


                            var mapRarity = item.Item.GetComponent<Mods>().ItemRarity;


                            if (MapAreasInBag.Contains(mapArea.Name))
                            {
                                if (Settings.Debug)
                                {
                                    LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                                }
                                continue;
                            }

                            if (!npcInvMapCanGiveCompletion(item))
                            {
                                continue;
                            }

                            if (mapRarity != ItemRarity.Unique)
                            {

                                if (bonusComp.Contains(mapArea)) continue; // check item quality
                            }
                            else

                            {
                                var mapUniqueName = item.Item.GetComponent<Mods>().UniqueName;

                                if (bonusComp.Any(r => r.Name == mapUniqueName)) continue;

                            }

                            ///-

                            var color = Color.White;

                            if (mapComponent.Tier > 10)
                            {
                                color = Color.Red;
                            }
                            else if (mapComponent.Tier > 5)
                            {
                                color = Color.Yellow;
                            }

                            var ignoreCompletion = false;

                            if (linesIgnoreMaps.Contains(mapArea.ToString()))
                            {
                                ignoreCompletion = true;

                            }

                            var auxindex = (int)item.InventoryPosition.X;
                            var drawRect2 = KiracPanel.GetChildFromIndices(0, 3).GetChildAtIndex(auxindex).GetClientRect();

                            var stringtoDraw = mapArea.Name;
                            if (ignoreCompletion)
                                
                                stringtoDraw += " --- IGNORED MAP";

                            Graphics.DrawText(stringtoDraw, drawListPos, color, 20);
                            drawListPos.Y += 20;
                            if (mapGridX < firstVisibleIndex || mapGridX > lastVisibleIndex) continue;
                            //LogMessage("map " + mapArea.ToString() + drawRect2.ToString(), 5, Color.Red);
                            if (!ignoreCompletion)
                            {
                                Graphics.DrawFrame(drawRect2, Settings.UncompletedMaps, 5);
                                if(Settings.DrawKiracMapsNaturalTier)
                                    Graphics.DrawText(naturalTier.ToString(), new Vector2(drawRect2.X + drawRect2.Width - 4 - naturalTier.ToString().Count() * 6, drawRect2.Y + drawRect2.Height - 14), Color.GreenYellow);

                            }
                            else
                            {
                                if(Settings.HighlightIgnoredMaps)
                                    Graphics.DrawImage("AtlasMapCross.png", drawRect2, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                            }
                                
                            
                                

                           

                        }

                    }
                }
                else if (ingameState.IngameUi.SellWindowHideout.IsVisible)  // VENDOR
                {
                    if (inv.Inventory.Rows == 1) continue;
                        

                    //foreach (var item in inv.Inventory.InventorySlotItems)
                    foreach (var item in ingameState.IngameUi.SellWindowHideout.OtherOfferItems)
                    {
                        var mapComponent = item.Item.GetComponent<Map>();

                        if (mapComponent == null)
                            continue;

                        var drawRect = item.GetClientRect();
                        drawRect.X = drawRect.X;
                        drawRect.Y = drawRect.Y;
                        var mapArea = mapComponent.Area;

                        if (MapAreasInBag.Contains(mapArea.Name))
                        {
                            if (Settings.Debug)
                            {
                                LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                            }
                            continue;
                        }

                        if (bonusComp.Contains(mapArea)) continue;

                        var color = Color.White;

                        if (mapComponent.Tier > 10)
                        {
                            color = Color.Red;
                        }
                        else if (mapComponent.Tier > 5)
                        {
                            color = Color.Yellow;
                        }
                        //LogMessage("Map: " + mapArea);

                        //LogMessage("Map: " + mapArea + " indice x:" + item.InventoryPosition.X + " indice y:" + item.InventoryPosition.Y);
                        var ignoreCompletion = false;

                        if (linesIgnoreMaps.Contains(mapArea.ToString()))
                        {
                            ignoreCompletion = true;

                        }
                        var stringtoDraw = mapArea.Name;
                        if (ignoreCompletion)
                            stringtoDraw += " --- IGNORED MAP";
                        Graphics.DrawText(mapArea.Name, drawListPos, color, 20);
                        Graphics.DrawFrame(drawRect, Color.Red, 5);
                        if (ignoreCompletion)
                            Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                        drawListPos.Y += 20;
                    }
                }else // kirac shop
                {
                    foreach (var item in ingameState.IngameUi.HaggleWindow.InventoryItems)
                    
                    {
                        var mapComponent = item.Item.GetComponent<Map>();

                        if (mapComponent == null)
                            continue;

                        var drawRect = item.GetClientRect();
                        drawRect.X = drawRect.X;
                        drawRect.Y = drawRect.Y;

                        var mapArea = mapComponent.Area;

                        //var shaper = shapered.Contains(mapArea);
                        if (MapAreasInBag.Contains(mapArea.Name))
                        {
                            if (Settings.Debug)
                            {
                                LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                            }
                            continue;
                        }

                        if (bonusComp.Contains(mapArea)) continue;

                        var color = Color.White;

                        if (mapComponent.Tier > 10)
                        {
                            color = Color.Red;
                        }
                        else if (mapComponent.Tier > 5)
                        {
                            color = Color.Yellow;
                        }
                        //LogMessage("Map: " + mapArea);

                        //LogMessage("Map: " + mapArea + " indice x:" + item.InventoryPosition.X + " indice y:" + item.InventoryPosition.Y);
                        var ignoreCompletion = false;

                        if (linesIgnoreMaps.Contains(mapArea.ToString()))
                        {
                            ignoreCompletion = true;

                        }
                        var stringtoDraw = mapArea.Name;
                        if (ignoreCompletion)
                            stringtoDraw += " --- IGNORED MAP";
                        Graphics.DrawText(mapArea.Name, drawListPos, color, 20);
                        Graphics.DrawFrame(drawRect, Color.Red, 5);
                        if (ignoreCompletion)
                            Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                        drawListPos.Y += 20;
                    }
                }
            }

          
        }
        /*
          
        
        private void DrawKiracMissionsTest()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
            var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            List<string> MapAreasInBag = GetFilteredCompletableItems(inventoryZone, mapCanGiveCompletion);



       

                
                //kirac mission++
                    if (GameController.Game.IngameState.IngameUi.ZanaMissionChoice.IsVisible)
                    {

                        var KiracPanel = GameController.Game.IngameState.IngameUi.ZanaMissionChoice;

                        //var inventory = KiracPanel.GetChildFromIndices(0, 3);
                        var inventory = KiracPanel.GetChildFromIndices(0, 3, 0, 0);
                        //LogMessage("Children indice 0 : " + KiracPanel.GetChildFromIndices(0).ChildCount.ToString());
                        //LogMessage("Children indice 0,3 : " + inventory.ChildCount.ToString());

                        var auxMap = KiracPanel?.GetChildFromIndices(0, 3);
                        var auxMap2 = KiracPanel?.GetChildFromIndices(0, 3, 0);
                        var auxMap3 = KiracPanel?.GetChildFromIndices(0, 3, 0, 0);

                    var auxMapList = KiracPanel?.GetChildFromIndices(0, 3).Children;

                    var zanaMisionAux = auxMap.GetChildrenAs<ExileCore.PoEMemory.Element>().ToList() ?? new List<ExileCore.PoEMemory.Element>();
                        var firstVisible = zanaMisionAux.First(x => x.IsVisible);
                        var lastVisible = zanaMisionAux.Last(x => x.IsVisible);
                        var firstVisibleIndex = firstVisible.IndexInParent;
                        var lastVisibleIndex = lastVisible.IndexInParent;

                        // get the list of maps in bag, check if they are completaable

                        foreach (var item in auxMapList)
                        {
                            
                            var mapComponent = item.Item.GetComponent<Map>();
                            var mapGridX = item.PosX;

                            if (mapComponent == null)
                                continue;

                            var drawRect = item.GetClientRect();
                            var mapArea = mapComponent.Area;
                            var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == mapArea.Name).BaseTier;


                            var mapRarity = item.Item.GetComponent<Mods>().ItemRarity;


                            if (MapAreasInBag.Contains(mapArea.Name))
                            {
                                if (Settings.Debug)
                                {
                                    LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                                }
                                continue;
                            }

                            if (!npcInvMapCanGiveCompletion(item))
                            {
                                continue;
                            }

                            if (mapRarity != ItemRarity.Unique)
                            {

                                if (bonusComp.Contains(mapArea)) continue; // check item quality
                            }
                            else

                            {
                                var mapUniqueName = item.Item.GetComponent<Mods>().UniqueName;

                                if (bonusComp.Any(r => r.Name == mapUniqueName)) continue;

                            }

                            ///-

                            var color = Color.White;

                            if (mapComponent.Tier > 10)
                            {
                                color = Color.Red;
                            }
                            else if (mapComponent.Tier > 5)
                            {
                                color = Color.Yellow;
                            }

                            var ignoreCompletion = false;

                            if (linesIgnoreMaps.Contains(mapArea.ToString()))
                            {
                                ignoreCompletion = true;

                            }

                            var auxindex = (int)item.InventoryPosition.X;
                            var drawRect2 = KiracPanel.GetChildFromIndices(0, 3).GetChildAtIndex(auxindex).GetClientRect();

                            var stringtoDraw = mapArea.Name;
                            if (ignoreCompletion)

                                stringtoDraw += " --- IGNORED MAP";

                            Graphics.DrawText(stringtoDraw, drawListPos, color, 20);
                            drawListPos.Y += 20;
                            if (mapGridX < firstVisibleIndex || mapGridX > lastVisibleIndex) continue;
                            //LogMessage("map " + mapArea.ToString() + drawRect2.ToString(), 5, Color.Red);
                            if (!ignoreCompletion)
                            {
                                Graphics.DrawFrame(drawRect2, Settings.UncompletedMaps, 5);
                                if (Settings.DrawKiracMapsNaturalTier)
                                    Graphics.DrawText(naturalTier.ToString(), new Vector2(drawRect2.X + drawRect2.Width - 4 - naturalTier.ToString().Count() * 6, drawRect2.Y + drawRect2.Height - 14), Color.GreenYellow);

                            }
                            else
                            {
                                if (Settings.HighlightIgnoredMaps)
                                    Graphics.DrawImage("AtlasMapCross.png", drawRect2, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                            }






                        }

                    }
                
                
                
            


        }

        */
        private void DrawNpcInvMapsAlt()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            var purchaseWindowHideout = GameController.Game.IngameState.IngameUi.PurchaseWindowHideout;
            var purchaseWindow = GameController.Game.IngameState.IngameUi.PurchaseWindow;
            ServerInventory purchaseWindowInvent = new ServerInventory();
            if (purchaseWindow.IsVisible)
            {
                
                purchaseWindowInvent = purchaseWindow.TabContainer.AllInventories.First().ServerInventory;
                
            }
            else if (purchaseWindowHideout.IsVisible)
            {
                
                purchaseWindowInvent = purchaseWindowHideout.TabContainer.AllInventories.First().ServerInventory;
                
            }

            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
            var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            List<string> MapAreasInBag = GetFilteredCompletableItems(inventoryZone, mapCanGiveCompletion);
            var KiracPanel = GameController.Game.IngameState.IngameUi.ZanaMissionChoice;

            foreach (var item in purchaseWindowInvent.InventorySlotItems)
            {

                var mapComponent = item.Item.GetComponent<Map>();
                var mapGridX = item.PosX;

                if (mapComponent == null)
                    continue;

                var drawRect = item.GetClientRect();
                var mapArea = mapComponent.Area;
                var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == mapArea.Name).BaseTier;


                var mapRarity = item.Item.GetComponent<Mods>().ItemRarity;


                if (MapAreasInBag.Contains(mapArea.Name))
                {
                    if (Settings.Debug)
                    {
                        LogMessage("Map " + mapArea.Name + " doesn't need to highlighted because a completeable copy is in bag", 5, Color.Red);
                    }
                    continue;
                }

                if (!npcInvMapCanGiveCompletion(item))
                {
                    continue;
                }

                if (mapRarity != ItemRarity.Unique)
                {

                    if (bonusComp.Contains(mapArea)) continue; // check item quality
                }
                else

                {
                    var mapUniqueName = item.Item.GetComponent<Mods>().UniqueName;

                    if (bonusComp.Any(r => r.Name == mapUniqueName)) continue;

                }

                ///-

                var color = Color.White;

                if (mapComponent.Tier > 10)
                {
                    color = Color.Red;
                }
                else if (mapComponent.Tier > 5)
                {
                    color = Color.Yellow;
                }

                var ignoreCompletion = false;

                if (linesIgnoreMaps.Contains(mapArea.ToString()))
                {
                    ignoreCompletion = true;

                }

                var auxindex = (int)item.InventoryPosition.X;
                //var drawRect2 = KiracPanel.GetChildFromIndices(0, 3).GetChildAtIndex(auxindex).GetClientRect();

                var stringtoDraw = mapArea.Name;
                if (ignoreCompletion)

                    stringtoDraw += " --- IGNORED MAP";

                Graphics.DrawText(stringtoDraw, drawListPos, color, 20);
                drawListPos.Y += 20;
                
                //LogMessage("map " + mapArea.ToString() + drawRect2.ToString(), 5, Color.Red);
                if (!ignoreCompletion)
                {
                    // 962
                    // 330
                    Graphics.DrawFrame(new RectangleF(drawRect.X-962, drawRect.Y-330, drawRect.Width, drawRect.Height), Settings.UncompletedMaps, 5);
                    if (Settings.DrawKiracMapsNaturalTier)
                        Graphics.DrawText(naturalTier.ToString(), new Vector2(drawRect.X + drawRect.Width - 4 - naturalTier.ToString().Count() * 6, drawRect.Y + drawRect.Height - 14), Color.GreenYellow);

                }
                else
                {
                    if (Settings.HighlightIgnoredMaps)
                        Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);
                }






            }

        }
        

        private void HiglightAllMaps(IList<NormalInventoryItem> items)
        {

            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;


            var disableOnHover = false;
            var disableOnHoverRect = new RectangleF();

            var inventoryItemIcon = ingameState.UIHover.AsObject<HoverItemIcon>();

            var tooltip = inventoryItemIcon?.Tooltip;

            if (tooltip != null)
            {
                disableOnHover = true;
                disableOnHoverRect = tooltip.GetClientRect();
            }
            if (Settings.EnableDiagonalProgressionHighlight)
            {
                foreach (var mapToRun in finalMapsToRun)
                {
                    if(Settings.Debug)
                        LogMessage("Best maps to run " + mapToRun.Item1.ToStringBestMapsToRun(), 5, Color.Pink);
                    //Graphics.DrawImage("ImagesAtlas.png", mapToRun.Item2, new RectangleF(.184f, .731f, .184f, .269f), Color.Pink);
                    if (disableOnHover && disableOnHoverRect.Intersects(mapToRun.Item3))
                        continue;
                    //Graphics.DrawImage("ImagesAtlas.png", mapToRun.Item2, new RectangleF(.184f, .731f, .184f, .269f), Color.Pink);
                    if (Settings.EnableDiagonalProgressionHighlight)
                    {
                        if (willGiveCompletion) { 
                            Graphics.DrawBox(mapToRun.Item3, Settings.DiagonalProgressionHighlightWillGiveCompletion);
                        }
                        else
                        {
                            Graphics.DrawBox(mapToRun.Item3, Settings.DiagonalProgressionHighlightWontGiveCompletion);
                        }
                    }
                        
                    //Graphics.DrawImage("Image

                }
            }
           

            foreach (var item in items)
            {
                
                var entity = item?.Item;

                if (entity == null) continue;
                var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);

                if (bit == null) continue;
                if (bit.ClassName != "Map" && bit.ClassName != "Maps") continue;
                var mapComponent = entity.GetComponent<Map>();
                var modsComponent = entity.GetComponent<Mods>();
                var baseComponent = entity.GetComponent<Base>();

                var rarity = modsComponent.ItemRarity;
                var corrupted = baseComponent.isCorrupted;

                var drawRect = item.GetClientRect();
                var drawRect2 = item.GetClientRect();

                if (disableOnHover && disableOnHoverRect.Intersects(drawRect))
                    continue;
                
                var offset = 3;
                drawRect.Top += offset;
                drawRect.Bottom -= offset;
                drawRect.Right -= offset;
                drawRect.Left += offset;

                
                drawRect2.Top += 20;
                drawRect2.Bottom -= 0;
                drawRect2.Right -= 0;
                drawRect2.Left += 20;

                var completed = 0;

                var area = mapComponent.Area;
                var tier = mapComponent.Tier;
                var mapMods = modsComponent.ExplicitMods.Count();

                //if (comp.Contains(area))
                //    completed++;

                if (bonusComp.Contains(area))
                    completed++;


                if (linesIgnoreMaps.Contains(area.ToString()) && Settings.HighlightIgnoredMaps)
                {
                    
                    Graphics.DrawImage("AtlasMapCross.png", drawRect, new RectangleF(1, 1, 1, 1), Settings.IgnoredMaps);

                }



                if (completed == 0)
                {

                    if (tier > 10 && (rarity < ItemRarity.Rare || !corrupted) && rarity != ItemRarity.Unique)
                    {
                        // vaal orb.
                        if (Settings.ShowIfMapIsReadyOrNeedsCurrency)
                            Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Settings.MapNeedsCurrency);
                        if (Settings.ShowCurrencyHint)
                            Graphics.DrawImage("vaal.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else if (tier > 5 && rarity < ItemRarity.Rare && rarity != ItemRarity.Unique)
                    {
                        // alc orb
                        if (Settings.ShowIfMapIsReadyOrNeedsCurrency)
                            Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Settings.MapNeedsCurrency);
                        if (Settings.ShowCurrencyHint)
                            Graphics.DrawImage("alc.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else if (tier <= 5 && rarity < ItemRarity.Magic && rarity != ItemRarity.Unique)
                    {
                        // trans orb
                        if (Settings.ShowIfMapIsReadyOrNeedsCurrency)
                            Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Settings.MapNeedsCurrency);
                        if (Settings.ShowCurrencyHint)
                            Graphics.DrawImage("transmute.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else if (tier <= 5 && rarity == ItemRarity.Magic && rarity != ItemRarity.Unique && mapMods < 2)
                    {
                        // aug orb
                        if (Settings.ShowIfMapIsReadyOrNeedsCurrency)
                            Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Settings.MapNeedsCurrency);
                        if (Settings.ShowCurrencyHint)
                            Graphics.DrawImage("augment.png", drawRect2, new RectangleF(1, 1, 1, 1));

                    }
                    else
                    {
                        if (Settings.ShowIfMapIsReadyOrNeedsCurrency)
                            Graphics.DrawImage("ImagesAtlas.png", drawRect, new RectangleF(.184f, .731f, .184f, .269f), Settings.MapReady);
                    }

                    // if map name is in not complete list then

                }
                var rectAux = drawRect;
                rectAux.Width = 20;
                rectAux.Height = 10;
                Graphics.DrawBox(rectAux, Color.Black, 5);
                if (Settings.ShowMapTierTopLeft)
                    Graphics.DrawText(tier.ToString(), new Vector2(drawRect.X, drawRect.Y - 1), Color.White);

                

            }
            
        }



        public class MapItem
        {
            public Color DrawColor = Color.Transparent;
            public RectangleF DrawRect;
            public string Name;
            public double Penalty;
            public int Tier { get; }

            public MapItem(string Name, RectangleF DrawRect, int tier)
            {
                this.Name = Name;
                this.DrawRect = DrawRect;
                Tier = tier;
            }
        }
    }
}
