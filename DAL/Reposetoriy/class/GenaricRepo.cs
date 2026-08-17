using BLL.Specification.Interface;
using DAL.Data; // حسب اسم الـ namespace بتاع الـ DbContext عندك
using DAL.Specification.Class;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Reposetoriy
{
    public class GenaricRepo<T> : IGenaricRepo<T> where T : class
    {
        protected readonly AppDbcontext _context;

        public GenaricRepo(AppDbcontext context)
        {
            _context = context;
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        // تنفيذ دالة الـ ExistsAsync بشكل صحيح وسريع باستخدام AnyAsync
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }
        public async Task<T?> GetByIdWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        // دالة مساعدة لتطبيق الشروط والـ Includes الخاصة بالـ Specification على الـ Query
        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return Evaluator<T>.GetQuery(_context.Set<T>(), spec);
        }
    }
}