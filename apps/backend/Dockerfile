# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for NuGet restore layer caching (test projects excluded)
COPY src/MedControl.Domain/MedControl.Domain.csproj src/MedControl.Domain/
COPY src/MedControl.Application/MedControl.Application.csproj src/MedControl.Application/
COPY src/MedControl.Infrastructure/MedControl.Infrastructure.csproj src/MedControl.Infrastructure/
COPY src/MedControl.Api/MedControl.Api.csproj src/MedControl.Api/

RUN dotnet restore src/MedControl.Api/MedControl.Api.csproj

# Copy source and publish in one step to avoid --no-restore invalidation issues
COPY src/ src/

RUN dotnet publish src/MedControl.Api/MedControl.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# Stage 2: Runtime (minimal image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN groupadd --system --gid 1001 medcontrol \
    && useradd --system --uid 1001 --gid 1001 --no-create-home medcontrol

COPY --from=build --chown=medcontrol:medcontrol /app/publish .

USER medcontrol

EXPOSE 8080

ENTRYPOINT ["dotnet", "MedControl.Api.dll"]
