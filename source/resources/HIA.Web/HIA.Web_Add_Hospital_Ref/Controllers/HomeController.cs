using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.ThongKe;
using HIA.Services.Files;
using HIA.Services.Report;
using HIA.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HIA.Core.Models.Token;
using HIA.Repositories.Base;

namespace HIA.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static readonly long DatetimeMinTimeTicks =
         (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        private readonly IFilesService _filesService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUserManagementService _UserManagementService;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IThongKeRepository _thongKeRepository;
        private readonly IBaseRepository<ThongKeBenh> _benhRepository;

        public HomeController(
             IFilesService filesService,
             IReportService reportService,
             IHttpContextAccessor httpContext,
             IUserManagementService UserManagementService,
             IHttpContextAccessor IHttpContextAccessor,
             IThongKeRepository thongkeRepository,
             IBaseRepository<ThongKeBenh> benhRepository
            )
        {
            _filesService = filesService;
            _reportService = reportService;
            _httpContext = httpContext;
            _UserManagementService = UserManagementService;
            _IHttpContextAccessor = IHttpContextAccessor;
            _thongKeRepository = thongkeRepository;
            _benhRepository = benhRepository;
        }

        private string ToJavaScriptMilliseconds(DateTime dte)
        {
            return ((dte.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000).ToString();
        }

        public async Task<IActionResult> Index()
        {
            //var khoi = _IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            //var res = _tokenHandle.IsTokenValid(request.Token);

            var userLogIn = await _UserManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);
            var currentCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value;
            var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            bool isMaster = currentRole.Equals(RoleConstant.MASTER);

            if (!userLogIn.Data.IsActivated) { 
                return RedirectToAction("SignOutAction", "Auth"); 
            }

            #region Linhist removed 03/09/22
            //ViewBag.TotalFilesDate = _reportService.TotalFileDate();
            
            //ViewBag.TotalFileMonth = _reportService.TotalFileMonth();

            //ViewBag.TotalFileDieuTriNoiTruDate = _reportService.TotalFileDieuTriNoiTruDate();

            //ViewBag.TotalFileDieuTriNoiTruMonth = _reportService.TotalFileDieuTriNoiTruMonth();

            //ViewBag.TotalFileRaVienDate = _reportService.TotalFileRaVienDate();

            //ViewBag.TotalFileRavienMonth = _reportService.TotalFileRavienMonth();

            //ViewBag.TotalFileTongTienDate = string.Format(CultureInfo.InvariantCulture, "{0:N0}", _reportService.TotalFileTongTienDate() );

            //ViewBag.TotalFileTongTienMonth =  string.Format(CultureInfo.InvariantCulture, "{0:N0}", _reportService.TotalFileTongTienMonth());
            #endregion

            #region Linhist added 28/07/22
            //List<StackeBarModel> stacks = new List<StackeBarModel>();
            DateTime dte = DateTime.Now;
            var thongkes = await _thongKeRepository.GetYearlyStats(dte, currentCSKCB, isMaster);

            var sumTks = thongkes.GroupBy(t => (t.TrongNgay / 100) * 10 + t.TheLoai)
                .Select(g => new ThongKeNgay()
                {
                    TheLoai = (byte?)(g.Key % 10),
                    TongHop = g.Sum(t => t.TongHop),
                    KhamBenh = g.Sum(t => t.KhamBenh),
                    NoiTru = g.Sum(t => t.NoiTru),
                    NgoaiTru = g.Sum(t => t.NgoaiTru),
                    TrongNgay = g.Key / 10,
                });


            int mon = 0;
            string lbl = "";
            double gt1 = 0d, gt2 = 0d, gt3 = 0d, avg = 0d;
            List<DataPoints> lstTongHop = new List<DataPoints>();
            List<DataPoints> lstKhamBenh = new List<DataPoints>();
            List<DataPoints> lstNoiTru = new List<DataPoints>();
            List<DataPoints> lstNgoaiTru = new List<DataPoints>();
            List<DataPoints> lstTongTien = new List<DataPoints>();

            List<DataPoints> lstTienBH = new List<DataPoints>();
            List<DataPoints> lstTienBN = new List<DataPoints>();
            List<DataPoints> lstChiTB = new List<DataPoints>();
            List<DataPoints> lstNoiTB = new List<DataPoints>();
            List<DataPoints> lstNgoaiTB = new List<DataPoints>();

            dte = dte.AddMonths(-11);
            for (int i = 0; i < 12; i++)
            {
                lbl = $"new Date({dte.Year}, {dte.Month - 1}, 1)&nbsp;";
                mon = (dte.Year * 100 + dte.Month);
                var point1 = sumTks.FirstOrDefault(t => t.TheLoai == 2 && t.TrongNgay == mon);
                gt1 = (point1?.TongHop ?? 0d);
                gt2 = (point1?.NoiTru ?? 0d);
                gt3 = (point1?.NgoaiTru ?? 0d);

                lstTongHop.Add(new DataPoints() { Label = lbl, Y = gt1 });
                lstKhamBenh.Add(new DataPoints() { Label = lbl, Y = point1?.KhamBenh ?? 0d });
                lstNoiTru.Add(new DataPoints() { Label = lbl, Y = gt2 });
                lstNgoaiTru.Add(new DataPoints() { Label = lbl, Y = gt3 });

                var point2 = sumTks.FirstOrDefault(t => t.TheLoai == 3 && t.TrongNgay == mon);
                avg = (point2?.TongHop ?? 0d);
                lstTongTien.Add(new DataPoints() { Label = lbl, Y = Math.Round(avg / 1000000, 2) });
                avg = gt1 > 0d ? avg / gt1 : 0d;
                lstChiTB.Add(new DataPoints() { Label = lbl, Y = Math.Round(avg / 1000000, 2) });

                avg = (point2?.NoiTru ?? 0d);
                avg = gt2 > 0d ? avg / gt2 : 0d;
                lstNoiTB.Add(new DataPoints() { Label = lbl, Y = Math.Round(avg / 1000000, 2) });

                avg = (point2?.NgoaiTru ?? 0d);
                avg = gt3 > 0d ? avg / gt3 : 0d;
                lstNgoaiTB.Add(new DataPoints() { Label = lbl, Y = Math.Round(avg / 1000000, 2) });

                var point3 = sumTks.FirstOrDefault(t => t.TheLoai == 4 && t.TrongNgay == mon);
                avg = (point3?.TongHop ?? 0d);
                lstTienBH.Add(new DataPoints() { Label = lbl, Y = Math.Round(avg / 1000000, 2) });

                var point4 = sumTks.FirstOrDefault(t => t.TheLoai == 5 && t.TrongNgay == mon);
                avg = (point4?.TongHop ?? 0d);
                lstTienBN.Add(new DataPoints() { Label = lbl, Y = Math.Round(avg / 1000000, 2) });

                dte = dte.AddMonths(1);
            }

            //Linhist added 03/09/22 doi Model moi cho Homepage
            TotalReports reports = new TotalReports()
            {
                TotalFilesDate = _reportService.TotalFileDate(),
                TotalFilesMonth = _reportService.TotalFileMonth(),
                TotalNoiTruDate = _reportService.TotalFileDieuTriNoiTruDate(),
                TotalNoiTruMonth = _reportService.TotalFileDieuTriNoiTruMonth(),
                TotalRaVienDate = _reportService.TotalFileRaVienDate(),
                TotalRaVienMonth = _reportService.TotalFileRavienMonth(),
                TongTienDate = _reportService.TotalFileTongTienDate(),
                TongTienMonth = _reportService.TotalFileTongTienMonth(),
                ChartPoints = new Dictionary<string, IEnumerable<DataPoints>>(),
                PieCharts = new List<DataPointsPie>()
            };

            reports.ChartPoints.Add("TongHop", lstTongHop);
            reports.ChartPoints.Add("KhamBenh", lstKhamBenh);
            reports.ChartPoints.Add("NoiTru", lstNoiTru);
            reports.ChartPoints.Add("NgoaiTru", lstNgoaiTru);
            reports.ChartPoints.Add("TongTien", lstTongTien);
            reports.ChartPoints.Add("TienBH", lstTienBH);
            reports.ChartPoints.Add("TienBN", lstTienBN);
            reports.ChartPoints.Add("ChiTB", lstChiTB);
            reports.ChartPoints.Add("NoiTB", lstNoiTB);
            reports.ChartPoints.Add("NgoaiTB", lstNgoaiTB);

            IEnumerable<ThongKeBenh> benhs = await _benhRepository.GetListAsync(x => x.MA_CSKCB == (isMaster ? "" : currentCSKCB));
            foreach (var b in benhs)
            {
                reports.PieCharts.Add(new DataPointsPie()
                {
                    name = b.TenBenh,
                    Y = b.SoLuong ?? 0,
                    exploded = (b.SoLuong % 2 == 0),
                });
            }

            return View(reports);
            #endregion

            #region Linhist removed 28/07/22
            //Random rnd = new Random();
            //var data = new List<StackeBarModel>();

            var xml1 = await _reportService.GetAllXML1();
            ////haimaster comment update code
            //var DataPoints1 = xml1.Where(y=>y.MA_LOAI_KCB==1 && y.NGAYLAP != null).GroupBy(x => new { x.NGAYLAP.Value.Month,x.NGAYLAP.Value.Year }).Select(n => new DataPoints
            //{
            //    Label = "Tháng " + n.Key.Month.ToString() +'/' + n.Key.Year.ToString(),
            //    Y = n.Count()
            //}).OrderBy(x => x.Label).ToList();

            //var dataItem1 = new StackeBarModel
            //{
            //    Type = "stackedBar",
            //    ShowInLegend = true,
            //    Name = "Khám bệnh",
            //    AxisYType = "secondary",
            //    Color = "#ffce56",
            //    DataPoints = DataPoints1
            //};
            //data.Add(dataItem1);

            //var DataPoints2 = xml1.Where(y => y.MA_LOAI_KCB == 2 && y.NGAYLAP != null).GroupBy(x => new { x.NGAYLAP.Value.Month, x.NGAYLAP.Value.Year }).Select(n => new DataPoints
            //{
            //    Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
            //    Y = n.Count()
            //}).OrderBy(x => x.Label).ToList();

            //var dataItem2 = new StackeBarModel
            //{
            //    Type = "stackedBar",
            //    ShowInLegend = true,
            //    Name = "Điều trị ngoại trú",
            //    AxisYType = "secondary",
            //    Color = "#36a2eb",
            //    DataPoints = DataPoints2
            //};
            //data.Add(dataItem2);

            //var DataPoints3 = xml1.Where(y => y.MA_LOAI_KCB == 3 && y.NGAYLAP != null).GroupBy(x => new { x.NGAYLAP.Value.Month, x.NGAYLAP.Value.Year }).Select(n => new DataPoints
            //{
            //    Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
            //    Y = n.Count()
            //}).OrderBy(x=>x.Label).ToList();

            //var dataItem3 = new StackeBarModel
            //{
            //    Type = "stackedBar",
            //    ShowInLegend = true,
            //    Name = "Điều trị nội trú",
            //    AxisYType = "secondary",
            //    Color = "#ff6384",
            //    DataPoints = DataPoints3
            //};
            //data.Add(dataItem3);
            #endregion

            #region Linhist removed 03/09/22
            //Report newHomeReport = new Report();

            ////newHomeReport.report1 = data;
            //newHomeReport.report2 = xml1.GroupBy(x => x.MA_BENH ).Select(n => new DataPointsPie
            //{
            //    name = n.Key,
            //    Y = n.Count(),
            //    exploded =false
            //}).OrderByDescending(z=>z.Y).Take(20).ToList();

            //foreach (DataPointsPie dataP in newHomeReport.report2)
            //{
            //    var tenbenh = await _reportService.GetTenBen(dataP.name);
            //    dataP.name = tenbenh?.TenBenh_Original_VN;
            //    if (dataP.Y % 2 == 0) dataP.exploded = true;
            //}
            //return View(newHomeReport);
            #endregion
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
