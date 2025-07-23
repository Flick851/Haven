using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Haven.Common.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Haven.Common.Services
{
    /// <summary>
    /// Interface for download manager integration
    /// </summary>
    public interface IDownloadManagerService
    {
        Task<bool> TestConnectionAsync(ServiceConfig config);
        Task<List<DownloadItem>> GetActiveDownloadsAsync();
        Task<DownloadHistory> GetHistoryAsync(int limit = 50);
    }

    /// <summary>
    /// Base class for download manager services
    /// </summary>
    public abstract class BaseDownloadManagerService : IDownloadManagerService
    {
        protected readonly ILogger _logger;
        protected readonly HttpClient _httpClient;
        protected readonly HavenConfiguration _config;

        protected BaseDownloadManagerService(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            HavenConfiguration config)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
        }

        public abstract Task<bool> TestConnectionAsync(ServiceConfig config);
        public abstract Task<List<DownloadItem>> GetActiveDownloadsAsync();
        public abstract Task<DownloadHistory> GetHistoryAsync(int limit = 50);

        protected async Task<T?> GetApiResponseAsync<T>(string url, string apiKey) where T : class
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-Api-Key", apiKey);
                
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get API response from {Url}", url);
            }
            
            return null;
        }
    }

    /// <summary>
    /// Radarr integration service
    /// </summary>
    public class RadarrService : BaseDownloadManagerService
    {
        public RadarrService(
            ILogger<RadarrService> logger,
            IHttpClientFactory httpClientFactory,
            HavenConfiguration config) 
            : base(logger, httpClientFactory, config)
        {
        }

        public override async Task<bool> TestConnectionAsync(ServiceConfig config)
        {
            if (!config.Enabled || string.IsNullOrEmpty(config.Url))
                return false;

            try
            {
                var systemStatus = await GetApiResponseAsync<RadarrSystemStatus>(
                    $"{config.Url.TrimEnd('/')}/api/v3/system/status", 
                    config.ApiKey);
                
                return systemStatus != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test Radarr connection");
                return false;
            }
        }

        public override async Task<List<DownloadItem>> GetActiveDownloadsAsync()
        {
            var downloads = new List<DownloadItem>();
            var config = _config.DownloadManagers.Radarr;
            
            if (!config.Enabled)
                return downloads;

            try
            {
                var queue = await GetApiResponseAsync<RadarrQueue>(
                    $"{config.Url.TrimEnd('/')}/api/v3/queue",
                    config.ApiKey);

                if (queue?.Records != null)
                {
                    foreach (var item in queue.Records)
                    {
                        downloads.Add(new DownloadItem
                        {
                            Id = item.Id.ToString(),
                            Title = item.Title,
                            Progress = CalculateProgress(item.Size, item.Sizeleft),
                            Status = MapStatus(item.Status),
                            Eta = item.Timeleft,
                            Source = "Radarr",
                            Type = MediaType.Movie
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Radarr downloads");
            }

            return downloads;
        }

        public override async Task<DownloadHistory> GetHistoryAsync(int limit = 50)
        {
            var history = new DownloadHistory { Items = new List<HistoryItem>() };
            var config = _config.DownloadManagers.Radarr;
            
            if (!config.Enabled)
                return history;

            try
            {
                var radarrHistory = await GetApiResponseAsync<RadarrHistory>(
                    $"{config.Url.TrimEnd('/')}/api/v3/history?pageSize={limit}",
                    config.ApiKey);

                if (radarrHistory?.Records != null)
                {
                    foreach (var item in radarrHistory.Records)
                    {
                        history.Items.Add(new HistoryItem
                        {
                            Id = item.Id.ToString(),
                            Title = item.SourceTitle,
                            Date = item.Date,
                            EventType = item.EventType,
                            Source = "Radarr"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Radarr history");
            }

            return history;
        }

        private double CalculateProgress(long totalSize, long sizeLeft)
        {
            if (totalSize == 0) return 0;
            return ((totalSize - sizeLeft) / (double)totalSize) * 100;
        }

        private DownloadStatus MapStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "downloading" => DownloadStatus.Downloading,
                "paused" => DownloadStatus.Paused,
                "completed" => DownloadStatus.Completed,
                "failed" => DownloadStatus.Failed,
                "queued" => DownloadStatus.Queued,
                _ => DownloadStatus.Unknown
            };
        }
    }

    /// <summary>
    /// Sonarr integration service
    /// </summary>
    public class SonarrService : BaseDownloadManagerService
    {
        public SonarrService(
            ILogger<SonarrService> logger,
            IHttpClientFactory httpClientFactory,
            HavenConfiguration config) 
            : base(logger, httpClientFactory, config)
        {
        }

        public override async Task<bool> TestConnectionAsync(ServiceConfig config)
        {
            if (!config.Enabled || string.IsNullOrEmpty(config.Url))
                return false;

            try
            {
                var systemStatus = await GetApiResponseAsync<SonarrSystemStatus>(
                    $"{config.Url.TrimEnd('/')}/api/v3/system/status", 
                    config.ApiKey);
                
                return systemStatus != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test Sonarr connection");
                return false;
            }
        }

        public override async Task<List<DownloadItem>> GetActiveDownloadsAsync()
        {
            var downloads = new List<DownloadItem>();
            var config = _config.DownloadManagers.Sonarr;
            
            if (!config.Enabled)
                return downloads;

            try
            {
                var queue = await GetApiResponseAsync<SonarrQueue>(
                    $"{config.Url.TrimEnd('/')}/api/v3/queue",
                    config.ApiKey);

                if (queue?.Records != null)
                {
                    foreach (var item in queue.Records)
                    {
                        downloads.Add(new DownloadItem
                        {
                            Id = item.Id.ToString(),
                            Title = $"{item.Series.Title} - {item.Episode.Title}",
                            Progress = CalculateProgress(item.Size, item.Sizeleft),
                            Status = MapStatus(item.Status),
                            Eta = item.Timeleft,
                            Source = "Sonarr",
                            Type = MediaType.Episode
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Sonarr downloads");
            }

            return downloads;
        }

        public override async Task<DownloadHistory> GetHistoryAsync(int limit = 50)
        {
            var history = new DownloadHistory { Items = new List<HistoryItem>() };
            var config = _config.DownloadManagers.Sonarr;
            
            if (!config.Enabled)
                return history;

            try
            {
                var sonarrHistory = await GetApiResponseAsync<SonarrHistory>(
                    $"{config.Url.TrimEnd('/')}/api/v3/history?pageSize={limit}",
                    config.ApiKey);

                if (sonarrHistory?.Records != null)
                {
                    foreach (var item in sonarrHistory.Records)
                    {
                        history.Items.Add(new HistoryItem
                        {
                            Id = item.Id.ToString(),
                            Title = item.SourceTitle,
                            Date = item.Date,
                            EventType = item.EventType,
                            Source = "Sonarr"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Sonarr history");
            }

            return history;
        }

        private double CalculateProgress(long totalSize, long sizeLeft)
        {
            if (totalSize == 0) return 0;
            return ((totalSize - sizeLeft) / (double)totalSize) * 100;
        }

        private DownloadStatus MapStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "downloading" => DownloadStatus.Downloading,
                "paused" => DownloadStatus.Paused,
                "completed" => DownloadStatus.Completed,
                "failed" => DownloadStatus.Failed,
                "queued" => DownloadStatus.Queued,
                _ => DownloadStatus.Unknown
            };
        }
    }

    // DTOs for download tracking
    public class DownloadItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public double Progress { get; set; }
        public DownloadStatus Status { get; set; }
        public string Eta { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public MediaType Type { get; set; }
    }

    public class DownloadHistory
    {
        public List<HistoryItem> Items { get; set; } = new List<HistoryItem>();
    }

    public class HistoryItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    public enum DownloadStatus
    {
        Unknown,
        Queued,
        Downloading,
        Paused,
        Completed,
        Failed
    }

    public enum MediaType
    {
        Movie,
        Episode,
        Season,
        Series
    }

    // API response DTOs
    internal class RadarrSystemStatus
    {
        public string Version { get; set; } = string.Empty;
    }

    internal class RadarrQueue
    {
        public List<RadarrQueueRecord> Records { get; set; } = new List<RadarrQueueRecord>();
    }

    internal class RadarrQueueRecord
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public long Size { get; set; }
        public long Sizeleft { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Timeleft { get; set; } = string.Empty;
    }

    internal class RadarrHistory
    {
        public List<RadarrHistoryRecord> Records { get; set; } = new List<RadarrHistoryRecord>();
    }

    internal class RadarrHistoryRecord
    {
        public int Id { get; set; }
        public string SourceTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    internal class SonarrSystemStatus
    {
        public string Version { get; set; } = string.Empty;
    }

    internal class SonarrQueue
    {
        public List<SonarrQueueRecord> Records { get; set; } = new List<SonarrQueueRecord>();
    }

    internal class SonarrQueueRecord
    {
        public int Id { get; set; }
        public SonarrSeries Series { get; set; } = new SonarrSeries();
        public SonarrEpisode Episode { get; set; } = new SonarrEpisode();
        public long Size { get; set; }
        public long Sizeleft { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Timeleft { get; set; } = string.Empty;
    }

    internal class SonarrSeries
    {
        public string Title { get; set; } = string.Empty;
    }

    internal class SonarrEpisode
    {
        public string Title { get; set; } = string.Empty;
    }

    internal class SonarrHistory
    {
        public List<SonarrHistoryRecord> Records { get; set; } = new List<SonarrHistoryRecord>();
    }

    internal class SonarrHistoryRecord
    {
        public int Id { get; set; }
        public string SourceTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string EventType { get; set; } = string.Empty;
    }
}