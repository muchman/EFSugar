using EFCoreSugar.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.FakeDatabase
{
    public class FakeRepo : BaseDbRepository<TestDbContext>, IFakeRepo
    {
        public IUserRepositoryGroup UserGroup { get; set; }
        public UserRepositoryGroup ConcreteUserGroup { get; set; }
        public FakeRepo(TestDbContext context, IServiceProvider provider) : base(context, provider) { }
    }

    public interface IFakeRepo : IBaseDbRepository
    {
        IUserRepositoryGroup UserGroup { get; set; }
    }
}
