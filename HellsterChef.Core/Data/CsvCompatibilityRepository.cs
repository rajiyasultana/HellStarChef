using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Data
{
    public class CsvCompatibilityRepository : ICompatibilityRepository
    {
        private readonly string _filePath;

        public CsvCompatibilityRepository(string filePath)
        {
            _filePath = filePath;
        }

        // CSV: Ingredient,Flavor,PreferredMin,PreferredMax,MaxTolerable
        public async Task<IEnumerable<IngredientCompatibility>> GetAllAsync()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<IngredientCompatibility>();

            var map = new Dictionary<string, IngredientCompatibility>(StringComparer.OrdinalIgnoreCase);
            var lines = await File.ReadAllLinesAsync(_filePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;
                string[] parts = line.Split(',', StringSplitOptions.None);
                if (parts.Length < 5) continue;
                string ing = parts[0].Trim();
                string flavorStr = parts[1].Trim();
                if (!Enum.TryParse<Flavor>(flavorStr, true, out Flavor f)) continue;
                if (!double.TryParse(parts[2], out double min)) min = 0.0;
                if (!double.TryParse(parts[3], out double max)) max = 0.0;
                if (!double.TryParse(parts[4], out double tol)) tol = max + 1.0;

                if (!map.ContainsKey(ing)) map[ing] = new IngredientCompatibility(ing);
                map[ing].FlavorRanges[f] = (min, max, tol);
            }

            return map.Values;
        }
    }
}