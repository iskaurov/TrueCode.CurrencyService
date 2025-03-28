# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ../.. .

WORKDIR /app/TrueCode.CurrencyService.UserApi
RUN dotnet publish -c Release -o /out

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "TrueCode.CurrencyService.UserApi.dll"]