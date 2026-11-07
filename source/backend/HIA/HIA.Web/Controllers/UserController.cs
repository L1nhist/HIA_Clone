using HIA.Core.Base;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.Services.HospitalManagement;
using HIA.Services.RoleManagement;
using HIA.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.MASTER_and_ADMIN)]
    public class UserController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IRoleService _roleService;
        private readonly IHospitalService _hospitalService;

        private readonly IHttpContextAccessor _IHttpContextAccessor;
        public UserController(
            IUserManagementService userManagementService,
            IRoleService roleService,
            IHospitalService hospitalService,
            IHttpContextAccessor IHttpContextAccessor
            )
        {
            _userManagementService = userManagementService;
            _roleService = roleService;
            _hospitalService = hospitalService;
            _IHttpContextAccessor = IHttpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request)
        {
            var currentUser = (_IHttpContextAccessor.HttpContext ?? throw new InvalidOperationException("Khong co HttpContext")).User;
            var userSid = currentUser.FindFirst(ClaimTypes.Sid)?.Value ?? throw new InvalidOperationException("Thieu claim Sid");
            var userLogIn = await _userManagementService.GetItemById(userSid);
            var userData = userLogIn.Data ?? throw new InvalidOperationException("Khong tim thay nguoi dung");
            if (!userData.IsActivated)
            {
                return RedirectToAction("SignOutAction", "Auth");
            }

            var paniationRequest = new PaginationRequestModel(request);
            var res = await _userManagementService.GetItems(paniationRequest);
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
            ViewBag.RoleList = (await _roleService.GetActiveRoles()).Data;
            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitals()).Data;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewUserModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var newUserUrl = $"{Uti.GetControllerName(nameof(AuthController))}/{nameof(AuthController.ResetPassword)}";
            var res = await _userManagementService.CreateNewItem(request, newUserUrl);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(UserController.Index), Uti.GetControllerName(nameof(UserController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyUserModel());

            ViewBag.RoleList = (await _roleService.GetActiveRoles()).Data;
            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitalsByID()).Data;

            var res = await _userManagementService.GetItemById(id);
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ModifyUserModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _userManagementService.UpdateItem(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(UserController.Index), Uti.GetControllerName(nameof(UserController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyUserModel());

            var res = await _userManagementService.GetItemById(id);
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAction(ModifyUserModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                return View(request);

            var res = await _userManagementService.DeleteItem(request.Id);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(UserController.Index), Uti.GetControllerName(nameof(UserController)));
            }

            ModelState.AddModelError(string.Empty, res.Message ?? string.Empty);
            return View(request);
        }
    }
}
