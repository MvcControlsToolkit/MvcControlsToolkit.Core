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
    [CollectionDefinition("RecursiveCopierCollection")]
    public class RecursiveCopierCollection : ICollectionFixture<DBInitializerAdvanced>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    [Collection("RecursiveCopierCollection")]
    public class RecursiveCopier
    {
        ICRUDRepository repository;
        ODataQueryProvider provider;
        TestContext context;

        public RecursiveCopier(DBInitializerAdvanced init)
        {
            repository = init.Repository;
            context = init.Context;

        }
        [Fact]
        public async Task AddFlattened()
        {
            var dto = new PersonDTOFlattened
            {
                Name = "NewName",
                Surname = "NewSurname",
                SpouseName = "NewSpouseName",
                SpouseSurname = "NewSpouseSurname",
                Children = new List<PersonDTOFlattened>()
                {
                    new PersonDTOFlattened
                    {
                        Name= "NewNameChildren",
                        Surname="NewSurnameChildren",
                        SpouseName = "NewSpouseNameChildren",
                        SpouseSurname = "NewSpouseSurnameChildren",
                    }
                },
                SpouseChildren = new List<PersonDTOFlattened>()
                {
                    new PersonDTOFlattened
                    {
                        Name= "NewNameChildrenSpouse",
                        Surname="NewSurnameChildrenSpouse",
                        SpouseName = "NewSpouseNameChildrenSpouse",
                        SpouseSurname = "NewSpouseSurnameChildrenSpouse",
                    }
                }
            };

            repository.Add<PersonDTOFlattened>(true, dto);
            await repository.SaveChanges();
            var id = dto.Id.Value;
            var res = await repository.GetById<PersonDTOFlattened, int>(id);
            Assert.NotNull(res);
            Assert.Equal(res.Name, "NewName");
            Assert.Equal(res.Surname, "NewSurname");
            Assert.Equal(res.SpouseName, "NewSpouseName");
            Assert.Equal(res.SpouseSurname, "NewSpouseSurname");

            Assert.NotNull(res.Children);
            Assert.Equal(res.Children.Count(), 1);
            var child = res.Children.FirstOrDefault();

            Assert.Equal(child.Name, "NewNameChildren");
            Assert.Equal(child.Surname, "NewSurnameChildren");
            Assert.Null(child.SpouseName);
            Assert.Null(child.SpouseSurname);

            Assert.Equal(res.SpouseChildren.Count(), 1);
            child = res.SpouseChildren.FirstOrDefault();

            Assert.Equal(child.Name, "NewNameChildrenSpouse");
            Assert.Equal(child.Surname, "NewSurnameChildrenSpouse");
            Assert.Equal(child.SpouseName, "NewSpouseNameChildrenSpouse");
            Assert.Equal(child.SpouseSurname, "NewSpouseSurnameChildrenSpouse");

        }
        [Fact]
        public async Task AddNotFlattened()
        {
            var dto = new PersonDTO
            {
                Name = "NewName",
                Surname = "NewSurname",
                Spouse = new PersonDTO
                {
                    Name = "NewSpouseName",
                    Surname = "NewSpouseSurname",
                    Children = new List<PersonDTO>()
                    {
                        new PersonDTO
                        {
                            Name = "NewNameChildrenSpouse",
                            Surname = "NewSurnameChildrenSpouse",

                        }
                    }

                },
                Children = new List<PersonDTO>()
                {
                    new PersonDTO
                    {
                        Name= "NewNameChildren",
                        Surname="NewSurnameChildren"

                    }
                }
            };

            repository.Add<PersonDTO>(true, dto);
            await repository.SaveChanges();
            var id = dto.Id.Value;
            var res = await repository.GetById<PersonDTO, int>(id);
            Assert.NotNull(res);
            Assert.Equal(res.Name, "NewName");
            Assert.Equal(res.Surname, "NewSurname");
            Assert.NotNull(res.Spouse);
            Assert.Equal(res.Spouse.Name, "NewSpouseName");
            Assert.Equal(res.Spouse.Surname, "NewSpouseSurname");

            Assert.NotNull(res.Children);
            Assert.Equal(res.Children.Count(), 1);
            var child = res.Children.FirstOrDefault();

            Assert.Equal(child.Name, "NewNameChildren");
            Assert.Equal(child.Surname, "NewSurnameChildren");
            Assert.Null(child.Spouse);

            Assert.NotNull(res.Spouse.Children);
            Assert.Equal(res.Spouse.Children.Count(), 1);
            child = res.Spouse.Children.FirstOrDefault();

            Assert.Equal(child.Name, "NewNameChildrenSpouse");
            Assert.Equal(child.Surname, "NewSurnameChildrenSpouse");
            Assert.Null(child.Spouse);

        }
        [Fact]
        public async Task ModifyFlattened()
        {
            var lres= await repository.GetPage<PersonDTOFlattened>(m => m.Name == "Root3", q => q.OrderBy(m => m.Name), 1, 10);
            var model = lres.Data.FirstOrDefault();
            model.Surname = "SurnameModified";
            model.SpouseName = "SpouseNameModified";
            var minId = model.Children.Min(m => m.Id);
            var maxId = model.Children.Max(m => m.Id);
            

            

            var list = model.Children.Where(m => m.Id != minId)
                .Append(new PersonDTOFlattened { Name = "AddedName", Surname = "AddedSurname" }).ToList();

            var changed = list.Where(m => m.Id == maxId).FirstOrDefault();
            changed.Surname = "ChildrenSurnameChanged";
            changed.SpouseSurname = "ChildrenSurnameSpouseChanged";
            model.Children = list;

            var minIdS = model.SpouseChildren.Min(m => m.Id);
            var maxIdS = model.SpouseChildren.Max(m => m.Id);
            

            

            var listS = model.SpouseChildren.Where(m => m.Id != minIdS)
                .Append(new PersonDTOFlattened { Name = "AddedName", Surname = "AddedSurname" }).ToList();
            changed = listS.Where(m => m.Id == maxIdS).FirstOrDefault();
            changed.Surname = "ChildrenSurnameChanged";
            changed.SpouseSurname = "ChildrenSurnameSpouseChanged";
            model.SpouseChildren = listS;
            var newCtx = new TestContext();
            var repos = new DefaultCRUDRepository<TestContext, Person>(newCtx, newCtx.Persons);

            repos.Update(false, model);
            await repos.SaveChanges();

            var id = model.Id.Value;

            model = await repos.GetById<PersonDTOFlattened, int>(id);

            Assert.Equal(model.Name, "Root3");
            Assert.Equal(model.Surname, "SurnameModified");
            Assert.Equal(model.SpouseName, "SpouseNameModified");
            Assert.Equal(model.SpouseSurname, "SpouseSurname3");
            Assert.Equal(model.Children.Count(), 4);
            Assert.Equal(model.SpouseChildren.Count(), 5);

            var deleted = model.Children.Where(m => m.Id == minId)
                .FirstOrDefault();
            Assert.Null(deleted);

            deleted = model.SpouseChildren.Where(m => m.Id == minIdS)
                .FirstOrDefault();
            Assert.NotNull(deleted);
            Assert.Equal(deleted.Name, "Name3SpouseChildren");

            changed = model.Children.Where(m => m.Id == maxId).FirstOrDefault();
            Assert.Equal(changed.Surname, "ChildrenSurnameChanged");
            Assert.Equal(changed.Name, "Name3Children");
            Assert.Equal(changed.SpouseSurname, "SpouseSurname3Children");
            Assert.Equal(changed.SpouseName, "SpouseName3Children");

            changed = model.SpouseChildren.Where(m => m.Id == maxIdS).FirstOrDefault();
            Assert.Equal(changed.Surname, "ChildrenSurnameChanged");
            Assert.Equal(changed.Name, "Name3SpouseChildren");
            Assert.Equal(changed.SpouseSurname, "ChildrenSurnameSpouseChanged");
            Assert.Equal(changed.SpouseName, "SpouseName3SpouseChildren");

            var added = model.Children.Where(m => m.Name == "AddedName").FirstOrDefault();
            Assert.NotNull(added);
            Assert.Equal(added.Surname, "AddedSurname");

            added = model.SpouseChildren.Where(m => m.Name == "AddedName").FirstOrDefault();
            Assert.NotNull(added);
            Assert.Equal(added.Surname, "AddedSurname");
        }

    }
}
