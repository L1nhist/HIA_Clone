//using AutoMapper;
//using HIA.Core.Base;
//using HIA.Core.Languages.Message;
//using HIA.Core.Models.Base;
//using HIA.Core.Models.Categories;
//using HIA.Core.Models.Files;
//using HIA.Core.Models.FileUploads;
//using HIA.Core.Models.Pagination;
//using HIA.Core.Models.Token;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using HIA.Repositories.Files;
//using HIA.Services.Base;
//using HIA.Services.HosoManagement;
//using HIA.Services.SysHistoryLogManagement;
//using LinqKit;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Query;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Linq.Dynamic.Core;
//using System.Linq.Expressions;
//using System.Net;
//using System.Reflection;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Z.EntityFramework.Plus;

//using System.Xml;
//using System.Xml.Serialization;
//using Nancy.Json;

//namespace HIA.Services.Files
//{
//    public class FilesService : IFilesService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IXml1Repository _xml1Repository;
//        private readonly IXml1ErrorRepository _xml1ErrorRepository;
//        private readonly IBaseRepository<XML2> _xml2Repository;
//        private readonly IBaseRepository<XML3> _xml3Repository;
//        private readonly IBaseRepository<XML4> _xml4Repository;
//        private readonly IBaseRepository<XML5> _xml5Repository;
//        private readonly IBaseRepository<DMError> _dMErrorRepository;
//        private readonly IBaseRepository<DMErrorMHTC> _dMErrorMHTCRepository;
//        //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//        private readonly IBaseRepository<DMKhoaBV> _dmKhoaRepository;
//        private readonly IBaseRepository<DMBacSi> _dmBacSiRepository;
//        private readonly IBaseRepository<ChiDinhDongThoi> _dmDongThoiRepository;
//        private readonly IBaseRepository<DMThuoc> _dmThuoc;
//        private readonly IBaseRepository<DMVTYT> _dmVTYT;
//        private readonly IBaseRepository<DMDichVu> _dmDichVu;
//        private readonly IHoSoManagementService _hosoService;
//        private readonly IBaseRepository<XML1Error_API> _xml1Error_APIRepository;
//        private readonly IBaseRepository<XML1_API> _xml1APIRepository;
//        private readonly ISysHistoryLogService _sysHistoryLogService;
//        private readonly IHttpContextAccessor _httpContext;
//        private readonly IMapper _mapper;
//        private readonly ILogger<FilesService> _logger;

//        public FilesService(
//            IUnitOfWork unitOfWork,
//            IXml1Repository xml1Repository,
//            IBaseRepository<XML2> xml2Repository,
//            IBaseRepository<XML3> xml3Repository,
//            IBaseRepository<XML4> xml4Repository,
//            IBaseRepository<XML5> xml5Repository,
//            IBaseRepository<DMError> dMErrorRepository,
//            IBaseRepository<DMErrorMHTC> dMErrorMHTCRepository,
//            //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//            IBaseRepository<DMKhoaBV> dmKhoaRepository,
//            IBaseRepository<DMBacSi> dmBacSiRepository,
//            IBaseRepository<ChiDinhDongThoi> dmDongThoiRepository,
//            IXml1ErrorRepository xml1ErrorRepository,
//            ISysHistoryLogService sysHistoryLogService,
//            IHttpContextAccessor httpContext,
//            IMapper mapper,
//            ILogger<FilesService> logger,
//            IHoSoManagementService hosoService,
//            IBaseRepository<XML1_API> xml1_APIRepository,
//            IBaseRepository<XML1Error_API> xml1Error_APIRepository,
//            IBaseRepository<DMThuoc> dmThuoc,
//            IBaseRepository<DMVTYT> dmVTYT,
//            IBaseRepository<DMDichVu> dmDichVu
//        )
//        {
//            _unitOfWork = unitOfWork;
//            _xml1Repository = xml1Repository;
//            _xml2Repository = xml2Repository;
//            _xml3Repository = xml3Repository;
//            _xml4Repository = xml4Repository;
//            _xml5Repository = xml5Repository;
//            _xml1ErrorRepository = xml1ErrorRepository;
//            _xml1APIRepository = xml1_APIRepository;
//            _xml1Error_APIRepository = xml1Error_APIRepository;
//            _sysHistoryLogService = sysHistoryLogService;
//            _httpContext = httpContext;
//            _mapper = mapper;
//            _logger = logger;
//            _dMErrorRepository = dMErrorRepository;
//            _dMErrorMHTCRepository = dMErrorMHTCRepository;
//            _dmKhoaRepository = dmKhoaRepository;
//            _dmBacSiRepository = dmBacSiRepository;
//            _dmDongThoiRepository = dmDongThoiRepository;
//            _hosoService = hosoService;
//            _dmThuoc = dmThuoc;
//            _dmVTYT = dmVTYT;
//            _dmDichVu = dmDichVu;

//        }

//        #region XML 1
//        public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionCreatedDateList()
//        {
//            var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
//            try
//            {
//                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
//                var currentHospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;

//                #region Validation
//                if (string.IsNullOrWhiteSpace(currentRole))
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                #endregion
//                #region Build Filter Query
//                Expression<Func<XML1, bool>> filter = PredicateBuilder.New<XML1>(x => !x.isDelete.Value);
//                // add filter with role admin
//                if (!string.IsNullOrWhiteSpace(currentHospitalId) && (currentRole.Equals(RoleConstant.ADMIN) || currentRole.Equals(RoleConstant.USER)))
//                {
//                    filter = filter.And(x => x.HospitalId.Equals(currentHospitalId));
//                }
//                #endregion
//                #region Build Sort Query
//                IList<Func<IOrderedQueryable<XML1>, IOrderedQueryable<XML1>>> sortFuncList = new List<Func<IOrderedQueryable<XML1>, IOrderedQueryable<XML1>>>();
//                // order by createDay latest
//                sortFuncList.Add(x => x.ThenByDescending(x => x.CreateDate));
//                #endregion

//                var items = (await _xml1Repository.GetListAsync(
//                            filter,
//                            x => x.OrderByDescending(x => x.CreateDate)))?
//                            .Select(s => s.CreateDate.Value.ToString("dd/MM/yyyy"))
//                            .Distinct()
//                            .Select(s => new SelectListItem
//                            {
//                                Text = s,
//                                Value = s
//                            }).ToList();

//                res.Data = items;
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<IEnumerable<SelectListItem>>(
//                    ex, new SelectList(new List<SelectListItem>()), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetSelectionCreatedDateList)}");
//            }

//            return res;
//        }

//        private FilterQueryableModel<XML1> BuildQueryToGetListData(FilterDownloadModel request, bool hasIncludes = true)
//        {
//            var result = new FilterQueryableModel<XML1>();

//            var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
//            var currentHospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;
//            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

//            #region Validation
//            if (string.IsNullOrWhiteSpace(currentRole)) return null;
//            #endregion

//            #region Build Filter Query
//            //haimaster 12/10/2022 thu thay doi
//            Expression<Func<XML1, bool>> filter = PredicateBuilder.New<XML1>(x => x.isDelete != true);
//            //Expression<Func<XML1, bool>> filter = PredicateBuilder.New<XML1>(x => x.MHTCID != null);
//            //Linhist changed 16/06/22 cho Master loc theo benh vien, neu ko phai master thi theo benh vien mac dinh
//            // add filter with role admin
//            //if (!string.IsNullOrWhiteSpace(currentHospitalId) && (currentRole.Equals(RoleConstant.ADMIN) || currentRole.Equals(RoleConstant.USER)))
//            //{
//            //    filter = filter.And(x => x.HospitalId.Equals(currentHospitalId));
//            //}
//            if (currentRole?.Equals(RoleConstant.MASTER) == true || currentRole?.Equals(RoleConstant.ADMIN) == true)
//            {
//                if (!string.IsNullOrEmpty(request.HospitalId))
//                {
//                    filter = filter.And(x => x.MA_CSKCB == request.HospitalId);
//                }
//            }
//            else
//            {
//                filter = filter.And(x => x.MA_CSKCB == request.HospitalId);
//            }

//            // add filter when has searchKey
//            if (!string.IsNullOrWhiteSpace(request.SearchKey))
//            {
//                var key = request.SearchKey.ToLower();
//                //Linhist removed 20/03/22 to push up performance
//                //Expression<Func<XML1, bool>> filterSearch = PredicateBuilder.New<XML1>();
//                //foreach (PropertyInfo propertyInfo in new XML1().GetType().GetProperties())
//                //{
//                //    if (propertyInfo.Name == nameof(XML1InfoModel.XML1ID)) continue;

//                //    filterSearch = PredicateExpresion<XML1>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
//                //}

//                string pattern = request.SearchKey;
//                Expression<Func<XML1, bool>> filterSearch = PredicateBuilder.New<XML1>();
//                filterSearch = filterSearch.Or(x => x.MA_THE != null && x.MA_THE.Contains(pattern))
//                                            .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern))
//                                            .Or(x => x.MA_BN != null && x.MA_BN.Contains(pattern))
//                                            .Or(x => x.HO_TEN != null && x.HO_TEN.Contains(pattern))
//                                            .Or(x => x.DIA_CHI != null && x.DIA_CHI.Contains(pattern));

//                filter = filter.And(filterSearch);
//            }

//            #region Linhist removed 10/03/22
//            //Linhist bo doan nay, thay bang code moi voi cac thuoc tinh moi
//            //// date filter
//            //if (request.DateFilters.Any())
//            //{
//            //    Expression<Func<XML1, bool>> filterDate = PredicateBuilder.New<XML1>();
//            //    foreach (var item in request.DateFilters)
//            //    {
//            //        filterDate = filterDate.Or(x => DateTime.Compare(x.CreateDate.Value, DateTime.Parse($"{item} 00:00:00")) >= 0
//            //                                            && DateTime.Compare(x.CreateDate.Value, DateTime.Parse($"{item} 23:59:59")) <= 0);
//            //    }
//            //    filter = filter.And(filterDate);
//            //}
//            //// department filter
//            //if (request.Departments.Any())
//            //{
//            //    filter = filter.And(x => request.Departments.Any(y => y.Equals(x.MA_KHOA)));
//            //}
//            //// date range
//            //if (!string.IsNullOrWhiteSpace(request.FilterFromDate))
//            //{
//            //    filter = filter.And(x => DateTime.Compare(x.NGAY_RA.Value, DateTime.Parse($"{request.FilterFromDate} 00:00:00")) >= 0);
//            //}
//            //if (!string.IsNullOrWhiteSpace(request.FilterToDate))
//            //{
//            //    filter = filter.And(x => DateTime.Compare(x.NGAY_RA.Value, DateTime.Parse($"{request.FilterToDate} 23:59:59")) <= 0);
//            //}
//            //// error filter
//            //if (request.ErrFilter.Any())
//            //{
//            //    Expression<Func<XML1, bool>> filterError = PredicateBuilder.New<XML1>();
//            //    foreach (var item in request.ErrFilter)
//            //    {
//            //        int errCode = 0;
//            //        Int32.TryParse(item, out errCode);
//            //        if (errCode == 0) continue;

//            //        filterError = filterError.Or(x => x.XML1Errors.Any(y => y.XEErrorCode.Value == errCode));
//            //    }

//            //    filter = filter.And(filterError);
//            //}
//            #endregion
//            //haimaster 11/10/2022
//            ///filter = filter.And(x => x.CreateDate<DateTime.Now.AddMinutes(10));
//            //if (!((request.FromDateCreate != null && request.FromDateCreate.HasValue) || (request.ToDateCreate != null && request.ToDateCreate.HasValue)|| (request.FromDateIn != null && request.FromDateIn.HasValue) || (request.ToDateIn != null && request.ToDateIn.HasValue)|| (request.FromDateOut != null && request.FromDateOut.HasValue) || (request.ToDateOut != null && request.ToDateOut.HasValue)||(request.FromDatePaid != null && request.FromDatePaid.HasValue) || (request.ToDatePaid != null && request.ToDatePaid.HasValue)))
//            //{
//            filter = filter.And(x => x.MHTCID != null);
//            //}

//            // department filter
//            if (request.Departments != null && request.Departments.Any())
//            {
//                filter = filter.And(x => request.Departments.Contains(x.MA_KHOA));
//            }

//            #region Linhist added 10/03/22
//            // create date filter
//            if ((request.FromDateCreate != null && request.FromDateCreate.HasValue) || (request.ToDateCreate != null && request.ToDateCreate.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterCreate = PredicateBuilder.New<XML1>(x => x.CreateDate != null);
//                if (request.FromDateCreate != null && request.FromDateCreate.HasValue)
//                {
//                    DateTime dayFrom = request.FromDateCreate.Value.Date;
//                    filterCreate = filterCreate.And(x => x.CreateDate.Value >= dayFrom);
//                }
//                if (request.ToDateCreate != null && request.ToDateCreate.HasValue)
//                {
//                    DateTime dayTo = request.ToDateCreate.Value.Date.AddDays(1);
//                    //if (dayTo > DateTime.Now) dayTo = DateTime.Now.AddHours(-2);

//                    filterCreate = filterCreate.And(x => x.CreateDate.Value < dayTo);

//                }
//                filter = filter.And(filterCreate);
//            }

//            // NGAY_VAO filter
//            if ((request.FromDateIn != null && request.FromDateIn.HasValue) || (request.ToDateIn != null && request.ToDateIn.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterIn = PredicateBuilder.New<XML1>(x => x.NGAY_VAO != null);
//                if (request.FromDateIn != null && request.FromDateIn.HasValue)
//                {
//                    DateTime dayInFrom = request.FromDateIn.Value.Date;
//                    filterIn = filterIn.And(x => x.NGAY_VAO.Value >= dayInFrom);
//                }
//                if (request.ToDateIn != null && request.ToDateIn.HasValue)
//                {
//                    DateTime dayInTo = request.ToDateIn.Value.Date.AddDays(1);
//                    //if (dayInTo > DateTime.Now) dayInTo = DateTime.Now.AddHours(-2);
//                    filterIn = filterIn.And(x => x.NGAY_VAO.Value < dayInTo);
//                }
//                filter = filter.And(filterIn);
//            }

//            // NGAY_RA filter
//            if ((request.FromDateOut != null && request.FromDateOut.HasValue) || (request.ToDateOut != null && request.ToDateOut.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterOut = PredicateBuilder.New<XML1>(x => x.NGAY_RA != null);
//                if (request.FromDateOut != null && request.FromDateOut.HasValue)
//                {
//                    DateTime dayOutFrom = request.FromDateOut.Value.Date;
//                    filterOut = filterOut.And(x => x.NGAY_RA.Value >= dayOutFrom);
//                }
//                if (request.ToDateOut != null && request.ToDateOut.HasValue)
//                {
//                    DateTime dayOutTo = request.ToDateOut.Value.Date.AddDays(1);
//                    //if (dayOutTo > DateTime.Now) dayOutTo = DateTime.Now.AddHours(-2);
//                    filterOut = filterOut.And(x => x.NGAY_RA.Value < dayOutTo);
//                }
//                filter = filter.And(filterOut);
//            }
//            #endregion

//            #region Linhist added 19/04/22
//            // NGAY_TTOAN filter
//            if ((request.FromDatePaid != null && request.FromDatePaid.HasValue) || (request.ToDatePaid != null && request.ToDatePaid.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterPaid = PredicateBuilder.New<XML1>(x => x.NGAY_TTOAN != null);
//                if (request.FromDatePaid != null && request.FromDatePaid.HasValue)
//                {
//                    DateTime dayPaidFrom = request.FromDatePaid.Value.Date;
//                    filterPaid = filterPaid.And(x => x.NGAY_TTOAN.Value >= dayPaidFrom);
//                }
//                if (request.ToDatePaid != null && request.ToDatePaid.HasValue)
//                {
//                    DateTime dayPaidTo = request.ToDatePaid.Value.Date.AddDays(1);
//                    //if (dayPaidTo > DateTime.Now) dayPaidTo = DateTime.Now.AddHours(-2);
//                    filterPaid = filterPaid.And(x => x.NGAY_TTOAN.Value < dayPaidTo);
//                }
//                filter = filter.And(filterPaid);
//            }

//            // MA_LOAI_KCB filter
//            if (!string.IsNullOrEmpty(request.TypeKCB?.Trim()))
//            {
//                filter = filter.And(x => x.MA_LOAI_KCB.ToString() == request.TypeKCB);
//            }

//            // Error status filter
//            if (request.Errors != null && request.Errors.Any())
//            {
//                Expression<Func<XML1, bool>> filterError = PredicateBuilder.New<XML1>();
//                foreach (var item in request.Errors)
//                {
//                    int errCode = 0;
//                    if (Int32.TryParse(item, out errCode))
//                    {
//                        filterError = filterError.Or(x => x.XML1Errors.Any(y => y.XEErrorCode.Value == errCode));
//                    }
//                }

//                filter = filter.And(filterError);
//            }
//            else if (true.Equals(request.Status))
//            {
//                filter = filter.And(x => x.Creator != 2);
//            }
//            else if (false.Equals(request.Status))
//            {
//                filter = filter.And(x => x.Creator == 2);
//            }
//            #endregion

//            result.FilterQueryable = filter;
//            #endregion

//            #region Build Sort Query
//            IList<Func<IOrderedQueryable<XML1>, IOrderedQueryable<XML1>>> sortFuncList = new List<Func<IOrderedQueryable<XML1>, IOrderedQueryable<XML1>>>
//            {
//                // order by createDay latest
//                x => x.OrderByDescending(x => x.CreateDate)
//            };

//            //Linhist removed 20/03/22 skip all sort query
//            //// dynamic sort by column
//            //var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
//            //if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
//            //{
//            //    switch (columnName)
//            //    {
//            //        default:
//            //            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
//            //            break;
//            //    }
//            //}
//            result.OrderQueryable = sortFuncList;
//            #endregion

//            #region Includes
//            Func<IQueryable<XML1>, IIncludableQueryable<XML1, object>> includeProperties = null;
//            if (hasIncludes)
//            {
//                //Linhist removed 20/03/22 skip join more table to push up performance
//                //includeProperties = i => i.Include(i1 => i1.Hospital).Include(i2 => i2.XML1Errors);
//                //haimaster 12/10/2022 bo include
//                //includeProperties = i => i.Include(i2 => i2.XML1Errors);
//            }
//            result.IncludeQueryable = includeProperties;
//            #endregion

//            return result;
//        }
//        public async Task<ResponseModel<List<XML1ErrorQuantityModel>>> GetXml1ErrorItems()
//        {
//            var res = new ResponseModel<List<XML1ErrorQuantityModel>>(HttpStatusCode.OK, "", new List<XML1ErrorQuantityModel>());
//            try
//            {
//                var hosId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;
//                var xml1Ids = (await _xml1Repository.GetListAsync(x => x.HospitalId.Equals(hosId)))?.Select(x => x.XML1ID);
//                var items = await _xml1ErrorRepository.GetListAsync(x => xml1Ids.Contains(x.XEXML1ID.Value));
//                res.Data = items.GroupBy(x => x.XEErrorCode).Select(x => new XML1ErrorQuantityModel
//                {
//                    XeErrorCode = x.Key.Value,
//                    Total = x.Count()
//                }).ToList();
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                   ex, new List<XML1ErrorQuantityModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml1ErrorItems)}");
//            }
//            return res;
//        }

//        public async Task<ResponseModel<PaginationResponseModel<XML1InfoModel>>> GetXml1Items(FilesPaginationRequest request)
//        {
//            var res = new ResponseModel<PaginationResponseModel<XML1InfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<XML1InfoModel>());

//            try
//            {
//                var filterModel = _mapper.Map<FilterDownloadModel>(request);
//                var queryable = BuildQueryToGetListData(filterModel);

//                //queryable.IncludeQueryable = i => i.Include(x => x.XML1Errors);

//                if (queryable == null)
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                Uti.HandleCronJobLog(_logger, $"H1. Xong buoc chuan bi Filter  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.ValidateFiles));



//                #region Get Data
//                DateTime dd = DateTime.Now;
//                PaginationResponseModel<XML1> items = await _xml1Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber,
//                    queryable.FilterQueryable, queryable.OrderQueryable, queryable.IncludeQueryable);
//                var xmlInfo = _mapper.Map<List<XML1InfoModel>>(items?.Items?.ToList());

//                TimeSpan tss = DateTime.Now - dd;
//                Uti.HandleCronJobLog(_logger, $"H2.Haimaster Load XML1 DS) () - Total time: {tss.TotalMilliseconds}", nameof(HoSoManagementService.ValidateFiles));
//                //Uti.HandleCronJobLog(_logger, $"H2. Xong buoc chuan bi get DS XML  " + $"  (tts).", nameof(HoSoManagementService.ValidateFiles));
//                foreach (XML1InfoModel info in xmlInfo)
//                {
//                    info.HasError = info.Creator == 2;

//                }
//                res.Data.Items = xmlInfo;
//                res.Data.TotalItems = items.TotalItems;
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                    ex, new PaginationResponseModel<XML1InfoModel>(), _logger, $"Loi khi lay DS XML1 {nameof(FilesService)} - {nameof(FilesService.GetXml1Items)}");
//            }
//            return res;
//        }

//        public async Task<List<ErrorCounterModel>> GetXml1ErrorsCounter(FilesPaginationRequest request)
//        {
//            List<ErrorCounterModel> res = new List<ErrorCounterModel>();

//            try
//            {
//                var filterModel = _mapper.Map<FilterDownloadModel>(request);
//                var queryable = BuildQueryToGetListData(filterModel);
//                if (queryable == null)
//                {
//                    return res;
//                }

//                #region Get Data
//                DateTime dd = DateTime.Now;
//                List<ErrorCounterModel> errLst = null;
//                var errors = await _dMErrorRepository.GetListAsync(e => e.ErrorCode != null);
//                var errMHTC = await _dMErrorMHTCRepository.GetListAsync(e => e.MA_CSKCB == filterModel.HospitalId);
//                if (errors != null && errMHTC != null)
//                {
//                    errLst = errMHTC.Join(errors, em => em.EMErrorCode,
//                                         er => er.ErrorCode,
//                                         (em, er) => new ErrorCounterModel()
//                                         {
//                                             ErrorCode = er.ErrorCode,
//                                             ErrorContent = er.ErrorContent,
//                                             ErrorAtXMLFile = er.ErrorAtXMLFile,
//                                             ErrorAtXMLTag = er.ErrorAtXMLTag,
//                                             ErrorEnabled = em.EMEnabled,
//                                         }).ToList();
//                }

//                if (errLst != null && errLst.Count > 0)
//                {
//                    var x1 = (await _xml1Repository.GetAllCustomSortAsync(queryable.FilterQueryable, queryable.OrderQueryable, queryable.IncludeQueryable))?.Select(x => x.XML1ID).ToList();
//                    if (x1 != null)
//                    {
//                        var xes = await _xml1ErrorRepository.GetListAsync(xe => (string.IsNullOrEmpty(filterModel.HospitalId) || xe.MA_CSKCB == filterModel.HospitalId) && xe.XEXML1ID != null && x1.Contains(xe.XEXML1ID.Value));
//                        if (xes != null)
//                        {
//                            foreach (var ec in errLst)
//                            {
//                                ec.ErrorCount = xes.Where(xe => xe.XEErrorCode == ec.ErrorCode).Select(xe => xe.XEXML1ID).Distinct().Count();
//                                if (ec.ErrorCount > 0)
//                                {
//                                    res.Add(ec);
//                                }
//                            }
//                        }
//                    }
//                }
//                #endregion
//            }
//            catch (Exception ex)
//            {
//                Uti.HandleInternalServerError(ex, new List<ErrorCounterModel>(), _logger, $"Loi khi lay DS XML1 {nameof(FilesService)} - {nameof(FilesService.GetXml1ErrorsCounter)}");
//            }
//            return res;
//        }

//        public ResponseModel<XML1InfoModel> TransformXml1ToModal(XML1InfoModalObject request)
//        {
//            var res = new ResponseModel<XML1InfoModel>(HttpStatusCode.OK, "", request.Xml1Data);
//            if (!request.ErrorList.Any(x => x.ErrorAtXMLFile.Equals("XML1"))) return res;

//            var errData = request.ErrorList.Where(x => x.ErrorAtXMLFile.Equals("XML1"));
//            request.Xml1Data.ErrorList = errData.ToList();
//            return res;
//        }

//        public async Task<ResponseModel<byte[]>> DownloadExcelFile(string filterDownloadString, int? fileType)
//        {
//            //Linhist added 19/10/23
//            return await Download_ExcelFile_New(filterDownloadString, fileType);

//            #region Linhist removed 19/10/23
//            //Uti.HandleCronJobLog(_logger, $"BAOCAO -1: Lay BC Tong Hop - Total time {DateTime.Now}", nameof(FilesService.DownloadExcelFile));
//            //DateTime d0 = DateTime.Now;
//            //var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
//            //var currentHospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;
//            ////Linhist them MA_CSKCB de tim kiem
//            //var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

//            //try
//            //{
//            //    if (string.IsNullOrWhiteSpace(filterDownloadString)) return null;
//            //    FilterDownloadModel request = JsonConvert.DeserializeObject<FilterDownloadModel>(filterDownloadString);
//            //    var queryable = BuildQueryToGetListData(request, true);

//            //    if (queryable == null)
//            //    {
//            //        res.Code = HttpStatusCode.Forbidden;
//            //        res.Message = MessageResource.ERR_403_FORBIDDEN;
//            //        return res;
//            //    }

//            //    #region Get List - Linhist removed 09/06/22
//            //    //var data = await _IXml1Repository.GetAllListXmlReferenceKhoi(currentHospitalId, queryable?.FilterQueryable);
//            //    //var dataDownload = _mapper.Map<List<XML1DownloadModel>>(data);

//            //    //res.Data = HandleFileCommonService.DownloadExcel<XML1DownloadModel>(dataDownload);

//            //    //Linhist removed 09/06/22
//            //    //Linhist them DM Khoa vao Excel
//            //    //res.Data = HandleFileCommonService.DownloadExcelReport6Sheets(data, dmError);
//            //    //IEnumerable<DMKhoa> dmKhoas = await _dmKhoaRepository.GetListAsync();

//            //    //Linhist removed 09/06/22
//            //    //Linhist them DM Bacsi vao Excel
//            //    //IEnumerable<DMBacSi> dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == currentMaCSKCB);

//            //    //Linhist thay function khac de giam tai
//            //    //res.Data = HandleFileCommonService.DownloadExcelReport6Sheets(data, dmError, dmKhoas);
//            //    //Linhist changed 28/04/22
//            //    //res.Data = HandleFileCommonService.DownloadExcelReport9Sheets(data, dmError, dmKhoas);

//            //    //Linhist changed 03/05/22
//            //    //res.Data = HandleFileCommonService.DownloadExcelReport8Sheets(data, dmError, dmKhoas, dmBacsis);
//            //    #endregion

//            //    //Linhist added 09/06/22
//            //    IEnumerable<XML1> data = null;
//            //    Uti.HandleCronJobLog(_logger, $"BAOCAO -2: Lay BC Tong Hop - time start {DateTime.Now}", nameof(FilesService.DownloadExcelFile));

//            //    try
//            //    {
//            //        DateTime d1 = DateTime.Now;

//            //        if (Uti.NumExists(fileType, 1, 2, 3))
//            //        {
//            //            data = await _xml1Repository.GetListXmlReport3(currentHospitalId, queryable?.FilterQueryable);
//            //        }
//            //        //Linhist added 09/10/22 them case file 4
//            //        else if (fileType == 4)
//            //        {
//            //            data = await _xml1Repository.GetXML1AndErrorsOnly(currentHospitalId, queryable?.FilterQueryable);
//            //        }
//            //        else
//            //        {
//            //            //Linhist changed 24/02/23
//            //            //Uti.HandleCronJobLog(_logger, $"BAOCAO -3: Lay BC Tong Hop - time start {DateTime.Now}", nameof(FilesService.DownloadExcelFile));

//            //            //data = await _IXml1Repository.GetAllListXmlReferenceKhoi(currentHospitalId, queryable?.FilterQueryable);
//            //            //Uti.HandleCronJobLog(_logger, $"BAOCAO -4: Lay BC Tong Hop - time end query {DateTime.Now}", nameof(FilesService.DownloadExcelFile));

//            //        }

//            //        TimeSpan t1 = DateTime.Now - d1;
//            //        Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Lay ho so (XML1, 2, 3, 4, 5) - Total time {t1.TotalSeconds}s", nameof(FilesService.DownloadExcelFile));
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        res = Uti.HandleInternalServerError<byte[]>(
//            //           ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)} - BAOCAO #1: Lay ho so (XML1, 2, 3, 4, 5)");
//            //    }

//            //    //Linhist added 09/06/22
//            //    DateTime d2 = DateTime.Now;
//            //    TimeSpan t2;
//            //    if (fileType == 3)
//            //    {
//            //        try
//            //        {
//            //            var dmDongThoi = await _dmDongThoiRepository.GetListAsync(x => (x.MA_LOI == 930 || x.MA_LOI == 1004) && x.HIEU_LUC == true);
//            //            t2 = DateTime.Now - d2;
//            //            Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Lay DMDongThoi - Total time {t2.TotalSeconds}s", nameof(FilesService.DownloadExcelFile));

//            //            res.Data = HandleFileCommonService.DownloadExcelIndication(data, dmDongThoi);
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            res = Uti.HandleInternalServerError<byte[]>(
//            //               ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)} - BAOCAO #2: Bao cao DV Dong thoi");
//            //        }
//            //    }
//            //    //Linhist added 09/10/22 them case filetype = 4
//            //    else if (fileType == 4)
//            //    {
//            //        try
//            //        {
//            //            var dmErrors = await _dMErrorRepository.GetListAsync(x => x.ErrorID != 0);
//            //            //Linhist added 02/04/23
//            //            var userErrList = (await _dMErrorMHTCRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB, null, null));

//            //            var lstErrors = new List<DMErrorInfoModel>();
//            //            foreach (var u in userErrList)
//            //            {
//            //                var e = dmErrors.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
//            //                if (e != null)
//            //                {
//            //                    lstErrors.Add(new DMErrorInfoModel
//            //                    {
//            //                        ErrorID = e.ErrorID,
//            //                        ErrorCode = e.ErrorCode,
//            //                        ErrorType = e.ErrorType,
//            //                        ErrorAtXMLFile = e.ErrorAtXMLFile,
//            //                        ErrorAtXMLTag = e.ErrorAtXMLTag,
//            //                        ErrorContent = e.ErrorContent,
//            //                        ErrorComment = e.ErrorComment,
//            //                        IsSelected = true,
//            //                        IsAlarm = u.EMEnabled != true,
//            //                    });
//            //                }
//            //            }

//            //            t2 = DateTime.Now - d2;
//            //            Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Lay DMError - Total time {t2.TotalSeconds}s", nameof(FilesService.DownloadExcelFile));

//            //            res.Data = HandleFileCommonService.DownloadExcelSumErrors(data, lstErrors);
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            res = Uti.HandleInternalServerError<byte[]>(
//            //               ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)} - BAOCAO #2: Bao cao Thong ke loi");
//            //        }
//            //    }
//            //    else
//            //    {
//            //        IEnumerable<DMKhoa> dmKhoas = null;
//            //        IEnumerable<DMBacSi> dmBacsis = null;
//            //        try
//            //        {
//            //            //Linhist added 09/06/22
//            //            dmKhoas = await _dmKhoaRepository.GetListAsync();
//            //            dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == currentMaCSKCB);

//            //            t2 = DateTime.Now - d2;
//            //            Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Lay DMBacsi & DMKhoa - Total time {t2.TotalSeconds}s", nameof(FilesService.DownloadExcelFile));
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            res = Uti.HandleInternalServerError<byte[]>(
//            //               ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)} - BAOCAO #2: Lay DM Khoa & BS");
//            //        }


//            //        try
//            //        {
//            //            if (fileType == 1)
//            //            {
//            //                res.Data = HandleFileCommonService.DownloadExcelDublicated(data, dmKhoas, dmBacsis);
//            //            }
//            //            else if (fileType == 2)
//            //            {
//            //                res.Data = HandleFileCommonService.DownloadExcelResources(data, dmKhoas, dmBacsis);
//            //            }
//            //            //Linhist added 24/02/23
//            //            else
//            //            {
//            //                try
//            //                {
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -5: Lay BC Tong Hop - time start send to Download Exxcel {DateTime.Now}", nameof(FilesService.DownloadExcelFile));
//            //                    DateTime d3 = DateTime.Now;
//            //                    var dmErrors = await _dMErrorRepository.GetListAsync(x => x.ErrorID != 0);
//            //                    //Linhist added 02/04/23
//            //                    var userErrList = (await _dMErrorMHTCRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB, null, null));

//            //                    var lstErrors = new List<DMErrorInfoModel>();
//            //                    foreach (var u in userErrList)
//            //                    {
//            //                        var e = dmErrors.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
//            //                        if (e != null)
//            //                        {
//            //                            lstErrors.Add(new DMErrorInfoModel
//            //                            {
//            //                                ErrorID = e.ErrorID,
//            //                                ErrorCode = e.ErrorCode,
//            //                                ErrorType = e.ErrorType,
//            //                                ErrorAtXMLFile = e.ErrorAtXMLFile,
//            //                                ErrorAtXMLTag = e.ErrorAtXMLTag,
//            //                                ErrorContent = e.ErrorContent,
//            //                                ErrorComment = e.ErrorComment,
//            //                                IsSelected = true,
//            //                                IsAlarm = u.EMEnabled != true,
//            //                            });
//            //                        }
//            //                    }

//            //                    //var data1 = await _IXml1Repository.GetXml_1_2(currentHospitalId, queryable?.FilterQueryable);
//            //                    //Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop - xml1 - xml2 {DateTime.Now} xml1: {data1.Count()}", nameof(FilesService.DownloadExcelFile));
//            //                    //var data2 = data1.SelectMany(x => x.XML2s).ToList();
//            //                    //var data3 = data1.SelectMany(x => x.XML3s).ToList();
//            //                    //var data4 = data1.SelectMany(x => x.XML4s).ToList();
//            //                    //Uti.HandleCronJobLog(_logger, $"BAOCAO -6.2: Lay BC Tong Hop - xml1 - xml2 {DateTime.Now} xml1: {data1.Count()} xml2: {data2.Count()} xml3: {data3.Count()} xml4: {data4.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    //data = await _IXml1Repository.GetXml_1_3(currentHospitalId, queryable?.FilterQueryable);
//            //                    //Uti.HandleCronJobLog(_logger, $"BAOCAO -7.1: Lay BC Tong Hop - time start send to Download Exxcel {DateTime.Now} xml1: {data.Count()}", nameof(FilesService.DownloadExcelFile));
//            //                    //var data5 = data.SelectMany(x => x.XML5s).ToList();
//            //                    //var dataE = data.SelectMany(x => x.XML1Errors).ToList();
//            //                    //Uti.HandleCronJobLog(_logger, $"BAOCAO -7.2: Lay BC Tong Hop - time start send to Download Exxcel {DateTime.Now} xml5: {data5.Count()} xmlErr: {dataE.Count()}", nameof(FilesService.DownloadExcelFile));



//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6: Lay BC Tong Hop le tung XML:", nameof(FilesService.DownloadExcelFile));
//            //                    var data1 = await _xml1Repository.GetXml_1_2(currentHospitalId, queryable?.FilterQueryable);
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop {DateTime.Now} xml1: {data1.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    var lstXMLIDs = data1.Select(x => x.XML1ID).ToList();

//            //                    var data2 = await _xml2Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value));
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop {DateTime.Now} xml2: {data2.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    var data3 = await _xml3Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value));
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop {DateTime.Now} xml3: {data3.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    var data4 = await _xml4Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value));
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop {DateTime.Now} xml4: {data4.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    var data5 = await _xml5Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value));
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop {DateTime.Now} xml5: {data5.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    var dataE = await _xml1ErrorRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XEXML1ID != null && lstXMLIDs.Contains(x.XEXML1ID.Value));
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO -6.1: Lay BC Tong Hop {DateTime.Now} xml6: {dataE.Count()}", nameof(FilesService.DownloadExcelFile));

//            //                    foreach (var x1 in data1)
//            //                    {
//            //                        var dt2 = data2.Where(x => x.XML1ID == x1.XML1ID);
//            //                        foreach (var x2 in dt2)
//            //                        {
//            //                            x2.XML1 = x1;
//            //                        }

//            //                        var dt3 = data3.Where(x => x.XML1ID == x1.XML1ID);
//            //                        foreach (var x3 in dt3)
//            //                        {
//            //                            x3.XML1 = x1;
//            //                        }

//            //                        var dt4 = data4.Where(x => x.XML1ID == x1.XML1ID);
//            //                        foreach (var x4 in dt4)
//            //                        {
//            //                            x4.XML1 = x1;
//            //                        }

//            //                        var dt5 = data5.Where(x => x.XML1ID == x1.XML1ID);
//            //                        foreach (var x5 in dt5)
//            //                        {
//            //                            x5.XML1 = x1;
//            //                        }

//            //                        var dtE = dataE.Where(x => x.XEXML1ID == x1.XML1ID);
//            //                        foreach (var xe in dtE)
//            //                        {
//            //                            xe.XEXML1 = x1;
//            //                        }
//            //                    }


//            //                    TimeSpan t3 = DateTime.Now - d3;
//            //                    Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Lay DMError BC Tong hop (Loai 4: Lay le tung XML) - Total time {t3.TotalSeconds}s", nameof(FilesService.DownloadExcelFile));

//            //                    res.Data = HandleFileCommonService.DownloadExcelTotalErrors_New(data1, data2, data3, data4, data5, dataE, lstErrors, dmKhoas, dmBacsis);
//            //                }
//            //                catch (Exception ex)
//            //                {
//            //                    res = Uti.HandleInternalServerError<byte[]>(
//            //                                ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)} - BAOCAO #H: Lay BC Tong hop Errors");
//            //                }
//            //            }
//            //            //Linhist removed 24/02/23
//            //            //else
//            //            //{
//            //            //    DateTime d3 = DateTime.Now;
//            //            //    var dmErrors = await _dMErrorRepository.GetListAsync(x => x.ErrorID != 0);

//            //            //    //Linhist changed 15/11/22
//            //            //    //var IdXml1HaveXml4Error = data.SelectMany(x => x.XML1Errors).Where(y => y.XEErrorCode == 431).Select(x => x.XEXML1ID).ToList();
//            //            //    //var xml1HaveXml4Error = await _IXml1Repository.GetAllListXmlReference_WithXml4Only(currentHospitalId, queryable?.FilterQueryable);
//            //            //    //res.Data = HandleFileCommonService.DownloadExcelTotalErrors(data, dmErrors, dmKhoas, dmBacsis, xml1HaveXml4Error);

//            //            //    //var xml1Ids = data.Select(x => x.XML1ID).ToList();
//            //            //    //var xml4InXml1s = await _xml4Repository.GetListAsync(x => x.XML1ID != null && xml1Ids.Contains(x.XML1ID.Value));

//            //            //    var xml4InXml1s = data.SelectMany(x => x.XML4s);

//            //            //    TimeSpan t3 = DateTime.Now - d3;

//            //            //    Uti.HandleCronJobLog(_logger, $"BAOCAO -5: Lay BC Tong Hop - time start send to Download Exxcel {DateTime.Now}", nameof(FilesService.DownloadExcelFile));

//            //            //    res.Data = HandleFileCommonService.DownloadExcelTotalErrors(data, dmErrors, dmKhoas, dmBacsis, xml4InXml1s);
//            //            //    Uti.HandleCronJobLog(_logger, $"BAOCAO -5: Lay BC Tong Hop - time end send to Download Exxcel {DateTime.Now}", nameof(FilesService.DownloadExcelFile));
//            //            //    Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Lay DMError BC Tong hop (Loai 3: Lay all XML - Khong dung AsNoTracking) - Total time {t3.TotalSeconds}s", nameof(FilesService.DownloadExcelFile));
//            //            //}
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            res = Uti.HandleInternalServerError<byte[]>(
//            //               ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)} - BAOCAO #3: Lay DM Errors");
//            //        }
//            //    }
//            //}
//            //catch (Exception ex)
//            //{
//            //    res = Uti.HandleInternalServerError<byte[]>(
//            //       ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)}");
//            //}

//            //TimeSpan t0 = DateTime.Now - d0;
//            //Uti.HandleCronJobLog(_logger, $"BAOCAO 0: Tong thoi gian lay BC - Total time {t0.TotalSeconds}s" + (res.Code == HttpStatusCode.OK ? "" : " - Khong the lay dc BC"), nameof(FilesService.DownloadExcelFile));
//            //return res;
//            #endregion
//        }
//        #endregion

//        #region Linhist added 27/02/23
//        private FilterQueryableModel<XML1> Build_Filter_Query(string filterDownloadString, string currentMaCSKCB = "")
//        {
//            FilterDownloadModel request = null;

//            try
//            {
//                request = JsonConvert.DeserializeObject<FilterDownloadModel>(filterDownloadString);
//            }
//            catch
//            {
//                request = new FilterDownloadModel();
//            }

//            var result = new FilterQueryableModel<XML1>();

//            Expression<Func<XML1, bool>> filter = PredicateBuilder.New<XML1>(x => x.NGAYLAP<DateTime.Now.AddSeconds(-60) && x.isDelete != true && x.MHTCID != null);
//            string maCSKCB = !string.IsNullOrWhiteSpace(currentMaCSKCB) ? currentMaCSKCB : (string.IsNullOrWhiteSpace(request.MaCSKCB) ? request.MaCSKCB : "");
//            if (maCSKCB != "")
//            {
//                filter = filter.And(x => x.MA_CSKCB == maCSKCB);
//            }

//            if (!string.IsNullOrWhiteSpace(request.SearchKey))
//            {
//                var key = request.SearchKey.ToLower();

//                string pattern = request.SearchKey;
//                Expression<Func<XML1, bool>> filterSearch = PredicateBuilder.New<XML1>();
//                filterSearch = filterSearch.Or(x => x.MA_THE != null && x.MA_THE.Contains(pattern))
//                                            .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern))
//                                            .Or(x => x.MA_BN != null && x.MA_BN.Contains(pattern))
//                                            .Or(x => x.HO_TEN != null && x.HO_TEN.Contains(pattern))
//                                            .Or(x => x.DIA_CHI != null && x.DIA_CHI.Contains(pattern));

//                filter = filter.And(filterSearch);
//            }

//            if (request.Departments?.Any() == true)
//            {
//                filter = filter.And(x => request.Departments.Contains(x.MA_KHOA));
//            }

//            if ((request.FromDateCreate != null && request.FromDateCreate.HasValue) || (request.ToDateCreate != null && request.ToDateCreate.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterCreate = PredicateBuilder.New<XML1>(x => x.CreateDate != null);
//                if (request.FromDateCreate != null && request.FromDateCreate.HasValue)
//                {
//                    DateTime dayFrom = request.FromDateCreate.Value.Date;
//                    filterCreate = filterCreate.And(x => x.CreateDate.Value >= dayFrom);
//                }
//                if (request.ToDateCreate != null && request.ToDateCreate.HasValue)
//                {
//                    DateTime dayTo = request.ToDateCreate.Value.Date.AddDays(1);
//                    filterCreate = filterCreate.And(x => x.CreateDate.Value < dayTo);

//                }
//                filter = filter.And(filterCreate);
//            }

//            // NGAY_VAO filter
//            if ((request.FromDateIn != null && request.FromDateIn.HasValue) || (request.ToDateIn != null && request.ToDateIn.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterIn = PredicateBuilder.New<XML1>(x => x.NGAY_VAO != null);
//                if (request.FromDateIn != null && request.FromDateIn.HasValue)
//                {
//                    DateTime dayInFrom = request.FromDateIn.Value.Date;
//                    filterIn = filterIn.And(x => x.NGAY_VAO.Value >= dayInFrom);
//                }
//                if (request.ToDateIn != null && request.ToDateIn.HasValue)
//                {
//                    DateTime dayInTo = request.ToDateIn.Value.Date.AddDays(1);
//                    filterIn = filterIn.And(x => x.NGAY_VAO.Value < dayInTo);
//                }
//                filter = filter.And(filterIn);
//            }

//            // NGAY_RA filter
//            if ((request.FromDateOut != null && request.FromDateOut.HasValue) || (request.ToDateOut != null && request.ToDateOut.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterOut = PredicateBuilder.New<XML1>(x => x.NGAY_RA != null);
//                if (request.FromDateOut != null && request.FromDateOut.HasValue)
//                {
//                    DateTime dayOutFrom = request.FromDateOut.Value.Date;
//                    filterOut = filterOut.And(x => x.NGAY_RA.Value >= dayOutFrom);
//                }
//                if (request.ToDateOut != null && request.ToDateOut.HasValue)
//                {
//                    DateTime dayOutTo = request.ToDateOut.Value.Date.AddDays(1);
//                    filterOut = filterOut.And(x => x.NGAY_RA.Value < dayOutTo);
//                }
//                filter = filter.And(filterOut);
//            }

//            // NGAY_TTOAN filter
//            if ((request.FromDatePaid != null && request.FromDatePaid.HasValue) || (request.ToDatePaid != null && request.ToDatePaid.HasValue))
//            {
//                Expression<Func<XML1, bool>> filterPaid = PredicateBuilder.New<XML1>(x => x.NGAY_TTOAN != null);
//                if (request.FromDatePaid != null && request.FromDatePaid.HasValue)
//                {
//                    DateTime dayPaidFrom = request.FromDatePaid.Value.Date;
//                    filterPaid = filterPaid.And(x => x.NGAY_TTOAN.Value >= dayPaidFrom);
//                }
//                if (request.ToDatePaid != null && request.ToDatePaid.HasValue)
//                {
//                    DateTime dayPaidTo = request.ToDatePaid.Value.Date.AddDays(1);
//                    filterPaid = filterPaid.And(x => x.NGAY_TTOAN.Value < dayPaidTo);
//                }
//                filter = filter.And(filterPaid);
//            }

//            // MA_LOAI_KCB filter
//            if (!string.IsNullOrEmpty(request.TypeKCB?.Trim()))
//            {
//                filter = filter.And(x => x.MA_LOAI_KCB.ToString() == request.TypeKCB);
//            }

//            // Error status filter
//            if (request.Errors?.Any() == true)
//            {
//                List<int?> lstErrors = new List<int?>();
//                Expression<Func<XML1, bool>> filterError = PredicateBuilder.New<XML1>();
//                foreach (var item in request.Errors)
//                {

//                    int errCode = 0;
//                    if (int.TryParse(item, out errCode))
//                    {
//                        lstErrors.Add(errCode);
//                    }
//                }

//                filterError = filterError.And(x => x.XML1Errors.Any(y => lstErrors.Contains(y.XEErrorCode)));
//            }
//            else if (true.Equals(request.Status))
//            {
//                filter = filter.And(x => x.Creator != 2);
//            }
//            else if (false.Equals(request.Status))
//            {
//                filter = filter.And(x => x.Creator == 2);
//            }

//            result.FilterQueryable = filter;
//            result.OrderQueryable = new List<Func<IOrderedQueryable<XML1>, IOrderedQueryable<XML1>>>
//            {
//                x => x.OrderByDescending(x => x.CreateDate)
//            };
//            return result;
//        }

//        //Linhist added 19/10/23
//        private DateTime Get_Selected_CreateDate(string filterDownloadString)
//        {
//            FilterDownloadModel request = new FilterDownloadModel();

//            try
//            {
//                request = JsonConvert.DeserializeObject<FilterDownloadModel>(filterDownloadString);
//            }
//            catch { }

//            DateTime d = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
//            if (request.FromDateCreate != null)
//            {
//                d = new DateTime(request.FromDateCreate.Value.Year, request.FromDateCreate.Value.Month, 1);
//            }
//            return d;
//        }

//        public async Task<ResponseModel<byte[]>> Download_ExcelFile_New(string filterDownloadString, int? fileType)
//        {
//            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
//            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

//            IEnumerable<XML1> data1 = null;
//            IEnumerable<XML2> data2 = null;
//            IEnumerable<XML3> data3 = null;
//            IEnumerable<XML4> data4 = null;
//            IEnumerable<XML5> data5 = null;

//            IEnumerable<XML1Error> dataE = null;
//            IEnumerable<DMBacSi> dmBacsis = null;
//            IEnumerable<ChiDinhDongThoi> dmDongthois = null;

//            //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//            IEnumerable<DMKhoaBV> dmKhoas = null;
//            IEnumerable<DMDichVu> dmDVs = null;

//            //Linhist changed 02/04/23
//            //IEnumerable<DMError> dmErrors = null;
//            List<DMErrorInfoModel> dmErrors = null;

//            //Linhist added 19/10/23
//            //BC loai 6 (Cham cong) can danh sach bac si trong xml2, xml3 trong tuan tu t2 den cn
//            if (fileType == 6)
//            {
//                try
//                {
//                    DateTime dStart = Get_Selected_CreateDate(filterDownloadString);
//                    DateTime dEnd = dStart.AddDays(32);
//                    while (dEnd.Month != dStart.Month)
//                    {
//                        dEnd = dEnd.AddDays(-1);
//                        if (dEnd.Month == dStart.Month)
//                        {
//                            dEnd = dEnd.AddDays(1);
//                            break;
//                        }
//                    }

//                    data2 = await _xml2Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.isDelete != true && x.XML1.isDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null && x.NGAY_YL >= dStart && x.NGAY_YL < dEnd, null, d => d.Include(x => x.XML1));
//                    data3 = await _xml3Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.isDelete != true && x.XML1.isDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null && x.NGAY_YL >= dStart && x.NGAY_KQ != null && x.NGAY_KQ < dEnd, null, d => d.Include(x => x.XML1));

//                    //var idXml1s = data2.Select(x => x.XML1ID).Union(data3.Select(x => x.XML1ID));
//                    //data1 = await _xml1Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.isDelete == true && idXml1s.Contains(x.XML1ID));
//                    //idXml1s = data1.Select(x => (int?)x.XML1ID);

//                    //data2 = data2.Where(x => !idXml1s.Contains(x.XML1ID)).ToList();
//                    //data3 = data3.Where(x => !idXml1s.Contains(x.XML1ID)).ToList();
//                    dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == currentMaCSKCB);
//                    res.Data = HandleFileCommonService.DownloadExcelWorkDays_New(data2, data3, dmBacsis, dStart, dEnd);

//                }
//                catch (Exception ex)
//                {
//                    res = Uti.HandleInternalServerError<byte[]>(
//                       ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.Download_ExcelFile_New)}");
//                }

//                return res;
//            }

//            try
//            {
//                var queryable = Build_Filter_Query(filterDownloadString, currentMaCSKCB);
//                data1 = await _xml1Repository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML1ID));

//                List<int?> lstXMLIDs = data1.Select(x => (int?)x.XML1ID).ToList();

//                //BC loai 0,1,2 can XML1,2
//                if (fileType < 3)
//                {
//                    data2 = await _xml2Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value), x => x.OrderByDescending(x1 => x1.XML1ID));
//                }

//                //BC loai 0,1,2,3 can XML3
//                if (fileType < 4)
//                {
//                    data3 = await _xml3Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value), x => x.OrderByDescending(x1 => x1.XML1ID));
//                }

//                //BC loai 0 (BC tong hop) can XML4,5
//                if (fileType == 0)
//                {
//                    data4 = await _xml4Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value), x => x.OrderByDescending(x1 => x1.XML1ID));
//                    data5 = await _xml5Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value), x => x.OrderByDescending(x1 => x1.XML1ID));
//                }

//                //BC loai 5 (BC trung XML4) can XML4
//                if (fileType == 5)
//                {
//                    data4 = await _xml4Repository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XML1ID != null && lstXMLIDs.Contains(x.XML1ID.Value), x => x.OrderByDescending(x1 => x1.XML1ID));
//                    var allMaDVs = data4.Select(x => x.MA_DICH_VU).Distinct().ToList();

//                    dmDVs = await _dmDichVu.GetListAsync(d => allMaDVs.Contains(d.MA_DICH_VU) && d.MA_CSKCB == currentMaCSKCB);
//                }

//                //BC loai 0 & 4 can Errors
//                if (fileType == 0 || fileType == 4)
//                {
//                    dataE = await _xml1ErrorRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.XEXML1ID != null && lstXMLIDs.Contains(x.XEXML1ID.Value));
//                    var lstErrors = await _dMErrorRepository.GetListAsync(x => x.ErrorCode != null);

//                    //Linhist added 02/04/23
//                    var userErrList = (await _dMErrorMHTCRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB, null, null));

//                    dmErrors = new List<DMErrorInfoModel>();
//                    foreach (var u in userErrList)
//                    {
//                        var e = lstErrors.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
//                        if (e != null)
//                        {
//                            dmErrors.Add(new DMErrorInfoModel
//                            {
//                                ErrorID = e.ErrorID,
//                                ErrorCode = e.ErrorCode,
//                                ErrorType = e.ErrorType,
//                                ErrorAtXMLFile = e.ErrorAtXMLFile,
//                                ErrorAtXMLTag = e.ErrorAtXMLTag,
//                                ErrorContent = e.ErrorContent,
//                                ErrorComment = e.ErrorComment,
//                                IsSelected = true,
//                                IsAlarm = u.EMEnabled != true,
//                            });
//                        }
//                    }
//                }

//                //Gan XML1 vao cac XML2,3,4,5,Errors
//                foreach (var x1 in data1)
//                {
//                    if (data2 != null)
//                    {
//                        var dt2 = data2.Where(x => x.XML1ID == x1.XML1ID);
//                        foreach (var x2 in dt2)
//                        {
//                            x2.XML1 = x1;
//                        }
//                    }

//                    if (data3 != null)
//                    {
//                        var dt3 = data3.Where(x => x.XML1ID == x1.XML1ID);
//                        foreach (var x3 in dt3)
//                        {
//                            x3.XML1 = x1;
//                        }
//                    }

//                    if (data4 != null)
//                    {
//                        var dt4 = data4.Where(x => x.XML1ID == x1.XML1ID);
//                        foreach (var x4 in dt4)
//                        {
//                            x4.XML1 = x1;
//                        }
//                    }

//                    if (data5 != null)
//                    {
//                        var dt5 = data5.Where(x => x.XML1ID == x1.XML1ID);
//                        foreach (var x5 in dt5)
//                        {
//                            x5.XML1 = x1;
//                        }
//                    }

//                    if (dataE != null)
//                    {
//                        var dtE = dataE.Where(x => x.XEXML1ID == x1.XML1ID);
//                        foreach (var xe in dtE)
//                        {
//                            xe.XEXML1 = x1;
//                        }
//                    }
//                }

//                if (fileType == 3)
//                {
//                    dmDongthois = await _dmDongThoiRepository.GetListAsync(x => (x.MA_LOI == 930 || x.MA_LOI == 1004) && x.HIEU_LUC == true);
//                    res.Data = HandleFileCommonService.DownloadExcelIndication_New(data1, data3, dmDongthois);
//                }
//                else if (fileType == 4)
//                {
//                    res.Data = HandleFileCommonService.DownloadExcelSumErrors_New(data1, dataE, dmErrors);
//                }
//                else if (fileType == 5)
//                {
//                    res.Data = HandleFileCommonService.DownloadExcelXml4Dublicated_New(dmDVs, data4);
//                }
//                else
//                {
//                    dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == currentMaCSKCB);
//                    dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == currentMaCSKCB);

//                    if (fileType == 1)
//                    {
//                        res.Data = HandleFileCommonService.DownloadExcelDublicated_New(data2, data3, dmKhoas, dmBacsis);
//                    }
//                    else if (fileType == 2)
//                    {
//                        res.Data = HandleFileCommonService.DownloadExcelResources_New(data2, data3, dmKhoas, dmBacsis);
//                    }
//                    else
//                    {
//                        res.Data = HandleFileCommonService.DownloadExcelTotalErrors_New(data1, data2, data3, data4, data5, dataE, dmErrors, dmKhoas, dmBacsis);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<byte[]>(
//                   ex, null, _logger, $"{nameof(FilesService)} - {nameof(FilesService.DownloadExcelFile)}");
//            }

//            return res;
//        }
//        #endregion

//        #region XML 2
//        public async Task<ResponseModel<PaginationResponseModel<XML2InfoModel>>> GetXml2Items(PaginationRequestModel request, int xml1Id)
//        {
//            var res = new ResponseModel<PaginationResponseModel<XML2InfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<XML2InfoModel>());
//            //Int32  maxml1 = xml1Id;

//            try
//            {
//                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

//                #region Validation
//                if (string.IsNullOrWhiteSpace(currentRole))
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                #endregion

//                #region Build Filter Query
//                //Expression<Func<XML2, bool>> filter = PredicateBuilder.New<XML2>(x => x.XML1ID.Equals(xml1Id) && !x.isDelete.Value);
//                //// add filter when has searchKey
//                //if (!string.IsNullOrWhiteSpace(request.SearchKey))
//                //{
//                //    var key = request.SearchKey.ToLower();
//                //    Expression<Func<XML2, bool>> filterSearch = PredicateBuilder.New<XML2>();
//                //    foreach (PropertyInfo propertyInfo in new XML2().GetType().GetProperties())
//                //    {
//                //        if (propertyInfo.Name == nameof(XML2InfoModel.XML1ID) || propertyInfo.Name == nameof(XML2InfoModel.XML2ID)) continue;

//                //        filterSearch = PredicateExpresion<XML2>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
//                //    }
//                //    filter = filter.And(filterSearch);
//                //}
//                #endregion

//                #region Build Sort Query
//                //IList<Func<IOrderedQueryable<XML2>, IOrderedQueryable<XML2>>> sortFuncList = new List<Func<IOrderedQueryable<XML2>, IOrderedQueryable<XML2>>>();
//                //var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
//                //// dynamic sort by column
//                //if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
//                //{
//                //    switch (columnName)
//                //    {
//                //        default:
//                //            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
//                //            break;
//                //    }
//                //}
//                #endregion

//                #region Get Data
//                //PaginationResponseModel<XML2> items = await _xml2Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber,
//                //    filter, sortFuncList);

//                //Linhist changed 24/11/22
//                //var items1 = await _xml2Repository.GetListAsync(x => x.XML1ID== maxml1);
//                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                //Linhist changed 24/11/22
//                //var items1 = await _xml3Repository.GetListAsync(x=> x.XML1ID.Equals(maxml1));
//                Expression<Func<XML2, bool>> filter = PredicateBuilder.New<XML2>(x => x.XML1ID.Equals(xml1Id) && x.MA_CSKCB == currentMaCSKCB);

//                if (!string.IsNullOrEmpty(request.SearchKey))
//                {
//                    filter = filter.And(x => x.MA_THUOC.Contains(request.SearchKey));
//                }
//                var items1 = await _xml2Repository.GetListAsync(filter);

//                //Linhist changed 09/10/22 them order by theo STT
//                //var itemInfo = _mapper.Map<List<XML2InfoModel>>(items1?.ToList());
//                var itemInfo = _mapper.Map<List<XML2InfoModel>>(items1?.ToList()).OrderBy(x => x.Stt);

//                res.Data.Items = itemInfo;
//                res.Data.TotalItems = itemInfo.Count();
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                    ex, new PaginationResponseModel<XML2InfoModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml2Items)}");
//            }
//            return res;
//        }
//        #endregion

//        #region XML 3
//        public async Task<ResponseModel<PaginationResponseModel<XML3InfoModel>>> GetXml3Items(PaginationRequestModel request, int xml1Id)
//        {
//            var res = new ResponseModel<PaginationResponseModel<XML3InfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<XML3InfoModel>());
//            Int32 maxml1 = xml1Id;
//            try
//            {
//                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

//                #region Validation
//                if (string.IsNullOrWhiteSpace(currentRole))
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                #endregion

//                #region Build Filter Query
//                //Expression<Func<XML3, bool>> filter = PredicateBuilder.New<XML3>(x => x.XML1ID.Equals(xml1Id));
//                //// add filter when has searchKey
//                //if (!string.IsNullOrWhiteSpace(request.SearchKey))
//                //{
//                //    var key = request.SearchKey.ToLower();
//                //    Expression<Func<XML3, bool>> filterSearch = PredicateBuilder.New<XML3>();
//                //    foreach (PropertyInfo propertyInfo in new XML3().GetType().GetProperties())
//                //    {
//                //        if (propertyInfo.Name == nameof(XML3InfoModel.XML1ID) || propertyInfo.Name == nameof(XML3InfoModel.XML3ID)) continue;

//                //        filterSearch = PredicateExpresion<XML3>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
//                //    }
//                //    filter = filter.And(filterSearch);
//                //}
//                #endregion

//                #region Build Sort Query
//                //IList<Func<IOrderedQueryable<XML3>, IOrderedQueryable<XML3>>> sortFuncList = new List<Func<IOrderedQueryable<XML3>, IOrderedQueryable<XML3>>>();
//                //var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
//                //// dynamic sort by column
//                //if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
//                //{
//                //    switch (columnName)
//                //    {
//                //        default:
//                //            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
//                //            break;
//                //    }
//                //}
//                #endregion

//                #region Get Data
//                //PaginationResponseModel<XML3> items = await _xml3Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber,
//                //    filter, sortFuncList);
//                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                //Linhist changed 24/11/22
//                //var items1 = await _xml3Repository.GetListAsync(x=> x.XML1ID.Equals(maxml1));
//                Expression<Func<XML3, bool>> filter = PredicateBuilder.New<XML3>(x => x.XML1ID.Equals(xml1Id) && x.MA_CSKCB == currentMaCSKCB);
//                if (!string.IsNullOrEmpty(request.SearchKey))
//                {
//                    filter = filter.And(x => x.MA_DICH_VU.Contains(request.SearchKey));
//                }
//                var items1 = await _xml3Repository.GetListAsync(filter);

//                //Linhist changed 09/10/22 them order by theo STT
//                //var itemInfo = _mapper.Map<List<XML3InfoModel>>(items1?.ToList());
//                var itemInfo = _mapper.Map<List<XML3InfoModel>>(items1?.ToList()).OrderBy(x => x.Stt);

//                res.Data.Items = itemInfo;
//                res.Data.TotalItems = itemInfo.Count();
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                    ex, new PaginationResponseModel<XML3InfoModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml3Items)}");
//            }
//            return res;
//        }
//        #endregion

//        #region XML 4
//        public async Task<ResponseModel<PaginationResponseModel<XML4InfoModel>>> GetXml4Items(PaginationRequestModel request, int xml1Id)
//        {
//            var res = new ResponseModel<PaginationResponseModel<XML4InfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<XML4InfoModel>());

//            try
//            {
//                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

//                #region Validation
//                if (string.IsNullOrWhiteSpace(currentRole))
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                #endregion

//                #region Build Filter Query
//                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                //Linhist changed 24/11/22
//                //var items1 = await _xml3Repository.GetListAsync(x=> x.XML1ID.Equals(maxml1));
//                Expression<Func<XML4, bool>> filter = PredicateBuilder.New<XML4>(x => x.XML1ID.Equals(xml1Id) && x.MA_CSKCB == currentMaCSKCB);
//                // add filter when has searchKey
//                if (!string.IsNullOrWhiteSpace(request.SearchKey))
//                {
//                    var key = request.SearchKey.ToLower();
//                    Expression<Func<XML4, bool>> filterSearch = PredicateBuilder.New<XML4>();
//                    foreach (PropertyInfo propertyInfo in new XML4().GetType().GetProperties())
//                    {
//                        if (propertyInfo.Name == nameof(XML4InfoModel.XML1ID) || propertyInfo.Name == nameof(XML4InfoModel.XML4ID)) continue;

//                        filterSearch = PredicateExpresion<XML4>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
//                    }
//                    filter = filter.And(filterSearch);
//                }
//                #endregion

//                #region Build Sort Query
//                IList<Func<IOrderedQueryable<XML4>, IOrderedQueryable<XML4>>> sortFuncList = new List<Func<IOrderedQueryable<XML4>, IOrderedQueryable<XML4>>>();
//                var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
//                // dynamic sort by column
//                if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
//                {
//                    switch (columnName)
//                    {
//                        default:
//                            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
//                            break;
//                    }
//                }
//                #endregion

//                #region Get Data
//                PaginationResponseModel<XML4> items = await _xml4Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber,
//                    filter, sortFuncList);

//                //Linhist changed 09/10/22 them order by theo STT
//                //var itemInfo = _mapper.Map<List<XML4InfoModel>>(items?.Items?.ToList());
//                var itemInfo = _mapper.Map<List<XML4InfoModel>>(items?.Items?.ToList()).OrderBy(x => x.Stt);

//                res.Data.Items = itemInfo;
//                res.Data.TotalItems = items.TotalItems;
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                    ex, new PaginationResponseModel<XML4InfoModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml4Items)}");
//            }
//            return res;
//        }
//        #endregion

//        #region XML 5
//        public async Task<ResponseModel<PaginationResponseModel<XML5InfoModel>>> GetXml5Items(PaginationRequestModel request, int xml1Id)
//        {
//            var res = new ResponseModel<PaginationResponseModel<XML5InfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<XML5InfoModel>());

//            try
//            {
//                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

//                #region Validation
//                if (string.IsNullOrWhiteSpace(currentRole))
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                #endregion

//                #region Build Filter Query
//                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                //Linhist changed 24/11/22
//                //var items1 = await _xml3Repository.GetListAsync(x=> x.XML1ID.Equals(maxml1));
//                Expression<Func<XML5, bool>> filter = PredicateBuilder.New<XML5>(x => x.XML1ID.Equals(xml1Id) && x.MA_CSKCB == currentMaCSKCB);
//                // add filter when has searchKey
//                if (!string.IsNullOrWhiteSpace(request.SearchKey))
//                {
//                    var key = request.SearchKey.ToLower();
//                    Expression<Func<XML5, bool>> filterSearch = PredicateBuilder.New<XML5>();
//                    foreach (PropertyInfo propertyInfo in new XML5().GetType().GetProperties())
//                    {
//                        if (propertyInfo.Name == nameof(XML5InfoModel.XML1ID) || propertyInfo.Name == nameof(XML5InfoModel.XML5ID)) continue;

//                        filterSearch = PredicateExpresion<XML5>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
//                    }
//                    filter = filter.And(filterSearch);
//                }
//                #endregion

//                #region Build Sort Query
//                IList<Func<IOrderedQueryable<XML5>, IOrderedQueryable<XML5>>> sortFuncList = new List<Func<IOrderedQueryable<XML5>, IOrderedQueryable<XML5>>>();
//                var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
//                // dynamic sort by column
//                if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
//                {
//                    switch (columnName)
//                    {
//                        default:
//                            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
//                            break;
//                    }
//                }
//                #endregion

//                #region Get Data
//                PaginationResponseModel<XML5> items = await _xml5Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber,
//                    filter, sortFuncList);

//                //Linhist changed 09/10/22 them order by theo STT
//                //var itemInfo = _mapper.Map<List<XML5InfoModel>>(items?.Items?.ToList());
//                var itemInfo = _mapper.Map<List<XML5InfoModel>>(items?.Items?.ToList()).OrderBy(x => x.Stt);

//                res.Data.Items = itemInfo;
//                res.Data.TotalItems = items.TotalItems;
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                    ex, new PaginationResponseModel<XML5InfoModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml5Items)}");
//            }
//            return res;
//        }
//        #endregion

//        #region XML 1 Error
//        public async Task RecheckXML1(int xml1Id)
//        {
//            await _hosoService.ValidateFiles(xml1Ids: xml1Id.ToString());
//        }
//        public async Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorItems(int xml1Id)
//        {
//            var res = new ResponseModel<IEnumerable<XML1ErrorInfoModel>>(HttpStatusCode.OK, "", new List<XML1ErrorInfoModel>());

//            try
//            {
//                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

//                #region Validation
//                if (string.IsNullOrWhiteSpace(currentRole))
//                {
//                    res.Code = HttpStatusCode.Forbidden;
//                    res.Message = MessageResource.ERR_403_FORBIDDEN;
//                    return res;
//                }
//                #endregion

//                Expression<Func<XML1Error, bool>> filter = PredicateBuilder.New<XML1Error>();
//                #region Get Data
//                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                //Linhist changed 24/11/22
//                //var items1 = await _xml3Repository.GetListAsync(x=> x.XML1ID.Equals(maxml1));
//                //Expression<Func<XML3, bool>> filter = PredicateBuilder.New<XML3>(x => x.XML1ID.Equals(xml1Id) && x.MA_CSKCB == currentMaCSKCB);

//                IEnumerable<XML1Error> items = await _xml1ErrorRepository.GetListAsync(
//                    x => x.XEXML1ID.Equals(xml1Id) && x.MA_CSKCB == currentMaCSKCB, x => x.OrderBy(o => o.XEXML1ID), null);

//                var itemInfo = _mapper.Map<List<XML1ErrorInfoModel>>(items?.ToList());


//                foreach (XML1ErrorInfoModel item in itemInfo)
//                {
//                    DMError listDmError = await _dMErrorRepository.GetItem(x => x.ErrorCode == item.XeErrorCode);
//                    item.ErrorAtXMLFile = listDmError.ErrorAtXMLFile;
//                    item.ErrorAtXMLTag = listDmError.ErrorAtXMLTag;
//                    item.ErrorContent = listDmError.ErrorContent;
//                }

//                res.Data = itemInfo;
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<IEnumerable<XML1ErrorInfoModel>>(
//                    ex, new List<XML1ErrorInfoModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml1ErrorItems)}");
//            }
//            return res;
//        }
//        public async Task<ResponseModel<IEnumerable<XML1ErrorInfoModelAPI>>> GetXml1ErrorItemsByFileName(List<string> fileNames, string maCSKCB)
//        {
//            var res = new ResponseModel<IEnumerable<XML1ErrorInfoModelAPI>>(HttpStatusCode.OK, "", new List<XML1ErrorInfoModelAPI>());

//            try
//            {
//                //var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                Expression<Func<XML1Error, bool>> filter = PredicateBuilder.New<XML1Error>();
//                #region Get Data
//                List<XML1> itemsXML1 = new List<XML1>();
//                foreach (string fileName in fileNames)
//                {
//                    var file = await _xml1Repository.GetItemOrderBy(x => x.FileName == fileName && x.MA_CSKCB == maCSKCB
//                    , x => x.OrderByDescending(o => o.XML1ID), i => i.Include(i1 => i1.XML2s).Include(i3 => i3.XML3s));


//                    if (file != null) itemsXML1.Add(file);


//                }
//                Uti.HandleCronJobLog(_logger, $"Check error API ({fileNames}) - Macskcb: {maCSKCB}", nameof(FilesService.GetXml1ErrorItemsByFileName));

//                if (itemsXML1 != null && itemsXML1.Any())
//                {

//                    //IEnumerable<XML1> itemsXML1 = (await _xml1Repository.GetListAsync(
//                    //   x => fileNames.Contains(x.FileName), x => x.OrderBy(o => o.XML1ID), i => i.Include(i1 => i1.XML2s).Include(i3 => i3.XML3s)));

//                    var xml1Ids = itemsXML1?.Select(x => x.XML1ID);
//                    var items = await _xml1ErrorRepository.GetListAsync(x => xml1Ids.Contains(x.XEXML1ID.Value) && x.MA_CSKCB == maCSKCB);

//                    IEnumerable<DMThuoc> _DMThuoc = new List<DMThuoc>();
//                    IEnumerable<DMVTYT> _DMVTYT = new List<DMVTYT>();
//                    IEnumerable<DMDichVu> _DMDichVu = new List<DMDichVu>();

//                    List<string> allMaThuoc = itemsXML1.SelectMany(y => y.XML2s.Select(z => z.MA_THUOC)).ToList();
//                    List<string> allMaVTYT = itemsXML1.SelectMany(y => y.XML3s.Select(z => z.MA_VAT_TU)).ToList();
//                    List<string> allMaDicVu = itemsXML1.SelectMany(y => y.XML3s.Select(z => z.MA_DICH_VU)).ToList();//file

//                    _DMThuoc = await _dmThuoc.GetListAsync(x => allMaThuoc.Contains(x.MA_THUOC) && x.MA_CSKCB == maCSKCB);
//                    _DMVTYT = await _dmVTYT.GetListAsync(x => allMaVTYT.Contains(x.MA_VAT_TU) && x.MA_CSKCB == maCSKCB);
//                    _DMDichVu = await _dmDichVu.GetListAsync(x => allMaDicVu.Contains(x.MA_DICH_VU) && x.MA_CSKCB == maCSKCB);//DB

//                    var itemInfo = _mapper.Map<List<XML1ErrorInfoModelAPI>>(items?.ToList());
//                    var allErrorCodes = itemInfo.Select(x => x.XeErrorCode).ToList();

//                    var listDmErrorAll = await _dMErrorRepository.GetListAsync(x => allErrorCodes.Contains(x.ErrorCode));

//                    foreach (XML1ErrorInfoModelAPI item in itemInfo)
//                    {
//                        DMError listDmError = listDmErrorAll.Where(x => x.ErrorCode == item.XeErrorCode).FirstOrDefault();
//                        item.ErrorAtXMLFile = listDmError.ErrorAtXMLFile;
//                        item.ErrorAtXMLTag = listDmError.ErrorAtXMLTag;
//                        item.ErrorContent = listDmError.ErrorContent;
//                        item.FileName = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID)?.FirstOrDefault().FileName;
//                        if (listDmError.ErrorAtXMLFile == "XML2")
//                        {
//                            item.Ten_AX = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML2s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().TEN_THUOC;
//                            item.Ma_AX = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML2s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_THUOC;
//                            item.CCHN = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML2s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_BAC_SI;
//                            item.DMThuoc = _DMThuoc.Where(x => x.MA_THUOC == item.Ma_AX).FirstOrDefault();
//                        }

//                        if (listDmError.ErrorAtXMLFile == "XML3")
//                        {
//                            item.Ten_AX = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML3s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().TEN_DICH_VU;
//                            item.Ma_AX = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML3s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_DICH_VU;
//                            item.DMDichVu = _DMDichVu.Where(x => x.MA_DICH_VU == item.Ma_AX).FirstOrDefault();

//                            if (item.Ma_AX == null || item.Ma_AX.Length == 0)
//                            {
//                                item.Ten_AX = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML3s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().TEN_VAT_TU;
//                                item.Ma_AX = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML3s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_VAT_TU;

//                                item.DMVTYT = _DMVTYT.Where(x => x.MA_VAT_TU == item.Ma_AX).FirstOrDefault();
//                            }

//                            item.CCHN = itemsXML1.Where(x => x.XML1ID == item.XEXML1ID).SelectMany(y => y.XML3s).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_BAC_SI;
//                        }
//                    }

//                    res.Data = itemInfo;
//                    #endregion

//                    // write log to db
//                    await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//                }
//                else
//                {
//                    return res;
//                }
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<IEnumerable<XML1ErrorInfoModelAPI>>(
//                    ex, new List<XML1ErrorInfoModelAPI>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml1ErrorItems)}");
//            }
//            return res;
//        }



//        public async Task<ResponseModel<IEnumerable<XML1ErrorInfoModelAPI>>> GetXml1ErrorItemsByFileNameAPI(string fileNames)
//        {
//            var res = new ResponseModel<IEnumerable<XML1ErrorInfoModelAPI>>(HttpStatusCode.OK, "", new List<XML1ErrorInfoModelAPI>());

//            try
//            {
//                //_xml1APIRepository;
//                //_xml1Error_APIRepository


//                Expression<Func<XML1Error_API, bool>> filter = PredicateBuilder.New<XML1Error_API>();
//                #region Get Data
//                //Linhist changed 27/02/23
//                //IEnumerable<XML1_API> itemsXML1 = (await _xml1APIRepository.GetListAsync(
//                //   x => fileNames == x.FileName, x => x.OrderBy(o => o.CreateDate), i => i.Include(i1 => i1.XML2_APIs).Include(i3 => i3.XML3_APIs).Include(i4 => i4.XML1Error_APIs)));

//                IEnumerable<XML1_API> itemsXML1 = (await _xml1APIRepository.GetListAsync(
//                   x => fileNames == x.FileName, x => x.OrderBy(o => o.CreateDate), i => i.Include(i1 => i1.XML2_APIs).Include(i3 => i3.XML3_APIs).Include(i4 => i4.XML1Error_APIs)));

//                //var xml1Ids = itemsXML1?.Select(x => x.XML1_APIID);
//                var items = itemsXML1.FirstOrDefault().XML1Error_APIs;

//                IEnumerable<DMThuoc> _DMThuoc = new List<DMThuoc>();
//                IEnumerable<DMVTYT> _DMVTYT = new List<DMVTYT>();
//                IEnumerable<DMDichVu> _DMDichVu = new List<DMDichVu>();

//                List<string> allMaThuoc = itemsXML1.SelectMany(y => y.XML2_APIs.Select(z => z.MA_THUOC)).ToList();
//                List<string> allMaVTYT = itemsXML1.SelectMany(y => y.XML3_APIs.Select(z => z.MA_VAT_TU)).ToList();
//                List<string> allMaDicVu = itemsXML1.SelectMany(y => y.XML3_APIs.Select(z => z.MA_DICH_VU)).ToList();//file

//                _DMThuoc = await _dmThuoc.GetListAsync(x => allMaThuoc.Contains(x.MA_THUOC) && x.MA_CSKCB == itemsXML1.FirstOrDefault().MA_CSKCB);
//                _DMVTYT = await _dmVTYT.GetListAsync(x => allMaVTYT.Contains(x.MA_VAT_TU) && x.MA_CSKCB == itemsXML1.FirstOrDefault().MA_CSKCB);
//                _DMDichVu = await _dmDichVu.GetListAsync(x => allMaDicVu.Contains(x.MA_DICH_VU) && x.MA_CSKCB == itemsXML1.FirstOrDefault().MA_CSKCB);//DB


//                var itemInfo = _mapper.Map<List<XML1ErrorInfoModelAPI>>(items?.ToList());


//                foreach (XML1ErrorInfoModelAPI item in itemInfo)
//                {
//                    DMError listDmError = await _dMErrorRepository.GetItem(x => x.ErrorCode == item.XeErrorCode);
//                    item.ErrorAtXMLFile = listDmError.ErrorAtXMLFile;
//                    item.ErrorAtXMLTag = listDmError.ErrorAtXMLTag;
//                    item.ErrorContent = listDmError.ErrorContent;
//                    item.FileName = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID)?.FirstOrDefault().FileName;
//                    if (listDmError.ErrorAtXMLFile == "XML2")
//                    {
//                        item.Ten_AX = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML2_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().TEN_THUOC;
//                        item.Ma_AX = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML2_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_THUOC;
//                        item.CCHN = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML2_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_BAC_SI;
//                        item.DMThuoc = _DMThuoc.Where(x => x.MA_THUOC == item.Ma_AX).FirstOrDefault();
//                    }

//                    if (listDmError.ErrorAtXMLFile == "XML3")
//                    {
//                        item.Ten_AX = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML3_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().TEN_DICH_VU;
//                        item.Ma_AX = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML3_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_DICH_VU;
//                        item.DMDichVu = _DMDichVu.Where(x => x.MA_DICH_VU == item.Ma_AX).FirstOrDefault();

//                        if (item.Ma_AX == null || item.Ma_AX.Length == 0)
//                        {
//                            item.Ten_AX = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML3_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().TEN_VAT_TU;
//                            item.Ma_AX = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML3_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_VAT_TU;
//                            item.DMVTYT = _DMVTYT.Where(x => x.MA_VAT_TU == item.Ma_AX).FirstOrDefault();
//                        }

//                        item.CCHN = itemsXML1.Where(x => x.XML1_APIID == item.XEXML1ID).SelectMany(y => y.XML3_APIs).Where(z => z.STT == item.XeErrorRow).FirstOrDefault().MA_BAC_SI;
//                    }
//                }

//                res.Data = itemInfo;
//                #endregion

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError<IEnumerable<XML1ErrorInfoModelAPI>>(
//                    ex, new List<XML1ErrorInfoModelAPI>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml1ErrorItems)}");
//            }
//            return res;
//        }
//        #endregion

//        #region Linhist added 09/06/22

//        public async Task<int> SetXml1Recheck(FilesPaginationRequest request)
//        {
//            var res = new ResponseModel<PaginationResponseModel<XML1InfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<XML1InfoModel>());

//            try
//            {
//                var filterModel = _mapper.Map<FilterDownloadModel>(request);
//                var queryable = BuildQueryToGetListData(filterModel, false);
//                if (queryable == null) return 0;

//                var items = await _xml1Repository.GetListAsync(queryable.FilterQueryable, null, null);
//                foreach (XML1 xml1 in items)
//                {
//                    xml1.CHECKED = FileStatusVerification.RECALL_VALIDATE_CHECK;
//                }

//                await _xml1Repository.UpdateListAsync(items);
//                await _xml1Repository.SaveChangesAsync();

//                // write log to db
//                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_UPDATE_STATUS);
//                return items.Count();
//            }
//            catch (Exception ex)
//            {
//                res = Uti.HandleInternalServerError(
//                    ex, new PaginationResponseModel<XML1InfoModel>(), _logger, $"{nameof(FilesService)} - {nameof(FilesService.GetXml1Items)}");
//            }
//            return 0;
//        }
//        #endregion

//        static string GetXMLErrorAtRow(string xml, Exception e)
//        {
//            try
//            {
//                int result;
//                int.TryParse(e.Message.Substring(35, 2), out result);
//                //int possition = int.Parse(e.Message.Substring(37, 2));
//                string error = "Lỗi dòng " + result.ToString();// + "   Vị trí lỗi: " + possition.ToString() + "     ";
//                List<string> foundValues4 = new List<string>();
//                XmlTextReader xmlReader = new XmlTextReader(new System.IO.StringReader(xml.Substring(xml.IndexOf(Environment.NewLine))));
//                while (xmlReader.Read())
//                {
//                    if (xmlReader.LineNumber == result)
//                    {
//                        switch (xmlReader.NodeType)
//                        {

//                            case XmlNodeType.Element:
//                                error += " Name: " + xmlReader.Name;
//                                break;
//                            case XmlNodeType.Text:
//                                error += " Value: " + xmlReader.Value;
//                                break;
//                            case XmlNodeType.EndElement:
//                                foundValues4.Add("");
//                                break;
//                        }
//                    }
//                }
//                // Close the reader.
//                xmlReader.Close();
//                return error;
//            }
//            catch
//            {
//                return "Lỗi đọc file.";
//            }
//        }
//        public static Encoding GetEncodingHT(byte[] bom)
//        {
//            // Analyze the BOM

//            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
//            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
//            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
//            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
//            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

//            // We actually have no idea what the encoding is if we reach this point, so
//            // you may wish to return null instead of defaulting to ASCII
//            return Encoding.ASCII;
//        }
//        private string fullPath = "E:/";

//        private string fullPathTempFolder = "E:/";

//        public async Task FileUploadAsycn(FileUpload fileUpload, string hospitalIdAPI = "", string hospitalNameAPI = "", string userIdAPI = "", string MA_CSKCB_API = "")
//        {
//            var khoi = await FileUploadAPI(fileUpload, hospitalIdAPI, hospitalNameAPI, userIdAPI, MA_CSKCB_API);

//        }

//        private const bool isUploadToTemFolder = false;

//        public async Task<ResponseModel<dynamic>> FileCheckAPI(XML1_API ListCheckAPI)
//        {

//            await _hosoService.CreateNewHoSoAPI(ListCheckAPI);
//            return new ResponseModel<dynamic> { Code = System.Net.HttpStatusCode.OK, Data = new { successMessage = "Upload oke !" }, Message = "" };

//        }


//        public async Task<ResponseModel<dynamic>> FileUploadAPI(FileUpload fileUpload, string hospitalIdAPI = "", string hospitalNameAPI = "", string userIdAPI = "", string MA_CSKCB_API = "", bool isFromHIS = false)
//        {
//            return new ResponseModel<dynamic>();
//            //Linhist added 24/03/22 --> sua loi upload duoc file nhung ko insert duoc vao DB
//            //Do DateTime.MinValue (01/01/0001) < SQL_DATETIME MIN VALUE (01/01/1753)
//            DateTime dMinSql = new DateTime(1753, 1, 1);

//            DateTime startTime = System.DateTime.Now;
//            //var hospitalName = "";
//            if (hospitalNameAPI.Length == 0)
//                hospitalNameAPI = _httpContext.HttpContext.User.Claims.First(claim => claim.Type == "HospitalName").Value ?? "";

//            if (MA_CSKCB_API.Length == 0)
//                MA_CSKCB_API = _httpContext.HttpContext.User.Claims.First(claim => claim.Type == "MA_CSKCB").Value ?? "";

//            //Linhist added 15/06/22 chan upload tu 19012
//            //if (MA_CSKCB_API == "19012") return null;

//            //Linhist changed 26/04/22 thay doi ten thu muc goc XML thanh MA_CSKCB
//            //fullPath = Uti.GenerateFilePath(hospitalNameAPI, DateTime.Now);
//            fullPath = Uti.GenerateFilePath(MA_CSKCB_API, DateTime.Now);

//            if (isUploadToTemFolder)
//            {
//                fullPathTempFolder = Uti.GenerateFilePathTemp(MA_CSKCB_API);
//                if (Directory.Exists(fullPathTempFolder))
//                {
//                    //Directory.Delete(fullPath, true);
//                }

//                if (!Directory.Exists(fullPathTempFolder))
//                {
//                    Directory.CreateDirectory(fullPathTempFolder);
//                }
//            }

//            if (Directory.Exists(fullPath))
//            {
//                //Directory.Delete(fullPath, true);
//            }

//            if (!Directory.Exists(fullPath))
//            {
//                Directory.CreateDirectory(fullPath);
//            }


//            List<XML1> finalXML1List = new List<XML1>();
//            int totalFile = fileUpload.FormFiles.Count;
//            int failed = 0;
//            int success = 0;
//            var successMessage = "Hồ sơ đã tải lên: <br/>";
//            var failedMessage = "<b>Hồ sơ tải lỗi: </b><br/>";
//            List<string> failedFiles = new List<string>();
//            var uploadStatus = "";
//            var uploadTime = "";
//            foreach (var aformFile in fileUpload.FormFiles)
//            {
//                var fileExtension = Path.GetExtension(aformFile.FileName);
//                if (aformFile != null && fileExtension.ToLower() == ".xml")
//                {
//                    string result = string.Empty;
//                    //haimaster comment 3/12/2021 detect encoding
//                    using (BinaryReader b = new BinaryReader(aformFile.OpenReadStream()))
//                    {
//                        byte[] binData = b.ReadBytes((int)aformFile.Length);
//                        if (GetEncodingHT(binData) == Encoding.UTF8)
//                        {
//                            result = System.Text.Encoding.Default.GetString(binData);
//                        }
//                        else if (GetEncodingHT(binData) == Encoding.Unicode)
//                        {
//                            result = System.Text.Encoding.Unicode.GetString(binData);
//                        }
//                        else
//                        {
//                            result = System.Text.Encoding.ASCII.GetString(binData);
//                        }

//                    }

//                    int iCnt = 0;

//                    try
//                    {
//                        var catalog1 = result.ParseXML<GIAMDINHHS>();


//                        //List<GIAMDINHHSTHONGTINHOSODANHSACHHOSOFILEHOSO> allHoso = catalog1.THONGTINHOSO.DANHSACHHOSO.HOSO.ToList();
//                        foreach (var hosoNho in catalog1.THONGTINHOSO.DANHSACHHOSO)
//                        {

//                            List<GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO> allHoso = hosoNho.ToList();
//                            XML1 newXML1 = new XML1();
//                            foreach (var hosocon in allHoso)
//                            {
//                                iCnt++;
//                                string caseSwitch = hosocon.LOAIHOSO;

//                                switch (caseSwitch)
//                                {
//                                    case "XML1":
//                                        //Linhist added 09/12/22
//                                        TONG_HOP xml1 = null;// Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<TONG_HOP>();
//                                        try
//                                        {
//                                            xml1 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE.Trim())).ParseXML<TONG_HOP>();
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} XML 1 ({Path.GetFileName(aformFile.FileName)}): Lỗi không đọc được file (do sai định dạng BASE64)<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML1 ({aformFile.FileName})");

//                                            //Truong hop loi XML1 phai bo qua het cac XMl2,3,4,5 cua XML1 nay
//                                            break;
//                                        }

//                                        try
//                                        {
//                                            //string khoi = Encoding.UTF8.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE));
//                                            //TONG_HOP xml1 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<TONG_HOP>();
//                                            //Mapper.Initialize(config => config.CreateMap<Source, Destination>()
//                                            //.ForMember(dest => dest.B, option => option.Ignore()));
//                                            newXML1.HO_TEN = xml1.HO_TEN;
//                                            newXML1.MA_THE = xml1.MA_THE;

//                                            //haimaster comment 2/12/2021
//                                            if (xml1.CAN_NANG.ToString().Trim().Length > 0) { newXML1.CAN_NANG = double.Parse(xml1.CAN_NANG); } else { newXML1.CAN_NANG = 0; };
//                                            newXML1.DIA_CHI = xml1.DIA_CHI;
//                                            newXML1.GIOI_TINH = xml1.GIOI_TINH;
//                                            newXML1.GT_THE_DEN = xml1.GT_THE_DEN;
//                                            newXML1.GT_THE_TU = xml1.GT_THE_TU;
//                                            newXML1.KET_QUA_DTRI = xml1.KET_QUA_DTRI;
//                                            newXML1.MA_BENH = xml1.MA_BENH;
//                                            newXML1.MA_BENHKHAC = xml1.MA_BENHKHAC;
//                                            newXML1.MA_BN = xml1.MA_BN;
//                                            newXML1.MA_CSKCB = xml1.MA_CSKCB;
//                                            newXML1.MA_DKBD = xml1.MA_DKBD;
//                                            newXML1.MA_KHOA = xml1.MA_KHOA;
//                                            newXML1.MA_KHUVUC = xml1.MA_KHUVUC;
//                                            newXML1.MA_LK = xml1.MA_LK;
//                                            newXML1.MA_LOAI_KCB = xml1.MA_LOAI_KCB;
//                                            if (xml1.MA_LYDO_VVIEN.ToString().Trim().Length > 0) { newXML1.MA_LYDO_VVIEN = (byte)int.Parse(xml1.MA_LYDO_VVIEN); } else { newXML1.MA_LYDO_VVIEN = 0; }
//                                            newXML1.MA_NOI_CHUYEN = xml1.MA_NOI_CHUYEN;
//                                            newXML1.MA_PTTT_QT = xml1.MA_PTTT_QT;
//                                            if (xml1.MA_TAI_NAN.Length > 0) newXML1.MA_TAI_NAN = (byte)int.Parse(xml1.MA_TAI_NAN);
//                                            newXML1.MIEN_CUNG_CT = xml1.MIEN_CUNG_CT;
//                                            newXML1.NAM_QT = xml1.NAM_QT;
//                                            //if (xml1.NGAY_RA != null) newXML1.XML1Errors.Add(new XML1Error() { XEErrorCode = 46 });
//                                            newXML1.NGAY_RA = DateTime.ParseExact(xml1.NGAY_RA, "yyyyMMddHHmm", null);
//                                            if (xml1.NGAY_SINH.ToString().Length != 4)
//                                            {
//                                                newXML1.NGAY_SINH = DateTime.ParseExact(xml1.NGAY_SINH, "yyyyMMdd", null);
//                                            }
//                                            else { newXML1.NGAY_SINH = DateTime.ParseExact(xml1.NGAY_SINH.ToString() + "0101", "yyyyMMdd", null); }
//                                            if (xml1.NGAY_TTOAN.ToString().Length == 0)
//                                            {
//                                                newXML1.NGAY_TTOAN = DateTime.ParseExact(xml1.NGAY_RA, "yyyyMMddHHmm", null);
//                                            }
//                                            else if (xml1.NGAY_TTOAN.ToString().Length != 12)
//                                            {
//                                                //haimaster comment 5/1/2022 Thoong bao loi ngay Thanh Toans sai dinh dangj => Break ko doc files nay
//                                                failedMessage = "Code 01 XML 1: " + Path.GetFileName(aformFile.FileName) + " Lỗi: NGAY_TTOAN không đủ 12 ký tự   <br/> ";
//                                            }
//                                            else if (xml1.NGAY_TTOAN.ToString() != "000101010000")
//                                            {
//                                                newXML1.NGAY_TTOAN = DateTime.ParseExact(xml1.NGAY_TTOAN, "yyyyMMddHHmm", null);
//                                            }
//                                            //if (xml1.NGAY_VAO != null) newXML1.XML1Errors.Add(new XML1Error() { XEErrorCode = 43 });
//                                            newXML1.NGAY_VAO = DateTime.ParseExact(xml1.NGAY_VAO, "yyyyMMddHHmm", null);
//                                            newXML1.SO_NGAY_DTRI = xml1.SO_NGAY_DTRI;
//                                            newXML1.STT = xml1.STT;
//                                            newXML1.TEN_BENH = xml1.TEN_BENH;
//                                            newXML1.THANG_QT = xml1.THANG_QT;
//                                            newXML1.TINH_TRANG_RV = xml1.TINH_TRANG_RV;
//                                            //newXML1.T_BHTT = xml1.T_BHTT;
//                                            if (xml1.T_BHTT?.Length > 0) { newXML1.T_BHTT = double.Parse(xml1.T_BHTT); } else { newXML1.T_BHTT = 0; }

//                                            //newXML1.T_BNCCT = xml1.T_BNCCT;
//                                            if (xml1.T_BNCCT?.Length > 0) { newXML1.T_BNCCT = double.Parse(xml1.T_BNCCT); } else { newXML1.T_BNCCT = 0; }

//                                            //newXML1.T_BNTT = xml1.T_BNTT;
//                                            if (xml1.T_BNTT?.Length > 0) { newXML1.T_BNTT = double.Parse(xml1.T_BNTT); } else { newXML1.T_BNTT = 0; }

//                                            //newXML1.T_VTYT = xml1.T_VTYT;
//                                            if (xml1.T_VTYT?.Length > 0) { newXML1.T_VTYT = double.Parse(xml1.T_VTYT); } else { newXML1.T_VTYT = 0; }

//                                            //newXML1.T_TONGCHI = xml1.T_TONGCHI;
//                                            if (xml1.T_TONGCHI?.Length > 0) { newXML1.T_TONGCHI = double.Parse(xml1.T_TONGCHI); } else { newXML1.T_TONGCHI = 0; }

//                                            //newXML1.T_THUOC = xml1.T_THUOC;
//                                            if (xml1.T_THUOC?.Length > 0) { newXML1.T_THUOC = double.Parse(xml1.T_THUOC); } else { newXML1.T_THUOC = 0; }

//                                            //newXML1.T_NGUONKHAC = xml1.T_NGUONKHAC;
//                                            if (xml1.T_NGUONKHAC?.Length > 0) { newXML1.T_NGUONKHAC = double.Parse(xml1.T_NGUONKHAC); } else { newXML1.T_NGUONKHAC = 0; }

//                                            if (xml1.T_NGOAIDS.Length > 0) { newXML1.T_NGOAIDS = double.Parse(xml1.T_NGOAIDS); } else { newXML1.T_NGOAIDS = 0; }
//                                            newXML1.FileName = Uti.ToFilename(aformFile.FileName);
//                                            newXML1.MIEN_CUNG_CT = xml1.MIEN_CUNG_CT;
//                                            newXML1.MA_PTTT_QT = xml1.MA_PTTT_QT;
//                                            newXML1.NGAYLAP = DateTime.Now;
//                                            //newXML1.MHTCID = xml1.MHTCID;
//                                            //newXML1.MA_NOI_DEN = xml1.MA_NOI_DEN;
//                                            //newXML1.MA_NOI_DI = xml1.MA_NOI_DI;
//                                            //newXML1.MHTCID = xml1.mh


//                                            //Linhist added 24/03/22 --> sua loi upload duoc file nhung ko insert duoc vao DB
//                                            if (newXML1.NGAY_RA <= dMinSql) newXML1.NGAY_RA = null;
//                                            if (newXML1.NGAY_VAO <= dMinSql) newXML1.NGAY_VAO = null;
//                                            if (newXML1.NGAY_SINH <= dMinSql) newXML1.NGAY_SINH = null;
//                                            if (newXML1.NGAY_TTOAN <= dMinSql) newXML1.NGAY_TTOAN = null;

//                                            success++;
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
//                                            failedMessage += "XML 1: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML1 ({aformFile.FileName})");
//                                        }
//                                        //finally
//                                        //{
//                                        //    //call this if exception occurs or not
//                                        //    //in this example, dispose the WebClient
//                                        //}
//                                        break;
//                                    case "XML2":
//                                        //Linhist added 09/12/22
//                                        DSACH_CHI_TIET_THUOC xml2 = null;//Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_THUOC>();
//                                        try
//                                        {
//                                            xml2 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE.Trim())).Replace("\n", "").ParseXML<DSACH_CHI_TIET_THUOC>();
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} XML 2 ({Path.GetFileName(aformFile.FileName)}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN}): Lỗi không đọc được file (do sai định dạng BASE64)<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML2 ({aformFile.FileName}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN})");
//                                            break;
//                                        }

//                                        try
//                                        {
//                                            //XmlDocument doc = new XmlDocument();
//                                            //string ola = Encoding.UTF8.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE));
//                                            ////doc.LoadXml(ola);
//                                            //doc.LoadXml(ola.Substring(ola.IndexOf(Environment.NewLine)));



//                                            //int count1 = 0;
//                                            //int count2 = 0;
//                                            //int count3 = 0;
//                                            //List<string> foundValues1 = new List<string>();
//                                            //List<string> foundValues2 = new List<string>();
//                                            //List<string> foundValues3 = new List<string>();

//                                            //foreach (XmlNode root in doc.ChildNodes)
//                                            //{
//                                            //    foreach (XmlNode recorset in root.ChildNodes)
//                                            //    {
//                                            //        count1++;
//                                            //        foundValues1.Add(recorset.Name);

//                                            //        foreach (XmlNode row in recorset.ChildNodes)
//                                            //        {
//                                            //            count2++;
//                                            //            foundValues2.Add(row.Name);
//                                            //            foreach (XmlNode value in row.ChildNodes)
//                                            //            {
//                                            //                count3++;
//                                            //                foundValues3.Add(value.Name);

//                                            //            }
//                                            //        }
//                                            //    }
//                                            //}
//                                            //DSACH_CHI_TIET_THUOC xml2 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_THUOC>();
//                                            List<XML2> listXML2 = new List<XML2>();
//                                            int countSTT = 1;
//                                            foreach (DSACH_CHI_TIET_THUOCCHI_TIET_THUOC chitiet in xml2.CHI_TIET_THUOC)
//                                            {
//                                                XML2 xml2temp = new XML2();
//                                                if (newXML1 != null)
//                                                {
//                                                    xml2temp.DON_GIA = chitiet.DON_GIA;
//                                                    xml2temp.DON_VI_TINH = chitiet.DON_VI_TINH;
//                                                    xml2temp.DUONG_DUNG = chitiet.DUONG_DUNG;
//                                                    xml2temp.SO_LUONG = chitiet.SO_LUONG;
//                                                    xml2temp.MUC_HUONG = (byte)chitiet.MUC_HUONG;
//                                                    xml2temp.MA_THUOC = chitiet.MA_THUOC;
//                                                    xml2temp.THANH_TIEN = chitiet.THANH_TIEN;
//                                                    //haimaster comment xu ly XML bi loi STT 6/1/2022
//                                                    xml2temp.STT = countSTT;
//                                                    countSTT++;
//                                                    xml2temp.PHAM_VI = chitiet.PHAM_VI;
//                                                    xml2temp.TEN_THUOC = chitiet.TEN_THUOC;
//                                                    xml2temp.TT_THAU = chitiet.TT_THAU;
//                                                    xml2temp.TYLE_TT = (byte)chitiet.TYLE_TT;
//                                                    //xml2temp.T_NGOAIDS = chitiet.T_NGOAIDS;
//                                                    if (chitiet.T_NGOAIDS?.Length > 0) { xml2temp.T_NGOAIDS = double.Parse(chitiet.T_NGOAIDS); } else { xml2temp.T_NGOAIDS = 0; }
//                                                    xml2temp.T_BHTT = chitiet.T_BHTT;
//                                                    xml2temp.T_BNCCT = chitiet.T_BNCCT;
//                                                    //xml2temp.T_NGUONKHAC = chitiet.T_NGUONKHAC;
//                                                    if (chitiet.T_NGUONKHAC?.Length > 0) { xml2temp.T_NGUONKHAC = double.Parse(chitiet.T_NGUONKHAC); } else { xml2temp.T_NGUONKHAC = 0; }

//                                                    xml2temp.MA_NHOM = chitiet.MA_NHOM;
//                                                    xml2temp.T_BNTT = chitiet.T_BNTT;
//                                                    if (chitiet.NGAY_YL.Length == 12) xml2temp.NGAY_YL = DateTime.ParseExact(chitiet.NGAY_YL, "yyyyMMddHHmm", null);
//                                                    xml2temp.MA_PTTT = chitiet.MA_PTTT;
//                                                    xml2temp.LIEU_DUNG = chitiet.LIEU_DUNG;
//                                                    xml2temp.SO_DANG_KY = chitiet.SO_DANG_KY;
//                                                    xml2temp.MA_KHOA = chitiet.MA_KHOA;
//                                                    xml2temp.MA_BAC_SI = chitiet.MA_BAC_SI;
//                                                    xml2temp.MA_BENH = chitiet.MA_BENH;
//                                                    xml2temp.MA_LK = chitiet.MA_LK;
//                                                    xml2temp.HAM_LUONG = chitiet.HAM_LUONG;


//                                                    //Linhist added 24/03/22 --> sua loi upload duoc file nhung ko insert duoc vao DB
//                                                    if (xml2temp.NGAY_YL <= dMinSql) xml2temp.NGAY_YL = null;

//                                                }
//                                                listXML2.Add(xml2temp);
//                                            }
//                                            if (newXML1 != null)
//                                            {
//                                                newXML1.XML2s = listXML2;
//                                            }
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE.Trim())), e);
//                                            failedMessage += "XML2: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1.XML2s, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML2 ({aformFile.FileName})");
//                                        }
//                                        break;
//                                    case "XML3":
//                                        //Linhist added 09/12/22
//                                        DSACH_CHI_TIET_DVKT xml3 = null;// Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_DVKT>();
//                                        try
//                                        {
//                                            xml3 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE.Trim())).ParseXML<DSACH_CHI_TIET_DVKT>();
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} XML 3 ({Path.GetFileName(aformFile.FileName)}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN}): Lỗi không đọc được file (do sai định dạng BASE64)<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML3 ({aformFile.FileName}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN})");
//                                            break;
//                                        }

//                                        try
//                                        {
//                                            //DSACH_CHI_TIET_DVKT xml3 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_DVKT>();
//                                            List<XML3> listXML3 = new List<XML3>();

//                                            //Linhist them STT de auto tang gia tri
//                                            int autoSTT = 1;
//                                            foreach (DSACH_CHI_TIET_DVKTCHI_TIET_DVKT chitiet in xml3.CHI_TIET_DVKT)
//                                            {
//                                                XML3 xml3temp = new XML3();
//                                                if (newXML1 != null)
//                                                {
//                                                    xml3temp.DON_GIA = chitiet.DON_GIA;
//                                                    xml3temp.DON_VI_TINH = chitiet.DON_VI_TINH;
//                                                    xml3temp.GOI_VTYT = chitiet.GOI_VTYT;
//                                                    xml3temp.MA_BAC_SI = chitiet.MA_BAC_SI;
//                                                    xml3temp.MA_BENH = chitiet.MA_BENH;
//                                                    xml3temp.MA_DICH_VU = chitiet.MA_DICH_VU;
//                                                    xml3temp.MA_GIUONG = chitiet.MA_GIUONG;
//                                                    xml3temp.MA_KHOA = chitiet.MA_KHOA;
//                                                    xml3temp.MA_LK = chitiet.MA_LK;
//                                                    xml3temp.MA_NHOM = chitiet.MA_NHOM;
//                                                    xml3temp.MA_PTTT = chitiet.MA_PTTT;
//                                                    xml3temp.MA_VAT_TU = chitiet.MA_VAT_TU;
//                                                    xml3temp.MUC_HUONG = (byte)chitiet.MUC_HUONG;
//                                                    if (chitiet.NGAY_KQ.Length == 12) xml3temp.NGAY_KQ = DateTime.ParseExact(chitiet.NGAY_KQ, "yyyyMMddHHmm", null);
//                                                    if (chitiet.NGAY_YL.Length == 12) xml3temp.NGAY_YL = DateTime.ParseExact(chitiet.NGAY_YL, "yyyyMMddHHmm", null);
//                                                    xml3temp.SO_LUONG = chitiet.SO_LUONG;

//                                                    //Linhist auto tang gia tri STT, khong lay trong xml
//                                                    //xml3temp.STT = chitiet.STT;
//                                                    xml3temp.STT = autoSTT++;

//                                                    xml3temp.TEN_DICH_VU = chitiet.TEN_DICH_VU;
//                                                    xml3temp.TEN_VAT_TU = chitiet.TEN_VAT_TU;
//                                                    xml3temp.THANH_TIEN = chitiet.THANH_TIEN;
//                                                    xml3temp.TT_THAU = chitiet.TT_THAU;
//                                                    xml3temp.TYLE_TT = (byte)chitiet.TYLE_TT;
//                                                    xml3temp.T_BHTT = chitiet.T_BHTT;
//                                                    xml3temp.T_BNCCT = chitiet.T_BNCCT;
//                                                    if (chitiet.T_NGOAIDS.ToString().Trim().Length != 0)
//                                                    {
//                                                        xml3temp.T_NGOAIDS = double.Parse(chitiet.T_NGOAIDS.ToString());
//                                                    }
//                                                    else xml3temp.T_NGOAIDS = null;
//                                                    //xml3temp.T_NGUONKHAC = chitiet.T_NGUONKHAC;
//                                                    if (chitiet.T_NGUONKHAC?.Length > 0) { xml3temp.T_NGUONKHAC = double.Parse(chitiet.T_NGUONKHAC); } else { xml3temp.T_NGUONKHAC = 0; }

//                                                    xml3temp.T_BNTT = chitiet.T_BNTT;
//                                                    //haimaster comment 2/12/2021
//                                                    if (chitiet.T_TRANTT.ToString().Trim().Length > 0) { xml3temp.T_TRANTT = Convert.ToDouble(chitiet.T_TRANTT); } else { xml3temp.T_TRANTT = 0; };


//                                                    //Linhist added 24/03/22 --> sua loi upload duoc file nhung ko insert duoc vao DB
//                                                    if (xml3temp.NGAY_KQ <= dMinSql) xml3temp.NGAY_KQ = null;
//                                                    if (xml3temp.NGAY_YL <= dMinSql) xml3temp.NGAY_YL = null;

//                                                }
//                                                listXML3.Add(xml3temp);
//                                            }
//                                            if (newXML1 != null)
//                                            {
//                                                newXML1.XML3s = listXML3;
//                                            }
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
//                                            failedMessage += "XML 3: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1.XML3s, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML3 ({aformFile.FileName})");
//                                        }
//                                        break;
//                                    case "XML4":
//                                        //Linhist added 09/12/22
//                                        DSACH_CHI_TIET_CLS xml4 = null;// Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_CLS>();
//                                        try
//                                        {
//                                            xml4 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE.Trim())).ParseXML<DSACH_CHI_TIET_CLS>();
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} XML 4 ({Path.GetFileName(aformFile.FileName)}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN}): Lỗi không đọc được file (do sai định dạng BASE64)<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML4 ({aformFile.FileName}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN})");
//                                            break;
//                                        }

//                                        try
//                                        {
//                                            //DSACH_CHI_TIET_CLS xml4 = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ParseXML<DSACH_CHI_TIET_CLS>();
//                                            List<XML4> listXML4 = new List<XML4>();
//                                            foreach (DSACH_CHI_TIET_CLSCHI_TIET_CLS chitiet in xml4.CHI_TIET_CLS)
//                                            {
//                                                XML4 xml4temp = new XML4();
//                                                if (newXML1 != null)
//                                                {
//                                                    xml4temp.GIA_TRI = chitiet.GIA_TRI;
//                                                    xml4temp.KET_LUAN = chitiet.KET_LUAN;
//                                                    xml4temp.MA_CHI_SO = chitiet.MA_CHI_SO;
//                                                    xml4temp.MA_DICH_VU = chitiet.MA_DICH_VU;
//                                                    xml4temp.MA_LK = chitiet.MA_LK;
//                                                    xml4temp.MA_MAY = chitiet.MA_MAY;
//                                                    xml4temp.MO_TA = chitiet.MO_TA;
//                                                    if (chitiet.NGAY_KQ?.Length == 12) xml4temp.NGAY_KQ = DateTime.ParseExact(chitiet.NGAY_KQ, "yyyyMMddHHmm", null);
//                                                    xml4temp.STT = chitiet.STT;
//                                                    xml4temp.TEN_CHI_SO = chitiet.TEN_CHI_SO;


//                                                    //Linhist added 24/03/22 --> sua loi upload duoc file nhung ko insert duoc vao DB
//                                                    if (xml4temp.NGAY_KQ <= dMinSql) xml4temp.NGAY_KQ = null;

//                                                }
//                                                listXML4.Add(xml4temp);
//                                            }
//                                            if (newXML1 != null)
//                                            {
//                                                newXML1.XML4s = listXML4;
//                                            }
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
//                                            failedMessage += "XML 4: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1.XML4s, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML4 ({aformFile.FileName})");
//                                        }
//                                        break;
//                                    case "XML5":
//                                        //Linhist added 09/12/22
//                                        String XML5Result = null;// Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ToString();
//                                        try
//                                        {
//                                            XML5Result = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE.Trim())).ToString();
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} XML 5 ({Path.GetFileName(aformFile.FileName)}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN}): Lỗi không đọc được file (do sai định dạng BASE64)<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML5 ({aformFile.FileName}): (XML1: MA_BN: {newXML1.MA_BN}, MA_THE: {newXML1.MA_THE}, HO_TEN: {newXML1.HO_TEN})");
//                                            break;
//                                        }

//                                        try
//                                        {
//                                            //haimaster comment 27/12/2021 auto add CDATA
//                                            //String XML5Result = Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)).ToString();

//                                            if (XML5Result.IndexOf("<![CDATA[") == -1)
//                                            {
//                                                XML5Result = XML5Result.Replace("<DIEN_BIEN>", "<DIEN_BIEN> <![CDATA[");
//                                                XML5Result = XML5Result.Replace("</DIEN_BIEN>", "]]> </DIEN_BIEN>");
//                                            }

//                                            DSACH_CHI_TIET_DIEN_BIEN_BENH xml5 = XML5Result.ParseXML<DSACH_CHI_TIET_DIEN_BIEN_BENH>();
//                                            List<XML5> listXML5 = new List<XML5>();
//                                            foreach (DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH chitiet in xml5.CHI_TIET_DIEN_BIEN_BENH)
//                                            {
//                                                XML5 xml5temp = new XML5();
//                                                if (newXML1 != null)
//                                                {
//                                                    xml5temp.DIEN_BIEN = chitiet.DIEN_BIEN;
//                                                    xml5temp.HOI_CHAN = chitiet.HOI_CHAN;
//                                                    xml5temp.MA_LK = chitiet.MA_LK;
//                                                    if (chitiet.NGAY_YL?.Length == 12) xml5temp.NGAY_YL = DateTime.ParseExact(chitiet.NGAY_YL, "yyyyMMddHHmm", null);
//                                                    xml5temp.PHAU_THUAT = chitiet.PHAU_THUAT;
//                                                    xml5temp.STT = chitiet.STT;


//                                                    //Linhist added 24/03/22 --> sua loi upload duoc file nhung ko insert duoc vao DB
//                                                    if (xml5temp.NGAY_YL <= dMinSql) xml5temp.NGAY_YL = null;

//                                                }
//                                                listXML5.Add(xml5temp);
//                                            }
//                                            if (newXML1 != null)
//                                            {
//                                                newXML1.XML5s = listXML5;
//                                            }
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            failed++;
//                                            string errorAt = GetXMLErrorAtRow(Encoding.Default.GetString(Convert.FromBase64String(hosocon.NOIDUNGFILE)), e);
//                                            failedMessage += "XML 5: " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
//                                            failedFiles.Add(aformFile.FileName);

//                                            // Linhist added 27/07/22 them log file 
//                                            Uti.HandleInternalServerError(e, newXML1.XML5s, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading XML5 ({aformFile.FileName})");
//                                        }
//                                        break;
//                                }


//                            }

//                            if (newXML1 != null && newXML1.HO_TEN != null)
//                            {
//                                //Linhist added 06/04/23 loai tru truong hop sai MA_CSKCB
//                                if (newXML1.MA_CSKCB != MA_CSKCB_API)
//                                {
//                                    failed++;
//                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} XML 1 ({Path.GetFileName(aformFile.FileName)}): Mã CSKCB không chính xác<br/> ";
//                                    failedFiles.Add(aformFile.FileName);
//                                }
//                                else
//                                {

//                                    //await _hosoService.CreateNewHoSo(newXML1);
//                                    successMessage += Path.GetFileName(aformFile.FileName) + "<br/>";

//                                    if (catalog1.THONGTINHOSO.DANHSACHHOSO.ToList().Count > 1)
//                                    {
//                                        // var fileName = System.DateTime.Now.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")) + "_" + newXML1.MA_LK + "_" + newXML1.MA_CSKCB + ".xml";
//                                        var fileName = Uti.ToFilename(newXML1.MA_LK) + ".xml";

//                                        var filePath = Path.Combine(fullPath, fileName);
//                                        //khoikhoi sua de luu dung xml object
//                                        GIAMDINHHS GIAMDINHHS = new GIAMDINHHS();
//                                        GIAMDINHHS.THONGTINHOSO = new GIAMDINHHSTHONGTINHOSO();
//                                        GIAMDINHHS.THONGTINHOSO.DANHSACHHOSO = new GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO[1][];

//                                        GIAMDINHHS.THONGTINHOSO.DANHSACHHOSO[0] = hosoNho;
//                                        var stringFile = GetXml(GIAMDINHHS);

//                                        await File.WriteAllTextAsync(filePath, stringFile);
//                                        newXML1.FileName = fileName;


//                                        if (isUploadToTemFolder)
//                                        {
//                                            var filePathTemp = Path.Combine(fullPathTempFolder, fileName);

//                                            await File.WriteAllTextAsync(filePathTemp, stringFile);

//                                        }


//                                    }
//                                    finalXML1List.Add(newXML1);

//                                }

//                            }
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        failed++;
//                        string errorAt = aformFile.FileName;
//                        failedMessage += "Lỗi đọc file tổng:" + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
//                        failedFiles.Add(aformFile.FileName);

//                        // Linhist added 27/07/22 them log file 
//                        Uti.HandleInternalServerError(e, finalXML1List, _logger, $"{nameof(FilesService.FileUploadAPI)} --- Reading ({aformFile.FileName})");
//                    }
//                }
//                else
//                {
//                    failed++;
//                    string errorAt = aformFile.FileName;
//                    failedMessage += "File không đúng định dạng " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
//                    failedFiles.Add(aformFile.FileName);
//                }

//                uploadStatus = $"Tổng số file tải lên: {totalFile} file(s) XML<br/> Tải lên thành công: {success} hồ sơ<br/>  Tải lên thất bại: {failed} hồ sơ<br/> ";
//                var formFile = aformFile;
//                if (formFile.Length > 0)
//                {
//                    try
//                    {
//                        DateTime dd = DateTime.Now;

//                        var filePath = Path.Combine(fullPath, Uti.ToFilename(formFile.FileName));

//                        using (var stream = System.IO.File.Create(filePath))
//                        {
//                            await formFile.CopyToAsync(stream);
//                        }
//                        TimeSpan tss = DateTime.Now - dd;
//                        Uti.HandleCronJobLog(_logger, $"S3.Haimaster Check API: Save XML to SDD) ({MA_CSKCB_API}) - Total time: {tss.TotalMilliseconds}", nameof(HoSoManagementService.ValidateFiles));
//                    }
//                    catch (Exception e)
//                    {
//                        string failE = e.InnerException.ToString();

//                    }
//                    //// File upload to database
//                    ////Getting FileName
//                    //var fileName = Path.GetFileName(formFile.FileName);
//                    ////Getting file Extension
//                    //var fileExtension = Path.GetExtension(fileName);
//                    //// concatenating  FileName + FileExtension
//                    //var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

//                    //var documentViewmodel = new DocumentViewmodel()
//                    //{
//                    //    Id = 0,
//                    //    FileName = newFileName,
//                    //    FileType = fileExtension,
//                    //    Created = DateTime.Now,
//                    //    Modified = DateTime.Now
//                    //};

//                    //using (var target = new MemoryStream())
//                    //{
//                    //    formFile.CopyTo(target);
//                    //    documentViewmodel.FileData = target.ToArray();
//                    //}

//                    // use this documentViewmodel to save record in database 
//                }
//            }

//            try
//            {
//                DateTime d = DateTime.Now;
//                await _hosoService.CreateNewHoSo(finalXML1List, hospitalIdAPI, hospitalNameAPI, userIdAPI, MA_CSKCB_API, isFromHIS);



//                TimeSpan ts = DateTime.Now - d;
//                Uti.HandleCronJobLog(_logger, $"S3.Haimaster Check API: Create Ho So (Save to DB) ({MA_CSKCB_API}) - Total time: {ts.TotalMilliseconds}", nameof(HoSoManagementService.ValidateFiles));

//            }
//            catch (Exception ex)
//            {
//                Uti.HandleCronJobLog(_logger, $"S3.Error Save to DB ({MA_CSKCB_API}) - " + ex.ToString(), nameof(HoSoManagementService.ValidateFiles));
//            }
//            //await _hosoService.CreateNewHoSo(finalXML1List);

//            // Process uploaded files
//            // Don't rely on or trust the FileName property without validation.
//            //successMessage = fileUpload.FormFiles.Count.ToString() + " files uploaded!!";

//            //return RedirectToAction("Index", "FileUpload");
//            TimeSpan interval = DateTime.Now.Subtract(startTime);
//            uploadTime = "Thời gian tải lên: " + interval.TotalSeconds.ToString() + " Giây<br/>";


//            return new ResponseModel<dynamic> { Code = System.Net.HttpStatusCode.OK, Data = new { successMessage, failedMessage, uploadStatus, uploadTime, failedFiles }, Message = "" };
//        }


//        public static string Serialize<T>(T dataToSerialize)
//        {
//            try
//            {
//                var stringwriter = new System.IO.StringWriter();
//                var serializer = new XmlSerializer(typeof(T));
//                serializer.Serialize(stringwriter, dataToSerialize);
//                return stringwriter.ToString();
//            }
//            catch
//            {
//                throw;
//            }
//        }

//        static string GetXml(object obj)
//        {
//            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

//            using (var writer = new Utf8StringWriter())
//            {
//                xmlSerializer.Serialize(writer, obj,
//                       new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
//                return writer.ToString();
//            }
//        }

//    }
//    public class Utf8StringWriter : StringWriter
//    {
//        public override Encoding Encoding
//        {
//            get { return Encoding.UTF8; }
//        }
//    }



//    internal static class ParseHelpers
//    {
//        private static JavaScriptSerializer json;
//        private static JavaScriptSerializer JSON { get { return json ?? (json = new Nancy.Json.JavaScriptSerializer()); } }

//        public static Stream ToStream(this string @this)
//        {
//            var stream = new MemoryStream();
//            var writer = new StreamWriter(stream);
//            writer.Write(@this);
//            writer.Flush();
//            stream.Position = 0;
//            return stream;
//        }


//        public static T ParseXML<T>(this string @this) where T : class
//        {
//            var reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
//            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
//        }

//        public static T ParseJSON<T>(this string @this) where T : class
//        {
//            return JSON.Deserialize<T>(@this.Trim());
//        }


//        public static T FromXml<T>(this string @this)
//        {
//            T returnedXmlClass = default(T);

//            try
//            {
//                using (TextReader reader = new StringReader(@this))
//                {
//                    try
//                    {
//                        returnedXmlClass =
//                            (T)new XmlSerializer(typeof(T)).Deserialize(reader);
//                    }
//                    catch (InvalidOperationException)
//                    {
//                        // String passed is not XML, simply return defaultXmlClass
//                    }
//                }
//            }
//            catch
//            {
//            }

//            return returnedXmlClass;
//        }
//    }
//}