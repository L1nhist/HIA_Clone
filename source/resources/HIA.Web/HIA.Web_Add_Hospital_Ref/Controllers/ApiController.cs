using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.FileUploads;
using HIA.Core.Models.User;
using HIA.Services.Auth;
using HIA.Services.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HIA.DatabaseModel.Entites;
using HIA.Services.HosoManagement;
using System.Globalization;

namespace HIA.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet]
        [Route("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok("test");
        }
        private readonly IAuthService _authService;
        private readonly TokenHandler _tokenHandle;
        private readonly IFilesService _FilesService;
        private readonly IHoSoManagementService _HoSoManagementService;
        private readonly ILogger<ApiController> _logger;

        public ApiController(IHoSoManagementService HoSoManagementService, IAuthService authService, TokenHandler tokenHandle, IFilesService FilesService, ILogger<ApiController> logger)
        {
            _authService = authService;
            _tokenHandle = tokenHandle;
            _FilesService = FilesService;
            _logger = logger;
            _HoSoManagementService = HoSoManagementService;
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel request)
        {
            var res = new ResponseModel<string>();
            if (!ModelState.IsValid)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = string.Join(" | ", ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage));
                return NotFound(res);
            }

            res = await _authService.SignInGetToken(request);
            string ExpiredAt = System.DateTime.Now.AddDays(30).ToShortDateString();
            return Ok(new { Code = res.Code, Message = res.Message, Token = res.Data.ToString(), ExpiredAt = ExpiredAt });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SignOut")]
        public async Task<IActionResult> SignOutAction()
        {
            await _authService.SignOut();
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("UploadFile")]
        public async Task<IActionResult> UploadFile([FromBody] FileUploading request)
        {
            var res = _tokenHandle.IsTokenValid(request.Token);
            if (res.Code != HttpStatusCode.OK)
            {
                return NotFound(res);
            }

            List<IFormFile> FormFiles = new List<IFormFile>();
            foreach (var file in request.ArrayFiles)
            {
                byte[] byteArray = System.Convert.FromBase64String(file.FileBytes);

                var ms = new MemoryStream(byteArray);

                IFormFile fromFile = new FormFile(ms, 0, ms.Length,
                    Path.GetFileNameWithoutExtension(file.FileName),
                    file.FileName
                );
                FormFiles.Add(fromFile);


            }
            FileUpload fileUp = new FileUpload() { FormFiles = FormFiles };
            bool IsFromHis = res.Data.Email == "APIVIP" || res.Data.Email == "DKTWTN";

            string filenames = "";
            request.ArrayFiles.Select(f => filenames += f.FileName + ";").ToArray();

            //haimaster 25.7.2022
            Uti.HandleCronJobLog(_logger, $"S1. Haimaster Check API {res.Data.Email} ({res.Data.MA_CSKCB}). FileName: {filenames}", nameof(ApiController.UploadFile));
            try
            {
                var uploadEdfile = await _FilesService.FileUploadAPI(fileUp, res.Data.HospitalId, res.Data.HospitalName, res.Data.Id, res.Data.MA_CSKCB, IsFromHis);
                return Ok(uploadEdfile);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"S1. Haimaster Check API {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
            }


            return Ok(null);


            //_FilesService.FileUploadAPI(fileUp, res.Data.HospitalId, res.Data.HospitalName, res.Data.Id, res.Data.MA_CSKCB);
            //List<string> failedFiles = new List<string>();
            //string successMessage = "";
            //string failedMessage = "";
            //string uploadStatus = "";
            //string uploadTime = "";

            //var uploadEdfile = new ResponseModel<dynamic> { Code = System.Net.HttpStatusCode.OK, Data = new { successMessage, failedMessage, uploadStatus, uploadTime, failedFiles }, Message = "" };

            //System.Threading.Thread.Sleep(9000);
            //return Ok(uploadEdfile);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CheckError")]
        public async Task<IActionResult> CheckError(FileCheckError request)
        {
            var res = _tokenHandle.IsTokenValid(request.Token);
            if (res.Code != HttpStatusCode.OK)
            {
                return NotFound(res);
            }
            var maCSKCB = res.Data.MA_CSKCB;
            var alo = await _FilesService.GetXml1ErrorItemsByFileName(request.FileName, maCSKCB);



            return Ok(alo);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("CheckFileAPI")]
        public async Task<IActionResult> CheckFileAPI([FromBody] Root request)
        {
            var res = _tokenHandle.IsTokenValid(request.token);
            if (res.Code != HttpStatusCode.OK)
            {
                return NotFound(res);
            }



            XML1_API xml1 = new XML1_API();



            if (request.XML1API?.TONG_HOP != null)
            {

                xml1.MA_CSKCB = request.XML1API.TONG_HOP.MA_CSKCB;
                xml1.MA_LK = request.XML1API.TONG_HOP.MA_LK;
                xml1.STT = request.XML1API.TONG_HOP.STT;
                xml1.MA_BN = request.XML1API.TONG_HOP.MA_BN;
                xml1.HO_TEN = request.XML1API.TONG_HOP.HO_TEN;
                DateTime ngaysinh = new DateTime(1900, 1, 1);
                DateTime.TryParse(request.XML1API.TONG_HOP.NGAY_SINH, out ngaysinh);
                if (ngaysinh.Year != 1)
                {
                    xml1.NGAY_SINH = ngaysinh;
                }
                else
                {
                    xml1.NGAY_SINH = null;
                }
                xml1.GIOI_TINH = request.XML1API.TONG_HOP.GIOI_TINH;
                xml1.DIA_CHI = request.XML1API.TONG_HOP.DIA_CHI;
                xml1.MA_THE = request.XML1API.TONG_HOP.MA_THE;
                xml1.MA_DKBD = request.XML1API.TONG_HOP.MA_DKBD;


            }

            List<XML2_API> listXML2 = new List<XML2_API>();

            if (request.XML2API?.DSACH_CHI_TIET_THUOC?.CHI_TIET_THUOC?.Count > 0)
            {
                foreach (var xml2 in request.XML2API?.DSACH_CHI_TIET_THUOC?.CHI_TIET_THUOC)
                {
                    XML2_API xML2_API = new XML2_API();
                    xML2_API.MA_LK = xml2.MA_LK;
                    xML2_API.STT = xml2.STT;
                    xML2_API.MA_THUOC = xml2.MA_THUOC;
                    xML2_API.MA_NHOM = xml2.MA_NHOM;
                    xML2_API.TEN_THUOC = xml2.TEN_THUOC;
                    xML2_API.DON_VI_TINH = xml2.DON_VI_TINH;
                    xML2_API.HAM_LUONG = xml2.HAM_LUONG;
                    xML2_API.DUONG_DUNG = xml2.DUONG_DUNG;
                    xML2_API.LIEU_DUNG = xml2.LIEU_DUNG;
                    xML2_API.SO_DANG_KY = xml2.SO_DANG_KY;
                    xML2_API.TT_THAU = xml2.TT_THAU;
                    xML2_API.PHAM_VI = xml2.PHAM_VI;
                    xML2_API.SO_LUONG = xml2.SO_LUONG;
                    xML2_API.DON_GIA = xml2.DON_GIA;
                    xML2_API.TYLE_TT = xml2.TYLE_TT;
                    xML2_API.THANH_TIEN = xml2.THANH_TIEN;
                    xML2_API.MUC_HUONG = xml2.MUC_HUONG;
                    xML2_API.T_NGUONKHAC = xml2.T_NGUONKHAC;
                    xML2_API.T_BNTT = xml2.T_BNTT;
                    xML2_API.T_BHTT = xml2.T_BHTT;
                    xML2_API.T_BNCCT = xml2.T_BNCCT;
                    xML2_API.T_NGOAIDS = xml2.T_NGOAIDS;
                    xML2_API.MA_KHOA = xml2.MA_KHOA;
                    xML2_API.MA_BAC_SI = xml2.MA_BAC_SI;
                    xML2_API.MA_BENH = xml2.MA_BENH;
                    DateTime ngayYl = new DateTime();
                    DateTime.TryParse(xml2.NGAY_YL, out ngayYl);
                    if (ngayYl.Year != 1)
                    {
                        xML2_API.NGAY_YL = ngayYl;
                    }
                    else
                    {
                        xML2_API.NGAY_YL = null;
                    }
                   
                    xML2_API.MA_PTTT = xml2.MA_PTTT;
                    xML2_API.MA_CSKCB = request.XML1API.TONG_HOP.MA_CSKCB;
                    listXML2.Add(xML2_API);
                }

            }

            List<XML3_API> listXML3 = new List<XML3_API>();

            if (request.XML3API?.DSACH_CHI_TIET_DVKT?.CHI_TIET_DVKT?.Count > 0)
            {
                foreach (var xml3 in request.XML3API?.DSACH_CHI_TIET_DVKT?.CHI_TIET_DVKT)
                {
                    XML3_API xML3_API = new XML3_API();
                    xML3_API.MA_LK = xml3.MA_LK;
                    xML3_API.STT = xml3.STT;
                    xML3_API.MA_DICH_VU = xml3.MA_DICH_VU;
                    xML3_API.MA_VAT_TU = xml3.MA_VAT_TU;
                    xML3_API.MA_NHOM = xml3.MA_NHOM;
                    xML3_API.GOI_VTYT = xml3.GOI_VTYT;
                    xML3_API.TEN_VAT_TU = xml3.TEN_VAT_TU;
                    xML3_API.TEN_DICH_VU = xml3.TEN_DICH_VU;
                    xML3_API.DON_VI_TINH = xml3.DON_VI_TINH;
                    xML3_API.PHAM_VI = xml3.PHAM_VI;
                    xML3_API.SO_LUONG = xml3.SO_LUONG;
                    xML3_API.DON_GIA = xml3.DON_GIA;
                    xML3_API.TT_THAU = xml3.TT_THAU;
                    xML3_API.TYLE_TT = xml3.TYLE_TT;
                    xML3_API.THANH_TIEN = xml3.THANH_TIEN;
                    xML3_API.T_TRANTT = xml3.T_TRANTT;
                    xML3_API.MUC_HUONG = xml3.MUC_HUONG;
                    xML3_API.T_NGUONKHAC = xml3.T_NGUONKHAC;
                    xML3_API.T_BNTT = xml3.T_BNTT;
                    xML3_API.T_BHTT = xml3.T_BHTT;
                    xML3_API.T_BNCCT = xml3.T_BNCCT;
                    xML3_API.T_NGOAIDS = xml3.T_NGOAIDS;
                    xML3_API.MA_KHOA = xml3.MA_KHOA;
                    xML3_API.MA_GIUONG = xml3.MA_GIUONG;
                    xML3_API.MA_BAC_SI = xml3.MA_BAC_SI;
                    xML3_API.MA_BENH = xml3.MA_BENH;
                    DateTime ngayYl = new DateTime();
                    DateTime.TryParse(xml3.NGAY_YL, out ngayYl);
                    if (ngayYl.Year != 1)
                    {
                        xML3_API.NGAY_YL = ngayYl;
                    }
                    else
                    {
                        xML3_API.NGAY_YL = null;
                    }
                    DateTime ngayKQ = new DateTime();
                    DateTime.TryParse(xml3.NGAY_KQ, out ngayKQ);
                    if (ngayKQ.Year != 1)
                    {
                        xML3_API.NGAY_KQ = ngayKQ;
                    }
                    else
                    {
                        xML3_API.NGAY_KQ = null;
                    }
                   
                    
                    xML3_API.MA_PTTT = xml3.MA_PTTT;
                    xML3_API.MA_CSKCB = request.XML1API.TONG_HOP.MA_CSKCB;
                    listXML3.Add(xML3_API);
                }

            }

            xml1.FileName = xml1.MA_CSKCB + "_" + xml1.MA_BN + "_" + xml1.HO_TEN+"_"+ DateTime.Now.ToString("yyMMddHHmmssff");
            xml1.XML2_APIs = listXML2;
            xml1.XML3_APIs = listXML3;
            xml1.HospitalId = res.Data.HospitalId;



            //haimaster 25.7.2022
            Uti.HandleCronJobLog(_logger, $"Check API {res.Data.Email} ({res.Data.MA_CSKCB})", nameof(ApiController.UploadFile));
            try
            {
                var uploadEdfile = await _FilesService.FileCheckAPI(xml1);
                await _HoSoManagementService.ValidateFiles(currentHospitalId: res.Data.HospitalId, currentMaCSKCB: xml1.MA_CSKCB, fileNames: xml1.FileName, isAPICheck: true);

                //return Ok(uploadEdfile);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"S1. Haimaster Check API {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.CheckFileAPI));
            }

            //var alo = await _FilesService.GetXml1ErrorItemsByFileName(request.FileName);
            var ketQua = await _FilesService.GetXml1ErrorItemsByFileNameAPI(xml1.FileName);



            return Ok(ketQua);



            //_FilesService.FileUploadAPI(fileUp, res.Data.HospitalId, res.Data.HospitalName, res.Data.Id, res.Data.MA_CSKCB);
            //List<string> failedFiles = new List<string>();
            //string successMessage = "";
            //string failedMessage = "";
            //string uploadStatus = "";
            //string uploadTime = "";

            //var uploadEdfile = new ResponseModel<dynamic> { Code = System.Net.HttpStatusCode.OK, Data = new { successMessage, failedMessage, uploadStatus, uploadTime, failedFiles }, Message = "" };

            //System.Threading.Thread.Sleep(9000);
            //return Ok(uploadEdfile);
        }
    }

    public class FileUploading
    {
        public string Token { get; set; }
        public List<FileHoSo> ArrayFiles { get; set; }

    }


    public class FileUploadingAPI
    {
        public Root Token { get; set; }


    }

    public class FileHoSo
    {
        public string FileName { get; set; }
        public string FileBytes { get; set; }

    }

    public class FileCheckError
    {
        public string Token { get; set; }
        public List<string> FileName { get; set; }

    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class CHITIETCL
    {
        public string MA_LK { get; set; }
        public int STT { get; set; }
        public string MA_DICH_VU { get; set; }
        public string MA_CHI_SO { get; set; }
        public string TEN_CHI_SO { get; set; }
        public double GIA_TRI { get; set; }
        public string MA_MAY { get; set; }
        public string MO_TA { get; set; }
        public string KET_LUAN { get; set; }
        public object NGAY_KQ { get; set; }
    }

    public class CHITIETDVKT
    {
        public string MA_LK { get; set; }
        public int STT { get; set; }
        public string MA_DICH_VU { get; set; }
        public string MA_VAT_TU { get; set; }
        public byte MA_NHOM { get; set; }
        public string GOI_VTYT { get; set; }
        public string TEN_VAT_TU { get; set; }
        public string TEN_DICH_VU { get; set; }
        public string DON_VI_TINH { get; set; }
        public byte PHAM_VI { get; set; }
        public int SO_LUONG { get; set; }
        public int DON_GIA { get; set; }
        public string TT_THAU { get; set; }
        public byte TYLE_TT { get; set; }
        public int THANH_TIEN { get; set; }
        public double T_TRANTT { get; set; }
        public byte MUC_HUONG { get; set; }
        public int T_NGUONKHAC { get; set; }
        public int T_BNTT { get; set; }
        public int T_BHTT { get; set; }
        public int T_BNCCT { get; set; }
        public int T_NGOAIDS { get; set; }
        public string MA_KHOA { get; set; }
        public string MA_GIUONG { get; set; }
        public string MA_BAC_SI { get; set; }
        public string MA_BENH { get; set; }
        public string NGAY_YL { get; set; }
        public string NGAY_KQ { get; set; }
        public byte MA_PTTT { get; set; }
    }

    public class CHITIETTHUOC
    {
        public string MA_LK { get; set; }
        public int STT { get; set; }
        public string MA_THUOC { get; set; }
        public byte MA_NHOM { get; set; }
        public string TEN_THUOC { get; set; }
        public string DON_VI_TINH { get; set; }
        public string HAM_LUONG { get; set; }
        public string DUONG_DUNG { get; set; }
        public string LIEU_DUNG { get; set; }
        public string SO_DANG_KY { get; set; }
        public string TT_THAU { get; set; }
        public byte PHAM_VI { get; set; }
        public int SO_LUONG { get; set; }
        public int DON_GIA { get; set; }
        public byte TYLE_TT { get; set; }
        public int THANH_TIEN { get; set; }
        public byte MUC_HUONG { get; set; }
        public int T_NGUONKHAC { get; set; }
        public int T_BNTT { get; set; }
        public int T_BHTT { get; set; }
        public int T_BNCCT { get; set; }
        public int T_NGOAIDS { get; set; }
        public string MA_KHOA { get; set; }
        public string MA_BAC_SI { get; set; }
        public string MA_BENH { get; set; }
        public string NGAY_YL { get; set; }
        public byte MA_PTTT { get; set; }
    }

    public class DSACHCHITIETCLS
    {
        public List<CHITIETCL> CHI_TIET_CLS { get; set; }
    }

    public class DSACHCHITIETDVKT
    {
        public List<CHITIETDVKT> CHI_TIET_DVKT { get; set; }
    }

    public class DSACHCHITIETTHUOC
    {
        public List<CHITIETTHUOC> CHI_TIET_THUOC { get; set; }
    }

    public class Root
    {
        public string token { get; set; }
        public XML1API XML1API { get; set; }
        public XML2API XML2API { get; set; }
        public XML3API XML3API { get; set; }
        public XML4API XML4API { get; set; }
    }

    public class TONGHOP
    {

        public string MA_CSKCB { get; set; }
        public string MA_LK { get; set; }
        public int STT { get; set; }
        public string MA_BN { get; set; }
        public string HO_TEN { get; set; }
        public string NGAY_SINH { get; set; }
        public byte GIOI_TINH { get; set; }
        public string DIA_CHI { get; set; }
        public string MA_THE { get; set; }
        public string MA_DKBD { get; set; }
    }

    public class XML1API
    {
        public TONGHOP TONG_HOP { get; set; }
    }

    public class XML2API
    {
        public DSACHCHITIETTHUOC DSACH_CHI_TIET_THUOC { get; set; }
    }

    public class XML3API
    {
        public DSACHCHITIETDVKT DSACH_CHI_TIET_DVKT { get; set; }
    }

    public class XML4API
    {
        public DSACHCHITIETCLS DSACH_CHI_TIET_CLS { get; set; }
    }



}
