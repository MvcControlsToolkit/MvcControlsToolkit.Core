using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using MvcControlsToolkit.Core.Options.Extensions;
using MvcControlsToolkit.Core.Options.Providers;
using MvcControlsToolkit.Core.ModelBinding;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using MvcControlsToolkit.Core.Options;
using MvcControlsToolkit.Core.TagHelpers.Providers;
using MvcControlsToolkit.Core.TagHelpers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using MvcControlsToolkit.Core.ViewFeatures;

namespace MvcControlsToolkit.Core.Extensions
{
    public static class CoreDIExtensions
    {
        private static bool initialized=false;
        public static IServiceCollection AddMvcControlsToolkit(this IServiceCollection services, Action<MvcControlsToolkitOptions> setupAction=null)
        {

            if (initialized) return services;
            services.AddSingleton<IValidationAttributeAdapterProvider, MvcControlsToolkit.Core.Validation.ValidationAttributeAdapterProviderExt>();
            
            services.AddScoped<RequestTransformationsRegister>();

            services.AddTransient<IConfigureOptions<MvcOptions>, MvcControlsToolkitOptionsSetup>();
            services.AddTransient<IConfigureOptions<MvcViewOptions>, MvcControlsToolkitViewOptionsSetup>();
            services.AddSingleton<DefaultTagHelpersProvider>();
            services.AddPreferences()
                .AddPreferencesClass<Html5InputSupport>("Browser.Html5InputSupport")
                .AddPreferencesProvider(new CookieProvider("Browser", "_browser_basic_capabilities") {Priority=0 })
                .AddPreferencesProvider(new RequestJsonProvider("Browser", "_browser_basic_capabilities"){Priority=1});
            services.AddTransient<IConfigureOptions<MvcControlsToolkitOptions>, MvcControlsToolkitSetup>();
            services.AddScoped<IViewBufferScope, SafeMemoryPoolViewBufferScope>();
            if (setupAction != null)
                services.Configure(setupAction);
            return services;
        }
        public static IServiceCollection AddTagHelpersProvider(this IServiceCollection services, Type providerType, ITagHelpersProvider instance=null)
        {
            if (instance == null)
                services.AddSingleton(providerType);
            else
                services.AddSingleton(providerType, instance);
            return services;

        }
     }
}
