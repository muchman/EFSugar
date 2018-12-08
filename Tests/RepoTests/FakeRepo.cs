using EFSugar.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeDatabase;

namespace Tests.RepoTests
{
    public class FakeRepo : BaseDbRepository<TestDbContext>
    {
        public FakeRepo(TestDbContext context, IServiceProvider provider) : base(context, provider) { }
    }
}
