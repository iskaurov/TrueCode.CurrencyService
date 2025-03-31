using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.IdentityModel.Tokens;

namespace TrueCode.CurrencyService.FinanceApi.Middlewares;

public class JwtAuthInterceptor : Interceptor
{
    private readonly string _jwtSecret;

    public JwtAuthInterceptor(IConfiguration configuration)
    {
        _jwtSecret = configuration["Jwt:Secret"]!;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var authHeader = context.RequestHeaders
            .FirstOrDefault(h => h.Key == "authorization")?.Value;

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing JWT token"));
        }

        var token = authHeader.Substring("Bearer ".Length);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return await continuation(request, context);
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid JWT token"));
        }
    }
}