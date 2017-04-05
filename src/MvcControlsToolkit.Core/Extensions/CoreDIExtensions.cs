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
using Microsoft.AspNetCore.Mvc.Razor;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MvcControlsToolkit.Core.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            services.Configure<MvcJsonOptions>(o => o.SerializerSettings.ContractResolver = new RuntimeTypeContractResolver());
            services.AddSingleton<DefaultTagHelpersProvider>();
            services.AddPreferences()
                .AddPreferencesClass<Html5InputSupport>("Browser.Html5InputSupport")
                .AddPreferencesProvider(new CookieProvider("Browser", "_browser_basic_capabilities") {Priority=0 })
                .AddPreferencesProvider(new RequestJsonProvider("Browser", "_browser_basic_capabilities"){Priority=1});
            services.AddTransient<IConfigureOptions<MvcControlsToolkitOptions>, MvcControlsToolkitSetup>();
            services.AddScoped<IViewBufferScope, SafeMemoryPoolViewBufferScope>();
            services.TryAddSingleton<IObjectModelValidator>(s =>
            {
                var options = s.GetRequiredService<IOptions<MvcOptions>>().Value;
                var metadataProvider = s.GetRequiredService<IModelMetadataProvider>();
                return new EnhancedObjectValidator(metadataProvider, options.ModelValidatorProviders);
            });
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
        public static IServiceCollection AddTagHelpersExtension<T>(this IServiceCollection services)
            where T: ITagHelpersExtension, new()
        {
            var extension = new T();
            if(extension.Providers != null)
            {
                foreach (var x in extension.Providers) services.AddSingleton(x.GetType(), x);
            }
            if(extension.ProviderExtensions != null)
            {
                foreach (var x in extension.ProviderExtensions) TagHelpersProviderExtensionsRegister.Register(x);
            }
            if (extension.HasEmbeddedFiles)
            {
                services.Configure<RazorViewEngineOptions>(options =>
                {
                    var assembly = typeof(T).GetTypeInfo().Assembly;
                    options.FileProviders.Add(
                      new EmbeddedFileProvider(assembly,
                      assembly?.GetName()?.Name));
                }
            );
            }
            return services;
        }
     }
}
