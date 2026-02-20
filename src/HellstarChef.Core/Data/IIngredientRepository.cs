using System.Collections.Generic;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Data
{
    public interface IIngredientRepository
    {
        IEnumerable<Ingredient> Load(string path);
    }
}
