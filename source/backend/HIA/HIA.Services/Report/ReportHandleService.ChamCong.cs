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
    private static bool HasMaBS(string? cchn, string? allMaBS)
    {
        if (string.IsNullOrEmpty(cchn) || string.IsNullOrEmpty(allMaBS)) return false;
        return allMaBS == cchn || $";{allMaBS.Trim()};".Contains($";{cchn.Trim()};");
    }

    public async Task<ResponseModel<byte[]>> BaoCao_ChamCong(string filterString)
    {
        var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
        var maCSKCB = GetMaCSKCB();
        var hospitalId = GetClaim(ClaimKey.HOSPITAL_ID);

        try
        {
            DateTime dStart = GetCreateDate_Selected(filterString);
            DateTime dEnd = dStart.AddDays(32);
            while (dEnd.Month != dStart.Month)
            {
                dEnd = dEnd.AddDays(-1);
                if (dEnd.Month == dStart.Month)
                {
                    dEnd = dEnd.AddDays(1);
                    break;
                }
            }

            var hospital = await _hospitalRepository.GetById(hospitalId);
            var ofct = new TimePeriod(hospital?.OfficeTime);



            var x01set = _hosoRepository.DbSet.Where(x => x.MA_CSKCB == maCSKCB && x.IsDelete != true);

            var all2 = await x01set.SelectMany(x => x.XML02s)
                                .Where(x2 => x2.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x2.MA_BAC_SI) && x2.NGAY_YL != null && x2.NGAY_YL >= dStart && x2.NGAY_YL < dEnd).ToListAsync();
            var data2 = all2.Select(x2 => new { XML2 = x2, Period = GetWorkPeriod(x2.NGAY_YL, ofct), x2.NGAY_YL.GetValueOrDefault().Date})
                                .GroupBy(x2 => new { x2.XML2.MA_BAC_SI, x2.Date, x2.Period }, (g, c) => c.First())
                                .Distinct().OrderBy(x2 => x2.XML2.NGAY_YL).ToList();

            var all3a = await x01set.SelectMany(x => x.XML03s)
                                    .Where(x3 => x3.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x3.MA_BAC_SI) && x3.NGAY_YL != null && x3.NGAY_YL >= dStart && x3.NGAY_KQ != null && x3.NGAY_KQ < dEnd).ToListAsync();
            var data3a = all3a.Select(x3 => new { XML3 = x3, Period = GetWorkPeriod(x3.NGAY_YL, ofct), x3.NGAY_YL.GetValueOrDefault().Date })
                                    .GroupBy(x3 => new { x3.XML3.MA_BAC_SI, x3.Date, x3.Period }, (g, c) => c.First())
                                    .Distinct().OrderBy(x3 => x3.XML3.NGAY_YL).ToList();


            var all3b = await x01set.SelectMany(x => x.XML03s)
                                    .Where(x3 => x3.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x3.NGUOI_THUC_HIEN) && x3.NGAY_TH_YL != null && x3.NGAY_TH_YL >= dStart && x3.NGAY_KQ != null && x3.NGAY_KQ < dEnd && (x3.MA_NHOM != 10 || x3.MA_NHOM != 11)).ToListAsync();
            var data3b = all3b.Select(x3 => new { XML3 = x3, Period = GetWorkPeriod(x3.NGAY_TH_YL, ofct), x3.NGAY_TH_YL.GetValueOrDefault().Date })
                                    .GroupBy(x3 => new { x3.XML3.NGUOI_THUC_HIEN, x3.Date, x3.Period }, (g, c) => c.First())
                                    .Distinct().OrderBy(x3 => x3.XML3.NGAY_TH_YL).ToList();


            var all4 = await x01set.Include(x => x.XML04s).SelectMany(x => x.XML04s)
                                    .Where(x4 => x4.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x4.MA_BS_DOC_KQ) && x4.NGAY_KQ != null && x4.NGAY_KQ >= dStart && x4.NGAY_KQ <= dEnd).ToListAsync();
            var data4 = all4.Select(x4 => new { XML4 = x4, Period = GetWorkPeriod(x4.NGAY_KQ, ofct), x4.NGAY_KQ.GetValueOrDefault().Date })
                                    .GroupBy(x4 => new { x4.XML4.MA_BS_DOC_KQ, x4.Date, x4.Period }, (g, c) => c.First())
                                    .Distinct().OrderBy(x4 => x4.XML4.NGAY_KQ).ToList();


            var all5 = await x01set.Include(x => x.XML05s).SelectMany(x => x.XML05s)
                                    .Where(x5 => x5.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x5.NGUOI_THUC_HIEN) && x5.THOI_DIEM_DBLS != null && x5.THOI_DIEM_DBLS >= dStart && x5.THOI_DIEM_DBLS <= dEnd).ToListAsync();
            var data5 = all5.Select(x5 => new { XML5 = x5, Period = GetWorkPeriod(x5.THOI_DIEM_DBLS, ofct), x5.THOI_DIEM_DBLS.GetValueOrDefault().Date })
                                    .GroupBy(x5 => new { x5.XML5.NGUOI_THUC_HIEN, x5.Date, x5.Period }, (g, c) => c.First())
                                    .Distinct().OrderBy(x5 => x5.XML5.THOI_DIEM_DBLS).ToList();


            var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);

            ICellStyle[] cellStyles;
            IWorkbook workbook = CreateWorkBook(out cellStyles);

            var allBS = dmBacsis.OrderBy(b => b.MA_KHOA);
            var lstX3Mores = new List<XML03>();

            var pivots = new Dictionary<DateTime, Dictionary<string, string>>();
            foreach (var bs in allBS)
            {
                string perd;
                DateTime dte;
                Dictionary<string, string>? lst;
                var cchn = bs.MA_CCHN ?? string.Empty;

                foreach (var x2 in data2)
                {
                    dte = x2.Date;


                    if (!pivots.TryGetValue(dte, out lst))
                    {
                        lst = new Dictionary<string, string>();
                        pivots.Add(dte, lst);
                    }

                    if (x2.XML2.MA_BAC_SI == cchn)
                    {
                        perd = x2.Period;
                        if (!lst.ContainsKey(cchn))
                        {
                            lst.Add(cchn, perd);
                        }
                        else
                        {
                            var str = lst[cchn];
                            if (!str.Contains(perd))
                            {
                                lst[cchn] = str + perd;
                            }
                        }
                    }
                }



                foreach (var x3 in data3a)
                {
                    dte = x3.Date;


                    if (!pivots.TryGetValue(dte, out lst))
                    {
                        lst = new Dictionary<string, string>();
                        pivots.Add(dte, lst);
                    }

                    if (HasMaBS(cchn, x3.XML3.MA_BAC_SI))
                    {
                        perd = x3.Period;
                        if (!lst.ContainsKey(cchn))
                        {
                            lst.Add(cchn, perd);
                        }
                        else
                        {
                            var str = lst[cchn];
                            if (!str.Contains(perd))
                            {
                                lst[cchn] = str + perd;
                            }
                        }
                    }
                }

                foreach (var x3 in data3b)
                {
                    dte = x3.Date;


                    if (!pivots.TryGetValue(dte, out lst))
                    {
                        lst = new Dictionary<string, string>();
                        pivots.Add(dte, lst);
                    }

                    if (x3.XML3.MA_NHOM == 1)
                    {
                        if (cchn == x3.XML3.NGUOI_THUC_HIEN || (x3.XML3.NGUOI_THUC_HIEN ?? string.Empty).StartsWith($"{cchn};"))
                        {
                            perd = x3.Period;
                            if (!lst.ContainsKey(cchn))
                            {
                                lst.Add(cchn, perd);
                            }
                            else
                            {
                                var str = lst[cchn];
                                if (!str.Contains(perd))
                                {
                                    lst[cchn] = str + perd;
                                }
                            }
                        }

                        if (cchn != x3.XML3.NGUOI_THUC_HIEN && ((x3.XML3.NGUOI_THUC_HIEN ?? string.Empty).EndsWith($";{cchn}") || (x3.XML3.NGUOI_THUC_HIEN ?? string.Empty).EndsWith($";{cchn};")) && x3.XML3.NGAY_KQ != null)
                        {
                            lstX3Mores.Add(x3.XML3);
                            var dte1 = x3.XML3.NGAY_KQ.Value;
                            if (!pivots.TryGetValue(dte1, out var lst1))
                            {
                                lst1 = new Dictionary<string, string>();
                                pivots.Add(dte1, lst1);
                            }

                            perd = GetWorkPeriod(x3.XML3.NGAY_KQ, ofct);
                            if (!lst1.ContainsKey(cchn))
                            {
                                lst1.Add(cchn, perd);
                            }
                            else
                            {
                                var str = lst1[cchn];
                                if (!str.Contains(perd))
                                {
                                    lst1[cchn] = str + perd;
                                }
                            }
                        }
                    }
                    else if (HasMaBS(cchn, x3.XML3.NGUOI_THUC_HIEN))
                    {
                        perd = x3.Period;
                        if (!lst.ContainsKey(cchn))
                        {
                            lst.Add(cchn, perd);
                        }
                        else
                        {
                            var str = lst[cchn];
                            if (!str.Contains(perd))
                            {
                                lst[cchn] = str + perd;
                            }
                        }
                    }
                }

                foreach (var x4 in data4)
                {
                    dte = x4.Date;


                    if (!pivots.TryGetValue(dte, out lst))
                    {
                        lst = new Dictionary<string, string>();
                        pivots.Add(dte, lst);
                    }

                    if (HasMaBS(cchn, x4.XML4.MA_BS_DOC_KQ))
                    {
                        perd = x4.Period;
                        if (!lst.ContainsKey(cchn))
                        {
                            lst.Add(cchn, perd);
                        }
                        else
                        {
                            var str = lst[cchn];
                            if (!str.Contains(perd))
                            {
                                lst[cchn] = str + perd;
                            }
                        }
                    }
                }

                foreach (var x5 in data5)
                {
                    dte = x5.Date;


                    if (!pivots.TryGetValue(dte, out lst))
                    {
                        lst = new Dictionary<string, string>();
                        pivots.Add(dte, lst);
                    }

                    if (HasMaBS(cchn, x5.XML5.NGUOI_THUC_HIEN))
                    {
                        perd = x5.Period;
                        if (!lst.ContainsKey(cchn))
                        {
                            lst.Add(cchn, perd);
                        }
                        else
                        {
                            var str = lst[cchn];
                            if (!str.Contains(perd))
                            {
                                lst[cchn] = str + perd;
                            }
                        }
                    }
                }
            }

            string fmBsCK = $"IFERROR(INDEX('Bác sĩ'!C:C, MATCH(\"{{0}}\", 'Bác sĩ'!B:B, 0)),\"\")";

            using (var fs = new MemoryStream())
            {

                #region Sheet 1: Cham cong nhan vien BV
                ISheet xlSheetBS = workbook.CreateSheet("Bác sĩ");
                int colBS = CreateHeader_ForWorkDays(xlSheetBS, cellStyles,
                                    $"Bảng Chấm công trong Tháng {dStart:MM} năm {dStart.Year}",
                                    "STT", "Mã CCHN", "Họ tên", "Chức danh", "Mã khoa", "Tên khoa");

                int rows = 5;
                foreach (var bs in allBS)
                {
                    var cchn = bs.MA_CCHN ?? string.Empty;
                    AppendRow(xlSheetBS, cellStyles, rows++, rows - 5, cchn, bs.HO_TEN, bs.CHUC_DANH, bs.MA_KHOA, bs.TEN_KHOA);
                }

                ICell xcel;
                ICellStyle celStyle;
                int cols = colBS;
                DateTime dte = dStart;
                Dictionary<string, string>? lstBS = null;
                while (dte < dEnd)
                {
                    cols++;
                    celStyle = dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday ? cellStyles[2] : cellStyles[1];

                    AddDateHeader_ForWorkDays(xlSheetBS, cols, dte, ServiceConstants.DAYS_OF_WEEK[(int)dte.DayOfWeek], celStyle);

                    rows = 5;
                    celStyle = dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday ? cellStyles[6] : cellStyles[3];

                    if (dte > DateTime.Now || !pivots.TryGetValue(dte, out lstBS))
                    {
                        lstBS = null;
                    }

                    foreach (var bs in allBS)
                    {
                        var cchn = bs.MA_CCHN ?? string.Empty;
                        xcel =xlSheetBS.GetRow(rows++).CreateCell(cols);
                        xcel.CellStyle = celStyle;

                        if (dte > DateTime.Now)
                        {
                            xcel.SetCellValue("");
                        }
                        else if (lstBS == null || !lstBS.ContainsKey(cchn))
                        {
                            xcel.SetCellValue("N");
                        }
                        else
                        {
                            xcel.SetCellValue(FixPeriod(lstBS[cchn]));
                        }
                    }

                    dte = dte.AddDays(1);
                }

                xlSheetBS.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
                xlSheetBS.AddMergedRegion(new CellRangeAddress(1, 1, 0, cols));
                SetAutoFeatures(xlSheetBS, cols + 1);
                #endregion

                #region Sheet 2: Cham cong user web
                var users = await _userRepository.GetListAsync(u => u.HospitalId == hospitalId && u.IsActivated && !u.IsDeleted);
                var logs = await _ulogRepository.DbSet.AsNoTracking()
                                        .Where(u => u.LogTime >= dStart && u.LogTime < dEnd && u.MA_CSKCB == maCSKCB)
                                        .GroupBy(u => new { u.UserID, u.LogTime.Date }, (g, u) => new { g.UserID, g.Date, Count = u.Count() })
                                        .ToListAsync();

                if (logs != null && logs.Any())
                {
                    ISheet xlSheetUS = workbook.CreateSheet("Người dùng");
                    int colUS = CreateHeader_ForWorkDays(xlSheetUS, cellStyles,
                                        $"Bảng Hoạt động trong Tháng {dStart:MM} năm {dStart.Year}",
                                        "STT", "Người dùng", "Email");

                    rows = 5;
                    foreach (var u in users)
                    {
                        AppendRow(xlSheetUS, cellStyles, rows++, rows - 5, u.FullName, u.Email);
                    }

                    cols = colUS;
                    dte = dStart;
                    while (dte < dEnd)
                    {
                        cols++;
                        celStyle = dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday ? cellStyles[2] : cellStyles[1];

                        AddDateHeader_ForWorkDays(xlSheetUS, cols, dte, ServiceConstants.DAYS_OF_WEEK[(int)dte.DayOfWeek], celStyle);

                        rows = 5;
                        celStyle = dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday ? cellStyles[6] : cellStyles[3];

                        foreach (var u in users)
                        {
                            xcel = xlSheetUS.GetRow(rows++).CreateCell(cols);
                            xcel.CellStyle = celStyle;
                            if (dte > DateTime.Now)
                            {
                                xcel.SetCellValue("");
                            }
                            else
                            {
                                xcel.SetCellValue(logs.Any(l => l.UserID == u.Id && l.Date == dte) ? "X" : "-");
                            }
                        }

                        dte = dte.AddDays(1);
                    }

                    xlSheetUS.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
                    xlSheetUS.AddMergedRegion(new CellRangeAddress(1, 1, 0, cols));
                    SetAutoFeatures(xlSheetBS, cols + 1);
                }
                #endregion

                #region Sheet 3: Cham cong XML2
                ISheet xlSheetX2 = workbook.CreateSheet("Danh sách XML02");
                rows = 2;
                cols = CreateHeader(xlSheetX2, cellStyles,
                            "Danh sách hồ sơ XML02 đã chấm công", // Sheet title
                            "STT", "Mã Lượt khám", //"Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Thuốc", "Tên Thuốc", "Mã Bác sĩ", "Tên Bác sĩ",
                            "Ngày làm việc", "Thời điểm");

                foreach (var x in data2)
                {
                    string? maBS =x.XML2.MA_BAC_SI;
                    var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                    AppendRow(xlSheetX2, cellStyles, rows++,
                            x.XML2.STT, x.XML2.MA_LK, //x.XML01.MA_BN, x.XML01.HO_TEN,
                            x.XML2.MA_THUOC, x.XML2.TEN_THUOC, maBS, fmBS[0],
                            x.XML2.NGAY_YL, x.Period);
                }
                SetAutoFeatures(xlSheetX2, cols, 1, 3, 5);
                #endregion

                #region Sheet 4: Cham cong XML3
                ISheet xlSheetX3 = workbook.CreateSheet("Danh sách XML03");
                rows = 2;
                cols = CreateHeader(xlSheetX3, cellStyles,
                            "Danh sách hồ sơ XML03 đã chấm công", // Sheet title
                            "STT", "Mã Lượt khám", //"Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã dịch vụ", "Tên dịch vụ", "Mã vật tư", "Tên vật tư",
                            "Mã Bác sĩ", "Tên Bác sĩ", "Người thực hiện", "Tên Người thực hiện",
                            "Ngày làm việc", "Thời điểm");

                foreach (var x in data3a)
                {
                    string? maBS =x.XML3.MA_BAC_SI;
                    var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                    AppendRow(xlSheetX3, cellStyles, rows++,
                            x.XML3.STT, x.XML3.MA_LK, //x.XML01.MA_BN, x.XML01.HO_TEN,
                            x.XML3.MA_DICH_VU, x.XML3.TEN_DICH_VU, x.XML3.MA_VAT_TU, x.XML3.TEN_VAT_TU,
                            maBS, fmBS[0], "", "",
                            x.XML3.NGAY_YL, x.Period);
                }
                foreach (var x in data3b)
                {
                    string? maBS =x.XML3.NGUOI_THUC_HIEN;
                    var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                    AppendRow(xlSheetX3, cellStyles, rows++,
                            x.XML3.STT, x.XML3.MA_LK, //x.XML01.MA_BN, x.XML01.HO_TEN,
                            x.XML3.MA_DICH_VU, x.XML3.TEN_DICH_VU, x.XML3.MA_VAT_TU, x.XML3.TEN_VAT_TU,
                            "", "", maBS, fmBS[0],
                            x.XML3.NGAY_TH_YL, x.Period);
                }
                foreach (var x in lstX3Mores)
                {
                    string? maBS =x.NGUOI_THUC_HIEN;
                    var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                    AppendRow(xlSheetX3, cellStyles, rows++,
                            x.STT, x.MA_LK,
                            x.MA_DICH_VU, x.TEN_DICH_VU, "", "",
                            $"Nhóm 1(MACCHNx2): {maBS}", fmBS[0],
                            x.NGAY_KQ, GetWorkPeriod(x.NGAY_KQ, ofct));
                }
                SetAutoFeatures(xlSheetX3, cols, 1, 3, 5, 7, 9);
                #endregion

                #region Sheet 5: Cham cong XML4
                ISheet xlSheetX4 = workbook.CreateSheet("Danh sách XML04");
                rows = 2;
                cols = CreateHeader(xlSheetX4, cellStyles,
                            "Danh sách hồ sơ XML04 đã chấm công", // Sheet title
                            "STT", "Mã Lượt khám", "Mã dịch vụ", //"Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Bác sĩ", "Tên Bác sĩ", "Ngày làm việc", "Thời điểm");

                foreach (var x in data4)
                {
                    string? maBS =x.XML4.MA_BS_DOC_KQ;
                    var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                    AppendRow(xlSheetX4, cellStyles, rows++,
                            x.XML4.STT, x.XML4.MA_LK, x.XML4.MA_DICH_VU, //x.XML01.MA_BN, x.XML01.HO_TEN,
                            maBS, fmBS[0], x.XML4.NGAY_KQ, x.Period);
                }
                SetAutoFeatures(xlSheetX4, cols, 1, 3);
                #endregion

                #region Sheet 6: Cham cong XML5
                ISheet xlSheetX5 = workbook.CreateSheet("Danh sách XML05");
                rows = 2;
                cols = CreateHeader(xlSheetX5, cellStyles,
                            "Danh sách hồ sơ XML05 đã chấm công", // Sheet title
                            "STT", "Mã Lượt khám", //"Mã Bệnh nhân", "Tên Bệnh nhân",
                            "Mã Bác sĩ", "Tên Bác sĩ", "Ngày làm việc", "Thời điểm");

                foreach (var x in data5)
                {
                    string? maBS =x.XML5.NGUOI_THUC_HIEN;
                    var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                    AppendRow(xlSheetX5, cellStyles, rows++,
                            x.XML5.STT, x.XML5.MA_LK, //x.XML01.MA_BN, x.XML01.HO_TEN,
                            maBS, fmBS[0], x.XML5.THOI_DIEM_DBLS, x.Period);
                }
                SetAutoFeatures(xlSheetX5, cols, 1, 3);
                #endregion

                workbook.Write(fs);
                res.Data = fs.ToArray();
            }
        }
        catch (Exception ex)
        {
            res = Fail(ex, nameof(BaoCao_ChamCong));
        }

        return res;
    }
}
