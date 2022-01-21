FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Launchers/Shaman.Launchers.Pair/*.csproj
RUN dotnet restore Launchers/Shaman.Launchers.Pair/*.csproj
RUN dotnet build Launchers/Shaman.Launchers.Pair/*.csproj -c Release
RUN dotnet publish Launchers/Shaman.Launchers.Pair/*.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Launchers.Pair.dll"]
