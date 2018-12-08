using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.FakeDatabase;
using Xunit;
using System.Linq.Expressions;
using EFSugar;
using FluentAssertions;
using EFSugar.Filters;

namespace Tests.FilterTestGoup
{
    public class FilterTests
    {
        //[Fact]
        //public void PredicateTest()
        //{
        //    using (var db = new TestDbContext())
        //    {
        //        db.Add(new TestClass() { Id = 1, Name = "Roger", Balance = 5});
        //        db.Add(new TestClass() { Id = 2, Name = "Bilbo", Balance = 9});
        //        db.Add(new TestClass() { Id = 3, Name = "Bilbo", Balance = 20 });
        //        db.SaveChanges();

        //        var filter = new TestFilter() { NameNotName = "Bilbo", Balance = 10 };

        //        var query = db.Set<TestClass>().AsQueryable();


        //        var results = query.ToList();
        //        results.Count.Should().Be(1);
        //        results.First().Name.Should().Be("Bilbo");
        //        results.First().Balance.Should().Be(9);

        //    }
        //}

        //[Fact]
        //public void PredicateGroupingTest()
        //{

        //    Expression<Func<TestClass2, bool>> one = tc => tc.One == 1;
        //    Expression<Func<TestClass2, bool>> two = tc => tc.Two == 1;
        //    Expression<Func<TestClass2, bool>> three = tc => tc.Three == 1;
        //    Expression<Func<TestClass2, bool>> four = tc => tc.Four == 1;

        //    //(true && false) && (true || true)


        //    var g1 = one.And(two);
        //    var g2 = two.Or(three);
        //    //g1.And(g2);

        //    //(true && false) && (true || true)
        //    var correct = one.And(two).And(three.Or(four));

        //    //((true && false) && true) || true)
        //    var wrong = one.And(two).And(three).Or(four);


        //    using (var db = new TestDbContext())
        //    {
        //        db.Add(new TestClass2() {Id = 1, One = 1, Two = 2, Three = 1, Four = 1 });
        //        db.SaveChanges();


        //        var query = db.Set<TestClass2>().AsQueryable();

        //        query.Where(wrong).ToList().Count.Should().Be(1);
        //        query.Where(correct).ToList().Count.Should().Be(0);
        //    }
        //}

        //[Fact]
        //public void SortByTest()
        //{
        //    using (var db = new TestDbContext())
        //    {
        //        db.Add(new TestClass2() { Id = 1, One = 1});
        //        db.Add(new TestClass2() { Id = 2, One = 3 });
        //        db.Add(new TestClass2() { Id = 3, One = 2 });
        //        db.Add(new TestClass2() { Id = 4, One = 5 });
        //        db.Add(new TestClass2() { Id = 5, One = 4 });
        //        db.Add(new TestClass2() { Id = 6, One = 7 });
        //        db.Add(new TestClass2() { Id = 7, One = 6 });
        //        db.SaveChanges();

        //        var filter = new TestFilter2();
        //        filter.SetOrderBy(f => f.One, OrderByDirection.Ascending);

        //        var query = db.Set<TestClass2>().AsQueryable();
        //        var res = filter.ApplyFilter(query);
        //        var results = res.Query.ToList();

        //    }
        //}
    }
}
