using System;
using Haven.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Haven.Server.Startup
{
    /// <summary>
    /// Handles displaying the Haven startup banner
    /// </summary>
    public static class HavenStartupBanner
    {
        /// <summary>
        /// Displays the Haven startup banner in the console
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="config">Haven configuration</param>
        public static void DisplayBanner(ILogger logger, HavenConfiguration config)
        {
            if (!config.ShowHavenBranding)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
    ██╗  ██╗ █████╗ ██╗   ██╗███████╗███╗   ██╗
    ██║  ██║██╔══██╗██║   ██║██╔════╝████╗  ██║
    ███████║███████║██║   ██║█████╗  ██╔██╗ ██║
    ██╔══██║██╔══██║╚██╗ ██╔╝██╔══╝  ██║╚██╗██║
    ██║  ██║██║  ██║ ╚████╔╝ ███████╗██║ ╚████║
    ╚═╝  ╚═╝╚═╝  ╚═╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝
    ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("    Enhanced Media Server v1.0");
            Console.WriteLine("    Based on Jellyfin - The Free Software Media System");
            Console.WriteLine();

            // Display feature status
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("    Enhanced Features Status:");
            Console.ForegroundColor = ConsoleColor.White;
            
            WriteFeatureStatus("API Compatibility Mode", config.ApiCompatibilityMode);
            WriteFeatureStatus("Netflix Mode", config.NetflixMode);
            WriteFeatureStatus("Intro Detection", config.IntroDetectionEnabled);
            WriteFeatureStatus("Download Tracking", config.DownloadTrackingEnabled);
            WriteFeatureStatus("ML Recommendations", config.RecommendationsEnabled);
            WriteFeatureStatus("Enhanced Transcoding", config.EnhancedTranscodingEnabled);
            
            Console.WriteLine();
            Console.WriteLine("    " + new string('─', 50));
            Console.WriteLine();

            logger.LogInformation("Haven Enhanced Media Server v{Version} starting up", "1.0");
            logger.LogInformation("API Compatibility Mode: {Mode}", config.ApiCompatibilityMode ? "Enabled" : "Disabled");
        }

        private static void WriteFeatureStatus(string featureName, bool enabled)
        {
            Console.Write($"    • {featureName}: ");
            if (enabled)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Enabled");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Disabled");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}