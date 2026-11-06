using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files130;
using HIA.Core.Models.Token;
using HIA.Services.Files;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Net;

namespace HIA.Services.Report;

public partial class ReportHandleService
{
    public async Task<ResponseModel<byte[]>> BaoCaoDayDuLieu(IEnumerable<HosoDayLaiInfo> hosos, string maCSKCB, int type)
    {
        var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);

        try
        {
            var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);
            string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

            var allKhoas = hosos.Select(x => x.MA_KHOA.Split(';').Last()).Distinct().OrderBy(x => x).ToList();
            using (var fs = new MemoryStream())
            {
                #region Sheet 1: DS File Bao cao
                ISheet xlSheet = workbook.CreateSheet("Thống kê lỗi");

                int rows = 2;
                int cols = CreateHeader(xlSheet, cellStyles,
                            $"Danh sách Hồ Sơ {(type == 1 ? "có sửa nhưng số lỗi y hệt" : type == 2 ? "đã sửa và cải thiện (giảm lỗi)" : type == 3 ? "có sửa nhưng xấu hơn (nhiều lỗi hơn)" : "không sửa lại")} - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                            "STT", "Mã LK", "Số lỗi", "Họ tên", "Số BHYT", "Ngày sinh",
                            "Mã khoa", "Tên khoa", "Mã chẩn đoán",
                            "Ngày vào", "Ngày ra", "Người dùng", "Email");

                foreach (var e in hosos)
                {
                    AppendRow(xlSheet, cellStyles, rows++, rows - 2,
                        e.MA_LK, e.SO_LOI, e.HO_TEN, e.SO_BHYT, e.NGAY_SINH,
                        e.MA_KHOA, GetFormular_TenKhoa(fmKhoa, e.MA_KHOA, true), e.MA_CHAN_DOAN,
                        e.NGAY_VAO, e.NGAY_RA, e.NGUOI_GUI?.Ten, e.NGUOI_GUI?.Email);
                }

                SetAutoFeatures(xlSheet, cols, 6);
                #endregion

                CreateLookupSheet(workbook, cellStyles, null, dmKhoas);
                workbook.Write(fs);
                res.Data = fs.ToArray();
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_ThongkeLoi));
        }

        return res;
    }
}
