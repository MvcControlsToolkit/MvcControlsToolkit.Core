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
    public class QueryDescriptionToSql_Setup 
    {
        ICRUDRepository repository;
        public QueryDescriptionToSql_Setup(DBInitializer init)
        {
            repository = init.Repository;
        }
        [Theory]
        [InlineData(4)]
        public async Task TestSetup(int all)
        {
            var res = await repository.GetPage<ReferenceType>(
                null,
                x => x.OrderBy(m => m.Id),
                1, 10
                );
            Assert.Equal(res.TotalCount, all);
            Assert.NotNull(res.Data);
            Assert.Equal(res.Data.Count, all);
        }
    }
}
