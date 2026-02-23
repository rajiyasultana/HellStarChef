using System.Collections.Generic;
using System.Linq;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Services
{
    public sealed class PlayerChef
    {
        // Mix ingredients by name from an available pool; returns a new Dish
        public Dish MixByName(IEnumerable<string> ingredientNames, IEnumerable<Ingredient> pool)
        {
            Dish dish = new Dish("Mixed Dish"); // Generic name; discovery happens later

            List<Ingredient> poolList = pool.ToList();
            foreach (string name in ingredientNames)
            {
                Ingredient? found = poolList.FirstOrDefault(i => i.Name == name);
                if (found is not null)
                {
                    // shallow copy to avoid mutating source
                    Ingredient copy = new Ingredient(found.Name)
                    {
                        Rarity = found.Rarity,
                        Rawness = found.Rawness,
                        IsToxic = found.IsToxic
                    };
                    foreach (KeyValuePair<Flavor, double> kv in found.FlavorProfile)
                    {
                        copy.FlavorProfile[kv.Key] = kv.Value;
                    }
                    foreach (SpecialTag t in found.SpecialTags)
                    {
                        copy.SpecialTags.Add(t);
                    }
                    dish.Ingredients.Add(copy);
                }
            }

            return dish;
        }
    }
}
