using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TrueCode.CurrencyService.Core;
using TrueCode.CurrencyService.Core.Options;
using TrueCode.CurrencyService.Core.Services;
using TrueCode.CurrencyService.Domain.Entities;
using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.Infrastructure.Common;
using TrueCode.CurrencyService.Infrastructure.Db;
using TrueCode.CurrencyService.Infrastructure.Repositories;
using TrueCode.CurrencyService.WebApi.UserApi;

var builder = WebApplication.CreateBuilder(args);

var envFile = builder.Environment.IsDevelopment() ? ".env.dev" : ".env";
EnvLoader.Load("../",envFile);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables();

// -------------------------------------
// JwtOptions из ENV
// -------------------------------------
builder.Services.Configure<JwtOptions>(opt =>
{
    opt.Key = builder.Configuration["JWT:Key"]!;
    opt.Issuer = builder.Configuration["JWT:Issuer"]!;
    opt.Audience = builder.Configuration["JWT:Audience"]!;
});

var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!);

// -------------------------------------
// Подключение к PostgreSQL
// -------------------------------------
builder.Services.AddDbContext<CurrencyDbContext>(options =>
    options.UseNpgsql(builder.Configuration["DEFAULT_CONNECTION"]));

// -------------------------------------
// JWT-аутентификация
// -------------------------------------
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});

// -------------------------------------
// Swagger + Bearer
// -------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// -------------------------------------
// Регистрация сервисов
// -------------------------------------
builder.Services.AddControllers();

builder.Services.AddScoped<IUserService, UserService>().
AddScoped<IUserRepository, UserRepository>().
AddScoped<ITokenGenerator, JwtTokenGenerator>().
AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// -------------------------------------
// Конфигурация порта
// -------------------------------------
var port = builder.Configuration["USER_API_PORT"] ?? "5151";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// -------------------------------------
// Построение и запуск
// -------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

namespace TrueCode.CurrencyService.WebApi.UserApi { public class Program; }