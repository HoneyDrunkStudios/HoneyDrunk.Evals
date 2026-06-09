using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Tests.Fixtures;

/// <summary>A deterministic in-test <see cref="IEvalSuite"/> with a fixed case set.</summary>
public sealed class StubEvalSuite : IEvalSuite
{
    private readonly IReadOnlyList<EvalCase> cases;
    private readonly RubricSpec rubric;

    /// <summary>Initializes a new instance of the <see cref="StubEvalSuite"/> class.</summary>
    /// <param name="suiteId">The suite id.</param>
    /// <param name="suiteVersion">The suite version.</param>
    /// <param name="isSensitive">Whether the suite is sensitive (D10).</param>
    /// <param name="cases">The cases the suite returns.</param>
    public StubEvalSuite(string suiteId, string suiteVersion, bool isSensitive, IReadOnlyList<EvalCase> cases)
    {
        this.SuiteId = suiteId;
        this.SuiteVersion = suiteVersion;
        this.IsSensitive = isSensitive;
        this.cases = cases;
        this.rubric = new RubricSpec([], new Dictionary<string, double>());
    }

    /// <inheritdoc />
    public string SuiteId { get; }

    /// <inheritdoc />
    public string SuiteVersion { get; }

    /// <inheritdoc />
    public bool IsSensitive { get; }

    /// <inheritdoc />
    public Task<IReadOnlyList<EvalCase>> GetCasesAsync(CancellationToken cancellationToken = default) => Task.FromResult(this.cases);

    /// <inheritdoc />
    public Task<RubricSpec> GetRubricAsync(CancellationToken cancellationToken = default) => Task.FromResult(this.rubric);
}
