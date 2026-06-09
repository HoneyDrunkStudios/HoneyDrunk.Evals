namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// A single score produced by an <see cref="IEvalScorer"/> for one case output.
/// </summary>
/// <param name="ScorerName">The name of the scorer that produced this score.</param>
/// <param name="Score">The numeric score (scorer-defined scale; typically normalized to <c>0.0</c>–<c>1.0</c>).</param>
/// <param name="Detail">Optional human-readable detail explaining the score.</param>
/// <param name="Tags">Scorer-defined metadata tags carried alongside the score.</param>
public sealed record EvalScore(string ScorerName, double Score, string? Detail, IReadOnlyDictionary<string, string> Tags);
