FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["SaraAgro.Api/SaraAgro.Api.csproj", "SaraAgro.Api/"]
COPY ["SaraAgro.Shared/SaraAgro.Shared.csproj", "SaraAgro.Shared/"]

RUN dotnet restore "SaraAgro.Api/SaraAgro.Api.csproj"

COPY . .

WORKDIR "/src/SaraAgro.Api"

RUN dotnet publish "SaraAgro.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SaraAgro.Api.dll"]