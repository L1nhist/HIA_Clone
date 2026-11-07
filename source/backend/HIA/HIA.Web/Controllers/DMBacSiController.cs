using HIA.Core.Base;
using HIA.Core.Languages.Message;
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
    public class DMBacSiController : Controller
    {
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly ICategoryByHospitalService<DMBacSi, DMBacSiInfoModel, DMBacSiModifyModel> _categoryService;
        public DMBacSiController(
            ISysHistoryLogService sysHistoryLogService,
            ICategoryByHospitalService<DMBacSi, DMBacSiInfoModel, DMBacSiModifyModel> categoryService
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
            var ignoreKeysForSearch = new List<string> { nameof(DMBacSi.BSID) };
            var res = await _categoryService.GetItems(paniationRequest, ignoreKeysForSearch, new DMBacSi());

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
        public async Task<IActionResult> Create(DMBacSiModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                ModelState.AddModelError(string.Empty, MessageResource.ERR_403_FORBIDDEN);
                return View(request);
            }

            request.Ma_CSKCB = currentMaCSKCB;
            request.Chuc_DANH = Uti.Get_ChucDanh(request.Chuc_DANH_NN);
            request.Vi_TRI = Uti.Get_ViTri(request.Ma_VI_TRI);
            var res = await _categoryService.CreateNewItem(request, new List<string> { nameof(DMBacSi.HO_TEN), nameof(DMBacSi.MA_CCHN) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMBacSiController.Index), Uti.GetControllerName(nameof(DMBacSiController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User create DMBacsi #{request.BSID} - {request.Ho_TEN} ({request.Ma_CCHN})");

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMBacSiModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMBacSi.BSID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DMBacSiModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(currentMaCSKCB) || string.IsNullOrWhiteSpace(request.Ma_CSKCB) || !currentMaCSKCB.Equals(request.Ma_CSKCB))
            {
                ModelState.AddModelError(string.Empty, MessageResource.ERR_403_FORBIDDEN);
                return View(request);
            }

            request.Chuc_DANH = Uti.Get_ChucDanh(request.Chuc_DANH_NN);
            request.Vi_TRI = Uti.Get_ViTri(request.Ma_VI_TRI);
            var res = await _categoryService.UpdateItem(request, nameof(DMBacSi.BSID), new List<string> { nameof(DMBacSi.HO_TEN), nameof(DMBacSi.MA_CCHN) });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMBacSiController.Index), Uti.GetControllerName(nameof(DMBacSiController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User update DMBacSi #{request.BSID} - {request.Ho_TEN} {request.Ma_CCHN}");

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new DMBacSiModifyModel());

            var res = await _categoryService.GetItemById(id, nameof(DMBacSi.BSID));
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAction(DMBacSiModifyModel request)
        {
            if (request.BSID == 0)
                return View(request);

            var res = await _categoryService.DeleteItem(request.BSID, nameof(DMBacSi.BSID));
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(DMBacSiController.Index), Uti.GetControllerName(nameof(DMBacSiController)));
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog($"User delete DMBacSi #{request.BSID}");

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var res = new ResponseModel<bool> { Code = HttpStatusCode.BadRequest, Message = "Bad request", Data = false };
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                ModelState.AddModelError(string.Empty, MessageResource.ERR_403_FORBIDDEN);
                return View(res);
            }

            var request = Request.Form.Files?.FirstOrDefault();
            if (request == null)
            {
                return Json(res);
            }

            //Linhist added 28/07/23 - ghi log vao DB khi upload DM moi
            await _sysHistoryLogService.AddSysLog("User upload DMBacSi");
            res = await _categoryService.UploadCategoryFile(request, new List<string> { nameof(DMBacSi.BSID) });
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData()
        {
            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _categoryService.DownloadExcelFile(filterString?.ToString() ?? "", new DMBacSi());
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"{Uti.GetControllerName(nameof(DMBacSiController))}-{DateTime.Now.Ticks}.xlsx";

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
