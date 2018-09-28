using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FilterTestGoup;

namespace Tests.FakeDatabase
{
    public class TestDbContext : DbContext
    {
        DbSet<TestClass> TestClass { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "Testing_DB");
        }
    }
}
