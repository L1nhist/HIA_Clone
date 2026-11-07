using HIA.Core.Models.Base;
using HIA.Data.Auths;
using HIA.Data.Records;

namespace HIA.Services.Verify;

public interface IXMLVerifyService
{
    Task<ResponseModel<XML01>> GetXMLNeedVerify(string? maCSKCB = null, Hospital? hospital = null);

    Task<bool> UpdateXMLVerifyResult(long xml01Id, string? maCSKCB, string? ketqua);

    Task<int> UpdatedVerify(CheckThe4210 result);
}
