FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NuGet.config", "./"]
COPY ["API/Notifications/Notifications.csproj", "API/Notifications/"]
RUN dotnet restore "API/Notifications/Notifications.csproj" --configfile NuGet.config
COPY . .
WORKDIR "/src/API/Notifications"
RUN dotnet publish "Notifications.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notifications.dll"]
