using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MvcControlsToolkit.Core.Views;

namespace MvcControlsToolkit.Core.TagHelpers
{
    //[HtmlTargetElement("input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    //public class Enhance1InputTagHelper : TagHelper
    //{
    //    private const string ForAttributeName = "asp-for";
    //    public override int Order
    //    {
    //        get
    //        {
    //            return -100000;
    //        }
    //    }
    //    public override void Process(TagHelperContext context, TagHelperOutput output)
    //    {
    //        var i = context.AllAttributes;
    //        var o = output.Attributes;
    //    }
    //}
    
    [HtmlTargetElement("input", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class EnhanceInputTagHelper: TagHelper 
    {
        private const string ForAttributeName = "asp-for";
        private static string[] positiveIntegerTypes = new string[] {nameof(Byte).ToLowerInvariant(), nameof(UInt16).ToLowerInvariant(), nameof(UInt32).ToLowerInvariant(), nameof(UInt64).ToLowerInvariant() };
        private static string[] integerTypes = new string[] { nameof(SByte).ToLowerInvariant(), nameof(Int16).ToLowerInvariant(), nameof(Int32).ToLowerInvariant(), nameof(Int64).ToLowerInvariant() };
        public const int order = int.MinValue + 10;
        public override int Order
        {
            get
            {
                return order;
            }
        }
        [HtmlAttributeName("type")]
        public string InputTypeName { get; set; }

        [HtmlAttributeName("min")]
        public string Min { get; set; }

        [HtmlAttributeName("max")]
        public string Max { get; set; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("value")]
        public string Value { get; set; }

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }
        private string coverter(object value, string hint)
        {
            if (value is DateTime)
            {
                DateTime dValue = (DateTime)value;
                if (hint == "date") return string.Format("{0:00}-{1:00}-{2:00}", dValue.Year, dValue.Month, dValue.Day);
                else if (hint == "time") return string.Format("{0:00}:{1:00}:{2:00}", dValue.Hour, dValue.Minute, dValue.Second);
                else return string.Format("{0:00}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}", dValue.Year, dValue.Month, dValue.Day, dValue.Hour, dValue.Minute, dValue.Second);

            }
            else if (value is TimeSpan)
            {
                TimeSpan dValue = (TimeSpan)value;
                return string.Format("{0:00}:{1:00}:{2:00}", dValue.Hours, dValue.Minutes, dValue.Seconds);
            }
            else if (value is IConvertible) return (value as IConvertible).ToString(CultureInfo.InvariantCulture);
            else return value.ToString();
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var modelExplorer = For.ModelExplorer;
            InputTypeName = InputTypeName == null ? null : InputTypeName.Trim().ToLowerInvariant();
            string min = string.IsNullOrEmpty(Min)? null : Min, max = string.IsNullOrEmpty(Max)? null : Max;
            
            var metaData = modelExplorer.Metadata;
            var typeName = modelExplorer.Metadata.UnderlyingOrModelType.Name.ToLowerInvariant();
            var hint = (metaData.DataTypeName ?? metaData.TemplateHint)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(InputTypeName) && !output.Attributes.ContainsName("type"))
            {
                string type=null;
                if (hint == "color") type = hint;
                else if (typeName == "single" || typeName == "double" || typeName == "decimal") type = "number";
                else if (typeName == "week" || typeName == "month") type = typeName;
                else if (typeName == "datetime" && string.IsNullOrEmpty(hint)) type = "datetime-local";

                if (type != null)
                {
                    
                    output.Attributes.Add("type", type);
                }
                
            }
            bool isDecimal = typeName == "single" || typeName == "double" || typeName == "decimal";
            
            bool isNumber = (string.IsNullOrEmpty(InputTypeName) || InputTypeName == "number" || InputTypeName == "range");
            bool isPositive = positiveIntegerTypes.Contains(typeName);
            bool isIntegerNP = integerTypes.Contains(typeName);
            
            bool isHtml5DateTime = (string.IsNullOrEmpty(InputTypeName) || InputTypeName == "date" || InputTypeName == "datetime" || InputTypeName == "datetime-local" || InputTypeName == "week" || InputTypeName == "month");
            bool isDateTimeType = typeName == "datetime" || typeName == "timespan" || typeName == "week" || typeName == "month";
            object minimum=null, maximum=null;
            RangeAttribute limits = metaData.ValidatorMetadata.Where(m => m is RangeAttribute).FirstOrDefault() as RangeAttribute;
            if (limits != null)
            {
                minimum = limits.Minimum;
                maximum = limits.Maximum;
            }
            else {
                DynamicRangeAttribute limitsExt = metaData.ValidatorMetadata.Where(m => m is DynamicRangeAttribute).FirstOrDefault() as DynamicRangeAttribute;
                if(limitsExt != null)
                {
                    List<string> a, b;
                    maximum=limitsExt.GetGlobalMaximum(modelExplorer.Container.Model, out a, out b);
                    minimum= limitsExt.GetGlobalMinimum(modelExplorer.Container.Model, out a, out b);
                }
            }

            if (string.IsNullOrWhiteSpace(Value) && ((isNumber && (isPositive || isIntegerNP || isDecimal)) ||(isHtml5DateTime && isDateTimeType)))

            {
                string value = modelExplorer.Model == null ? null : coverter(modelExplorer.Model, hint);
                if (!string.IsNullOrEmpty(value))
                {
                    if (!output.Attributes.ContainsName("value"))
                        output.Attributes.Add("value", value);
                }
            }
            if (typeName == "week")
            {
                string value = modelExplorer.Model == null ? "" : ((Week)modelExplorer.Model).StartDate().ToString("yyyy-MM-dd");

                output.Attributes.Add("data-date-value",  value);
            }
            if (minimum != null || maximum != null)
            {
                if (min == null && minimum != null)
                {
                    if (isNumber && isPositive)
                    {

                        var trueMin = ((minimum is string) ? Convert.ChangeType(minimum, limits.OperandType, CultureInfo.InvariantCulture) : minimum) as IComparable;
                        if (trueMin.CompareTo(Convert.ChangeType("0", trueMin.GetType())) < 0) min = "0";
                        else min = (trueMin as IFormattable).ToString(null, CultureInfo.InvariantCulture);


                    }
                    else if (isNumber && (isIntegerNP || isDecimal))
                    {
                        min = (minimum is string) ? minimum as string : (minimum as IConvertible).ToString(CultureInfo.InvariantCulture);
                    }
                    else if(isHtml5DateTime && isDateTimeType)
                    {
                        min = (minimum is string) ? minimum as string : (minimum is IConvertible ? coverter(minimum, hint) : minimum.ToString());
                    }
                }
                if (max == null && maximum != null)
                {
                    if (isNumber && (isPositive || isIntegerNP || isDecimal))
                        max = (maximum is string) ? maximum as string : (maximum as IConvertible).ToString(CultureInfo.InvariantCulture);
                    else if(isHtml5DateTime && isDateTimeType)
                    {
                        max = (maximum is string) ? maximum as string : (maximum is IConvertible ? coverter(maximum, hint) : maximum.ToString());
                    }
                }
            }

            if (isNumber && isPositive && min == null)
            {
                min = "0";
            }
            if (min != null)
            {
                
                if (output.Attributes.ContainsName("min")) output.Attributes.Remove(output.Attributes["min"]);
                output.Attributes.Add("min", min);

            }
            if (max != null)
            {
                if (output.Attributes.ContainsName("max")) output.Attributes.Remove(output.Attributes["max"]);
                output.Attributes.Add("max", max);
            }
            if(InputTypeName == "range")
            {
                var fullName = ViewContext.ViewData.GetFullHtmlFieldName(For.Name);
                fullName = fullName.Length > 0 ? fullName + "._" : "_";
                output.PostElement.AppendHtml("<input name='"+ fullName + "' type='hidden'/>");
            }
            
        }
    }
}
