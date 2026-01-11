using System.Collections.Concurrent;
using SupportValidation.Api.Models;

namespace SupportValidation.Api.Services;

public sealed class InMemoryValidationStore : IValidationStore
{
    private readonly ConcurrentDictionary<Guid, ValidationRun> _runs = new();

    public IReadOnlyList<ValidationRun> GetAll()
        => _runs.Values
            .OrderByDescending(r => r.Timestamp)
            .ToList();

    public ValidationRun? GetById(Guid id)
        => _runs.TryGetValue(id, out var run) ? run : null;

    public void Add(ValidationRun run)
        => _runs[run.Id] = run;
}
