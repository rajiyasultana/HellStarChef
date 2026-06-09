using System;
using System.Collections.Generic;
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
        public Flavor? Flavor { get; set; }
        public Comparison Comparison { get; set; }
        public double Value { get; set; }

        public SpecialTag? RequiresTag { get; set; }
        public bool RequiresTagPresence { get; set; }

        public List<string> RequiredBaseNames { get; set; }
        public bool RequireAllBases { get; set; }

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
