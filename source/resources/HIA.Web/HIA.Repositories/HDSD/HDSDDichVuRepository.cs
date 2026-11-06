using HIA.Core.Base;
using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace HIA.Repositories.HDSD
{
    public class HDSDDichVuRepository : BaseRepository<HDSDDichVu>, IHDSDDichVuRepository
    {
        private readonly DbFactory _dbFactory;
        private DbSet<HDSDDichVu> _dbSet;
        private readonly IHttpContextAccessor _httpContext;

        public HDSDDichVuRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
        {

            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _dbSet = _dbFactory.DbContext.Set<HDSDDichVu>();
            _httpContext = httpContext;
        }

        private Expression<Func<HDSDDichVu, bool>> BuildFilter(string currentCSKCB = "", bool isMaster = false)
        {
            if (isMaster)
            {
                return PredicateBuilder.New<HDSDDichVu>(true);
            }
            
            return PredicateBuilder.New<HDSDDichVu>(x => x.MA_CSKCB == null || x.MA_CSKCB == "" || x.MA_CSKCB == currentCSKCB || (";" + x.MA_CSKCB + ";").Contains($";{currentCSKCB};"));
        }

        public async Task<IEnumerable<HDSDDichVu>> GetActiveRules(string currentCSKCB = "", bool isMaster = false)
        {
            Expression<Func<HDSDDichVu, bool>> filter = BuildFilter(currentCSKCB, isMaster);
            filter = filter.And(x => x.HIEU_LUC == true);
            return await GetListAsync(filter, null, null);
        }

        public async Task<PaginationResponseModel<HDSDDichVu>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false)
        {
            Expression<Func<HDSDDichVu, bool>> filter = BuildFilter(currentCSKCB, isMaster);

            // add filter when has searchKey
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.ToLower();
                filter = filter.And(x =>
                        x.MA_DICH_VU.Contains(key) || x.TEN_DICH_VU.ToLower().Contains(key) || x.MA_LOI.ToLower().Contains(key)
                    );
            }

            IList<Func<IOrderedQueryable<HDSDDichVu>, IOrderedQueryable<HDSDDichVu>>> sortFuncList = new List<Func<IOrderedQueryable<HDSDDichVu>, IOrderedQueryable<HDSDDichVu>>>();
            sortFuncList.Add(x => x.OrderByDescending(x => x.HDSDDVID));
            
            return await GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, sortFuncList, null);
        }
    }
}
