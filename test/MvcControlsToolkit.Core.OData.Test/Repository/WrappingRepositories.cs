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
using MvcControlsToolkit.Core.Business.Transformations;

namespace MvcControlsToolkit.Core.OData.Test.Repository
{
    [Collection("Database collection")]
    public class WrappingRepositories
    {
        ICRUDRepository repository;
        public WrappingRepositories(DBInitializer init)
        {
            repository = init.Repository;
        }
        [Fact]
        public async Task ODataRepository()
        {
            var odata = new DefaultWebQueryRepository(repository);
            var query = new ODataQueryProvider();
            query.Filter = "AString eq 'dummy1'";
            query.OrderBy = "Id desc";
            query.Skip = "0";
            query.Top = "1";
            var res0 = await odata.ExecuteQuery<ReferenceType, ReferenceTypeExtended>(query);
            Assert.NotNull(res0);
            Assert.Equal(res0.TotalCount, 2);
            Assert.Equal(res0.TotalPages, 2);
            Assert.Equal(res0.Page, 1);
            Assert.NotNull(res0.Data);
            Assert.Equal(res0.Data.Count, 1);
            var el = res0.Data.First();
            Assert.Equal(el.AString, "dummy1");
            query = new ODataQueryProvider();
            query.Filter = "AString eq 'dummy1'";
            query.OrderBy = "Id desc";
            query.Skip = "1";
            query.Top = "1";
            var res1 = await odata.ExecuteQuery<ReferenceType, ReferenceTypeExtended>(query);
            Assert.NotNull(res1);
            Assert.Equal(res1.TotalCount, 2);
            Assert.Equal(res1.TotalPages, 2);
            Assert.Equal(res1.Page, 2);
            Assert.NotNull(res1.Data);
            Assert.Equal(res1.Data.Count, 1);
            var el1 = res1.Data.First();
            Assert.Equal(el1.AString, "dummy1");
            Assert.True(el.Id.Value > el1.Id.Value);
        }
        [Fact]
        public async Task ODataTransformationRepository()
        {
            var odata = new ODataTransformationRepositoryFarm()
                .Add<ReferenceVM, ReferenceType, ReferenceTypeExtended>()
                .Create(repository);
            var query = new ODataQueryProvider();
            query.Filter = "AString eq 'dummy1'";
            query.OrderBy = "Id desc";
            query.Skip = "0";
            query.Top = "1";
            var res0 = await odata.ExecuteQuery<ReferenceVM, ReferenceVMExtended>(query);
            Assert.NotNull(res0);
            Assert.Equal(res0.TotalCount, 2);
            Assert.Equal(res0.TotalPages, 2);
            Assert.Equal(res0.Page, 1);
            Assert.NotNull(res0.Data);
            Assert.Equal(res0.Data.Count, 1);
            var el = res0.Data.First();
            Assert.Equal(el.AString, "dummy1");
            query = new ODataQueryProvider();
            query.Filter = "AString eq 'dummy1'";
            query.OrderBy = "Id desc";
            query.Skip = "1";
            query.Top = "1";
            var res1 = await odata.ExecuteQuery<ReferenceVM, ReferenceVMExtended>(query);
            Assert.NotNull(res1);
            Assert.Equal(res1.TotalCount, 2);
            Assert.Equal(res1.TotalPages, 2);
            Assert.Equal(res1.Page, 2);
            Assert.NotNull(res1.Data);
            Assert.Equal(res1.Data.Count, 1);
            var el1 = res1.Data.First();
            Assert.Equal(el1.AString, "dummy1");
            Assert.True(el.Id.Value > el1.Id.Value);
            Assert.False(el is ReferenceVMExtended);
        }
        [Fact]
        public async Task ODataRepositoryWithGrouping()
        {
            var odata = new DefaultWebQueryRepository(repository);
            var query = new ODataQueryProvider();
            query.OrderBy = "AString asc";
            query.Apply = "groupby((AString, AMonth))";
            query.Skip = "0";
            query.Top = "100";
            var res0 = await odata.ExecuteQuery<ReferenceType, ReferenceTypeExtended>(query);
            Assert.NotNull(res0);
            Assert.Equal(res0.TotalCount, 2);
            Assert.Equal(res0.TotalPages, 1);
            Assert.Equal(res0.Page, 1);
            Assert.NotNull(res0.Data);
            Assert.Equal(res0.Data.Count, 2);
            var el = res0.Data.First();
            Assert.Equal(el.AString, "dummy1");
            Assert.True(el is ReferenceTypeExtended);
        }
        [Fact]
        public async Task ODataTransformationRepositoryWithGrouping()
        {
            var odata = new ODataTransformationRepositoryFarm()
                .Add<ReferenceVM, ReferenceType, ReferenceTypeExtended>()
                .Create(repository) as ODataTransformationRepository;
            var query = new ODataQueryProvider();
            query.OrderBy = "AString asc";
            query.Apply = "groupby((AString, AMonth))";
            query.Skip = "0";
            query.Top = "100";
            var res0 = await odata.ExecuteQuery<ReferenceVM, ReferenceVMExtended>(query);
            Assert.NotNull(res0);
            Assert.Equal(res0.TotalCount, 2);
            Assert.Equal(res0.TotalPages, 1);
            Assert.Equal(res0.Page, 1);
            Assert.NotNull(res0.Data);
            Assert.Equal(res0.Data.Count, 2);
            var el = res0.Data.First();
            Assert.Equal(el.AString, "dummy1");
            Assert.True(el is ReferenceVM);
        }
    }
}
