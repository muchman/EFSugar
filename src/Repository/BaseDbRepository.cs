using EFCoreSugar.Global;
using EFCoreSugar.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSugar.Repository
{
    public abstract class BaseDbRepository<T> : IBaseDbRepository where T : DbContext
    {
        public DbContext DBContext { get; }
        private bool _deferred;

        public BaseDbRepository(T context, IServiceProvider serviceProvider)
        {
            DBContext = context;
            var thisType = this.GetType();

            if (!EFCoreSugarPropertyCollection.BaseDbRepositoryGroupTypeProperties.TryGetValue(thisType, out var props))
            {
                props = EFCoreSugarPropertyCollection.RegisterBaseDbRepositoryGroupProperties(thisType);
            }
            
            foreach (var prop in props)
            {
                var group = serviceProvider.GetService(prop.PropertyType) as IBaseRepositoryGroup;
                group.ParentBaseRepository = this;
                prop.SetValue(this, group);
            }
        }

        public TEntity Create<TEntity>(TEntity entity) where TEntity : class
        {
            DBContext.Set<TEntity>().Add(entity);
            if (!_deferred)
            {
                DBContext.SaveChanges();
            }
            return entity;
        }

        public async Task<TEntity> CreateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
        {
            await DBContext.Set<TEntity>().AddAsync(entity);
            if (!_deferred)
            {
                await DBContext.SaveChangesAsync(cancellationToken);
            }
            return entity;
        }

        public int Delete<TEntity>(TEntity entity) where TEntity : class
        {
            DBContext.Set<TEntity>().Remove(entity);
            if (!_deferred)
            {
                return DBContext.SaveChanges();
            }
            return 0;
        }

        public async Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken) ) where TEntity : class
        {
            DBContext.Set<TEntity>().Remove(entity);
            if (!_deferred)
            {
                return await DBContext.SaveChangesAsync(cancellationToken);
            }
            return 0;
        }

        public int Update<TEntity>(TEntity entity) where TEntity : class
        {
            DBContext.Entry(entity).State = EntityState.Modified;
            if (!_deferred)
            {
                return DBContext.SaveChanges();
            }
            return 0;
        }

        public async Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class
        {
            DBContext.Entry(entity).State = EntityState.Modified;
            if (!_deferred)
            {
               return await DBContext.SaveChangesAsync(cancellationToken);
            }
            return 0;
        }

        public void SetDeferred(bool defer)
        {
            _deferred = defer;
        }

        public int SaveChanges()
        {
            return DBContext.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await DBContext.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<TEntity> GetQueryable<TEntity>(bool trackChanges = true) where TEntity : class
        {
            try
            {
                var dbset = DBContext.Set<TEntity>();
                if (trackChanges)
                {
                    return dbset.AsQueryable();
                }
                else
                {
                    return dbset.AsNoTracking();
                }
            }
            catch(InvalidCastException ex)
            { 
                var dbquery = DBContext.Query<TEntity>();
                if(dbquery != null)
                {
                    if (trackChanges)
                    {
                        return dbquery.AsQueryable();
                    }
                    else
                    {
                        return dbquery.AsNoTracking();
                    }
                }
                else
                {
                    throw new Exception($"No Set or DbQuery of type {typeof(TEntity)} found.");
                }
            }
        }

        public IEnumerable<TEntity> GetAll<TEntity>(bool trackChanges = true) where TEntity : class
        {
            return GetQueryable<TEntity>(trackChanges).ToList();
        }

        public TEntity GetSingle<TEntity>(object key, bool trackChanges = true) where TEntity : class
        {
            return DBContext.Set<TEntity>().Find(key);
        }
    }
}
