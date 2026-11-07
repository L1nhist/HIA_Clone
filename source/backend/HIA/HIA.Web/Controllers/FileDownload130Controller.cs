using HIA.Services.HospitalManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HIA.Core.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.Services.Categories;
using HIA.Services.Files;
using HIA.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIA.Web.Controllers
{
    [Authorize]
    public class FileDownload130Controller : Controller
    {
        private readonly IFilesService130 _filesService;
        private readonly IDMKhoaBVService _dMKhoaBVService;
        private readonly IDMErrorService130 _dMErrorService;

        //Linhist added 16/06/22
        private readonly IHospitalService _hospitalService;

        private readonly IUserManagementService _UserManagementService;

        public FileDownload130Controller(
            IFilesService130 filesService,
            IDMKhoaBVService dMKhoaBVService,
            IDMErrorService130 dMErrorService,
            IHospitalService hospitalService,
            IUserManagementService UserManagementService
        )
        {
            _filesService = filesService;
            _dMKhoaBVService = dMKhoaBVService;
            _dMErrorService = dMErrorService;
            _UserManagementService = UserManagementService;

            //Linhist added 16/06/22
            _hospitalService = hospitalService;
        }

        public async Task<IActionResult> Index()
        {
            var userLogIn = await _UserManagementService.GetItemById(GetClaimValue(ClaimTypes.Sid));

            if (userLogIn.Data?.IsActivated != true)
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
        public async Task<IActionResult> Index(List<string> dateIns, List<string> dateOuts, List<string> dateCreates, List<string> datePaids, List<string>? departments, List<string>? listErrors, List<int?> timeRanges, string loaiKCB, int? status, string hospital, int? fileType)
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

            var res = await _filesService.DownloadBaoCao(JsonDefaults.Serialize(filterDownloadModel), fileType);
            if (res?.Data == null)
            {
                return BadRequest();
            }

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

            return File(fileContent.ByteArray, $"application/{fileContent.ContentType}", fileContent.Name);
        }

        [HttpGet]
        public IActionResult DownloadXml1(string fileName, string date)
        {
            //Linhist changed 26/04/22 thay doi ten thu muc goc XML thanh MA_CSKCB
            var hospitalName = HttpContext.User.FindFirst(ClaimKey.MA_CSKCB)?.Value ?? "";
            if (!DateTime.TryParse(date, out var dteDate)) return BadRequest();

            var path = SafeFilePath.Resolve(Uti.GenerateFilePath(hospitalName, dteDate), fileName);
            if (path == null) return BadRequest();
            if (!System.IO.File.Exists(path)) return NotFound();

            return PhysicalFile(path, FileTypeConstant.XML_TYPE, fileName);
        }

        [HttpGet]
        public IActionResult DownloadClientInstall()
        {
            var path = $"C:\\Install\\EgdbhClient.msi";
            return PhysicalFile(path, "application/rtf", "EgdbhClient.msi");
        }

        private string GetClaimValue(string claimType) =>
            User.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Thiếu claim {claimType}.");
    }
}
