using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Providers.InMemory;
using Xunit;

namespace HoneyDrunk.Evals.Providers.InMemory.Tests;

/// <summary>Round-trip tests for the in-memory report store and suite repository.</summary>
public sealed class InMemoryStoreTests
{
    private static EvalReport Report(string id) => new()
    {
        ReportId = id,
        SuiteId = "suite-1",
        SuiteVersion = "1.0.0",
        TargetId = "target-1",
        StartedAt = DateTimeOffset.UnixEpoch,
        CompletedAt = DateTimeOffset.UnixEpoch,
        ScorerSet = ["automated-rubric"],
        CaseResults = [],
        Summary = new EvalReportSummary(0d, 0, 0, 0),
    };

    /// <summary>A saved report round-trips by id.</summary>
    [Fact]
    public async Task ReportStore_round_trips()
    {
        var store = new InMemoryReportStore();
        var report = Report("r1");

        await store.SaveAsync(report);
        var fetched = await store.GetAsync("r1");

        Assert.NotNull(fetched);
        Assert.Equal("r1", fetched!.ReportId);
        Assert.Same(report, fetched);
    }

    /// <summary>A missing report id returns null.</summary>
    [Fact]
    public async Task ReportStore_returns_null_for_missing()
    {
        var store = new InMemoryReportStore();
        Assert.Null(await store.GetAsync("nope"));
    }

    /// <summary>Two stores do not share state (cross-test isolation).</summary>
    [Fact]
    public async Task ReportStore_instances_are_isolated()
    {
        var a = new InMemoryReportStore();
        var b = new InMemoryReportStore();

        await a.SaveAsync(Report("r1"));

        Assert.Null(await b.GetAsync("r1"));
        Assert.Single(await a.GetAllAsync());
        Assert.Empty(await b.GetAllAsync());
    }

    /// <summary>A saved suite round-trips by id.</summary>
    [Fact]
    public async Task SuiteRepository_round_trips()
    {
        var repo = new InMemorySuiteRepository();
        var suite = new StubSuite("suite-x", "1.0.0");

        await repo.AddAsync(suite);
        var fetched = await repo.GetAsync("suite-x");

        Assert.Same(suite, fetched);
        Assert.Single(await repo.GetAllAsync());
    }

    private sealed class StubSuite(string suiteId, string suiteVersion) : IEvalSuite
    {
        public string SuiteId { get; } = suiteId;

        public string SuiteVersion { get; } = suiteVersion;

        public bool IsSensitive => false;

        public Task<IReadOnlyList<EvalCase>> GetCasesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EvalCase>>([]);

        public Task<RubricSpec> GetRubricAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new RubricSpec([], new Dictionary<string, double>()));
    }
}
