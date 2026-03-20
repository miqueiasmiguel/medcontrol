# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY MedControl.slnx ./
COPY src/MedControl.Domain/MedControl.Domain.csproj src/MedControl.Domain/
COPY src/MedControl.Application/MedControl.Application.csproj src/MedControl.Application/
COPY src/MedControl.Infrastructure/MedControl.Infrastructure.csproj src/MedControl.Infrastructure/
COPY src/MedControl.Api/MedControl.Api.csproj src/MedControl.Api/

RUN dotnet restore MedControl.slnx

# Copy source
COPY src/ src/

RUN dotnet build MedControl.slnx \
    --no-restore \
    --configuration Release \
    --warnaserror

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish src/MedControl.Api/MedControl.Api.csproj \
    --no-build \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# Stage 3: Runtime (minimal image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN groupadd --system --gid 1001 medcontrol \
    && useradd --system --uid 1001 --gid 1001 --no-create-home medcontrol

COPY --from=publish --chown=medcontrol:medcontrol /app/publish .

USER medcontrol

EXPOSE 8080

ENTRYPOINT ["dotnet", "MedControl.Api.dll"]
