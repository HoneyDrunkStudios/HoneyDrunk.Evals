# HoneyDrunk.Evals

Runtime composition for the HoneyDrunk.Evals evaluation and quality substrate per ADR-0023: the default `IEvaluator` (`DefaultEvaluator`), the default scorer set (`AutomatedRubricScorer` deterministic + `ModelAsJudgeScorer` non-deterministic), the `ChatTarget` with the D6 router-bypass pinning primitive, observation-only Operator/Audit observers, the D10 content-carve-out telemetry emitter, and DI wiring.

## Abstractions-only peer references (invariant 120 / invariant 44 / invariant 116)

Packet 04's loose dependency table lists peer-Node *runtime* packages (`HoneyDrunk.AI`, `HoneyDrunk.Operator`, `HoneyDrunk.Kernel`). This package deliberately deviates: it references **only** each peer Node's `.Abstractions` package — `HoneyDrunk.AI.Abstractions`, `HoneyDrunk.Operator.Abstractions`, `HoneyDrunk.Audit.Abstractions`, and `HoneyDrunk.Kernel.Abstractions`. Everything the runtime needs (`IChatClient`, `IModelProvider`, `ModelCapabilityDeclaration`, `ISafetyFilter`, `ICostGuard`, `IAuditLog`, `ITelemetryActivityFactory`, `TenantId`) lives in those `.Abstractions` packages, so taking the runtime packages would create unnecessary downstream coupling and violate the downstream-coupling invariants. The host composes concrete provider/runtime implementations.

## How to consume

```csharp
using HoneyDrunk.Evals;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    // host supplies IChatClient / IModelProvider (HoneyDrunk.AI) and ITelemetryActivityFactory (HoneyDrunk.Kernel)
    .AddHoneyDrunkEvals()
    .AddChatTarget("chat-default")                      // router-managed target
    .AddChatTarget("chat-pinned", pinnedDeclaration)    // D6 router-bypass: pins a ModelCapabilityDeclaration
    .BuildServiceProvider();

var evaluator = services.GetRequiredService<IEvaluator>();
var scorers = services.GetServices<IEvalScorer>().ToArray();
var report = await evaluator.RunAsync(suite, target, scorers);
// report.PinnedModelCapability records the pin (D6); report carries full provenance (D12).
```

## Notes

- **D6 router-bypass** — when a `ChatTarget` pins a `ModelCapabilityDeclaration`, it resolves the matching `IModelProvider` from DI and bypasses the router. The pin is recorded on every `EvalReport`.
- **D10 content carve-out** — `EvalsTelemetry` includes case content on telemetry payloads when `IEvalSuite.IsSensitive == false`, and strips to metadata-only when `true`.
- **Observation-only** — the Operator/Audit observers (`SafetyFilterObserver`, `CostGuardObserver`, `AuditLogObserver`) never enforce or write. `AuditLogObserver` never calls `IAuditLog.AppendAsync`.

## Install

```bash
dotnet add package HoneyDrunk.Evals --version 0.1.0
```

See XML documentation and ADR-0023 for the initial public surface.
