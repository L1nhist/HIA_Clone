using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.User;
using HIA.Services.Auth;
using HIA.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserManagementService _userManagementService;
        public AuthController(
            IAuthService authService,
            IUserManagementService userManagementService
            )
        {
            _authService = authService;
            _userManagementService = userManagementService;
        }

       [HttpGet]
       [AllowAnonymous]
       public IActionResult HealthCheck()
        {
            return Ok("test");
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInModel request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var res = await _authService.SignIn(request);
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HomeController.Index), Uti.GetControllerName(nameof(HomeController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }
        [HttpGet]
        public IActionResult InvalidToken()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return View(nameof(AuthController.SignIn));

            var res = await _authService.CheckToken(token);
            if (res.Data == null) return View(nameof(AuthController.InvalidToken));

            return View(new ResetPassModel { Token = token });
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPassModel request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            if (!ModelState.IsValid)
            {
                res.Message = string.Join(" | ", ModelState.Values
                                         .SelectMany(v => v.Errors)
                                         .Select(e => e.ErrorMessage));
                res.Code = HttpStatusCode.BadRequest;
                return Ok(res);
            }
            var newUserUrl = $"{Uti.GetControllerName(nameof(AuthController))}/{nameof(AuthController.ResetPassword)}";
            res = await _authService.ForgotPass(request.EmailForgot, newUserUrl);
            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Token)) return View(nameof(AuthController.SignIn));

            if (!ModelState.IsValid)
                return View(request);

            var checkToken = await _authService.CheckToken(request.Token);
            if (checkToken.Data == null) return View(nameof(AuthController.InvalidToken));

            var resUpdatePass = await _userManagementService.ResetPassword(request.NewPassword, checkToken.Data);
            if (resUpdatePass.Code != HttpStatusCode.OK)
            {
                ModelState.AddModelError(string.Empty, resUpdatePass.Message);
                return View(request);
            }

            var res = await _authService.SignIn(new SignInModel { 
                Email = checkToken.Data.Email,
                Password = request.NewPassword
            });
            if (res.Code == HttpStatusCode.OK)
            {
                return RedirectToAction(nameof(HomeController.Index), Uti.GetControllerName(nameof(HomeController)));
            }

            ModelState.AddModelError(string.Empty, res.Message);
            return View(request);
        }
        [Authorize]
        public async Task<IActionResult> SignOutAction()
        {
            var res = await _authService.SignOut();
            if (res.Data)
            {
                return View(nameof(UserController.SignIn));
            }
            return RedirectToAction(nameof(HomeController.Error), Uti.GetControllerName(nameof(HomeController)));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword request)
        {
            var res = new ResponseModel<dynamic>(HttpStatusCode.OK, "", null);
            if (!ModelState.IsValid)
            {
                res.Data = ModelState.Where(ms => ms.Value.Errors.Any())
                    .Select(x => new { x.Key, x.Value.Errors.FirstOrDefault().ErrorMessage });
                res.Code = HttpStatusCode.BadRequest;
                return Ok(res);
            }

            var resChange = await _userManagementService.ChangePassword(request.CurrentPassword, request.NewPassword);
            if (resChange.Code != HttpStatusCode.OK)
            {
                res.Data = new List<object> { new { Key = "Common", ErrorMessage = resChange.Message } };
                res.Code = resChange.Code;
                return Ok(res);
            }

            return Ok(res);
        }
    }
}
