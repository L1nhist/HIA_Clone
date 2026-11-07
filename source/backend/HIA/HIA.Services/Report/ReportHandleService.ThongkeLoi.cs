
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files;
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
    public async Task<ResponseModel<byte[]>> BaoCao_ThongkeLoi(string filterString)
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

            var queryable = FilesService130.BuildQueryToGetListData(filterModel, hasDeleted: true);
            var data = await _hosoRepository.GetListAsync(queryable.FilterQueryable);
            var data1 = data.Where(x => x.IsDelete != true).ToList();

            var errors = await AttachXMLError(maCSKCB, data1);
            var errCodes = errors.Select(e => e.MA_LOI).Distinct().ToList();
            var dmErrors = await GetDMErrors(maCSKCB, errCodes);
            var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
            var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

            var allKhoas = data1.Select(x => (x.MA_KHOA ?? string.Empty).Split(';').Last()).Distinct().OrderBy(x => x).ToList();
            dmErrors = dmErrors.OrderBy(e => e.error_file);

            using (var fs = new MemoryStream())
            {
                #region Sheet 1: DS File Bao cao
                ISheet xlSheetF = workbook.CreateSheet("Thống kê lỗi");

                int rows = 2;

                string[] hVals = new string[allKhoas.Count + 4];
                hVals[0] = "Mã Lỗi";
                hVals[1] = "Loại lỗi";
                hVals[2] = "Nội dung lỗi";
                hVals[3] = "Hồ sơ lỗi";
                allKhoas.CopyTo(hVals, 4);

                int cols = CreateHeader(xlSheetF, cellStyles,
                            $"Báo Cáo Thống kê {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                            hVals);

                var xmlKhoa = data1.GroupBy(x => new { x.XML01ID, MA_KHOA = (x.MA_KHOA ?? string.Empty).Split(";").Last() }, (k, g) => new { k.XML01ID, k.MA_KHOA }).Distinct().ToList();
                var errCnts = errors.GroupBy(e => new { e.XML01ID, e.MA_LOI }, (k, g) => new { k.XML01ID, k.MA_LOI, Cnt = g.Count() })
                                    .Join(xmlKhoa, e => e.XML01ID, x => x.XML01ID, (e, x) => new { x.XML01ID, x.MA_KHOA, e.MA_LOI, e.Cnt })
                                    .GroupBy(x => new { x.MA_KHOA, x.MA_LOI }, (k, g) => new { k.MA_KHOA, k.MA_LOI, SO_LOI = g.Sum(e => e.Cnt) });

                foreach (var e in dmErrors)
                {
                    var lst = errCnts.Where(x => x.MA_LOI == e.error_code);
                    if (!lst.Any()) continue;

                    List<object?> values = new()
                    {
                        e.error_code,
                        e.is_alert ? "CẢNH BÁO" : "BÁO LỖI",
                        e.message,
                        e.error_file,
                    };

                    foreach (var k in allKhoas)
                    {
                        values.Add(lst.FirstOrDefault(e => e.MA_KHOA == k)?.SO_LOI ?? 0);
                    }

                    AppendRow(xlSheetF, cellStyles, rows++, values.ToArray());
                }

                SetAutoFeatures(xlSheetF, cols, 1, -2);
                #endregion

                #region SheetBuilder 2: DS Ho so day len & loi
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";
                var x01Ids = data1.Select(x => x.XML01ID).ToList();
                var allLKs = data1.Select(x => x.MA_LK).Distinct().ToList();

                var minDte = data1.Min(x => x.CreateDate).GetValueOrDefault().Date;
                var maxDte = data1.Max(x => x.CreateDate).GetValueOrDefault().Date.AddDays(1);



                #region Linhist changed 08/03/25













                #endregion
                var allStates = await _stateRepository.GetListAsync(x => allLKs.Contains(x.MA_LK) && x.MA_CSKCB == maCSKCB && x.CheckDate >= minDte && x.CheckDate < maxDte);
                var allX0s = allStates.GroupBy(x => new { x.MA_LK, x.MA_CSKCB, x.CheckDate }, (k, g) => new { k.MA_LK, k.CheckDate, ChkCnt = g.Count(), ErrCnt = g.Sum(c => c.TONG_SO_LOI) }).ToList();

                if (allX0s != null && allX0s.Any())
                {


                    ISheet xlSheetX = workbook.CreateSheet("Thống kê hồ sơ");

                    rows = 2;
                    cols = CreateHeader(xlSheetX, cellStyles,
                                $"Báo Cáo Thống kê Hồ sơ tải lên - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                                "STT", "Ngày kiểm tra", "Số lần đẩy hồ sơ", "Mã liên kết", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ", "Mã Khoa", "Tên Khoa", "Tổng số lỗi");

                    var data1Join = data1.GroupBy(x => new { x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA }, (k, g) => k).Distinct().ToList();
                    var d01joiner = data1Join.Join(allX0s, d => d.MA_LK, x => x.MA_LK, (d, x) => new { d.MA_LK, d.MA_BN, d.HO_TEN, d.MA_THE_BHYT, d.MA_KHOA, x.CheckDate, x.ChkCnt, x.ErrCnt })
                                                .Distinct().OrderBy(c => c.MA_LK).GroupBy(x => x.MA_LK, (k, g) => g.OrderByDescending(c => c.CheckDate)).ToList();
                    foreach (var grp in d01joiner)
                    {
                        var lan = grp.Count();
                        foreach (var x in grp)
                        {
                            AppendRow(xlSheetX, cellStyles, rows++, rows - 2, x.CheckDate, $"Lần {lan--}", x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.MA_KHOA, true), x.ErrCnt);
                        }
                    }

                    SetAutoFeatures(xlSheetX, cols, 5, 7);
                }
                #region Linhist removed 061026 - ko dùng check changes nữa





                #endregion
                #endregion

                #region SheetBuilder 3: Thong ke so lan đay ho so
                var cntStates = allStates.GroupBy(x => new { x.MA_LK, x.MA_CSKCB }, (k, g) => new { k.MA_LK, AllCnt = g.OrderBy(s => s.CheckDate).Select(s => s.TONG_SO_LOI).ToList() });
                var allCnts = data1.Select(x => new { x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA }).Distinct();

                ISheet xlSheetT = workbook.CreateSheet("Thống kê số lần đẩy");

                rows = 2;
                cols = CreateHeader(xlSheetT, cellStyles,
                            $"Báo Cáo Thống kê Hồ sơ tải lên tổng hợp số lần - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                            "STT", "Mã LK", "Mã BN", "Họ tên", "Mã thẻ", "Mã khoa", "Tên khoa", "Lần 1", "Lần 2", "Lần 3", "Lần 4", "Lần 5", "Lần 6", "Lần 7", "Lần 8", "Lần 9", "Lần 10", "Tổng số lần", "Chênh lệnh");

                fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";
                if (allCnts?.Any() == true)
                {
                    foreach (var x in allCnts)
                    {
                        var arr = new object?[] { rows - 2, x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.MA_KHOA, true), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "" };
                        var cnt = cntStates.FirstOrDefault(e => e.MA_LK == x.MA_LK);
                        if (cnt != null)
                        {
                            if (cnt.AllCnt == null || cnt.AllCnt.Count == 0)
                            {
                                arr[arr.Length - 1] = "Không kiểm tra lại";
                            }
                            else if (cnt.AllCnt.Count == 1)
                            {
                                arr[7] = cnt.AllCnt[0];
                                arr[arr.Length - 2] = 1;
                                arr[arr.Length - 1] = "Không kiểm tra lại";
                            }
                            else
                            {
                                int i = 7;
                                foreach (var c in cnt.AllCnt)
                                {
                                    if (i > arr.Length - 3) break;
                                    arr[i++] = c;
                                }

                                arr[arr.Length - 2] = cnt.AllCnt.Count;
                                var diff = cnt.AllCnt[0] - cnt.AllCnt[cnt.AllCnt.Count - 1];
                                arr[arr.Length - 1] = diff < 0 ? "Tăng lỗi" : diff > 0 ? "Giảm lỗi" : "Không thay đổi";
                            }
                        }

                        AppendRow(xlSheetT, cellStyles, rows++, arr);
                    }

                    SetAutoFeatures(xlSheetT, cols, 1, 14);
                }
                #endregion

                #region SheetBuilder 4: Thong ke ho so day len theo khoa
                var khoaGroups = data1.GroupBy(x => new { MA_KHOA = (x.MA_KHOA ?? string.Empty).Split(';').Last() }, (k, g) => new { k.MA_KHOA, Cnt = g.Count() }).OrderBy(x => x.MA_KHOA).Distinct().ToList();
                ISheet xlSheetZ = workbook.CreateSheet("Thống kê theo khoa");

                rows = 2;
                cols = CreateHeader(xlSheetZ, cellStyles,
                            $"Báo Cáo Thống kê Hồ sơ tải lên theo khoa - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                            "STT", "Mã Khoa", "Tên Khoa", "Số lần đẩy hồ sơ");

                foreach (var f in khoaGroups)
                {
                    AppendRow(xlSheetZ, cellStyles, rows++, rows - 2, f.MA_KHOA, GetFormular_TenKhoa(fmKhoa, f.MA_KHOA, true), f.Cnt);
                }

                SetAutoFeatures(xlSheetZ, cols, 2);
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
