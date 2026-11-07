using HIA.Core.Base;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.Categories;
using HIA.Services.HDSDManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class HDSDThuocController : Controller
    {
        private readonly IHDSDThuocService _hdsdThuocService;
        private readonly IDMErrorService130 _dmErrorService;

        public HDSDThuocController(
            IHDSDThuocService hdsdThuocService,
            IDMErrorService130 dmErrorService
        )
        {
            _hdsdThuocService = hdsdThuocService;
            _dmErrorService = dmErrorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var res = await _hdsdThuocService.GetItems(paniationRequest);
            if (res.Code == HttpStatusCode.OK)
            {
                var page = res.Data ?? throw new InvalidOperationException("Khong co du lieu phan trang");
                return Ok(new
                {
                    draw = paniationRequest.Draw,
                    recordsFiltered = page.TotalItems,
                    recordsTotal = page.TotalItems,
                    data = page.Items
                });
            }

            return Ok(res.Message);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_THUOC);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ModifyHDSDThuocModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hdsdThuocService.CreateNewItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDThuocController.Index), Uti.GetControllerName(nameof(HDSDThuocController)));
            }

            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_THUOC);
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyHDSDThuocModel());

            var dmErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_THUOC);
            var res = await _hdsdThuocService.GetItemById(id);
            var data = res.Data ?? throw new InvalidOperationException("Khong tim thay HDSD thuoc");
            var err = dmErrors.FirstOrDefault(x => x.ErrorCode == data.Ma_LOI);
            if (err != null)
            {
                data.Ma_LOI = err.ErrorCode;
                data.Noi_DUNG_LOI = err.Message;
            }
            ViewBag.DMErrors = dmErrors;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ModifyHDSDThuocModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hdsdThuocService.UpdateItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDThuocController.Index), Uti.GetControllerName(nameof(HDSDThuocController)));
            }

            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_THUOC);
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        public async Task<IActionResult> Activate(int? id, bool? sts)
        {
            var res = await _hdsdThuocService.ActivateItem(id, sts);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDThuocController.Index), Uti.GetControllerName(nameof(HDSDThuocController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return RedirectToAction(nameof(HDSDThuocController.Index), Uti.GetControllerName(nameof(HDSDThuocController)));
        }
    }
}
