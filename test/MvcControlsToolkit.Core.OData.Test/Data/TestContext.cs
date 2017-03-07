﻿using System;
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
    }
}