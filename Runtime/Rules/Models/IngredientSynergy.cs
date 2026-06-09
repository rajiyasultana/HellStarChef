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
        public string Ingredient1 { get; set; }
        public string Ingredient2 { get; set; }
        public SynergyEffect Effect { get; set; }
        public string Description { get; set; }
        // Score modifier applied to overall compatibility when this synergy matches
        public double ScoreModifier { get; set; }
        // If true, this synergy causes a hard failure (dish garbage) when matched (e.g., true for Poison)
        public bool HardFail { get; set; }
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