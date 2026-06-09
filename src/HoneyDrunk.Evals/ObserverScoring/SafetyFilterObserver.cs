using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Operator.Abstractions;

namespace HoneyDrunk.Evals.ObserverScoring;

/// <summary>
/// Observation-only composition of Operator's <see cref="ISafetyFilter"/> (ADR-0023 D7, invariant 121).
/// Invokes the filter to <em>observe</em> what it would do on a target output and records the result;
/// it never uses the filter to block or gate a live path.
/// </summary>
public sealed class SafetyFilterObserver
{
    private readonly ISafetyFilter safetyFilter;

    /// <summary>Initializes a new instance of the <see cref="SafetyFilterObserver"/> class.</summary>
    /// <param name="safetyFilter">The Operator safety filter, composed observation-only.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="safetyFilter"/> is <c>null</c>.</exception>
    public SafetyFilterObserver(ISafetyFilter safetyFilter)
    {
        ArgumentNullException.ThrowIfNull(safetyFilter);
        this.safetyFilter = safetyFilter;
    }

    /// <summary>
    /// Observes whether the safety filter would fire on the supplied target output. Returns <c>true</c>
    /// when the filter would have blocked the output, <c>false</c> when it ran and allowed it. The result
    /// is a scoring signal only; no enforcement occurs.
    /// </summary>
    /// <param name="testCase">The case the output was produced for.</param>
    /// <param name="targetOutput">The target output to observe.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns><c>true</c> if the filter would fire; otherwise <c>false</c>.</returns>
    public async Task<bool> ObserveAsync(EvalCase testCase, string targetOutput, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCase);
        ArgumentNullException.ThrowIfNull(targetOutput);

        var context = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["case_id"] = testCase.CaseId,
            ["suite_version"] = testCase.SuiteVersion,
            ["mode"] = "eval-observation",
        };

        var request = new SafetyFilterRequest(targetOutput, "eval-output", context);
        var result = await this.safetyFilter.CheckAsync(request, cancellationToken).ConfigureAwait(false);

        // The filter "fired" when it did not allow the content.
        return !result.Allowed;
    }
}
