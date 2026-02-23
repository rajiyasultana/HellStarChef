using System.Collections.Generic;

namespace HellsterChef.Core.Models
{
    public sealed class Dish
    {
        public string Name { get; init; }
        public List<Ingredient> Ingredients { get; init; }

        public Dish(string name)
        {
            Name = name;
            Ingredients = new List<Ingredient>();
        }
    }
}
