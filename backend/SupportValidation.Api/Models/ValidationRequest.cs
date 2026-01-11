using System.ComponentModel.DataAnnotations;

namespace SupportValidation.Api.Models;

public sealed class ValidationRequest
{
    [Required]
    public string? State { get; set; }

    [Required]
    public List<string>? Provisions { get; set; }
}
