FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app
COPY . .
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet clean Web/Shaman.BackEnd/*.csproj
RUN dotnet restore Web/Shaman.BackEnd/*.csproj
RUN dotnet publish Web/Shaman.BackEnd/*.csproj -c Release -o ../../build
COPY validation.json ../build

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Shaman.BackEnd.dll"]