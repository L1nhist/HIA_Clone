using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.Data.Auths;
using HIA.Repositories.Base;
using HIA.Repositories.User;
using HIA.Services.Email;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;

namespace HIA.Services.UserManagement;

public class UserManagementService : IUserManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IBaseRepository<Hospital> _hospitalRepository;
    private readonly IBaseRepository<UserRole> _userRoleRepository;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly TokenHandler _tokenHandle;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<UserManagementService> _logger;
    public UserManagementService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IBaseRepository<UserRole> userRoleRepository,
        ISysHistoryLogService sysHistoryLogService,
        TokenHandler tokenHandle,
        IEmailService emailService,
        IHttpContextAccessor httpContext,
        ILogger<UserManagementService> logger,
        IBaseRepository<Hospital> hospitalRepository
        )
    {
        _unitOfWork = unitOfWork;
        _userRoleRepository = userRoleRepository;
        _userRepository = userRepository;
        _sysHistoryLogService = sysHistoryLogService;
        _tokenHandle = tokenHandle;
        _emailService = emailService;
        _httpContext = httpContext;
        _logger = logger;
        _hospitalRepository = hospitalRepository;
    }

    private HttpContext CurrentContext => _httpContext.HttpContext ?? throw new InvalidOperationException("No HttpContext");

    private string GetClaim(string claimType)
    {
        return CurrentContext.User.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Missing claim {claimType}");
    }

    #region GET DATA
    public async Task<ResponseModel<PaginationResponseModel<UserInfoModel>>> GetItems(PaginationRequestModel request)
    {
        var data = new PaginationResponseModel<UserInfoModel>();
        var res = new ResponseModel<PaginationResponseModel<UserInfoModel>>(HttpStatusCode.OK, "", data);

        try
        {
            var currentRole = GetClaim(ClaimTypes.Role);
            var currentHospitalId = GetClaim(ClaimKey.HOSPITAL_ID);
            var currentMaCSKCB = GetClaim(ClaimKey.MA_CSKCB);
            #region Validation
            if (string.IsNullOrWhiteSpace(currentRole) || currentRole.Equals(RoleConstant.USER))
            {
                res.Code = HttpStatusCode.Forbidden;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
            #endregion

            #region Build Filter Query
            Expression<Func<User, bool>> filter = PredicateBuilder.New<User>(x => !x.IsDeleted && x.Hospital != null && x.Hospital.IsActivated && !x.Hospital.IsDeleted);
            // add filter with role admin
            if (!string.IsNullOrWhiteSpace(currentHospitalId) && currentRole.Contains(RoleConstant.ADMIN) && currentMaCSKCB != "BOYTE")
            {
                filter = filter.And(x => x.HospitalId == currentHospitalId || (x.Hospital != null && x.Hospital.SuperHospital != null && x.Hospital.SuperHospital.Id == currentHospitalId));
            }
            // add filter when has searchKey
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.ToLower();
                filter = filter.And(x =>
                        (x.Hospital != null && (x.Hospital.Name ?? "").ToLower().Contains(key))
                        || (x.Email ?? "").ToLower().Contains(key)
                        || (x.FullName ?? "").ToLower().Contains(key)
                        || (x.PhoneNumber ?? "").ToLower().Contains(key)
                        || x.UserRoles.Any(u => (u.Role.Name ?? "").ToLower().Contains(key))
                    );
            }
            #endregion
            #region Build Sort Query
            IList<Func<IOrderedQueryable<User>, IOrderedQueryable<User>>> sortFuncList = new List<Func<IOrderedQueryable<User>, IOrderedQueryable<User>>>();

            var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn ?? string.Empty);
            // master always sort by hospotal
            if (currentRole.Contains(RoleConstant.MASTER) || currentMaCSKCB == "BOYTE")
            {
                if (columnName == nameof(UserInfoModel.HospitalName) && request.OrderType == "desc")
                    sortFuncList.Add((ordered) => ordered.ThenByDescending(o => o.HospitalId));
                else
                    sortFuncList.Add((ordered) => ordered.ThenBy(o => o.HospitalId));
            }
            // dynamic sort by column
            if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
            {
                switch (columnName)
                {
                    case nameof(UserInfoModel.HospitalName):
                        // ignore
                        break;
                    case nameof(UserInfoModel.RoleName):
                        if (request.OrderType == "asc")
                            sortFuncList.Add(x => x.ThenBy(y => y.UserRoles.Select(r => r.Role.Name).FirstOrDefault()));
                        else
                            sortFuncList.Add(x => x.ThenByDescending(y => y.UserRoles.Select(r => r.Role.Name).FirstOrDefault()));
                        break;
                    default:
                        sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
                        break;
                }
            }
            #endregion
            #region Get User
            PaginationResponseModel<User> users = await _userRepository
                    .GetListPaginationCustomSortAsync(
                        request.PageIndex,
                        request.PageNumber,
                        filter,
                        sortFuncList,
                        i => i.Include(i1 => i1.UserRoles).ThenInclude(i11 => i11.Role).Include(i2 => i2.Hospital));

            data.Items = users.Items.Select(s => new UserInfoModel
            {
                Id = s.Id,
                Email = s.Email,
                FullName = s.FullName,
                PhoneNumber = s.PhoneNumber,
                RoleId = string.Join(",", s.UserRoles.Select(x => x.RoleId).ToArray()),
                RoleName = string.Join(",", s.UserRoles.Select(x => x.Role.Name).ToArray()),
                HospitalId = s.HospitalId,
                HospitalName = s.Hospital?.Name,
                IsActivated = s.IsActivated,
                IsFirstTimeLogin = s.IsFirstTimeLogin
            }).ToList();
            data.TotalItems = users.TotalItems;
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Get user list by filter");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new PaginationResponseModel<UserInfoModel>(), _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.GetItems)}");
        }
        return res;
    }

    public async Task<ResponseModel<ModifyUserModel>> GetItemById(string id)
    {
        var res = new ResponseModel<ModifyUserModel>(HttpStatusCode.OK, "", new ModifyUserModel());

        try
        {
            #region Validation
            if (string.IsNullOrWhiteSpace(id))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                return res;
            }
            #endregion

            var item = await _userRepository.GetItem(
                x => x.Id == id,
                y => y.Include(t => t.UserRoles).ThenInclude(t1 => t1.Role).Include(t2 => t2.Hospital))
                ?? throw new InvalidOperationException($"User not found: {id}");

            var itemData = new ModifyUserModel();
            itemData.MapFrom(item);
            itemData.Password = Uti.PasswordDecrypt(itemData.Password);
            itemData.RoleId = string.Join(",", item.UserRoles.Select(x => x.RoleId).ToArray());
            itemData.RoleName = string.Join(",", item.UserRoles.Select(x => x.Role.Name).ToArray());
            itemData.HospitalName = item.Hospital?.Name;
            itemData.HospitalId = item.Hospital?.MA_CSKCB ?? string.Empty;

            res.Data = itemData;
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, new ModifyUserModel(), _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.GetItemById)}");
        }

        return res;
    }
    #endregion

    #region UPDATE DATA
    public async Task<ResponseModel<bool>> CreateNewItem(NewUserModel request, string newUserUrl)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var currentRole = GetClaim(ClaimTypes.Role);
            var validateRequestModel = request.ToModifyUserModel();

            #region Validtion
            res = ValidateModifyItem(validateRequestModel, currentRole, false);
            if (res.Data == false) return res;

            res = await CheckEmailExist(request.Email);
            if (res.Data == false) return res;
            #endregion

            #region add user and userRole
            request.Password = Uti.PasswordEncrypt(request.IsValidated ? "123456" : Uti.RandomGuidString());
            request.Email = request.Email.Trim();
            request.IsFirstTimeLogin = !request.IsValidated;
            var savedModel = request.MapTo() ?? throw new InvalidOperationException("Cannot map NewUserModel to User");
            savedModel.Hospital = null; // MapTo does not set Hospital; keep null as before
            savedModel.HospitalId = await UpdateHospitalIdWhenRoleAdmin(currentRole, savedModel.HospitalId);

            await _userRepository.InsertAsBaseEntityAsync(savedModel);

            var userRole = new UserRole
            {
                RoleId = request.RoleId,
                UserId = savedModel.Id
            };
            await _userRoleRepository.InsertAsync(userRole);

            await _unitOfWork.CommitAsync();
            #endregion

            #region sent email confirm
            if (!request.IsValidated)
            {
                var token = _tokenHandle.BuildToken(new TokenRequestModel
                {
                    Id = savedModel.Id,
                    Type = TypeUpdatePass.NEW_USER.ToString()
                }).Token;
                var url = $"{Uti.GetFullDomainUrl(CurrentContext)}/{newUserUrl}?token={token}";
                _emailService.SendEmailNewUser(url, request.FullName, request.Email, true);
            }
            #endregion

            // write log to db
            await _sysHistoryLogService.AddSysLog("Create new user");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.CreateNewItem)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> UpdateItem(ModifyUserModel request)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            var currentRole = GetClaim(ClaimTypes.Role);
            #region Validtion
            res = ValidateModifyItem(request, currentRole);
            if (res.Data == false) return res;

            var item = await _userRepository.GetItem(
                x => !string.IsNullOrWhiteSpace(request.Id) && x.Id == request.Id,
                y => y.Include(t => t.UserRoles).ThenInclude(t1 => t1.Role));
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            #endregion

            item.HospitalId = await UpdateHospitalIdWhenRoleAdmin(currentRole, request.HospitalId);
            // update email
            if (!string.Equals(item.Email, request.Email))
            {
                res = await CheckEmailExist(request.Email);
                if (res.Data == false) return res;
                item.Email = request.Email.Trim();
            }
            item.Password = Uti.PasswordEncrypt(request.Password.Trim());
            item.FullName = request.FullName.Trim();
            item.PhoneNumber = request.PhoneNumber?.Trim();
            item.IsActivated = request.IsActivated;

            if (!item.UserRoles.Any(x => x.RoleId == request.RoleId))
            {
                _userRoleRepository.DeleteList(item.UserRoles.ToList());

                var userRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = request.RoleId,
                        UserId = item.Id
                    }
                };
                item.UserRoles = new List<UserRole>();

                _userRoleRepository.InsertList(userRoles);
            }

            _userRepository.UpdateAsBaseEntity(item);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("Update user");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.CreateNewItem)}");
        }
        return res;
    }

    private async Task<string?> UpdateHospitalIdWhenRoleAdmin(string currentRole, string? hospitalIdRequest)
    {
        var hospitalId = currentRole.Contains(RoleConstant.MASTER) || currentRole.Contains(RoleConstant.ADMIN)
            ? hospitalIdRequest
            : GetClaim(ClaimKey.HOSPITAL_ID);

        var hospital = await _hospitalRepository.GetItem(x => x.MA_CSKCB == hospitalId || x.Id == hospitalId);

        if (hospital != null) return hospital.Id;

        return hospitalId;
    }

    private async Task<ResponseModel<bool>> CheckEmailExist(string name)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        var lowerName = name.Trim().ToLower();
        var isExist = await _userRepository.AnyAsync(x => (x.Email ?? "").ToLower() == lowerName && !x.IsDeleted);
        if (isExist)
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_400_EXIST_EMAIL;
            res.Data = false;
            return res;
        }
        return res;
    }

    private static ResponseModel<bool> ValidateModifyItem(ModifyUserModel request, string currentRole, bool isCheckPassRequired = true)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        if (string.IsNullOrWhiteSpace(request.Email) || (string.IsNullOrWhiteSpace(request.Password) && isCheckPassRequired))
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_400_REQUIRED_EMAIL_PASSWORD;
            res.Data = false;
            return res;
        }
        if (!currentRole.Contains(RoleConstant.MASTER) && request.RoleId.Equals(RoleConstant.MASTER))
        {
            res.Code = HttpStatusCode.Forbidden;
            res.Message = MessageResource.ERR_403_FORBIDDEN;
            res.Data = false;
            return res;
        }
        return res;
    }

    public async Task<ResponseModel<bool>> DeleteItem(string id)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            #region Validtion
            var item = await _userRepository.GetById(id);
            if (item == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }
            #endregion

            item.IsActivated = false;
            item.IsDeleted = true;
            _userRepository.UpdateAsBaseEntity(item);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("Soft Delete user");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.CreateNewItem)}");
        }
        return res;
    }
    #endregion

    #region Handle Password
    public async Task<ResponseModel<bool>> ResetPassword(string request, User model)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
        try
        {
            model.Password = Uti.PasswordEncrypt(request.Trim());
            model.IsFirstTimeLogin = false;
            _userRepository.UpdateAsBaseEntity(model);

            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("Reset Password");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.ResetPassword)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> ChangePassword(string currentPass, string newPassword)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);

        try
        {
            var currentId = GetClaim(ClaimTypes.Sid);
            if (string.IsNullOrWhiteSpace(currentId))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }

            var user = await _userRepository.GetItem(x => x.Id == currentId && x.IsActivated && !x.IsDeleted && !x.IsFirstTimeLogin);
            if (user == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INVALID_ID;
                res.Data = false;
                return res;
            }

            var currentPassHash = Uti.PasswordEncrypt(currentPass.Trim());
            if (!currentPassHash.Equals(user.Password))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_INCORRECT_CURRENT_PASSWORD;
                res.Data = false;
                return res;
            }

            user.Password = Uti.PasswordEncrypt(newPassword.Trim());
            _userRepository.UpdateAsBaseEntity(user);
            await _unitOfWork.CommitAsync();

            // write log to db
            await _sysHistoryLogService.AddSysLog("Change Password");
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(UserManagementService)} - {nameof(UserManagementService.ChangePassword)}");
        }
        return res;
    }
    #endregion

    public async Task<ResponseModel<User>> GetActiveUserAndHospital(string? idOrEmail, string? maCSKCB, string? role)
    {
        //Lấy thông tin user trong Database kèm bệnh viện trực thuộc
        idOrEmail = idOrEmail?.Trim().ToLower();
        maCSKCB = maCSKCB?.Trim().ToLower();
        role = role?.Trim().ToLower();
        var res = new ResponseModel<User>(HttpStatusCode.OK, "", null);
        if (string.IsNullOrEmpty(idOrEmail) || string.IsNullOrEmpty(maCSKCB) || string.IsNullOrEmpty(role))
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_403_FORBIDDEN;

            return res;
        }

        var usr = await _userRepository.GetItem(x => (x.Id == idOrEmail || (x.Email ?? "").ToLower() == idOrEmail) && x.IsActivated && !x.IsDeleted, q => q.Include(u => u.Hospital));

        //Kiểm tra nếu user hoặc bệnh viện bị khóa thì trả về lỗi Forbidden
        if (usr == null || usr.Hospital == null || (usr.Hospital.MA_CSKCB == maCSKCB && (!usr.Hospital.IsActivated || usr.Hospital.IsDeleted)))
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_403_FORBIDDEN;
            return res;
        }

        res.Data = usr;
        //Nếu maCSKCB trong token cùng giống viện trực thuộc thì trả về ok
        var hospital = usr.Hospital;

        if (hospital.MA_CSKCB == maCSKCB) return res;

        //Trường hợp maCSKCB khác nhau:
        if (role != RoleConstant.MASTER && hospital.MA_CSKCB != "00")
        {
            var userHospitalCode = hospital.MA_CSKCB;
            if (userHospitalCode == null
                || (userHospitalCode.Length < 5 && !maCSKCB.StartsWith(userHospitalCode))
                || (userHospitalCode.Length >= 5 && !maCSKCB.Equals(userHospitalCode)))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }
        }

        //Kiểm tra lại thông tin bệnh viện trong Database
        var dbHospital = await _hospitalRepository.GetItem(h => h.MA_CSKCB == maCSKCB && h.IsActivated && !h.IsDeleted);
        if (dbHospital == null)
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = MessageResource.ERR_403_FORBIDDEN;
            return res;
        }

        usr.Hospital = dbHospital;
        return res;
    }
}
