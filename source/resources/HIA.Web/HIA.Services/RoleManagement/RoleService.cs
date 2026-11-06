using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Services.RoleManagement
{
    public class RoleService : IRoleService
    {
        private readonly IBaseRepository<Role> _roleRepository;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<RoleService> _logger;
        public RoleService(
            IBaseRepository<Role> roleRepository,
            IHttpContextAccessor httpContext,
            ILogger<RoleService> logger
            )
        {
            _roleRepository = roleRepository;
            _httpContext = httpContext;
            _logger = logger;
        }
        public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveRoles()
        {
            var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

                IEnumerable<Role> roles = new List<Role>();
                if (currentRole.Contains(RoleConstant.MASTER))
                {
                    roles = await _roleRepository.GetListAsync(
                            x => !!x.IsActivated && !x.IsDeleted,
                            o => o.OrderBy(o1 => o1.Name));
                }
                else if (currentRole.Contains(RoleConstant.ADMIN))
                {
                    roles = await _roleRepository.GetListAsync(
                           x => !!x.IsActivated && !x.IsDeleted && !x.Id.Equals(RoleConstant.MASTER),
                           o => o.OrderBy(o1 => o1.Name));
                }

                res.Data = roles?.Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id
                }).ToList();
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<IEnumerable<SelectListItem>>(
                    ex, new List<SelectListItem>(), _logger, $"{nameof(RoleService)} - {nameof(RoleService.GetActiveRoles)}");
            }

            return res;
        }
    }
}
