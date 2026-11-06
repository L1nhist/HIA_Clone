//using HIA.Core.Models.Base;
//using HIA.Core.Models.Categories;
//using HIA.Core.Models.Files;
//using HIA.Core.Models.FileUploads;
//using HIA.Core.Models.Pagination;
//using HIA.DatabaseModel.Entites;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace HIA.Services.Files
//{
//    public interface IFilesService
//    {
//        Task<ResponseModel<IEnumerable<SelectListItem>>> GetSelectionCreatedDateList();
//        Task<ResponseModel<PaginationResponseModel<XML1InfoModel>>> GetXml1Items(FilesPaginationRequest request);
//        Task<ResponseModel<List<XML1ErrorQuantityModel>>> GetXml1ErrorItems();

//        ResponseModel<XML1InfoModel> TransformXml1ToModal(XML1InfoModalObject request);
//        Task<ResponseModel<PaginationResponseModel<XML2InfoModel>>> GetXml2Items(PaginationRequestModel request, int xml1Id);
//        Task<ResponseModel<PaginationResponseModel<XML3InfoModel>>> GetXml3Items(PaginationRequestModel request, int xml1Id);
//        Task<ResponseModel<PaginationResponseModel<XML4InfoModel>>> GetXml4Items(PaginationRequestModel request, int xml1Id);
//        Task<ResponseModel<PaginationResponseModel<XML5InfoModel>>> GetXml5Items(PaginationRequestModel request, int xml1Id);

//        //Linhist added 09/06/22
//        Task<int> SetXml1Recheck(FilesPaginationRequest request);

//        Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorItems(int xml1Id);
//        Task RecheckXML1(int xml1Id);
        
//        Task<ResponseModel<IEnumerable<XML1ErrorInfoModelAPI>>> GetXml1ErrorItemsByFileName(List<string> fileNames,string maCSKCB);
//        Task<ResponseModel<byte[]>> DownloadExcelFile(string filterDownloadString, int? fileType);

//        //Linhist removed 19/10/23
//        //Task<ResponseModel<byte[]>> Download_ExcelFile_New(string filterDownloadString, int? fileType);

//        Task FileUploadAsycn(FileUpload fileUpload, string hospitalIdAPI = "", string hospitalNameAPI = "", string userIdAPI = "", string MA_CSKCB_API = "");
//        Task<ResponseModel<dynamic>> FileUploadAPI(FileUpload fileUpload, string hospitalIdAPI = "", string hospitalNameAPI = "", string userIdAPI = "", string MA_CSKCB_API = "",bool isFromHIS = false);
//        Task<ResponseModel<dynamic>> FileCheckAPI(XML1_API ListCheckAPI);
//        Task<ResponseModel<IEnumerable<XML1ErrorInfoModelAPI>>> GetXml1ErrorItemsByFileNameAPI(string fileNames);

//        Task<List<ErrorCounterModel>> GetXml1ErrorsCounter(FilesPaginationRequest request);
//    }
//}
