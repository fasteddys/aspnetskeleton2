﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Karambolo.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using WebApp.Common.Infrastructure.Validation;
using WebApp.Core.Infrastructure;
using WebApp.Service;
using WebApp.Service.Infrastructure;
using WebApp.Service.Infrastructure.Caching;
using WebApp.Service.Infrastructure.Database;
using WebApp.Service.Infrastructure.Events;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Mailing;
using WebApp.Service.Settings;
using WebApp.Service.Translations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceLayerServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services, IServiceProvider optionsProvider)
        {
            services.AddDataAccess(optionsProvider);

            services.AddMemoryCache();
            services.AddOptions<MemoryCacheOptions>()
                .Configure<IClock>((options, clock) => options.Clock = new ClockAdapter(clock));

            services.AddSingleton<ICache, InProcessCache>();

            services
                .AddSingleton<EventBus>()
                .AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<EventBus>())
                .AddSingleton<IEventListener>(sp => sp.GetRequiredService<EventBus>());

            services
                .AddSingleton<ISettingsSource, DbSettingsSource>()
                .AddSingleton<ISettingsProvider, SettingsProvider>();

            services
                .AddSingleton<ITranslationsSource, FileTranslationsSource>()
                .AddSingleton<ITranslationsProvider, TranslationsProvider>();

            services
                .AddSingleton(sp =>
                    sp.GetRequiredService<ISettingsProvider>().EnableLocalization() ?
                    ActivatorUtilities.CreateInstance<POStringLocalizerFactory>(sp) :
                    (IStringLocalizerFactory)NullStringLocalizerFactory.Instance)
                .AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            services.AddApplicationInitializers();

            services.AddSingleton<IExecutionContextAccessor, DefaultExecutionContextAccessor>();

            services.AddSingleton<InterceptorConfiguration.ConfigureDispatcherOptions>();

            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
            services.AddSingleton<IConfigureOptions<CommandDispatcherOptions>>(sp => sp.GetRequiredService<InterceptorConfiguration.ConfigureDispatcherOptions>());

            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            services.AddSingleton<IConfigureOptions<QueryDispatcherOptions>>(sp => sp.GetRequiredService<InterceptorConfiguration.ConfigureDispatcherOptions>());

            services.AddSingleton<IMailTypeCatalog, MailTypeCatalog>();
            services.AddSingleton<IMailSenderService, MailSenderService>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<IMailSenderService>());

            services.AddAssemblyServices(typeof(ServiceLayerServiceCollectionExtensions).Assembly);

            return services;
        }

        private static IServiceCollection AddApplicationInitializers(this IServiceCollection services)
        {
            // registration order matters!

            services.AddScoped<IApplicationInitializer, DbInitializer>();

            services.AddScoped<IApplicationInitializer>(sp => new DelegatedApplicationInitializer(ct =>
            {
                var settingsProvider = sp.GetRequiredService<ISettingsProvider>();
                var translationsProvider = sp.GetRequiredService<ITranslationsProvider>();
                return Task.WhenAll(settingsProvider.Initialization, translationsProvider.Initialization).AsCancelable(ct);
            }));

            return services;
        }

        private static void AddAssemblyServices(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                if (type.IsClass && !type.IsAbstract)
                {
                    services.AddValidators(type);
                    services.AddCommandHandlers(type);
                    services.AddQueryHandlers(type);
                    services.AddMailTypeDefinition(type);
                }
        }

        private static void AddValidators(this IServiceCollection services, Type type)
        {
            foreach (var interfaceType in type.GetClosedInterfaces(typeof(IValidator<>)))
                services.AddTransient(interfaceType, type);
        }

        private static void AddCommandHandlers(this IServiceCollection services, Type type)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(CommandHandler<>))
                    services.AddTransient(baseType, type);
        }

        private static void AddQueryHandlers(this IServiceCollection services, Type type)
        {
            for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(QueryHandler<,>))
                    services.AddTransient(baseType, type);
        }

        private static void AddMailTypeDefinition(this IServiceCollection services, Type type)
        {
            if (type.HasInterface(typeof(IMailTypeDefinition)))
                services.AddTransient(typeof(IMailTypeDefinition), type);
        }

        private sealed class ClockAdapter : ISystemClock
        {
            private readonly IClock _clock;

            public ClockAdapter(IClock clock)
            {
                _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            }

            DateTimeOffset ISystemClock.UtcNow => _clock.UtcNow;
        }
    }
}
