using System.Linq.Expressions;
using HIA.Core.Base;
using HIA.Core.Models.Pagination;
using HIA.Data.Entities;
using HIA.Repositories.Base;
using LinqKit;
using Microsoft.AspNetCore.Http;

namespace HIA.Repositories.HDSD;

public class ChiDinhDongThoiRepository : BaseRepository<ChiDinhDongThoi>, IChiDinhDongThoiRepository
{
    public ChiDinhDongThoiRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    {
    }

    private static Expression<Func<ChiDinhDongThoi, bool>> BuildFilter(string currentCSKCB = "", bool isMaster = false)
    {
        if (isMaster)
        {
            return PredicateBuilder.New<ChiDinhDongThoi>(true);
        }

        return PredicateBuilder.New<ChiDinhDongThoi>(x => x.MA_CSKCB == null || x.MA_CSKCB == "" || x.MA_CSKCB == currentCSKCB || (";" + x.MA_CSKCB + ";").Contains($";{currentCSKCB};"));
    }

    public async Task<IEnumerable<ChiDinhDongThoi>> GetActiveRules(string currentCSKCB = "", bool isMaster = false)
    {
        Expression<Func<ChiDinhDongThoi, bool>> filter = BuildFilter(currentCSKCB, isMaster);
        filter = filter.And(x => x.HIEU_LUC == true);
        return await GetListAsync(filter, null, null);
    }

    public async Task<PaginationResponseModel<ChiDinhDongThoi>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false)
    {
        Expression<Func<ChiDinhDongThoi, bool>> filter = BuildFilter(currentCSKCB, isMaster);

        // add filter when has searchKey
        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            var key = request.SearchKey.ToLower();
            filter = filter.And(x =>
                (x.MA_DICH_VU ?? "").Contains(key) || (x.DICH_VU_KHAC ?? "").Contains(key) || (x.VAT_TU_BHYT ?? "").Contains(key) || (x.VAT_TU_KO_BH ?? "").Contains(key)
            );
        }

        IList<Func<IOrderedQueryable<ChiDinhDongThoi>, IOrderedQueryable<ChiDinhDongThoi>>> sortFuncList = new List<Func<IOrderedQueryable<ChiDinhDongThoi>, IOrderedQueryable<ChiDinhDongThoi>>>();
        sortFuncList.Add(x => x.OrderByDescending(x => x.CDDTID));

        return await GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, sortFuncList, null);
    }
}
