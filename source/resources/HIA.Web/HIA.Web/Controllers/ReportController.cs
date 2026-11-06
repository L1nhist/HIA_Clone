using HIA.Core.Base;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.ThongKe;
using HIA.Services.HospitalManagement;
using HIA.Services.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IReportHandleService _repHandleService;
        private readonly IHospitalService _hospitalService;
        private readonly IThongKeRepository _thongKeRepository;
        private readonly IBaseRepository<UserLogs> _userLogsRepository;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IConfiguration _configs;
        private readonly IMemoryCache _cache;

        public ReportController(
            IReportService reportService,
            IReportHandleService repHandleService,
            IHospitalService hospitalService,
            IThongKeRepository thongkeRepository,
            IBaseRepository<UserLogs> userLogsRepository,
            IHttpContextAccessor httpContext,
            IConfiguration configs,
            IMemoryCache cache
        )
        {
            _reportService = reportService;
            _repHandleService = repHandleService;
            _hospitalService = hospitalService;
            _thongKeRepository = thongkeRepository;
            _userLogsRepository = userLogsRepository;
            _httpContext = httpContext;
            _configs = configs;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value ?? "";
                if (!_cache.TryGetValue(maCSKCB, out AIStatModel data))
                {
                    var endPnt = $"{_configs.GetSection("AppSettings:StatPath").Value}{maCSKCB}";
                    string endkey = _configs.GetSection("AppSettings:StatKey").Value;

                    using var http = new HttpClient();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // X-Api-Key đặt cố định ở đây — không log lại request headers ở tầng gọi để tránh lộ key ra log.
                    http.DefaultRequestHeaders.Remove("X-Api-Key");
                    http.DefaultRequestHeaders.Add("X-Api-Key", endkey);

                    var resp = await http.GetAsync(endPnt);
                    var respBody = await resp.Content.ReadAsStringAsync();
                    data = JsonSerializer.Deserialize<AIStatModel>(respBody);
                    // Set cache options
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)) // Max lifecycle
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));  // Resets time if accessed

                    if (data != null && data.Day != null && data.Day.TheoNgay != null)
                    {
                        data.DataLines = data.Day.TheoNgay.Select(e => new DataPoints { Y = e.SoLanDay, Label = DateTime.TryParseExact(e.Ngay, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? $"new Date({d.Year},{d.Month - 1},{d.Day})" : "" }).ToList();
                        data.DataBars = data.Day.TheoNgay.Select(e => new DataPoints { Y = e.SoHoSo, Label = DateTime.TryParseExact(e.Ngay, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? $"new Date({d.Year},{d.Month - 1},{d.Day})" : "" }).ToList();
                    }
                    if (data != null && data.DayLai != null)
                    {
                        data.DataPies.Clear();
                        data.DataPies.Add(new DataPointsPie { Y = data.DayLai.SoCaCaiThien, name = "Cải thiện (giảm lỗi)", exploded = true });
                        data.DataPies.Add(new DataPointsPie { Y = data.DayLai.SoCaYHet, name = "Không thay đổi", exploded = true });
                        data.DataPies.Add(new DataPointsPie { Y = data.DayLai.SoCaXauHon, name = "Xấu hơn (tăng lỗi)", exploded = true });
                    }

                    // Save data to cache
                    _cache.Set(maCSKCB, data, cacheEntryOptions);
                }


                return View(data ?? new());
            }
            catch// (Exception ex)
            { }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Totalize(string createDate)
        {
            DateTime dte = DateTime.Now;
            if (string.IsNullOrEmpty(createDate) || !DateTime.TryParseExact(createDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dte))
            {
                dte = DateTime.Now;
            }

            TotalizeReport res = null;
            Summary cskcbrunning = new Summary();
            var hospitals = await _hospitalService.GetActiveByOwner();
            if (hospitals != null && hospitals.Data.Any())
            {
                ViewBag.Hospitals = hospitals.Data;

                res = await _reportService.GetThongKe(dte);
                int totalXML = 0;
                int totatCSKCBactiveToDay = 0;
                if (res != null)
                {
                    var curs = await _hospitalService.CountFilesToday();
                    if (curs != null)
                    {
                        var allCSKCBs = curs.Select(e => e.Key).Distinct().ToList();
                        var activs = await _userLogsRepository.GetListAsync(x => allCSKCBs.Contains(x.MA_CSKCB) && x.LogTime.Date == DateTime.Now.Date && x.Action.ToLower().Contains("báo cáo"));
                        foreach (var d in curs)
                        {
                            totalXML += d.Value;
                            totatCSKCBactiveToDay += 1;
                            res.Add(new TotalizeItem
                            {
                                MaCSKCB = d.Key,
                                Hospital = "",
                                TotalFiles = d.Value,
                                FromDate = DateTime.Now.Date
                            });
                            cskcbrunning.Add(new Summaryitem
                            {
                                MaCSKCB = d.Key,
                                Hospital = "",
                                isUploadFiles = d.Value > 0,
                                isActive = activs.Any(x => x.MA_CSKCB == d.Key),
                                FromDate = DateTime.Now.Date
                            });
                    }
                    }
                    res.SelectedDate = dte;
                }

                ViewBag.TotalXML = totalXML;
                ViewBag.TotalHospitals = hospitals.Data.Count();
                ViewBag.TotatCSKCBactiveToDay = totatCSKCBactiveToDay;
                ViewBag.CSKCBrunning = cskcbrunning.Count(x=>x.isActive==true);
            }

            return View(res);
        }

        [HttpGet]
        public async Task<IActionResult> Summary(string createDate)
        {
            DateTime dte = DateTime.Now;
            if (string.IsNullOrEmpty(createDate) || !DateTime.TryParseExact(createDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dte))
            {
                dte = DateTime.Now;
            }

            Summary res = null;
            var hospitals = await _hospitalService.GetActiveByOwner();
            if (hospitals != null && hospitals.Data.Any())
            {
                ViewBag.Hospitals = hospitals.Data;

                res = await _reportService.GetSummary(dte);
                if (res != null && res.Any())
                {
                    var allCSKCBs = res.Select(e => e.MaCSKCB).Distinct().ToList();
                    var minDte = res.Min(e => e.FromDate);
                    var maxDte = res.Max(e => e.FromDate);
                    var activs = await _userLogsRepository.GetListAsync(x => allCSKCBs.Contains(x.MA_CSKCB) && x.LogTime.Date >= minDte && x.LogTime <= maxDte && x.Action.ToLower().Contains("báo cáo"));
                    foreach (var r in res)
                    {
                        r.isActive = activs?.Any(e => e.MA_CSKCB == r.MaCSKCB && e.LogTime.Date == r.FromDate) == true;
                    }
                }    
                

                var curs = await _hospitalService.CountFilesToday();
                if (curs != null)
                {
                    var allCSKCBs = curs.Select(e => e.Key).Distinct().ToList();
                    var activs = await _userLogsRepository.GetListAsync(x => allCSKCBs.Contains(x.MA_CSKCB) && x.LogTime.Date == DateTime.Now.Date && x.Action.ToLower().Contains("báo cáo"));
                    foreach (var d in curs)
                    {
                        res.Add(new Summaryitem
                        {
                            MaCSKCB = d.Key,
                            Hospital = "",
                            TotalUploads = d.Value,
                            isUploadFiles = d.Value > 0,
                            isActive = activs?.Any(e => e.MA_CSKCB == d.Key) == true,
                            FromDate = DateTime.Now.Date
                        });
                    }
                    res.SelectedDate = dte;
                }
                // Lấy danh sách bệnh viện không có trong res
               
                ViewBag.TotalHospitals = hospitals.Data.Count();
             
                var hospitalactive = res.Where(x => x.isActive == true).Select(x=>x.MaCSKCB).Distinct().Count();
                ViewBag.Hospitalactive = hospitalactive;
                var hospitalupload = res.Where(x => x.isUploadFiles == true).Select(x=>x.MaCSKCB).Distinct().Count();
                ViewBag.Hospitalupload = hospitalupload;

            }
            return View(res);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int type = 0)
        {
            try
            {
                var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value ?? "";
                if (!_cache.TryGetValue(maCSKCB, out AIStatModel data))
                {
                    var endPnt = $"{_configs.GetSection("AppSettings:StatPath").Value}{maCSKCB}";
                    string endkey = _configs.GetSection("AppSettings:StatKey").Value;

                    using var http = new HttpClient();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // X-Api-Key đặt cố định ở đây — không log lại request headers ở tầng gọi để tránh lộ key ra log.
                    http.DefaultRequestHeaders.Remove("X-Api-Key");
                    http.DefaultRequestHeaders.Add("X-Api-Key", endkey);

                    var resp = await http.GetAsync(endPnt);
                    var respBody = await resp.Content.ReadAsStringAsync();
                    data = JsonSerializer.Deserialize<AIStatModel>(respBody);
                    // Set cache options
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)) // Max lifecycle
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));  // Resets time if accessed

                    // Save data to cache
                    _cache.Set(maCSKCB, data, cacheEntryOptions);
                }

                var xel = await _repHandleService.BaoCaoDayDuLieu(type == 1 ? data.DayLai.DSYHet : type == 2 ? data.DayLai.DSCaiThien : type == 3 ? data.DayLai.DSXauHon : data.DayLai.DSKoChinhSua, maCSKCB, type);
                string contentType = $"application/{FileTypeConstant.XLSX_TYPE}";
                return File(xel.Data, contentType, $"{(type == 1 ? "DS_Sua_Y_Het" : type == 2 ? "DS_Cai_Thien" : type == 3 ? "DS_Xau_Hon" : "DS_Ko_Chinh_Sua")}.xlsx");
            }
            catch// (Exception ex)
            { }

            return NotFound();
        }
    }
}