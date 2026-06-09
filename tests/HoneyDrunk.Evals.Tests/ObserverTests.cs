using HoneyDrunk.Audit.Abstractions;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.ObserverScoring;
using HoneyDrunk.Operator.Abstractions;
using NSubstitute;
using Xunit;

namespace HoneyDrunk.Evals.Tests;

/// <summary>Tests that the Operator/Audit observers are observation-only (D7, invariant 121).</summary>
public sealed class ObserverTests
{
    private static EvalCase Case(params (string Key, string Value)[] tags) => new()
    {
        CaseId = "c1",
        SuiteVersion = "1.0.0",
        InputJson = "in",
        Tags = tags.ToDictionary(t => t.Key, t => t.Value, StringComparer.Ordinal),
    };

    /// <summary>The safety-filter observer reports a fire without enforcing.</summary>
    [Fact]
    public async Task SafetyFilterObserver_observes_fire_without_enforcing()
    {
        var filter = Substitute.For<ISafetyFilter>();
        filter.CheckAsync(Arg.Any<SafetyFilterRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SafetyFilterResult(Allowed: false, BlockReason: "policy", FiredRules: ["r1"]));

        var fired = await new SafetyFilterObserver(filter).ObserveAsync(Case(), "bad output");

        Assert.True(fired);
        await filter.Received(1).CheckAsync(Arg.Any<SafetyFilterRequest>(), Arg.Any<CancellationToken>());
    }

    /// <summary>The safety-filter observer reports no fire when the filter allows the output.</summary>
    [Fact]
    public async Task SafetyFilterObserver_observes_allow()
    {
        var filter = Substitute.For<ISafetyFilter>();
        filter.CheckAsync(Arg.Any<SafetyFilterRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SafetyFilterResult(Allowed: true, BlockReason: null, FiredRules: []));

        var fired = await new SafetyFilterObserver(filter).ObserveAsync(Case(), "fine output");

        Assert.False(fired);
    }

    /// <summary>The cost-guard observer reads status only — it never records a cost event.</summary>
    [Fact]
    public async Task CostGuardObserver_reads_status_without_recording()
    {
        var guard = Substitute.For<ICostGuard>();
        guard.GetStatusAsync("suite-scope", "daily", Arg.Any<CancellationToken>())
            .Returns(new CostStatus(Spent: 4m, Limit: 10m, Remaining: 6m, Window: "daily"));

        var marker = await new CostGuardObserver(guard).ObserveAsync("suite-scope", "daily");

        Assert.Equal(6m, marker);
        await guard.DidNotReceive().RecordAsync(Arg.Any<CostEvent>(), Arg.Any<CancellationToken>());
        await guard.DidNotReceive().CheckBudgetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>());
    }

    /// <summary>The audit-log observer never appends to the audit log (D7/D9).</summary>
    [Fact]
    public async Task AuditLogObserver_never_appends()
    {
        var auditLog = Substitute.For<IAuditLog>();
        var observer = new AuditLogObserver(auditLog);

        var required = observer.Observe(Case(("approval_required", "true")));
        var notRequired = observer.Observe(Case());

        Assert.True(required);
        Assert.False(notRequired);
        Assert.Same(auditLog, observer.AuditSink);
        await auditLog.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }
}
