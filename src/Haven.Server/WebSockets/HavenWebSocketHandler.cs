using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Haven.Api.Models;
using Haven.Common.Configuration;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Net;
using Microsoft.Extensions.Logging;

namespace Haven.Server.WebSockets
{
    /// <summary>
    /// WebSocket handler for Haven-specific real-time features
    /// </summary>
    public class HavenWebSocketHandler : IWebSocketListener
    {
        private readonly ILogger<HavenWebSocketHandler> _logger;
        private readonly HavenConfiguration _config;
        private readonly Dictionary<Guid, IWebSocketConnection> _connections;

        public HavenWebSocketHandler(
            ILogger<HavenWebSocketHandler> logger,
            HavenConfiguration config)
        {
            _logger = logger;
            _config = config;
            _connections = new Dictionary<Guid, IWebSocketConnection>();
        }

        /// <summary>
        /// Process incoming WebSocket messages
        /// </summary>
        public async Task ProcessMessageAsync(WebSocketMessageInfo message)
        {
            if (message.MessageType == "HavenSubscribe")
            {
                await HandleSubscription(message);
            }
            else if (message.MessageType == "HavenUnsubscribe")
            {
                await HandleUnsubscription(message);
            }
        }

        /// <summary>
        /// Send download progress update to subscribed clients
        /// </summary>
        public async Task SendDownloadProgressAsync(DownloadProgressNotification notification)
        {
            if (!_config.DownloadTrackingEnabled)
                return;

            var message = new HavenWebSocketMessage
            {
                MessageType = "DownloadProgress",
                Data = JsonSerializer.Serialize(notification)
            };

            await BroadcastToSubscribedClientsAsync("downloads", message);
        }

        /// <summary>
        /// Send recommendation update notification
        /// </summary>
        public async Task SendRecommendationUpdateAsync(RecommendationUpdateNotification notification)
        {
            if (!_config.RecommendationsEnabled)
                return;

            var message = new HavenWebSocketMessage
            {
                MessageType = "RecommendationUpdate",
                Data = JsonSerializer.Serialize(notification)
            };

            // Send to specific user
            await SendToUserAsync(notification.UserId, message);
        }

        /// <summary>
        /// Send intro detection complete notification
        /// </summary>
        public async Task SendIntroDetectionCompleteAsync(IntroDetectionCompleteNotification notification)
        {
            if (!_config.IntroDetectionEnabled)
                return;

            var message = new HavenWebSocketMessage
            {
                MessageType = "IntroDetectionComplete",
                Data = JsonSerializer.Serialize(notification)
            };

            await BroadcastToSubscribedClientsAsync("introdetection", message);
        }

        /// <summary>
        /// Send transcoding optimization notification
        /// </summary>
        public async Task SendTranscodingOptimizedAsync(string deviceId, string message)
        {
            if (!_config.EnhancedTranscodingEnabled)
                return;

            var wsMessage = new HavenWebSocketMessage
            {
                MessageType = "TranscodingOptimized",
                Data = JsonSerializer.Serialize(new { deviceId, message })
            };

            await BroadcastToSubscribedClientsAsync("transcoding", wsMessage);
        }

        private async Task HandleSubscription(WebSocketMessageInfo message)
        {
            var subscriptionType = message.Data?.ToString();
            if (string.IsNullOrEmpty(subscriptionType))
                return;

            _logger.LogInformation("WebSocket client subscribing to {Type}", subscriptionType);

            // Track subscription
            if (!_connections.ContainsKey(message.Connection.Id))
            {
                _connections[message.Connection.Id] = message.Connection;
            }

            // Send initial data based on subscription type
            switch (subscriptionType.ToLowerInvariant())
            {
                case "downloads":
                    await SendInitialDownloadStatus(message.Connection);
                    break;
                case "recommendations":
                    await SendInitialRecommendationStatus(message.Connection);
                    break;
                case "introdetection":
                    await SendInitialIntroDetectionStatus(message.Connection);
                    break;
                case "transcoding":
                    await SendInitialTranscodingStatus(message.Connection);
                    break;
            }
        }

        private async Task HandleUnsubscription(WebSocketMessageInfo message)
        {
            _logger.LogInformation("WebSocket client unsubscribing");
            _connections.Remove(message.Connection.Id);
        }

        private async Task SendInitialDownloadStatus(IWebSocketConnection connection)
        {
            // Send current download status
            var status = new
            {
                enabled = _config.DownloadTrackingEnabled,
                activeDownloads = 0, // Would fetch from services
                queuedDownloads = 0
            };

            var message = new HavenWebSocketMessage
            {
                MessageType = "DownloadStatus",
                Data = JsonSerializer.Serialize(status)
            };

            await SendMessageAsync(connection, message);
        }

        private async Task SendInitialRecommendationStatus(IWebSocketConnection connection)
        {
            var status = new
            {
                enabled = _config.RecommendationsEnabled,
                lastUpdate = DateTime.UtcNow
            };

            var message = new HavenWebSocketMessage
            {
                MessageType = "RecommendationStatus",
                Data = JsonSerializer.Serialize(status)
            };

            await SendMessageAsync(connection, message);
        }

        private async Task SendInitialIntroDetectionStatus(IWebSocketConnection connection)
        {
            var status = new
            {
                enabled = _config.IntroDetectionEnabled,
                itemsProcessed = 0, // Would fetch from database
                itemsPending = 0
            };

            var message = new HavenWebSocketMessage
            {
                MessageType = "IntroDetectionStatus",
                Data = JsonSerializer.Serialize(status)
            };

            await SendMessageAsync(connection, message);
        }

        private async Task SendInitialTranscodingStatus(IWebSocketConnection connection)
        {
            var status = new
            {
                enabled = _config.EnhancedTranscodingEnabled,
                activeStreams = 0, // Would fetch from transcoding manager
                optimizedProfiles = 0
            };

            var message = new HavenWebSocketMessage
            {
                MessageType = "TranscodingStatus",
                Data = JsonSerializer.Serialize(status)
            };

            await SendMessageAsync(connection, message);
        }

        private async Task BroadcastToSubscribedClientsAsync(string subscriptionType, HavenWebSocketMessage message)
        {
            var tasks = new List<Task>();

            foreach (var connection in _connections.Values)
            {
                if (connection.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageAsync(connection, message));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendToUserAsync(Guid userId, HavenWebSocketMessage message)
        {
            var tasks = new List<Task>();

            foreach (var connection in _connections.Values)
            {
                if (connection.UserId == userId && connection.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageAsync(connection, message));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendMessageAsync(IWebSocketConnection connection, HavenWebSocketMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                await connection.SendAsync(json, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send WebSocket message");
            }
        }
    }

    /// <summary>
    /// Background service for periodic updates
    /// </summary>
    public class HavenWebSocketUpdateService
    {
        private readonly ILogger<HavenWebSocketUpdateService> _logger;
        private readonly HavenWebSocketHandler _handler;
        private readonly HavenConfiguration _config;
        private Timer? _updateTimer;

        public HavenWebSocketUpdateService(
            ILogger<HavenWebSocketUpdateService> logger,
            HavenWebSocketHandler handler,
            HavenConfiguration config)
        {
            _logger = logger;
            _handler = handler;
            _config = config;
        }

        public void Start()
        {
            _updateTimer = new Timer(SendPeriodicUpdates, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public void Stop()
        {
            _updateTimer?.Dispose();
        }

        private async void SendPeriodicUpdates(object? state)
        {
            try
            {
                if (_config.DownloadTrackingEnabled)
                {
                    // Send download progress updates
                    // This would fetch real data from download managers
                    var notification = new DownloadProgressNotification
                    {
                        DownloadId = Guid.NewGuid().ToString(),
                        Title = "Example Download",
                        Progress = Random.Shared.Next(0, 100),
                        Status = "downloading",
                        Eta = "5 minutes",
                        Source = "Radarr"
                    };

                    await _handler.SendDownloadProgressAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send periodic updates");
            }
        }
    }
}