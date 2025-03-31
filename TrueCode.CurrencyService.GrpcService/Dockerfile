FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ../.. .

WORKDIR /app/TrueCode.CurrencyService.GrpcService
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:5163

ENTRYPOINT ["dotnet", "TrueCode.CurrencyService.GrpcService.dll"]