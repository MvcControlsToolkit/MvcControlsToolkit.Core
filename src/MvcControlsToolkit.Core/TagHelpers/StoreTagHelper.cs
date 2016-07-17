 using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcControlsToolkit.Core.Views;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using System;
using MvcControlsToolkit.Core.Transformations;
using System.Reflection;

namespace MvcControlsToolkit.Core.TagHelpers
{
    [HtmlTargetElement("store-model", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class StoreTagHelper: TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string EncryptedName = "encrypted";

        public StoreTagHelper(IOptions<MvcViewOptions> optionsAccessor)
        {
            IdAttributeDotReplacement = optionsAccessor.Value.HtmlHelperOptions.IdAttributeDotReplacement;
        }
        string IdAttributeDotReplacement;

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(EncryptedName)]
        public bool Encrypted { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        [HtmlAttributeName("id")]
        public string Id { get; set; }
        private static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
           bool canHaveNames = ViewContext.GenerateNames();
            output.TagName = "input";
            output.Attributes.Add("type", "hidden");
            Type type = Encrypted ? typeof(EncryptedJsonTransformation<>) : typeof(JsonTransformation<>);
            type = type.MakeGenericType(new Type[] { For.ModelExplorer.ModelType });
            IBindingTransformation trasf = Activator.CreateInstance(type) as IBindingTransformation;
            trasf.Context = ViewContext.HttpContext;
            string res = type.GetMethod("Transform").Invoke(trasf, new object[] { For.Model }) as string;
            output.Attributes.Add("value", res);
            if (!canHaveNames)
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
                
                string name = combinePrefixes(For.Name, TransformationsRegister.GetPrefix(type));
                string fullName = ViewContext.ViewData.GetFullHtmlFieldName(name);
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
            base.Process(context, output);
        }
    }
}
