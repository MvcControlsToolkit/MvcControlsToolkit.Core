using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcControlsToolkit.Core.OData.Test.Models;

namespace MvcControlsToolkit.Core.OData.Test.Data
{
    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=OData-Test-32abf61e-f504-462a-8c8a-b04e55d505bt;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
        public DbSet<ReferenceModel> ReferenceModels { get; set; }
        public DbSet<NestedReferenceModel> NestedReferenceModels { get; set; }
        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ReferenceModel>()
                .HasMany(m => m.Children)
                .WithOne(m => m.Father)
                .HasForeignKey(m => m.FatherId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Person>()
                .HasOne(m => m.Spouse)
                .WithOne(m => m.SpouseOf)
                .HasForeignKey<Person>(m => m.SpouseOfId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.Entity<Person>()
                .HasMany(m => m.Children)
                .WithOne(m => m.Parent)
                .HasForeignKey(m => m.ParentId);


        }
    }
}
