using System.Linq;
using HellstarChef.Core.Data;
using HellstarChef.Core.Models;
using HellstarChef.Core.Services;
using Xunit;

namespace HellstarChef.Core.Tests
{
    public class DishEvaluatorTests
    {
        [Fact]
        public void FlavorAggregation_Works()
        {
            var set = IngredientRepository.DefaultSet();
            var evaluator = new HellstarChef.Core.Services.DishEvaluator();
            var totals = evaluator.Evaluate(new Dish("Test"), set).Totals;
            Assert.Equal(1.0, totals.Get(Flavor.Savory));
            Assert.Equal(1.0, totals.Get(Flavor.Umami));
            Assert.Equal(1.0, totals.Get(Flavor.Spicy));
            Assert.Equal(0.5, totals.Get(Flavor.Bitter));
            Assert.Equal(0.5, totals.Get(Flavor.Sweet));
        }

        [Fact]
        public void SpicySoup_RuleMatches()
        {
            // Spicy soup rule: Spicy ≥ 1.5, Savory ≥ 0.5, Bitter ≤ 1.0
            var ruleDish = new Dish("Spicy Soup", new DishRule[] {
                new FlavorThresholdRule(Flavor.Spicy, min:1.5),
                new FlavorThresholdRule(Flavor.Savory, min:0.5),
                new FlavorThresholdRule(Flavor.Bitter, max:1.0)
            });

            // To reach Spicy 1.5, combine FirePaper (1.0) + another FirePaper
            var ingredients = new[] { IngredientRepository.FirePaper, IngredientRepository.FirePaper, IngredientRepository.Fish };
            var evaluator = new HellstarChef.Core.Services.DishEvaluator();
            var result = evaluator.Evaluate(ruleDish, ingredients);

            Assert.True(result.IsMatch);
        }

        [Fact]
        public void InfernoMemoryStew_RuleMatches()
        {
            // Dish: Inferno Memory Stew
            var dish = new Dish("Inferno Memory Stew", new DishRule[] {
                new FlavorThresholdRule(Flavor.Spicy, min:2.0),
                new FlavorThresholdRule(Flavor.Umami, min:1.0),
                new SpecialTagRule(SpecialTag.Memory, required: true),
                new FlavorThresholdRule(Flavor.Bitter, max:0.7)
            });

            var ingredients = new Ingredient[] {
                IngredientRepository.FirePaper,
                IngredientRepository.Chili,
                IngredientRepository.Fish,
                IngredientRepository.Herb
            };

            var evaluator = new HellstarChef.Core.Services.DishEvaluator();
            var result = evaluator.Evaluate(dish, ingredients);
            Assert.True(result.IsMatch);
        }
    }
}
