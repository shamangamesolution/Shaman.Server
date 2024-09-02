FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . .
# Install dotnet-dump
RUN dotnet tool install -g dotnet-dump

ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj
RUN dotnet restore Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj
RUN dotnet publish Launchers/Shaman.Launchers.Standalone.Balancing/*.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Copy dotnet-dump from build-env
COPY --from=build-env /root/.dotnet /root/.dotnet
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Launchers.Standalone.Balancing.dll"]
