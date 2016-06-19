using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.Views;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
                var prov = ViewContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
                var dict = new ViewDataDictionary(prov, ViewContext.ViewData.ModelState);
                dict.Model = pres;
                dict.TemplateInfo.HtmlFieldPrefix = ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(prefix);
                dict[TagHelpersProviderContext.Field] = ViewContext.ViewData[TagHelpersProviderContext.Field];
                var newContext = new ViewContext(ViewContext, ViewContext.View, dict, ViewContext.TempData, sw, 
                    new HtmlHelperOptions {
                        Html5DateRenderingMode =ViewContext.Html5DateRenderingMode,
                        ClientValidationEnabled= ViewContext.ClientValidationEnabled,
                        ValidationMessageElement = ViewContext.ValidationMessageElement,
                        ValidationSummaryMessageElement= ViewContext.ValidationSummaryMessageElement
                    });
                childContent = await newContext.RenderPartialView(UsePartial);
            }
            output.Content.SetHtmlContent(childContent);
        }


    }
}
