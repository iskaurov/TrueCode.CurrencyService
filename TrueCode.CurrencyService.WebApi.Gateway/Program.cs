using TrueCode.CurrencyService.Infrastructure.Common;

var builder = WebApplication.CreateBuilder(args);
var envFile = builder.Environment.IsDevelopment() ? ".env.dev" : ".env";
EnvLoader.Load("../", envFile);
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var port = builder.Configuration["GATEWAY_PORT"] ?? "5150";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.MapReverseProxy();
app.Run();