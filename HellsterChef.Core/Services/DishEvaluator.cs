using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HellsterChef.Core.Models;
using HellsterChef.Core.Data;
using HellsterChef.Core.Rules;

namespace HellsterChef.Core.Services
{
    public sealed class DishEvaluationResult
    {
        public bool Matches { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public DishQuality Quality { get; set; }
        public bool IsPoisoned { get; set; }
        public double CompatibilityScore { get; set; }
        public List<string> CompatibilityNotes { get; set; } = new List<string>();
    }

    public sealed class DishEvaluator : IDishEvaluator
    {
        private readonly ICompatibilityRepository _compatRepo;
        private readonly ISynergyRepository _synergyRepo;

        public DishEvaluator() : this(
            new CsvCompatibilityRepository(Path.Combine("d:\\Rajiya\\HellStarChef", "data", "compatibility.csv")),
            new CsvSynergyRepository(Path.Combine("d:\\Rajiya\\HellStarChef", "data", "synergies.csv")))
        {
        }

        public DishEvaluator(ICompatibilityRepository compatRepo, ISynergyRepository synergyRepo)
        {
            _compatRepo = compatRepo;
            _synergyRepo = synergyRepo;
        }

        public DishEvaluationResult Evaluate(Dish dish, DishRule rule)
        {
            DishEvaluationResult result = new DishEvaluationResult();

            // Check for at least one main ingredient (Protein, Vegetable, or Grain)
            bool hasBase = dish.Ingredients.Any(i => i.Type == IngredientType.Protein || i.Type == IngredientType.Vegetable || i.Type == IngredientType.Grain);
            if (!hasBase)
            {
                result.Reasons.Add("No main ingredient (protein, vegetable, or grain) found; cannot form a proper dish.");
                result.Matches = false;
                result.Quality = DishQuality.Dumb;
                return result;
            }

            // aggregate flavor values from ingredients
            Dictionary<Flavor, double> flavorTotals = new Dictionary<Flavor, double>();
            foreach (Ingredient ing in dish.Ingredients)
            {
                foreach (KeyValuePair<Flavor, double> kv in ing.FlavorProfile)
                {
                    if (!flavorTotals.ContainsKey(kv.Key)) flavorTotals[kv.Key] = 0.0;
                    flavorTotals[kv.Key] += kv.Value;
                }
            }
            
            // Compute data-driven compatibility against per-ingredient flavor preferences
            var compatibilityResult = CheckCompatibility(dish.Ingredients, flavorTotals);
            result.CompatibilityScore = compatibilityResult.Score;
            result.CompatibilityNotes.AddRange(compatibilityResult.Notes);

            // If compatibility is strongly negative, mark as incompatible
            if (result.CompatibilityScore <= -1.0)
            {
                result.Matches = false;
                result.Quality = DishQuality.Dumb;
                result.Reasons.AddRange(result.CompatibilityNotes);
                return result;
            }

            // Check for poisonous, incompatible, or other synergies
            var synergyResult = CheckSynergies(dish.Ingredients);
            if (synergyResult.PoisonIssues.Any())
            {
                result.IsPoisoned = true;
                result.Matches = false;
                result.Quality = DishQuality.Dumb;
                result.Reasons.AddRange(synergyResult.PoisonIssues);
                return result;
            }

            if (synergyResult.IncompatibleIssues.Any())
            {
                result.Matches = false;
                result.Quality = DishQuality.Dumb;
                result.Reasons.AddRange(synergyResult.IncompatibleIssues);
                result.CompatibilityScore = Math.Min(result.CompatibilityScore, -2.0);
                return result;
            }

            // Attach bonuses and penalties as compatibility notes
            result.CompatibilityNotes.AddRange(synergyResult.BonusNotes);
            result.CompatibilityNotes.AddRange(synergyResult.PenaltyIssues.Select(p => "Unfavorable pairing: " + p));

            bool allOk = true;
            int closeCount = 0;
            int exactCount = 0;
            foreach (Condition cond in rule.Conditions)
            {
                if (cond.Flavor is not null)
                {
                    flavorTotals.TryGetValue(cond.Flavor.Value, out double total);
                    bool ok = cond.Evaluate(total);
                    if (!ok)
                    {
                        allOk = false;
                        double diff = cond.Comparison == Comparison.GreaterOrEqual ? cond.Value - total : total - cond.Value;
                        if (Math.Abs(diff) <= 0.5) closeCount++;
                        result.Reasons.Add($"Flavor {cond.Flavor} check failed: got {total}, needed {cond.Comparison} {cond.Value}");
                    }
                    else
                    {
                        // Check if exact
                        if (cond.Comparison == Comparison.GreaterOrEqual && total >= cond.Value && total <= cond.Value + 0.1) exactCount++;
                        else if (cond.Comparison == Comparison.LessOrEqual && total <= cond.Value && total >= cond.Value - 0.1) exactCount++;
                        else if (cond.Comparison == Comparison.Equal && Math.Abs(total - cond.Value) < 0.1) exactCount++;
                    }
                }
                else if (cond.RequiresTag is not null)
                {
                    bool has = dish.Ingredients.Any(i => i.SpecialTags.Contains(cond.RequiresTag.Value));
                    if (has != cond.RequiresTagPresence)
                    {
                        allOk = false;
                        result.Reasons.Add($"Tag {cond.RequiresTag} presence check failed: expected {cond.RequiresTagPresence}");
                    }
                    else
                    {
                        exactCount++; // Tags are exact
                    }
                }
                else if (cond.RequiredBaseNames is not null && cond.RequiredBaseNames.Count > 0)
                {
                    if (cond.RequireAllBases)
                    {
                        bool hasAll = cond.RequiredBaseNames.All(name => dish.Ingredients.Any(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase)));
                        if (!hasAll)
                        {
                            allOk = false;
                            result.Reasons.Add($"Required base ingredients missing (need all): {string.Join(" + ", cond.RequiredBaseNames)}");
                        }
                        else
                        {
                            exactCount++; // Base is exact
                        }
                    }
                    else
                    {
                        bool hasAny = cond.RequiredBaseNames.Any(name => dish.Ingredients.Any(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase)));
                        if (!hasAny)
                        {
                            allOk = false;
                            result.Reasons.Add($"Required base ingredient not found; options: {string.Join(", ", cond.RequiredBaseNames)}");
                        }
                        else
                        {
                            exactCount++; // Base is exact
                        }
                    }
                }
            }

            result.Matches = allOk && result.Reasons.Count == 0;

            // Determine quality
            bool hasToxic = dish.Ingredients.Any(i => i.IsToxic);
            int baseCount = dish.Ingredients.Count(i => i.Type == IngredientType.Protein || i.Type == IngredientType.Vegetable || i.Type == IngredientType.Grain);
            if (!result.Matches)
            {
                if (closeCount > 0 && !hasToxic && baseCount == 1)
                    result.Quality = DishQuality.Good;
                else
                    result.Quality = DishQuality.Dumb;
            }
            else
            {
                if (exactCount == rule.Conditions.Count && !hasToxic && baseCount == 1)
                    result.Quality = DishQuality.Excellent;
                else
                    result.Quality = DishQuality.Good;
            }

            return result;
        }

        private sealed class SynergyCheckResult
        {
            public List<string> PoisonIssues { get; } = new List<string>();
            public List<string> IncompatibleIssues { get; } = new List<string>();
            public List<string> PenaltyIssues { get; } = new List<string>();
            public List<string> BonusNotes { get; } = new List<string>();
        }

        private SynergyCheckResult CheckSynergies(IEnumerable<Ingredient> ingredients)
        {
            var result = new SynergyCheckResult();
            var ingNames = new HashSet<string>(ingredients.Select(i => i.Name), StringComparer.OrdinalIgnoreCase);

            var synergies = _synergyRepo.GetAllAsync().GetAwaiter().GetResult();
            foreach (var s in synergies)
            {
                if (ingNames.Contains(s.Ingredient1) && ingNames.Contains(s.Ingredient2))
                {
                    switch (s.Effect)
                    {
                        case SynergyEffect.Poison:
                            result.PoisonIssues.Add($"Poisonous combination detected: {s.Description} ({s.Ingredient1} + {s.Ingredient2})");
                            break;
                        case SynergyEffect.Incompatible:
                            result.IncompatibleIssues.Add($"Incompatible combination: {s.Description} ({s.Ingredient1} + {s.Ingredient2})");
                            break;
                        case SynergyEffect.Penalty:
                            result.PenaltyIssues.Add($"{s.Description} ({s.Ingredient1} + {s.Ingredient2})");
                            break;
                        case SynergyEffect.Bonus:
                            result.BonusNotes.Add($"Nice pairing: {s.Description} ({s.Ingredient1} + {s.Ingredient2})");
                            break;
                    }
                }
            }

            return result;
        }

        private (double Score, List<string> Notes) CheckCompatibility(IEnumerable<Ingredient> ingredients, Dictionary<Flavor, double> flavorTotals)
        {
            var notes = new List<string>();
            var entries = _compatRepo.GetAllAsync().GetAwaiter().GetResult().ToList();
            var compMap = entries.ToDictionary(e => e.Ingredient, StringComparer.OrdinalIgnoreCase);

            // Evaluate per main ingredient (Protein/Vegetable/Grain) preferences
            var bases = ingredients.Where(i => i.Type == IngredientType.Protein || i.Type == IngredientType.Vegetable || i.Type == IngredientType.Grain).ToList();
            if (!bases.Any()) return (0.0, notes);

            double totalScore = 0.0;
            int checks = 0;

            foreach (var b in bases)
            {
                if (!compMap.TryGetValue(b.Name, out var comp)) continue;
                foreach (var kv in comp.FlavorRanges)
                {
                    checks++;
                    Flavor f = kv.Key;
                    var range = kv.Value;
                    double min = range.Min;
                    double max = range.Max;
                    double tol = range.MaxTolerable;
                    flavorTotals.TryGetValue(f, out double val);

                    if (val >= min && val <= max)
                    {
                        totalScore += 1.0;
                    }
                    else if (val > max && val <= tol)
                    {
                        totalScore -= 0.5;
                        notes.Add($"{b.Name} tolerates some {f}, but level {val} is higher than preferred {max}");
                    }
                    else if (val > tol)
                    {
                        totalScore -= 2.0;
                        notes.Add($"{b.Name} strongly dislikes excessive {f} (level {val})");
                    }
                    else if (val < min)
                    {
                        totalScore -= 0.5;
                        notes.Add($"{b.Name} prefers more {f} (needs >={min}, got {val})");
                    }
                }
            }

            double score = checks > 0 ? totalScore / checks : 0.0; // normalized
            return (score, notes);
        }

        // Discover which dishes match the mixed ingredients
        public List<string> DiscoverDishes(Dish dish, IEnumerable<DishRule> allRules)
        {
            List<string> discovered = new List<string>();
            foreach (DishRule rule in allRules)
            {
                DishEvaluationResult res = Evaluate(dish, rule);
                if (res.Matches)
                {
                    discovered.Add(rule.Name);
                }
            }
            return discovered;
        }
    }
}
