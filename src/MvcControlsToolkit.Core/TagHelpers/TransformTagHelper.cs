using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.Views;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using System.IO;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("scope", Attributes = ForAttributeName+","+ TransformationName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class TransformTagHelper: Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string TransformationName = "transformation";
        

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(TransformationName)]
        public IBindingTransformation Transformation { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("use-partial")]
        public string UsePartial { get; set; }

        private static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }

        public override async Task ProcessAsync (TagHelperContext context, TagHelperOutput output)
        {
            if (Transformation==null) throw new ArgumentNullException(TransformationName);
            Transformation.Context = ViewContext.HttpContext;
            var type = Transformation.GetType();

            var pres= type.GetMethod("Transform").Invoke(Transformation, new object[] { For.Model });

            var prefix = TransformationsRegister.GetPrefix(type);
            prefix = combinePrefixes(For.Name, prefix);
            output.TagName = string.Empty;
            string childContent;
            if (string.IsNullOrWhiteSpace(UsePartial))
            {
                using (var scope = new RenderingScope(pres, ViewContext.ViewData.GetFullHtmlFieldName(prefix), ViewContext.ViewData))
                {
                    childContent = output.Content.IsModified ? output.Content.GetContent() :
                        (await output.GetChildContentAsync()).GetContent();
                }
            }
            else
            {
                var sw = new StringWriter();
                var dict = new ViewDataDictionary(ViewContext.ViewData);
                dict.Model = pres;
                var newContext = new ViewContext(ViewContext, ViewContext.View, dict, ViewContext.TempData, sw, new HtmlHelperOptions());
                childContent = await newContext.RenderPartialView(UsePartial);
            }
            output.Content.SetHtmlContent(childContent);
        }


    }
}
