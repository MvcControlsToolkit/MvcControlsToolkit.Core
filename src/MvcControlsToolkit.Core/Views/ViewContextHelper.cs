﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using MvcControlsToolkit.Core.TagHelpers;
using MvcControlsToolkit.Core.TagHelpers.Providers;

namespace MvcControlsToolkit.Core.Views
{
    public static class ViewContextHelper
    {
        public async static Task<string> RenderPartialView(this ViewContext context, string viewName, ICompositeViewEngine viewEngine = null, ViewEngineResult viewResult = null)
        {
            viewEngine = viewEngine ?? context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            viewResult = viewResult ?? viewEngine.FindView(context, viewName, false);

            await viewResult.View.RenderAsync(context);

            return context.Writer.ToString();

        }
        public static string GetFullHtmlFieldName(this ViewDataDictionary viewData, string prefix)
        {
            var pres=viewData.TemplateInfo.GetFullHtmlFieldName(prefix);
            if (viewData[RenderingScope.Field] != null)
            {
                var index = pres.IndexOf('.');
                if (index < 0) return string.Empty;
                else return pres.Substring(index + 1);
            }
            else return pres;
        }
        public static ITagHelpersProvider TagHelperProvider(this ViewContext context )
        {
            var viewData = context.ViewData;
            ITagHelpersProvider res = viewData[TagHelpersProviderContext.Field] as ITagHelpersProvider;
            if(res == null)
            {
                viewData[TagHelpersProviderContext.Field]=res= context.HttpContext.RequestServices.GetService<DefaultTagHelpersProvider>();
            }
            return res;
        }
        public static bool GenerateNames(this ViewContext context)
        {
            return context.TagHelperProvider().GenerateNames;
        }


    }
}
