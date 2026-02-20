using System;
using System.Collections.Generic;
using System.IO;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Data
{
    public static class CsvParsing
    {
        public static string[] SplitCsvLine(string line) => line.Split(',');

        public static Dictionary<Flavor, double> ParseFlavorDict(string flavorPart)
        {
            var dict = new Dictionary<Flavor, double>();
            if (string.IsNullOrWhiteSpace(flavorPart)) return dict;
            var pairs = flavorPart.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length != 2) continue;
                var key = kv[0].Trim();
                var valStr = kv[1].Trim();
                if (Enum.TryParse<Flavor>(key, true, out var f) && double.TryParse(valStr, out var d))
                {
                    dict[f] = d;
                }
            }
            return dict;
        }

        public static List<SpecialTag> ParseSpecialTags(string tagsPart)
        {
            var list = new List<SpecialTag>();
            if (string.IsNullOrWhiteSpace(tagsPart)) return list;
            var parts = tagsPart.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in parts)
            {
                if (Enum.TryParse<SpecialTag>(t.Trim(), true, out var st)) list.Add(st);
            }
            return list;
        }

        public static List<DishRule> ParseDishRules(string rulesPart)
        {
            var rules = new List<DishRule>();
            if (string.IsNullOrWhiteSpace(rulesPart)) return rules;
            var pairs = rulesPart.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (kv.Length != 2) continue;
                var key = kv[0].Trim();
                var valStr = kv[1].Trim();
                if (!double.TryParse(valStr, out double val)) continue;

                if (key.EndsWith("_min", StringComparison.OrdinalIgnoreCase))
                {
                    string fname = key.Substring(0, key.Length - 4);
                    if (Enum.TryParse<Flavor>(fname, true, out var f))
                    {
                        rules.Add(new FlavorThresholdRule(f, min: val));
                    }
                }
                else if (key.EndsWith("_max", StringComparison.OrdinalIgnoreCase))
                {
                    string fname = key.Substring(0, key.Length - 4);
                    if (Enum.TryParse<Flavor>(fname, true, out var f))
                    {
                        rules.Add(new FlavorThresholdRule(f, max: val));
                    }
                }
            }
            return rules;
        }
    }
}
