using System.Collections.Generic;
using System.Threading.Tasks;
using HellsterChef.Core.Models;

namespace HellsterChef.Core.Data
{
    public interface ISynergyRepository
    {
        Task<IEnumerable<IngredientSynergy>> GetAllAsync();
    }
}