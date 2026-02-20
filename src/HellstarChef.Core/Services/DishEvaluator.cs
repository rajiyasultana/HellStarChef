using System.Collections.Generic;
using System.Linq;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Services
{
    public class DishEvaluationResult
    {
        public bool IsMatch { get; }
        public FlavorTotals Totals { get; }
        public HashSet<SpecialTag> Tags { get; }

        public DishEvaluationResult(bool isMatch, FlavorTotals totals, HashSet<SpecialTag> tags)
        {
            IsMatch = isMatch;
            Totals = totals;
            Tags = tags;
        }
    }

    public static class DishEvaluator
    {
        public static DishEvaluationResult Evaluate(Dish dish, IEnumerable<Ingredient> ingredients)
        {
            var totals = new Dictionary<Flavor, double>();
            var tags = new HashSet<SpecialTag>();

            foreach (var ing in ingredients)
            {
                foreach (var kv in ing.FlavorProfile)
                {
                    if (!totals.ContainsKey(kv.Key)) totals[kv.Key] = 0.0;
                    totals[kv.Key] += kv.Value;
                }

                foreach (var t in ing.SpecialTags) tags.Add(t);
            }

            var flavorTotals = new FlavorTotals(totals);

            var all = dish.Rules.All(r => r.IsSatisfied(flavorTotals, tags));
            return new DishEvaluationResult(all, flavorTotals, tags);
        }
    }
}
