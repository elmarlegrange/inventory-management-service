# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Stage 2: SDK Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for optimal layer caching
COPY ["src/InventoryManagement.Domain/InventoryManagement.Domain.csproj", "src/InventoryManagement.Domain/"]
COPY ["src/InventoryManagement.Application/InventoryManagement.Application.csproj", "src/InventoryManagement.Application/"]
COPY ["src/InventoryManagement.Infrastructure/InventoryManagement.Infrastructure.csproj", "src/InventoryManagement.Infrastructure/"]
COPY ["src/InventoryManagement.Api/InventoryManagement.Api.csproj", "src/InventoryManagement.Api/"]

RUN dotnet restore "src/InventoryManagement.Api/InventoryManagement.Api.csproj"

# Copy source code and build
COPY ["src/", "src/"]
WORKDIR "/src/src/InventoryManagement.Api"
RUN dotnet build "InventoryManagement.Api.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "InventoryManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagement.Api.dll"]
