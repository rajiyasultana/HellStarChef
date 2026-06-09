using System.Collections.Generic;

namespace HellsterChef.Core.Models
{
    public sealed class IngredientCompatibility
    {
        public string Ingredient { get; set; }
        public Dictionary<Flavor, (double Min, double Max, double MaxTolerable)> FlavorRanges { get; set; }

        public IngredientCompatibility(string ingredient)
        {
            Ingredient = ingredient;
            FlavorRanges = new Dictionary<Flavor, (double, double, double)>();
        }
    }
}