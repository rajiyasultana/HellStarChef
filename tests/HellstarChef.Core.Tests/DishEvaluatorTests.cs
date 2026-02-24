using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
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
        public async Task InfernoMemoryStew_IsDiscovered_WhenIngredientsSelected()
        {
            // Load ingredients from CSV
            string ingredientsPath = @"d:\Rajiya\HellStarChef\data\ingredients.csv";
            var ingredientRepo = new CsvIngredientRepository(ingredientsPath);
            List<Ingredient> pool = (await ingredientRepo.GetAllAsync()).ToList();
            Assert.IsNotEmpty(pool, "Ingredients CSV should load data");

            // Load rules from CSV
            string rulesPath = @"d:\Rajiya\HellStarChef\data\dishrules.csv";
            var ruleRepo = new CsvDishRuleRepository(rulesPath);
            List<DishRule> rules = (await ruleRepo.GetAllAsync()).ToList();
            Assert.IsNotEmpty(rules, "Dish rules CSV should load data");

            // Player selects ingredients (without knowing the dish)
            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "FirePaper", "FirePaper", "Fish", "Herb" };
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();

            // Discover which dishes match
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            // Assert that "Inferno Memory Stew" is discovered
            Assert.Contains("Inferno Memory Stew", discovered, "Inferno Memory Stew should be discovered");
        }

        [Test]
        public async Task SweetHealingSoup_IsDiscovered_WithHoneyAndMushroom()
        {
            var ingredientRepo = new CsvIngredientRepository(@"d:\Rajiya\HellStarChef\data\ingredients.csv");
            List<Ingredient> pool = (await ingredientRepo.GetAllAsync()).ToList();
            var ruleRepo = new CsvDishRuleRepository(@"d:\Rajiya\HellStarChef\data\dishrules.csv");
            List<DishRule> rules = (await ruleRepo.GetAllAsync()).ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "Honey", "Mushroom", "Fish" };
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            Assert.Contains("Sweet Healing Soup", discovered, "Sweet Healing Soup should be discovered with Honey, Mushroom, and Fish");
        }

        [Test]
        public async Task SpicyPowerSalad_IsDiscovered_WithChili()
        {
            var ingredientRepo = new CsvIngredientRepository(@"d:\Rajiya\HellStarChef\data\ingredients.csv");
            List<Ingredient> pool = (await ingredientRepo.GetAllAsync()).ToList();
            var ruleRepo = new CsvDishRuleRepository(@"d:\Rajiya\HellStarChef\data\dishrules.csv");
            List<DishRule> rules = (await ruleRepo.GetAllAsync()).ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "Chili", "Ice", "Fish" }; // Chili has Spicy=1.5, Power=true; Ice has no flavors
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            Assert.Contains("Spicy Power Salad", discovered, "Spicy Power Salad should be discovered with Chili");
        }

        [Test]
        public async Task NoDishDiscovered_WithRandomIngredients()
        {
            var ingredientRepo = new CsvIngredientRepository(@"d:\Rajiya\HellStarChef\data\ingredients.csv");
            List<Ingredient> pool = (await ingredientRepo.GetAllAsync()).ToList();
            var ruleRepo = new CsvDishRuleRepository(@"d:\Rajiya\HellStarChef\data\dishrules.csv");
            List<DishRule> rules = (await ruleRepo.GetAllAsync()).ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "Ice", "Ice" }; // Ice has no flavors/tags, won't match any rule
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            Assert.IsEmpty(discovered, "No dishes should be discovered with only Ice");
        }

        [Test]
        public async Task MultipleDishesDiscovered_WithMatchingIngredients()
        {
            var ingredientRepo = new CsvIngredientRepository(@"d:\Rajiya\HellStarChef\data\ingredients.csv");
            List<Ingredient> pool = (await ingredientRepo.GetAllAsync()).ToList();
            var ruleRepo = new CsvDishRuleRepository(@"d:\Rajiya\HellStarChef\data\dishrules.csv");
            List<DishRule> rules = (await ruleRepo.GetAllAsync()).ToList();

            PlayerChef chef = new PlayerChef();
            // Ingredients that could match multiple rules if rules overlap
            List<string> selection = new List<string> { "Honey", "Mushroom", "Chili", "Fish" };
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            // Assuming rules don't overlap, but test for at least one
            Assert.IsNotEmpty(discovered, "At least one dish should be discovered");
        }

        [Test]
        public async Task InvalidIngredientName_DoesNotCrash()
        {
            var ingredientRepo = new CsvIngredientRepository(@"d:\Rajiya\HellStarChef\data\ingredients.csv");
            List<Ingredient> pool = (await ingredientRepo.GetAllAsync()).ToList();
            var ruleRepo = new CsvDishRuleRepository(@"d:\Rajiya\HellStarChef\data\dishrules.csv");
            List<DishRule> rules = (await ruleRepo.GetAllAsync()).ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "NonExistentIngredient", "Fish" };
            Dish dish = chef.MixByName(selection, pool);

            // Should only include Fish
            Assert.AreEqual(1, dish.Ingredients.Count, "Only valid ingredients should be added");
            Assert.AreEqual("Fish", dish.Ingredients[0].Name);
        }
    }
}
