using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Data
{
    public class CsvIngredientRepository : IIngredientRepository
    {
        private readonly string _filePath;

        public CsvIngredientRepository(string filePath)
        {
            _filePath = filePath;
        }

        // Expected CSV columns: Name,Flavors,Rarity,Rawness,IsToxic,SpecialTags,Type
        // Flavors format: "Savory:1;Umami:1;Spicy:0.5"

        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<Ingredient>();
            
            var lines = await File.ReadAllLinesAsync(_filePath);
            var ingredients = new List<Ingredient>();
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Split(',', StringSplitOptions.None);
                if (parts.Length < 7) continue;

                string name = parts[0].Trim();
                string flavors = parts[1].Trim();
                if (flavors.StartsWith("\"") && flavors.EndsWith("\""))
                {
                    flavors = flavors[1..^1];
                }
                if (!int.TryParse(parts[2], out int rarity)) rarity = 0;
                if (!double.TryParse(parts[3], out double rawness)) rawness = 0.0;
                bool isToxic = bool.TryParse(parts[4], out bool tox) && tox;
                string[] tags = parts[5].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string typeStr = parts[6].Trim();
                IngredientType type = Enum.TryParse<IngredientType>(typeStr, true, out IngredientType it) ? it : IngredientType.Vegetable;

                Ingredient ingredient = new Ingredient(name)
                {
                    Rarity = rarity,
                    Rawness = rawness,
                    IsToxic = isToxic,
                    Type = type
                };

                if (!string.IsNullOrWhiteSpace(flavors))
                {
                    string[] pairs = flavors.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string p in pairs)
                    {
                        string[] kv = p.Split(':', StringSplitOptions.TrimEntries);
                        if (kv.Length != 2) continue;
                        if (Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                        {
                            ingredient.FlavorProfile[f] = v;
                        }
                    }
                }

                foreach (string t in tags)
                {
                    if (Enum.TryParse<SpecialTag>(t, true, out SpecialTag st)) ingredient.SpecialTags.Add(st);
                }

                ingredients.Add(ingredient);
            }
            
            return ingredients;
        }
    }
}
