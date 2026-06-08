using System.Collections.Generic;
using System.Threading.Tasks;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Data
{
    public interface IDishRuleRepository
    {
        Task<IEnumerable<DishRule>> GetAllAsync();
    }
}