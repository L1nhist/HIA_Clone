using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Web.Controllers
{
    [Authorize]
    public class DMError130Controller : Controller
    {
        private readonly IDMErrorService130 _dMError130Service;
        public DMError130Controller(
            IDMErrorService130 dMError130Service
            )
        {
            _dMError130Service = dMError130Service;
        }

        public async Task<IActionResult> Index()
        {
            var res = await _dMError130Service.GetItems();
            return View(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Save(DMErrorModifyModel request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return Ok(new ResponseModel<bool>
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = MessageResource.ERR_400_NO_DATA,
                    Data = false
                });
            }

            var res = await _dMError130Service.Save(request);
            return Ok(res);
        }
    }
}
