using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MvcControlsToolkit.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using MvcControlsToolkit.Core.ModelBinding;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;




namespace MvcControlsToolkit.Core.Extensions
{
    public class MvcControlsToolkitOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {

            options.ModelMetadataDetailsProviders.Add(new Validation.ValidationMetadataProvider());
           
           
            int bcount = 0;
            IModelBinderProvider res = null;
            foreach (var b in options.ModelBinderProviders)
            {
                if (b is SimpleTypeModelBinderProvider)
                {
                    res = b;
                    break;
                }
                bcount++;
            }
            if (res != null)
            {
                options.ModelBinderProviders.Remove(res);
                options.ModelBinderProviders.Insert(bcount, new SimpleTypeModelBinderProviderExt());
            }
            
          options.ModelBinderProviders.Insert(0, new TransformationModelBinderProvider());
          
        }
    }

    public class MvcControlsToolkitViewOptionsSetup : IConfigureOptions<MvcViewOptions>
    {
        IServiceProvider serviceProvider;
        public MvcControlsToolkitViewOptionsSetup(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public void Configure(MvcViewOptions options)
        {
            var dataAnnotationsLocalizationOptions =
                serviceProvider.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var stringLocalizerFactory = serviceProvider.GetService<IStringLocalizerFactory>();
            

            var toRemove = options.ClientModelValidatorProviders.Where(m => m.GetType() == typeof(NumericClientModelValidatorProvider)).SingleOrDefault();
            if(toRemove != null)
            options.ClientModelValidatorProviders.Remove(toRemove);

            options.ClientModelValidatorProviders.Add(new TypeClientModelValidatorProvider());
            

        }
    }
}
