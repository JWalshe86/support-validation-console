namespace SupportValidation.Api.Models;

public sealed class ValidationRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public string State { get; init; } = "";
    public List<string> Provisions { get; init; } = new();

    public ValidationStatus Status { get; init; }
    public List<string> MissingProvisions { get; init; } = new();
}
