
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
