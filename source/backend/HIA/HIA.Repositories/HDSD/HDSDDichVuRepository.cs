using System.Linq.Expressions;
using HIA.Core.Base;
using HIA.Core.Models.Pagination;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using LinqKit;
using Microsoft.AspNetCore.Http;

namespace HIA.Repositories.HDSD;

public class HDSDDichVuRepository : BaseRepository<HDSDDichVu>, IHDSDDichVuRepository
{
    public HDSDDichVuRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    {
    }

    private static Expression<Func<HDSDDichVu, bool>> BuildFilter(string currentCSKCB = "", bool isMaster = false)
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
                (x.MA_DICH_VU ?? "").Contains(key) || (x.TEN_DICH_VU ?? "").ToLower().Contains(key) || (x.MA_LOI ?? "").ToLower().Contains(key)
            );
        }

        IList<Func<IOrderedQueryable<HDSDDichVu>, IOrderedQueryable<HDSDDichVu>>> sortFuncList = new List<Func<IOrderedQueryable<HDSDDichVu>, IOrderedQueryable<HDSDDichVu>>>();
        sortFuncList.Add(x => x.OrderByDescending(x => x.HDSDDVID));

        return await GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, sortFuncList, null);
    }
}
