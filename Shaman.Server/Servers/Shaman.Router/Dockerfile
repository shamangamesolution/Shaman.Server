FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean   Servers/Shaman.Router/*.csproj
RUN dotnet restore Servers/Shaman.Router/*.csproj 
RUN dotnet publish Servers/Shaman.Router/*.csproj -c Release -o ../../build

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.Router.dll"]