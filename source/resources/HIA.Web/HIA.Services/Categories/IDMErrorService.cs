//using HIA.Core.Models.Base;
//using HIA.Core.Models.Categories;
//using HIA.Core.Models.Files;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace HIA.Services.Categories
//{
//    public interface IDMErrorService
//    {
//        Task<ResponseModel<IEnumerable<SelectErrorsItems>>> GetSelectionErrorListByHospital();

//        Task<ResponseModel<List<DMErrorInfoModel>>> GetItems();

//        Task<ResponseModel<bool>> Save(DMErrorModifyModel request);

//        //Linhist added 01/04/23
//        Task<ResponseModel<IEnumerable<DMErrorInfoModel>>> GetAlertErrorListByHospital();

//        //Linhist added 07/03/24
//        Task<ResponseModel<bool>> SaveDefaultErrors(string maCSKCB);
//    }
//}
