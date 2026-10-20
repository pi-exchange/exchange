# Контекст сборки — корень репозитория (docker compose build, см. docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0.402 AS build
WORKDIR /src

# Сначала только файлы, определяющие зависимости: слой restore кешируется.
COPY global.json Directory.Build.props ./
COPY src/Exchange.Api/Exchange.Api.csproj src/Exchange.Api/packages.lock.json src/Exchange.Api/
RUN dotnet restore src/Exchange.Api/Exchange.Api.csproj --locked-mode

COPY src ./src
RUN dotnet publish src/Exchange.Api/Exchange.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Exchange.Api.dll"]
