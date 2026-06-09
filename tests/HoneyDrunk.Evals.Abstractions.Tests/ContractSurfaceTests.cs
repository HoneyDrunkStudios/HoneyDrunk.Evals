using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Kernel.Abstractions.Identity;
using Xunit;

namespace HoneyDrunk.Evals.Abstractions.Tests;

/// <summary>Compile-and-construct smoke tests for the six D3 contract surfaces.</summary>
public sealed class ContractSurfaceTests
{
    /// <summary>The D3 records are constructible and carry the typed pin and tenant fields.</summary>
    [Fact]
    public void Records_are_constructible_with_typed_pin_and_tenant()
    {
        var capability = new ModelCapabilityDeclaration("provider", "model", 1024, true, false, false, ["local"], 1m, 2m);

        var testCase = new EvalCase
        {
            CaseId = "case-1",
            SuiteVersion = "1.0.0",
            InputJson = "{\"q\":\"ping\"}",
            ExpectedOutputJson = "{\"a\":\"pong\"}",
            RubricCriteriaJson = null,
            Tags = new Dictionary<string, string> { ["k"] = "v" },
        };

        var score = new EvalScore("scorer", 0.9d, "ok", new Dictionary<string, string>());
        var caseResult = new EvalCaseResult(testCase.CaseId, "pong", [score], new ObservedOperatorPrediction(false, 0.01m, null));
        var summary = new EvalReportSummary(0.9d, 1, 1, 0);

        var report = new EvalReport
        {
            ReportId = "report-1",
            SuiteId = "suite-1",
            SuiteVersion = "1.0.0",
            TargetId = "target-1",
            PinnedModelCapability = capability,
            StartedAt = DateTimeOffset.UnixEpoch,
            CompletedAt = DateTimeOffset.UnixEpoch,
            ScorerSet = ["scorer"],
            CaseResults = [caseResult],
            Summary = summary,
        };

        Assert.Equal(TenantId.Internal, testCase.TenantId);
        Assert.Equal(TenantId.Internal, report.TenantId);
        Assert.Same(capability, report.PinnedModelCapability);
        Assert.Equal("model", report.PinnedModelCapability!.ModelId);
    }

    /// <summary>The rubric spec record is constructible.</summary>
    [Fact]
    public void RubricSpec_is_constructible()
    {
        var rubric = new RubricSpec(["automated-rubric"], new Dictionary<string, double> { ["automated-rubric"] = 1.0d });
        Assert.Single(rubric.RequiredScorers);
        Assert.Equal(1.0d, rubric.ScorerWeights["automated-rubric"]);
    }
}
