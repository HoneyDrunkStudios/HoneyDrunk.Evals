using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Operator.Abstractions;

namespace HoneyDrunk.Evals.ObserverScoring;

/// <summary>
/// Observation-only composition of Operator's <see cref="ICostGuard"/> (ADR-0023 D7, invariant 121).
/// Reads the cost budget status to <em>observe</em> what the cost of a case would be against a scope;
/// it never records a cost event or enforces a budget on a live path.
/// </summary>
public sealed class CostGuardObserver
{
    private readonly ICostGuard costGuard;

    /// <summary>Initializes a new instance of the <see cref="CostGuardObserver"/> class.</summary>
    /// <param name="costGuard">The Operator cost guard, composed observation-only.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="costGuard"/> is <c>null</c>.</exception>
    public CostGuardObserver(ICostGuard costGuard)
    {
        ArgumentNullException.ThrowIfNull(costGuard);
        this.costGuard = costGuard;
    }

    /// <summary>
    /// Observes the cost marker for a case by reading the cost guard's current budget status for the
    /// supplied scope and window. Returns the remaining budget as the observed marker. No cost event is
    /// recorded and no budget is enforced — this is a read-only scoring signal.
    /// </summary>
    /// <param name="scope">The cost scope to observe (for example a suite or tenant scope).</param>
    /// <param name="window">The budget window to observe (for example <c>"daily"</c>).</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The observed cost marker (remaining budget) for the scope and window.</returns>
    public async Task<decimal> ObserveAsync(string scope, string window, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(scope);
        ArgumentException.ThrowIfNullOrEmpty(window);

        var status = await this.costGuard.GetStatusAsync(scope, window, cancellationToken).ConfigureAwait(false);
        return status.Remaining;
    }
}
