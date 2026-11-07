using HIA.Core.Base;
using HIA.Core.Models.Categories;
using HIA.Data.Catalogs;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace HIA.Services.Report;

public partial class ReportHandleService
{
    private const int Excel_ColWide = 256 * 72;
    private const int Excel_ColMild = 256 * 28;

    #region Private Static Methods
    // Same results as Uti.CodeMatch/CodeContain for every input, accepting nullable entity fields.
    private static bool MatchCode(string? value, string? pattern)
        => (value == null && pattern == null) || (value != null && pattern != null && Uti.CodeMatch(value, pattern));

    private static bool ContainsCode(string? value, string? pattern)
        => value != null && pattern != null && Uti.CodeContain(value, pattern);

    private static void AddDateHeader_ForWorkDays(ISheet xlSheet, int col, DateTime dte, string dayOfWeek, ICellStyle celStyle)
    {
        //Add header for date
        ICell xcel = xlSheet.GetRow(3).CreateCell(col);
        xcel.SetCellValue($"{dte:dd}");
        xcel.CellStyle = celStyle;

        xcel = xlSheet.GetRow(4).CreateCell(col);
        xcel.SetCellValue($"{dayOfWeek}");
        xcel.CellStyle = celStyle;
    }

    private static void AppendRow(ISheet sheet, ICellStyle[]? cellStyles, int rowIdx = 0, params object?[] values)
    {
        if (values.Length == 0) return;

        IRow row = sheet.CreateRow(rowIdx);
        for (int i = 0; i < values.Length; i++)
        {
            ICell cell = row.CreateCell(i);

            ICellStyle? style = cellStyles == null || cellStyles.Length < 1 ? null : cellStyles.Length > 5 ? cellStyles[3] : cellStyles[0];
            ICellStyle oStyle = sheet.GetColumnStyle(i);

            if (cellStyles != null && cellStyles.Length > 5)
            {
                if (oStyle.Alignment == HorizontalAlignment.Left)
                {
                    style = cellStyles[4];
                }
                else if (oStyle.Alignment == HorizontalAlignment.Right)
                {
                    style = cellStyles[5];
                }
            }
            cell.CellStyle = style;

            string val = values[i] is DateTime date ? ExcelDate.Format(date) : $"{values[i]}";
            if (val.StartsWith("$="))
            {
                cell.SetCellFormula(val[2..]);
            }
            else
            {
                cell.SetCellValue(val);
            }
        }
    }

    private static int CreateHeader(ISheet sheet, ICellStyle[] cellStyles, string title, params string[] columns)
    {
        int rIdx = 0;
        if (columns.Length == 0) return 0;

        if (!string.IsNullOrEmpty(title))
        {
            var cellRanged = new CellRangeAddress(0, 0, 0, columns.Length - 1);
            sheet.AddMergedRegion(cellRanged);

            IRow rCap = sheet.CreateRow(rIdx++);
            rCap.HeightInPoints = 36f;

            ICell cell = rCap.CreateCell(0);
            cell.SetCellValue(title.ToUpper());
            cell.CellStyle = cellStyles[0];
        }

        IRow row = sheet.CreateRow(rIdx);
        row.HeightInPoints = 24f;

        for (int i = 0; i < columns.Length; i++)
        {
            ICell cell = row.CreateCell(i);
            cell.SetCellValue(columns[i].ToUpper());
            cell.CellStyle = cellStyles[1];

            ICellStyle style = sheet.Workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            if (Uti.StrExists(columns[i], true, "tên", "địa chỉ", "chi tiết", "chuẩn đoán", "liều dùng"))
            {
                style.Alignment = HorizontalAlignment.Left;
            }
            if (Uti.StrExists(columns[i], true, "giá", "lượng", "tiền"))
            {
                style.Alignment = HorizontalAlignment.Right;
            }

            sheet.SetDefaultColumnStyle(i, style);
        }

        return columns.Length;
    }

    private static int CreateHeader_ForWorkDays(ISheet xlSheet, ICellStyle[] cellStyles, string title, params string[] rows)
    {
        IRow xrow = xlSheet.CreateRow(0);
        xrow.HeightInPoints = 36f;

        ICell xcel = xrow.CreateCell(0);
        xcel.SetCellValue(title.ToUpper());
        xcel.CellStyle = cellStyles[0];

        xrow = xlSheet.CreateRow(1);
        xcel = xrow.CreateCell(0);
        xcel.SetCellValue("Ghi chú: [S]: Sáng, [C]: Chiều, [T]: Tối, [X]: Có hoạt động, [-]: Không hoạt động");

        xlSheet.CreateRow(2);
        xrow = xlSheet.CreateRow(3);
        xrow.HeightInPoints = 24f;

        for (int i = 0; i < rows.Length; i++)
        {
            xcel = xrow.CreateCell(i);
            xcel.SetCellValue(rows[i].ToUpper());
            xcel.CellStyle = cellStyles[1];

            ICellStyle style = xlSheet.Workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            xlSheet.SetDefaultColumnStyle(i, style);
        }

        xrow = xlSheet.CreateRow(4);
        xrow.HeightInPoints = 24f;
        for (int i = 0; i < rows.Length; i++)
        {
            xcel = xrow.CreateCell(i);
            xcel.CellStyle = cellStyles[1];
            xlSheet.AddMergedRegion(new CellRangeAddress(3, 4, i, i));
        }

        return rows.Length - 1;
    }

    private static void CreateLookupSheet(IWorkbook workbook, ICellStyle[] cellStyles, IEnumerable<DMBacSi>? dmBacsis, IEnumerable<DMKhoaBV>? dmKhoas)
    {
        int rows, cols;
        Dictionary<string, string> dKhoa = new Dictionary<string, string>();

        #region Sheet phu: DM Khoa
        if (dmKhoas != null)
        {
            var dataK = dmKhoas.OrderBy(k => k.MA_KHOA);

            ISheet xlSheetK = workbook.CreateSheet("DM Khoa");
            rows = 1;
            cols = CreateHeader(xlSheetK, cellStyles, "", "Mã Khoa", "Tên Khoa");

            foreach (var x in dataK)
            {
                dKhoa[x.MA_KHOA ?? ""] = x.TEN_KHOA ?? "";
                AppendRow(xlSheetK, cellStyles, rows++, x.MA_KHOA, x.TEN_KHOA);
            }
            SetAutoFeatures(xlSheetK, cols);
        }
        #endregion

        #region Sheet phu: DM Bac si
        if (dmBacsis != null)
        {
            string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas?.Count() ?? 0) + 5},2,FALSE),\"\")";
            var dataBS = dmBacsis.OrderBy(b => b.MA_CCHN);
            ISheet xlSheetB = workbook.CreateSheet("DM Bác sĩ");
            rows = 2;
            cols = CreateHeader(xlSheetB, cellStyles, "", "Mã CNHN", "Tên Bác sĩ", "Chức danh", "Mã Khoa", "Tên Khoa", "Mã BHXH");

            foreach (var x in dataBS)
            {
                var khoa = (x.MA_KHOA ?? "").Split(';')[0].Trim();
                khoa = dKhoa.TryGetValue(khoa, out var ten) ? ten : "";
                AppendRow(xlSheetB, cellStyles, rows++, x.MA_CCHN, x.HO_TEN, x.CHUC_DANH, x.MA_KHOA, khoa, x.MA_BHXH);
            }
            SetAutoFeatures(xlSheetB, cols);
        }
        #endregion
    }

    private static IWorkbook CreateWorkBook(out ICellStyle[] cellStyles)
    {
        //Create new excel
        IWorkbook workbook = new XSSFWorkbook();

        //Create some predefined style for cell
        IFont boldFontTitle = workbook.CreateFont();
        boldFontTitle.IsBold = true;
        boldFontTitle.FontHeightInPoints = 20;

        IFont boldFont = workbook.CreateFont();
        boldFont.IsBold = true;
        boldFont.FontHeightInPoints = 13;

        cellStyles = new ICellStyle[] {
            workbook.CreateCellStyle(),
            workbook.CreateCellStyle(),
            workbook.CreateCellStyle(),
            workbook.CreateCellStyle(),
            workbook.CreateCellStyle(),
            workbook.CreateCellStyle(),
            workbook.CreateCellStyle()
        };

        //CellStyle 0 for Page Title
        cellStyles[0].SetFont(boldFontTitle);

        //CellStyle 1, 2 for Row Caption
        for (int i = 1; i < 3; i++)
        {
            cellStyles[i].SetFont(boldFont);
            cellStyles[i].FillPattern = FillPattern.SolidForeground;
        }

        cellStyles[1].FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;

        //CellStyle 2 for Row Caption in Sat & Sun
        cellStyles[2].FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;

        //Other CellStyle 3 to 5 for Row data
        //CellStyle 4 for align left, CellStyle 5 for align right
        for (int i = 0; i < cellStyles.Length; i++)
        {
            cellStyles[i].Alignment = i == 4 ? HorizontalAlignment.Left : i == 5 ? HorizontalAlignment.Right : HorizontalAlignment.Center;
            cellStyles[i].VerticalAlignment = i < 3 ? VerticalAlignment.Center : VerticalAlignment.Top;
            cellStyles[i].WrapText = true;

            if (i > 0)
            {
                cellStyles[i].BorderBottom = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
                cellStyles[i].BorderRight = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
                cellStyles[i].BorderTop = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
                cellStyles[i].BorderLeft = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
            }
        }

        //Other CellStyle 6 for Row data in Sat & Sun
        cellStyles[6].FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
        cellStyles[6].FillPattern = FillPattern.SolidForeground;

        return workbook;
    }

    private static DateTime GetCreateDate_Selected(string filterString)
    {
        var selDte = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        try
        {
            var from = JsonDefaults.Deserialize<FilterDownloadModel>(filterString)?.FromDateCreate;
            if (from != null)
            {
                selDte = new DateTime(from.Value.Year, from.Value.Month, 1);
            }
        }
        catch { }

        return selDte;
    }

    private static string[] GetFormular_BacSi(string[] fmBacsis, ref string? MA_BACSI)
    {
        string[] results = new string[fmBacsis.Length];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = "";
        }

        if (string.IsNullOrWhiteSpace(MA_BACSI)) return results;

        //Loai tru trng hop MA_BS co 2 chuoi
        MA_BACSI = MA_BACSI.Replace("; ", "$");
        string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lstBs.Length; i++)
        {
            lstBs[i] = lstBs[i].Trim().Replace("$", "; ");
            for (int k = 0; k < fmBacsis.Length; k++)
            {
                results[k] += (i == 0 ? "$=" : " & CHAR(10) & ") + string.Format(fmBacsis[k], lstBs[i]);
            }
        }

        MA_BACSI = string.Join(Environment.NewLine, lstBs);
        return results;
    }

    private static string GetFormular_KhoaTH(string fmBacsi, string? MA_BACSI)
    {
        if (string.IsNullOrWhiteSpace(MA_BACSI)) return "";

        //Loai tru trng hop MA_BS co 2 chuoi
        MA_BACSI = MA_BACSI.Replace("; ", "$");
        string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
        return "$=" + string.Format(fmBacsi, lstBs.Length > 0 ? lstBs[0] : "");
    }

    private static string GetFormular_TenKhoa(string fmKhoa, string? MA_KHOA, bool getLast = false)
    {
        string result = "";
        if (string.IsNullOrWhiteSpace(MA_KHOA)) return result;
        if (fmKhoa.StartsWith("$="))
        {
            fmKhoa = fmKhoa[2..];
        }

        string[] lstKs = MA_KHOA.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (getLast)
            lstKs = new string[] { lstKs.Last().Trim() };

        if (lstKs.Length < 1)
            return "";

        for (int i = 0; i < lstKs.Length; i++)
        {
            lstKs[i] = lstKs[i].Trim();
            result += (i == 0 ? "$=" : " & \"; \" & ") + string.Format(fmKhoa, lstKs[i]);
        }
        return result;
    }

    // Moi ngay trong bao cao (ca ngay sinh) cung dinh dang ExcelDate.OutputFormat; null -> chuoi rong.
    // Tham so noTime giu de tuong thich cac cho goi, khong con tac dung.
    private static string GetNgayThang(DateTime? ngayThang, bool noTime = false)
        => ngayThang == null ? string.Empty : ExcelDate.Format(ngayThang.Value);

    private static string GetType_BacSi(string? CHUCDANH_NN)
        => int.TryParse(CHUCDANH_NN, out int idx) && idx > 0 && idx < ServiceConstants.ALL_CHUC_DANH.Length ? ServiceConstants.ALL_CHUC_DANH[idx - 1] : "";

    private static string GetType_GioiTinh(byte? gioiTinh)
        => gioiTinh == 1 ? "Nam" : gioiTinh == 2 ? "Nữ" : "";

    //Linhist added 24/01/26
    private static string GetWorkPeriod(DateTime? date, TimePeriod period)
    {
        if (date == null) return "";

        if (period.IsValid)
        {
            if (period.InRange(date)) return date.Value.Hour < 13 ? "S" : "C";
            return "T";
        }

        if (date.Value.Hour >= 7 && date.Value.Hour < 13) return "S";
        else if (date.Value.Hour >= 13 && date.Value.Hour < 17) return "C";
        else return "T";
    }

    //Linhist added 24/01/26
    private static string FixPeriod(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "N";

        var res = "";
        if (value.Contains('S')) res = "S";
        if (value.Contains('C')) res += (res.Length > 0 ? "-" : "") + "C";
        if (value.Contains('T')) res += (res.Length > 0 ? "-" : "") + "T";
        return res;
    }

    private static void SetAutoFeatures(ISheet sheet, int cols, params int[] widens)
    {
        for (int i = 0; i < cols; i++)
        {
            if (widens.Contains(i))
            {
                sheet.SetColumnWidth(i, Excel_ColMild);
                continue;
            }
            else if (widens.Contains(-i))
            {
                sheet.SetColumnWidth(i, Excel_ColWide);
                continue;
            }

            sheet.AutoSizeColumn(i);
        }
    }
    #endregion
}
