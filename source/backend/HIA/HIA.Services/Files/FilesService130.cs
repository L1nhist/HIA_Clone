using System.Data;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Claims;
using System.Text;

using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files;
using HIA.Core.Models.FileUploads130;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Data.Base;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Core.Models.Files130;
using HIA.Core.Models.FileUploads;
using HIA.Services.HosoManagement130;
using HIA.Services.SysHistoryLogManagement;
using System.Xml.Serialization;
using System.Xml;
using System.Linq.Expressions;
using System.Reflection;
using HIA.Services.Report;
using Microsoft.EntityFrameworkCore.Query;
using HIA.Services.Categories;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Rules;
using HIA.Data.Reports;
using HIA.Data.Auths;

namespace HIA.Services.Files;

public class FilesService130 : IFilesService130
{
    #region Constructions
    private readonly IMapper _mapper;
    private readonly ILogger<FilesService130> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISysHistoryLogService _sysHistoryLogService;
    private readonly IHttpContextAccessor _httpContext;

    private readonly IBaseRepository<Hospital> _hospitalRepository;
    private readonly IDMErrorService130 _dmErrorRepository;
    private readonly IBaseRepository<DMBacSi> _dmBacSiRepository;
    private readonly IBaseRepository<DMKhoaBV> _dmKhoaRepository;
    private readonly IBaseRepository<ChiDinhDongThoi> _dmDongThoiRepository;

    private readonly IXml01Repository _hosoRepository;
    private readonly IHoSoManagementService130 _hosoService;

    private readonly IBaseRepository<XMLState> _stateRepository;
    private readonly IBaseRepository<XML02> _xml02Repository;
    private readonly IBaseRepository<XML03> _xml03Repository;
    private readonly IBaseRepository<XML04> _xml04Repository;
    private readonly IBaseRepository<XML05> _xml05Repository;
    private readonly IBaseRepository<XML06> _xml06Repository;
    private readonly IBaseRepository<XML07> _xml07Repository;
    private readonly IBaseRepository<XML08> _xml08Repository;
    private readonly IBaseRepository<XML09> _xml09Repository;
    private readonly IBaseRepository<XML10> _xml10Repository;
    private readonly IBaseRepository<XML11> _xml11Repository;
    private readonly IBaseRepository<XML13> _xml13Repository;
    private readonly IBaseRepository<XML14> _xml14Repository;
    private readonly IBaseRepository<XML15> _xml15Repository;
    private readonly IBaseRepository<XMLError> _errorRepository;

    private readonly IBaseRepository<UserLogs> _userLogsRepository;

    private readonly IReportHandleService _reportHandle;

    private ClaimsPrincipal CurrentUser => _httpContext.HttpContext?.User ?? throw new InvalidOperationException("Không có HttpContext");

    private string RequiredClaim(string type) => CurrentUser.FindFirst(type)?.Value ?? throw new InvalidOperationException($"Thiếu claim {type}");

    private string CurrentMaCSKCB => RequiredClaim(ClaimKey.MA_CSKCB);

    private static T Need<T>(T? value) where T : class => value ?? throw new InvalidOperationException("Dữ liệu bắt buộc bị thiếu");

    private static T[] Rows<T>(T[]? items) => items ?? throw new InvalidDataException("Không có dữ liệu chi tiết");

    public FilesService130(
        IMapper mapper,
        ILogger<FilesService130> logger,
        IUnitOfWork unitOfWork,
        ISysHistoryLogService sysHistoryLogService,
        IHttpContextAccessor httpContext,

        IBaseRepository<Hospital> hospitalRepository,
        IDMErrorService130 dmErrorRepository,
        IBaseRepository<DMKhoaBV> dmKhoaRepository,
        IBaseRepository<DMBacSi> dmBacSiRepository,
        IBaseRepository<ChiDinhDongThoi> dmDongThoiRepository,

        IXml01Repository hosoRepository,
        IHoSoManagementService130 hosoService,

        IBaseRepository<XMLState> xmlStateRepository,
        IBaseRepository<XML02> xml02Repository,
        IBaseRepository<XML03> xml03Repository,
        IBaseRepository<XML04> xml04Repository,
        IBaseRepository<XML05> xml05Repository,
        IBaseRepository<XML06> xml06Repository,
        IBaseRepository<XML07> xml07Repository,
        IBaseRepository<XML08> xml08Repository,
        IBaseRepository<XML09> xml09Repository,
        IBaseRepository<XML10> xml10Repository,
        IBaseRepository<XML11> xml11Repository,
        IBaseRepository<XML13> xml13Repository,
        IBaseRepository<XML14> xml14Repository,
        IBaseRepository<XML15> xml15Repository,
        IBaseRepository<XMLError> errorRepository,

        IBaseRepository<UserLogs> userLogsRepository,

        IReportHandleService reportHandle
    )
    {
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _sysHistoryLogService = sysHistoryLogService;
        _httpContext = httpContext;

        _hospitalRepository = hospitalRepository;
        _dmErrorRepository = dmErrorRepository;
        _dmBacSiRepository = dmBacSiRepository;
        _dmKhoaRepository = dmKhoaRepository;
        _dmDongThoiRepository = dmDongThoiRepository;

        _hosoRepository = hosoRepository;
        _hosoService = hosoService;

        _stateRepository = xmlStateRepository;
        _xml02Repository = xml02Repository;
        _xml03Repository = xml03Repository;
        _xml04Repository = xml04Repository;
        _xml05Repository = xml05Repository;
        _xml06Repository = xml06Repository;
        _xml07Repository = xml07Repository;
        _xml08Repository = xml08Repository;
        _xml09Repository = xml09Repository;
        _xml10Repository = xml10Repository;
        _xml11Repository = xml11Repository;
        _xml13Repository = xml13Repository;
        _xml14Repository = xml14Repository;
        _xml15Repository = xml15Repository;
        _errorRepository = errorRepository;

        _userLogsRepository = userLogsRepository;
        _reportHandle = reportHandle;
    }
    #endregion

    #region File Upload
    private Encoding GetEncoding(byte[] bom)
    {
        // Analyze the BOM

        if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
        if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
        if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
        if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
        if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

        // We actually have no idea what the encoding is if we reach this point, so
        // you may wish to return null instead of defaulting to ASCII
        return Encoding.ASCII;
    }

    static string GetXmlString(object obj)
    {
        var serializer = new XmlSerializer(obj.GetType());
        using var writer = new Utf8StringWriter();
        serializer.Serialize(writer, obj, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
        return writer.ToString();
    }

    //Upload qua web, lay thong tin hospitalID, hospitalName, creatorID, maCSKCB tu AutToken
    public async Task<ResponseModel<dynamic>?> FileUploadAsync(FileUpload fileUpload)
    {
        var user = CurrentUser;
        var hospitalID = RequiredClaim(ClaimKey.HOSPITAL_ID);
        var hospitalName = user.Claims.First(claim => claim.Type == "HospitalName").Value ?? "";
        var maCSKCB = CurrentMaCSKCB;
        var userID = RequiredClaim(ClaimTypes.Sid);
        var email = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(email))
        {
            email = user.FindFirst(ClaimTypes.Email)?.Value;
        }

        return await FileUploadAPI(fileUpload, hospitalID, hospitalName, userID, maCSKCB, email, true);
    }

    //Truyen file qua API se phai truyen vao hospitalID, hospitalName, creatorID, maCSKCB
    public async Task<ResponseModel<dynamic>?> FileUploadAPI(FileUpload fileUpload, string hospitalID, string hospitalName, string userID, string maCSKCB, string? email, bool isUpload, bool isFromHIS = false)
    {
        //Kiem tra thong tin cua benh vien truoc khi upload
        var hospital = await _hospitalRepository.GetItem(e => e.MA_CSKCB == maCSKCB &&  e.IsActivated == true);
        if (hospital == null)
        {
            return new ResponseModel<dynamic> { Code = HttpStatusCode.Forbidden, Data = new { successMessage = "", failedMessage = "Thông tin bệnh viện không chính xác", uploadStatus = "", uploadTime = "0" }, Message = "" };
        }

        if (fileUpload.FormFiles.Count >= 100)
        {
            return new ResponseModel<dynamic> { Code = HttpStatusCode.Forbidden, Data = new { successMessage = "", failedMessage = "Số lượng file upload vượt quá giới hạn (100 files)/ Nếu đẩy số lượng lớn, bạn hãy đẩy qua Mdinsight Adapter Client", uploadStatus = "", uploadTime = "0" }, Message = "" };
        }

        DateTime startTime = DateTime.Now;
        string filePath = Uti.GenerateFilePath130(maCSKCB, startTime);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        int totalFile = fileUpload.FormFiles.Count;
        int failed = 0;
        int success = 0;
        var successMessage = "Hồ sơ đã tải lên: <br/>";
        var failedMessage = "<b>Hồ sơ tải lỗi: </b><br/>";

        List<XML01> newXml1s = new();
        List<string> failedFiles = new();
        foreach (var aformFile in fileUpload.FormFiles)
        {
            var fileExtension = Path.GetExtension(aformFile.FileName);
            if (fileExtension?.ToLower() != ".xml")
            {
                failed++;
                string errorAt = aformFile.FileName;
                failedMessage += "File không đúng định dạng " + Path.GetFileName(aformFile.FileName) + " Lỗi: " + errorAt + "<br/> ";// +" "+ e.InnerException.ToString() + "<br/> ";
                failedFiles.Add(aformFile.FileName);
                continue;
            }

            try
            {
                string result = "";
                using (BinaryReader b = new(aformFile.OpenReadStream()))
                {
                    byte[] binData = b.ReadBytes((int)aformFile.Length);
                    Encoding enc = GetEncoding(binData);
                    if (enc == Encoding.UTF8)
                    {
                        result = Encoding.Default.GetString(binData);
                    }
                    else if (enc == Encoding.Unicode)
                    {
                        result = enc.GetString(binData);
                    }
                    else
                    {
                        result = Encoding.ASCII.GetString(binData);
                    }
                }

                int iCnt = 0;
                XML01? xml01 = null;
                List<string> fileLst;
                var hosoGD = result.ParseXML<GIAMDINHHS130>();
                var thongTinHoSo = hosoGD.THONGTINHOSO ?? throw new InvalidDataException("Thiếu THONGTINHOSO");
                var danhSachHoSo = thongTinHoSo.DANHSACHHOSO ?? throw new InvalidDataException("Thiếu DANHSACHHOSO");

                //Linhist changed 190826
                var maxNumDub = "08101;25001;34001;340001".Contains(maCSKCB) ? 20 : "27009".Contains(maCSKCB) ? 5 : 2;

                foreach (var dsHS in danhSachHoSo)
                {
                    iCnt++;
                    xml01 = null;
                    fileLst = new List<string>();

                    foreach (var fileHoso in dsHS)
                    {
                        string fileContent = (fileHoso.NOIDUNGFILE ?? "").Trim();
                        if (string.IsNullOrEmpty(fileContent)) continue;
                        try
                        {
                            fileContent = Encoding.Default.GetString(Convert.FromBase64String(fileContent)).Replace("\n", "");
                        }
                        catch
                        {
                            failed++;
                            failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt}: của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng BASE64<br/> ";
                            failedFiles.Add(aformFile.FileName);
                            return null;
                        }

                        if (fileHoso.LOAIHOSO == "XML1")
                        {
                            HOSO_XML01 file1;
                            try
                            {
                                file1 = fileContent.ParseXML<HOSO_XML01>();

                                if (file1.MA_CSKCB != maCSKCB)
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: XML01 có MA_CSKCB không chính xác<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    return null;
                                }

                                //haimaster update tranh truong hop cho vao vong lap day di day lai 1 ho so
                                var numberhs = _hosoRepository.DbSet.AsNoTracking().Count(x => x.MA_LK == file1.MA_LK && x.MA_CSKCB == maCSKCB && x.IsDelete != true);
                                if (numberhs > maxNumDub)
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: XML01 có Mã liên kết đã tồn tại trong hệ thống<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    return null;
                                }
                            }
                            catch
                            {
                                failed++;
                                failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: XML01 không đọc được do sai định dạng XML<br/> ";
                                failedFiles.Add(aformFile.FileName);
                                return null;
                            }

                            try
                            {
                                xml01 = file1.Export();
                                xml01.NGAY_LAP = EvalHelper.ToDateSql(thongTinHoSo.NGAYLAP ?? string.Empty);
                                xml01.FileName = Uti.ToFilename(aformFile.FileName);
                                xml01.HospitalID = hospitalID;
                                xml01.CreatorID = userID;
                                xml01.CreateDate = DateTime.Now;
                                xml01.IsDelete = false;
                                xml01.IsUpload = isUpload;
                                success++;
                            }
                            catch { }

                            continue;
                        }

                        if (xml01 == null) break;

                        switch (fileHoso.LOAIHOSO)
                        {
                            //XML02
                            case "XML2":
                                CHITIEU_XML02 list2;
                                try
                                {
                                    //Remove xmlns="" (empty namespace
                                    list2 = fileContent.ParseXML<CHITIEU_XML02>();
                                }
                                catch (Exception ex)
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML <br/> {ex.ToString()}<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file02 in Rows(list2.DANH_SACH?.CHI_TIETs))
                                    {
                                        file02.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;

                            //XML03
                            case "XML3":
                                CHITIEU_XML03 list3;
                                try
                                {
                                    list3 = fileContent.ParseXML<CHITIEU_XML03>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file03 in Rows(list3.DANH_SACH?.CHI_TIETs))
                                    {
                                        file03.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;

                            //XML04
                            case "XML4":
                                CHITIEU_XML04 list4;
                                try
                                {
                                    list4 = fileContent.ParseXML<CHITIEU_XML04>();
                                }
                                catch (Exception ex)
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> {ex.ToString()}<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file04 in Rows(list4.DANH_SACH?.CHI_TIETs))
                                    {
                                        file04.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;

                            //XML05
                            case "XML5":
                                CHITIEU_XML05 list5;
                                try
                                {
                                    list5 = fileContent.ParseXML<CHITIEU_XML05>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file05 in Rows(list5.DANH_SACH?.CHI_TIETs))
                                    {
                                        file05.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;

                            //XML06
                            case "XML6":
                                CHITIEU_XML06 list6;
                                try
                                {
                                    list6 = fileContent.ParseXML<CHITIEU_XML06>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng BASE64<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file06 in Rows(list6.DANH_SACH?.CHI_TIETs))
                                    {
                                        file06.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;

                            //XML07
                            case "XML7":
                                HOSO_XML07 file7;
                                try
                                {
                                    file7 = fileContent.ParseXML<HOSO_XML07>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    file7.Export().AppendTo(xml01);
                                    success++;
                                }
                                catch { }

                                break;

                            //XML08
                            case "XML8":
                                HOSO_XML08 file8;
                                try
                                {
                                    file8 = fileContent.ParseXML<HOSO_XML08>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    file8.Export().AppendTo(xml01);
                                    success++;
                                }
                                catch { }

                                break;

                            //XML09
                            case "XML9":
                                CHITIEU_XML09 list9;
                                try
                                {
                                    list9 = fileContent.ParseXML<CHITIEU_XML09>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file09 in Rows(list9.DANH_SACH?.CHI_TIETs))
                                    {
                                        file09.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;

                            //XML10
                            case "XML10":
                                HOSO_XML10 file10;
                                try
                                {
                                    file10 = fileContent.ParseXML<HOSO_XML10>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    file10.Export().AppendTo(xml01);
                                    success++;
                                }
                                catch { }

                                break;

                            //XML11
                            case "XML11":
                                HOSO_XML11 file11;
                                try
                                {
                                    file11 = fileContent.Replace("\n", "").ParseXML<HOSO_XML11>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt}: XML11 Nghỉ hưởng BHXH của file {Path.GetFileName(aformFile.FileName)} lỗi: không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    file11.Export().AppendTo(xml01);
                                    success++;
                                }
                                catch { }

                                break;

                            //XML13
                            case "XML13":
                                HOSO_XML13 file13;
                                try
                                {
                                    file13 = fileContent.Replace("\n", "").ParseXML<HOSO_XML13>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt}: XML13 Nghỉ hưởng BHXH của file {Path.GetFileName(aformFile.FileName)} lỗi: không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    file13.Export().AppendTo(xml01);
                                    success++;
                                }
                                catch { }

                                break;

                            //XML14
                            case "XML14":
                                HOSO_XML14 file14;
                                try
                                {
                                    file14 = fileContent.Replace("\n", "").ParseXML<HOSO_XML14>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt}: XML14 Nghỉ hưởng BHXH của file {Path.GetFileName(aformFile.FileName)} lỗi: không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    file14.Export().AppendTo(xml01);
                                    success++;
                                }
                                catch { }

                                break;

                            //XML15
                            case "XML15":
                                CHITIEU_XML15 list15;
                                try
                                {
                                    list15 = fileContent.ParseXML<CHITIEU_XML15>();
                                }
                                catch
                                {
                                    failed++;
                                    failedMessage += $" &nbsp; + Hồ sơ thứ {iCnt} của file {Path.GetFileName(aformFile.FileName)} lỗi: {fileHoso.LOAIHOSO} không đọc được do sai định dạng XML<br/> ";
                                    failedFiles.Add(aformFile.FileName);
                                    break;
                                }

                                try
                                {
                                    foreach (var file15 in Rows(list15.DANH_SACH?.CHI_TIETs))
                                    {
                                        file15.Export().AppendTo(xml01);
                                    }
                                    success++;
                                }
                                catch { }

                                break;
                        }
                    }

                    if (xml01 != null)
                    {
                        xml01.HasFiles = ";";
                        if (xml01.XML02s.Any()) xml01.HasFiles += "2;";
                        if (xml01.XML03s.Any()) xml01.HasFiles += "3;";
                        if (xml01.XML04s.Any()) xml01.HasFiles += "4;";
                        if (xml01.XML05s.Any()) xml01.HasFiles += "5;";
                        if (xml01.XML06s.Any()) xml01.HasFiles += "6;";
                        if (xml01.XML07s.Any()) xml01.HasFiles += "7;";
                        if (xml01.XML08s.Any()) xml01.HasFiles += "8;";
                        if (xml01.XML09s.Any()) xml01.HasFiles += "9;";
                        if (xml01.XML10s.Any()) xml01.HasFiles += "10;";
                        if (xml01.XML11s.Any()) xml01.HasFiles += "11;";
                        if (xml01.XML13s.Any()) xml01.HasFiles += "13;";
                        if (xml01.XML14s.Any()) xml01.HasFiles += "14;";
                        if (xml01.XML15s.Any()) xml01.HasFiles += "15;";

                        successMessage += Path.GetFileName(aformFile.FileName) + "<br/>";

                        if (danhSachHoSo.Length > 1)
                        {
                            var fileName = Uti.ToFilename(Need(xml01.MA_LK)) + ".xml";
                            GIAMDINHHS130 newHoso = new()
                            {
                                THONGTINDONVI = hosoGD.THONGTINDONVI,
                                THONGTINHOSO = new()
                                {
                                    DANHSACHHOSO = new FILEHOSO[1][] { dsHS }
                                },
                            };

                            await File.WriteAllTextAsync(Path.Combine(filePath, fileName), GetXmlString(newHoso));
                            xml01.FileName = fileName;
                        }

                        newXml1s.Add(xml01);



                    }
                }
            }
            catch (Exception ex)
            {
                failed++;
                var s = ex.Message;
                failedMessage += $"Lỗi đọc file {Path.GetFileName(aformFile.FileName)}: Nội dung file rỗng hoặc không đúng định dạng.<br/> ";
                failedFiles.Add(aformFile.FileName);
            }

            var formFile = aformFile;
            if (formFile.Length > 0)
            {
                try
                {
                    using var stream = File.Create(Path.Combine(filePath, Uti.ToFilename(formFile.FileName)));
                    await formFile.CopyToAsync(stream);
                }
                catch { }
            }
        }

        try
        {
            //1.8.2024 thêm isFormHIS cho giám định luôn các hồ sơ đẩy qua API
            await _hosoService.CreateNewXML(newXml1s, maCSKCB, email, isFromHIS);
        }
        catch
        {
            failed++;
            failedMessage += $" &nbsp; + Không thể input vào DB<br/> ";

        }

        TimeSpan interval = DateTime.Now.Subtract(startTime);
        string uploadStatus = $"Tổng số file tải lên: {totalFile} file(s) XML<br/> Tải lên thành công: {success} hồ sơ<br/>  Tải lên thất bại: {failed} hồ sơ<br/> ";
        string uploadTime = $"Thời gian tải lên: {interval.TotalSeconds}giây<br/>";

        return new ResponseModel<dynamic> { Code = HttpStatusCode.OK, Data = new { successMessage, failedMessage, uploadStatus, uploadTime, failedFiles }, Message = "" };
    }
    #endregion

    private bool HasRole<T>(ResponseModel<T> result)
    {
        var currentRole = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrWhiteSpace(currentRole)) return true;

        if (result != null)
        {
            result.Code = HttpStatusCode.Forbidden;
            result.Message = MessageResource.ERR_403_FORBIDDEN;
        }
        return false;
    }

    public static FilterQueryableModel<XML01> BuildQueryToGetListData(FilterDownloadModel request, bool hasIncludes = true, bool hasDeleted = false)
    {
        var result = new FilterQueryableModel<XML01>();

        #region Build Filter Query
        var filter = PredicateBuilder.New<XML01>(true);
        if (!hasDeleted)
        {
            filter = PredicateBuilder.New<XML01>(x => x.IsDelete != true);
        }
        if (!string.IsNullOrEmpty(request.HospitalId))
        {
            filter = filter.And(x => x.MA_CSKCB == request.HospitalId);
        }
        if (!string.IsNullOrEmpty(request.MaCSKCB))
        {
            filter = filter.And(x => x.MA_CSKCB == request.MaCSKCB);
        }
        if (!string.IsNullOrWhiteSpace(request.SearchKey))
        {
            var key = request.SearchKey.ToLower();
            string pattern = request.SearchKey;

            var filterSearch = PredicateBuilder.New<XML01>();
            filterSearch = filterSearch.Or(x => x.MA_THE_BHYT != null && x.MA_THE_BHYT.Contains(pattern))
                                        .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern))
                                        .Or(x => x.MA_BN != null && x.MA_BN.Contains(pattern))
                                        .Or(x => x.HO_TEN != null && x.HO_TEN.Contains(pattern));

            filter = filter.And(filterSearch);
        }

        if (request.Departments != null && request.Departments.Any())
        {
            //18.7.2024 haimaster update thử nghiệm
            //Linhist added 30/12/24
            var allDeps = ";" + string.Join(";", request.Departments.ToArray()) + ";";
            var filterDep = PredicateBuilder.New<XML01>(x => allDeps.Contains(";" + x.MA_KHOA + ";"));

            foreach (var maKhoa in request.Departments)
            {
                filterDep = filterDep.Or(x => (";" + x.MA_KHOA + ";").Contains(";" + maKhoa + ";"));
            }
            filter = filter.And(x => x.MA_KHOA != null).And(filterDep);
        }

        int fromHour = request.FromHour ?? 0;
        int fromMin = request.FromMin ?? 0;
        int toHour = request.ToHour ?? 23;
        int toMin = request.ToMin ?? 59;

        // Create Date filter

        if (request.FromDateCreate != null && request.FromDateCreate.HasValue)
        {
            DateTime dayFrom = request.FromDateCreate.Value.Date.AddHours(fromHour).AddMinutes(fromMin);
            filter = filter.And(x => x.CreateDate >= dayFrom);
        }
        if (request.ToDateCreate != null && request.ToDateCreate.HasValue)
        {
            DateTime dayTo = request.ToDateCreate.Value.Date.AddHours(toHour).AddMinutes(toMin).AddMilliseconds(1);
            if (toHour == 0 && toMin == 0) dayTo = dayTo.AddDays(1).AddMilliseconds(-1);
            filter = filter.And(x => x.CreateDate < dayTo);
        }

        // NGAY_VAO filter
        if (request.FromDateIn != null && request.FromDateIn.HasValue)
        {
            DateTime dayInFrom = request.FromDateIn.Value.Date.AddHours(fromHour).AddMinutes(fromMin);
            filter = filter.And(x => x.NGAY_VAO >= dayInFrom);
        }
        if (request.ToDateIn != null && request.ToDateIn.HasValue)
        {
            DateTime dayInTo = request.ToDateIn.Value.Date.AddHours(toHour).AddMinutes(toMin).AddMilliseconds(1);
            if (toHour == 0 && toMin == 0) dayInTo = dayInTo.AddDays(1).AddMilliseconds(-1);
            filter = filter.And(x => x.NGAY_VAO < dayInTo);
        }

        // NGAY_RA filter
        if (request.FromDateOut != null && request.FromDateOut.HasValue)
        {
            DateTime dayOutFrom = request.FromDateOut.Value.Date.AddHours(fromHour).AddMinutes(fromMin);
            filter = filter.And(x => x.NGAY_RA >= dayOutFrom);
        }
        if (request.ToDateOut != null && request.ToDateOut.HasValue)
        {
            DateTime dayOutTo = request.ToDateOut.Value.Date.AddHours(toHour).AddMinutes(toMin).AddMilliseconds(1);
            if (toHour == 0 && toMin == 0) dayOutTo = dayOutTo.AddDays(1).AddMilliseconds(-1);
            filter = filter.And(x => x.NGAY_RA < dayOutTo);
        }

        // NGAY_TTOAN filter
        if (request.FromDatePaid != null && request.FromDatePaid.HasValue)
        {
            DateTime dayPaidFrom = request.FromDatePaid.Value.Date.AddHours(fromHour).AddMinutes(fromMin);
            filter = filter.And(x => x.NGAY_TTOAN >= dayPaidFrom);
        }
        if (request.ToDatePaid != null && request.ToDatePaid.HasValue)
        {
            DateTime dayPaidTo = request.ToDatePaid.Value.Date.AddHours(toHour).AddMinutes(toMin).AddMilliseconds(1);
            if (toHour == 0 && toMin == 0) dayPaidTo = dayPaidTo.AddDays(1).AddMilliseconds(-1);
            filter = filter.And(x => x.NGAY_TTOAN < dayPaidTo);
        }

        // MA_LOAI_KCB filter
        if (!string.IsNullOrEmpty(request.TypeKCB?.Trim()))
        {
            filter = filter.And(x => x.MA_LOAI_KCB == request.TypeKCB.ToString() || x.MA_LOAI_KCB == "0" + request.TypeKCB.ToString());
        }

        // Error status filter
        var filterError = PredicateBuilder.New<XML01>(true);
        if (request.Errors != null && request.Errors.Any())
        {
            List<int?> errors = request.Errors.Select(e => Uti.ToNum(e)).Where(e => e != null).ToList();
            filterError = filterError.Or(x => x.Errors.Any(y => errors.Contains(y.MA_LOI)));
        }
        else if (1.Equals(request.Status))
        {
            filterError = filterError.Or(x => x.State == null || x.State.TONG_SO_LOI == null || x.State.CANH_BAO == null || x.State.TONG_SO_LOI + x.State.CANH_BAO == 0);
        }
        else if (2.Equals(request.Status))
        {
            filterError = filterError.Or(x => x.State != null && x.State.TONG_SO_LOI != null && x.State.CANH_BAO != null && x.State.TONG_SO_LOI + x.State.CANH_BAO > 0);
        }
        else if (3.Equals(request.Status) && (request.MaCSKCB == "27009" || request.HospitalId == "27009"))
        {
            filterError = filterError.Or(x => x.CreatorID == "8d69edc4-66df-4fec-b270-fc694115f14a");
        }

        filter = filter.And(filterError);
        result.FilterQueryable = filter;
        #endregion

        #region Build Sort Query
        result.OrderQueryable = new List<Func<IOrderedQueryable<XML01>, IOrderedQueryable<XML01>>>
        {
            x => x.OrderByDescending(x => x.CreateDate)
        };
        #endregion

        #region Includes
        if (hasIncludes)
        {
            result.IncludeQueryable = x => (IIncludableQueryable<XML01, object>)x.Include(y => y.State);
        }
        #endregion

        return result;
    }

    #region Get XML
    public async Task<ResponseModel<PaginationResponseModel<XML01Info>>> GetXml01Items(FilesPaginationRequest request)
    {
        var page = new PaginationResponseModel<XML01Info>();
        var result = new ResponseModel<PaginationResponseModel<XML01Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var filterModel = _mapper.Map<FilterDownloadModel>(request);

            var query = BuildQueryToGetListData(filterModel);
            var data = await _hosoRepository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, query.FilterQueryable, query.OrderQueryable, query.IncludeQueryable);

            page.Items = data.Items.Select(x => new XML01Info(x)).ToList();
            page.TotalItems = data.TotalItems;
        }
        catch { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML02Info>>> GetXml02Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML02Info>();
        var result = new ResponseModel<PaginationResponseModel<XML02Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {

            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML02>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            if (!string.IsNullOrEmpty(request.SearchKey))
            {
                filter = filter.And(x => (x.TEN_THUOC != null && x.TEN_THUOC.Contains(request.SearchKey)) || (x.MA_THUOC != null && x.MA_THUOC.Contains(request.SearchKey)));
            }
            var orderBy = new List<Func<IOrderedQueryable<XML02>, IOrderedQueryable<XML02>>>
            {
                x => x.OrderBy(x => x.STT)
            };

            var data = await _xml02Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, orderBy);
            page.Items = data.Items.Select(x => new XML02Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML03Info>>> GetXml03Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML03Info>();
        var result = new ResponseModel<PaginationResponseModel<XML03Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML03>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            if (!string.IsNullOrEmpty(request.SearchKey))
            {
                var filterSearch = PredicateBuilder.New<XML03>(x => (x.MA_DICH_VU != null && x.MA_DICH_VU.Contains(request.SearchKey)))
                                .Or(x => (x.MA_VAT_TU != null && x.MA_VAT_TU.Contains(request.SearchKey)))
                                .Or(x => (x.TEN_DICH_VU != null && x.TEN_DICH_VU.Contains(request.SearchKey)))
                                .Or(x => (x.TEN_VAT_TU != null && x.TEN_VAT_TU.Contains(request.SearchKey)));

                filter = filter.And(filterSearch);
            }

            var orderBy = new List<Func<IOrderedQueryable<XML03>, IOrderedQueryable<XML03>>>
            {
                x => x.OrderBy(x => x.STT)
            };

            var data = await _xml03Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, orderBy);
            page.Items = data.Items.Select(x => new XML03Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML04Info>>> GetXml04Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML04Info>();
        var result = new ResponseModel<PaginationResponseModel<XML04Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML04>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            if (!string.IsNullOrEmpty(request.SearchKey))
            {
                filter = filter.And(x => (x.MA_DICH_VU != null && x.MA_DICH_VU.Contains(request.SearchKey)) || (x.MA_CHI_SO != null && x.MA_CHI_SO.Contains(request.SearchKey)) || (x.TEN_CHI_SO != null && x.TEN_CHI_SO.Contains(request.SearchKey)));
            }

            var orderBy = new List<Func<IOrderedQueryable<XML04>, IOrderedQueryable<XML04>>>
            {
                x => x.OrderBy(x => x.STT)
            };

            var data = await _xml04Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, orderBy);
            page.Items = data.Items.Select(x => new XML04Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML05Info>>> GetXml05Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML05Info>();
        var result = new ResponseModel<PaginationResponseModel<XML05Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML05>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            if (!string.IsNullOrEmpty(request.SearchKey))
            {
                var filterSearch = PredicateBuilder.New<XML05>(x => (x.NGUOI_THUC_HIEN != null && x.NGUOI_THUC_HIEN.Contains(request.SearchKey)))
                                        .Or(x => (x.GIAI_DOAN_BENH != null && x.GIAI_DOAN_BENH.Contains(request.SearchKey)))
                                        .Or(x => (x.DIEN_BIEN_LS != null && x.DIEN_BIEN_LS.Contains(request.SearchKey)))
                                        .Or(x => (x.HOI_CHAN != null && x.HOI_CHAN.Contains(request.SearchKey)));

                filter = filter.And(filterSearch);
            }

            var orderBy = new List<Func<IOrderedQueryable<XML05>, IOrderedQueryable<XML05>>>
            {
                x => x.OrderBy(x => x.STT)
            };

            var data = await _xml05Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, orderBy);
            page.Items = data.Items.Select(x => new XML05Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML06Info>>> GetXml06Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML06Info>();
        var result = new ResponseModel<PaginationResponseModel<XML06Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML06>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml06Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);
            page.Items = data.Items.Select(x => new XML06Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML07Info>>> GetXml07Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML07Info>();
        var result = new ResponseModel<PaginationResponseModel<XML07Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML07>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml07Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);
            page.Items = data.Items.Select(x => new XML07Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML08Info>>> GetXml08Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML08Info>();
        var result = new ResponseModel<PaginationResponseModel<XML08Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML08>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml08Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);
            page.Items = data.Items.Select(x => new XML08Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML09Info>>> GetXml09Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML09Info>();
        var result = new ResponseModel<PaginationResponseModel<XML09Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML09>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml09Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);
            page.Items = data.Items.Select(x => new XML09Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML10Info>>> GetXml10Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML10Info>();
        var result = new ResponseModel<PaginationResponseModel<XML10Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML10>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml10Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);
            page.Items = data.Items.Select(x => new XML10Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML11Info>>> GetXml11Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML11Info>();
        var result = new ResponseModel<PaginationResponseModel<XML11Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML11>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml11Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);

            page.Items = data.Items.Select(x => new XML11Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML13Info>>> GetXml13Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML13Info>();
        var result = new ResponseModel<PaginationResponseModel<XML13Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML13>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml13Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);

            page.Items = data.Items.Select(x => new XML13Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML14Info>>> GetXml14Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML14Info>();
        var result = new ResponseModel<PaginationResponseModel<XML14Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML14>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml14Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);

            page.Items = data.Items.Select(x => new XML14Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }

    public async Task<ResponseModel<PaginationResponseModel<XML15Info>>> GetXml15Items(PaginationRequestModel request, int? xml01ID)
    {
        var page = new PaginationResponseModel<XML15Info>();
        var result = new ResponseModel<PaginationResponseModel<XML15Info>>(HttpStatusCode.OK, "", page);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var filter = PredicateBuilder.New<XML15>(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB);
            var data = await _xml15Repository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter);

            page.Items = data.Items.Select(x => new XML15Info(x));
            page.TotalItems = data.TotalItems;
        }
        catch
        { }

        return result;
    }
    #endregion

    //25.9.2024 haimaster update check tra ra gia tri bi loi trong XML
    public object? GetPropertyValue(object obj, string? propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName ?? throw new ArgumentNullException(nameof(propertyName)));

        return prop?.GetValue(obj, null);
    }

    private static string NotNull(object? value) => value?.ToString() ?? throw new InvalidOperationException("Không có giá trị");

    private static IEnumerable<IRecord>? XmlRows(XML01 x, string errorFile) => errorFile switch
    {
        "XML2" => x.XML02s,
        "XML3" => x.XML03s,
        "XML4" => x.XML04s,
        "XML5" => x.XML05s,
        "XML6" => x.XML06s,
        "XML7" => x.XML07s,
        "XML8" => x.XML08s,
        "XML9" => x.XML09s,
        "XML10" => x.XML10s,
        "XML11" => x.XML11s,
        "XML13" => x.XML13s,
        "XML14" => x.XML14s,
        "XML15" => x.XML15s,
        _ => null,
    };

    //linhist added 24/06/26
    private static string GetProp(object? obj, string? property)
    {
        if (obj == null || string.IsNullOrEmpty(property)) return "Không có giá trị";

        var prop = obj.GetType().GetProperty(property);
        return prop?.GetValue(obj, null)?.ToString() ?? "Không có giá trị";

    }

    //linhist added 24/06/26
    public async Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorModels(XML01? xml01)
    {
        var res = new ResponseModel<IEnumerable<XML1ErrorInfoModel>>(HttpStatusCode.OK, "", new List<XML1ErrorInfoModel>());
        if (xml01 == null)
        {
            res.Code = HttpStatusCode.BadRequest;
            res.Message = "Không có hồ sơ đang tìm kiếm";
            return res;
        }

        //Linhist changed 110926

        var dmErrors = await _dmErrorRepository.GetAllByHospital(xml01.MA_CSKCB) ?? throw new InvalidOperationException("Không lấy được danh mục lỗi");
        var allCodes = dmErrors.Select(x => x.ErrorCode).ToList();
        var errors = await _errorRepository.GetListAsync(e => e.XML01ID == xml01.XML01ID && e.MA_CSKCB == xml01.MA_CSKCB && allCodes.Contains(e.MA_LOI));
        if (!errors.Any())
        {
            res.Code = HttpStatusCode.OK;
            res.Message = "Hồ sơ không có lỗi";
            return res;
        }

        var errInfos = errors.Join(dmErrors, e => e.MA_LOI, d => d.ErrorCode, (e, d) => new XML1ErrorInfoModel
        {
            ErrorAtXMLFile = d.ErrorAtXMLFile,
            ErrorAtXMLTag = d.ErrorAtXMLTag,
            ErrorContent = d.ErrorContent,
            ErrorType = d.ErrorType,
            FileName = xml01.FileName,
            XeErrorCode = e.MA_LOI,
            XeErrorRow = e.STT,
            ErrorDesc = e.ErrorDesc,
            ErrorMode = d.IsAlarm != true
        }).Distinct().ToList();

        var inners = string.Join(";", errInfos.Select(e => Need(e.ErrorAtXMLFile).Replace("XML", "")).Distinct().ToArray());

        try
        {
            await _hosoService.AttachInnerXML(new[] { xml01 }, inners);
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"CheckError for ({xml01.XML01ID}::{xml01.FileName}) CSKCB: {xml01.MA_CSKCB}. Error: {ex.Message}", nameof(GetXml1ErrorModels));
        }

        foreach (var err in errInfos)
        {
            try
            {
                switch (err.ErrorAtXMLFile)
                {
                    case "XML2":
                        var x2 = xml01.XML02s.FirstOrDefault(x => x.STT == err.XeErrorRow);
                        err.ErrorConts = GetProp(x2, err.ErrorAtXMLTag);
                        if (x2 == null) throw new InvalidOperationException("Không tìm thấy dòng XML2");
                        err.MA_AX = x2.MA_THUOC;
                        err.TenAX = x2.TEN_THUOC;
                        err.Ma_BS_YL = x2.MA_BAC_SI;
                        break;

                    case "XML3":
                        var x3 = xml01.XML03s.FirstOrDefault(x => x.STT == err.XeErrorRow);
                        err.ErrorConts = GetProp(x3, err.ErrorAtXMLTag);
                        if (x3 == null) throw new InvalidOperationException("Không tìm thấy dòng XML3");
                        if (x3.MA_NHOM == 10)
                        {
                            err.MA_AX = x3.MA_VAT_TU;
                            err.TenAX = x3.TEN_VAT_TU;
                        }
                        else
                        {
                            err.MA_AX = x3.MA_DICH_VU;
                            err.TenAX = x3.TEN_DICH_VU;
                        }
                        err.Ma_BS_YL = x3.MA_BAC_SI;
                        err.Ma_BS_ChiDinh = x3.NGUOI_THUC_HIEN;
                        break;

                    case "XML4":
                        var x4 = xml01.XML04s.FirstOrDefault(x => x.STT == err.XeErrorRow);
                        err.ErrorConts = GetProp(x4, err.ErrorAtXMLTag);
                        if (x4 == null) throw new InvalidOperationException("Không tìm thấy dòng XML4");
                        err.MA_AX = x4.MA_DICH_VU;
                        err.Ma_BS_YL = x4.MA_BS_DOC_KQ;
                        break;

                    default:
                        err.ErrorConts = GetProp(xml01, err.ErrorAtXMLTag);
                        break;
                }
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"CheckError.GetFiles {err.ErrorConts}| {err.ErrorAtXMLTag}| {xml01.XML01ID} |{err.XeErrorCode} ({xml01.FileName}). CSKCB: {xml01.MA_CSKCB}. Error: {ex.Message}", nameof(GetXml1ErrorModels));
            }
        }

        res.Data = errInfos;
        res.Message = "Lấy nội dung lỗi thành công";

        // write log to db
        await _sysHistoryLogService.AddSysLog($"Get XML01 Error Model for {xml01.XML01ID}; {xml01.FileName}");

        return res;
    }

    #region Linhist changed 24/06/26

    public async Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorItemByFileName(List<string> fileNames, string maCSKCB)
    {
        var res = new ResponseModel<IEnumerable<XML1ErrorInfoModel>>(HttpStatusCode.OK, "", new List<XML1ErrorInfoModel>());

        try
        {
            var name = fileNames.FirstOrDefault(f => !string.IsNullOrEmpty(f));
            var file = await _hosoRepository.GetItemOrderBy(x => x.FileName == name && x.MA_CSKCB == maCSKCB && x.IsDelete != true, x => x.OrderByDescending(o => o.XML01ID));
            return await GetXml1ErrorModels(file);
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<IEnumerable<XML1ErrorInfoModel>>(
                ex, new List<XML1ErrorInfoModel>(), _logger, $"{nameof(FilesService130)} - {nameof(GetXml1ErrorItemByFileName)}");
            res.Message = ex.ToString();
            Uti.HandleCronJobLog(_logger, $"CheckError.GetFiles Co loi  {ex.Message} ({fileNames.FirstOrDefault(f => !string.IsNullOrEmpty(f))}). CSKCB: {maCSKCB}", nameof(FilesService130));
        }

        return res;
    }
    #endregion

    //1.8.2024 haimaster update get Error cho 130 API
    public async Task<ResponseModel<IEnumerable<XML1ErrorInfoModel>>> GetXml1ErrorItemsByFileName(List<string> fileNames, string maCSKCB)
    {
        var res = new ResponseModel<IEnumerable<XML1ErrorInfoModel>>(HttpStatusCode.OK, "", new List<XML1ErrorInfoModel>());

        try
        {
            #region Get Data
            List<XML01> itemsXML1 = new List<XML01>();
            foreach (string fileName in fileNames)
            {
                //Doan nay can Linh ho tro de tach cau lenh sau do gop tay
                //26.9.2024 tam bo chi lay den XML5 vien Tuyen Tinh qua tay
                var file = await _hosoRepository.GetItemOrderBy(x => x.FileName == fileName && x.MA_CSKCB == maCSKCB
                , x => x.OrderByDescending(o => o.XML01ID)/*, i => i.Include(i1 => i1.XML02s).Include(i3 => i3.XML03s).Include(i4 => i4.XML04s).Include(i5 => i5.XML05s)*/);

                if (file != null)
                {

                    itemsXML1.Add(file);
                }

            }

            if (itemsXML1 != null && itemsXML1.Any())
            {

                var xml1Ids = itemsXML1.Select(x => x.XML01ID).ToList();
                var items = await _errorRepository.GetListAsync(x => xml1Ids.Contains(x.XML01ID) && x.MA_CSKCB == maCSKCB);
                if (items.Any())
                {

                    var itemInfo = new List<XML1ErrorInfoModel>();
                    var allErrorCodes = items.Select(x => x.MA_LOI).ToList();

                    //Linhist changed 110926
                    var lstDMErrors = await _dmErrorRepository.GetAllByHospital(maCSKCB) ?? throw new InvalidOperationException("Không lấy được danh mục lỗi");

                    string inner = "";
                    for (int i = 2; i < 16; i++)
                    {
                        if (i == 12) continue;

                        //Linhist changed 110926
                        if (lstDMErrors.Any(x => x.ErrorAtXMLFile == $"XML{i}"))
                        {
                            inner += $"{i};";
                        }
                    }

                    try
                    {
                        await _hosoService.AttachInnerXML(itemsXML1, inner.Trim());
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"CheckError.GetFiles {ex.ToString()} ({fileNames.FirstOrDefault()}). CSKCB: {maCSKCB}", nameof(FilesService130));
                    }

                    foreach (var i in items)
                    {
                        var item = new XML1ErrorInfoModel();
                        //Linhist changed 110926
                        var dmError = lstDMErrors.FirstOrDefault(x => x.ErrorCode == i.MA_LOI);
                        item.ErrorAtXMLFile = dmError?.ErrorAtXMLFile;
                        item.ErrorAtXMLTag = dmError?.ErrorAtXMLTag;
                        item.ErrorContent = dmError?.ErrorContent;
                        item.ErrorType = dmError?.ErrorType;
                        item.FileName = itemsXML1.Where(x => x.XML01ID == i.XML01ID).FirstOrDefault()?.FileName;
                        item.XeErrorCode = i.MA_LOI;
                        item.XeErrorRow = i.STT;
                        item.ErrorConts = "";

                        //ErrorMode == true -> bao loi, ErrorMode == false -> canh bao ko phai loi
                        //Linhist changed 110926
                        item.ErrorMode = (dmError ?? throw new InvalidOperationException("Không có danh mục lỗi")).IsAlarm != true;

                        var errorFile = dmError.ErrorAtXMLFile;
                        var tag = dmError.ErrorAtXMLTag;
                        if (errorFile is "XML1" or "XML2" or "XML3" or "XML4" or "XML5" or "XML6" or "XML7" or "XML8" or "XML9" or "XML10" or "XML11" or "XML13" or "XML14" or "XML15")
                        {
                            try
                            {
                                var rows = itemsXML1.Where(x => x.XML01ID == i.XML01ID)
                                    .SelectMany(x => errorFile == "XML1" ? new IRecord[] { x } : XmlRows(x, errorFile) ?? [])
                                    .Where(z => z.STT == i.STT);
                                var row = rows.FirstOrDefault();

                                if (errorFile == "XML1")
                                {
                                    item.ErrorConts = (row == null ? null : GetPropertyValue(row, tag))?.ToString() ?? "Không có giá trị";
                                }
                                else
                                {
                                    //XML3: MA_NHOM duoc doc truoc khi lay noi dung loi
                                    var isvattu = row is XML03 r3 ? NotNull(r3.MA_NHOM).ToString() : null;
                                    item.ErrorConts = NotNull(row == null ? null : GetPropertyValue(row, tag)).ToString() ?? "Không có giá trị";
                                    switch (row)
                                    {
                                        case XML02 x2:
                                            item.MA_AX = NotNull(x2.MA_THUOC);
                                            item.TenAX = NotNull(x2.TEN_THUOC);
                                            item.Ma_BS_YL = NotNull(x2.MA_BAC_SI);
                                            break;
                                        case XML03 x3:
                                            if (isvattu == "10")
                                            {
                                                item.MA_AX = NotNull(x3.MA_VAT_TU);
                                                item.TenAX = NotNull(x3.TEN_VAT_TU);
                                            }
                                            item.MA_AX = NotNull(x3.MA_DICH_VU);
                                            item.TenAX = NotNull(x3.TEN_DICH_VU);
                                            item.Ma_BS_YL = NotNull(x3.MA_BAC_SI);
                                            item.Ma_BS_ChiDinh = NotNull(x3.NGUOI_THUC_HIEN);
                                            break;
                                        case XML04 x4:
                                            item.MA_AX = NotNull(x4.MA_DICH_VU);
                                            item.Ma_BS_YL = NotNull(x4.MA_BS_DOC_KQ);
                                            break;
                                    }
                                }
                            }
                            catch
                            {
                                Uti.HandleCronJobLog(_logger, $"CheckError.GetFiles {item.ErrorConts}| {item.ErrorAtXMLTag}| {i.XML01ID} |{item.XeErrorCode} ({fileNames.FirstOrDefault()}). CSKCB: {maCSKCB}", nameof(FilesService130));
                            }
                        }

                        itemInfo.Add(item);
                    }

                    res.Data = itemInfo;
                    res.Message = "Lấy nội dung lỗi thành công";
                    #endregion

                    // write log to db
                    await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
                }
                else
                {
                    res.Code = HttpStatusCode.OK;
                    res.Message = "Hồ sơ không có lỗi";
                    return res;
                }
            }
            else
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = "Không có hồ sơ đang tìm kiếm";
                return res;
            }
        }
        catch (Exception ex)
        {
            res = Uti.HandleInternalServerError<IEnumerable<XML1ErrorInfoModel>>(
                ex, new List<XML1ErrorInfoModel>(), _logger, $"{nameof(FilesService130)} - {nameof(FilesService130.GetXml1ErrorItemsByFileName)}");
            res.Message = ex.ToString();
            Uti.HandleCronJobLog(_logger, $"CheckError.GetFiles Co loi  {ex.ToString()} ({fileNames.FirstOrDefault()}). CSKCB: {maCSKCB}", nameof(FilesService130));

        }
        return res;
    }

    #region Linhist changed 24/06/26

    public async Task<ResponseModel<IEnumerable<XMLErrorFullInfo>>> GetXmlErrorItems(int? xml01ID)
    {
        var res = new ResponseModel<IEnumerable<XMLErrorFullInfo>>(HttpStatusCode.OK, "", new List<XMLErrorFullInfo>());
        if (!HasRole(res)) return res;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;
            var file = await _hosoRepository.GetItemOrderBy(x => x.XML01ID == xml01ID && x.MA_CSKCB == currentMaCSKCB && x.IsDelete != true, x => x.OrderByDescending(o => o.XML01ID));
            var errors = await GetXml1ErrorModels(file);
            if (errors?.Data?.Any() == true)
            {
                res.Code = HttpStatusCode.OK;
                res.Data = errors.Data.Select(e => new XMLErrorFullInfo(e)).OrderBy(e => e.error_code).ToList();
                res.Message = "Lấy nội dung lỗi thành công";
            }
            else
            {
                res.Code = HttpStatusCode.NotFound;
                res.Message = "Hồ sơ không có lỗi";
            }
        }
        catch (Exception ex)
        {
            res.Message = ex.ToString();
            Uti.HandleCronJobLog(_logger, $"GetErrors Co loi  {ex.Message} (ID: {xml01ID}).", nameof(GetXmlErrorItems));
        }

        return res;
    }

    #endregion

    public async Task<ResponseModel<bool>> RecheckHoso(params long[] xmlIds)
    {
        var result = new ResponseModel<bool>(HttpStatusCode.OK, "", false);
        if (!HasRole(result)) return result;

        try
        {
            var currentMaCSKCB = CurrentMaCSKCB;

            //---------------------------------------------
            await _hosoService.AppendHosoToValidate(currentMaCSKCB, xmlIds);

            //14.11.2024 thêm hàm xóa XMLstate ở đây
            //Nhờ Linh thêm đoạn xóa toàn bộ XMLState của thằng XMLID cần giám định lại
            var states = await _stateRepository.GetListAsync(x => xmlIds.Contains(x.XML01ID) && x.MA_CSKCB == currentMaCSKCB);
            if (states?.Any() == true)
            {
                _stateRepository.DeleteList(states.ToList());
                await _stateRepository.SaveChangesAsync();
            }

            result.Data = true;
        }
        catch
        {
            result.Code = HttpStatusCode.BadRequest;
            result.Data = false;
        }

        return result;
    }

    public async Task<ResponseModel<byte[]>?> DownloadBaoCao(string filterDownloadString, int? fileType)
    {

        var maCSKCB = CurrentMaCSKCB;
        var userID = RequiredClaim(ClaimTypes.Sid);
        var useremail = RequiredClaim(ClaimTypes.NameIdentifier);

        //13.2.2025 userlog
            await _userLogsRepository.InsertAsync(new UserLogs()
            {
                UserID = userID,
                Email = useremail,
                Action = "Lấy báo cáo hệ thống",
                MA_CSKCB = maCSKCB,
                LogTime = DateTime.Now,
            });
            await _userLogsRepository.SaveChangesAsync();

        //Linhist changed 05/11/2024
        switch (fileType)
        {
            case 0:
                return await _reportHandle.BaoCao_TongHop(filterDownloadString);

            case 1:
                return await _reportHandle.BaoCao_TrungDichVu(filterDownloadString);

            case 2:
                return await _reportHandle.BaoCao_TrungThuocDV(filterDownloadString);

            case 3:
                return await _reportHandle.BaoCao_ChiDinhDV(filterDownloadString);

            case 4:
                return await _reportHandle.BaoCao_ThongkeLoi(filterDownloadString);

            case 5:
                return await _reportHandle.BaoCao_TrungMaMay(filterDownloadString);

            case 6:
                return await _reportHandle.BaoCao_ChamCong(filterDownloadString);
        }

        return null;

    }
}

public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding
    {
        get { return Encoding.UTF8; }
    }
}

internal static class ParseHelpers
{
    public static Stream ToStream(this string @this)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(@this);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public static T ParseXML<T>(this string @this) where T : class
    {
        var reader = XmlReader.Create(@this.Trim().ToStream(), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
        return new XmlSerializer(typeof(T)).Deserialize(reader) as T ?? throw new InvalidDataException("XML rỗng");
    }
}
