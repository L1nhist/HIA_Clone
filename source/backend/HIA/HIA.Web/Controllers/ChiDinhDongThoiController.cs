using HIA.Core.Base;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.Categories;
using HIA.Services.HDSDManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class ChiDinhDongThoiController : Controller
    {
        private readonly IChiDinhDongThoiService _chiDinhDTService;
        //Linhist removed 29/03/25
        private readonly IDMErrorService130 _dmErrorService;

        public ChiDinhDongThoiController(
            IChiDinhDongThoiService chiDinhDTService,
            //Linhist removed 29/03/25
            IDMErrorService130 dmErrorService
        )
        {
            _chiDinhDTService = chiDinhDTService;
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
            var res = await _chiDinhDTService.GetItems(paniationRequest);
            if (res.Code == HttpStatusCode.OK && res.Data != null)
            {
                return Ok(new
                {
                    draw = paniationRequest.Draw,
                    recordsFiltered = res.Data.TotalItems,
                    recordsTotal = res.Data.TotalItems,
                    data = res.Data.Items
                });
            }

            return Ok(res.Message);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.PHACDO_CHIDINH);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ChiDinhDongThoiModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _chiDinhDTService.CreateNewItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(ChiDinhDongThoiController.Index), Uti.GetControllerName(nameof(ChiDinhDongThoiController)));
            }

            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.PHACDO_CHIDINH);
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return await Create();

            var dmErrors = await _dmErrorService.GetAllByType(ErrorMessageType.PHACDO_CHIDINH);
            var res = await _chiDinhDTService.GetItemById(id);
            if (res.Data == null)
                return NotFound();

            var err = dmErrors.FirstOrDefault(e => e.ErrorCode == res.Data.Ma_LOI);
            if (err != null)
            {
                res.Data.Ma_LOI = err.ErrorCode;
                res.Data.Noi_DUNG_LOI = err.Message;
            }
            ViewBag.DMErrors = dmErrors;
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ChiDinhDongThoiModifyModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _chiDinhDTService.UpdateItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(ChiDinhDongThoiController.Index), Uti.GetControllerName(nameof(ChiDinhDongThoiController)));
            }

            ViewBag.DMErrors = await _dmErrorService.GetAllByType(ErrorMessageType.PHACDO_CHIDINH);
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        public async Task<IActionResult> Activate(int? id, bool? sts)
        {
            var res = await _chiDinhDTService.ActivateItem(id, sts);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(ChiDinhDongThoiController.Index), Uti.GetControllerName(nameof(ChiDinhDongThoiController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return RedirectToAction(nameof(ChiDinhDongThoiController.Index), Uti.GetControllerName(nameof(ChiDinhDongThoiController)));
        }
    }
}
