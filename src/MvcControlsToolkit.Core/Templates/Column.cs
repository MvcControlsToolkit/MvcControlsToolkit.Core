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
using System.Text;
using System.Globalization;

namespace MvcControlsToolkit.Core.Templates
{
    public enum SupportedGridSystems {Bootstrap3, Bootstrap4 };
    public class Column
    {
        protected static IDictionary<KeyValuePair<Type, string>, ColumnLayoutAttribute> Layouts = new ConcurrentDictionary<KeyValuePair<Type, string>, ColumnLayoutAttribute>();
        protected static IDictionary<KeyValuePair<Type, string>, QueryAttribute> QueryOptionsDictionary = new ConcurrentDictionary<KeyValuePair<Type, string>, QueryAttribute>();
        public RowType Row { get; internal set; }
        public ModelExpression For { get; private set; }
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
        public decimal[] DetailWidths { get; set; }
        public int[] DisplayDetailWidths { get; set; }
        public int[] EditDetailWidths { get; set; }
        public string ColumnCssClass { get; set; }
        public int? Order { get; set; }
        public int NaturalOrder { get; set; }
        public string AdditionalPrefix { get; set; }
        public int? ColSpan { get; set; }
        public string InputCssClass { get; set; }
        public string CheckboxCssClass { get; set; }
        public QueryOptions? Queries { get; set;}
        private string name;
        public string Name {get
            {
                return For == null ? name : For.Metadata.PropertyName;
            } }
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
            if (DetailWidths == null || DetailWidths.Length == 0) return 100;
            if (i >= DetailWidths.Length) i = DetailWidths.Length - 1;
            return DetailWidths[i];
        }
        public Column(ModelExpression expression, Template<Column> displayTemplate, Template<Column> editTemplate =null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            For = expression;
            DisplayTemplate = displayTemplate;
            EditTemplate = editTemplate;
            
        }
        public Column(string name, Template<Column> displayTemplate, Template<Column> editTemplate = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            this.name = name;
            DisplayTemplate = displayTemplate;
            EditTemplate = editTemplate;

        }
        protected static string combinePrefixes(string p1, string p2)
        {
            return (string.IsNullOrEmpty(p1) ? p2 : (string.IsNullOrEmpty(p2) ? p1 : p1 + "." + p2));

        }
        internal Column CopyBasic()
        {
            var res = new Column(For, null);
            res.Hidden = Hidden;
            if (prepared)
            {
                res.Order = Order;
                res.Widths = Widths;
                res.DetailWidths = DetailWidths;
                res.ColumnTitle = ColumnTitle;
                res.Description = Description ?? res.ColumnTitle;
                res.PlaceHolder = PlaceHolder;
                res.DisplayFormat = DisplayFormat;
                res.NullDisplayText = NullDisplayText;
                res.prepared = true;
            }
            return res;
        }
        public string GetTotalClass(SupportedGridSystems gs, bool edit)
        {
            string[] allStyles;
            
            var cssClass = ColumnCssClass ?? "form-group";
            
            var currWidths = edit ? EditDetailWidths : DisplayDetailWidths;
            if (currWidths != null && currWidths.Length > 0)
            {
                if (gs == SupportedGridSystems.Bootstrap3)
                    allStyles = new string[] { "col-xs-", "col-sm-", "col-md-", "col-lg-" };
                else if (gs == SupportedGridSystems.Bootstrap4)
                    allStyles = new string[] { "col-xs-", "col-sm-", "col-md-", "col-lg-", ".col-xl-" };
                else throw new NotImplementedException();
                var toBuild = new StringBuilder();
                toBuild.Append(cssClass);
                for (int i = 0; i < Math.Min(allStyles.Length, currWidths.Length); i++)
                {
                    if (currWidths[i] == 0 || (i != 0 && currWidths[i] == currWidths[i - 1]))
                    {
                        continue;
                    }
                    toBuild.Append(" ");
                    toBuild.Append(allStyles[i]);
                    toBuild.Append(currWidths[i].ToString(CultureInfo.InvariantCulture));

                }
                cssClass = toBuild.ToString();
            }
            else cssClass += " col-xs-12";
            return cssClass;
        }
        public void Prepare()
        {
            if (prepared) return;
            if (For == null)
            {
                if (!Hidden.HasValue) Hidden = false;
                    prepared = true;
                return;
            }
            var metaData = For.Metadata;
            if (!Hidden.HasValue) Hidden = For.Metadata.HideSurroundingHtml;
            if (!Order.HasValue) Order = For.Metadata.Order;
            
            if (ColumnConnection!= null)
            {
                var infos = ColumnConnection.DisplayProperty.Metadata;
                if (string.IsNullOrEmpty(ColumnTitle)) ColumnTitle = infos.GetDisplayName();
                if (string.IsNullOrEmpty(Description)) Description = infos.Description ?? ColumnTitle;
                if (string.IsNullOrEmpty(PlaceHolder)) PlaceHolder = infos.Placeholder;
                if (string.IsNullOrEmpty(DisplayFormat)) DisplayFormat = infos.DisplayFormatString;
                if (string.IsNullOrEmpty(NullDisplayText)) NullDisplayText = infos.NullDisplayText;
            }
            else
            {
                if (string.IsNullOrEmpty(ColumnTitle)) ColumnTitle = metaData.GetDisplayName();
                if (string.IsNullOrEmpty(Description)) Description = metaData.Description ?? ColumnTitle;
                if (string.IsNullOrEmpty(PlaceHolder)) PlaceHolder = metaData.Placeholder;
                if (string.IsNullOrEmpty(DisplayFormat)) DisplayFormat = metaData.DisplayFormatString;
                if (string.IsNullOrEmpty(NullDisplayText)) NullDisplayText = metaData.NullDisplayText;
            }
            
            if(Widths == null || DetailWidths == null)
            {
                var pair = new KeyValuePair<Type, string>(Row.For.Metadata.ModelType, For.Metadata.PropertyName);
                ColumnLayoutAttribute res;
                if (!Layouts.TryGetValue(pair, out res)){
                    res = Row.For.Metadata.ModelType.GetTypeInfo().GetProperty(For.Metadata.PropertyName)
                        .GetCustomAttribute(typeof(ColumnLayoutAttribute), false) as ColumnLayoutAttribute;
                    Layouts[pair] = res;
                    if (res != null) res.Prepare();
                    
                 }
                if (res != null)
                {
                    if(Widths==null)
                        Widths =  res.Widths;
                    if(DetailWidths == null)
                        DetailWidths = res.DetailWidths;
                }
                
            }
            if (Row.QueryEnabled.HasValue && Row.QueryEnabled.Value)
            {
                string propertyName = ColumnConnection != null && ColumnConnection.QueryDisplay ?
                    ColumnConnection.DisplayProperty.Metadata.PropertyName :
                    For.Metadata.PropertyName;

                Type propertyType = ColumnConnection != null && ColumnConnection.QueryDisplay ?
                    ColumnConnection.DisplayProperty.Metadata.ModelType :
                    For.Metadata.ModelType;

                var pair = new KeyValuePair<Type, string>(Row.For.Metadata.ModelType, propertyName);
                QueryAttribute res;
                if (!QueryOptionsDictionary.TryGetValue(pair, out res))
                {
                    res = Row.For.Metadata.ModelType.GetTypeInfo().GetProperty(For.Metadata.PropertyName)
                        .GetCustomAttribute(typeof(QueryAttribute), false) as QueryAttribute;
                    QueryOptionsDictionary[pair] = res;
                    

                }
                if (res != null)
                {
                    if (Queries.HasValue) Queries = res.AllowedForProperty(Queries.Value, propertyType);
                    else Queries = res.AllowedForProperty(propertyType);
                }
                else  Queries = QueryOptions.None;
            }
            else Queries = QueryOptions.None;
            prepared = true;
        } 
        public async Task<IHtmlContent> InvokeEdit(object o, ContextualizedHelpers helpers, string overridePrefix=null)
        {
            if (EditTemplate == null) return new HtmlString(string.Empty);
            return await EditTemplate.Invoke(new ModelExpression(
                combinePrefixes(AdditionalPrefix, For.Name), For.ModelExplorer.GetExplorerForModel(o)), 
                this, helpers, overridePrefix);
        }
        public async Task<IHtmlContent> InvokeDisplay(object o, ContextualizedHelpers helpers, string overridePrefix = null)
        {
            if (DisplayTemplate == null) return new HtmlString(string.Empty);
            return await DisplayTemplate.Invoke(
                new ModelExpression(combinePrefixes(AdditionalPrefix, For.Name), For.ModelExplorer.GetExplorerForModel(o)), 
                this, helpers, overridePrefix);
        }
        public async Task<IHtmlContent> InvokeEdit(ContextualizedHelpers helpers, ModelExpression expression)
        {
            if (EditTemplate == null) return new HtmlString(string.Empty);
            return await EditTemplate.Invoke(expression, this, helpers);
        }
        public async Task<IHtmlContent> InvokeDisplay(ContextualizedHelpers helpers, ModelExpression expression)
        {
            if (DisplayTemplate == null) return new HtmlString(string.Empty);
            return await DisplayTemplate.Invoke(expression, this, helpers);
        }
    }
}
