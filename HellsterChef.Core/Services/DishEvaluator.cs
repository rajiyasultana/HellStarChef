using System.Collections.Generic;
using System.Linq;
using HellsterChef.Core.Models;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Services
{
    public sealed class DishEvaluationResult
    {
        public bool Matches { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
    }

    public sealed class DishEvaluator
    {
        public DishEvaluationResult Evaluate(Dish dish, DishRule rule)
        {
            DishEvaluationResult result = new DishEvaluationResult();

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

            bool allOk = true;
            foreach (Condition cond in rule.Conditions)
            {
                if (cond.Flavor is not null)
                {
                    flavorTotals.TryGetValue(cond.Flavor.Value, out double total);
                    bool ok = cond.Evaluate(total);
                    if (!ok)
                    {
                        allOk = false;
                        result.Reasons.Add($"Flavor {cond.Flavor} check failed: got {total}, needed {cond.Comparison} {cond.Value}");
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
                }
            }

            result.Matches = allOk && result.Reasons.Count == 0;
            return result;
        }
    }
}
