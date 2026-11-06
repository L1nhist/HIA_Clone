using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.DatabaseModel.Entites;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.HDSDManagement
{
    public interface IHDSDDichVuService
    {
        Task<ResponseModel<PaginationResponseModel<HDSDDichVuInfoModel>>> GetItems(PaginationRequestModel request);
        Task<ResponseModel<ModifyHDSDDichVuModel>> GetItemById(string id);
        Task<ResponseModel<bool>> CreateNewItem(ModifyHDSDDichVuModel request);
        Task<ResponseModel<bool>> UpdateItem(ModifyHDSDDichVuModel request);
        Task<ResponseModel<bool>> ActivateItem(int? id, bool? hieuLuc);
    }
}
