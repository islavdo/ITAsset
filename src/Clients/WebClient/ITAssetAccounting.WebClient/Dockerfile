FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Clients/WebClient/ITAssetAccounting.WebClient/ITAssetAccounting.WebClient.csproj
RUN dotnet publish src/Clients/WebClient/ITAssetAccounting.WebClient/ITAssetAccounting.WebClient.csproj -c Release -o /app/publish --no-restore
FROM nginx:1.27-alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY deploy/nginx/web-client.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
