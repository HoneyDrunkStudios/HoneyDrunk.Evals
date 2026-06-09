namespace HoneyDrunk.Evals;

/// <summary>Options for HoneyDrunk.Evals runtime registration.</summary>
public sealed class EvalsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the default model-as-judge scorer is registered
    /// alongside the deterministic rubric scorer when <c>AddHoneyDrunkEvals</c> wires the default
    /// scorer set. Defaults to <c>true</c>.
    /// </summary>
    public bool RegisterModelAsJudgeScorer { get; set; } = true;
}
