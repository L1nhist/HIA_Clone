using HIA.Core.Models.Base;
using HIA.Core.Models.Hospital;
using HIA.Core.Models.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HIA.Services.HospitalManagement;

public interface IHospitalService
{
    Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveHospitals();
    Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveHospitalsByID();
    Task<ResponseModel<IEnumerable<HospitalInfoModel>>> GetActiveByOwner();
    Task<ResponseModel<PaginationResponseModel<HospitalInfoModel>>> GetItems(PaginationRequestModel request);
    Task<ResponseModel<ModifyHospitalModel>> GetCurrent();
    Task<ResponseModel<ModifyHospitalModel>> GetItemById(string id);
    Task<ResponseModel<bool>> CreateNewItem(ModifyHospitalModel request);
    Task<ResponseModel<bool>> UpdateItem(ModifyHospitalModel request);
    Task<ResponseModel<bool>> DeleteItem(string id);

    Task<bool> HasIncludedChild(string id, string maCSKCB);
    Task<Dictionary<string, int>?> CountFilesToday();

    Task<int> RemoveDublicated(string maCSKCB, DateTime tuNgay, DateTime denNgay);

    Task<int> RemoveDublicated130(string maCSKCB);

    //Linhist added 190826
    Task<int> RemoveDubSingle(string maCSKCB, string maLK);

    Task<int> UpdateCategoryPrice(string maCSKCB, int hangBV);

    //Linhist added 17/07/26
    Task<ResponseModel<IEnumerable<HospitalAIInfo>>> GetAll();
}
