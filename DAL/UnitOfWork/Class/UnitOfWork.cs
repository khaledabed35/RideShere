using DAL.Data;
using DAL.Reposetoriy;
using DAL.UnitOfWork.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbcontext _context;
        private Hashtable _repositories;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(AppDbcontext context)
        {
            _context = context;
        }

        public IGenaricRepo<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenaricRepo<>);
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(TEntity)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IGenaricRepo<TEntity>)_repositories[type];
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // --- دعم الـ Transactions بشكل احترافي ---

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_currentTransaction != null) return null;
            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}