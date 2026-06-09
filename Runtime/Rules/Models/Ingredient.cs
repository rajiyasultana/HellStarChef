using System.Collections.Generic;

namespace HellsterChef.Core.Models
{
    public sealed class Ingredient
    {
        public string Name { get; set; }
        public Dictionary<Flavor, double> FlavorProfile { get; set; }
        public int Rarity { get; set; }
        public double Rawness { get; set; }
        public bool IsToxic { get; set; }
        public HashSet<SpecialTag> SpecialTags { get; set; }
        public IngredientType Type { get; set; }

        public Ingredient(string name)
        {
            Name = name;
            FlavorProfile = new Dictionary<Flavor, double>();
            SpecialTags = new HashSet<SpecialTag>();
        }
    }
}
