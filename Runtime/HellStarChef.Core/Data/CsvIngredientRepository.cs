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
            
            // Replaced ReadAllLinesAsync with Task.Run for older .NET versions
            var lines = await Task.Run(() => File.ReadAllLines(_filePath));
            var ingredients = new List<Ingredient>();
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Split(new[] { ',' }, StringSplitOptions.None);
                if (parts.Length < 7) continue;

                string name = parts[0].Trim();
                string flavors = parts[1].Trim();
                
                // Replaced C# 8 range indexer flavors[1..^1] with Substring
                if (flavors.StartsWith("\"") && flavors.EndsWith("\"") && flavors.Length >= 2)
                {
                    flavors = flavors.Substring(1, flavors.Length - 2);
                }
                
                if (!int.TryParse(parts[2], out int rarity)) rarity = 0;
                if (!double.TryParse(parts[3], out double rawness)) rawness = 0.0;
                bool isToxic = bool.TryParse(parts[4], out bool tox) && tox;
                
                // Removed TrimEntries, using LINQ instead
                string[] tags = parts[5].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim())
                                        .ToArray();
                                        
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
                    // Removed TrimEntries, using LINQ instead
                    string[] pairs = flavors.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(p => p.Trim())
                                            .ToArray();
                                            
                    foreach (string p in pairs)
                    {
                        string[] kv = p.Split(new[] { ':' }, StringSplitOptions.None)
                                       .Select(k => k.Trim())
                                       .ToArray();
                                       
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
