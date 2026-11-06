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
    public class HDSDDichVuService : IHDSDDichVuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHDSDDichVuRepository _hdsdDichVuRepository;
        //private readonly IBaseRepository<DMError> _dmErrorRepository;
        private readonly IDMErrorService130 _dmErrorService;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<HDSDDichVuService> _logger;
        public HDSDDichVuService(
            IUnitOfWork unitOfWork,
            IHDSDDichVuRepository hdsdDichVuRepository,
            //IBaseRepository<DMError> dmErrorRepository,
            IDMErrorService130 dmErrorService,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<HDSDDichVuService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _hdsdDichVuRepository = hdsdDichVuRepository;
            //_dmErrorRepository = dmErrorRepository;
            _dmErrorService = dmErrorService;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseModel<PaginationResponseModel<HDSDDichVuInfoModel>>> GetItems(PaginationRequestModel request)
        {
            var res = new ResponseModel<PaginationResponseModel<HDSDDichVuInfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<HDSDDichVuInfoModel>());

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

                PaginationResponseModel<HDSDDichVu> items = await _hdsdDichVuRepository.GetListPaginationRules(request, currentCSKCB, currentRole.Equals(RoleConstant.MASTER));

                if (items != null && items.Items != null && items.Items.Any())
                {
                    //IEnumerable<DMError> errors = await _dmErrorRepository.GetListAsync();
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

                    res.Data.Items = lstRules;
                    res.Data.TotalItems = items.TotalItems;
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
                int hdsdID = 0;
                int.TryParse(id, out hdsdID);

                #region Validation
                if (hdsdID < 1)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    return res;
                }
                #endregion

                var item = await _hdsdDichVuRepository.GetById(hdsdID);
                var itemData = _mapper.Map<ModifyHDSDDichVuModel>(item);
                res.Data = itemData;
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, new ModifyHDSDDichVuModel(), _logger, $"{nameof(HDSDDichVuService)} - {nameof(HDSDDichVuService.GetItemById)}");
            }

            return res;
        }

        public async Task<ResponseModel<bool>> CreateNewItem(ModifyHDSDDichVuModel request)
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
                                ErrorCode = await _dmErrorService.GetLastErrorCode() + 1,
                                ErrorType = ErrorMessageType.HDSD_DICHVU,
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
                var currentCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                HDSDDichVu item;

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

                item = await _hdsdDichVuRepository.GetItem(x => x.HDSDDVID == request.Id);
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

        private ResponseModel<bool> ValidateModifyItem(ModifyHDSDDichVuModel request, string currentRole)
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

                HDSDDichVu item;
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

                item = await _hdsdDichVuRepository.GetById(id);
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
                    ex, false, _logger, $"{nameof(HDSDThuocService)} - {nameof(HDSDThuocService.ActivateItem)}");
            }
            return res;
        }
    }
}
