using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.CheckThe;
using HIA.Repositories.Files;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Services.Verify
{
    public class XMLVerifyService : IXMLVerifyService
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<XMLVerifyService> _logger;
        private readonly IXml01Repository _hosoRepository;
        private readonly IBaseRepository<Hospital> _hospitalRepository;
        //private readonly IBaseRepository<XMLVerify> _verifyRepository;
        //private readonly IBaseRepository<XMLResult> _resultRepository;
        private readonly ICheckThe130Repository _checkThe130Repository;

        public XMLVerifyService(
            IOptions<AppSettings> appSettings,
            ILogger<XMLVerifyService> logger,
            IXml01Repository hosoRepository,
            IBaseRepository<Hospital> hospitalRepository,
            //IBaseRepository<XMLVerify> verifyRepository,
            //IBaseRepository<XMLResult> resultRepository,
            ICheckThe130Repository checkThe130Repository
        )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            _hosoRepository = hosoRepository;
            _hospitalRepository = hospitalRepository;
            //_verifyRepository = verifyRepository;
            //_resultRepository = resultRepository;
            _checkThe130Repository = checkThe130Repository;
        }

        public async Task<ResponseModel<XML01>> GetXMLNeedVerify(string maCSKCB = null, Hospital hospital = null)
        {
            var res = new ResponseModel<XML01>(HttpStatusCode.BadRequest, "", null);
            return await Task.FromResult(res);
            //if (maCSKCB == null )
            //{
            //    Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]:truyen vao ma NULL: {maCSKCB}", nameof(XMLVerifyService.GetXMLNeedVerify));
            //    res.Code = HttpStatusCode.BadRequest;
            //    return res;
            //}

            //try
            //{
            //    hospital = hospital ?? await _hospitalRepository.GetItem(h => h.MA_CSKCB == maCSKCB);
            //    if (hospital == null || hospital.FileAPICount >= hospital.MaxFilePerDate)
            //    {
            //        //Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]:CSKCB het lan kiem tra thong tuyen: {maCSKCB}", nameof(XMLVerifyService.GetXMLNeedVerify));
            //        res.Code = HttpStatusCode.BadRequest;
            //        return res;
            //    }

            //    maCSKCB = hospital.MA_CSKCB;
            //    var xmlVerify = await _verifyRepository.GetItemOrderBy(c => c.MA_CSKCB == maCSKCB, t => t.OrderByDescending(c => c.XML01ID));
            //    if (xmlVerify == null)
            //    {
            //        //Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]:Khong co ho so kiem tra thong tuyen trong bang verify: {maCSKCB}", nameof(XMLVerifyService.GetXMLNeedVerify));
            //        res.Code = HttpStatusCode.NotFound;
            //        return res;
            //    }

            //    await _verifyRepository.DeleteListByFilterAsync(x => x.XML01ID == xmlVerify.XML01ID);
            //    await _verifyRepository.SaveChangesAsync();

            //    var x01res = await _hosoRepository.GetAllIncludedStateByListID(maCSKCB, xmlVerify.XML01ID);
            //    if (x01res?.Any() != true)
            //    {
            //       // Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]:Khong co ho so kiem tra thong tuyen trong XML01: {maCSKCB}", nameof(XMLVerifyService.GetXMLNeedVerify));
            //        res.Code = HttpStatusCode.NotFound;
            //        return res;
            //    }

            //    var x01 = x01res.First();
            //    if (string.IsNullOrEmpty(x01.MA_THE_BHYT?.Trim()))
            //    {
            //        //Linhist changed 21/09/24
            //        //Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]:XML01 khong co the BHYT: {maCSKCB}", nameof(XMLVerifyService.GetXMLNeedVerify));
            //        //res.Data = x01;
            //        //res.Code = HttpStatusCode.NotFound;
            //        //return res;
            //        return await GetXMLNeedVerify(hospital: hospital);
            //    }

            //    hospital.FileAPICount++;
            //    _hospitalRepository.UpdateAsBaseEntity(hospital);
            //    await _hospitalRepository.SaveChangesAsync();

            //    res.Data = x01;
            //    return res;
            //}
            //catch (Exception ex)
            //{
            //    Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]: Get XML need Verify fails MA_CSKCB: {hospital.MA_CSKCB}", nameof(XMLVerifyService.GetXMLNeedVerify));

            //    res.Code = HttpStatusCode.BadRequest;
            //    res.Message = ex.Message;
            //    return res;
            //};
        }

        //public async Task<bool> UpdatedVerify(CheckThe130 result)
        //{
        //    try
        //    {
        //        //Kiem tra neu da ton tai ban ghi trung thi ko insert nua
        //        bool existed=false;
        //        try
        //        {
        //             existed = await _checkThe130Repository.AnyAsync(e => e.MA_THE_BHYT == result.MA_THE_BHYT && e.MA_CSKCB == result.MA_CSKCB && e.MA_BN == result.MA_BN && e.ERROR_CODE == result.ERROR_CODE);
        //        }
        //        catch (Exception ex)
        //        {
        //            Uti.HandleCronJobLog(_logger, $"Save Ketqua thong tuyen loi khau kiem tra existed: {result.MA_CSKCB}, Lỗi: {ex.ToString()}", nameof(XMLVerifyService.UpdatedVerify));

        //        }

        //        if (!existed)
        //        {
        //            try
        //            {
        //                await _checkThe130Repository.InsertAsync(result);
        //                await _resultRepository.SaveChangesAsync();
        //                Uti.HandleCronJobLog(_logger, $"Save Ketqua thong tuyen thannh cong: {result.MA_CSKCB}", nameof(XMLVerifyService.UpdatedVerify));
        //            }
        //            catch (Exception ex)
        //            {
        //                Uti.HandleCronJobLog(_logger, $"Save Ketqua thong tuyen loi khau insert: {result.MA_CSKCB}, Lỗi: {ex.ToString()}", nameof(XMLVerifyService.UpdatedVerify));

        //            }
        //        }
        //        else
        //        {
        //           // Uti.HandleCronJobLog(_logger, $"Ketqua thong tuyen da ton tai, khong luu lai nua, CSKCB: {result.MA_CSKCB}, Mã BN: {result.MA_BN}", nameof(XMLVerifyService.UpdatedVerify));
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Uti.HandleCronJobLog(_logger, $"Save Ketqua thong tuyen fails MA_CSKCB: {result.MA_CSKCB}", nameof(XMLVerifyService.UpdatedVerify));
        //        return false;
        //    };
        //}

        //Linhist changed 20/10/24
        //return 1: success, 0: false or error, -1: dublicated or existed data
        public async Task<int> UpdatedVerify(CheckThe4210 result)
        {
            try
            {
                //Lấy bản ghi có thời gian gần nhất với hiện tại (giả sử có trường DateTime TG_CHECK_THE trong CheckThe4210)
                var theinfor = await _checkThe130Repository.GetItemOrderBy(
                    c => c.MA_THE_BHYT == result.MA_THE_BHYT
                        && c.MA_CSKCB == result.MA_CSKCB
                        && c.MA_BN == result.MA_BN,
                    q => q.OrderByDescending(x => x.TG_CHECK_THE) // TG_CHECK_THE là trường thời gian kiểm tra
                );
                if (theinfor != null)
                {
                    if (theinfor.ERROR_CONTENTS == result.ERROR_CONTENTS)
                    {
                        return -1; // Duplicated
                    }
                }
                // Insert the result and save changes
                await _checkThe130Repository.InsertAsync(result);
                await _checkThe130Repository.SaveChangesAsync();
                Uti.HandleCronJobLog(_logger, $"Save Ket qua thong tuyen thannh cong: MaBN: {result.MA_BN}, MA The:{result.MA_THE_BHYT}, MA CSKCB:{result.MA_CSKCB}", nameof(XMLVerifyService.UpdatedVerify));
                return 1;
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Save Ket qua thong tuyen fails: MaBN: {result.MA_BN}, MA The:{result.MA_THE_BHYT}, MA_CSKCB: {result.MA_CSKCB}, Error: {ex.ToString()}", nameof(XMLVerifyService.UpdatedVerify));
                return 0;
            }
        }

        public async Task<bool> UpdateXMLVerifyResult(long xml01Id, string maCSKCB, string ketqua)
        {
            return await Task.FromResult(false);
            //try
            //{
            //    await _resultRepository.InsertAsync(new XMLResult
            //    {
            //        XML01ID = xml01Id,
            //        MA_CSKCB = maCSKCB,
            //        KET_QUA = ketqua,
            //        LastSent = DateTime.Now
            //    });
            //    await _resultRepository.SaveChangesAsync();
            //    Uti.HandleCronJobLog(_logger, $"SendXMLResult: Save Ketqua thong tuyen thannh cong: {maCSKCB}", nameof(XMLVerifyService.UpdateXMLVerifyResult));

            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    Uti.HandleCronJobLog(_logger, $"SendXMLResult: Save Ketqua thong tuyen fails MA_CSKCB: {maCSKCB}", nameof(XMLVerifyService.UpdateXMLVerifyResult));
            //    return false;
            //};
        }

        //public async Task<IEnumerable<XMLResult>> GetXMLHasResult(string maCSKCB)
        //{
        //    try
        //    {
        //        //Tinh toan so ban ghi moi lan chay
        //        int maxVerifyInTime = _appSettings.BackgroundServices.VerifyXML.MinuteVerifyTime * 60;
        //        maxVerifyInTime = (int)Math.Floor((double)maxVerifyInTime - (maxVerifyInTime * _appSettings.BackgroundServices.VerifyXML.PercentItemInTime / 100));

        //        //Tam thoi fix so ban ghi 1 lan chay
        //        maxVerifyInTime = 30;
        //        //var thes = await _checkTheRepository.GetListPaginationAsync(0, maxVerifyInTime, c => c.MA_CSKCB == MA_CSKCB && c.TG_CHECK_THE == null, t => t.OrderByDescending(c => c.XML1ID));
        //        var thes = await _resultRepository.GetListPaginationAsync(0, maxVerifyInTime, c => c.MA_CSKCB == maCSKCB, t => t.OrderByDescending(c => c.XML01ID));
        //        if (thes == null || !thes.Items.Any()) return null;
        //        return thes.Items;
        //    }
        //    catch (Exception ex)
        //    {
        //        Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]: Get XML result to check fails MA_CSKCB: {maCSKCB}", nameof(XMLVerifyService.UpdateXMLVerifyResult));
        //        return null;
        //    };
        //}
    }
}
