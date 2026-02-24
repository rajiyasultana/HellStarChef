namespace HellsterChef.Core.Models
{
    public enum SynergyEffect
    {
        Bonus,
        Penalty,
        Poison,
        Incompatible
    }

    public sealed class IngredientSynergy
    {
        public string Ingredient1 { get; init; }
        public string Ingredient2 { get; init; }
        public SynergyEffect Effect { get; init; }
        public string Description { get; init; }
        // Score modifier applied to overall compatibility when this synergy matches
        public double ScoreModifier { get; init; }
        // If true, this synergy causes a hard failure (dish garbage) when matched (e.g., true for Poison)
        public bool HardFail { get; init; }

        public IngredientSynergy(string ing1, string ing2, SynergyEffect effect, string desc, double scoreModifier = 0.0, bool hardFail = false)
        {
            Ingredient1 = ing1;
            Ingredient2 = ing2;
            Effect = effect;
            Description = desc;
            ScoreModifier = scoreModifier;
            HardFail = hardFail;
        }
    }
}