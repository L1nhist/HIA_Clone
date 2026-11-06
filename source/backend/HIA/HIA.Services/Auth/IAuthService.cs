using HIA.Core.Models.Base;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.Data.Auths;

namespace HIA.Services.Auth;

public interface IAuthService
{
    Task<ResponseModel<string>> SignIn(SignInModel request);
    Task<ResponseModel<TokenRequestModel>> SignInWinForm(SignInModel request);
    Task<ResponseModel<string>> SignInGetToken(SignInModel request);
    Task<ResponseModel<TokenResponseModel>> SignInGetTokenResp(SignInModel request);
    Task<ResponseModel<bool>> SignOut();
    Task<ResponseModel<User>> CheckToken(string token);
    Task<ResponseModel<User>> CheckToken2(string token);
    Task<ResponseModel<bool>> ForgotPass(string email, string newUserUrl);
}
