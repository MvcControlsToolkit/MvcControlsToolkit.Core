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
    [HtmlTargetElement(TagName, Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class ImportSettingsTagHelper:TagHelper
    {
        internal const string contextName = "_father_context_";
        private const string ForAttributeName = "asp-for";
        private const string TagName = "import-settings";
        private const string ViewNameName = "view-name";

        [HtmlAttributeName(ViewNameName)]
        public string ViewName { get; set; }
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set;}

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private IHtmlHelper helper;
        public ImportSettingsTagHelper(IHtmlHelper helper)
        {
            this.helper = helper;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(ViewName)) new ArgumentNullException(ViewNameName);
            var rc = context.GetFatherReductionContext();
            output.TagName = string.Empty;
            (helper as IViewContextAware).Contextualize(ViewContext);
            var vd = new ViewDataDictionary<object>(ViewContext.ViewData);
            vd.Model = For.Model;
            vd[contextName] = rc;
            var res= await helper.PartialAsync(ViewName, For.Model, vd);
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(res);
        }
    }
}
