# Getting Started with Haven

Welcome to Haven, the enhanced Jellyfin fork with advanced features! This guide will help you get up and running quickly.

## Prerequisites

- A computer or server capable of running Jellyfin
- At least 2GB RAM (4GB+ recommended)
- Storage space for your media library
- (Optional) Docker and Docker Compose

## Installation Methods

### Method 1: Docker (Recommended)

1. **Install Docker and Docker Compose** on your system

2. **Create a docker-compose.yml file**:
```yaml
version: "3.8"

services:
  haven:
    image: flick851/haven:latest
    container_name: haven
    restart: unless-stopped
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=America/New_York
    volumes:
      - ./config:/config
      - ./cache:/cache
      - /path/to/your/media:/media
    ports:
      - "8096:8096"
```

3. **Start Haven**:
```bash
docker-compose up -d
```

4. **Access Haven** at `http://your-server-ip:8096`

### Method 2: Manual Installation

1. **Download the latest release** from [GitHub Releases](https://github.com/Flick851/Haven/releases)

2. **Extract the archive** to your preferred directory

3. **Run Haven**:
   - Linux/Mac: `./havenfin`
   - Windows: `havenfin.exe`

4. **Access Haven** at `http://localhost:8096`

## Initial Setup

### Step 1: Welcome Screen

1. Select your preferred language
2. Click "Next"

### Step 2: Create Admin Account

1. Choose a username for your admin account
2. Set a strong password
3. Click "Next"

### Step 3: Configure Media Libraries

1. Click "Add Media Library"
2. Select the content type (Movies, TV Shows, Music, etc.)
3. Add folders containing your media
4. Configure library settings:
   - Enable metadata downloading
   - Select metadata providers
   - Set preferred language
5. Click "Save"

### Step 4: Enable Haven Features

1. Navigate to **Dashboard → Advanced → Haven Settings**
2. Enable desired features:
   - **Netflix Mode**: Modern UI with hover previews
   - **Intro Detection**: Automatic skip markers
   - **Download Tracking**: Monitor download progress
   - **ML Recommendations**: Personalized suggestions
   - **Enhanced Transcoding**: Smart format selection

### Step 5: Configure Download Managers (Optional)

If you use Radarr, Sonarr, or other download managers:

1. Go to **Haven Settings → Download Managers**
2. Enable your download manager
3. Enter the URL and API key
4. Test the connection
5. Save settings

## First Time Usage

### Adding Media

Haven will automatically scan your media libraries and download metadata. This process may take some time depending on your library size.

### Accessing Your Server

- **Web Browser**: Navigate to `http://your-server-ip:8096`
- **Mobile Apps**: Use any Jellyfin app and enter your server address
- **TV Apps**: Install Jellyfin app and add your server

### User Management

1. Go to **Dashboard → Users**
2. Click "Add User"
3. Configure user permissions:
   - Library access
   - Remote access
   - Live TV access
   - Admin privileges

## Enabling Enhanced Features

### Netflix Mode

1. Enable in Haven Settings
2. Refresh your browser
3. Hover over items to see previews
4. Enjoy dynamic content rows

### Intro Detection

1. Enable in Haven Settings
2. Play any TV show or movie
3. Skip intro/credits buttons will appear automatically
4. Manual adjustment available in playback settings

### Smart Recommendations

1. Enable ML Recommendations
2. Watch a few items to train the engine
3. Check your home screen for personalized rows:
   - "Because You Watched"
   - "Top Picks For You"
   - "Trending Now"

### Download Tracking

1. Configure download managers
2. Enable Download Tracking
3. View progress in:
   - Dashboard widget
   - Mobile notifications
   - Web interface sidebar

## App Compatibility

Haven maintains 100% compatibility with Jellyfin apps:

### Official Apps
- Jellyfin Web
- Jellyfin for Android
- Jellyfin for iOS
- Jellyfin for Android TV
- Jellyfin for Roku
- Jellyfin for Kodi

### Third-Party Apps
- Infuse
- Streamyfin
- Findroid
- JellyCon
- Gelli

## Performance Optimization

### Hardware Acceleration

1. Go to **Dashboard → Playback**
2. Enable hardware acceleration
3. Select your GPU type:
   - Intel Quick Sync
   - NVIDIA NVENC
   - AMD AMF
   - VAAPI (Linux)

### Network Settings

1. Go to **Dashboard → Networking**
2. Configure:
   - LAN networks (for direct play)
   - Remote access settings
   - Port forwarding (if needed)

## Troubleshooting

### Can't access server

1. Check firewall settings
2. Verify port 8096 is open
3. Try accessing via IP address
4. Check Docker logs: `docker logs haven`

### Apps won't connect

1. Ensure "API Compatibility Mode" is enabled
2. Check server address format
3. Verify user has remote access permission

### Playback issues

1. Check transcoding settings
2. Verify ffmpeg is installed
3. Enable hardware acceleration if available
4. Check network bandwidth

### Features not showing

1. Hard refresh browser (Ctrl+F5)
2. Clear browser cache
3. Restart Haven server
4. Check feature is enabled in settings

## Getting Help

- **Documentation**: [https://github.com/Flick851/Haven/docs](https://github.com/Flick851/Haven/docs)
- **Issues**: [https://github.com/Flick851/Haven/issues](https://github.com/Flick851/Haven/issues)
- **Discord**: Join our community server
- **Reddit**: r/HavenMediaServer

## Next Steps

- Explore the [Configuration Guide](configuration.md) for advanced settings
- Read the [API Documentation](api.md) for integration options
- Check out recommended [companion apps and tools](tools.md)
- Join our community for tips and support

Welcome to Haven - enjoy your enhanced media experience!