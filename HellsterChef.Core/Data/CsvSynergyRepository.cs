using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Data
{
    public class CsvSynergyRepository : ISynergyRepository
    {
        private readonly string _filePath;

        public CsvSynergyRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IEnumerable<IngredientSynergy>> GetAllAsync()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<IngredientSynergy>();

            var lines = await File.ReadAllLinesAsync(_filePath);
            var synergies = new List<IngredientSynergy>();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Split(',', StringSplitOptions.None);
                if (parts.Length < 4) continue;

                string ing1 = parts[0].Trim();
                string ing2 = parts[1].Trim();
                string effectStr = parts[2].Trim();
                string desc = parts[3].Trim();

                if (Enum.TryParse<SynergyEffect>(effectStr, true, out SynergyEffect effect))
                {
                    synergies.Add(new IngredientSynergy(ing1, ing2, effect, desc));
                }
            }

            return synergies;
        }
    }
}