using HIA.Core.Base;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.DatabaseModel.Entites;
using HIA.Services.HospitalManagement;
using HIA.Services.RoleManagement;
using HIA.Services.UserManagement;
using HIA.Services.HosoManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize(Roles = RoleConstant.ALL)]
    public class HoSoController : Controller
    {
        private readonly IHoSoManagementService _hosoService;
        public HoSoController(
            IHoSoManagementService hosoService
            )
        {
            _hosoService = hosoService;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            //ViewBag.RoleList = (await _roleService.GetActiveRoles()).Data;
            //ViewBag.HospitalList = (await _hospitalService.GetActiveHospitals()).Data;
            await _hosoService.ValidateFiles();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(List<XML1> request)
        {
            //if (!ModelState.IsValid)
            //    return View(request);

            //var res = await _hosoService.CreateNewHoSo(request);
            //if (res.Code == HttpStatusCode.OK)
            //{
            //    return RedirectToAction(nameof(UserController.Index), Uti.GetControllerName(nameof(UserController)));
            //}
            //return View(res);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            //if (string.IsNullOrWhiteSpace(userId))
            //    return View(new ModifyUserModel());

            //ViewBag.RoleList = (await _roleService.GetActiveRoles()).Data;
            //ViewBag.HospitalList = (await _hospitalService.GetActiveHospitals()).Data;

            //var res = await _userManagementService.GetItemById(userId);
            //return View(res.Data);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ModifyUserModel request)
        {
            //if (!ModelState.IsValid)
            //    return View(request);

            //var res = await _userManagementService.UpdateItem(request);
            //if (res.Code == HttpStatusCode.OK)
            //{
            //    return RedirectToAction(nameof(UserController.Index), Uti.GetControllerName(nameof(UserController)));
            //}
            //return View(res);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string userId)
        {
            //if (string.IsNullOrWhiteSpace(userId))
            //    return View(new ModifyUserModel());

            //var res = await _userManagementService.GetItemById(userId);
            //return View(res.Data);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DeletePost(string userId)
        {
            //if (string.IsNullOrWhiteSpace(userId))
            //    return View(new ModifyUserModel());

            //var res = await _userManagementService.DeleteItem(userId);
            //if (res.Code == HttpStatusCode.OK)
            //{
            //    return RedirectToAction(nameof(UserController.Index), Uti.GetControllerName(nameof(UserController)));
            //}
            //return View(res);
            return View();
        }
    }
}
