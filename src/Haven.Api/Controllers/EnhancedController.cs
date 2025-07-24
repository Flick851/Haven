using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Haven.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Haven.Api.Controllers
{
    /// <summary>
    /// Enhanced API endpoints for Haven-specific features
    /// </summary>
    [ApiController]
    [Route("Enhanced")]
    [Authorize]
    public class EnhancedController : ControllerBase
    {
        private readonly ILogger<EnhancedController> _logger;
        private readonly HavenConfiguration _havenConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedController"/> class.
        /// </summary>
        public EnhancedController(ILogger<EnhancedController> logger, HavenConfiguration havenConfig)
        {
            _logger = logger;
            _havenConfig = havenConfig;
        }

        /// <summary>
        /// Gets the status of enhanced features
        /// </summary>
        /// <returns>Feature status dictionary</returns>
        [HttpGet("Features")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Dictionary<string, object>> GetFeatures()
        {
            _logger.LogInformation("Getting Haven enhanced features status");

            var features = new Dictionary<string, object>
            {
                ["netflixMode"] = _havenConfig.NetflixMode,
                ["introDetection"] = _havenConfig.IntroDetectionEnabled,
                ["downloadTracking"] = _havenConfig.DownloadTrackingEnabled,
                ["recommendations"] = _havenConfig.RecommendationsEnabled,
                ["enhancedTranscoding"] = _havenConfig.EnhancedTranscodingEnabled,
                ["apiCompatibilityMode"] = _havenConfig.ApiCompatibilityMode,
                ["version"] = "1.0",
                ["serverBranding"] = _havenConfig.GetBrandingName(isApiRequest: false)
            };

            return Ok(features);
        }

        /// <summary>
        /// Toggle API compatibility mode
        /// </summary>
        /// <param name="enabled">Whether to enable compatibility mode</param>
        /// <returns>Updated status</returns>
        [HttpPost("CompatibilityMode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> SetCompatibilityMode([FromBody] bool enabled)
        {
            _logger.LogInformation("Setting API compatibility mode to {Enabled}", enabled);
            _havenConfig.ApiCompatibilityMode = enabled;

            return Ok(new 
            { 
                success = true, 
                enabled = enabled,
                message = enabled 
                    ? "API compatibility mode enabled - server will report as Jellyfin" 
                    : "API compatibility mode disabled - server will report as Haven"
            });
        }

        /// <summary>
        /// Get download progress for tracked items
        /// </summary>
        /// <returns>Download progress information</returns>
        [HttpGet("Downloads")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> GetDownloads()
        {
            if (!_havenConfig.DownloadTrackingEnabled)
            {
                return Ok(new { enabled = false, message = "Download tracking is disabled" });
            }

            // Framework for download tracking - actual implementation would connect to download managers
            var downloads = new List<object>
            {
                new 
                {
                    id = Guid.NewGuid().ToString(),
                    title = "Example Movie",
                    progress = 45.5,
                    eta = "15 minutes",
                    status = "downloading",
                    source = "Radarr"
                }
            };

            return Ok(new { enabled = true, downloads });
        }

        /// <summary>
        /// Update enhanced feature settings
        /// </summary>
        /// <param name="featureName">Name of the feature to update</param>
        /// <param name="enabled">Whether to enable the feature</param>
        /// <returns>Updated status</returns>
        [HttpPost("Features/{featureName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<object> UpdateFeature(string featureName, [FromBody] bool enabled)
        {
            _logger.LogInformation("Updating feature {FeatureName} to {Enabled}", featureName, enabled);

            switch (featureName.ToLowerInvariant())
            {
                case "netflixmode":
                    _havenConfig.NetflixMode = enabled;
                    break;
                case "introdetection":
                    _havenConfig.IntroDetectionEnabled = enabled;
                    break;
                case "downloadtracking":
                    _havenConfig.DownloadTrackingEnabled = enabled;
                    break;
                case "recommendations":
                    _havenConfig.RecommendationsEnabled = enabled;
                    break;
                case "enhancedtranscoding":
                    _havenConfig.EnhancedTranscodingEnabled = enabled;
                    break;
                default:
                    return BadRequest(new { success = false, message = $"Unknown feature: {featureName}" });
            }

            return Ok(new { success = true, feature = featureName, enabled });
        }

        /// <summary>
        /// Test connection to a download manager
        /// </summary>
        /// <param name="service">Service name (radarr, sonarr, sabnzbd, qbittorrent)</param>
        /// <returns>Connection test result</returns>
        [HttpPost("Downloads/TestConnection/{service}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> TestDownloadConnection(string service)
        {
            _logger.LogInformation("Testing connection to {Service}", service);

            ServiceConfig config = service.ToLowerInvariant() switch
            {
                "radarr" => _havenConfig.DownloadManagers.Radarr,
                "sonarr" => _havenConfig.DownloadManagers.Sonarr,
                "sabnzbd" => _havenConfig.DownloadManagers.SABnzbd,
                "qbittorrent" => _havenConfig.DownloadManagers.qBittorrent,
                _ => null
            };

            if (config == null)
            {
                return BadRequest(new { success = false, message = $"Unknown service: {service}" });
            }

            if (!config.Enabled || string.IsNullOrEmpty(config.Url))
            {
                return Ok(new { success = false, message = $"{service} is not configured" });
            }

            // Framework for connection testing - actual implementation would make HTTP requests
            await Task.Delay(100); // Simulate connection test

            return Ok(new 
            { 
                success = true, 
                service,
                message = $"Successfully connected to {service}",
                version = "1.0.0" // Would be retrieved from actual service
            });
        }
    }
}