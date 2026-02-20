using System.Collections.Generic;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Data
{
    public static class IngredientRepository
    {
        public static Ingredient Fish => new Ingredient(
            "Fish",
            new Dictionary<Flavor, double> { { Flavor.Savory, 1.0 }, { Flavor.Umami, 1.0 } },
            rarity: 1,
            rawness: 1.0,
            isToxic: false);

        public static Ingredient FirePaper => new Ingredient(
            "FirePaper",
            new Dictionary<Flavor, double> { { Flavor.Spicy, 1.0 }, { Flavor.Bitter, 0.5 } },
            rarity: 1);

        public static Ingredient Herb => new Ingredient(
            "Herb",
            new Dictionary<Flavor, double> { { Flavor.Sweet, 0.5 } },
            rarity: 1,
            specialTags: new[] { SpecialTag.Memory });

        public static Ingredient Ice => new Ingredient(
            "Ice",
            new Dictionary<Flavor, double>(),
            rarity: 1);

        public static IEnumerable<Ingredient> DefaultSet() => new[] { Fish, FirePaper, Herb, Ice };
    }
}
