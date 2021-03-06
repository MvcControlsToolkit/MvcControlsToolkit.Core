﻿using System;
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
        protected static IDictionary<KeyValuePair<Type, string>, Tuple<QueryAttribute, FilterLayoutAttribute>> QueryOptionsDictionary = new ConcurrentDictionary<KeyValuePair<Type, string>, Tuple<QueryAttribute, FilterLayoutAttribute>>();
        protected string[] bootstrap3Grid = new string[] { "col-xs-", "col-sm-", "col-md-", "col-lg-" };
        protected string[] bootstrap4Grid = new string[] { "col-xs-", "col-sm-", "col-md-", "col-lg-", "col-xl-" };
        protected string[] bootstrap3Visibility = new string[] { "clearfix visible-xs-block", "clearfix visible-sm-block", "clearfix visible-md-block", "clearfix visible-lg-block" };
        protected string[] bootstrap4Visibility = new string[] { "clearfix hidden-sm-up", "clearfix hidden-xs-down hidden-md-up", "clearfix hidden-sm-down hidden-lg-up", "clearfix hidden-md-down hidden-xl-up", "clearfix hidden-lg-down" };
        public RowType Row { get; internal set; }
        public ModelExpression For { get; private set; }
        public Template<Column> EditTemplate { get; set; }
        public Template<Column> DisplayTemplate { get; set; }
        public Template<Column> FilterTemplate { get; set; }
        private string _columnTitle;
        private string localize(string x)
        {
            if (x == null) return null;
            var localizer= Row.GetLocalizer();
            if (localizer != null) return localizer[x];
            else return x;
        }
        public string ColumnTitle {
            get {
                return _columnTitle != null || For == null ? localize(_columnTitle) : For.Metadata.GetDisplayName();
            }
            set { _columnTitle = value; } }
        public bool? Hidden { get; set; }
        public bool ReadOnly { get; set; }
        public bool EditOnly { get; set; }
        public string DisplayFormat { get; set; }
        private string _placeHolder;
        public string PlaceHolder
        {
            get
            {
                return _placeHolder != null || For == null ? localize(_placeHolder) : For.Metadata.Placeholder;
            }
            set { _placeHolder = value; }
        }
        private string _nullDisplayText;
        public string NullDisplayText
        {
            get
            {
                return _nullDisplayText != null || For == null ? localize(_nullDisplayText) : For.Metadata.NullDisplayText;
            }
            set { _nullDisplayText = value; }
        }
        private string _description;
        public string Description
        {
            get
            {
                return (_description != null || For == null ? localize(_description) : For.Metadata.Description)??ColumnTitle ;
            }
            set { _description = value; }
        }
        public decimal[] Widths { get; set; }
        public decimal[] DetailWidths { get; set; }
        public int[] DisplayDetailWidths { get; set; }
        public int[] EditDetailWidths { get; set; }
        public bool[] DisplayDetailEndRow { get; set; }
        public bool[] EditDetailEndRow { get; set; }
        public string ColumnCssClass { get; set; }
        public int? Order { get; set; }
        public int NaturalOrder { get; set; }
        public string AdditionalPrefix { get; set; }
        public int? ColSpan { get; set; }
        public string InputDetailCssClass { get; set; }
        public string InputCssClass { get; set; }
        public string CheckboxCssClass { get; set; }
        public string CheckboxDetailCssClass { get; set; }
        public QueryOptions? Queries { get; set;}
        public QueryOptions[] FilterClauses { get; set; }
        private string name;
        public string Name {get
            {
                return For == null ? name : For.Metadata.PropertyName;
            } }
        public ColumnConnectionInfos ColumnConnection { get; set; }
        public Column CloneColumn()
        {
            return this.MemberwiseClone() as Column;
        }
        internal string PrepareConnections(RowType row)
        {
            if (ColumnConnection != null || For==null || string.IsNullOrEmpty(For.Metadata.PropertyName)) return null;
            var pres = row?.For?.Metadata?.ModelType?.GetTypeInfo()?.GetProperty(For.Metadata.PropertyName)?
                        .GetCustomAttribute<ColumnConnectionAttribute>(false);
            if (pres == null) return null;
            else
            {
                ColumnConnection = pres.GetConnection(row);
                return ColumnConnection.DisplayProperty.Metadata.PropertyName;
            }
            
        }
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
        public string GetLineBreakClass(SupportedGridSystems gs, bool edit, int level)
        {

            string[] allClasses = gs == SupportedGridSystems.Bootstrap3 ? bootstrap3Visibility : bootstrap4Visibility;
            bool[] allEnds;
            if (edit) allEnds = EditDetailEndRow;
            else allEnds = DisplayDetailEndRow;
            if(allEnds==null || allEnds.Length==0) return null;
            if (level < 0) level = 0;
            else if (level >= allEnds.Length) return null;
            return allEnds[level] ? allClasses[level] : null;
        }
        public string GetTotalClass(SupportedGridSystems gs, bool edit)
        {
            string[] allStyles;
            
            var cssClass = ColumnCssClass ?? "form-group";
            
            var currWidths = edit ? EditDetailWidths : DisplayDetailWidths;
            if (currWidths != null && currWidths.Length > 0)
            {
                if (gs == SupportedGridSystems.Bootstrap3)
                    allStyles = bootstrap3Grid;
                else if (gs == SupportedGridSystems.Bootstrap4)
                    allStyles = bootstrap4Grid;
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
            
            //if (ColumnConnection!= null)
            //{
            //    var infos = ColumnConnection.DisplayProperty.Metadata;
            //    if (string.IsNullOrEmpty(ColumnTitle)) ColumnTitle = infos.GetDisplayName();
            //    if (string.IsNullOrEmpty(Description)) Description = infos.Description ?? ColumnTitle;
            //    if (string.IsNullOrEmpty(PlaceHolder)) PlaceHolder = infos.Placeholder;
            //    if (string.IsNullOrEmpty(DisplayFormat)) DisplayFormat = infos.DisplayFormatString;
            //    if (string.IsNullOrEmpty(NullDisplayText)) NullDisplayText = infos.NullDisplayText;
            //}
            //else
            //{
            //    if (string.IsNullOrEmpty(ColumnTitle)) ColumnTitle = metaData.GetDisplayName();
            //    if (string.IsNullOrEmpty(Description)) Description = metaData.Description ?? ColumnTitle;
            //    if (string.IsNullOrEmpty(PlaceHolder)) PlaceHolder = metaData.Placeholder;
            //    if (string.IsNullOrEmpty(DisplayFormat)) DisplayFormat = metaData.DisplayFormatString;
            //    if (string.IsNullOrEmpty(NullDisplayText)) NullDisplayText = metaData.NullDisplayText;
            //}
            
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
            
            prepared = true;
        } 
        internal void PrepareQueryOptions()
        {
            if (Row.QueryEnabled.HasValue && Row.QueryEnabled.Value)
            {
                string propertyName = ColumnConnection != null && ColumnConnection.QueryDisplay ?
                    ColumnConnection.DisplayProperty.Metadata.PropertyName :
                    For.Metadata.PropertyName;

                Type propertyType = ColumnConnection != null && ColumnConnection.QueryDisplay ?
                    ColumnConnection.DisplayProperty.Metadata.ModelType :
                    For.Metadata.ModelType;

                var pair = new KeyValuePair<Type, string>(Row.For.Metadata.ModelType, propertyName);
                Tuple<QueryAttribute, FilterLayoutAttribute> res;
                if (!QueryOptionsDictionary.TryGetValue(pair, out res))
                {
                    res = Tuple.Create(Row.For.Metadata.ModelType.GetTypeInfo().GetProperty(For.Metadata.PropertyName)
                                        .GetCustomAttribute(typeof(QueryAttribute), false) as QueryAttribute,
                                    Row.For.Metadata.ModelType.GetTypeInfo().GetProperty(For.Metadata.PropertyName)
                        .GetCustomAttribute(typeof(FilterLayoutAttribute), false) as FilterLayoutAttribute);
                    QueryOptionsDictionary[pair] = res;


                }
                if (res != null)
                {
                    if (res.Item1 != null)
                    {
                        if (Queries.HasValue) Queries = res.Item1.AllowedForProperty(Queries.Value, propertyType);
                        else Queries = res.Item1.AllowedForProperty(propertyType);
                    }
                    if (res.Item2 != null)
                    {
                        if (FilterClauses == null) FilterClauses = res.Item2.FilterClauses;
                    }
                }
                else Queries = QueryOptions.None;
            }
            if(Queries == null) Queries = QueryOptions.None;
            if (FilterClauses == null) FilterClauses = new QueryOptions[] { Queries.Value & QueryOptions.AllFilters };
            else
            {
                for(int i=0; i<FilterClauses.Length; i++)
                {
                    FilterClauses[i] = FilterClauses[i] & Queries.Value & QueryOptions.AllFilters;
                }
            }
        }
        public bool CanFilter {get{ return For != null && (Queries & QueryOptions.AllFilters) != QueryOptions.None; }}
        public bool CanSort { get { return For != null && (Queries & QueryOptions.OrderBy) == QueryOptions.OrderBy; } }
        public bool CanGroup { get { return For != null && (Queries & QueryOptions.GroupBy) == QueryOptions.GroupBy; } }
        public bool CanAggregate { get { return For != null ; } }
        public async Task<IHtmlContent> InvokeEdit(object o, ContextualizedHelpers helpers, string overridePrefix=null)
        {
            if (EditTemplate == null) return new HtmlString(string.Empty);
            return await EditTemplate.Invoke(new ModelExpression(
                combinePrefixes(AdditionalPrefix, For.Name), For.ModelExplorer.GetExplorerForModel(o)), 
                this, helpers, overridePrefix);
        }
        public async Task<IHtmlContent> InvokeFilter(object o, ContextualizedHelpers helpers, string overridePrefix = null)
        {
            if (FilterTemplate == null) return new HtmlString(string.Empty);
            return await FilterTemplate.Invoke(new ModelExpression(
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
        public async Task<IHtmlContent> InvokeFilter(ContextualizedHelpers helpers, ModelExpression expression)
        {
            if (FilterTemplate == null) return new HtmlString(string.Empty);
            return await FilterTemplate.Invoke(expression, this, helpers);
        }
        
        public async Task<IHtmlContent> InvokeDisplay(ContextualizedHelpers helpers, ModelExpression expression)
        {
            if (DisplayTemplate == null) return new HtmlString(string.Empty);
            return await DisplayTemplate.Invoke(expression, this, helpers);
        }
    }
}
