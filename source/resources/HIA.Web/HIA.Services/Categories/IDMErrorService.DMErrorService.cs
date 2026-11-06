//using AutoMapper;
//using HIA.Core.Base;
//using HIA.Core.Languages.Message;
//using HIA.Core.Models.Base;
//using HIA.Core.Models.Categories;
//using HIA.Core.Models.Files;
//using HIA.Core.Models.Token;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using HIA.Services.SysHistoryLogManagement;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace HIA.Services.Categories
//{
//    public class DMErrorService : IDMErrorService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        //private readonly IBaseRepository<DMError> _dmErrorRepo;
//        private readonly IBaseRepository<DMError130> _dmError130Repo;
//        private readonly IBaseRepository<DMErrorMHTC> _dmErrorMHTCRepo;
//        private readonly ISysHistoryLogService _sysHistoryLogService;
//        private readonly IHttpContextAccessor _httpContext;
//        private readonly IMapper _mapper;
//        private readonly ILogger<DMErrorService> _logger;
//        public DMErrorService(
//            IUnitOfWork unitOfWork,
//            //IBaseRepository<DMError> dmErrorRepo,
//            IBaseRepository<DMError130> dmError130Repo,
//            IBaseRepository<DMErrorMHTC> dmErrorMHTCRepo,
//            ISysHistoryLogService sysHistoryLogService,
//            IHttpContextAccessor httpContext,
//            IMapper mapper,
//            ILogger<DMErrorService> logger
//            )
//        {
//            _unitOfWork = unitOfWork;
//            //_dmErrorRepo = dmErrorRepo;
//            _dmError130Repo = dmError130Repo;
//            _dmErrorMHTCRepo = dmErrorMHTCRepo;
//            _sysHistoryLogService = sysHistoryLogService;
//            _httpContext = httpContext;
//            _mapper = mapper;
//            _logger = logger;
//        }

//        public async Task<ResponseModel<IEnumerable<SelectErrorsItems>>> GetSelectionErrorListByHospital()
//        {
//            var res = new ResponseModel<IEnumerable<SelectErrorsItems>>(HttpStatusCode.OK, "", new List<SelectErrorsItems>());
//            try
//            {
//                var isMaster = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value?.Contains(RoleConstant.MASTER);
//                var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                if (string.IsNullOrWhiteSpace(maCSKCB))  return res;

//                //var errorList = await _dmErrorRepo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                var errorList = await _dmError130Repo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                if (isMaster == true)
//                {
//                    res.Data = errorList.Select(x => new SelectErrorsItems
//                    {
//                        Text = x.Message,
//                        Value = x.ErrorCode.Value.ToString(),
//                    }).ToList();
//                }
//                else
//                {
//                    var userErrList = (await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null))?.Select(x => x.EMErrorCode).ToList();

//                    res.Data = errorList.Where(x => userErrList.Contains(x.ErrorCode)).Select(x => new SelectErrorsItems
//                    {
//                        Text = x.Message,
//                        Value = x.ErrorCode.Value.ToString(),
//                    }).ToList();
//                }
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<IEnumerable<SelectErrorsItems>>(
//                    ex, new List<SelectErrorsItems>(), _logger, $"{nameof(DMErrorService)} - {nameof(DMErrorService.GetSelectionErrorListByHospital)}");
//            }

//            return res;
//        }

//        public async Task<ResponseModel<List<DMErrorInfoModel>>> GetItems()
//        {
//            var res = new ResponseModel<List<DMErrorInfoModel>>(HttpStatusCode.OK, "", new List<DMErrorInfoModel>());
//            try
//            {
//                var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                if (string.IsNullOrWhiteSpace(maCSKCB))
//                {
//                    res.Code = HttpStatusCode.BadRequest;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }

//                #region Get List
//                //Linhist changed 01/04/23
//                //var errorList = await _dmErrorRepo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                //var userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null);

//                //res.Data = errorList.Select(x => new DMErrorInfoModel
//                //{
//                //    ErrorID = x.ErrorID,
//                //    ErrorCode = x.ErrorCode,
//                //    ErrorType = x.ErrorType,
//                //    ErrorAtXMLFile = x.ErrorAtXMLFile,
//                //    ErrorAtXMLTag = x.ErrorAtXMLTag,
//                //    ErrorContent = x.ErrorContent,
//                //    ErrorComment = x.ErrorComment,
//                //    IsSelected = userErrList.Any(y => y.EMErrorCode.HasValue && y.EMErrorCode.Equals(x.ErrorCode))
//                //}).ToList();
//                //var errorList = await _dmErrorRepo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                var errorList = await _dmError130Repo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                res.Data = errorList.Select(e => new DMErrorInfoModel
//                {
//                    ErrorID = e.ErrorId,
//                    ErrorCode = e.ErrorCode,
//                    ErrorType = e.ErrorType,
//                    ErrorAtXMLFile = e.ErrorFile,
//                    ErrorAtXMLTag = e.ErrorTag,
//                    ErrorContent = e.Message,
//                    ErrorComment = e.Comment,
//                    IsSelected = false,
//                    IsAlarm = false,
//                }).ToList();

//                var userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null);
//                foreach (var u in userErrList)
//                {
//                    var e = res.Data.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
//                    if (e != null)
//                    {
//                        e.IsSelected = true;
//                        e.IsAlarm = u.EMEnabled != true;
//                    }
//                }
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog("Get List Error");
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<List<DMErrorInfoModel>>(
//                    ex, null, _logger, $"{nameof(DMErrorService)} - {nameof(GetItems)}");
//            }
//            return res;
//        }

//        public async Task<ResponseModel<bool>> Save(DMErrorModifyModel request)
//        {
//            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
//            try
//            {
//                var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                if (string.IsNullOrWhiteSpace(maCSKCB))
//                {
//                    res.Code = HttpStatusCode.BadRequest;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }

//                #region Delete all user error
//                _dmErrorMHTCRepo.DeleteListByFilter(x => x.MA_CSKCB.Equals(maCSKCB));
//                #endregion

//                #region Add new
//                var items = new List<DMErrorMHTC>();

//                foreach (var item in request.Data)
//                {
//                    if (int.TryParse(item, out int errCode))
//                    {
//                        items.Add(new DMErrorMHTC
//                        {
//                            //Linhist changed 01/04/23 - ErrorCode > 0 --> Bao Loi, ErrorCode < 0 --> Canh Bao
//                            //EMErrorCode = errCode,
//                            EMErrorCode = Math.Abs(errCode),
//                            MA_CSKCB = maCSKCB,
//                            EMCreateDate = DateTime.Now,
//                            //Linhist changed 01/04/23 - ErrorCode > 0 --> Bao Loi, ErrorCode < 0 --> Canh Bao
//                            //EMEnabled = true,
//                            EMEnabled = (errCode > 0),
//                            EMCreator = _httpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""
//                        });
//                    }
//                }
//                await _dmErrorMHTCRepo.InsertListAsync(items);

//                await _unitOfWork.CommitAsync();
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog("Save List Error");
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<bool>(
//                    ex, false, _logger, $"{nameof(DMErrorService)} - {nameof(Save)}");
//            }
//            return res;
//        }

//        //Linhist added 01/04/23
//        public async Task<ResponseModel<IEnumerable<DMErrorInfoModel>>> GetAlertErrorListByHospital()
//        {
//            var res = new ResponseModel<IEnumerable<DMErrorInfoModel>>(HttpStatusCode.OK, "", new List<DMErrorInfoModel>());
//            try
//            {
//                var isMaster = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value?.Contains(RoleConstant.MASTER);
//                var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                if (string.IsNullOrWhiteSpace(maCSKCB)) return res;

//                //var errorList = await _dmErrorRepo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                var errorList = await _dmError130Repo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
//                if (isMaster == true)
//                {
//                    res.Data = errorList.Select(e => new DMErrorInfoModel
//                    {
//                        ErrorID = e.ErrorId,
//                        ErrorCode = e.ErrorCode,
//                        ErrorType = e.ErrorType,
//                        ErrorAtXMLFile = e.ErrorFile,
//                        ErrorAtXMLTag = e.ErrorTag,
//                        ErrorContent = e.Message,
//                        ErrorComment = e.Comment,
//                        IsSelected = true,
//                        IsAlarm = false,
//                    }).ToList();
//                }
//                else
//                {
//                    var userErrList = (await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null));

//                    var lstErrors = new List<DMErrorInfoModel>();
//                    //foreach (var u in userErrList)
//                    //{
//                    //    var e = errorList.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
//                    //    if (e != null)
//                    //    {
//                    //        lstErrors.Add(new DMErrorInfoModel
//                    //        {
//                    //            ErrorID = e.ErrorID,
//                    //            ErrorCode = e.ErrorCode,
//                    //            ErrorType = e.ErrorType,
//                    //            ErrorAtXMLFile = e.ErrorAtXMLFile,
//                    //            ErrorAtXMLTag = e.ErrorAtXMLTag,
//                    //            ErrorContent = e.ErrorContent,
//                    //            ErrorComment = e.ErrorComment,
//                    //            IsSelected = true,
//                    //            IsAlarm = u.EMEnabled != true,
//                    //        });
//                    //    }
//                    //}
//                    foreach (var e in errorList)
//                    {
//                        var u = userErrList.FirstOrDefault(a => a.EMErrorCode == e.ErrorCode);
//                        lstErrors.Add(new DMErrorInfoModel
//                        {
//                            ErrorID = e.ErrorId,
//                            ErrorCode = e.ErrorCode,
//                            ErrorType = e.ErrorType,
//                            ErrorAtXMLFile = e.ErrorFile,
//                            ErrorAtXMLTag = e.ErrorTag,
//                            ErrorContent = e.Message,
//                            ErrorComment = e.Comment,
//                            IsSelected = true,
//                            IsAlarm = u?.EMEnabled == false,
//                        });
//                    }

//                    res.Data = lstErrors;
//                }
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<IEnumerable<DMErrorInfoModel>>(
//                    ex, new List<DMErrorInfoModel>(), _logger, $"{nameof(DMErrorService)} - {nameof(DMErrorService.GetSelectionErrorListByHospital)}");
//            }

//            return res;
//        }

//        //Linhist added 07/03/24
//        public async Task<ResponseModel<bool>> SaveDefaultErrors(string maCSKCB)
//        {
//            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
//            try
//            {
//                if (string.IsNullOrWhiteSpace(maCSKCB))
//                {
//                    res.Code = HttpStatusCode.BadRequest;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }

//                //var errorList = await _dmErrorRepo.GetListAsync(e => e.IsDefault == true);
//                var errorList = await _dmError130Repo.GetListAsync(e => e.IsDefault == true);
//                if (!errorList.Any())
//                {
//                    res.Data = false;
//                    return res;
//                }

//                #region Add new
//                var items = new List<DMErrorMHTC>();

//                foreach (var err in errorList)
//                {
//                    items.Add(new DMErrorMHTC
//                    {
//                        //Linhist changed 01/04/23 - ErrorCode > 0 --> Bao Loi, ErrorCode < 0 --> Canh Bao
//                        //EMErrorCode = errCode,
//                        EMErrorCode = err.ErrorCode,
//                        MA_CSKCB = maCSKCB,
//                        EMCreateDate = DateTime.Now,
//                        EMEnabled = true,
//                        EMCreator = _httpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""
//                    });
//                }
//                await _dmErrorMHTCRepo.InsertListAsync(items);

//                await _unitOfWork.CommitAsync();
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog("Save List Error");
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<bool>(
//                    ex, false, _logger, $"{nameof(DMErrorService)} - {nameof(GetItems)}");
//            }
//            return res;
//        }

//    }
//}