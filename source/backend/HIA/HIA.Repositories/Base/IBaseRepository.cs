using System.Linq.Expressions;
using HIA.Core.Models.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HIA.Repositories.Base;

public interface IBaseRepository<T> where T : class
{
    DbSet<T> DbSet { get; }

    #region GET
    Task<T?> GetById(dynamic id);
    IQueryable<T> GetCount(Expression<Func<T, bool>>? filter = null);
    Task<TKey> GetMinMax<TKey>(bool isMax, Expression<Func<T, TKey>> selector);
    Task<T?> GetItemOrderBy(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null);
    Task<T?> GetItem(Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null);

    Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>>? filter = null
        , Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
        , Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null);

    Task<PaginationResponseModel<T>> GetListPaginationAsync(
        int pageIndex
        , int pageSize
        , Expression<Func<T, bool>>? filter = null
        , Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null
        , Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null
    );

    Task<PaginationResponseModel<T>> GetListPaginationCustomSortAsync(
        int pageIndex
        , int pageSize
        , Expression<Func<T, bool>>? filter = null
        , IEnumerable<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>>? orderBy = null
        , Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null
    );

    Task<IEnumerable<T>> GetAllCustomSortAsync(
        Expression<Func<T, bool>>? filter = null
        , IEnumerable<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>>? orderBy = null
        , Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null
    );
    #endregion

    #region UPDATE
    Task InsertAsync(T entity);

    void InsertList(List<T> entity);
    Task InsertListAsync(List<T> entity);
    Task InsertAsBaseEntityAsync(T entity);

    void Update(T entity);

    void UpdateList(IEnumerable<T> entity);

    Task UpdateListAsync(IEnumerable<T> entity);

    void UpdateAsBaseEntity(T entity);

    void UpdateRemove(T entity);
    void UpdateRemoveAsBaseEntity(T entity);

    void Delete(T entity);
    void DeleteList(List<T> entity);
    void DeleteAll();
    void DeleteListByFilter(Expression<Func<T, bool>> filter);

    Task DeleteListByFilterAsync(Expression<Func<T, bool>> filter);
    #endregion

    Task SaveChangesAsync();

    Task SaveChangesAsyncNoBulk();
    Task<bool> AnyAsync(
        Expression<Func<T, bool>>? filter,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? includeProperties = null);

    Task<int> CallProcedure(string command, params object[] parameters);

    Task<int> CountCustomAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderFunc = null
    );
}
