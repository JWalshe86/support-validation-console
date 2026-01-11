using SupportValidation.Api.Models;

namespace SupportValidation.Api.Services;

public sealed class ValidationService : IValidationService
{
    private static readonly Dictionary<string, List<string>> RequiredByState =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["DE"] = new() { "A", "B" },
            ["IE"] = new() { "A" },
            ["US"] = new() { "C" }
        };

    private readonly IValidationStore _store;

    public ValidationService(IValidationStore store)
    {
        _store = store;
    }

    public ValidationRun Validate(ValidationRequest request)
    {
        var state = (request.State ?? "").Trim();
        var provisions = request.Provisions ?? new List<string>();

        var cleaned = provisions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var required = RequiredByState.TryGetValue(state, out var req)
            ? req
            : new List<string>();

        var missing = required
            .Where(r => !cleaned.Contains(r, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var status = missing.Count == 0 ? ValidationStatus.PASSED : ValidationStatus.FAILED;

        var run = new ValidationRun
        {
            State = state,
            Provisions = cleaned,
            Status = status,
            MissingProvisions = missing
        };

        _store.Add(run);
        return run;
    }
}
