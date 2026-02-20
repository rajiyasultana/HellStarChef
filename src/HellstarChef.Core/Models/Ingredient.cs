using System.Collections.Generic;

namespace HellstarChef.Core.Models
{
    public class Ingredient
    {
        public string Name { get; }
        public Dictionary<Flavor, double> FlavorProfile { get; }
        public int Rarity { get; }
        public double Rawness { get; }
        public bool IsToxic { get; }
        public HashSet<SpecialTag> SpecialTags { get; }

        public Ingredient(string name,
            Dictionary<Flavor, double>? flavorProfile = null,
            int rarity = 1,
            double rawness = 1.0,
            bool isToxic = false,
            IEnumerable<SpecialTag>? specialTags = null)
        {
            Name = name;
            FlavorProfile = flavorProfile ?? new Dictionary<Flavor, double>();
            Rarity = rarity;
            Rawness = rawness;
            IsToxic = isToxic;
            SpecialTags = specialTags != null ? new HashSet<SpecialTag>(specialTags) : new HashSet<SpecialTag>();
        }
    }
}
