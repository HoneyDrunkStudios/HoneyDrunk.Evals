using System.Globalization;
using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Scoring;

/// <summary>
/// Non-deterministic scorer that composes an <see cref="IChatClient"/> to judge a target output
/// against the case's rubric criteria (rubric-as-prompt). Marked <see cref="IsDeterministic"/> =
/// <c>false</c> per ADR-0023 D5 because it performs inference. Inference always goes through
/// <see cref="IChatClient"/> — the scorer never instantiates a provider or a routing layer of its own.
/// </summary>
public sealed class ModelAsJudgeScorer : IEvalScorer
{
    /// <summary>The stable scorer name.</summary>
    public const string Name = "model-as-judge";

    private readonly IChatClient chatClient;

    /// <summary>Initializes a new instance of the <see cref="ModelAsJudgeScorer"/> class.</summary>
    /// <param name="chatClient">The chat client used to judge outputs.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chatClient"/> is <c>null</c>.</exception>
    public ModelAsJudgeScorer(IChatClient chatClient)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        this.chatClient = chatClient;
    }

    /// <inheritdoc />
    public string ScorerName => Name;

    /// <inheritdoc />
    public bool IsDeterministic => false;

    /// <inheritdoc />
    public async Task<EvalScore> ScoreAsync(EvalCase testCase, string targetOutput, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testCase);
        ArgumentNullException.ThrowIfNull(targetOutput);

        var rubric = testCase.RubricCriteriaJson ?? testCase.ExpectedOutputJson ?? "(no rubric criteria supplied)";
        var prompt =
            "You are an evaluation judge. Score the candidate output from 0.0 to 1.0 against the rubric. " +
            "Reply with the numeric score only.\n" +
            $"Rubric:\n{rubric}\n\nCandidate output:\n{targetOutput}";

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, "You are a strict, terse evaluation judge."),
            new ChatMessage(ChatRole.User, prompt),
        };

        var completion = await this.chatClient.CompleteAsync(messages, options: null, cancellationToken).ConfigureAwait(false);
        var raw = completion.Message.Content;
        var tags = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["judge_model"] = completion.ModelId,
        };

        var score = TryParseScore(raw);
        return new EvalScore(Name, score, $"Judge model '{completion.ModelId}' returned: {raw}", tags);
    }

    private static double TryParseScore(string raw)
    {
        if (double.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return Math.Clamp(parsed, 0d, 1d);
        }

        return 0d;
    }
}
