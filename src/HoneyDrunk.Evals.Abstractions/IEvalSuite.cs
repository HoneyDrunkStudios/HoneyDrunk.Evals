namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// A named, versioned collection of <see cref="EvalCase"/>s with a rubric specification.
/// Suite content (cases, rubric, thresholds) is owned by the consumer; Evals owns only the shape.
/// </summary>
public interface IEvalSuite
{
    /// <summary>Gets the stable suite identifier.</summary>
    string SuiteId { get; }

    /// <summary>Gets the suite version. Versioning is a Node-local invariant.</summary>
    string SuiteVersion { get; }

    /// <summary>
    /// D10 carve-out flag. <c>true</c> means eval-signal telemetry strips content (metadata only);
    /// <c>false</c> means content (case inputs, target outputs) MAY ride on telemetry payloads.
    /// </summary>
    /// <remarks>
    /// Follow-up: this boolean is intentionally first-pass (S1). It is expected to evolve to an enum
    /// (e.g. <c>SensitivityClassification</c> with values None / PII / Regulated / Internal-Only) once a
    /// downstream consumer drives a concrete need for graded sensitivity. The contract-shape canary
    /// scoped to <c>IEvalSuite</c> per D14 will catch the shape drift on that evolution and require a
    /// version bump.
    /// </remarks>
    bool IsSensitive { get; }

    /// <summary>Gets the ordered collection of cases that make up this suite.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The suite's cases.</returns>
    Task<IReadOnlyList<EvalCase>> GetCasesAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the rubric specification governing how this suite is scored.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The rubric specification.</returns>
    Task<RubricSpec> GetRubricAsync(CancellationToken cancellationToken = default);
}
