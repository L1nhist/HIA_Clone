using HIA.Core.Models.Base;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.User;
using HIA.Data.Auths;

namespace HIA.Services.UserManagement;

public interface IUserManagementService
{
    Task<ResponseModel<PaginationResponseModel<UserInfoModel>>> GetItems(PaginationRequestModel request);
    Task<ResponseModel<ModifyUserModel>> GetItemById(string id);
    Task<ResponseModel<bool>> CreateNewItem(NewUserModel request, string newUserUrl);
    Task<ResponseModel<bool>> UpdateItem(ModifyUserModel request);
    Task<ResponseModel<bool>> DeleteItem(string id);
    Task<ResponseModel<bool>> ResetPassword(string request, User model);
    Task<ResponseModel<bool>> ChangePassword(string currentPass, string newPassword);
    Task<ResponseModel<User>> GetActiveUserAndHospital(string? email, string? maCSKCB, string? role);
}
