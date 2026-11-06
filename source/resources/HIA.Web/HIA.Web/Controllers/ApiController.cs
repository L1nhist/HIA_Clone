using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files;
using HIA.Core.Models.FileUploads;
using HIA.Core.Models.Token;
using HIA.Core.Models.User;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Services.Auth;
using HIA.Services.Files;
//Linhist removed 29/03/25
//using HIA.Services.HosoManagement;
using HIA.Services.HosoManagement130;
using HIA.Services.HospitalManagement;
using HIA.Services.Report;
using HIA.Services.UserManagement;
using HIA.Services.Verify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

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
            return Ok("MDInsight API Running...");
        }

        private readonly IAuthService _authService;
        private readonly TokenHandler _tokenHandle;
        //Linhist removed 29/03/25
        //private readonly IFilesService _FilesService;
        //private readonly IHoSoManagementService _HoSoManagementService;
        private readonly IXml01Repository _hosoRepository;
        private readonly IFilesService130 _FilesService130;
        private readonly IHoSoManagementService130 _HoSoManagementService130;
        private readonly ILogger<ApiController> _logger;

        private readonly IHospitalService _hospitalService;
        private readonly IUserManagementService _userService;
        private readonly IXMLVerifyService _verifyService;
        private readonly IXMLValidateRepository _validateRepository;
        private readonly IBaseRepository<DMErrorMHTC> _dmErrorMHTCRepository;
        private readonly IBaseRepository<UserLogs> _userlogRepository;
        private readonly IBaseRepository<XMLState> _stateRepository;

        private readonly IBaseRepository<DMThuoc> _dmThuocRepository;
        private readonly IBaseRepository<DMDichVu> _dmDichVuRepository;
        private readonly IBaseRepository<DMVTYT> _dmVTYTRepository;
        private readonly IBaseRepository<DMThietBi> _dmThietBiRepository;
        private readonly IBaseRepository<DMDVGiuong> _dmDvGiuongRepository;
        private readonly IBaseRepository<DMMaGiuong> _dmMaGiuongRepository;
        private readonly IBaseRepository<DMKhoaBV> _dmKhoaBVRepository;
        private readonly IBaseRepository<DMBacSi> _dmBacSiRepository;
        private readonly IBaseRepository<DMCSKCB> _dmCSKCBRepository;


        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configs;
        private readonly IReportService _reportService;

        private string convertDate(string date)
        {
            string dday = DateTime.Now.Date.ToString();
            if (date.Length == 4)
            {
                return date;
            }
            if (date.Length >= 8)
            {
                string years = date.Substring(0, 4);
                string months = date.Substring(4, 2);
                string days = date.Substring(6, 2);
                dday = days + "/" + months + "/" + years;
                if (months == "00" && days == "00") dday = years;
            }
            return dday;
        }

        //Linhist changed 25/12/24
        //private VerifyCode APICHECK(NhanLichSuKCB2024 check, XML01OUT rows,string MA_CSKCB)
        private VerifyCode APICHECK(XML01OUT rows,string MA_CSKCB)
        {
         
            VerifyCode VRCode = new VerifyCode();
            VRCode.ErrorCode = "228";
            VRCode.ErrorContents = "Lỗi server BHYT";

            //Linhist added 25/12/24
            if (rows == null) return VRCode;

            //linhist added 25/12/24
            var lskcb = JsonSerializer.Deserialize<NhanLichSuKCB2024[]>(rows.ErrorDetail);
            foreach (var check in lskcb)
            {
                var dsLichSuKCB2018 = check.dsLichSuKCB2018;
                //Kiểm tra mã DKKCBBD

                //Linhist changed 25/12/24
                //if (check == null || rows == null)
                //{
                //    VRCode.ErrorCode = "228";
                //    VRCode.ErrorContents = "Lỗi server BHYT";
                //    return VRCode;
                //}
                //else
                //{
                if (check == null) return VRCode;

                try
                {
                    //Check ngay sinh
                    if (!string.IsNullOrEmpty(check.ngaySinh))
                    {
                        if (convertDate(rows.NGAY_SINH) != check.ngaySinh)
                        {
                            VRCode.ErrorCode = "216";
                            VRCode.ErrorContents = "Thẻ sai ngày sinh";

                            //Linhist added 25/12/24
                            return VRCode;
                        }
                    }

                    if (check.maKetQua == "000")
                    {
                        //Linhist changed 25/12/24
                        //if (dsLichSuKCB2018 != null)
                        //{
                        //    if (dsLichSuKCB2018.Count != 0)
                        //    {
                        if (dsLichSuKCB2018 != null && dsLichSuKCB2018.Count != 0)
                        {
                            try
                            {
                                foreach (var i in dsLichSuKCB2018)
                                {
                                    DateTime _ngayraThe = DateTime.ParseExact(i.ngayRa, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                                    DateTime _ngayraBN = DateTime.ParseExact(rows.NGAY_RA, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                                    DateTime _ngayvaoBN = DateTime.ParseExact(rows.NGAY_VAO, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                                    if (i.maCSKCB != MA_CSKCB)
                                    {
                                        if (_ngayraBN == _ngayraThe)
                                        {
                                            VRCode.ErrorCode = "2583";
                                            VRCode.ErrorContents = "Bệnh nhân đã đi khám chữa bệnh tại đơn vị khác, ra viện cùng ngày";
                                            return VRCode;

                                        }

                                        if (_ngayraThe >= _ngayvaoBN && _ngayraThe <= _ngayraBN)
                                        {
                                            VRCode.ErrorCode = "2584";
                                            VRCode.ErrorContents = "Bệnh nhân có phát sinh chi phí KCB tại đơn vị khác trong thời gian nằm Viện";
                                            return VRCode;
                                        }
                                    }
                                }
                            }
                            catch //(Exception e)
                            {

                            }
                            
                            //Linhist removed 25/12/24
                            //}
                        }

                        if (!string.IsNullOrEmpty(check.maDKBD))
                        {
                            if (check.maDKBD != rows.MA_DKBD)
                            {
                                VRCode.ErrorCode = "218";
                                VRCode.ErrorContents = "Thẻ sai nơi đăng ký KCB ban đầu";
                                return VRCode;
                            }
                        }

                        if (!string.IsNullOrEmpty(check.maDKBDMoi))
                        {
                            if (check.maDKBD != rows.MA_DKBD)
                            {
                                VRCode.ErrorCode = "2585";
                                VRCode.ErrorContents = "Thẻ sai nơi đăng ký KCB ban đầu (So với mã đăng ký KCB BĐ mới trả về từ cổng)";
                                return VRCode;
                            }
                        }

                        if (!string.IsNullOrEmpty(check.gioiTinh))
                        {
                            string GT = "";
                            if (rows.GIOI_TINH == 1) GT = "Nam";
                            if (rows.GIOI_TINH == 2) GT = "Nữ";
                            if (rows.GIOI_TINH == 3) GT = "Chưa xác định";

                            if (check.gioiTinh != GT)
                            {
                                VRCode.ErrorCode = "217";
                                VRCode.ErrorContents = "Thẻ sai giới tính";
                                return VRCode;
                            }
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(check.gtTheDen) && !string.IsNullOrEmpty(check.gtTheTu))
                            {
                                string[] gtthetu = rows.GT_THE_TU.Split(";");
                                string[] gttheden = rows.GT_THE_DEN.Split(";");
                                if (gtthetu.Length == 1)
                                {
                                    string gttu = convertDate(gtthetu[0]);
                                    if (gttu != check.gtTheTu)
                                    {
                                        VRCode.ErrorCode = "1183";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }

                                    if (DateTime.ParseExact(convertDate(rows.NGAY_VAO), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date < DateTime.ParseExact(check.gtTheTu, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
                                    {
                                        VRCode.ErrorCode = "211";
                                        VRCode.ErrorContents = "KCB khi chưa đến hạn";
                                        return VRCode;
                                    }
                                }
                                else
                                {
                                    string gttu = convertDate(gtthetu[0]);
                                    if (gttu != check.gtTheTu)
                                    {
                                        VRCode.ErrorCode = "1183";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }
                                    gttu = convertDate(gtthetu[1]);
                                    if (gttu != check.gtTheTuMoi)
                                    {
                                        VRCode.ErrorCode = "1183";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }
                                    if (DateTime.ParseExact(convertDate(rows.NGAY_VAO), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date < DateTime.ParseExact(check.gtTheTuMoi, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
                                    {
                                        VRCode.ErrorCode = "211";
                                        VRCode.ErrorContents = "KCB khi chưa đến hạn";
                                        return VRCode;
                                    }
                                }
                                if (gttheden.Length == 1)
                                {
                                    string gttd = convertDate(gttheden[0]);
                                    if (gttd != check.gtTheDen)
                                    {
                                        VRCode.ErrorCode = "1184";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }
                                    if (DateTime.ParseExact(convertDate(rows.NGAY_RA), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date > DateTime.ParseExact(check.gtTheDen, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
                                    {
                                        VRCode.ErrorCode = "212";
                                        VRCode.ErrorContents = "Hết hạn thẻ khi chưa ra viện";
                                        return VRCode;
                                    }
                                }
                                else
                                {
                                    string gttd = convertDate(gttheden[0]);
                                    if (gttd != check.gtTheDen)
                                    {
                                        VRCode.ErrorCode = "1184";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }
                                    gttd = convertDate(gttheden[1]);
                                    if (gttd != check.gtTheDenMoi)
                                    {
                                        VRCode.ErrorCode = "1184";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }
                                    if (DateTime.ParseExact(convertDate(rows.NGAY_RA), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date > DateTime.ParseExact(check.gtTheDenMoi, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
                                    {
                                        VRCode.ErrorCode = "212";
                                        VRCode.ErrorContents = "Hết hạn thẻ khi chưa ra viện";
                                        return VRCode;
                                    }
                                }
                            }
                        }
                        catch //(Exception e)
                        {

                        }


                        if (!string.IsNullOrEmpty(check.maKV))
                        {
                            if (check.maKV != rows.MA_KHUVUC)
                            {
                                VRCode.ErrorCode = "1968";
                                VRCode.ErrorContents = "Mã khu vực không đúng với giá trị thông tuyến trả về";
                                return VRCode;
                            }
                        }
                    }
                    if (check.maKetQua == "001")
                    {
                        VRCode.ErrorCode = "222";
                        VRCode.ErrorContents = "Thẻ BHYT do Bộ Quốc Phòng quản lý, đề nghị kiểm tra giấy tùy thân";
                        return VRCode;
                    }

                    if (check.maKetQua == "002")
                    {
                        VRCode.ErrorCode = "223";
                        VRCode.ErrorContents = "Thẻ BHYT do Bộ Công An quản lý, đề nghị kiểm tra giấy tùy thân";
                        return VRCode;
                    }
                    if (check.maKetQua == "003")
                    {
                        VRCode.ErrorCode = "3454";
                        VRCode.ErrorContents = "Thẻ BHYT cũ, hết giá trị sử dụng, đã được cấp thẻ mới";
                        return VRCode;
                    }
                    if (check.maKetQua == "010")
                    {
                        VRCode.ErrorCode = "010";
                        VRCode.ErrorContents = "Thẻ BHYT cũ, Còn giá trị sử dụng, đã được cấp thẻ mới";
                        return VRCode;
                    }
                    if (check.maKetQua == "051")
                    {
                        VRCode.ErrorCode = "209";
                        VRCode.ErrorContents = "Mã thẻ BHYT Không đúng";
                        return VRCode;
                    }
                    if (check.maKetQua == "052")
                    {
                        VRCode.ErrorCode = "2566";
                        VRCode.ErrorContents = "Mã tỉnh cấp thẻ BHYT (ký tự thứ 4,5) của thẻ BHYT không đúng";
                        return VRCode;
                    }
                    if (check.maKetQua == "053")
                    {
                        VRCode.ErrorCode = "2567";
                        VRCode.ErrorContents = "Mã quyền lợi BHYT (ký tự thứ 3) của thẻ BHYT không đúng";
                        return VRCode;
                    }
                    if (check.maKetQua == "050")
                    {
                        VRCode.ErrorCode = "214";
                        VRCode.ErrorContents = "Không thấy thông tin của thẻ BHYT";
                        return VRCode;
                    }
                    if (check.maKetQua == "060")
                    {
                        VRCode.ErrorCode = "215";
                        VRCode.ErrorContents = "Thẻ BHYT sai họ tên";
                        return VRCode;
                    }
                    if (check.maKetQua == "061")
                    {
                        VRCode.ErrorCode = "227";
                        VRCode.ErrorContents = "Thẻ BHYT sai họ tên (đúng ký tự đầu)";
                        return VRCode;
                    }
                    if (check.maKetQua == "070")
                    {
                        VRCode.ErrorCode = "216";
                        VRCode.ErrorContents = "Thẻ BHYT sai ngày sinh";
                        return VRCode;
                    }
                    if (check.maKetQua == "100")
                    {
                        VRCode.ErrorCode = "228";
                        VRCode.ErrorContents = "Lỗi khi lấy dữ liệu sổ thẻ";
                        return VRCode;
                    }
                    if (check.maKetQua == "101")
                    {
                        VRCode.ErrorCode = "221";
                        VRCode.ErrorContents = "Lỗi server BHYT";
                        return VRCode;
                    }
                    if (check.maKetQua == "110")
                    {
                        VRCode.ErrorCode = "230";
                        VRCode.ErrorContents = "Thẻ BHYT đã thu hồi";
                        return VRCode;
                    }
                    if (check.maKetQua == "120")
                    {
                        VRCode.ErrorCode = "231";
                        VRCode.ErrorContents = "Thẻ BHYT đã báo giảm";
                        return VRCode;
                    }
                    if (check.maKetQua == "121")
                    {
                        VRCode.ErrorCode = "232";
                        VRCode.ErrorContents = "Thẻ BHYT đã báo giảm chuyển ngoại tỉnh";
                        return VRCode;
                    }
                    if (check.maKetQua == "122")
                    {
                        VRCode.ErrorCode = "233";
                        VRCode.ErrorContents = "Thẻ BHYT đã báo giảm chuyển nội tỉnh";
                        return VRCode;
                    }
                    if (check.maKetQua == "123")
                    {
                        VRCode.ErrorCode = "234";
                        VRCode.ErrorContents = "Thẻ BHYT đã báo giảm do tăng lại cùng đơn vị";
                        return VRCode;
                    }
                    if (check.maKetQua == "124")
                    {
                        VRCode.ErrorCode = "235";
                        VRCode.ErrorContents = "Thẻ BHYT đã báo giảm ngừng tham gia";
                        return VRCode;
                    }
                    if (check.maKetQua == "130")
                    {
                        VRCode.ErrorCode = "236";
                        VRCode.ErrorContents = "Trẻ em không xuất trình thẻ BHYT";
                        return VRCode;
                    }
                    if (check.maKetQua == "205")
                    {
                        VRCode.ErrorCode = "2568";
                        VRCode.ErrorContents = "Lỗi sai định dạng tham số";
                        return VRCode;
                    }
                    if (check.maKetQua == "401")
                    {
                        VRCode.ErrorCode = "2569";
                        VRCode.ErrorContents = "Lỗi xác thực tài khoản";
                        return VRCode;
                    }
                    if (check.maKetQua == "054")
                    {
                        VRCode.ErrorCode = "2570";
                        VRCode.ErrorContents = "Số CCCD của cán bộ thực hiện tra cứu không tồn tại trong danh sách người sử dụng do CSKCB đăng ký";
                        return VRCode;
                    }
                    if (check.maKetQua == "055")
                    {
                        VRCode.ErrorCode = "2571";
                        VRCode.ErrorContents = "Họ và tên của cán bộ thực hiện tra cứu không khớp với số CCCD";
                        return VRCode;
                    }
                    if (check.maKetQua == "000")
                    {
                        VRCode.ErrorCode = "000";
                        VRCode.ErrorContents = "Thông tin thẻ BH chính xác";
                        return VRCode;
                    }
                    else
                    {
                        VRCode.ErrorCode = check.maKetQua;
                        VRCode.ErrorContents = "Lỗi mới updated của BH => Xử lý sau";
                        return VRCode;
                    }

                }
                catch //(Exception ex)
                {
                }

                //Linhist changed 25/12/24
                //}
            }
            return VRCode;
        }


        public ApiController(
            //Linhist removed 29/03/25
            //IFilesService FilesService,
            //IHoSoManagementService HoSoManagementService,
            IHoSoManagementService130 HoSoManagementService130,
            IAuthService authService,
            TokenHandler tokenHandle,
            IFilesService130 FilesService130,
            ILogger<ApiController> logger,
            IHospitalService hospitalService,
            IUserManagementService userService,
            IXMLVerifyService verifyService,
            IXMLValidateRepository validateRepository,
            IBaseRepository<UserLogs> userlogRepository,
            IBaseRepository<DMErrorMHTC> dmErrorMHTCRepository,
            IBaseRepository<XMLState> stateRepository,
            IBaseRepository<DMThuoc> dmThuocRepository,
            IBaseRepository<DMDichVu> dmDichVuRepository,
            IBaseRepository<DMVTYT> dmVTYTRepository,
            IBaseRepository<DMThietBi> dmThietBiRepository,
            IBaseRepository<DMDVGiuong> dmDvGiuongRepository,
            IBaseRepository<DMMaGiuong> dmMaGiuongRepository,
            IBaseRepository<DMKhoaBV> dmKhoaBVRepository,
            IBaseRepository<DMBacSi> dmBacSiRepository,
            IBaseRepository<DMCSKCB> dmCSKCBRepository,
            IReportService reportService,
            IXml01Repository hosoRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configs
        )
        {
            _authService = authService;
            _tokenHandle = tokenHandle;
            //Linhist removed 29/03/25
            //_FilesService = FilesService;
            //_HoSoManagementService = HoSoManagementService;
            _FilesService130 = FilesService130;
            _logger = logger;
            _HoSoManagementService130 = HoSoManagementService130;

            _hospitalService = hospitalService;
            _userService = userService;
            _verifyService = verifyService;
            _validateRepository = validateRepository;
            _userlogRepository = userlogRepository;
            _dmErrorMHTCRepository = dmErrorMHTCRepository;
            _hosoRepository = hosoRepository;
            _httpContextAccessor = httpContextAccessor;
            _configs = configs;
            _reportService = reportService;
            _stateRepository = stateRepository;
            _dmThuocRepository = dmThuocRepository;
            _dmDichVuRepository = dmDichVuRepository;
            _dmKhoaBVRepository = dmKhoaBVRepository;
            _dmVTYTRepository = dmVTYTRepository;
            _dmThietBiRepository = dmThietBiRepository;
            _dmBacSiRepository = dmBacSiRepository;
            _dmDvGiuongRepository = dmDvGiuongRepository;
            _dmMaGiuongRepository = dmMaGiuongRepository;
            _dmCSKCBRepository = dmCSKCBRepository;
        }

        #region Linhist added 14/08/24
        private async Task<User> GetAuthen(ResponseModel<TokenRequestModel> res)
        {
            if (res.Code != HttpStatusCode.OK) return null;
            //Uti.HandleCronJobLog(_logger, $"API.GetAuthen {res.Data.Id} {res.Data.MA_CSKCB} {res.Data.RoleId}", nameof(ApiController.UploadFile));
            //if (res.Code.ToString() != "OK") return null;
            var chk = await _userService.GetActiveUserAndHospital(res.Data.Id, res.Data.MA_CSKCB, res.Data.RoleId);
            if (chk.Code != HttpStatusCode.OK)
            {
              //  Uti.HandleCronJobLog(_logger, $"SEC1.GetAuthen {res.Data.Id} fail. User {res.Data.Email} ({res.Data.MA_CSKCB}).", nameof(ApiController.UploadFile));
                return null;
            }

            return chk.Data;
        }
        #endregion

        [HttpPost]
        [AllowAnonymous]
        [Route("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel request)
        {
            //Linhist changed 30/03/26 - lấy expiredat từ token trả về
            //var res = new ResponseModel<string>();
            var res = new ResponseModel<TokenResponseModel>();
            if (!ModelState.IsValid)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = string.Join(" | ", ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage));
                return NotFound(res);
            }

            //Linhist changed 30/03/26 - lấy expiredat từ token trả về
            //res = await _authService.SignInGetToken(request);
            //return Ok(new { Code = res.Code, Message = res.Message, Token = res.Data.ToString() });
            res = await _authService.SignInGetTokenResp(request);
            if (res.Data == null)
            {
                res.Code = HttpStatusCode.Unauthorized;
                res.Message = "Đăng nhập thất bại";
                return Unauthorized(res);
            }

            return Ok(new { Code = res.Code, Message = "", Token = res.Data.Token, ExpiredAt = res.Data.ExpireAt });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CheckToken")]
        public async Task<IActionResult> CheckToken(string token)
        {
            var res = _tokenHandle.IsTokenValid(token);
            if (res.Code != HttpStatusCode.OK) return Unauthorized();
            
            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();
            
            var result = new TokenResultModel()
            {
                Id = res.Data.Id,
                Email = res.Data.Email,
                FullName = res.Data.FullName,
                HospitalName = res.Data.HospitalName,
                MA_CSKCB = res.Data.MA_CSKCB,
                ExpireAt = res.Data.ExpireAt
            };
            return Ok(result);
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
            var userlog = new List<UserLogs>();
            var res = _tokenHandle.IsTokenValid(request.Token);
            //if (res.Code != HttpStatusCode.OK)
            //{
            //    return NotFound(res);
            //}

            //var chk = await _userService.IsActiveUserAndHospital(res.Data.Id, res.Data.MA_CSKCB, res.Data.RoleId);
            //if (!chk.Data)
            //{
            //    return Unauthorized(chk);
            //}

            //Linhist changed 12/08/24
            try
            {
                var user = await GetAuthen(res);
                if (user == null) return Unauthorized();
                if(request.ArrayFiles.Count>10) return BadRequest("Số lượng file gửi lên vượt quá 10 file");
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
            
                //bool IsFromHis = res.Data.Email == "APIVIP" || res.Data.Email == "DKTWTN";

                string filenames = "";
                request.ArrayFiles.Select(f => filenames += f.FileName + ";").ToArray();

                DateTime t = DateTime.Now;
                //haimaster 25.7.2022
                Uti.HandleCronJobLog(_logger, $"A1. Đẩy hồ sơ giám định {res.Data.Email} ({res.Data.MA_CSKCB}). FileName: {filenames}", nameof(ApiController.UploadFile));
                try
                {
                    var uploadEdfile = await _FilesService130.FileUploadAPI(fileUp, res.Data.HospitalId, res.Data.HospitalName, res.Data.Id, res.Data.MA_CSKCB, res.Data.Email, true, true);
                    //13.2.2025 User Logs
                    //if (res.Data.MA_CSKCB == "25001" || res.Data.MA_CSKCB== "91009")
                    //{
                        //var ipArray = Dns.GetHostAddresses(Dns.GetHostName())
                        //                .Where(x => x.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork))
                        //                .Select(x => Convert.ToString(x))
                        //                .ToArray();
                        userlog.Add(new UserLogs()
                        {
                            UserID = res.Data.Id.ToString(),
                            Email = res.Data.Email.ToString(),
                            Action = "Đẩy hồ sơ giám định qua API",
                            MA_CSKCB = res.Data.MA_CSKCB,
                            //UrlAccess = ipArray.Length > 0 ? string.Join(",", ipArray) : "",
                            LogTime = DateTime.Now,
                        });
                    //}
                    await _userlogRepository.InsertListAsync(userlog);
                    await _userlogRepository.SaveChangesAsync();

                    Uti.HandleCronJobLog(_logger, $"A2. Đã kiểm tra xong, user ID:{res.Data.Id} ({res.Data.MA_CSKCB}). FileName: {filenames} {(DateTime.Now-t).TotalMilliseconds}", nameof(ApiController.UploadFile));
                    return Ok(uploadEdfile);
                
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"A0. UploadFiles API {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
                }
            }
            catch (Exception ex1)
            {
                Uti.HandleCronJobLog(_logger, $"A0. UploadFiles API  {res.Data.Email} ({res.Data.MA_CSKCB}). Error: {ex1.Message}", nameof(ApiController.UploadFile));

            }

            return Ok(null);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("UploadFile130")]
        public async Task<IActionResult> UploadFile130([FromBody] FileUploading request)
        {
            var userlog = new List<UserLogs>();
            var res = _tokenHandle.IsTokenValid(request.Token);
            //if (res.Code != HttpStatusCode.OK)
            //{
            //    return NotFound(res);
            //}

            //var chk = await _userService.IsActiveUserAndHospital(res.Data.Id, res.Data.MA_CSKCB, res.Data.RoleId);
            //if (!chk.Data)
            //{
            //    Uti.HandleCronJobLog(_logger, $"SEC1. Check UserID {res.Data.Id} fail. User {res.Data.Email} ({res.Data.MA_CSKCB}).", nameof(ApiController.UploadFile));
            //    return Unauthorized(chk);
            //}

            //Linhist changed 12/08/24
            var user = await GetAuthen(res);
            if (user == null)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Check API User Faile {res.Code}", nameof(ApiController.UploadFile));
                return Unauthorized();
            }
            if (request.ArrayFiles.Count > 10)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Total Files >10 {res.Data.Email} ({res.Data.MA_CSKCB})", nameof(ApiController.UploadFile));
                return BadRequest("Số lượng file gửi lên vượt quá 10 file");
            }

            string filenames = "";
            FileUpload fileUp;
            try
            {
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
                fileUp = new FileUpload() { FormFiles = FormFiles };
                //bool IsFromHis = res.Data.Email == "APIVIP" || res.Data.Email == "DKTWTN";

                filenames = string.Join(";", request.ArrayFiles.Select(f => f.FileName));
            }
            catch(Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Check API Convert File Faile {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
                return BadRequest("File gửi lên không đúng định dạng");
            }

            //haimaster 25.7.2022
            Uti.HandleCronJobLog(_logger, $"S1.UploadFile130 Check HS {res.Data.Email} ({res.Data.MA_CSKCB}). FileName: {filenames}", nameof(ApiController.UploadFile));
            try
            {
                var uploadEdfile = await _FilesService130.FileUploadAPI(fileUp, res.Data.HospitalId, res.Data.HospitalName, res.Data.Id, res.Data.MA_CSKCB, res.Data.Email, true, false);
                //Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Upload OK {res.Data.Email} ({res.Data.MA_CSKCB}) - Message: {uploadEdfile.Message.ToString()}", nameof(ApiController.UploadFile));

                return Ok(uploadEdfile);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Check API {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
            }

            return Ok(null);
        }

        #region Linhist removed 29/03/25
        [HttpPost]
        [AllowAnonymous]
        [Route("CheckError")]
        public async Task<IActionResult> CheckError(FileCheckError request)
        {
            DateTime t = DateTime.Now;
            var res = _tokenHandle.IsTokenValid(request.Token);

            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var maCSKCB = res.Data.MA_CSKCB;
            
            if (request.FileName == null || request.FileName.Count == 0) return BadRequest("Không tìm thấy hồ sơ giám định");

            try
            {
                var xml01 = await _hosoRepository.GetItemOrderBy(x => x.FileName == request.FileName.FirstOrDefault() && x.MA_CSKCB == maCSKCB && x.IsDelete != true, x => x.OrderByDescending(o => o.XML01ID));
                if (xml01 == null) return NotFound("Không tìm thấy hồ sơ giám định");

                var state = await _stateRepository.GetItem(x => x.XML01ID == xml01.XML01ID);
                if (state == null || state.CheckDate == null) return Ok(new ResponseModel<IEnumerable<XML1ErrorInfoModel>>(HttpStatusCode.OK, "Hồ sơ chưa giám định", null));
                if (state.TONG_SO_LOI == 0) return Ok(new ResponseModel<IEnumerable<XML1ErrorInfoModel>>(HttpStatusCode.OK, "Hồ sơ không có lỗi", null));

                var errs = await _FilesService130.GetXml1ErrorItemByFileName(request.FileName, maCSKCB);
                var dmes = await _dmErrorMHTCRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.EMInternal == true);

                Uti.HandleCronJobLog(_logger, $"CheckError.F1 Time: {(DateTime.Now - t).TotalMilliseconds} {res.Data.Email} ({res.Data.MA_CSKCB}). FileName: {request.FileName.FirstOrDefault()}", nameof(ApiController.CheckError));

                if (dmes == null || !dmes.Any()) return Ok(errs);
                if (errs == null || errs.Data == null) return BadRequest("Không tìm thấy hồ sơ giám định");

                var cods = dmes.Select(x => x.EMErrorCode).ToList();
                errs.Data = errs.Data.Where(e => !cods.Contains(e.XeErrorCode)).ToList();
                return Ok(errs);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"CheckError.F0 Time: {(DateTime.Now - t).TotalMilliseconds} {res.Data.Email} ({res.Data.MA_CSKCB}). FileName: {request.FileName.FirstOrDefault()} + Lỗi:{ex}", nameof(ApiController.CheckError));
            }

            return BadRequest("Không tìm thấy hồ sơ giám định");
        }
        #endregion

        [HttpPost]
        [AllowAnonymous]
        [Route("APIVerifyUpdated")]
        public async Task<IActionResult> APIVerifyUpdated(UpdatedVerifyData request)
        {
            DateTime t = DateTime.Now;
            var res = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var data = request.Data;
            if (string.IsNullOrEmpty(data?.ErrorDetail) || data.ErrorDetail.Length < 50)
                return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "", false));

            try
            {
                var VRcode = APICHECK(data, res.Data.MA_CSKCB.ToString());
                var updated = await _verifyService.UpdatedVerify(new CheckThe4210
                {
                    MA_BN = data.MA_BN,
                    MA_LK = data.MA_LK,
                    MA_CSKCB = user.Hospital.MA_CSKCB,
                    MA_DKBD = data.MA_DKBD,
                    MA_KHUVUC = data.MA_KHUVUC,
                    MA_THE_BHYT = data.MA_THE_BHYT,
                    HO_TEN = data.HO_TEN,
                    GIOI_TINH = data.GIOI_TINH,
                    NGAY_SINH = EvalHelper.ToDateSql(data.NGAY_SINH),
                    GT_THE_DEN = data.GT_THE_DEN,
                    GT_THE_TU = data.GT_THE_TU,
                    TG_CHECK_THE = DateTime.Now,
                    ERROR_CODE = VRcode.ErrorCode,
                    ERROR_CONTENTS = data.ErrorDetail,
                });

                if (updated == 1)
                {
                    if (!string.IsNullOrEmpty(data.MA_LK) && !string.IsNullOrEmpty(user.Hospital.MA_CSKCB))
                    {
                        // Kiểm tra lại đoạn code, bổ sung kiểm tra null cho user.Hospital và các thuộc tính liên quan
                        try
                        {
                            // Kiểm tra user.Hospital null
                            if (user?.Hospital == null)
                            {
                                Uti.HandleCronJobLog(_logger, $"user.Hospital is null. MA_LK:{data.MA_LK}, Ma The: {data.MA_THE_BHYT}", nameof(ApiController.APIVerifyUpdated));
                                return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "Hospital info missing", false));
                            }

                            // Kiểm tra data.MA_LK null
                            if (string.IsNullOrEmpty(data?.MA_LK))
                            {
                                Uti.HandleCronJobLog(_logger, $"data.MA_LK is null. Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
                                return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "MA_LK missing", false));
                            }

                            var xml01 = await _hosoRepository.GetAllByMaLK(user.Hospital.MA_CSKCB, data.MA_LK);
                            if (xml01 == null)
                            {
                                Uti.HandleCronJobLog(_logger, $"Không lấy được xmlids: Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
                            }
                            if (xml01 != null && xml01.Any())
                            {
                                var xmlIds = xml01.Select(x => x.XML01ID).ToArray();
                                try
                                {
                                    await _HoSoManagementService130.RemoveHosoFromValidate(xmlIds);
                                    await _HoSoManagementService130.AppendHosoToValidate(user.Hospital.MA_CSKCB, xmlIds);
                                }
                                catch (Exception ex)
                                {
                                    Uti.HandleCronJobLog(_logger, $"Không lấy được xmlids:{xml01.Count()}, Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB} + Lỗi:{ex}", nameof(ApiController.APIVerifyUpdated));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var maCSKCB = user?.Hospital?.MA_CSKCB ?? "null";
                            Uti.HandleCronJobLog(_logger, $"Không chuyển thông tin qua giám định được MA_LK:{data?.MA_LK}, Ma The: {data?.MA_THE_BHYT}, MA_CSKCB: {maCSKCB} + Lỗi:{ex} + {ex.ToString()}", nameof(ApiController.APIVerifyUpdated));
                        }
                    }
                    else
                    {
                        Uti.HandleCronJobLog(_logger, $"Không lấy được MA_LK:{data.MA_LK}, Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
                    }

                    Uti.HandleCronJobLog(_logger, $"CheckThe4210 Updated Thanh cong:MA BN: {data.MA_BN}, Ma THE:{data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
                    return Ok(new ResponseModel<bool>(HttpStatusCode.OK, "", true));
                }
                else if (updated == -1)
                {
                    return Ok(new ResponseModel<bool>(HttpStatusCode.Ambiguous, "", true));
                }
                else
                {
                    return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "", false));
                }
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Khong the check the: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB} {ex}", nameof(ApiController.APIVerifyUpdated));
                return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "", false));
            }
        }


        #region Linhist added 13/05/26
        [HttpPost]
        [AllowAnonymous]
        [Route("ClientVerify")]
        public async Task<IActionResult> ClientVerify(ClientVerifiedData request)
        {
            var resp = new ResponseModel<bool>();

            DateTime t = DateTime.Now;
            var res = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(res);
            if (user == null || user.Hospital == null || user.Hospital.MA_CSKCB != request.MA_CSKCB) return Unauthorized();

            try
            {
                var updated = await _verifyService.UpdatedVerify(new CheckThe4210
                {
                    MA_BN = request.MA_BN,
                    MA_LK = request.MA_LK,
                    MA_CSKCB = request.MA_CSKCB,
                    MA_DKBD = request.MA_DKBD,
                    MA_KHUVUC = request.MA_KHUVUC,
                    MA_THE_BHYT = request.MA_THE_BHYT,
                    HO_TEN = request.HO_TEN,
                    GIOI_TINH = request.GIOI_TINH,
                    NGAY_SINH = EvalHelper.ToDateSql(request.NGAY_SINH),
                    GT_THE_DEN = request.GT_THE_DEN,
                    GT_THE_TU = request.GT_THE_TU,
                    TG_CHECK_THE = DateTime.Now,
                    ERROR_CODE = request.MaLoi,
                    ERROR_CONTENTS = request.NDLoi,
                });
                
                if (updated == -1)
                {
                    resp.Code = HttpStatusCode.Ambiguous;
                    return Ok(resp);
                }
                else if (updated != 1)
                {
                    resp.Code = HttpStatusCode.InternalServerError;
                    return Ok(resp);
                }
                
                var xml01 = await _hosoRepository.GetAllByMaLK(request.MA_CSKCB, request.MA_LK);
                if (xml01 == null)
                {
                    Uti.HandleCronJobLog(_logger, $"Không lấy được xmlids: Ma The: {request.MA_THE_BHYT}, MA_CSKCB: {request.MA_CSKCB}", nameof(ClientVerify));
                    resp.Code = HttpStatusCode.NotFound;
                    return Ok(resp);
                }

                var xmlIds = xml01.Select(x => x.XML01ID).ToArray();
                try
                {
                    await _HoSoManagementService130.RemoveHosoFromValidate(xmlIds);
                    await _HoSoManagementService130.AppendHosoToValidate(request.MA_CSKCB, xmlIds);
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Không lấy được xmlids:{xml01.Count()}, Ma The: {request.MA_THE_BHYT}, MA_CSKCB: {request.MA_CSKCB} + Lỗi:{ex}", nameof(ClientVerify));
                }

                Uti.HandleCronJobLog(_logger, $"CheckThe4210 Updated Thanh cong:MA BN: {request.MA_BN}, Ma THE:{request.MA_THE_BHYT}, MA_CSKCB: {request.MA_CSKCB}", nameof(ClientVerify));
                resp.Code = HttpStatusCode.OK;
                resp.Data = true;
                return Ok(resp);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Khong the check the: {request.MA_THE_BHYT}, MA_CSKCB: {request.MA_CSKCB} {ex}", nameof(ClientVerify));
                resp.Code = HttpStatusCode.InternalServerError;
                return Ok(resp);
            }
        }
        #endregion

        #region Linhist removed 29/03/25
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("CheckFileAPI")]
        //public async Task<IActionResult> CheckFileAPI([FromBody] Root request)
        //{
        //    var res = _tokenHandle.IsTokenValid(request.token);
        //    if (res.Code != HttpStatusCode.OK)
        //    {
        //        return NotFound(res);
        //    }

        //    XML1_API xml1 = new();
        //    if (request.XML1API?.TONG_HOP != null)
        //    {

        //        xml1.MA_CSKCB = request.XML1API.TONG_HOP.MA_CSKCB;
        //        xml1.MA_LK = request.XML1API.TONG_HOP.MA_LK;
        //        xml1.STT = request.XML1API.TONG_HOP.STT;
        //        xml1.MA_BN = request.XML1API.TONG_HOP.MA_BN;
        //        xml1.HO_TEN = request.XML1API.TONG_HOP.HO_TEN;
        //        DateTime ngaysinh = new DateTime(1900, 1, 1);
        //        DateTime.TryParse(request.XML1API.TONG_HOP.NGAY_SINH, out ngaysinh);
        //        if (ngaysinh.Year != 1)
        //        {
        //            xml1.NGAY_SINH = ngaysinh;
        //        }
        //        else
        //        {
        //            xml1.NGAY_SINH = null;
        //        }
        //        xml1.GIOI_TINH = request.XML1API.TONG_HOP.GIOI_TINH;
        //        xml1.DIA_CHI = request.XML1API.TONG_HOP.DIA_CHI;
        //        xml1.MA_THE = request.XML1API.TONG_HOP.MA_THE;
        //        xml1.MA_DKBD = request.XML1API.TONG_HOP.MA_DKBD;
        //    }

        //    List<XML2_API> listXML2 = new();

        //    if (request.XML2API?.DSACH_CHI_TIET_THUOC?.CHI_TIET_THUOC?.Count > 0)
        //    {
        //        foreach (var xml2 in request.XML2API?.DSACH_CHI_TIET_THUOC?.CHI_TIET_THUOC)
        //        {
        //            XML2_API xML2_API = new XML2_API();
        //            xML2_API.MA_LK = xml2.MA_LK;
        //            xML2_API.STT = xml2.STT;
        //            xML2_API.MA_THUOC = xml2.MA_THUOC;
        //            xML2_API.MA_NHOM = xml2.MA_NHOM;
        //            xML2_API.TEN_THUOC = xml2.TEN_THUOC;
        //            xML2_API.DON_VI_TINH = xml2.DON_VI_TINH;
        //            xML2_API.HAM_LUONG = xml2.HAM_LUONG;
        //            xML2_API.DUONG_DUNG = xml2.DUONG_DUNG;
        //            xML2_API.LIEU_DUNG = xml2.LIEU_DUNG;
        //            xML2_API.SO_DANG_KY = xml2.SO_DANG_KY;
        //            xML2_API.TT_THAU = xml2.TT_THAU;
        //            xML2_API.PHAM_VI = xml2.PHAM_VI;
        //            xML2_API.SO_LUONG = xml2.SO_LUONG;
        //            xML2_API.DON_GIA = xml2.DON_GIA;
        //            xML2_API.TYLE_TT = xml2.TYLE_TT;
        //            xML2_API.THANH_TIEN = xml2.THANH_TIEN;
        //            xML2_API.MUC_HUONG = xml2.MUC_HUONG;
        //            xML2_API.T_NGUONKHAC = xml2.T_NGUONKHAC;
        //            xML2_API.T_BNTT = xml2.T_BNTT;
        //            xML2_API.T_BHTT = xml2.T_BHTT;
        //            xML2_API.T_BNCCT = xml2.T_BNCCT;
        //            xML2_API.T_NGOAIDS = xml2.T_NGOAIDS;
        //            xML2_API.MA_KHOA = xml2.MA_KHOA;
        //            xML2_API.MA_BAC_SI = xml2.MA_BAC_SI;
        //            xML2_API.MA_BENH = xml2.MA_BENH;
        //            DateTime ngayYl = new DateTime();
        //            DateTime.TryParse(xml2.NGAY_YL, out ngayYl);
        //            if (ngayYl.Year != 1)
        //            {
        //                xML2_API.NGAY_YL = ngayYl;
        //            }
        //            else
        //            {
        //                xML2_API.NGAY_YL = null;
        //            }

        //            xML2_API.MA_PTTT = xml2.MA_PTTT;
        //            xML2_API.MA_CSKCB = request.XML1API.TONG_HOP.MA_CSKCB;
        //            listXML2.Add(xML2_API);
        //        }

        //    }

        //    List<XML3_API> listXML3 = new();

        //    if (request.XML3API?.DSACH_CHI_TIET_DVKT?.CHI_TIET_DVKT?.Count > 0)
        //    {
        //        foreach (var xml3 in request.XML3API?.DSACH_CHI_TIET_DVKT?.CHI_TIET_DVKT)
        //        {
        //            XML3_API xML3_API = new XML3_API();
        //            xML3_API.MA_LK = xml3.MA_LK;
        //            xML3_API.STT = xml3.STT;
        //            xML3_API.MA_DICH_VU = xml3.MA_DICH_VU;
        //            xML3_API.MA_VAT_TU = xml3.MA_VAT_TU;
        //            xML3_API.MA_NHOM = xml3.MA_NHOM;
        //            xML3_API.GOI_VTYT = xml3.GOI_VTYT;
        //            xML3_API.TEN_VAT_TU = xml3.TEN_VAT_TU;
        //            xML3_API.TEN_DICH_VU = xml3.TEN_DICH_VU;
        //            xML3_API.DON_VI_TINH = xml3.DON_VI_TINH;
        //            xML3_API.PHAM_VI = xml3.PHAM_VI;
        //            xML3_API.SO_LUONG = xml3.SO_LUONG;
        //            xML3_API.DON_GIA = xml3.DON_GIA;
        //            xML3_API.TT_THAU = xml3.TT_THAU;
        //            xML3_API.TYLE_TT = xml3.TYLE_TT;
        //            xML3_API.THANH_TIEN = xml3.THANH_TIEN;
        //            xML3_API.T_TRANTT = xml3.T_TRANTT;
        //            xML3_API.MUC_HUONG = xml3.MUC_HUONG;
        //            xML3_API.T_NGUONKHAC = xml3.T_NGUONKHAC;
        //            xML3_API.T_BNTT = xml3.T_BNTT;
        //            xML3_API.T_BHTT = xml3.T_BHTT;
        //            xML3_API.T_BNCCT = xml3.T_BNCCT;
        //            xML3_API.T_NGOAIDS = xml3.T_NGOAIDS;
        //            xML3_API.MA_KHOA = xml3.MA_KHOA;
        //            xML3_API.MA_GIUONG = xml3.MA_GIUONG;
        //            xML3_API.MA_BAC_SI = xml3.MA_BAC_SI;
        //            xML3_API.MA_BENH = xml3.MA_BENH;
        //            DateTime ngayYl = new DateTime();
        //            DateTime.TryParse(xml3.NGAY_YL, out ngayYl);
        //            if (ngayYl.Year != 1)
        //            {
        //                xML3_API.NGAY_YL = ngayYl;
        //            }
        //            else
        //            {
        //                xML3_API.NGAY_YL = null;
        //            }
        //            DateTime ngayKQ = new DateTime();
        //            DateTime.TryParse(xml3.NGAY_KQ, out ngayKQ);
        //            if (ngayKQ.Year != 1)
        //            {
        //                xML3_API.NGAY_KQ = ngayKQ;
        //            }
        //            else
        //            {
        //                xML3_API.NGAY_KQ = null;
        //            }


        //            xML3_API.MA_PTTT = xml3.MA_PTTT;
        //            xML3_API.MA_CSKCB = request.XML1API.TONG_HOP.MA_CSKCB;
        //            listXML3.Add(xML3_API);
        //        }

        //    }

        //    xml1.FileName = xml1.MA_CSKCB + "_" + xml1.MA_BN + "_" + xml1.HO_TEN+"_"+ DateTime.Now.ToString("yyMMddHHmmssff");
        //    xml1.XML2_APIs = listXML2;
        //    xml1.XML3_APIs = listXML3;
        //    xml1.HospitalId = res.Data.HospitalId;

        //    //haimaster 25.7.2022
        //    Uti.HandleCronJobLog(_logger, $"Check API {res.Data.Email} ({res.Data.MA_CSKCB})", nameof(ApiController.UploadFile));
        //    try
        //    {
        //        var uploadEdfile = await _FilesService.FileCheckAPI(xml1);
        //        await _HoSoManagementService.ValidateFiles(currentHospitalId: res.Data.HospitalId, currentMaCSKCB: xml1.MA_CSKCB, fileNames: xml1.FileName, isAPICheck: true);

        //        //return Ok(uploadEdfile);
        //    }
        //    catch (Exception ex)
        //    {
        //        Uti.HandleCronJobLog(_logger, $"S1. Haimaster Check API {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.CheckFileAPI));
        //    }

        //    //var alo = await _FilesService.GetXml1ErrorItemsByFileName(request.FileName);
        //    var ketQua = await _FilesService.GetXml1ErrorItemsByFileNameAPI(xml1.FileName);

        //    return Ok(ketQua);
        //}

        //#region Linhist added 11/05/23
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("GetFilesByDate")]
        //[Produces("application/xml")]
        //public async Task<IActionResult> GetFilesByDate(FileDownload request)
        //{
        //    var res = _tokenHandle.IsTokenValid(request.Token);
        //    return Unauthorized(res);

        //    if (request == null || string.IsNullOrEmpty(request.Hospital))
        //    {
        //        return NotFound(request);
        //    }

        //    //if (RequestLimit.IsLimited("/api/getfilesbydate", ipAdress))
        //    //{
        //    //    return Forbid("Request is not allowed");
        //    //}

        //    //Linhist added 11/06/23
        //    //Kiem tra token dang nhap
        //    res = _tokenHandle.IsTokenValid(request.Token);
        //    if (res.Code != HttpStatusCode.OK)
        //    {
        //        return NotFound(res);
        //    }

        //    //Linhist added 09/08/23
        //    string fileGet = string.IsNullOrEmpty(request.FileName) ? "All files" : request.FileName;
        //    Uti.HandleCronJobLog(_logger, $"User {res.Data.FullName} ({res.Data.Email}) call API to Get File(s) from {res.Data.MA_CSKCB}: {fileGet} {res.Data.Email}", nameof(ApiController.GetFilesByDate));

        //    bool isMaster = res.Data.RoleId == RoleConstant.MASTER;
        //    bool isAdmin = res.Data.RoleId == RoleConstant.ADMIN;
        //    if (!isAdmin && !isMaster)
        //    {
        //        return Unauthorized(res);
        //    }

        //    //Kiem tra gioi han request 5 lan/1s
        //    //string ipAdress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //    if (RateLimit.IsLimited("/api/getfilesbydate", request.Token))
        //    {
        //        return Forbid("Request is not allowed");
        //    }

        //    //Kiem tra thu muc luu tru file (neu ko ton tai thi bao loi luon)
        //    DateTime dte = DateTime.Now;
        //    DateTime.TryParseExact(request.FromDate, new string[] { "dd-MM-yyyy", "dd/MM/yyyy", "dd-MM-yy", "dd/MM/yy", "ddMMyyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out dte);
        //    string dirPath = Uti.GenerateFilePath(request.Hospital, dte);
        //    if (!Directory.Exists(dirPath))
        //    {
        //        return NotFound(request);
        //    }

        //    //Kiem tra MA CSKCB co thuoc User ko
        //    bool isBoYte = res.Data.MA_CSKCB == "BOYTE";
        //    bool isCungBV = res.Data.MA_CSKCB == request.Hospital;
        //    if (!isMaster && !isBoYte && !isCungBV)
        //    {
        //        //var hasHospital = await _hospitalService.HasIncludedChild(res.Data.HospitalId, request.Hospital);
        //        if (res.Data.MA_CSKCB.Length >= 5 || request.Hospital.StartsWith(res.Data.MA_CSKCB))
        //        {
        //            return NotFound(request.Hospital);
        //        }
        //    }

        //    string fileContent = "";
        //    if (!string.IsNullOrEmpty(request.FileName))
        //    {
        //        string fileLoc = $"{dirPath}\\{request.FileName}";
        //        try
        //        {
        //            fileContent = System.IO.File.ReadAllText(fileLoc);
        //            return new ContentResult
        //            {
        //                ContentType = "application/xml",
        //                Content = fileContent,
        //                StatusCode = 200
        //            };
        //        }
        //        catch
        //        {
        //            return NotFound(request);
        //        }
        //    }

        //    var files = await _HoSoManagementService.ListFilesByCreateDate(dte, request.Hospital);
        //    if (files == null)
        //    {
        //        return NotFound(files);
        //    }

        //    foreach (var file in files)
        //    {
        //        fileContent += $"<FILE_NAME>{file}</FILE_NAME>{Environment.NewLine}";
        //    }

        //    return new ContentResult
        //    {
        //        ContentType = "application/xml",
        //        Content = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><FILE_LIST>{Environment.NewLine}{fileContent}</FILE_LIST>",
        //        StatusCode = 200
        //    };
        //}
        //#endregion
        #endregion

        #region Linhist added 14/08/24
        [HttpPost]
        [AllowAnonymous]
        [Route("GetXMLVerify")]
        public async Task<IActionResult> GetXMLVerify(AuthToken TK)
        {
            //Uti.HandleCronJobLog(_logger, $"API.GetXMLVerify. {TK.Token}", nameof(ApiController.CheckFileAPI));

            var res = _tokenHandle.IsTokenValid(TK.Token);
            //Uti.HandleCronJobLog(_logger, $"API.GetXMLVerify. {res.Data.Email} ({res.Data.MA_CSKCB}) {res.Code}", nameof(ApiController.CheckFileAPI));
            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();
            //Uti.HandleCronJobLog(_logger, $"API.GetXMLVerify. qua buoc xac thuc {res.Data.Email} ({res.Data.MA_CSKCB}) {res.Code} {user.Email}", nameof(ApiController.CheckFileAPI));


            var x01res = await _verifyService.GetXMLNeedVerify(hospital: user.Hospital);
            //var x01res = await _verifyService.GetXMLNeedVerify(res.Data.MA_CSKCB);
            //Uti.HandleCronJobLog(_logger, $"API.GetXMLVerify. qua buoc lay XML {res.Code}", nameof(ApiController.CheckFileAPI));

            if (x01res.Code != HttpStatusCode.OK || x01res.Data == null) return NotFound();

            var x01 = x01res.Data;
            return Ok(new THONGTUYEN
            {
                HO_TEN = x01.HO_TEN,
                MA_HS = x01.XML01ID.ToString("x"),
                MA_THE = x01.MA_THE_BHYT,
                NGAY_SINH = x01.NGAY_SINH?.ToString("dd/MM/yyyy") ?? "",
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SendXMLResult")]
        public async Task<IActionResult> SendXMLResult(VerifyData request)
        {
            //Uti.HandleCronJobLog(_logger, $"SendXMLResult. B1. kiem tra token  {request.Token} {request.MaHoSo} ", nameof(VerifyXML));
            var res = _tokenHandle.IsTokenValid(request.Token);
            //Uti.HandleCronJobLog(_logger, $"SendXMLResult. B2. kiem tra token  {res.Code} {res.Data.MA_CSKCB} ", nameof(VerifyXML));

            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();
            //Uti.HandleCronJobLog(_logger, $"SendXMLResult. B3. kiem tra user  {res.Data.MA_CSKCB}", nameof(VerifyXML));

            long id = 0;
            try
            {
                id = Convert.ToInt64(request.MaHoSo, 16);
            }
            catch { }

            if (id < 1) return BadRequest();

            try
            {
                await _verifyService.UpdateXMLVerifyResult(id, user.Hospital.MA_CSKCB, request.KetQua);
                return Ok(new ResponseModel<bool>(HttpStatusCode.OK, "", true));
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Khong the check the: {id}, MA_CSKCB: {user.Hospital.MA_CSKCB} {ex.StackTrace}", nameof(VerifyXML));
                throw;
            };
        }

        #endregion

        #region Linhist added 10/08/26

        [HttpPost]
        [AllowAnonymous]
        [Route("GetXMLWaiting")]
        public async Task<IActionResult> GetXMLWaiting([FromBody] AuthToken token)
        {
            var res = _tokenHandle.IsTokenValid(token?.Token);
            if (res.Code != HttpStatusCode.OK) return Unauthorized();

            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var isMaster = res.Data.RoleId == RoleConstant.MASTER;

            try
            {
                var files = isMaster ? await _validateRepository.GetListAsync() : await _validateRepository.GetListAsync(x => x.MA_CSKCB == res.Data.MA_CSKCB);
                if (files == null || !files.Any()) return NotFound("Không còn hồ sơ đang đợi giám định");

                var ids = files.Select(x => x.XML01ID).ToArray();
                var xml01s = isMaster ? await _hosoRepository.GetListAsync(x => ids.Contains(x.XML01ID)) : await _hosoRepository.GetAllByListID(res.Data.MA_CSKCB, ids);
                if (xml01s == null || !xml01s.Any()) return NotFound("Không còn hồ sơ đang đợi giám định");

                var waits = xml01s.Select(x => new FileWaiting { MA_LK = x.MA_LK, FileName = x.FileName, CreateDate = x.CreateDate, MA_CSKCB = x.MA_CSKCB }).ToList();
                if (waits == null || !waits.Any()) return NotFound();
                return Ok(waits);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"List all XML waiting to validate {res.Data.Email} ({res.Data.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.GetXMLWaiting));
                return Problem("Error");
            }
        }
        #endregion

        //Linhist added 131026
        [HttpPost]
        [AllowAnonymous]
        [Route("GetDMData")]
        public async Task<IActionResult> GetDMData(SyncDMInfo request)
        {
            var res = new ResponseModel<IEnumerable>(HttpStatusCode.NotFound, string.Empty, null);
            var tok = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(tok);
            if (user == null) return Unauthorized();

            var md5 = Uti.GetMd5Hash($"{user.Email}###{request.Type}###{user.Hospital.MA_CSKCB}###{DateTime.Now:yyyyMMdd}");
            if (string.IsNullOrEmpty(request.Key) ||  request.Key != md5) return BadRequest("Key không hợp lệ");

            try
            {
                switch (request.Type?.ToLower())
                {
                    case "thuoc":
                        res.Data = await _dmThuocRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "dichvu":
                        res.Data = await _dmDichVuRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "vattu":
                        res.Data = await _dmVTYTRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "thietbi":
                        res.Data = await _dmThietBiRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "khoa":
                        res.Data = await _dmKhoaBVRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "giuong":
                        res.Data = await _dmDvGiuongRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "magiuong":
                        res.Data = await _dmMaGiuongRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "bacsy":
                        res.Data = await _dmBacSiRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    case "cskcb":
                        res.Data = await _dmCSKCBRepository.GetListAsync(e => e.MA_CSKCB == user.Hospital.MA_CSKCB);
                        break;
                    default:
                        return BadRequest("Không tìm thấy dữ liệu");
                }

                res.Code = HttpStatusCode.OK;
                return Ok(res);
            }
            catch// (Exception ex)
            {
                //Uti.HandleCronJobLog(_logger, $"Khong the check the: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB} {ex}", nameof(ApiController.APIVerifyUpdated));
                return NotFound("Đã xảy ra lỗi khi lấy dữ liệu");
            }
        }
        private T Bound<T>(object obj)
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<T>(((JsonElement)obj).GetRawText(), opts);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SyncDMData")]
        public async Task<IActionResult> SyncDMData(SyncDMData request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.NotFound, string.Empty, false);
            var tok = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(tok);
            if (user == null) return Unauthorized();

            var md5 = Uti.GetMd5Hash($"{user.Email}###{request.Type}###{user.Hospital.MA_CSKCB}###{DateTime.Now:yyyyMMdd}");
            if (string.IsNullOrEmpty(request.Key) || request.Key != md5) return BadRequest("Key không hợp lệ");

            try
            {
                switch (request.Type?.ToLower())
                {
                    case "thuoc":
                        var thuocs = request.Data.Select(e => Bound<DMThuoc>(e)).ToList();
                        if (thuocs.Any())
                        {
                            await _dmThuocRepository.InsertListAsync(thuocs.ToList());
                            await _dmThuocRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmThuocRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.THID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmThuocRepository.SaveChangesAsync();
                        }
                        break;
                    case "dichvu":
                        var dichVus = request.Data.Select(e => Bound<DMDichVu>(e)).ToList();
                        if (dichVus.Any())
                        {
                            await _dmDichVuRepository.InsertListAsync(dichVus.ToList());
                            await _dmDichVuRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmDichVuRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.DVID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmDichVuRepository.SaveChangesAsync();
                        }
                        break;
                    case "vattu":
                        var vatTus = request.Data.Select(e => Bound<DMVTYT>(e)).ToList();
                        if (vatTus.Any())
                        {
                            await _dmVTYTRepository.InsertListAsync(vatTus.ToList());
                            await _dmVTYTRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmVTYTRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.VTID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmVTYTRepository.SaveChangesAsync();
                        }
                        break;
                    case "thietbi":
                        var thietBis = request.Data.Select(e => Bound<DMThietBi>(e)).ToList();
                        if (thietBis.Any())
                        {
                            await _dmThietBiRepository.InsertListAsync(thietBis.ToList());
                            await _dmThietBiRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmThietBiRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.TBID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmThietBiRepository.SaveChangesAsync();
                        }
                        break;
                    case "khoa":
                        var khoas = request.Data.Select(e => Bound<DMKhoaBV>(e)).ToList();
                        if (khoas.Any())
                        {
                            await _dmKhoaBVRepository.InsertListAsync(khoas.ToList());
                            await _dmKhoaBVRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmKhoaBVRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.KBVID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmKhoaBVRepository.SaveChangesAsync();
                        }
                        break;
                    case "giuong":
                        var giuongs = request.Data.Select(e => Bound<DMDVGiuong>(e)).ToList();
                        if (giuongs.Any())
                        {
                            await _dmDvGiuongRepository.InsertListAsync(giuongs.ToList());
                            await _dmDvGiuongRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmDvGiuongRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.GID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmDvGiuongRepository.SaveChangesAsync();
                        }
                        break;
                    case "magiuong":
                        var maGigs = request.Data.Select(e => Bound<DMMaGiuong>(e)).ToList();
                        if (maGigs.Any())
                        {
                            await _dmMaGiuongRepository.InsertListAsync(maGigs.ToList());
                            await _dmMaGiuongRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmMaGiuongRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.GID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmMaGiuongRepository.SaveChangesAsync();
                        }
                        break;
                    case "bacsy":
                        var bacSis = request.Data.Select(e => Bound<DMBacSi>(e)).ToList();
                        if (bacSis.Any())
                        {
                            await _dmBacSiRepository.InsertListAsync(bacSis.ToList());
                            await _dmBacSiRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmBacSiRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.BSID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmBacSiRepository.SaveChangesAsync();
                        }
                        break;
                    case "cskcb":
                        var cskcbs = request.Data.Select(e => Bound<DMCSKCB>(e)).ToList();
                        if (cskcbs.Any())
                        {
                            await _dmCSKCBRepository.InsertListAsync(cskcbs.ToList());
                            await _dmCSKCBRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmCSKCBRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.ID) && e.MA_CSKCB == user.Hospital.MA_CSKCB);
                            await _dmCSKCBRepository.SaveChangesAsync();
                        }
                        break;
                    default:
                        return BadRequest("Không tìm thấy dữ liệu");
                }

                res.Data = true;
                res.Code = HttpStatusCode.OK;
                return Ok(res);
            }
            catch// (Exception ex)
            {
                //Uti.HandleCronJobLog(_logger, $"Khong the check the: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital.MA_CSKCB} {ex}", nameof(ApiController.APIVerifyUpdated));
                return NotFound("Đã xảy ra lỗi khi lấy dữ liệu");
            }
        }
    }

    public class FileUploading
    {
        public string Token { get; set; }
        public List<FileHoSo> ArrayFiles { get; set; }

    }

    public class FileHoSo
    {
        public string FileName { get; set; }
        public string FileBytes { get; set; }

    }

    public class AuthToken
    {
        public string Token { get; set; }
    }

    public class FileUploadingAPI
    {
        public Root Token { get; set; }
    }

    public class FileCheckError
    {
        public string Token { get; set; }
        public List<string> FileName { get; set; }

    }

    public class VerifyData
    {
        public string Token { get; set; }
        public string MaHoSo { get; set; }
        public string KetQua { get; set; }
    }

    public class UpdatedVerifyData
    {
        public string Token { get; set; }
        public XML01OUT Data { get; set; }
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

    public class SyncDMInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int Page { get; set; } = 0;
    }

    public class SyncDMData : SyncDMInfo
    {
        public IEnumerable<object> Data { get; set; } = null;
        public long[] Skips { get; set; } = Array.Empty<long>();
    }

    #region Linhist added 11/05/23
    public class FileDownload
    {
        public string Token { get; set; }
        public string FromDate { get; set; }
        public string Hospital { get; set; }
        public string FileName { get; set; }
    }
    #endregion

    #region Linhist added 14/08/24
    public class THONGTUYEN
    {
        public string MA_HS { get; set; }
        public string MA_THE { get; set; }
        public string HO_TEN { get; set; }
        public string NGAY_SINH { get; set; }
    }
    #endregion

    #region UpdatedVerify
    public partial class XML01OUT
    {
        public bool VerifyStatus { get; set; }
        public bool ValidateStatus { get; set; }
        public string FileNames { get; set; }
        public string MA_LK { get; set; }
        public string STT { get; set; }
        public string MA_BN { get; set; }
        public string HO_TEN { get; set; }
        public string SO_CCCD { get; set; }
        public string MA_THE_BHYT { get; set; }
        public string NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string MA_DKBD { get; set; }
        public string GT_THE_TU { get; set; }
        public string GT_THE_DEN { get; set; }
        public string NGAY_RA { get; set; }
        public string NGAY_VAO { get; set; }
        public string MA_KHUVUC { get; set; }

        /// <summary>
        ///File này để lưu đường dẫn
        /// </summary>
        public string FilesStatusCode { get; set; }

        public VerifyCode ErrosVerify { get; set; }

        public string ErrorDetail { get; set; }
        public bool isUpdatedAPI { get; set; }
        public string MaLoi { get; set; }
        public string NDLoi { get; set; }


    }

    public partial class VerifyCode
    {
        public string ErrorCode { get; set; }
        public string ErrorContents { get; set; }

    }
    public partial class ClientVerifiedData
    {
        public string Token { get; set; }
        public string MA_LK { get; set; }
        public string STT { get; set; }
        public string MA_BN { get; set; }
        public string HO_TEN { get; set; }
        public string SO_CCCD { get; set; }
        public string MA_THE_BHYT { get; set; }
        public string NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string MA_DKBD { get; set; }
        public string GT_THE_TU { get; set; }
        public string GT_THE_DEN { get; set; }
        public string NGAY_RA { get; set; }
        public string NGAY_VAO { get; set; }
        public string MA_KHUVUC { get; set; }
        public string MA_CSKCB { get; set; }
        public string MaLoi { get; set; }
        public string NDLoi { get; set; }
        public string FileNames { get; set; }
        public bool isUpdatedAPI { get; set; }
    }
    #endregion

    #region Linhist added 10/08/26
    public class FileWaiting
    {
        public string MA_LK { get; set; }

        public string MA_CSKCB { get; set; }

        public string FileName { get; set; }

        public DateTime? CreateDate { get; set; }
    }
    #endregion
}
