using System.Diagnostics;
using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Kernel.Abstractions.Telemetry;

namespace HoneyDrunk.Evals.Tests.Fixtures;

/// <summary>
/// A telemetry activity factory that records the tags it was asked to start activities with and returns
/// no activity (unsampled). Lets tests assert on the carve-out behavior of <c>EvalsTelemetry</c>.
/// </summary>
public sealed class NullTelemetryActivityFactory : ITelemetryActivityFactory
{
    /// <summary>Gets the recorded (name, tags) pairs from each <see cref="Start"/> call.</summary>
    public List<(string Name, IReadOnlyDictionary<string, object?>? Tags)> Calls { get; } = [];

    /// <inheritdoc />
    public Activity? Start(string name, IReadOnlyDictionary<string, object?>? additionalTags = null)
    {
        this.Calls.Add((name, additionalTags));
        return null;
    }

    /// <inheritdoc />
    public Activity? StartExplicit(string name, IGridContext gridContext, IOperationContext? operationContext = null, IReadOnlyDictionary<string, object?>? additionalTags = null)
    {
        this.Calls.Add((name, additionalTags));
        return null;
    }
}
