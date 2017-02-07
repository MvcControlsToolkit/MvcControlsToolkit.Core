using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;
using Xunit;
using Xunit.Abstractions;

namespace MvcControlsToolkit.Core.OData.Test.Query
{
    public class ODataQueryProvider_Integration
    {
        ODataQueryProvider provider;
        private readonly ITestOutputHelper output;
        public ODataQueryProvider_Integration(ITestOutputHelper output)
        {
            provider = new ODataQueryProvider();
            this.output = output;
        }
        [Theory]
        [InlineData(50, 10, 6)]
        public void Paging (int from, int howMany, int page)
        {
            provider.Skip = from.ToString();
            provider.Top = howMany.ToString();

            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);
            Assert.Equal(res.Skip, from);
            Assert.Equal(res.Take, howMany);
            Assert.Equal(res.Page, page);

            Assert.Equal(res.ToString().Replace(" ", ""), "$skip=" + res.Skip.ToString() + "&$top=" + res.Take.ToString());
        }
        [Theory]
        [InlineData("http://www.dummy.com?id=1",
            "(AString ge 'Hello')",
            "AString asc",
            null,
            "groupby((AString), aggregate(AMonth with countdistinct as resint))",
            "50",
            "10",
            "http://www.dummy.com?id=1&$filter=(AString ge 'Hello')&$apply=groupby((AString), aggregate(AMonth with countdistinct as resint))&$orderby=AString asc&$skip=50&$top=10")]
        [InlineData("http://www.dummy.com?id=1",
            null,
            "AString asc",
            "Hello OR dummy",
            "groupby((AString), aggregate(AMonth with countdistinct as resint))",
            "50",
            "10",
            "http://www.dummy.com?id=1&$search=(Hello OR dummy)&$apply=groupby((AString), aggregate(AMonth with countdistinct as resint))&$orderby=AString asc&$skip=50&$top=10")]
        public void Integration(string url, string filter, string sorting, string search, string apply, string skip, string top, string totalUrl)
        {
            provider.Filter = filter;
            provider.Search = search;
            provider.Apply = apply;
            provider.OrderBy = sorting;
            provider.Skip = skip;
            provider.Top = top;

            var res = provider.Parse<ReferenceType>();
            Assert.NotNull(res);

            res.CustomUrlEncode(x => x);

            Assert.Equal(res.AddToUrl(url).Replace(" ", ""),
                totalUrl.Replace(" ", ""));
        }
    }
}
