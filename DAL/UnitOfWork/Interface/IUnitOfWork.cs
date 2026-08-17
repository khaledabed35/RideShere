using DAL.Reposetoriy;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace DAL.UnitOfWork.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepo<TEntity> GetRepository<TEntity>() where TEntity : class;
        Task<int> CompleteAsync();

        // أضف هذه الدوال لدعم الـ Transactions
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}