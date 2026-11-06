using HIA.Core.Models.Diagram;
using HIA.Services.Categories;
using HIA.Services.Email;
using HIA.Services.Files;
using HIA.Services.HospitalManagement;
using HIA.Services.Report;
using HIA.Services.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Nancy.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IDMKhoaBVService _dMKhoaBVService;
        private readonly IEmailService _emailService;
        private readonly IDMErrorService130 _dMErrorService;

        //Linhist added 16/06/22
        private readonly IHospitalService _hospitalService;

        private readonly IUserManagementService _UserManagementService;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        public ReportController(
             IReportService reportService,
            IDMKhoaBVService dMKhoaBVService,
            IEmailService emailService,
            IDMErrorService130 dMErrorService,
            IHospitalService hospitalService,
             IUserManagementService UserManagementService,
             IHttpContextAccessor IHttpContextAccessor
            )
        {
            _reportService = reportService;
            _hospitalService = hospitalService;
            _dMKhoaBVService = dMKhoaBVService;
            _emailService = emailService;
            _dMErrorService = dMErrorService;
            _UserManagementService = UserManagementService;
            _IHttpContextAccessor = IHttpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            //Linhist removed 20/03/22 to push up perfomance
            //var quatityErrors = (await _filesService.GetXml1ErrorItems()).Data;
            //ViewBag.CreatedDateList = (await _filesService.GetSelectionCreatedDateList()).Data;
            var userLogIn = await _UserManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            if (!userLogIn.Data.IsActivated)
            {
                return RedirectToAction("SignOutAction", "Auth");
            }

            ViewBag.DepartmentList = (await _dMKhoaBVService.GetSelectionList()).Data;
            var errList = (await _dMErrorService.GetSelectionErrorListByHospital()).Data;

            //Linhist removed 20/03/22 to push up perfomance
            //foreach (var item in quatityErrors)
            //{
            //    var errItem = errList.FirstOrDefault(x => x.Value.Equals(item.XeErrorCode.ToString()));
            //    if (errItem == null) continue;

            //    errItem.Quantity = item.Total;
            //}
            //errList = errList.OrderByDescending(x => x.Quantity);

            ViewBag.ErrorList = errList;

            //Linhist added 16/06/22
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;

            var data = new List<StackeBarModel>();

            var xml1 = await _reportService.GetAllXML1();
            var DataPoints1 = xml1.Where(y => y.MA_LOAI_KCB == "01" && y.NGAY_LAP != null).GroupBy(x => new { x.NGAY_LAP.Value.Month, x.NGAY_LAP.Value.Year }).Select(n => new DataPoints
            {
                Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
                Y = n.Count()
            }).OrderBy(x => x.Label).ToList();

            var dataItem1 = new StackeBarModel
            {
                Type = "stackedBar",
                ShowInLegend = true,
                Name = "Khám bệnh",
                AxisYType = "secondary",
                Color = "#ffce56",
                DataPoints = DataPoints1
            };
            data.Add(dataItem1);

            var DataPoints2 = xml1.Where(y => y.MA_LOAI_KCB == "02" && y.NGAY_LAP != null).GroupBy(x => new { x.NGAY_LAP.Value.Month, x.NGAY_LAP.Value.Year }).Select(n => new DataPoints
            {
                Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
                Y = n.Count()
            }).OrderBy(x => x.Label).ToList();

            var dataItem2 = new StackeBarModel
            {
                Type = "stackedBar",
                ShowInLegend = true,
                Name = "Điều trị ngoại trú",
                AxisYType = "secondary",
                Color = "#36a2eb",
                DataPoints = DataPoints2
            };
            data.Add(dataItem2);

            var DataPoints3 = xml1.Where(y => y.MA_LOAI_KCB == "03" && y.NGAY_LAP != null).GroupBy(x => new { x.NGAY_LAP.Value.Month, x.NGAY_LAP.Value.Year }).Select(n => new DataPoints
            {
                Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
                Y = n.Count()
            }).OrderBy(x => x.Label).ToList();

            var dataItem3 = new StackeBarModel
            {
                Type = "stackedBar",
                ShowInLegend = true,
                Name = "Điều trị nội trú",
                AxisYType = "secondary",
                Color = "#ff6384",
                DataPoints = DataPoints3
            };
            data.Add(dataItem3);

            Report newHomeReport = new Report();

            newHomeReport.report1 = data;
            newHomeReport.report2 = xml1.GroupBy(x => x.MA_BENH_CHINH).Select(n => new DataPointsPie
            {
                name = n.Key,
                Y = n.Count(),
                exploded = false
            }).OrderByDescending(z => z.Y).Take(20).ToList();

            foreach (DataPointsPie dataP in newHomeReport.report2)
            {
                var tenbenh = await _reportService.GetTenBen(dataP.name);
                dataP.name = tenbenh?.TenBenh_Original_VN;
                if (dataP.Y % 2 == 0) dataP.exploded = true;
            }
            return View(newHomeReport);
        }

        public async Task<IActionResult> Index1()
        {
            //Linhist removed 20/03/22 to push up perfomance
            //var quatityErrors = (await _filesService.GetXml1ErrorItems()).Data;
            //ViewBag.CreatedDateList = (await _filesService.GetSelectionCreatedDateList()).Data;
            var userLogIn = await _UserManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            if (!userLogIn.Data.IsActivated)
            {
                return RedirectToAction("SignOutAction", "Auth");
            }

            ViewBag.DepartmentList = (await _dMKhoaBVService.GetSelectionList()).Data;
            var errList = (await _dMErrorService.GetSelectionErrorListByHospital()).Data;

            //Linhist removed 20/03/22 to push up perfomance
            //foreach (var item in quatityErrors)
            //{
            //    var errItem = errList.FirstOrDefault(x => x.Value.Equals(item.XeErrorCode.ToString()));
            //    if (errItem == null) continue;

            //    errItem.Quantity = item.Total;
            //}
            //errList = errList.OrderByDescending(x => x.Quantity);

            ViewBag.ErrorList = errList;

            //Linhist added 16/06/22
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;

            var data = new List<StackeBarModel>();

            var xml1 = await _reportService.GetAllXML1();
            var DataPoints1 = xml1.Where(y => y.MA_LOAI_KCB == "01" && y.NGAY_LAP != null).GroupBy(x => new { x.NGAY_LAP.Value.Month, x.NGAY_LAP.Value.Year }).Select(n => new DataPoints
            {
                Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
                Y = n.Count()
            }).OrderBy(x => x.Label).ToList();

            var dataItem1 = new StackeBarModel
            {
                Type = "stackedBar",
                ShowInLegend = true,
                Name = "Khám bệnh",
                AxisYType = "secondary",
                Color = "#ffce56",
                DataPoints = DataPoints1
            };
            data.Add(dataItem1);

            var DataPoints2 = xml1.Where(y => y.MA_LOAI_KCB == "02" && y.NGAY_LAP != null).GroupBy(x => new { x.NGAY_LAP.Value.Month, x.NGAY_LAP.Value.Year }).Select(n => new DataPoints
            {
                Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
                Y = n.Count()
            }).OrderBy(x => x.Label).ToList();

            var dataItem2 = new StackeBarModel
            {
                Type = "stackedBar",
                ShowInLegend = true,
                Name = "Điều trị ngoại trú",
                AxisYType = "secondary",
                Color = "#36a2eb",
                DataPoints = DataPoints2
            };
            data.Add(dataItem2);

            var DataPoints3 = xml1.Where(y => y.MA_LOAI_KCB == "03" && y.NGAY_LAP != null).GroupBy(x => new { x.NGAY_LAP.Value.Month, x.NGAY_LAP.Value.Year }).Select(n => new DataPoints
            {
                Label = "Tháng " + n.Key.Month.ToString() + '/' + n.Key.Year.ToString(),
                Y = n.Count()
            }).OrderBy(x => x.Label).ToList();

            var dataItem3 = new StackeBarModel
            {
                Type = "stackedBar",
                ShowInLegend = true,
                Name = "Điều trị nội trú",
                AxisYType = "secondary",
                Color = "#ff6384",
                DataPoints = DataPoints3
            };
            data.Add(dataItem3);

            Report newHomeReport = new Report();

            newHomeReport.report1 = data;
            newHomeReport.report2 = xml1.GroupBy(x => x.MA_BENH_CHINH).Select(n => new DataPointsPie
            {
                name = n.Key,
                Y = n.Count(),
                exploded = false
            }).OrderByDescending(z => z.Y).Take(20).ToList();

            foreach (DataPointsPie dataP in newHomeReport.report2)
            {
                var tenbenh = await _reportService.GetTenBen(dataP.name);
                dataP.name = tenbenh?.TenBenh_Original_VN;
                if (dataP.Y % 2 == 0) dataP.exploded = true;
            }
            return View(newHomeReport);
        }
    }
}
