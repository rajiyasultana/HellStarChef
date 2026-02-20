# Hellstar Chef (backend prototype)

Projects added:

- `src/HellstarChef.Core` - core models and evaluator
- `tests/HellstarChef.Core.Tests` - xUnit tests

Run tests:

```bash
dotnet test
```

This prototype includes `Ingredient`, `Dish`, rule types (`FlavorThresholdRule`, `SpecialTagRule`), a `DishEvaluator`, and sample ingredients (Fish, FirePaper, Herb, Ice). Tests cover flavor aggregation and rule matching (including the provided `Inferno Memory Stew`).
# HellStarChef
A cooking game core
