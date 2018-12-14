using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Repository
{
    public interface IRepositoryGroup
    {
        IBaseDbRepository ParentBaseRepository { get; set; }
    }
}
