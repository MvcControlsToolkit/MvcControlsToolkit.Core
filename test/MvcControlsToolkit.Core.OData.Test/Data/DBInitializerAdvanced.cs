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
    public class DBInitializerAdvanced : IDisposable
    {
        static DBInitializerAdvanced()
        {
            DefaultCRUDRepository<TestContext, Person>
                .DeclareProjection(m => new PersonDTOFlattened
                {
                    Children = m.Children.Select(l => new PersonDTOFlattened { }),
                    SpouseChildren = m.Spouse.Children.Select(l => new PersonDTOFlattened { })
                });
            DefaultCRUDRepository<TestContext, Person>
                .DeclareUpdateProjection<PersonDTOFlattened>
                (
                    m =>
                    
                    new Person
                    {
                        Spouse = m.SpouseName == null ? null : new Person
                        {
                            Children= m.SpouseChildren.Select(l => new Person {Spouse = l.SpouseName == null ? null : new Person { }  }).ToList()
                        }, 
                        Children=  m.Children.Select(l => new Person { }).ToList()
                    }
                );
            DefaultCRUDRepository<TestContext, Person>
                .DeclareProjection(m => new PersonDTOFlattenedAuto
                {
                    Children = m.Children.Select(l => new PersonDTOFlattened { }),
                    SpouseChildren = m.Spouse.Children.Select(l => new PersonDTOFlattened { })
                });
            DefaultCRUDRepository<TestContext, Person>
                .DeclareProjection(m => new PersonDTO
                {
                    Children = m.Children.Select(l => new PersonDTO { }),
                    Spouse = new PersonDTO {Children= m.Spouse.Children.Select(l => new PersonDTO { }) } 
                });
            DefaultCRUDRepository<TestContext, Person>
                .DeclareProjection(m => new PersonDTOAuto
                {
                    Children = m.Children.Select(l => new PersonDTO {Spouse=new PersonDTO { }  }),
                    Spouse = new PersonDTO { Children = m.Spouse.Children.Select(l => new PersonDTO { Spouse = new PersonDTO { } }) }
                });
            DefaultCRUDRepository<TestContext, Person>
                .DeclareUpdateProjection<PersonDTO>
                (
                    m =>
                    new Person
                    {

                        Spouse =
                        m.Spouse == null ? null :
                        new Person
                        {
                            Children = m.Spouse.Children.Select(l => new Person {  }).ToList()
                        },
                        Children = m.Children.Select(l => new Person {  }).ToList()
                    }
                );
        }
        private TestContext context;
        ICRUDRepository repository;
        public DBInitializerAdvanced()
        {
            context = new TestContext();
            repository = new DefaultCRUDRepository<TestContext, Person>(context, context.Persons);
            context.Database.Migrate();
            context.Database.ExecuteSqlCommand("delete from Persons");
            context.SaveChanges();
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Persons, RESEED, 0)");
            context.SaveChanges();
            for (int i=0; i<4; i++)
            {
                Person model = new Person
                {
                    Name = "Root" + i,
                    Surname = "Surname" + i,
                    Spouse = new Person
                    {
                        Name = "SpouseName" + i,
                        Surname = "SpouseSurname" + i
                    }
                };
                var children = new List<Person>();
                var spouseChildren = new List<Person>();
                for (int j=0; j<4; j++)
                {
                    children.Add(new Person
                    {
                        Name = "Name" + i + "Children",
                        Surname = "Surname" + i + "Children",
                        Spouse = new Person
                        {
                            Name = "SpouseName" + i + "Children",
                            Surname = "SpouseSurname" + i + "Children"
                        }
                    });
                }
                for (int j = 0; j < 4; j++)
                {
                    spouseChildren.Add(new Person
                    {
                        Name = "Name" + i + "SpouseChildren" ,
                        Surname = "Surname" + i + "SpouseChildren",
                        Spouse = new Person
                        {
                            Name = "SpouseName" + i + "SpouseChildren",
                            Surname = "SpouseSurname" + i + "SpouseChildren"
                        }
                    });
                }
                model.Children = children;
                model.Spouse.Children = spouseChildren;
                context.Persons.Add(model);
                context.SaveChanges();
            }
        }
        public ICRUDRepository Repository { get { return repository; } }
        public TestContext Context { get { return context; } }
        public void Dispose()
        {
            context.Dispose();
        }
    }
    
}
