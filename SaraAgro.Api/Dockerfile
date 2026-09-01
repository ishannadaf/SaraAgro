# =========================================================
# BUILD STAGE
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first for better Docker layer caching
COPY ["SaraAgro.Api/SaraAgro.Api.csproj", "SaraAgro.Api/"]
COPY ["SaraAgro.Shared/SaraAgro.Shared.csproj", "SaraAgro.Shared/"]

# Restore API dependencies
RUN dotnet restore "SaraAgro.Api/SaraAgro.Api.csproj"

# Copy complete solution
COPY . .

# Build API
WORKDIR "/src/SaraAgro.Api"

RUN dotnet publish "SaraAgro.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================================================
# RUNTIME STAGE
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Railway provides the PORT environment variable.
# ASP.NET Core will listen on all network interfaces.
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "SaraAgro.Api.dll"]