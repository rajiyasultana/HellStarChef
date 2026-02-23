using System.Collections.Generic;

namespace HellsterChef.Core.Models
{
    public sealed class Ingredient
    {
        public string Name { get; init; }
        public Dictionary<Flavor, double> FlavorProfile { get; init; }
        public int Rarity { get; init; }
        public double Rawness { get; init; }
        public bool IsToxic { get; init; }
        public HashSet<SpecialTag> SpecialTags { get; init; }

        public Ingredient(string name)
        {
            Name = name;
            FlavorProfile = new Dictionary<Flavor, double>();
            SpecialTags = new HashSet<SpecialTag>();
        }
    }
}
