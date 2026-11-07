using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Data.Auths;
using HIA.Data.Records;
using HIA.Repositories.CheckThe;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HIA.Services.Verify;

public class XMLVerifyService : IXMLVerifyService
{
    private readonly ILogger<XMLVerifyService> _logger;
    private readonly ICheckThe130Repository _checkThe130Repository;

    public XMLVerifyService(
        ILogger<XMLVerifyService> logger,
        ICheckThe130Repository checkThe130Repository
    )
    {
        _logger = logger;
        _checkThe130Repository = checkThe130Repository;
    }

    public async Task<ResponseModel<XML01>> GetXMLNeedVerify(string? maCSKCB = null, Hospital? hospital = null)
    {
        var res = new ResponseModel<XML01>(HttpStatusCode.BadRequest, "", null);
        return await Task.FromResult(res);
    }

    public async Task<bool> UpdateXMLVerifyResult(long xml01Id, string? maCSKCB, string? ketqua)
    {
        return await Task.FromResult(false);
    }

    //Linhist changed 20/10/24
    //return 1: success, 0: false or error, -1: dublicated or existed data
    public async Task<int> UpdatedVerify(CheckThe4210 result)
    {
        try
        {
            //Lấy bản ghi có thời gian gần nhất với hiện tại
            var theinfor = await _checkThe130Repository.GetItemOrderBy(
                c => c.MA_THE_BHYT == result.MA_THE_BHYT
                    && c.MA_CSKCB == result.MA_CSKCB
                    && c.MA_BN == result.MA_BN,
                q => q.OrderByDescending(x => x.TG_CHECK_THE)
            );
            if (theinfor != null && theinfor.ERROR_CONTENTS == result.ERROR_CONTENTS)
            {
                return -1; // Duplicated
            }
            await _checkThe130Repository.InsertAsync(result);
            await _checkThe130Repository.SaveChangesAsync();
            Uti.HandleCronJobLog(_logger, $"Save Ket qua thong tuyen thannh cong: MaBN: {result.MA_BN}, MA The:{result.MA_THE_BHYT}, MA CSKCB:{result.MA_CSKCB}", nameof(XMLVerifyService.UpdatedVerify));
            return 1;
        }
        catch (Exception ex)
        {
            Uti.HandleCronJobLog(_logger, $"Save Ket qua thong tuyen fails: MaBN: {result.MA_BN}, MA The:{result.MA_THE_BHYT}, MA_CSKCB: {result.MA_CSKCB}, Error: {ex}", nameof(XMLVerifyService.UpdatedVerify));
            return 0;
        }
    }
}
