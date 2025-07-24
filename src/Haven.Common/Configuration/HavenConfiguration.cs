using System;
using System.Collections.Generic;

namespace Haven.Common.Configuration
{
    /// <summary>
    /// Enhanced configuration for Haven server features
    /// </summary>
    public class HavenConfiguration
    {
        /// <summary>
        /// Gets or sets whether API compatibility mode is enabled
        /// When enabled, server reports as "Jellyfin Server" to maintain app compatibility
        /// </summary>
        public bool ApiCompatibilityMode { get; set; } = true;

        /// <summary>
        /// Gets or sets whether Netflix-style UI mode is enabled
        /// </summary>
        public bool NetflixMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether intro detection is enabled
        /// </summary>
        public bool IntroDetectionEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether download tracking is enabled
        /// </summary>
        public bool DownloadTrackingEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the recommendations engine is enabled
        /// </summary>
        public bool RecommendationsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether enhanced transcoding is enabled
        /// </summary>
        public bool EnhancedTranscodingEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the internal branding name
        /// </summary>
        public string InternalBrandingName { get; set; } = "Haven";

        /// <summary>
        /// Gets or sets the external branding name (for API compatibility)
        /// </summary>
        public string ExternalBrandingName { get; set; } = "Jellyfin Server";

        /// <summary>
        /// Gets or sets the Haven version suffix
        /// </summary>
        public string VersionSuffix { get; set; } = "-haven.1.0";

        /// <summary>
        /// Gets or sets download manager configurations
        /// </summary>
        public DownloadManagerConfig DownloadManagers { get; set; } = new DownloadManagerConfig();

        /// <summary>
        /// Gets or sets whether to show Haven branding in logs and console
        /// </summary>
        public bool ShowHavenBranding { get; set; } = true;

        /// <summary>
        /// Gets the branding name based on context
        /// </summary>
        /// <param name="isApiRequest">Whether this is for an API request</param>
        /// <returns>The appropriate branding name</returns>
        public string GetBrandingName(bool isApiRequest = false)
        {
            if (isApiRequest && ApiCompatibilityMode)
            {
                return ExternalBrandingName;
            }
            return InternalBrandingName;
        }
    }

    /// <summary>
    /// Configuration for download manager integrations
    /// </summary>
    public class DownloadManagerConfig
    {
        /// <summary>
        /// Gets or sets Radarr configuration
        /// </summary>
        public ServiceConfig Radarr { get; set; } = new ServiceConfig();

        /// <summary>
        /// Gets or sets Sonarr configuration
        /// </summary>
        public ServiceConfig Sonarr { get; set; } = new ServiceConfig();

        /// <summary>
        /// Gets or sets SABnzbd configuration
        /// </summary>
        public ServiceConfig SABnzbd { get; set; } = new ServiceConfig();

        /// <summary>
        /// Gets or sets qBittorrent configuration
        /// </summary>
        public ServiceConfig qBittorrent { get; set; } = new ServiceConfig();
    }

    /// <summary>
    /// Configuration for individual download services
    /// </summary>
    public class ServiceConfig
    {
        /// <summary>
        /// Gets or sets whether this service is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the service URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the API key
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional service-specific settings
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; set; } = new Dictionary<string, string>();
    }
}