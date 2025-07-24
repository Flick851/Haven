using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Jellyfin.Server.Implementations;
using Haven.Common.Configuration;
using Haven.Server.Startup;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Extensions.Logging;

namespace Haven.Server
{
    /// <summary>
    /// Class containing the entry point of the Haven application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The name of logging configuration file.
        /// </summary>
        public const string LoggingConfigFileDefault = "logging.default.json";

        /// <summary>
        /// The name of the logging configuration file containing the system-specific override settings.
        /// </summary>
        public const string LoggingConfigFileSystem = "logging.json";

        private static readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private static readonly ILoggerFactory _loggerFactory = new SerilogLoggerFactory();
        private static ILogger _logger = NullLogger.Instance;
        private static bool _restartOnShutdown;

        /// <summary>
        /// The entry point of the application.
        /// </summary>
        /// <param name="args">The command line arguments passed.</param>
        /// <returns>The application exit code.</returns>
        public static Task<int> Main(string[] args)
        {
            static Task<int> ErrorParsingArguments(IEnumerable<e> errors)
            {
                Environment.ExitCode = 1;
                return Task.FromResult(0);
            }

            // Parse the command line arguments and either start the app or exit indicating error
            return Parser.Default.ParseArguments<StartupOptions>(args)
                .MapResult(StartApp, ErrorParsingArguments);
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="options">The <see cref="StartupOptions" /> provided on the command line.</param>
        /// <returns>The application exit code.</returns>
        private static async Task<int> StartApp(StartupOptions options)
        {
            // Initialize Haven configuration
            var havenConfig = new HavenConfiguration();
            
            // Load Haven-specific settings if config file exists
            var havenConfigPath = Path.Combine(options.ConfigDir ?? "config", "haven.json");
            if (File.Exists(havenConfigPath))
            {
                var configJson = await File.ReadAllTextAsync(havenConfigPath);
                // In production, deserialize JSON to HavenConfiguration
                // For now, using default configuration
            }

            StartupHelpers.InitializeLoggingFramework(options, appPaths);
            _logger = _loggerFactory.CreateLogger("Main");

            // Display Haven banner
            HavenStartupBanner.DisplayBanner(_logger, havenConfig);

            AppDomain.CurrentDomain.UnhandledException += (_, e)
                => _logger.LogCritical((Exception)e.ExceptionObject, "Unhandled Exception");

            _logger.LogInformation(
                "Haven version: {Version}",
                Assembly.GetEntryAssembly()!.GetName().Version!.ToString(3) + havenConfig.VersionSuffix);

            PerformPreInitialization(options.ConfigDir, options.DataDir);

            Migrations.Run(options, _loggerFactory);

            var appPaths = ServerApplicationPaths.DefaultApplicationPaths;
            NetCoreApp.ResourcePath = appPaths.ResourcesPath;

            var appHost = new CoreAppHost(
                appPaths,
                _loggerFactory,
                options,
                // Pass Haven configuration to the app host
                services =>
                {
                    services.AddSingleton(havenConfig);
                });

            try
            {
                var webHost = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webHostBuilder =>
                    {
                        webHostBuilder.ConfigureKestrel((builderContext, options) =>
                        {
                            var addresses = appHost.NetManager.GetAllBindInterfaces();
                            bool flagged = false;
                            foreach (var netAdd in addresses)
                            {
                                _logger.LogInformation("Kestrel is listening on {Address}", IPAddress.IPv6Any.Equals(netAdd.Address) ? "All IPv6 addresses" : netAdd.Address);
                                options.Listen(netAdd.Address, appHost.HttpPort, listenOptions =>
                                {
                                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                                    listenOptions.UseHttps(appHost.Certificate);
                                });
                            }
                        })
                        .ConfigureAppConfiguration(config => config.ConfigureAppConfiguration(options, appPaths, _loggerFactory))
                        .UseSerilog()
                        .UseStartup(_ => new Startup(appHost, havenConfig));
                    })
                    .ConfigureServices(services =>
                    {
                        // Register Haven services
                        services.AddSingleton(havenConfig);
                    })
                    .Build();

                await appHost.InitAsync().ConfigureAwait(false);
                await webHost.StartAsync(_tokenSource.Token).ConfigureAwait(false);

                if (options.NoAutoRunWebApp || options.NoWebClient)
                {
                    webHost.ServerFeatures
                        .Get<IServerAddressesFeature>()
                        ?.Addresses
                        .ToList()
                        .ForEach(address => _logger.LogInformation("Haven is running at {Address}", address));
                }

                await Task.Delay(Timeout.Infinite, _tokenSource.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error starting Haven");
                return 1;
            }
            finally
            {
                appHost?.Dispose();
            }

            return 0;
        }

        /// <summary>
        /// Performs pre-initialization tasks.
        /// </summary>
        /// <param name="configDir">The configuration directory.</param>
        /// <param name="dataDir">The data directory.</param>
        private static void PerformPreInitialization(string? configDir, string? dataDir)
        {
            // Increase the max http request limit
            // The default connection limit is 10 for ASP.NET hosted applications and 2 for all others.
            ServicePointManager.DefaultConnectionLimit = Math.Max(96, ServicePointManager.DefaultConnectionLimit);

            // Disable the "Expect: 100-Continue" header
            ServicePointManager.Expect100Continue = false;
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        internal static void Shutdown()
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel();
            }
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        internal static void Restart()
        {
            _restartOnShutdown = true;
            Shutdown();
        }
    }
}