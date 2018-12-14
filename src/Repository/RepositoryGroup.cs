using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFCoreSugar.Repository
{
    public abstract class RepositoryGroup<TEntity> : IRepositoryGroup where TEntity : class
    {
        public IBaseDbRepository ParentBaseRepository { get; set; }

        public virtual TEntity GetSingle(object key)
        {
            return ParentBaseRepository.GetSingle<TEntity>(key);
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return ParentBaseRepository.GetAll<TEntity>();
        }

        public virtual void Update(TEntity entity)
        {
            ParentBaseRepository.Update(entity);
        }

        protected virtual IQueryable<TEntity> GetQueryable()
        {
            return ParentBaseRepository.GetQueryable<TEntity>();
        }

        public virtual TEntity Create(TEntity entity)
        {
            return ParentBaseRepository.Create(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            ParentBaseRepository.Delete(entity);
        }
        protected DbSet<T> Set<T>() where T : class
        {
            return ParentBaseRepository.DBContext.Set<T>();
        }
    }
}
