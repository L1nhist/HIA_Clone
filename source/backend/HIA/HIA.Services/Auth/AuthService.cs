using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.Data.Auths;
using HIA.Repositories.Base;
using HIA.Repositories.User;
using HIA.Services.Email;
using HIA.Services.SysHistoryLogManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace HIA.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly TokenHandler _tokenHandle;
    private readonly IEmailService _emailService;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly ExcludedEmailValidator _excludedEmails;
    public AuthService(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ISysHistoryLogService sysHistoryLogService,
        TokenHandler tokenHandle,
        IEmailService emailService,
        IOptions<AppSettings> appSettings,
        ILogger<AuthService> logger,
        ExcludedEmailValidator excludedEmails
        )
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _sysHistoryLogService = sysHistoryLogService;
        _tokenHandle = tokenHandle;
        _emailService = emailService;
        _appSettings = appSettings;
        _logger = logger;
        _excludedEmails = excludedEmails;
    }

    public async Task<ResponseModel<bool>> SignOut()
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);

        try
        {
            await _tokenHandle.Signout();

            // write log to db
            await _sysHistoryLogService.AddSysLog(Constant.AUTH_SIGN_OUT_FUNC);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, false, _logger, $"{nameof(AuthService)} - {nameof(AuthService.SignOut)}");
        }
        return res;
    }

    private async Task<ResponseModel<TokenRequestModel>> GetTokenRequestSignIn(SignInModel request)
    {
        var res = new ResponseModel<TokenRequestModel>(HttpStatusCode.OK, "", null);
        try
        {
            #region Validation
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_REQUIRED_EMAIL_PASSWORD;
                return res;
            }
            #endregion

            var hashPass = Uti.PasswordEncrypt(request.Password.Trim());
            var user = await _userRepository.GetItem(x =>
                (x.Email ?? "").ToLower().Equals(request.Email.Trim().ToLower()) && x.Password.Equals(hashPass)
                    && x.IsActivated && !x.IsFirstTimeLogin && !x.IsDeleted && x.Hospital != null && x.Hospital.IsActivated && !x.Hospital.IsDeleted,
                y => y.Include(t => t.UserRoles).ThenInclude(t1 => t1.Role).Include(t2 => t2.Hospital));

            //Linhist changed 13/01/26
            if (user == null || _excludedEmails.IsExcluded(user.Email))
            {
                res.Code = HttpStatusCode.Unauthorized;
                res.Message = MessageResource.ERR_401_UNAUTHORIZED;
                return res;
            }

            var tokenRequest = new TokenRequestModel
            {
                Id = user.Id,
                Email = user.Email,
                RoleId = string.Join(",", user.UserRoles.Select(x => x.RoleId).ToArray()),
                RoleName = string.Join(",", user.UserRoles.Select(x => x.Role.Name).ToArray()),
                HospitalId = user.HospitalId,
                HospitalName = user.Hospital?.Name,
                FullName = user.FullName,
                MA_CSKCB = user.Hospital?.MA_CSKCB,
            };

            res.Data = tokenRequest;
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<TokenRequestModel>(
                ex, default, _logger, $"{nameof(AuthService)} - {nameof(AuthService.GetTokenRequestSignIn)}");
        }
        return res;
    }
    public async Task<ResponseModel<string>> SignIn(SignInModel request)
    {
        var res = new ResponseModel<string>(HttpStatusCode.OK, "", "");
        var tokenRequest = await GetTokenRequestSignIn(request);
        if (tokenRequest.Code != HttpStatusCode.OK)
        {
            res.Code = tokenRequest.Code;
            res.Message = tokenRequest.Message;
            return res;
        }

        try
        {
            var tokenRes = await _tokenHandle.CreateAndSignin(tokenRequest.Data ?? throw new InvalidOperationException("Missing token request data."));
            res.Data = tokenRes.Token;

            // write log to db
            await _sysHistoryLogService.AddSysLog(Constant.AUTH_SIGN_IN_FUNC, request.Email);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, "", _logger, $"{nameof(AuthService)} - {nameof(AuthService.SignIn)}");
        }
        return res;
    }

    public async Task<ResponseModel<TokenRequestModel>> SignInWinForm(SignInModel request)
    {
        var res = new ResponseModel<TokenRequestModel>(HttpStatusCode.OK, "", null);
        try
        {
            res = await GetTokenRequestSignIn(request);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<TokenRequestModel>(
                ex, default, _logger, $"{nameof(AuthService)} - {nameof(AuthService.SignIn)}");
        }
        return res;
    }

    public async Task<ResponseModel<string>> SignInGetToken(SignInModel request)
    {
        var res = new ResponseModel<string>(HttpStatusCode.OK, "", "");
        var tokenRequest = await GetTokenRequestSignIn(request);
        if (tokenRequest.Code != HttpStatusCode.OK)
        {
            res.Code = tokenRequest.Code;
            res.Message = tokenRequest.Message;
            return res;
        }

        try
        {
            var tokenRes = _tokenHandle.BuildToken(tokenRequest.Data ?? throw new InvalidOperationException("Missing token request data."));
            res.Data = tokenRes.Token;
            //Linhist added 30/03/26 - lấy expire date trả về
            res.Message = tokenRes.ExpireAt.ToString();

            // write log to db
            await _sysHistoryLogService.AddSysLog(Constant.AUTH_SIGN_IN_API_FUNC, request.Email);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError(
                ex, "", _logger, $"{nameof(AuthService)} - {nameof(AuthService.SignInGetToken)}");
        }
        return res;
    }

    #region Linhist added 31/03/26

    public async Task<ResponseModel<TokenResponseModel>> SignInGetTokenResp(SignInModel request)
    {
        var res = new ResponseModel<TokenResponseModel>(HttpStatusCode.OK, "", null);
        var tokenRequest = await GetTokenRequestSignIn(request);
        if (tokenRequest.Code != HttpStatusCode.OK)
        {
            res.Code = tokenRequest.Code;
            res.Message = tokenRequest.Message;
            return res;
        }

        try
        {
            var tokenRes = _tokenHandle.BuildToken(tokenRequest.Data ?? throw new InvalidOperationException("Missing token request data."));
            res.Data = tokenRes;

            // write log to db
            await _sysHistoryLogService.AddSysLog(Constant.AUTH_SIGN_IN_API_FUNC, request.Email);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<TokenResponseModel>(
                ex, default, _logger, $"{nameof(AuthService)} - {nameof(AuthService.SignInGetTokenResp)}");
        }
        return res;
    }
    #endregion

    public async Task<ResponseModel<User>> CheckToken(string token)
    {
        var res = new ResponseModel<User>(HttpStatusCode.OK, "", null);
        try
        {
            var tokenData = _tokenHandle.IsTokenValid(token);
            if (tokenData.Code != HttpStatusCode.OK || tokenData.Data == null) return res;

            var isFirstTimeLogin = true; // TypeUpdatePass.NEW_USER
            if (tokenData.Data.Type == TypeUpdatePass.RESET.ToString()) isFirstTimeLogin = false;
            var user = await _userRepository.GetItem(x => x.Id.Equals(tokenData.Data.Id) && x.IsActivated && !x.IsDeleted && x.IsFirstTimeLogin == isFirstTimeLogin);

            //Linhist added 13/01/26
            if (user == null || _excludedEmails.IsExcluded(user.Email)) return res;
            res.Data = user;
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<User>(
                ex, default, _logger, $"{nameof(AuthService)} - {nameof(AuthService.CheckToken)}");
        }
        return res;
    }


    public async Task<ResponseModel<User>> CheckToken2(string token)
    {
        var res = new ResponseModel<User>(HttpStatusCode.OK, "", null);
        try
        {
            var tokenData = _tokenHandle.IsTokenValid(token);
            if (tokenData.Code != HttpStatusCode.OK || tokenData.Data == null) return res;

            var user = await _userRepository.GetItem(x => x.Id.Equals(tokenData.Data.Id) && x.IsActivated && !x.IsDeleted);
            res.Data = user;
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<User>(
                ex, default, _logger, $"{nameof(AuthService)} - {nameof(AuthService.CheckToken)}");
        }
        return res;
    }

    public async Task<ResponseModel<bool>> ForgotPass(string email, string newUserUrl)
    {
        var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);

        try
        {
            var user = await _userRepository.GetItem(x => (x.Email ?? "").ToLower().Equals(email.Trim().ToLower()) && x.IsActivated && !x.IsDeleted && !x.IsFirstTimeLogin);
            if (user == null)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_EXIST_EMAIL;
                res.Data = false;
                return res;
            }

            #region sent email confirm
            var token = _tokenHandle.BuildToken(new TokenRequestModel
            {
                Id = user.Id,
                Type = TypeUpdatePass.RESET.ToString()
            })?.Token;
            if (!Uti.TryGetPublicBaseUrl(_appSettings.Value.PublicBaseUrl, out var publicBaseUrl))
            {
                _logger.LogError("AppSettings:PublicBaseUrl chua cau hinh hoac khong hop le");
                res.Code = HttpStatusCode.InternalServerError;
                res.Message = Uti.PublicBaseUrlErrorMessage;
                res.Data = false;
                return res;
            }
            var url = $"{publicBaseUrl}/{newUserUrl}?token={token}";
            _emailService.SendEmailNewUser(url, user.FullName ?? string.Empty, user.Email ?? string.Empty, false);
            #endregion
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<bool>(
                ex, false, _logger, $"{nameof(AuthService)} - {nameof(AuthService.CheckToken)}");
        }
        return res;
    }
}
