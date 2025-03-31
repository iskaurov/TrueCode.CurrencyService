using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using TrueCode.CurrencyService.Core.Services;
using TrueCode.CurrencyService.Domain.Repositories;
using TrueCode.CurrencyService.FinanceApi.Middlewares;
using TrueCode.CurrencyService.Infrastructure.Common;
using TrueCode.CurrencyService.Infrastructure.Db;
using TrueCode.CurrencyService.Infrastructure.Repositories;
using CurrencyService = TrueCode.CurrencyService.Core.Services.CurrencyService;

var builder = WebApplication.CreateBuilder(args);

var envFile = builder.Environment.IsDevelopment() ? ".env.dev" : ".env";
EnvLoader.Load("../",envFile);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables();

builder.Services.AddScoped<JwtAuthInterceptor>();

// БД
builder.Services.AddDbContext<CurrencyDbContext>(options =>
    options.UseNpgsql(builder.Configuration["DEFAULT_CONNECTION"]));

// JWT
var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!);

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
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});
builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Finance API", Version = "v1" });
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

builder.Services.
    AddScoped<IUserRepository, UserRepository>().
    AddScoped<ICurrencyService, CurrencyService>();

builder.Services.AddControllers();

// -------------------------------------
// Конфигурация порта
// -------------------------------------
var port = builder.Configuration["FINANCE_API_PORT"] ?? "5152";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port), listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

namespace TrueCode.CurrencyService.FinanceApi { public abstract class Program; }