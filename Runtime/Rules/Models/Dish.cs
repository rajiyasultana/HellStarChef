using System.Collections.Generic;

namespace HellsterChef.Core.Models
{
    public sealed class Dish
    {
        public string Name { get; set; }
        public List<Ingredient> Ingredients { get; set; }

        public Dish(string name)
        {
            Name = name;
            Ingredients = new List<Ingredient>();
        }
    }
}
