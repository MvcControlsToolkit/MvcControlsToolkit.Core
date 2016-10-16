using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using MvcControlsToolkit.Core.Templates;

namespace WebTestCore.ViewComponents
{
    public class TestTemplate: ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(object model, Column options, string prefix, ModelStateDictionary modelState, IStringLocalizerFactory localizerFactory, ContextualizedHelpers helpers)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = prefix;
            ViewData["LocalizerFactory"] = localizerFactory;
            ViewData["ContextualizedHelpers"] = helpers;
            ModelState.Merge(modelState);
            return View(model);
        }
    }
}
