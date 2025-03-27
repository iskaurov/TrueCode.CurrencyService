using System.ComponentModel.DataAnnotations;

namespace TrueCode.CurrencyService.UserApi.Models.Auth;

public class LoginRequest
{
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}   