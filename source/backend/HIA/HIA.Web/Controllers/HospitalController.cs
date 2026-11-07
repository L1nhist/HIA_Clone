using HIA.Core.Base;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Hospital;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.Categories;
using HIA.Services.HospitalManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class HospitalController : Controller
    {
        private readonly IHospitalService _hospitalService;
        private readonly IDMErrorService130 _dMErrorService;

        public HospitalController(
            IHospitalService hospitalService,
            IDMErrorService130 dMErrorService
            )
        {
            _hospitalService = hospitalService;
            _dMErrorService = dMErrorService;
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
                var page = res.Data ?? throw new InvalidOperationException("Khong co du lieu phan trang");
                return Ok(DataTableResponse.Create(paniationRequest.Draw, page.TotalItems, page.Items));
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
                if (request.PredefError)
                {
                    await _dMErrorService.SaveDefaultErrors(request.MA_CSKCB ?? string.Empty);
                }

                return RedirectToAction(nameof(HospitalController.Index), Uti.GetControllerName(nameof(HospitalController)));
            }

            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitalsByID()).Data;
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitalsByID()).Data;

            if (string.IsNullOrWhiteSpace(id))
                return View("Create");

            var res = await _hospitalService.GetItemById(id);
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
                await _hospitalService.UpdateCategoryPrice(request.MA_CSKCB ?? string.Empty, request.MaxFilePerDate);
                return RedirectToAction(nameof(HospitalController.Index), Uti.GetControllerName(nameof(HospitalController)));
            }

            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitalsByID()).Data;
            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> EditSpec()
        {
            var res = await _hospitalService.GetCurrent();
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> EditSpec(ModifyHospitalModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _hospitalService.UpdateItem(request);
            ModelState.AddModelError(string.Empty, res.Code == HttpStatusCode.OK ? "Cập nhật thành công" : res.Message ?? string.Empty);
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

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }
    }
}
