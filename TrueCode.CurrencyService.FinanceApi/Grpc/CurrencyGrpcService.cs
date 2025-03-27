using System.Security.Claims;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using TrueCode.CurrencyService.Application.Services;

namespace TrueCode.CurrencyService.FinanceApi.Grpc;

[Authorize]
public class CurrencyGrpcService(ICurrencyService currencyService) 
    : CurrencyService.CurrencyServiceBase
{
    public override async Task<FavoritesResponse> GetFavorites(FavoritesRequest request, ServerCallContext context)
    {
        var userIdClaim = context.GetHttpContext().User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or missing JWT token"));
        }

        var currencies = await currencyService.GetFavoritesByUserIdAsync(userId);

        var response = new FavoritesResponse();
        response.Currencies.AddRange(currencies.Select(c => new Currency
        {
            Name = c.Name,
            Rate = (double)c.Rate
        }));

        return response;
    }
}