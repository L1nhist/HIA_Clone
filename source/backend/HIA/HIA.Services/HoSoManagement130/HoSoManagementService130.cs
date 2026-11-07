using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Token;
using HIA.Data.Auths;
using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Reports;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Repositories.CheckThe;
using HIA.Repositories.Files;
using HIA.Repositories.HDSD;
using HIA.Services.Categories;
using HIA.Services.SysHistoryLogManagement;
using HIA.Services.Validate130;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HIA.Services.HosoManagement130;

public class HoSoManagementService130 : IHoSoManagementService130
{
    #region Constructions
    //Linhist changed 02/03/26
    private List<string> lstMA_CSKCB_DANGTT = new List<string>();
    private readonly ILogger<HoSoManagementService130> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppSettings _appSettings;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IDMErrorService130 _dmErrorService;

    private readonly IXml01Repository _hosoRepository;
    private readonly IBaseRepository<XMLState> _stateRepository;
    private readonly IBaseRepository<XMLError> _errorRepository;
    private readonly IBaseRepository<XML02> _xml02Repository;
    private readonly IBaseRepository<XML03> _xml03Repository;
    private readonly IBaseRepository<UserLogs> _userlogRepository;
    private readonly IXMLValidateRepository _validateRepository;

    private readonly IHDSDThuocRepository _HDSDThuocRepository;
    private readonly IHDSDDichVuRepository _HDSDDichVuRepository;
    private readonly IBaseRepository<ChiDinhDongThoi> _ChiDinhDongThoiRepository;
    private readonly IBaseRepository<PhacDoDieuTri> _PhacDoDieuTriRepository;
    //Linhist removed 29/03/25
    private readonly IBaseRepository<DMErrorMHTC> _dmErrorMHTCRepo;
    private readonly IBaseRepository<Hospital> _hospitalRepository;
    private readonly IBaseRepository<DMThuoc> _DMThuocRepository;
    private readonly IBaseRepository<DMVTYT> _DMVTYTRepository;
    private readonly IBaseRepository<DMDichVu> _DMDichVuRepository;
    private readonly IBaseRepository<DMDVThuCong> _DMThuCongRepository;
    private readonly IBaseRepository<DMBacSi> _DMBacSiRepository;
    private readonly IBaseRepository<DMKhoa> _DMKhoaRepository;
    private readonly IBaseRepository<DMKhoaBV> _DMKhoaBVRepository;
    private readonly IBaseRepository<DMDacBiet> _DMDacBietRepository;
    private readonly IBaseRepository<DMYHDTICD10> _DMYHDTRepository;
    private readonly IBaseRepository<DMPhauThuat> _DMPhauThuatRepository;
    private readonly IBaseRepository<DMGiuong> _DMGiuongRepository;
    private readonly IBaseRepository<DMThietBi> _DMThietBiRepository;
    private readonly IBaseRepository<DMBVTuyenTinh> _DMBVTuyenTinhRepository;
    private readonly ICheckThe130Repository _checkTheRepository;

    private readonly IBaseRepository<DMChung130> _DMChungRepository;
    private readonly IBaseRepository<DichVuMaMay> _dvuMaMayRepository;

    public HoSoManagementService130(

        ILogger<HoSoManagementService130> logger,
        IUnitOfWork unitOfWork,
        IOptions<AppSettings> appSettings,
        IHttpContextAccessor httpContext,
        ISysHistoryLogService sysHistoryLogService,
        IDMErrorService130 dmErrorService,

        IXml01Repository hosoRepository,
        IBaseRepository<XMLState> xml00Repository,
        IBaseRepository<XMLError> errorRepository,
        IBaseRepository<XML02> xml02Repository,
        IBaseRepository<XML03> xml03Repository,
        IXMLValidateRepository validateRepository,
        IBaseRepository<UserLogs> userlogRepository,

        IHDSDThuocRepository HDSDThuocRepository,
        IHDSDDichVuRepository HDSDDichVuRepository,
        IBaseRepository<ChiDinhDongThoi> ChiDinhDongThoiRepository,
        IBaseRepository<PhacDoDieuTri> PhacDoDieuTriRepository,
        //Linhist removed 29/03/25
        IBaseRepository<DMErrorMHTC> dmErrorMHTCRepo,
        IBaseRepository<Hospital> hospitalRepository,
        IBaseRepository<DMThuoc> DMThuocRepository,
        IBaseRepository<DMVTYT> DMVTYTRepository,
        IBaseRepository<DMDichVu> DMDichVuRepository,
        IBaseRepository<DMDVThuCong> DMThuCongRepository,
        IBaseRepository<DMBacSi> DMBacSiRepository,
        IBaseRepository<DMKhoa> DMKhoaRepository,
        IBaseRepository<DMKhoaBV> DMKhoaBVRepository,
        IBaseRepository<DMDacBiet> DMDacBietRepository,
        IBaseRepository<DMYHDTICD10> DMYHDTRepository,
        IBaseRepository<DMPhauThuat> DMPhauThuatRepository,
        IBaseRepository<DMGiuong> DMGiuongRepository,
        IBaseRepository<DMThietBi> DMThietBiRepository,
        IBaseRepository<DMBVTuyenTinh> DMBVTuyenTinhRepository,
        ICheckThe130Repository checkTheRepository,
        IBaseRepository<DMChung130> dmChungRepository,
        IBaseRepository<DichVuMaMay> dvuMaMayRepository

    )
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _appSettings = appSettings.Value;
        _httpContext = httpContext;
        _sysHistoryLogService = sysHistoryLogService;

        _hosoRepository = hosoRepository;
        _stateRepository = xml00Repository;
        _errorRepository = errorRepository;
        _xml02Repository = xml02Repository;
        _xml03Repository = xml03Repository;
        _validateRepository = validateRepository;
        _userlogRepository = userlogRepository;
        _dmErrorService = dmErrorService;

        _HDSDThuocRepository = HDSDThuocRepository;
        _HDSDDichVuRepository = HDSDDichVuRepository;
        _ChiDinhDongThoiRepository = ChiDinhDongThoiRepository;
        _PhacDoDieuTriRepository = PhacDoDieuTriRepository;
        //Linhist removed 29/03/25
        _dmErrorMHTCRepo = dmErrorMHTCRepo;
        _hospitalRepository = hospitalRepository;
        _DMThuocRepository = DMThuocRepository;
        _DMVTYTRepository = DMVTYTRepository;
        _DMDichVuRepository = DMDichVuRepository;
        _DMThuCongRepository  = DMThuCongRepository;
        _DMBacSiRepository = DMBacSiRepository;
        _DMKhoaRepository = DMKhoaRepository;
        _DMKhoaBVRepository = DMKhoaBVRepository;
        _DMDacBietRepository = DMDacBietRepository;
        _DMYHDTRepository = DMYHDTRepository;
        _DMPhauThuatRepository = DMPhauThuatRepository;
        _DMGiuongRepository = DMGiuongRepository;
        _DMThietBiRepository = DMThietBiRepository;
        _DMBVTuyenTinhRepository = DMBVTuyenTinhRepository;
        _checkTheRepository = checkTheRepository;
        _DMChungRepository = dmChungRepository;
        _dvuMaMayRepository = dvuMaMayRepository;
    }
    #endregion
    public static class CSKCBValidateing
    {
        // Key: Mã hồ sơ (MA_LK hoặc ID), Value: đang xử lý hay không
        public static ConcurrentDictionary<string, bool> DangXuLy = new ConcurrentDictionary<string, bool>();
    }

    #region CronJob function
    public Task ValidateHosoFrom4210() => Task.CompletedTask;

    public async Task ValidateHoso()
    {
        //Khoi dong sau Validate 4210 cu 2s

        while (true)
        {
            try
            {
                //12.7.2024 Haimaster update lên lấy 30 viện giám định, tránh trường hợp ưu tiên các viện đẩy ít
                var lstCSKCB = new string[] { "25001" };
                foreach (string maCSKCB in lstCSKCB)
                {
                    try
                    {
                        if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                        var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 50);
                        Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Lay {lstIds.Count()} ho so moi nhat tu viện ít hồ sơ nhất {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                        DateTime dteStart = DateTime.Now;
                        await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                        TimeSpan execCounter = DateTime.Now - dteStart;
                        Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Giam dinh xong Viện ít hồ sơ nhất {lstIds.Count()} file cua {maCSKCB}. Thoi gian thuc hien {execCounter.TotalMilliseconds}", nameof(ValidateHoso));
                        await Task.Delay(100);
                    }
                    catch(Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Giam dinh loi, MACSKCB: {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                    }
                    finally
                    {
                        CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);

                    }
                }

            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Giam dinh loi [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

            }

            await Task.Delay(100);
        }
    }

    private const string VIP1 = "25001;27009;31333;34001;34016;340001";
    private const string VIP2 = "01235;01258;08101;10007;10061;14002;14011;14279;15901;19012;25011;20007;24276;25003;25012;26030;31300;31313;31336;31338;34003;34020";

    private readonly Stopwatch _watch1 = Stopwatch.StartNew();
    private readonly Stopwatch _watch2 = Stopwatch.StartNew();
    private readonly Stopwatch _watch3 = Stopwatch.StartNew();

    public async Task ValidateHosoVIP1()
    {
        if (Debugger.IsAttached) return;

        while (true)
        {
            try
            {
                //Bắt đầu lấy 10 viện có số hồ sơ ít nhất > 15 hồ sơ để giám định
                var lstCSKCB = VIP1.Split(';');
                foreach (string maCSKCB in lstCSKCB)
                {
                    try
                    {
                        if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                        var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 10);
                        if (lstIds == null || !lstIds.Any())
                        {
                            continue;
                        }

                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Lấy {lstIds.Count()} hồ sơ cũ nhất từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                        _watch1.Restart();
                        await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                        _watch1.Stop();
                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Giám định xong {lstIds.Count()} hồ sơ từ viện {maCSKCB}. Thời gian thực hiện {_watch1.Elapsed.TotalMilliseconds}", nameof(ValidateHoso));
                        await Task.Delay(50);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Lỗi giám định từ viện {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                    }
                    finally
                    {
                        _watch1.Stop();
                        CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);
                    }
                }

            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Lỗi giám định [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

            }

            await Task.Delay(100);
        }
    }

    public async Task ValidateHosoVIP2()
    {
        if (Debugger.IsAttached) return;

        while (true)
        {
            try
            {
                //Bắt đầu lấy 10 viện có số hồ sơ nhiều nhất > 15 hồ sơ để giám định
                var lstCSKCB = VIP2.Split(';');
                foreach (string maCSKCB in lstCSKCB)
                {
                    try
                    {
                        if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                        var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 20);
                        if (lstIds == null || !lstIds.Any())
                        {
                            continue;
                        }

                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Lấy {lstIds.Count()} hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                        _watch2.Restart();
                        await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                        _watch2.Stop();
                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Giám định xong {lstIds.Count()} hồ sơ từ viện {maCSKCB}. Thời gian thực hiện {_watch2.Elapsed.TotalMilliseconds}", nameof(ValidateHoso));
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Lỗi giám định từ viện {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                    }
                    finally
                    {
                        _watch2.Stop();
                        CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);
                    }
                }

            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Lỗi giám định [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

            }

            await Task.Delay(500);
        }
    }

    public async Task ValidateHosoVIP3()
    {
        if (Debugger.IsAttached) return;

        while (true)
        {
            try
            {
                var lstCSKCB = await _validateRepository.GetLessCSKCB($"{VIP1};{VIP2}");
                foreach (string maCSKCB in lstCSKCB)
                {
                    try
                    {
                        if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                        var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 30);
                        if (lstIds == null || !lstIds.Any())
                        {
                            continue;
                        }

                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Lấy {lstIds.Count()} hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                        _watch3.Restart();
                        await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                        _watch3.Stop();
                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Giám định xong {lstIds.Count()} hồ sơ từ viện {maCSKCB}. Thời gian thực hiện {_watch3.Elapsed.TotalMilliseconds}", nameof(ValidateHoso));
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Giám định lỗi từ viện {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                    }
                    finally
                    {
                        _watch3.Stop();
                        CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);
                    }
                }

            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Giám định lỗi [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

            }

            await Task.Delay(1000);
        }
    }

    #endregion

    public async Task AppendHosoToValidate(string? maCSKCB = "", params long[] xmlIds)
    {
        var vals = xmlIds.Select(id => new XMLValidate() { XML01ID = id, MA_CSKCB = maCSKCB ?? string.Empty }).ToList();
        _validateRepository.InsertList(vals);
        await _validateRepository.SaveChangesAsync();
    }

    public async Task RemoveHosoFromValidate(params long[] xmlIds)
    {
        await _validateRepository.DeleteListByFilterAsync(x => xmlIds.Contains(x.XML01ID));
        await _validateRepository.SaveChangesAsync();
    }

    public async Task<ResponseModel<bool>> CreateNewXML(List<XML01> xmls, string? maCSKCB = null, string? email = null, bool fromHIS = false)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            await _hosoRepository.InsertListAsync(xmls);
            await _hosoRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"CreateNewXML130 fails: {ex.Message}{Environment.NewLine}{ex.StackTrace}", nameof(CreateNewXML));
            res = Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(CreateNewXML)}");
            return res;
        }

        try
        {
            var xmlIds = xmls.Select(x => x.XML01ID).ToArray();
            if (fromHIS)
            {
                await ValidateXML(maCSKCB, email, fromHIS, xmlIds: xmlIds);
            }
            else
            {
                await AppendHosoToValidate(maCSKCB, xmlIds);
            }
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"CreateNewXML130 fails: {ex.Message}{Environment.NewLine}{ex.StackTrace}", nameof(CreateNewXML));
            res = Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(CreateNewXML)}");
        }

        return res;
    }

    public async Task ValidateXML(string? maCSKCB = null, string? email = null, bool fromAPI = false, bool fromHIS = false, params long[] xmlIds)
    {
        #region Get XML & DM
        //Linhist removed 02/03/26
        var userEmail = email ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userEmail) && _httpContext.HttpContext?.User != null)
        {
            userEmail = _httpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                userEmail = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            }
        }

        if (xmlIds == null || xmlIds.Length < 1)
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: khong co danh sach XML01ID de kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(maCSKCB))
            {
                maCSKCB = _httpContext.HttpContext?.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? throw new InvalidOperationException("Thiếu claim MA_CSKCB");
            }
        }
        catch
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: MA_CSKCB de trong, khong the kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;

        }

        if (string.IsNullOrEmpty(maCSKCB))
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: MA_CSKCB de trong, khong the kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        //Linhist changed 20/03/26 - move down
        //catch

        IEnumerable<XML01> xml01s;
        try
        {
            //Linhist changed 30/08/24
            xml01s = await _hosoRepository.GetAllByListID(maCSKCB, xmlIds);

            //Linhist added 03/03/26: xóa hết id khỏi bảng XMLValidate nếu bên trên ko select được XML01 nào
            if (xml01s == null || !xml01s.Any())
            {
                await RemoveHosoFromValidate(xmlIds);
                Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai XML theo danh sach XML01ID gui vao, da xoa khoi XMLValidate", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            //Linhist removed 060826
            //Linhist changed 02/03/26
        }
        catch
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: Khong lay duoc XML, khong the kiem tra, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        //Linhist added 060826
        try
        {
            if (!await AttachInnerXML(xml01s))
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: Co loi khi lay cac XML 2,3,4... Khong giam dinh ho so, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }
        }
        catch
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: Khong lay duoc XML, khong the kiem tra, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        try
        {
            if (!await AttachXMLState(xml01s))
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: Co loi khi lay XMLState Khong giam dinh ho so, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }
        }
        catch
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: Khong lay duoc XMLState, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        if (xml01s == null || !xml01s.Any())
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai XML theo danh sach XML01ID gui vao", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }
        // haimastervn 23.10.2025 updatecode test

        //Linhist changed 20/03/26
        TimePeriod _officialTime;
        try
        {
            var hospital = await _hospitalRepository.GetItem(x => x.MA_CSKCB == maCSKCB) ?? throw new InvalidOperationException($"Không tồn tại bệnh viện {maCSKCB}");
            _officialTime = new(hospital.OfficeTime);
        }
        catch
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai benh vien co MA_CSKCB {maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        IEnumerable<HDSDThuoc> _ruleThuocs = new List<HDSDThuoc>();
        IEnumerable<HDSDDichVu> _ruleDichVus = new List<HDSDDichVu>();
        IEnumerable<DMThuoc> _dmThuoc = new List<DMThuoc>();
        IEnumerable<DMVTYT> _dmVTYT = new List<DMVTYT>();
        IEnumerable<DMDichVu> _dmDichVu = new List<DMDichVu>();
        IEnumerable<DMDVThuCong> _dmThuCong = new List<DMDVThuCong>();
        IEnumerable<DMBacSi> _dmBacSi = new List<DMBacSi>();
        IEnumerable<DMKhoa> _dmKhoa = new List<DMKhoa>();
        IEnumerable<DMKhoaBV> _dmKhoaBV = new List<DMKhoaBV>();
        IEnumerable<DMYHDTICD10> _dmYHCTs = new List<DMYHDTICD10>();
        IEnumerable<DMThietBi> _dmThietBi = new List<DMThietBi>();
        IEnumerable<DMBVTuyenTinh> _dmTuyenTinhs = new List<DMBVTuyenTinh>();
        IEnumerable<DMErrorMHTC> _userErrList = new List<DMErrorMHTC>();
        IEnumerable<ChiDinhDongThoi> _ruleDongThois = new List<ChiDinhDongThoi>();
        IEnumerable<PhacDoDieuTri> _rulePhacDos = new List<PhacDoDieuTri>();
        IEnumerable<DMChung130> _dmChung = new List<DMChung130>();
        IEnumerable<DMError130> _dmError130 = new List<DMError130>();
        IEnumerable<DMErrorMHTC> _dMErrorMHTCs = new List<DMErrorMHTC>();
        IEnumerable<DichVuMaMay> _dvuMamays = new List<DichVuMaMay>();

        //1250	1
        //	MA_DICH_VU	Bệnh nhân Ngoại trú nhưng kê giường nội trú
        //1251  1   XML3    MA_DICH_VU  Kê giường ngoại khoa sai TT39/ 2018
        Dictionary<byte, string> _dicPhauThuats = await GetDMPhauThuat();
        Dictionary<string, string> _dicDMDB = await GetDMDacBiet();
        Dictionary<string, string> _dicGiuongs = await GetDMGiuong(maCSKCB);

        try
        {
            //Linhist changed 20/03/26 - selected before

            _ruleThuocs = await _HDSDThuocRepository.GetActiveRules(maCSKCB);
            _ruleDichVus = await _HDSDDichVuRepository.GetActiveRules(maCSKCB);
            _dmBacSi = await _DMBacSiRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
            _dmKhoa = await _DMKhoaRepository.GetListAsync();
            _dmKhoaBV = await _DMKhoaBVRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
            _dmYHCTs = await _DMYHDTRepository.GetListAsync();
            _dmTuyenTinhs = await _DMBVTuyenTinhRepository.GetListAsync();
            _userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null);
            _ruleDongThois = await _ChiDinhDongThoiRepository.GetListAsync(x => true.Equals(x.HIEU_LUC));
            _rulePhacDos = await _PhacDoDieuTriRepository.GetListAsync();
            _dmChung = await _DMChungRepository.GetListAsync();
            _dmThuCong = await _DMThuCongRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
            _dmThietBi = await _DMThietBiRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
            _dmError130 = await _dmErrorService.GetAll() ?? throw new InvalidOperationException("Không lấy được danh mục lỗi 130");
            _dMErrorMHTCs = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB == maCSKCB);
            _dvuMamays = await _dvuMaMayRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);

            //Linhist added 110926 - loại bỏ lỗi đã xóa trên hệ thống
            _dMErrorMHTCs = _dMErrorMHTCs.Where(e => _dmError130.Any(x => e.EMErrorCode == x.ErrorCode && x.IsDelete != true)).ToList();

            if (fromAPI || fromHIS)
            {
                var maThuocNeed = xml01s.SelectMany(y => y.XML02s).Select(y => y.MA_THUOC).ToList();
                _dmThuoc = await _DMThuocRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && maThuocNeed.Contains(x.MA_THUOC));

                var maDMVTYTNeed = xml01s.SelectMany(y => y.XML03s).Select(y => y.MA_VAT_TU).ToList();
                _dmVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.MA_VAT_TU != null && x.MA_VAT_TU != "" && maDMVTYTNeed.Contains(x.MA_VAT_TU));

                var maDichVuNeed = xml01s.SelectMany(y => y.XML03s).Select(y => y.MA_DICH_VU).ToList();
                _dmDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.MA_DICH_VU != null && x.MA_DICH_VU != "" && maDichVuNeed.Contains(x.MA_DICH_VU));
            }
            else
            {
                _dmThuoc = await _DMThuocRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dmVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dmDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
            }
        }
        catch
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: khong lay duoc danh sach cac DM phu tro", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return;
        }

        DateTime dv = DateTime.Now;

        //Note Chưa hiểu sao lại khao báo mấy phần này
        string allMaDVRuleDTs = string.Join(';', _ruleDongThois.Select(x => x.MA_DICH_VU).ToArray());
        string allMaDVKhacRuleDTs = string.Join(';', _ruleDongThois.Select(x => x.DICH_VU_KHAC).ToArray());
        string allMAVTRuleDTs = string.Join(';', _ruleDongThois.Select(x => x.VAT_TU_BHYT).ToArray());

        var allDichVuDTs = new List<XML03>();
        var allDvKhacDTs = new List<XML03>();
        var allVatTuDTs = new List<XML03>();
        var xml3CungNgays = new List<XML03>();
        var dicErrors = _dmError130.ToDictionary(x => x.ErrorCode ?? throw new InvalidOperationException("Mã lỗi rỗng"), x => x.Message ?? string.Empty);
        #endregion

        try
        {
            var x01Validator = new ValidateXML01(_officialTime, _hosoRepository, _dmYHCTs, _dmBacSi, _dmKhoaBV, _dmChung, _dicDMDB, dicErrors, _logger);
            var x23Validator = new ValidateAllXML23(_officialTime, _dmBacSi, _userErrList, _dmYHCTs, _dicDMDB, _dicGiuongs, _dicPhauThuats, _ruleDongThois, _dmThuoc, _dmKhoaBV, _dmDichVu, _dmVTYT, _dmBacSi, _dmThuCong, _dmThietBi, _ruleDichVus, _ruleThuocs,_dmChung,_dmError130, _hosoRepository,_xml03Repository, _xml02Repository, _dMErrorMHTCs, _dvuMamays, _logger);
            var x04Validator = new ValidateXML04(_officialTime, _dmDichVu, _dmBacSi,_dmError130,_dmChung, _logger);

            var x05Validator = new ValidateXML05(_dmBacSi, _dmError130, _dmChung, _logger);
            var x06Validator = new ValidateXML06(_dmBacSi, _logger);
            var x07Validator = new ValidateXML07(_dmBacSi, _logger);
            var x08Validator = new ValidateXML08(_dmBacSi, _logger);
            var x09Validator = new ValidateXML09(_dmBacSi, _logger);
            var x10Validator = new ValidateXML10(_dmBacSi, _logger);
            var x11Validator = new ValidateXML11(_dmBacSi, _logger);
            var x13Validator = new ValidateXML13(_dmYHCTs, _dmBacSi, _logger);
            var x14Validator = new ValidateXML14(_dmYHCTs, _dmBacSi, _logger);
            var x15Validator = new ValidateXML15(_dmBacSi, _logger);

            //Linhist added 15/05/26
            var ttValidator = new ValidateXMLTT(_checkTheRepository, _logger);

            var dte130To3176 = new DateTime(2026, 2, 1);
            foreach (var x01 in xml01s)
            {
                if (x01.IsDelete == true) continue;

                string[] maThes = new string[0];
                DateTime gttheTU = DateTime.MinValue;
                DateTime gttheTU2 = DateTime.MinValue;
                DateTime gttheDEN = DateTime.MinValue;
                DateTime gttheDEN2 = DateTime.MinValue;

                byte?[] mucHuongs = new byte?[] { null, null, null, null };

                var xmlCommon = new XMLCommonInfo(x01);

                #region XML01
                try
                {
                    if (!await x01Validator.Validate(x01, xmlCommon)) continue;
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML01 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }
                #endregion

                #region XML02
                #endregion

                #region XML03
                try
                {

                    await x23Validator.Validate(x01, xmlCommon);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML23 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }
                #endregion

                #region XML04
                var sortedX04s = x01.XML04s.OrderBy(x => x.MA_DICH_VU);
                foreach (var x04 in sortedX04s)
                {
                    try
                    {
                        await x04Validator.Validate(x04);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML04 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }

                Validate_DacBiet_XML4(x01);
                #endregion

                #region XML05
                foreach (var x05 in x01.XML05s)
                {
                    try
                    {
                        await x05Validator.Validate(x05);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML05 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML06
                foreach (var x06 in x01.XML06s)
                {
                    try
                    {
                        await x06Validator.Validate(x06);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML06 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML07
                foreach (var x07 in x01.XML07s)
                {
                    if (x07 is null)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML07 loi null HOSO {x01.MA_LK}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                    else
                    {
                        try
                        {
                            await x07Validator.Validate(x07);

                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML07 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                }
                #endregion

                #region XML08
                foreach (var x08 in x01.XML08s)
                {
                    try
                    {
                        await x08Validator.Validate(x08);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML08 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML09
                foreach (var x09 in x01.XML09s)
                {
                    try
                    {
                        await x09Validator.Validate(x09);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML09 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML10
                foreach (var x10 in x01.XML10s)
                {
                    try
                    {
                        await x10Validator.Validate(x10);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML10 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML11
                foreach (var x11 in x01.XML11s)
                {
                    try
                    {
                        await x11Validator.Validate(x11);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML11 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML13
                foreach (var x13 in x01.XML13s)
                {
                    try
                    {
                        await x13Validator.Validate(x13);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML13 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML14
                foreach (var x14 in x01.XML14s)
                {
                    try
                    {
                        await x14Validator.Validate(x14);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML14 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                #region XML15
                foreach (var x15 in x01.XML15s)
                {
                    try
                    {
                        await x15Validator.Validate(x15);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML15 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                }
                #endregion

                //Linhist added 15/05/26
                #region CHECKTHE THONG TUYEN
                try
                {
                    await ttValidator.Validate(x01);
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so CHECK THE THONG TUYEN loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }
                #endregion

                //Linhist added 09/02/26
                //Trước ngày 01/02/2026 bỏ mã lỗi 3176
                if (x01.NGAY_VAO != null && x01.NGAY_VAO < dte130To3176)
                {
                    x01.RemoveErrorsByCodes(2673, 2674, 2692, 2693, 2698, 2699, 2702, 2703, 2704, 2865, 2868, 2869, 2876, 2877, 2881, 2882, 2883, 2884);
                }
                //Từ ngày 01/02/2026 bỏ mã lỗi 130
                else
                {
                    x01.RemoveErrorsByCodes(1852, 1869, 1910, 1924, 1948, 2023, 2024, 2093, 2096, 2159, 2165, 2171, 2190, 2324, 2402, 2403, 2404, 2405, 2406, 2407, 2408, 2409, 2410, 2411, 2412, 2413);
                }
            }

        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError(ex, false, _logger, "Validate130");
            //Linhist removed 29/03/25
            //new ImportError

            await _unitOfWork.CommitAsync();
        }

        try
        {
            //13.2.2025 checkchange haimaster
            var errors = new List<XMLError>();
            var lstX0s = new List<XMLState>();
            //Linhist removed 22/05/25 - ko check thong tuyen lai nua
            var errCodes = _dMErrorMHTCs.Select(e => e.EMErrorCode).ToHashSet();
            var altCodes = _dMErrorMHTCs.Where(e => e.EMEnabled == false).Select(e => e.EMErrorCode).ToList();

            DateTime? lastChecked = DateTime.Now;
            foreach (var x01 in xml01s)
            {
                if (x01.IsDelete == true) continue;

                var allErrs = "";
                //Sao chep loi khong kem tham chieu XML01 de insert khong keo theo ca ho so
                var e01s = x01.Errors.Where(e => e.MA_LOI.HasValue &&  errCodes.Contains(e.MA_LOI)).OrderBy(e => e.MA_LOI).Distinct().Select(DetachError).ToList();
                foreach (var e in e01s)
                {
                    allErrs += $"{e.MA_LOI};";
                    //checkchange only 25001 giờ update hết
                }

                //Linhist changed 30/07/26

                var x00 = new XMLState
                {
                    XML01ID = x01.XML01ID,
                    MA_CSKCB = x01.MA_CSKCB,
                    MA_LK = x01.MA_LK,
                    DS_LOI = allErrs,
                    TONG_SO_LOI = e01s.Count,
                    CANH_BAO = e01s.Count(e => altCodes.Contains(e.MA_LOI)),
                    Counter = x01.State == null ? 1 : x01.State.Counter + 1,
                    CheckDate = DateTime.Now,
                    VerifyDate = x01.State?.VerifyDate ?? DateTime.Now,
                    Checker = userEmail,
                    CreatedDate = x01.CreateDate ?? DateTime.Now,
                };
                x01.State = null;
                lstX0s.Add(x00);

                //Linhist changed 08/04/25

                //Linhist removed 15/05/26

                //Linhist removed 22/05/25 - ko check thong tuyen lai nua

                errors.AddRange(e01s);

                //Linhist changed 08/04/25
            }
            try
            {

                try
                {
                    await _errorRepository.DeleteListByFilterAsync(x => x.MA_CSKCB == maCSKCB && xmlIds.Contains(x.XML01ID));
                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: Commit xoa XML Error :{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }

                try
                {
                    await _stateRepository.DeleteListByFilterAsync(x => x.MA_CSKCB == maCSKCB && xmlIds.Contains(x.XML01ID));

                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: Commit xoa XML00:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }

                try
                {
                    await _errorRepository.InsertListAsync(errors);
                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: Commit luu XMLError loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }

                try
                {
                    await _stateRepository.InsertListAsync(lstX0s);
                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: Commit luu XML00:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                }

                //checkchange

                //Linhist removed 22/05/25 - ko check thong tuyen lai nua
                try
                {
                    await RemoveHosoFromValidate(xmlIds);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Validate130: Lỗi Bước xóa XML đã giám định tại Validate {maCSKCB}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Validate130: Luu ket qua giam dinh loi loi {maCSKCB}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Validate130: Luu ket qua giam dinh loi loi {maCSKCB}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }

    //Mac dinh includes se lay tat ca XML tu 02-11: "2;3;4;5;6;7;8;9;10;11;13;14;15"
    //Neu chi dinh ro [inners] thi se lay XML theo so chi dinh vd: "2;3;5;7"
    public async Task<bool> AttachInnerXML(IEnumerable<XML01> xmls, string? innerList = null)
    {
        //Linhist changed 02/03/26

        if (!xmls.Any()) return false;
        var inners = (string.IsNullOrWhiteSpace(innerList) ? "2;3;4;5;6;7;8;9;10;11;13;14;15" : innerList).Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (inners.Length == 0)
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: List attached XML not exists", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return false;
        }

        try
        {
            //Linhist changed 02/03/26

            var maCSKCB = xmls.First().MA_CSKCB;
            var lstIds = xmls.Select(x => x.XML01ID).ToArray();
            var attachers = new List<(string Name, Action<XML01> Attach)>();

            async Task<bool> Load<T>(string tag) where T : class, IRecord<XML01>
            {
                if (!inners.Contains(tag)) return true;

                var name = tag.PadLeft(2, '0');
                IEnumerable<T> rows;
                try
                {
                    rows = await _hosoRepository.GetInnersByListID<T>(maCSKCB, lstIds);
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML{name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    return false;
                }

                attachers.Add((name, xml01 => xml01.AddRange(rows.Where(x => x.XML01ID == xml01.XML01ID).ToList())));
                return true;
            }

            if (!await Load<XML02>("2") || !await Load<XML03>("3") || !await Load<XML04>("4") || !await Load<XML05>("5")
                || !await Load<XML06>("6") || !await Load<XML07>("7") || !await Load<XML08>("8") || !await Load<XML09>("9")
                || !await Load<XML10>("10") || !await Load<XML11>("11") || !await Load<XML13>("13") || !await Load<XML14>("14")
                || !await Load<XML15>("15"))
            {
                return false;
            }

            foreach (var xml01 in xmls)
            {
                foreach (var (name, attach) in attachers)
                {
                    try
                    {
                        attach(xml01);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML{name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            return false;
        }

        return true;
    }

    //Mac dinh includes se lay tat ca XML tu 02-11: "2;3;4;5;6;7;8;9;10;11;13;14;15"
    //Neu chi dinh ro [inners] thi se lay XML theo so chi dinh vd: "2;3;5;7"
    public async Task<bool> AttachXMLState(IEnumerable<XML01> xmls)
    {
        var lastID = 0L;
        try
        {
            var maCSKCB = xmls.First().MA_CSKCB;
            var lstIds = xmls.Select(x => x.XML01ID).ToList();
            var xml00s = await _stateRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));

            foreach (var xml01 in xmls)
            {
                lastID = xml01.XML01ID;
                xml01.State = xml00s.FirstOrDefault(x => x.XML01ID == xml01.XML01ID) ?? new XMLState()
                {
                    XML01 = xml01,
                    XML01ID = xml01.XML01ID,
                    MA_CSKCB = xml01.MA_CSKCB,
                    MA_LK = xml01.MA_LK,
                    DS_LOI = string.Empty,
                    TONG_SO_LOI = 0,
                    CANH_BAO = 0,
                    CheckDate = DateTime.Now,
                    Checker = string.Empty,
                    Counter = 0,
                    IsUpload = xml01.IsUpload,
                    VerifyDate = null,
                };
            }
            return true;
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)}");
            _logger.LogError(ex, $"Validate130: Lỗi AttachXML00 - tới ID <{lastID}>: {xmls.Count()}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        return false;
    }

    #region Get List All Categories
    private async Task<Dictionary<string, string>> GetDMDacBiet()
    {
        var _DMDacBiet = await _DMDacBietRepository.GetListAsync();
        var dic = new Dictionary<string, string>();
        foreach (var db in _DMDacBiet)
        {
            var name = db.DMDacBietName ?? throw new InvalidOperationException("DMDacBietName rỗng");
            if (char.IsDigit(name[0]))
            {
                dic.Add(db.DMDacBietValue ?? throw new InvalidOperationException("DMDacBietValue rỗng"), name);
            }
            else
            {
                dic.Add(name.Trim().ToLower(), db.DMDacBietValue ?? string.Empty);
            }
        }

        if (!dic.ContainsKey("congkham")) dic.Add("congkham", "");
        return dic;
    }

    private async Task<Dictionary<string, string>> GetDMGiuong(string maCSKCB)
    {
        var dic = new Dictionary<string, string>();
        var dmGiuongs = await _DMGiuongRepository.GetListAsync(g => !string.IsNullOrEmpty(g.GiuongCode) && g.GiuongType != null && g.MA_CSKCB == maCSKCB);

        //DM Giuong Ngoai tru
        var giuongNgoais = dmGiuongs.Where(g => g.GiuongType == 1);
        var codeNgoais = giuongNgoais.Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
        dic.Add($"GiuongNgoaiAll", string.Join(";", codeNgoais));

        var clsNgoais = giuongNgoais.Select(g => g.GiuongClass).Distinct().OrderBy(c => c);
        foreach (var cls in clsNgoais)
        {
            var codes = giuongNgoais.Where(g => g.GiuongClass == cls).Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
            dic.Add($"GiuongNgoai{cls}", string.Join(";", codes));
        }

        //DM Giuong Noi tru
        var giuongNois = dmGiuongs.Where(g => g.GiuongType == 2);
        var codeNois = giuongNois.Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
        dic.Add($"GiuongNoiAll", string.Join(";", codeNois));

        var clsNois = giuongNois.Select(g => g.GiuongClass).Distinct().OrderBy(c => c);
        foreach (var cls in clsNois)
        {
            var codes = giuongNois.Where(g => g.GiuongClass == cls).Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
            dic.Add($"GiuongNoi{cls}", string.Join(";", codes));
        }

        //DM Giuong Cap Cuu
        var codeCapCuus = dmGiuongs.Where(g => g.GiuongType == 3).Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
        dic.Add($"GiuongCapCuu", string.Join(";", codeCapCuus));

        return dic;
    }

    private async Task<Dictionary<byte, string>> GetDMPhauThuat()
    {
        //DM Phhau thuat Giuong Loai
        var dic = new Dictionary<byte, string>();
        var dmPhauThuats = await _DMPhauThuatRepository.GetListAsync(p => p.PhauThuatGiuongLoai != null);
        var giuongLoais = dmPhauThuats.Select(p => p.PhauThuatGiuongLoai).Distinct().OrderBy(l => l);
        foreach (var l in giuongLoais)
        {
            if (l == null) continue;
            var codes = dmPhauThuats.Where(p => p.PhauThuatGiuongLoai == l).Select(p => p.PhauThuatCode).Distinct().OrderBy(c => c);
            dic.Add(l.Value, string.Join(";", codes));
        }
        return dic;
    }
    #endregion

    private const string strDVXml4 = "20.0059.0140;20.0059.0140;02.0295.0498;02.0295.0498;02.0295.0498;02.0295.0498;02.0286.0497;02.0286.0497;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0282.0318;02.0282.0318;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0271.0140;02.0271.0140;20.0076.0140;01.0232.0140;01.0232.0140;02.0307.0136;02.0307.0136;02.0307.0136;02.0307.0136;02.0307.0136;02.0262.0136;02.0262.0136;20.0053.0105;20.0053.0105;02.0217.0183;02.0369.0185;01.0101.0125;02.0038.0125;02.0283.0141;02.0284.0141;02.0275.0141;02.0275.0141;02.0275.0141;02.0275.0141;20.0055.0496;20.0056.0141;02.0036.0127;02.0036.0127;02.0036.0128;02.0036.0128;02.0045.0130;02.0045.0130;02.0045.0132;02.0045.0132;02.0043.0127;02.0043.0127;02.0304.0134;02.0304.0134;02.0304.0134;02.0304.0134;02.0304.0134;02.0309.0138;02.0309.0138;02.0309.0138;20.0072.0191;20.0044.0503;03.2340.0157;03.2340.0157";

    private static XMLError DetachError(XMLError e) => new()
    {
        XMLEID = e.XMLEID,
        XML01ID = e.XML01ID,
        MA_CSKCB = e.MA_CSKCB,
        STT = e.STT,
        MA_LOI = e.MA_LOI,
        ErrorDesc = e.ErrorDesc,
        ErrorDate = e.ErrorDate,
    };

    private void Validate_DacBiet_XML4(XML01 x01)
    {
        //431 	1 	XML4 	KET_LUAN 	Dịch vụ Giải phẫu bệnh nhuộm hai màu không có kết luận Trợt, loét, polyp, U hoặc Khối
        var allX04s = x01.XML04s.Where(x => Uti.CodeContain(x.MA_DICH_VU ?? string.Empty, strDVXml4));
        if (!allX04s.Any()) return;

        if (x01.XML05s.Any(x => Uti.StrExists(x.DIEN_BIEN_LS ?? string.Empty, true, "Trợt", "loét", "polyp", "U hoặc Khối"))) return;

        foreach (var x04 in allX04s)
        {
            x04.RaiseError(431);
        }
    }

}
