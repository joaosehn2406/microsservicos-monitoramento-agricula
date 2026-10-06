FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NuGet.config", "./"]
COPY ["API/WeatherCollector/WeatherCollector.csproj", "API/WeatherCollector/"]
RUN dotnet restore "API/WeatherCollector/WeatherCollector.csproj" --configfile NuGet.config
COPY . .
WORKDIR "/src/API/WeatherCollector"
RUN dotnet publish "WeatherCollector.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WeatherCollector.dll"]
