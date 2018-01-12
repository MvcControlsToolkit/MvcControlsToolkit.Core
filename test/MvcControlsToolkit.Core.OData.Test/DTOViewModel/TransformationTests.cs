using System;
using System.Collections.Generic;
using System.Text;
using MvcControlsToolkit.Core.Views;
using Xunit;
using Xunit.Abstractions;
using MvcControlsToolkit.Core.Business.Transformations;
using MvcControlsToolkit.Core.OData.Test.Data;
using MvcControlsToolkit.Core.OData.Test.Models;
using MvcControlsToolkit.Core.Types;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MvcControlsToolkit.Core.OData.Test.DTOViewModel
{

    [CollectionDefinition("TransformationCollection")]
    public class TransformationCollection : ICollectionFixture<DBInitializer>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    [Collection("TransformationCollection")]
    public class TransformationTests
    {
        private List<ReferenceModel> allModels;
        public TransformationTests(DBInitializer init)
        {
            allModels = new TestContext().
                ReferenceModels.Include(m => m.Children).OrderBy(m => m.Id).ToList();
        }
        [Fact]
        public void SingleTransform()
        {
            var context = new MappingContext()
                .Add<ReferenceModel, ReferenceTypeWithChildren> (m => new ReferenceTypeWithChildren
                {
                    AMonth = Month.FromDateTime(m.AMonth),
                    ANMonth = Month.FromDateTime(m.ANMonth.Value),
                    AWeek = Week.FromDateTime(m.AWeek),
                    ANWeek = Week.FromDateTime(m.ANWeek.Value),
                    Children = m.Children.Select(l => new NestedReferenceType { })
                });
            var res = allModels[0].Map(context).To<ReferenceTypeWithChildren>();
            Assert.NotNull(res);
            Assert.NotNull(res.Children);
            Assert.Equal(res.Children.Count(), 2);
            Assert.Equal(res.AWeek, Week.FromDateTime(new DateTime(2016, 10, 7)));
            Assert.Equal(res.AMonth, Month.FromDateTime(new DateTime(2016, 10, 7)));
            Assert.Equal(res.AString, "dummy1");
        }
        [Fact]
        public void SingleTransformDefault()
        {
            MappingContext.Default
                .Add<ReferenceModel, ReferenceTypeWithChildren>(m => new ReferenceTypeWithChildren
                {
                    AMonth = Month.FromDateTime(m.AMonth),
                    ANMonth = Month.FromDateTime(m.ANMonth.Value),
                    AWeek = Week.FromDateTime(m.AWeek),
                    ANWeek = Week.FromDateTime(m.ANWeek.Value),
                    Children = m.Children.Select(l => new NestedReferenceType { })
                });
            var res = allModels[0].Map().To<ReferenceTypeWithChildren>();
            Assert.NotNull(res);
            Assert.NotNull(res.Children);
            Assert.Equal(res.Children.Count(), 2);
            Assert.Equal(res.AWeek, Week.FromDateTime(new DateTime(2016, 10, 7)));
            Assert.Equal(res.AMonth, Month.FromDateTime(new DateTime(2016, 10, 7)));
            Assert.Equal(res.AString, "dummy1");
        }
        [Fact]
        public void MultipleTransformDefault()
        {
            MappingContext.Default
                .Add<ReferenceModel, ReferenceTypeWithChildren>(m => new ReferenceTypeWithChildren
                {
                    AMonth = Month.FromDateTime(m.AMonth),
                    ANMonth = Month.FromDateTime(m.ANMonth.Value),
                    AWeek = Week.FromDateTime(m.AWeek),
                    ANWeek = Week.FromDateTime(m.ANWeek.Value),
                    Children = m.Children.Select(l => new NestedReferenceType { })
                });
            var ieres = allModels.MapIEnumerable().To<ReferenceTypeWithChildren>();
            Assert.Equal(ieres.Count(), 4);
            var res = ieres.ToArray()[0];
            Assert.NotNull(res);
            Assert.NotNull(res.Children);
            Assert.Equal(res.Children.Count(), 2);
            Assert.Equal(res.AWeek, Week.FromDateTime(new DateTime(2016, 10, 7)));
            Assert.Equal(res.AMonth, Month.FromDateTime(new DateTime(2016, 10, 7)));
            Assert.Equal(res.AString, "dummy1");
        }
    }
}
