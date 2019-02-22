using EFCoreSugar.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSugar.Repository
{
    public interface IRepositoryGroup<TEntity> : IBaseRepositoryGroup where TEntity : class
    {
        TEntity GetSingle(object key);
        IEnumerable<TEntity> GetAll();
        int Update(TEntity entity);
        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        TEntity Create(TEntity entity);
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
        int Delete(TEntity entity);
        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));
    }
}
