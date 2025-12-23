# =============================================================================
# CobranzaCloud Backend - Dockerfile
# =============================================================================

# ============ BUILD STAGE ============
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY src/CobranzaCloud.Api/*.csproj ./CobranzaCloud.Api/
COPY src/CobranzaCloud.Core/*.csproj ./CobranzaCloud.Core/
COPY src/CobranzaCloud.Application/*.csproj ./CobranzaCloud.Application/
COPY src/CobranzaCloud.Infrastructure/*.csproj ./CobranzaCloud.Infrastructure/

RUN dotnet restore CobranzaCloud.Api/CobranzaCloud.Api.csproj

# Copy source code and build
COPY src/ .
RUN dotnet publish CobranzaCloud.Api/CobranzaCloud.Api.csproj -c Release -o /app/publish --no-restore

# ============ DEVELOPMENT STAGE ============
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS development
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Entry point for development with hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/CobranzaCloud.Api/CobranzaCloud.Api.csproj", "--urls", "http://+:5000"]

# ============ PRODUCTION STAGE ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS production
WORKDIR /app
EXPOSE 5000

# Security: Create non-root user
RUN addgroup -g 1000 dotnet && \
    adduser -u 1000 -G dotnet -s /bin/sh -D dotnet

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R dotnet:dotnet /app

USER dotnet

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "CobranzaCloud.Api.dll"]
