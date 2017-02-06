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

    public class ODataQueryProvider_Aggregation
    {
        ODataQueryProvider provider;
        private readonly ITestOutputHelper output;
        public ODataQueryProvider_Aggregation(ITestOutputHelper output)
        {
            provider = new ODataQueryProvider();
            this.output = output;
        }
        [Theory]
        [InlineData("groupby((AString, AMonth))", 2, 0)]
        [InlineData("groupby((AString, AMonth), aggregate(ADecimal with average as resdecimal, AInt with min as resint))", 2, 2)]
        [InlineData("groupby((AString), aggregate(ADouble with average as resdouble, AInt with max as resint))", 1, 2)]
        [InlineData("groupby((AString), aggregate(AShort with sum as resshort, AInt with sum as resint))", 1, 2)]
        [InlineData("groupby((AString), aggregate(AFloat with sum as resfloat, AInt with average as resint))", 1, 2)]
        [InlineData("groupby((AString), aggregate(AFloat with max as resfloat, AShort with sum as resint))", 1, 2)]
        [InlineData("groupby((AString), aggregate(AShort with sum as resint))", 1, 1)]
        [InlineData("groupby((AString), aggregate(AMonth with countdistinct as resint))", 1, 1)]
        public void ParseToString(string groupby, int keysCount, int aggCounts)
        {
            provider.Apply = groupby;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);
            Assert.NotNull(res.Grouping);
            Assert.NotNull(res.Grouping.Keys);
            

            Assert.Equal(res.Grouping.Keys.Count, keysCount);
            if (res.Grouping.Aggregations != null)
                Assert.Equal(res.Grouping.Aggregations.Count, aggCounts);
            else
                Assert.Equal(aggCounts, 0);

            Assert.Equal(groupby.Replace(" ", ""), res.Grouping.ToString().Replace(" ", ""));
        }
    }
}
