using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TrueCode.CurrencyService.Core.Services;
using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.GrpcService.Services;
using TrueCode.CurrencyService.Infrastructure.Db;
using TrueCode.CurrencyService.Infrastructure.Repositories;
//using TrueCode.CurrencyService.GrpcService.Services;
using TrueCode.CurrencyService.Infrastructure.Common;

var builder = WebApplication.CreateBuilder(args);

// === Подгружаем .env (учитывая среду) ===
var envFile = builder.Environment.IsDevelopment() ? ".env.dev" : ".env";
EnvLoader.Load("../", envFile);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables();

// === JWT ===
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// === БД и зависимости ===
var connectionString = builder.Configuration["DEFAULT_CONNECTION"];
builder.Services.AddDbContext<CurrencyDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddGrpcReflection();
builder.Services.AddAuthorization();

// === Порт ===
var port = builder.Configuration["GRPC_SERVICE_PORT"] ?? "5153";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port), listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<CurrencyGrpcService>();
if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();