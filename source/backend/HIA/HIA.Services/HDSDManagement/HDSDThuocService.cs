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

public class HDSDThuocService : IHDSDThuocService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHDSDThuocRepository _hdsdThuocRepository;
    private readonly IDMErrorService130 _dmErrorService;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IMapper _mapper;
    private readonly ILogger<HDSDThuocService> _logger;
    public HDSDThuocService(
        IUnitOfWork unitOfWork,
        IHDSDThuocRepository hdsdThuocRepository,
        IDMErrorService130 dmErrorService,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        IMapper mapper,
        ILogger<HDSDThuocService> logger
        )
    {
        _unitOfWork = unitOfWork;
        _hdsdThuocRepository = hdsdThuocRepository;
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

    public async Task<ResponseModel<PaginationResponseModel<HDSDThuocInfoModel>>> GetItems(PaginationRequestModel request)
    {
        var data = new PaginationResponseModel<HDSDThuocInfoModel>();
        var res = new ResponseModel<PaginationResponseModel<HDSDThuocInfoModel>>(HttpStatusCode.OK, "", data);

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

            PaginationResponseModel<HDSDThuoc> items = await _hdsdThuocRepository.GetListPaginationRules(request, currentCSKCB, currentRole.Equals(RoleConstant.MASTER));

            if (items.Items.Any())
            {
                var errors130 = await _dmErrorService.GetAllByType(ErrorMessageType.HDSD_THUOC);
                var lstRules = new List<HDSDThuocInfoModel>();

                foreach (var rule in items.Items)
                {
                    var err = errors130.FirstOrDefault(e => e.ErrorCode == rule.MA_LOI);

                    lstRules.Add(new HDSDThuocInfoModel
                    {
                        Id = rule.HDSDThuocID,
                        Ma_THUOC = rule.MA_THUOC,
                        Ten_THUOC = rule.TEN_THUOC,
                        Duong_DUNG = rule.DUONG_DUNG,
                        Chi_DINH = rule.CHI_DINH,
                        Do_TUOI = rule.TUOI,
                        Co_THAI = rule.CO_THAI,
                        Cho_CON_BU = rule.CHO_CON_BU,
                        Ma_BENH = rule.MA_BENH,
                        Ten_BENH = rule.TEN_BENH,
                        Thuoc_KHAC = rule.THUOC_KHAC,
                        Dich_VU = rule.DICH_VU,
                        Tien_SU_BENH = rule.TIEN_SU_BENH,
                        Tyle_TT = rule.TYLE_TT,
                        Tre_SO_SINH = rule.TRE_SO_SINH,
                        Thoi_GIAN_TOI_THIEU = rule.THOI_GIAN_TOI_THIEU,
                        Noi_DUNG_LOI = err?.Message,
                        Hieu_LUC = rule.HIEU_LUC,
                        Ma_CSKCB = rule.MA_CSKCB
                    });
                }

                data.Items = lstRules;
                data.TotalItems = items.TotalItems;
            }

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get HDSD Thuoc list by filter");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new PaginationResponseModel<HDSDThuocInfoModel>(), _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<ModifyHDSDThuocModel>> GetItemById(string id)
    {
        var res = new ResponseModel<ModifyHDSDThuocModel>(HttpStatusCode.OK, "", new ModifyHDSDThuocModel());

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

            var item = await _hdsdThuocRepository.GetById(hdsdID);
            res.Data = _mapper.Map<ModifyHDSDThuocModel>(item);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new ModifyHDSDThuocModel(), _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.GetItemById)}");
        }

        return res;
    }

    // Tao hoac cap nhat noi dung loi cua rule. Tra ve false khi khong luu duoc (caller tra BadRequest). Loi bat ngo bi bo qua nhu ban cu.
    private async Task<bool> SaveRuleError(ModifyHDSDThuocModel request, string currentRole, string currentCSKCB)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Noi_DUNG_LOI)) return true;

            DMError130? savedError130;
            if (request.Ma_LOI == null || request.Ma_LOI <= 0)
            {
                savedError130 = new DMError130()
                {
                    ErrorCode = await _dmErrorService.GetLastErrorCode() + 1,
                    ErrorType = ErrorMessageType.HDSD_THUOC,
                    ErrorFile = "XML2",
                    ErrorTag = "MA_THUOC",
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
            _logger.LogWarning(ex, $"{nameof(HDSDThuocService)} - {nameof(SaveRuleError)}: {ex.Message}");
        }
        return true;
    }

    public async Task<ResponseModel<bool>> CreateNewItem(ModifyHDSDThuocModel request)
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

            var savedModel = _mapper.Map<HDSDThuoc>(request);
            if (currentRole.Equals(RoleConstant.ADMIN))
            {
                savedModel.MA_CSKCB = currentCSKCB;
            }

            savedModel.HIEU_LUC = false;
            await _hdsdThuocRepository.InsertAsync(savedModel);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("HDSDThuoc created new");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.CreateNewItem)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> UpdateItem(ModifyHDSDThuocModel request)
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

            var item = await _hdsdThuocRepository.GetItem(x => x.HDSDThuocID == request.Id);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            #endregion

            item.MA_THUOC = request.Ma_THUOC;
            item.TEN_THUOC = request.Ten_THUOC;
            item.DUONG_DUNG = request.Duong_DUNG;
            item.CHI_DINH = request.Chi_DINH;
            item.TUOI = request.Do_TUOI;
            item.CO_THAI = request.Co_THAI;
            item.CHO_CON_BU = request.Cho_CON_BU;
            item.MA_BENH = request.Ma_BENH;
            item.TEN_BENH = request.Ten_BENH;
            item.THUOC_KHAC = request.Thuoc_KHAC;
            item.DICH_VU = request.Dich_VU;
            item.TIEN_SU_BENH = request.Tien_SU_BENH;
            item.TYLE_TT = request.Tyle_TT;
            item.TRE_SO_SINH = request.Tre_SO_SINH;
            item.THOI_GIAN_TOI_THIEU = request.Thoi_GIAN_TOI_THIEU;
            item.MA_LOI = request.Ma_LOI;
            if (currentRole.Equals(RoleConstant.ADMIN))
            {
                item.MA_CSKCB = currentCSKCB;
            }
            else
            {
                item.MA_CSKCB = request.Ma_CSKCB;
            }

            _hdsdThuocRepository.Update(item);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("HDSDThuoc updated");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.UpdateItem)}");
        }
        return res;
    }

    private static ResponseModel<bool> ValidateModifyItem(ModifyHDSDThuocModel request, string currentRole)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        if (!currentRole.Equals(RoleConstant.MASTER) && !currentRole.Equals(RoleConstant.ADMIN))
        {
            res.Code = HttpStatusCode.Forbidden;
            res.Message = MessageResource.ERR_403_FORBIDDEN;
            res.Data = false;
            return res;
        }
        if (string.IsNullOrWhiteSpace(request.Ma_THUOC))
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_400_REQUIRED;
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

            var item = await _hdsdThuocRepository.GetById(id);
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
            _hdsdThuocRepository.Update(item);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("HDSDThuoc soft deleted");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.ActivateItem)}");
        }
        return res;
    }
}
