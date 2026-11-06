using HIA.Core.Models.Base;
using HIA.DatabaseModel.Entites;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIA.Services.Verify
{
    public interface IXMLVerifyService
    {
        Task<ResponseModel<XML01>> GetXMLNeedVerify(string maCSKCB = null, Hospital hospital = null);

        Task<bool> UpdateXMLVerifyResult(long xml01Id, string maCSKCB, string ketqua);

        //Task<IEnumerable<XMLResult>> GetXMLHasResult(string maCSKCB);

        Task<int> UpdatedVerify(CheckThe4210 result);
    }
}
