using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace MvcControlsToolkit.Core.ModelBinding
{
    public class TransformationModelBinderProvider : IModelBinderProvider
    {
        protected IModelBinderProvider originalComplexModelBinderProvider;
        public TransformationModelBinderProvider(IModelBinderProvider originalBinder)
        {
            this.originalComplexModelBinderProvider = originalBinder;
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType)
            {
                return new TransformationModelBinder(context.CreateBinder, originalComplexModelBinderProvider.GetBinder(context));
            }
            return null;
        }
    }
}
