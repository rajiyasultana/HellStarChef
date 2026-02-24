using System;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Rules
{
    public enum Comparison
    {
        GreaterOrEqual,
        LessOrEqual,
        Equal
    }

    public sealed class Condition
    {
        // If Flavor is set, Value is compared against aggregated flavor value
        public Flavor? Flavor { get; init; }
        public Comparison Comparison { get; init; }
        public double Value { get; init; }

        // Tag checks
        public SpecialTag? RequiresTag { get; init; }
        public bool RequiresTagPresence { get; init; }

        // Base ingredient check (can be multiple options).
        // If `RequireAllBases` is true then all names in `RequiredBaseNames` must be present (AND).
        // Otherwise any one of them is sufficient (OR).
        public List<string>? RequiredBaseNames { get; init; }
        public bool RequireAllBases { get; init; }

        public bool Evaluate(double aggregatedFlavorValue)
        {
            return Comparison switch
            {
                Comparison.GreaterOrEqual => aggregatedFlavorValue >= Value,
                Comparison.LessOrEqual => aggregatedFlavorValue <= Value,
                Comparison.Equal => Math.Abs(aggregatedFlavorValue - Value) < 1e-9,
                _ => false
            };
        }
    }
}
