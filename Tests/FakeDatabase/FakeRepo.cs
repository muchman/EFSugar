using EFCoreSugar.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.FakeDatabase
{
    public class FakeRepo : BaseDbRepository<TestDbContext>
    {
        public FakeRepo(TestDbContext context, IServiceProvider provider) : base(context, provider) { }
    }
}
