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
    public class QueryDescriptionToSql_NestedObjects
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        public QueryDescriptionToSql_NestedObjects(DBInitializer init)
        {
            repository = init.Repository;
            provider = new ODataQueryProvider();
        }
        
        [Theory]
        [InlineData("AString asc", 4, "dummy1", 2)]
        [InlineData("AString desc", 4, "dummy2", 2)]
        [InlineData("ABool asc, AString asc", 4, "dummy2", 2)]
        [InlineData("ABool asc, AString desc", 4, "dummy2", 2)]
        [InlineData("ABool desc, AString desc", 4, "dummy1", 2)]
        [InlineData("", 4, null, 2)]
        public async Task NestedWithSorting(string command, int totalResults, string firstVAlue, int nChildren)
        {

            provider.OrderBy = command;
            if (string.IsNullOrWhiteSpace(provider.OrderBy))
                provider.OrderBy = "Id asc";
            var q = provider.Parse<ReferenceTypeWithChildren>();

            Assert.NotNull(q);

            var sortingClause = q.GetSorting();
            var res = await repository.GetPage<ReferenceTypeWithChildren>(
                    null,
                    sortingClause,
                    1, 10
                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
            {
                Assert.Equal(res.Data.First().AString, firstVAlue);
                
                Assert.Equal(res.Data.First().Children.Count(), nChildren);
                int nCount = 0;
                foreach(var child in res.Data.First().Children)
                {
                    Assert.Equal(child.AInt, nCount);
                    nCount++;
                }
            }
        }
        [Theory]
        [InlineData("AString asc", 4, "dummy1", 2)]
        [InlineData("AString desc", 4, "dummy2", 2)]
        [InlineData("ABool asc, AString asc", 4, "dummy2", 2)]
        [InlineData("ABool asc, AString desc", 4, "dummy2", 2)]
        [InlineData("ABool desc, AString desc", 4, "dummy1", 2)]
        [InlineData("", 4, null, 2)]
        public  async Task NestedWithSortingAndInterfaces(string command, int totalResults, string firstVAlue, int nChildren)
        {

            provider.OrderBy = command;
            if (string.IsNullOrWhiteSpace(provider.OrderBy))
                provider.OrderBy = "Id asc";
            var q = provider.Parse<IFilterReferenceTypeWithChildren>();

            Assert.NotNull(q);

            var sortingClause = q.GetSorting();
            var res =  await repository.GetPage(
                    null,
                    sortingClause,
                    1, 10
                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
            {
                Assert.Equal(res.Data.First().AString, firstVAlue);
                Assert.NotEqual((res.Data.First() as ReferenceTypeWithChildren).ANInt, null);
                Assert.Equal(res.Data.First().Children.Count(), nChildren);
                int nCount = 0;
                foreach (var child in res.Data.First().Children)
                {
                    Assert.Equal(child.AInt, nCount);
                    Assert.Null((child as NestedReferenceType).AString);
                    nCount++;
                }
            }
        }
    }
}
