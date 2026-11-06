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

public class ChiDinhDongThoiService : IChiDinhDongThoiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChiDinhDongThoiRepository _chiDinhDTRepository;
    private readonly IDMErrorService130 _dmErrorService;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<ChiDinhDongThoiService> _logger;

    public ChiDinhDongThoiService(
        IUnitOfWork unitOfWork,
        IChiDinhDongThoiRepository chiDinhDTRepository,
        IDMErrorService130 dmErrorService,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        ILogger<ChiDinhDongThoiService> logger
        )
    {
        _unitOfWork = unitOfWork;
        _chiDinhDTRepository = chiDinhDTRepository;
        _dmErrorService = dmErrorService;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;
        _logger = logger;
    }

    private string GetClaim(string claimType)
    {
        var user = _httpContext.HttpContext?.User ?? throw new InvalidOperationException("No HttpContext");
        return user.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Missing claim {claimType}");
    }

    private static int CungLoai(ChiDinhDongThoi rule)
    {
        return rule.CUNG_HO_SO == true ? 1 : rule.CUNG_NGAY_YL == true ? 2 : rule.CUNG_THOI_DIEM == true ? 3 : 0;
    }

    public async Task<ResponseModel<PaginationResponseModel<ChiDinhDongThoiInfoModel>>> GetItems(PaginationRequestModel request)
    {
        var data = new PaginationResponseModel<ChiDinhDongThoiInfoModel>();
        var res = new ResponseModel<PaginationResponseModel<ChiDinhDongThoiInfoModel>>(HttpStatusCode.OK, "", data);

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

            PaginationResponseModel<ChiDinhDongThoi> items = await _chiDinhDTRepository.GetListPaginationRules(request, currentCSKCB, currentRole.Equals(RoleConstant.MASTER));

            if (items.Items.Any())
            {
                var errors130 = await _dmErrorService.GetAllByType(ErrorMessageType.PHACDO_CHIDINH);
                var lstRules = new List<ChiDinhDongThoiInfoModel>();

                foreach (var rule in items.Items)
                {
                    var err = errors130.FirstOrDefault(e => e.ErrorCode == rule.MA_LOI);

                    lstRules.Add(new ChiDinhDongThoiInfoModel
                    {
                        CddtID = rule.CDDTID,
                        Ma_DICH_VU = rule.MA_DICH_VU,
                        Dich_VU_KHAC = rule.DICH_VU_KHAC,
                        Vat_TU_BHYT = rule.VAT_TU_BHYT,
                        Vat_TU_KO_BH = rule.VAT_TU_KO_BH,
                        Chi_DINH = rule.CHI_DINH,
                        Cung_LOAI = CungLoai(rule),
                        Noi_DUNG_LOI = err?.Message,
                        Hieu_LUC = rule.HIEU_LUC,
                        Ma_CSKCB = rule.MA_CSKCB,
                    });
                }

                data.Items = lstRules;
                data.TotalItems = items.TotalItems;
            }

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get ChiDinhDongThoi list by filter");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new PaginationResponseModel<ChiDinhDongThoiInfoModel>(), _logger, $"{nameof(ChiDinhDongThoiService)} - {nameof(ChiDinhDongThoiService.GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<ChiDinhDongThoiModifyModel>> GetItemById(string id)
    {
        var res = new ResponseModel<ChiDinhDongThoiModifyModel>(HttpStatusCode.OK, "", new ChiDinhDongThoiModifyModel());

        try
        {
            int.TryParse(id, out int cID);

            #region Validation
            if (cID < 1)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                return res;
            }
            #endregion

            var item = await _chiDinhDTRepository.GetById(cID)
                ?? throw new InvalidOperationException($"ChiDinhDongThoi not found: {cID}");
            res.Data = new ChiDinhDongThoiModifyModel()
            {
                CddtID = item.CDDTID,
                Chi_DINH = item.CHI_DINH == true,
                Ma_CSKCB = item.MA_CSKCB,
                Ma_DICH_VU = item.MA_DICH_VU,
                Dich_VU_KHAC = item.DICH_VU_KHAC,
                Vat_TU_BHYT = item.VAT_TU_BHYT,
                Vat_TU_KO_BH = item.VAT_TU_KO_BH,
                Cung_LOAI = CungLoai(item),
                Hieu_LUC = item.HIEU_LUC,
                Ma_LOI = item.MA_LOI,
                Noi_DUNG_LOI = string.Empty,
            };
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new ChiDinhDongThoiModifyModel(), _logger, $"{nameof(ChiDinhDongThoiService)} - {nameof(ChiDinhDongThoiService.GetItemById)}");
        }

        return res;
    }

    // Tao hoac cap nhat noi dung loi cua rule. Tra ve false khi khong luu duoc (caller tra BadRequest). Loi bat ngo bi bo qua nhu ban cu.
    private async Task<bool> SaveRuleError(ChiDinhDongThoiModifyModel request, string currentRole, string currentCSKCB)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Noi_DUNG_LOI)) return true;

            DMError130? savedError130;
            if (request.Ma_LOI == null || request.Ma_LOI <= 0)
            {
                int maxErrCode = await _dmErrorService.GetLastErrorCode();
                savedError130 = new DMError130()
                {
                    ErrorCode = maxErrCode + 1,
                    ErrorType = ErrorMessageType.PHACDO_CHIDINH,
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
            _logger.LogWarning(ex, $"{nameof(ChiDinhDongThoiService)} - {nameof(SaveRuleError)}: {ex.Message}");
        }
        return true;
    }

    public async Task<ResponseModel<bool>> CreateNewItem(ChiDinhDongThoiModifyModel request)
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

            var savedModel = new ChiDinhDongThoi()
            {
                CDDTID = request.CddtID,
                CHI_DINH = request.Chi_DINH,
                MA_DICH_VU = request.Ma_DICH_VU,
                DICH_VU_KHAC = request.Dich_VU_KHAC,
                VAT_TU_BHYT = request.Vat_TU_BHYT,
                VAT_TU_KO_BH = request.Vat_TU_KO_BH,
                CUNG_HO_SO = request.Cung_LOAI == 1,
                CUNG_NGAY_YL = request.Cung_LOAI == 2,
                CUNG_THOI_DIEM = request.Cung_LOAI == 3,
                MA_LOI = request.Ma_LOI,
            };

            if (currentRole.Equals(RoleConstant.ADMIN))
            {
                savedModel.MA_CSKCB = currentCSKCB;
            }

            savedModel.HIEU_LUC = false;
            await _chiDinhDTRepository.InsertAsync(savedModel);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("ChiDinhDongThoi created new");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(ChiDinhDongThoiService)} - {nameof(ChiDinhDongThoiService.CreateNewItem)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> UpdateItem(ChiDinhDongThoiModifyModel request)
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

            var item = await _chiDinhDTRepository.GetItem(x => x.CDDTID == request.CddtID);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            #endregion

            item.MA_DICH_VU = request.Ma_DICH_VU;
            item.DICH_VU_KHAC = request.Dich_VU_KHAC;
            item.CHI_DINH = request.Chi_DINH;
            item.VAT_TU_BHYT = request.Vat_TU_BHYT;
            item.VAT_TU_KO_BH = request.Vat_TU_KO_BH;
            item.CUNG_HO_SO = request.Cung_LOAI == 1;
            item.CUNG_NGAY_YL = request.Cung_LOAI == 2;
            item.CUNG_THOI_DIEM = request.Cung_LOAI == 3;
            item.MA_LOI = request.Ma_LOI;

            if (currentRole.Equals(RoleConstant.ADMIN))
            {
                item.MA_CSKCB = currentCSKCB;
            }
            else
            {
                item.MA_CSKCB = request.Ma_CSKCB;
            }

            _chiDinhDTRepository.Update(item);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("ChiDinhDongThoi updated");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(ChiDinhDongThoiService)} - {nameof(ChiDinhDongThoiService.UpdateItem)}");
        }
        return res;
    }

    private static ResponseModel<bool> ValidateModifyItem(ChiDinhDongThoiModifyModel request, string currentRole)
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

            var item = await _chiDinhDTRepository.GetById(id);
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
            _chiDinhDTRepository.Update(item);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("ChiDinhDongThoi soft deleted");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(ChiDinhDongThoiService)} - {nameof(ChiDinhDongThoiService.ActivateItem)}");
        }
        return res;
    }
}
