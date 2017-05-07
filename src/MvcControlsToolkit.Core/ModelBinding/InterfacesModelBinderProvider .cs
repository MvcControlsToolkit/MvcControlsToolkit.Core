using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MvcControlsToolkit.Core.Extensions;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class InterfacesModelBinderProvider: IModelBinderProvider
    {
        private IDIMeta servicesInfo;
        public InterfacesModelBinderProvider(IDIMeta servicesInfo)
        {
            this.servicesInfo = servicesInfo;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.IsCollectionType &&
                servicesInfo.IsRegistred(context.Metadata.ModelType) &&
                (context.BindingInfo.BindingSource == null ||
                !context.BindingInfo.BindingSource
                .CanAcceptDataFrom(BindingSource.Services)))
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
                for (var i = 0; i < context.Metadata.Properties.Count; i++)
                {
                    var property = context.Metadata.Properties[i];
                    propertyBinders.Add(property, context.CreateBinder(property));
                }
                return new InterfacesModelBinder(propertyBinders);
            }

            return null;
        }
    }
}
