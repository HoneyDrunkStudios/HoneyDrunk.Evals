namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// The top-line summary of an evaluation run.
/// </summary>
/// <param name="AggregateScore">The aggregated score across all cases.</param>
/// <param name="CaseCount">The total number of cases evaluated.</param>
/// <param name="PassedCount">The number of cases that passed.</param>
/// <param name="FailedCount">The number of cases that failed.</param>
public sealed record EvalReportSummary(double AggregateScore, int CaseCount, int PassedCount, int FailedCount);
