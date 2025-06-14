using ExileCore.PoEMemory.MemoryObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasHelper
{
    public class AtlasMap
    {
        public WorldArea WorldArea { get; set; }
        public int BaseTier { get; set; }
        public List<AtlasMap> AdjacentMaps { get; set; }
        public bool Completed { get; set; }

        
        // Constructor to initialize the properties
        public AtlasMap(WorldArea worldArea, int baseTier, List<AtlasMap> adjacentMaps, bool completed)
        {
            WorldArea = worldArea;
            BaseTier = baseTier;
            AdjacentMaps = adjacentMaps;
            Completed = completed;
        }
        public void PrintAdjacentMaps()
        {
            Console.WriteLine($"Adjacent Maps to {WorldArea.Name}: {string.Join(", ", AdjacentMaps.Select(map => map.WorldArea.Name))}");
        }
        public override string ToString()
        {
            var adjacentMapNames = string.Join(", ", AdjacentMaps.Select(map => map.WorldArea.Name + " T" + map.BaseTier));
            return $"{WorldArea}, BaseTier: {BaseTier}, Completed: {Completed}, Adjacent Maps: [{adjacentMapNames}]";
        }

        public string ToStringBestMapsToRun()
        {
            var adjacentMapNames = string.Join(", ", AdjacentMaps.Where(map => !map.Completed).Select(map => map.WorldArea.Name + " T" + map.BaseTier) );
            return $" {WorldArea}, BaseTier: {BaseTier}, Completed: {Completed}, Uncompleted Adjacent Maps: [{adjacentMapNames}], Sum of base tiers of those maps {SumOfBaseTiersOfUncompletedAdjacentMaps()}";
        }

        public double AverageBaseTierOfUncompletedAdjacentMaps()
        {
            var uncompletedAdjacentMaps = AdjacentMaps.Where(map => !map.Completed).ToList();
            if (!uncompletedAdjacentMaps.Any())
            {
                return 0; // If no uncompleted adjacent maps, return 0
            }
            return uncompletedAdjacentMaps.Average(map => map.BaseTier);
        }

        public int CountOfUncompletedAdjacentMaps()
        {
            return AdjacentMaps.Count(map => !map.Completed);
        }

        
        public int SumOfBaseTiersOfUncompletedAdjacentMaps()
        {
            return AdjacentMaps?.Where(map => !map.Completed).Sum(map => map.BaseTier) ?? 0;
        }
    }
}
