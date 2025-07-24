using System;
using System.Collections.Generic;
using MediaBrowser.Model.System;

namespace Haven.Common.Extensions
{
    /// <summary>
    /// Extended SystemInfo class for Haven-specific features
    /// </summary>
    public class HavenSystemInfo : PublicSystemInfo
    {
        /// <summary>
        /// Gets or sets custom properties for Haven features
        /// This allows adding enhanced features without breaking API compatibility
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a HavenSystemInfo from a standard PublicSystemInfo
        /// </summary>
        /// <param name="baseInfo">The base system info</param>
        /// <param name="havenConfig">Haven configuration</param>
        /// <returns>Extended system info with Haven features</returns>
        public static HavenSystemInfo FromPublicSystemInfo(PublicSystemInfo baseInfo, HavenConfiguration havenConfig)
        {
            var havenInfo = new HavenSystemInfo
            {
                LocalAddress = baseInfo.LocalAddress,
                ServerName = havenConfig.GetBrandingName(isApiRequest: true),
                Version = baseInfo.Version + havenConfig.VersionSuffix,
                ProductName = havenConfig.GetBrandingName(isApiRequest: true),
                OperatingSystem = baseInfo.OperatingSystem,
                Id = baseInfo.Id,
                StartupWizardCompleted = baseInfo.StartupWizardCompleted
            };

            // Add Haven-specific properties
            havenInfo.CustomProperties["HavenVersion"] = "1.0";
            havenInfo.CustomProperties["EnhancedFeaturesEnabled"] = true;
            havenInfo.CustomProperties["NetflixModeAvailable"] = havenConfig.NetflixMode;
            havenInfo.CustomProperties["IntroDetectionAvailable"] = havenConfig.IntroDetectionEnabled;
            havenInfo.CustomProperties["DownloadTrackingAvailable"] = havenConfig.DownloadTrackingEnabled;
            havenInfo.CustomProperties["RecommendationsAvailable"] = havenConfig.RecommendationsEnabled;
            havenInfo.CustomProperties["EnhancedTranscodingAvailable"] = havenConfig.EnhancedTranscodingEnabled;

            return havenInfo;
        }

        /// <summary>
        /// Converts back to standard PublicSystemInfo for API compatibility
        /// </summary>
        /// <returns>Standard PublicSystemInfo without custom properties</returns>
        public PublicSystemInfo ToPublicSystemInfo()
        {
            return new PublicSystemInfo
            {
                LocalAddress = this.LocalAddress,
                ServerName = this.ServerName,
                Version = this.Version,
                ProductName = this.ProductName,
                OperatingSystem = this.OperatingSystem,
                Id = this.Id,
                StartupWizardCompleted = this.StartupWizardCompleted
            };
        }
    }
}