using HIA.Core.Base;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Hospital;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.HospitalManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class HospitalController : Controller
    {
        private readonly IHospitalService _hospitalService;
        public HospitalController(
            IHospitalService hospitalService
            )
        {
            _hospitalService = hospitalService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var res = await _hospitalService.GetItems(paniationRequest);
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
            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitalsByID()).Data;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ModifyHospitalModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hospitalService.CreateNewItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HospitalController.Index), Uti.GetControllerName(nameof(HospitalController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyHospitalModel());

            var res = await _hospitalService.GetItemById(id);
            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitalsByID()).Data;
            return View(res.Data);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ModifyHospitalModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hospitalService.UpdateItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HospitalController.Index), Uti.GetControllerName(nameof(HospitalController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyHospitalModel());

            var res = await _hospitalService.GetItemById(id);
            return View(res.Data);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAction(ModifyHospitalModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                return View(request);

            var res = await _hospitalService.DeleteItem(request.Id);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HospitalController.Index), Uti.GetControllerName(nameof(HospitalController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }
    }
}
