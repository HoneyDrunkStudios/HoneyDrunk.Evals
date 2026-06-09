# Changelog

## [0.1.0] - 2026-06-09

- Stood up the HoneyDrunk.Evals solution per ADR-0023: three packages (`Abstractions`, runtime, `Providers.InMemory`), the six D3 contract surfaces, the default runtime (`DefaultEvaluator`, `ChatTarget` with D6 router-bypass pinning, `AutomatedRubricScorer` + `ModelAsJudgeScorer`, observation-only `SafetyFilterObserver` / `CostGuardObserver` / `AuditLogObserver`, `EvalsTelemetry` with the D10 content carve-out, `AddHoneyDrunkEvals()` / `AddChatTarget()` DI wiring), the in-memory provider backend, tests, documentation, and the standard CI pipeline including the Abstractions-scoped API-compatibility canary.
