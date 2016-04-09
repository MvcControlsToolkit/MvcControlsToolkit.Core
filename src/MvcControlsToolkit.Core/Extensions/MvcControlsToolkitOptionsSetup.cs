using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Microsoft.AspNet.Mvc.Internal;


namespace MvcControlsToolkit.Core.Extensions
{
    public class MvcControlsToolkitOptionsSetup : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            var toRemove = options.ModelMetadataDetailsProviders.Where(m => m.GetType() == typeof(DefaultValidationMetadataProvider)).SingleOrDefault();
            if(toRemove != null) options.ModelMetadataDetailsProviders.Remove(toRemove);
            options.ModelMetadataDetailsProviders.Add(new Validation.ValidationMetadataProvider());
        }
    }
}
