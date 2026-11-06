using HIA.Core.Models.Base;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HIA.Services.RoleManagement;

public interface IRoleService
{
    Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveRoles();
}
