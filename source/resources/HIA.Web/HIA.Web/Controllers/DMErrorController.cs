//using HIA.Core.Languages.Message;
//using HIA.Core.Models.Base;
//using HIA.Core.Models.Categories;
//using HIA.Services.Categories;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Net;
//using System.Threading.Tasks;

//namespace HIA.Web.Controllers
//{
//    [Authorize]
//    public class DMErrorController : Controller
//    {
//        private readonly IDMErrorService _dMErrorService;
//        public DMErrorController(
//            IDMErrorService dMErrorService
//            )
//        {
//            _dMErrorService = dMErrorService;
//        }
//        public async Task<IActionResult> Index()
//        {
//            var res = await _dMErrorService.GetItems();
//            return View(res.Data);
//        }
//        [HttpPost]
//        public async Task<IActionResult> Save(DMErrorModifyModel request)
//        {
//            if (request.Data.Count == 0)
//            {
//                return Ok(new ResponseModel<bool>
//                {
//                    Code = HttpStatusCode.BadRequest,
//                    Message = MessageResource.ERR_400_NO_DATA,
//                    Data = false
//                });
//            }
//            var res = await _dMErrorService.Save(request);
//            return Ok(res);
//        }
//    }
//}
