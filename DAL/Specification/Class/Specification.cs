using BLL.Specification.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BLL.Specification.Class
{
    public class Specification<T> : ISpecification<T> where T : class
    {
        public Expression<Func<T, bool>>? Criteria { get; set; }
        public List<Expression<Func<T, bool>>>? CriteriaList { get; set; } = new List<Expression<Func<T, bool>>>();
        public List<Expression<Func<T, object>>> Includes { get; }
          = new();

        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }

        public int? Take { get; set; }
        public int? Skip { get; set; }
        public bool isPaging { get; set; }
        public int? Count { get; set; }
        public Specification() { }

        public Specification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            isPaging = true;
        }
        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        int? ISpecification<T>.Take { get => Take; set => Take = value; }
        int? ISpecification<T>.Skip { get => Skip; set => Skip = value; }
        bool ISpecification<T>.isPaging { get => isPaging; set => isPaging = value; }
        int? ISpecification<T>.Count { get => Count; set => Count = value; }
    }
}

