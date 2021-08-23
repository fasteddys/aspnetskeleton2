﻿using System.Collections.Generic;
using System.Linq;
using Karambolo.AspNetCore.Bundling;
using Karambolo.AspNetCore.Bundling.Internal.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApp.UI.Infrastructure.Theming;

namespace WebApp.UI
{
    public class Bundles : DesignTimeBundlingConfiguration
    {
        // setup for run-time mode bundling
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, UIOptions.BundleOptions options)
        {
            var bundling = services.AddBundling()
                .UseNUglify()
                .UseHashVersioning()
                .AddCss()
                .AddJs()
                .AddSass()
                .AddEcmaScript();

            if (options.UsePersistentCache)
            {
                bundling.UseFileSystemCaching();
                services.Configure<FileSystemBundleCacheOptions>(configuration.GetSection("Response:BundleCaching"));
            }
            else
                bundling.UseMemoryCaching();

            if (options.EnableResponseMinification)
                bundling.EnableMinification();

            if (options.EnableResponseCaching)
                bundling.EnableCacheHeader(options.CacheHeaderMaxAge ?? UIOptions.DefaultCacheHeaderMaxAge);

            if (environment.IsDevelopment())
            {
                bundling
                    .EnableChangeDetection()
                    .EnableSourceIncludes();
            }
        }

        public Bundles() { }

        // setup for design-time mode bundling
        public override IEnumerable<IBundlingModule> Modules => base.Modules.Concat(new IBundlingModule[]
        {
            new NUglifyBundlingModule(),
            new SassBundlingModule(),
            new EcmaScriptBundlingModule(),
        });

        public override void Configure(BundleCollectionConfigurer bundles)
        {
            var themeProvider =
                bundles.AppServices.GetService<IThemeProvider>() ??
                new ThemeProvider(bundles.AppServices.GetRequiredService<IWebHostEnvironment>());

            var themes = themeProvider.GetThemes();

            #region Global

            bundles.AddCss("/css/global/vendor.css")
                .Include("/lib/font-awesome/css/font-awesome.css");

            bundles.AddJs("/js/global/vendor.js")
                .Include("/lib/jquery/dist/jquery.js")
                .Include("/lib/jquery-validation/dist/jquery.validate.js")
                .Include("/lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js")
                .Include("/lib/bootstrap/dist/js/bootstrap.bundle.js");

            bundles.AddJs("/js/global/site.js")
                .Include("/js/*.js");

            #endregion

            #region Public UI

            // These bundles are unused as the template project doesn't have a public UI (like a landing page) and
            // the unrestricted pages (account-related pages mainly) use the dashboard's stylesheet currently.

            bundles.AddJs("/js/public/site.js")
                .Include("/js/public/site.js")
                .EnableEs6ModuleBundling();

            bundles.AddSass("/css/public/site.css")
                .Include("/scss/public/site.scss");

            #endregion

            #region Dashboard

            bundles.AddJs("/js/dashboard/vendor.js")
                .Include("/js/dashboard/coreui.js");

            bundles.AddJs("/js/dashboard/site.js")
                .Include("/js/dashboard/site.js")
                .EnableEs6ModuleBundling();

            for (int i = 0, n = themes.Count; i < n; i++)
            {
                var sourcePath = themeProvider.GetThemePath(ThemeProvider.ThemesBasePath, themes[i]);
                var destPath = themeProvider.GetThemePath("/css", themes[i]);

                bundles.AddSass(destPath + "/dashboard/site.css")
                    .Include(sourcePath + "/dashboard/site.scss");
            }

            #endregion
        }
    }
}
