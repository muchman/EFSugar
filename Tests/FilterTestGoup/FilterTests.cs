using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.FakeDatabase;
using Xunit;
using System.Linq.Expressions;
using EFSugar;
using FluentAssertions;

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
                var results = query.ToList();
               
            }
        }

        [Fact]
        public void PredicateGroupingTest()
        {

            Expression<Func<TestClass2, bool>> one = tc => tc.One == 1;
            Expression<Func<TestClass2, bool>> two = tc => tc.Two == 1;
            Expression<Func<TestClass2, bool>> three = tc => tc.Three == 1;
            Expression<Func<TestClass2, bool>> four = tc => tc.Four == 1;

            //(true && false) && (true || true)


            var g1 = one.And(two);
            var g2 = two.Or(three);
            //g1.And(g2);

            //(true && false) && (true || true)
            var correct = one.And(two).And(three.Or(four));

            //((true && false) && true) || true)
            var wrong = one.And(two).And(three).Or(four);


            using (var db = new TestDbContext())
            {
                db.Add(new TestClass2() {Id = 1, One = 1, Two = 2, Three = 1, Four = 1 });
                db.SaveChanges();


                var query = db.Set<TestClass2>().AsQueryable();

                query.Where(wrong).ToList().Count.Should().Be(1);
                query.Where(correct).ToList().Count.Should().Be(0);
            }
        }
    }
}
