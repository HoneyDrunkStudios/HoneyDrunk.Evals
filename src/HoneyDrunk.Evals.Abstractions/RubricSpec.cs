namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// The rubric specification for a suite — which scorers are required and how their scores are weighted.
/// </summary>
/// <param name="RequiredScorers">The scorer names a valid run of the suite must include.</param>
/// <param name="ScorerWeights">Per-scorer weights used when aggregating the suite score.</param>
public sealed record RubricSpec(IReadOnlyList<string> RequiredScorers, IReadOnlyDictionary<string, double> ScorerWeights);
