using HIA.Core.Models.Diagram;
using HIA.Services.Report;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        public ReportController(
             IReportService reportService
            )
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var data = new List<StackeBarModel>();

            var xml1 = await _reportService.GetAllXML1();
            var DataPoints1 = xml1.Where(y => y.MA_LOAI_KCB == 1 && y.NGAYLAP != null).GroupBy(x => new { x.NGAYLAP.Value.Month, x.NGAYLAP.Value.Year }).Select(n => new DataPoints
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

            var DataPoints2 = xml1.Where(y => y.MA_LOAI_KCB == 2 && y.NGAYLAP != null).GroupBy(x => new { x.NGAYLAP.Value.Month, x.NGAYLAP.Value.Year }).Select(n => new DataPoints
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

            var DataPoints3 = xml1.Where(y => y.MA_LOAI_KCB == 3 && y.NGAYLAP != null).GroupBy(x => new { x.NGAYLAP.Value.Month, x.NGAYLAP.Value.Year }).Select(n => new DataPoints
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
            newHomeReport.report2 = xml1.GroupBy(x => x.MA_BENH).Select(n => new DataPointsPie
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
