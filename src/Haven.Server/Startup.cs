using System;
using System.Net.Http;
using Jellyfin.Api.Auth;
using Jellyfin.Api.Constants;
using Jellyfin.Server.Extensions;
using Haven.Api.Controllers;
using Haven.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Haven.Server
{
    /// <summary>
    /// Haven startup configuration.
    /// </summary>
    public class Startup
    {
        private readonly IServerApplicationHost _serverApplicationHost;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly HavenConfiguration _havenConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="serverApplicationHost">The server application host.</param>
        /// <param name="havenConfiguration">The Haven configuration.</param>
        public Startup(IServerApplicationHost serverApplicationHost, HavenConfiguration havenConfiguration)
        {
            _serverApplicationHost = serverApplicationHost;
            _serverConfigurationManager = serverApplicationHost.ConfigurationManager;
            _havenConfiguration = havenConfiguration;
        }

        /// <summary>
        /// Configures the service collection for the application.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            services.AddHttpContextAccessor();
            services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = _serverApplicationHost.HttpsPort;
            });
            
            // Add standard Jellyfin services
            services.AddJellyfinApi(_serverConfigurationManager);
            services.AddJellyfinApiSwagger();
            
            // Add Haven-specific services
            services.AddSingleton(_havenConfiguration);
            services.AddScoped<EnhancedController>();
            
            // Configure CORS for Haven
            services.AddCors(options =>
            {
                options.AddPolicy("HavenCorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add authentication
            services.AddJellyfinApiAuthorization();

            var productHeader = new System.Net.Http.Headers.ProductInfoHeaderValue(
                _havenConfiguration.GetBrandingName(isApiRequest: false),
                _serverApplicationHost.ApplicationVersionString);
            var acceptJsonHeader = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json", 1.0);
            var acceptAnyHeader = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*", 0.8);

            services
                .AddHttpClient(NamedClient.Default, c =>
                {
                    c.DefaultRequestHeaders.UserAgent.Add(productHeader);
                    c.DefaultRequestHeaders.Accept.Add(acceptJsonHeader);
                    c.DefaultRequestHeaders.Accept.Add(acceptAnyHeader);
                })
                .ConfigurePrimaryHttpMessageHandler(defaultHttpClientHandlerDelegate);

            services.AddHealthChecks()
                .AddCheck<DbHealthCheck>("HavenDb");

            services.AddHavenServices();
        }

        /// <summary>
        /// Configures the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBaseUrlRedirection();

            // Enable middleware to serve swagger-ui
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Haven API V1");
                    c.RoutePrefix = "api-docs/swagger";
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCompression();
            app.UseCors("HavenCorsPolicy");
            app.UseHttpsRedirection();

            app.UsePathTrim();
            app.UseQueryStringDecoding();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            // Add custom Haven middleware
            app.UseHavenEnhancements(_havenConfiguration);
        }
    }

    /// <summary>
    /// Extension methods for adding Haven services.
    /// </summary>
    public static class HavenServiceExtensions
    {
        /// <summary>
        /// Adds Haven-specific services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHavenServices(this IServiceCollection services)
        {
            // Add Haven-specific services here
            // For example: recommendation engine, download manager, etc.
            return services;
        }

        /// <summary>
        /// Adds Haven enhancement middleware to the pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="config">Haven configuration.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseHavenEnhancements(this IApplicationBuilder app, HavenConfiguration config)
        {
            // Add Haven-specific middleware here
            // For example: request interception for dual branding
            app.Use(async (context, next) =>
            {
                // Intercept API requests to maintain compatibility
                if (context.Request.Path.StartsWithSegments("/System/Info") && config.ApiCompatibilityMode)
                {
                    context.Items["UseJellyfinBranding"] = true;
                }
                
                await next();
            });

            return app;
        }
    }
}