using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Evals.Telemetry;
using HoneyDrunk.Kernel.Abstractions.Identity;

namespace HoneyDrunk.Evals.Orchestration;

/// <summary>
/// Default <see cref="IEvaluator"/>. Pulls cases from the suite, invokes the target per case, runs the
/// scorer set on each output, and assembles an <see cref="EvalReport"/> with full run provenance per
/// ADR-0023 D12 (invariant 122) — including the typed pinned <c>ModelCapabilityDeclaration</c> (D6) and
/// the carried <c>TenantId</c> (ADR-0026 / I3).
/// </summary>
/// <remarks>
/// The evaluator emits telemetry through <see cref="EvalsTelemetry"/>, honoring the D10 content carve-out
/// (invariant 124). It never mutates the target under test (invariant 121). Reports are durable artifacts
/// (invariant 123); the host wires a store such as <c>InMemoryReportStore</c> to persist them.
/// </remarks>
public sealed class DefaultEvaluator : IEvaluator
{
    private readonly EvalsTelemetry telemetry;
    private readonly TimeProvider timeProvider;

    /// <summary>Initializes a new instance of the <see cref="DefaultEvaluator"/> class.</summary>
    /// <param name="telemetry">The telemetry emitter.</param>
    /// <param name="timeProvider">The time provider used for run timestamps.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required argument is <c>null</c>.</exception>
    public DefaultEvaluator(EvalsTelemetry telemetry, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(telemetry);
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.telemetry = telemetry;
        this.timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<EvalReport> RunAsync(IEvalSuite suite, IEvalTarget target, IReadOnlyList<IEvalScorer> scorers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(suite);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(scorers);

        var startedAt = this.timeProvider.GetUtcNow();
        using var suiteActivity = this.telemetry.StartSuiteRun(suite, target);

        var cases = await suite.GetCasesAsync(cancellationToken).ConfigureAwait(false);
        var caseResults = new List<EvalCaseResult>(cases.Count);
        var scorerNames = scorers.Select(s => s.ScorerName).ToArray();

        var passedCount = 0;
        var aggregateScoreSum = 0d;
        var tenant = TenantId.Internal;

        foreach (var testCase in cases)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Carry the tenant from the first non-internal case so per-tenant runs are attributable (I3).
            if (tenant.IsInternal && !testCase.TenantId.IsInternal)
            {
                tenant = testCase.TenantId;
            }

            var output = await target.InvokeAsync(testCase, cancellationToken).ConfigureAwait(false);
            using var caseActivity = this.telemetry.StartCaseResult(suite, testCase, output);

            var scores = new List<EvalScore>(scorers.Count);
            foreach (var scorer in scorers)
            {
                var score = await scorer.ScoreAsync(testCase, output, cancellationToken).ConfigureAwait(false);
                scores.Add(score);
            }

            var caseScore = scores.Count == 0 ? 0d : scores.Average(s => s.Score);
            aggregateScoreSum += caseScore;
            if (caseScore >= PassThreshold)
            {
                passedCount++;
            }

            caseResults.Add(new EvalCaseResult(testCase.CaseId, output, scores, OperatorObservations: null));
        }

        var completedAt = this.timeProvider.GetUtcNow();
        var caseCount = caseResults.Count;
        var aggregateScore = caseCount == 0 ? 0d : aggregateScoreSum / caseCount;

        return new EvalReport
        {
            ReportId = Guid.NewGuid().ToString("N"),
            SuiteId = suite.SuiteId,
            SuiteVersion = suite.SuiteVersion,
            TargetId = target.TargetId,
            PinnedModelCapability = target.PinnedModelCapability,
            TenantId = tenant,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            ScorerSet = scorerNames,
            CaseResults = caseResults,
            Summary = new EvalReportSummary(aggregateScore, caseCount, passedCount, caseCount - passedCount),
        };
    }

    private const double PassThreshold = 0.5d;
}
