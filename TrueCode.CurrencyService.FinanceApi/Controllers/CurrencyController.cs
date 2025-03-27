using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrueCode.CurrencyService.Core.Services;

namespace TrueCode.CurrencyService.FinanceApi.Controllers;

[ApiController]
[Route("currency")]
[Authorize]
public class CurrencyController(ICurrencyService currencyService) : ControllerBase
{
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var result = await currencyService.GetFavoritesByUserIdAsync(userId);
        return Ok(result);
    }
}