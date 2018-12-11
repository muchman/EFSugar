using EFSugar.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using Tests.FakeEntities;

namespace Tests.FakeDatabase
{
    public class UserRepositoryGroup : RepositoryGroup<User>
    {
        public User GetUsersBySpecialMagic(string magicstuff)
        {
            //execute special queries in here and return a user

            return null;
        }
    }
}
