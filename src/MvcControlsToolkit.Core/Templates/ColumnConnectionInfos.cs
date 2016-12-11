using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MvcControlsToolkit.Core.Templates
{
    public class ColumnConnectionInfos
    {
        public ModelExpression DisplayProperty { get; private set; }

        public string ItemsDisplayProperty { get; private set; }

        public string ItemsValueProperty { get; private set; }

        public bool QueryDisplay { get; private set; }

        public ColumnConnectionInfos(
            ModelExpression displayProperty,
            string itemsDisplayProperty,
            string itemsValueProperty,
            bool queryDisplay=false)
        {
            DisplayProperty = displayProperty;
            ItemsDisplayProperty = itemsDisplayProperty;
            ItemsValueProperty = itemsValueProperty;
            QueryDisplay = queryDisplay;

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
    }
}
