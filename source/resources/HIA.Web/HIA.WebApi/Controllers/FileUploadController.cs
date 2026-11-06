//using HIA.Core.Base;
//using HIA.Core.Models.Base;
//using HIA.Core.Models.FileUploads;
//using HIA.Services.Auth;
//using HIA.Services.Files;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;

//namespace HIA.WebApi.Controllers
//{
//    [Authorize]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class FileUploadController : ControllerBase
//    {
//        private readonly IAuthService _authService;
//        private readonly TokenHandler _tokenHandle;
//        private readonly IFilesService _FilesService;


//        public FileUploadController(IAuthService authService, TokenHandler tokenHandle, IFilesService FilesService)
//        {
//            _authService = authService;
//            _tokenHandle = tokenHandle;
//            _FilesService = FilesService;
//        }
//        [HttpGet]
//        [Route("HealthCheck")]
//        public IActionResult HealthCheck()
//        {

//            return Ok("test");
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        [Route("UploadFile")]
//        public async Task<IActionResult> UploadFile([FromBody] FileUploading request)
//        {
//            //var res = new ResponseModel<string>();
//            if (!ModelState.IsValid)
//            {
//                //res.Code = HttpStatusCode.BadRequest;
//                //res.Message = string.Join(" | ", ModelState.Values
//                //                .SelectMany(v => v.Errors)
//                //                .Select(e => e.ErrorMessage));
//                return NotFound();
//            }
//            //var khoi = await _authService.CheckToken2(request.Token);
//            var res = _tokenHandle.IsTokenValid(request.Token);


//            //var tokenRes = await _tokenHandle.CreateAndSignin(res.Data);
//            //byte[] byteArray = System.Text.ASCIIEncoding.ASCII.GetBytes(request.FileBytes);

//            byte[] byteArray = System.Convert.FromBase64String(request.FileBytes);

//            using (var ms = new MemoryStream(byteArray))
//            {
//                IFormFile fromFile = new FormFile(ms, 0, ms.Length,
//                    "alo",
//                    "alo.xml"
//                );
//                List<IFormFile> FormFiles = new List<IFormFile>();
//                FormFiles.Add(fromFile);
//                FileUpload fileUp = new FileUpload() { FormFiles = FormFiles };
//                var uploadEdfile = await _FilesService.FileUploadAPI(fileUp, res.Data.HospitalId, res.Data.HospitalName, res.Data.Id, res.Data.MA_CSKCB);
//            }

            
//            //using (var fs = new FileStream("G:\\New folder\\alo.txt", FileMode.Create))
//            //{
//            //    fs.Write(byteArray, 0, byteArray.Length);

//            //}

//            return Ok(res);
//        }
//    }

//    public class FileUploading
//    {
//        public string Token { get; set; }
//        public string FileName { get; set; }
//        public string FileBytes { get; set; }

//    }
//}
