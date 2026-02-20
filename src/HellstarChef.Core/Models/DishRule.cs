using System.Collections.Generic;

namespace HellstarChef.Core.Models
{
    public record FlavorTotals(Dictionary<Flavor, double> Totals)
    {
        public double Get(Flavor f) => Totals.TryGetValue(f, out var v) ? v : 0.0;
    }

    public abstract class DishRule
    {
        public abstract bool IsSatisfied(FlavorTotals totals, HashSet<SpecialTag> tags);
    }

    public class FlavorThresholdRule : DishRule
    {
        public Flavor Flavor { get; }
        public double? Min { get; }
        public double? Max { get; }

        public FlavorThresholdRule(Flavor flavor, double? min = null, double? max = null)
        {
            Flavor = flavor;
            Min = min;
            Max = max;
        }

        public override bool IsSatisfied(FlavorTotals totals, HashSet<SpecialTag> tags)
        {
            var value = totals.Get(Flavor);
            if (Min.HasValue && value < Min.Value) return false;
            if (Max.HasValue && value > Max.Value) return false;
            return true;
        }
    }

    public class SpecialTagRule : DishRule
    {
        public SpecialTag Tag { get; }
        public bool Required { get; }

        public SpecialTagRule(SpecialTag tag, bool required = true)
        {
            Tag = tag;
            Required = required;
        }

        public override bool IsSatisfied(FlavorTotals totals, HashSet<SpecialTag> tags)
        {
            var has = tags.Contains(Tag);
            return Required ? has : !has;
        }
    }
}
