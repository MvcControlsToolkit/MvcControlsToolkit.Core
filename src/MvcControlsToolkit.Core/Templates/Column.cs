using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MvcControlsToolkit.Core.TagHelpers;
using MvcControlsToolkit.Core.DataAnnotations;
using System.Collections.Concurrent;
using System.Reflection;

namespace MvcControlsToolkit.Core.Templates
{
    public class Column
    {
        protected static IDictionary<KeyValuePair<Type, string>, ColumnLayoutAttribute> Layouts = new ConcurrentDictionary<KeyValuePair<Type, string>, ColumnLayoutAttribute>();
        public RowType Row { get; internal set; }
        public ModelExpression For { get; private set; }
        public bool IsDetail { get;  set; }
        public Template<Column> EditTemplate { get; set; }
        public Template<Column> DisplayTemplate { get; set; }
        public string ColumnTitle { get; set; }
        public bool? Hidden { get; set; }
        public bool ReadOnly { get; set; }
        public bool EditOnly { get; set; }
        public string DisplayFormat { get; set; }
        public string PlaceHolder { get; set; }
        public string NullDisplayText { get; set; }
        public string Description { get; set; }
        public decimal[] Widths { get; set; }
        public int[] DisplayDetailWidths { get; set; }
        public int[] EditDetailWidths { get; set; }
        public string ColumnCssClass { get; set; }
        public int? Order { get; set; }
        public int NaturalOrder { get; set; }
        public string AdditionalPrefix { get; set; }
        public int? ColSpan { get; set; }
        public string InputCssClass { get; set; }
        public string CheckboxCssClass { get; set; }

        public ColumnConnectionInfos ColumnConnection { get; set; }

    protected bool prepared;
        private TypeInfo _TypeInfos = null;
        public TypeInfo TypeInfos
        {
            get
            {
                if (_TypeInfos == null) _TypeInfos = For.Metadata.ModelType.GetTypeInfo();
                return _TypeInfos;
            }
        }
        public decimal GetWidth(int i)
        {
            if (i < 0) i = 0;
            if (Widths == null || Widths.Length == 0) return 100;
            if (i >= Widths.Length) i = Widths.Length - 1;
            return Widths[i];
        }
        public Column(ModelExpression expression, Template<Column> displayTemplate, Template<Column> editTemplate =null, bool isDetail=false)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            For = expression;
            DisplayTemplate = displayTemplate;
            EditTemplate = editTemplate;
            
        }
        protected static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        public void Prepare()
        {
            if (prepared) return;
            var metaData = For.Metadata;
            if (!Hidden.HasValue) Hidden = For.Metadata.HideSurroundingHtml;
            if (!Order.HasValue) Order = For.Metadata.Order;
            if (string.IsNullOrEmpty(ColumnTitle)) ColumnTitle = metaData.GetDisplayName();
            if (string.IsNullOrEmpty(Description)) Description = metaData.Description?? ColumnTitle;
            if (string.IsNullOrEmpty(PlaceHolder)) PlaceHolder = metaData.Placeholder;
            if (ColumnConnection!= null)
            {
                var infos = ColumnConnection.DisplayProperty.Metadata;
                if (string.IsNullOrEmpty(DisplayFormat)) DisplayFormat = infos.DisplayFormatString;
                if (string.IsNullOrEmpty(NullDisplayText)) NullDisplayText = infos.NullDisplayText;
            }
            else
            {
                if (string.IsNullOrEmpty(DisplayFormat)) DisplayFormat = metaData.DisplayFormatString;
                if (string.IsNullOrEmpty(NullDisplayText)) NullDisplayText = metaData.NullDisplayText;
            }
            
            if(Widths == null)
            {
                var pair = new KeyValuePair<Type, string>(Row.For.Metadata.ModelType, For.Metadata.PropertyName);
                ColumnLayoutAttribute res;
                if (Layouts.TryGetValue(pair, out res)){
                    res = Row.For.Metadata.ModelType.GetTypeInfo().GetProperty(For.Metadata.PropertyName)
                        .GetCustomAttribute(typeof(ColumnLayoutAttribute), false) as ColumnLayoutAttribute;
                    Layouts[pair] = res;
                    if (res != null) res.Prepare();
                    
                 }
                if (res != null) Widths = IsDetail ? res.DetailWidths : res.Widths;
                
            }
            prepared = true;
        } 
        public IHtmlContent InvokeEdit(object o, ContextualizedHelpers helpers)
        {
            if (EditTemplate == null) return new HtmlString(string.Empty);
            return EditTemplate.Invoke(new ModelExpression(
                combinePrefixes(AdditionalPrefix, For.Name), For.ModelExplorer.GetExplorerForModel(o)), 
                this, helpers);
        }
        public IHtmlContent InvokeDisplay(object o, ContextualizedHelpers helpers)
        {
            if (DisplayTemplate == null) return new HtmlString(string.Empty);
            return DisplayTemplate.Invoke(
                new ModelExpression(combinePrefixes(AdditionalPrefix, For.Name), For.ModelExplorer.GetExplorerForModel(o)), 
                this, helpers);
        }
        public IHtmlContent InvokeEdit(ContextualizedHelpers helpers, ModelExpression expression)
        {
            if (EditTemplate == null) return new HtmlString(string.Empty);
            return EditTemplate.Invoke(expression, this, helpers);
        }
        public IHtmlContent InvokeDisplay(ContextualizedHelpers helpers, ModelExpression expression)
        {
            if (DisplayTemplate == null) return new HtmlString(string.Empty);
            return DisplayTemplate.Invoke(expression, this, helpers);
        }
    }
}
