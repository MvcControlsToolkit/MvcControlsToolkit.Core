using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.Extensions;
using MvcControlsToolkit.Core.TagHelpers;
using MvcControlsToolkit.Core.TagHelpers.Providers;

namespace MvcControlsToolkit.Core.Views
{
    public class CustomContext<T>: IDisposable
    {
        private Action<CustomContext<T>> off;
        private T payLoad;
        public T Data { get { return payLoad; } }
        public CustomContext(T payLoad, Action on, Action<CustomContext<T>> off)
        {
            if (on == null) throw new ArgumentNullException(nameof(on));
            if (off == null) throw new ArgumentNullException(nameof(off));
            this.off = off;
            this.payLoad = payLoad;
            on();
        }

        public void Dispose()
        {
            off(this);
        }
    }
    public static class ViewContextHelper
    {
        
        private const string filterMode = "_FilterMode_On";
        public async static Task<string> RenderPartialView(this ViewContext context, string viewName, ICompositeViewEngine viewEngine = null, ViewEngineResult viewResult = null)
        {
            viewEngine = viewEngine ?? context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            viewResult = viewResult ?? viewEngine.FindView(context, viewName, false);

            await viewResult.View.RenderAsync(context);

            return context.Writer.ToString();

        }
        public static string GetFullHtmlFieldName(this ViewDataDictionary viewData, string prefix)
        {
            var pres=prefix;
            if (!string.IsNullOrEmpty(pres) && !char.IsUpper(pres[0]) && viewData[RenderingScope.Field] != null)
            {
                var index = pres.IndexOf('.');
                if (index < 0) return viewData.TemplateInfo.GetFullHtmlFieldName(string.Empty);
                else
                {
                    
                    return viewData.TemplateInfo.GetFullHtmlFieldName(pres.Substring(index + 1));
                }
            }
            else return viewData.TemplateInfo.GetFullHtmlFieldName(pres);
        }
        public static ITagHelpersProvider TagHelperProvider(this ViewContext context )
        {
            var viewData = context.ViewData;
            ITagHelpersProvider res = viewData[TagHelpersProviderContext.Field] as ITagHelpersProvider;
            if(res == null)
            {
                viewData[TagHelpersProviderContext.Field]=res= MvcControlsToolkitOptions.Instance.DefaultProvider;
            }
            return res;
        }
        public static bool GenerateNames(this ViewContext context)
        {
            return context.TagHelperProvider().GenerateNames;
        }

        public static CustomContext<bool> FilterRendering(this ViewContext context)
        {
            return new CustomContext<bool>(
                context.ClientValidationEnabled,
                () =>
                {
                    context.ClientValidationEnabled = false;
                    context.HttpContext.Items[filterMode] = true;
                },
                (x) =>
                {
                    context.HttpContext.Items.Remove(filterMode);
                    context.ClientValidationEnabled = x.Data;
                }
                );
        }
        public static bool IsFilterRendering(this ViewContext context)
        {
            return context.HttpContext.Items.ContainsKey(filterMode);
        }
     }
}
