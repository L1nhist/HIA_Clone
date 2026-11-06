using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.ALL)]
    public class DMThuocController : Controller
    {
        private readonly ICategoryByHospitalService<DMThuoc, DMThuocInfoModel, DMThuocModifyModel> _categoryService;
        public DMThuocController(
            ICategoryByHospitalService<DMThuoc, DMThuocInfoModel, DMThuocModifyModel> categoryService
            )
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var ignoreKeysForSearch = new List<string> { nameof(DMThuoc.THID) };
            var res = await _categoryService.GetItems(paniationRequest, ignoreKeysForSearch, new DMThuoc());

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
        public async Task<IActionResult> Create(DMThuocModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.CreateNewItem(request, new List<string> { nameof(DMThuoc.TEN_THUOC), nameof(DMThuoc.MA_THUOC) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMThuocController.Index), Uti.GetControllerName(nameof(DMThuocController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMThuocModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMThuoc.THID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DMThuocModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _categoryService.UpdateItem(request, nameof(DMThuoc.THID), new List<string> { nameof(DMThuoc.TEN_THUOC), nameof(DMThuoc.MA_THUOC) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMThuocController.Index), Uti.GetControllerName(nameof(DMThuocController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMThuocModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMThuoc.THID));
            return View(res.Data);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAction(DMThuocModifyModel request)
        {
            if (request.THID == 0)
                return View(request);

            var res = await _categoryService.DeleteItem(request.THID, nameof(DMThuoc.THID));
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMThuocController.Index), Uti.GetControllerName(nameof(DMThuocController)));
            }

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
            
            res = await _categoryService.UploadCategoryFile(request, new List<string> { nameof(DMThuoc.THID), nameof(DMThuoc.MA_CSKCB) });
            return Json(res);
        }
        [HttpGet]
        public async Task<IActionResult> GetDownloadData()
        {
            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService.DownloadExcelFile(filterString?.ToString() ?? "", new DMThuoc());
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DMThuocController))}-{DateTime.Now.Ticks}.xlsx";

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
    }
}
