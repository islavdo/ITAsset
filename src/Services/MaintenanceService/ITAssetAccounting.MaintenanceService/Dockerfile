FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/MaintenanceService/ITAssetAccounting.MaintenanceService/ITAssetAccounting.MaintenanceService.csproj
RUN dotnet publish src/Services/MaintenanceService/ITAssetAccounting.MaintenanceService/ITAssetAccounting.MaintenanceService.csproj -c Release -o /app/publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ITAssetAccounting.MaintenanceService.dll"]
