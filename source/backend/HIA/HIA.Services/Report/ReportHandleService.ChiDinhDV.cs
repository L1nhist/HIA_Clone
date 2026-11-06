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
    public async Task<ResponseModel<byte[]>> BaoCao_ChiDinhDV(string filterString)
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
            await AttachInnerXML(data1, "3");

            var dmDongThois = await _dmDongThoiRepository.GetListAsync(x => (x.MA_LOI == 930 || x.MA_LOI == 1004) && x.HIEU_LUC == true);

            IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);
            var data3 = data1.SelectMany(x => x.XML03s);

            using (var fs = new MemoryStream())
            {
                #region Sheet 1: DVKT Tuong Duong Trung gio - Loi 930
                ISheet xlSheet930 = workbook.CreateSheet("DVKT Tương đương");
                int rows = 2;
                int cols = CreateHeader(xlSheet930, cellStyles,
                            "Danh sách DVKT tương đương bị trùng ngày", // Sheet title
                            "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thẻ", "Ngày sinh", "Giới tính",
                            "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
                            "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

                var dongThoi_err930 = dmDongThois.Where(x => x.MA_LOI == 930);
                foreach (var rule in dongThoi_err930)
                {
                    foreach (var xml1 in data1)
                    {
                        var dichVus = data3.Where(x => x.XML01ID == xml1.XML01ID && MatchCode(x.MA_DICH_VU, rule.MA_DICH_VU) && x.NGAY_YL != null);
                        var dvKhacs = data3.Where(x => x.XML01ID == xml1.XML01ID && MatchCode(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null);
                        var allDates = dichVus.Select(x => x.NGAY_YL.GetValueOrDefault().Date).Distinct().ToList();
                        allDates.AddRange(dvKhacs.Select(x => x.NGAY_YL.GetValueOrDefault().Date).Distinct().ToList());

                        foreach (var dte in allDates)
                        {
                            var matches1 = dichVus.Where(x => x.NGAY_YL.GetValueOrDefault().Date == dte).ToList();
                            var matches2 = dvKhacs.Where(x => x.NGAY_YL.GetValueOrDefault().Date == dte).ToList();

                            var cnt1 = matches1.Count();
                            var cnt2 = matches2.Count();

                            int cnt = Math.Max(cnt1, cnt2);
                            for (int i = 0; i < cnt; i++)
                            {
                                var m1 = i < cnt1 ? matches1[i] : new XML03();
                                var m2 = i < cnt2 ? matches2[i] : new XML03();

                                AppendRow(xlSheet930, cellStyles, rows++,
                                            rows - 2, xml1.MA_LK, xml1.MA_BN, xml1.HO_TEN,
                                            xml1.MA_THE_BHYT, GetNgayThang(xml1.NGAY_SINH, true), GetType_GioiTinh(xml1.GIOI_TINH),
                                            m1.MA_DICH_VU, m1.TEN_DICH_VU, m1.NGAY_YL, m1.NGAY_KQ,
                                            m2.MA_DICH_VU, m2.TEN_DICH_VU, m2.NGAY_YL, m2.NGAY_KQ
                                        );
                            }
                        }
                    }
                }
                SetAutoFeatures(xlSheet930, cols);
                #endregion

                #region Sheet 2: Chi dinh thieu DV, VT - Loi 1004
                var dongThoi_err1004 = dmDongThois.Where(x => x.MA_LOI == 1004);

                List<XML03> errVTs = new();
                List<XML03> errDVs = new();

                foreach (var rule in dongThoi_err1004)
                {
                    foreach (var xml1 in data1)
                    {
                        var dvMatches = data3.Where(x => x.XML01ID == xml1.XML01ID && string.IsNullOrEmpty(x.MA_VAT_TU) && MatchCode(x.MA_DICH_VU, rule.MA_DICH_VU));
                        var vtMatches = data3.Where(x => x.XML01ID == xml1.XML01ID && MatchCode(x.MA_VAT_TU, rule.VAT_TU_BHYT));

                        if (!vtMatches.Any())
                        {
                            foreach (var x3 in dvMatches)
                            {
                                if (errDVs.Contains(x3)) continue;
                                if (dongThoi_err1004.Any(r => MatchCode(x3.MA_DICH_VU, r.MA_DICH_VU) && MatchCode(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

                                errDVs.Add(x3);
                            }
                        }
                        else if (!dvMatches.Any())
                        {
                            foreach (var x3 in vtMatches)
                            {
                                if (errVTs.Contains(x3)) continue;
                                if (dongThoi_err1004.Any(r => MatchCode(x3.MA_DICH_VU, r.MA_DICH_VU) && MatchCode(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

                                errVTs.Add(x3);
                            }
                        }
                    }
                }

                ISheet xlSheetDV1004 = workbook.CreateSheet("DV thiếu VT");
                int rowsDV = 2;
                cols = CreateHeader(xlSheetDV1004, cellStyles,
                                    "Danh sách Chỉ định DVKT thiếu VTYT", // Sheet title
                                    "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                    "Mã Thẻ", "Ngày sinh", "Giới tính",
                                    "Mã Dịch vụ", "Tên Dịch vụ", "Ngày Kết quả DV", "Ngày Y lệnh DV", "Nội dung lỗi");

                foreach (var x3 in errDVs)
                {
                    AppendRow(xlSheetDV1004, cellStyles, rowsDV++,
                            rowsDV - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                            x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetType_GioiTinh(x3.XML01.GIOI_TINH),
                            x3.MA_DICH_VU, x3.TEN_DICH_VU, x3.NGAY_YL, x3.NGAY_KQ,
                            "Chỉ định dịch vụ kỹ thuật không có vật tư kỹ thuật đồng thời"
                        );
                }

                SetAutoFeatures(xlSheetDV1004, cols);


                ISheet xlSheetVT1004 = workbook.CreateSheet("VT thiếu DV");
                int rowsVT = 2;
                cols = CreateHeader(xlSheetVT1004, cellStyles,
                                    "Danh sách Chỉ định VTYT thiếu DVKT", // Sheet title
                                    "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                    "Mã Thẻ", "Ngày sinh", "Giới tính",
                                    "Mã Vật tư", "Tên Vật tư", "Ngày Kết quả VT", "Ngày Y lệnh VT", "Nội dung lỗi");

                foreach (var x3 in errVTs)
                {
                    AppendRow(xlSheetVT1004, cellStyles, rowsVT++,
                            rowsVT - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                            x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetType_GioiTinh(x3.XML01.GIOI_TINH),
                            x3.MA_VAT_TU, x3.TEN_VAT_TU, x3.NGAY_YL, x3.NGAY_KQ,
                            "Chỉ định vật tư kỹ thuật không có dịch vụ kỹ thuật đồng thời"
                        );
                }

                SetAutoFeatures(xlSheetVT1004, cols);
                #endregion

                workbook.Write(fs);
                res.Data = fs.ToArray();
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_ChiDinhDV));
        }

        return res;
    }
}
