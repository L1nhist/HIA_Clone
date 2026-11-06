//using HIA.Core.Base;
//using HIA.Core.Models.Base;
//using HIA.Core.Models.Categories;
//using HIA.Core.Models.DataTable;
//using HIA.Core.Models.Files;
//using HIA.Core.Models.Pagination;
//using HIA.Core.Models.Token;
//using HIA.Services.Categories;
//using HIA.Services.Email;
//using HIA.Services.Files;
//using HIA.Services.HosoManagement;
//using HIA.Services.HospitalManagement;
//using HIA.Services.UserManagement;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc.ViewEngines;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Security.Claims;
//using System.Threading;
//using System.Threading.Tasks;

//namespace HIA.Web.Controllers
//{
//    [Authorize]
//    public class FilesController : Controller
//    {
//        private readonly IFilesService _filesService;
//        private readonly IFilesService130 _filesService130;
//        private readonly IDMKhoaBVService _dMKhoaBVService;
//        private readonly IEmailService _emailService;
//        private readonly IDMErrorService _dMErrorService;
//        private ICompositeViewEngine _viewEngine;

//        //Linhist added 16/06/22
//        private readonly IHospitalService _hospitalService;

//        private readonly IUserManagementService _UserManagementService;
//        private readonly IHttpContextAccessor _IHttpContextAccessor;

//        //Linhist added 10/11/22
//        private readonly ILogger<FilesController> _logger;

//        public FilesController(
//            IFilesService130 filesService130,
//            IFilesService filesService,
//            IDMKhoaBVService dMKhoaBVService,
//            IEmailService emailService,
//            IDMErrorService dMErrorService,
//            IHospitalService hospitalService,
//            ICompositeViewEngine viewEngine,
//             IUserManagementService UserManagementService,
//             IHttpContextAccessor IHttpContextAccessor,
//             ILogger<FilesController> logger
//            )
//        {
//            _filesService130 = filesService130;
//            _filesService = filesService;
//            _dMKhoaBVService = dMKhoaBVService;
//            _emailService = emailService;
//            _dMErrorService = dMErrorService;
//            _viewEngine = viewEngine;
//            _UserManagementService = UserManagementService;
//            _IHttpContextAccessor = IHttpContextAccessor;

//            //Linhist added 16/06/22
//            _hospitalService = hospitalService;

//            //Linhist added 10/11/22
//            _logger = logger;
//        }

//        public async Task<IActionResult> Index()
//        {
//            //Linhist removed 20/03/22 to push up perfomance
//            //var quatityErrors = (await _filesService.GetXml1ErrorItems()).Data;
//            //ViewBag.CreatedDateList = (await _filesService.GetSelectionCreatedDateList()).Data;
//            var userLogIn = await _UserManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

//            if (!userLogIn.Data.IsActivated)
//            {
//                return RedirectToAction("SignOutAction", "Auth");
//            }

//            ViewBag.DepartmentList = (await _dMKhoaBVService.GetSelectionList()).Data;
//            //Linhist removed 01/04/23
//            //var errList = (await _dMErrorService.GetSelectionErrorListByHospital()).Data;

//            //Linhist removed 20/03/22 to push up perfomance
//            //foreach (var item in quatityErrors)
//            //{
//            //    var errItem = errList.FirstOrDefault(x => x.Value.Equals(item.XeErrorCode.ToString()));
//            //    if (errItem == null) continue;
                
//            //    errItem.Quantity = item.Total;
//            //}
//            //errList = errList.OrderByDescending(x => x.Quantity);

//            //Linhist removed 01/04/23
//            //ViewBag.ErrorList = errList;

//            //Linhist added 16/06/22
//            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;

//            //Linhist added 01/04/23
//            var errList = (await _dMErrorService.GetAlertErrorListByHospital()).Data;
//            ViewBag.AlertList = errList.Where(x => x.IsAlarm).Select(x => x.ErrorCode).ToArray();
//            ViewBag.ErrorList = errList.Select(x => new SelectErrorsItems
//            {
//                Text = x.ErrorContent,
//                Value = x.ErrorCode.Value.ToString(),
//            }).ToList();

//            return View();
//        }

//        //Linhist added 27/04/23
//        public async Task<IActionResult> Counting()
//        {
//            var userLogIn = await _UserManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

//            if (!userLogIn.Data.IsActivated)
//            {
//                return RedirectToAction("SignOutAction", "Auth");
//            }

//            ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;

//            //Linhist added 01/04/23
//            var errList = (await _dMErrorService.GetAlertErrorListByHospital()).Data;
//            ViewBag.AlertList = errList.Where(x => x.IsAlarm).Select(x => x.ErrorCode).ToArray();

//            return View();
//        }


//        #region Linhist removed 10/03/22
//        //[HttpPost]
//        //Linhist bo doan nay de thay the bang doan ben duoi, co tong hop ngay thang
//        //public async Task<IActionResult> GetItems(DataTableAjaxPostModel request, List<string> dateFilter, List<string> departments, List<string> errFilter, string filterFromDate, string filterToDate)
//        //{
//        //    var paniationRequest = new PaginationRequestModel(request);
//        //    var paginationRequest = new FilesPaginationRequest(paniationRequest, dateFilter, departments, errFilter, filterFromDate, filterToDate);
//        //    var res = await _filesService.GetXml1Items(paginationRequest);

//        //    var filterDownloadModel = new FilterDownloadModel
//        //    {
//        //        SearchKey = paniationRequest.SearchKey,
//        //        OrderByColumn = paniationRequest.OrderByColumn,
//        //        OrderType = paniationRequest.OrderType,
//        //        DateFilters = dateFilter,
//        //        Departments = departments,
//        //        ErrFilter = errFilter,
//        //        FilterFromDate = filterFromDate,
//        //        FilterToDate = filterToDate
//        //    };
//        #endregion

//        //Linhist changed 19/04/22 them datePaid & typeKCB
//        //Linhist changed 16/06/22 them string hospitalId
//        [HttpPost]
//        public async Task<IActionResult> GetItems(DataTableAjaxPostModel request, List<string> filterDateIns, List<string> filterDateOuts, List<string> filterDateCreates, List<string> filterDatePaids, List<string> departments, List<string> filterErrors, string? filterKCB, int? filterStatus, string filterHospital)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            //Linhist changed 27/02/23
//            //var paginationRequest = new FilesPaginationRequest(paniationRequest, filterDateIns, filterDateOuts, filterDateCreates, filterDatePaids, departments, filterErrors, filterKCB, filterStatus, filterHospital);
//            var paginationRequest = new FilesPaginationRequest(paniationRequest, filterDateIns, filterDateOuts, filterDateCreates, filterDatePaids, departments, filterErrors, filterKCB, filterStatus, filterHospital);
//            var res = await _filesService.GetXml1Items(paginationRequest);
//            //var res = await _filesService130.GetXml1Items(paginationRequest);

//            var filterDownloadModel = new FilterDownloadModel
//            {
//                SearchKey = paniationRequest.SearchKey,
//                OrderByColumn = paniationRequest.OrderByColumn,
//                OrderType = paniationRequest.OrderType,
//                FromDateIn = paginationRequest.FromDateIn,
//                ToDateIn = paginationRequest.ToDateIn,
//                FromDateOut = paginationRequest.FromDateOut,
//                ToDateOut = paginationRequest.ToDateOut,
//                FromDateCreate = paginationRequest.FromDateCreate,
//                ToDateCreate = paginationRequest.ToDateCreate,
//                FromDatePaid = paginationRequest.FromDatePaid,
//                ToDatePaid = paginationRequest.ToDatePaid,
//                TypeKCB = paginationRequest.TypeKCB,
//                Status = paginationRequest.Status,
//                Departments = departments,
//                Errors = paginationRequest.Errors,
//                HospitalId = paginationRequest.HospitalId,

//                //Linhist added 27/02/23
//                MaCSKCB = filterHospital,
//            };
//            TempData["FILTER_DOWNLOAD_MODEL"] = JsonConvert.SerializeObject(filterDownloadModel);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML1InfoModel>(),
//            });
//        }
//        [HttpPost]
//        public async Task<IActionResult> GetCounterItems(DataTableAjaxPostModel request, List<string> dateRange, int? dateType, string? kcbType, List<string> errorList, string hospitalCode)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            var paginationRequest = new FilesPaginationRequest(paniationRequest, null, null, dateType == 0 ? dateRange : null, dateType == 1 ? dateRange : null, null, errorList, kcbType, 2, hospitalCode);
//            var res = await _filesService.GetXml1Items(paginationRequest);

//            var filterDownloadModel = new FilterDownloadModel
//            {
//                SearchKey = paniationRequest.SearchKey,
//                OrderByColumn = paniationRequest.OrderByColumn,
//                OrderType = paniationRequest.OrderType,
//                FromDateIn = paginationRequest.FromDateIn,
//                ToDateIn = paginationRequest.ToDateIn,
//                FromDateOut = paginationRequest.FromDateOut,
//                ToDateOut = paginationRequest.ToDateOut,
//                FromDateCreate = paginationRequest.FromDateCreate,
//                ToDateCreate = paginationRequest.ToDateCreate,
//                FromDatePaid = paginationRequest.FromDatePaid,
//                ToDatePaid = paginationRequest.ToDatePaid,
//                TypeKCB = paginationRequest.TypeKCB,
//                Status = paginationRequest.Status,
//                Departments = null,
//                Errors = paginationRequest.Errors,
//                HospitalId = paginationRequest.HospitalId,
//                MaCSKCB = hospitalCode,
//            };
//            TempData["FILTER_DOWNLOAD_MODEL"] = JsonConvert.SerializeObject(filterDownloadModel);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML1InfoModel>(),
//            });
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetCounterErrors(DataTableAjaxPostModel request, List<string> dateRange, int? dateType, string? kcbType, string hospitalCode)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            var paginationRequest = new FilesPaginationRequest(paniationRequest, null, null, dateType == 0 ? dateRange : null, dateType == 1 ? dateRange : null, null, null, kcbType, 2, hospitalCode);
//            var res = await _filesService.GetXml1ErrorsCounter(paginationRequest);

//            return Ok(res);
//        }

//        [HttpPost]
//        public IActionResult TransformDataToPartialXml1([FromBody] XML1InfoModalObject request)
//        {
//            var res = _filesService.TransformXml1ToModal(request);
//            return PartialView("_xml1Tab", res.Data);
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetItemsXml2(DataTableAjaxPostModel request, int xml1Id)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            if (xml1Id == 0) return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = 0,
//                recordsTotal = 0,
//                data = new List<string>()
//            });

//            var res = await _filesService.GetXml2Items(paniationRequest, xml1Id);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML2InfoModel>(),
//            });
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetItemsXml3(DataTableAjaxPostModel request, int xml1Id)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            if (xml1Id == 0) return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = 0,
//                recordsTotal = 0,
//                data = new List<string>()
//            });

//            var res = await _filesService.GetXml3Items(paniationRequest, xml1Id);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML3InfoModel>(),
//            });
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetItemsXml4(DataTableAjaxPostModel request, int xml1Id)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            if (xml1Id == 0) return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = 0,
//                recordsTotal = 0,
//                data = new List<string>()
//            });

//            var res = await _filesService.GetXml4Items(paniationRequest, xml1Id);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML4InfoModel>(),
//            });
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetItemsXml5(DataTableAjaxPostModel request, int xml1Id)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            if (xml1Id == 0) return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = 0,
//                recordsTotal = 0,
//                data = new List<string>()
//            });

//            var res = await _filesService.GetXml5Items(paniationRequest, xml1Id);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML5InfoModel>(),
//            });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetItemsXml1Error(int xml1Id)
//        {
//            var res = await _filesService.GetXml1ErrorItems(xml1Id);
//            PartialViewResult partialViewResult = PartialView("_errorTab", res.Data);
//            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
//            return Ok(new { PartialView = viewContent, Data = res.Data });
//        }

//        [HttpGet]
//        public async Task<IActionResult> RecheckXml1Error(int xml1Id)
//        {
//            await _filesService.RecheckXML1(xml1Id);
//            var res = await _filesService.GetXml1ErrorItems(xml1Id);
//            PartialViewResult partialViewResult = PartialView("_errorTab", res.Data);
//            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
//            return Ok(new { PartialView = viewContent, Data = res.Data });
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetItemsHelthCareHistory(DataTableAjaxPostModel request, int xml1Id)
//        {
//            var paniationRequest = new PaginationRequestModel(request);
//            if (xml1Id == 0) return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = 0,
//                recordsTotal = 0,
//                data = new List<string>()
//            });

//            var res = await _filesService.GetXml3Items(paniationRequest, xml1Id);

//            var isSuccess = res.Code == HttpStatusCode.OK;
//            return Ok(new
//            {
//                draw = paniationRequest.Draw,
//                recordsFiltered = isSuccess ? res.Data.TotalItems : 0,
//                recordsTotal = isSuccess ? res.Data.TotalItems : 0,
//                data = isSuccess ? res.Data.Items : new List<XML3InfoModel>(),
//            });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData1()
//        {
//            //Linhist added 10/11/22
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Tong Hop] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData1));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 0);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"Hoso-Tong-hop-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData2()
//        {
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Trung Gio] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData2));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 1);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"Hoso-Trung-gio-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData3()
//        {
//            //Linhist added 10/11/22
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Thuoc-DV-VT] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData3));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 2);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"Hoso-Thuoc-DV-VT-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }

//        #region Linhist added 09/06/22
//        //Linhist changed 16/06/22 them string hospitalId
//        public async Task<IActionResult> SetItemsRecheck(List<string> filterDateIns, List<string> filterDateOuts, List<string> filterDateCreates, List<string> filterDatePaids, List<string> departments, List<string> filterErrors, string? filterKCB, int? filterStatus, string filterHospital)
//        {
//            var paginationRequest = new FilesPaginationRequest(new PaginationRequestModel(), filterDateIns, filterDateOuts, filterDateCreates, filterDatePaids, departments, filterErrors, filterKCB, filterStatus, filterHospital);
//            int res = await _filesService.SetXml1Recheck(paginationRequest);

//            return Ok(new
//            {
//                recordsTotal = res,
//            });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData4()
//        {
//            //Linhist added 10/11/22
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Chi Dinh] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData4));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 3);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"Hoso-Chidinh-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }
//        #endregion

//        #region Linhist added 09/10/22
//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData5()
//        {
//            //Linhist added 10/11/22
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Thong ke Loi] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData5));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 4);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"Thongke-Loi-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }
//        #endregion

//        #region Linhist added 27/07/23
//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData6()
//        {
//            //Linhist added 10/11/22
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Trung XMl4] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData5));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 5);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"TrungMaMay-Xml4-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }
//        #endregion

//        #region Linhist added 19/10/23
//        [HttpGet]
//        public async Task<IActionResult> GetDownloadData7()
//        {
//            //Linhist added 10/11/22
//            var currentUser = HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
//            var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            Uti.HandleCronJobLog(_logger, $"User [{currentUser}] cua vien {currentMaCSKCB} lay [Bao Cao Trung XMl4] vao ({DateTime.Now:hh:mm:ss}).", nameof(FilesController.GetDownloadData5));

//            var filterString = TempData["FILTER_DOWNLOAD_MODEL"];
//            if (filterString == null)
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });

//            var res = await _filesService.DownloadExcelFile(filterString?.ToString() ?? "", 6);
//            if (res?.Data == null)
//            {
//                TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//                return Ok(new ResponseModel<string> { Code = HttpStatusCode.BadRequest });
//            }
//            string fileName = $"Chamcong-{DateTime.Now.Ticks}.xlsx";
//            var fileContent = new FileContentModel
//            {
//                Name = fileName,
//                ContentType = FileTypeConstant.XLSX_TYPE,
//                ByteArray = res.Data
//            };

//            #region send email with attachment
//            var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!string.IsNullOrWhiteSpace(toEmail))
//            {
//                _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
//            }
//            #endregion

//            var guid = Guid.NewGuid().ToString();
//            TempData["FILTER_DOWNLOAD_MODEL"] = filterString;
//            TempData[guid] = JsonConvert.SerializeObject(res.Data);
//            return Ok(new ResponseModel<object> { Code = HttpStatusCode.OK, Data = new { FileGuid = guid, FileName = fileName } });
//        }
//        #endregion

//        [HttpGet]
//        public IActionResult Download(string fileGuid, string fileName)
//        {
//            if (string.IsNullOrWhiteSpace(fileGuid) || TempData[fileGuid] == null)
//            {
//                return new EmptyResult();
//            }

//            var data = JsonConvert.DeserializeObject<byte[]>(TempData[fileGuid].ToString());
//            string contentType = $"application/{FileTypeConstant.XLSX_TYPE}";
//            return File(data, contentType, fileName);
//        }
//        [HttpGet]
//        public IActionResult DownloadXml1(string fileName, string date)
//        {
//            //Linhist changed 26/04/22 thay doi ten thu muc goc XML thanh MA_CSKCB
//            //var hospitalName = HttpContext.User.FindFirst(ClaimKey.HOSPITAL_NAME)?.Value ?? "";
//            var hospitalName = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? "";
//            var path = $"{Uti.GenerateFilePath(hospitalName, DateTime.Parse(date))}\\{fileName}";
//            return PhysicalFile(path, FileTypeConstant.XML_TYPE, fileName);
//        }
//        [HttpGet]
//        public IActionResult DownloadClientInstall()
//        {
//            var path = $"C:\\Install\\MDinsight_Adaptor_4210.msi";
//            return PhysicalFile(path, "application/rtf", "MDinsight_Adaptor_4210.msi");
//        }
        

//        private string ConvertViewToString(ControllerContext controllerContext, PartialViewResult pvr, ICompositeViewEngine _viewEngine)
//        {
//            using (StringWriter writer = new StringWriter())
//            {
//                ViewEngineResult vResult = _viewEngine.FindView(controllerContext, pvr.ViewName, false);
//                ViewContext viewContext = new ViewContext(controllerContext, vResult.View, pvr.ViewData, pvr.TempData, writer, new HtmlHelperOptions());

//                vResult.View.RenderAsync(viewContext);

//                return writer.GetStringBuilder().ToString();
//            }
//        }

//        #region Linhist added 09/06/22
//        public async Task<IActionResult> RemoveDublicate(List<string> filterDatePaids)
//        {
//            try
//            {
//                var currentMaCSKCB = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//                if (string.IsNullOrEmpty(currentMaCSKCB)) return BadRequest();

//                DateTime tuNgay = DateTime.Now.Date.AddDays(-30);
//                DateTime denNgay = DateTime.Now.Date.AddDays(1);
//                if (filterDatePaids?.Count > 0)
//                {
//                    DateTime.TryParse($"{filterDatePaids[0]}", out tuNgay);
//                    tuNgay.AddDays(-1);
//                }
//                if (filterDatePaids?.Count > 1)
//                {
//                    DateTime.TryParse($"{filterDatePaids[1]}", out denNgay);
//                }
//                int res = await _hospitalService.RemoveDublicated(currentMaCSKCB, tuNgay, denNgay);

//                return Ok(new
//                {
//                    recordsTotal = res,
//                });
//            }
//            catch (Exception ex)
//            { }

//            return Ok(new
//            {
//                recordsTotal = 0,
//            });
//        }
//        #endregion
//    }
//}
