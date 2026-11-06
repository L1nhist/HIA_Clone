using HIA.Core.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.Categories;
using HIA.Services.Email;
using HIA.Services.Files;
using HIA.Services.HospitalManagement;
using HIA.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize]
    public class FileDownload130Controller : Controller
    {
        private readonly IFilesService130 _filesService;
        private readonly IDMKhoaBVService _dMKhoaBVService;
        private readonly IEmailService _emailService;
        private readonly IDMErrorService130 _dMErrorService;

        //Linhist added 16/06/22
        private readonly IHospitalService _hospitalService;

        private readonly IUserManagementService _UserManagementService;
        private readonly IHttpContextAccessor _IHttpContextAccessor;

        public FileDownload130Controller(
            IFilesService130 filesService,
            IDMKhoaBVService dMKhoaBVService,
            IEmailService emailService,
            IDMErrorService130 dMErrorService,
            IHospitalService hospitalService,
             IUserManagementService UserManagementService,
             IHttpContextAccessor IHttpContextAccessor
        )
        {
            _filesService = filesService;
            _dMKhoaBVService = dMKhoaBVService;
            _emailService = emailService;
            _dMErrorService = dMErrorService;
            _UserManagementService = UserManagementService;
            _IHttpContextAccessor = IHttpContextAccessor;

            //Linhist added 16/06/22
            _hospitalService = hospitalService;
        }

        public async Task<IActionResult> Index()
        {
            //Linhist removed 20/03/22 to push up perfomance
            //var quatityErrors = (await _filesService.GetXml1ErrorItems()).Data;
            //ViewBag.CreatedDateList = (await _filesService.GetSelectionCreatedDateList()).Data;
            var userLogIn = await _UserManagementService.GetItemById(_IHttpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

            if (!userLogIn.Data.IsActivated)
            {
                return RedirectToAction("SignOutAction", "Auth");
            }

            ViewBag.DepartmentList = (await _dMKhoaBVService.GetSelectionList()).Data;
            ViewBag.ErrorList = (await _dMErrorService.GetSelectionErrorListByHospital()).Data;

            if (User.FindFirst(ClaimTypes.Role)?.Value?.Contains(RoleConstant.MASTER) == true)
            {
                ViewBag.Hospitals = (await _hospitalService.GetActiveHospitals()).Data;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<string> dateIns, List<string> dateOuts, List<string> dateCreates, List<string> datePaids, List<string> departments, List<string> listErrors, List<int?> timeRanges, string loaiKCB, int? status, string hospital, int? fileType)
        {
            var paniationRequest = new PaginationRequestModel();
            departments = departments?.Where(d => !string.IsNullOrEmpty(d)).ToList();
            listErrors = listErrors?.Where(e => !string.IsNullOrEmpty(e)).ToList();
            var paginationRequest = new FilesPaginationRequest(paniationRequest, dateIns, dateOuts, dateCreates, datePaids, departments, listErrors, timeRanges, loaiKCB, status, "");
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
                MaCSKCB = hospital,
            };

            var res = await _filesService.DownloadBaoCao(JsonConvert.SerializeObject(filterDownloadModel), fileType);

            string fileName = $"{DateTime.Now.Ticks}.xlsx";
            switch (fileType)
            {
                case 1: fileName = $"Hoso-Trung-gio-{fileName}"; break;
                case 2: fileName = $"Hoso-Thuoc-DV-VT-{fileName}"; break;
                case 3: fileName = $"Hoso-Chidinh-{fileName}"; break;
                case 4: fileName = $"Thongke-Loi-{fileName}"; break;
                default: fileType = 0; fileName = $"Hoso-Tong-hop-{fileName}"; break;
            }

            var fileContent = new FileContentModel
            {
                Name = fileName,
                ContentType = FileTypeConstant.XLSX_TYPE,
                ByteArray = res.Data
            };

            //Linhist removed 100826
            //var toEmail = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (!string.IsNullOrWhiteSpace(toEmail))
            //{
            //    _emailService.SendEmailAttachmentWithBytes(toEmail, fileContent);
            //}

            return File(fileContent.ByteArray, $"application/{fileContent.ContentType}", fileContent.Name);
        }

        [HttpGet]
        public IActionResult DownloadXml1(string fileName, string date)
        {
            //Linhist changed 26/04/22 thay doi ten thu muc goc XML thanh MA_CSKCB
            //var hospitalName = HttpContext.User.FindFirst(ClaimKey.HOSPITAL_NAME)?.Value ?? "";
            var hospitalName = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? "";
            var path = $"{Uti.GenerateFilePath(hospitalName, DateTime.Parse(date))}\\{fileName}";
            return PhysicalFile(path, FileTypeConstant.XML_TYPE, fileName);
        }

        [HttpGet]
        public IActionResult DownloadClientInstall()
        {
            var path = $"C:\\Install\\EgdbhClient.msi";
            return PhysicalFile(path, "application/rtf", "EgdbhClient.msi");
        }
    }
}