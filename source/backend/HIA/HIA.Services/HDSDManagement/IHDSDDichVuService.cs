using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;

namespace HIA.Services.HDSDManagement;

public interface IHDSDDichVuService
{
    Task<ResponseModel<PaginationResponseModel<HDSDDichVuInfoModel>>> GetItems(PaginationRequestModel request);
    Task<ResponseModel<ModifyHDSDDichVuModel>> GetItemById(string id);
    Task<ResponseModel<bool>> CreateNewItem(ModifyHDSDDichVuModel request);
    Task<ResponseModel<bool>> UpdateItem(ModifyHDSDDichVuModel request);
    Task<ResponseModel<bool>> ActivateItem(int? id, bool? hieuLuc);
}
