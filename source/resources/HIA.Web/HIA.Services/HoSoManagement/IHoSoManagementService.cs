//using HIA.Core.Models.Base;
//using HIA.Core.Models.Files;
//using HIA.DatabaseModel.Entites;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace HIA.Services.HosoManagement
//{
//    public interface IHoSoManagementService
//    {

//        Task<ResponseModel<bool>> CreateNewHoSoAPI(XML1_API request, string hospitalIdAPI = "", string hospitalNameAPI = "", string userIdAPI = "", string MA_CSKCB_API = "", bool isFromHIS = false);

//        Task<ResponseModel<bool>> CreateNewHoSo(List<XML1> request, string hospitalIdAPI = "", string hospitalNameAPI = "", string userIdAPI = "", string MA_CSKCB_API = "", bool isFromHIS = false);
//        Task ValidateFiles(string currentHospitalId = "", string hospitalNameAPI = "", string currentUserId = "", string currentMaCSKCB = "", string xml1Ids = "", string fileNames = "",bool isAPICheck = false, bool isFromHis = false);
//        Task VerifyXML1();
//        Task ValidateNewXML();

//        //Linhist added 12/02/23
//        Task Verify_TheBH();

//        //Linhist added 11/05/23
//        Task<List<string>> ListFilesByCreateDate(DateTime fromDate, string maCSKCB);
//    }
//}
