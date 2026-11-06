using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace HIA.Core.Models.Pagination;

public class FilterQueryableModel<T>
{
    public Expression<Func<T, bool>>? FilterQueryable { get; set; }
    public IEnumerable<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>>? OrderQueryable { get; set; }
    public Func<IQueryable<T>, IIncludableQueryable<T, object>>? IncludeQueryable { get; set; }
}
