FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS base
WORKDIR /app
RUN apk add --no-cache ttf-dejavu fontconfig icu-libs

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["worker/hobio.worker.csproj", "worker/"]
COPY ["shared/hobio.shared.csproj", "shared/"]
RUN dotnet restore "worker/hobio.worker.csproj"

COPY . .
WORKDIR "/src/worker"
RUN dotnet build "hobio.worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "hobio.worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "hobio.worker.dll"]
