using HoneyDrunk.AI.Abstractions.Providers;

namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// The thing being evaluated — a chat model, an agent, a retrieval pipeline, or a memory-backed
/// workflow. Abstracts over what a case is run against without binding Evals to any consumer runtime.
/// </summary>
/// <remarks>
/// Per ADR-0023 D6, <see cref="IEvalTarget"/> is the Grid's only sanctioned router-bypass boundary
/// (invariant 125): when <see cref="PinnedModelCapability"/> is non-null an implementation MAY resolve
/// the pinned model directly, bypassing the routing layer, because regression detection cannot use the
/// router it is built to test. No <see cref="IEvalTarget"/> implementation may mutate the target under
/// test (invariant 121 — read-only observer). This is also a canary-frozen hot-path surface (invariant 126).
/// </remarks>
public interface IEvalTarget
{
    /// <summary>Gets the stable target identifier recorded on every <see cref="EvalReport"/>.</summary>
    string TargetId { get; }

    /// <summary>
    /// Gets the pinned model capability when this target pins a specific model for deterministic
    /// selection (router-bypass per D6); <c>null</c> when the target is router-managed. Typed reference
    /// into <c>HoneyDrunk.AI.Abstractions</c>.
    /// </summary>
    ModelCapabilityDeclaration? PinnedModelCapability { get; }

    /// <summary>Invokes the target for a single case and returns its raw output.</summary>
    /// <param name="testCase">The case to run against the target.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The target's output for the case.</returns>
    Task<string> InvokeAsync(EvalCase testCase, CancellationToken cancellationToken = default);
}
