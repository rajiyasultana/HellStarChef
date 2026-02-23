using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using HellsterChef.Core.Models;
using HellsterChef.Core.Rules;
using HellsterChef.Core.Services;
using HellsterChef.Core.Data;

namespace HellstarChef.Core.Tests
{
    [TestFixture]
    public class DishEvaluatorTests
    {
        [Test]
        public void InfernoMemoryStew_IsDiscovered_WhenIngredientsSelected()
        {
            // Load ingredients from CSV
            string ingredientsPath = @"d:\Rajiya\HellStarChef\data\ingredients.csv";
            List<Ingredient> pool = CsvIngredientRepository.ReadFromFile(ingredientsPath).ToList();
            Assert.IsNotEmpty(pool, "Ingredients CSV should load data");

            // Load rules from CSV
            string rulesPath = @"d:\Rajiya\HellStarChef\data\dishrules.csv";
            List<DishRule> rules = CsvDishRuleRepository.ReadFromFile(rulesPath).ToList();
            DishRule? rule = rules.FirstOrDefault(r => r.Name == "Inferno Memory Stew");
            Assert.IsNotNull(rule, "Dish rule should be loaded");

            // Player selects ingredients
            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "FirePaper", "FirePaper", "Fish", "Herb" };
            Dish dish = chef.MixByName("Inferno Memory Stew", selection, pool);

            DishEvaluator eval = new DishEvaluator();

            // Act
            DishEvaluationResult res = eval.Evaluate(dish, rule);

            // Assert
            Assert.IsTrue(res.Matches, string.Join("; ", res.Reasons));
        }
    }
}
