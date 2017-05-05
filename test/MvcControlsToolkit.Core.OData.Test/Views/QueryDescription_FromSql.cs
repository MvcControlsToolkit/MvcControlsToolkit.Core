using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.OData.Test.Data;
using MvcControlsToolkit.Core.Types;
using MvcControlsToolkit.Core.Views;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MvcControlsToolkit.Core.OData.Test.Views
{
    public class QueryDescription_FromSql
    {
        ODataQueryProvider provider;
        private readonly ITestOutputHelper output;
        public QueryDescription_FromSql(ITestOutputHelper output)
        {
            provider = new ODataQueryProvider();
            this.output = output;
        }
        [Theory]
        //constants
        [InlineData("ABool eq true")]
        [InlineData("ANBool eq false")]
        [InlineData("ANBool eq null")]
        [InlineData("ALong eq 1")]
        [InlineData("ANLong eq 2")]
        [InlineData("ANLong eq null")]
        [InlineData("AInt eq 1")]
        [InlineData("ANInt eq 2")]
        [InlineData("ANInt eq null")]
        [InlineData("AShort eq 1")]
        [InlineData("ANShort eq 2")]
        [InlineData("ANShort eq null")]
        [InlineData("ADouble eq 1.5")]
        [InlineData("ANDouble eq 2.5")]
        [InlineData("ANDouble eq null")]
        [InlineData("AFloat eq 1.5")]
        [InlineData("ANFloat eq 2.5")]
        [InlineData("ANFloat eq null")]
        [InlineData("AString eq 'Hello'")]
        [InlineData("AString eq null")]
        [InlineData("ADecimal eq 1.5")]
        [InlineData("ANDecimal eq 2.5")]
        [InlineData("ANDecimal eq null")]
        [InlineData("AGuid eq 01234567-89ab-cdef-0123-456789abcdef")]
        [InlineData("ANGuid eq 01234567-89ab-cdef-0123-456789abcdef")]
        [InlineData("ANGuid eq null")]
        [InlineData("ATime eq 07:59:59.999")]
        [InlineData("ANTime eq 07:59:59.999")]
        [InlineData("ANTime eq null")]
        [InlineData("ADuration eq duration'P12DT23H59M59.999999999999S'")]
        [InlineData("ANDuration eq duration'P12DT23H59M59.999999999999S'")]
        [InlineData("ANDuration eq null")]
        [InlineData("ADate eq 2012-12-03")]
        [InlineData("ANDate eq 2012-12-03")]
        [InlineData("ANDate eq null")]
        [InlineData("AMonth eq 2012-11-01")]
        [InlineData("ANMonth eq 2012-11-01")]
        [InlineData("ANMonth eq null")]
        [InlineData("AWeek eq 2012-11-01")]
        [InlineData("ANWeek eq 2012-11-01")]
        [InlineData("ANWeek eq null")]
        [InlineData("ADateTime eq 2012-12-03T07:16:23Z")]
        [InlineData("ANDateTime eq 2012-12-03T07:16:23Z")]
        [InlineData("ANDateTime eq null")]
        [InlineData("ADateTimeOffset eq 2012-12-03T07:16:23Z")]
        [InlineData("ANDateTimeOffset eq 2012-12-03T07:16:23Z")]
        [InlineData("ANDateTimeOffset eq null")]

        //operators
        [InlineData("AString ne 'Hello'")]
        [InlineData("AString eq 'Hello'")]
        [InlineData("AInt gt 5")]
        [InlineData("AInt ge 5")]
        [InlineData("AInt lt 5")]
        [InlineData("AInt le 5")]
        [InlineData("'Hello' ne AString")]
        [InlineData("'Hello' eq AString")]
        [InlineData("5 gt AInt")]
        [InlineData("5 ge AInt")]
        [InlineData("5 lt AInt")]
        [InlineData("5 le AInt")]
        [InlineData("contains(AString,'Hello')")]
        [InlineData("startswith(AString,'Hello')")]
        [InlineData("endswith(AString,'Hello')")]
        [InlineData("contains('Hello', AString)")]
        [InlineData("startswith('Hello', AString)")]
        [InlineData("endswith('Hello', AString)")]

        //logical operators
        [InlineData("AString ne 'Hello' and ADouble eq 1.5")]
        [InlineData("AString ne 'Hello' or ADouble eq 1.5")]
        [InlineData("AString ne 'Hello' and not (ADouble eq 1.5)")]
        [InlineData("not (ADouble eq 1.5)")]
        public void QueryableToInternal(string filter)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);

            Assert.NotNull(res.Filter);
            filter = res.Filter.ToString();

            var linQExpression = res.GetFilterExpression();

            Assert.NotNull(linQExpression);

            var iFilter=QueryFilterClause.FromLinQExpression(linQExpression);
            var filter1 = iFilter.ToString();

            Assert.Equal(filter, filter1);
        }
        [Theory]
        [InlineData("AString ne 'Hello' and ADouble eq 1.5", "ADecimal eq 1.5", false)]
        [InlineData("AString ne 'Hello' and ADouble eq 1.5", "ADecimal eq 1.5", true)]
        [InlineData("AString ne 'Hello'", "ADecimal eq 1.5", true)]
        [InlineData("AString ne 'Hello'", "ADecimal eq 1.5", false)]
        [InlineData("", "ADecimal eq 1.5", false)]
        [InlineData("not (ADouble eq 1.5)", "ADecimal eq 1.5", true)]
        [InlineData("not (ADouble eq 1.5)", "ADecimal eq 1.5", true)]
        public void AddingQueryableToInternalFilter(string filter, string toAdd, bool isOr)
        {
            provider.Filter = filter;
            var res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);
            if(!string.IsNullOrEmpty(filter))
                Assert.NotNull(res.Filter);
            var ifilter1 = res.Filter;
            var iRes1 = res;
            provider = new ODataQueryProvider();
            provider.Filter = toAdd;
            res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);

            Assert.NotNull(res.Filter);

            var ifilter2 = res.Filter;
            var iRes2 = res;

            var totalFilter = isOr ? string.Format("({0}) or ({1})", filter, toAdd)
                : string.Format("({0}) and ({1})", filter, toAdd);

            provider = new ODataQueryProvider();
            provider.Filter = string.IsNullOrEmpty(filter) ? toAdd : totalFilter;
            res = provider.Parse<ReferenceType>();

            Assert.NotNull(res);

            Assert.NotNull(res.Filter);

            var iTotalFilter = res.Filter;

            var linQToAdd = iRes2.GetFilterExpression();

            Assert.NotNull(linQToAdd);

            iRes1.AddFilterCondition(linQToAdd, isOr);

            

            Assert.Equal(iRes1.Filter.ToString(), iTotalFilter.ToString());
        }
    }
}
