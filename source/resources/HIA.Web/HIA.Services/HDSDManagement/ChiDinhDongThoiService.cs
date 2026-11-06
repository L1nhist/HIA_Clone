using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.HDSD;
using HIA.Services.Categories;
using HIA.Services.SysHistoryLogManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Services.HDSDManagement
{
    public class ChiDinhDongThoiService : IChiDinhDongThoiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChiDinhDongThoiRepository _chiDinhDTRepository;
        //private readonly IBaseRepository<DMError> _dmErrorRepository;
        private readonly IDMErrorService130 _dmErrorService;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ChiDinhDongThoiService> _logger;

        public ChiDinhDongThoiService(
            IUnitOfWork unitOfWork,
            IChiDinhDongThoiRepository chiDinhDTRepository,
            //IBaseRepository<DMError> dmErrorRepository,
            IDMErrorService130 dmErrorService,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<ChiDinhDongThoiService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _chiDinhDTRepository = chiDinhDTRepository;
            //_dmErrorRepository = dmErrorRepository;
            _dmErrorService = dmErrorService;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseModel<PaginationResponseModel<ChiDinhDongThoiInfoModel>>> GetItems(PaginationRequestModel request)
        {
            var res = new ResponseModel<PaginationResponseModel<ChiDinhDongThoiInfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<ChiDinhDongThoiInfoModel>());

            try
            {
                var currentCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

                #region Validation
                if (!currentRole.Equals(RoleConstant.MASTER) && !currentRole.Equals(RoleConstant.ADMIN))
                {
                    res.Code = HttpStatusCode.Forbidden;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                #endregion

                PaginationResponseModel<ChiDinhDongThoi> items = await _chiDinhDTRepository.GetListPaginationRules(request, currentCSKCB, currentRole.Equals(RoleConstant.MASTER));

                if (items != null && items.Items != null && items.Items.Any())
                {
                    //IEnumerable<DMError> errors = await _dmErrorRepository.GetListAsync();
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
                            Cung_LOAI = rule.CUNG_HO_SO == true ? 1 : rule.CUNG_NGAY_YL == true ? 2 : rule.CUNG_THOI_DIEM == true ? 3 : 0,
                            Noi_DUNG_LOI = err?.Message,
                            Hieu_LUC = rule.HIEU_LUC,
                            Ma_CSKCB = rule.MA_CSKCB,
                        });
                    }

                    res.Data.Items = lstRules;
                    res.Data.TotalItems = items.TotalItems;
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
                int cID = 0;
                int.TryParse(id, out cID);

                #region Validation
                if (cID < 1)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    return res;
                }
                #endregion

                var item = await _chiDinhDTRepository.GetById(cID);
                res.Data = new ChiDinhDongThoiModifyModel()
                {
                    CddtID = item.CDDTID,
                    Chi_DINH = item.CHI_DINH == true,
                    Ma_CSKCB = item.MA_CSKCB,
                    Ma_DICH_VU = item.MA_DICH_VU,
                    Dich_VU_KHAC = item.DICH_VU_KHAC,
                    Vat_TU_BHYT = item.VAT_TU_BHYT,
                    Vat_TU_KO_BH = item.VAT_TU_KO_BH,
                    Cung_LOAI = item.CUNG_HO_SO == true ? 1 : item.CUNG_NGAY_YL == true ? 2 : item.CUNG_THOI_DIEM == true ? 3 : 0,
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

        public async Task<ResponseModel<bool>> CreateNewItem(ChiDinhDongThoiModifyModel request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var currentCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

                #region Validtion
                res = ValidateModifyItem(request, currentRole);
                if (res.Data == false) return res;
                #endregion

                try
                {
                    if (!string.IsNullOrEmpty(request.Noi_DUNG_LOI))
                    {
                        //DMError savedError;
                        DMError130 savedError130;

                        if (request.Ma_LOI == null || request.Ma_LOI <= 0)
                        {
                            //Linhist changed 110926
                            ////Linhist changed 29/11/22
                            ////Lay max error code
                            //int maxErrCode = 0;
                            ////var errs = await _dmErrorRepository.GetListAsync();
                            //var errs = await _dmErrorService.GetListAsync();
                            //if (errs != null)
                            //{
                            //    maxErrCode = errs.Max(e => e.ErrorCode).Value;
                            //}
                            int maxErrCode = await _dmErrorService.GetLastErrorCode();

                            //savedError = new DMError()
                            //{
                            //    ErrorCode = maxErrCode + 1,
                            //    ErrorType = ErrorMessageType.HDSD_DICHVU,
                            //    ErrorAtXMLFile = "XML3",
                            //    ErrorAtXMLTag = "MA_DICH_VU",
                            //    ErrorContent = request.Noi_DUNG_LOI,
                            //    ErrorComment = "",
                            //};
                            savedError130 = new DMError130()
                            {
                                ErrorCode = maxErrCode + 1,
                                ErrorType = ErrorMessageType.PHACDO_CHIDINH,
                                ErrorFile = "XML3",
                                ErrorTag = "MA_DICH_VU",
                                Message = request.Noi_DUNG_LOI,
                                Comment = "",
                            };

                            //Linhist changed 110926
                            ////await _dmErrorRepository.InsertAsync(savedError);
                            //await _dmErrorService.InsertAsync(savedError130);
                            //await _unitOfWork.CommitAsync();

                            ////Linhist removed 29/11/22
                            ////Set errorCode theo max errorCode
                            ////savedError.ErrorCode = savedError.ErrorID;
                            ////_dmErrorRepository.Update(savedError);

                            ////Sau khi them ma loi moi thi active luon cho vien do
                            //if (currentRole.Equals(RoleConstant.ADMIN))
                            //{
                            //    var errAppend = new DMErrorMHTC
                            //    {
                            //        EMErrorCode = savedError130.ErrorCode,
                            //        MA_CSKCB = currentCSKCB,
                            //        EMCreateDate = DateTime.Now,
                            //        EMEnabled = true,
                            //    };
                            //    await _dmErrorMHTCRepository.InsertAsync(errAppend);
                            //}
                            var acted = await _dmErrorService.InsertAndActived(savedError130, currentRole.Equals(RoleConstant.ADMIN) ? currentCSKCB : null);
                            if (!acted)
                            {
                                res.Code = HttpStatusCode.BadRequest;
                                res.Message = MessageResource.ERR_400_NO_DATA;
                                res.Data = false;
                                return res;
                            }
                        }
                        else
                        {
                            //Linhist changed 110926
                            //savedError = await _dmErrorRepository.GetItem(x => x.ErrorCode == request.Ma_LOI);
                            //savedError130 = await _dmErrorService.GetItem(x => x.ErrorCode == request.Ma_LOI);

                            //if (request.Noi_DUNG_LOI != savedError130.Message)
                            //{
                            //    //savedError.ErrorContent = request.Noi_DUNG_LOI;
                            //    //_dmErrorRepository.Update(savedError);

                            //    savedError130.Message = request.Noi_DUNG_LOI;
                            //    _dmErrorService.Update(savedError130);
                            //}
                            savedError130 = await _dmErrorService.UpdateError(request.Ma_LOI, request.Noi_DUNG_LOI);
                            if (savedError130 == null)
                            {
                                res.Code = HttpStatusCode.BadRequest;
                                res.Message = MessageResource.ERR_400_NO_DATA;
                                res.Data = false;
                                return res;
                            }
                        }

                        request.Ma_LOI = savedError130.ErrorCode;
                    }
                }
                catch { }

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
                var currentCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                ChiDinhDongThoi item;

                #region Validtion
                res = ValidateModifyItem(request, currentRole);
                if (res.Data == false) return res;

                try
                {
                    if (!string.IsNullOrEmpty(request.Noi_DUNG_LOI))
                    {
                        //Linhist removed 29/03/25
                        //DMError savedError;
                        DMError130 savedError130;

                        if (request.Ma_LOI == null || request.Ma_LOI <= 0)
                        {
                            //Linhist changed 110926
                            ////Linhist changed 29/11/22
                            ////Lay max error code
                            //int maxErrCode = 0;
                            ////var errs = await _dmErrorRepository.GetListAsync();
                            //var errs = await _dmErrorService.GetListAsync();
                            //if (errs != null)
                            //{
                            //    maxErrCode = errs.Max(e => e.ErrorCode).Value;
                            //}
                            var maxErrCode = await _dmErrorService.GetLastErrorCode();

                            //Linhist removed 29/03/25
                            //savedError = new DMError()
                            //{
                            //    ErrorCode = maxErrCode + 1,
                            //    ErrorType = ErrorMessageType.HDSD_DICHVU,
                            //    ErrorAtXMLFile = "XML3",
                            //    ErrorAtXMLTag = "MA_DICH_VU",
                            //    ErrorContent = request.Noi_DUNG_LOI,
                            //    ErrorComment = "",
                            //};
                            savedError130 = new DMError130()
                            {
                                ErrorCode = maxErrCode + 1,
                                ErrorType = ErrorMessageType.PHACDO_CHIDINH,
                                ErrorFile = "XML3",
                                ErrorTag = "MA_DICH_VU",
                                Message = request.Noi_DUNG_LOI,
                                Comment = "",
                            };

                            //Linhist changed 110926
                            //await _dmErrorRepository.InsertAsync(savedError);
                            //await _dmErrorService.InsertAsync(savedError130);
                            //await _unitOfWork.CommitAsync();


                            ////Linhist removed 29/11/22
                            ////Set errorCode theo max errorCode
                            ////savedError.ErrorCode = savedError.ErrorID;
                            ////_dmErrorRepository.Update(savedError);

                            ////Sau khi them ma loi moi thi active luon cho vien do
                            //if (currentRole.Equals(RoleConstant.ADMIN))
                            //{
                            //    var errAppend = new DMErrorMHTC
                            //    {
                            //        EMErrorCode = savedError130.ErrorCode,
                            //        MA_CSKCB = currentCSKCB,
                            //        EMCreateDate = DateTime.Now,
                            //        EMEnabled = true,
                            //    };
                            //    await _dmErrorMHTCRepository.InsertAsync(errAppend);
                            //}
                            var acted = await _dmErrorService.InsertAndActived(savedError130, currentRole.Equals(RoleConstant.ADMIN) ? currentCSKCB : null);
                            if (!acted)
                            {
                                res.Code = HttpStatusCode.BadRequest;
                                res.Message = MessageResource.ERR_400_NO_DATA;
                                res.Data = false;
                                return res;
                            }
                        }
                        else
                        {
                            //Linhist changed 110926
                            //savedError = await _dmErrorRepository.GetItem(x => x.ErrorCode == request.Ma_LOI);
                            //savedError130 = await _dmErrorService.GetItem(x => x.ErrorCode == request.Ma_LOI);

                            //if (request.Noi_DUNG_LOI != savedError130.Message)
                            //{
                            //    //savedError.ErrorContent = request.Noi_DUNG_LOI;
                            //    //_dmErrorRepository.Update(savedError);

                            //    savedError130.Message = request.Noi_DUNG_LOI;
                            //    _dmErrorService.Update(savedError130);
                            //}
                            savedError130 = await _dmErrorService.UpdateError(request.Ma_LOI, request.Noi_DUNG_LOI);
                            if (savedError130 == null)
                            {
                                res.Code = HttpStatusCode.BadRequest;
                                res.Message = MessageResource.ERR_400_NO_DATA;
                                res.Data = false;
                                return res;
                            }
                        }

                        request.Ma_LOI = savedError130.ErrorCode;
                    }
                }
                catch { }

                item = await _chiDinhDTRepository.GetItem(x => x.CDDTID == request.CddtID);
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

        private ResponseModel<bool> ValidateModifyItem(ChiDinhDongThoiModifyModel request, string currentRole)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            if (!currentRole.Equals(RoleConstant.MASTER) && !currentRole.Equals(RoleConstant.ADMIN))
            {
                res.Code = HttpStatusCode.Forbidden;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                res.Data = false;
                return res;
            }
            //if (string.IsNullOrWhiteSpace(request.Ma_DICH_VU))
            //{
            //    res.Code = HttpStatusCode.BadRequest;
            //    res.Message = MessageResource.ERR_400_REQUIRED;
            //    res.Data = false;
            //    return res;
            //}
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
                var currentCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

                ChiDinhDongThoi item;
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

                item = await _chiDinhDTRepository.GetById(id);
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
                    ex, false, _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.ActivateItem)}");
            }
            return res;
        }
    }
}
