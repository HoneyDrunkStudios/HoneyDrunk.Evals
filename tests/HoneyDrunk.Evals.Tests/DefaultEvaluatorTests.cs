using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Orchestration;
using HoneyDrunk.Evals.Scoring;
using HoneyDrunk.Evals.Telemetry;
using HoneyDrunk.Evals.Tests.Fixtures;
using HoneyDrunk.Kernel.Abstractions.Identity;
using Xunit;

namespace HoneyDrunk.Evals.Tests;

/// <summary>Provenance and orchestration tests for <see cref="DefaultEvaluator"/>.</summary>
public sealed class DefaultEvaluatorTests
{
    private static EvalCase Case(string id, string input, string expected, TenantId? tenant = null) => new()
    {
        CaseId = id,
        SuiteVersion = "1.0.0",
        InputJson = input,
        ExpectedOutputJson = expected,
        Tags = new Dictionary<string, string>(),
        TenantId = tenant ?? TenantId.Internal,
    };

    private static DefaultEvaluator NewEvaluator() =>
        new(new EvalsTelemetry(new NullTelemetryActivityFactory()), TimeProvider.System);

    /// <summary>The report records suite, target, scorer-set, timestamps, and the typed pin.</summary>
    [Fact]
    public async Task RunAsync_records_full_provenance_including_typed_pin()
    {
        var pin = new ModelCapabilityDeclaration("provider", "model-x", 4096, true, false, false, ["local"], 1m, 2m);
        var suite = new StubEvalSuite("suite-a", "1.0.0", isSensitive: false, [Case("c1", "in", "out")]);
        var target = new EchoEvalTarget("target-a", "out", pin);
        var scorers = new IEvalScorer[] { new AutomatedRubricScorer() };

        var report = await NewEvaluator().RunAsync(suite, target, scorers);

        Assert.Equal("suite-a", report.SuiteId);
        Assert.Equal("1.0.0", report.SuiteVersion);
        Assert.Equal("target-a", report.TargetId);
        Assert.Equal([AutomatedRubricScorer.Name], report.ScorerSet);
        Assert.Same(pin, report.PinnedModelCapability);
        Assert.True(report.CompletedAt >= report.StartedAt);
        Assert.Single(report.CaseResults);
        Assert.Equal(1, report.Summary.PassedCount);
    }

    /// <summary>When no model is pinned the report's pin is null.</summary>
    [Fact]
    public async Task RunAsync_records_null_pin_when_target_unpinned()
    {
        var suite = new StubEvalSuite("suite-b", "1.0.0", isSensitive: false, [Case("c1", "in", "out")]);
        var target = new EchoEvalTarget("target-b", "out");
        var scorers = new IEvalScorer[] { new AutomatedRubricScorer() };

        var report = await NewEvaluator().RunAsync(suite, target, scorers);

        Assert.Null(report.PinnedModelCapability);
    }

    /// <summary>The report carries the tenant from the cases, defaulting to Internal.</summary>
    [Fact]
    public async Task RunAsync_carries_tenant_through()
    {
        var tenant = TenantId.NewId();
        var suite = new StubEvalSuite("suite-c", "1.0.0", isSensitive: false, [Case("c1", "in", "out", tenant)]);
        var target = new EchoEvalTarget("target-c", "out");
        var scorers = new IEvalScorer[] { new AutomatedRubricScorer() };

        var report = await NewEvaluator().RunAsync(suite, target, scorers);

        Assert.Equal(tenant, report.TenantId);
    }

    /// <summary>The report defaults the tenant to Internal when no case carries one.</summary>
    [Fact]
    public async Task RunAsync_defaults_tenant_to_internal()
    {
        var suite = new StubEvalSuite("suite-d", "1.0.0", isSensitive: false, [Case("c1", "in", "out")]);
        var target = new EchoEvalTarget("target-d", "out");
        var scorers = new IEvalScorer[] { new AutomatedRubricScorer() };

        var report = await NewEvaluator().RunAsync(suite, target, scorers);

        Assert.Equal(TenantId.Internal, report.TenantId);
    }
}
