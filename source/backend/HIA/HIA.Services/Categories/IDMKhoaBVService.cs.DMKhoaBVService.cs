using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Repositories.Base;
using HIA.Services.Base;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Categories;

public class DMKhoaBVService : IDMKhoaBVService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<DMKhoa> _dMKhoaRepos;
    private readonly IBaseRepository<DMKhoaBV> _dMKhoaBVRepo;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<DMKhoaBVService> _logger;

    public DMKhoaBVService(
        IUnitOfWork unitOfWork,
        IBaseRepository<DMKhoa> dMKhoaRepos,
        IBaseRepository<DMKhoaBV> dMKhoaBVRepo,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        ILogger<DMKhoaBVService> logger)
    {
        _unitOfWork = unitOfWork;
        _dMKhoaRepos = dMKhoaRepos;
        _dMKhoaBVRepo = dMKhoaBVRepo;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;
        _logger = logger;
    }

    private string GetClaim(string claimType)
        => _httpContext.HttpContext?.User.FindFirst(claimType)?.Value ?? string.Empty;

    #region DMKHoa
    public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionList()
    {
        var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
        try
        {
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);

            if (string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            var items = await _dMKhoaBVRepo.GetListAsync(
                x => x.MA_CSKCB.Equals(currentMaCSKCB),
                o => o.OrderBy(o1 => o1.TEN_KHOA));
            res.Data = items.Select(s => new SelectListItem
            {
                Text = s.TEN_KHOA,
                Value = s.MA_KHOA
            });
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<IEnumerable<SelectListItem>>(
                ex, new List<SelectListItem>(), _logger, $"{nameof(DMKhoaBVService)} - {nameof(GetSelectionList)}");
        }

        return res;
    }

    public async Task<ResponseModel<List<DMKhoaBVInfoModel>>> GetItems()
    {
        var res = new ResponseModel<List<DMKhoaBVInfoModel>>(HttpStatusCode.OK, "", new List<DMKhoaBVInfoModel>());
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

            #region Get List
            var khoaList = await _dMKhoaRepos.GetListAsync(null, y => y.OrderBy(o => o.KhoaName), null);
            var khoaBVList = (await _dMKhoaBVRepo.GetListAsync(x => x.MA_CSKCB.Equals(currentMaCSKCB), null, null)).ToList();

            List<DMKhoaBVInfoModel> items;
            if (roleId.Equals(RoleConstant.USER))
            {
                items = khoaList.Where(x => khoaBVList.Any(y => y.MA_KHOA == x.KhoaCode))
                    .Select(x => new DMKhoaBVInfoModel
                    {
                        KhoaID = x.KhoaID,
                        KhoaCode = x.KhoaCode,
                        KhoaName = x.KhoaName,
                        IsSelected = true
                    }).ToList();
            }
            else
            {
                items = khoaList.Select(x => new DMKhoaBVInfoModel
                {
                    KhoaID = x.KhoaID,
                    KhoaCode = x.KhoaCode,
                    KhoaName = x.KhoaName,
                    IsSelected = khoaBVList.Any(y => y.MA_KHOA == x.KhoaCode)
                }).ToList();
            }
            res.Data = items;
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get List DMKhoa");
        }
        catch (Exception ex)
        {
            res = CategoryResponses.InternalError<List<DMKhoaBVInfoModel>>(
                ex, _logger, $"{nameof(DMKhoaBVService)} - {nameof(GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> Save(DMKhoaBVModifyModel request)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(roleId) || roleId.Equals(RoleConstant.USER) || string.IsNullOrWhiteSpace(currentMaCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            #region Delete all user error
            _dMKhoaBVRepo.DeleteListByFilter(x => x.MA_CSKCB.Equals(currentMaCSKCB));
            #endregion
            #region Add new
            var items = new List<DMKhoaBV>();
            foreach (var item in request.Data)
            {
                items.Add(new DMKhoaBV
                {
                    MA_CSKCB = currentMaCSKCB,
                    MA_KHOA = item.KhoaCode,
                    TEN_KHOA = item.KhoaName,
                    TU_NGAY = DateTime.Now.ToString()
                });
            }
            await _dMKhoaBVRepo.InsertListAsync(items);

            await _unitOfWork.CommitAsync();
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Save List DMKhoaBV");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMKhoaBVService)} - {nameof(Save)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> DeleteItem(int id)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var roleId = GetClaim(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(roleId) || roleId.Equals(RoleConstant.USER))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            #region Validtion
            var item = await _dMKhoaRepos.GetById(id);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            var khoaCode = item.KhoaCode;
            if (await _dMKhoaBVRepo.AnyAsync(x => x.MA_KHOA == khoaCode))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_500_DMKHOABV_EXIST;
                res.Data = false;
                return res;
            }
            #endregion

            _dMKhoaRepos.Delete(item);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("Delete DMKhoa");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMKhoaBVService)} - {nameof(DeleteItem)}");
        }
        return res;
    }
    #endregion

    #region DMKhoa New
    private static FilterQueryableModel<DMKhoaBV> BuildQueryToGetListData(FilterDownloadModel request, string maCSKCB)
    {
        var result = new FilterQueryableModel<DMKhoaBV>();

        #region Build Filter Query
        Expression<Func<DMKhoaBV, bool>> filter = PredicateBuilder.New<DMKhoaBV>(x => x.MA_CSKCB.Equals(maCSKCB));
        // add filter when has searchKey
        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            var key = request.SearchKey.ToLower();
            Expression<Func<DMKhoaBV, bool>> filterSearch = PredicateBuilder.New<DMKhoaBV>();
            foreach (PropertyInfo propertyInfo in typeof(DMKhoaBV).GetProperties())
            {
                if (request.IgnoreKeysForSearch?.Contains(propertyInfo.Name) == true) continue;

                filterSearch = PredicateExpresion<DMKhoaBV>.BuildExpressionForFilter(filterSearch, propertyInfo, key);
            }
            filter = filter.And(filterSearch);
        }
        result.FilterQueryable = filter;
        #endregion

        #region Build Sort Query
        var sortFuncList = new List<Func<IOrderedQueryable<DMKhoaBV>, IOrderedQueryable<DMKhoaBV>>>();

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

    public async Task<ResponseModel<PaginationResponseModel<DMKhoaBVInfoBasicModel>>> GetDMKhoaBVItems(PaginationRequestModel request, List<string> ignoreKeysForSearch)
    {
        var funcName = "GetItems DMKhoa New";
        var res = new ResponseModel<PaginationResponseModel<DMKhoaBVInfoBasicModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<DMKhoaBVInfoBasicModel>());
        try
        {
            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(maCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            var filterModel = new FilterDownloadModel();
            ((IMapable<PaginationRequestModel>)filterModel).MapFrom(request);
            filterModel.IgnoreKeysForSearch = ignoreKeysForSearch;
            var queryable = BuildQueryToGetListData(filterModel, maCSKCB);

            PaginationResponseModel<DMKhoaBV> items = await _dMKhoaBVRepo.GetListPaginationCustomSortAsync(
                request.PageIndex, request.PageNumber, queryable.FilterQueryable, queryable.OrderQueryable, queryable.IncludeQueryable);
            var itemInfo = items.Items.Select(x =>
            {
                var m = new DMKhoaBVInfoBasicModel();
                m.MapFrom(x);
                return m;
            }).ToList();

            res.Data = new PaginationResponseModel<DMKhoaBVInfoBasicModel>(items.TotalItems, itemInfo);

            // write log to db
            await _sysHistoryLogService.AddSysLog($"{nameof(DMKhoaBV)} - {funcName}");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new PaginationResponseModel<DMKhoaBVInfoBasicModel>(), _logger, $"{nameof(DMKhoaBVService)} - {funcName}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> CreateDMKhoaBVNewItem(DMKhoaBVModifyBasicModel request)
    {
        var funcName = "CreateNewItem DMKhoaBV New";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);

            #region Validtion
            if (string.IsNullOrWhiteSpace(maCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                res.Data = false;
                return res;
            }
            if (string.IsNullOrWhiteSpace(request.MA_KHOA) || string.IsNullOrWhiteSpace(request.TEN_KHOA))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = $"{DisplayFieldResource.MA_KHOA} {DisplayFieldResource.OR_TEXT} {DisplayFieldResource.TEN_KHOA} {MessageResource.ERR_400_REQUIRED}";
                res.Data = false;
                return res;
            }

            var savedModel = request.MapTo() ?? throw new InvalidOperationException("Cannot map DMKhoaBV model to DMKhoaBV");
            savedModel.MA_CSKCB = maCSKCB;
            savedModel.TU_NGAY = DateTime.Now.ToString();

            var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, maCSKCB, true);
            if (isExistUniqueFields)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = $"{DisplayFieldResource.MA_KHOA} {DisplayFieldResource.OR_TEXT} {DisplayFieldResource.TEN_KHOA} {MessageResource.ERR_400_EXIST}.";
                res.Data = false;
                return res;
            }
            #endregion

            var dmKhoa = new DMKhoa
            {
                KhoaCode = request.MA_KHOA,
                KhoaName = request.TEN_KHOA
            };
            await _dMKhoaRepos.InsertAsync(dmKhoa);
            await _dMKhoaBVRepo.SaveChangesAsync();

            await _dMKhoaBVRepo.InsertAsync(savedModel);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog($"{nameof(DMKhoaBV)} - {funcName}");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(DMKhoaBVService)} - {funcName}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> UpdateDMKhoaBVItem(DMKhoaBVModifyBasicModel request)
    {
        var funcName = "UpdateItem DMKhoaBV New";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            #region Validtion
            if (string.IsNullOrWhiteSpace(request.MA_KHOA) || string.IsNullOrWhiteSpace(request.TEN_KHOA) || string.IsNullOrWhiteSpace(request.MA_CSKCB)
                || request.KBVID <= 0)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = $"{DisplayFieldResource.MA_KHOA} " +
                    $"{DisplayFieldResource.OR_TEXT} {DisplayFieldResource.TEN_KHOA} " +
                    $"{DisplayFieldResource.OR_TEXT} {DisplayFieldResource.MA_CSKCB} " +
                    $"{MessageResource.ERR_400_REQUIRED}";
                res.Data = false;
                return res;
            }
            var savedModel = request.MapTo() ?? throw new InvalidOperationException("Cannot map DMKhoaBV model to DMKhoaBV");
            var isExistUniqueFields = await CheckUniqueFieldsExist(savedModel, request.MA_CSKCB, false);
            if (isExistUniqueFields)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = $"{DisplayFieldResource.TEN_KHOA} {MessageResource.ERR_400_EXIST}.";
                res.Data = false;
                return res;
            }
            #endregion

            _dMKhoaBVRepo.Update(savedModel);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog($"{nameof(DMKhoaBV)} - {funcName}");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(DMKhoaBVService)} - {funcName}");
        }
        return res;
    }

    private async Task<bool> CheckUniqueFieldsExist(DMKhoaBV entityRequest, string maCSKCB, bool isCreate)
    {
        var maKhoa = (entityRequest.MA_KHOA ?? string.Empty).ToLower();
        var tenKhoa = (entityRequest.TEN_KHOA ?? string.Empty).ToLower();

        var isExist = isCreate
            ? await _dMKhoaBVRepo.AnyAsync(x => x.MA_CSKCB.Equals(maCSKCB)
                && ((x.MA_KHOA ?? string.Empty).ToLower() == maKhoa || (x.TEN_KHOA ?? string.Empty).ToLower() == tenKhoa))
            : await _dMKhoaBVRepo.AnyAsync(x => x.MA_CSKCB.Equals(maCSKCB) && (x.TEN_KHOA ?? string.Empty).ToLower() == tenKhoa);
        if (isExist) return isExist;

        if (isCreate)
            isExist = await _dMKhoaRepos.AnyAsync(x => (x.KhoaCode ?? string.Empty).ToLower() == maKhoa);

        return isExist;
    }

    public async Task<ResponseModel<bool>> DeleteDMKhoaBVItem(int id)
    {
        var funcName = "DeleteItem DMKhoaBV New";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            #region Validtion
            DMKhoaBV? item = await _dMKhoaBVRepo.GetById(id);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            #endregion

            _dMKhoaBVRepo.Delete(item);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog($"{nameof(DMKhoaBV)} - {funcName}");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(DMKhoaBVService)} - {funcName}");
        }
        return res;
    }
    #endregion

    public async Task<ResponseModel<bool>> UploadCategoryFile(IFormFile request)
    {
        var funcName = "UploadCategoryFile DMKhoaBV New";
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            List<DMKhoaBV> data = HandleFile130CommonService.UploadExcel<DMKhoaBV>(request, new List<string> { nameof(DMKhoaBV.KBVID) });

            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            foreach (var item in data)
            {
                item.MA_CSKCB = maCSKCB;
            }

            var savedData = data.Where(x => !string.IsNullOrWhiteSpace(x.MA_KHOA) && !string.IsNullOrWhiteSpace(x.TEN_KHOA)).ToList();

            if (savedData.Count == 0)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_NO_DATA;
                res.Data = false;
                return res;
            }

            var allDMKhoa = (await _dMKhoaRepos.GetListAsync()).ToList();
            foreach (var item in savedData)
            {
                var isExist = allDMKhoa.Any(x => x.KhoaCode == item.MA_KHOA);
                if (isExist) continue;

                var dmKhoa = new DMKhoa
                {
                    KhoaCode = item.MA_KHOA,
                    KhoaName = item.TEN_KHOA
                };
                await _dMKhoaRepos.InsertAsync(dmKhoa);
                await _dMKhoaBVRepo.SaveChangesAsync();
            }

            #region update Db
            //Linhist changed 08/03/23
            //Chi xoa DMKhoa theo MA_CSKCB
            await _dMKhoaBVRepo.DeleteListByFilterAsync(k => k.MA_CSKCB == maCSKCB);

            await _dMKhoaBVRepo.InsertListAsync(savedData);

            await _unitOfWork.CommitAsync();
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog($"{nameof(DMKhoaBV)} - {funcName}");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(DMKhoaBVService)} - {funcName}");
        }
        return res;
    }
}
