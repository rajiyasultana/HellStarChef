using System.Collections.Generic;

namespace HellstarChef.Core.Models
{
    public class Dish
    {
        public string Name { get; }
        public List<DishRule> Rules { get; }

        public Dish(string name, IEnumerable<DishRule>? rules = null)
        {
            Name = name;
            Rules = rules != null ? new List<DishRule>(rules) : new List<DishRule>();
        }
    }
}
