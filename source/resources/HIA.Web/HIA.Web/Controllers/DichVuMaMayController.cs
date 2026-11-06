using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Services.Categories;
using HIA.Services.HDSDManagement;
using HIA.Services.HospitalManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize]
    public class DichVuMaMayController : Controller
    {
        private readonly IDichVuMaMayService _dvmmService;
        private readonly IHospitalService _hospitalService;
        private readonly ICategoryByHospitalService<DichVuMaMay, DichVuMaMayInfoModel, DichVuMaMayModifyModel> _categoryService;

        public DichVuMaMayController(
            IDichVuMaMayService dvmmService,
            IHospitalService hospitalService,
            ICategoryByHospitalService<DichVuMaMay, DichVuMaMayInfoModel, DichVuMaMayModifyModel> categoryService
            )
        {
            _categoryService = categoryService;
            _hospitalService = hospitalService;
            _dvmmService = dvmmService;
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        public async Task<IActionResult> Index()
        {
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            return View();
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request, string maCSKCB, string search)
        {
            var paniationRequest = new PaginationRequestModel(request);
            maCSKCB ??= HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            paniationRequest.SearchKey = search;
            var res = await _dvmmService.GetItems(paniationRequest, maCSKCB);

            if (res.Code == HttpStatusCode.OK)
            {
                return Ok(new
                {
                    draw = paniationRequest.Draw,
                    recordsFiltered = res.Data.TotalItems,
                    recordsTotal = res.Data.TotalItems,
                    data = res.Data.Items,
                });
            }

            return Ok(res.Message);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            return View();
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Create(DichVuMaMayModifyModel request)
        {
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            if (!ModelState.IsValid)
                return View(request);

            var res = await _dvmmService.Create(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DichVuMaMayController.Index), Uti.GetControllerName(nameof(DichVuMaMayController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var itmId))
                return View(new DichVuMaMayModifyModel());

            var res = await _dvmmService.GetItemById(itmId);
            return View(res.Data);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Edit(DichVuMaMayModifyModel request)
        {
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            if (!ModelState.IsValid)
                return View(request);

            var res = await _dvmmService.Update(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DichVuMaMayController.Index), Uti.GetControllerName(nameof(DichVuMaMayController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id, string maCSKCB)
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Không tìm thấy dữ liệu", Data = false };
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var itmId))
                return Json(res);

            res = await _dvmmService.Delete(itmId, maCSKCB);
            return Json(res);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Bad request", Data = false };
            var request = Request.Form.Files?.FirstOrDefault();
            if (request == null)
            {
                return Json(request);
            }

            res = await _dvmmService.UploadFile(request);
            return Json(res);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpGet]
        public async Task<IActionResult> GetDownloadData(string maCSKCB, string search)
        {
            if (string.IsNullOrEmpty(maCSKCB))
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService.DownloadExcelFile(maCSKCB, search);
            if (res?.Data == null)
            {
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DichVuMaMayController))}-{DateTime.Now.Ticks}.xlsx";

            var guid = Guid.NewGuid().ToString();
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpGet]
        public IActionResult Download(string fileGuid, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileGuid) || TempData[fileGuid] == null)
            {
                return new EmptyResult();
            }

            var data = JsonConvert.DeserializeObject<byte[]>(TempData[fileGuid].ToString());
            string contentType = $"application/{FileTypeConstant.XLSX_TYPE}";
            return File(data, contentType, fileName);
        }
    }
}
