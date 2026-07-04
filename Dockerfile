# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BeeKeeperApp.csproj", "./"]
RUN dotnet restore "./BeeKeeperApp.csproj"
COPY . .
RUN dotnet publish "BeeKeeperApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BeeKeeperApp.dll"]
