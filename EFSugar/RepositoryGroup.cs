using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFSugar
{
    public interface IRepositoryGroup
    {

    }
    public abstract class RepositoryGroup<TEntity, TKey> : IRepositoryGroup where TEntity : class
    {
        protected DbContext DBContext;
        private IBaseDbRepository ParentBaseFunctions { get; set; }


        public RepositoryGroup(DbContext context, IBaseDbRepository parent)
        {
            DBContext = context;
            ParentBaseFunctions = parent;
        }

        public RepositoryGroup()
        {
        }
        public virtual TEntity GetSingle(TKey key)
        {
            return ParentBaseFunctions.GetSingle<TEntity>(key);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return ParentBaseFunctions.GetAll<TEntity>();
        }

        public virtual void Update(TEntity entity)
        {
            ParentBaseFunctions.Update(entity);
        }

        protected virtual IQueryable<TEntity> GetQueryable()
        {
            return ParentBaseFunctions.GetQueryable<TEntity>();
        }

        public virtual TEntity Create(TEntity entity)
        {
            return ParentBaseFunctions.Create(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            ParentBaseFunctions.Delete(entity);
        }
        protected DbSet<T> Set<T>() where T : class
        {
            return DBContext.Set<T>();
        }
    }
}
