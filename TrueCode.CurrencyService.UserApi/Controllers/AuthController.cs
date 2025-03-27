using Microsoft.AspNetCore.Mvc;
using TrueCode.CurrencyService.Application.Services;
using TrueCode.CurrencyService.UserApi.Models.Auth;

namespace TrueCode.CurrencyService.UserApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService) 
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            await userService.RegisterAsync(request.Name, request.Password);
            return Ok();
        }
        catch (Exception ex)
        {
            return Conflict(ex.Message); // Можно заменить на более тонкую обработку
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var token = await userService.LoginAsync(request.Name, request.Password);
            return Ok(new AuthResponse { Token = token });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}