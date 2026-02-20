using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Data
{
    // CSV columns: Name,Rules,SpecialTags
    // Rules format: "Spicy_min=2.0;Umami_min=1.0;Bitter_max=0.7"
    // SpecialTags format: "Memory|Healing"
    public class CsvDishRepository : IDishRepository
    {
        public IEnumerable<Dish> Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Dish CSV not found", path);

            List<Dish> outList = new List<Dish>();
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0) return outList;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = CsvParsing.SplitCsvLine(line);
                string name = parts.Length > 0 ? parts[0].Trim() : throw new InvalidDataException("Missing dish name");
                string rulesPart = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                string tagsPart = parts.Length > 2 ? parts[2].Trim() : string.Empty;

                List<DishRule> rules = CsvParsing.ParseDishRules(rulesPart);

                if (!string.IsNullOrEmpty(tagsPart))
                {
                    var tagList = CsvParsing.ParseSpecialTags(tagsPart);
                    foreach (var st in tagList)
                    {
                        rules.Add(new SpecialTagRule(st, required: true));
                    }
                }

                outList.Add(new Dish(name, rules));
            }

            return outList;
        }
    }
}
