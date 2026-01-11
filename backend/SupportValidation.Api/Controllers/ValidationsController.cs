using Microsoft.AspNetCore.Mvc;
using SupportValidation.Api.Models;
using SupportValidation.Api.Services;

namespace SupportValidation.Api.Controllers;

[ApiController]
public sealed class ValidationsController : ControllerBase
{
    private readonly IValidationService _validationService;
    private readonly IValidationStore _store;

    public ValidationsController(IValidationService validationService, IValidationStore store)
    {
        _validationService = validationService;
        _store = store;
    }

    [HttpPost("/validate")]
    [ProducesResponseType(typeof(ValidationRun), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<ValidationRun> Validate([FromBody] ValidationRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid payload",
                Detail = "Request body must be a valid JSON object."
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.State))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid payload",
                Detail = "`state` is required and must be a non-empty string."
            });
        }

        if (request.Provisions is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid payload",
                Detail = "`provisions` is required and must be an array of strings."
            });
        }

        var run = _validationService.Validate(request);
        return Ok(run);
    }

    [HttpGet("/validations")]
    [ProducesResponseType(typeof(IEnumerable<ValidationRun>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ValidationRun>> GetAll()
        => Ok(_store.GetAll());

    [HttpGet("/validations/{id:guid}")]
    [ProducesResponseType(typeof(ValidationRun), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<ValidationRun> GetById([FromRoute] Guid id)
    {
        var run = _store.GetById(id);
        if (run is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not found",
                Detail = $"No validation run exists for id '{id}'."
            });
        }

        return Ok(run);
    }
}
