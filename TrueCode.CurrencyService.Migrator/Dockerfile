#FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
#USER $APP_UID
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["TrueCode.CurrencyService.Migrator/TrueCode.CurrencyService.Migrator.csproj", "TrueCode.CurrencyService.Migrator/"]
#COPY ["TrueCode.CurrencyService.Infrastructure.Db/TrueCode.CurrencyService.Infrastructure.Db.csproj", "TrueCode.CurrencyService.Infrastructure.Db/"]
#COPY ["TrueCode.CurrencyService.Core/TrueCode.CurrencyService.Core.csproj", "TrueCode.CurrencyService.Core/"]
#COPY ["TrueCode.CurrencyService.Core.Domain/TrueCode.CurrencyService.Core.Domain.csproj", "TrueCode.CurrencyService.Core.Domain/"]
#RUN dotnet restore "TrueCode.CurrencyService.Migrator/TrueCode.CurrencyService.Migrator.csproj"
#COPY . .
#WORKDIR "/src/TrueCode.CurrencyService.Migrator"
#RUN dotnet build "TrueCode.CurrencyService.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "TrueCode.CurrencyService.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "TrueCode.CurrencyService.Migrator.dll"]

# Используем базовый образ .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Копируем всё решение
COPY ../.. .

# Переходим в каталог мигратора
WORKDIR /app/TrueCode.CurrencyService.Migrator

# Сборка (можно без restore — уже всё скопировано)
RUN dotnet publish -c Release -o /out

# Финальный образ
FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

COPY --from=build /out .

# Запуск
ENTRYPOINT ["dotnet", "TrueCode.CurrencyService.Migrator.dll"]