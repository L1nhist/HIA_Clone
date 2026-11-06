using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Http;

namespace HIA.Services.HDSDManagement;

public interface IDichVuMaMayService
{
    Task<ResponseModel<DichVuMaMayModifyModel>> GetItemById(int id);
    Task<ResponseModel<PaginationResponseModel<DichVuMaMayInfoModel>>> GetItems(PaginationRequestModel request, string maCSKCB);
    Task<ResponseModel<bool>> Create(DichVuMaMayModifyModel request);
    Task<ResponseModel<bool>> Update(DichVuMaMayModifyModel request);
    Task<ResponseModel<bool>> Delete(int id, string maCSKCB);

    Task<ResponseModel<bool>> UploadFile(IFormFile request);
}
