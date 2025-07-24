# Haven Enhanced API Documentation

## Overview

Haven extends the Jellyfin API with additional endpoints under the `/Enhanced/*` namespace. All enhanced endpoints require authentication and respect the same permission model as standard Jellyfin endpoints.

## Base URL

```
http(s)://your-server:8096/Enhanced/
```

## Authentication

Use the same authentication methods as Jellyfin:
- API Key in header: `X-Emby-Token: {api_key}`
- API Key in query: `?api_key={api_key}`

## Feature Management

### Get Feature Status

```http
GET /Enhanced/Features
```

**Response:**
```json
{
  "netflixMode": true,
  "introDetection": false,
  "downloadTracking": true,
  "recommendations": true,
  "enhancedTranscoding": false,
  "apiCompatibilityMode": true,
  "version": "1.0",
  "serverBranding": "Haven"
}
```

### Update Feature

```http
POST /Enhanced/Features/{featureName}
Content-Type: application/json

true
```

**Parameters:**
- `featureName`: One of `netflixmode`, `introdetection`, `downloadtracking`, `recommendations`, `enhancedtranscoding`

**Response:**
```json
{
  "success": true,
  "feature": "netflixmode",
  "enabled": true
}
```

## Download Tracking

### Get Active Downloads

```http
GET /Enhanced/Downloads
```

**Response:**
```json
{
  "enabled": true,
  "downloads": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "Movie Title",
      "progress": 45.5,
      "eta": "15 minutes",
      "status": "downloading",
      "source": "Radarr"
    }
  ]
}
```

### Test Download Manager Connection

```http
POST /Enhanced/Downloads/TestConnection/{service}
```

**Parameters:**
- `service`: One of `radarr`, `sonarr`, `sabnzbd`, `qbittorrent`

**Response:**
```json
{
  "success": true,
  "service": "radarr",
  "message": "Successfully connected to radarr",
  "version": "3.2.2"
}
```

## Netflix Mode

### Get Netflix Mode Configuration

```http
GET /Enhanced/NetflixMode/config
```

**Response:**
```json
{
  "enableHoverPreviews": true,
  "enableDynamicRows": true,
  "enableAutoPlayPreviews": true,
  "previewDelayMs": 1000,
  "enableBillboardHero": true,
  "enableQuickActions": true
}
```

### Get Dynamic Content Rows

```http
GET /Enhanced/NetflixMode/rows/{userId}
```

**Response:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Continue Watching",
    "type": "ContinueWatching",
    "items": [...],
    "showProgress": true,
    "enableHoverPreview": true
  },
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "title": "Trending Now",
    "type": "Trending",
    "items": [...],
    "showProgress": false,
    "enableHoverPreview": true
  }
]
```

### Get Hover Preview Data

```http
GET /Enhanced/NetflixMode/preview/{itemId}
```

**Response:**
```json
{
  "itemId": "550e8400-e29b-41d4-a716-446655440000",
  "previewVideoUrl": "/api/items/550e8400-e29b-41d4-a716-446655440000/preview",
  "previewDuration": "00:00:30",
  "keyMoments": [
    {
      "time": "00:00:05",
      "description": "Opening scene"
    }
  ],
  "quickActions": [
    {
      "type": "Play",
      "icon": "play_arrow",
      "primary": true
    }
  ]
}
```

## Intro Detection

### Get Intro Markers

```http
GET /Enhanced/IntroDetection/{itemId}/intro
```

**Response:**
```json
{
  "itemId": "550e8400-e29b-41d4-a716-446655440000",
  "introStart": "00:00:05",
  "introEnd": "00:01:35",
  "confidence": 0.85,
  "detectionMethod": "Audio Fingerprinting",
  "detectedAt": "2024-01-15T10:30:00Z"
}
```

### Get Credit Markers

```http
GET /Enhanced/IntroDetection/{itemId}/credits
```

**Response:**
```json
{
  "itemId": "550e8400-e29b-41d4-a716-446655440000",
  "creditStart": "00:42:00",
  "hasPostCreditScene": false,
  "confidence": 0.92,
  "detectionMethod": "Visual Pattern Analysis",
  "detectedAt": "2024-01-15T10:30:00Z"
}
```

### Generate Chapters

```http
POST /Enhanced/IntroDetection/{itemId}/generate-chapters
```

**Response:**
```json
[
  {
    "name": "Opening",
    "startPositionTicks": 0,
    "imageTag": "abc12345"
  },
  {
    "name": "Introduction",
    "startPositionTicks": 950000000,
    "imageTag": "def67890"
  }
]
```

## Recommendations

### Get Personalized Recommendations

```http
GET /Enhanced/Recommendations/{userId}
```

**Response:**
```json
[
  {
    "title": "Because You Watched",
    "items": [...],
    "type": "BecauseYouWatched"
  },
  {
    "title": "Top Picks For You",
    "items": [...],
    "type": "TopPicks"
  }
]
```

### Get Similar Items

```http
GET /Enhanced/Recommendations/similar/{itemId}?limit=10
```

**Response:**
```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "name": "Similar Movie",
    "type": "Movie",
    ...
  }
]
```

### Track User Interaction

```http
POST /Enhanced/Recommendations/interaction
Content-Type: application/json

{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "itemId": "660e8400-e29b-41d4-a716-446655440001",
  "type": "Watched"
}
```

**Interaction Types:**
- `Watched`
- `Started`
- `Liked`
- `Disliked`
- `AddedToWatchlist`

## Enhanced Transcoding

### Get Transcoding Presets

```http
GET /Enhanced/Transcoding/presets
```

**Response:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "High Quality",
    "description": "Best quality for high-end devices",
    "videoBitrate": 20000000,
    "audioBitrate": 320000,
    "videoCodec": "h264",
    "audioCodec": "aac",
    "profile": "high",
    "level": "4.1"
  }
]
```

### Analyze Device Capabilities

```http
GET /Enhanced/Transcoding/device/{deviceId}/capabilities
```

**Response:**
```json
{
  "deviceId": "device123",
  "supportsHEVC": true,
  "supportsAV1": false,
  "supportsHDR": true,
  "maxBitrate": 20000000,
  "maxWidth": 3840,
  "maxHeight": 2160,
  "preferredVideoCodec": "h264",
  "preferredAudioCodec": "aac",
  "hardwareAcceleration": "Auto",
  "lastAnalyzed": "2024-01-15T10:30:00Z"
}
```

## Error Responses

All endpoints follow standard HTTP status codes:

- `200 OK` - Success
- `400 Bad Request` - Invalid request
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

**Error Response Format:**
```json
{
  "success": false,
  "message": "Feature is disabled",
  "error": "FEATURE_DISABLED"
}
```

## Rate Limiting

Enhanced endpoints follow the same rate limiting as standard Jellyfin APIs. Default limits:
- 1000 requests per hour per API key
- 100 requests per minute per IP

## WebSocket Events

Haven extends Jellyfin's WebSocket API with additional event types:

- `DownloadProgress` - Real-time download updates
- `RecommendationUpdate` - New recommendations available
- `IntroDetected` - Intro/credit detection completed
- `TranscodingOptimized` - Transcoding profile optimized