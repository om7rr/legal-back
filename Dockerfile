# Multi-stage build for the API. Build context = backend/
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore LegalPlatform.slnx
RUN dotnet publish src/LegalPlatform.Api/LegalPlatform.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
# Run as the non-root user provided by the base image.
USER $APP_UID
ENTRYPOINT ["dotnet", "LegalPlatform.Api.dll"]
