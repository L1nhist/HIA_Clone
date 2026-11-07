using HIA.Core.Models.Token;
using HIA.Data.Auths;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Reports;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Repositories.User;
using HIA.Services.Categories;
using HIA.Services.HospitalManagement;
using HIA.Services.Report;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIDataController : ControllerBase
    {
        private readonly IDMErrorService130 _dmErrorService130;
        private readonly IBaseRepository<DMThuoc> _dmThuocRepository;
        private readonly IBaseRepository<DMVTYT> _dmVTYTRepository;
        private readonly IBaseRepository<DMDichVu> _dmDichVuRepository;
        private readonly IBaseRepository<DMDVThuCong> _dmThuCongRepository;
        private readonly IBaseRepository<DMBacSi> _dmBacSiRepository;
        private readonly IBaseRepository<DMKhoaBV> _dmKhoaBVRepository;
        private readonly IBaseRepository<DMDacBiet> _dmDacBietRepository;
        private readonly IBaseRepository<DMYHDTICD10> _dmYHDTRepository;
        private readonly IBaseRepository<DMPhauThuat> _dmPhauThuatRepository;
        private readonly IBaseRepository<DMGiuong> _dmGiuongRepository;
        private readonly IBaseRepository<DMThietBi> _dmThietBiRepository;
        private readonly IBaseRepository<DMChung130> _dmChungRepository;
        private readonly IBaseRepository<ChiDinhDongThoi> _chiDinhRepository;
        private readonly IBaseRepository<DichVuMaMay> _dichVuMaMayRepository;
        private readonly IBaseRepository<HDSDDichVu> _hdsdDichVuRepository;
        private readonly IBaseRepository<HDSDThuoc> _hdsdThuocRepository;
        private readonly IBaseRepository<CheckThe4210> _checkTheRepository;
        private readonly IBaseRepository<SysHistoryLog> _sysLogRepository;
        private readonly IBaseRepository<UserLogs> _usrLogRepository;
        private readonly IBaseRepository<XMLState> _stateRepository;
        private readonly IXml01Repository _xml01Repository;
        private readonly IHospitalService _hospitalService;
        private readonly IReportService _reportService;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AIDataController> _logger;
        private readonly IConfiguration _configs;

        public AIDataController(
            IDMErrorService130 dmErrorService130,
            IBaseRepository<DMThuoc> dmThuocRepository,
            IBaseRepository<DMVTYT> dmVTYTRepository,
            IBaseRepository<DMDichVu> dmDichVuRepository,
            IBaseRepository<DMDVThuCong> dmThuCongRepository,
            IBaseRepository<DMBacSi> dmBacSiRepository,
            IBaseRepository<DMKhoaBV> dmKhoaBVRepository,
            IBaseRepository<DMDacBiet> dmDacBietRepository,
            IBaseRepository<DMYHDTICD10> dmYHDTRepository,
            IBaseRepository<DMPhauThuat> dmPhauThuatRepository,
            IBaseRepository<DMGiuong> dmGiuongRepository,
            IBaseRepository<DMThietBi> dmThietBiRepository,
            IBaseRepository<DMChung130> dmChungRepository,
            IBaseRepository<ChiDinhDongThoi> chiDinhRepository,
            IBaseRepository<DichVuMaMay> dichVuMaMayRepository,
            IBaseRepository<HDSDDichVu> hdsdDichVuRepository,
            IBaseRepository<HDSDThuoc> hdsdThuocRepository,
            IBaseRepository<CheckThe4210> checkTheRepository,
            IBaseRepository<SysHistoryLog> sysLogRepository,
            IBaseRepository<UserLogs> usrLogRepository,
            IBaseRepository<XMLState> stateRepository,
            IXml01Repository xml01Repository,
            IHospitalService hospitalService,
            IReportService reportService,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AIDataController> logger,
            IConfiguration configs
        )
        {
            _dmErrorService130 = dmErrorService130;
            _dmThuocRepository = dmThuocRepository;
            _dmVTYTRepository = dmVTYTRepository;
            _dmDichVuRepository = dmDichVuRepository;
            _dmThuCongRepository = dmThuCongRepository;
            _dmBacSiRepository = dmBacSiRepository;
            _dmKhoaBVRepository = dmKhoaBVRepository;
            _dmDacBietRepository = dmDacBietRepository;
            _dmYHDTRepository = dmYHDTRepository;
            _dmPhauThuatRepository = dmPhauThuatRepository;
            _dmGiuongRepository = dmGiuongRepository;
            _dmThietBiRepository = dmThietBiRepository;
            _dmChungRepository = dmChungRepository;
            _chiDinhRepository = chiDinhRepository;
            _dichVuMaMayRepository = dichVuMaMayRepository;
            _hdsdDichVuRepository = hdsdDichVuRepository;
            _hdsdThuocRepository = hdsdThuocRepository;
            _checkTheRepository = checkTheRepository;
            _sysLogRepository = sysLogRepository;
            _usrLogRepository = usrLogRepository;
            _stateRepository = stateRepository;
            _xml01Repository = xml01Repository;
            _hospitalService = hospitalService;
            _reportService = reportService;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configs = configs;
        }

        private bool IsAuthorized(string key)
        {
            var ips = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(ips)) return false;

            var aKey = _configs.GetSection("AppSettings:AuthKey").Value;
            var aIps = _configs.GetSection("AppSettings:AuthIPS").Value;
            if (string.IsNullOrEmpty(aKey) || string.IsNullOrEmpty(aIps)) return false;
            if (key != aKey || (aIps != "*" && ips != aIps)) return false;
            return true;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetHospitals")]
        public async Task<IActionResult> GetHospitals(string key)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var hosp = await _hospitalService.GetAll();
                if (hosp == null || hosp.Data == null || !hosp.Data.Any()) return NotFound();
                return Ok(hosp);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDmErrors")]
        public async Task<IActionResult> GetDMErrors(string key)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmErrorService130.GetAllListing();
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetXmlCount")]
        public async Task<IActionResult> GetXmlCount(string key, string date)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dte)) return NotFound();

                var res = await _reportService.GetDaily(dte);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetXmlError")]
        public async Task<IActionResult> GetXmlErrors(string key, string maCSKCB, long? xml01ID, string maLK)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (string.IsNullOrEmpty(maCSKCB) || ((xml01ID == null || xml01ID <= 0) && string.IsNullOrEmpty(maLK))) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                XML01? xml01 = null;
                var x01s = xml01ID == null || xml01ID < 0 ? null : await _xml01Repository.GetAllByListID(maCSKCB, xml01ID.Value);
                if (x01s != null && x01s.Any())
                {
                    xml01 = x01s.FirstOrDefault();
                }

                if (xml01 == null && !string.IsNullOrEmpty(maLK))
                {
                    x01s = await _xml01Repository.GetAllByMaLK(maCSKCB, maLK);
                    if (x01s != null && x01s.Any())
                    {
                        xml01 = x01s.OrderByDescending(x => x.CreateDate).First();
                        xml01ID = xml01.XML01ID;
                    }
                }

                if (xml01 == null) return NotFound();

                var erCodes = await _dmErrorService130.GetAllByHospital(maCSKCB);
                var errs = await _xml01Repository.GetErrorsByListID(maCSKCB.Trim(), xml01.XML01ID);
                var res = errs?.Where(e => erCodes == null || erCodes.Any(c => c.ErrorCode == e.MA_LOI)).Select(x => new
                {
                    x.XMLEID,
                    x.XML01ID,
                    xml01.MA_CSKCB,
                    xml01.MA_LK,
                    xml01.MA_BN,
                    x.STT,
                    x.MA_LOI,
                    x.ErrorDesc,
                    x.ErrorDate,
                }).ToList();

                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetXmlRaw")]
        public async Task<IActionResult> GetXmlRaw(string key, string maCSKCB, long? xml01ID, string maLK, string file)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (string.IsNullOrEmpty(maCSKCB) || ((xml01ID == null || xml01ID <= 0) && string.IsNullOrEmpty(maLK))) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                XML01? xml01 = null;
                var x01s = xml01ID == null || xml01ID < 0 ? null : await _xml01Repository.GetAllByListID(maCSKCB, xml01ID.Value);
                if (x01s != null && x01s.Any())
                {
                    xml01 = x01s.FirstOrDefault();
                }

                if (xml01 == null && !string.IsNullOrEmpty(maLK))
                {
                    x01s = await _xml01Repository.GetAllByMaLK(maCSKCB, maLK);
                    if (x01s != null && x01s.Any())
                    {
                        xml01 = x01s.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                    }
                }

                if (xml01 == null) return NotFound();

                var x = 1;
                if (int.TryParse((file ?? "1").ToLower().Replace("xml", "").Trim(), out var f)) x = f;

                switch (x)
                {
                    case 2:
                        var xml02s = await _xml01Repository.GetInnersByListID<XML02>(maCSKCB, xml01.XML01ID);
                        return xml02s != null ? Ok(xml02s) : NotFound();

                    case 3:
                        var xml03s = await _xml01Repository.GetInnersByListID<XML03>(maCSKCB, xml01.XML01ID);
                        return xml03s != null ? Ok(xml03s) : NotFound();

                    case 4:
                        var xml04s = await _xml01Repository.GetInnersByListID<XML04>(maCSKCB, xml01.XML01ID);
                        return xml04s != null ? Ok(xml04s) : NotFound();

                    case 5:
                        var xml05s = await _xml01Repository.GetInnersByListID<XML05>(maCSKCB, xml01.XML01ID);
                        return xml05s != null ? Ok(xml05s) : NotFound();

                    case 6:
                        var xml06s = await _xml01Repository.GetInnersByListID<XML06>(maCSKCB, xml01.XML01ID);
                        if (xml06s == null || !xml06s.Any())
                        {
                            foreach (var xml06 in xml01.XML06s)
                            {
                                xml06.SO_CCCD = ".....";
                            }
                        }
                        return xml06s != null ? Ok(xml06s) : NotFound();

                    case 7:
                        var xml07s = await _xml01Repository.GetInnersByListID<XML07>(maCSKCB, xml01.XML01ID);
                        if (xml07s == null || !xml07s.Any())
                        {
                            foreach (var xml07 in xml01.XML07s)
                            {
                                xml07.HO_TEN_CHA = ".....";
                                xml07.HO_TEN_ME = ".....";
                            }
                        }
                        return xml07s != null ? Ok(xml07s) : NotFound();

                    case 8:
                        var xml08s = await _xml01Repository.GetInnersByListID<XML08>(maCSKCB, xml01.XML01ID);
                        return xml08s != null ? Ok(xml08s) : NotFound();

                    case 9:
                        var xml09s = await _xml01Repository.GetInnersByListID<XML09>(maCSKCB, xml01.XML01ID);
                        if (xml09s == null || !xml09s.Any())
                        {
                            foreach (var xml09 in xml01.XML09s)
                            {
                                xml09.HO_TEN_CHA = ".....";
                                xml09.HO_TEN_CON = ".....";
                                xml09.HO_TEN_NND = ".....";
                                xml09.SO_CCCD_NND = ".....";
                            }
                        }
                        return xml09s != null ? Ok(xml09s) : NotFound();

                    case 10:
                        var xml10s = await _xml01Repository.GetInnersByListID<XML10>(maCSKCB, xml01.XML01ID);
                        return xml10s != null ? Ok(xml10s) : NotFound();

                    case 11:
                        var xml11s = await _xml01Repository.GetInnersByListID<XML11>(maCSKCB, xml01.XML01ID);
                        return xml11s != null ? Ok(xml11s) : NotFound();

                    case 12:
                        return NotFound();

                    case 13:
                        var xml13s = await _xml01Repository.GetInnersByListID<XML13>(maCSKCB, xml01.XML01ID);
                        return xml13s != null ? Ok(xml13s) : NotFound();

                    case 14:
                        var xml14s = await _xml01Repository.GetInnersByListID<XML14>(maCSKCB, xml01.XML01ID);
                        return xml14s != null ? Ok(xml14s) : NotFound();

                    case 15:
                        var xml15s = await _xml01Repository.GetInnersByListID<XML15>(maCSKCB, xml01.XML01ID);
                        return xml15s != null ? Ok(xml15s) : NotFound();

                    default:
                        if (xml01 != null)
                        {
                            xml01.HO_TEN = ".....";
                            xml01.SO_CCCD = ".....";
                            xml01.DIA_CHI = ".....";
                            xml01.DIEN_THOAI = ".....";
                        }
                        return xml01 != null ? Ok(xml01) : NotFound();
                }
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetCheckThe")]
        public async Task<IActionResult> GetCheckThe(string key, string maCSKCB, string maLK)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _checkTheRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && t.MA_LK == maLK);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMThuoc")]
        public async Task<IActionResult> GetDMThuoc(string key, string maCSKCB, string maThuoc)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmThuocRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && t.MA_THUOC == maThuoc);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMDichVu")]
        public async Task<IActionResult> GetDMDichVu(string key, string maCSKCB, string maDichVu)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmDichVuRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && t.MA_DICH_VU == maDichVu);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMVTYT")]
        public async Task<IActionResult> GetDMVTYT(string key, string maCSKCB, string maVTYT)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmVTYTRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && t.MA_VAT_TU == maVTYT);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMThietBi")]
        public async Task<IActionResult> GetDMThietBi(string key, string maCSKCB, string maMay)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmThietBiRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && (t.MA_MAY == maMay || (t.MA_MAY != null && maMay.Contains(t.MA_MAY))));
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMDVThuCong")]
        public async Task<IActionResult> GetDMDVThuCong(string key, string maCSKCB, string maDichVu)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmThuCongRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && t.MA_DICH_VU == maDichVu);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMBacSi")]
        public async Task<IActionResult> GetDMBacSi(string key, string maCSKCB, string maBacsi)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmBacSiRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && ((t.MA_CCHN != null && maBacsi.Contains(t.MA_CCHN)) || (t.MA_BHXH != null && maBacsi.Contains(t.MA_BHXH))));
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMKhoaBV")]
        public async Task<IActionResult> GetDMKhoaBV(string key, string maCSKCB, string maKhoa)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmKhoaBVRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && (t.MA_KHOA == maKhoa || (t.MA_KHOA != null && maKhoa.Contains(t.MA_KHOA))));
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMYHDTICD10")]
        public async Task<IActionResult> GetDMYHDTICD10(string key, string maYHDT, string maICD10)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                if (!string.IsNullOrEmpty(maYHDT))
                {
                    var res = await _dmYHDTRepository.GetListAsync(t => t.YHDTCode == maYHDT);
                    return Ok(res);
                }
                else if (!string.IsNullOrEmpty(maICD10))
                {
                    var res = await _dmYHDTRepository.GetListAsync(t => t.ICD10Code == maICD10);
                    return Ok(res);
                }
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMChung130")]
        public async Task<IActionResult> GetDMChung130(string key, string loai)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmChungRepository.GetListAsync(t => t.LOAI == loai);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMDacBiet")]
        public async Task<IActionResult> GetDMDacBiet(string key, string name)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmDacBietRepository.GetListAsync(t => t.DMDacBietName == name);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMGiuong")]
        public async Task<IActionResult> GetDMGiuong(string key, string maCSKCB, string maGiuong)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmGiuongRepository.GetListAsync(t => t.MA_CSKCB == maCSKCB && t.GiuongCode == maGiuong);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDMPhauThuat")]
        public async Task<IActionResult> GetDMPhauThuat(string key, string maPhauThuat)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _dmPhauThuatRepository.GetListAsync(t => t.PhauThuatCode == maPhauThuat);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetDichVuMaMay")]
        public async Task<IActionResult> GetDichVuMaMay(string key, string maCSKCB, string maDichVu)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var where = PredicateBuilder.New<DichVuMaMay>(true);
                if (!string.IsNullOrEmpty(maCSKCB))
                {
                    where = where.And(e => e.MA_CSKCB == maCSKCB || (";" + e.MA_CSKCB + ";").Contains(";" + maCSKCB + ";"));
                }

                if (!string.IsNullOrEmpty(maDichVu))
                {
                    where = where.And(e => e.MA_DICH_VU == maDichVu || (";" + e.MA_DICH_VU + ";").Contains(";" + maDichVu + ";"));
                }

                var res = await _dichVuMaMayRepository.GetListAsync(where, q => q.OrderBy(e => e.DVMMID));
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetChiDinhDongThoi")]
        public async Task<IActionResult> GetChiDinhDongThoi(string key)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _chiDinhRepository.GetListAsync();
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetHDSDDichVu")]
        public async Task<IActionResult> GetHDSDDichVu(string key)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var res = await _hdsdDichVuRepository.GetListAsync();
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetHDSDThuoc")]
        public async Task<IActionResult> GetHDSDThuoc(string key)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var context = ((IInfrastructure<IServiceProvider>)_hdsdThuocRepository.DbSet).Instance.GetRequiredService<ICurrentDbContext>().Context;
                context.ChangeTracker.LazyLoadingEnabled = false;

                var res = await _hdsdThuocRepository.GetListAsync();

                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers(string key, string maCSKCB)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                var users = await _userRepository.GetListAsync(u => u.Hospital != null && u.Hospital.MA_CSKCB == maCSKCB && u.IsActivated && !u.IsDeleted, null, q => q.Include(u => u.UserRoles));
                var res = users?.Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    IsAdmin = u.UserRoles != null && u.UserRoles.Any(r => r.RoleId == RoleConstant.ADMIN),
                }).ToList();
                return res != null ? Ok(res) : NotFound();
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetUserActions")]
        public async Task<IActionResult> GetUserActions(string key, string? maCSKCB, string email, string date)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                // Cùng truy vấn với BaseRepository.GetListAsync (AsNoTracking + Where + Include), viết trực tiếp để Include Hospital? không lệch kiểu với IBaseRepository.
                IEnumerable<User>? urs = string.IsNullOrEmpty(maCSKCB) ? null : await _userRepository.DbSet.AsNoTracking()
                                                .Where(u => u.Hospital != null && u.Hospital.MA_CSKCB == maCSKCB && u.IsActivated && !u.IsDeleted)
                                                .Include(u => u.Hospital).ToListAsync();
                if (urs == null && !string.IsNullOrEmpty(email))
                {
                    urs = await _userRepository.DbSet.AsNoTracking()
                                                .Where(u => u.Email == email && u.IsActivated && !u.IsDeleted)
                                                .Include(u => u.Hospital).ToListAsync();
                    if (urs == null || !urs.Any()) return NotFound();
                    var hospital = urs.First().Hospital;
                    if (hospital == null) return NotFound();
                    maCSKCB = hospital.MA_CSKCB;
                }

                if (urs == null || !urs.Any()) return NotFound();
                if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dte)) return NotFound();

                var emails = urs.Select(u => u.Email).ToList();
                var slogs = await _sysLogRepository.DbSet.AsQueryable().AsNoTracking()
                                                .Where(l => emails.Contains(l.Email) && l.ExecutionTime != null && l.ExecutionTime >= dte.Date && l.ExecutionTime < dte.Date.AddDays(1))
                                                .GroupBy(l => new { l.Email, l.FunctionName }, (k, g) => k).ToListAsync();

                var ulogs = await _usrLogRepository.DbSet.AsQueryable().AsNoTracking()
                                                .Where(l => emails.Contains(l.Email) && l.LogTime >= dte.Date && l.LogTime < dte.Date.AddDays(1))
                                                .GroupBy(l => new { l.Email, l.Action }, (k, g) => k).ToListAsync();

                var ids = urs.Select(u => u.Id).ToList();
                var xls = await _xml01Repository.DbSet.AsQueryable().AsNoTracking()
                                                .Where(x => x.CreatorID != null && ids.Contains(x.CreatorID) && x.MA_CSKCB == maCSKCB && x.CreateDate >= dte.Date && x.CreateDate < dte.Date.AddDays(1))
                                                .GroupBy(x => new { x.CreatorID, x.IsUpload }, (k, g) => k).ToListAsync();

                return Ok(urs.Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    has_Login = slogs?.Any(l => l.Email == u.Email && l.FunctionName == "SignIn") ?? false,
                    has_LoginAPI = slogs?.Any(l => l.Email == u.Email && l.FunctionName == "SignIn API") ?? false,
                    has_ViewFiles = slogs?.Any(l => l.Email == u.Email && l.FunctionName == "Get hospital list by filter") ?? false,
                    has_Report = ulogs?.Any(l => l.Email == u.Email && l.Action == "Lấy báo cáo hệ thống") ?? false,
                    has_ChatGPT = ulogs?.Any(l => l.Email == u.Email && l.Action == "Sử dụng API ChatGPT") ?? false,
                    has_Upload = xls?.Any(x => x.CreatorID == u.Id && x.IsUpload == true) ?? false,
                    has_UploadAPI = xls?.Any(x => x.CreatorID == u.Id && x.IsUpload != true) ?? false,
                }).ToArray());
            }
            catch
            { }

            return NotFound();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("GetXMLStates")]
        public async Task<IActionResult> GetXMLStates(string key, string maCSKCB, string date)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dte)) return NotFound();

                var states = await _stateRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.CreatedDate >= dte.Date && x.CreatedDate < dte.Date.AddDays(1));
                if (states == null) return NotFound();

                var xml01s = await _xml01Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.CreateDate >= dte.Date && x.CreateDate < dte.Date.AddDays(1));
                var res = states.Join(xml01s, s => s.XML01ID, x => x.XML01ID, (s, x) => new
                {
                    s.XML01ID,
                    s.MA_CSKCB,
                    s.MA_LK,
                    s.DS_LOI,
                    s.TONG_SO_LOI,
                    s.CANH_BAO,
                    s.Counter,
                    s.Checker,
                    s.CheckDate,
                    s.VerifyDate,
                    s.IsUpload,
                    s.CreatedDate,
                    x?.MA_BN,
                    x?.HO_TEN,
                    x?.SO_CCCD,
                    x?.MA_THE_BHYT,
                    x?.NGAY_SINH,
                    x?.GIOI_TINH,
                    x?.MA_LOAI_KCB,
                    x?.MA_BENH_CHINH,
                    x?.MA_BENH_KT,
                    x?.MA_KHOA,
                    x?.NGAY_LAP,
                    x?.NGAY_RA,
                    x?.NGAY_VAO,
                    x?.CreatorID,
                });

                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SetErrorDesc")]
        public async Task<IActionResult> SetErrorDesc(string key, [FromForm] int[] codes, [FromForm] string[] descs)
        {
            try
            {
                //Check Authen
                if (string.IsNullOrEmpty(key)) return BadRequest();
                if (!IsAuthorized(key)) return Unauthorized();

                if (codes?.Any() != true || descs?.Any() != true || codes.Length != descs.Length) return BadRequest();

                var res = await _dmErrorService130.UpdateErrorDescs(codes, descs);
                return Ok(res);
            }
            catch
            { }

            return NotFound();
        }
    }
}
