using System;
using System.Collections.Generic;
using System.Text;

namespace EFSugar.Repository
{
    public interface IRepositoryGroup
    {
        IBaseDbRepository ParentBaseRepository { get; set; }
    }
}
