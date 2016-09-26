using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public enum TemplateShowType {Display=0, Edit=1 }
    [HtmlTargetElement("asp-template", Attributes = TypeName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TemplateTagHelper: TagHelper
    {
        private const string TypeName = "type";

        [HtmlAttributeName(TypeName)]
        public TemplateShowType TemplateType { get; set; }

        [HtmlAttributeName("use-partial")]
        public string Partial { get; set; }
        [HtmlAttributeName("use-view-component")]
        public string ViewComponent { get; set; }
        [HtmlAttributeName("use-view-component")]
        public Func<object,  object, ContextualizedHelpers, IHtmlContent> TemplateFunction { get; set; }
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }



        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var rc = context.GetFatherReductionContext();
            output.TagName = string.Empty;
            output.Content.SetHtmlContent(string.Empty);

            if (rc.RowParsingDisabled) return;
            
            if (rc.CurrentToken == TagTokens.Column)
            {

                rc.Results.Add(new ReductionResult(
                    TemplateType == TemplateShowType.Display ? TagTokens.DTemplate : TagTokens.ETemplate,
                    0,
                    Partial != null || ViewComponent != null ?
                    new Template<Column>(Partial != null ? TagHelpers.TemplateType.Partial : TagHelpers.TemplateType.ViewComponent,  
                        Partial??ViewComponent) 
                    :
                    new Template<Column>(TemplateFunction == null ? TagHelpers.TemplateType.InLine : TagHelpers.TemplateType.Function,
                        TemplateFunction != null ? TemplateFunction
                        :
                        (x, y, z) =>
                        {

                            
                            var t = output.GetChildContentAsync(false);
                            t.Wait();
                            return new HtmlString(t.Result.GetContent().ToString());
                        },
                        ViewContext
                    )));
            }
            else
            {
                rc.Results.Add(new ReductionResult(
                    TemplateType == TemplateShowType.Display ? TagTokens.DTemplate : TagTokens.ETemplate,
                    0,
                    Partial != null || ViewComponent != null ?
                    new Template<RowType>(Partial != null ? TagHelpers.TemplateType.Partial : TagHelpers.TemplateType.ViewComponent,
                        Partial ?? ViewComponent)
                    :
                    new Template<RowType>(TemplateFunction == null ? TagHelpers.TemplateType.InLine : TagHelpers.TemplateType.Function,
                        TemplateFunction != null ? TemplateFunction
                        :
                        (x, y, z) =>
                            {

                                
                                var t = output.GetChildContentAsync(false);
                                t.Wait();
                                return new HtmlString(t.Result.GetContent().ToString());
                            },
                        ViewContext
                    )));
            }
        }
    }
}
