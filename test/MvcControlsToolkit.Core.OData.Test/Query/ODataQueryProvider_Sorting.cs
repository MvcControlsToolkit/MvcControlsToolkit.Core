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
    public class ODataQueryProvider_Sorting
    {
        ODataQueryProvider provider;
        private readonly ITestOutputHelper output;
        public ODataQueryProvider_Sorting(ITestOutputHelper output)
        {
            provider = new ODataQueryProvider();
            this.output = output;
        }
        [Theory]
        [InlineData("AString asc, AFloat desc", 2)]
        [InlineData("AString asc, AFloat desc, ADuration asc", 3)]
        [InlineData("AString asc", 1)]
        public void ParseToString(string sorting, int count)
        {
            provider.OrderBy = sorting;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);
            Assert.NotNull(res.Sorting);
            Assert.Equal(res.Sorting.Count, count);
            var nf = res.EncodeSorting();
            Assert.Equal(nf.Replace(" ", ""), sorting.Replace(" ", ""));
        }
    }
}
