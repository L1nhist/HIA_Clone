using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Services.SysHistoryLogManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Services.Department
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<DMKhoa> _departmentRepository;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartmentService> _logger;
        public DepartmentService(
            IUnitOfWork unitOfWork,
            IBaseRepository<DMKhoa> departmentRepository,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<DepartmentService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _departmentRepository = departmentRepository;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionList()
        {
            var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
            try
            {
                var items = (await _departmentRepository.GetListAsync(
                        null,
                        o => o.OrderBy(o1 => o1.KhoaName)))?
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
}
