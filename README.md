# Hellstar Chef (backend prototype)

Projects added:

- `src/HellstarChef.Core` - core models and evaluator
- `tests/HellstarChef.Core.Tests` - xUnit tests

Run tests:

```bash
dotnet test
```

This prototype includes `Ingredient`, `Dish`, rule types (`FlavorThresholdRule`, `SpecialTagRule`), a `DishEvaluator`, and sample ingredients (Fish, FirePaper, Herb, Ice). Tests cover flavor aggregation and rule matching (including the provided `Inferno Memory Stew`).

Adding new ingredients, tags, or recipes (scalable data-driven approach):

- Ingredients: add new entries to `data/ingredients.json` (name, `FlavorProfile` object, `Rarity`, `Rawness`, `IsToxic`, `SpecialTags` array). The library will automatically load `data/ingredients.json` when present and fall back to built-in defaults.
 - Ingredients: add new entries to `data/ingredients.csv`. The CSV columns are: `Name,FlavorProfile,Rarity,Rawness,IsToxic,SpecialTags`.
	 - `FlavorProfile` example: `Savory=1.0;Umami=1.0` (pairs separated by `;`, `key=value`).
	 - `SpecialTags` example: `Memory|Healing` (tags separated by `|`).
 - Dishes/recipes: add new entries to `data/dishes.csv`. Columns: `Name,Rules,SpecialTags`.
	 - `Rules` example: `Spicy_min=2.0;Umami_min=1.0;Bitter_max=0.7` (pairs separated by `;`, suffix `_min` or `_max`).
	 - `SpecialTags` example: `Memory|Healing` (tags separated by `|`).
- Dishes/recipes: create a `data/dishes.json` and implement a loader (not yet included) or add via code using `Dish` and `DishRule` classes.
- Tags/Enums: to add new flavors or `SpecialTag` values permanently, update `src/HellstarChef.Core/Models/Flavor.cs` or `SpecialTag.cs` and recompile; for temporary custom tags keep using existing tags in JSON.

Design notes for scalability:

- The project now exposes `IIngredientRepository` and `FileIngredientRepository` so you can plug in other sources (databases, web APIs).
 - The project now exposes `IIngredientRepository` and `CsvIngredientRepository` so you can plug in other sources (databases, web APIs).
- `DishEvaluator` implements `IDishEvaluator` for DI and easier testing.
- JSON data files live under the `data/` folder for developer-driven expansion without code edits.

# HellStarChef
A cooking game core
