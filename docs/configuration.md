# Haven Configuration Guide

## Overview

Haven stores its enhanced feature configuration in `config/haven.json`. This file controls all Haven-specific features while maintaining compatibility with the standard Jellyfin configuration.

## Configuration File Structure

```json
{
  "ApiCompatibilityMode": true,
  "NetflixMode": false,
  "IntroDetectionEnabled": true,
  "DownloadTrackingEnabled": true,
  "RecommendationsEnabled": true,
  "EnhancedTranscodingEnabled": true,
  "InternalBrandingName": "Haven",
  "ExternalBrandingName": "Jellyfin Server",
  "VersionSuffix": "-haven.1.0",
  "ShowHavenBranding": true,
  "DownloadManagers": {
    "Radarr": {
      "Enabled": false,
      "Url": "http://localhost:7878",
      "ApiKey": "",
      "AdditionalSettings": {}
    },
    "Sonarr": {
      "Enabled": false,
      "Url": "http://localhost:8989",
      "ApiKey": "",
      "AdditionalSettings": {}
    },
    "SABnzbd": {
      "Enabled": false,
      "Url": "http://localhost:8080",
      "ApiKey": "",
      "AdditionalSettings": {}
    },
    "qBittorrent": {
      "Enabled": false,
      "Url": "http://localhost:8081",
      "ApiKey": "",
      "AdditionalSettings": {}
    }
  }
}
```

## Feature Configuration

### API Compatibility Mode

**Setting**: `ApiCompatibilityMode`  
**Default**: `true`  
**Description**: When enabled, Haven reports as "Jellyfin Server" to maintain compatibility with existing Jellyfin apps.

```json
"ApiCompatibilityMode": true
```

### Netflix Mode

**Setting**: `NetflixMode`  
**Default**: `false`  
**Description**: Enables Netflix-style UI enhancements including hover previews, dynamic content rows, and modern layouts.

```json
"NetflixMode": true
```

When enabled, additional configuration is available via the API:
- Hover preview delays
- Auto-play settings
- Billboard hero sections
- Quick action buttons

### Intro Detection

**Setting**: `IntroDetectionEnabled`  
**Default**: `false`  
**Description**: Enables AI-powered intro and credit detection for automatic skip markers.

```json
"IntroDetectionEnabled": true
```

### Download Tracking

**Setting**: `DownloadTrackingEnabled`  
**Default**: `false`  
**Description**: Enables real-time download progress tracking from integrated download managers.

```json
"DownloadTrackingEnabled": true
```

### ML Recommendations

**Setting**: `RecommendationsEnabled`  
**Default**: `false`  
**Description**: Enables machine learning-powered content recommendations based on viewing patterns.

```json
"RecommendationsEnabled": true
```

### Enhanced Transcoding

**Setting**: `EnhancedTranscodingEnabled`  
**Default**: `false`  
**Description**: Enables intelligent transcoding with automatic device detection and optimal format selection.

```json
"EnhancedTranscodingEnabled": true
```

## Download Manager Integration

### Radarr Configuration

```json
"Radarr": {
  "Enabled": true,
  "Url": "http://radarr.local:7878",
  "ApiKey": "your-radarr-api-key",
  "AdditionalSettings": {
    "QualityProfile": "HD-1080p",
    "RootFolder": "/movies"
  }
}
```

### Sonarr Configuration

```json
"Sonarr": {
  "Enabled": true,
  "Url": "http://sonarr.local:8989",
  "ApiKey": "your-sonarr-api-key",
  "AdditionalSettings": {
    "QualityProfile": "HD-720p/1080p",
    "RootFolder": "/tv"
  }
}
```

### SABnzbd Configuration

```json
"SABnzbd": {
  "Enabled": true,
  "Url": "http://sabnzbd.local:8080",
  "ApiKey": "your-sabnzbd-api-key",
  "AdditionalSettings": {
    "Category": "movies"
  }
}
```

### qBittorrent Configuration

```json
"qBittorrent": {
  "Enabled": true,
  "Url": "http://qbittorrent.local:8081",
  "ApiKey": "your-qbittorrent-api-key",
  "AdditionalSettings": {
    "Category": "media"
  }
}
```

## Environment Variables

Haven supports configuration via environment variables:

- `HAVEN_ENHANCED_FEATURES`: Enable/disable all enhanced features (true/false)
- `HAVEN_API_COMPATIBILITY`: Force API compatibility mode (true/false)
- `HAVEN_CONFIG_PATH`: Path to haven.json configuration file

## API Endpoints

Haven exposes additional API endpoints for feature management:

- `GET /Enhanced/Features` - Get feature status
- `POST /Enhanced/Features/{feature}` - Enable/disable specific feature
- `GET /Enhanced/Downloads` - Get download progress
- `POST /Enhanced/CompatibilityMode` - Toggle compatibility mode

## Migration from Jellyfin

Haven automatically migrates existing Jellyfin configurations. No manual intervention is required. Your existing:
- User accounts
- Libraries
- Metadata
- Plugins
- Settings

Will all be preserved and continue to work.

## Troubleshooting

### Apps showing "Server Unreachable"

Ensure `ApiCompatibilityMode` is set to `true`.

### Download tracking not working

1. Verify download manager URLs are accessible
2. Check API keys are correct
3. Test connection via `/Enhanced/Downloads/TestConnection/{service}`

### Features not appearing

1. Check feature is enabled in configuration
2. Restart Haven server after configuration changes
3. Clear browser cache if using web interface