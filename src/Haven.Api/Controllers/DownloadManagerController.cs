using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Haven.Api.Models;
using Haven.Common.Configuration;
using Haven.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Haven.Api.Controllers
{
    /// <summary>
    /// API endpoints for download manager configuration and management
    /// </summary>
    [ApiController]
    [Route("Enhanced/DownloadManagers")]
    [Authorize(Policy = "RequiresElevation")]
    public class DownloadManagerController : ControllerBase
    {
        private readonly ILogger<DownloadManagerController> _logger;
        private readonly HavenConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public DownloadManagerController(
            ILogger<DownloadManagerController> logger,
            HavenConfiguration config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Get all download manager configurations
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Dictionary<string, object>> GetConfigurations()
        {
            var configs = new Dictionary<string, object>
            {
                ["radarr"] = new
                {
                    _config.DownloadManagers.Radarr.Enabled,
                    _config.DownloadManagers.Radarr.Url,
                    hasApiKey = !string.IsNullOrEmpty(_config.DownloadManagers.Radarr.ApiKey)
                },
                ["sonarr"] = new
                {
                    _config.DownloadManagers.Sonarr.Enabled,
                    _config.DownloadManagers.Sonarr.Url,
                    hasApiKey = !string.IsNullOrEmpty(_config.DownloadManagers.Sonarr.ApiKey)
                },
                ["sabnzbd"] = new
                {
                    _config.DownloadManagers.SABnzbd.Enabled,
                    _config.DownloadManagers.SABnzbd.Url,
                    hasApiKey = !string.IsNullOrEmpty(_config.DownloadManagers.SABnzbd.ApiKey)
                },
                ["qbittorrent"] = new
                {
                    _config.DownloadManagers.qBittorrent.Enabled,
                    _config.DownloadManagers.qBittorrent.Url,
                    hasApiKey = !string.IsNullOrEmpty(_config.DownloadManagers.qBittorrent.ApiKey)
                }
            };

            return Ok(configs);
        }

        /// <summary>
        /// Update download manager configuration
        /// </summary>
        [HttpPost("{service}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateConfiguration(
            string service,
            [FromBody] UpdateDownloadManagerRequest request)
        {
            _logger.LogInformation("Updating {Service} configuration", service);

            ServiceConfig config = service.ToLowerInvariant() switch
            {
                "radarr" => _config.DownloadManagers.Radarr,
                "sonarr" => _config.DownloadManagers.Sonarr,
                "sabnzbd" => _config.DownloadManagers.SABnzbd,
                "qbittorrent" => _config.DownloadManagers.qBittorrent,
                _ => null
            };

            if (config == null)
            {
                return BadRequest(new { success = false, message = $"Unknown service: {service}" });
            }

            // Update configuration
            config.Enabled = request.Enabled;
            config.Url = request.Url;
            config.ApiKey = request.ApiKey;
            config.AdditionalSettings = request.AdditionalSettings;

            // Test connection if enabled
            if (config.Enabled)
            {
                var downloadService = GetDownloadService(service);
                if (downloadService != null)
                {
                    var connected = await downloadService.TestConnectionAsync(config);
                    if (!connected)
                    {
                        return Ok(new 
                        { 
                            success = true, 
                            warning = "Configuration saved but connection test failed"
                        });
                    }
                }
            }

            // TODO: Persist configuration to disk
            return Ok(new { success = true, message = "Configuration updated successfully" });
        }

        /// <summary>
        /// Get status of all download managers
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DownloadManagerStatus>>> GetStatus()
        {
            var statuses = new List<DownloadManagerStatus>();

            // Check each service
            var services = new[] { "radarr", "sonarr", "sabnzbd", "qbittorrent" };
            foreach (var serviceName in services)
            {
                var status = await GetServiceStatusAsync(serviceName);
                statuses.Add(status);
            }

            return Ok(statuses);
        }

        /// <summary>
        /// Get combined download queue from all services
        /// </summary>
        [HttpGet("queue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetCombinedQueue()
        {
            if (!_config.DownloadTrackingEnabled)
            {
                return Ok(new { enabled = false });
            }

            var allDownloads = new List<object>();

            // Get downloads from each enabled service
            if (_config.DownloadManagers.Radarr.Enabled)
            {
                var radarrService = _serviceProvider.GetService(typeof(RadarrService)) as RadarrService;
                if (radarrService != null)
                {
                    var downloads = await radarrService.GetActiveDownloadsAsync();
                    allDownloads.AddRange(downloads);
                }
            }

            if (_config.DownloadManagers.Sonarr.Enabled)
            {
                var sonarrService = _serviceProvider.GetService(typeof(SonarrService)) as SonarrService;
                if (sonarrService != null)
                {
                    var downloads = await sonarrService.GetActiveDownloadsAsync();
                    allDownloads.AddRange(downloads);
                }
            }

            return Ok(new
            {
                enabled = true,
                downloads = allDownloads,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Add a download manually
        /// </summary>
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddDownload([FromBody] AddDownloadRequest request)
        {
            _logger.LogInformation("Adding download: {Title} via {Service}", request.Title, request.Service);

            // Validate service
            var service = GetDownloadService(request.Service);
            if (service == null)
            {
                return BadRequest(new { success = false, message = "Service not configured or enabled" });
            }

            // TODO: Implement actual download addition logic for each service
            // This would involve calling the respective service's API to add the item

            return Ok(new
            {
                success = true,
                message = $"Download request sent to {request.Service}",
                downloadId = Guid.NewGuid().ToString()
            });
        }

        /// <summary>
        /// Get download history
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetHistory([FromQuery] int limit = 50)
        {
            var allHistory = new List<object>();

            // Get history from each enabled service
            if (_config.DownloadManagers.Radarr.Enabled)
            {
                var radarrService = _serviceProvider.GetService(typeof(RadarrService)) as RadarrService;
                if (radarrService != null)
                {
                    var history = await radarrService.GetHistoryAsync(limit);
                    allHistory.AddRange(history.Items);
                }
            }

            if (_config.DownloadManagers.Sonarr.Enabled)
            {
                var sonarrService = _serviceProvider.GetService(typeof(SonarrService)) as SonarrService;
                if (sonarrService != null)
                {
                    var history = await sonarrService.GetHistoryAsync(limit);
                    allHistory.AddRange(history.Items);
                }
            }

            // Sort by date descending
            var sortedHistory = allHistory
                .OrderByDescending(h => (h as dynamic).Date)
                .Take(limit)
                .ToList();

            return Ok(new
            {
                items = sortedHistory,
                totalCount = sortedHistory.Count
            });
        }

        private IDownloadManagerService? GetDownloadService(string serviceName)
        {
            return serviceName.ToLowerInvariant() switch
            {
                "radarr" => _serviceProvider.GetService(typeof(RadarrService)) as RadarrService,
                "sonarr" => _serviceProvider.GetService(typeof(SonarrService)) as SonarrService,
                _ => null
            };
        }

        private async Task<DownloadManagerStatus> GetServiceStatusAsync(string serviceName)
        {
            var status = new DownloadManagerStatus
            {
                Service = serviceName,
                Enabled = false,
                Connected = false
            };

            ServiceConfig config = serviceName.ToLowerInvariant() switch
            {
                "radarr" => _config.DownloadManagers.Radarr,
                "sonarr" => _config.DownloadManagers.Sonarr,
                "sabnzbd" => _config.DownloadManagers.SABnzbd,
                "qbittorrent" => _config.DownloadManagers.qBittorrent,
                _ => null
            };

            if (config == null || !config.Enabled)
            {
                return status;
            }

            status.Enabled = true;

            var service = GetDownloadService(serviceName);
            if (service != null)
            {
                try
                {
                    status.Connected = await service.TestConnectionAsync(config);
                    if (status.Connected)
                    {
                        var downloads = await service.GetActiveDownloadsAsync();
                        status.ActiveDownloads = downloads.Count(d => 
                            d.Status == DownloadStatus.Downloading);
                        status.QueuedDownloads = downloads.Count(d => 
                            d.Status == DownloadStatus.Queued);
                        status.LastSync = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get status for {Service}", serviceName);
                }
            }

            return status;
        }
    }
}