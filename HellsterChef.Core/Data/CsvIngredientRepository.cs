using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Data
{
    public static class CsvIngredientRepository
    {
        // Expected CSV columns: Name,Flavors,Rarity,Rawness,IsToxic,SpecialTags
        // Flavors format: "Savory:1;Umami:1;Spicy:0.5"

        public static IEnumerable<Ingredient> ReadFromFile(string path)
        {
            if (!File.Exists(path)) yield break;
            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Split(',', StringSplitOptions.None);
                if (parts.Length < 6) continue;

                string name = parts[0].Trim();
                string flavors = parts[1].Trim();
                if (!int.TryParse(parts[2], out int rarity)) rarity = 0;
                if (!double.TryParse(parts[3], out double rawness)) rawness = 0.0;
                bool isToxic = bool.TryParse(parts[4], out bool tox) && tox;
                string[] tags = parts[5].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                Ingredient ingredient = new Ingredient(name)
                {
                    Rarity = rarity,
                    Rawness = rawness,
                    IsToxic = isToxic
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

                yield return ingredient;
            }
        }
    }
}
