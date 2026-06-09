using System.Collections.Concurrent;
using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Providers.InMemory;

/// <summary>
/// Thread-safe in-memory store for <see cref="EvalReport"/> artifacts, keyed by
/// <see cref="EvalReport.ReportId"/>. Satisfies the durability contract (ADR-0023 D13, invariant 123)
/// for the lifetime of the process; production hosts swap in a durable backend per D2.
/// </summary>
public sealed class InMemoryReportStore
{
    private readonly ConcurrentDictionary<string, EvalReport> reports = new(StringComparer.Ordinal);

    /// <summary>Saves a report, overwriting any existing report with the same <see cref="EvalReport.ReportId"/>.</summary>
    /// <param name="report">The report to save.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is <c>null</c>.</exception>
    public Task SaveAsync(EvalReport report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(report);
        this.reports[report.ReportId] = report;
        return Task.CompletedTask;
    }

    /// <summary>Gets a report by id, or <c>null</c> when no report with that id is stored.</summary>
    /// <param name="reportId">The report identifier.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The stored report, or <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="reportId"/> is empty.</exception>
    public Task<EvalReport?> GetAsync(string reportId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(reportId);
        return Task.FromResult(this.reports.TryGetValue(reportId, out var report) ? report : null);
    }

    /// <summary>Gets all stored reports.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A snapshot of all stored reports.</returns>
    public Task<IReadOnlyList<EvalReport>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<EvalReport> snapshot = this.reports.Values.ToArray();
        return Task.FromResult(snapshot);
    }
}
