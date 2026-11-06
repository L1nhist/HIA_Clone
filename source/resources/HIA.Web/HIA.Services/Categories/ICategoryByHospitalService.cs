using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.Categories
{
    public interface ICategoryByHospitalService<TEntity, TDisplayModel, TModifyModel> 
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

        //Linhist added 22/02/25
        // handle files changes
        Task<ResponseModel<bool>> ReplaceCategoryFile(IFormFile request, List<string> keysString);
        Task<ResponseModel<byte[]>> DownloadExcelFile(string maCSKCB, string search);
        Task<ResponseModel<byte[]>> DownloadExcelFile(string filterDownloadString, TEntity emptyModel);

        //Linhist added 09/01/24
        Task<bool> AllowImport();
        Task<ResponseModel<bool>> ImportFromAPI();
    }
}
