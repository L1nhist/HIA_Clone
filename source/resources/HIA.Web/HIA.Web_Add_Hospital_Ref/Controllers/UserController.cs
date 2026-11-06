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
            if (_IHttpContextAccessor.HttpContext.User != null)
            {
                var userLogIn = await _userManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

                if (!userLogIn.Data.IsActivated)
                {
                    return RedirectToAction("SignOutAction", "Auth");
                }
            }
            var paniationRequest = new PaginationRequestModel(request);
            var res = await _userManagementService.GetItems(paniationRequest);
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

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(new ModifyUserModel());

            ViewBag.RoleList = (await _roleService.GetActiveRoles()).Data;
            ViewBag.HospitalList = (await _hospitalService.GetActiveHospitals()).Data;

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

            ModelState.AddModelError(string.Empty, res.Message);
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

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }
    }
}
