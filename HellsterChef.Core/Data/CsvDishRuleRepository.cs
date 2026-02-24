using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HellsterChef.Core.Models;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Data
{
    public static class CsvDishRuleRepository
    {
        // Expected CSV: Name,Conditions
        // Conditions format: "Flavor>=Value;Tag=true;Flavor<=Value"
        public static IEnumerable<DishRule> ReadFromFile(string path)
        {
            if (!File.Exists(path)) yield break;

            foreach (string line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Split(',', StringSplitOptions.None);
                if (parts.Length < 2) continue;

                string name = parts[0].Trim();
                string conditionsStr = parts[1].Trim();

                DishRule rule = new DishRule(name);

                if (!string.IsNullOrWhiteSpace(conditionsStr))
                {
                    string[] condParts = conditionsStr.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string cond in condParts)
                    {
                        Condition? parsed = ParseCondition(cond);
                        if (parsed is not null) rule.Conditions.Add(parsed);
                    }
                }

                yield return rule;
            }
        }

        private static Condition? ParseCondition(string cond)
        {
            // Examples: "Spicy>=2.0", "Memory=true", "Bitter<=0.7"
            if (cond.Contains(">="))
            {
                string[] kv = cond.Split(">=", StringSplitOptions.TrimEntries);
                if (kv.Length == 2 && Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                    return new Condition { Flavor = f, Comparison = Comparison.GreaterOrEqual, Value = v };
            }
            else if (cond.Contains("<="))
            {
                string[] kv = cond.Split("<=", StringSplitOptions.TrimEntries);
                if (kv.Length == 2 && Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                    return new Condition { Flavor = f, Comparison = Comparison.LessOrEqual, Value = v };
            }
            else if (cond.Contains("=="))
            {
                string[] kv = cond.Split("==", StringSplitOptions.TrimEntries);
                if (kv.Length == 2 && Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                    return new Condition { Flavor = f, Comparison = Comparison.Equal, Value = v };
            }
            else if (cond.Contains("=") && !cond.Contains(">=") && !cond.Contains("<=") && !cond.Contains("=="))
            {
                // Tag presence: "Memory=true" or "Base=Beef"
                string[] kv = cond.Split('=', StringSplitOptions.TrimEntries);
                if (kv.Length == 2)
                {
                    if (Enum.TryParse<SpecialTag>(kv[0], true, out SpecialTag t) && bool.TryParse(kv[1], out bool presence))
                        return new Condition { RequiresTag = t, RequiresTagPresence = presence };
                    else if (kv[0] == "Base")
                        return new Condition { RequiredBaseName = kv[1] };
                }
            }
            return null;
        }
    }
}