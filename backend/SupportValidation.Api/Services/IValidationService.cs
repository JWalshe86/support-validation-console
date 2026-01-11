using SupportValidation.Api.Models;

namespace SupportValidation.Api.Services;

public interface IValidationService
{
    ValidationRun Validate(ValidationRequest request);
}
