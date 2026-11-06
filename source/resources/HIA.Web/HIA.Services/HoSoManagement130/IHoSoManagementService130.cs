using System.Collections.Generic;
using System.Threading.Tasks;
using HIA.Core.Models.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Services.HosoManagement130
{
    public interface IHoSoManagementService130
    {
        Task ValidateHoso();

        Task ValidateHosoFrom4210();
        Task ValidateHosoVIP1();

        Task ValidateHosoVIP2();

        Task ValidateHosoVIP3();

        //Task VerifyHoso();

        Task AppendHosoToValidate(string maCSKCB = "", params long[] xmlIds);

        Task RemoveHosoFromValidate(params long[] xmlIds);

        //Task AppendHosoToVerify(Hospital hospital, params long[] xmlIds);

        //Task RemoveHosoFromVerify(params long[] xmlIds);

        Task<bool> AttachInnerXML(IEnumerable<XML01> xmls, string innerList = null);

        Task<ResponseModel<bool>> CreateNewXML(List<XML01> xmls, string maCSKCB = null, string email = null, bool fromHIS = false);

        Task ValidateXML(string maCSKCB = null, string email = null, bool fromAPI = false, bool fromHIS = false, params long[] xmlIds);
    }
}