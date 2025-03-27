using TrueCode.CurrencyService.Domain.Entities;

namespace TrueCode.CurrencyService.Application;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}