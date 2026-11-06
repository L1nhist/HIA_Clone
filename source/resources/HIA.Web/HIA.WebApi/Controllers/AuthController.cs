using HIA.Core.Models.Base;
using HIA.Core.Models.User;
using HIA.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok("test");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel request)
        {
            var res = new ResponseModel<string>();
            if (!ModelState.IsValid)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = string.Join(" | ", ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage));
                return NotFound(res);
            }

            res = await _authService.SignInGetToken(request);
            return Ok(res);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SignOut")]
        public async Task<IActionResult> SignOutAction()
        {
            await _authService.SignOut();
            return Ok();
        }
    }
}
