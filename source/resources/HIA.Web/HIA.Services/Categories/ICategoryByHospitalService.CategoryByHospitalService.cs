using AutoMapper;
using HIA.Core.Base;
using HIA.Core.HTML;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Services.Base;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace HIA.Services.Categories
{
    public class CategoryByHospitalService<TEntity, TDisplayModel, TModifyModel> : ICategoryByHospitalService<TEntity, TDisplayModel, TModifyModel> 
        where TEntity : class
        where TDisplayModel : class
        where TModifyModel : class
    {
        private readonly string serviceName = $"{nameof(TEntity)}_CategoryService";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<TEntity> _cateRepos;
        private readonly IBaseRepository<Hospital> _hospitalRepos;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryByHospitalService<TEntity, TModifyModel, TModifyModel>> _logger;
        public CategoryByHospitalService(
            IUnitOfWork unitOfWork,
            IBaseRepository<TEntity> cateRepos,
            IBaseRepository<Hospital> hospitalRepos,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<CategoryByHospitalService<TEntity, TModifyModel, TModifyModel>> logger
            )
        {
            _unitOfWork = unitOfWork;
            _cateRepos = cateRepos;
            _hospitalRepos = hospitalRepos;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }

        private FilterQueryableModel<TEntity> BuildQueryToGetListData(FilterDownloadModel request, TEntity emptyModel, string maCSKCB)
        {
            var result = new FilterQueryableModel<TEntity>();
            #region Build Filter Query
            Expression<Func<TEntity, bool>> filter = PredicateBuilder.New<TEntity>(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(maCSKCB));
            // add filter when has searchKey
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.ToLower();
                Expression<Func<TEntity, bool>> filterSearch = PredicateBuilder.New<TEntity>();
                foreach (PropertyInfo propertyInfo in emptyModel.GetType().GetProperties())
                {
                    if (request.IgnoreKeysForSearch.Contains(propertyInfo.Name)) continue;

                    filterSearch = PredicateExpresion<TEntity>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
                }
                filter = filter.And(filterSearch);
            }
            result.FilterQueryable = filter;
            #endregion
            #region Build Sort Query
            IList<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>> sortFuncList = new List<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>>();

            var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
            // dynamic sort by column
            if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
            {
                switch (columnName)
                {
                    default:
                        sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
                        break;
                }
            }
            result.OrderQueryable = sortFuncList;
            #endregion
            #region Includes
            result.IncludeQueryable = null;
            #endregion

            return result;
        }
        public async Task<ResponseModel<PaginationResponseModel<TDisplayModel>>> GetItems(PaginationRequestModel request, List<string> ignoreKeysForSearch, TEntity emptyModel)
        {
            var funcName = "GetItems";
            var res = new ResponseModel<PaginationResponseModel<TDisplayModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<TDisplayModel>());
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }

                var filterModel = _mapper.Map<FilterDownloadModel>(request);
                filterModel.IgnoreKeysForSearch = ignoreKeysForSearch;
                var queryable = BuildQueryToGetListData(filterModel, emptyModel, currentMaCSKCB);
                #region Get List
                PaginationResponseModel<TEntity> items = (await _cateRepos.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, 
                    queryable?.FilterQueryable, queryable?.OrderQueryable, queryable?.IncludeQueryable));
                var itemInfo = _mapper.Map<List<TDisplayModel>>(items?.Items?.ToList());

                res.Data.Items = itemInfo;
                res.Data.TotalItems = items.TotalItems;
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog($"{nameof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, new PaginationResponseModel<TDisplayModel>(), _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }
        public async Task<ResponseModel<TModifyModel>> GetItemById(string id, string keyString)
        {
            var funcName = "GetItemById";
            var res = new ResponseModel<TModifyModel>(HttpStatusCode.OK, "", default(TModifyModel));

            try
            {
                #region Validation
                int idEntity = 0;
                var isValid = int.TryParse(id, out idEntity);
                if (!isValid || idEntity == 0)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    return res;
                }
                #endregion

                var item = await _cateRepos.GetItem(
                    x => EF.Property<int>(x, keyString).Equals(idEntity),
                    null);

                var itemData = _mapper.Map<TModifyModel>(item);
                res.Data = itemData;
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, default(TModifyModel), _logger, $"{serviceName} - {funcName}");
            }

            return res;
        }

        public async Task<ResponseModel<bool>> CreateNewItem(TModifyModel request, List<string> uniqueFieldNames)
        {
            var funcName = "CreateNewItem";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentUserId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
                var currentUserName = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                var savedModel = _mapper.Map<TEntity>(request);
                (savedModel as dynamic).MA_CSKCB = currentMaCSKCB;

                #region Validtion
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, uniqueFieldNames, currentMaCSKCB);
                if (!!isExistUniqueFields) {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = $"{string.Join($" {DisplayFieldResource.OR_TEXT} ", uniqueFieldNames.ToArray())} {MessageResource.ERR_400_EXIST}";
                    res.Data = false;
                    return res;
                }
                #endregion

                await _cateRepos.InsertAsync(savedModel);

                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog($"Create new of {typeof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> UpdateItem(TModifyModel request, string keyString, List<string> uniqueFieldNames)
        {
            var funcName = "UpdateItem";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                #region Validtion
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                var propertyInfo = request.GetType().GetProperty(keyString);
                var keyValue = (int)propertyInfo.GetValue(request, null);

                var entity = await _cateRepos.GetItem(
                    x => EF.Property<int>(x, keyString).Equals(keyValue),
                    null);
                if (entity == null)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    res.Data = false;
                    return res;
                }

                var savedModel = _mapper.Map<TEntity>(request);
                if (uniqueFieldNames.Any())
                {
                    var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, uniqueFieldNames, currentMaCSKCB, entity);
                    if (!!isExistUniqueFields)
                    {
                        res.Code = HttpStatusCode.BadRequest;
                        res.Message = $"{string.Join($" {DisplayFieldResource.OR_TEXT} ", uniqueFieldNames.ToArray())} {MessageResource.ERR_400_EXIST}";
                        res.Data = false;
                        return res;
                    }
                }
                #endregion

                _cateRepos.Update(savedModel);

                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog($"Update of {typeof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        private async Task<bool> CheckUniqueFieldsExist(TEntity entityRequest, List<string> uniqueFieldNames, string maCSKCB, TEntity entity = null)
        {
            Expression<Func<TEntity, bool>> filter = PredicateBuilder.New<TEntity>(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(maCSKCB));
            Expression<Func<TEntity, bool>> filterSearch = PredicateBuilder.New<TEntity>();
            // update
            if (entity != null)
            {
                foreach (var item in uniqueFieldNames)
                {
                    var propertyInfoRequest = entityRequest.GetType().GetProperty(item);
                    var valueRequest = ((string)propertyInfoRequest.GetValue(entityRequest, null))?.ToLower();

                    var propertyInfoEntity = entity.GetType().GetProperty(item);
                    var valueEntity = ((string)propertyInfoEntity.GetValue(entity, null))?.ToLower();

                    filterSearch = filterSearch.Or(x => !valueRequest.Equals(valueEntity) && EF.Property<string>(x, item).ToLower().Equals(valueRequest));
                }
            } 
            else
            {
                foreach (var item in uniqueFieldNames)
                {
                    var propertyInfo = entityRequest.GetType().GetProperty(item);
                    var value = ((string)propertyInfo.GetValue(entityRequest, null))?.ToLower();

                    filterSearch = filterSearch.Or(x => EF.Property<string>(x, item).ToLower().Equals(value));
                }
            }
            filter = filter.And(filterSearch);

            var isExist = await _cateRepos.AnyAsync(filter);
            return isExist;
        }

        public async Task<ResponseModel<bool>> DeleteItem(int id, string keyString)
        {
            var funcName = "DeleteItem";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                #region Validtion
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                TEntity item = await _cateRepos.GetById(id);
                if (item == null)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    res.Data = false;
                    return res;
                }
                #endregion

                _cateRepos.Delete(item);
                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog($"Delete of {typeof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> UploadCategoryFile(IFormFile request, List<string> keysString)
        {
            var funcName = "UploadCategoryFile";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                #region Validtion
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                #endregion

                List<TEntity> data = HandleFile130CommonService.UploadExcel<TEntity>(request, keysString);
                //Linhist added 041126
                var isBacsi = typeof(TEntity) == typeof(DMBacSi);
                foreach (var item in data)
                {
                    (item as dynamic).MA_CSKCB = currentMaCSKCB;

                    if (isBacsi)
                    {
                        var bs = item as DMBacSi;
                        if (string.IsNullOrWhiteSpace(bs.CHUC_DANH) && !string.IsNullOrEmpty(bs.CHUC_DANH_NN))
                        {
                            bs.CHUC_DANH = Uti.Get_ChucDanh(bs.CHUC_DANH_NN);
                        }
                        else if (!string.IsNullOrWhiteSpace(bs.CHUC_DANH) && string.IsNullOrEmpty(bs.CHUC_DANH_NN))
                        {
                            bs.CHUC_DANH_NN = Uti.Get_CDNN(bs.CHUC_DANH);
                        }

                        if (string.IsNullOrWhiteSpace(bs.VI_TRI) && !string.IsNullOrEmpty(bs.MA_VI_TRI))
                        {
                            bs.VI_TRI = Uti.Get_ViTri(bs.MA_VI_TRI);
                        }
                        else if (!string.IsNullOrWhiteSpace(bs.VI_TRI) && string.IsNullOrEmpty(bs.MA_VI_TRI))
                        {
                            bs.MA_VI_TRI = Uti.Get_MaViTri(bs.VI_TRI);
                        }
                    }
                }

                #region update Db
                _cateRepos.DeleteListByFilter(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(currentMaCSKCB));

                //foreach(var dat in data) 
                //{
                //    await _cateRepos.InsertAsync(dat);
                //    await _unitOfWork.CommitAsync();
                //}

                await _cateRepos.InsertListAsync(data);

                await _unitOfWork.CommitAsync();
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog($"Upload file of {typeof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        //Linhist added 22/02/25
        public async Task<ResponseModel<bool>> ReplaceCategoryFile(IFormFile request, List<string> keysString)
        {
            var funcName = "UploadCategoryFile";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                #region Validtion
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                #endregion

                List<TEntity> data = HandleFile130CommonService.UploadExcel<TEntity>(request, keysString);
                foreach (var item in data)
                {
                    (item as dynamic).MA_CSKCB = currentMaCSKCB;
                }

                #region update Db
                var filter = PredicateBuilder.New<TEntity>(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(currentMaCSKCB));
                var type = typeof(TEntity);
                if (type == typeof(DMDichVu))
                {
                    var allMaDVs = data.Select(x => (x as DMDichVu).MA_DICH_VU).Distinct().ToList();
                    filter = filter.And(x => allMaDVs.Contains((x as DMDichVu).MA_DICH_VU));
                }
                else if (type == typeof(DMThuoc))
                {
                    var allMaDVs = data.Select(x => (x as DMThuoc).MA_THUOC).Distinct().ToList();
                    filter = filter.And(x => allMaDVs.Contains((x as DMThuoc).MA_THUOC));
                }
                else if (type == typeof(DMVTYT))
                {
                    var allMaDVs = data.Select(x => (x as DMVTYT).MA_VAT_TU).Distinct().ToList();
                    filter = filter.And(x => allMaDVs.Contains((x as DMVTYT).MA_VAT_TU));
                }
                else if (type == typeof(DMBacSi))
                {
                    var allMaDVs = data.Select(x => (x as DMBacSi).MA_CCHN).Distinct().ToList();
                    filter = filter.And(x => allMaDVs.Contains((x as DMBacSi).MA_CCHN));
                }
                else if (type == typeof(DMKhoaBV))
                {
                    var allMaDVs = data.Select(x => (x as DMKhoaBV).MA_KHOA).Distinct().ToList();
                    filter = filter.And(x => allMaDVs.Contains((x as DMKhoaBV).MA_KHOA));
                }

                var oldData = await _cateRepos.GetListAsync(filter);
                var oldChg = new List<TEntity>();
                var newChg = new List<TEntity>();
                foreach (var item in data)
                {
                    var isNew = true;
                    if (type == typeof(DMDichVu))
                    {
                        var dv = item as DMDichVu;
                        var lst = oldData.Where(x => (x as DMDichVu).MA_DICH_VU == dv.MA_DICH_VU &&
                                                            (string.IsNullOrEmpty(dv.QUYET_DINH) || (x as DMDichVu).QUYET_DINH == dv.QUYET_DINH) &&
                                                            (string.IsNullOrEmpty(dv.TU_NGAY) || (x as DMDichVu).TU_NGAY == dv.TU_NGAY) &&
                                                            (string.IsNullOrEmpty(dv.DEN_NGAY) || (x as DMDichVu).DEN_NGAY == dv.DEN_NGAY)).ToList();
                        foreach (var chg in lst)
                        {
                            (chg as DMDichVu).DON_GIA = dv.DON_GIA;
                            (chg as DMDichVu).MA_GIA = dv.MA_GIA;
                            (chg as DMDichVu).TEN_DICH_VU = dv.TEN_DICH_VU;
                            oldChg.Add(chg);
                            isNew = false;
                        }
                    }
                    //else if (type == typeof(DMThuoc))
                    //{
                    //    var th = item as DMThuoc;
                    //    foreach (var chg in oldData.Where(x => (x as DMThuoc).MA_THUOC == th.MA_THUOC).ToList())
                    //    {
                    //        th.THID = (chg as DMThuoc).THID;
                    //        th.MA_CSKCB = (chg as DMThuoc).MA_CSKCB;
                    //        isNew = false;
                    //    }
                    //}
                    //else if (type == typeof(DMVTYT))
                    //{
                    //    var vt = item as DMVTYT;
                    //    foreach (var chg in oldData.Where(x => (x as DMVTYT).MA_VAT_TU == vt.MA_VAT_TU).ToList())
                    //    {
                    //        vt.VTID = (chg as DMVTYT).VTID;
                    //        vt.MA_CSKCB = (chg as DMVTYT).MA_CSKCB;
                    //        isNew = false;
                    //    }
                    //}
                    //else if (type == typeof(DMBacSi))
                    //{
                    //    var bs = item as DMBacSi;
                    //    foreach (var chg in oldData.Where(x => (x as DMBacSi).MACCHN == bs.MACCHN).ToList())
                    //    {
                    //        bs.BacSiID = (chg as DMBacSi).BacSiID;
                    //        bs.MA_CSKCB = (chg as DMBacSi).MA_CSKCB;
                    //        isNew = false;
                    //    }
                    //}
                    //else if (type == typeof(DMKhoaBV))
                    //{
                    //    var kh = item as DMKhoaBV;
                    //    foreach (var chg in oldData.Where(x => (x as DMKhoaBV).MA_KHOA == kh.MA_KHOA).ToList())
                    //    {
                    //        kh.KBVID = (chg as DMKhoaBV).KBVID;
                    //        kh.MA_CSKCB = (chg as DMKhoaBV).MA_CSKCB;
                    //        isNew = false;
                    //    }
                    //}

                    if (isNew) newChg.Add(item);
                }

                if (oldChg.Any())
                {
                    await _cateRepos.UpdateListAsync(oldChg);
                }

                if (newChg.Any())
                {
                    await _cateRepos.InsertListAsync(newChg);
                }

                await _unitOfWork.CommitAsync();
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog($"Upload file of {typeof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        public async Task<ResponseModel<byte[]>> DownloadExcelFile(string maCSKCB, string search)
        {
            var funcName = "DownloadExcelFile";
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(maCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }

                var queryable = new FilterQueryableModel<TEntity>()
                {
                    FilterQueryable = PredicateBuilder.New<TEntity>(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(maCSKCB)),
                    OrderQueryable = new List<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>>
                    {
                        q => q.OrderBy(x => EF.Property<string>(x, nameof(DichVuMaMay.MA_DICH_VU)))
                    },
                    IncludeQueryable = null,
                };

                if (!string.IsNullOrEmpty(search))
                {
                    queryable.FilterQueryable = queryable.FilterQueryable.And(x => EF.Property<string>(x, nameof(DichVuMaMay.MA_MAY)).Contains(search) || EF.Property<string>(x, nameof(DichVuMaMay.MA_DICH_VU)).Contains(search));
                }

                #region Get List
                var data = await _cateRepos.GetAllCustomSortAsync(queryable?.FilterQueryable, queryable?.OrderQueryable, queryable?.IncludeQueryable);
                data ??= new List<TEntity>();
                #endregion

                res.Data = HandleFile130CommonService.DownloadExcel<TEntity>(data);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<byte[]>(
                    ex, null, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        public async Task<ResponseModel<byte[]>> DownloadExcelFile(string filterDownloadString, TEntity emptyModel)
        {
            var funcName = "DownloadExcelFile";
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            try
            {
                var roleId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                if (string.IsNullOrWhiteSpace(filterDownloadString)) return null;
                FilterDownloadModel request = JsonConvert.DeserializeObject<FilterDownloadModel>(filterDownloadString);
                var queryable = BuildQueryToGetListData(request, emptyModel, currentMaCSKCB);

                #region Get List
                var data = await _cateRepos.GetAllCustomSortAsync(queryable?.FilterQueryable, queryable?.OrderQueryable, queryable?.IncludeQueryable);
                if (data.Count() == 0)
                {
                    data = new List<TEntity> { emptyModel };
                }
                #endregion

                res.Data = HandleFile130CommonService.DownloadExcel<TEntity>(data);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<byte[]>(
                    ex, null, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }


        private CategoryImporter _importer = null;
        private string _errorScraping = "";

        private async Task<DataTable> Scrape_DMThuoc()
        {
            string content = await _importer.DMThuoc_GetFirstPage();
            if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
            {
                _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                return null;
            }

            DataTable dt = _importer.DMThuoc_PeakTableCaption(content);
            if (dt == null || dt.Columns.Count < 1)
            {
                _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                return null;
            }

            if (!_importer.DMThuoc_PeakContent(content, dt))
            {
                if (dt == null || dt.Rows.Count < 1)
                {
                    _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                    return null;
                }

                return dt;
            }

            int pgCnt = _importer.DMThuoc_PeakPageCount(content);
            if (pgCnt < 0)
            {
                _errorScraping = "Không đọc được số trang từ cổng BH";
                return dt;
            }

            for (int i = 1; i < pgCnt; i++)
            {
                var html = await _importer.DMThuoc_GetNextPage(i);
                if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
                {
                    _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                    return null;
                }

                if (!_importer.DMThuoc_PeakContent(html, dt)) break;
            }

            return dt;
        }

        private async Task<DataTable> Scrape_DMDichVu()
        {
            string content = await _importer.DMDichVu_GetFirstPage();
            if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
            {
                _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                return null;
            }

            DataTable dt = _importer.DMDichVu_PeakTableCaption(content);
            if (dt == null || dt.Columns.Count < 1)
            {
                _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                return null;
            }

            if (!_importer.DMDichVu_PeakContent(content, dt))
            {
                if (dt == null || dt.Rows.Count < 1)
                {
                    _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                    return null;
                }

                return dt;
            }

            int pgCnt = _importer.DMDichVu_PeakPageCount(content);
            if (pgCnt < 0)
            {
                _errorScraping = "Không đọc được số trang từ cổng BH";
                return dt;
            }

            for (int i = 1; i < pgCnt; i++)
            {
                try
                {
                    var html = await _importer.DMDichVu_GetNextPage(i);
                    if (string.IsNullOrEmpty(html) || !html.Contains("<") || !html.Contains(">"))
                    {
                        _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                        return null;
                    }

                    if (!_importer.DMDichVu_PeakContent(html, dt)) break;
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
            }

            return dt;
        }

        private async Task<DataTable> Scrape_DMVatTu()
        {
            string content = await _importer.DMVatTu_GetFirstPage();
            if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
            {
                _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                return null;
            }

            DataTable dt = _importer.DMVatTu_PeakTableCaption(content);
            if (dt == null || dt.Columns.Count < 1)
            {
                _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                return null;
            }

            if (!_importer.DMVatTu_PeakContent(content, dt))
            {
                if (dt == null || dt.Rows.Count < 1)
                {
                    _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                    return null;
                }

                return dt;
            }

            int pgCnt = _importer.DMVatTu_PeakPageCount(content);
            if (pgCnt < 0)
            {
                _errorScraping = "Không đọc được số trang từ cổng BH";
                return dt;
            }

            for (int i = 1; i < pgCnt; i++)
            {
                var html = await _importer.DMVatTu_GetNextPage(i);
                if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
                {
                    _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                    return null;
                }

                if (!_importer.DMVatTu_PeakContent(html, dt)) break;
            }

            return dt;
        }

        private async Task<DataTable> Scrape_DMBacSi()
        {
            string content = await _importer.DMBacSi_GetFirstPage();
            if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
            {
                _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                return null;
            }

            DataTable dt = _importer.DMBacSi_PeakTableCaption(content);
            if (dt == null || dt.Columns.Count < 1)
            {
                _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                return null;
            }

            if (!_importer.DMBacSi_PeakContent(content, dt))
            {
                if (dt == null || dt.Rows.Count < 1)
                {
                    _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                    return null;
                }

                return dt;
            }

            int pgCnt = _importer.DMBacSi_PeakPageCount(content);
            if (pgCnt < 0)
            {
                _errorScraping = "Không đọc được số trang từ cổng BH";
                return dt;
            }

            for (int i = 1; i < pgCnt; i++)
            {
                var html = await _importer.DMBacSi_GetNextPage(i);
                if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
                {
                    _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                    return null;
                }

                if (!_importer.DMBacSi_PeakContent(html, dt)) break;
            }

            return dt;
        }

        private async Task<DataTable> Scrape_DMKhoa()
        {
            string content = await _importer.DMKhoa_GetFirstPage();
            if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
            {
                _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                return null;
            }

            DataTable dt = _importer.DMKhoa_PeakTableCaption(content);
            if (dt == null || dt.Columns.Count < 1)
            {
                _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                return null;
            }

            if (!_importer.DMKhoa_PeakContent(content, dt))
            {
                if (dt == null || dt.Rows.Count < 1)
                {
                    _errorScraping = "Định dạng dữ liệu từ cổng BH đã thay đổi";
                    return null;
                }

                return dt;
            }

            int pgCnt = _importer.DMKhoa_PeakPageCount(content);
            if (pgCnt < 0)
            {
                _errorScraping = "Không đọc được số trang từ cổng BH";
                return dt;
            }

            for (int i = 1; i < pgCnt; i++)
            {
                var html = await _importer.DMKhoa_GetNextPage(i);
                if (string.IsNullOrEmpty(content) || !content.Contains("<") || !content.Contains(">"))
                {
                    _errorScraping = "Không đọc được dữ liệu từ cổng BH";
                    return null;
                }

                if (!_importer.DMKhoa_PeakContent(html, dt)) break;
            }

            return dt;
        }

        private async Task<List<TEntity>> ScrapeFromAPI(string username, string password)
        {
            _errorScraping = "";
            DataTable dt = null;
            Type t = typeof(TEntity);
            _importer = new(username, password);

            try
            {
                if (t == typeof(DMThuoc))
                {
                    dt = await Scrape_DMThuoc();
                }
                else if (t == typeof(DMDichVu))
                {
                    dt = await Scrape_DMDichVu();
                }
                else if (t == typeof(DMVTYT))
                {
                    dt = await Scrape_DMVatTu();
                }
                else if (t == typeof(DMBacSi))
                {
                    dt = await Scrape_DMBacSi();
                }
                else if (t == typeof(DMKhoaBV))
                {
                    dt = await Scrape_DMKhoa();
                }

                return JsonConvert.DeserializeObject<List<TEntity>>(JsonConvert.SerializeObject(dt));
            }
            catch (Exception ex)
            {
                _errorScraping = ex.Message;// "Có lỗi trong quá trình tải dữ liệu từ cổng BH";
                return null;
            }
        }

        public async Task<bool> AllowImport()
        {
            var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            if (string.IsNullOrEmpty(currentRole) || string.IsNullOrWhiteSpace(currentMaCSKCB)) return false;
            if (currentRole != RoleConstant.MASTER && currentRole != RoleConstant.ADMIN) return false;

            var hos = await _hospitalRepos.GetItem(h => h.MA_CSKCB == currentMaCSKCB);
            return (hos != null && !string.IsNullOrEmpty(hos.Username) && !string.IsNullOrEmpty(hos.Password));
        }

        public async Task<ResponseModel<bool>> ImportFromAPI()
        {
            var funcName = "UploadCategoryFile";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                #region Validtion
                if (string.IsNullOrEmpty(currentRole) || string.IsNullOrWhiteSpace(currentMaCSKCB) || !(currentRole == RoleConstant.MASTER || currentRole == RoleConstant.ADMIN))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                var hos = await _hospitalRepos.GetItem(h => h.MA_CSKCB == currentMaCSKCB);
                if (hos == null)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                else if (string.IsNullOrEmpty(hos.Username) || string.IsNullOrEmpty(hos.Password))
                {
                    res.Code = HttpStatusCode.NotAcceptable;
                    res.Message = "Không có thông tin truy cập vào cổng BH!";
                    return res;
                }
                #endregion

                List<TEntity> data = await ScrapeFromAPI(hos.Username, hos.Password);
                if (!string.IsNullOrEmpty(_errorScraping))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = _errorScraping;
                    _errorScraping = "";
                    return res;
                }

                foreach (dynamic item in data)
                {
                    item.MA_CSKCB = currentMaCSKCB;
                }

                #region update Db
                _cateRepos.DeleteListByFilter(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(currentMaCSKCB));
                await _cateRepos.InsertListAsync(data);
                await _unitOfWork.CommitAsync();
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog($"Import data from API {typeof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }
    }
}
