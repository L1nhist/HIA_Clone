using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.User;
using HIA.Services.Email;
using HIA.Services.HospitalManagement;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Services.UserManagement
{
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
        private readonly IMapper _mapper;
        private readonly ILogger<UserManagementService> _logger;
        public UserManagementService(
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            IBaseRepository<UserRole> userRoleRepository,
            ISysHistoryLogService sysHistoryLogService,
            TokenHandler tokenHandle,
            IEmailService emailService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
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
            _mapper = mapper;
            _logger = logger;
            _hospitalRepository = hospitalRepository;
        }

        #region GET DATA
        public async Task<ResponseModel<PaginationResponseModel<UserInfoModel>>> GetItems(PaginationRequestModel request)
        {
            var res = new ResponseModel<PaginationResponseModel<UserInfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<UserInfoModel>());

            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentHospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                #region Validation
                if (string.IsNullOrWhiteSpace(currentRole) || currentRole.Equals(RoleConstant.USER))
                {
                    res.Code = HttpStatusCode.Forbidden;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                #endregion

                #region Build Filter Query
                Expression<Func<User, bool>> filter = PredicateBuilder.New<User>(x => !x.IsDeleted && !!x.Hospital.IsActivated && !x.Hospital.IsDeleted);
                // add filter with role admin
                if (!string.IsNullOrWhiteSpace(currentHospitalId) && currentRole.Contains(RoleConstant.ADMIN)&&currentMaCSKCB != "BOYTE")
                {
                    filter = filter.And(x => x.HospitalId.Equals(currentHospitalId)||x.Hospital.SuperHospital.Id==currentHospitalId);
                }
                // add filter when has searchKey
                if (!string.IsNullOrWhiteSpace(request.SearchKey))
                {
                    var key = request.SearchKey.ToLower();
                    filter = filter.And(x =>
                            x.Hospital.Name.ToLower().Contains(key)
                            || x.Email.ToLower().Contains(key)
                            || x.FullName.ToLower().Contains(key)
                            || x.PhoneNumber.ToLower().Contains(key)
                            || x.UserRoles.Any(u => u.Role.Name.ToLower().Contains(key))
                        );
                }
                #endregion
                #region Build Sort Query
                IList<Func<IOrderedQueryable<User>, IOrderedQueryable<User>>> sortFuncList = new List<Func<IOrderedQueryable<User>, IOrderedQueryable<User>>>();

                var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
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
                                sortFuncList.Add(x => x.ThenBy(y => y.UserRoles.FirstOrDefault().Role.Name));
                            else
                                sortFuncList.Add(x => x.ThenByDescending(y => y.UserRoles.FirstOrDefault().Role.Name));
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

                res.Data.Items = users?.Items?.Select(s => new UserInfoModel
                {
                    Id = s.Id,
                    Email = s.Email,
                    FullName = s.FullName,
                    PhoneNumber = s.PhoneNumber,
                    RoleId = string.Join(",", s.UserRoles.Select(x => x.RoleId).ToArray()),
                    RoleName = string.Join(",", s.UserRoles.Select(x => x.Role.Name).ToArray()),
                    HospitalId = s.HospitalId,
                    HospitalName = s.Hospital.Name,
                    IsActivated = s.IsActivated,
                    IsFirstTimeLogin = s.IsFirstTimeLogin
                }).ToList();
                res.Data.TotalItems = users.TotalItems;
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
                    x => x.Id.Equals(id),
                    y => y.Include(t => t.UserRoles).ThenInclude(t1 => t1.Role).Include(t2 => t2.Hospital));

                var itemData = _mapper.Map<ModifyUserModel>(item);
                itemData.Password = Uti.PasswordDecrypt(itemData.Password);
                itemData.RoleId = string.Join(",", item.UserRoles.Select(x => x.RoleId).ToArray());
                itemData.RoleName = string.Join(",", item.UserRoles.Select(x => x.Role.Name).ToArray());
                itemData.HospitalName = item.Hospital.Name;
                itemData.HospitalId = item.Hospital.MA_CSKCB;

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
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var validateRequestModel = _mapper.Map<ModifyUserModel>(request);

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
                var savedModel = _mapper.Map<User>(request);
                savedModel.Hospital = null; // after automapper, hospital is set empty model --> must set null
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
                    })?.Token;
                    var url = $"{Uti.GetFullDomainUrl(_httpContext.HttpContext)}/{newUserUrl}?token={token}";
                    _emailService.SendEmailNewUser(url, savedModel.FullName, savedModel.Email, true);
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
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                User item;
                #region Validtion
                res = ValidateModifyItem(request, currentRole);
                if (res.Data == false) return res;


                item = await _userRepository.GetItem(
                    x => !string.IsNullOrWhiteSpace(request.Id) && x.Id.Equals(request.Id),
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
                if (!item.Email.Equals(request.Email))
                {
                    res = await CheckEmailExist(request.Email);
                    if (res.Data == false) return res;
                    item.Email = request.Email.Trim();
                }
                item.Password = Uti.PasswordEncrypt(request.Password.Trim());
                item.FullName = request.FullName?.Trim();
                item.PhoneNumber = request.PhoneNumber?.Trim();
                item.IsActivated = request.IsActivated;

                if (!item.UserRoles.Any(x => x.RoleId == request.RoleId))
                {
                    _userRoleRepository.DeleteList(item.UserRoles.ToList());

                    var userRoles = new List<UserRole>();
                    userRoles.Add(new UserRole
                    {
                        RoleId = request.RoleId,
                        UserId = item.Id
                    });
                    item.UserRoles = null;

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

        private async Task<string> UpdateHospitalIdWhenRoleAdmin(string currentRole, string hospitalIdRequest)
        {
            var hospitalId = currentRole.Contains(RoleConstant.MASTER) || currentRole.Contains(RoleConstant.ADMIN)
                ? hospitalIdRequest
                : _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;

            var id = await _hospitalRepository.GetItem(x => x.MA_CSKCB == hospitalId || x.Id == hospitalId);

            if (id != null) return id.Id;

            return hospitalId;
        }
        private async Task<ResponseModel<bool>> CheckEmailExist(string name)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            var isExist = await _userRepository.AnyAsync(x => x.Email.ToLower().Equals(name.Trim().ToLower()) && !x.IsDeleted);
            if (isExist)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_EXIST_EMAIL;
                res.Data = false;
                return res;
            }
            return res;
        }
        private ResponseModel<bool> ValidateModifyItem(ModifyUserModel request, string currentRole, bool isCheckPassRequired = true)
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
                User item;
                #region Validtion

                item = await _userRepository.GetById(id);
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
                var currentId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
                if (string.IsNullOrWhiteSpace(currentId))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    res.Data = false;
                    return res;
                }

                var user = await _userRepository.GetItem(x => x.Id.Equals(currentId) && x.IsActivated && !x.IsDeleted && !x.IsFirstTimeLogin);
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

        public async Task<ResponseModel<User>> GetActiveUserAndHospital(string idOrEmail, string maCSKCB, string role)
        {
            //Lấy thông tin user trong Database kèm bệnh viện trực thuộc
            idOrEmail = idOrEmail?.Trim()?.ToLower();
            maCSKCB = maCSKCB?.Trim().ToLower();
            role = role?.Trim().ToLower();
            var res = new ResponseModel<User>(HttpStatusCode.OK, "", null);
            if (string.IsNullOrEmpty(idOrEmail) || string.IsNullOrEmpty(maCSKCB) || string.IsNullOrEmpty(role))
            {
                //Uti.HandleCronJobLog(_logger, $"API.GetActiveUserAndHospital B1 {idOrEmail} {maCSKCB} {role}", nameof(UserManagement.IUserManagementService));
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;

                return res;
            }

            var usr = await _userRepository.GetItem(x => (x.Id == idOrEmail || x.Email.ToLower() == idOrEmail) && x.IsActivated == true && x.IsDeleted != true, q => q.Include(u => u.Hospital));
            //Uti.HandleCronJobLog(_logger, $"API.GetActiveUserAndHospital B2 {idOrEmail} {maCSKCB} {role} {usr.Email} {usr.Hospital}", nameof(UserManagement.IUserManagementService));

            //Kiểm tra nếu user hoặc bệnh viện bị khóa thì trả về lỗi Forbidden 
            if (usr == null || usr.Hospital == null || (usr.Hospital.MA_CSKCB == maCSKCB && (usr.Hospital.IsActivated != true || usr.Hospital.IsDeleted == true)))
            {
               // Uti.HandleCronJobLog(_logger, $"API.GetActiveUserAndHospital B3 {idOrEmail} {maCSKCB} {role}", nameof(UserManagement.IUserManagementService));
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            res.Data = usr;
            //Nếu maCSKCB trong token cùng giống viện trực thuộc thì trả về ok
            var hospital = usr.Hospital;
            //Uti.HandleCronJobLog(_logger, $"API.GetActiveUserAndHospital B2 {idOrEmail} {maCSKCB} {role} {usr.Email} {hospital.MA_CSKCB}", nameof(UserManagement.IUserManagementService));

            if (hospital.MA_CSKCB == maCSKCB) return res;

            //Trường hợp maCSKCB khác nhau:
            if (role != RoleConstant.MASTER && hospital.MA_CSKCB != "00")
            {
                //Uti.HandleCronJobLog(_logger, $"API.GetActiveUserAndHospital B4 {idOrEmail} {maCSKCB} {role}", nameof(UserManagement.IUserManagementService));
                if ((usr.Hospital.MA_CSKCB.Length < 5 && !maCSKCB.StartsWith(usr.Hospital.MA_CSKCB)) || (usr.Hospital.MA_CSKCB.Length >= 5 && !maCSKCB.Equals(usr.Hospital.MA_CSKCB)))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
            }

            //Kiểm tra lại thông tin bệnh viện trong Database
            hospital = await _hospitalRepository.GetItem(h => h.MA_CSKCB == maCSKCB && h.IsActivated == true && h.IsDeleted != true);
            if (hospital == null)
            {
                //Uti.HandleCronJobLog(_logger, $"API.GetActiveUserAndHospital B5 {idOrEmail} {maCSKCB} {role}", nameof(UserManagement.IUserManagementService));

                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                return res;
            }

            res.Data.Hospital = hospital;
            return res;
        }
    }
}
