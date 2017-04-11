using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.OptionsParsing;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public enum TemplateShowType {Display=0, Edit=1, Filter = 2 }
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

        private bool cloned = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            
            if (Partial == null && ViewComponent == null && !cloned)
            {
                var clone = this.MemberwiseClone() as TemplateTagHelper;
                clone.cloned = true;
                 var fun = typeof(TagHelperOutput).GetTypeInfo()
                    .GetField("_getChildContentAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(output) as Func<bool, HtmlEncoder, Task<TagHelperContent>>;
                var exContext = fun.Target as TagHelperExecutionContext;
                var _startTagHelperWritingScope = typeof(TagHelperExecutionContext)
                    .GetField("_startTagHelperWritingScope", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(exContext) as Action<HtmlEncoder>;
                var _endTagHelperWritingScope = typeof(TagHelperExecutionContext)
                    .GetField("_endTagHelperWritingScope", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(exContext) as Func<TagHelperContent>;
                var _executeChildContentAsync = typeof(TagHelperExecutionContext)
                    .GetField("_executeChildContentAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(exContext) as Func<Task>;
                var newExContext = new TagHelperExecutionContext("asp-template", TagMode.StartTagAndEndTag, exContext.Items, context.UniqueId,
                    _executeChildContentAsync,
                    _startTagHelperWritingScope,
                    _endTagHelperWritingScope
                    );
                newExContext.TagHelpers.Add(exContext.TagHelpers[0]);
                newExContext.AddHtmlAttribute(context.AllAttributes["type"]);
                clone.Process(newExContext.Context, newExContext.Output);
                output.SuppressOutput();
                return;
            }
            var rc = context.GetFatherReductionContext();

            output.SuppressOutput();

            if (rc.RowParsingDisabled) return;
             
            if (rc.CurrentToken == TagTokens.Column)
            {

                
                rc.Results.Add(new ReductionResult(
                    TemplateType == TemplateShowType.Display ? TagTokens.DTemplate : (TemplateType == TemplateShowType.Edit ? TagTokens.ETemplate : TagTokens.ETemplate),
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
                    TemplateType == TemplateShowType.Display ? TagTokens.DTemplate : (TemplateType == TemplateShowType.Edit ? TagTokens.ETemplate : TagTokens.ETemplate),
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
