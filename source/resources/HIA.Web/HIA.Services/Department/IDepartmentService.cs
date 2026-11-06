using HIA.Core.Models.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Services.Department
{
    public interface IDepartmentService
    {
        Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionList();
    }
}
