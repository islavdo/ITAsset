FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/Gateway/ITAssetAccounting.Gateway/ITAssetAccounting.Gateway.csproj
RUN dotnet publish src/Services/Gateway/ITAssetAccounting.Gateway/ITAssetAccounting.Gateway.csproj -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ITAssetAccounting.Gateway.dll"]
