using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Data
{
    // Simple CSV loader. Columns: Name,FlavorProfile,Rarity,Rawness,IsToxic,SpecialTags
    // FlavorProfile format: "Savory=1.0;Umami=1.0" (pairs separated by ';', key=Flavor enum)
    // SpecialTags format: "Memory|Healing"
    public class CsvIngredientRepository : IIngredientRepository
    {
        public IEnumerable<Ingredient> Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Ingredient CSV not found", path);

            List<Ingredient> outList = new List<Ingredient>();
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0) return outList;

            // assume first line is header
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // naive CSV split (no quoted commas expected in fields)
                string[] parts = CsvParsing.SplitCsvLine(line);
                string name = parts.Length > 0 ? parts[0].Trim() : throw new InvalidDataException("Missing name");
                string flavorPart = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                string rarityPart = parts.Length > 2 ? parts[2].Trim() : "1";
                string rawnessPart = parts.Length > 3 ? parts[3].Trim() : "1.0";
                string isToxicPart = parts.Length > 4 ? parts[4].Trim() : "false";
                string tagsPart = parts.Length > 5 ? parts[5].Trim() : string.Empty;

                Dictionary<Flavor, double> flavorDict = CsvParsing.ParseFlavorDict(flavorPart);

                int rarity = int.TryParse(rarityPart, out int r) ? r : 1;
                double rawness = double.TryParse(rawnessPart, out double rr) ? rr : 1.0;
                bool isToxic = bool.TryParse(isToxicPart, out bool it) && it;

                List<SpecialTag> tags = CsvParsing.ParseSpecialTags(tagsPart);

                outList.Add(new Ingredient(name, flavorDict, rarity, rawness, isToxic, tags));
            }

            return outList;
        }
    }
}
