using HoneyDrunk.Kernel.Abstractions.Identity;

namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// A single evaluation case — input, expected output or rubric criteria, and per-case metadata.
/// Value type; cataloged with <c>kind: "type"</c> per the grid-wide naming rule.
/// </summary>
public sealed record EvalCase
{
    /// <summary>Gets the case identifier, unique within the suite.</summary>
    public required string CaseId { get; init; }

    /// <summary>Gets the suite version this case was authored against.</summary>
    public required string SuiteVersion { get; init; }

    /// <summary>Gets the case input, serialized as JSON.</summary>
    public required string InputJson { get; init; }

    /// <summary>Gets the expected output, serialized as JSON, when the case asserts an exact expectation.</summary>
    public string? ExpectedOutputJson { get; init; }

    /// <summary>Gets the rubric criteria, serialized as JSON, when the case is scored against a rubric.</summary>
    public string? RubricCriteriaJson { get; init; }

    /// <summary>Gets the case metadata tags.</summary>
    public required IReadOnlyDictionary<string, string> Tags { get; init; }

    /// <summary>
    /// Gets the strong-typed multi-tenant identity for this case per ADR-0026 (I3). Defaults to
    /// <see cref="TenantId.Internal"/> so a single-tenant suite does not need to plumb it. Carried onto
    /// the case so per-tenant regression analysis (e.g. "did model X regress on tenant Y's suite but not
    /// tenant Z's") is expressible without rebinding cases between tenants.
    /// </summary>
    public TenantId TenantId { get; init; } = TenantId.Internal;
}
