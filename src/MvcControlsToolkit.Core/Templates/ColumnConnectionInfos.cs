using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MvcControlsToolkit.Core.DataAnnotations;

namespace MvcControlsToolkit.Core.Templates
{
    public class ColumnConnectionInfos
    {
        public ModelExpression DisplayProperty { get; private set; }

        public string ItemsDisplayProperty { get; private set; }

        public string ItemsValueProperty { get; private set; }

        public bool QueryDisplay { get; private set; }

        public Type ItemsProvider { get; private set; }

        public ColumnConnectionInfos(
            ModelExpression displayProperty,
            string itemsDisplayProperty,
            string itemsValueProperty,
            bool queryDisplay=false,
            Type itemsProvider = null)
        {
            DisplayProperty = displayProperty;
            ItemsDisplayProperty = itemsDisplayProperty;
            ItemsValueProperty = itemsValueProperty;
            QueryDisplay = queryDisplay;
            ItemsProvider = itemsProvider;
        }
        
    }
    public class ColumnConnectionInfosOnLine : ColumnConnectionInfos
    {
        public string ItemsUrl { get; private set; }
        public string UrlToken { get; private set; }

        public string DataSetName { get; private set; }
        public uint MaxResults { get; private set; }
        public ColumnConnectionInfosOnLine(
        ModelExpression displayProperty,
        string itemsDisplayProperty,
        string itemsValueProperty,
        string itemsUrl,
        string urlToken,
        uint maxResults, 
        string dataSetName,
        bool queryDisplay = false
        )
            : base(displayProperty,
                 itemsDisplayProperty,
                 itemsValueProperty,
                 queryDisplay)
        {
            ItemsUrl = itemsUrl;
            UrlToken = urlToken;
            DataSetName = dataSetName;
            MaxResults = maxResults; 
        }
        public ColumnConnectionInfosOnLine(
        ModelExpression displayProperty,
        string itemsDisplayProperty,
        string itemsValueProperty,
        Type itemsProvider,
        uint maxResults, 
        string dataSetName,
        bool queryDisplay = false
        )
            : base(displayProperty,
                 itemsDisplayProperty,
                 itemsValueProperty,
                 queryDisplay,
                 itemsProvider)
        {
            
            DataSetName = dataSetName;
            MaxResults = maxResults;
        }
        public IDispalyValueSuggestionsProvider SuggestionsProvider(IServiceProvider services)
        {
            if(ItemsUrl != null)
            {
                return new DefaultDispalyValueSuggestionsProvider(ItemsUrl, UrlToken??"_ s");
            }
            var provider = services.GetService(ItemsProvider) as IDispalyValueSuggestionsProvider;
            if (provider != null)
            {
                provider.DisplayValueSuggestionsUrlToken = provider.DisplayValueSuggestionsUrlToken ?? "_ s";
                return provider;
            }
            else return null;
        }
    }
    public class ColumnConnectionInfosStatic : ColumnConnectionInfos
    {
        public string ClientItemsSelector { get; private set; }
        public Func<object, Task<IEnumerable>> ItemsSelector { get; private set; }
        public ColumnConnectionInfosStatic(
        ModelExpression displayProperty,
        string itemsDisplayProperty,
        string itemsValueProperty,
        Func<object, Task<IEnumerable>> itemsSelector,
        string clientItemsSelector,
        bool queryDisplay = false)
            : base(displayProperty,
                 itemsDisplayProperty,
                 itemsValueProperty,
                 queryDisplay)
        {
            ClientItemsSelector = clientItemsSelector;
            ItemsSelector = itemsSelector;
        }
        public ColumnConnectionInfosStatic(
        ModelExpression displayProperty,
        string itemsDisplayProperty,
        string itemsValueProperty,
        Type itemsProvider,
        bool queryDisplay = false)
            : base(displayProperty,
                 itemsDisplayProperty,
                 itemsValueProperty,
                 queryDisplay, itemsProvider)
        {
            
        }
        public async Task<IEnumerable> GetItems(IServiceProvider services, object o)
        {
            if (ItemsSelector != null) return await ItemsSelector(o);
            if (ItemsProvider != null)
            {
                var provider = services.GetService(ItemsProvider) as IDispalyValueItemsProvider;
                if (provider != null) return await provider.GetDisplayValuePairs(o);

            }
            
            return null;
        }
        public string GetClientItemsSelector(IServiceProvider services)
        {
            if (ClientItemsSelector != null) return ClientItemsSelector;
            if(ItemsProvider != null)
            {
                var provider = services.GetService(ItemsProvider) as IDispalyValueItemsProvider;
                if (provider != null) return provider.ClientDisplayValueItemsSelector;
            }
            return null;
        }
    }
}
