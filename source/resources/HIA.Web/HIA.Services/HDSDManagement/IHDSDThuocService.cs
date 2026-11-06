using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.DatabaseModel.Entites;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.HDSDManagement
{
    public interface IHDSDThuocService
    {
        Task<ResponseModel<PaginationResponseModel<HDSDThuocInfoModel>>> GetItems(PaginationRequestModel request);
        Task<ResponseModel<ModifyHDSDThuocModel>> GetItemById(string id);
        Task<ResponseModel<bool>> CreateNewItem(ModifyHDSDThuocModel request);
        Task<ResponseModel<bool>> UpdateItem(ModifyHDSDThuocModel request);
        Task<ResponseModel<bool>> ActivateItem(int? id, bool? hieuLuc);
    }
}
