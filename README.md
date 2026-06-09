# HoneyDrunk.Evals

HoneyDrunk.Evals is the AI sector's evaluation and quality substrate established by [ADR-0023](https://github.com/HoneyDrunkStudios/HoneyDrunk.Architecture/blob/main/adrs/ADR-0023-stand-up-honeydrunk-evals-node.md). It owns the mechanics of evaluation — how a case is defined, how a target is invoked under test, how outputs are scored, and how results are reported — so downstream Nodes (Agents, Knowledge, Memory, Flow, Sim, Lore, HoneyHub) get a shared regression story instead of each inventing their own harness. Evals provides the harness; consumers provide the suite content.

## Packages

| Package | Purpose |
|---|---|
| `HoneyDrunk.Evals.Abstractions` | The six D3 contracts every downstream Node compiles against: `IEvaluator`, `IEvalScorer`, `IEvalSuite`, `IEvalTarget`, and the `EvalCase` / `EvalReport` records (plus supporting `EvalScore`, `RubricSpec`, `EvalCaseResult`, `EvalReportSummary`, `ObservedOperatorPrediction`). |
| `HoneyDrunk.Evals` | Runtime composition: `DefaultEvaluator`, `ChatTarget`, the default scorer set, observation-only Operator/Audit observers, telemetry, and DI wiring. |
| `HoneyDrunk.Evals.Providers.InMemory` | First-wave provider-slot backend: `InMemoryReportStore` + `InMemorySuiteRepository`. |

Downstream Nodes take a runtime dependency **only** on `HoneyDrunk.Evals.Abstractions` (invariant 120); composition (which scorer set, which report store) is a host-time concern.

## How to consume

```csharp
using HoneyDrunk.Evals;
using HoneyDrunk.Evals.Abstractions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    // host wires the upstream IChatClient / IModelProvider (HoneyDrunk.AI) and
    // ITelemetryActivityFactory (HoneyDrunk.Kernel) composition first
    .AddHoneyDrunkEvals()
    .AddChatTarget("chat-default")                      // router-managed target
    .AddChatTarget("chat-pinned", pinnedDeclaration)    // D6 router-bypass: pins a ModelCapabilityDeclaration
    .BuildServiceProvider();

var evaluator = services.GetRequiredService<IEvaluator>();
var scorers = services.GetServices<IEvalScorer>().ToArray();
EvalReport report = await evaluator.RunAsync(suite, target, scorers);
```

## Key behaviors

- **D6 router-bypass** — `ChatTarget` is the Grid's only sanctioned router-bypass boundary (invariant 125). When it pins a `ModelCapabilityDeclaration`, it resolves the matching `IModelProvider` from DI and bypasses the router (regression detection cannot use the router it tests). The pin is recorded on every `EvalReport` (`report.PinnedModelCapability`), keeping the bypass auditable (invariant 122).
- **D10 content carve-out** — eval-signal telemetry MAY carry case content (input, output, expected) when `IEvalSuite.IsSensitive == false`; when `true`, payloads carry metadata only (invariant 124). This is a deliberate carve-out from the content-never-in-telemetry rule the other AI-sector Nodes adopt.
- **Observation-only** — Operator/Audit observers never enforce or write. `AuditLogObserver` never calls `IAuditLog.AppendAsync` (invariant 121, D7/D9).

## Invariants

This Node honors the ADR-0023 invariants assigned numbers **58–64**: 58 downstream-coupling, 59 read-only-observer, 60 provenance, 61 durable-not-ephemeral, 62 content-carve-out, 63 router-bypass, 64 contract-shape canary.

## Status

`0.1.0` — first shippable per ADR-0023 packet 04. Concrete `AgentTarget` / `RetrievalTarget` / `MemoryTarget`, the Monte Carlo / N-trial distribution surface, and a production report store are deferred to follow-up packets.
