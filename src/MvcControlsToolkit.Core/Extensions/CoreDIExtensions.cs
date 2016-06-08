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

namespace MvcControlsToolkit.Core.Extensions
{
    public static class CoreDIExtensions
    {
        public static IServiceCollection AddMvcControlsToolkit(this IServiceCollection services, Action<MvcControlsToolkitOptions> setupAction=null)
        {

            services.AddSingleton<IValidationAttributeAdapterProvider, MvcControlsToolkit.Core.Validation.ValidationAttributeAdapterProviderExt>();
            services.AddScoped<RequestTransformationsRegister>();

            services.AddTransient<IConfigureOptions<MvcOptions>, MvcControlsToolkitOptionsSetup>();
            services.AddTransient<IConfigureOptions<MvcViewOptions>, MvcControlsToolkitViewOptionsSetup>();

            services.AddPreferences()
                .AddPreferencesClass<MvcControlsToolkit.Core.Options.Html5InputSupport>("Browser.Html5InputSupport")
                .AddPreferencesProvider(new CookieProvider("Browser", "_browser_basic_capabilities") {Priority=0 })
                .AddPreferencesProvider(new RequestJsonProvider("Browser", "_browser_basic_capabilities"){Priority=1});
            services.AddTransient<IConfigureOptions<MvcControlsToolkitOptions>, MvcControlsToolkitSetup>();
            if (setupAction != null)
                services.Configure(setupAction);
            return services;
        }
    }
}
