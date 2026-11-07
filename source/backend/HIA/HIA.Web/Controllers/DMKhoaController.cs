using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Catalogs;
using HIA.Services.Categories;
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
    public class DMKhoaController : Controller
    {
        private readonly ICategoryService<DMKhoa, DMKhoaInfoModel, DMKhoaModifyModel> _categoryService;
        private readonly IDMKhoaBVService _dMKhoaBVService;
        public DMKhoaController(
            ICategoryService<DMKhoa, DMKhoaInfoModel, DMKhoaModifyModel> categoryService,
            IDMKhoaBVService dMKhoaBVService
            )
        {
            _categoryService = categoryService;
            _dMKhoaBVService = dMKhoaBVService;
        }

        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var ignoreKeysForSearch = new List<string> { nameof(DMKhoa.KhoaID) };
            var res = await _categoryService.GetItems(paniationRequest, ignoreKeysForSearch, new DMKhoa());

            var filterdownloadmodel = new FilterDownloadModel
            {
                SearchKey = paniationRequest.SearchKey,
                OrderByColumn = paniationRequest.OrderByColumn,
                OrderType = paniationRequest.OrderType,
                IgnoreKeysForSearch = ignoreKeysForSearch
            };
            TempData["FILTER_DOWNLOAD_MODEL"] = JsonDefaults.Serialize(filterdownloadmodel);

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
        public async Task<IActionResult> Create(DMKhoaModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.CreateNewItem(request, new List<string> { nameof(DMKhoa.KhoaCode), nameof(DMKhoa.KhoaName) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMKhoaBVController.Index), Uti.GetControllerName(nameof(DMKhoaBVController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMKhoaModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMKhoa.KhoaID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DMKhoaModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.UpdateItem(request, nameof(DMKhoa.KhoaID), new List<string> { nameof(DMKhoa.KhoaCode), nameof(DMKhoa.KhoaName) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMKhoaBVController.Index), Uti.GetControllerName(nameof(DMKhoaBVController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMKhoaModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMKhoa.KhoaID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAction(DMKhoaModifyModel request)
        {
            if (request.KhoaID == 0)
                return View(request);

            var res = await _dMKhoaBVService.DeleteItem(request.KhoaID);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMKhoaBVController.Index), Uti.GetControllerName(nameof(DMKhoaBVController)));
            }

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
                return Json(request);
            }

            res = await _categoryService.UploadCategoryFile(request, new List<string> { nameof(DMKhoa.KhoaID) });
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData()
        {
            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService.DownloadExcelFile(filterString?.ToString() ?? "", new DMKhoa());
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DMKhoaController))}-{DateTime.Now.Ticks}.xlsx";

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
    }
}
