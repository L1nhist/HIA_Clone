using System.Linq.Expressions;
using System.Security.Claims;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Token;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Reports;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Report;

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IXml01Repository _xml1Repository;
    private readonly IBaseRepository<DMBenh> _DMBenhRepository;
    private readonly IBaseRepository<UserLogs> _userlogRepository;
    private readonly IBaseRepository<ThongKeNgay> _thongkeRepository;

    public ReportService(
        ILogger<ReportService> logger,
        IHttpContextAccessor httpContext,
        IXml01Repository xml1Repository,
        IBaseRepository<DMBenh> DMBenhRepository,
        IBaseRepository<UserLogs> userlogRepository,
        IBaseRepository<ThongKeNgay> thongkeRepository)
    {
        _logger = logger;
        _httpContext = httpContext;
        _xml1Repository = xml1Repository;
        _DMBenhRepository = DMBenhRepository;
        _userlogRepository = userlogRepository;
        _thongkeRepository = thongkeRepository;
    }

    private ClaimsPrincipal CurrentUser =>
        _httpContext.HttpContext?.User ?? throw new InvalidOperationException("No current HttpContext.");

    private string GetClaim(string type) =>
        CurrentUser.FindFirst(type)?.Value ?? throw new InvalidOperationException($"Missing claim {type}.");

    private string GetMaCSKCB() => GetClaim(ClaimKey.MA_CSKCB);

    public async Task<DMBenh?> GetTenBen(string code)
    {
        return await _DMBenhRepository.GetItem(x => x.BenhCode == code);
    }

    public async Task<IEnumerable<XML01>> GetAllXML1()
    {
        var maCSKCB = GetMaCSKCB();
        return await _xml1Repository.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB));
    }

    // Files of the current hospital ("00" = all hospitals) created in [from, to), not deleted.
    private IQueryable<XML01> Files(DateTime from, DateTime to, Expression<Func<XML01, bool>>? extra = null)
    {
        var maCSKCB = GetMaCSKCB();
        var where = PredicateBuilder.New<XML01>(x => x.CreateDate >= from && x.CreateDate < to && !true.Equals(x.IsDelete));
        if (maCSKCB != "00")
        {
            where = where.And(x => x.MA_CSKCB.Equals(maCSKCB));
        }
        if (extra != null)
        {
            where = where.And(extra);
        }
        return _xml1Repository.GetCount(where);
    }

    private IQueryable<XML01> FilesToday(Expression<Func<XML01, bool>>? extra = null)
    {
        var today = DateTime.Now.Date;
        return Files(today, today.AddDays(1), extra);
    }

    private IQueryable<XML01> FilesThisMonth(Expression<Func<XML01, bool>>? extra = null)
    {
        var now = DateTime.Now;
        var first = new DateTime(now.Year, now.Month, 1);
        return Files(first, first.AddMonths(1), extra);
    }

    public int TotalFileDate() => FilesToday().Count();

    public int TotalFileMonth() => FilesThisMonth().Count();

    public int TotalFileDieuTriNoiTruDate() => FilesToday(x => x.MA_LOAI_KCB == "03").Count();

    public int TotalFileDieuTriNoiTruMonth() => FilesThisMonth(x => x.MA_LOAI_KCB == "03").Count();

    public int TotalFileRaVienDate() => FilesToday(x => x.MA_LOAI_RV == 2 || x.MA_LOAI_RV == 5).Count();

    public int TotalFileRavienMonth() => FilesThisMonth(x => x.MA_LOAI_RV == 2 || x.MA_LOAI_RV == 5).Count();

    public double? TotalFileTongTienDate()
    {
        double? total = 0;
        if (GetMaCSKCB() != "00")
        {
            total = FilesToday().Sum(y => y.T_TONGCHI_BV) / 1000;
        }
        if (total < 0) return total * -1;
        return total;
    }

    public double? TotalFileTongTienMonth()
    {
        double? total = 0;
        var maCSKCB = GetMaCSKCB();
        try
        {
            if (maCSKCB != "00")
            {
                total = FilesThisMonth().Sum(y => y.T_TONGCHI_BV) / 1000;
            }
            if (total < 0) return total * -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TotalFileTongTienMonth");
            return 0;
        }
        return total;
    }

    private static int ToDateKey(DateTime dte) => dte.Year * 10000 + dte.Month * 100 + dte.Day;

    private static DateTime FromDateKey(int dd) => new DateTime(dd / 10000, (dd % 10000) / 100, dd % 100);

    // ThongKeNgay of the last `days` days up to byDate, scoped by the current role/hospital.
    private async Task<IEnumerable<ThongKeNgay>> GetThongKeNgay(DateTime? byDate, int days)
    {
        var where = PredicateBuilder.New<ThongKeNgay>(e => e.TheLoai == 2);
        var dte = byDate ?? DateTime.Now;
        var dFrom = ToDateKey(dte);
        var dTo = ToDateKey(dte.AddDays(-days));
        where = where.And(e => e.TrongNgay >= dTo && e.TrongNgay <= dFrom);

        var currentRole = GetClaim(ClaimTypes.Role);
        var currentMaCSKCB = GetMaCSKCB();

        if (currentRole.Contains(RoleConstant.MASTER) || currentMaCSKCB == "BOYTE" || currentMaCSKCB == "00")
        { }
        else if (currentMaCSKCB.Length < 5)
        {
            where = where.And(e => e.MA_CSKCB.StartsWith(currentMaCSKCB));
        }
        else
        {
            where = where.And(e => e.MA_CSKCB == currentMaCSKCB);
        }

        return await _thongkeRepository.GetListAsync(where);
    }

    public async Task<TotalizeReport> GetThongKe(DateTime? byDate = null)
    {
        var thongKes = await GetThongKeNgay(byDate, 30);
        var lst = new TotalizeReport();
        foreach (var tk in thongKes)
        {
            lst.Add(new TotalizeItem
            {
                MaCSKCB = tk.MA_CSKCB,
                Hospital = "",
                TotalFiles = (int)(tk.TongHop ?? 0),
                FromDate = FromDateKey(tk.TrongNgay ?? 0)
            });
        }

        return lst;
    }

    public async Task<Summary> GetSummary(DateTime? byDate = null)
    {
        var thongKes = await GetThongKeNgay(byDate, 7);
        var lst = new Summary();
        foreach (var tk in thongKes)
        {
            lst.Add(new Summaryitem
            {
                MaCSKCB = tk.MA_CSKCB,
                Hospital = "",
                TotalUploads = (int)(tk.TongHop ?? 0f),
                isUploadFiles = tk.TongHop != null && tk.TongHop > 0,
                FromDate = FromDateKey(tk.TrongNgay ?? 0)
            });
        }

        return lst;
    }

    public async Task<Summary> GetDaily(DateTime dte)
    {
        var dateKey = ToDateKey(dte);
        var thongKes = await _thongkeRepository.GetListAsync(e => e.TheLoai == 2 && e.TrongNgay == dateKey);
        var lst = new Summary();
        var maCSKCBs = new List<string>();
        foreach (var tk in thongKes)
        {
            if (!maCSKCBs.Contains(tk.MA_CSKCB))
            {
                maCSKCBs.Add(tk.MA_CSKCB);
            }

            lst.Add(new Summaryitem
            {
                MaCSKCB = tk.MA_CSKCB,
                Hospital = "",
                TotalUploads = (int)(tk.TongHop ?? 0f),
                isUploadFiles = tk.TongHop != null && tk.TongHop > 0,
                FromDate = dte
            });
        }
        if (lst.Count == 0) return lst;

        var actives = await _userlogRepository.GetListAsync(x => x.MA_CSKCB != null && maCSKCBs.Contains(x.MA_CSKCB) && x.LogTime.Date == dte && x.Action != null && x.Action.ToLower().Contains("báo cáo"));
        foreach (var r in lst)
        {
            r.isActive = actives.Any(e => e.MA_CSKCB == r.MaCSKCB);
        }

        return lst;
    }
}
