FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Launchers/Shaman.Launchers.MM/*.csproj
RUN dotnet restore Launchers/Shaman.Launchers.MM/*.csproj
RUN dotnet publish Launchers/Shaman.Launchers.MM/*.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Launchers.MM.dll"]
