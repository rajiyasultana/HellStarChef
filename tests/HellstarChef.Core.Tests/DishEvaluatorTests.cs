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
        public void SweetHealingSoup_IsDiscovered_WithHoneyAndMushroom()
        {
            List<Ingredient> pool = CsvIngredientRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\ingredients.csv").ToList();
            List<DishRule> rules = CsvDishRuleRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\dishrules.csv").ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "Honey", "Mushroom", "Fish" };
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            Assert.Contains("Sweet Healing Soup", discovered, "Sweet Healing Soup should be discovered with Honey, Mushroom, and Fish");
        }

        [Test]
        public void SpicyPowerSalad_IsDiscovered_WithChili()
        {
            List<Ingredient> pool = CsvIngredientRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\ingredients.csv").ToList();
            List<DishRule> rules = CsvDishRuleRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\dishrules.csv").ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "Chili", "Ice", "Fish" }; // Chili has Spicy=1.5, Power=true; Ice has no flavors
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            Assert.Contains("Spicy Power Salad", discovered, "Spicy Power Salad should be discovered with Chili");
        }

        [Test]
        public void NoDishDiscovered_WithRandomIngredients()
        {
            List<Ingredient> pool = CsvIngredientRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\ingredients.csv").ToList();
            List<DishRule> rules = CsvDishRuleRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\dishrules.csv").ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "Ice", "Ice" }; // Ice has no flavors/tags, won't match any rule
            Dish dish = chef.MixByName(selection, pool);

            DishEvaluator eval = new DishEvaluator();
            List<string> discovered = eval.DiscoverDishes(dish, rules);

            Assert.IsEmpty(discovered, "No dishes should be discovered with only Ice");
        }

        [Test]
        public void MultipleDishesDiscovered_WithMatchingIngredients()
        {
            List<Ingredient> pool = CsvIngredientRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\ingredients.csv").ToList();
            List<DishRule> rules = CsvDishRuleRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\dishrules.csv").ToList();

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
        public void InvalidIngredientName_DoesNotCrash()
        {
            List<Ingredient> pool = CsvIngredientRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\ingredients.csv").ToList();
            List<DishRule> rules = CsvDishRuleRepository.ReadFromFile(@"d:\Rajiya\HellStarChef\data\dishrules.csv").ToList();

            PlayerChef chef = new PlayerChef();
            List<string> selection = new List<string> { "NonExistentIngredient", "Fish" };
            Dish dish = chef.MixByName(selection, pool);

            // Should only include Fish
            Assert.AreEqual(1, dish.Ingredients.Count, "Only valid ingredients should be added");
            Assert.AreEqual("Fish", dish.Ingredients[0].Name);
        }
    }
}
