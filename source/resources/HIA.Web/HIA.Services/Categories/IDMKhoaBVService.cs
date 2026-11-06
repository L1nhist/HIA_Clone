using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.Categories
{
    public interface IDMKhoaBVService
    {
        #region DMKhoa
        Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionList();
        Task<ResponseModel<List<DMKhoaBVInfoModel>>> GetItems();
        Task<ResponseModel<bool>> Save(DMKhoaBVModifyModel request);
        Task<ResponseModel<bool>> DeleteItem(int id);
        #endregion

        #region DMKhoa New
        Task<ResponseModel<PaginationResponseModel<DMKhoaBVInfoBasicModel>>> GetDMKhoaBVItems(PaginationRequestModel request, List<string> ignoreKeysForSearch);
        Task<ResponseModel<bool>> CreateDMKhoaBVNewItem(DMKhoaBVModifyBasicModel request);
        Task<ResponseModel<bool>> UpdateDMKhoaBVItem(DMKhoaBVModifyBasicModel request);
        Task<ResponseModel<bool>> DeleteDMKhoaBVItem(int id);

        Task<ResponseModel<bool>> UploadCategoryFile(IFormFile request);
        #endregion
    }
}
