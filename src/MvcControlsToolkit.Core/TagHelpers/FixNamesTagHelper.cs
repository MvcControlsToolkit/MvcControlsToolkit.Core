using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.Views;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace MvcControlsToolkit.Core.TagHelpers
{
    
    public abstract class FixNamesTagHelperBase: TagHelper
    {
        private const string ForAttributeName = "asp-for";
        
        public FixNamesTagHelperBase(IOptions<MvcViewOptions> optionsAccessor)
        {
            IdAttributeDotReplacement = optionsAccessor.Value.HtmlHelperOptions.IdAttributeDotReplacement;
        }
        string IdAttributeDotReplacement;
        public const int order = int.MinValue + 5;
        public override int Order
        {
            get
            {
                return order;
            }
        }

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        [HtmlAttributeName("id")]
        public string Id { get; set; }

        private void copyAttributes(TagHelperOutput output)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                output.Attributes.Add("name", Name);
            }
            if (!string.IsNullOrWhiteSpace(Id))
            {
                output.Attributes.Add("id", Id);
            }
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            bool canHaveNames = ViewContext.GenerateNames();
            bool correctNames = ViewContext.ViewData[RenderingScope.Field] as RenderingScope != null;
            if (!correctNames)
            {
                copyAttributes(output);
                return;
            }
            else if (!canHaveNames)
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    output.Attributes.Add("name", Name);
                }
                else output.Attributes.Add("name", string.Empty);
                if (!string.IsNullOrWhiteSpace(Id))
                {
                    output.Attributes.Add("id", Id);
                }
                else output.Attributes.Add("Id", string.Empty);
            }
            else
            {
                string fullName = ViewContext.ViewData.GetFullHtmlFieldName(For.Name);
                if (string.IsNullOrWhiteSpace(Name))
                {
                    output.Attributes.Add("name", fullName);
                }
                else
                {
                    output.Attributes.Add("name", Name);
                }
                if (string.IsNullOrWhiteSpace(Id))
                {
                    output.Attributes.Add("id", TagBuilder.CreateSanitizedId(fullName, IdAttributeDotReplacement));
                }
                else
                {
                    output.Attributes.Add("id", Id);
                }
            }
        }
    }
    [HtmlTargetElement("input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public  class FixInputNamesTagHelper : FixNamesTagHelperBase
    {
        private const string ForAttributeName = "asp-for";

        public FixInputNamesTagHelper(IOptions<MvcViewOptions> optionsAccessor):base(optionsAccessor)
        {

        }
    }

    [HtmlTargetElement("select", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public  class FixSelectNamesTagHelper : FixNamesTagHelperBase
    {
        private const string ForAttributeName = "asp-for";

        public FixSelectNamesTagHelper(IOptions<MvcViewOptions> optionsAccessor) : base(optionsAccessor)
        {

        }
    }

    [HtmlTargetElement("textarea", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public  class FixTextAreaNamesTagHelper : FixNamesTagHelperBase
    {
        private const string ForAttributeName = "asp-for";

        public FixTextAreaNamesTagHelper(IOptions<MvcViewOptions> optionsAccessor) : base(optionsAccessor)
        {

        }
    }
}
