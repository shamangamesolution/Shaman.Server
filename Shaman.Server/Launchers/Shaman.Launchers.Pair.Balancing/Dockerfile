FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Servers/Shaman.Game/*.csproj
RUN dotnet restore Servers/Shaman.Game/*.csproj
RUN dotnet build Servers/Shaman.Game/*.csproj -c Release
RUN dotnet publish Servers/Shaman.Game/*.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Game.dll"]
