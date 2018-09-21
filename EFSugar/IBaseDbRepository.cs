using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSugar
{
    public interface IBaseDbRepository
    {
        TEntity Create<TEntity>(TEntity entity) where TEntity : class;
        void Delete<TEntity>(TEntity entity) where TEntity : class;
        void SaveChanges();
        void SetDeferred(bool defer);
        void Update<TEntity>(TEntity entity) where TEntity : class;
        IQueryable<TEntity> GetQueryable<TEntity>(bool trackChanges = true) where TEntity : class;
        IEnumerable<TEntity> GetAll<TEntity>(bool trackChanges = true) where TEntity : class;
        TEntity GetSingle<TEntity>(object key, bool trackChanges = true) where TEntity : class;
    }
}
