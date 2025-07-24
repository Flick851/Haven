using System;
using System.Collections.Generic;
using Haven.Common.Configuration;

namespace Haven.Api.Models
{
    /// <summary>
    /// Request model for updating download manager configuration
    /// </summary>
    public class UpdateDownloadManagerRequest
    {
        public string Service { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public Dictionary<string, string> AdditionalSettings { get; set; } = new();
    }

    /// <summary>
    /// Response model for download manager status
    /// </summary>
    public class DownloadManagerStatus
    {
        public string Service { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool Connected { get; set; }
        public string Version { get; set; } = string.Empty;
        public int ActiveDownloads { get; set; }
        public int QueuedDownloads { get; set; }
        public DateTime? LastSync { get; set; }
    }

    /// <summary>
    /// Request model for manual download addition
    /// </summary>
    public class AddDownloadRequest
    {
        public string Service { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // movie, series
        public string Title { get; set; } = string.Empty;
        public string ImdbId { get; set; } = string.Empty;
        public string TmdbId { get; set; } = string.Empty;
        public string TvdbId { get; set; } = string.Empty;
        public int? Year { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
    }

    /// <summary>
    /// Response model for enhanced system info
    /// </summary>
    public class HavenSystemInfoResponse
    {
        public string ServerName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public bool StartupWizardCompleted { get; set; }
        public HavenFeatures HavenFeatures { get; set; } = new();
        public Dictionary<string, object> CustomProperties { get; set; } = new();
    }

    /// <summary>
    /// Haven-specific features status
    /// </summary>
    public class HavenFeatures
    {
        public string HavenVersion { get; set; } = "1.0";
        public bool ApiCompatibilityMode { get; set; }
        public bool NetflixMode { get; set; }
        public bool IntroDetection { get; set; }
        public bool DownloadTracking { get; set; }
        public bool Recommendations { get; set; }
        public bool EnhancedTranscoding { get; set; }
        public List<DownloadManagerStatus> DownloadManagers { get; set; } = new();
    }

    /// <summary>
    /// WebSocket notification models
    /// </summary>
    public class HavenWebSocketMessage
    {
        public string MessageType { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DownloadProgressNotification
    {
        public string DownloadId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Eta { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    public class RecommendationUpdateNotification
    {
        public Guid UserId { get; set; }
        public string UpdateType { get; set; } = string.Empty;
        public int NewItemsCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class IntroDetectionCompleteNotification
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public bool IntroDetected { get; set; }
        public bool CreditsDetected { get; set; }
        public TimeSpan? IntroStart { get; set; }
        public TimeSpan? IntroEnd { get; set; }
        public TimeSpan? CreditsStart { get; set; }
    }
}