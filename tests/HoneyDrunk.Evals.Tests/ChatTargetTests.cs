using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Orchestration;
using HoneyDrunk.Evals.Scoring;
using HoneyDrunk.Evals.Targets;
using HoneyDrunk.Evals.Telemetry;
using HoneyDrunk.Evals.Tests.Fixtures;
using NSubstitute;
using Xunit;

namespace HoneyDrunk.Evals.Tests;

/// <summary>Tests for <see cref="ChatTarget"/> including the D6 router-bypass pinning path.</summary>
public sealed class ChatTargetTests
{
    private static EvalCase Case(string input) => new()
    {
        CaseId = "c1",
        SuiteVersion = "1.0.0",
        InputJson = input,
        Tags = new Dictionary<string, string>(),
    };

    /// <summary>Router-managed mode invokes the supplied chat client and exposes no pin.</summary>
    [Fact]
    public async Task Unpinned_target_uses_supplied_chat_client()
    {
        var client = Substitute.For<IChatClient>();
        client.CompleteAsync(Arg.Any<IReadOnlyList<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "pong"), "router:model"));

        var target = new ChatTarget("chat-default", client);
        var output = await target.InvokeAsync(Case("ping"));

        Assert.Null(target.PinnedModelCapability);
        Assert.Equal("pong", output);
    }

    /// <summary>
    /// Pinned mode resolves the matching provider, bypasses the router, and surfaces the pin so the
    /// evaluator records it on the report.
    /// </summary>
    [Fact]
    public async Task Pinned_target_resolves_provider_and_records_pin_end_to_end()
    {
        var pin = new ModelCapabilityDeclaration("openai", "gpt-x", 8192, true, false, true, ["eastus"], 3m, 6m);

        var pinnedClient = Substitute.For<IChatClient>();
        pinnedClient.CompleteAsync(Arg.Any<IReadOnlyList<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "pinned-out"), "openai:gpt-x"));

        var provider = Substitute.For<IModelProvider>();
        provider.ProviderId.Returns("openai");
        provider.GetChatClient("gpt-x").Returns(pinnedClient);

        var otherProvider = Substitute.For<IModelProvider>();
        otherProvider.ProviderId.Returns("anthropic");

        var target = new ChatTarget("chat-pinned", pin, [otherProvider, provider]);

        Assert.Same(pin, target.PinnedModelCapability);
        provider.Received(1).GetChatClient("gpt-x");

        var suite = new StubEvalSuite("suite-pin", "1.0.0", isSensitive: false, [Case("ping")]);
        var scorers = new IEvalScorer[] { new AutomatedRubricScorer() };
        var evaluator = new DefaultEvaluator(new EvalsTelemetry(new NullTelemetryActivityFactory()), TimeProvider.System);

        var report = await evaluator.RunAsync(suite, target, scorers);

        Assert.Equal(pin, report.PinnedModelCapability);
        Assert.Equal("pinned-out", report.CaseResults[0].TargetOutput);
    }

    /// <summary>Pinned mode throws when no provider matches the pinned declaration.</summary>
    [Fact]
    public void Pinned_target_throws_when_no_provider_matches()
    {
        var pin = new ModelCapabilityDeclaration("missing", "m", 1024, false, false, false, ["local"], null, null);
        Assert.Throws<InvalidOperationException>(() => new ChatTarget("chat-pinned", pin, []));
    }
}
