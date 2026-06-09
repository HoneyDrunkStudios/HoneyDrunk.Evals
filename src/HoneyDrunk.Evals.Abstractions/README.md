# HoneyDrunk.Evals.Abstractions

Dependency-light contracts for the HoneyDrunk.Evals evaluation and quality substrate per ADR-0023 — the six D3 surfaces every downstream Node compiles against: `IEvaluator`, `IEvalScorer`, `IEvalSuite`, `IEvalTarget`, and the `EvalCase` / `EvalReport` records (plus the supporting `EvalScore`, `RubricSpec`, `EvalCaseResult`, `EvalReportSummary`, and `ObservedOperatorPrediction` records).

This package references only upstream `*.Abstractions` siblings (`HoneyDrunk.Kernel.Abstractions` for the `TenantId` strong type, `HoneyDrunk.AI.Abstractions` for `ModelCapabilityDeclaration`) plus `HoneyDrunk.Standards`. No runtime HoneyDrunk packages (invariant 120 / invariant 1).

## Install

```bash
dotnet add package HoneyDrunk.Evals.Abstractions --version 0.1.0
```

## Public API

See XML documentation and ADR-0023 for the initial public surface.
