using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSugar.Repository
{
    public interface IBaseDbRepository
    {
        DbContext DBContext { get; }
        TEntity Create<TEntity>(TEntity entity) where TEntity : class;
        Task<TEntity> CreateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class;
        int Delete<TEntity>(TEntity entity) where TEntity : class;
        Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        int Update<TEntity>(TEntity entity) where TEntity : class;
        Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class;
        void SetDeferred(bool defer);
        IQueryable<TEntity> GetQueryable<TEntity>(bool trackChanges = true) where TEntity : class;
        IEnumerable<TEntity> GetAll<TEntity>(bool trackChanges = true) where TEntity : class;
        TEntity GetSingle<TEntity>(object key, bool trackChanges = true) where TEntity : class;
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
#if FEATURE_2_0
        DbQuery<TEntity> Query<TEntity>() where TEntity : class;
#endif
        void RecycleDbContext();
    }
}
