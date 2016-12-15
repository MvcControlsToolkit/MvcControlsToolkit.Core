using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.TagHelpers.Providers;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public class TagHelpersProviderContext: IDisposable
    {
        private  const string field = "__current_provider__";
        internal static string Field { get { return field; } }
        public ITagHelpersProvider Current { get { return provider; } }
        public bool HasPrevious {get { return oldProviderContext != null; } } 
        ITagHelpersProvider provider;
        TagHelpersProviderContext oldProviderContext;
        ViewContext context;
        public TagHelpersProviderContext(ITagHelpersProvider provider)
        {
            this.provider = provider;
        }
        public TagHelpersProviderContext(ITagHelpersProvider provider, ViewContext context)
        {
            this.provider = provider;
            this.context = context;
            oldProviderContext = context.ViewData[field] as TagHelpersProviderContext ??
                new TagHelpersProviderContext(context.HttpContext.RequestServices.GetService<DefaultTagHelpersProvider>());
            if (oldProviderContext == provider) return;
            oldProviderContext.provider.UnPrepareViewContext(context);
            context.ViewData[field] = this;
            context.ClientValidationEnabled = provider.RequireUnobtrusiveValidation;
            provider.PrepareViewContext(context);
        }

        

        public void Dispose()
        {
            if (oldProviderContext == provider) return;
            provider.UnPrepareViewContext(context);
            context.ViewData[field]  = oldProviderContext;
            context.ClientValidationEnabled = oldProviderContext.provider.RequireUnobtrusiveValidation;
            oldProviderContext.provider.PrepareViewContext(context);
        }
    }
}
