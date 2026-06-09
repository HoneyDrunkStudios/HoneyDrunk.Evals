namespace HoneyDrunk.Evals.Abstractions;

/// <summary>
/// Observation-only Operator signals captured during a case run (D7).
/// Each property is nullable to distinguish two distinct states:
///   - <c>null</c> means the corresponding observer was not wired into the host's composition (e.g. Operator unavailable, observer not registered, or this target did not flow through that signal path).
///   - non-null means the observer ran and recorded the value (including <c>false</c> / <c>0</c> as a meaningful observation).
/// </summary>
/// <param name="SafetyFilterFired">
/// <c>true</c> when <c>ISafetyFilter</c> would have fired on the target output; <c>false</c> when it ran and did not fire; <c>null</c> when the safety-filter observer was not wired.
/// </param>
/// <param name="CostMarker">The observed cost marker for the case; <c>null</c> when the cost observer was not wired.</param>
/// <param name="ApprovalWouldBeRequired">
/// <c>true</c> when an approval path would have been required; <c>false</c> when it would not; <c>null</c> when the audit observer was not wired.
/// </param>
public sealed record ObservedOperatorPrediction(bool? SafetyFilterFired, decimal? CostMarker, bool? ApprovalWouldBeRequired);
