FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app
COPY . .
RUN dotnet clean Monkeys.csproj
RUN dotnet restore Monkeys.csproj
RUN dotnet publish Monkeys.csproj -c Release -o build

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/build .
ENTRYPOINT ["dotnet", "Monkeys.dll"]
