using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microservices.Common.Interfaces
{
    public interface IRepository<T> where T : IEntity
    {
        Task DeleteAsync(Guid Id);

        Task<IReadOnlyCollection<T>> GetAllAsync();

        Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T,bool>> filter);

        Task<T> GetByIdAsync(Guid Id);

        Task<T> GetByIdAsync(Expression<Func<T, bool>> filter);

        Task InsertAsync(T Item);
        
        Task UpdateAsync(T Item);
    }
}