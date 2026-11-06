using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HIA.Repositories.ThongKe
{
    public interface IThongKeRepository : IBaseRepository<ThongKeNgay>
    {
        Task<IEnumerable<ThongKeNgay>> GetYearlyStats(DateTime fromDate, string currentCSKCB = "", bool isMaster = false);
    }
}