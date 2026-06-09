using System.Collections.Generic;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Rules
{
    public sealed class DishRule
    {
        public string Name { get; set; }
        public List<Condition> Conditions { get;   set; }

        public DishRule(string name)
        {
            Name = name;
            Conditions = new List<Condition>();
        }
    }
}
