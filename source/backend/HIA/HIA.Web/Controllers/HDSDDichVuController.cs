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
    public class HDSDDichVuController : Controller
    {
        private readonly IHDSDDichVuService _hdsdDichVuService;
        private readonly IDMErrorService130 _dmErrorService;

        public HDSDDichVuController(
            IHDSDDichVuService hdsdDichVuService,
            IDMErrorService130 dmErrorService
        )
        {
            _hdsdDichVuService = hdsdDichVuService;
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
            var res = await _hdsdDichVuService.GetItems(paniationRequest);
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
            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ModifyHDSDDichVuModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hdsdDichVuService.CreateNewItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
            }

            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyHDSDDichVuModel());

            var dmErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            var res = await _hdsdDichVuService.GetItemById(id);
            var data = res.Data ?? throw new InvalidOperationException("Khong tim thay HDSD dich vu");
            var err = dmErrors.FirstOrDefault(e => e.ErrorCode == data.Ma_LOI);
            if (err != null)
            {
                data.Ma_LOI = err.ErrorCode;
                data.Noi_DUNG_LOI = err.Message;
            }
            ViewBag.DMErrors = dmErrors;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ModifyHDSDDichVuModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hdsdDichVuService.UpdateItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
            }

            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        public async Task<IActionResult> Activate(int? id, bool? sts)
        {
            var res = await _hdsdDichVuService.ActivateItem(id, sts);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
        }
    }
}
