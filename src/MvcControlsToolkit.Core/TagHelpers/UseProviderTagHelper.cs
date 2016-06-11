using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Reflection;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("use-provider", Attributes = ProviderTypeName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class UseProviderTagHelper: Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
    {
        private const string ProviderTypeName = "provider-type";

        [HtmlAttributeName(ProviderTypeName)]
        public Type ProviderType { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (ProviderType == null) throw new ArgumentNullException(nameof(ProviderType));
            if (!typeof(ITagHelpersProvider).IsAssignableFrom(ProviderType)) throw new ArgumentException(nameof(ProviderType));
            var instance = ViewContext.HttpContext.RequestServices.GetService(ProviderType) as ITagHelpersProvider;
            if (instance == null) throw new ArgumentException(nameof(ProviderType));
            string childContent;
            output.TagName = string.Empty;
            using (var providerContext = new TagHelpersProviderContext(instance, ViewContext))
            {
                childContent = output.Content.IsModified ? output.Content.GetContent() :
                        (await output.GetChildContentAsync()).GetContent();
            }
            output.Content.SetHtmlContent(childContent);
        }
   }
}
