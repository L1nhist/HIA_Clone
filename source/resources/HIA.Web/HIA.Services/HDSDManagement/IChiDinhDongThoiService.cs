using HIA.Core.Models.Base;
using HIA.Core.Models.HDSD;
using HIA.Core.Models.Pagination;
using System.Threading.Tasks;

namespace HIA.Services.HDSDManagement
{
    public interface IChiDinhDongThoiService
    {
        Task<ResponseModel<PaginationResponseModel<ChiDinhDongThoiInfoModel>>> GetItems(PaginationRequestModel request);
        Task<ResponseModel<ChiDinhDongThoiModifyModel>> GetItemById(string id);
        Task<ResponseModel<bool>> CreateNewItem(ChiDinhDongThoiModifyModel request);
        Task<ResponseModel<bool>> UpdateItem(ChiDinhDongThoiModifyModel request);
        Task<ResponseModel<bool>> ActivateItem(int? id, bool? hieuLuc);
    }
}
