using BLL.Specification.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DAL.Reposetoriy
{
    public  interface IGenaricRepo<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();

        Task<T?> GetByIdAsync(object id);

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdWithSpecAsync(ISpecification<T> spec);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task <bool> SaveChangesAsync();
        Task<T?> GetByAsync(Expression<Func<T, bool>> predicate);
    }
}
