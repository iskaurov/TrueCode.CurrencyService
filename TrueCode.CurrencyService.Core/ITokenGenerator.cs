using TrueCode.CurrencyService.Domain.Entities;

namespace TrueCode.CurrencyService.Core;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}