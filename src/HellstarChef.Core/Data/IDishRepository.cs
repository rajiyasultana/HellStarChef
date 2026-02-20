using System.Collections.Generic;
using HellstarChef.Core.Models;

namespace HellstarChef.Core.Data
{
    public interface IDishRepository
    {
        IEnumerable<Dish> Load(string path);
    }
}
