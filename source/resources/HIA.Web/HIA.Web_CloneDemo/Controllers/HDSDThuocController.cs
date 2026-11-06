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

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class HDSDThuocController : Controller
    {
        private readonly IHDSDThuocService _hdsdThuocService;
        //Linhist removed 29/03/25
        //private readonly IBaseRepository<DMError> _dmErrorRepository;
        private readonly IBaseRepository<DMError130> _dmErrorRepository;

        public HDSDThuocController(
            IHDSDThuocService hdsdThuocService,
            //Linhist removed 29/03/25
            //IBaseRepository<DMError> dmErrorRepository
            IBaseRepository<DMError130> dmErrorRepository
        )
        {
            _hdsdThuocService = hdsdThuocService;
            //Linhist removed 29/03/25
            //_dmErrorRepository = dmErrorRepository;
            _dmErrorRepository = dmErrorRepository;
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
            ViewBag.DMErrors = await _dmErrorRepository.GetListAsync(x => x.ErrorType == ErrorMessageType.HDSD_THUOC);
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

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyHDSDThuocModel());

            ViewBag.DMErrors = await _dmErrorRepository.GetListAsync(x => x.ErrorType == ErrorMessageType.HDSD_THUOC);
            var res = await _hdsdThuocService.GetItemById(id);
            return View(res.Data);
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

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        public async Task<IActionResult> Activate(int? id, bool? sts)
        {
            var res = await _hdsdThuocService.ActivateItem(id, sts);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HDSDThuocController.Index), Uti.GetControllerName(nameof(HDSDThuocController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return RedirectToAction(nameof(HDSDThuocController.Index), Uti.GetControllerName(nameof(HDSDThuocController)));
        }
    }
}
