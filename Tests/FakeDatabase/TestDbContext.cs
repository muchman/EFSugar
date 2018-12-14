using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeEntities;
using Tests.FilterTestGoup;

namespace Tests.FakeDatabase
{
    public class TestDbContext : DbContext
    {
        DbSet<Order> Orders { get; set; }
        DbSet<User> Users{ get; set; }
        DbSet<OrderType> OrderTypes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        }
    }
}
