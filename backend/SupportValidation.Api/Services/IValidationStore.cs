using SupportValidation.Api.Models;

namespace SupportValidation.Api.Services;

public interface IValidationStore
{
    IReadOnlyList<ValidationRun> GetAll();
    ValidationRun? GetById(Guid id);
    void Add(ValidationRun run);
}
