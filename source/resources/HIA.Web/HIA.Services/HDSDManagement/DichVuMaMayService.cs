using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using HIA.Core.Languages.DisplayField;
using HIA.Services.Base;
using System.Data.Entity.Infrastructure;
using HIA.Core.Models.HDSD;

namespace HIA.Services.HDSDManagement
{
    public class DichVuMaMayService : IDichVuMaMayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<DichVuMaMay> _dvmmRepos;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<DichVuMaMayService> _logger;

        public DichVuMaMayService(
            IUnitOfWork unitOfWork,
            IBaseRepository<DichVuMaMay> dvmmRepos,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<DichVuMaMayService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _dvmmRepos = dvmmRepos;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ResponseModel<DichVuMaMayModifyModel>> GetItemById(int id)
        {
            var res = new ResponseModel<DichVuMaMayModifyModel>(HttpStatusCode.OK, "", null);

            try
            {
                var item = await _dvmmRepos.GetItem(x => x.DVMMID == id, null);

                var itemData = _mapper.Map<DichVuMaMayModifyModel>(item);
                res.Data = itemData;

                // write log to db
                await _sysHistoryLogService.AddSysLog("Get One DichVuMaMay");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<DichVuMaMayModifyModel>(
                    ex, null, _logger, $"{nameof(DichVuMaMayService)} - {nameof(GetItemById)}");
            }

            return res;
        }

        public async Task<ResponseModel<PaginationResponseModel<DichVuMaMayInfoModel>>> GetItems(PaginationRequestModel request, string maCSKCB)
        {
            var res = new ResponseModel<PaginationResponseModel<DichVuMaMayInfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<DichVuMaMayInfoModel>());
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }

                var queryFilter = new FilterQueryableModel<DichVuMaMay>
                {
                    FilterQueryable = PredicateBuilder.New<DichVuMaMay>(x => x.MA_CSKCB == maCSKCB),
                    OrderQueryable = new List<Func<IOrderedQueryable<DichVuMaMay>, IOrderedQueryable<DichVuMaMay>>>
                    {
                        q => q.OrderBy(x => x.MA_DICH_VU)
                    },
                    IncludeQueryable = null
                };

                if (!string.IsNullOrEmpty(request.SearchKey))
                {
                    queryFilter.FilterQueryable = queryFilter.FilterQueryable.And(x => x.MA_MAY.Contains(request.SearchKey) || x.MA_DICH_VU.Contains(request.SearchKey));
                }

                PaginationResponseModel<DichVuMaMay> items = await _dvmmRepos.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, queryFilter.FilterQueryable, queryFilter.OrderQueryable, queryFilter.IncludeQueryable);
                var itemInfo = _mapper.Map<List<DichVuMaMayInfoModel>>(items?.Items?.ToList());

                res.Data.Items = itemInfo;
                res.Data.TotalItems = items.TotalItems;

                // write log to db
                await _sysHistoryLogService.AddSysLog("Get List DichVuMaMay");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<PaginationResponseModel<DichVuMaMayInfoModel>>(
                    ex, null, _logger, $"{nameof(DichVuMaMayService)} - {nameof(GetItems)}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> Create(DichVuMaMayModifyModel request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }

                if (string.IsNullOrWhiteSpace(request.MA_DICH_VU))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = $"{DisplayFieldResource.MA_DICH_VU} {MessageResource.ERR_400_REQUIRED}";
                    res.Data = false;
                    return res;
                }

                var savedModel = _mapper.Map<DichVuMaMay>(request);
                savedModel.MA_MAY = string.IsNullOrEmpty(savedModel.MA_MAY?.Trim()) ? string.Empty : savedModel.MA_MAY;
                if (await CheckUniqueFieldsExist(savedModel))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = $"{DisplayFieldResource.MA_DICH_VU} {DisplayFieldResource.OR_TEXT} {DisplayFieldResource.MA_MAY} đã tồn tại";
                    res.Data = false;
                    return res;
                }

                await _dvmmRepos.InsertAsync(savedModel);
                await _dvmmRepos.SaveChangesAsync();
                //await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog("Add new DichVuMaMay");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(DichVuMaMayService)} - {nameof(Create)}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> Update(DichVuMaMayModifyModel request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }

                var savedModel = _mapper.Map<DichVuMaMay>(request);
                savedModel.MA_MAY = string.IsNullOrEmpty(savedModel.MA_MAY?.Trim()) ? string.Empty : savedModel.MA_MAY;
                _dvmmRepos.Update(savedModel);
                await _dvmmRepos.SaveChangesAsync();
                //await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog("Update DichVuMaMay");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(DichVuMaMayService)} - {nameof(Update)}");
            }
            return res;
        }

        private async Task<bool> CheckUniqueFieldsExist(DichVuMaMay entityRequest)
        {
            return await _dvmmRepos.AnyAsync(x => x.MA_CSKCB.Equals(entityRequest.MA_CSKCB) && x.MA_DICH_VU.Equals(entityRequest.MA_DICH_VU) && x.MA_MAY.ToLower().Equals(entityRequest.MA_MAY.ToLower()));
        }

        public async Task<ResponseModel<bool>> Delete(int id, string maCSKCB)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }

                #region Validtion
                var item = await _dvmmRepos.DbSet.FindAsync(id, maCSKCB);
                if (item == null)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    res.Data = false;
                    return res;
                }
                #endregion

                _dvmmRepos.Delete(item);
                await _dvmmRepos.SaveChangesAsync();
                //await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog("Delete DichVuMaMay");
            }
            //Linhist fix tạm, fire lỗi mà vẫn xóa đc
            catch (DbUpdateConcurrencyException dex)
            {
                // Inspect the entry causing the issue
                var entry = dex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    // databaseValues is null means the record was already deleted in DB
                    return res;
                }

                // Rethrow if the exception was triggered by a different conflict type
                throw;
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateConcurrencyException) return res;

                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(DichVuMaMayService)} - {nameof(Delete)}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> UploadFile(IFormFile request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                var data = HandleFile130CommonService.UploadExcel<DichVuMaMay>(request, new List<string> { nameof(DichVuMaMay.DVMMID) });
                var savedData = data.Where(x => !string.IsNullOrWhiteSpace(x.MA_DICH_VU)).Select(x => { x.MA_MAY = string.IsNullOrEmpty(x.MA_MAY?.Trim()) ? string.Empty : x.MA_MAY; return x; }).ToList();

                if (!savedData.Any())
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_NO_DATA;
                    res.Data = false;
                    return res;
                }

                await _dvmmRepos.InsertListAsync(savedData.ToList());
                await _dvmmRepos.SaveChangesAsync();

                //await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog("Import all DichVuMaMay");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(DichVuMaMayService)} - {nameof(UploadFile)}");
            }
            return res;
        }
    }
}
