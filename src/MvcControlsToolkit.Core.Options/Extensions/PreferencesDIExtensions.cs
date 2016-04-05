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
        public static IServiceCollection AddPreferencesClass<T>(this IServiceCollection services, string prefix)
            where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("prefix");
            services.AddScoped<T>(x => 
                x.GetService<IPreferencesService>().BuildOptionsObject<T>());
            DefaultPreferencesService.OptionsObjectsInfos[typeof(T)] = prefix;
            return services;
        }
        public static IServiceCollection AddPreferencesProvider(this IServiceCollection services, IOptionsProvider provider)
        {
            if (provider == null) throw new ArgumentException("provider");
            ProvidersDictionary.Add(provider);
            return services;
        }
    }
}
