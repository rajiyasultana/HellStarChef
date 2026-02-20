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

                string[] parts = line.Split(',');
                string name = parts.Length > 0 ? parts[0].Trim() : throw new InvalidDataException("Missing dish name");
                string rulesPart = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                string tagsPart = parts.Length > 2 ? parts[2].Trim() : string.Empty;

                List<DishRule> rules = new List<DishRule>();

                if (!string.IsNullOrEmpty(rulesPart))
                {
                    string[] rulePairs = rulesPart.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string pair in rulePairs)
                    {
                        string[] kv = pair.Split('=', StringSplitOptions.RemoveEmptyEntries);
                        if (kv.Length != 2) continue;
                        string key = kv[0].Trim();
                        string valStr = kv[1].Trim();
                        if (!double.TryParse(valStr, out double val)) continue;

                        if (key.EndsWith("_min", StringComparison.OrdinalIgnoreCase))
                        {
                            string fname = key.Substring(0, key.Length - 4);
                            if (Enum.TryParse<Flavor>(fname, true, out Flavor f))
                            {
                                rules.Add(new FlavorThresholdRule(f, min: val));
                            }
                        }
                        else if (key.EndsWith("_max", StringComparison.OrdinalIgnoreCase))
                        {
                            string fname = key.Substring(0, key.Length - 4);
                            if (Enum.TryParse<Flavor>(fname, true, out Flavor f))
                            {
                                rules.Add(new FlavorThresholdRule(f, max: val));
                            }
                        }
                        else
                        {
                            // unknown rule shape - ignore
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tagsPart))
                {
                    string[] tagParts = tagsPart.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string t in tagParts)
                    {
                        if (Enum.TryParse<SpecialTag>(t.Trim(), true, out SpecialTag st))
                        {
                            rules.Add(new SpecialTagRule(st, required: true));
                        }
                    }
                }

                outList.Add(new Dish(name, rules));
            }

            return outList;
        }
    }
}
