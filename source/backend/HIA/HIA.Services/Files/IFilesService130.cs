
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.FileUploads;
using HIA.Core.Models.Files;
using HIA.Core.Models.Pagination;
using HIA.Data.Records;
namespace HIA.Services.Files;

public interface IFilesService130
{
    Task<ResponseModel<dynamic>?> FileUploadAsync(FileUpload fileUpload);

    Task<ResponseModel<dynamic>?> FileUploadAPI(FileUpload fileUpload, string hospitalID, string hospitalName, string userID, string maCSKCB, string? email, bool isUpload = false, bool isFromHIS = false);

    Task<ResponseModel<PaginationResponseModel<XML01Info>>> GetXml01Items(FilesPaginationRequest request);
    Task<ResponseModel<PaginationResponseModel<XML02Info>>> GetXml02Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML03Info>>> GetXml03Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML04Info>>> GetXml04Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML05Info>>> GetXml05Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML06Info>>> GetXml06Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML07Info>>> GetXml07Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML08Info>>> GetXml08Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML09Info>>> GetXml09Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML10Info>>> GetXml10Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML11Info>>> GetXml11Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML13Info>>> GetXml13Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML14Info>>> GetXml14Items(PaginationRequestModel request, int? xml1ID);
    Task<ResponseModel<PaginationResponseModel<XML15Info>>> GetXml15Items(PaginationRequestModel request, int? xml1ID);

    Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorItemsByFileName(List<string> fileNames, string maCSKCB);

    Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorItemByFileName(List<string> fileNames, string maCSKCB);

    Task<ResponseModel<IEnumerable<XMLErrorFullInfo>>> GetXmlErrorItems(int? xml1ID);

    Task<ResponseModel<bool>> RecheckHoso(params long[] xmlIds);

    Task<ResponseModel<byte[]>?> DownloadBaoCao(string filterDownloadString, int? fileType);

    Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorModels(XML01? xml01);
}
