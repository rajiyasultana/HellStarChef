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
    public class DishEvaluator : IDishEvaluator
    {
        public DishEvaluationResult Evaluate(Dish dish, IEnumerable<Ingredient> ingredients)
        {
            Dictionary<Flavor, double> totals = new Dictionary<Flavor, double>();
            HashSet<SpecialTag> tags = new HashSet<SpecialTag>();

            foreach (Ingredient ing in ingredients)
            {
                if (ing == null) continue;

                foreach (KeyValuePair<Flavor, double> kv in ing.FlavorProfile)
                {
                    if (!totals.ContainsKey(kv.Key)) totals[kv.Key] = 0.0;
                    totals[kv.Key] += kv.Value;
                }

                foreach (SpecialTag t in ing.SpecialTags) tags.Add(t);
            }

            FlavorTotals flavorTotals = new FlavorTotals(totals);

            bool all = dish.Rules.All(r => r.IsSatisfied(flavorTotals, tags));
            return new DishEvaluationResult(all, flavorTotals, tags);
        }
    }
}
