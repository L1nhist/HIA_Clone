using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using LinqKit;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HIA.Repositories.ThongKe
{
    public class ThongKeRepository : BaseRepository<ThongKeNgay>, IThongKeRepository
    {
        public ThongKeRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
        { }

        public async Task<IEnumerable<ThongKeNgay>> GetYearlyStats(DateTime fromDate, string currentCSKCB = "", bool isMaster = false)
        {
            int monFst;
            fromDate = fromDate.AddMonths(-11);
            monFst = fromDate.Year * 10000 + fromDate.Month * 100;

            try
            {
                Expression<Func<ThongKeNgay, bool>> filter = PredicateBuilder.New<ThongKeNgay>(t => t.TrongNgay > monFst);
                if (!isMaster)
                {
                    filter = filter.And(t => t.MA_CSKCB == currentCSKCB);
                }    
                //filter = filter.And(t => t.MA_CSKCB == (isMaster ? "" : currentCSKCB));
               
                return await GetListAsync(filter, null, null);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }

            return null;
        }
    }
}
