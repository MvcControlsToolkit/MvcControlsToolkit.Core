using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace MvcControlsToolkit.Core.ModelBinding
{
    public class SimpleTypeModelBinderProviderExt : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.IsComplexType)
            {
                return new SimpleTypeModelBinderExt(context.Metadata.ModelType);
            }

            return null;
        }
    }
}
