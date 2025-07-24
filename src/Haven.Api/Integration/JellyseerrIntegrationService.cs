using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Haven.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Haven.Api.Integration
{
    /// <summary>
    /// Service for integrating with Jellyseerr request management
    /// </summary>
    public interface IJellyseerrIntegrationService
    {
        Task<bool> TestConnectionAsync(string url, string apiKey);
        Task<JellyseerrStatus> GetStatusAsync();
        Task<List<MediaRequest>> GetPendingRequestsAsync();
        Task<bool> ApproveRequestAsync(int requestId);
        Task<bool> DeclineRequestAsync(int requestId);
        Task<bool> CreateWebhookAsync(string webhookUrl);
    }

    /// <summary>
    /// Jellyseerr integration service implementation
    /// </summary>
    public class JellyseerrIntegrationService : IJellyseerrIntegrationService
    {
        private readonly ILogger<JellyseerrIntegrationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly HavenConfiguration _config;

        public JellyseerrIntegrationService(
            ILogger<JellyseerrIntegrationService> logger,
            IHttpClientFactory httpClientFactory,
            HavenConfiguration config)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
        }

        public async Task<bool> TestConnectionAsync(string url, string apiKey)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/v1/status");
                request.Headers.Add("X-Api-Key", apiKey);
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test Jellyseerr connection");
                return false;
            }
        }

        public async Task<JellyseerrStatus> GetStatusAsync()
        {
            // Implementation would connect to configured Jellyseerr instance
            return new JellyseerrStatus
            {
                Version = "1.0.0",
                TotalRequests = 0,
                PendingRequests = 0,
                ProcessingRequests = 0
            };
        }

        public async Task<List<MediaRequest>> GetPendingRequestsAsync()
        {
            var requests = new List<MediaRequest>();
            
            // Implementation would fetch from Jellyseerr API
            // GET /api/v1/request?filter=pending
            
            return requests;
        }

        public async Task<bool> ApproveRequestAsync(int requestId)
        {
            try
            {
                // POST /api/v1/request/{requestId}/approve
                var url = $"{GetJellyseerrUrl()}/api/v1/request/{requestId}/approve";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-Api-Key", GetApiKey());
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<bool> DeclineRequestAsync(int requestId)
        {
            try
            {
                // POST /api/v1/request/{requestId}/decline
                var url = $"{GetJellyseerrUrl()}/api/v1/request/{requestId}/decline";
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("X-Api-Key", GetApiKey());
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decline request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<bool> CreateWebhookAsync(string webhookUrl)
        {
            try
            {
                // Configure webhook for Haven notifications
                var payload = new
                {
                    enabled = true,
                    types = new[] { "MEDIA_PENDING", "MEDIA_APPROVED", "MEDIA_AVAILABLE" },
                    options = new
                    {
                        webhookUrl = webhookUrl,
                        jsonPayload = JsonSerializer.Serialize(new
                        {
                            text = "{{event}}",
                            media = "{{media}}",
                            request = "{{request}}"
                        })
                    }
                };

                var url = $"{GetJellyseerrUrl()}/api/v1/settings/notifications/webhook";
                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("X-Api-Key", GetApiKey());
                
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create webhook");
                return false;
            }
        }

        private string GetJellyseerrUrl()
        {
            // Would be stored in Haven configuration
            return _config.DownloadManagers.AdditionalSettings.GetValueOrDefault("JellyseerrUrl", "");
        }

        private string GetApiKey()
        {
            // Would be stored in Haven configuration
            return _config.DownloadManagers.AdditionalSettings.GetValueOrDefault("JellyseerrApiKey", "");
        }
    }

    // Data models
    public class JellyseerrStatus
    {
        public string Version { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ProcessingRequests { get; set; }
    }

    public class MediaRequest
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // movie, tv
        public string Status { get; set; } = string.Empty; // pending, approved, processing, available
        public MediaInfo Media { get; set; } = new();
        public RequestedBy RequestedBy { get; set; } = new();
        public DateTime RequestedDate { get; set; }
    }

    public class MediaInfo
    {
        public string Title { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string ImdbId { get; set; } = string.Empty;
        public string TmdbId { get; set; } = string.Empty;
    }

    public class RequestedBy
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}