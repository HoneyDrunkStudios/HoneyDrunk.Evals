using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Tests.Fixtures;

/// <summary>An <see cref="IEvalTarget"/> that echoes a fixed output, optionally carrying a pin.</summary>
public sealed class EchoEvalTarget : IEvalTarget
{
    private readonly string output;

    /// <summary>Initializes a new instance of the <see cref="EchoEvalTarget"/> class.</summary>
    /// <param name="targetId">The target id.</param>
    /// <param name="output">The fixed output to echo.</param>
    /// <param name="pinnedModelCapability">The optional pinned capability.</param>
    public EchoEvalTarget(string targetId, string output, ModelCapabilityDeclaration? pinnedModelCapability = null)
    {
        this.TargetId = targetId;
        this.output = output;
        this.PinnedModelCapability = pinnedModelCapability;
    }

    /// <inheritdoc />
    public string TargetId { get; }

    /// <inheritdoc />
    public ModelCapabilityDeclaration? PinnedModelCapability { get; }

    /// <inheritdoc />
    public Task<string> InvokeAsync(EvalCase testCase, CancellationToken cancellationToken = default) => Task.FromResult(this.output);
}
