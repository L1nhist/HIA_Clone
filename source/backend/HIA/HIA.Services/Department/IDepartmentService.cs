using HIA.Core.Models.Base;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HIA.Services.Department;

public interface IDepartmentService
{
    Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionList();
}
