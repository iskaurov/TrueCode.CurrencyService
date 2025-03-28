# Dockerfile для CurrencyUpdater
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем весь исходный код
COPY ../.. .

# Переходим в каталог CurrencyUpdater и публикуем
WORKDIR /app/TrueCode.CurrencyService.CurrencyUpdater
RUN dotnet publish -c Release -o /out

# Финальный образ
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .

# Поддержка переменных окружения через .env
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "TrueCode.CurrencyService.CurrencyUpdater.dll"]
