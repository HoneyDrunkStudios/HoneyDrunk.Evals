using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Scoring;

/// <summary>
/// Deterministic scorer that compares a target output against the case's
/// <see cref="EvalCase.ExpectedOutputJson"/> using exact-match semantics. Marked
/// <see cref="IsDeterministic"/> = <c>true</c> per ADR-0023 D5 — it performs no inference.
/// </summary>
public sealed class AutomatedRubricScorer : IEvalScorer
{
    /// <summary>The stable scorer name.</summary>
    public const string Name = "automated-rubric";

    /// <inheritdoc />
    public string ScorerName => Name;

    /// <inheritdoc />
    public bool IsDeterministic => true;

    /// <inheritdoc />
    public Task<EvalScore> ScoreAsync(EvalCase testCase, string targetOutput, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCase);
        ArgumentNullException.ThrowIfNull(targetOutput);

        var tags = new Dictionary<string, string>(StringComparer.Ordinal);

        if (testCase.ExpectedOutputJson is null)
        {
            tags["match"] = "no-expectation";
            return Task.FromResult(new EvalScore(Name, 0d, "Case declares no ExpectedOutputJson; automated rubric scoring is not applicable.", tags));
        }

        var matched = string.Equals(
            NormalizeForComparison(targetOutput),
            NormalizeForComparison(testCase.ExpectedOutputJson),
            StringComparison.Ordinal);

        tags["match"] = matched ? "exact" : "mismatch";
        var detail = matched ? "Output exactly matched the expected output." : "Output did not match the expected output.";
        return Task.FromResult(new EvalScore(Name, matched ? 1d : 0d, detail, tags));
    }

    private static string NormalizeForComparison(string value) => value.Trim();
}
