using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Catalogs;
using HIA.Services.Categories;
using HIA.Services.SysHistoryLogManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.ALL)]
    public class DMDichVuController : Controller
    {
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly ICategoryByHospitalService<DMDichVu, DMDichVuInfoModel, DMDichVuModifyModel> _categoryService;

        public DMDichVuController(
            ISysHistoryLogService sysHistoryLogService,
            ICategoryByHospitalService<DMDichVu, DMDichVuInfoModel, DMDichVuModifyModel> categoryService
            )
        {
            _sysHistoryLogService = sysHistoryLogService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.AllowImport = await _categoryService.AllowImport();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var ignoreKeysForSearch = new List<string> { nameof(DMDichVu.DVID) };
            var res = await _categoryService.GetItems(paniationRequest, ignoreKeysForSearch, new DMDichVu());

            var filterDownloadModel = new FilterDownloadModel
            {
                SearchKey = paniationRequest.SearchKey,
                OrderByColumn = paniationRequest.OrderByColumn,
                OrderType = paniationRequest.OrderType,
                IgnoreKeysForSearch = ignoreKeysForSearch
            };
            TempData["FILTER_DOWNLOAD_MODEL"] = JsonDefaults.Serialize(filterDownloadModel);

            if (res.Code == HttpStatusCode.OK && res.Data != null)
            {
                return Ok(DataTableResponse.Create(paniationRequest.Draw, res.Data.TotalItems, res.Data.Items));
            }

            return Ok(res.Message);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DMDichVuModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.CreateNewItem(request, new List<string> { nameof(DMDichVu.TEN_DICH_VU), nameof(DMDichVu.MA_DICH_VU) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMDichVuController.Index), Uti.GetControllerName(nameof(DMDichVuController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User create DMDichVu #{request.DVID} - {request.MA_DICH_VU} {request.TEN_DICH_VU}");

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMDichVuModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMDichVu.DVID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DMDichVuModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.UpdateItem(request, nameof(DMDichVu.DVID), new List<string> { nameof(DMDichVu.TEN_DICH_VU), nameof(DMDichVu.MA_DICH_VU) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMDichVuController.Index), Uti.GetControllerName(nameof(DMDichVuController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User update DMDichVu #{request.DVID} - {request.MA_DICH_VU} {request.TEN_DICH_VU}");

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMDichVuModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMDichVu.DVID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAction(DMDichVuModifyModel request)
        {
            if (request.DVID == 0)
                return View(request);

            var res = await _categoryService.DeleteItem(request.DVID, nameof(DMDichVu.DVID));
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMDichVuController.Index), Uti.GetControllerName(nameof(DMDichVuController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User delete DMDichVu #{request.DVID} - {request.MA_DICH_VU} {request.TEN_DICH_VU}");

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Bad request", Data = false };
            var request = Request.Form.Files?.FirstOrDefault();
            if (request == null)
            {
                return Json(res);
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog("User upload DMDichVu");

            res = await _categoryService.UploadCategoryFile(request, new List<string> { nameof(DMDichVu.DVID) });
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData()
        {
            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService.DownloadExcelFile(filterString?.ToString() ?? "", new DMDichVu());
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DMDichVuController))}-{DateTime.Now.Ticks}.xlsx";

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonDefaults.Serialize(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public IActionResult Download(string fileGuid, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileGuid) || TempData[fileGuid] is not string json)
            {
                return new EmptyResult();
            }

            var data = JsonDefaults.Deserialize<byte[]>(json);
            if (data == null)
            {
                return new EmptyResult();
            }

            string contentType = $"application/{FileTypeConstant.XLSX_TYPE}";
            return File(data, contentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Import()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Không có thông tin", Data = false };
            await _sysHistoryLogService.AddSysLog("User import DMThuoc from API BH");

            res = await _categoryService.ImportFromAPI();
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> Replace()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Bad request", Data = false };
            var request = Request.Form.Files?.FirstOrDefault();
            if (request == null)
            {
                return Json(request);
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog("User replace DMDichVu");

            res = await _categoryService.ReplaceCategoryFile(request, new List<string> { nameof(DMDichVu.DVID) });
            return Json(res);
        }
    }
}
