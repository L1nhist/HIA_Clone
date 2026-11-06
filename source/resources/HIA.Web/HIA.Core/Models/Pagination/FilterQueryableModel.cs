using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Pagination
{
    public class FilterQueryableModel<T>
    {
        public Expression<Func<T, bool>> FilterQueryable { get; set; }
        public IEnumerable<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>> OrderQueryable { get; set; }
        public Func<IQueryable<T>, IIncludableQueryable<T, object>> IncludeQueryable { get; set; }
    }
}
