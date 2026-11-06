using HIA.Core.Models.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.RoleManagement
{
    public interface IRoleService
    {
        Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveRoles();
    }
}
