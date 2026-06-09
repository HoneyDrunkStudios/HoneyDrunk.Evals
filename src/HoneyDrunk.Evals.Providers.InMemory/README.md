# HoneyDrunk.Evals.Providers.InMemory

The first-wave provider-slot backend for HoneyDrunk.Evals per ADR-0023 D2/D13: `InMemoryReportStore` (thread-safe `EvalReport` persistence keyed by `ReportId`) and `InMemorySuiteRepository` (thread-safe `IEvalSuite` fixture store). Zero-network and deterministic — for tests, local development, and seed evaluation runs. A production-grade report store ships under a separate packet once a real consumer drives the shape.

References only `HoneyDrunk.Evals.Abstractions` (plus `HoneyDrunk.Standards`), per invariant 3.

## Install

```bash
dotnet add package HoneyDrunk.Evals.Providers.InMemory --version 0.1.0
```

## Consume

```csharp
var store = new InMemoryReportStore();
await store.SaveAsync(report);
var roundTripped = await store.GetAsync(report.ReportId);
```
