using HIA.Core.Models.User;
using HIA.Services.Auth;
using HIA.Services.UserManagement;
using HIA.Services.Verify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.FileUploads;
using HIA.Core.Models.Files;
using HIA.Core.Models.Token;
using HIA.Data.Auths;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Reports;
using HIA.Data.Rules;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Services.Files;
using HIA.Services.HosoManagement130;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;

namespace HIA.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private static readonly JsonSerializerOptions BoundOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly IAuthService _authService;
        private readonly TokenHandler _tokenHandle;
        private readonly IXml01Repository _hosoRepository;
        private readonly IFilesService130 _FilesService130;
        private readonly IHoSoManagementService130 _HoSoManagementService130;
        private readonly ILogger<ApiController> _logger;
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

        public ApiController(
            IHoSoManagementService130 HoSoManagementService130,
            IAuthService authService,
            TokenHandler tokenHandle,
            IFilesService130 FilesService130,
            ILogger<ApiController> logger,
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
            IXml01Repository hosoRepository
        )
        {
            _authService = authService;
            _tokenHandle = tokenHandle;
            _FilesService130 = FilesService130;
            _logger = logger;
            _HoSoManagementService130 = HoSoManagementService130;
            _userService = userService;
            _verifyService = verifyService;
            _validateRepository = validateRepository;
            _userlogRepository = userlogRepository;
            _dmErrorMHTCRepository = dmErrorMHTCRepository;
            _hosoRepository = hosoRepository;
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

        [HttpGet]
        [Route("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok("MDInsight API Running...");
        }

        private static string Req(string? value)
            => value ?? throw new InvalidOperationException("Thieu du lieu bat buoc");

        private static string convertDate(string date)
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

        private VerifyCode APICHECK(XML01OUT rows, string MA_CSKCB)
        {
            VerifyCode VRCode = new VerifyCode();
            VRCode.ErrorCode = "228";
            VRCode.ErrorContents = "Lỗi server BHYT";

            if (rows == null) return VRCode;

            var lskcb = JsonSerializer.Deserialize<NhanLichSuKCB2024[]>(rows.ErrorDetail ?? string.Empty)
                ?? throw new InvalidOperationException("Du lieu lich su KCB rong");
            foreach (var check in lskcb)
            {
                var dsLichSuKCB2018 = check.dsLichSuKCB2018;

                if (check == null) return VRCode;

                try
                {
                    //Check ngay sinh
                    if (!string.IsNullOrEmpty(check.ngaySinh))
                    {
                        if (convertDate(Req(rows.NGAY_SINH)) != check.ngaySinh)
                        {
                            VRCode.ErrorCode = "216";
                            VRCode.ErrorContents = "Thẻ sai ngày sinh";
                            return VRCode;
                        }
                    }

                    if (check.maKetQua == "000")
                    {
                        if (dsLichSuKCB2018 != null && dsLichSuKCB2018.Count != 0)
                        {
                            try
                            {
                                foreach (var i in dsLichSuKCB2018)
                                {
                                    DateTime _ngayraThe = DateTime.ParseExact(Req(i.ngayRa), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                                    DateTime _ngayraBN = DateTime.ParseExact(Req(rows.NGAY_RA), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                                    DateTime _ngayvaoBN = DateTime.ParseExact(Req(rows.NGAY_VAO), "yyyyMMddHHmm", CultureInfo.InvariantCulture);
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
                            catch
                            {
                            }
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
                                string[] gtthetu = Req(rows.GT_THE_TU).Split(";");
                                string[] gttheden = Req(rows.GT_THE_DEN).Split(";");
                                if (gtthetu.Length == 1)
                                {
                                    string gttu = convertDate(gtthetu[0]);
                                    if (gttu != check.gtTheTu)
                                    {
                                        VRCode.ErrorCode = "1183";
                                        VRCode.ErrorContents = "Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về";
                                        return VRCode;
                                    }

                                    if (DateTime.ParseExact(convertDate(Req(rows.NGAY_VAO)), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date < DateTime.ParseExact(check.gtTheTu, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
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
                                    if (DateTime.ParseExact(convertDate(Req(rows.NGAY_VAO)), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date < DateTime.ParseExact(Req(check.gtTheTuMoi), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
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
                                    if (DateTime.ParseExact(convertDate(Req(rows.NGAY_RA)), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date > DateTime.ParseExact(check.gtTheDen, "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
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
                                    if (DateTime.ParseExact(convertDate(Req(rows.NGAY_RA)), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date > DateTime.ParseExact(Req(check.gtTheDenMoi), "dd/MM/yyyy", CultureInfo.InvariantCulture).Date)
                                    {
                                        VRCode.ErrorCode = "212";
                                        VRCode.ErrorContents = "Hết hạn thẻ khi chưa ra viện";
                                        return VRCode;
                                    }
                                }
                            }
                        }
                        catch
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
                catch
                {
                }
            }
            return VRCode;
        }

        private async Task<User?> GetAuthen(ResponseModel<TokenRequestModel> res)
        {
            var tok = res.Data;
            if (res.Code != HttpStatusCode.OK || tok == null) return null;

            var chk = await _userService.GetActiveUserAndHospital(tok.Id, tok.MA_CSKCB, tok.RoleId);
            if (chk.Code != HttpStatusCode.OK)
            {
                return null;
            }

            return chk.Data;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel request)
        {
            var res = new ResponseModel<TokenResponseModel>();
            if (!ModelState.IsValid)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = string.Join(" | ", ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage));
                return NotFound(res);
            }

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

            var tok = res.Data ?? throw new InvalidOperationException("Token khong co du lieu");
            var result = new TokenResultModel()
            {
                Id = tok.Id,
                Email = tok.Email,
                FullName = tok.FullName,
                HospitalName = tok.HospitalName,
                MA_CSKCB = tok.MA_CSKCB,
                ExpireAt = tok.ExpireAt
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

            try
            {
                var user = await GetAuthen(res);
                if (user == null) return Unauthorized();
                var tok = res.Data ?? throw new InvalidOperationException("Token khong co du lieu");
                if (request.ArrayFiles.Count > 10) return BadRequest("Số lượng file gửi lên vượt quá 10 file");
                List<IFormFile> FormFiles = new List<IFormFile>();

                foreach (var file in request.ArrayFiles)
                {
                    byte[] byteArray = System.Convert.FromBase64String(Req(file.FileBytes));

                    var ms = new MemoryStream(byteArray);

                    var fileName = Req(file.FileName);
                    IFormFile fromFile = new FormFile(ms, 0, ms.Length,
                        Path.GetFileNameWithoutExtension(fileName) ?? string.Empty,
                        fileName
                    );
                    FormFiles.Add(fromFile);
                }

                FileUpload fileUp = new FileUpload() { FormFiles = FormFiles };

                var filenames = string.Concat(request.ArrayFiles.Select(f => f.FileName + ";"));

                DateTime t = DateTime.Now;
                //haimaster 25.7.2022
                Uti.HandleCronJobLog(_logger, $"A1. Đẩy hồ sơ giám định {tok.Email} ({tok.MA_CSKCB}). FileName: {filenames}", nameof(ApiController.UploadFile));
                try
                {
                    var uploadEdfile = await _FilesService130.FileUploadAPI(fileUp, tok.HospitalId ?? string.Empty, tok.HospitalName ?? string.Empty, tok.Id ?? string.Empty, tok.MA_CSKCB ?? string.Empty, tok.Email, true, true);
                    //13.2.2025 User Logs
                    userlog.Add(new UserLogs()
                    {
                        UserID = tok.Id,
                        Email = tok.Email,
                        Action = "Đẩy hồ sơ giám định qua API",
                        MA_CSKCB = tok.MA_CSKCB,
                        LogTime = DateTime.Now,
                    });
                    await _userlogRepository.InsertListAsync(userlog);
                    await _userlogRepository.SaveChangesAsync();

                    Uti.HandleCronJobLog(_logger, $"A2. Đã kiểm tra xong, user ID:{tok.Id} ({tok.MA_CSKCB}). FileName: {filenames} {(DateTime.Now - t).TotalMilliseconds}", nameof(ApiController.UploadFile));
                    return Ok(uploadEdfile);
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"A0. UploadFiles API {tok.Email} ({tok.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
                }
            }
            catch (Exception ex1)
            {
                Uti.HandleCronJobLog(_logger, $"A0. UploadFiles API  {res.Data?.Email} ({res.Data?.MA_CSKCB}). Error: {ex1.Message}", nameof(ApiController.UploadFile));
            }

            return Ok(null);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("UploadFile130")]
        public async Task<IActionResult> UploadFile130([FromBody] FileUploading request)
        {
            var res = _tokenHandle.IsTokenValid(request.Token);

            var user = await GetAuthen(res);
            if (user == null)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Check API User Faile {res.Code}", nameof(ApiController.UploadFile));
                return Unauthorized();
            }
            var tok = res.Data ?? throw new InvalidOperationException("Token khong co du lieu");
            if (request.ArrayFiles.Count > 10)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Total Files >10 {tok.Email} ({tok.MA_CSKCB})", nameof(ApiController.UploadFile));
                return BadRequest("Số lượng file gửi lên vượt quá 10 file");
            }

            string filenames = "";
            FileUpload fileUp;
            try
            {
                List<IFormFile> FormFiles = new List<IFormFile>();
                foreach (var file in request.ArrayFiles)
                {
                    byte[] byteArray = System.Convert.FromBase64String(Req(file.FileBytes));

                    var ms = new MemoryStream(byteArray);

                    var fileName = Req(file.FileName);
                    IFormFile fromFile = new FormFile(ms, 0, ms.Length,
                        Path.GetFileNameWithoutExtension(fileName) ?? string.Empty,
                        fileName
                    );
                    FormFiles.Add(fromFile);
                }
                fileUp = new FileUpload() { FormFiles = FormFiles };

                filenames = string.Join(";", request.ArrayFiles.Select(f => f.FileName));
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Check API Convert File Faile {tok.Email} ({tok.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
                return BadRequest("File gửi lên không đúng định dạng");
            }

            //haimaster 25.7.2022
            Uti.HandleCronJobLog(_logger, $"S1.UploadFile130 Check HS {tok.Email} ({tok.MA_CSKCB}). FileName: {filenames}", nameof(ApiController.UploadFile));
            try
            {
                var uploadEdfile = await _FilesService130.FileUploadAPI(fileUp, tok.HospitalId ?? string.Empty, tok.HospitalName ?? string.Empty, tok.Id ?? string.Empty, tok.MA_CSKCB ?? string.Empty, tok.Email, true, false);

                return Ok(uploadEdfile);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"S1. UploadFiles130  Check API {tok.Email} ({tok.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.UploadFile));
            }

            return Ok(null);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CheckError")]
        public async Task<IActionResult> CheckError(FileCheckError request)
        {
            DateTime t = DateTime.Now;
            var res = _tokenHandle.IsTokenValid(request.Token);

            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var tok = res.Data ?? throw new InvalidOperationException("Token khong co du lieu");
            var maCSKCB = tok.MA_CSKCB ?? string.Empty;

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

                Uti.HandleCronJobLog(_logger, $"CheckError.F1 Time: {(DateTime.Now - t).TotalMilliseconds} {tok.Email} ({tok.MA_CSKCB}). FileName: {request.FileName.FirstOrDefault()}", nameof(ApiController.CheckError));

                if (dmes == null || !dmes.Any()) return Ok(errs);
                if (errs == null || errs.Data == null) return BadRequest("Không tìm thấy hồ sơ giám định");

                var cods = dmes.Select(x => x.EMErrorCode).ToList();
                errs.Data = errs.Data.Where(e => !cods.Contains(e.XeErrorCode)).ToList();
                return Ok(errs);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"CheckError.F0 Time: {(DateTime.Now - t).TotalMilliseconds} {tok.Email} ({tok.MA_CSKCB}). FileName: {request.FileName.FirstOrDefault()} + Lỗi:{ex}", nameof(ApiController.CheckError));
            }

            return BadRequest("Không tìm thấy hồ sơ giám định");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("APIVerifyUpdated")]
        public async Task<IActionResult> APIVerifyUpdated(UpdatedVerifyData request)
        {
            var res = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var tok = res.Data ?? throw new InvalidOperationException("Token khong co du lieu");
            var data = request.Data;
            if (data == null || string.IsNullOrEmpty(data.ErrorDetail) || data.ErrorDetail.Length < 50)
                return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "", false));

            try
            {
                var hospital = user.Hospital ?? throw new InvalidOperationException("User khong co co so KCB");
                var VRcode = APICHECK(data, tok.MA_CSKCB ?? throw new InvalidOperationException("Token khong co MA_CSKCB"));
                var updated = await _verifyService.UpdatedVerify(new CheckThe4210
                {
                    MA_BN = data.MA_BN,
                    MA_LK = data.MA_LK,
                    MA_CSKCB = hospital.MA_CSKCB,
                    MA_DKBD = data.MA_DKBD,
                    MA_KHUVUC = data.MA_KHUVUC,
                    MA_THE_BHYT = data.MA_THE_BHYT,
                    HO_TEN = data.HO_TEN,
                    GIOI_TINH = data.GIOI_TINH,
                    NGAY_SINH = EvalHelper.ToDateSql(data.NGAY_SINH ?? string.Empty),
                    GT_THE_DEN = data.GT_THE_DEN,
                    GT_THE_TU = data.GT_THE_TU,
                    TG_CHECK_THE = DateTime.Now,
                    ERROR_CODE = VRcode.ErrorCode,
                    ERROR_CONTENTS = data.ErrorDetail,
                });

                if (updated == 1)
                {
                    if (!string.IsNullOrEmpty(data.MA_LK) && !string.IsNullOrEmpty(hospital.MA_CSKCB))
                    {
                        try
                        {
                            var xml01 = await _hosoRepository.GetAllByMaLK(hospital.MA_CSKCB, data.MA_LK);
                            if (xml01 == null)
                            {
                                Uti.HandleCronJobLog(_logger, $"Không lấy được xmlids: Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
                            }
                            if (xml01 != null && xml01.Any())
                            {
                                var xmlIds = xml01.Select(x => x.XML01ID).ToArray();
                                try
                                {
                                    await _HoSoManagementService130.RemoveHosoFromValidate(xmlIds);
                                    await _HoSoManagementService130.AppendHosoToValidate(hospital.MA_CSKCB, xmlIds);
                                }
                                catch (Exception ex)
                                {
                                    Uti.HandleCronJobLog(_logger, $"Không lấy được xmlids:{xml01.Count()}, Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {hospital.MA_CSKCB} + Lỗi:{ex}", nameof(ApiController.APIVerifyUpdated));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Không chuyển thông tin qua giám định được MA_LK:{data.MA_LK}, Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {hospital.MA_CSKCB} + Lỗi:{ex} + {ex}", nameof(ApiController.APIVerifyUpdated));
                        }
                    }
                    else
                    {
                        Uti.HandleCronJobLog(_logger, $"Không lấy được MA_LK:{data.MA_LK}, Ma The: {data.MA_THE_BHYT}, MA_CSKCB: {hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
                    }

                    Uti.HandleCronJobLog(_logger, $"CheckThe4210 Updated Thanh cong:MA BN: {data.MA_BN}, Ma THE:{data.MA_THE_BHYT}, MA_CSKCB: {hospital.MA_CSKCB}", nameof(ApiController.APIVerifyUpdated));
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
                Uti.HandleCronJobLog(_logger, $"Khong the check the: {data.MA_THE_BHYT}, MA_CSKCB: {user.Hospital?.MA_CSKCB} {ex}", nameof(ApiController.APIVerifyUpdated));
                return Ok(new ResponseModel<bool>(HttpStatusCode.InternalServerError, "", false));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ClientVerify")]
        public async Task<IActionResult> ClientVerify(ClientVerifiedData request)
        {
            var resp = new ResponseModel<bool>();

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
                    NGAY_SINH = EvalHelper.ToDateSql(request.NGAY_SINH ?? string.Empty),
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

                var xml01 = await _hosoRepository.GetAllByMaLK(request.MA_CSKCB ?? string.Empty, request.MA_LK ?? string.Empty);
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

        [HttpPost]
        [AllowAnonymous]
        [Route("GetXMLVerify")]
        public async Task<IActionResult> GetXMLVerify(AuthToken TK)
        {
            var res = _tokenHandle.IsTokenValid(TK.Token ?? string.Empty);
            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var x01res = await _verifyService.GetXMLNeedVerify(hospital: user.Hospital);
            var x01 = x01res.Data;
            if (x01res.Code != HttpStatusCode.OK || x01 == null) return NotFound();

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
            var res = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            long id = 0;
            try
            {
                id = Convert.ToInt64(request.MaHoSo, 16);
            }
            catch { }

            if (id < 1) return BadRequest();

            var hospital = user.Hospital ?? throw new InvalidOperationException("User khong co co so KCB");
            try
            {
                await _verifyService.UpdateXMLVerifyResult(id, hospital.MA_CSKCB, request.KetQua);
                return Ok(new ResponseModel<bool>(HttpStatusCode.OK, "", true));
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Khong the check the: {id}, MA_CSKCB: {hospital.MA_CSKCB} {ex.StackTrace}", nameof(VerifyXML));
                throw;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetXMLWaiting")]
        public async Task<IActionResult> GetXMLWaiting([FromBody] AuthToken token)
        {
            var res = _tokenHandle.IsTokenValid(token?.Token ?? string.Empty);
            if (res.Code != HttpStatusCode.OK) return Unauthorized();

            var user = await GetAuthen(res);
            if (user == null) return Unauthorized();

            var tok = res.Data ?? throw new InvalidOperationException("Token khong co du lieu");
            var isMaster = tok.RoleId == RoleConstant.MASTER;
            var maCSKCB = tok.MA_CSKCB;

            try
            {
                var files = isMaster ? await _validateRepository.GetListAsync() : await _validateRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                if (files == null || !files.Any()) return NotFound("Không còn hồ sơ đang đợi giám định");

                var ids = files.Select(x => x.XML01ID).ToArray();
                var xml01s = isMaster ? await _hosoRepository.GetListAsync(x => ids.Contains(x.XML01ID)) : await _hosoRepository.GetAllByListID(maCSKCB ?? string.Empty, ids);
                if (xml01s == null || !xml01s.Any()) return NotFound("Không còn hồ sơ đang đợi giám định");

                var waits = xml01s.Select(x => new FileWaiting { MA_LK = x.MA_LK, FileName = x.FileName, CreateDate = x.CreateDate, MA_CSKCB = x.MA_CSKCB }).ToList();
                if (!waits.Any()) return NotFound();
                return Ok(waits);
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"List all XML waiting to validate {tok.Email} ({tok.MA_CSKCB}) - Error: {ex.Message} --- {ex.StackTrace}", nameof(ApiController.GetXMLWaiting));
                return Problem("Error");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetDMData")]
        public async Task<IActionResult> GetDMData(SyncDMInfo request)
        {
            var res = new ResponseModel<IEnumerable>(HttpStatusCode.NotFound, string.Empty, null);
            var tok = _tokenHandle.IsTokenValid(request.Token);
            var user = await GetAuthen(tok);
            if (user == null) return Unauthorized();

            var hospital = user.Hospital ?? throw new InvalidOperationException("User khong co co so KCB");
            var md5 = Uti.GetMd5Hash($"{user.Email}###{request.Type}###{hospital.MA_CSKCB}###{DateTime.Now:yyyyMMdd}");
            if (string.IsNullOrEmpty(request.Key) || request.Key != md5) return BadRequest("Key không hợp lệ");

            try
            {
                switch (request.Type?.ToLower())
                {
                    case "thuoc":
                        res.Data = await _dmThuocRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "dichvu":
                        res.Data = await _dmDichVuRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "vattu":
                        res.Data = await _dmVTYTRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "thietbi":
                        res.Data = await _dmThietBiRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "khoa":
                        res.Data = await _dmKhoaBVRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "giuong":
                        res.Data = await _dmDvGiuongRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "magiuong":
                        res.Data = await _dmMaGiuongRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "bacsy":
                        res.Data = await _dmBacSiRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    case "cskcb":
                        res.Data = await _dmCSKCBRepository.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                        break;
                    default:
                        return BadRequest("Không tìm thấy dữ liệu");
                }

                res.Code = HttpStatusCode.OK;
                return Ok(res);
            }
            catch
            {
                return NotFound("Đã xảy ra lỗi khi lấy dữ liệu");
            }
        }

        private static T Bound<T>(object obj) where T : class
        {
            return JsonSerializer.Deserialize<T>(((JsonElement)obj).GetRawText(), BoundOptions)
                ?? throw new JsonException("Du lieu khong hop le");
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

            var hospital = user.Hospital ?? throw new InvalidOperationException("User khong co co so KCB");
            var md5 = Uti.GetMd5Hash($"{user.Email}###{request.Type}###{hospital.MA_CSKCB}###{DateTime.Now:yyyyMMdd}");
            if (string.IsNullOrEmpty(request.Key) || request.Key != md5) return BadRequest("Key không hợp lệ");

            try
            {
                var items = request.Data ?? throw new InvalidOperationException("Thieu du lieu Data");
                switch (request.Type?.ToLower())
                {
                    case "thuoc":
                        var thuocs = items.Select(e => Bound<DMThuoc>(e)).ToList();
                        if (thuocs.Any())
                        {
                            await _dmThuocRepository.InsertListAsync(thuocs);
                            await _dmThuocRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmThuocRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.THID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmThuocRepository.SaveChangesAsync();
                        }
                        break;
                    case "dichvu":
                        var dichVus = items.Select(e => Bound<DMDichVu>(e)).ToList();
                        if (dichVus.Any())
                        {
                            await _dmDichVuRepository.InsertListAsync(dichVus);
                            await _dmDichVuRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmDichVuRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.DVID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmDichVuRepository.SaveChangesAsync();
                        }
                        break;
                    case "vattu":
                        var vatTus = items.Select(e => Bound<DMVTYT>(e)).ToList();
                        if (vatTus.Any())
                        {
                            await _dmVTYTRepository.InsertListAsync(vatTus);
                            await _dmVTYTRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmVTYTRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.VTID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmVTYTRepository.SaveChangesAsync();
                        }
                        break;
                    case "thietbi":
                        var thietBis = items.Select(e => Bound<DMThietBi>(e)).ToList();
                        if (thietBis.Any())
                        {
                            await _dmThietBiRepository.InsertListAsync(thietBis);
                            await _dmThietBiRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmThietBiRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.TBID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmThietBiRepository.SaveChangesAsync();
                        }
                        break;
                    case "khoa":
                        var khoas = items.Select(e => Bound<DMKhoaBV>(e)).ToList();
                        if (khoas.Any())
                        {
                            await _dmKhoaBVRepository.InsertListAsync(khoas);
                            await _dmKhoaBVRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmKhoaBVRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.KBVID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmKhoaBVRepository.SaveChangesAsync();
                        }
                        break;
                    case "giuong":
                        var giuongs = items.Select(e => Bound<DMDVGiuong>(e)).ToList();
                        if (giuongs.Any())
                        {
                            await _dmDvGiuongRepository.InsertListAsync(giuongs);
                            await _dmDvGiuongRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmDvGiuongRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.GID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmDvGiuongRepository.SaveChangesAsync();
                        }
                        break;
                    case "magiuong":
                        var maGigs = items.Select(e => Bound<DMMaGiuong>(e)).ToList();
                        if (maGigs.Any())
                        {
                            await _dmMaGiuongRepository.InsertListAsync(maGigs);
                            await _dmMaGiuongRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmMaGiuongRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.GID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmMaGiuongRepository.SaveChangesAsync();
                        }
                        break;
                    case "bacsy":
                        var bacSis = items.Select(e => Bound<DMBacSi>(e)).ToList();
                        if (bacSis.Any())
                        {
                            await _dmBacSiRepository.InsertListAsync(bacSis);
                            await _dmBacSiRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmBacSiRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.BSID) && e.MA_CSKCB == hospital.MA_CSKCB);
                            await _dmBacSiRepository.SaveChangesAsync();
                        }
                        break;
                    case "cskcb":
                        var cskcbs = items.Select(e => Bound<DMCSKCB>(e)).ToList();
                        if (cskcbs.Any())
                        {
                            await _dmCSKCBRepository.InsertListAsync(cskcbs);
                            await _dmCSKCBRepository.SaveChangesAsync();
                        }

                        if (request.Skips?.Any() == true)
                        {
                            await _dmCSKCBRepository.DeleteListByFilterAsync(e => request.Skips.Contains(e.ID) && e.MA_CSKCB == hospital.MA_CSKCB);
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
            catch
            {
                return NotFound("Đã xảy ra lỗi khi lấy dữ liệu");
            }
        }
    }

    public class FileUploading
    {
        public string Token { get; set; } = string.Empty;
        public List<FileHoSo> ArrayFiles { get; set; } = new();
    }

    public class FileHoSo
    {
        public string? FileName { get; set; }
        public string? FileBytes { get; set; }
    }

    public class AuthToken
    {
        public string? Token { get; set; }
    }

    public class FileCheckError
    {
        public string Token { get; set; } = string.Empty;
        public List<string> FileName { get; set; } = new();
    }

    public class VerifyData
    {
        public string Token { get; set; } = string.Empty;
        public string? MaHoSo { get; set; }
        public string? KetQua { get; set; }
    }

    public class UpdatedVerifyData
    {
        public string Token { get; set; } = string.Empty;
        public XML01OUT? Data { get; set; }
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
        public IEnumerable<object>? Data { get; set; }
        public long[] Skips { get; set; } = Array.Empty<long>();
    }

    public class THONGTUYEN
    {
        public string? MA_HS { get; set; }
        public string? MA_THE { get; set; }
        public string? HO_TEN { get; set; }
        public string? NGAY_SINH { get; set; }
    }

    public partial class XML01OUT
    {
        public bool VerifyStatus { get; set; }
        public bool ValidateStatus { get; set; }
        public string? FileNames { get; set; }
        public string? MA_LK { get; set; }
        public string? STT { get; set; }
        public string? MA_BN { get; set; }
        public string? HO_TEN { get; set; }
        public string? SO_CCCD { get; set; }
        public string? MA_THE_BHYT { get; set; }
        public string? NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string? MA_DKBD { get; set; }
        public string? GT_THE_TU { get; set; }
        public string? GT_THE_DEN { get; set; }
        public string? NGAY_RA { get; set; }
        public string? NGAY_VAO { get; set; }
        public string? MA_KHUVUC { get; set; }

        /// <summary>
        /// File này để lưu đường dẫn
        /// </summary>
        public string? FilesStatusCode { get; set; }

        public VerifyCode? ErrosVerify { get; set; }

        public string? ErrorDetail { get; set; }
        public bool isUpdatedAPI { get; set; }
        public string? MaLoi { get; set; }
        public string? NDLoi { get; set; }
    }

    public partial class VerifyCode
    {
        public string? ErrorCode { get; set; }
        public string? ErrorContents { get; set; }
    }

    public partial class ClientVerifiedData
    {
        public string Token { get; set; } = string.Empty;
        public string? MA_LK { get; set; }
        public string? STT { get; set; }
        public string? MA_BN { get; set; }
        public string? HO_TEN { get; set; }
        public string? SO_CCCD { get; set; }
        public string? MA_THE_BHYT { get; set; }
        public string? NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string? MA_DKBD { get; set; }
        public string? GT_THE_TU { get; set; }
        public string? GT_THE_DEN { get; set; }
        public string? NGAY_RA { get; set; }
        public string? NGAY_VAO { get; set; }
        public string? MA_KHUVUC { get; set; }
        public string? MA_CSKCB { get; set; }
        public string? MaLoi { get; set; }
        public string? NDLoi { get; set; }
        public string? FileNames { get; set; }
        public bool isUpdatedAPI { get; set; }
    }

    public class FileWaiting
    {
        public string? MA_LK { get; set; }
        public string? MA_CSKCB { get; set; }
        public string? FileName { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
