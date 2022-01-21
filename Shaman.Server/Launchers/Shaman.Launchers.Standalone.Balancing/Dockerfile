FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj
RUN dotnet restore Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj
RUN dotnet publish Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj -c Release -o /app/build

FROM microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Launchers.Standalone.Balancing.dll"]
