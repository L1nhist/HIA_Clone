using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Repositories.HDSD;
using HIA.Services.Categories;
using HIA.Services.SysHistoryLogManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

namespace HIA.Services.HDSDManagement;

public class HDSDDichVuService : IHDSDDichVuService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHDSDDichVuRepository _hdsdDichVuRepository;
    private readonly IDMErrorService130 _dmErrorService;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IMapper _mapper;
    private readonly ILogger<HDSDDichVuService> _logger;
    public HDSDDichVuService(
        IUnitOfWork unitOfWork,
        IHDSDDichVuRepository hdsdDichVuRepository,
        IDMErrorService130 dmErrorService,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        IMapper mapper,
        ILogger<HDSDDichVuService> logger
        )
    {
        _unitOfWork = unitOfWork;
        _hdsdDichVuRepository = hdsdDichVuRepository;
        _dmErrorService = dmErrorService;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;
        _mapper = mapper;
        _logger = logger;
    }

    private string GetClaim(string claimType)
    {
        var user = _httpContext.HttpContext?.User ?? throw new InvalidOperationException("No HttpContext");
        return user.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Missing claim {claimType}");
    }

    public async Task<ResponseModel<PaginationResponseModel<HDSDDichVuInfoModel>>> GetItems(PaginationRequestModel request)
    {
        var data = new PaginationResponseModel<HDSDDichVuInfoModel>();
        var res = new ResponseModel<PaginationResponseModel<HDSDDichVuInfoModel>>(HttpStatusCode.OK, "", data);

        try
        {
            var currentCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            var currentRole = GetClaim(ClaimTypes.Role);

            #region Validation
            if (!currentRole.Equals(RoleConstant.MASTER) && !currentRole.Equals(RoleConstant.ADMIN))
            {
                res.Code = HttpStatusCode.Forbidden;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            #endregion

            PaginationResponseModel<HDSDDichVu> items = await _hdsdDichVuRepository.GetListPaginationRules(request, currentCSKCB, currentRole.Equals(RoleConstant.MASTER));

            if (items.Items.Any())
            {
                var errors130 = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_DICHVU);
                var lstRules = new List<HDSDDichVuInfoModel>();

                foreach (var rule in items.Items)
                {
                    var err = errors130.FirstOrDefault(e => e.ErrorCode?.ToString() == rule.MA_LOI);

                    lstRules.Add(new HDSDDichVuInfoModel
                    {
                        Id = rule.HDSDDVID,
                        Ma_DICH_VU = rule.MA_DICH_VU,
                        Ten_DICH_VU = rule.TEN_DICH_VU,
                        Chi_DINH = rule.CHI_DINH,
                        Do_TUOI = rule.TUOI,
                        Co_THAI = rule.CO_THAI,
                        Cho_CON_BU = rule.CHO_CON_BU,
                        Ma_BENH = rule.MA_BENH,
                        Thuoc_KHAC = rule.THUOC_KHAC,
                        Ma_NHOM = rule.MA_NHOM,
                        Dich_VU_KHAC = rule.DICH_VU_KHAC,
                        Tien_SU_BENH = rule.TIEN_SU_BENH,
                        Tyle_TT = rule.TYLE_TT,
                        Thoi_GIAN_TOI_THIEU = rule.THOI_GIAN_TOI_THIEU,
                        Thoi_GIAN_THUC_HIEN = rule.THOI_GIAN_THUC_HIEN,
                        Noi_DUNG_LOI = err?.Message,
                        Hieu_LUC = rule.HIEU_LUC,
                        Ma_CSKCB = rule.MA_CSKCB,
                        Ma_LOAI_KCB = rule.MA_LOAI_KCB,
                    });
                }

                data.Items = lstRules;
                data.TotalItems = items.TotalItems;
            }

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get HDSD DichVu list by filter");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new PaginationResponseModel<HDSDDichVuInfoModel>(), _logger, $"{nameof(HDSDDichVuService)} - {nameof(HDSDDichVuService.GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<ModifyHDSDDichVuModel>> GetItemById(string id)
    {
        var res = new ResponseModel<ModifyHDSDDichVuModel>(HttpStatusCode.OK, "", new ModifyHDSDDichVuModel());

        try
        {
            int.TryParse(id, out int hdsdID);

            #region Validation
            if (hdsdID < 1)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                return res;
            }
            #endregion

            var item = await _hdsdDichVuRepository.GetById(hdsdID);
            res.Data = _mapper.Map<ModifyHDSDDichVuModel>(item);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new ModifyHDSDDichVuModel(), _logger, $"{nameof(HDSDDichVuService)} - {nameof(HDSDDichVuService.GetItemById)}");
        }

        return res;
    }

    // Tao hoac cap nhat noi dung loi cua rule. Tra ve false khi khong luu duoc (caller tra BadRequest). Loi bat ngo bi bo qua nhu ban cu.
    private async Task<bool> SaveRuleError(ModifyHDSDDichVuModel request, string currentRole, string currentCSKCB)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Noi_DUNG_LOI)) return true;

            DMError130? savedError130;
            if (request.Ma_LOI == null || request.Ma_LOI <= 0)
            {
                var max = await _dmErrorService.GetLastErrorCode();
                savedError130 = new DMError130()
                {
                    ErrorCode = max + 1,
                    ErrorType = ErrorMessageType.HDSD_DICHVU,
                    ErrorFile = "XML3",
                    ErrorTag = "MA_DICH_VU",
                    Message = request.Noi_DUNG_LOI,
                    Comment = "",
                };

                //Sau khi them ma loi moi thi active luon cho vien do
                var acted = await _dmErrorService.InsertAndActived(savedError130, currentRole.Equals(RoleConstant.ADMIN) ? currentCSKCB ?? string.Empty : string.Empty);
                if (!acted) return false;
            }
            else
            {
                savedError130 = await _dmErrorService.UpdateError(request.Ma_LOI, request.Noi_DUNG_LOI);
                if (savedError130 == null) return false;
            }

            request.Ma_LOI = savedError130.ErrorCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"{nameof(HDSDDichVuService)} - {nameof(SaveRuleError)}: {ex.Message}");
        }
        return true;
    }

    public async Task<ResponseModel<bool>> CreateNewItem(ModifyHDSDDichVuModel request)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var currentCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            var currentRole = GetClaim(ClaimTypes.Role);

            #region Validtion
            res = ValidateModifyItem(request, currentRole);
            if (res.Data == false) return res;
            #endregion

            if (!await SaveRuleError(request, currentRole, currentCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_NO_DATA;
                res.Data = false;
                return res;
            }

            var savedModel = _mapper.Map<HDSDDichVu>(request);
            if (currentRole.Equals(RoleConstant.ADMIN))
            {
                savedModel.MA_CSKCB = currentCSKCB;
            }

            savedModel.HIEU_LUC = false;
            await _hdsdDichVuRepository.InsertAsync(savedModel);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("HDSDDichVu created new");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(HDSDDichVuService)} - {nameof(HDSDDichVuService.CreateNewItem)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> UpdateItem(ModifyHDSDDichVuModel request)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var currentCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            var currentRole = GetClaim(ClaimTypes.Role);

            #region Validtion
            res = ValidateModifyItem(request, currentRole);
            if (res.Data == false) return res;

            if (!await SaveRuleError(request, currentRole, currentCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_NO_DATA;
                res.Data = false;
                return res;
            }

            var item = await _hdsdDichVuRepository.GetItem(x => x.HDSDDVID == request.Id);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            #endregion

            item.MA_DICH_VU = request.Ma_DICH_VU;
            item.TEN_DICH_VU = request.Ten_DICH_VU;
            item.CHI_DINH = request.Chi_DINH;
            item.TUOI = request.Do_TUOI;
            item.CO_THAI = request.Co_THAI;
            item.CHO_CON_BU = request.Cho_CON_BU;
            item.MA_BENH = request.Ma_BENH;
            item.THUOC_KHAC = request.Thuoc_KHAC;
            item.MA_NHOM = request.Ma_NHOM;
            item.DICH_VU_KHAC = request.Dich_VU_KHAC;
            item.TIEN_SU_BENH = request.Tien_SU_BENH;
            item.TYLE_TT = request.Tyle_TT;
            item.THOI_GIAN_TOI_THIEU = request.Thoi_GIAN_TOI_THIEU;
            item.THOI_GIAN_THUC_HIEN = request.Thoi_GIAN_THUC_HIEN;
            item.MA_LOI = request.Ma_LOI?.ToString();
            item.MA_LOAI_KCB = request.Ma_LOAI_KCB;
            if (currentRole.Equals(RoleConstant.ADMIN))
            {
                item.MA_CSKCB = currentCSKCB;
            }
            else
            {
                item.MA_CSKCB = request.Ma_CSKCB;
            }

            _hdsdDichVuRepository.Update(item);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("HDSDDichVu updated");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(HDSDDichVuService)} - {nameof(HDSDDichVuService.UpdateItem)}");
        }
        return res;
    }

    private static ResponseModel<bool> ValidateModifyItem(ModifyHDSDDichVuModel request, string currentRole)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        if (!currentRole.Equals(RoleConstant.MASTER) && !currentRole.Equals(RoleConstant.ADMIN))
        {
            res.Code = HttpStatusCode.Forbidden;
            res.Message = MessageResource.ERR_403_FORBIDDEN;
            res.Data = false;
            return res;
        }
        if ((request.Ma_LOI == null || request.Ma_LOI <= 0) && string.IsNullOrWhiteSpace(request.Noi_DUNG_LOI))
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_400_REQUIRED;
            res.Data = false;
            return res;
        }
        return res;
    }

    public async Task<ResponseModel<bool>> ActivateItem(int? id, bool? hieuLuc)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var currentCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            var currentRole = GetClaim(ClaimTypes.Role);

            #region Validtion
            if (!currentRole.Equals(RoleConstant.MASTER) && !currentRole.Equals(RoleConstant.ADMIN))
            {
                res.Code = HttpStatusCode.Forbidden;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                res.Data = false;
                return res;
            }

            if (id == null || id < 1)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }

            var item = await _hdsdDichVuRepository.GetById(id);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }

            //Chi cho phep nhung rule tao boi chinh MA_CSKCB
            if (currentRole.Equals(RoleConstant.ADMIN) && item.MA_CSKCB != currentCSKCB)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                res.Data = false;
                return res;
            }
            #endregion

            item.HIEU_LUC = (hieuLuc == true);
            _hdsdDichVuRepository.Update(item);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("HDSDDichVu soft deleted");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(HDSDDichVuService)} - {nameof(HDSDDichVuService.ActivateItem)}");
        }
        return res;
    }
}
