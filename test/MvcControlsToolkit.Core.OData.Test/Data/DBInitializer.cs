using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.OData.Test.Models;
using MvcControlsToolkit.Core.Types;
using Xunit;

namespace MvcControlsToolkit.Core.OData.Test.Data
{
    public class DBInitializer: IDisposable
    {
        static DBInitializer()
        {
            DefaultCRUDRepository<TestContext, ReferenceModel>.
                DeclareProjection(m => new ReferenceType
                {
                    AMonth=Month.FromDateTime(m.AMonth),
                    ANMonth=Month.FromDateTime(m.ANMonth.Value),
                    AWeek=Week.FromDateTime(m.AWeek),
                    ANWeek=Week.FromDateTime(m.ANWeek.Value)
                });
        }
        private TestContext context;
        ICRUDRepository repository;
        public DBInitializer()
        {
            context = new TestContext();
            repository = new DefaultCRUDRepository<TestContext, ReferenceModel>(context, context.ReferenceModels);
            context.Database.Migrate();
            if (!context.ReferenceModels.Any())
            {
                ReferenceModel model = null;

                model = new ReferenceModel
                {
                    ABool = true,
                    ANBool = true,
                    AString = "dummy1",
                    ADate = new DateTime(2016, 10, 3),
                    ANDate = new DateTime(2016, 10, 3),
                    AMonth = new DateTime(2016, 10, 1),
                    ANMonth = new DateTime(2016, 10, 1),
                    AWeek = new DateTime(2016, 10, 7),
                    ANWeek = new DateTime(2016, 10, 7),
                    ADateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ANDateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ADateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    ANDateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    AGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    ANGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    AInt = 10,
                    ANInt = 10,
                    ALong = 10,
                    ANLong = 10,
                    AShort = 10,
                    ANShort = 10,
                    ADecimal = 10.5m,
                    ANDecimal = 10.5m,
                    AFloat = 10.5f,
                    ANFloat = 10.5f,
                    ADouble = 10.5f,
                    ANDouble = 10.5f,
                    ATime = new TimeSpan(20, 10, 0),
                    ANTime = new TimeSpan(20, 10, 0),
                    ADuration = new TimeSpan(10, 10, 0),
                    ANDuration = new TimeSpan(10, 10, 0)
                };

                context.ReferenceModels.Add(model);

                model = new ReferenceModel
                {
                    ABool = false,
                    ANBool = false,
                    AString = "dummy2",
                    ADate = new DateTime(2016, 10, 3),
                    ANDate = new DateTime(2016, 10, 3),
                    AMonth = new DateTime(2016, 10, 1),
                    ANMonth = new DateTime(2016, 10, 1),
                    AWeek = new DateTime(2016, 10, 7),
                    ANWeek = new DateTime(2016, 10, 7),
                    ADateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ANDateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ADateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    ANDateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    AGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    ANGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    AInt = 10,
                    ANInt = 10,
                    ALong = 10,
                    ANLong = 10,
                    AShort = 10,
                    ANShort = 10,
                    ADecimal = 10.5m,
                    ANDecimal = 10.5m,
                    AFloat = 10.5f,
                    ANFloat = 10.5f,
                    ADouble = 10.5f,
                    ANDouble = 10.5f,
                    ATime = new TimeSpan(20, 10, 0),
                    ANTime = new TimeSpan(20, 10, 0),
                    ADuration = new TimeSpan(10, 10, 0),
                    ANDuration = new TimeSpan(10, 10, 0)
                };

                context.ReferenceModels.Add(model);

                model = new ReferenceModel
                {
                    ABool = true,
                    ANBool = true,
                    AString = "dummy1",
                    ADate = new DateTime(2016, 10, 3),
                    ANDate = new DateTime(2016, 10, 3),
                    AMonth = new DateTime(2016, 10, 1),
                    ANMonth = new DateTime(2016, 10, 1),
                    AWeek = new DateTime(2016, 10, 7),
                    ANWeek = new DateTime(2016, 10, 7),
                    ADateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ANDateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ADateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    ANDateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    AGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    ANGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    AInt = 10,
                    ANInt = 10,
                    ALong = 10,
                    ANLong = 10,
                    AShort = 10,
                    ANShort = 10,
                    ADecimal = 10.5m,
                    ANDecimal = 10.5m,
                    AFloat = 10.5f,
                    ANFloat = 10.5f,
                    ADouble = 10.5f,
                    ANDouble = 10.5f,
                    ATime = new TimeSpan(20, 10, 0),
                    ANTime = new TimeSpan(20, 10, 0),
                    ADuration = new TimeSpan(10, 10, 0),
                    ANDuration = new TimeSpan(10, 10, 0)
                };

                context.ReferenceModels.Add(model);

                model = new ReferenceModel
                {
                    ABool = false,
                    ANBool = false,
                    AString = "dummy2",
                    ADate = new DateTime(2016, 10, 3),
                    ANDate = new DateTime(2016, 10, 3),
                    AMonth = new DateTime(2016, 10, 1),
                    ANMonth = new DateTime(2016, 10, 1),
                    AWeek = new DateTime(2016, 10, 7),
                    ANWeek = new DateTime(2016, 10, 7),
                    ADateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ANDateTime = new DateTime(2016, 10, 8, 20, 0, 0),
                    ADateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    ANDateTimeOffset = new DateTimeOffset(2016, 10, 8, 20, 0, 0, new TimeSpan(0, 0, 0)),
                    AGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    ANGuid = new Guid("01234567-89ab-cdef-0123-456789abcdef"),
                    AInt = 10,
                    ANInt = 10,
                    ALong = 10,
                    ANLong = 10,
                    AShort = 10,
                    ANShort = 10,
                    ADecimal = 10.5m,
                    ANDecimal = 10.5m,
                    AFloat = 10.5f,
                    ANFloat = 10.5f,
                    ADouble = 10.5f,
                    ANDouble = 10.5f,
                    ATime = new TimeSpan(20, 10, 0),
                    ANTime = new TimeSpan(20, 10, 0),
                    ADuration = new TimeSpan(10, 10, 0),
                    ANDuration = new TimeSpan(10, 10, 0)
                };

                context.ReferenceModels.Add(model);

                context.SaveChanges();
            }

            
        }
        public ICRUDRepository Repository { get { return repository; } }
        public void Dispose()
        {
            context.Dispose();
        }
    }
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DBInitializer>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
