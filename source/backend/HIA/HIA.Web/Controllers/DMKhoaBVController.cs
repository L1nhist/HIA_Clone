using HIA.Core.Base;
using HIA.Core.Languages.Message;
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
    public class DMKhoaBVController : Controller
    {
        private readonly IDMKhoaBVService _dMKhoaBVService;
        private readonly ICategoryService<DMKhoaBV, DMKhoaBVInfoBasicModel, DMKhoaBVModifyBasicModel> _categoryService;
        private readonly ICategoryByHospitalService<DMKhoaBV, DMKhoaBVInfoBasicModel, DMKhoaBVModifyBasicModel> _categoryService2;

        public DMKhoaBVController(
            IDMKhoaBVService dMKhoaBVService,
            ICategoryService<DMKhoaBV, DMKhoaBVInfoBasicModel, DMKhoaBVModifyBasicModel> categoryService,
            ICategoryByHospitalService<DMKhoaBV, DMKhoaBVInfoBasicModel, DMKhoaBVModifyBasicModel> categoryService2
            )
        {
            _dMKhoaBVService = dMKhoaBVService;
            _categoryService = categoryService;
            _categoryService2 = categoryService2;
        }

        public async Task<IActionResult> Index()
        {
            var res = await _dMKhoaBVService.GetItems();
            return View(res.Data);
        }

        [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Save(DMKhoaBVModifyModel request)
        {
            if (request.Data.Count == 0)
            {
                return Ok(new ResponseModel<bool>
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = MessageResource.ERR_400_NO_DATA,
                    Data = false
                });
            }
            var res = await _dMKhoaBVService.Save(request);
            return Ok(res);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        public async Task<IActionResult> List()
        {
            ViewBag.AllowImport = await _categoryService2.AllowImport();
            return View();
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var ignoreKeysForSearch = new List<string> { nameof(DMKhoaBV.KBVID) };
            var res = await _dMKhoaBVService.GetDMKhoaBVItems(paniationRequest, ignoreKeysForSearch);

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

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Create(DMKhoaBVModifyBasicModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _dMKhoaBVService.CreateDMKhoaBVNewItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMKhoaBVController.List), Uti.GetControllerName(nameof(DMKhoaBVController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMKhoaBVModifyBasicModel());

            var res = await _categoryService.GetItemById(id, nameof(DMKhoaBV.KBVID));
            return View(res.Data);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Edit(DMKhoaBVModifyBasicModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _dMKhoaBVService.UpdateDMKhoaBVItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMKhoaBVController.List), Uti.GetControllerName(nameof(DMKhoaBVController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMKhoaBVModifyBasicModel());

            var res = await _categoryService.GetItemById(id, nameof(DMKhoaBV.KBVID));
            return View(res.Data);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> DeleteAction(DMKhoaBVModifyBasicModel request)
        {
            if (request.KBVID == 0)
                return View(request);

            var res = await _dMKhoaBVService.DeleteDMKhoaBVItem(request.KBVID);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMKhoaBVController.List), Uti.GetControllerName(nameof(DMKhoaBVController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Bad request", Data = false };
            var request = Request.Form.Files?.FirstOrDefault();
            if (request == null)
            {
                return Json(request);
            }

            res = await _dMKhoaBVService.UploadCategoryFile(request);
            return Json(res);
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
        [HttpGet]
        public async Task<IActionResult> GetDownloadData()
        {
            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService2.DownloadExcelFile(filterString?.ToString() ?? "", new DMKhoaBV());
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DMKhoaBVController))}-{DateTime.Now.Ticks}.xlsx";

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonDefaults.Serialize(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [Authorize(Roles = RoleConstant.ADMIN)]
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

            res = await _categoryService2.ImportFromAPI();
            return Json(res);
        }
    }
}
