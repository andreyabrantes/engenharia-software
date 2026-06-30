# ============================================================
# Dockerfile — Backend .NET 8 API
# Multi-stage build para imagem final reduzida
# ============================================================

# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia csproj e restaura dependencias (cache layer)
COPY backend/BoraAli.Core/BoraAli.Core.csproj      BoraAli.Core/
COPY backend/BoraAli.Infrastructure/BoraAli.Infrastructure.csproj BoraAli.Infrastructure/
COPY backend/BoraAli.Api/BoraAli.Api.csproj        BoraAli.Api/
COPY backend/BoraAli.Tests/BoraAli.Tests.csproj    BoraAli.Tests/
RUN dotnet restore backend/BoraAli.Api/BoraAli.Api.csproj

# Copia todo o codigo e compila
COPY backend/ .
RUN dotnet publish BoraAli.Api/BoraAli.Api.csproj -c Release -o /app

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Cria diretorio para SQLite e logs
RUN mkdir -p /app/data /app/Logs

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:5188
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/BoraAli.db"

EXPOSE 5188

ENTRYPOINT ["dotnet", "BoraAli.Api.dll"]
