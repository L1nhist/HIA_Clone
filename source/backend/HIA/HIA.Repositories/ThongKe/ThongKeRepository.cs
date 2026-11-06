using System.Linq.Expressions;
using HIA.Core.Base;
using HIA.Data.Entities;
using HIA.Repositories.Base;
using LinqKit;
using Microsoft.AspNetCore.Http;

namespace HIA.Repositories.ThongKe;

public class ThongKeRepository : BaseRepository<ThongKeNgay>, IThongKeRepository
{
    public ThongKeRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    { }

    public async Task<IEnumerable<ThongKeNgay>?> GetYearlyStats(DateTime fromDate, string currentCSKCB = "", bool isMaster = false)
    {
        fromDate = fromDate.AddMonths(-11);
        int monFst = fromDate.Year * 10000 + fromDate.Month * 100;

        try
        {
            Expression<Func<ThongKeNgay, bool>> filter = PredicateBuilder.New<ThongKeNgay>(t => t.TrongNgay > monFst);
            if (!isMaster)
            {
                filter = filter.And(t => t.MA_CSKCB == currentCSKCB);
            }

            return await GetListAsync(filter, null, null);
        }
        catch (Exception)
        {
            // Hanh vi cu: nuot loi, tra ve null.
        }

        return null;
    }
}
