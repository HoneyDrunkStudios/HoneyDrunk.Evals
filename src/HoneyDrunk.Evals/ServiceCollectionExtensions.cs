using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.AI.Abstractions.Providers;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Orchestration;
using HoneyDrunk.Evals.Scoring;
using HoneyDrunk.Evals.Targets;
using HoneyDrunk.Evals.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HoneyDrunk.Evals;

/// <summary>Registers HoneyDrunk.Evals runtime services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HoneyDrunk.Evals runtime services: the default <see cref="IEvaluator"/>, the default
    /// <see cref="IEvalScorer"/> set (the deterministic <see cref="AutomatedRubricScorer"/> and,
    /// unless disabled, the non-deterministic <see cref="ModelAsJudgeScorer"/>), the telemetry emitter,
    /// and a <see cref="TimeProvider"/> fallback.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional runtime options configuration.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>
    /// Targets are registered separately via <see cref="AddChatTarget(IServiceCollection, string, ModelCapabilityDeclaration?)"/>
    /// because target identity and model pinning are per-evaluation concerns. The host supplies the
    /// upstream <c>IChatClient</c> / <c>IModelProvider</c> composition and the Kernel telemetry factory.
    /// </remarks>
    public static IServiceCollection AddHoneyDrunkEvals(this IServiceCollection services, Action<EvalsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new EvalsOptions();
        services.AddOptions<EvalsOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
            configure(options);
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<EvalsTelemetry>();
        services.AddSingleton<IEvaluator, DefaultEvaluator>();

        services.AddSingleton<IEvalScorer, AutomatedRubricScorer>();
        if (options.RegisterModelAsJudgeScorer)
        {
            services.AddSingleton<IEvalScorer, ModelAsJudgeScorer>();
        }

        return services;
    }

    /// <summary>
    /// Registers a <see cref="ChatTarget"/> as an <see cref="IEvalTarget"/>. When
    /// <paramref name="pinnedModelCapability"/> is non-null the target resolves the matching
    /// <see cref="IModelProvider"/> and bypasses the router per ADR-0023 D6 (invariant 125); otherwise it
    /// uses the host-composed router-managed <see cref="IChatClient"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="targetId">The stable target identifier.</param>
    /// <param name="pinnedModelCapability">The optional model capability to pin for the run.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddChatTarget(this IServiceCollection services, string targetId, ModelCapabilityDeclaration? pinnedModelCapability = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(targetId);

        if (pinnedModelCapability is null)
        {
            services.AddSingleton<IEvalTarget>(sp => new ChatTarget(targetId, sp.GetRequiredService<IChatClient>()));
        }
        else
        {
            services.AddSingleton<IEvalTarget>(sp => new ChatTarget(targetId, pinnedModelCapability, sp.GetServices<IModelProvider>()));
        }

        return services;
    }
}
