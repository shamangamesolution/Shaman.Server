FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj
RUN dotnet restore Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj
RUN dotnet publish Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj -c Release -o ../../build

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Launchers.Standalone.Balancing.dll"]
