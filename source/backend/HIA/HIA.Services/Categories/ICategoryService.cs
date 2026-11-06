using HIA.Core.Models.Base;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Http;

namespace HIA.Services.Categories;

public interface ICategoryService<TEntity, TDisplayModel, TModifyModel>
    where TEntity : class
    where TDisplayModel : class
    where TModifyModel : class
{
    Task<ResponseModel<PaginationResponseModel<TDisplayModel>>> GetItems(PaginationRequestModel request, List<string> ignoreKeysForSearch, TEntity emptyModel);
    Task<ResponseModel<TModifyModel>> GetItemById(string id, string keyString);
    Task<ResponseModel<bool>> CreateNewItem(TModifyModel request, List<string> uniqueFieldNames);
    Task<ResponseModel<bool>> UpdateItem(TModifyModel request, string keyString, List<string> uniqueFieldNames);
    Task<ResponseModel<bool>> DeleteItem(int id, string keyString);

    // handle files
    Task<ResponseModel<bool>> UploadCategoryFile(IFormFile request, List<string> keysString);
    Task<ResponseModel<bool>> UploadCategoryFileWithHospital(IFormFile request, List<string> keysString, string maCSKCBKey, string maCSKCBValue);
    Task<ResponseModel<byte[]>?> DownloadExcelFile(string filterDownloadString, TEntity emptyModel);
}
