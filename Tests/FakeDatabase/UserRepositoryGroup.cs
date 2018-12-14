using EFCoreSugar.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeEntities;

namespace Tests.FakeDatabase
{
    public class UserRepositoryGroup : RepositoryGroup<User>, IUserRepositoryGroup
    {
        public int GetUsersBySpecialMagic(string magicstuff)
        {
            //execute special queries in here and return a user

            return 1;
        }
    }

    public interface IUserRepositoryGroup : IRepositoryGroup<User>
    {
        int GetUsersBySpecialMagic(string magicstuff);
    }
}
