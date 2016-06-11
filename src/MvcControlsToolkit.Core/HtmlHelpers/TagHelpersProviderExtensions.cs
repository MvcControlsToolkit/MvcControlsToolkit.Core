using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.TagHelpers;
using System.Reflection;


namespace MvcControlsToolkit.Core.HtmlHelpers
{
    public static class TagHelpersProviderExtensions
    {
        public static HtmlString UseProvider(this IHtmlHelper h, Type providerType)
        {
            if (providerType == null) throw new ArgumentNullException(nameof(providerType));
            if (!typeof(ITagHelpersProvider).IsAssignableFrom(providerType)) throw new ArgumentException(nameof(providerType));
            var instance = h.ViewContext.HttpContext.RequestServices.GetService(providerType) as ITagHelpersProvider;
            if(instance == null) throw new ArgumentException(nameof(providerType));
            new TagHelpersProviderContext(instance, h.ViewContext);
            return new HtmlString(string.Empty);
        }

        public static HtmlString UnUseProvider(this IHtmlHelper h)
        {
            var pc = h.ViewContext.ViewData[TagHelpersProviderContext.Field] as TagHelpersProviderContext;
            if (pc != null && pc.HasPrevious) pc.Dispose();
            return new HtmlString(string.Empty);
        }
    }
}
