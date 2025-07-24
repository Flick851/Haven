using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Haven.Common.Configuration;
using Haven.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Haven.Api.Controllers
{
    /// <summary>
    /// API endpoints for Netflix-style UI features
    /// </summary>
    [ApiController]
    [Route("Enhanced/NetflixMode")]
    [Authorize]
    public class NetflixModeController : ControllerBase
    {
        private readonly ILogger<NetflixModeController> _logger;
        private readonly INetflixModeService _netflixModeService;
        private readonly HavenConfiguration _config;

        public NetflixModeController(
            ILogger<NetflixModeController> logger,
            INetflixModeService netflixModeService,
            HavenConfiguration config)
        {
            _logger = logger;
            _netflixModeService = netflixModeService;
            _config = config;
        }

        /// <summary>
        /// Get Netflix mode configuration
        /// </summary>
        [HttpGet("config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<NetflixModeConfig>> GetConfiguration()
        {
            if (!_config.NetflixMode)
            {
                return Ok(new { enabled = false, message = "Netflix mode is disabled" });
            }

            var config = await _netflixModeService.GetConfigurationAsync();
            return Ok(config);
        }

        /// <summary>
        /// Update Netflix mode configuration
        /// </summary>
        [HttpPost("config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateConfiguration([FromBody] NetflixModeConfig config)
        {
            if (!_config.NetflixMode)
            {
                return BadRequest(new { success = false, message = "Netflix mode is disabled" });
            }

            var success = await _netflixModeService.UpdateConfigurationAsync(config);
            return Ok(new { success });
        }

        /// <summary>
        /// Get dynamic content rows for a user
        /// </summary>
        [HttpGet("rows/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DynamicRow>>> GetDynamicRows(Guid userId)
        {
            if (!_config.NetflixMode)
            {
                return Ok(new List<DynamicRow>());
            }

            var rows = await _netflixModeService.GetDynamicRowsAsync(userId);
            return Ok(rows);
        }

        /// <summary>
        /// Get hover preview data for an item
        /// </summary>
        [HttpGet("preview/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<HoverPreviewData>> GetHoverPreview(Guid itemId)
        {
            if (!_config.NetflixMode)
            {
                return NotFound();
            }

            var preview = await _netflixModeService.GetHoverPreviewAsync(itemId);
            return Ok(preview);
        }
    }

    /// <summary>
    /// API endpoints for intro detection features
    /// </summary>
    [ApiController]
    [Route("Enhanced/IntroDetection")]
    [Authorize]
    public class IntroDetectionController : ControllerBase
    {
        private readonly ILogger<IntroDetectionController> _logger;
        private readonly IIntroDetectionService _introDetectionService;
        private readonly HavenConfiguration _config;

        public IntroDetectionController(
            ILogger<IntroDetectionController> logger,
            IIntroDetectionService introDetectionService,
            HavenConfiguration config)
        {
            _logger = logger;
            _introDetectionService = introDetectionService;
            _config = config;
        }

        /// <summary>
        /// Get intro markers for an item
        /// </summary>
        [HttpGet("{itemId}/intro")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IntroDetectionResult>> GetIntroMarkers(Guid itemId)
        {
            if (!_config.IntroDetectionEnabled)
            {
                return NotFound();
            }

            var result = await _introDetectionService.DetectIntroAsync(itemId);
            return Ok(result);
        }

        /// <summary>
        /// Get credit markers for an item
        /// </summary>
        [HttpGet("{itemId}/credits")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CreditDetectionResult>> GetCreditMarkers(Guid itemId)
        {
            if (!_config.IntroDetectionEnabled)
            {
                return NotFound();
            }

            var result = await _introDetectionService.DetectCreditsAsync(itemId);
            return Ok(result);
        }

        /// <summary>
        /// Generate chapters for an item
        /// </summary>
        [HttpPost("{itemId}/generate-chapters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ChapterInfo>>> GenerateChapters(Guid itemId)
        {
            if (!_config.IntroDetectionEnabled)
            {
                return BadRequest(new { success = false, message = "Intro detection is disabled" });
            }

            var chapters = await _introDetectionService.GenerateChaptersAsync(itemId);
            return Ok(chapters);
        }

        /// <summary>
        /// Manually save detection results
        /// </summary>
        [HttpPost("{itemId}/save")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SaveDetectionResults(
            Guid itemId,
            [FromBody] DetectionResultsDto results)
        {
            if (!_config.IntroDetectionEnabled)
            {
                return BadRequest(new { success = false, message = "Intro detection is disabled" });
            }

            await _introDetectionService.SaveDetectionResultAsync(
                itemId,
                results.IntroResult,
                results.CreditResult);

            return Ok(new { success = true });
        }
    }

    /// <summary>
    /// API endpoints for recommendations
    /// </summary>
    [ApiController]
    [Route("Enhanced/Recommendations")]
    [Authorize]
    public class RecommendationsController : ControllerBase
    {
        private readonly ILogger<RecommendationsController> _logger;
        private readonly IRecommendationEngine _recommendationEngine;
        private readonly HavenConfiguration _config;

        public RecommendationsController(
            ILogger<RecommendationsController> logger,
            IRecommendationEngine recommendationEngine,
            HavenConfiguration config)
        {
            _logger = logger;
            _recommendationEngine = recommendationEngine;
            _config = config;
        }

        /// <summary>
        /// Get personalized recommendations for a user
        /// </summary>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RecommendationGroup>>> GetRecommendations(Guid userId)
        {
            if (!_config.RecommendationsEnabled)
            {
                return Ok(new List<RecommendationGroup>());
            }

            var recommendations = await _recommendationEngine.GetPersonalizedRecommendationsAsync(userId);
            return Ok(recommendations);
        }

        /// <summary>
        /// Get similar items
        /// </summary>
        [HttpGet("similar/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MediaBrowser.Model.Dto.BaseItemDto>>> GetSimilarItems(
            Guid itemId,
            [FromQuery] int limit = 10)
        {
            if (!_config.RecommendationsEnabled)
            {
                return Ok(new List<MediaBrowser.Model.Dto.BaseItemDto>());
            }

            var items = await _recommendationEngine.GetSimilarItemsAsync(itemId, limit);
            return Ok(items);
        }

        /// <summary>
        /// Get trending items
        /// </summary>
        [HttpGet("trending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MediaBrowser.Model.Dto.BaseItemDto>>> GetTrending(
            [FromQuery] int limit = 20)
        {
            if (!_config.RecommendationsEnabled)
            {
                return Ok(new List<MediaBrowser.Model.Dto.BaseItemDto>());
            }

            var items = await _recommendationEngine.GetTrendingItemsAsync(limit);
            return Ok(items);
        }

        /// <summary>
        /// Track user interaction
        /// </summary>
        [HttpPost("interaction")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> TrackInteraction([FromBody] InteractionDto interaction)
        {
            if (!_config.RecommendationsEnabled)
            {
                return Ok(new { success = false, message = "Recommendations are disabled" });
            }

            await _recommendationEngine.UpdateUserInteractionAsync(
                interaction.UserId,
                interaction.ItemId,
                interaction.Type);

            return Ok(new { success = true });
        }
    }

    /// <summary>
    /// API endpoints for enhanced transcoding
    /// </summary>
    [ApiController]
    [Route("Enhanced/Transcoding")]
    [Authorize]
    public class TranscodingController : ControllerBase
    {
        private readonly ILogger<TranscodingController> _logger;
        private readonly IEnhancedTranscodingService _transcodingService;
        private readonly HavenConfiguration _config;

        public TranscodingController(
            ILogger<TranscodingController> logger,
            IEnhancedTranscodingService transcodingService,
            HavenConfiguration config)
        {
            _logger = logger;
            _transcodingService = transcodingService;
            _config = config;
        }

        /// <summary>
        /// Get transcoding presets
        /// </summary>
        [HttpGet("presets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TranscodingPreset>>> GetPresets()
        {
            if (!_config.EnhancedTranscodingEnabled)
            {
                return Ok(new List<TranscodingPreset>());
            }

            var presets = await _transcodingService.GetPresetsAsync();
            return Ok(presets);
        }

        /// <summary>
        /// Update a transcoding preset
        /// </summary>
        [HttpPost("presets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdatePreset([FromBody] TranscodingPreset preset)
        {
            if (!_config.EnhancedTranscodingEnabled)
            {
                return BadRequest(new { success = false, message = "Enhanced transcoding is disabled" });
            }

            var success = await _transcodingService.UpdatePresetAsync(preset);
            return Ok(new { success });
        }

        /// <summary>
        /// Analyze device capabilities
        /// </summary>
        [HttpGet("device/{deviceId}/capabilities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DeviceOptimization>> GetDeviceCapabilities(string deviceId)
        {
            if (!_config.EnhancedTranscodingEnabled)
            {
                return NotFound();
            }

            var capabilities = await _transcodingService.AnalyzeDeviceCapabilitiesAsync(deviceId);
            return Ok(capabilities);
        }
    }

    // DTOs
    public class DetectionResultsDto
    {
        public IntroDetectionResult? IntroResult { get; set; }
        public CreditDetectionResult? CreditResult { get; set; }
    }

    public class InteractionDto
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public InteractionType Type { get; set; }
    }
}