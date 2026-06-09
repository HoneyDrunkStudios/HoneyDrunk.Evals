# Changelog

## [0.1.0] - 2026-06-09

- Introduced `HoneyDrunk.Evals` runtime for the ADR-0023 scaffold: `DefaultEvaluator`, `ChatTarget` (D6 router-bypass pinning), `AutomatedRubricScorer` + `ModelAsJudgeScorer`, observation-only `SafetyFilterObserver` / `CostGuardObserver` / `AuditLogObserver`, `EvalsTelemetry` (D10 content carve-out), and `AddHoneyDrunkEvals()` / `AddChatTarget()` DI wiring. References peer Nodes' `.Abstractions` packages only.
