using System.Collections.Generic;
using HellsterChef.Core.Models;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Services
{
    public interface IDishEvaluator
    {
        DishEvaluationResult Evaluate(Dish dish, DishRule rule);
        List<string> DiscoverDishes(Dish dish, IEnumerable<DishRule> allRules);
    }
}
