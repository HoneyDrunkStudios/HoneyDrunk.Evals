namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// Top-level orchestration surface for the Evals Node. Runs a suite of cases against a target,
/// applies a scorer set to each case output, and produces a durable <see cref="EvalReport"/>.
/// </summary>
/// <remarks>
/// Per ADR-0023 D14 this is one of the canary-frozen hot-path surfaces (invariant 126): a shape
/// change to <see cref="RunAsync"/> without a version bump fails the contract-shape canary.
/// </remarks>
public interface IEvaluator
{
    /// <summary>
    /// Runs every case in <paramref name="suite"/> against <paramref name="target"/>, scores each
    /// output with the supplied <paramref name="scorers"/>, and returns the assembled report.
    /// </summary>
    /// <param name="suite">The named, versioned suite supplying the cases and rubric.</param>
    /// <param name="target">The thing under evaluation (chat model, agent, pipeline, workflow).</param>
    /// <param name="scorers">The ordered scorer set applied to each case output.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The completed <see cref="EvalReport"/> with full run provenance per ADR-0023 D12 (invariant 122).</returns>
    Task<EvalReport> RunAsync(IEvalSuite suite, IEvalTarget target, IReadOnlyList<IEvalScorer> scorers, CancellationToken cancellationToken = default);
}
