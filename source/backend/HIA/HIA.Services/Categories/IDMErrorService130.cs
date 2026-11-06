using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files;
using HIA.Data.Rules;

namespace HIA.Services.Categories;

public interface IDMErrorService130
{
    Task<ResponseModel<IEnumerable<SelectErrorsItems>>> GetSelectionErrorListByHospital();

    Task<IEnumerable<DMError130>?> GetAll(bool isDelete = false);
    Task<ResponseModel<List<DMErrorInfoModel>>> GetItems();
    Task<ResponseModel<bool>> Save(DMErrorModifyModel request);

    //Linhist added 07/03/24
    Task<ResponseModel<bool>> SaveDefaultErrors(string maCSKCB);

    Task<ResponseModel<IEnumerable<DMErrorInfoModel>>> GetAlertErrorListByHospital();
    Task<ResponseModel<List<DMError130>>> GetAllListing();
    Task<List<DMErrorInfoModel>?> GetAllByHospital(string maCSKCB);
    Task<IEnumerable<DMError130>> GetAllByType(int type);
    Task<int> GetLastErrorCode();
    Task<bool> InsertAndActived(DMError130 error, string maCSKCB);

    Task<DMError130?> UpdateError(int? errorCode, string message);
    Task<ResponseModel<IEnumerable<DMError130>>> UpdateErrorDescs(int[] errorCodes, string[] errorDescs);
}
