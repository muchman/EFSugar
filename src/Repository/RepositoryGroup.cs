using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSugar.Repository
{
    public abstract class RepositoryGroup<TEntity> : IRepositoryGroup<TEntity> where TEntity : class
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

        public virtual int Update(TEntity entity)
        {
            return ParentBaseRepository.Update(entity);
        }

        public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ParentBaseRepository.UpdateAsync(entity, cancellationToken);
        }

        protected virtual IQueryable<TEntity> GetQueryable()
        {
            return ParentBaseRepository.GetQueryable<TEntity>();
        }

        protected virtual IQueryable<T> GetQueryable<T>() where T : class
        {
            return ParentBaseRepository.GetQueryable<T>();
        }

        public virtual TEntity Create(TEntity entity)
        {
            return ParentBaseRepository.Create(entity);
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ParentBaseRepository.CreateAsync(entity, cancellationToken);
        }

        public virtual int Delete(TEntity entity)
        {
            return ParentBaseRepository.Delete(entity);
        }

        public virtual async Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ParentBaseRepository.DeleteAsync(entity, cancellationToken);
        }

        protected DbSet<T> Set<T>() where T : class
        {
            return ParentBaseRepository.Set<T>();
        }

        protected DbQuery<T> Query<T>() where T : class
        {
            return ParentBaseRepository.Query<T>();
        }

        protected void RecycleDbContext()
        {
            ParentBaseRepository.RecycleDbContext();
        }
    }
}
