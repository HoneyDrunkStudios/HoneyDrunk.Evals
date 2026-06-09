using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Kernel.Abstractions.Identity;

namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// The durable, structured result of an evaluation run — per-case scores, run provenance, and summary.
/// Value type; cataloged with <c>kind: "type"</c>. Durable per ADR-0023 D13 (invariant 123).
/// </summary>
/// <remarks>
/// Per ADR-0023 D14 this is one of the canary-frozen hot-path surfaces (invariant 126). Every report
/// records full run provenance per D12 (invariant 122): suite identity and version, target identity,
/// the pinned <see cref="ModelCapabilityDeclaration"/> (when a model was pinned per D6), timestamps,
/// and the scorer set used.
/// </remarks>
public sealed record EvalReport
{
    /// <summary>Gets the report identifier, unique per run.</summary>
    public required string ReportId { get; init; }

    /// <summary>Gets the identifier of the suite that was run.</summary>
    public required string SuiteId { get; init; }

    /// <summary>Gets the version of the suite that was run.</summary>
    public required string SuiteVersion { get; init; }

    /// <summary>Gets the identifier of the target that was evaluated.</summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// Gets the typed model-capability declaration the target pinned for this run (router-bypass per D6);
    /// <c>null</c> when no pin was applied. Recording the pin keeps the bypass auditable per invariant 122.
    /// </summary>
    public ModelCapabilityDeclaration? PinnedModelCapability { get; init; }

    /// <summary>
    /// Gets the strong-typed multi-tenant identity for this run per ADR-0026 (I3). Defaults to
    /// <see cref="TenantId.Internal"/> so single-tenant callers do not need to plumb it.
    /// </summary>
    public TenantId TenantId { get; init; } = TenantId.Internal;

    /// <summary>Gets the timestamp at which the run started.</summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>Gets the timestamp at which the run completed.</summary>
    public required DateTimeOffset CompletedAt { get; init; }

    /// <summary>Gets the ordered set of scorer names applied during the run.</summary>
    public required IReadOnlyList<string> ScorerSet { get; init; }

    /// <summary>Gets the per-case results produced by the run.</summary>
    public required IReadOnlyList<EvalCaseResult> CaseResults { get; init; }

    /// <summary>Gets the run summary.</summary>
    public required EvalReportSummary Summary { get; init; }
}
