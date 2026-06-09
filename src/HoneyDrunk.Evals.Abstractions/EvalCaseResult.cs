namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// The result of evaluating a single case — its output, the scores produced for it, and any
/// observation-only Operator signals captured during the run.
/// </summary>
/// <param name="CaseId">The case the result is for.</param>
/// <param name="TargetOutput">The raw output the target produced for the case.</param>
/// <param name="Scores">The scores produced by the scorer set for this case.</param>
/// <param name="OperatorObservations">Observation-only Operator signals (D7); <c>null</c> when no observers were wired.</param>
public sealed record EvalCaseResult(string CaseId, string TargetOutput, IReadOnlyList<EvalScore> Scores, ObservedOperatorPrediction? OperatorObservations);
