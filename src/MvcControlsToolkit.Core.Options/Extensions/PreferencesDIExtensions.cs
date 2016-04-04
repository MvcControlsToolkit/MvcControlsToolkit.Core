using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MvcControlsToolkit.Core.Options.Extensions
{
    public static class PreferencesDIExtensions
    {
        public static IServiceCollection AddPreferences(this IServiceCollection services)
        {
            services.AddScoped<IOptionsDictionary, DefaultOptionsDictionary>();
            services.AddScoped<IPreferencesService, DefaultPreferencesService>();
            return services;
        }
        public static IServiceCollection AddPreferencesClass<T>(this IServiceCollection services)
            where T : class, new()
        {
            services.AddScoped<T>(x => x.GetService<IPreferencesService>().BuildOptionsObject<T>());
            return services;
        }
        public static IServiceCollection AddPreferencesProvider<T>(this IServiceCollection services, IOptionsProvider provider)
        {
            ProvidersDictionary.Add(provider);
            return services;
        }
    }
}
