FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "nuget.config"]
COPY ["src/domain/CodeDesignPlus.Net.Microservice.Tenants.Application/CodeDesignPlus.Net.Microservice.Tenants.Application.csproj", "domain/CodeDesignPlus.Net.Microservice.Tenants.Application/"]
COPY ["src/domain/CodeDesignPlus.Net.Microservice.Tenants.Domain/CodeDesignPlus.Net.Microservice.Tenants.Domain.csproj", "domain/CodeDesignPlus.Net.Microservice.Tenants.Domain/"]
COPY ["src/domain/CodeDesignPlus.Net.Microservice.Tenants.Infrastructure/CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.csproj", "domain/CodeDesignPlus.Net.Microservice.Tenants.Infrastructure/"]
COPY ["src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/CodeDesignPlus.Net.Microservice.Tenants.gRpc.csproj", "entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/"]
RUN dotnet restore "./entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/CodeDesignPlus.Net.Microservice.Tenants.gRpc.csproj" /p:PublishReadyToRun=true
COPY ["src/domain/", "domain/"]
COPY ["src/entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/", "entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/"]

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet publish "./entrypoints/CodeDesignPlus.Net.Microservice.Tenants.gRpc/CodeDesignPlus.Net.Microservice.Tenants.gRpc.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
EXPOSE 8080
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_GCDynamicAdaptationMode=1 \
    DOTNET_GCConserveMemory=5 \
    DOTNET_GCHeapHardLimitPercent=70
USER app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeDesignPlus.Net.Microservice.Tenants.gRpc.dll"]
