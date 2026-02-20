using System.Collections.Generic;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Services
{
    public interface IDishEvaluator
    {
        DishEvaluationResult Evaluate(Dish dish, IEnumerable<Ingredient> ingredients);
    }
}
