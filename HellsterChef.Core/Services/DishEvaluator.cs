using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HellsterChef.Core.Models;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Services
{
    public sealed class DishEvaluationResult
    {
        public bool Matches { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public DishQuality Quality { get; set; }
        public bool IsPoisoned { get; set; }
    }

    public sealed class DishEvaluator
    {
        public DishEvaluationResult Evaluate(Dish dish, DishRule rule)
        {
            DishEvaluationResult result = new DishEvaluationResult();

            // Check for at least one main ingredient (Protein, Vegetable, or Grain)
            bool hasBase = dish.Ingredients.Any(i => i.Type == IngredientType.Protein || i.Type == IngredientType.Vegetable || i.Type == IngredientType.Grain);
            if (!hasBase)
            {
                result.Reasons.Add("No main ingredient (protein, vegetable, or grain) found; cannot form a proper dish.");
                result.Matches = false;
                result.Quality = DishQuality.Dumb;
                return result;
            }

            // aggregate flavor values from ingredients
            Dictionary<Flavor, double> flavorTotals = new Dictionary<Flavor, double>();
            foreach (Ingredient ing in dish.Ingredients)
            {
                foreach (KeyValuePair<Flavor, double> kv in ing.FlavorProfile)
                {
                    if (!flavorTotals.ContainsKey(kv.Key)) flavorTotals[kv.Key] = 0.0;
                    flavorTotals[kv.Key] += kv.Value;
                }
            }
            
            // Check for poisonous or strongly incompatible ingredient synergies
            var poisonReasons = CheckPoisonousSynergies(dish.Ingredients);
            if (poisonReasons.Any())
            {
                result.IsPoisoned = true;
                result.Matches = false;
                result.Quality = DishQuality.Dumb;
                result.Reasons.AddRange(poisonReasons);
                return result;
            }

            bool allOk = true;
            int closeCount = 0;
            int exactCount = 0;
            foreach (Condition cond in rule.Conditions)
            {
                if (cond.Flavor is not null)
                {
                    flavorTotals.TryGetValue(cond.Flavor.Value, out double total);
                    bool ok = cond.Evaluate(total);
                    if (!ok)
                    {
                        allOk = false;
                        double diff = cond.Comparison == Comparison.GreaterOrEqual ? cond.Value - total : total - cond.Value;
                        if (Math.Abs(diff) <= 0.5) closeCount++;
                        result.Reasons.Add($"Flavor {cond.Flavor} check failed: got {total}, needed {cond.Comparison} {cond.Value}");
                    }
                    else
                    {
                        // Check if exact
                        if (cond.Comparison == Comparison.GreaterOrEqual && total >= cond.Value && total <= cond.Value + 0.1) exactCount++;
                        else if (cond.Comparison == Comparison.LessOrEqual && total <= cond.Value && total >= cond.Value - 0.1) exactCount++;
                        else if (cond.Comparison == Comparison.Equal && Math.Abs(total - cond.Value) < 0.1) exactCount++;
                    }
                }
                else if (cond.RequiresTag is not null)
                {
                    bool has = dish.Ingredients.Any(i => i.SpecialTags.Contains(cond.RequiresTag.Value));
                    if (has != cond.RequiresTagPresence)
                    {
                        allOk = false;
                        result.Reasons.Add($"Tag {cond.RequiresTag} presence check failed: expected {cond.RequiresTagPresence}");
                    }
                    else
                    {
                        exactCount++; // Tags are exact
                    }
                }
                else if (cond.RequiredBaseNames is not null && cond.RequiredBaseNames.Count > 0)
                {
                    bool has = cond.RequiredBaseNames.Any(name => dish.Ingredients.Any(i => i.Name == name));
                    if (!has)
                    {
                        allOk = false;
                        result.Reasons.Add($"Required base ingredient not found; options: {string.Join(", ", cond.RequiredBaseNames)}");
                    }
                    else
                    {
                        exactCount++; // Base is exact
                    }
                }
            }

            result.Matches = allOk && result.Reasons.Count == 0;

            // Determine quality
            bool hasToxic = dish.Ingredients.Any(i => i.IsToxic);
            int baseCount = dish.Ingredients.Count(i => i.Type == IngredientType.Protein || i.Type == IngredientType.Vegetable || i.Type == IngredientType.Grain);
            if (!result.Matches)
            {
                if (closeCount > 0 && !hasToxic && baseCount == 1)
                    result.Quality = DishQuality.Good;
                else
                    result.Quality = DishQuality.Dumb;
            }
            else
            {
                if (exactCount == rule.Conditions.Count && !hasToxic && baseCount == 1)
                    result.Quality = DishQuality.Excellent;
                else
                    result.Quality = DishQuality.Good;
            }

            return result;
        }

        private List<string> CheckPoisonousSynergies(IEnumerable<Ingredient> ingredients)
        {
            var issues = new List<string>();
            string path = Path.Combine("d:\\Rajiya\\HellStarChef", "data", "synergies.csv");
            if (!File.Exists(path)) return issues;

            var lines = File.ReadAllLines(path);
            var ingNames = new HashSet<string>(ingredients.Select(i => i.Name));

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;
                // Expect: Ingredient1,Ingredient2,Effect,Description
                string[] parts = line.Split(',', StringSplitOptions.None);
                if (parts.Length < 4) continue;
                string a = parts[0].Trim();
                string b = parts[1].Trim();
                string effect = parts[2].Trim();
                string desc = parts[3].Trim();

                if (ingNames.Contains(a) && ingNames.Contains(b))
                {
                    if (effect.Equals("Poison", StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add($"Poisonous combination detected: {desc} ({a} + {b})");
                    }
                    else if (effect.Equals("Penalty", StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add($"Unfavorable pairing: {desc} ({a} + {b})");
                    }
                }
            }

            return issues;
        }

        // Discover which dishes match the mixed ingredients
        public List<string> DiscoverDishes(Dish dish, IEnumerable<DishRule> allRules)
        {
            List<string> discovered = new List<string>();
            foreach (DishRule rule in allRules)
            {
                DishEvaluationResult res = Evaluate(dish, rule);
                if (res.Matches)
                {
                    discovered.Add(rule.Name);
                }
            }
            return discovered;
        }
    }
}
