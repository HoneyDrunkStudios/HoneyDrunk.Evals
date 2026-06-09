using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Targets;

/// <summary>
/// Default <see cref="IEvalTarget"/> that wraps an <see cref="IChatClient"/>. When a
/// <see cref="ModelCapabilityDeclaration"/> is pinned, the target resolves the matching
/// <see cref="IModelProvider"/> from the host's composition and bypasses the router per ADR-0023 D6
/// (invariant 125); otherwise it uses the router-managed <see cref="IChatClient"/> it was given.
/// </summary>
/// <remarks>
/// The pin (when applied) is the typed declaration the host wires; the resolved provider's
/// <see cref="IModelProvider.GetChatClient"/> is used for the run, and the declaration is surfaced on
/// <see cref="PinnedModelCapability"/> so the evaluator records it on every <see cref="EvalReport"/>
/// (invariant 122). This implementation never mutates the target under test (invariant 121).
/// </remarks>
public sealed class ChatTarget : IEvalTarget
{
    private readonly IChatClient chatClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatTarget"/> class in router-managed mode.
    /// </summary>
    /// <param name="targetId">The stable target identifier.</param>
    /// <param name="chatClient">The router-managed chat client used to invoke the target.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="targetId"/> is empty.</exception>
    public ChatTarget(string targetId, IChatClient chatClient)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetId);
        ArgumentNullException.ThrowIfNull(chatClient);
        this.TargetId = targetId;
        this.chatClient = chatClient;
        this.PinnedModelCapability = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatTarget"/> class in pinned (router-bypass) mode.
    /// The chat client is sourced from the matching provider's
    /// <see cref="IModelProvider.GetChatClient"/> for the pinned model, bypassing the router per D6.
    /// </summary>
    /// <param name="targetId">The stable target identifier.</param>
    /// <param name="pinnedModelCapability">The model capability to pin for the run.</param>
    /// <param name="providers">The available model providers, sourced through the host's DI container.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="targetId"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no provider matches the pinned declaration.</exception>
    public ChatTarget(string targetId, ModelCapabilityDeclaration pinnedModelCapability, IEnumerable<IModelProvider> providers)
    {
        ArgumentException.ThrowIfNullOrEmpty(targetId);
        ArgumentNullException.ThrowIfNull(pinnedModelCapability);
        ArgumentNullException.ThrowIfNull(providers);

        var provider = providers.FirstOrDefault(p => string.Equals(p.ProviderId, pinnedModelCapability.ProviderId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                $"No IModelProvider registered for pinned provider '{pinnedModelCapability.ProviderId}'. Router-bypass pinning per ADR-0023 D6 requires the pinned provider to be composed in the host.");

        this.TargetId = targetId;
        this.PinnedModelCapability = pinnedModelCapability;
        this.chatClient = provider.GetChatClient(pinnedModelCapability.ModelId);
    }

    /// <inheritdoc />
    public string TargetId { get; }

    /// <inheritdoc />
    public ModelCapabilityDeclaration? PinnedModelCapability { get; }

    /// <inheritdoc />
    public async Task<string> InvokeAsync(EvalCase testCase, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCase);

        var messages = new[] { new ChatMessage(ChatRole.User, testCase.InputJson) };
        var completion = await this.chatClient.CompleteAsync(messages, options: null, cancellationToken).ConfigureAwait(false);
        return completion.Message.Content;
    }
}
