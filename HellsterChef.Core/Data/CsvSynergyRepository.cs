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

                // Optional columns: ScoreModifier, HardFail
                double scoreModifier = 0.0;
                bool hardFail = false;
                if (parts.Length >= 5)
                {
                    double.TryParse(parts[4].Trim(), out scoreModifier);
                }
                if (parts.Length >= 6)
                {
                    bool.TryParse(parts[5].Trim(), out hardFail);
                }

                if (Enum.TryParse<SynergyEffect>(effectStr, true, out SynergyEffect effect))
                {
                    // Default behaviors when values not provided
                    if (parts.Length < 5)
                    {
                        switch (effect)
                        {
                            case SynergyEffect.Bonus: scoreModifier = 0.5; break;
                            case SynergyEffect.Penalty: scoreModifier = -0.5; break;
                            case SynergyEffect.Incompatible: scoreModifier = -2.0; break;
                            case SynergyEffect.Poison: scoreModifier = -10.0; hardFail = true; break;
                        }
                    }

                    synergies.Add(new IngredientSynergy(ing1, ing2, effect, desc, scoreModifier, hardFail));
                }
            }

            return synergies;
        }
    }
}