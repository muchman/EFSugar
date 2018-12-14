using EFCoreSugar.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreSugar.Repository
{
    public interface IRepositoryGroup<TEntity> : IBaseRepositoryGroup where TEntity : class
    {
        TEntity GetSingle(object key);
        IEnumerable<TEntity> GetAll();
        void Update(TEntity entity);
        TEntity Create(TEntity entity);
        void Delete(TEntity entity);

    }
}
