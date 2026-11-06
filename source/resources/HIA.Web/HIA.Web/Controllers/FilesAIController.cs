using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using HIA.Core.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.DataTable;
using HIA.Core.Models.Files;
using HIA.Core.Models.Files130;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.Categories;
using HIA.Services.Email;
using HIA.Services.Files;
using HIA.Services.HospitalManagement;
using HIA.Core.Models.Base;
using System.Security.Claims;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System.Net.Http;

namespace HIA.Web.Controllers
{
    [Authorize]
    public class FilesAIController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<Files130Controller> _logger;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IFilesService130 _filesService;
        private readonly IDMKhoaBVService _dmKhoaService;
        private readonly IDMErrorService130 _dmErrorService;
        private readonly IHospitalService _hospitalService;
        private readonly IBaseRepository<UserLogs> _userLogsRepository;

        

        public FilesAIController(
            IEmailService emailService,
            ILogger<Files130Controller> logger,
            ICompositeViewEngine viewEngine,
            IHttpContextAccessor httpContextAccessor,

            IFilesService130 filesService,
            IDMKhoaBVService dMKhoaBVService,
            IDMErrorService130 dMErrorService,
            IHospitalService hospitalService,
            IBaseRepository<UserLogs> userLogsRepository
        )
        {
            _emailService = emailService;
            _logger = logger;
            _viewEngine = viewEngine;
            _httpContextAccessor = httpContextAccessor;

            _filesService = filesService;
            _dmKhoaService = dMKhoaBVService;
            _dmErrorService = dMErrorService;
            _hospitalService = hospitalService;
            _userLogsRepository = userLogsRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            ViewBag.DepartmentList = (await _dmKhoaService.GetSelectionList()).Data;
            ViewBag.ErrorList = (await _dmErrorService.GetAlertErrorListByHospital()).Data;

            //13.2.2025 userlog
            var maCSKCB = _httpContextAccessor.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            var userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var useremail = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //if (maCSKCB == "25001" || maCSKCB== "91009")
            //{
                //var ipArray = Dns.GetHostAddresses(Dns.GetHostName())
                //                .Where(x => x.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork))
                //                .Select(x => Convert.ToString(x))
                //                .ToArray();
                await _userLogsRepository.InsertAsync(new UserLogs()
                {
                    UserID = userID,
                    Email = useremail,
                    Action = "Lấy báo cáo hệ thống",
                    MA_CSKCB = maCSKCB,
                    //UrlAccess = ipArray.Length > 0 ? string.Join(",", ipArray) : "",
                    LogTime = DateTime.Now,
                });
                await _userLogsRepository.SaveChangesAsync();
            //}

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetXML01(DataTableAjaxPostModel request, List<string> filterDateIns, List<string> filterDateOuts, List<string> filterDateCreates, List<string> filterDatePaids, List<string> departments, List<string> filterErrors, List<int?> timeRanges, string filterKCB, int? filterStatus, string filterHospital)
        {
            var paniationRequest = new PaginationRequestModel(request);
            var paginationRequest = new FilesPaginationRequest(paniationRequest, filterDateIns, filterDateOuts, filterDateCreates, filterDatePaids, departments, filterErrors, timeRanges, filterKCB, filterStatus, filterHospital);
            var res = await _filesService.GetXml01Items(paginationRequest);

            var filterDownloadModel = new FilterDownloadModel
            {
                SearchKey = paniationRequest.SearchKey,
                OrderByColumn = paniationRequest.OrderByColumn,
                OrderType = paniationRequest.OrderType,
                FromDateIn = paginationRequest.FromDateIn,
                ToDateIn = paginationRequest.ToDateIn,
                FromDateOut = paginationRequest.FromDateOut,
                ToDateOut = paginationRequest.ToDateOut,
                FromDateCreate = paginationRequest.FromDateCreate,
                ToDateCreate = paginationRequest.ToDateCreate,
                FromDatePaid = paginationRequest.FromDatePaid,
                ToDatePaid = paginationRequest.ToDatePaid,
                TypeKCB = paginationRequest.TypeKCB,
                Status = paginationRequest.Status,
                Departments = departments,
                Errors = paginationRequest.Errors,
                HospitalId = paginationRequest.HospitalId,
                MaCSKCB = filterHospital,
            };
            TempData["FILTER_DOWNLOAD_MODEL"] = JsonConvert.SerializeObject(filterDownloadModel);

            var isSuccess = res.Code == HttpStatusCode.OK;
            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
                data = isSuccess ? res.Data.Items : new List<XML01Info>(),
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML02(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml02Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML02Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML03(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml03Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML03Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML04(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml04Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML04Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML05(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml05Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML05Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML06(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml06Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML06Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML07(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml07Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML07Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML08(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml08Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML08Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML09(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml09Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML09Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML10(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml10Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML10Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML11(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml11Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML11Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML13(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml13Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML13Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML14(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml14Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML14Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXML15(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml15Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML15Info>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetXmlError(int xml01ID)
        {
            var res = await _filesService.GetXmlErrorItems(xml01ID);
            return Ok(res.Data.ToArray());
        }

        [HttpPost]
        public async Task<IActionResult> GetHistory(DataTableAjaxPostModel request, int? xml01ID)
        {
            var paniationRequest = new PaginationRequestModel(request);
            if (xml01ID != null && xml01ID > 0)
            {
                var result = await _filesService.GetXml03Items(paniationRequest, xml01ID.Value);
                if (result.Code == HttpStatusCode.OK)
                {
                    return Ok(new
                    {
                        draw = paniationRequest.Draw,
                        recordsFiltered = result.Data.TotalItems,
                        recordsTotal = result.Data.TotalItems,
                        data = result.Data.Items,
                    });
                }
            }

            return Ok(new
            {
                draw = paniationRequest.Draw,
                recordsFiltered = 0,
                recordsTotal = 0,
                data = new List<XML03Info>()
            });
        }

        [HttpGet]
        public IActionResult Download(string fileGuid, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileGuid) || TempData[fileGuid] == null)
            {
                return new EmptyResult();
            }

            var data = JsonConvert.DeserializeObject<byte[]>(TempData[fileGuid].ToString());
            string contentType = $"application/{FileTypeConstant.XLSX_TYPE}";
            return File(data, contentType, fileName);
        }

        [HttpGet]
        public IActionResult DownloadHoso(string fileName, string date)
        {
            var maCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? "";
            var path = $"{Uti.GenerateFilePath130(maCSKCB, DateTime.Parse(date))}\\{fileName}";
            return PhysicalFile(path, FileTypeConstant.XML_TYPE, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> RecheckHoso(long? xml01ID)
        {
            if (xml01ID == null)
            {
                return BadRequest();
            }
            
            var res = await _filesService.RecheckHoso(xml01ID.Value);
            return Ok(new { res.Data });
        }

        public async Task<IActionResult> RecheckAll(List<string> filterDateIns, List<string> filterDateOuts, List<string> filterDateCreates, List<string> filterDatePaids, List<string> departments, List<string> filterErrors, List<int?> timeRanges, string filterKCB, int? filterStatus, string filterHospital)
        {
            var paginationRequest = new FilesPaginationRequest(new PaginationRequestModel(), filterDateIns, filterDateOuts, filterDateCreates, filterDatePaids, departments, filterErrors, timeRanges, filterKCB, filterStatus, filterHospital);
            var res = await _filesService.GetXml01Items(paginationRequest);
            if (res == null || res.Data == null)
            {
                return BadRequest();
            }
            var ids = res.Data.Items.Select(x => x.xml01id).ToArray();

            var cnt = await _filesService.RecheckHoso(ids);
            return Ok(new { cnt.Data, recordsTotal = ids.Length });
        }

        [HttpGet]
        public IActionResult DownloadClientInstall()
        {
            var path = $"C:\\Install\\MDinsight_Adaptor_4750_v12.msi";
            return PhysicalFile(path, "application/rtf", "MDinsight_Adaptor_4750.msi");
        }

        [HttpGet]
        public IActionResult DownloadProInstall()
        {
            var path = $"C:\\Install\\MTGAdaptorClientSetup_271124.msi";
            return PhysicalFile(path, "application/rtf", "MTGAdaptorClientSetup_271124.msi");
        }

        private string ConvertViewToString(ControllerContext controllerContext, PartialViewResult pvr, ICompositeViewEngine _viewEngine)
        {
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = _viewEngine.FindView(controllerContext, pvr.ViewName, false);
                ViewContext viewContext = new ViewContext(controllerContext, vResult.View, pvr.ViewData, pvr.TempData, writer, new HtmlHelperOptions());

                vResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData1()
        {
            //Linhist added 10/11/22
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Tong Hop] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData1));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 0);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"Hoso-Tong-hop-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData2()
        {
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Trung Gio] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData2));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 1);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"Hoso-Trung-gio-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData3()
        {
            //Linhist added 10/11/22
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Thuoc-DV-VT] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData3));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 2);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"Hoso-Thuoc-DV-VT-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData4()
        {
            //Linhist added 10/11/22
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Chi Dinh] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData4));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 3);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"Hoso-Chidinh-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData5()
        {
            //Linhist added 10/11/22
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Thong ke Loi] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData5));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 4);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"Thongke-Loi-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData6()
        {
            //Linhist added 10/11/22
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Trung XMl4] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData5));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 5);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"TrungMaMay-Xml4-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        [HttpGet]
        public async Task<IActionResult> GetDownloadData7()
        {
            //Linhist added 10/11/22
            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Cham cong] vao ({DateTime.Now:hh:mm:ss}).", nameof(Files130Controller.GetDownloadData5));

            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
            if (filterString == null)
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

            var res = await _filesService.DownloadBaoCao(filterString?.ToString() ?? "", 6);
            if (res?.Data == null)
            {
                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
            }
            string fileName = $"Chamcong-{DateTime.Now.Ticks}.xlsx";
            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            #region send email with attachment
            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            }
            #endregion

            var guid = Guid.NewGuid().ToString();
            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
            TempData[guid] = JsonConvert.SerializeObject(res.Data);
            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
        }

        public async Task<IActionResult> RemoveDublicate(List<string> filterDatePaids)
        {
            try
            {
                var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                if (string.IsNullOrEmpty(currentMaCSKCB)) return BadRequest();

                //DateTime tuNgay = DateTime.Now.Date.AddDays(-30);
                //DateTime denNgay = DateTime.Now.Date.AddDays(1);
                //if (filterDatePaids?.Count > 0)
                //{
                //    DateTime.TryParse($"{filterDatePaids[0]}", out tuNgay);
                //    tuNgay.AddDays(-1);
                //}
                //if (filterDatePaids?.Count > 1)
                //{
                //    DateTime.TryParse($"{filterDatePaids[1]}", out denNgay);
                //}
                int res = await _hospitalService.RemoveDublicated130(currentMaCSKCB);

                return Ok(new
                {
                    recordsTotal = res,
                });
            }
            catch //(Exception ex)
            { }

            return Ok(new
            {
                recordsTotal = 0,
            });
        }
    }
}
