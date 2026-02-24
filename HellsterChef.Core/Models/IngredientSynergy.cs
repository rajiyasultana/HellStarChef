namespace HellsterChef.Core.Models
{
    public enum SynergyEffect
    {
        Bonus,
        Penalty,
        Poison
    }

    public sealed class IngredientSynergy
    {
        public string Ingredient1 { get; init; }
        public string Ingredient2 { get; init; }
        public SynergyEffect Effect { get; init; }
        public string Description { get; init; }

        public IngredientSynergy(string ing1, string ing2, SynergyEffect effect, string desc)
        {
            Ingredient1 = ing1;
            Ingredient2 = ing2;
            Effect = effect;
            Description = desc;
        }
    }
}