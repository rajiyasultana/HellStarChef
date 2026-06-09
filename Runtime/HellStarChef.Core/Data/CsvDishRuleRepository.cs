using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HellsterChef.Core.Models;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Data
{
    public class CsvDishRuleRepository : IDishRuleRepository
    {
        private readonly string _filePath;

        public CsvDishRuleRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IEnumerable<DishRule>> GetAllAsync()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<DishRule>();
            
            // ReadAllLinesAsync is not available in .NET Standard 2.1, using Task.Run as a workaround
            var lines = await Task.Run(() => File.ReadAllLines(_filePath));
            var rules = new List<DishRule>();
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Split(new[] { ',' }, StringSplitOptions.None);
                if (parts.Length < 2) continue;

                string name = parts[0].Trim();
                string conditionsStr = parts[1].Trim();

                DishRule rule = new DishRule(name);

                if (!string.IsNullOrWhiteSpace(conditionsStr))
                {
                    string[] condParts = conditionsStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(s => s.Trim())
                                                      .ToArray();

                    foreach (string cond in condParts)
                    {
                        Condition parsed = ParseCondition(cond);
                        if (parsed != null) rule.Conditions.Add(parsed); // Changed 'is not null' to '!= null'
                    }
                }

                rules.Add(rule);
            }
            
            return rules;
        }

        private static Condition ParseCondition(string cond)
        {
            if (cond.Contains(">="))
            {
                string[] kv = cond.Split(new[] { ">=" }, StringSplitOptions.None)
                                  .Select(s => s.Trim())
                                  .ToArray();
                if (kv.Length == 2 && Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                    return new Condition { Flavor = f, Comparison = Comparison.GreaterOrEqual, Value = v };
            }
            else if (cond.Contains("<="))
            {
                string[] kv = cond.Split(new[] { "<=" }, StringSplitOptions.None)
                                  .Select(s => s.Trim())
                                  .ToArray();
                if (kv.Length == 2 && Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                    return new Condition { Flavor = f, Comparison = Comparison.LessOrEqual, Value = v };
            }
            else if (cond.Contains("=="))
            {
                string[] kv = cond.Split(new[] { "==" }, StringSplitOptions.None)
                                  .Select(s => s.Trim())
                                  .ToArray();
                if (kv.Length == 2 && Enum.TryParse<Flavor>(kv[0], true, out Flavor f) && double.TryParse(kv[1], out double v))
                    return new Condition { Flavor = f, Comparison = Comparison.Equal, Value = v };
            }
            else if (cond.Contains("=") && !cond.Contains(">=") && !cond.Contains("<=") && !cond.Contains("=="))
            {
                string[] kv = cond.Split(new[] { '=' }, StringSplitOptions.None)
                                  .Select(s => s.Trim())
                                  .ToArray();
                if (kv.Length == 2)
                {
                    if (Enum.TryParse<SpecialTag>(kv[0], true, out SpecialTag t) && bool.TryParse(kv[1], out bool presence))
                        return new Condition { RequiresTag = t, RequiresTagPresence = presence };
                    else if (kv[0] == "Base")
                    {
                        string val = kv[1];
                        if (val.Contains('+'))
                        {
                            var names = val.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToList();
                            return new Condition { RequiredBaseNames = names, RequireAllBases = true };
                        }
                        else
                        {
                            var names = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToList();
                            return new Condition { RequiredBaseNames = names, RequireAllBases = false };
                        }
                    }
                }
            }
            return null;
        }
    }
}