using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcControlsToolkit.Core.Business.Utilities;
using MvcControlsToolkit.Core.OData.Test.Data;
using Xunit;
using MvcControlsToolkit.Core.OData.Test.Models;

namespace MvcControlsToolkit.Core.OData.Test.Repository
{
    [CollectionDefinition("StandardCopierSelectCollection")]
    public class StandardCopierSelectCollection : ICollectionFixture<DBInitializerAdvanced>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    [CollectionDefinition("StandardCopierUpdateCollection")]
    public class StandardCopierUpdateCollection : ICollectionFixture<DBInitializerAdvanced>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    [Collection("StandardCopierSelectCollection")]
    public class StandardCopierSelect
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        string standardSorting = "Name desc";
        string standardWhere = "startswith(Name,'Root')";
        public StandardCopierSelect(DBInitializerAdvanced init)
        {
            repository = init.Repository;
            provider = new ODataQueryProvider();
            provider.Filter = standardWhere;
            provider.OrderBy = standardSorting;
        }
        [Fact]
        public async Task FlattenedSelect()
        {
            var q = provider.Parse<PersonDTOFlattenedAuto>();
            
            Assert.NotNull(q);
            Assert.NotNull(q.Filter);
            Assert.NotNull(q.Sorting);
            var filterExpression = q.GetFilterExpression();
            var sortExpression = q.GetSorting();

            var res = await repository.GetPage(filterExpression, sortExpression, 1, 10);
            Assert.NotNull(res);
            Assert.Equal(res.TotalCount, 4);
            Assert.Equal(res.Data.Count, 4);
            var first = res.Data.First();
            Assert.Equal(first.Name, "Root3");
            Assert.Equal(first.SpouseName, "SpouseName3");
            Assert.Equal(first.SpouseSurname, "SpouseSurname3");

            Assert.Equal(first.Children.Count(), 4);
            var firstChild = first.Children.First();

            Assert.Equal(firstChild.Name, "Name3Children");
            Assert.Equal(firstChild.SpouseName, "SpouseName3Children");
            Assert.Equal(firstChild.SpouseSurname, "SpouseSurname3Children");

            Assert.Equal(first.SpouseChildren.Count(), 4);
            firstChild = first.SpouseChildren.First();

            Assert.Equal(firstChild.Name, "Name3SpouseChildren");
            Assert.Equal(firstChild.SpouseName, "SpouseName3SpouseChildren");
            Assert.Equal(firstChild.SpouseSurname, "SpouseSurname3SpouseChildren");

        }
        [Fact]
        public async Task NotFlattenedSelect()
        {
            var q = provider.Parse<PersonDTOAuto>();

            Assert.NotNull(q);
            Assert.NotNull(q.Filter);
            Assert.NotNull(q.Sorting);
            var filterExpression = q.GetFilterExpression();
            var sortExpression = q.GetSorting();

            var res = await repository.GetPage(filterExpression, sortExpression, 1, 10);
            Assert.NotNull(res);
            Assert.Equal(res.TotalCount, 4);
            Assert.Equal(res.Data.Count, 4);
            var first = res.Data.First();
            Assert.Equal(first.Name, "Root3");
            Assert.NotNull(first.Spouse);
            Assert.Equal(first.Spouse.Name, "SpouseName3");
            Assert.Equal(first.Spouse.Surname, "SpouseSurname3");

            Assert.Equal(first.Children.Count(), 4);
            var firstChild = first.Children.First();

            Assert.Equal(firstChild.Name, "Name3Children");
            Assert.NotNull(firstChild.Spouse);
            Assert.Equal(firstChild.Spouse.Name, "SpouseName3Children");
            Assert.Equal(firstChild.Spouse.Surname, "SpouseSurname3Children");

            Assert.Equal(first.Spouse.Children.Count(), 4);
            firstChild = first.Spouse.Children.First();

            Assert.Equal(firstChild.Name, "Name3SpouseChildren");
            Assert.NotNull(firstChild.Spouse);
            Assert.Equal(firstChild.Spouse.Name, "SpouseName3SpouseChildren");
            Assert.Equal(firstChild.Spouse.Surname, "SpouseSurname3SpouseChildren");

        }
    }

    [Collection("StandardCopierUpdateCollection")]
    public class StandardCopierUpdate
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        TestContext context;
        string standardSorting = "Name asc";
        string standardWhere = "startswith(Name,'Root')";
        public StandardCopierUpdate(DBInitializerAdvanced init)
        {
            repository = init.Repository;
            context = init.Context;
            provider = new ODataQueryProvider();
            provider.Filter = standardWhere;
            provider.OrderBy = standardSorting;
        }
        [Fact]
        public async Task Update()
        {
            var id= context.Persons.Where(m => m.Name == "Root0")
                .Select(m => m.Id)
                .SingleOrDefault();
            var dto = new PersonDTOFlattenedAuto()
            {
                Id = id,
                Name = "Root0",
                Surname = "SurnameModified0",
                SpouseName = "SpouseName0",
                SpouseSurname = "SpouseSurnameModified0"
            };
            repository.Update(false, dto);
            await repository.SaveChanges();
            var q = provider.Parse<PersonDTOFlattenedAuto>();

            Assert.NotNull(q);
            Assert.NotNull(q.Filter);
            Assert.NotNull(q.Sorting);
            var filterExpression = q.GetFilterExpression();
            var sortExpression = q.GetSorting();

            var res = await repository.GetPage(filterExpression, sortExpression, 1, 10);
            var first = res.Data.First();
            Assert.Equal(first.Name, "Root0");
            Assert.Equal(first.Surname, "SurnameModified0");
            Assert.Equal(first.SpouseName, "SpouseName0");
            Assert.Equal(first.SpouseSurname, "SpouseSurnameModified0");
        }
        [Fact]
        public async Task Add()
        {
            
            var dto = new PersonDTOFlattenedAuto()
            {
                Name = "RootNew",
                Surname = "SurnameNew",
                SpouseName = "SpouseNameNew",
                SpouseSurname = "SpouseSurnameNew"
            };
            repository.Add(true, dto);
            await repository.SaveChanges();
            Assert.NotNull(dto.Id);
            var id = dto.Id.Value;
            var added = await repository.GetById<PersonDTOFlattenedAuto, int>(id);
            Assert.NotNull(added);
            
            Assert.Equal(added.Name, "RootNew");
            Assert.Equal(added.Surname, "SurnameNew");
            Assert.Equal(added.SpouseName, "SpouseNameNew");
            Assert.Equal(added.SpouseSurname, "SpouseSurnameNew");
        }
        [Fact]
        public async Task Delete()
        {
            var tempC = new TestContext();
            Person test;
            tempC.Persons.Add(test=new Person {
                Name="DeleteTestName",
                Surname = "DeleteTestSurName",
            });
            tempC.SaveChanges();
            var id = test.Id;
            tempC.Dispose();
            tempC = new TestContext();
            var repo = new DefaultCRUDRepository<TestContext, Person>(tempC, tempC.Persons);
            repo.Delete<int>(id);
            await repo.SaveChanges();
            tempC.Dispose();
            tempC = new TestContext();
            repo = new DefaultCRUDRepository<TestContext, Person>(tempC, tempC.Persons);
            var deleted = await repo.GetById<PersonDTOFlattenedAuto, int>(id);
            Assert.Null(deleted);

            id = tempC.Persons.Where(m => m.Id == id)
                .Select(m => m.Id)
                .SingleOrDefault();
            tempC.Dispose();
            Assert.Equal(id, 0);
        }

    }
}
