using System.Data;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Core.HTML;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Auths;
using HIA.Data.Catalogs;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Services.Base;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Categories;

public class CategoryByHospitalService<TEntity, TDisplayModel, TModifyModel> : ICategoryByHospitalService<TEntity, TDisplayModel, TModifyModel>
    where TEntity : class
    where TDisplayModel : class, IMapable<TEntity>, new()
    where TModifyModel : class, IMapable<TEntity>, new()
{
    private const string ScrapeReadError = "Không đọc được dữ liệu từ cổng BH";
    private const string ScrapeFormatError = "Định dạng dữ liệu từ cổng BH đã thay đổi";

    private readonly string serviceName = $"{nameof(TEntity)}_CategoryService";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<TEntity> _cateRepos;
    private readonly IBaseRepository<Hospital> _hospitalRepos;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<CategoryByHospitalService<TEntity, TModifyModel, TModifyModel>> _logger;

    private string _errorScraping = "";

    public CategoryByHospitalService(
        IUnitOfWork unitOfWork,
        IBaseRepository<TEntity> cateRepos,
        IBaseRepository<Hospital> hospitalRepos,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        ILogger<CategoryByHospitalService<TEntity, TModifyModel, TModifyModel>> logger)
    {
        _unitOfWork = unitOfWork;
        _cateRepos = cateRepos;
        _hospitalRepos = hospitalRepos;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;
        _logger = logger;
    }

    private static TEntity ToEntity(TModifyModel request) =>
        request.MapTo() ?? throw new InvalidOperationException($"Cannot map {typeof(TModifyModel).Name} to {typeof(TEntity).Name}");

    private string GetClaim(string claimType)
        => _httpContext.HttpContext?.User.FindFirst(claimType)?.Value ?? string.Empty;

    private static FilterQueryableModel<TEntity> BuildQueryToGetListData(FilterDownloadModel request, TEntity emptyModel, string maCSKCB)
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
                if (request.IgnoreKeysForSearch?.Contains(propertyInfo.Name) == true) continue;

                filterSearch = PredicateExpresion<TEntity>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
            }
            filter = filter.And(filterSearch);
        }
        result.FilterQueryable = filter;
        #endregion

        #region Build Sort Query
        var sortFuncList = new List<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>>();

        var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn ?? string.Empty);
        // dynamic sort by column
        if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
        {
            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
        }
        result.OrderQueryable = sortFuncList;
        #endregion

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<TDisplayModel>>> GetItems(PaginationRequestModel request, List<string> ignoreKeysForSearch, TEntity emptyModel)
    {
        var funcName = "GetItems";
        var res = new ResponseModel<PaginationResponseModel<TDisplayModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<TDisplayModel>());
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            var filterModel = new FilterDownloadModel();
            ((IMapable<PaginationRequestModel>)filterModel).MapFrom(request);
            filterModel.IgnoreKeysForSearch = ignoreKeysForSearch;
            var queryable = BuildQueryToGetListData(filterModel, emptyModel, currentMaCSKCB);

            PaginationResponseModel<TEntity> items = await _cateRepos.GetListPaginationCustomSortAsync(
                request.PageIndex, request.PageNumber, queryable.FilterQueryable, queryable.OrderQueryable, queryable.IncludeQueryable);
            var itemInfo = items.Items.Select(x =>
            {
                var m = new TDisplayModel();
                m.MapFrom(x);
                return m;
            }).ToList();

            res.Data = new PaginationResponseModel<TDisplayModel>(items.TotalItems, itemInfo);

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
        var res = new ResponseModel<TModifyModel>(HttpStatusCode.OK, "", null);

        try
        {
            #region Validation
            var isValid = int.TryParse(id, out int idEntity);
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

            var itemData = item == null ? null : new TModifyModel();
            itemData?.MapFrom(item);
            res.Data = itemData;
        }
        catch (Exception ex)
        {
            res = CategoryResponses.InternalError<TModifyModel>(ex, _logger, $"{serviceName} - {funcName}");
        }

        return res;
    }

    public async Task<ResponseModel<bool>> CreateNewItem(TModifyModel request, List<string> uniqueFieldNames)
    {
        var funcName = "CreateNewItem";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            var savedModel = ToEntity(request);
            (savedModel as dynamic).MA_CSKCB = currentMaCSKCB;

            #region Validtion
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, uniqueFieldNames, currentMaCSKCB);
            if (isExistUniqueFields)
            {
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
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

            #region Validtion
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            var propertyInfo = request.GetType().GetProperty(keyString)
                ?? throw new InvalidOperationException($"Property {keyString} not found");
            var keyValue = (int?)propertyInfo.GetValue(request, null)
                ?? throw new InvalidOperationException($"Property {keyString} has no value");

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

            var savedModel = ToEntity(request);
            if (uniqueFieldNames.Any())
            {
                var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, uniqueFieldNames, currentMaCSKCB, entity);
                if (isExistUniqueFields)
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

    private async Task<bool> CheckUniqueFieldsExist(TEntity entityRequest, List<string> uniqueFieldNames, string maCSKCB, TEntity? entity = null)
    {
        Expression<Func<TEntity, bool>> filter = PredicateBuilder.New<TEntity>(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(maCSKCB));
        Expression<Func<TEntity, bool>> filterSearch = PredicateBuilder.New<TEntity>();
        // update
        if (entity != null)
        {
            foreach (var item in uniqueFieldNames)
            {
                var propertyInfoRequest = entityRequest.GetType().GetProperty(item)
                    ?? throw new InvalidOperationException($"Property {item} not found");
                var valueRequest = ((string?)propertyInfoRequest.GetValue(entityRequest, null))?.ToLower();

                var propertyInfoEntity = entity.GetType().GetProperty(item)
                    ?? throw new InvalidOperationException($"Property {item} not found");
                var valueEntity = ((string?)propertyInfoEntity.GetValue(entity, null))?.ToLower();

                filterSearch = filterSearch.Or(x => valueRequest != valueEntity && EF.Property<string>(x, item).ToLower().Equals(valueRequest));
            }
        }
        else
        {
            foreach (var item in uniqueFieldNames)
            {
                var propertyInfo = entityRequest.GetType().GetProperty(item)
                    ?? throw new InvalidOperationException($"Property {item} not found");
                var value = ((string?)propertyInfo.GetValue(entityRequest, null))?.ToLower();

                filterSearch = filterSearch.Or(x => EF.Property<string>(x, item).ToLower().Equals(value));
            }
        }
        filter = filter.And(filterSearch);

        return await _cateRepos.AnyAsync(filter);
    }

    public async Task<ResponseModel<bool>> DeleteItem(int id, string keyString)
    {
        var funcName = "DeleteItem";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

            #region Validtion
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            TEntity? item = await _cateRepos.GetById(id);
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
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

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
            foreach (var item in data)
            {
                (item as dynamic).MA_CSKCB = currentMaCSKCB;

                if (item is not DMBacSi bs) continue;

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

            #region update Db
            _cateRepos.DeleteListByFilter(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(currentMaCSKCB));

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
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

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
                var allMaDVs = data.OfType<DMDichVu>().Select(x => x.MA_DICH_VU).Distinct().ToList();
                filter = filter.And(x => allMaDVs.Contains(EF.Property<string?>(x, nameof(DMDichVu.MA_DICH_VU))));
            }
            else if (type == typeof(DMThuoc))
            {
                var allMaDVs = data.OfType<DMThuoc>().Select(x => x.MA_THUOC).Distinct().ToList();
                filter = filter.And(x => allMaDVs.Contains(EF.Property<string?>(x, nameof(DMThuoc.MA_THUOC))));
            }
            else if (type == typeof(DMVTYT))
            {
                var allMaDVs = data.OfType<DMVTYT>().Select(x => x.MA_VAT_TU).Distinct().ToList();
                filter = filter.And(x => allMaDVs.Contains(EF.Property<string?>(x, nameof(DMVTYT.MA_VAT_TU))));
            }
            else if (type == typeof(DMBacSi))
            {
                var allMaDVs = data.OfType<DMBacSi>().Select(x => x.MA_CCHN).Distinct().ToList();
                filter = filter.And(x => allMaDVs.Contains(EF.Property<string?>(x, nameof(DMBacSi.MA_CCHN))));
            }
            else if (type == typeof(DMKhoaBV))
            {
                var allMaDVs = data.OfType<DMKhoaBV>().Select(x => x.MA_KHOA).Distinct().ToList();
                filter = filter.And(x => allMaDVs.Contains(EF.Property<string?>(x, nameof(DMKhoaBV.MA_KHOA))));
            }

            var oldData = (await _cateRepos.GetListAsync(filter)).ToList();
            var oldChg = new List<TEntity>();
            var newChg = new List<TEntity>();
            foreach (var item in data)
            {
                var isNew = true;
                if (item is DMDichVu dv)
                {
                    foreach (var chg in oldData)
                    {
                        if (chg is not DMDichVu old
                            || old.MA_DICH_VU != dv.MA_DICH_VU
                            || (!string.IsNullOrEmpty(dv.QUYET_DINH) && old.QUYET_DINH != dv.QUYET_DINH)
                            || (!string.IsNullOrEmpty(dv.TU_NGAY) && old.TU_NGAY != dv.TU_NGAY)
                            || (!string.IsNullOrEmpty(dv.DEN_NGAY) && old.DEN_NGAY != dv.DEN_NGAY))
                        {
                            continue;
                        }

                        old.DON_GIA = dv.DON_GIA;
                        old.MA_GIA = dv.MA_GIA;
                        old.TEN_DICH_VU = dv.TEN_DICH_VU;
                        oldChg.Add(chg);
                        isNew = false;
                    }
                }

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
            var roleId = GetClaim(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(maCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            Expression<Func<TEntity, bool>> filter = PredicateBuilder.New<TEntity>(x => EF.Property<string>(x, nameof(Hospital.MA_CSKCB)).Equals(maCSKCB));
            if (!string.IsNullOrEmpty(search))
            {
                filter = filter.And(x => EF.Property<string>(x, nameof(DichVuMaMay.MA_MAY)).Contains(search) || EF.Property<string>(x, nameof(DichVuMaMay.MA_DICH_VU)).Contains(search));
            }

            var orderQueryable = new List<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>>
            {
                q => q.OrderBy(x => EF.Property<string>(x, nameof(DichVuMaMay.MA_DICH_VU)))
            };

            var data = await _cateRepos.GetAllCustomSortAsync(filter, orderQueryable, null);

            res.Data = HandleFile130CommonService.DownloadExcel(data);
        }
        catch (Exception ex)
        {
            res = CategoryResponses.InternalError<byte[]>(ex, _logger, $"{serviceName} - {funcName}");
        }
        return res;
    }

    public async Task<ResponseModel<byte[]>?> DownloadExcelFile(string filterDownloadString, TEntity emptyModel)
    {
        var funcName = "DownloadExcelFile";
        var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            if (string.IsNullOrWhiteSpace(filterDownloadString)) return null;
            FilterDownloadModel request = JsonDefaults.Deserialize<FilterDownloadModel>(filterDownloadString) ?? new FilterDownloadModel();
            var queryable = BuildQueryToGetListData(request, emptyModel, currentMaCSKCB);

            #region Get List
            var data = await _cateRepos.GetAllCustomSortAsync(queryable.FilterQueryable, queryable.OrderQueryable, queryable.IncludeQueryable);
            if (!data.Any())
            {
                data = new List<TEntity> { emptyModel };
            }
            #endregion

            res.Data = HandleFile130CommonService.DownloadExcel(data);
        }
        catch (Exception ex)
        {
            res = CategoryResponses.InternalError<byte[]>(ex, _logger, $"{serviceName} - {funcName}");
        }
        return res;
    }

    #region Import from BH portal
    private async Task<DataTable?> Scrape_DMThuoc(CategoryImporter importer)
    {
        string? content = await importer.DMThuoc_GetFirstPage();
        if (string.IsNullOrEmpty(content) || !content.Contains('<') || !content.Contains('>'))
        {
            _errorScraping = ScrapeReadError;
            return null;
        }

        DataTable? dt = importer.DMThuoc_PeakTableCaption(content);
        if (dt == null || dt.Columns.Count < 1)
        {
            _errorScraping = ScrapeFormatError;
            return null;
        }

        if (!importer.DMThuoc_PeakContent(content, dt))
        {
            if (dt.Rows.Count < 1)
            {
                _errorScraping = ScrapeFormatError;
                return null;
            }

            return dt;
        }

        int pgCnt = importer.DMThuoc_PeakPageCount(content);
        if (pgCnt < 0)
        {
            _errorScraping = "Không đọc được số trang từ cổng BH";
            return dt;
        }

        for (int i = 1; i < pgCnt; i++)
        {
            var html = await importer.DMThuoc_GetNextPage(i);

            if (!importer.DMThuoc_PeakContent(html ?? string.Empty, dt)) break;
        }

        return dt;
    }

    private async Task<DataTable?> Scrape_DMDichVu(CategoryImporter importer)
    {
        string? content = await importer.DMDichVu_GetFirstPage();
        if (string.IsNullOrEmpty(content) || !content.Contains('<') || !content.Contains('>'))
        {
            _errorScraping = ScrapeReadError;
            return null;
        }

        DataTable? dt = importer.DMDichVu_PeakTableCaption(content);
        if (dt == null || dt.Columns.Count < 1)
        {
            _errorScraping = ScrapeFormatError;
            return null;
        }

        if (!importer.DMDichVu_PeakContent(content, dt))
        {
            if (dt.Rows.Count < 1)
            {
                _errorScraping = ScrapeFormatError;
                return null;
            }

            return dt;
        }

        int pgCnt = importer.DMDichVu_PeakPageCount(content);
        if (pgCnt < 0)
        {
            _errorScraping = "Không đọc được số trang từ cổng BH";
            return dt;
        }

        for (int i = 1; i < pgCnt; i++)
        {
            try
            {
                var html = await importer.DMDichVu_GetNextPage(i);
                if (string.IsNullOrEmpty(html) || !html.Contains('<') || !html.Contains('>'))
                {
                    _errorScraping = ScrapeReadError;
                    return null;
                }

                if (!importer.DMDichVu_PeakContent(html, dt)) break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{ServiceName}: skip page {Page} when importing DMDichVu", serviceName, i);
            }
        }

        return dt;
    }

    private async Task<DataTable?> Scrape_DMVatTu(CategoryImporter importer)
    {
        string? content = await importer.DMVatTu_GetFirstPage();
        if (string.IsNullOrEmpty(content) || !content.Contains('<') || !content.Contains('>'))
        {
            _errorScraping = ScrapeReadError;
            return null;
        }

        DataTable? dt = importer.DMVatTu_PeakTableCaption(content);
        if (dt == null || dt.Columns.Count < 1)
        {
            _errorScraping = ScrapeFormatError;
            return null;
        }

        if (!importer.DMVatTu_PeakContent(content, dt))
        {
            if (dt.Rows.Count < 1)
            {
                _errorScraping = ScrapeFormatError;
                return null;
            }

            return dt;
        }

        int pgCnt = importer.DMVatTu_PeakPageCount(content);
        if (pgCnt < 0)
        {
            _errorScraping = "Không đọc được số trang từ cổng BH";
            return dt;
        }

        for (int i = 1; i < pgCnt; i++)
        {
            var html = await importer.DMVatTu_GetNextPage(i);

            if (!importer.DMVatTu_PeakContent(html ?? string.Empty, dt)) break;
        }

        return dt;
    }

    private async Task<DataTable?> Scrape_DMBacSi(CategoryImporter importer)
    {
        string? content = await importer.DMBacSi_GetFirstPage();
        if (string.IsNullOrEmpty(content) || !content.Contains('<') || !content.Contains('>'))
        {
            _errorScraping = ScrapeReadError;
            return null;
        }

        DataTable? dt = importer.DMBacSi_PeakTableCaption(content);
        if (dt == null || dt.Columns.Count < 1)
        {
            _errorScraping = ScrapeFormatError;
            return null;
        }

        if (!importer.DMBacSi_PeakContent(content, dt))
        {
            if (dt.Rows.Count < 1)
            {
                _errorScraping = ScrapeFormatError;
                return null;
            }

            return dt;
        }

        int pgCnt = importer.DMBacSi_PeakPageCount(content);
        if (pgCnt < 0)
        {
            _errorScraping = "Không đọc được số trang từ cổng BH";
            return dt;
        }

        for (int i = 1; i < pgCnt; i++)
        {
            var html = await importer.DMBacSi_GetNextPage(i);

            if (!importer.DMBacSi_PeakContent(html ?? string.Empty, dt)) break;
        }

        return dt;
    }

    private async Task<DataTable?> Scrape_DMKhoa(CategoryImporter importer)
    {
        string? content = await importer.DMKhoa_GetFirstPage();
        if (string.IsNullOrEmpty(content) || !content.Contains('<') || !content.Contains('>'))
        {
            _errorScraping = ScrapeReadError;
            return null;
        }

        DataTable? dt = importer.DMKhoa_PeakTableCaption(content);
        if (dt == null || dt.Columns.Count < 1)
        {
            _errorScraping = ScrapeFormatError;
            return null;
        }

        if (!importer.DMKhoa_PeakContent(content, dt))
        {
            if (dt.Rows.Count < 1)
            {
                _errorScraping = ScrapeFormatError;
                return null;
            }

            return dt;
        }

        int pgCnt = importer.DMKhoa_PeakPageCount(content);
        if (pgCnt < 0)
        {
            _errorScraping = "Không đọc được số trang từ cổng BH";
            return dt;
        }

        for (int i = 1; i < pgCnt; i++)
        {
            var html = await importer.DMKhoa_GetNextPage(i);

            if (!importer.DMKhoa_PeakContent(html ?? string.Empty, dt)) break;
        }

        return dt;
    }

    /// <summary>Maps each row to an entity by column name (case-insensitive); empty cells keep the default value.</summary>
    private static List<TEntity> ToEntities(DataTable dt)
    {
        var props = typeof(TEntity).GetProperties()
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        var result = new List<TEntity>();
        foreach (DataRow row in dt.Rows)
        {
            var entity = Activator.CreateInstance<TEntity>();
            foreach (DataColumn col in dt.Columns)
            {
                if (!props.TryGetValue(col.ColumnName, out var prop)) continue;

                var value = row[col];
                if (value == DBNull.Value) continue;

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (targetType != typeof(string) && string.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture))) continue;

                prop.SetValue(entity, Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
            }
            result.Add(entity);
        }
        return result;
    }

    private async Task<List<TEntity>?> ScrapeFromAPI(string username, string password)
    {
        _errorScraping = "";
        DataTable? dt = null;
        Type t = typeof(TEntity);
        var importer = new CategoryImporter(username, password);

        try
        {
            if (t == typeof(DMThuoc))
            {
                dt = await Scrape_DMThuoc(importer);
            }
            else if (t == typeof(DMDichVu))
            {
                dt = await Scrape_DMDichVu(importer);
            }
            else if (t == typeof(DMVTYT))
            {
                dt = await Scrape_DMVatTu(importer);
            }
            else if (t == typeof(DMBacSi))
            {
                dt = await Scrape_DMBacSi(importer);
            }
            else if (t == typeof(DMKhoaBV))
            {
                dt = await Scrape_DMKhoa(importer);
            }

            return dt == null ? null : ToEntities(dt);
        }
        catch (Exception ex)
        {
            _errorScraping = ex.Message;
            return null;
        }
    }

    public async Task<bool> AllowImport()
    {
        var currentRole = GetClaim(ClaimTypes.Role);
        var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);
        if (string.IsNullOrEmpty(currentRole) || string.IsNullOrWhiteSpace(currentMaCSKCB)) return false;
        if (currentRole != RoleConstant.MASTER && currentRole != RoleConstant.ADMIN) return false;

        var hos = await _hospitalRepos.GetItem(h => h.MA_CSKCB == currentMaCSKCB);
        return hos != null && !string.IsNullOrEmpty(hos.Username) && !string.IsNullOrEmpty(hos.Password);
    }

    public async Task<ResponseModel<bool>> ImportFromAPI()
    {
        var funcName = "UploadCategoryFile";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var currentRole = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

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

            List<TEntity>? data = await ScrapeFromAPI(hos.Username, hos.Password);
            if (!string.IsNullOrEmpty(_errorScraping))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = _errorScraping;
                _errorScraping = "";
                return res;
            }
            if (data == null) throw new InvalidOperationException($"No data imported for {typeof(TEntity).Name}");

            foreach (var item in data)
            {
                (item as dynamic).MA_CSKCB = currentMaCSKCB;
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
    #endregion
}
