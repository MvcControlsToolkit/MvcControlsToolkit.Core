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
    public class QueryDescriptionToSql_Grouping
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        public QueryDescriptionToSql_Grouping(DBInitializer init)
        {
            repository = init.Repository;
            provider = new ODataQueryProvider();
        }
        [Theory]
        
        [InlineData("groupby((AString, ABool))", 2, 0)]
        [InlineData("groupby((AString))", 2, 0)]
        [InlineData("groupby((AString, ABool), aggregate(ADecimal with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AFloat with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AShort with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(ADateTime with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(ADateTimeOffset with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AMonth with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AWeek with countdistinct as AInt))", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with average as AInt))", 2, 10)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with max as AInt))", 2, 10)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with min as AInt))", 2, 10)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with sum as AInt))", 2, 20)]
        [InlineData("", 4, null)]
        public async Task RightGrouping(string command, int totalResults, int? firstVAlue)
        {
            provider.Apply = command;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);

            var groupingClause = q.GetGrouping();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    (x => x.OrderBy(m => m.Id)),
                    1, 10,
                    groupingClause
                    
                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().AInt, firstVAlue.Value);
        }
        [Theory]
        [InlineData("groupby((AString, ABool), aggregate(AFloat with max as AFloat))", 2, 10.5f)]
        [InlineData("groupby((AString, ABool), aggregate(AFloat with min as AFloat))", 2, 10.5f)]
        [InlineData("groupby((AString, ABool), aggregate(AFloat with sum as AFloat))", 2, 21.0f)]

        public async Task RightGroupingFloat(string command, int totalResults, float? firstVAlue)
        {
            provider.Apply = command;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);
            Assert.True(q.CompatibleProperty("AString"));
            Assert.True(q.CompatibleProperty("ABool"));
            Assert.True(q.CompatibleProperty("AFloat"));
            Assert.False(q.CompatibleProperty("AFake"));
            var groupingClause = q.GetGrouping();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    (x => x.OrderBy(m => m.Id)),
                    1, 10,
                    groupingClause

                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().AFloat, firstVAlue.Value);
        }
        [Theory]
        [InlineData("groupby((AString, ABool), aggregate(ADouble with average as ADouble))", 2, 10.5)]
        [InlineData("groupby((AString, ABool), aggregate(ADouble with max as ADouble))", 2, 10.5)]
        [InlineData("groupby((AString, ABool), aggregate(ADouble with min as ADouble))", 2, 10.5)]
        [InlineData("groupby((AString, ABool), aggregate(ADouble with sum as ADouble))", 2, 21.0)]
        public async Task RightGroupingDouble(string command, int totalResults, double? firstVAlue)
        {
            provider.Apply = command;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);

            var groupingClause = q.GetGrouping();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    (x => x.OrderBy(m => m.Id)),
                    1, 10,
                    groupingClause

                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().ADouble, firstVAlue.Value);
        }
        [Theory]
        [InlineData("groupby((AString, ABool), aggregate(ALong with average as ALong))", 2, 10L)]
        [InlineData("groupby((AString, ABool), aggregate(ALong with max as ALong))", 2, 10L)]
        [InlineData("groupby((AString, ABool), aggregate(ALong with min as ALong))", 2, 10L)]
        [InlineData("groupby((AString, ABool), aggregate(ALong with sum as ALong))", 2, 20L)]
        public async Task RightGroupingLong(string command, int totalResults, long? firstVAlue)
        {
            provider.Apply = command;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);

            var groupingClause = q.GetGrouping();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    (x => x.OrderBy(m => m.Id)),
                    1, 10,
                    groupingClause

                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().ALong, firstVAlue.Value);
        }
        [Theory]
        [InlineData("groupby((AString, ABool), aggregate(ADecimal with average as ADecimal))", 2, "10.5")]
        [InlineData("groupby((AString, ABool), aggregate(ADecimal with max as ADecimal))", 2, "10.5")]
        [InlineData("groupby((AString, ABool), aggregate(ADecimal with min as ADecimal))", 2, "10.5")]
        [InlineData("groupby((AString, ABool), aggregate(ADecimal with sum as ADecimal))", 2, "21.0")]
        public async Task RightGroupingDecimal(string command, int totalResults, string firstVAlue)
        {
            provider.Apply = command;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);

            var groupingClause = q.GetGrouping();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    (x => x.OrderBy(m => m.Id)),
                    1, 10,
                    groupingClause

                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().ADecimal, Convert.ToDecimal(firstVAlue, CultureInfo.InvariantCulture));
        }
        [Theory]
        [InlineData("groupby((AString, ABool), aggregate(ANDecimal with average as ANDecimal))", 2, "10.5")]
        [InlineData("groupby((AString, ABool), aggregate(ANDecimal with max as ANDecimal))", 2, "10.5")]
        [InlineData("groupby((AString, ABool), aggregate(ANDecimal with min as ANDecimal))", 2, "10.5")]
        [InlineData("groupby((AString, ABool), aggregate(ANDecimal with sum as ANDecimal))", 2, "21.0")]
        public async Task RightGroupingNDecimal(string command, int totalResults, string firstVAlue)
        {
            provider.Apply = command;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);

            var groupingClause = q.GetGrouping();
            var res = await repository.GetPage<ReferenceType>(
                    null,
                    (x => x.OrderBy(m => m.Id)),
                    1, 10,
                    groupingClause

                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().ANDecimal, Convert.ToDecimal(firstVAlue, CultureInfo.InvariantCulture));
        }

        [Theory]

        [InlineData("groupby((AString, ABool))", "AString asc", 2, 0)]
        [InlineData("groupby((AString))", "AString asc", 2, 0)]
        [InlineData("groupby((AString, ABool), aggregate(ADecimal with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AFloat with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AShort with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(ADateTime with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(ADateTimeOffset with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AMonth with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AWeek with countdistinct as ANewInt))", "AString asc", 2, 1)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with average as ANewInt))", "AString asc", 2, 10)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with max as ANewInt))", "AString asc", 2, 10)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with min as ANewInt))", "AString asc", 2, 10)]
        [InlineData("groupby((AString, ABool), aggregate(AInt with sum as ANewInt))", "AString asc", 2, 20)]
        [InlineData("", "AString asc", 4, null)]
        public async Task ExtendedGrouping(string command, string sorting, int totalResults, int? firstVAlue)
        {
            provider.Apply = command;
            provider.OrderBy = sorting;
            var q = provider.Parse<ReferenceType>();
            var atype = new { AString = 7, ABool = 12, a = 1, b = 2 };
            Assert.NotNull(q);

            var groupingClause = q.GetGrouping<ReferenceTypeExtended>();
            var sortingClause = q.GetSorting<ReferenceTypeExtended>();
      
            var res = await repository.GetPageExtended<ReferenceType, ReferenceTypeExtended>(
                    null,
                    sortingClause,
                    1, 10,
                    groupingClause

                );
            Assert.Equal(res.TotalCount, totalResults);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, totalResults);
            if (firstVAlue != null)
                Assert.Equal(res.Data.First().ANewInt, firstVAlue.Value);
        }
    }
}
