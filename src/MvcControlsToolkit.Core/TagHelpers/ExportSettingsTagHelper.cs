using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement(TagName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ExportSettingsTagHelper:TagHelper
    {
        private const string TagName = "export-settings";
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            
            var rc = ViewContext.ViewData[ImportSettingsTagHelper.contextName] as ReductionContext;
            output.TagName = string.Empty;
            if(rc != null) context.SetChildrenReductionContext(rc);
            var res= await output.GetChildContentAsync();
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(res);
        }
    }
}
