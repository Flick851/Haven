# Haven - Enhanced Media Server

<p align="center">
<img alt="Haven Logo" src="https://raw.githubusercontent.com/Flick851/Haven/main/banner.png"/>
<br/>
<strong>The Enhanced Jellyfin Fork with Advanced Features</strong>
<br/>
<a href="https://github.com/Flick851/Haven">
<img alt="GitHub Stars" src="https://img.shields.io/github/stars/Flick851/Haven">
</a>
<a href="https://github.com/Flick851/Haven/releases">
<img alt="GitHub Release" src="https://img.shields.io/github/v/release/Flick851/Haven">
</a>
<a href="https://github.com/Flick851/Haven/blob/main/LICENSE">
<img alt="License" src="https://img.shields.io/github/license/Flick851/Haven">
</a>
</p>

Haven is an enhanced fork of Jellyfin that adds powerful features while maintaining complete compatibility with existing Jellyfin apps and clients.

## ✨ Key Features

### 🎭 Dual Branding System
- Reports as "Jellyfin Server" to maintain app compatibility
- Shows "Haven" branding internally for enhanced features

### 🎬 Netflix-Style UI Mode
- Modern hover previews
- Dynamic content rows
- Sleek, contemporary layouts

### 🔍 Smart Features
- **Intro/Credit Detection**: AI-powered skip markers
- **ML Recommendations**: Personalized content suggestions
- **Enhanced Transcoding**: Intelligent format selection
- **Download Tracking**: Real-time progress from your download managers

### 🔧 Download Manager Integration
Native integration with:
- Radarr
- Sonarr
- SABnzbd
- qBittorrent

### 🚀 Enhanced API
New `/Enhanced/*` endpoints for:
- Feature management
- Download progress tracking
- Compatibility mode toggling
- Advanced configurations

## 📦 Installation

### Docker (Recommended)
```bash
docker run -d \
  --name haven \
  -p 8096:8096 \
  -v /path/to/config:/config \
  -v /path/to/media:/media \
  flick851/haven:latest
```

### Manual Installation
1. Download the latest release from [Releases](https://github.com/Flick851/Haven/releases)
2. Extract to your preferred directory
3. Run `./havenfin` (Linux/Mac) or `havenfin.exe` (Windows)

## 🔧 Configuration

Haven stores its configuration in `config/haven.json`. All enhanced features can be toggled on/off:

```json
{
  "ApiCompatibilityMode": true,
  "NetflixMode": false,
  "IntroDetectionEnabled": true,
  "DownloadTrackingEnabled": true,
  "RecommendationsEnabled": true,
  "EnhancedTranscodingEnabled": true
}
```

## 🤝 Compatibility

Haven maintains 100% API compatibility with Jellyfin, meaning:
- ✅ All Jellyfin apps work without modification
- ✅ Existing plugins remain compatible
- ✅ Database migrations are seamless
- ✅ Configuration files are preserved

## 🛠️ Building from Source

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+ (for web client)
- Git

### Build Steps
```bash
# Clone the repository
git clone https://github.com/Flick851/Haven.git
cd Haven

# Build the server
dotnet build

# Run the server
dotnet run --project src/Haven.Server
```

## 📚 Documentation

- [Getting Started](docs/getting-started.md)
- [Configuration Guide](docs/configuration.md)
- [API Documentation](docs/api.md)
- [Plugin Development](docs/plugins.md)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License

Haven is licensed under the GPL-2.0 License, same as Jellyfin. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

Haven is built on top of the excellent [Jellyfin](https://github.com/jellyfin/jellyfin) project. We are grateful to the Jellyfin team and community for their work.

## ⚠️ Disclaimer

Haven is a community fork and is not officially affiliated with or endorsed by the Jellyfin project.