FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Launchers/Shaman.Launchers.Game/*.csproj
RUN dotnet restore Launchers/Shaman.Launchers.Game/*.csproj
RUN dotnet build Launchers/Shaman.Launchers.Game/*.csproj -c Release
RUN dotnet publish Launchers/Shaman.Launchers.Game/*.csproj -c Release -o ../../build

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Launchers.Game.dll"]
