using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Kernel.Abstractions.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace HoneyDrunk.Evals.Tests;

/// <summary>DI registration tests for <c>AddHoneyDrunkEvals</c> and <c>AddChatTarget</c>.</summary>
public sealed class ServiceCollectionExtensionsTests
{
    private static IServiceCollection BaseHost()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<ITelemetryActivityFactory>());
        services.AddSingleton(Substitute.For<IChatClient>());
        return services;
    }

    /// <summary>AddHoneyDrunkEvals resolves the evaluator and the default scorer set.</summary>
    [Fact]
    public void AddHoneyDrunkEvals_resolves_evaluator_and_scorers()
    {
        using var provider = BaseHost().AddHoneyDrunkEvals().BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IEvaluator>());

        var scorers = provider.GetServices<IEvalScorer>().ToArray();
        Assert.Contains(scorers, s => s.IsDeterministic);      // AutomatedRubricScorer
        Assert.Contains(scorers, s => !s.IsDeterministic);     // ModelAsJudgeScorer
    }

    /// <summary>Disabling the judge scorer registers only the deterministic scorer.</summary>
    [Fact]
    public void AddHoneyDrunkEvals_can_disable_judge_scorer()
    {
        using var provider = BaseHost()
            .AddHoneyDrunkEvals(o => o.RegisterModelAsJudgeScorer = false)
            .BuildServiceProvider();

        var scorers = provider.GetServices<IEvalScorer>().ToArray();
        Assert.Single(scorers);
        Assert.True(scorers[0].IsDeterministic);
    }

    /// <summary>AddChatTarget (unpinned) resolves an IEvalTarget with no pin.</summary>
    [Fact]
    public void AddChatTarget_unpinned_resolves_target()
    {
        using var provider = BaseHost().AddHoneyDrunkEvals().AddChatTarget("chat-default").BuildServiceProvider();

        var target = provider.GetRequiredService<IEvalTarget>();
        Assert.Equal("chat-default", target.TargetId);
        Assert.Null(target.PinnedModelCapability);
    }

    /// <summary>AddChatTarget (pinned) resolves a router-bypass target carrying the pin.</summary>
    [Fact]
    public void AddChatTarget_pinned_resolves_target_with_pin()
    {
        var pin = new ModelCapabilityDeclaration("openai", "gpt-x", 8192, true, false, true, ["eastus"], 3m, 6m);

        var provider = Substitute.For<IModelProvider>();
        provider.ProviderId.Returns("openai");
        provider.GetChatClient("gpt-x").Returns(Substitute.For<IChatClient>());

        var services = BaseHost();
        services.AddSingleton(provider);

        using var sp = services.AddHoneyDrunkEvals().AddChatTarget("chat-pinned", pin).BuildServiceProvider();

        var target = sp.GetRequiredService<IEvalTarget>();
        Assert.Equal(pin, target.PinnedModelCapability);
    }
}
