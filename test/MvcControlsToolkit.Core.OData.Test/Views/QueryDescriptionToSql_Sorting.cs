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
    public class QueryDescriptionToSql_Sorting
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        public QueryDescriptionToSql_Sorting(DBInitializer init)
        {
            repository = init.Repository;
            provider = new ODataQueryProvider();

        }
        [Theory]
        [InlineData("AString asc", 4, "dummy1")]
        [InlineData("AString desc", 4, "dummy2")]
        [InlineData("ABool asc, AString asc", 4, "dummy2")]
        [InlineData("ABool asc, AString desc", 4, "dummy2")]
        [InlineData("ABool desc, AString desc", 4, "dummy1")]
        [InlineData("", 4, null)]
        public async Task RightSorting(string command, int totalResults, string firstVAlue)
        {
            provider.OrderBy = command;
            var q = provider.Parse<ReferenceType>();

            Assert.NotNull(q);
            
            var sortingClause = q.GetSorting();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    sortingClause?? (x => x.OrderBy(m => m.Id)),
                    1, 10
                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if(firstVAlue != null)
                Assert.Equal(res.Data.First().AString, firstVAlue); 
        }
    }
}
