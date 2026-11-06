using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files130;
using HIA.Core.Models.Token;
using HIA.Data.Records;
using HIA.Services.Files;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Net;

namespace HIA.Services.Report;

public partial class ReportHandleService
{
    public async Task<ResponseModel<byte[]>> BaoCao_TrungMaMay(string filterString)
    {
        var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
        var maCSKCB = GetMaCSKCB();

        try
        {
            var filterModel = JsonDefaults.Deserialize<FilterDownloadModel>(filterString);
            filterModel ??= new FilterDownloadModel()
            {
                FromDateCreate = DateTime.Now.Date,
                ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
            };

            filterModel.MaCSKCB = maCSKCB;
            var queryable = BuildQuery_XML01(filterModel);
            var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));
            await AttachInnerXML(data1);

            var errors = await AttachXMLError(maCSKCB, data1);
            var errCodes = errors.Select(e => e.MA_LOI).Distinct().ToList();
            var dmErrors = await GetDMErrors(maCSKCB, errCodes);
            var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
            var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

            using (var fs = new MemoryStream())
            {
                #region Sheet 1: Bang filter loi
                ISheet xlSheetFilter = workbook.CreateSheet("DS Trùng");
                int rows = 2;
                int cols = CreateHeader(xlSheetFilter, cellStyles,
                            "Danh sách Trùng mã máy và giờ kết quả tại XML4", // Sheet title
                            "STT", "Tên file", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ",
                            "Mã Dịch vụ", "Tên dịch vụ", "Mã Máy", "Mã Nhóm", //"Mã Chỉ số", "Tên Chỉ số",
                            "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                IRow row = xlSheetFilter.CreateRow(2);
                ICell cell = row.CreateCell(0);
                var dataJoin = data1.SelectMany(x => x.XML03s).Join(data1.SelectMany(x => x.XML04s), x3 => new { x3.XML01ID, x3.MA_DICH_VU, x3.NGAY_KQ }, x4 => new { x4.XML01ID, x4.MA_DICH_VU, x4.NGAY_KQ },
                    (x3, x4) => new
                    {
                        x3.XML01,
                        x3.MA_DICH_VU,
                        x3.TEN_DICH_VU,
                        x3.MA_MAY,
                        x3.MA_NHOM,
                        x4.TEN_CHI_SO,
                        x4.MA_BS_DOC_KQ,
                        x4.NGAY_KQ
                    });

                foreach (var t in dataJoin)
                {
                    AppendRow(xlSheetFilter, cellStyles, rows++,
                            rows - 2, t.XML01.FileName, t.XML01.MA_LK, t.XML01.MA_BN, t.XML01.HO_TEN, t.XML01.MA_THE_BHYT,
                            t.MA_DICH_VU, t.TEN_DICH_VU, t.TEN_CHI_SO, t.MA_NHOM,
                            t.NGAY_KQ, t.XML01.NGAY_VAO, t.XML01.NGAY_RA, t.XML01.NGAY_TTOAN,
                            string.Format("{0:yyyy/MM/dd HH:mm:ss}", t.NGAY_KQ)
                        );
                }

                SetAutoFeatures(xlSheetFilter, cols);
                #endregion


                #region Sheet 1: Danh sach all XML4

                ISheet xlSheetX4 = workbook.CreateSheet("XML4");
                rows = 2;
                cols = CreateHeader(xlSheetX4, cellStyles,
                            "Danh sách Mã máy và giờ kết quả tại XML4", // Sheet title
                            "STT", "Tên file", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ",
                            "Mã Dịch vụ", "Tên dịch vụ", "Mã Máy", "Mã Nhóm",
                            "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                var allXml4 = data1.SelectMany(x => x.XML04s).OrderBy(x => x.XML01ID).ToList();
                foreach (var t in allXml4)
                {


                }

                SetAutoFeatures(xlSheetX4, cols);
                #endregion

                CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);
                workbook.Write(fs);
                res.Data = fs.ToArray();
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_TrungMaMay));
        }

        return res;
    }
}
