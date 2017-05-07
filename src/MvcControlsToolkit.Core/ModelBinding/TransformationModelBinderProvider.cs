using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class TransformationModelBinderProvider : IModelBinderProvider
    {
        protected IModelBinderProvider originalComplexModelBinderProvider;
        protected IModelBinderProvider InterfacesModelBinderProvider;
        public TransformationModelBinderProvider(IModelBinderProvider originalBinder,
            IModelBinderProvider InterfacesModelBinderProvider)
        {
            this.originalComplexModelBinderProvider = originalBinder;
            this.InterfacesModelBinderProvider = InterfacesModelBinderProvider;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType)
            {
                return new TransformationModelBinder(context.CreateBinder,
                    InterfacesModelBinderProvider.GetBinder(context) ??
                    originalComplexModelBinderProvider.GetBinder(context), 
                    context.MetadataProvider);
            }
            return null;
        }
    }
}
