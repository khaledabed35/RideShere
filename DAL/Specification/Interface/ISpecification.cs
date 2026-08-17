using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BLL.Specification.Interface
{
    public interface ISpecification<T>where T : class
    {



        Expression<Func<T, bool>>? Criteria { get; set; }
        List<Expression<Func<T, bool>>>? CriteriaList { get; set; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>>? OrderBy { get;  }
        Expression<Func<T, object>>? OrderByDescending { get;  }




        public int? Take { get; set; }
        public int? Skip { get; set; }

        public int? Count { get; set; }
        public bool isPaging { get; set; }
    }
}