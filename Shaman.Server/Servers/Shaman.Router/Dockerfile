FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Servers/Shaman.Router/*.csproj
RUN dotnet restore Servers/Shaman.Router/*.csproj 
RUN dotnet publish Servers/Shaman.Router/*.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Router.dll"]
