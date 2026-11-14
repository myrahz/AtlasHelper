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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using MapKey = ExileCore.PoEMemory.Components.MapKey;

namespace AtlasHelper
{
    public class AtlasHelper : BaseSettingsPlugin<AtlasHelperSettings>
    {

        private bool _vendorProcessing;
        private CancellationTokenSource _vendorCts;
        private SyncTask<bool> _vendorTask;
        private Vector2 _windowOffset;

        private List<int> checkpointsKirac = new List<int>
        {
            0,3,7,14,22,29,36,43,50,58,65,72,79,90,101,115
        };

        private Dictionary<int, string> checkPointsKiracTier = new Dictionary<int, string>
        {
            {3, "T4"},
            {7, "T5"},
            {14, "T6"},
            {22, "T7"},
            {29, "T8"},
            {36, "T9"},
            {43, "T10"},
            {50, "T11"},
            {58, "T12"},
            {65, "T13"},
            {72, "T14"},
            {79, "T15"},
            {90, "T16"},
            {101, "T16"},
            {115, "T16"}
        };
        private string completionBonusString = "";
        private int completionBonusWorking = -2;
        private List<String> linesIgnoreMaps = new List<String>();
        private Color[] SelectColors;

        private List<(WorldArea, int, bool)> BonusAreasBaseTiers = new List<(WorldArea, int, bool)>();

        private List<AtlasMap> ListAtlasMaps = new List<AtlasMap>();
        private List<AtlasMap> ListAtlasMapsCompleteable = new List<AtlasMap>();
        private int highestCompletedTier = 0;

        private IList<WorldArea> bonusComp;


        private List<(AtlasMap, int, RectangleF)> finalMapsToRun = new List<(AtlasMap, int, RectangleF)>();
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


            Input.RegisterKey(Keys.LControlKey);
            var ok = base.Initialise();
            _windowOffset = GameController.Window.GetWindowRectangleTimeCache.TopLeft;
            _vendorCts = new CancellationTokenSource();
            GetAtlasNodes();
            if (!initImage)
                return false;



            return true;
        }


        public override void AreaChange(AreaInstance area)
        {
            GetAtlasNodes();
            UpdateCompletion();
            //refreshCompletedAreas();


        }
        private class CachedMapSlot
        {
            public string MapKey;                 
            public int PosX;
            public int PosY;
            public Vector2 ClickPoint;           
        }


        public override void Render()
        {



            // test highest completed tier
            if (Settings.Debug)
            {
                LogMessage($"Highest completed tier: {highestCompletedTier}", 5, Color.LightBlue);
                LogMessage($"Number of atlas maps that award completion: {ListAtlasMaps.Count(atlasMap => atlasMap.AdjacentMaps.Count > 0)}", 5, Color.LightBlue);
            }
            renderKiracCompletionShopRefresh();
            checkMapIgnore();
            DrawPlayerInvMaps();
            DrawNpcInvMaps();
            //DrawNpcInvMaps();
            //DrawNpcInvMapsAlt();
            DrawDiagonalProgression();

            HandleVendor3For1ToggleAndTask();

        }

        private void renderKiracCompletionShopRefresh()
        {
            var textRect = new Vector2(Settings.KiracShopSpyX, Settings.KiracShopSpyY);
            Graphics.DrawText(completionBonusString, textRect, Color.White, FontAlign.Center); // - new Vector2(size.Width / 2, size.Height / 2)
        }

        private void HandleVendor3For1ToggleAndTask()
        {
            if (Settings.Vendor3For1Hotkey.PressedOnce())
            {
                _vendorProcessing = !_vendorProcessing;

                if (_vendorProcessing)
                {
   
                    _vendorCts = new CancellationTokenSource();
                    LogMessage("Vendor 3-for-1 started.", 5, Color.LimeGreen);
                }
                else
                {
                    LogMessage("Vendor 3-for-1 stopped.", 5, Color.Yellow);
                    Input.KeyUp(Keys.LControlKey);
                    _vendorCts?.Cancel();
                    _vendorCts?.Dispose();
                    _vendorTask = null;
                }
            }

            if (_vendorProcessing)
            {
                TaskUtils.RunOrRestart(ref _vendorTask, () => RunVendor3For1Async(_vendorCts.Token));
            }
        }
        private bool IsSellWindowVisible()
        {
            var ui = GameController.Game.IngameState.IngameUi;
            return ui.SellWindowHideout.IsVisible || ui.SellWindow.IsVisible;
        }

        private Entity ReadVendorResultMapEntity()
        {
            var ui = GameController.Game.IngameState.IngameUi;

            IEnumerable<NormalInventoryItem> otherOffer = null;

            if (ui.SellWindowHideout.IsVisible)
                otherOffer = ui.SellWindowHideout.OtherOfferItems;
            else if (ui.SellWindow.IsVisible)
                otherOffer = ui.SellWindow.OtherOfferItems;

            if (otherOffer == null) return null;

            foreach (var it in otherOffer)
            {
                var ent = it?.Item;
                if (ent == null) continue;
                if (ent.GetComponent<MapKey>() != null)
                    return ent;
            }

            return null;
        }




        private async Task UndoVendorOfferAsync(CancellationToken token)
        {
            var ui = GameController.Game.IngameState.IngameUi;

            // TODO: replace with the correct list of "your offer items" for your SellWindow
            // Examples (depends on your ExileCore version):
            // var offerItems = ui.SellWindowHideout.YourOfferItems;
            var offerItems = ui.SellWindow.YourOfferItems;

            //IEnumerable<NormalInventoryItem> offerItems = null; // <-- set this

            if (offerItems == null)
                return;

            Input.KeyDown(Keys.LControlKey);
            try
            {
                foreach (var it in offerItems)
                {
                    token.ThrowIfCancellationRequested();
                    var rect = it?.GetClientRect();
                    if (rect == null) continue;

                    Input.SetCursorPos(rect.Value.Center + _windowOffset);
                    await Task.Delay(30, token);
                    Input.Click(MouseButtons.Left);
                    await Task.Delay(50, token);
                }
            }
            finally
            {
                Input.KeyUp(Keys.LControlKey);
            }

            await Task.Delay(80, token);
        }

        private static IEnumerable<List<T>> Combinations<T>(IList<T> list, int k)
        {
            if (k <= 0 || k > list.Count) yield break;

            var idx = new int[k];
            for (int i = 0; i < k; i++) idx[i] = i;

            while (true)
            {
                var combo = new List<T>(k);
                for (int i = 0; i < k; i++) combo.Add(list[idx[i]]);
                yield return combo;

                int t = k - 1;
                while (t >= 0 && idx[t] == list.Count - k + t) t--;
                if (t < 0) yield break;

                idx[t]++;
                for (int i = t + 1; i < k; i++)
                    idx[i] = idx[i - 1] + 1;
            }
        }
        private async Task ClickVendorAcceptAsync(CancellationToken token)
        {
            var ui = GameController.Game.IngameState.IngameUi;

            if (ui.SellWindowHideout.IsVisible)
            {
                var p = ui.SellWindowHideout.AcceptButton.GetClientRect().Center + _windowOffset;
                Input.SetCursorPos(p);
            }
            else if (ui.SellWindow.IsVisible)
            {
                var p = ui.SellWindow.AcceptButton.GetClientRect().Center + _windowOffset;
                Input.SetCursorPos(p);
            }
            else return;

            await Task.Delay(40, token);
            Input.Click(MouseButtons.Left);
            await Task.Delay(80, token);
        }
        private async SyncTask<bool> RunVendor3For1Async(CancellationToken token)
        {
            var completeableNames = new HashSet<string>(
                ListAtlasMapsCompleteable
                    .Where(m => m?.WorldArea != null && !string.IsNullOrEmpty(m.WorldArea.Name))
                    .Select(m => m.WorldArea.Name),
                StringComparer.OrdinalIgnoreCase
            );

            try
            {
                var ingame = GameController.Game.IngameState;

                if (!IsSellWindowVisible())
                    return false;

                // Refresh window offset right before caching click points
                _windowOffset = GameController.Window.GetWindowRectangleTimeCache.TopLeft;

                // UniDy style: iterate SERVER inventory (not VisibleInventoryItems)
                var slots = ingame.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

                // Cache map slots + their click positions BEFORE doing any clicking
                var cached = new List<CachedMapSlot>();
                var inventoryMapNames = new HashSet<string>(
                    cached.Select(c => c.MapKey).Where(n => !string.IsNullOrEmpty(n)),
                    StringComparer.OrdinalIgnoreCase
                );

           

                foreach (var s in slots)
                {
                    token.ThrowIfCancellationRequested();
                    if (s?.Item == null) continue;

                    var map = s.Item.GetComponent<MapKey>();
                    if (map == null) continue;

                    var modsComponent = s.Item.GetComponent<Mods>();
        
                    var rarity = modsComponent.ItemRarity;

                    if (rarity > ItemRarity.Rare ) continue;
                    var key = map.GetArea()?.Name ?? s.Item.Path;


                    // IMPORTANT: cache click point NOW
                    var click = s.GetClientRect().Center + _windowOffset;

                    cached.Add(new CachedMapSlot
                    {
                        MapKey = key,
                        PosX = s.PosX,
                        PosY = s.PosY,
                        ClickPoint = click
                    });
                }

                if (cached.Count < 3)
                    return false;

                // Group by "same kind of map"
                var groups = cached
                    .GroupBy(x => x.MapKey)
                    .Where(g => g.Count() >= 3)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                if (groups.Count == 0)
                    return false;

                // Build a global plan with stable ordering + indices
                var plan = new List<(string groupKey, List<CachedMapSlot> combo)>();
                foreach (var g in groups)
                {
                    var list = g.ToList();
                    foreach (var combo in Combinations(list, 3))
                        plan.Add((g.Key, combo.ToList()));
                }

                // Debug: print all combinations at the beginning
                if (Settings.Debug3To1)
                {
                    LogMessage($"[3to1] Inventory maps cached: {cached.Count}", 5, Color.Cyan);
                    LogMessage($"[3to1] Groups with >=3: {groups.Count}", 5, Color.LightBlue);
                    LogMessage($"[3to1] Total combos to try: {plan.Count}", 5, Color.LightBlue);

                    for (int i = 0; i < plan.Count; i++)
                    {
                        var (groupKey, combo) = plan[i];
                        LogMessage(
                            $"[3to1] PLAN #{i} group='{groupKey}' combo=({combo[0].PosX},{combo[0].PosY}) ({combo[1].PosX},{combo[1].PosY}) ({combo[2].PosX},{combo[2].PosY})",
                            5,
                            Color.LightGray
                        );
                    }

                    if (Settings.Simulate3To1)
                        return true; // stop after printing
                }

                // Execute plan in order; log which combo is being tried
                for (int i = 0; i < plan.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (!IsSellWindowVisible()) return false;

                    var (groupKey, combo) = plan[i];

                    if (Settings.Debug3To1)
                    {
                        LogMessage(
                            $"[3to1] TRY  #{i} group='{groupKey}' combo=({combo[0].PosX},{combo[0].PosY}) ({combo[1].PosX},{combo[1].PosY}) ({combo[2].PosX},{combo[2].PosY})",
                            5,
                            Color.Yellow
                        );
                    }

                    await UndoVendorOfferAsync(token);

                    // Ctrl+click 3 maps into vendor offer using cached points
                    Input.KeyDown(Keys.LControlKey);
                    try
                    {
                        foreach (var slot in combo)
                        {
                            token.ThrowIfCancellationRequested();

                            Input.SetCursorPos(slot.ClickPoint);
                            await Task.Delay(40, token);
                            Input.Click(MouseButtons.Left);
                            await Task.Delay(60, token);
                        }
                    }
                    finally
                    {
                        Input.KeyUp(Keys.LControlKey);
                    }

                    await Task.Delay(120, token);

                    // Read vendor result
                    var result = ReadVendorResultMapEntity();
                    if (result == null)
                    {
                        if (Settings.Debug3To1)
                            LogMessage($"[3to1] RESULT #{i}: <null> (no recipe output)", 5, Color.Orange);

                        await UndoVendorOfferAsync(token);
                        continue;
                    }

                    var resultName = result.GetComponent<MapKey>()?.GetArea()?.Name ?? result.Path;

                    if (Settings.Debug3To1)
                        LogMessage($"[3to1] RESULT #{i}: '{resultName}'", 5, Color.LightGreen);

                    if (vendorMapCanGiveCompletion(resultName, inventoryMapNames))
                    {

                    
                        LogMessage($"[3to1] RESULT #{i}: '{resultName}' IS IN COMPLETEABLE!!", 5, Color.LightGreen);
                        return true;
                    }
                    await UndoVendorOfferAsync(token);
                }

                // Exhausted all combinations
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception e)
            {
                LogError($"RunVendor3For1Async error: {e}");
                return false;
            }
            finally
            {
                _vendorProcessing = false;
                Input.KeyUp(Keys.LControlKey);
                LogMessage("[3to1] Finished.", 5, Color.LightBlue);
            }
        }
        


        private void UpdateCompletion()
        {
            var completionBonus = GameController.Game.IngameState?.ServerData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList().Count ?? 0;

            int nextCompletionThreshold = 1;
            string kiracShopMaps = "T1";
            nextCompletionThreshold = checkpointsKirac.First(x => x > completionBonus);
            int auxCheckpoint = checkpointsKirac.Last(x => x <= nextCompletionThreshold);

            kiracShopMaps = checkPointsKiracTier[auxCheckpoint];
            completionBonusString = "Bonus: " + completionBonus.ToString() + "(" + kiracShopMaps + ") -->" + nextCompletionThreshold.ToString();

            completionBonusWorking = completionBonus;
        }
        private void DrawDiagonalProgression()
        {

            var ingameState = GameController.Game.IngameState;
            finalMapsToRun.Clear();

            if (ingameState.IngameUi.InventoryPanel.IsVisible)
            {

                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;

                if (ingameState.IngameUi.StashElement.IsVisible && ingameState.IngameUi.StashElement.VisibleStash != null)
                {

                    foreach (var normalStashItem in ingameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems)
                    {
                        inventoryZone.Insert(inventoryZone.Count, normalStashItem);
                    }
                }



                var mapsThatGiveCompletion = new List<(AtlasMap, int, RectangleF)>();
                var mapsThatWontGiveCompletion = new List<(AtlasMap, int, RectangleF)>();



                // if highest tier < 3          

                foreach (var item in inventoryZone)
                {
                    var entity = item?.Item;

                    if (entity == null) continue;
                    var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);

                    if (bit == null) continue;
                    if (bit.ClassName != "Map" && bit.ClassName != "Maps") continue;
                    var mapComponent = entity.GetComponent<MapKey>();
                    var modsComponent = entity.GetComponent<Mods>();
                    var baseComponent = entity.GetComponent<Base>();

                    var rarity = modsComponent.ItemRarity;
                    var corrupted = baseComponent.isCorrupted;
                    var drawRect = item.GetClientRect();
                    var uniqueName = modsComponent.UniqueName;
                    var area = mapComponent.GetArea();
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

                    if (naturalTier == 17)
                    {
                        continue;
                    }
                    if (linesIgnoreMaps.Contains(area.ToString()))
                    {

                        continue;

                    }

                    if (mapCanGiveCompletion(item))
                    {
                        if (rarity != ItemRarity.Unique)
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
        private bool vendorMapCanGiveCompletion(string areaName, HashSet<string> inventoryMapNames)
        {
            if (string.IsNullOrEmpty(areaName))
                return false;

            // NEW RULE: if we already have this map in inventory, don't accept it
            if (inventoryMapNames != null && inventoryMapNames.Contains(areaName))
            {
                if (Settings.Debug3To1)
                    LogMessage($"Map {areaName} wont give completion because you already have it in inventory", 5, Settings.IgnoredMaps);

                return false;
            }

            // Not even a map you can complete on atlas list
            if (!ListAtlasMapsCompleteable.Any(atlasMap => atlasMap.WorldArea.Name == areaName))
            {
                if (Settings.Debug3To1)
                    LogMessage($"Map {areaName} wont give completion because it isn't present on the atlas list", 5, Settings.IgnoredMaps);

                return false;
            }

            // Already completed
            if (bonusComp.Any(a => a.Name == areaName))
            {
                if (Settings.Debug3To1)
                    LogMessage($"Map {areaName} wont give completion because it is already completed", 5, Settings.IgnoredMaps);

                return false;
            }

            // Otherwise: can give completion
            if (Settings.Debug3To1)
                LogMessage($"Map {areaName} will give completion", 5, Settings.UncompletedMaps);

            return true;
        }
        private bool mapCanGiveCompletion(NormalInventoryItem mapNormalInventoryItem)
        {
            var entity = mapNormalInventoryItem?.Item;
            var bit = GameController.Files.BaseItemTypes.Translate(entity.Path);


            if (bit == null) return false; ;
            if (bit.ClassName != "Map" && bit.ClassName != "Maps") return false; ;
            var mapComponent = entity.GetComponent<MapKey>();
            var modsComponent = entity.GetComponent<Mods>();
            var baseComponent = entity.GetComponent<Base>();
            var rarity = modsComponent.ItemRarity;
            var corrupted = baseComponent.isCorrupted;
            var uniqueName = modsComponent.UniqueName;
            var area = mapComponent.GetArea();
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
            var mapComponent = entity.GetComponent<MapKey>();
            var modsComponent = entity.GetComponent<Mods>();
            var baseComponent = entity.GetComponent<Base>();
            var rarity = modsComponent.ItemRarity;
            var corrupted = baseComponent.isCorrupted;
            var area = mapComponent.GetArea();
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
            else if (naturalTier > tier)
            {
                if (Settings.Debug)
                {
                    LogMessage("Map " + area + " | " + rarity + " wont give completion because GGG Nerfed us, and the map tier offered is lower than its natural tier", 5, Settings.IgnoredMaps);
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
            bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            ListAtlasMaps.Clear();
            BonusAreasBaseTiers.Clear();
            //bonusCompWithoutUniques = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList().Where(x => !x.Id.Contains("Unique"));



            foreach (var node in atlasNodes)
            {

                var baseAtlasNodeAddress = node.Address;
                //var tier0 = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x51);
                var tier0 = node.BaseTier;


                var AtlasNodeKeys = ingameState.M.Read<DatArrayStruct>(baseAtlasNodeAddress + 0x41);

                //var numberOfNeighbours = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x41);
                var numberOfNeighbours = node.Connections.Count();
                var isNormalMap = numberOfNeighbours > 1;


                BonusAreasBaseTiers.Add((node.Area, tier0, isNormalMap));
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
                if (filteredBonus.Count() > 0)
                    highestCompletedTier = filteredBonus.Max(x => x.Item2);
            }
        }
        private void checkMapIgnore()
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();

            //if (linesIgnoreMaps.Contains(GameController.IngameState.Data.CurrentArea.Name))
            if (linesIgnoreMaps.Contains(GameController.IngameState.Data.CurrentArea.Name) && !bonusComp.Any(x => x.Name == GameController.IngameState.Data.CurrentArea.Name))
            {
                Vector2 newInfoPanel2 = new Vector2(822, 722);
                var drawBox2 = new RectangleF(newInfoPanel2.X, newInfoPanel2.Y, 350, 60);
                if (Settings.WarnInsideMapToNotComplete)
                {
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

            return items.Where(condition).Select(item => item.Item.GetComponent<MapKey>().GetArea().Name).ToList();
        }

        private void DrawNpcInvMapsOld()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            var comp = serverData.CompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
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

                            var mapComponent = item.Item.GetComponent<MapKey>();
                            var mapGridX = item.PosX;

                            if (mapComponent == null)
                                continue;

                            var drawRect = item.GetClientRect();
                            var mapArea = mapComponent.GetArea();
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
                else // kirac shop
                {
                    foreach (var item in inv.Inventory.InventorySlotItems)
                    {
                        var mapComponent = item.Item.GetComponent<MapKey>();

                        if (mapComponent == null)
                            continue;

                        var drawRect = item.GetClientRect();
                        drawRect.X = drawRect.X - 961.0f;
                        drawRect.Y = drawRect.Y - 325.0f;

                        var mapArea = mapComponent.GetArea();

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
        private void DrawNpcInvMaps()
        {
            var ingameState = GameController.Game.IngameState;

            var serverData = ingameState.ServerData;
            var npcInv = serverData.NPCInventories;

            if (npcInv == null || npcInv.Count == 0) return;

            var bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            var comp = serverData.CompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
            var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            List<string> MapAreasInBag = GetFilteredCompletableItems(inventoryZone, mapCanGiveCompletion);

            if (ingameState.IngameUi.SellWindowHideout.IsVisible)  // VENDOR
            {



                //foreach (var item in inv.Inventory.InventorySlotItems)
                foreach (var item in ingameState.IngameUi.SellWindowHideout.OtherOfferItems)
                {
                    var mapComponent = item.Item.GetComponent<MapKey>();

                    if (mapComponent == null)
                        continue;

                    var drawRect = item.GetClientRect();
                    drawRect.X = drawRect.X;
                    drawRect.Y = drawRect.Y;
                    var mapArea = mapComponent.GetArea();

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

                            var mapComponent = item.Item.GetComponent<MapKey>();
                            var mapGridX = item.PosX;

                            if (mapComponent == null)
                                continue;

                            var drawRect = item.GetClientRect();
                            var mapArea = mapComponent.GetArea();
                            var naturalTier = ListAtlasMaps.FirstOrDefault(x => x.WorldArea.Name == mapArea.Name).BaseTier;


                            var mapRarity = item.Item.GetComponent<Mods>().ItemRarity;
                            var mapmods = item.Item.GetComponent<Mods>().ItemMods;
                            // CHECK ITS MODS 

                            // mods to watch
                            var modsToWatch = new List<string>
                                {
                                    "MapMissionDisplayHarvestBeasts",
                                    "MapMissionDisplayHugeHarvest",
                                    "MapMissionDisplayEssenceExiles"
                                };

                            var auxindex = (int)item.InventoryPosition.X;
                            var drawRect2 = KiracPanel.GetChildFromIndices(0, 3).GetChildAtIndex(auxindex).GetClientRect();
                            var stringtoDraw = mapArea.Name;

                            var matchedMod = mapmods
                                .FirstOrDefault(m => modsToWatch.Any(k =>
                                    m.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                                ));

                            if (matchedMod != null)
                            {
                                if (Settings.Debug)
                                {
                                    LogMessage("Map " + mapArea.Name + " Contains a watchable mod" + matchedMod.ToString(), 5, Color.Red);
                                }
                                Graphics.DrawText(stringtoDraw + " " + matchedMod.ToString(), drawListPos, Color.Gold, 20);
                                drawListPos.Y += 20;
                                Graphics.DrawFrame(drawRect2, Color.Gold, 5);
                            }
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

                else if (ingameState.IngameUi.HaggleWindow.IsVisible)// kirac shop
                {
                    foreach (var item in ingameState.IngameUi.HaggleWindow.InventoryItems)

                    {
                        var mapComponent = item.Item.GetComponent<MapKey>();

                        if (mapComponent == null)
                            continue;

                        var drawRect = item.GetClientRect();
                        drawRect.X = drawRect.X;
                        drawRect.Y = drawRect.Y;

                        var mapArea = mapComponent.GetArea();

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

            var bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            var comp = serverData.CompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
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
                            
                            var mapComponent = item.Item.GetComponent<MapKey>();
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

            var bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            var comp = serverData.CompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            //var shapered = serverData.ShaperElderAreas;

            var drawListPos = new Vector2(200, 200);
            var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            List<string> MapAreasInBag = GetFilteredCompletableItems(inventoryZone, mapCanGiveCompletion);
            var KiracPanel = GameController.Game.IngameState.IngameUi.ZanaMissionChoice;

            foreach (var item in purchaseWindowInvent.InventorySlotItems)
            {

                var mapComponent = item.Item.GetComponent<MapKey>();
                var mapGridX = item.PosX;

                if (mapComponent == null)
                    continue;

                var drawRect = item.GetClientRect();
                var mapArea = mapComponent.GetArea();
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
                    Graphics.DrawFrame(new RectangleF(drawRect.X - 962, drawRect.Y - 330, drawRect.Width, drawRect.Height), Settings.UncompletedMaps, 5);
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
            var bonusComp = serverData.BonusCompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();
            var comp = serverData.CompletedNodes.Select(n => n.Area).Where(a => a != null).ToList();


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
                    if (Settings.Debug)
                        LogMessage("Best maps to run " + mapToRun.Item1.ToStringBestMapsToRun(), 5, Color.Pink);
                    //Graphics.DrawImage("ImagesAtlas.png", mapToRun.Item2, new RectangleF(.184f, .731f, .184f, .269f), Color.Pink);
                    if (disableOnHover && disableOnHoverRect.Intersects(mapToRun.Item3))
                        continue;
                    //Graphics.DrawImage("ImagesAtlas.png", mapToRun.Item2, new RectangleF(.184f, .731f, .184f, .269f), Color.Pink);
                    if (Settings.EnableDiagonalProgressionHighlight)
                    {
                        if (willGiveCompletion)
                        {
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
                var mapComponent = entity.GetComponent<MapKey>();
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

                var area = mapComponent.GetArea();
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


// Added for current ExileCore: the MapKey component (formerly Map) no longer exposes its WorldArea,
// so resolve it from the item's base name (or unique name).
internal static class MapKeyAreaExtensions
{
    public static ExileCore.PoEMemory.MemoryObjects.WorldArea GetArea(this ExileCore.PoEMemory.Components.MapKey mapKey)
    {
        var owner = mapKey?.Owner;
        if (owner == null) return null;
        var mods = owner.GetComponent<ExileCore.PoEMemory.Components.Mods>();
        var name = !string.IsNullOrEmpty(mods?.UniqueName)
            ? mods.UniqueName
            : owner.GetComponent<ExileCore.PoEMemory.Components.Base>()?.Name?.Replace(" Map", "");
        if (string.IsNullOrEmpty(name)) return null;
        return mapKey.TheGame.Files.WorldAreas.EntriesList.FirstOrDefault(a => a.Name == name);
    }
}
