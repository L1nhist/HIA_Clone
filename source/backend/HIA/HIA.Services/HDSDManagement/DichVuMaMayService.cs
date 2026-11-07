using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Services.Base;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace HIA.Services.HDSDManagement;

public class DichVuMaMayService : IDichVuMaMayService
{
    private readonly IBaseRepository<DichVuMaMay> _dvmmRepos;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<DichVuMaMayService> _logger;

    public DichVuMaMayService(
        IBaseRepository<DichVuMaMay> dvmmRepos,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        ILogger<DichVuMaMayService> logger
        )
    {
        _dvmmRepos = dvmmRepos;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;
        _logger = logger;
    }

    private string GetClaim(string claimType)
    {
        var user = _httpContext.HttpContext?.User ?? throw new InvalidOperationException("No HttpContext");
        return user.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Missing claim {claimType}");
    }

    public async Task<ResponseModel<DichVuMaMayModifyModel>> GetItemById(int id)
    {
        var res = new ResponseModel<DichVuMaMayModifyModel>(HttpStatusCode.OK, "", null);

        try
        {
            var item = await _dvmmRepos.GetItem(x => x.DVMMID == id, null);

            var itemData = item == null ? null : new DichVuMaMayModifyModel();
            itemData?.MapFrom(item);
            res.Data = itemData;

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get One DichVuMaMay");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<DichVuMaMayModifyModel?>(
                ex, null, _logger, $"{nameof(DichVuMaMayService)} - {nameof(GetItemById)}")!;
        }

        return res;
    }

    public async Task<ResponseModel<PaginationResponseModel<DichVuMaMayInfoModel>>> GetItems(PaginationRequestModel request, string maCSKCB)
    {
        var data = new PaginationResponseModel<DichVuMaMayInfoModel>();
        var res = new ResponseModel<PaginationResponseModel<DichVuMaMayInfoModel>>(HttpStatusCode.OK, "", data);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            Expression<Func<DichVuMaMay, bool>> filter = PredicateBuilder.New<DichVuMaMay>(x => x.MA_CSKCB == maCSKCB);
            var order = new List<Func<IOrderedQueryable<DichVuMaMay>, IOrderedQueryable<DichVuMaMay>>>
            {
                q => q.OrderBy(x => x.MA_DICH_VU)
            };

            if (!string.IsNullOrEmpty(request.SearchKey))
            {
                var key = request.SearchKey;
                filter = filter.And(x => (x.MA_MAY ?? "").Contains(key) || (x.MA_DICH_VU ?? "").Contains(key));
            }

            PaginationResponseModel<DichVuMaMay> items = await _dvmmRepos.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, order, null);

            data.Items = items.Items.Select(x =>
            {
                var m = new DichVuMaMayInfoModel();
                m.MapFrom(x);
                return m;
            }).ToList();
            data.TotalItems = items.TotalItems;

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get List DichVuMaMay");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new PaginationResponseModel<DichVuMaMayInfoModel>(), _logger, $"{nameof(DichVuMaMayService)} - {nameof(GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> Create(DichVuMaMayModifyModel request)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

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

            var savedModel = request.MapTo() ?? throw new InvalidOperationException("Cannot map DichVuMaMayModifyModel to DichVuMaMay");
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
            var roleId = GetClaim(ClaimTypes.Role);
            if (!RoleConstant.MASTER_and_ADMIN.Contains(roleId))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            var savedModel = request.MapTo() ?? throw new InvalidOperationException("Cannot map DichVuMaMayModifyModel to DichVuMaMay");
            savedModel.MA_MAY = string.IsNullOrEmpty(savedModel.MA_MAY?.Trim()) ? string.Empty : savedModel.MA_MAY;
            _dvmmRepos.Update(savedModel);
            await _dvmmRepos.SaveChangesAsync();

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
        var maCSKCB = entityRequest.MA_CSKCB;
        var maDichVu = entityRequest.MA_DICH_VU;
        var maMay = (entityRequest.MA_MAY ?? "").ToLower();
        return await _dvmmRepos.AnyAsync(x => x.MA_CSKCB == maCSKCB && x.MA_DICH_VU == maDichVu && (x.MA_MAY ?? "").ToLower() == maMay);
    }

    public async Task<ResponseModel<bool>> Delete(int id, string maCSKCB)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
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

            // write log to db
            await _sysHistoryLogService.AddSysLog("Delete DichVuMaMay");
        }
        //Linhist fix tạm, fire lỗi mà vẫn xóa đc
        catch (DbUpdateConcurrencyException)
        {
            // Ban ghi da bi xoa trong DB: coi nhu thanh cong
            return res;
        }
        catch (Exception ex)
        {
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
            var roleId = GetClaim(ClaimTypes.Role);
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

            await _dvmmRepos.InsertListAsync(savedData);
            await _dvmmRepos.SaveChangesAsync();

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
