# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["otr-backend/otr-backend.csproj", "otr-backend/"]
RUN dotnet restore "./otr-backend/otr-backend.csproj"
COPY . .
WORKDIR "/src/otr-backend"
RUN dotnet build "./otr-backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./otr-backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .
# wwwroot/profile-pictures and keys get named volumes mounted over them at runtime, which
# Docker initializes as root-owned — pre-create + chown so the non-root app user can write.
RUN mkdir -p /app/wwwroot/profile-pictures /app/keys \
    && chown -R $APP_UID /app/wwwroot /app/keys
USER $APP_UID
ENTRYPOINT ["dotnet", "otr-backend.dll"]