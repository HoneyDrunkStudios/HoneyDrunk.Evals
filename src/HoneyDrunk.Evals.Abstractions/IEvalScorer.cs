namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// A scoring function over a single case output. Implementations are either automated
/// (regex, schema validation, exact-match rubric) or model-as-judge (using an inference call).
/// </summary>
/// <remarks>
/// Per ADR-0023 D14 this is one of the canary-frozen hot-path surfaces (invariant 126): it is the
/// injection point for custom scoring, so shape drift without a version bump fails the canary.
/// </remarks>
public interface IEvalScorer
{
    /// <summary>Gets the stable, human-readable scorer name recorded on each <see cref="EvalScore"/>.</summary>
    string ScorerName { get; }

    /// <summary>
    /// Gets a value indicating whether this scorer is deterministic. Automated scorers return
    /// <c>true</c>; model-as-judge scorers (which call inference) return <c>false</c> per ADR-0023 D5.
    /// </summary>
    bool IsDeterministic { get; }

    /// <summary>Scores a single target output for the supplied case.</summary>
    /// <param name="testCase">The case the output was produced for.</param>
    /// <param name="targetOutput">The raw output produced by the target under evaluation.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The produced score.</returns>
    Task<EvalScore> ScoreAsync(EvalCase testCase, string targetOutput, CancellationToken cancellationToken = default);
}
