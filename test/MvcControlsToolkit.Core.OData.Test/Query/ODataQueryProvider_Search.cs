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
    public class ODataQueryProvider_Search
    {
        ODataQueryProvider provider;
        private readonly ITestOutputHelper output;
        public ODataQueryProvider_Search(ITestOutputHelper output)
        {
            provider = new ODataQueryProvider();
            this.output = output;
        }
        [Theory]
        [InlineData("Hello", QueryFilterBooleanOperator.and)]
        [InlineData("Hello dummy", QueryFilterBooleanOperator.and)]
        [InlineData("Hello AND dummy", QueryFilterBooleanOperator.and)]
        [InlineData("Hello OR dummy", QueryFilterBooleanOperator.or)]
        [InlineData("Hello AND NOT dummy", QueryFilterBooleanOperator.and)]
        [InlineData("Hello OR (crowd AND NOT dummy)", QueryFilterBooleanOperator.or)]
        public void ParseToString(string search, int op)
        {
            provider.Search = search;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);
            Assert.NotNull(res.Search);
            Assert.NotNull(res.Search.Value);
            provider.Search = res.Search.ToString();
            res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);
            Assert.NotNull(res.Search);
            Assert.NotNull(res.Search.Value);
            Assert.Equal(res.Search.Value.Operator, op);
            Assert.Equal(res.Search.ToString(), provider.Search);
            
        }
    }
}
