using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public interface ITagHelpersProvider
    {
        bool GenerateNames { get; }
        bool RequireUnobtrusiveValidation { get; }

        void PrepareViewContext(ViewContext context);

        void UnPrepareViewContext(ViewContext context);

        Action<TagHelperContext, TagHelperOutput, TagHelper> InputProcess { get; }

        Func<TagHelperContext, TagHelperOutput, TagHelper, TagProcessorOptions,  ContextualizedHelpers, Task> GetTagProcessor(string tagName);

         DefaultTemplates GetDefaultTemplates(string tagName);

        IHtmlContent RenderButton(StandardButtons buttonType, string arguments, string cssClass, ContextualizedHelpers helpers, IStringLocalizer localizer, bool visibleText=false, bool isSubmit=false);
    }
}
