FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ARG BUILD_CONFIGURATION=Release
COPY ["api/hobio.api.csproj", "api/"]
COPY ["shared/hobio.shared.csproj", "shared/"]
RUN dotnet restore "api/hobio.api.csproj"

COPY . .
WORKDIR "/src/api"
RUN dotnet build "hobio.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "hobio.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hobio.api.dll"]
