FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update && \
    apt-get install -y fonts-dejavu fontconfig libicu-dev && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["worker/hobio.worker.csproj", "worker/"]
COPY ["shared/hobio.shared.csproj", "shared/"]
RUN dotnet restore "worker/hobio.worker.csproj"

COPY . .
WORKDIR "/src/worker"
RUN find /src/worker -name "*.cs" | sort 
RUN dotnet build "hobio.worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "hobio.worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "hobio.worker.dll"]
