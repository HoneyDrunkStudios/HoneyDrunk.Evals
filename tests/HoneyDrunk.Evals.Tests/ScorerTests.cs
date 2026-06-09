using HoneyDrunk.AI.Abstractions.Chat;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Scoring;
using NSubstitute;
using Xunit;

namespace HoneyDrunk.Evals.Tests;

/// <summary>Determinism-flag and behavior tests for the default scorer set.</summary>
public sealed class ScorerTests
{
    private static EvalCase Case(string expected) => new()
    {
        CaseId = "c1",
        SuiteVersion = "1.0.0",
        InputJson = "in",
        ExpectedOutputJson = expected,
        Tags = new Dictionary<string, string>(),
    };

    /// <summary>The automated rubric scorer is deterministic.</summary>
    [Fact]
    public void AutomatedRubricScorer_is_deterministic()
    {
        Assert.True(new AutomatedRubricScorer().IsDeterministic);
    }

    /// <summary>The automated rubric scorer scores an exact match as 1.0.</summary>
    [Fact]
    public async Task AutomatedRubricScorer_scores_exact_match()
    {
        var score = await new AutomatedRubricScorer().ScoreAsync(Case("answer"), "answer");
        Assert.Equal(1d, score.Score);
        Assert.Equal(AutomatedRubricScorer.Name, score.ScorerName);
    }

    /// <summary>The automated rubric scorer scores a mismatch as 0.0.</summary>
    [Fact]
    public async Task AutomatedRubricScorer_scores_mismatch()
    {
        var score = await new AutomatedRubricScorer().ScoreAsync(Case("answer"), "wrong");
        Assert.Equal(0d, score.Score);
    }

    /// <summary>The model-as-judge scorer is non-deterministic.</summary>
    [Fact]
    public void ModelAsJudgeScorer_is_non_deterministic()
    {
        var client = Substitute.For<IChatClient>();
        Assert.False(new ModelAsJudgeScorer(client).IsDeterministic);
    }

    /// <summary>The model-as-judge scorer composes IChatClient and parses the numeric verdict.</summary>
    [Fact]
    public async Task ModelAsJudgeScorer_composes_chat_client()
    {
        var client = Substitute.For<IChatClient>();
        client.CompleteAsync(Arg.Any<IReadOnlyList<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>())
            .Returns(new ChatCompletion(new ChatMessage(ChatRole.Assistant, "0.75"), "judge:model"));

        var score = await new ModelAsJudgeScorer(client).ScoreAsync(Case("answer"), "candidate");

        Assert.Equal(0.75d, score.Score);
        await client.Received(1).CompleteAsync(Arg.Any<IReadOnlyList<ChatMessage>>(), Arg.Any<ChatOptions?>(), Arg.Any<CancellationToken>());
    }
}
