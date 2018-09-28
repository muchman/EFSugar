using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.FakeDatabase;
using Xunit;

namespace Tests.FilterTestGoup
{
    public class FilterTests
    {
        [Fact]
        public void PredicateTest()
        {
            using (var db = new TestDbContext())
            {
                db.Add(new TestClass() { Id = 1, Name = "Roger" });
                db.Add(new TestClass() { Id = 2, Name = "Bilbo" });
                db.SaveChanges();

                var filter = new TestFilter() { Name = "Bilbo" };

                var query = db.Set<TestClass>().AsQueryable();
                query = filter.ApplyFilter(query);
                //query = query.Where(t => t.Name == "Bilbo");
                var results = query.ToList();

            }
        }
    }
}
