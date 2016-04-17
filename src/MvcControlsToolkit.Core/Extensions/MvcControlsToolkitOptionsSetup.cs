using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Mvc.Rendering;
using MvcControlsToolkit.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.AspNet.Mvc.DataAnnotations;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using MvcControlsToolkit.Core.ModelBinding;




namespace MvcControlsToolkit.Core.Extensions
{
    public class MvcControlsToolkitOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            
            options.ModelMetadataDetailsProviders.Add(new Validation.ValidationMetadataProvider());
            int bcount = 0;
            IModelBinder res = null;
            foreach (var b in options.ModelBinders)
            {
                if (b is SimpleTypeModelBinder)
                {
                    res = b;
                    break;
                }
                bcount++;
            }
            if(res != null)
            {
                options.ModelBinders.Remove(res);
                options.ModelBinders.Insert(bcount, new SimpleTypeModelBinderExt());
            }
            options.ModelBinders.Insert(0, new TransformationModelBinder());
            
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

            toRemove = options.ClientModelValidatorProviders.Where(m => m.GetType() == typeof(DataAnnotationsClientModelValidatorProvider)).SingleOrDefault();
            if (toRemove != null)
                options.ClientModelValidatorProviders.Remove(toRemove);

            options.ClientModelValidatorProviders.Add(new DataAnnotationsClientModelValidatorProviderExt(dataAnnotationsLocalizationOptions, stringLocalizerFactory));
            options.ClientModelValidatorProviders.Add(new TypeClientModelValidatorProvider());
            

        }
    }
}
