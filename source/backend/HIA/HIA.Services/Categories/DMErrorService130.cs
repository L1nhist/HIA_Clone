using System.Security.Claims;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Token;
using HIA.Repositories.Base;
using HIA.Services.SysHistoryLogManagement;
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Files;
using HIA.Data.Rules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HIA.Services.Categories;

public class DMErrorService130 : IDMErrorService130
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<DMError130> _dmErrorRepo;
    private readonly IBaseRepository<DMErrorMHTC> _dmErrorMHTCRepo;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<DMErrorService130> _logger;

    public DMErrorService130(
        IUnitOfWork unitOfWork,
        IBaseRepository<DMError130> dmErrorRepo,
        IBaseRepository<DMErrorMHTC> dmErrorMHTCRepo,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,
        ILogger<DMErrorService130> logger)
    {
        _unitOfWork = unitOfWork;
        _dmErrorRepo = dmErrorRepo;
        _dmErrorMHTCRepo = dmErrorMHTCRepo;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;
        _logger = logger;
    }

    private string GetClaim(string claimType)
        => _httpContext.HttpContext?.User.FindFirst(claimType)?.Value ?? string.Empty;

    private static DMErrorInfoModel ToInfo(DMError130 e, bool isSelected, bool isAlarm = false, bool isInternal = false)
        => new()
        {
            ErrorID = e.ErrorId,
            ErrorCode = e.ErrorCode,
            ErrorType = e.ErrorType,
            ErrorAtXMLFile = e.ErrorFile,
            ErrorAtXMLTag = e.ErrorTag,
            ErrorContent = e.Message,
            ErrorComment = e.Comment,
            IsSelected = isSelected,
            IsAlarm = isAlarm,
            IsInternal = isInternal
        };

    public async Task<ResponseModel<IEnumerable<SelectErrorsItems>>> GetSelectionErrorListByHospital()
    {
        var res = new ResponseModel<IEnumerable<SelectErrorsItems>>(HttpStatusCode.OK, "", new List<SelectErrorsItems>());
        try
        {
            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(maCSKCB)) return res;

            var errorList = await _dmErrorRepo.GetListAsync(e => e.IsDelete != true, q => q.OrderBy(e => e.ErrorCode), null);

            //Tam thoi chua filter error theo vien ma select all
            res.Data = errorList.Select(x => new SelectErrorsItems
            {
                Text = x.Message,
                Value = x.ErrorCode.ToString(),
            }).ToList();
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<IEnumerable<SelectErrorsItems>>(
                ex, new List<SelectErrorsItems>(), _logger, $"{nameof(DMErrorService130)} - {nameof(GetSelectionErrorListByHospital)}");
        }

        return res;
    }

    public async Task<IEnumerable<DMError130>?> GetAll(bool isDelete = false)
    {
        try
        {
            return isDelete ?
                await _dmErrorRepo.GetListAsync(e => e.IsDelete != true, y => y.OrderBy(o => o.ErrorCode), null) :
                await _dmErrorRepo.GetListAsync(null, y => y.OrderBy(o => o.ErrorCode), null);
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError<List<DMErrorInfoModel>?>(
                ex, null, _logger, $"{nameof(DMErrorService130)} - {nameof(GetAll)}");
        }
        return null;
    }

    public async Task<ResponseModel<List<DMErrorInfoModel>>> GetItems()
    {
        var res = new ResponseModel<List<DMErrorInfoModel>>(HttpStatusCode.OK, "", new List<DMErrorInfoModel>());
        try
        {
            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(maCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            #region Get List
            var errorList = await _dmErrorRepo.GetListAsync(e => e.IsDelete != true, y => y.OrderBy(o => o.ErrorCode), null);
            var data = errorList.Select(e => ToInfo(e, false)).ToList();
            res.Data = data;

            var userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null);
            foreach (var u in userErrList)
            {
                var e = data.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
                if (e == null) continue;

                e.IsSelected = true;
                e.IsAlarm = u.EMEnabled != true;
                e.IsInternal = u.EMInternal == true;
            }
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get List Error");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<List<DMErrorInfoModel>>(
                ex, new List<DMErrorInfoModel>(), _logger, $"{nameof(DMErrorService130)} - {nameof(GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> Save(DMErrorModifyModel request)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(maCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            #region Delete all user error
            _dmErrorMHTCRepo.DeleteListByFilter(x => x.MA_CSKCB.Equals(maCSKCB));
            #endregion

            #region Add new
            var items = new List<DMErrorMHTC>();
            var creator = GetClaim(ClaimTypes.NameIdentifier);

            if (request.Data.Count > 0)
            {
                foreach (var item in request.Data)
                {
                    if (!string.IsNullOrEmpty(item) && int.TryParse(item, out int errCode))
                    {
                        items.Add(new DMErrorMHTC
                        {
                            EMErrorCode = Math.Abs(errCode),
                            MA_CSKCB = maCSKCB,
                            EMCreateDate = DateTime.Now,
                            EMEnabled = errCode > 0,
                            EMCreator = creator
                        });
                    }
                }
            }
            else if (request.Items.Count > 0)
            {
                foreach (var item in request.Items)
                {
                    if (item != null && item.Length > 0 && int.TryParse(item[0], out int errCode))
                    {
                        items.Add(new DMErrorMHTC
                        {
                            EMErrorCode = Math.Abs(errCode),
                            MA_CSKCB = maCSKCB,
                            EMCreateDate = DateTime.Now,
                            EMEnabled = item.Length > 1 && item[1] == "1",
                            EMInternal = item.Length > 2 && item[2] == "1",
                            EMCreator = creator
                        });
                    }
                }
            }

            await _dmErrorMHTCRepo.InsertListAsync(items);
            await _unitOfWork.CommitAsync();
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Save List Error");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMErrorService130)} - {nameof(Save)}");
        }
        return res;
    }

    //Linhist added 01/04/23
    public async Task<ResponseModel<IEnumerable<DMErrorInfoModel>>> GetAlertErrorListByHospital()
    {
        var res = new ResponseModel<IEnumerable<DMErrorInfoModel>>(HttpStatusCode.OK, "", new List<DMErrorInfoModel>());
        try
        {
            var maCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            if (string.IsNullOrWhiteSpace(maCSKCB)) return res;

            var errorList = await _dmErrorRepo.GetListAsync(e => e.IsDelete != true, y => y.OrderBy(o => o.ErrorCode), null);

            //Tam thoi chua filter error theo vien ma select all
            res.Data = errorList.Select(e => ToInfo(e, true)).ToList();
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<IEnumerable<DMErrorInfoModel>>(
                ex, new List<DMErrorInfoModel>(), _logger, $"{nameof(DMErrorService130)} - {nameof(GetAlertErrorListByHospital)}");
        }

        return res;
    }

    public async Task<ResponseModel<List<DMError130>>> GetAllListing()
    {
        var res = new ResponseModel<List<DMError130>>(HttpStatusCode.OK, "", new List<DMError130>());
        try
        {
            var errorList = await _dmErrorRepo.GetListAsync(e => e.IsDelete != true, y => y.OrderBy(o => o.ErrorCode), null);
            res.Data = errorList.ToList();
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<List<DMError130>>(
                ex, new List<DMError130>(), _logger, $"{nameof(DMErrorService130)} - {nameof(GetAllListing)}");
        }
        return res;
    }

    public async Task<List<DMErrorInfoModel>?> GetAllByHospital(string maCSKCB)
    {
        try
        {
            var errorList = (await _dmErrorRepo.GetListAsync(e => e.IsDelete != true, y => y.OrderBy(o => o.ErrorCode), null)).ToList();
            var userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null);
            var res = new List<DMErrorInfoModel>();
            foreach (var u in userErrList)
            {
                var e = errorList.FirstOrDefault(a => a.ErrorCode == u.EMErrorCode);
                if (e == null) continue;

                res.Add(ToInfo(e, true, u.EMEnabled != true, u.EMInternal == true));
            }

            return res;
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError<List<DMErrorInfoModel>?>(
                ex, null, _logger, $"{nameof(DMErrorService130)} - {nameof(GetAllByHospital)}");
            return null;
        }
    }

    //Linhist added 07/03/24
    public async Task<ResponseModel<bool>> SaveDefaultErrors(string maCSKCB)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            if (string.IsNullOrWhiteSpace(maCSKCB))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            var errorList = await _dmErrorRepo.GetListAsync(e => e.IsDefault == true && e.IsDelete != true);
            if (!errorList.Any())
            {
                res.Data = false;
                return res;
            }

            #region Add new
            var creator = GetClaim(ClaimTypes.NameIdentifier);
            var items = new List<DMErrorMHTC>();

            foreach (var err in errorList)
            {
                items.Add(new DMErrorMHTC
                {
                    //Linhist changed 01/04/23 - ErrorCode > 0 --> Bao Loi, ErrorCode < 0 --> Canh Bao
                    EMErrorCode = err.ErrorCode,
                    MA_CSKCB = maCSKCB,
                    EMCreateDate = DateTime.Now,
                    EMEnabled = true,
                    EMCreator = creator
                });
            }
            await _dmErrorMHTCRepo.InsertListAsync(items);

            await _unitOfWork.CommitAsync();
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Save List Error");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMErrorService130)} - {nameof(SaveDefaultErrors)}");
        }
        return res;
    }

    public async Task<IEnumerable<DMError130>> GetAllByType(int type)
        => await _dmErrorRepo.GetListAsync(e => e.ErrorType == type && e.IsDelete != true, q => q.OrderBy(e => e.ErrorCode));

    public async Task<int> GetLastErrorCode()
        => await _dmErrorRepo.GetMinMax(true, e => e.ErrorCode ?? 0);

    public async Task<bool> InsertAndActived(DMError130 error, string maCSKCB)
    {
        try
        {
            await _dmErrorRepo.InsertAsync(error);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("Save List Error");

            //Sau khi them ma loi moi thi active luon cho vien do
            if (!string.IsNullOrEmpty(maCSKCB))
            {
                var errAppend = new DMErrorMHTC
                {
                    EMErrorCode = error.ErrorCode,
                    MA_CSKCB = maCSKCB,
                    EMCreateDate = DateTime.Now,
                    EMEnabled = true,
                };
                await _dmErrorMHTCRepo.InsertAsync(errAppend);
            }

            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMErrorService130)} - {nameof(InsertAndActived)}");
        }

        return false;
    }

    public async Task<DMError130?> UpdateError(int? errorCode, string message)
    {
        try
        {
            var error = await _dmErrorRepo.GetItem(x => x.ErrorCode == errorCode);
            if (error == null) return null;

            if (message != error.Message)
            {
                error.Message = message;
                _dmErrorRepo.Update(error);
                await _unitOfWork.CommitAsync();
            }
            return error;
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMErrorService130)} - {nameof(UpdateError)}");
        }

        return null;
    }

    public async Task<ResponseModel<IEnumerable<DMError130>>> UpdateErrorDescs(int[] errorCodes, string[] errorDescs)
    {
        var res = new ResponseModel<IEnumerable<DMError130>>(HttpStatusCode.NoContent, string.Empty, new List<DMError130>());
        try
        {
            var errors = (await _dmErrorRepo.GetListAsync(x => errorCodes.Contains(x.ErrorCode ?? 0))).ToList();
            if (errors.Count == 0)
            {
                res.Code = HttpStatusCode.NotFound;
                res.Message = MessageResource.ERR_400_NO_DATA;
            }

            for (int i = 0; i < errorCodes.Length; i++)
            {
                var err = errors.FirstOrDefault(x => x.ErrorCode == errorCodes[i]);
                if (err == null) continue;

                err.ErrorExplanation = errorDescs[i];
            }

            await _dmErrorRepo.UpdateListAsync(errors);
            await _dmErrorRepo.SaveChangesAsync();
            res.Data = errors;
            res.Code = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(DMErrorService130)} - {nameof(UpdateErrorDescs)}");

            res.Code = HttpStatusCode.InternalServerError;
            res.Message = ex.Message;
        }

        return res;
    }
}
