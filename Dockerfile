# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Haven.Server/Haven.Server.csproj", "src/Haven.Server/"]
COPY ["src/Haven.Api/Haven.Api.csproj", "src/Haven.Api/"]
COPY ["src/Haven.Common/Haven.Common.csproj", "src/Haven.Common/"]
COPY ["src/Haven.Data/Haven.Data.csproj", "src/Haven.Data/"]
COPY ["Haven.sln", "./"]

# Restore dependencies
RUN dotnet restore "Haven.sln"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/Haven.Server"
RUN dotnet build "Haven.Server.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Haven.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install ffmpeg and other dependencies
RUN apt-get update && apt-get install -y \
    ffmpeg \
    libfontconfig1 \
    libgomp1 \
    libva-drm2 \
    mesa-va-drivers \
    && rm -rf /var/lib/apt/lists/*

# Create directories
RUN mkdir -p /config /cache /media /transcodes

# Copy published files
COPY --from=publish /app/publish .

# Copy web client files (placeholder - in production, would copy actual web files)
RUN mkdir -p /app/jellyfin-web
COPY --from=publish /app/publish/wwwroot /app/jellyfin-web

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8 \
    JELLYFIN_DATA_DIR=/config \
    JELLYFIN_CACHE_DIR=/cache \
    JELLYFIN_CONFIG_DIR=/config/config \
    JELLYFIN_LOG_DIR=/config/log \
    JELLYFIN_MEDIA_DIR=/media \
    JELLYFIN_TRANSCODE_DIR=/transcodes \
    HAVEN_ENHANCED_FEATURES=true

# Expose ports
EXPOSE 8096/tcp
EXPOSE 8920/tcp
EXPOSE 1900/udp
EXPOSE 7359/udp

# Volume mappings
VOLUME ["/config", "/cache", "/media", "/transcodes"]

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8096/health || exit 1

# Set entrypoint
ENTRYPOINT ["dotnet", "havenfin.dll"]