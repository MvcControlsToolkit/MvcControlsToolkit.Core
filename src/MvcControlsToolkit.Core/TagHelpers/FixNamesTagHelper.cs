using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.Views;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MvcControlsToolkit.Core.Validation;

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

        public  override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var prov = ViewContext.TagHelperProvider();
            bool canHaveNames = prov.GenerateNames;
            bool correctNames = ViewContext.ViewData[RenderingScope.Field] as RenderingScope != null;
            bool filtering = ViewContext.IsFilterRendering();


            prov?.InputProcess?.Invoke(context, output, this);
            if (!canHaveNames)
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    output.Attributes.Add("name", Name);
                }
                else output.Attributes.Add("name", null);
                if (!string.IsNullOrWhiteSpace(Id))
                {
                    output.Attributes.Add("id", Id);
                }
                else output.Attributes.Add("Id", null);
            }
            else if (!correctNames)
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    output.Attributes.Add("name", Name);
                }
                if (!string.IsNullOrWhiteSpace(Id))
                {
                    output.Attributes.Add("id", Id);
                }
                else if (filtering)
                {
                    output.Attributes.Add("id", null);
                }
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
                
                
                
                if (!string.IsNullOrWhiteSpace(Id))
                {
                    output.Attributes.Add("id", Id);
                        
                }
                else if (filtering)
                {
                    output.Attributes.Add("id", null);
                }
                else
                {
                    output.Attributes.Add("id", TagBuilder.CreateSanitizedId(fullName, IdAttributeDotReplacement));
                }
                
            }
            if(filtering )
            {
                
                if (prov.RequireUnobtrusiveValidation)
                {
                    var toAdd = TypeClientModelValidator.GetAttributes(For.Metadata);
                    foreach (var pair in toAdd) output.Attributes.Add(pair.Key, pair.Value);
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

    [HtmlTargetElement("select", Attributes = ForAttributeName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class FixSelectNamesTagHelper : FixNamesTagHelperBase
    {
        private const string ForAttributeName = "asp-for";
        private const string OptionsListForName = "asp-options-for";
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression OptionsListFor { get; set; }
        
        public FixSelectNamesTagHelper(IOptions<MvcViewOptions> optionsAccessor) : base(optionsAccessor)
        {

        }
        
    }
    
    [HtmlTargetElement("textarea", Attributes = ForAttributeName, TagStructure = TagStructure.NormalOrSelfClosing)]
    public  class FixTextAreaNamesTagHelper : FixNamesTagHelperBase
    {
        private const string ForAttributeName = "asp-for";

        public FixTextAreaNamesTagHelper(IOptions<MvcViewOptions> optionsAccessor) : base(optionsAccessor)
        {

        }
        
    }
}
