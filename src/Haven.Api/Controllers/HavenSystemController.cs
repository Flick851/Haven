using System.Threading.Tasks;
using Haven.Api.Models;
using Haven.Common.Configuration;
using Haven.Common.Extensions;
using MediaBrowser.Common.Api;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Haven.Api.Controllers
{
    /// <summary>
    /// Override Jellyfin's system controller for dual branding
    /// </summary>
    [ApiController]
    [Route("System")]
    public class HavenSystemController : ControllerBase
    {
        private readonly ILogger<HavenSystemController> _logger;
        private readonly IServerApplicationHost _appHost;
        private readonly IApplicationPaths _appPaths;
        private readonly HavenConfiguration _havenConfig;

        public HavenSystemController(
            ILogger<HavenSystemController> logger,
            IServerApplicationHost appHost,
            IApplicationPaths appPaths,
            HavenConfiguration havenConfig)
        {
            _logger = logger;
            _appHost = appHost;
            _appPaths = appPaths;
            _havenConfig = havenConfig;
        }

        /// <summary>
        /// Gets public information about the server
        /// </summary>
        /// <response code="200">Server information returned.</response>
        /// <returns>A <see cref="PublicSystemInfo"/> with public info about the system.</returns>
        [HttpGet("Info/Public")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<PublicSystemInfo> GetPublicSystemInfo()
        {
            // Check if this is an API request that needs compatibility
            var isApiRequest = Request.Headers.ContainsKey("X-Emby-Client") ||
                             Request.Headers.ContainsKey("X-MediaBrowser-Client") ||
                             Request.Query.ContainsKey("api_key");

            _logger.LogDebug("System info requested. API Request: {IsApi}, Compatibility Mode: {Mode}",
                isApiRequest, _havenConfig.ApiCompatibilityMode);

            var baseInfo = new PublicSystemInfo
            {
                LocalAddress = _appHost.GetSmartApiUrl(Request),
                ServerName = _havenConfig.GetBrandingName(isApiRequest),
                Version = _appHost.ApplicationVersionString + _havenConfig.VersionSuffix,
                ProductName = _havenConfig.GetBrandingName(isApiRequest),
                OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                Id = _appHost.SystemId,
                StartupWizardCompleted = _appHost.GetStartupWizardCompleted()
            };

            // If not in compatibility mode or internal request, return Haven-branded info
            if (!isApiRequest || !_havenConfig.ApiCompatibilityMode)
            {
                var havenInfo = HavenSystemInfo.FromPublicSystemInfo(baseInfo, _havenConfig);
                return Ok(havenInfo);
            }

            // Return standard Jellyfin-branded info for app compatibility
            return Ok(baseInfo);
        }

        /// <summary>
        /// Gets Haven-specific system information
        /// </summary>
        [HttpGet("Info/Haven")]
        [Authorize(Policy = "DefaultAuthorization")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HavenSystemInfoResponse>> GetHavenSystemInfo()
        {
            var havenInfo = new HavenSystemInfoResponse
            {
                ServerName = _havenConfig.InternalBrandingName,
                Version = _appHost.ApplicationVersionString + _havenConfig.VersionSuffix,
                ProductName = _havenConfig.InternalBrandingName,
                OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                Id = _appHost.SystemId,
                StartupWizardCompleted = _appHost.GetStartupWizardCompleted(),
                HavenFeatures = new HavenFeatures
                {
                    HavenVersion = "1.0",
                    ApiCompatibilityMode = _havenConfig.ApiCompatibilityMode,
                    NetflixMode = _havenConfig.NetflixMode,
                    IntroDetection = _havenConfig.IntroDetectionEnabled,
                    DownloadTracking = _havenConfig.DownloadTrackingEnabled,
                    Recommendations = _havenConfig.RecommendationsEnabled,
                    EnhancedTranscoding = _havenConfig.EnhancedTranscodingEnabled
                }
            };

            // Add download manager status
            if (_havenConfig.DownloadTrackingEnabled)
            {
                // This would fetch real status from services
                havenInfo.HavenFeatures.DownloadManagers = new List<DownloadManagerStatus>
                {
                    new DownloadManagerStatus
                    {
                        Service = "Radarr",
                        Enabled = _havenConfig.DownloadManagers.Radarr.Enabled,
                        Connected = false
                    },
                    new DownloadManagerStatus
                    {
                        Service = "Sonarr",
                        Enabled = _havenConfig.DownloadManagers.Sonarr.Enabled,
                        Connected = false
                    }
                };
            }

            havenInfo.CustomProperties["EnhancedFeaturesActive"] = 
                _havenConfig.NetflixMode || 
                _havenConfig.IntroDetectionEnabled || 
                _havenConfig.DownloadTrackingEnabled ||
                _havenConfig.RecommendationsEnabled ||
                _havenConfig.EnhancedTranscodingEnabled;

            return Ok(havenInfo);
        }
    }
}