using System.Diagnostics;
using HoneyDrunk.Evals.Abstractions;
using HoneyDrunk.Kernel.Abstractions.Telemetry;

namespace HoneyDrunk.Evals.Telemetry;

/// <summary>
/// Emits per-suite-run, per-case-result, and per-regression activities for the Evals Node via
/// Kernel's <see cref="ITelemetryActivityFactory"/>.
/// </summary>
/// <remarks>
/// D10 content carve-out (invariant 124): when <see cref="IEvalSuite.IsSensitive"/> is <c>false</c>,
/// activity payloads MAY include case content (case input, target output, expected output). When it
/// is <c>true</c>, payloads carry metadata only (identity, scores, durations). This is a deliberate
/// carve-out from the content-never-in-telemetry rule the other AI-sector Nodes adopt; Evals emits to
/// Pulse one-way and takes no runtime dependency on Pulse.
/// </remarks>
public sealed class EvalsTelemetry
{
    /// <summary>The suite-run activity name.</summary>
    public const string SuiteRunActivityName = "HoneyDrunk.Evals.SuiteRun";

    /// <summary>The per-case-result activity name.</summary>
    public const string CaseResultActivityName = "HoneyDrunk.Evals.CaseResult";

    /// <summary>The regression activity name.</summary>
    public const string RegressionActivityName = "HoneyDrunk.Evals.Regression";

    /// <summary>The suite id tag.</summary>
    public const string SuiteIdTag = "honeydrunk.evals.suite_id";

    /// <summary>The suite version tag.</summary>
    public const string SuiteVersionTag = "honeydrunk.evals.suite_version";

    /// <summary>The target id tag.</summary>
    public const string TargetIdTag = "honeydrunk.evals.target_id";

    /// <summary>The case id tag.</summary>
    public const string CaseIdTag = "honeydrunk.evals.case_id";

    /// <summary>The aggregate-score tag.</summary>
    public const string AggregateScoreTag = "honeydrunk.evals.aggregate_score";

    /// <summary>The sensitive-suite tag.</summary>
    public const string SensitiveTag = "honeydrunk.evals.sensitive";

    /// <summary>The case-input content tag (carved out when the suite is sensitive).</summary>
    public const string CaseInputTag = "honeydrunk.evals.case_input";

    /// <summary>The target-output content tag (carved out when the suite is sensitive).</summary>
    public const string TargetOutputTag = "honeydrunk.evals.target_output";

    /// <summary>The expected-output content tag (carved out when the suite is sensitive).</summary>
    public const string ExpectedOutputTag = "honeydrunk.evals.expected_output";

    private readonly ITelemetryActivityFactory activityFactory;

    /// <summary>Initializes a new instance of the <see cref="EvalsTelemetry"/> class.</summary>
    /// <param name="activityFactory">The Kernel telemetry activity factory.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="activityFactory"/> is <c>null</c>.</exception>
    public EvalsTelemetry(ITelemetryActivityFactory activityFactory)
    {
        ArgumentNullException.ThrowIfNull(activityFactory);
        this.activityFactory = activityFactory;
    }

    /// <summary>Starts a suite-run activity. Carries only metadata (no per-case content).</summary>
    /// <param name="suite">The suite being run.</param>
    /// <param name="target">The target under evaluation.</param>
    /// <returns>The started activity, or <c>null</c> if not sampled.</returns>
    public Activity? StartSuiteRun(IEvalSuite suite, IEvalTarget target)
    {
        ArgumentNullException.ThrowIfNull(suite);
        ArgumentNullException.ThrowIfNull(target);

        var tags = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [SuiteIdTag] = suite.SuiteId,
            [SuiteVersionTag] = suite.SuiteVersion,
            [TargetIdTag] = target.TargetId,
            [SensitiveTag] = suite.IsSensitive,
        };

        return this.activityFactory.Start(SuiteRunActivityName, tags);
    }

    /// <summary>
    /// Starts a per-case-result activity. Honors the D10 carve-out: case content is included only when
    /// <paramref name="suite"/> is not sensitive.
    /// </summary>
    /// <param name="suite">The suite the case belongs to.</param>
    /// <param name="testCase">The case that was evaluated.</param>
    /// <param name="targetOutput">The output the target produced.</param>
    /// <returns>The started activity, or <c>null</c> if not sampled.</returns>
    public Activity? StartCaseResult(IEvalSuite suite, EvalCase testCase, string targetOutput)
    {
        ArgumentNullException.ThrowIfNull(suite);
        ArgumentNullException.ThrowIfNull(testCase);

        var tags = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [SuiteIdTag] = suite.SuiteId,
            [SuiteVersionTag] = suite.SuiteVersion,
            [CaseIdTag] = testCase.CaseId,
            [SensitiveTag] = suite.IsSensitive,
        };

        if (!suite.IsSensitive)
        {
            tags[CaseInputTag] = testCase.InputJson;
            tags[TargetOutputTag] = targetOutput;
            tags[ExpectedOutputTag] = testCase.ExpectedOutputJson;
        }

        return this.activityFactory.Start(CaseResultActivityName, tags);
    }

    /// <summary>
    /// Starts a regression activity. Honors the D10 carve-out: the case input is included only when
    /// <paramref name="suite"/> is not sensitive.
    /// </summary>
    /// <param name="suite">The suite the regression was detected on.</param>
    /// <param name="testCase">The case that regressed.</param>
    /// <param name="previousScore">The previous baseline score.</param>
    /// <param name="currentScore">The current (regressed) score.</param>
    /// <returns>The started activity, or <c>null</c> if not sampled.</returns>
    public Activity? StartRegression(IEvalSuite suite, EvalCase testCase, double previousScore, double currentScore)
    {
        ArgumentNullException.ThrowIfNull(suite);
        ArgumentNullException.ThrowIfNull(testCase);

        var tags = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [SuiteIdTag] = suite.SuiteId,
            [SuiteVersionTag] = suite.SuiteVersion,
            [CaseIdTag] = testCase.CaseId,
            [SensitiveTag] = suite.IsSensitive,
            ["honeydrunk.evals.previous_score"] = previousScore,
            ["honeydrunk.evals.current_score"] = currentScore,
            ["honeydrunk.evals.score_delta"] = currentScore - previousScore,
        };

        if (!suite.IsSensitive)
        {
            tags[CaseInputTag] = testCase.InputJson;
        }

        return this.activityFactory.Start(RegressionActivityName, tags);
    }
}
