using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Services.Categories;
using HIA.Services.SysHistoryLogManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.ALL)]
    public class DMVTYTController : Controller
    {
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly ICategoryByHospitalService<DMVTYT, DMVTYTInfoModel, DMVTYTModifyModel> _categoryService;
        public DMVTYTController(
            ISysHistoryLogService sysHistoryLogService,
            ICategoryByHospitalService<DMVTYT, DMVTYTInfoModel, DMVTYTModifyModel> categoryService
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
            var ignoreKeysForSearch = new List<string> { nameof(DMVTYT.VTID) };
            var res = await _categoryService.GetItems(paniationRequest, ignoreKeysForSearch, new DMVTYT());

            var filterDownloadModel = new FilterDownloadModel
            {
                SearchKey = paniationRequest.SearchKey,
                OrderByColumn = paniationRequest.OrderByColumn,
                OrderType = paniationRequest.OrderType,
                IgnoreKeysForSearch = ignoreKeysForSearch
            };
            TempData["FILTER_DOWNLOAD_MODEL"] = JsonConvert.SerializeObject(filterDownloadModel);

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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DMVTYTModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.CreateNewItem(request, new List<string> { nameof(DMVTYT.TEN_VAT_TU), nameof(DMVTYT.MA_VAT_TU) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMVTYTController.Index), Uti.GetControllerName(nameof(DMVTYTController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User create DMVatTu #{request.VTID} - {request.MA_VAT_TU} {request.TEN_VAT_TU}");

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMVTYTModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMVTYT.VTID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DMVTYTModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.UpdateItem(request, nameof(DMVTYT.VTID), new List<string> { nameof(DMVTYT.TEN_VAT_TU), nameof(DMVTYT.MA_VAT_TU) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMVTYTController.Index), Uti.GetControllerName(nameof(DMVTYTController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User update DMVatTu #{request.VTID} - {request.MA_VAT_TU} {request.TEN_VAT_TU}");

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMVTYTModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMVTYT.VTID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAction(DMVTYTModifyModel request)
        {
            if (request.VTID == 0)
                return View(request);

            var res = await _categoryService.DeleteItem(request.VTID, nameof(DMVTYT.VTID));
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMVTYTController.Index), Uti.GetControllerName(nameof(DMVTYTController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User delete DMVatTu #{request.VTID} - {request.MA_VAT_TU} {request.TEN_VAT_TU}");

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Bad request", Data = false};
            var request = Request.Form.Files?.FirstOrDefault();
            if (request == null)
            {
                return Json(request);
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog("User upload DMVatTu");

            res = await _categoryService.UploadCategoryFile(request, new List<string> { nameof(DMVTYT.VTID) });
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData()
        {
            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService.DownloadExcelFile(filterString?.ToString() ?? "", new DMVTYT());
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DMVTYTController))}-{DateTime.Now.Ticks}.xlsx";

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

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

        [HttpPost]
        public async Task<IActionResult> Import()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Không có thông tin", Data = false };
            await _sysHistoryLogService.AddSysLog("User import DMThuoc from API BH");

            res = await _categoryService.ImportFromAPI();
            return Json(res);
        }
    }
}
