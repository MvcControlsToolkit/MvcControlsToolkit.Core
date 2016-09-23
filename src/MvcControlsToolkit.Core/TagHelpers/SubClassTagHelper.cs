using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.ModelBinding.DerivedClasses;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("subclass", Attributes = ForAttributeName+"," + SubClassTypeName)]
    public class SubClassTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string SubClassTypeName = "subclass-type";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(SubClassTypeName)]
        public Type SubClassType { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        private static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (SubClassType == null) throw new ArgumentNullException(SubClassTypeName);

            if (!For.Metadata.ModelType.GetTypeInfo().IsAssignableFrom(SubClassType))
                throw new ArgumentException(string.Format(DefaultMessages.NotASubclass, SubClassType.Name, For.Metadata.ModelType.Name), SubClassTypeName);

            if (For.Model != null && !SubClassType.GetTypeInfo().IsAssignableFrom(For.Model.GetType()))
            {
                output.TagName = string.Empty;
                output.Content.SetHtmlContent(string.Empty);
                return;
            }

            var prefix = DerivedClassesRegister.GetCodeFromType(SubClassType);
            prefix = combinePrefixes(For.Name, prefix);
            output.TagName = string.Empty;
            string childContent;
            using (var scope = new RenderingScope(For.Model, ViewContext.ViewData.GetFullHtmlFieldName(prefix), ViewContext.ViewData))
            {
                childContent = output.Content.IsModified ? output.Content.GetContent() :
                    (await output.GetChildContentAsync()).GetContent();

            }
            output.Content.SetHtmlContent(childContent);
        }

    }
}
