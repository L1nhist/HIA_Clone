using HIA.Data.Records;

namespace HIA.Services.Verify;

public interface IXMLVerifyService
{
    Task<int> UpdatedVerify(CheckThe4210 result);
}
