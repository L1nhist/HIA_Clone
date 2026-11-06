using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Data.Catalogs;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HIA.Services.Department;

public class DepartmentService : IDepartmentService
{
    private readonly IBaseRepository<DMKhoa> _departmentRepository;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IBaseRepository<DMKhoa> departmentRepository,
        ILogger<DepartmentService> logger
        )
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionList()
    {
        var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
        try
        {
            var items = (await _departmentRepository.GetListAsync(
                    null,
                    o => o.OrderBy(o1 => o1.KhoaName)))
                    .Select(s => new SelectListItem
                    {
                        Text = s.KhoaName,
                        Value = s.KhoaCode
                    }).ToList();

            res.Data = items;
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<IEnumerable<SelectListItem>>(
                ex, new SelectList(new List<SelectListItem>()), _logger, $"{nameof(DepartmentService)} - {nameof(DepartmentService.GetSelectionList)}");
        }

        return res;
    }
}
