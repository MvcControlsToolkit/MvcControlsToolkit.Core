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
    [Collection("Database collection")]
    public class QueryDescriptionToSql_Filter 
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        public QueryDescriptionToSql_Filter(DBInitializer init)
        {
            repository = init.Repository;
            provider= new ODataQueryProvider(); 
        }
        [Theory]
        [InlineData("AString eq 'dummy1'", 2)]
        [InlineData("'dummy1' eq AString", 2)]
        [InlineData("AInt le 10", 4)]
        [InlineData("AInt lt 10", 0)]
        [InlineData("10 gt AInt", 0)]
        [InlineData("AInt ge 10", 4)]
        [InlineData("AInt gt 10", 0)]
        [InlineData("AString ne 'dummy1'", 2)]
        [InlineData("'dummy' ne AString", 4)]
        [InlineData("AString ne 'dummy'", 4)]
        [InlineData("startswith(AString,'dummy')", 4)]
        [InlineData("startswith('dummy1a', AString)", 2)]
        [InlineData("endswith(AString,'1')", 2)]
        [InlineData("endswith('adummy1', AString)", 2)]
        [InlineData("contains(AString,'umm')", 4)]
        [InlineData("contains('adummy1a', AString)", 2)]
        public async Task Operators(string filter, int all)
        {
            provider.Filter = filter;
            var q = provider.Parse<ReferenceType>();

            Assert.NotNull(q);
            Assert.NotNull(q.Filter);
            var filterExpression = q.GetFilterExpression();
            var res = await repository.GetPage<ReferenceType>(
                filterExpression,
                x => x.OrderBy(m => m.Id),
                1, 10
                );
            Assert.Equal(res.TotalCount, all);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, all);
        }
        [Theory]
        [InlineData("AString eq 'dummy1'", 2)]
        [InlineData("'dummy1' eq AString", 2)]

        [InlineData("AInt eq 10", 4)]
        [InlineData("AInt ne 10", 0)]
        [InlineData("ANInt eq 10", 4)]
        [InlineData("ANInt ne 10", 0)]

        [InlineData("AShort eq 10", 4)]
        [InlineData("AShort ne 10", 0)]
        [InlineData("ANShort eq 10", 4)]
        [InlineData("ANShort ne 10", 0)]

        [InlineData("ALong eq 10", 4)]
        [InlineData("ALong ne 10", 0)]
        [InlineData("ANLong eq 10", 4)]
        [InlineData("ANLong ne 10", 0)]

        [InlineData("ADecimal eq 10.5", 4)]
        [InlineData("ADecimal ne 10.5", 0)]
        [InlineData("ANDecimal eq 10.5", 4)]
        [InlineData("ANDecimal ne 10.5", 0)]

        [InlineData("AFloat eq 10.5", 4)]
        [InlineData("AFloat ne 10.5", 0)]
        [InlineData("ANFloat eq 10.5", 4)]
        [InlineData("ANFloat ne 10.5", 0)]

        [InlineData("ADouble eq 10.5", 4)]
        [InlineData("ADouble ne 10.5", 0)]
        [InlineData("ANDouble eq 10.5", 4)]
        [InlineData("ANDouble ne 10.5", 0)]
        

        [InlineData("ABool eq true", 2)]
        [InlineData("ANBool ne true", 2)]
        [InlineData("ANBool eq true", 2)]
        [InlineData("ABool ne true", 2)]

        [InlineData("AGuid eq 01234567-89ab-cdef-0123-456789abcdef", 4)]
        [InlineData("AGuid ne 01234567-89ab-cdef-0123-456789abcdef", 0)]
        [InlineData("ANGuid eq 01234567-89ab-cdef-0123-456789abcdef", 4)]
        [InlineData("ANGuid ne 01234567-89ab-cdef-0123-456789abcdef", 0)]


        [InlineData("ATime eq 20:10:00.000", 4)]
        [InlineData("ATime ne 20:10:00.000", 0)]
        [InlineData("ANTime eq 20:10:00.000", 4)]
        [InlineData("ANTime ne 20:10:00.000", 0)]

        [InlineData("ADuration eq duration'P00DT10H10M00.000000000000S'", 4)]
        [InlineData("ADuration ne duration'P00DT10H10M00.000000000000S'", 0)]
        [InlineData("ANDuration eq duration'P00DT10H10M00.000000000000S'", 4)]
        [InlineData("ANDuration ne duration'P00DT10H10M00.000000000000S'", 0)]

        [InlineData("ADate eq 2016-10-03", 4)]
        [InlineData("ADate ne 2016-10-03", 0)]
        [InlineData("ANDate eq 2016-10-03", 4)]
        [InlineData("ANDate ne 2016-10-03", 0)]

        [InlineData("AMonth eq 2016-10-01", 4)]
        [InlineData("AMonth ne 2016-10-01", 0)]
        [InlineData("ANMonth eq 2016-10-01", 4)]
        [InlineData("ANMonth ne 2016-10-01", 0)]

        [InlineData("ADateTime eq 2016-10-08T20:00:00Z",  4)]
        [InlineData("ADateTime ne 2016-10-08T20:00:00Z",  0)]
        [InlineData("ANDateTime eq 2016-10-08T20:00:00Z", 4)]
        [InlineData("ANDateTime ne 2016-10-08T20:00:00Z", 0)]

        [InlineData("ADateTimeOffset eq 2016-10-08T20:00:00Z", 4)]
        [InlineData("ADateTimeOffset ne 2016-10-08T20:00:00Z", 0)]
        [InlineData("ANDateTimeOffset eq 2016-10-08T20:00:00Z", 4)]
        [InlineData("ANDateTimeOffset ne 2016-10-08T20:00:00Z", 0)]

        
        public async Task Constants(string filter, int all)
        {
            provider.Filter = filter;
            var q = provider.Parse<ReferenceType>();

            Assert.NotNull(q);
            Assert.NotNull(q.Filter);
            var filterExpression = q.GetFilterExpression();
            var res = await repository.GetPage<ReferenceType>(
                filterExpression,
                x => x.OrderBy(m => m.Id),
                1, 10
                );
            Assert.Equal(res.TotalCount, all);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, all);
        }
        [Theory]
        [InlineData("(AString eq 'dummy1') or (ABool eq false)", 4)]
        [InlineData("(AString eq 'dummy1') or not (ABool eq true)", 4)]
        [InlineData("not (AString eq 'dummy1') or (ABool eq true)", 4)]
        [InlineData("(AString eq 'dummy1') and (ABool eq true)", 2)]
        [InlineData("(AString eq 'dummy1') and (not (ABool eq true))", 0)]
        public async Task LogicalOperators(string filter, int all)
        {
            provider.Filter = filter;
            var q = provider.Parse<ReferenceType>();

            Assert.NotNull(q);
            Assert.NotNull(q.Filter);
            var filterExpression = q.GetFilterExpression();
            var res = await repository.GetPage<ReferenceType>(
                filterExpression,
                x => x.OrderBy(m => m.Id),
                1, 10
                );
            Assert.Equal(res.TotalCount, all);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, all);
        }
    }
}
