using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Services.Base;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace HIA.Services.Categories
{
    public class CategoryService<TEntity, TDisplayModel, TModifyModel> : ICategoryService<TEntity, TDisplayModel, TModifyModel> 
        where TEntity : class
        where TDisplayModel : class
        where TModifyModel : class
    {
        private readonly string serviceName = $"{nameof(TEntity)}_CategoryService";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<TEntity> _cateRepos;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService<TEntity, TModifyModel, TModifyModel>> _logger;
        public CategoryService(
            IUnitOfWork unitOfWork,
            IBaseRepository<TEntity> cateRepos,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<CategoryService<TEntity, TModifyModel, TModifyModel>> logger
            )
        {
            _unitOfWork = unitOfWork;
            _cateRepos = cateRepos;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }

        private FilterQueryableModel<TEntity> BuildQueryToGetListData(FilterDownloadModel request, TEntity emptyModel)
        {
            var result = new FilterQueryableModel<TEntity>();
            #region Build Filter Query
            Expression<Func<TEntity, bool>> filter = PredicateBuilder.New<TEntity>(true);
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
                var filterModel = _mapper.Map<FilterDownloadModel>(request);
                filterModel.IgnoreKeysForSearch = ignoreKeysForSearch;
                var queryable = BuildQueryToGetListData(filterModel, emptyModel);
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
                var savedModel = _mapper.Map<TEntity>(request);

                #region Validtion
                var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, uniqueFieldNames);
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
                await _sysHistoryLogService.AddSysLog($"{nameof(TEntity)} - {funcName}");
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
                #region Validtion
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
                    var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, uniqueFieldNames, entity);
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
                await _sysHistoryLogService.AddSysLog($"{nameof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        private async Task<bool> CheckUniqueFieldsExist(TEntity entityRequest, List<string> uniqueFieldNames, TEntity entity = null)
        {
            Expression<Func<TEntity, bool>> filter = PredicateBuilder.New<TEntity>();
            // update
            if (entity != null)
            {
                foreach (var item in uniqueFieldNames)
                {
                    var propertyInfoRequest = entityRequest.GetType().GetProperty(item);
                    var valueRequest = ((string)propertyInfoRequest.GetValue(entityRequest, null))?.ToLower();

                    var propertyInfoEntity = entity.GetType().GetProperty(item);
                    var valueEntity = ((string)propertyInfoEntity.GetValue(entity, null))?.ToLower();

                    filter = filter.Or(x => !valueRequest.Equals(valueEntity) && EF.Property<string>(x, item).ToLower().Equals(valueRequest));
                }
            } 
            else
            {
                foreach (var item in uniqueFieldNames)
                {
                    var propertyInfo = entityRequest.GetType().GetProperty(item);
                    var value = ((string)propertyInfo.GetValue(entityRequest, null))?.ToLower();

                    filter = filter.Or(x => EF.Property<string>(x, item).ToLower().Equals(value));
                }
            }
            
            var isExist = await _cateRepos.AnyAsync(filter);
            return isExist;
        }

        public async Task<ResponseModel<bool>> DeleteItem(int id, string keyString)
        {
            var funcName = "DeleteItem";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                #region Validtion
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
                await _sysHistoryLogService.AddSysLog($"{nameof(TEntity)} - {funcName}");
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
                #region validation
                #endregion

                List<TEntity> data = HandleFile130CommonService.UploadExcel<TEntity>(request, keysString);

                //Linhist added 041126
                var isBacsi = typeof(TEntity) == typeof(DMBacSi);
                foreach (var item in data)
                {
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
                _cateRepos.DeleteAll();
                await _cateRepos.InsertListAsync(data);

                await _unitOfWork.CommitAsync();
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog($"{nameof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> UploadCategoryFileWithHospital(IFormFile request, List<string> keysString, string maCSKCBKey, string maCSKCBValue)
        {
            var funcName = "UploadCategoryFile";
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                #region validation
                #endregion

                List<TEntity> data = HandleFile130CommonService.UploadExcel<TEntity>(request, keysString);

                foreach (var item in data)
                {
                    foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
                    {
                        if (!propertyInfo.Name.Equals(maCSKCBKey)) continue;
                        propertyInfo.SetValue(item, maCSKCBValue);
                    }
                }
                #region update Db
                _cateRepos.DeleteAll();
                await _cateRepos.InsertListAsync(data);

                await _unitOfWork.CommitAsync();
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog($"{nameof(TEntity)} - {funcName}");
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{serviceName} - {funcName}");
            }
            return res;
        }

        public async Task<ResponseModel<byte[]>> DownloadExcelFile(string filterDownloadString, TEntity emptyModel)
        {
            var funcName = "DownloadExcelFile";
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            try
            {
                if (string.IsNullOrWhiteSpace(filterDownloadString)) return null;
                FilterDownloadModel request = JsonConvert.DeserializeObject<FilterDownloadModel>(filterDownloadString);
                var queryable = BuildQueryToGetListData(request, emptyModel);

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
    }
}
