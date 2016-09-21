using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers.Providers
{
    public class DefaultTagHelpersProvider : ITagHelpersProvider
    {
        public bool GenerateNames
        {
            get
            {
                return true;
            }

            
        }

        public Action<TagHelperContext, TagHelperOutput, TagHelper> InputProcess
        {
            get
            {
                return null;
            }
        }

        public bool RequireUnobtrusiveValidation
        {
            get
            {
                return true;
            }


        }

        public DefaultTemplates GetDefaultTemplates(string tagName)
        {
            return null;
        }

        public Func<TagHelperContext, TagHelperOutput, TagHelper, TagProcessorOptions, ContextualizedHelpers, Task> GetTagProcessor(string tagName)
        {
            return null;
        }

        public void PrepareViewContext(ViewContext context)
        {
            
        }

        public IHtmlContent RenderButton(StandardButtons buttonType, string arguments, string cssClass, ContextualizedHelpers helpers, IStringLocalizer localizer, bool visibleText=false)
        {
            return new HtmlString(string.Empty);
        }

        public void UnPrepareViewContext(ViewContext context)
        {
            
        }
    }
}
