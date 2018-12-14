using EFCoreSugar.Global;
using EFCoreSugar.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            DBContext.Set<TEntity>().Remove(entity);
            if (!_deferred)
            {
                DBContext.SaveChanges();
            }
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            DBContext.Entry(entity).State = EntityState.Modified;
            if (!_deferred)
            {
                DBContext.SaveChanges();
            }
        }

        public void SetDeferred(bool defer)
        {
            _deferred = defer;
        }

        public void SaveChanges()
        {
            DBContext.SaveChanges();
        }

        public IQueryable<TEntity> GetQueryable<TEntity>(bool trackChanges = true) where TEntity : class
        {
            var dbset = DBContext.Set<TEntity>();
            IQueryable<TEntity> queryable;
            if (trackChanges)
            {
                queryable = dbset.AsQueryable();
            }
            else
            {
                queryable = dbset.AsNoTracking();
            }

            return queryable;
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
