using HIA.Data.Reports;
using HIA.Repositories.Base;

namespace HIA.Repositories.ThongKe;

public interface IThongKeRepository : IBaseRepository<ThongKeNgay>
{
    Task<IEnumerable<ThongKeNgay>?> GetYearlyStats(DateTime fromDate, string currentCSKCB = "", bool isMaster = false);
}
