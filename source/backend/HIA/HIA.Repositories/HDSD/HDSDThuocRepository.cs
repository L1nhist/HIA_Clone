using System.Linq.Expressions;
using HIA.Core.Base;
using HIA.Core.Models.Pagination;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using LinqKit;
using Microsoft.AspNetCore.Http;

namespace HIA.Repositories.HDSD;

public class HDSDThuocRepository : BaseRepository<HDSDThuoc>, IHDSDThuocRepository
{
    public HDSDThuocRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    {
    }

    private static Expression<Func<HDSDThuoc, bool>> BuildFilter(string currentCSKCB = "", bool isMaster = false)
    {
        if (isMaster)
        {
            return PredicateBuilder.New<HDSDThuoc>(true);
        }

        //Linhist added 26/02/23
        //Filter rieng cho HP (31313)
        if (currentCSKCB == "31313" || currentCSKCB == "31919")
        {
            return PredicateBuilder.New<HDSDThuoc>(x => x.MA_CSKCB == currentCSKCB);
        }

        return PredicateBuilder.New<HDSDThuoc>(x => x.MA_CSKCB == null || x.MA_CSKCB == "" || x.MA_CSKCB == currentCSKCB || (";" + x.MA_CSKCB + ";").Contains($";{currentCSKCB};"));
    }

    public async Task<IEnumerable<HDSDThuoc>> GetActiveRules(string currentCSKCB = "", bool isMaster = false)
    {
        Expression<Func<HDSDThuoc, bool>> filter = BuildFilter(currentCSKCB, isMaster);
        filter = filter.And(x => x.HIEU_LUC == true);
        return await GetListAsync(filter, null, null);
    }

    public async Task<PaginationResponseModel<HDSDThuoc>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false)
    {
        Expression<Func<HDSDThuoc, bool>> filter = BuildFilter(currentCSKCB, isMaster);

        // add filter when has searchKey
        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            var key = request.SearchKey.ToLower();
            if (int.TryParse(key, out var loiValue))
            {
                filter = filter.And(x =>
                    (x.MA_THUOC ?? "").Contains(key) ||
                    (x.TEN_THUOC ?? "").ToLower().Contains(key) ||
                    x.MA_LOI == loiValue
                );
            }
            else
            {
                filter = filter.And(x =>
                    (x.MA_THUOC ?? "").Contains(key) ||
                    (x.TEN_THUOC ?? "").ToLower().Contains(key)
                );
            }
        }

        IList<Func<IOrderedQueryable<HDSDThuoc>, IOrderedQueryable<HDSDThuoc>>> sortFuncList = new List<Func<IOrderedQueryable<HDSDThuoc>, IOrderedQueryable<HDSDThuoc>>>();
        sortFuncList.Add(x => x.OrderByDescending(x => x.HDSDThuocID));

        return await GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, sortFuncList, null);
    }
}
