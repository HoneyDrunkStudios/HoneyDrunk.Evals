using System.Collections.Concurrent;
using HoneyDrunk.Evals.Abstractions;

namespace HoneyDrunk.Evals.Providers.InMemory;

/// <summary>
/// Thread-safe in-memory repository of <see cref="IEvalSuite"/> definitions, keyed by
/// <see cref="IEvalSuite.SuiteId"/>. Suitable for tests, fixtures, and local development per ADR-0023 D2.
/// </summary>
public sealed class InMemorySuiteRepository
{
    private readonly ConcurrentDictionary<string, IEvalSuite> suites = new(StringComparer.Ordinal);

    /// <summary>Adds or replaces a suite, keyed by its <see cref="IEvalSuite.SuiteId"/>.</summary>
    /// <param name="suite">The suite to store.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suite"/> is <c>null</c>.</exception>
    public Task AddAsync(IEvalSuite suite, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(suite);
        this.suites[suite.SuiteId] = suite;
        return Task.CompletedTask;
    }

    /// <summary>Gets a suite by id, or <c>null</c> when no suite with that id is stored.</summary>
    /// <param name="suiteId">The suite identifier.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The stored suite, or <c>null</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="suiteId"/> is empty.</exception>
    public Task<IEvalSuite?> GetAsync(string suiteId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(suiteId);
        return Task.FromResult(this.suites.TryGetValue(suiteId, out var suite) ? suite : null);
    }

    /// <summary>Gets all stored suites.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A snapshot of all stored suites.</returns>
    public Task<IReadOnlyList<IEvalSuite>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<IEvalSuite> snapshot = this.suites.Values.ToArray();
        return Task.FromResult(snapshot);
    }
}
