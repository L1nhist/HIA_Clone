

using HIA.Core.Models.FileUploads;
using HIA.Services.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
namespace HIA.Web.Controllers
{
    [Authorize]
    public class FileUpload130Controller : Controller
    {
        private readonly IFilesService130 _FilesService130;

        public FileUpload130Controller(
            IFilesService130 filesService130
        )
        {
            _FilesService130 = filesService130;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(FileUpload fileUpload)
        {
            return Json(await _FilesService130.FileUploadAsync(fileUpload));
        }
    }
}
