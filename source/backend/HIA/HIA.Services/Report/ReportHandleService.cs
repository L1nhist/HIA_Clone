
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Auths;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Reports;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Services.Categories;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
namespace HIA.Services.Report;

public partial class ReportHandleService : IReportHandleService
{
    private readonly ILogger<ReportHandleService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    private readonly IBaseRepository<Hospital> _hospitalRepository;
    private readonly IDMErrorService130 _dmErrorService;
    private readonly IBaseRepository<DMBacSi> _dmBacSiRepository;
    private readonly IBaseRepository<DMKhoaBV> _dmKhoaRepository;
    private readonly IBaseRepository<ChiDinhDongThoi> _dmDongThoiRepository;

    private readonly IXml01Repository _hosoRepository;
    private readonly IBaseRepository<XMLState> _stateRepository;
    private readonly IBaseRepository<XML02> _xml02Repository;
    private readonly IBaseRepository<XML03> _xml03Repository;
    private readonly IBaseRepository<XML04> _xml04Repository;
    private readonly IBaseRepository<XML05> _xml05Repository;
    private readonly IBaseRepository<XML06> _xml06Repository;
    private readonly IBaseRepository<XML07> _xml07Repository;
    private readonly IBaseRepository<XML08> _xml08Repository;
    private readonly IBaseRepository<XML09> _xml09Repository;
    private readonly IBaseRepository<XML10> _xml10Repository;
    private readonly IBaseRepository<XML11> _xml11Repository;
    private readonly IBaseRepository<XML13> _xml13Repository;
    private readonly IBaseRepository<XML14> _xml14Repository;
    private readonly IBaseRepository<XML15> _xml15Repository;
    private readonly IBaseRepository<XMLError> _errorRepository;
    private readonly IBaseRepository<User> _userRepository;
    private readonly IBaseRepository<UserLogs> _ulogRepository;

    public ReportHandleService(
        ILogger<ReportHandleService> logger,
        IHttpContextAccessor httpContext,

        IBaseRepository<Hospital> hospitalRepository,
        IDMErrorService130 dmErrorService,
        IBaseRepository<DMKhoaBV> dmKhoaRepository,
        IBaseRepository<DMBacSi> dmBacSiRepository,
        IBaseRepository<ChiDinhDongThoi> dmDongThoiRepository,

        IXml01Repository hosoRepository,
        IBaseRepository<XMLState> xml00Repository,
        IBaseRepository<XML02> xml02Repository,
        IBaseRepository<XML03> xml03Repository,
        IBaseRepository<XML04> xml04Repository,
        IBaseRepository<XML05> xml05Repository,
        IBaseRepository<XML06> xml06Repository,
        IBaseRepository<XML07> xml07Repository,
        IBaseRepository<XML08> xml08Repository,
        IBaseRepository<XML09> xml09Repository,
        IBaseRepository<XML10> xml10Repository,
        IBaseRepository<XML11> xml11Repository,
        IBaseRepository<XML13> xml13Repository,
        IBaseRepository<XML14> xml14Repository,
        IBaseRepository<XML15> xml15Repository,
        IBaseRepository<XMLError> errorRepository,
        IBaseRepository<User> userRepository,
        IBaseRepository<UserLogs> ulogRepository)
    {
        _logger = logger;
        _httpContext = httpContext;

        _hospitalRepository = hospitalRepository;
        _dmErrorService = dmErrorService;
        _dmBacSiRepository = dmBacSiRepository;
        _dmKhoaRepository = dmKhoaRepository;
        _dmDongThoiRepository = dmDongThoiRepository;

        _hosoRepository = hosoRepository;
        _stateRepository = xml00Repository;
        _xml02Repository = xml02Repository;
        _xml03Repository = xml03Repository;
        _xml04Repository = xml04Repository;
        _xml05Repository = xml05Repository;
        _xml06Repository = xml06Repository;
        _xml07Repository = xml07Repository;
        _xml08Repository = xml08Repository;
        _xml09Repository = xml09Repository;
        _xml10Repository = xml10Repository;
        _xml11Repository = xml11Repository;
        _xml13Repository = xml13Repository;
        _xml14Repository = xml14Repository;
        _xml15Repository = xml15Repository;
        _errorRepository = errorRepository;
        _userRepository = userRepository;
        _ulogRepository = ulogRepository;
    }

    #region Private Methods
    private string GetClaim(string type) =>
        _httpContext.HttpContext?.User.FindFirst(type)?.Value
        ?? throw new InvalidOperationException($"Missing claim {type}.");

    private string GetMaCSKCB() => GetClaim(ClaimKey.MA_CSKCB);

    private ResponseModel<byte[]> Fail(Exception ex, string method)
    {
        _logger.LogError(ex, "{Service}: {Message}", $"{nameof(ReportHandleService)} - {method}", ex.Message);
        return new ResponseModel<byte[]>(System.Net.HttpStatusCode.InternalServerError, ex.Message, null);
    }

    private async Task<IEnumerable<T>?> LoadInner<T>(IBaseRepository<T> repo, string[] inners, string tag, string maCSKCB, List<long> lstIds, int count)
        where T : class, HIA.Data.Base.IRecord
    {
        if (!inners.Contains(tag)) return null;
        try
        {
            return await repo.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML{tag}-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{count} nội dung lỗi:" + ex.Message, $"{nameof(ReportHandleService)} - {nameof(AttachInnerXML)} - XML{tag}");
            return null;
        }
    }

    //Mac dinh includes se lay tat ca XML tu 02-11: "2;3;4;5;6;7;8;9;10;11;13;14;15"
    //Neu chi dinh ro [inners] thi se lay XML theo so chi dinh vd: "2;3;5;7"
    private async Task AttachInnerXML(IEnumerable<XML01> xmls, string? innerList = null)
    {
        innerList = innerList?.Trim();
        string[] inners = "2;3;4;5;6;7;8;9;10;11;13;14;15".Split(';');
        if (!string.IsNullOrEmpty(innerList))
        {
            var lst = innerList.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (lst.Length > 0)
            {
                inners = lst;
            }
        }

        try
        {
            var lstIds = xmls.Select(x => x.XML01ID).ToList();
            var first = xmls.FirstOrDefault();
            if (first == null) return;
            string maCSKCB = first.MA_CSKCB;
            int n = lstIds.Count;

            var xml02s = await LoadInner(_xml02Repository, inners, "2", maCSKCB, lstIds, n);
            var xml03s = await LoadInner(_xml03Repository, inners, "3", maCSKCB, lstIds, n);
            var xml04s = await LoadInner(_xml04Repository, inners, "4", maCSKCB, lstIds, n);
            var xml05s = await LoadInner(_xml05Repository, inners, "5", maCSKCB, lstIds, n);
            var xml06s = await LoadInner(_xml06Repository, inners, "6", maCSKCB, lstIds, n);
            var xml07s = await LoadInner(_xml07Repository, inners, "7", maCSKCB, lstIds, n);
            var xml08s = await LoadInner(_xml08Repository, inners, "8", maCSKCB, lstIds, n);
            var xml09s = await LoadInner(_xml09Repository, inners, "9", maCSKCB, lstIds, n);
            var xml10s = await LoadInner(_xml10Repository, inners, "10", maCSKCB, lstIds, n);
            var xml11s = await LoadInner(_xml11Repository, inners, "11", maCSKCB, lstIds, n);
            var xml13s = await LoadInner(_xml13Repository, inners, "13", maCSKCB, lstIds, n);
            var xml14s = await LoadInner(_xml14Repository, inners, "14", maCSKCB, lstIds, n);
            var xml15s = await LoadInner(_xml15Repository, inners, "15", maCSKCB, lstIds, n);

            foreach (var xml01 in xmls)
            {
                var id = xml01.XML01ID;
                if (xml02s != null) xml01.AddRange(xml02s.Where(x => x.XML01ID == id));
                if (xml03s != null) xml01.AddRange(xml03s.Where(x => x.XML01ID == id));
                if (xml04s != null) xml01.AddRange(xml04s.Where(x => x.XML01ID == id));
                if (xml05s != null) xml01.AddRange(xml05s.Where(x => x.XML01ID == id));
                if (xml06s != null) xml01.AddRange(xml06s.Where(x => x.XML01ID == id));
                if (xml07s != null) xml01.AddRange(xml07s.Where(x => x.XML01ID == id));
                if (xml08s != null) xml01.AddRange(xml08s.Where(x => x.XML01ID == id));
                if (xml09s != null) xml01.AddRange(xml09s.Where(x => x.XML01ID == id));
                if (xml10s != null) xml01.AddRange(xml10s.Where(x => x.XML01ID == id));
                if (xml11s != null) xml01.AddRange(xml11s.Where(x => x.XML01ID == id));
                if (xml13s != null) xml01.AddRange(xml13s.Where(x => x.XML01ID == id));
                if (xml14s != null) xml01.AddRange(xml14s.Where(x => x.XML01ID == id));
                if (xml15s != null) xml01.AddRange(xml15s.Where(x => x.XML01ID == id));
            }
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(ReportHandleService)} - {nameof(AttachInnerXML)}");
        }
    }

    private async Task<IEnumerable<XMLError>> AttachXMLError(string ma_CSKCB, IEnumerable<XML01> x01s)
    {
        List<long> lstXMLIDs = x01s.Select(x => x.XML01ID).ToList();
        IEnumerable<XMLError> dataE = new List<XMLError>();
        try
        {
            dataE = await _errorRepository.GetListAsync(x => x.MA_CSKCB == ma_CSKCB && lstXMLIDs.Contains(x.XML01ID));
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin BC Tổng hợp, Đơn vị:{ma_CSKCB}-Số lượng hồ sơ:{lstXMLIDs.Count} nội dung lỗi:" + ex.Message, $"{nameof(ReportHandleService)} - {nameof(AttachInnerXML)} - XML-Error");
        }

        return dataE;
    }

    private FilterQueryableModel<XML01> BuildQuery_XML01(FilterDownloadModel request)
    {
        var result = new FilterQueryableModel<XML01>();

        var filter = PredicateBuilder.New<XML01>(x => x.IsDelete != true);
        if (!string.IsNullOrEmpty(request.HospitalId))
        {
            filter = filter.And(x => x.MA_CSKCB == request.HospitalId);
        }
        if (!string.IsNullOrEmpty(request.MaCSKCB))
        {
            filter = filter.And(x => x.MA_CSKCB == request.MaCSKCB);
        }
        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            string pattern = request.SearchKey;

            var filterSearch = PredicateBuilder.New<XML01>();
            filterSearch = filterSearch.Or(x => x.MA_THE_BHYT != null && x.MA_THE_BHYT.Contains(pattern))
                                        .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern))
                                        .Or(x => x.MA_BN != null && x.MA_BN.Contains(pattern))
                                        .Or(x => x.HO_TEN != null && x.HO_TEN.Contains(pattern));

            filter = filter.And(filterSearch);
        }
        //Linhist changed 02/01/25
        if (request.Departments != null && request.Departments.Any())
        {
            var allDeps = ";" + string.Join(";", request.Departments.ToArray()) + ";";
            var filterDep = PredicateBuilder.New<XML01>(x => allDeps.Contains(";" + x.MA_KHOA + ";"));

            foreach (var maKhoa in request.Departments)
            {
                filterDep = filterDep.Or(x => (";" + x.MA_KHOA + ";").Contains(";" + maKhoa + ";"));
            }
            filter = filter.And(x => x.MA_KHOA != null).And(filterDep);
        }
        if (request.FromDateCreate.HasValue)
        {
            DateTime dayFrom = request.FromDateCreate.Value.Date;
            filter = filter.And(x => x.CreateDate >= dayFrom);
        }
        if (request.ToDateCreate.HasValue)
        {
            DateTime dayTo = request.ToDateCreate.Value.Date.AddDays(1);
            filter = filter.And(x => x.CreateDate < dayTo);
        }
        if (request.FromDateIn.HasValue)
        {
            DateTime dayInFrom = request.FromDateIn.Value.Date;
            filter = filter.And(x => x.NGAY_VAO >= dayInFrom);
        }
        if (request.ToDateIn.HasValue)
        {
            DateTime dayInTo = request.ToDateIn.Value.Date.AddDays(1);
            filter = filter.And(x => x.NGAY_VAO < dayInTo);
        }
        if (request.FromDateOut.HasValue)
        {
            DateTime dayOutFrom = request.FromDateOut.Value.Date;
            filter = filter.And(x => x.NGAY_RA >= dayOutFrom);
        }
        if (request.ToDateOut.HasValue)
        {
            DateTime dayOutTo = request.ToDateOut.Value.Date.AddDays(1);
            filter = filter.And(x => x.NGAY_RA < dayOutTo);
        }
        if (request.FromDatePaid.HasValue)
        {
            DateTime dayPaidFrom = request.FromDatePaid.Value.Date;
            filter = filter.And(x => x.NGAY_TTOAN >= dayPaidFrom);
        }
        if (request.ToDatePaid.HasValue)
        {
            DateTime dayPaidTo = request.ToDatePaid.Value.Date.AddDays(1);
            filter = filter.And(x => x.NGAY_TTOAN < dayPaidTo);
        }

        // MA_LOAI_KCB filter
        var typeKcb = request.TypeKCB?.Trim();
        if (!string.IsNullOrEmpty(typeKcb))
        {
            filter = filter.And(x => x.MA_LOAI_KCB == typeKcb || x.MA_LOAI_KCB == "0" + typeKcb);
        }

        // Error status filter
        var filterError = PredicateBuilder.New<XML01>(true);
        if (request.Errors != null && request.Errors.Any())
        {
            List<int?> errors = request.Errors.Select(e => Uti.ToNum(e)).Where(e => e != null).ToList();
            filterError = filterError.Or(x => x.Errors.Any(y => errors.Contains(y.MA_LOI)));
        }

        filter = filter.And(filterError);
        result.FilterQueryable = filter;

        result.OrderQueryable = new List<Func<IOrderedQueryable<XML01>, IOrderedQueryable<XML01>>>
        {
            x => x.OrderByDescending(y => y.CreateDate)
        };

        return result;
    }

    private async Task<IEnumerable<XMLErrorInfo>> GetDMErrors(string ma_CSKCB, IEnumerable<int?> errCodes)
    {
        var errors = await _dmErrorService.GetAllByHospital(ma_CSKCB);
        return errors.Select(e => new XMLErrorInfo
        {
            error_code = e.ErrorCode,
            error_file = e.ErrorAtXMLFile,
            error_tag = e.ErrorAtXMLTag,
            message = e.ErrorContent,
            comment = e.ErrorComment,
            is_alert = e.IsAlarm,
        }).ToList();
    }
    #endregion
}
