using System.Collections.Generic;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace AtlasHelper
{
    public class AtlasHelperSettings : ISettings
    {
        public Dictionary<string, int> MapStashAmount = new Dictionary<string, int>();
        public Dictionary<string, int[]> MapRegionsAmount = new Dictionary<string, int[]>();

        public AtlasHelperSettings()
        {
            
        }

       

        
        public ToggleNode Enable { get; set; } = new ToggleNode();
        public ToggleNode EnableDiagonalProgressionHighlight { get; set; } = new ToggleNode();
        public ToggleNode Debug { get; set; } = new ToggleNode(true);
        public ToggleNode SimulateHighestCompletedTier { get; set; } = new ToggleNode();
        
        public RangeNode<int> HighestCompletedTierSim { get; set; } = new RangeNode<int>(1, 2, 16);

        //[Menu("League")] // gonna use from GameController.Game.IngameState.ServerData.League
        //public ListNode League { get; set; } = new ListNode();

        public ToggleNode HighlightIgnoredMaps { get; set; } = new ToggleNode();
        public ToggleNode WarnInsideMapToNotComplete { get; set; } = new ToggleNode();
        public ToggleNode ShowCurrencyHint { get; set; } = new ToggleNode();
        public ToggleNode ShowMapTierTopLeft { get; set; } = new ToggleNode();
        public ToggleNode ShowIfMapIsReadyOrNeedsCurrency { get; set; } = new ToggleNode();
        public ToggleNode DrawKiracMapsNaturalTier { get; set; } = new ToggleNode();
        [Menu("Colors")]
        public ColorNode UncompletedMaps { get; set; } = new ColorNode(Color.LightGreen);
        public ColorNode IgnoredMaps { get; set; } = new ColorNode(Color.Red);
        public ColorNode MapReady { get; set; } = new ColorNode(Color.LightGreen);
        public ColorNode MapNeedsCurrency { get; set; } = new ColorNode(Color.Yellow);
        public ColorNode DiagonalProgressionHighlight { get; set; } = new ColorNode(Color.DeepPink);
    }
}