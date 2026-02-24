using System.Collections.Generic;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Services
{
    public interface IPlayerChef
    {
        Dish MixByName(IEnumerable<string> ingredientNames, IEnumerable<Ingredient> pool);
    }
}