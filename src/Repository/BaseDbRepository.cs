using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EFCoreSugar.Repository
{
    public class BaseDbRepository<T> : IBaseDbRepository where T : DbContext
    {
        public DbContext DBContext { get; }
        private static IEnumerable<PropertyInfo> PropertyInfoCollection { get; set; }
        private bool _deferred;

        public BaseDbRepository(T context, IServiceProvider serviceProvider)
        {
            DBContext = context;

            if(PropertyInfoCollection == null)
            {
                PropertyInfoCollection = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => typeof(IRepositoryGroup).IsAssignableFrom(pi.PropertyType));
            }
            
            foreach (var prop in PropertyInfoCollection)
            {
                var group = serviceProvider.GetService(prop.PropertyType) as IRepositoryGroup;
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
