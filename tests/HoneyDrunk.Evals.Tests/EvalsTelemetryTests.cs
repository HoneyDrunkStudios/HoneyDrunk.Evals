using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Telemetry;
using HoneyDrunk.Evals.Tests.Fixtures;
using Xunit;

namespace HoneyDrunk.Evals.Tests;

/// <summary>Tests the D10 content carve-out behavior of <see cref="EvalsTelemetry"/> (invariant 124).</summary>
public sealed class EvalsTelemetryTests
{
    private static EvalCase Case() => new()
    {
        CaseId = "c1",
        SuiteVersion = "1.0.0",
        InputJson = "secret-input",
        ExpectedOutputJson = "expected",
        Tags = new Dictionary<string, string>(),
    };

    /// <summary>A non-sensitive suite includes case content on the per-case activity payload.</summary>
    [Fact]
    public void NonSensitive_suite_includes_content()
    {
        var factory = new NullTelemetryActivityFactory();
        var telemetry = new EvalsTelemetry(factory);
        var suite = new StubEvalSuite("suite-a", "1.0.0", isSensitive: false, []);

        telemetry.StartCaseResult(suite, Case(), "target-output");

        var (_, tags) = Assert.Single(factory.Calls);
        Assert.NotNull(tags);
        Assert.Equal("secret-input", tags![EvalsTelemetry.CaseInputTag]);
        Assert.Equal("target-output", tags[EvalsTelemetry.TargetOutputTag]);
        Assert.Equal("expected", tags[EvalsTelemetry.ExpectedOutputTag]);
    }

    /// <summary>A sensitive suite strips content from the per-case activity payload (metadata only).</summary>
    [Fact]
    public void Sensitive_suite_strips_content()
    {
        var factory = new NullTelemetryActivityFactory();
        var telemetry = new EvalsTelemetry(factory);
        var suite = new StubEvalSuite("suite-b", "1.0.0", isSensitive: true, []);

        telemetry.StartCaseResult(suite, Case(), "target-output");

        var (_, tags) = Assert.Single(factory.Calls);
        Assert.NotNull(tags);
        Assert.False(tags!.ContainsKey(EvalsTelemetry.CaseInputTag));
        Assert.False(tags.ContainsKey(EvalsTelemetry.TargetOutputTag));
        Assert.False(tags.ContainsKey(EvalsTelemetry.ExpectedOutputTag));
        Assert.Equal("c1", tags[EvalsTelemetry.CaseIdTag]);
        Assert.Equal(true, tags[EvalsTelemetry.SensitiveTag]);
    }

    /// <summary>The suite-run activity carries metadata only regardless of sensitivity.</summary>
    [Fact]
    public void SuiteRun_carries_metadata_only()
    {
        var factory = new NullTelemetryActivityFactory();
        var telemetry = new EvalsTelemetry(factory);
        var suite = new StubEvalSuite("suite-c", "2.0.0", isSensitive: false, []);
        var target = new EchoEvalTarget("target-c", "out");

        telemetry.StartSuiteRun(suite, target);

        var (name, tags) = Assert.Single(factory.Calls);
        Assert.Equal(EvalsTelemetry.SuiteRunActivityName, name);
        Assert.NotNull(tags);
        Assert.Equal("suite-c", tags![EvalsTelemetry.SuiteIdTag]);
        Assert.Equal("target-c", tags[EvalsTelemetry.TargetIdTag]);
        Assert.False(tags.ContainsKey(EvalsTelemetry.CaseInputTag));
    }
}
