using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MvcControlsToolkit.Core.Templates;

namespace MvcControlsToolkit.Core.DataAnnotations
{
    public interface IDispalyValueItemsProvider
    {
        Task<IEnumerable> GetDisplayValuePairs(object x);
        string ClientDisplayValueItemsSelector { get; }

    }
    public interface IDispalyValueSuggestionsProvider
    {
        string GetDisplayValueSuggestionsUrl(IUrlHelper uri);
        string DisplayValueSuggestionsUrlToken { get; set; }
    }
    public class DefaultDispalyValueSuggestionsProvider: IDispalyValueSuggestionsProvider
    {
        public string GetDisplayValueSuggestionsUrl(IUrlHelper uri)
        {
            return displayValueSuggestionsUrl;
        }
        public string DisplayValueSuggestionsUrlToken { get; set; }
        private string displayValueSuggestionsUrl;
        public DefaultDispalyValueSuggestionsProvider(
            string displayValueSuggestionsUrl,
            string displayValueSuggestionsUrlToken
            )
        {
            this.displayValueSuggestionsUrl = displayValueSuggestionsUrl;
            DisplayValueSuggestionsUrlToken = displayValueSuggestionsUrlToken;
        }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnConnectionAttribute: Attribute
    {
        private Func<RowType, ColumnConnectionInfos> computeInfo = null;
        public ColumnConnectionAttribute(string displayPropertyString,
        string itemsDisplayProperty,
        string itemsValueProperty,
        Type itemsProviderType,
        bool queryDisplay = false)
        {
            computeInfo = row =>
                new ColumnConnectionInfosStatic(
                    row.For.ModelExplorer
                        .Properties.Where(m => m.Metadata.PropertyName == displayPropertyString)
                        .Select(m => new ModelExpression(m.Metadata.PropertyName, m))
                        .FirstOrDefault(),
                    itemsDisplayProperty,
                    itemsValueProperty,
                    itemsProviderType,
                    queryDisplay);
        }
        public ColumnConnectionAttribute(
            string displayPropertyString,
            string itemsDisplayProperty,
            string itemsValueProperty,
            Type itemsProviderType,
            uint maxResults,
            string dataSetName = null,
            bool queryDisplay = false)
        {
            computeInfo = row =>
                new ColumnConnectionInfosOnLine(
                    row.For.ModelExplorer
                        .Properties.Where(m => m.Metadata.PropertyName == displayPropertyString)
                        .Select(m => new ModelExpression(m.Metadata.PropertyName, m))
                        .FirstOrDefault(),
                    itemsDisplayProperty,
                    itemsValueProperty,
                    itemsProviderType,
                    maxResults, 
                    dataSetName?? (displayPropertyString??string.Empty) + "_DataSet",
                    queryDisplay);
        }
        internal  ColumnConnectionInfos GetConnection(RowType row)
        {
            return computeInfo(row);
        }
    }
}
