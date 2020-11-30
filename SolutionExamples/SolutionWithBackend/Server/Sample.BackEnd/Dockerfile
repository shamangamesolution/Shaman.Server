FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean Bundles/Shaman.BackEnd/*.csproj
RUN dotnet restore Bundles/Shaman.BackEnd/*.csproj --ignore-failed-sources
RUN dotnet publish Bundles/Shaman.BackEnd/*.csproj -c Release -o ../../build

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.BackEnd.dll"]