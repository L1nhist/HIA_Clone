using HIA.Core.Base;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Repositories.Base
{

    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        //private DbContext _context;
        private readonly DbFactory _dbFactory;
        private readonly DbSet<T> _dbSet;
        private readonly IHttpContextAccessor _httpContext;
        //private readonly ILogger<BaseRepository<T>> _logger;

        public BaseRepository(DbFactory dbFactory,
            IHttpContextAccessor httpContext)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _dbSet = _dbFactory.DbContext.Set<T>();
            _httpContext = httpContext;
            //_logger = logger;
             Z.EntityFramework.Extensions.LicenseManager.AddLicense("REDACTED", "REDACTED");
        }

        public DbSet<T> DbSet { get => _dbSet; }

        #region GET
        // Get item by Id
        public async Task<T> GetById(dynamic id)
        {
            return await _dbSet.FindAsync(id);
        }

        public IQueryable<T> GetCount(Expression<Func<T, bool>> filter = null)
        {
            return _dbSet.AsNoTracking().Where(filter);
        }

        public async Task<TKey> GetMinMax<TKey>(bool isMax, Expression<Func<T, TKey>> selector)
        {
            return (isMax ? await _dbSet.AsNoTracking().MaxAsync(selector) : await _dbSet.AsNoTracking().MinAsync(selector)) ?? default;
        }

        // Get item by condition and include
        public async Task<T> GetItem(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            var query = _dbSet.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (includeProperties != null)
            {
                query = includeProperties(query);
            }

            return await query.FirstOrDefaultAsync();
        }


        public async Task<T> GetItemOrderBy(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            var query = _dbSet.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            if (includeProperties != null)
            {
                query = includeProperties(query);
            }

            return await query.FirstOrDefaultAsync();
        }

        // Get list by condition and include, order
        public async Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>> filter = null
        , Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null
        , Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            var query = _dbSet.AsNoTracking();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
                query = includeProperties(query);

            if (orderBy != null)
                return await orderBy(query).ToListAsync();
            else
                return await query.ToListAsync();
        }

        // Get list with pagination by condition and include, order
        public async Task<PaginationResponseModel<T>> GetListPaginationAsync(
            int pageIndex
            , int pageSize
            , Expression<Func<T, bool>> filter = null
            , Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null
            , Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            IEnumerable<T> result;
            var query = _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                query = includeProperties(query);
            }

            var totalItems = query.Count();

            if (orderBy != null)
            {
                result = await orderBy(query).Pagination(pageIndex, pageSize).ToListAsync();
            }
            else
            {
                result = await query.Pagination(pageIndex, pageSize).ToListAsync();
            }
            return new PaginationResponseModel<T>(totalItems, result);

        }

        public async Task<PaginationResponseModel<T>> GetListPaginationCustomSortAsync(
            int pageIndex
            , int pageSize
            , Expression<Func<T, bool>> filter = null
            , IEnumerable<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>> orderFuncList = null
            , Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            IEnumerable<T> result;
            //var query = _dbSet.AsQueryable();
            //khoikhoi chưa biết check null filter chỗ này khởi tạo query là ji`
            //haimaster 11/10/2022
            DateTime dd = DateTime.Now;

            //haimaster 11/10/2022 bo asNotacking
            var query = _dbSet.AsNoTracking().Where(filter);
            TimeSpan tss = DateTime.Now - dd;
            //Uti.HandleCronJobLog(_logger, $"H3.Haimaster Load XML1 DS) () - Total time: {tss.TotalMilliseconds}", nameof(BaseRepository<T>.GetListPaginationCustomSortAsync));

            if (includeProperties != null)
            {
                query = includeProperties(query);
            }

            var totalItems = query.Count();

            if (orderFuncList != null && orderFuncList.Any())
            {
                IOrderedQueryable<T> orderQueryable = query.OrderBy(o => true);
                foreach (var orderFunc in orderFuncList)
                {
                    orderQueryable = orderFunc(orderQueryable);
                }
                result = await orderQueryable.Pagination(pageIndex, pageSize).ToListAsync();
            }
            else
            {
                result = await query.Pagination(pageIndex, pageSize).ToListAsync();
            }

            //var sql = ((System.Data.Entity.Core.Objects.ObjectQuery)query).ToTraceString();

            //var sql = query.ToQueryString();

            return new PaginationResponseModel<T>(totalItems, result);
        }

        public async Task<IEnumerable<T>> GetAllCustomSortAsync(
            Expression<Func<T, bool>> filter = null
            , IEnumerable<Func<IOrderedQueryable<T>, IOrderedQueryable<T>>> orderFuncList = null
            , Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            IEnumerable<T> result;
            var query = _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                query = includeProperties(query);
            }


            if (orderFuncList != null && orderFuncList.Any())
            {
                IOrderedQueryable<T> orderQueryable = query.OrderBy(o => true);
                foreach (var orderFunc in orderFuncList)
                {
                    orderQueryable = orderFunc(orderQueryable);
                }
                result = await orderQueryable.ToListAsync();
            }
            else
            {
                result = await query.ToListAsync();
            }
            return result;

        }
        #endregion

        #region UPDATE
        public async Task InsertAsync(T entity)
        {
            _dbFactory.DbContext.Entry(entity);
            //await _dbSet.SingleInsertAsync(entity);
            await _dbSet.AddAsync(entity);
        }

        public void InsertList(List<T> entity)
        {
            // Batched AddRange + SaveChanges to avoid large single transaction/locks
            if (entity == null || entity.Count == 0) return;
            var ctx = _dbFactory.DbContext;
            var batchSize = 1000;
            try
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                for (int i = 0; i < entity.Count; i += batchSize)
                {
                    var batch = entity.Skip(i).Take(batchSize).ToList();
                    _dbSet.AddRange(batch);
                    ctx.SaveChanges();
                    ctx.ChangeTracker.Clear();
                }
            }
            finally
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public async Task InsertListAsync(List<T> entity)
        {
            //_dbFactory.DbContext.Entry(entity);
            await _dbSet.AddRangeAsync(entity);
            //await _dbSet.BulkInsertAsync(entity);
        }

        public async Task InsertAsBaseEntityAsync(T entity)
        {
            (entity as dynamic).Id = Guid.NewGuid().ToString();
            (entity as dynamic).CreatedDate = DateTime.Now;
            (entity as dynamic).IsDeleted = false;

            var userId = _httpContext.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                (entity as dynamic).CreatedBy = userId;

            _dbFactory.DbContext.Entry(entity);
            await _dbSet.SingleInsertAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Attach(entity);
            _dbFactory.DbContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateList(IEnumerable<T> entity)
        {
            _dbSet.UpdateRange(entity);
        }

        public async Task UpdateListAsync(IEnumerable<T> entity)
        {
            if (entity == null) return;
            var list = entity as IList<T> ?? entity.ToList();
            if (list.Count == 0) return;

            var ctx = _dbFactory.DbContext;
            var batchSize = 1000;
            try
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                for (int i = 0; i < list.Count; i += batchSize)
                {
                    var batch = list.Skip(i).Take(batchSize).ToList();
                    _dbSet.UpdateRange(batch);
                    await ctx.SaveChangesAsync();
                    ctx.ChangeTracker.Clear();
                }
            }
            finally
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }
        public void UpdateAsBaseEntity(T entity)
        {
            (entity as dynamic).UpdatedDate = DateTime.Now;
            var userId = _httpContext.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                (entity as dynamic).UpdatedBy = userId;

            _dbSet.Attach(entity);
            _dbFactory.DbContext.Entry(entity).State = EntityState.Modified;
        }

        // soft delete
        public void UpdateRemove(T entity)
        {
            _dbFactory.DbContext.Entry(entity).State = EntityState.Modified;
            _dbSet.Attach(entity);
        }
        public void UpdateRemoveAsBaseEntity(T entity)
        {
            (entity as dynamic).UpdatedDate = DateTime.Now;
            (entity as dynamic).IsActivated = false;
            (entity as dynamic).IsDeleted = true;

            var userId = _httpContext.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
                (entity as dynamic).UpdatedBy = userId;

            _dbFactory.DbContext.Entry(entity).State = EntityState.Modified;
            _dbSet.Attach(entity);
        }

        // hard delete
        public void Delete(T entity)
        {
            //_dbFactory.DbContext.Entry(entity).State = EntityState.Deleted;
            _dbSet.SingleDelete(entity);
        }

        public void DeleteList(List<T> entity)
        {
            if (entity == null || entity.Count == 0) return;
            var ctx = _dbFactory.DbContext;
            var batchSize = 1000;
            try
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                for (int i = 0; i < entity.Count; i += batchSize)
                {
                    var batch = entity.Skip(i).Take(batchSize).ToList();
                    _dbSet.RemoveRange(batch);
                    ctx.SaveChanges();
                    ctx.ChangeTracker.Clear();
                }
            }
            finally
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }
        public void DeleteAll()
        {
            // Delete in batches to avoid long-running single transaction/locks
            var ctx = _dbFactory.DbContext;
            var batchSize = 1000;
            try
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                while (true)
                {
                    var batch = _dbSet.AsNoTracking().Take(batchSize).ToList();
                    if (!batch.Any()) break;
                    _dbSet.RemoveRange(batch);
                    ctx.SaveChanges();
                    ctx.ChangeTracker.Clear();
                }
            }
            finally
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }
        public void DeleteListByFilter(Expression<Func<T, bool>> filter)
        {
            if (filter == null) return;

            var ctx = _dbFactory.DbContext;
            var query = _dbSet.Where(filter);
            var toDelete = query.ToList();
            if (!toDelete.Any()) return;

            var batchSize = 1000;
            try
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = false;
                for (int i = 0; i < toDelete.Count; i += batchSize)
                {
                    var batch = toDelete.Skip(i).Take(batchSize).ToList();
                    _dbSet.RemoveRange(batch);
                    ctx.SaveChanges();
                    ctx.ChangeTracker.Clear();
                }
            }
            finally
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public Task DeleteListByFilterAsync(Expression<Func<T, bool>> filter)
        {
            if (filter == null) return Task.CompletedTask;

            var entities = _dbSet.Where(filter);
            //await _dbSet.BulkDeleteAsync(entities);
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
        #endregion

        public async Task SaveChangesAsync()
        {
            await _dbFactory.DbContext.SaveChangesAsync();
            _dbFactory.DbContext.ChangeTracker.Clear();
        }


        public async Task SaveChangesAsyncNoBulk()
        {
            await _dbFactory.DbContext.SaveChangesAsync();
            _dbFactory.DbContext.ChangeTracker.Clear();
        }

        public async Task<bool> AnyAsync(
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includeProperties = null)
        {
            var query = _dbSet.AsNoTracking();

            if (includeProperties != null)
            {
                query = includeProperties(query);
            }

            if (filter != null)
            {
                return await query.AnyAsync(filter);
            }

            return await query.AnyAsync();
        }

        public async Task<int> CallProcedure(string command, params object[] parameters)
        {
            return await _dbFactory.DbContext.Database.ExecuteSqlRawAsync(command, parameters);
        }
        public async Task<int> CountCustomAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderFunc = null
        )
        {
            var query = _dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderFunc != null)
            {
                IOrderedQueryable<T> orderQueryable = query.OrderBy(o => true);
                query = orderFunc(orderQueryable);
            }

            return await query.CountAsync();
        }
    }
}
