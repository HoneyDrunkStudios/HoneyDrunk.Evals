using HoneyDrunk.Audit.Abstractions;
using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.ObserverScoring;

/// <summary>
/// Observation-only composition of Audit's <see cref="IAuditLog"/> (ADR-0023 D7 / D9, invariant 121).
/// Evals never writes audit — the audit trail belongs to Operator/Audit. This observer is wired so a
/// scorer can express "would this case have required an approval path?" as an observation marker without
/// ever calling <see cref="IAuditLog.AppendAsync"/>.
/// </summary>
/// <remarks>
/// Per D7/D9 Evals reads the audit record Operator produced (if any); it does not produce one. The held
/// <see cref="IAuditLog"/> reference documents the composition edge and lets a host swap in a query-capable
/// implementation later, but <see cref="AppendAsync"/> is never invoked from this Node — doing so would
/// violate the read-only-observer invariant.
/// </remarks>
public sealed class AuditLogObserver
{
    private readonly IAuditLog auditLog;

    /// <summary>Initializes a new instance of the <see cref="AuditLogObserver"/> class.</summary>
    /// <param name="auditLog">The audit log, composed observation-only; never appended to from Evals.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="auditLog"/> is <c>null</c>.</exception>
    public AuditLogObserver(IAuditLog auditLog)
    {
        ArgumentNullException.ThrowIfNull(auditLog);
        this.auditLog = auditLog;
    }

    /// <summary>
    /// Gets the composed audit-sink reference. Exposed so a host can confirm the observation edge is wired
    /// without granting any write capability — the reference is read-only and is never appended to.
    /// </summary>
    public IAuditLog AuditSink => this.auditLog;

    /// <summary>
    /// Observes whether the supplied case would require an approval path, returning an observation marker.
    /// This is a pure, side-effect-free observation: it does not append to the audit log and does not
    /// invoke any enforcement path. At stand-up the marker is derived from case metadata only.
    /// </summary>
    /// <param name="testCase">The case to observe.</param>
    /// <returns>
    /// <c>true</c> when the case carries an <c>"approval_required"</c> tag set to <c>"true"</c>;
    /// otherwise <c>false</c>. Returned synchronously to make the no-write contract explicit.
    /// </returns>
    public bool Observe(EvalCase testCase)
    {
        ArgumentNullException.ThrowIfNull(testCase);

        // Intentionally never calls this.auditLog.AppendAsync — Evals is observation-only (D7/D9).
        return testCase.Tags.TryGetValue("approval_required", out var value)
            && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
