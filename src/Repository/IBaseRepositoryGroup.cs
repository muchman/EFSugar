using System;
using System.Collections.Generic;
using System.Text;

namespace EFCoreSugar.Repository.Interfaces
{
    public interface IBaseRepositoryGroup
    {
        IBaseDbRepository ParentBaseRepository { get; set; }
    }
}
