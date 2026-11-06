using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Repositories.Base;
using HIA.Services.HDSDManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using HIA.Services.Categories;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class HDSDDichVuController : Controller
    {
        private readonly IHDSDDichVuService _hdsdDichVuService;
        //Linhist removed 29/03/25
        //private readonly IBaseRepository<DMError> _dmErrorRepository;
        private readonly IDMErrorService130 _dmError130Repository;

        public HDSDDichVuController(
            IHDSDDichVuService hdsdDichVuService,
            //Linhist removed 29/03/25
            //IBaseRepository<DMError> dmErrorRepository,
            IDMErrorService130 dmError130Repository
        )
        {
            _hdsdDichVuService = hdsdDichVuService;
            //Linhist removed 29/03/25
            //_dmErrorRepository = dmErrorRepository;
            _dmError130Repository = dmError130Repository;
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
            ViewBag.DMErrors = await _dmError130Repository.GetAllByType(ErrorMessageType.HDSD_DICHVU);
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

            ViewBag.DMErrors = await _dmError130Repository.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyHDSDDichVuModel());

            var dmErrors = await _dmError130Repository.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            var res = await _hdsdDichVuService.GetItemById(id);
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
        public async Task<IActionResult> Edit(ModifyHDSDDichVuModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hdsdDichVuService.UpdateItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
            }

            ViewBag.DMErrors = await _dmError130Repository.GetAllByType(ErrorMessageType.HDSD_DICHVU);
            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        public async Task<IActionResult> Activate(int? id, bool? sts)
        {
            var res = await _hdsdDichVuService.ActivateItem(id, sts);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return RedirectToAction(nameof(HDSDDichVuController.Index), Uti.GetControllerName(nameof(HDSDDichVuController)));
        }
    }
}