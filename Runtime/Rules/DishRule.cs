using System.Collections.Generic;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Rules
{
    public sealed class DishRule
    {
        public string Name { get; init; }
        public List<Condition> Conditions { get; init; }

        public DishRule(string name)
        {
            Name = name;
            Conditions = new List<Condition>();
        }
    }
}
