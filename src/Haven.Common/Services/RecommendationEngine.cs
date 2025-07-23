using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Haven.Common.Services
{
    /// <summary>
    /// Interface for ML-powered recommendations engine
    /// </summary>
    public interface IRecommendationEngine
    {
        Task<List<RecommendationGroup>> GetPersonalizedRecommendationsAsync(Guid userId);
        Task<List<BaseItemDto>> GetSimilarItemsAsync(Guid itemId, int limit = 10);
        Task UpdateUserInteractionAsync(Guid userId, Guid itemId, InteractionType type);
        Task<List<BaseItemDto>> GetTrendingItemsAsync(int limit = 20);
    }

    /// <summary>
    /// ML-powered recommendation engine for Haven
    /// </summary>
    public class HavenRecommendationEngine : IRecommendationEngine
    {
        private readonly ILogger<HavenRecommendationEngine> _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;

        // In-memory storage for demonstration - in production, use proper ML model storage
        private readonly Dictionary<Guid, UserProfile> _userProfiles = new();
        private readonly Dictionary<Guid, ItemFeatures> _itemFeatures = new();

        public HavenRecommendationEngine(
            ILogger<HavenRecommendationEngine> logger,
            ILibraryManager libraryManager,
            IUserManager userManager,
            IUserDataManager userDataManager)
        {
            _logger = logger;
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataManager = userDataManager;
        }

        public async Task<List<RecommendationGroup>> GetPersonalizedRecommendationsAsync(Guid userId)
        {
            _logger.LogInformation("Generating personalized recommendations for user {UserId}", userId);
            
            var recommendations = new List<RecommendationGroup>();
            
            try
            {
                var user = _userManager.GetUserById(userId);
                if (user == null) return recommendations;

                // Get user viewing history
                var recentlyWatched = await GetRecentlyWatchedAsync(userId);
                
                // Generate different recommendation groups
                recommendations.Add(new RecommendationGroup
                {
                    Title = "Because You Watched",
                    Items = await GetBecauseYouWatchedAsync(userId, recentlyWatched),
                    Type = RecommendationType.BecauseYouWatched
                });

                recommendations.Add(new RecommendationGroup
                {
                    Title = "Trending Now",
                    Items = await GetTrendingItemsAsync(15),
                    Type = RecommendationType.Trending
                });

                recommendations.Add(new RecommendationGroup
                {
                    Title = "Top Picks For You",
                    Items = await GetTopPicksAsync(userId),
                    Type = RecommendationType.TopPicks
                });

                recommendations.Add(new RecommendationGroup
                {
                    Title = "Continue Watching",
                    Items = await GetContinueWatchingAsync(userId),
                    Type = RecommendationType.ContinueWatching
                });

                recommendations.Add(new RecommendationGroup
                {
                    Title = "New Releases",
                    Items = await GetNewReleasesAsync(),
                    Type = RecommendationType.NewReleases
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate recommendations for user {UserId}", userId);
            }

            return recommendations;
        }

        public async Task<List<BaseItemDto>> GetSimilarItemsAsync(Guid itemId, int limit = 10)
        {
            var similarItems = new List<BaseItemDto>();
            
            try
            {
                var item = _libraryManager.GetItemById(itemId);
                if (item == null) return similarItems;

                // Get item features (genres, tags, year, etc.)
                var features = ExtractItemFeatures(item);
                
                // Find similar items based on features
                var allItems = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { item.GetType().Name },
                    IsVirtualItem = false,
                    Limit = 100
                });

                var scoredItems = allItems
                    .Where(i => i.Id != itemId)
                    .Select(i => new
                    {
                        Item = i,
                        Score = CalculateSimilarityScore(features, ExtractItemFeatures(i))
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(limit)
                    .Select(x => x.Item.ToDto())
                    .ToList();

                similarItems.AddRange(scoredItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get similar items for {ItemId}", itemId);
            }

            return similarItems;
        }

        public async Task UpdateUserInteractionAsync(Guid userId, Guid itemId, InteractionType type)
        {
            _logger.LogDebug("Updating user interaction: User {UserId}, Item {ItemId}, Type {Type}", 
                userId, itemId, type);

            // Update user profile based on interaction
            if (!_userProfiles.ContainsKey(userId))
            {
                _userProfiles[userId] = new UserProfile { UserId = userId };
            }

            var profile = _userProfiles[userId];
            var item = _libraryManager.GetItemById(itemId);
            
            if (item != null)
            {
                var features = ExtractItemFeatures(item);
                UpdateUserPreferences(profile, features, type);
            }

            await Task.CompletedTask;
        }

        public async Task<List<BaseItemDto>> GetTrendingItemsAsync(int limit = 20)
        {
            // Get items with high recent activity
            var recentDate = DateTime.UtcNow.AddDays(-7);
            
            var trendingItems = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "Movie", "Series" },
                IsVirtualItem = false,
                MinDateCreated = recentDate,
                OrderBy = new[] { (ItemSortBy.DateCreated, SortOrder.Descending) },
                Limit = limit
            });

            return trendingItems.Select(i => i.ToDto()).ToList();
        }

        private async Task<List<BaseItem>> GetRecentlyWatchedAsync(Guid userId)
        {
            var userData = _userDataManager.GetAllUserData(userId)
                .Where(d => d.Played && d.LastPlayedDate.HasValue)
                .OrderByDescending(d => d.LastPlayedDate)
                .Take(20)
                .ToList();

            var items = new List<BaseItem>();
            foreach (var data in userData)
            {
                var item = _libraryManager.GetItemById(data.ItemId);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private async Task<List<BaseItemDto>> GetBecauseYouWatchedAsync(Guid userId, List<BaseItem> recentlyWatched)
        {
            var recommendations = new List<BaseItemDto>();
            
            foreach (var watched in recentlyWatched.Take(5))
            {
                var similar = await GetSimilarItemsAsync(watched.Id, 3);
                recommendations.AddRange(similar);
            }

            return recommendations.Distinct().Take(15).ToList();
        }

        private async Task<List<BaseItemDto>> GetTopPicksAsync(Guid userId)
        {
            // Use user profile to generate top picks
            if (!_userProfiles.ContainsKey(userId))
            {
                return await GetTrendingItemsAsync(10);
            }

            var profile = _userProfiles[userId];
            var allItems = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "Movie", "Series" },
                IsVirtualItem = false,
                Limit = 100
            });

            var scoredItems = allItems
                .Select(i => new
                {
                    Item = i,
                    Score = CalculateUserPreferenceScore(profile, ExtractItemFeatures(i))
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .Select(x => x.Item.ToDto())
                .ToList();

            return scoredItems;
        }

        private async Task<List<BaseItemDto>> GetContinueWatchingAsync(Guid userId)
        {
            var userData = _userDataManager.GetAllUserData(userId)
                .Where(d => !d.Played && d.PlaybackPositionTicks > 0)
                .OrderByDescending(d => d.LastPlayedDate)
                .Take(10)
                .ToList();

            var items = new List<BaseItemDto>();
            foreach (var data in userData)
            {
                var item = _libraryManager.GetItemById(data.ItemId);
                if (item != null)
                {
                    items.Add(item.ToDto());
                }
            }

            return items;
        }

        private async Task<List<BaseItemDto>> GetNewReleasesAsync()
        {
            var recentDate = DateTime.UtcNow.AddDays(-30);
            
            var newItems = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "Movie", "Series" },
                IsVirtualItem = false,
                MinDateCreated = recentDate,
                OrderBy = new[] { (ItemSortBy.DateCreated, SortOrder.Descending) },
                Limit = 15
            });

            return newItems.Select(i => i.ToDto()).ToList();
        }

        private ItemFeatures ExtractItemFeatures(BaseItem item)
        {
            return new ItemFeatures
            {
                ItemId = item.Id,
                Genres = item.Genres?.ToList() ?? new List<string>(),
                Tags = item.Tags?.ToList() ?? new List<string>(),
                Year = item.ProductionYear ?? 0,
                Rating = item.CommunityRating ?? 0,
                Runtime = item.RunTimeTicks ?? 0
            };
        }

        private double CalculateSimilarityScore(ItemFeatures item1, ItemFeatures item2)
        {
            double score = 0;

            // Genre similarity
            var genreOverlap = item1.Genres.Intersect(item2.Genres).Count();
            score += genreOverlap * 0.4;

            // Tag similarity
            var tagOverlap = item1.Tags.Intersect(item2.Tags).Count();
            score += tagOverlap * 0.3;

            // Year similarity (closer years = higher score)
            var yearDiff = Math.Abs(item1.Year - item2.Year);
            score += Math.Max(0, 10 - yearDiff) * 0.1;

            // Rating similarity
            var ratingDiff = Math.Abs(item1.Rating - item2.Rating);
            score += Math.Max(0, 10 - ratingDiff) * 0.2;

            return score;
        }

        private double CalculateUserPreferenceScore(UserProfile profile, ItemFeatures item)
        {
            double score = 0;

            // Check genre preferences
            foreach (var genre in item.Genres)
            {
                if (profile.GenrePreferences.ContainsKey(genre))
                {
                    score += profile.GenrePreferences[genre];
                }
            }

            // Check tag preferences
            foreach (var tag in item.Tags)
            {
                if (profile.TagPreferences.ContainsKey(tag))
                {
                    score += profile.TagPreferences[tag] * 0.5;
                }
            }

            return score;
        }

        private void UpdateUserPreferences(UserProfile profile, ItemFeatures features, InteractionType type)
        {
            double weight = type switch
            {
                InteractionType.Watched => 1.0,
                InteractionType.Liked => 1.5,
                InteractionType.Disliked => -1.0,
                InteractionType.Started => 0.5,
                _ => 0
            };

            // Update genre preferences
            foreach (var genre in features.Genres)
            {
                if (!profile.GenrePreferences.ContainsKey(genre))
                    profile.GenrePreferences[genre] = 0;
                
                profile.GenrePreferences[genre] += weight;
            }

            // Update tag preferences
            foreach (var tag in features.Tags)
            {
                if (!profile.TagPreferences.ContainsKey(tag))
                    profile.TagPreferences[tag] = 0;
                
                profile.TagPreferences[tag] += weight * 0.5;
            }
        }
    }

    // Supporting classes
    public class RecommendationGroup
    {
        public string Title { get; set; } = string.Empty;
        public List<BaseItemDto> Items { get; set; } = new List<BaseItemDto>();
        public RecommendationType Type { get; set; }
    }

    public enum RecommendationType
    {
        BecauseYouWatched,
        TopPicks,
        Trending,
        ContinueWatching,
        NewReleases,
        Genre,
        Mood
    }

    public enum InteractionType
    {
        Watched,
        Started,
        Liked,
        Disliked,
        AddedToWatchlist
    }

    internal class UserProfile
    {
        public Guid UserId { get; set; }
        public Dictionary<string, double> GenrePreferences { get; set; } = new();
        public Dictionary<string, double> TagPreferences { get; set; } = new();
        public List<Guid> WatchedItems { get; set; } = new();
        public List<Guid> LikedItems { get; set; } = new();
        public List<Guid> DislikedItems { get; set; } = new();
    }

    internal class ItemFeatures
    {
        public Guid ItemId { get; set; }
        public List<string> Genres { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public int Year { get; set; }
        public float Rating { get; set; }
        public long Runtime { get; set; }
    }
}