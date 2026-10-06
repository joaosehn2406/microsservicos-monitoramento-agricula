FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NuGet.config", "./"]
COPY ["API/Storage/Storage.csproj", "API/Storage/"]
RUN dotnet restore "API/Storage/Storage.csproj" --configfile NuGet.config
COPY . .
WORKDIR "/src/API/Storage"
RUN dotnet publish "Storage.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Storage.dll"]
