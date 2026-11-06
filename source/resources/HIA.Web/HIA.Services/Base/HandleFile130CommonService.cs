using HIA.Core.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Files130;
using HIA.DatabaseModel.Entites;
using HIA.Services.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using NPOI.HPSF;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using static Z.EntityFramework.Extensions.BatchUpdate;

namespace HIA.Services.Base
{
    public class HandleFile130CommonService
    {
        private static Dictionary<Type, string[][]> FieldMappings;

        static HandleFile130CommonService()
        {
            FieldMappings = new()
            {
                {
                    typeof(DMThuoc),
                    new string[][]
                    {
                        "DEN_NGAY,DENNGAY".Split(','),
                        "DEN,DEN_NGAY".Split(',')
                    }
                },
                {
                    typeof(DMDichVu),
                    new string[][]
                    {
                        "MA_TUONG_DUONG,MA_NHOM_5937,TEN_DVKT_PHEDUYET,QUY_TRINH,TUNGAY,DENNGAY".Split(','),
                        "MA_DICH_VU,MANHOM_5937,TEN_DICH_VU,QUYET_DINH,TU_NGAY,DEN_NGAY".Split(','),
                    }
                },
                {
                    typeof(DMDVThuCong),
                    new string[][]
                    {
                        "MA_TUONG_DUONG,TEN_DVKT_PHEDUYET,QUY_TRINH,TUNGAY,DENNGAY".Split(','),
                        "MA_DICH_VU,TEN_DICH_VU,QUYET_DINH,TU_NGAY,DEN_NGAY".Split(','),
                    }
                },
                {
                    typeof(DMVTYT),
                    new string[][]
                    {
                        "HANG_SX,DENNGAY".Split(','),
                        "NHA_SX,DEN_NGAY".Split(','),
                    }
                },
                {
                    typeof(DMBacSi),
                    new string[][]
                    {
                        "SODINHDANH,SO_DINH_DANH,TUNGAY,DENNGAY,VITRI,VI_TRI,CHUCDANH,CHUCDANH_NN,MACCHN,THOIGIAN_DK,THOIGIAN_NGAY,THOIGIAN_TUAN".Split(','),
                        "SO_CCCD,SO_CCCD,TU_NGAY,DEN_NGAY,MA_VI_TRI,MA_VI_TRI,CHUC_DANH,CHUC_DANH_NN,MA_CCHN,TG_DANGKY,TG_NGAY,TG_TUAN".Split(',')
                    }
                },
                {
                    typeof(DMKhoaBV),
                    new string[][]
                    {
                        "MA,TEN".Split(','),
                        "MA_KHOA,TEN_KHOA".Split (','),
                    }
                },
                {
                    typeof(DMThietBi),
                    new string[][]
                    {
                        "TEN_TB".Split(','),
                        "TEN_THIET_BI".Split(','),
                    }
                },
                {
                    typeof(DichVuMaMay),
                    new string[][]
                    {
                        new string[0],
                        new string[0],
                    }
                }
            };
        }

        static public List<T> UploadExcel<T>(IFormFile request, List<string> keysString)
        {
            //List<T> data;

            DataTable dtTable = new DataTable();
            List<string> rowList = new List<string>();
            ISheet sheet;
            using (var stream = new MemoryStream())
            {
                request.CopyTo(stream);
                stream.Position = 0;
                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
                sheet = xssWorkbook.GetSheetAt(0);
                IRow headerRow = sheet.GetRow(0);
                int cellCount = headerRow.LastCellNum;
                List<int> ignoreColumns = new List<int>();

                //Linhist added 09/01/23
                //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
                int colNhom9 = -1;
                int colNhom5 = -1;
                int colHamLuong = -1;

                string[][] flds = FieldMappings[typeof(T)];
                for (int j = 0; j < cellCount; j++)
                {
                    ICell cell = headerRow.GetCell(j);
                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                    if (keysString.Contains(cell.ToString()))
                    {
                        ignoreColumns.Add(j);
                        continue;
                    }

                    string capt = Uti.ReplaceValuePairs(cell.ToString(), flds);
                    if (!dtTable.Columns.Contains(capt))
                    {
                        dtTable.Columns.Add(capt);
                    }

                    //Linhist added 09/01/23
                    //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
                    if (Uti.IsEqual(cell.ToString().Trim(), "MANHOM_9324", ignoreCase: true))
                    {
                        colNhom9 = dtTable.Columns.Count - 1;
                    }
                    if (Uti.IsEqual(cell.ToString().Trim(), "MANHOM_5937", ignoreCase: true))
                    {
                        colNhom5 = dtTable.Columns.Count - 1;
                    }

                    //Linhist added 09/01/23
                    if (Uti.IsEqual(cell.ToString().Trim(), "HAM_LUONG", ignoreCase: true))
                    {
                        colHamLuong = j;
                    }

                    //Linhist removed 06/03/24
                    //Linhist added 08/03/23
                    //Dat la ten cot MA --> MA_KHOA, TEN --> TEN_KHOA trong bang DMKhoaBV
                    //if (Uti.IsEqual(cell.ToString().Trim(), "MA", ignoreCase: true))
                    //{
                    //    dtTable.Columns[dtTable.Columns.Count - 1].ColumnName = "MA_KHOA";
                    //}
                    //if (Uti.IsEqual(cell.ToString().Trim(), "TEN", ignoreCase: true))
                    //{
                    //    dtTable.Columns[dtTable.Columns.Count - 1].ColumnName = "TEN_KHOA";
                    //}
                }

                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (ignoreColumns.Contains(j)) continue;

                        if (colHamLuong > -1 && j == colHamLuong)
                        {
                            rowList.Add(row.GetCell(j)?.ToString().Trim());
                        }
                        else if (row.GetCell(j) != null)
                        {
                            var cellValue = row.GetCell(j)?.ToString().Trim();
                            if (double.TryParse(cellValue, out double numericValue))
                                cellValue = cellValue.Replace(',', '.');

                            rowList.Add(cellValue);
                        }
                        else
                        {
                            rowList.Add(null);
                        }
                    }
                    if (rowList.Count > 0)
                    {
                        //Linhist added 09/01/23
                        //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
                        if (colNhom5 != -1 && colNhom9 != -1)
                        {
                            if (rowList[colNhom5] == null || rowList[colNhom5].ToString() == "")
                            {
                                rowList[colNhom5] = rowList[colNhom9];
                            }
                        }

                        dtTable.Rows.Add(rowList.ToArray());
                    }
                    rowList.Clear();
                }
            }

            return JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(dtTable));
        }

        static public byte[] DownloadExcel<T>(IEnumerable<T> data)
        {
            DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data), (typeof(DataTable)));
            byte[] result;
            using (var fs = new MemoryStream())
            {
                // xlsx: XSSFWorkbook  --- xls: HSSFWorkbook  
                IWorkbook workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Sheet1");

                List<string> columns = new List<string>();
                IRow row = excelSheet.CreateRow(0);
                int columnIndex = 0;

                foreach (DataColumn column in table.Columns)
                {
                    columns.Add(column.ColumnName);
                    row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
                    columnIndex++;
                }

                int rowIndex = 1;
                foreach (DataRow dsrow in table.Rows)
                {
                    row = excelSheet.CreateRow(rowIndex);
                    int cellIndex = 0;
                    foreach (string col in columns)
                    {
                        row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
                        cellIndex++;
                    }

                    rowIndex++;
                }
                workbook.Write(fs);
                result = fs.ToArray();
            }

            return result;
        }

        private static int _excelColWide = 256 * 72;
        private static int _excelColMild = 256 * 28;
        private static string[] _chucDanhs = new string[] { "Bác sĩ", "Y sĩ", "Điều dưỡng", "Hộ sinh", "Kỹ thuật viên", "Cử nhân X-quang", "Dược sĩ Đại học", "Dược sĩ Trung cấp" };

        #region Private methods
        private static string LoaiBS(string CHUCDANH_NN)
            => int.TryParse(CHUCDANH_NN, out int idx) && idx > 0 && idx < _chucDanhs.Length ? _chucDanhs[idx - 1] : "";

        private static string GetGioiTinh(byte? gioiTinh)
            => gioiTinh == 1 ? "Nam" : (gioiTinh == 2 ? "Nữ" : "");

        private static string GetNgayThang(DateTime? ngayThang, bool noTime = false)
            => noTime ? $"{ngayThang:dd/MM/yyyy}" : $"{ngayThang:dd/MM/yyyy HH:mm:ss}";

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

            cellStyles = new ICellStyle[] { workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle() };

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
                cellStyles[i].Alignment = (i == 4 ? HorizontalAlignment.Left : (i == 5 ? HorizontalAlignment.Right : HorizontalAlignment.Center));
                cellStyles[i].VerticalAlignment = (i < 3 ? VerticalAlignment.Center : VerticalAlignment.Top);
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

        private static int CreateHeader(ISheet sheet, ICellStyle[] cellStyles, string title, params string[] columns)
        {
            int rIdx = 0;
            if (columns == null || columns.Length == 0) return 0;

            if (!string.IsNullOrEmpty(title))
            {
                var cellRanged = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, columns.Length - 1);
                sheet.AddMergedRegion(cellRanged);

                IRow rCap = sheet.CreateRow(rIdx++);
                rCap.HeightInPoints = 36f;

                ICell cell = rCap.CreateCell(0);
                cell.SetCellValue(title?.ToUpper());
                cell.CellStyle = cellStyles[0];
            }

            IRow row = sheet.CreateRow(rIdx);
            row.HeightInPoints = 24f;

            for (int i = 0; i < columns.Length; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(columns[i]?.ToUpper());
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

        static private int CreateHeaderForWorkDays(ISheet xlSheet, DateTime dStart, ICellStyle[] cellStyles)
        {
            IRow xrow = xlSheet.CreateRow(0);
            xrow.HeightInPoints = 36f;

            ICell xcel = xrow.CreateCell(0);
            xcel.SetCellValue($"Bảng Chấm công trong Tháng {dStart:MM} năm {dStart.Year}".ToUpper());
            xcel.CellStyle = cellStyles[0];

            xrow = xlSheet.CreateRow(1);
            xcel = xrow.CreateCell(0);
            xcel.SetCellValue("Ghi chú: [Đ]: Đi làm, [N]: Nghỉ");

            xlSheet.CreateRow(2);
            xrow = xlSheet.CreateRow(3);
            xrow.HeightInPoints = 24f;

            string[] caps = new string[] { "STT", "Mã CCHN", "Họ tên", "Chức danh", "Mã khoa", "Tên khoa" };
            for (int i = 0; i < caps.Length; i++)
            {
                xcel = xrow.CreateCell(i);
                xcel.SetCellValue(caps[i].ToUpper());
                xcel.CellStyle = cellStyles[1];

                ICellStyle style = xlSheet.Workbook.CreateCellStyle();
                style.Alignment = HorizontalAlignment.Center;
                if (i == 2 || i == 3)
                {
                    style.Alignment = HorizontalAlignment.Left;
                }

                xlSheet.SetDefaultColumnStyle(i, style);
            }

            xrow = xlSheet.CreateRow(4);
            xrow.HeightInPoints = 24f;
            for (int i = 0; i < caps.Length; i++)
            {
                xcel = xrow.CreateCell(i);
                xcel.CellStyle = cellStyles[1];
                xlSheet.AddMergedRegion(new CellRangeAddress(3, 4, i, i));
            }

            return caps.Length - 1;
        }

        static private void AddDateHeaderForWorkDays(ISheet xlSheet, int col, DateTime dte, string dayOfWeek, ICellStyle celStyle)
        {
            //Add header for date
            ICell xcel = xlSheet.GetRow(3).CreateCell(col);
            xcel.SetCellValue($"{dte:dd}");
            xcel.CellStyle = celStyle;

            xcel = xlSheet.GetRow(4).CreateCell(col);
            xcel.SetCellValue($"{dayOfWeek}");
            xcel.CellStyle = celStyle;
        }

        private static void AppendRow(ISheet sheet, ICellStyle[] cellStyles, int rowIdx = 0, params object[] values)
        {
            if (values == null || values.Length == 0) return;

            IRow row = sheet.CreateRow(rowIdx);
            for (int i = 0; i < values.Length; i++)
            {
                ICell cell = row.CreateCell(i);

                ICellStyle style = (cellStyles == null || cellStyles.Length < 1 ? null : (cellStyles.Length > 5 ? cellStyles[3] : cellStyles?[0]));
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

                string val = $"{values[i]}";
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

        private static string[] GetFormularBacSi_New(string[] fmBacsis, ref string MA_BACSI)
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

        private static string GetFormularKhoaTH(string fmBacsi, string MA_BACSI)
        {
            if (string.IsNullOrWhiteSpace(MA_BACSI)) return "";

            //Loai tru trng hop MA_BS co 2 chuoi
            MA_BACSI = MA_BACSI.Replace("; ", "$");
            string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return "$=" + string.Format(fmBacsi, lstBs.Length > 0 ? lstBs[0] : "");
        }

        private static void SetAutoFeatures(ISheet sheet, int cols, params int[] widens)
        {
            for (int i = 0; i < cols; i++)
            {
                if (widens != null)
                {
                    if (widens.Contains(i))
                    {
                        sheet.SetColumnWidth(i, _excelColMild);
                        continue;
                    }
                    else if (widens.Contains(-i))
                    {
                        sheet.SetColumnWidth(i, _excelColWide);
                        continue;
                    }
                }

                sheet.AutoSizeColumn(i);
            }
        }

        private static void CreateLookupSheet(IWorkbook workbook, ICellStyle[] cellStyles, IEnumerable<DMBacSi> dmBacsis, IEnumerable<DMKhoaBV> dmKhoas)
        {
            int rows = 1, cols = 0;
            #region Sheet phu: DM Khoa
            if (dmKhoas != null)
            {
                var dataK = dmKhoas.OrderBy(k => k.MA_KHOA);

                ISheet xlSheetK = workbook.CreateSheet("DM Khoa");
                rows = 1;
                cols = CreateHeader(xlSheetK, cellStyles, "", "Mã Khoa", "Tên Khoa");

                foreach (var x in dataK)
                {
                    AppendRow(xlSheetK, cellStyles, rows++, x.MA_KHOA, x.TEN_KHOA);
                }
                SetAutoFeatures(xlSheetK, cols);
            }
            #endregion

            #region Sheet phu: DM Bac si
            if (dmBacsis != null)
            {
                var dataBS = dmBacsis.OrderBy(b => b.MA_CCHN);

                ISheet xlSheetB = workbook.CreateSheet("DM Bác sĩ");
                rows = 2;
                cols = CreateHeader(xlSheetB, cellStyles, "", "Mã CNHN", "Tên Bác sĩ", "Chức danh", "Mã Khoa");

                foreach (var x in dataBS)
                {
                    //AppendRow(xlSheetB, cellStyles, rows++, x.MACCHN, x.HO_TEN, LoaiBS(x.CHUCDANH_NN), x.MA_KHOA);
                    AppendRow(xlSheetB, cellStyles, rows++, x.MA_CCHN, x.HO_TEN, x.CHUC_DANH, x.MA_KHOA);
                }
                SetAutoFeatures(xlSheetB, cols);
            }
            #endregion
        }
        #endregion

        static public byte[] Download_BaoCaoTongHop(IEnumerable<XML01> x01, IEnumerable<XMLError> dataE, IEnumerable<XMLErrorInfo> dmErrors, IEnumerable<DMKhoaBV> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
        {
            try
            {
                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                var allErrors = dataE;
                var allErrCodes = allErrors.GroupBy(x => x.MA_LOI, (k, g) => new { ErrorCode = k, Count = g.Count() });
                var grpErrors = allErrCodes.Join(dmErrors, x => x.ErrorCode, y => y.error_code, (x, y) => new
                {
                    y.error_code,
                    y.error_file,
                    y.error_tag,
                    x.Count,
                    y.message,
                    y.is_alert
                }).OrderBy(e => e.error_file);

                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
                string fmKhoaTH = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:D{(dmBacsis.Count() + 5)},4,FALSE),\"\")";
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";
                string fmError = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:D{(grpErrors.Count() + 5)},3,FALSE),\"\")";
                string fmEtype = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:C{(grpErrors.Count() + 5)},2,FALSE),\"\")";
                string fmEtag = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:F{(grpErrors.Count() + 5)},5,FALSE),\"\")";

                byte[] result = null;
                using (var fs = new MemoryStream())
                {
                    #region Sheet 1: DS File Bao cao
                    var grpFiles = x01.OrderBy(x => x.FileName);
                    ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");

                    int rows = 2;
                    int cols = CreateHeader(xlSheetF, cellStyles,
                                $"Báo Cáo Tổng Hợp {x01.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                                "STT", "Tên File", "Mã CSKCB",
                                "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính", "Địa chỉ",
                                "Mã Khoa", "Tên Khoa",
                                "Ngày vào", "Ngày ra", "Ngày lập");

                    foreach (var x in grpFiles)
                    {
                        AppendRow(xlSheetF, cellStyles, rows++,
                                    rows - 2, x.FileName, x.MA_CSKCB,
                                    x.MA_LK, x.MA_BN, x.HO_TEN,
                                    x.MA_THE_BHYT, GetNgayThang(x.NGAY_SINH, true), GetGioiTinh(x.GIOI_TINH), x.DIA_CHI,
                                    x.MA_KHOA, string.Format(fmKhoa, x.MA_KHOA),
                                    GetNgayThang(x.NGAY_VAO), GetNgayThang(x.NGAY_RA), GetNgayThang(x.CreateDate));
                    }

                    SetAutoFeatures(xlSheetF, cols, -1, 11);
                    #endregion


                    #region Sheet 2: Tong hop Loi
                    ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Lỗi");
                    rows = 2;
                    cols = CreateHeader(xlSheetE, cellStyles,
                                        "Tổng hợp số lỗi trong file hồ sơ", // Sheet title
                                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi", "Loại File lỗi", "Lỗi tại Thẻ", "Số lỗi");

                    foreach (var x2 in grpErrors)
                    {
                        AppendRow(xlSheetE, cellStyles, rows++,
                                    rows - 2, x2.error_code, x2.is_alert ? "CẢNH BÁO" : "BÁO LỖI", x2.message, x2.error_file, x2.error_tag, x2.Count);
                    }

                    SetAutoFeatures(xlSheetE, cols, 2, -3);
                    #endregion


                    #region Sheet 3: DS Loi XML
                    ISheet xlSheet1 = workbook.CreateSheet("XML1");
                    rows = 2;
                    cols = CreateHeader(xlSheet1, cellStyles,
                                "Danh sách lỗi XML1", // Sheet title
                                "HS lỗi", "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", 
                                
                                
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân","Số CCCD","Ngày sinh","Giới tính","nhóm máu",
                                "Mã quốc tịch","Mã dân tộc","Mã nghề nghiệp","Địa chỉ","Mã tỉnh cư trú","Mã Huyện","Mã xã cư trú","Điện thoại",
                                "Mã Thẻ", "Mã ĐKKCBBĐ","Thẻ từ","Thẻ đến","Ngày miễn CCT","Lý do vào viện","Lý do vào viện nội trú","Mã lý do vào viện nội trú",
                                "Chẩn đoán vào","Chẩn đoán ra","Mã bệnh chính","Mã bệnh kèm theo","Mã bệnh YHCT","Mã PTTT QT","Mã đối tượng KCB","Mã nơi đi","Mã nơi đến","Mã tai nạn",
                                "Ngày vào","ngày vào nội trú","ngày ra","giấy chuyển tuyến","Số ngày điều trị","PP Điều trị","Kết quả DT","Mã loại ra viện","Ghi chú",
                                "Ngày thanh toán","Tiền thuốc","Tiền VTTY",
                                "Tổng chi BV","Tổng chi BH", "Tiền BNTT",
                                "Tiền BNCTT","Tiền BHTT","Tiền Nguồn khác",
                                "Tiền BHTT GDV","Năm QT","Tháng QT","Mã loại KCB",
                                "Mã Khoa","Tên Khoa", "Mã CSKCB","Mã khu vực","Cân nặng","Cân nặng con","Năm năm liên tục","ngày tái khám","Mã hồ sơ bệnh án",
                                "mã TTDV"
                                );

                    //for (var i = 1; i < 16; i++)
                    //{
                        List<int?> errCodes = dmErrors.Where(e => e.error_file == "XML1").Select(e => e.error_code).ToList();
                        var errors = allErrors.Where(a => errCodes.Contains(a.MA_LOI));

                        foreach (var x in errors)
                        {
                            AppendRow(xlSheet1, cellStyles, rows++,
                                    $"XML1", x.STT, x.MA_LOI, string.Format(fmEtype, x.MA_LOI), string.Format(fmEtag, x.MA_LOI), string.Format(fmError, x.MA_LOI),
                                    x.XML01.MA_LK, x.XML01.MA_BN, x.XML01.HO_TEN,x.XML01.SO_CCCD,GetNgayThang(x.XML01.NGAY_SINH),x.XML01.GIOI_TINH,x.XML01.NHOM_MAU,
                                    x.XML01.MA_QUOCTICH,x.XML01.MA_DANTOC,x.XML01.MA_NGHE_NGHIEP,x.XML01.DIA_CHI,x.XML01.MATINH_CU_TRU,x.XML01.MAHUYEN_CU_TRU,x.XML01.MAXA_CU_TRU,x.XML01.DIEN_THOAI,
                                    x.XML01.MA_THE_BHYT, x.XML01.MA_DKBD,x.XML01.GT_THE_TU,x.XML01.GT_THE_DEN,x.XML01.NGAY_MIEN_CCT,x.XML01.LY_DO_VV,x.XML01.LY_DO_VNT,x.XML01.MA_LY_DO_VNT,
                                    x.XML01.CHAN_DOAN_VAO,x.XML01.CHAN_DOAN_RV,x.XML01.MA_BENH_CHINH,x.XML01.MA_BENH_KT,x.XML01.MA_BENH_YHCT,x.XML01.MA_PTTT_QT,x.XML01.MA_DOITUONG_KCB,x.XML01.MA_NOI_DI,x.XML01.MA_NOI_DEN,x.XML01.MA_TAI_NAN,
                                    GetNgayThang(x.XML01.NGAY_VAO),GetNgayThang(x.XML01.NGAY_VAO_NOI_TRU),GetNgayThang(x.XML01.NGAY_RA),x.XML01.GIAY_CHUYEN_TUYEN,x.XML01.SO_NGAY_DTRI,x.XML01.PP_DIEU_TRI,x.XML01.KET_QUA_DTRI,x.XML01.MA_LOAI_RV,x.XML01.GHI_CHU,
                                    GetNgayThang(x.XML01.NGAY_TTOAN), string.Format("{0:##,##0.0#}", x.XML01.T_THUOC).Replace(".", "$").Replace(",", ".").Replace("$", ","), string.Format("{0:##,##0.0#}", x.XML01.T_VTYT).Replace(".", "$").Replace(",", ".").Replace("$", ","),
                                    string.Format("{0:##,##0.0#}", x.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","), string.Format("{0:##,##0.0#}", x.XML01.T_TONGCHI_BH).Replace(".", "$").Replace(",", ".").Replace("$", ","), string.Format("{0:##,##0.0#}", x.XML01.T_BNTT).Replace(".", "$").Replace(",", ".").Replace("$", ","),
                                    string.Format("{0:##,##0.0#}", x.XML01.T_BNCCT).Replace(".", "$").Replace(",", ".").Replace("$", ","), string.Format("{0:##,##0.0#}", x.XML01.T_BHTT).Replace(".", "$").Replace(",", ".").Replace("$", ","), string.Format("{0:##,##0.0#}", x.XML01.T_NGUONKHAC).Replace(".", "$").Replace(",", ".").Replace("$", ","),
                                    string.Format("{0:##,##0.0#}", x.XML01.T_BHTT_GDV).Replace(".", "$").Replace(",", ".").Replace("$", ","),x.XML01.NAM_QT,x.XML01.THANG_QT,x.XML01.MA_LOAI_KCB,
                                    x.XML01.MA_KHOA, string.Format(fmKhoa, x.XML01.MA_KHOA), x.XML01.MA_CSKCB,x.XML01.MA_KHUVUC,x.XML01.CAN_NANG,x.XML01.CAN_NANG_CON,x.XML01.NAM_NAM_LIEN_TUC,x.XML01.NGAY_TAI_KHAM,x.XML01.MA_HSBA,
                                    x.XML01.MA_TTDV
                                    
                                    );
                        }
                    //}

                    SetAutoFeatures(xlSheet1, cols, 3, -5, 13, -16);
                    #endregion

                    #region Sheet 4: DS Loi X02
                    List<int?> X02ErCodes = dmErrors.Where(e => e.error_file == "XML2").Select(e => e.error_code).ToList();
                    var X02Errors = allErrors.Where(a => X02ErCodes.Contains(a.MA_LOI));

                    var data2Join =
                        x01.SelectMany(x => x.XML02s)
                        .Join(X02Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X02 = x, XER = e })
                        .Where(x => x.X02.STT == x.XER.STT);

                    ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML02");
                    rows = 2;
                    cols = CreateHeader(xlSheet2, cellStyles,
                                "Danh sách lỗi của XML02", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa",
                                "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh",
                                "Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện",
                                "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
                                "TT Thầu", "Đơn giá", "Số lượng", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh","Ngày thực hiên Y Lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                                "Tổng Chi");

                    foreach (var x in data2Join)
                    {
                        string maBS = x.X02.MA_BAC_SI;
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet2, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X02.XML01.MA_LK, x.X02.XML01.MA_BN, x.X02.XML01.HO_TEN,
                                x.X02.XML01.MA_THE_BHYT, GetNgayThang(x.X02.XML01.NGAY_SINH, true), GetGioiTinh(x.X02.XML01.GIOI_TINH),
                                x.X02.XML01.MA_KHOA, string.Format(fmKhoa, x.X02.XML01.MA_KHOA),
                                maBS, ttBS[0], ttBS[1], x.X02.MA_KHOA,
                                x.X02.XML01.MA_BENH_CHINH, x.X02.XML01.MA_BENH_KT, x.X02.XML01.CHAN_DOAN_RV,
                                x.X02.MA_THUOC, x.X02.MA_NHOM, x.X02.TEN_THUOC, x.X02.DON_VI_TINH, x.X02.LIEU_DUNG, x.X02.SO_DANG_KY,
                                x.X02.TT_THAU, x.X02.DON_GIA, x.X02.SO_LUONG, x.X02.MUC_HUONG, x.X02.TYLE_TT_BH,  x.X02.THANH_TIEN_BV,
                                GetNgayThang(x.X02.NGAY_YL), GetNgayThang(x.X02.NGAY_TH_YL), GetNgayThang(x.X02.XML01.NGAY_VAO), GetNgayThang(x.X02.XML01.NGAY_RA), GetNgayThang(x.X02.XML01.NGAY_TTOAN),
                                string.Format("{0:##,##0.0#}", x.X02.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","));
                    }

                    SetAutoFeatures(xlSheet2, cols, 2, -4, 12, 14, 15, -19);
                    #endregion

                    #region Sheet 5: Loi XML3
                    List<int?> xml3ErCodes = dmErrors.Where(e => e.error_file == "XML3").Select(e => e.error_code).ToList();
                    var xml3Errors = allErrors.Where(a => xml3ErCodes.Contains(a.MA_LOI));

                    var data3Join =
                        x01.SelectMany(x => x.XML03s)
                        .Join(xml3Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X03 = x, XER = e })
                        .Where(x => x.X03.STT == x.XER.STT);

                    ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML03");
                    rows = 2;
                    cols = CreateHeader(xlSheet3, cellStyles,
                                "Danh sách lỗi của XML03", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa","Phạm vi",
                                "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh", "Khoa Thực hiện", "Mã Người thực hiện", "Tên người thực hiện",
                                "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư", "Mã máy",
                                "Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện",
                                "ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh","Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                                "Tổng Chi");

                    foreach (var x in data3Join)
                    {
                        string maBS = x.X03.MA_BAC_SI;
                        string maBSTH = x.X03.NGUOI_THUC_HIEN;
                        string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);
                        string[] ttBStt = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBSTH);
                        AppendRow(xlSheet3, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X03.XML01.MA_LK, x.X03.XML01.MA_BN, x.X03.XML01.HO_TEN,
                                x.X03.XML01.MA_THE_BHYT, GetNgayThang(x.X03.XML01.NGAY_SINH, true), GetGioiTinh(x.X03.XML01.GIOI_TINH),
                                x.X03.XML01.MA_KHOA, string.Format(fmKhoa, x.X03.XML01.MA_KHOA),x.X03.PHAM_VI,
                                maBS, ttBS[0], ttBS[1], x.X03.MA_KHOA, tenKTH, maBSTH, ttBStt[0],
                                x.X03.MA_DICH_VU, x.X03.TEN_DICH_VU, x.X03.MA_VAT_TU, x.X03.TEN_VAT_TU, x.X03.MA_MAY,
                                x.X03.XML01.MA_BENH_CHINH, x.X03.XML01.MA_BENH_KT, x.X03.XML01.CHAN_DOAN_RV,
                                x.X03.DON_VI_TINH, x.X03.TT_THAU, x.X03.DON_GIA_BV, x.X03.MUC_HUONG, x.X03.TYLE_TT_BH, x.X03.THANH_TIEN_BV,
                                GetNgayThang(x.X03.NGAY_YL),GetNgayThang(x.X03.NGAY_TH_YL), GetNgayThang(x.X03.NGAY_KQ), GetNgayThang(x.X03.XML01.NGAY_VAO), GetNgayThang(x.X03.XML01.NGAY_RA), GetNgayThang(x.X03.XML01.NGAY_TTOAN),
                                string.Format("{0:##,##0.0#}", x.X03.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","));
                    }

                    SetAutoFeatures(xlSheet3, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Sheet 6: Loi XML4
                    List<int?> xml4ErCodes = dmErrors.Where(e => e.error_file == "XML4").Select(e => e.error_code).ToList();
                    var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.MA_LOI));
                    
                    var data4Join =
                        x01.SelectMany(x => x.XML04s)
                        .Join(xml4Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X04 = x, XER = e })
                        .Where(x => x.X04.STT == x.XER.STT);

                    ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML04");
                    rows = 2;
                    cols = CreateHeader(xlSheet4, cellStyles,
                                "Danh sách lỗi của XML04", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                //"Mã Khoa", "Tên Khoa",
                               // "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", //"Khoa y lệnh", "Khoa Thực hiện", "Mã Người thực hiện", "Tên người thực hiện",
                                "Mã Dịch vụ", //"Tên Dịch vụ", //"Mã Vật tư", "Tên Vật tư",
                                //"Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện",
                                //"ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                                //"Ngày Y lệnh", "Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                                //"Tổng Chi"
                                "Mã chỉ số","Tên chỉ số","Giá trị","Đơn vị đo","Mô tả", "Kết luận", "Ngày kết quả", "Mã Bác sĩ đọc kết quả", "Tên bác sĩ đọc kết quả", "Chức Danh"
                                );

                    foreach (var x in data4Join)
                    {
                        string maBS = x.X04.MA_BS_DOC_KQ;
                        
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);
                        
                        AppendRow(xlSheet4, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X04.XML01.MA_LK, x.X04.XML01.MA_BN, x.X04.XML01.HO_TEN,
                                x.X04.XML01.MA_THE_BHYT, GetNgayThang(x.X04.XML01.NGAY_SINH, true), GetGioiTinh(x.X04.XML01.GIOI_TINH),
                                //x.X04.XML01.MA_KHOA, string.Format(fmKhoa, x.X04.XML01.MA_KHOA),
                               // maBS, ttBS[0], ttBS[1],// x.X03.MA_KHOA, tenKTH, maBSTH, ttBStt[0],
                                x.X04.MA_DICH_VU,// x.X03.TEN_DICH_VU, x.X03.MA_VAT_TU, x.X03.TEN_VAT_TU,
                                //x.X03.XML01.MA_BENH_CHINH, x.X03.XML01.MA_BENH_KT, x.X03.XML01.CHAN_DOAN_RV,
                                //x.X03.DON_VI_TINH, x.X03.TT_THAU, x.X03.DON_GIA_BV, x.X03.MUC_HUONG, x.X03.TYLE_TT_BH, x.X03.THANH_TIEN_BV,
                                //GetNgayThang(x.X03.NGAY_YL), GetNgayThang(x.X03.NGAY_TH_YL), GetNgayThang(x.X03.NGAY_KQ), GetNgayThang(x.X03.XML01.NGAY_VAO), GetNgayThang(x.X03.XML01.NGAY_RA), GetNgayThang(x.X03.XML01.NGAY_TTOAN),
                                //string.Format("{0:##,##0.0#}", x.X03.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ",")
                                x.X04.MA_CHI_SO, x.X04.TEN_CHI_SO,x.X04.GIA_TRI,x.X04.DON_VI_DO,x.X04.MO_TA,x.X04.KET_LUAN,x.X04.NGAY_KQ, maBS, ttBS[0], ttBS[1]
                                );
                    }

                    SetAutoFeatures(xlSheet4, cols, 2, -4, 12, 14, 15, 17, -24);

                    #endregion

                    #region Sheet 7: Loi XML5
                    List<int?> xml5ErCodes = dmErrors.Where(e => e.error_file == "XML5").Select(e => e.error_code).ToList();
                    var xml5Errors = allErrors.Where(a => xml5ErCodes.Contains(a.MA_LOI));

                    var data5Join =
                        x01.SelectMany(x => x.XML05s)
                        .Join(xml5Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X05 = x, XER = e })
                        .Where(x => x.X05.STT == x.XER.STT);

                    ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML05");
                    rows = 2;
                    cols = CreateHeader(xlSheet5, cellStyles,
                                "Danh sách lỗi của XML05", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                //"Mã Thẻ", "Ngày sinh", "Giới tính",
                                //"Mã Khoa", "Tên Khoa",
                                //"Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh", "Khoa Thực hiện",
                                //"Mã Dịch vụ", /*"Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư",*/
                                //"Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán vào viện",
                                //"ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                                //"Ngày Y lệnh", "Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                                //"Tổng Chi"
                                "Diễn biến Lâm Sàng", "Giai đoạn bệnh", "Hội chẩn", "Phẫu thuật", "Thời điểm diễn biến lâm sàng", "người thực hiện");

                    foreach (var x in data5Join)
                    {
                        string maBS = x.X05.NGUOI_THUC_HIEN;
                        string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet5, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X05.XML01.MA_LK, x.X05.XML01.MA_BN, x.X05.XML01.HO_TEN,
                                //x.X03.XML01.MA_THE_BHYT, GetNgayThang(x.X03.XML01.NGAY_SINH, true), GetGioiTinh(x.X03.XML01.GIOI_TINH),
                                //x.X03.XML01.MA_KHOA, string.Format(fmKhoa, x.X03.XML01.MA_KHOA),
                                //maBS, ttBS[0], ttBS[1], x.X03.MA_KHOA, tenKTH,
                                //x.X04.MA_DICH_VU, /*x.X03.TEN_DICH_VU, x.X03.MA_VAT_TU, x.X03.TEN_VAT_TU,*/
                                //x.X03.XML01.MA_BENH_CHINH, x.X03.XML01.MA_BENH_KT, x.X03.XML01.CHAN_DOAN_VAO,
                                //x.X03.DON_VI_TINH, x.X03.TT_THAU, x.X03.DON_GIA_BV, x.X03.MUC_HUONG, x.X03.TYLE_TT_BH, x.X03.THANH_TIEN_BV,
                                //GetNgayThang(x.X03.NGAY_YL), GetNgayThang(x.X03.NGAY_TH_YL), GetNgayThang(x.X03.NGAY_KQ), GetNgayThang(x.X03.XML01.NGAY_VAO), GetNgayThang(x.X03.XML01.NGAY_RA), GetNgayThang(x.X03.XML01.NGAY_TTOAN),
                                //string.Format("{0:##,##0.0#}", x.X03.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ",")
                                x.X05.DIEN_BIEN_LS, x.X05.GIAI_DOAN_BENH, x.X05.HOI_CHAN, x.X05.PHAU_THUAT, GetNgayThang(x.X05.THOI_DIEM_DBLS), x.X05.NGUOI_THUC_HIEN
                                );
                    }

                    SetAutoFeatures(xlSheet5, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion


                    #region Báo cáo XML6
                    List<int?> xml6ErCodes = dmErrors.Where(e => e.error_file == "XML6").Select(e => e.error_code).ToList();
                    var xml6Errors = allErrors.Where(a => xml6ErCodes.Contains(a.MA_LOI));

                    var data6Join =
                        x01.SelectMany(x => x.XML06s)
                        .Join(xml6Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X06 = x, XER = e })
                        .Where(x => x.X06.STT == x.XER.STT);

                    ISheet xlSheet6 = workbook.CreateSheet("Lỗi XML06");
                    rows = 2;
                    cols = CreateHeader(xlSheet6, cellStyles,
                                "Danh sách lỗi của XML06", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Thời điểm khẳng định HIV","Thời điểm nhận thuốc ARV","Mã phác đồ điều trị","Mã bậc phác đồ điều trị","Mã lý do đăng ký","Mã loại điều trị lao","Mã phác đồ điều trị bệnh lao",
                                "Mã Cơ sở lấy mẫu xét nghiệm","Thời điểm bắt đầu điều trị lao","Nơi làm kết quả xét nghiệm","Thời điểm kết thúc điều trị lao","Nơi đầu tiên nhận thuốc ARV","Lý do chỉ định xét nghiệm đo tải lượng virus",
                                "Thời điểm lấy mẫu","Kết quả xét nghiệm tải lượng","Thời điểm kết quả xét nghiệm","Mã đối tượng", "Mã tình trạng","Phương pháp sàng lọc lao","Mã lần thực hiện xét nghiệm PCR","Ngày bệnh nhân thực hiện PCR",
                                "Ngày bệnh nhân có kết quả PCR","Mã kết quả xét nghiệm PCR","Mã kết quả điều trị Lao","Thời điểm ghi nhận thông tin mang thai","Thời điểm bắt đàu điều trị CTX","Mã xử trí","Ngày bắt đầu xử trí ARV","Ngày kết thúc xử lý ARV",
                                "Mã phác đồ điều trị HIV", "Mã giai đoạn lâm sàng", "Mã bậc phác đồ điều trị","Mã nhóm đối tượng","Số ngày thuốc ARV","Ngày chuyển phác đồ điều trị","Mã lý do chuyển phác đồ"
                               );

                    foreach (var x in data6Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet6, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X06.XML01.MA_LK, x.X06.XML01.MA_BN, x.X06.XML01.HO_TEN,
                                GetNgayThang(x.X06.NGAYKD_HIV),x.X06.BDDT_ARV,x.X06.MA_PHAC_DO_DIEU_TRI_BD,x.X06.MA_BAC_PHAC_DO_BD,x.X06.MA_LYDO_DTRI,x.X06.LOAI_DTRI_LAO,x.X06.PHACDO_DTRI_LAO,
                                x.X06.NOI_LAY_MAU_XN,GetNgayThang(x.X06.NGAYBD_DTRI_LAO),x.X06.NOI_XN_KD,GetNgayThang(x.X06.NGAYKT_DTRI_LAO),x.X06.NOI_BDDT_ARV,x.X06.MA_LYDO_XNTL_VR,
                                GetNgayThang(x.X06.NGAY_XN_TLVR),x.X06.KQ_XNTL_VR,GetNgayThang(x.X06.NGAY_KQ_XN_TLVR),x.X06.MA_LOAI_BN,x.X06.MA_TINH_TRANG_DK,x.X06.SANG_LOC_LAO,x.X06.LAN_XN_PCR,GetNgayThang(x.X06.NGAY_XN_PCR),
                                GetNgayThang(x.X06.NGAY_KQ_XN_PCR),x.X06.MA_KQ_XN_PCR,x.X06.KQ_DTRI_LAO,GetNgayThang(x.X06.NGAY_NHAN_TT_MANG_THAI),GetNgayThang(x.X06.NGAY_BAT_DAU_DT_CTX),x.X06.MA_XU_TRI,GetNgayThang(x.X06.NGAY_BAT_DAU_XU_TRI),GetNgayThang(x.X06.NGAY_KET_THUC_XU_TRI),
                                x.X06.MA_PHAC_DO_DIEU_TRI,x.X06.GIAI_DOAN_LAM_SANG,x.X06.MA_BAC_PHAC_DO,x.X06.NHOM_DOI_TUONG,x.X06.SO_NGAY_CAP_THUOC_ARV,x.X06.NGAY_CHUYEN_PHAC_DO,x.X06.LY_DO_CHUYEN_PHAC_DO
                                );
                    }

                    SetAutoFeatures(xlSheet6, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion


                    #region Báo cáo XML7
                    List<int?> xml7ErCodes = dmErrors.Where(e => e.error_file == "XML7").Select(e => e.error_code).ToList();
                    var xml7Errors = allErrors.Where(a => xml7ErCodes.Contains(a.MA_LOI));

                    var data7Join =
                        x01.SelectMany(x => x.XML07s)
                        .Join(xml7Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X07 = x, XER = e })
                        .Where(x => x.X07.STT == x.XER.STT);

                    ISheet xlSheet7 = workbook.CreateSheet("Lỗi XML07");
                    rows = 2;
                    cols = CreateHeader(xlSheet7, cellStyles,
                                "Danh sách lỗi của XML07", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi","Mã Bệnh nhân","Tên Bệnh nhân","Mã LK","Mã thẻ",
                                "Số lưu trú","Mã Y Tế","Mã Khoa ra viện","Ngày vào","Ngày ra","Mã đình chỉ thai","Nguyên nhân đình chỉ thai","Thời gian đình chỉ thai","Tuổi thai","Chẩn đoán ra viện",
                                "Phương pháp điều trị","Ghi chú","Mã TTDV","Mã BS","Tên BS","Ngày Chứng từ","Mã Cha", "Mã Mẹ","Mã thẻ tạm","Họ tên cha","Họ tên mẹ","Số ngày nghỉ","Ngoại trú từ ngày","Ngoại trú đến ngày"
                               );

                    foreach (var x in data7Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet7, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X07.XML01.MA_BN,x.X07.XML01.HO_TEN,x.X07.XML01.MA_LK,x.X07.XML01.MA_THE_BHYT,
                                x.X07.SO_LUU_TRU,x.X07.MA_YTE,x.X07.MA_KHOA_RV,GetNgayThang(x.X07.NGAY_VAO),GetNgayThang(x.X07.NGAY_RA),x.X07.MA_DINH_CHI_THAI,x.X07.NGUYENNHAN_DINHCHI,x.X07.THOIGIAN_DINHCHI,x.X07.TUOI_THAI,x.X07.CHAN_DOAN_RV,
                                x.X07.PP_DIEUTRI,x.X07.GHI_CHU,x.X07.MA_TTDV,x.X07.MA_BS,x.X07.TEN_BS,GetNgayThang(x.X07.NGAY_CT),x.X07.MA_CHA,x.X07.MA_ME,x.X07.MA_THE_TAM,x.X07.HO_TEN_CHA,x.X07.HO_TEN_ME,x.X07.SO_NGAY_NGHI,GetNgayThang(x.X07.NGOAITRU_TUNGAY),GetNgayThang(x.X07.NGOAITRU_DENNGAY)

                                );
                    }

                    SetAutoFeatures(xlSheet7, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML8
                    List<int?> xml8ErCodes = dmErrors.Where(e => e.error_file == "XML8").Select(e => e.error_code).ToList();
                    var xml8Errors = allErrors.Where(a => xml8ErCodes.Contains(a.MA_LOI));

                    var data8Join =
                        x01.SelectMany(x => x.XML08s)
                        .Join(xml8Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X08 = x, XER = e })
                        .Where(x => x.X08.STT == x.XER.STT);

                    ISheet xlSheet8 = workbook.CreateSheet("Lỗi XML08");
                    rows = 2;
                    cols = CreateHeader(xlSheet8, cellStyles,
                                "Danh sách lỗi của XML08", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Mã loại KCB","Họ tên Cha","Họ tên mẹ","Người giám hộ","Đơn vị","Ngày vào","Ngày ra","Chẩn đoán vào","Chẩn đoán ra viện","Quá trình bệnh lý","Tóm tắt KQ","Phương pháp điều trị","Ngày sinh con",
                                "Ngày con chết","Số con chết","Kết quả điều trị","Ghi chú", "Mã TTDV","Ngày chứng từ","Mã thẻ tạm"
                               );

                    foreach (var x in data8Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet8, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X08.XML01.MA_BN,x.X08.XML01.HO_TEN,x.X08.XML01.MA_LK,x.X08.XML01.MA_THE_BHYT,
                                x.X08.MA_LOAI_KCB,x.X08.HO_TEN_CHA,x.X08.HO_TEN_ME,x.X08.NGUOI_GIAM_HO,x.X08.DON_VI,GetNgayThang(x.X08.NGAY_VAO),GetNgayThang(x.X08.NGAY_RA),x.X08.CHAN_DOAN_VAO,x.X08.CHAN_DOAN_RV,x.X08.QT_BENHLY,x.X08.TOMTAT_KQ,x.X08.PP_DIEUTRI,GetNgayThang(x.X08.NGAY_SINHCON),
                                GetNgayThang(x.X08.NGAY_CONCHET),x.X08.SO_CONCHET,x.X08.KET_QUA_DTRI,x.X08.GHI_CHU,x.X08.MA_TTDV, GetNgayThang(x.X08.NGAY_CT),x.X08.MA_THE_TAM
                                );
                    }

                    SetAutoFeatures(xlSheet8, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion


                    #region Báo cáo XML9
                    List<int?> xml9ErCodes = dmErrors.Where(e => e.error_file == "XML9").Select(e => e.error_code).ToList();
                    var xml9Errors = allErrors.Where(a => xml9ErCodes.Contains(a.MA_LOI));

                    var data9Join =
                        x01.SelectMany(x => x.XML09s)
                        .Join(xml9Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X09 = x, XER = e })
                        .Where(x => x.X09.STT == x.XER.STT);

                    ISheet xlSheet9 = workbook.CreateSheet("Lỗi XML09");
                    rows = 2;
                    cols = CreateHeader(xlSheet9, cellStyles,
                                "Danh sách lỗi của XML09", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi","Mã loại KCB", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Mã BHXH người nuôi dưỡng","Mã thẻ NND","Họ tên NND","Ngày sinh NND","Mã DT NND","Số CCCD NND","Ngày cấp CCCD NND","Nơi cấp CCCD NND","Nơi cư trú NND","Mã QT",
                                "Mã Tỉnh","Mã huyện","Mã xã","Họ tên cha", "Mã thẻ tạm","Họ tên con","Giới tính","Số con","Lần sinh","Số con sống","Cân nặng con","Ngày sinh con","Nơi sinh con",
                                "Trình trạng con","Sinh con phẫu thuật","Sinh con dưới 32 tuần","Ghi chú","Người đỡ đẻ","Người ghi phiếu","Ngày chứng từ","Số Chứng từ","Quyển số","Mã TTDV"
                               );

                    foreach (var x in data9Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet9, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X09.XML01.MA_LOAI_KCB,x.X09.XML01.MA_BN,x.X09.XML01.HO_TEN,x.X09.XML01.MA_LK,x.X09.XML01.MA_THE_BHYT,
                                x.X09.MA_BHXH_NND,x.X09.MA_THE_NND,x.X09.HO_TEN_NND,GetNgayThang(x.X09.NGAYSINH_NND),x.X09.MA_DANTOC_NND,x.X09.SO_CCCD_NND,GetNgayThang(x.X09.NGAYCAP_CCCD_NND),x.X09.NOICAP_CCCD_NND,x.X09.NOI_CU_TRU_NND,x.X09.MA_QUOCTICH,
                                x.X09.MATINH_CU_TRU,x.X09.MAHUYEN_CU_TRU,x.X09.MAXA_CU_TRU,x.X09.HO_TEN_CHA,x.X09.MA_THE_TAM,x.X09.HO_TEN_CON,x.X09.GIOI_TINH_CON,x.X09.SO_CON,x.X09.LAN_SINH,x.X09.SO_CON_SONG,x.X09.CAN_NANG_CON,GetNgayThang(x.X09.NGAY_SINH_CON),x.X09.NOI_SINH_CON,
                                x.X09.TINH_TRANG_CON,x.X09.SINHCON_PHAUTHUAT,x.X09.SINHCON_DUOI32TUAN,x.X09.GHI_CHU,x.X09.NGUOI_DO_DE,x.X09.NGUOI_GHI_PHIEU,GetNgayThang(x.X09.NGAY_CT),x.X09.SO,x.X09.QUYEN_SO,x.X09.MA_TTDV
                                );
                    }

                    SetAutoFeatures(xlSheet9, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML10
                    List<int?> xml10ErCodes = dmErrors.Where(e => e.error_file == "XML10").Select(e => e.error_code).ToList();
                    var xml10Errors = allErrors.Where(a => xml10ErCodes.Contains(a.MA_LOI));

                    var data10Join =
                        x01.SelectMany(x => x.XML10s)
                        .Join(xml10Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X10 = x, XER = e })
                        .Where(x => x.X10.STT == x.XER.STT);

                    ISheet xlSheet10 = workbook.CreateSheet("Lỗi XML10");
                    rows = 2;
                    cols = CreateHeader(xlSheet10, cellStyles,
                                "Danh sách lỗi của XML10", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số Seri","Số chứng từ","Số ngày","Đơn vị","Chẩn đoán ra viện","Từ ngày","Đến ngày","Mã TTDV","Tên BS", "Mã BS", "Ngày chứng từ"
                               );

                    foreach (var x in data10Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet10, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X10.XML01.MA_BN,x.X10.XML01.HO_TEN,x.X10.XML01.MA_LK,x.X10.XML01.MA_THE_BHYT,
                                x.X10.SO_SERI,x.X10.SO_CT,x.X10.SO_NGAY,x.X10.DON_VI,x.X10.CHAN_DOAN_RV,GetNgayThang(x.X10.TU_NGAY),GetNgayThang(x.X10.DEN_NGAY),x.X10.MA_TTDV,x.X10.TEN_BS,x.X10.MA_BS,x.X10.NGAY_CT
                                );
                    }

                    SetAutoFeatures(xlSheet10, cols, 2, -4, 12, 14, 15, 17, -24);

                    #endregion


                    #region Báo cáo XML11
                    List<int?> xml11ErCodes = dmErrors.Where(e => e.error_file == "XML11").Select(e => e.error_code).ToList();
                    var xml11Errors = allErrors.Where(a => xml11ErCodes.Contains(a.MA_LOI));

                    var data11Join =
                        x01.SelectMany(x => x.XML11s)
                        .Join(xml11Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X11 = x, XER = e })
                        .Where(x => x.X11.STT == x.XER.STT);

                    ISheet xlSheet11 = workbook.CreateSheet("Lỗi XML11");
                    rows = 2;
                    cols = CreateHeader(xlSheet11, cellStyles,
                                "Danh sách lỗi của XML11", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số Chứng Từ", "Số Seri","Số KCB","Đơn vị","Mã BHXH","Mã thẻ BHYT","Chẩn đoán ra viện","Phương pháp điều trị","Mã đình chỉ thai","Nguyên nhân đình chỉ","Tuổi thai","Số ngày nghỉ",
                                "Từ ngày","Đến ngày","Họ tên Cha","Họ tên mẹ","Mã TTDV","Mã BS","Ngày CT","Mã thẻ tạm", "Mẫu số"
                               );

                    foreach (var x in data11Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet11, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X11.XML01.MA_BN,x.X11.XML01.HO_TEN,x.X11.XML01.MA_LK,x.X11.XML01.MA_THE_BHYT,
                                x.X11.SO_CT,x.X11.SO_SERI,x.X11.SO_KCB,x.X11.DON_VI,x.X11.MA_BHXH,x.X11.MA_THE_BHYT,x.X11.CHAN_DOAN_RV,x.X11.PP_DIEUTRI,x.X11.MA_DINH_CHI_THAI,x.X11.NGUYENNHAN_DINHCHI,x.X11.TUOI_THAI,x.X11.SO_NGAY_NGHI,
                                GetNgayThang(x.X11.TU_NGAY),GetNgayThang(x.X11.DEN_NGAY),x.X11.HO_TEN_CHA,x.X11.HO_TEN_ME,x.X11.MA_TTDV,x.X11.MA_BS,GetNgayThang(x.X11.NGAY_CT),x.X11.MA_THE_TAM,x.X11.MAU_SO
                                );
                    }

                    SetAutoFeatures(xlSheet11, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML13
                    List<int?> xml13ErCodes = dmErrors.Where(e => e.error_file == "XML13").Select(e => e.error_code).ToList();
                    var xml13Errors = allErrors.Where(a => xml13ErCodes.Contains(a.MA_LOI));

                    var data13Join =
                        x01.SelectMany(x => x.XML13s)
                        .Join(xml13Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X13 = x, XER = e })
                        .Where(x => x.X13.STT == x.XER.STT);

                    ISheet xlSheet13 = workbook.CreateSheet("Lỗi XML13");
                    rows = 2;
                    cols = CreateHeader(xlSheet13, cellStyles,
                                "Danh sách lỗi của XML13", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số hồ sơ","Số chuyển tuyến","Giấy chuyển tuyến","Mã CSKCB","Mã nơi đi","Mã nơi đến","Họ Tên","Ngày sinh","Giới tính","Mã quốc tịch","Mã dân tộc","Mã nghề nghiệp","Địa chỉ",
                                "Mã thẻ BHYT","Giá trị thẻ đến","Ngày vào","Ngày vào nội trú","Ngày ra","Dấu hiệu LS","Chẩn đoán ra viện","Quá trình bệnh lý","Tóm tắt KQ","Phương pháp điều trị",
                                "Mã bệnh chính","Mã bệnh YHCT","Tên dịch vụ","Tên thuốc","Phương pháp điều trị","Mã loại ra viện","Mã lý do chuyển tuyến","Hướng điều trị","Phương tiện vận chuyển","Họ tên người hộ tống",
                                "Chức dang người hộ tống","Mã bác sĩ","Mã TTDV"
                               );

                    foreach (var x in data13Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet13, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X13.XML01.MA_BN,x.X13.XML01.HO_TEN,x.X13.XML01.MA_LK,x.X13.XML01.MA_THE_BHYT,
                                x.X13.SO_HOSO,x.X13.SO_CHUYENTUYEN,x.X13.GIAY_CHUYEN_TUYEN,x.X13.MA_CSKCB,x.X13.MA_NOI_DI,x.X13.MA_NOI_DEN,x.X13.HO_TEN,GetNgayThang(x.X13.NGAY_SINH),x.X13.GIOI_TINH,x.X13.MA_QUOCTICH,x.X13.MA_DANTOC,x.X13.MA_NGHE_NGHIEP,x.X13.DIA_CHI,
                                x.X13.MA_THE_BHYT,x.X13.GT_THE_DEN,GetNgayThang(x.X13.NGAY_VAO),GetNgayThang(x.X13.NGAY_VAO_NOI_TRU),GetNgayThang(x.X13.NGAY_RA),x.X13.DAU_HIEU_LS,x.X13.CHAN_DOAN_RV,x.X13.QT_BENHLY,x.X13.TOMTAT_KQ,x.X13.PP_DIEUTRI,
                                x.X13.MA_BENH_CHINH,x.X13.MA_BENH_YHCT,x.X13.TEN_DICH_VU,x.X13.TEN_THUOC,x.X13.PP_DIEU_TRI,x.X13.MA_LOAI_RV,x.X13.MA_LYDO_CT,x.X13.HUONG_DIEU_TRI,x.X13.PHUONGTIEN_VC,x.X13.HOTEN_NGUOI_HT,
                                x.X13.CHUCDANH_NGUOI_HT,x.X13.MA_BAC_SI,x.X13.MA_TTDV

                                );
                    }

                    SetAutoFeatures(xlSheet13, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML14
                    List<int?> xml14ErCodes = dmErrors.Where(e => e.error_file == "XML14").Select(e => e.error_code).ToList();
                    var xml14Errors = allErrors.Where(a => xml14ErCodes.Contains(a.MA_LOI));

                    var data14Join =
                        x01.SelectMany(x => x.XML14s)
                        .Join(xml14Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X14 = x, XER = e })
                        .Where(x => x.X14.STT == x.XER.STT);

                    ISheet xlSheet14 = workbook.CreateSheet("Lỗi XML14");
                    rows = 2;
                    cols = CreateHeader(xlSheet14, cellStyles,
                                "Danh sách lỗi của XML14", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số giấy hẹn khám lại","Mã CSKCB","Họ tên","Ngày sinh","Giới tính","Địa chỉ","Mã thẻ BHYT","Giá trị thẻ đến","Ngày vào","ngày vào nội trú","ngày ra","ngày hẹn khám lại","chẩn đoán ra viện",
                                "Mã bệnh chính","mã bệnh kèm theo","Mã bệnh YHCT","Mã đối tượng KCB","Mã bác sĩ","Mã TTDV","Ngày chứng tử"
                               );

                    foreach (var x in data14Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet14, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X14.XML01.MA_BN,x.X14.XML01.HO_TEN,x.X14.XML01.MA_LK,x.X14.XML01.MA_THE_BHYT,
                                x.X14.SO_GIAYHEN_KL,x.X14.MA_CSKCB,x.X14.HO_TEN,GetNgayThang(x.X14.NGAY_SINH),x.X14.GIOI_TINH,x.X14.DIA_CHI,x.X14.MA_THE_BHYT,x.X14.GT_THE_DEN,GetNgayThang(x.X14.NGAY_VAO), GetNgayThang(x.X14.NGAY_VAO_NOI_TRU),GetNgayThang(x.X14.NGAY_RA),GetNgayThang(x.X14.NGAY_HEN_KL),x.X14.CHAN_DOAN_RV,
                                x.X14.MA_BENH_CHINH,x.X14.MA_BENH_KT,x.X14.MA_BENH_YHCT,x.X14.MA_DOITUONG_KCB,x.X14.MA_BAC_SI,x.X14.MA_TTDV,GetNgayThang(x.X14.NGAY_CT)                               
                                );
                    }

                    SetAutoFeatures(xlSheet14, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML15
                    List<int?> xml15ErCodes = dmErrors.Where(e => e.error_file == "XML15").Select(e => e.error_code).ToList();
                    var xml15Errors = allErrors.Where(a => xml15ErCodes.Contains(a.MA_LOI));

                    var data15Join =
                        x01.SelectMany(x => x.XML15s)
                        .Join(xml15Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X15 = x, XER = e })
                        .Where(x => x.X15.STT == x.XER.STT);

                    ISheet xlSheet15 = workbook.CreateSheet("Lỗi XML15");
                    rows = 2;
                    cols = CreateHeader(xlSheet15, cellStyles,
                                "Danh sách lỗi của XML15", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Mã bệnh nhân","Họ tên","Số CCCD","PHân loại loai theo VT","Phân loại lao theo TS","Phân loại lao HIV","Phân loại Lao VK","Loại điều trị Lao","Ngày bắt đầu điều trị lao",
                                "Phác đồ điều trị lao","Ngày kết thúc điều trị lao","Kết quả điều trị Lao","Mã CSKCB","Ngày khẳng định HIV","Thời điểm nhận thuốc ARV","Ngày bắt đầu điều trị CTX"
                               );

                    foreach (var x in data15Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet15, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),x.X15.XML01.MA_BN,x.X15.XML01.HO_TEN,x.X15.XML01.MA_LK,x.X15.XML01.MA_THE_BHYT,
                                x.X15.MA_BN,x.X15.HO_TEN,x.X15.SO_CCCD,x.X15.PHANLOAI_LAO_VITRI,x.X15.PHANLOAI_LAO_TS,x.X15.PHANLOAI_LAO_HIV,x.X15.PHANLOAI_LAO_VK,GetNgayThang(x.X15.NGAYBD_DTRI_LAO),
                                x.X15.PHACDO_DTRI_LAO,GetNgayThang(x.X15.NGAYKT_DTRI_LAO),x.X15.KET_QUA_DTRI_LAO,x.X15.MA_CSKCB,x.X15.NGAYKD_HIV,GetNgayThang(x.X15.BDDT_ARV),GetNgayThang(x.X15.NGAY_BAT_DAU_DT_CTX)

                                );
                    }

                    SetAutoFeatures(xlSheet15, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion
                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                string stk = ex.StackTrace;
            }

            return null;
        }


        static public byte[] Download_BaocaoTrungDichVu(IEnumerable<XML01> data1, IEnumerable<DMKhoaBV> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
        {
            try
            {
                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";

                var allXML2 = data1.SelectMany(x => x.XML02s);
                var allXMl3 = data1.SelectMany(x => x.XML03s);

                byte[] result = null;
                using (var fs = new MemoryStream())
                {
                    #region Sheet 1: Loi BS DV Trung TG - Nhom 8
                    var dataNhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041")) &&
                                                        !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL !=null &&
                                                        !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232")
                                                        &&(x.NGAY_TH_YL.Value.Date ==x.NGAY_KQ.Value.Date /*|| Uti.CodeContain(x.MA_DICH_VU,"16.0050.1014; 16.0061.1011; 16.0232.1016; 16.0232.1017; 03.1942.1010; 03.1944.1017; 03.1853.1011; 03.1858.1012; 03.1858.1013; 03.1858.1014; 03.1858.1015; 03.1944.1016; 16.0044.1013; 16.0050.1012; 16.0050.1013; 16.0050.1015; 16.0052.1012; 16.0052.1014; 16.0052.1015; 16.0054.1015;")*/)
                                                        );

                    var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, x1 => x1.NGUOI_THUC_HIEN, x2 => x2.NGUOI_THUC_HIEN, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ));
                                            //.Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x1.NGAY_TH_YL <= a.x2.NGAY_TH_YL && a.x1.NGAY_KQ >= a.x2.NGAY_TH_YL));
                    // .Where(a =>  a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

                    ISheet xlSheetG8 = workbook.CreateSheet("Trùng Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật)");
                    int rows = 2;
                    int cols = CreateHeader(xlSheetG8, cellStyles,
                                "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8 (Phãu thuật) và nhóm 18 (Thủ Thuật)", // Sheet title
                                "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1","Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                "Mã Dịch vụ 2", "Tên Dịch vụ 2",  "Ngày Y lệnh 2","Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                    foreach (var t in xml3Nhom8Trung)
                    {
                        string maBS1 = t.x1.NGUOI_THUC_HIEN;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.NGUOI_THUC_HIEN;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetG8, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL),GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                            );
                    }
                    SetAutoFeatures(xlSheetG8, cols, 3, 4, 11, 18, 19, 26);
                    #endregion

                    #region Sheet 2: Loi BS DV Trung TG - Nhom 2
                    var dataNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 || x.MA_NHOM==3 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL !=null && x.MA_MAY !=null);

                    var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, x1 => x1.NGUOI_THUC_HIEN, x2 => x2.NGUOI_THUC_HIEN, (x1, x2) => new { x1, x2 })
                                            .Where(a =>  a.x1.MA_LK != a.x2.MA_LK && a.x1.MA_MAY == a.x2.MA_MAY && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL);

                    ISheet xlSheetG2 = workbook.CreateSheet("Trùng thực hiện  Nhóm 2,3 - CĐHA -TDCN");
                    rows = 2;
                    cols = CreateHeader(xlSheetG2, cellStyles,
                                        "Danh sách Bác sĩ thực hiện trên cùng mã máy - Nhóm 2,3, 8, 18 CĐHA -TDCN - PT - TT", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1","Ngày thực hiện y lệnh 1", "Ngày Kết quả 1","Mã máy 1",
                                        "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2","Ngày thực hiện Y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                    foreach (var t in xml3Nhom2Trung)
                    {
                        string maBS1 = t.x1.NGUOI_THUC_HIEN;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.NGUOI_THUC_HIEN;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetG2, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL),GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),t.x1.MA_MAY,
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU,  GetNgayThang(t.x2.NGAY_YL),GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ),t.x2.MA_MAY
                            );
                    }

                    SetAutoFeatures(xlSheetG2, cols, 3, 4, 11, 18, 19, 26);
                    #endregion
                    //Thêm 1 báo cáo trùng thời gian thực hiện y lệnh
                    #region Sheet 3: Báo cáo trùng thời gian thực hiện y lệnh dịch vụ
                    var dataTrungTHYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15,1,13));

                    var dataTrung = dataTrungTHYL.Join(dataTrungTHYL, x1 => x1.NGUOI_THUC_HIEN, x2 => x2.NGUOI_THUC_HIEN, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL);

                    ISheet xlSheetG8TH = workbook.CreateSheet("Trùng thời gian thực hiện XML3 dịch vụ kỹ thuật");
                    rows = 2;
                    cols = CreateHeader(xlSheetG8TH, cellStyles,
                               "Danh sách Trùng thời gian thực hiện XML3 Thực hiện Y Lệnh", // Sheet title
                               "STT", "Mã lượt khám 1", "Mã Bác sĩ thực hiện 1", "Tên Bác sĩ thực hiện 1", "Chức danh 1",
                               "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                               "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                               "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                               "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1","Mã máy 1",
                               "Mã Lượt khám 2", "Mã Bác sĩ thực hiện 2", "Tên Bác sĩ thực hiện 2", "Chức danh 2",
                               "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                               "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                               "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                               "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                    foreach (var t in dataTrung)
                    {
                        string maBS1 = t.x1.NGUOI_THUC_HIEN; 
                        
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.NGUOI_THUC_HIEN;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetG8TH, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                            );
                    }

                    SetAutoFeatures(xlSheetG8TH, cols, 3, 4, 11, 18, 19, 26);
                    #endregion
                    #region Sheet 4: Báo cáo trùng thời gian thực hiện y lệnh khám bệnh (Trùng giờ Thực hiện)
                    var dataTrungKB = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && Uti.NumExists(x.MA_NHOM, 13));

                    var dataTrungx = dataTrungKB.Join(dataTrungKB, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL);

                    ISheet xlSheetGxTH = workbook.CreateSheet("Trùng thời gian y lệnh Khám");
                    rows = 2;
                    cols = CreateHeader(xlSheetGxTH, cellStyles,
                               "Danh sách Trùng thời gian y lệnh Khám Bệnh", // Sheet title
                               "STT", "Mã lượt khám 1", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 1",
                               "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                               "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                               "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                               "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1",
                               "Mã Lượt khám 2", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 2",
                               "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                               "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                               "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                               "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                    foreach (var t in dataTrungx)
                    {
                        string maBS1 = t.x1.MA_BAC_SI;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.MA_BAC_SI;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetGxTH, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                            );
                    }

                    SetAutoFeatures(xlSheetGxTH, cols, 3, 4, 11, 18, 19, 26);

                    #endregion
                    #region Sheet 5: Báo cáo trùng thời gian thực hiện y lệnh khám bệnh
                    var dataTrungKBx = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && Uti.NumExists(x.MA_NHOM, 13));

                    var dataTrungxx = dataTrungKBx.Join(dataTrungKBx, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL);

                    ISheet xlSheetGxTHx = workbook.CreateSheet("Trùng thời gian thực hiện YL Khám");
                    rows = 2;
                    cols = CreateHeader(xlSheetGxTHx, cellStyles,
                               "Danh sách Trùng thời gian thực hiện YL Khám Bệnh", // Sheet title
                               "STT", "Mã lượt khám 1", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 1",
                               "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                               "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                               "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                               "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1",
                               "Mã Lượt khám 2", "Mã Bác sĩ khám", "Tên Bác sĩ khám", "Chức danh 2",
                               "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                               "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                               "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                               "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                    foreach (var t in dataTrungxx)
                    {
                        string maBS1 = t.x1.MA_BAC_SI;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.MA_BAC_SI;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetGxTHx, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                            );
                    }
                    SetAutoFeatures(xlSheetGxTHx, cols, 3, 4, 11, 18, 19, 26);

                    #endregion
                    //#region Sheet 2: Loi BS DV Trung TG - Nhom 2
                    //var dataNhom2x = allXMl3.Where(x => x.MA_NHOM == 2 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null);

                    //var xml3Nhom2Trungx = dataNhom2.Join(dataNhom2, x1 => x1.NGUOI_THUC_HIEN, x2 => x2.NGUOI_THUC_HIEN, (x1, x2) => new { x1, x2 })
                    //                        .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL <= a.x2.NGAY_KQ && a.x1.NGAY_KQ >= a.x2.NGAY_TH_YL);

                    //ISheet xlSheetG2x = workbook.CreateSheet("Trùng Nhóm 2 - CĐHA");
                    //rows = 2;
                    //cols = CreateHeader(xlSheetG2x, cellStyles,
                    //                    "Danh sách Bác sĩ & Dịch vụ trùng khoảng thời gian - Nhóm 2 CĐHA", // Sheet title
                    //                    "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                    //                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                    //                    "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                    //                    "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                    //                    "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                    //                    "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                    //                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                    //                    "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                    //                    "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                    //                    "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2", "Ngày Kết quả 2");

                    //foreach (var t in xml3Nhom2Trungx)
                    //{
                    //    string maBS1 = t.x1.NGUOI_THUC_HIEN;
                    //    string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    //    string maBS2 = t.x2.NGUOI_THUC_HIEN;
                    //    string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    //    AppendRow(xlSheetG2x, cellStyles, rows++,
                    //            rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    //            t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                    //            t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                    //            t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                    //            t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                    //            t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    //            t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                    //            t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                    //            t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                    //            t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                    //        );
                    //}

                    //SetAutoFeatures(xlSheetG2x, cols, 3, 4, 11, 18, 19, 26);
                    //#endregion
                    //#region Sheet 5: Loi DV Trung Ngay Y Lenh cua Noi Tru & Ngoai Tru (loc loai KCB & loai tru vat tu hay giuong benh)

                    //var dataTrungYL = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15) && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null);
                    //var xml3TrungYL = dataTrungYL.Join(dataTrungYL,
                    //                                    x1 => x1.NGAY_YL,
                    //                                    x2 => x2.NGAY_YL,
                    //                                    (x1, x2) => new { x1, x2 })
                    //                        .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && Uti.FirstMatch(a.x1.MA_BAC_SI, a.x2.MA_BAC_SI));

                    //ISheet xlSheetYL = workbook.CreateSheet("Trùng Y Lệnh");
                    //rows = 2;
                    //cols = CreateHeader(xlSheetYL, cellStyles,
                    //            "Danh sách Dịch vụ Nội - Ngoại trú trùng thời gian y lệnh", // Sheet title
                    //            "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                    //            "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1",
                    //            "Mã Khoa 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1",
                    //            "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                    //            "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2",
                    //            "Mã Khoa 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

                    //foreach (var t in xml3TrungYL)
                    //{
                    //    string maBS1 = t.x1.MA_BAC_SI;
                    //    string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    //    string maBS2 = t.x2.MA_BAC_SI;
                    //    string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    //    AppendRow(xlSheetYL, cellStyles, rows++,
                    //            rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    //            t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                    //            t.x1.MA_KHOA, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL),
                    //            t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    //            t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                    //            t.x2.MA_KHOA, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL)
                    //        );
                    //}

                    //SetAutoFeatures(xlSheetYL, cols, 3, 4, 12, 13);
                    //#endregion
                    
                    #region Sheet 5: Loi Trung Giuong
                    var dataGiuong = allXMl3.Where(x => (x.MA_NHOM == 14 || x.MA_NHOM == 15) && !string.IsNullOrEmpty(x.MA_GIUONG) && x.NGAY_YL != null && x.NGAY_KQ != null);

                    var xml3GiuongTrung = dataGiuong.Join(dataGiuong, x1 => x1.MA_GIUONG, x2 => x2.MA_GIUONG, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL && a.x1.MA_KHOA == a.x2.MA_KHOA && (a.x1.TYLE_TT_DV > 50 || a.x2.TYLE_TT_DV > 50));

                    ISheet xlSheetGG = workbook.CreateSheet("Trùng Giường");
                    rows = 2;
                    cols = CreateHeader(xlSheetGG, cellStyles,
                                "Danh sách Dịch vụ Giường trùng thời gian", // Sheet caption
                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                "Mã Khoa 1", "Tên Khoa 1",
                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Giường 1", "Tỷ lệ thanh toán DV 1",
                                "Ngày Kết quả 1", "Ngày Y lệnh 1",
                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                "Mã Khoa 2", "Tên Khoa 2",
                                "Mã Dịch vụ 1", "Tên Dịch vụ 2", "Giường 2", "Tỷ lệ thanh toán DV 2",
                                "Ngày Kết quả 2", "Ngày Y lệnh 2");

                    foreach (var t in xml3GiuongTrung)
                    {
                        string maBS1 = t.x1.MA_BAC_SI;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.MA_BAC_SI;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetGG, cellStyles, rows++,
                            rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                            t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                            t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                            t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
                            t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, t.x1.MA_GIUONG, t.x1.TYLE_TT_DV,
                            GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
                            t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                            t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                            t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                            t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
                            t.x2.MA_KHOA, t.x2.TEN_DICH_VU, t.x2.MA_GIUONG, t.x2.TYLE_TT_DV,
                            GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL));
                    }

                    SetAutoFeatures(xlSheetGG, cols, 3, 4, 11, 19, 20, 27);
                    #endregion
                    #region Sheet 6: Báo cáo trùng thời gian y lệnh dịch vụ
                    var dataTrungYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15));

                    var dataTrungTYL = dataTrungYL.Join(dataTrungYL, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL);

                    ISheet xlSheetG9TH = workbook.CreateSheet("Trùng thời gian y lệnh XML3 dịch vụ kỹ thuật");
                    rows = 2;
                    cols = CreateHeader(xlSheetG9TH, cellStyles,
                               "Danh sách Trùng thời gian XML3 Y Lệnh", // Sheet title
                               "STT", "Mã lượt khám 1", "Mã Bác sĩ YL 1", "Tên Bác sĩ YL 1", "Chức danh 1",
                               "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                               "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                               "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                               "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                               "Mã Lượt khám 2", "Mã Bác sĩ YL 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                               "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                               "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                               "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                               "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                    foreach (var t in dataTrungTYL)
                    {
                        string maBS1 = t.x1.MA_BAC_SI;

                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.MA_BAC_SI;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetG9TH, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                            );
                    }

                    SetAutoFeatures(xlSheetG9TH, cols, 3, 4, 11, 18, 19, 26);
                    #endregion

                    #region Sheet 7: Loi BS DV Trung TG - Nhom 2,3
                    var dataNhom2MM = allXMl3.Where(x => x.MA_NHOM == 2 || x.MA_NHOM == 3 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && x.MA_MAY != null);

                    var xml3Nhom2TrungMM = dataNhom2.Join(dataNhom2MM, x1 => x1.MA_MAY, x2 => x2.MA_MAY, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL);

                    ISheet xlSheetG2MM = workbook.CreateSheet("Mã Máy  Nhóm 2,3 - CĐHA -TDCN");
                    rows = 2;
                    cols = CreateHeader(xlSheetG2MM, cellStyles,
                                        "Danh sách cùng mã máy - Nhóm 2,3 CĐHA -TDCN", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                        "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                    foreach (var t in xml3Nhom2TrungMM)
                    {
                        string maBS1 = t.x1.NGUOI_THUC_HIEN;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.NGUOI_THUC_HIEN;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetG2MM, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                            );
                    }

                    SetAutoFeatures(xlSheetG2MM, cols, 3, 4, 11, 18, 19, 26);
                    #endregion

                    #region Sheet 8: Loi MM- Nhom 8
                    var dataNhom8MM = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041")) &&
                                                        !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null &&
                                                        !string.IsNullOrEmpty(x.MA_MAY) &&
                                                        !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232")
                                                        && (x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date /*|| Uti.CodeContain(x.MA_DICH_VU,"16.0050.1014; 16.0061.1011; 16.0232.1016; 16.0232.1017; 03.1942.1010; 03.1944.1017; 03.1853.1011; 03.1858.1012; 03.1858.1013; 03.1858.1014; 03.1858.1015; 03.1944.1016; 16.0044.1013; 16.0050.1012; 16.0050.1013; 16.0050.1015; 16.0052.1012; 16.0052.1014; 16.0052.1015; 16.0054.1015;")*/)
                                                        );

                    var xml3Nhom8TrungMM = dataNhom8MM.Join(dataNhom8MM, x1 => x1.MA_MAY, x2 => x2.MA_MAY, (x1, x2) => new { x1, x2 })
                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ));
                    //.Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x1.NGAY_TH_YL <= a.x2.NGAY_TH_YL && a.x1.NGAY_KQ >= a.x2.NGAY_TH_YL));
                    // .Where(a =>  a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

                    ISheet xlSheetG8MM = workbook.CreateSheet("Trùng Mã Máy Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật)");
                     rows = 2;
                     cols = CreateHeader(xlSheetG8MM, cellStyles,
                                "Danh sách Mã máy trùng thời gian - Nhóm 8 (Phãu thuật) và nhóm 18 (Thủ Thuật)", // Sheet title
                                "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                    foreach (var t in xml3Nhom8TrungMM)
                    {
                        string maBS1 = t.x1.NGUOI_THUC_HIEN;
                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                        string maBS2 = t.x2.NGUOI_THUC_HIEN;
                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                        AppendRow(xlSheetG8MM, cellStyles, rows++,
                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetGioiTinh(t.x1.XML01.GIOI_TINH),
                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetGioiTinh(t.x2.XML01.GIOI_TINH),
                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ),t.x2.MA_MAY
                            );
                    }
                    SetAutoFeatures(xlSheetG8MM, cols, 3, 4, 11, 18, 19, 26);
                    #endregion
                    //#region Sheet 6: Loi Trung Ngay Y Lenh cua Thuoc & DV
                    //var X02TRG = allXML2.Where(x => x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));
                    //var xml3TRG = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15) && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));

                    //var allTRG_YL = X02TRG.Join(xml3TRG, x2 => x2.NGAY_YL,
                    //                                    x3 => x3.NGAY_YL,
                    //                                    (x2, x3) => new { x2, x3 })
                    //                        .Where(a => a.x2.MA_LK != a.x3.MA_LK && Uti.FirstMatch(a.x2.MA_BAC_SI, a.x3.MA_BAC_SI));

                    //ISheet xlSheetTRG = workbook.CreateSheet("Trùng Thuốc DV");
                    //rows = 2;

                    //cols = CreateHeader(xlSheetTRG, cellStyles,
                    //            "Danh sách trùng thời điểm bác sĩ y lệnh Thuốc với Dịch vụ", // Sheet title
                    //            "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                    //            "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1", "Mã Nhóm 1",
                    //            "Mã Khoa 1", "Tên Thuốc 1", "Ngày Y lệnh 1",
                    //            "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                    //            "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2", "Mã Nhóm 2",
                    //            "Mã Khoa 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

                    //foreach (var t in allTRG_YL)
                    //{
                    //    string maBS1 = t.x2.MA_BAC_SI;
                    //    string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    //    string maBS2 = t.x3.MA_BAC_SI;
                    //    string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    //    AppendRow(xlSheetTRG, cellStyles, rows++,
                    //            rows - 2, t.x2.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    //            t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN, t.x2.MA_NHOM,
                    //            t.x2.MA_KHOA, t.x2.TEN_THUOC, GetNgayThang(t.x2.NGAY_YL),
                    //            t.x3.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    //            t.x3.XML01.MA_BN, t.x3.XML01.HO_TEN, t.x3.MA_NHOM,
                    //            t.x3.MA_KHOA, t.x3.TEN_DICH_VU, GetNgayThang(t.x3.NGAY_YL)
                    //        );
                    //}

                    //SetAutoFeatures(xlSheetTRG, cols, 3, 4, 12, 13);
                    //#endregion

                    // var X02EVR = X02TRG; //data2.Where(x => x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));
                    //var xml3EVR = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && string.IsNullOrWhiteSpace(x.MA_VAT_TU) && x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI) && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15));

                    // //#region Sheet 6: Canh bao Trung Thuoc & DV (Update ko lấy ngày giường và vật tư)

                    // //var allEVR_22 = X02EVR.Join(X02EVR, x21 => x21.NGAY_YL,
                    // //                                    x22 => x22.NGAY_YL,
                    // //                                    (x21, x22) => new { x21, x22 })
                    // //                        .Where(a => a.x21.MA_LK != a.x22.MA_LK && Uti.FirstMatch(a.x21.MA_BAC_SI, a.x22.MA_BAC_SI));

                    // //var allEVR_23 = X02EVR.Join(xml3EVR, x2 => x2.NGAY_YL,
                    // //                                    x3 => x3.NGAY_YL,
                    // //                                    (x2, x3) => new { x2, x3 })
                    // //                        .Where(a => a.x2.MA_LK != a.x3.MA_LK && Uti.FirstMatch(a.x2.MA_BAC_SI, a.x3.MA_BAC_SI));

                    // //var allEVR_33 = xml3EVR.Join(xml3EVR, x31 => x31.NGAY_YL,
                    // //                                    x32 => x32.NGAY_YL,
                    // //                                    (x31, x32) => new { x31, x32 })
                    // //                        .Where(a => a.x31.MA_LK != a.x32.MA_LK && Uti.FirstMatch(a.x31.MA_BAC_SI, a.x32.MA_BAC_SI));

                    // //ISheet xlSheetEVR = workbook.CreateSheet("Cảnh báo Trùng Y Lệnh");
                    // //rows = 2;

                    // //cols = CreateHeader(xlSheetEVR, cellStyles,
                    // //            "Danh sách Cảnh báo trùng thời điểm bác sĩ y lệnh Thuốc với Dịch vụ", // Sheet title
                    // //            "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                    // //            "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1", "Mã Nhóm 1",
                    // //            "Mã Khoa 1", "Loại KCB 1", "Loại 1", "Tên 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
                    // //            "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                    // //            "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2", "Mã Nhóm 2",
                    // //            "Mã Khoa 2", "Loại KCB 2", "Loại 2", "Tên 2", "Ngày Kết quả 1", "Ngày Y lệnh 2");

                    // //foreach (var t in allEVR_22)
                    // //{
                    // //    string maBS1 = t.x21.MA_BAC_SI;
                    // //    string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    // //    string maBS2 = t.x22.MA_BAC_SI;
                    // //    string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    // //    AppendRow(xlSheetEVR, cellStyles, rows++,
                    // //            rows - 2, t.x21.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    // //            t.x21.XML01.MA_BN, t.x21.XML01.HO_TEN, t.x21.MA_NHOM,
                    // //            t.x21.MA_KHOA, t.x21.XML01.MA_LOAI_KCB, "Thuốc", t.x21.TEN_THUOC, "", GetNgayThang(t.x21.NGAY_YL), 
                    // //            t.x22.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    // //            t.x22.XML01.MA_BN, t.x22.XML01.HO_TEN, t.x22.MA_NHOM,
                    // //            t.x22.MA_KHOA, t.x22.XML01.MA_LOAI_KCB, "Thuốc", t.x22.TEN_THUOC, "", GetNgayThang(t.x22.NGAY_YL)
                    // //        );
                    // //}

                    // //foreach (var t in allEVR_23)
                    // //{
                    // //    string maBS1 = t.x2.MA_BAC_SI;
                    // //    string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    // //    string maBS2 = t.x3.MA_BAC_SI;
                    // //    string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    // //    AppendRow(xlSheetEVR, cellStyles, rows++,
                    // //            rows - 2, t.x2.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    // //            t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN, t.x2.MA_NHOM,
                    // //            t.x2.MA_KHOA, t.x2.XML01.MA_LOAI_KCB, "Thuốc", t.x2.TEN_THUOC, "", GetNgayThang(t.x2.NGAY_YL),
                    // //            t.x3.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    // //            t.x3.XML01.MA_BN, t.x3.XML01.HO_TEN, t.x3.MA_NHOM,
                    // //            t.x3.MA_KHOA, t.x3.XML01.MA_LOAI_KCB, "Dịch vụ", t.x3.TEN_DICH_VU, GetNgayThang(t.x3.NGAY_KQ), GetNgayThang(t.x3.NGAY_YL)
                    // //        );
                    // //}

                    // //foreach (var t in allEVR_33)
                    // //{
                    // //    string maBS1 = t.x31.MA_BAC_SI;
                    // //    string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    // //    string maBS2 = t.x32.MA_BAC_SI;
                    // //    string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    // //    AppendRow(xlSheetEVR, cellStyles, rows++,
                    // //            rows - 2, t.x31.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    // //            t.x31.XML01.MA_BN, t.x31.XML01.HO_TEN, t.x31.MA_NHOM,
                    // //            t.x31.MA_KHOA, t.x31.XML01.MA_LOAI_KCB, "Dịch vụ", t.x31.TEN_DICH_VU, GetNgayThang(t.x31.NGAY_KQ), GetNgayThang(t.x31.NGAY_YL),
                    // //            t.x32.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    // //            t.x32.XML01.MA_BN, t.x32.XML01.HO_TEN, t.x32.MA_NHOM,
                    // //            t.x32.MA_KHOA, t.x32.XML01.MA_LOAI_KCB, "Dịch vụ", t.x32.TEN_DICH_VU, GetNgayThang(t.x32.NGAY_KQ), GetNgayThang(t.x32.NGAY_YL)
                    // //        );
                    // //}

                    // //SetAutoFeatures(xlSheetEVR, cols, 3, 4, 15, 16);
                    // //#endregion

                    // #region Sheet 7: Canh bao Trung Ngay KQ DV
                    // var allNgayKQ = xml3EVR.Join(xml3EVR, x31 => x31.NGAY_KQ,
                    //                                     x32 => x32.NGAY_KQ,
                    //                                     (x31, x32) => new { x31, x32 })
                    //                         .Where(a => a.x31.MA_LK != a.x32.MA_LK && Uti.FirstMatch(a.x31.MA_BAC_SI, a.x32.MA_BAC_SI));

                    // ISheet xlSheetKQ = workbook.CreateSheet("Cảnh báo Trùng Ngày KQ");
                    // rows = 2;

                    // cols = CreateHeader(xlSheetKQ, cellStyles,
                    //             "Danh sách Cảnh báo trùng thời điểm Kết quả Dịch vụ", // Sheet title
                    //             "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                    //             "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1", "Mã Nhóm 1",
                    //             "Mã Khoa 1", "Loại KCB 1", "Loại 1", "Tên 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
                    //             "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                    //             "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2", "Mã Nhóm 2",
                    //             "Mã Khoa 2", "Loại KCB 2", "Loại 2", "Tên 2", "Ngày Kết quả 1", "Ngày Y lệnh 2");

                    // foreach (var t in allNgayKQ)
                    // {
                    //     string maBS1 = t.x31.MA_BAC_SI;
                    //     string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                    //     string maBS2 = t.x32.MA_BAC_SI;
                    //     string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                    //     AppendRow(xlSheetKQ, cellStyles, rows++,
                    //             rows - 2, t.x31.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                    //             t.x31.XML01.MA_BN, t.x31.XML01.HO_TEN, t.x31.MA_NHOM,
                    //             t.x31.MA_KHOA, t.x31.XML01.MA_LOAI_KCB, "Dịch vụ", t.x31.TEN_DICH_VU, GetNgayThang(t.x31.NGAY_KQ), GetNgayThang(t.x31.NGAY_YL),
                    //             t.x32.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                    //             t.x32.XML01.MA_BN, t.x32.XML01.HO_TEN, t.x32.MA_NHOM,
                    //             t.x32.MA_KHOA, t.x32.XML01.MA_LOAI_KCB, "Dịch vụ", t.x32.TEN_DICH_VU, GetNgayThang(t.x32.NGAY_KQ), GetNgayThang(t.x32.NGAY_YL)
                    //         );
                    // }

                    // SetAutoFeatures(xlSheetKQ, cols, 3, 4, 15, 16);
                    // #endregion


                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;

            }
            catch { }

            return null;
        }

        static public byte[] Download_BaoCaoTrungThuocDV(IEnumerable<XML01> data1, IEnumerable<DMKhoaBV> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
        {
            try
            {
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";

                var data2 = data1.SelectMany(x => x.XML02s);
                var data3 = data1.SelectMany(x => x.XML03s);
                var data4 = data1.SelectMany(x => x.XML04s);
                var data5 = data1.SelectMany(x => x.XML05s);

                byte[] result = null;
                using (var fs = new MemoryStream())
                {
                    #region Sheet 1: Thuoc X02
                    ISheet xlSheet2 = workbook.CreateSheet("DS Thuốc");
                    int rows = 2;
                    int cols = CreateHeader(xlSheet2, cellStyles,
                                "Danh sách tổng hợp Thuốc X02", // Sheet title
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
                                "TT Thầu", "Đơn giá", "Số lượng", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh","Ngày TH Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    foreach (var x2 in data2)
                    {
                        string maBS = x2.MA_BAC_SI;
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet2, cellStyles, rows++,
                                rows - 2, x2.XML01.MA_LK, x2.XML01.MA_BN, x2.XML01.HO_TEN,
                                x2.XML01.MA_THE_BHYT, GetNgayThang(x2.XML01.NGAY_SINH, true), GetGioiTinh(x2.XML01.GIOI_TINH),
                                x2.MA_KHOA, string.Format(fmKhoa, x2.MA_KHOA), maBS, ttBS[0], ttBS[1],
                                x2.XML01.MA_BENH_CHINH, x2.XML01.MA_BENH_KT, x2.XML01.CHAN_DOAN_RV,
                                x2.MA_THUOC, x2.MA_NHOM, x2.TEN_THUOC, x2.DON_VI_TINH, x2.LIEU_DUNG, x2.SO_DANG_KY,
                                x2.TT_THAU, x2.DON_GIA, x2.SO_LUONG, x2.TYLE_TT_BH, x2.THANH_TIEN_BV,
                                GetNgayThang(x2.NGAY_YL), GetNgayThang(x2.NGAY_TH_YL), GetNgayThang(x2.XML01.NGAY_VAO), GetNgayThang(x2.XML01.NGAY_RA), GetNgayThang(x2.XML01.NGAY_TTOAN));
                    }

                    SetAutoFeatures(xlSheet2, cols, 8, 10, 11, -14);
                    #endregion


                    #region Sheet 2: Dich vu XML3
                    var data3DVs = data3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.NumExists(x.MA_NHOM, 10, 11));

                    ISheet xlSheetDV = workbook.CreateSheet("DS Dịch vụ");
                    rows = 2;
                    cols = CreateHeader(xlSheetDV, cellStyles,
                                "Danh sách tổng hợp Dịch vụ XML3", // Sheet caption
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa", "Mã Bác Y lệnh", "Tên Bác sĩ", "Chức danh", "Mã Bác Sĩ Thực hiện","Tên Bác sĩ thực hiện","Chức danh",
                                "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                "Mã Dịch vụ","Mã nhóm", "Tên Dịch vụ","Mã giường","Mã máy",
                                "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh","Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    foreach (var x3 in data3DVs)
                    {
                        string maBS = x3.MA_BAC_SI;
                        string mabsTH = x3.NGUOI_THUC_HIEN;
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);
                        string[] ttbsTH= GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref mabsTH);

                        AppendRow(xlSheetDV, cellStyles, rows++,
                                rows - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                                x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetGioiTinh(x3.XML01.GIOI_TINH),
                                x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1], mabsTH, ttbsTH[0], ttbsTH[1],
                                x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                x3.MA_DICH_VU, x3.MA_NHOM, x3.TEN_DICH_VU,x3.MA_GIUONG,x3.MA_MAY,
                                x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.THANH_TIEN_BV,
                                GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_TH_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN));
                    }
                    SetAutoFeatures(xlSheetDV, cols, 8, 10, 11, -14);
                    #endregion


                    #region Sheet 3: Vat tu XML3
                    var data3VTs = data3.Where(x => !string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.NumExists(x.MA_NHOM, 10, 11));

                    ISheet xlSheetVT = workbook.CreateSheet("DS VTYT");
                    rows = 2;
                    cols = CreateHeader(xlSheetVT, cellStyles, "Danh sách tổng hợp Vật tư XML3",
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                "Mã Vật tư", "Tên Vật tư",
                                "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    foreach (var x3 in data3VTs)
                    {
                        string maBS = x3.MA_BAC_SI;
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheetVT, cellStyles, rows++,
                                rows - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                                x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetGioiTinh(x3.XML01.GIOI_TINH),
                                x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1],
                                x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                x3.MA_VAT_TU, x3.TEN_VAT_TU,
                                x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.THANH_TIEN_BV,
                                GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN));
                    }

                    SetAutoFeatures(xlSheetVT, cols, 8, 10, 11, -14);
                    #endregion

                    //-------------------------------------------------------------
                    #region Sheet 4: XML4
                    

                    ISheet xlSheetXML4 = workbook.CreateSheet("DS XML4");
                    rows = 2;
                    cols = CreateHeader(xlSheetXML4, cellStyles, "Danh sách tổng hợp XML4",
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                //"Mã Khoa", "Tên Khoa" //"Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                //"Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                //"Mã Vật tư", "Tên Vật tư",
                                //"ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
                                //"Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");
                                "Mã dịch vụ", "Mã chỉ số", "Tên chỉ số", "Giá Trị", "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả", "Mã BS Đọc kết quả","Tên BS Đọc KQ", "Chức danh");

                    foreach (var x4 in data4)
                    {
                        string maBS = x4.MA_BS_DOC_KQ;
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheetXML4, cellStyles, rows++,
                                rows - 2, x4.XML01.MA_LK, x4.XML01.MA_BN, x4.XML01.HO_TEN,
                                x4.XML01.MA_THE_BHYT, GetNgayThang(x4.XML01.NGAY_SINH, true), GetGioiTinh(x4.XML01.GIOI_TINH),
                                //x4.MA_KHOA, string.Format(fmKhoa, x4.MA_KHOA), //maBS, ttBS[0], ttBS[1],
                                //x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                //x3.MA_VAT_TU, x3.TEN_VAT_TU,
                                //x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.THANH_TIEN_BV,
                                //GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN)
                                x4.MA_DICH_VU, x4.MA_CHI_SO, x4.TEN_CHI_SO, x4.GIA_TRI, x4.DON_VI_DO, x4.MO_TA, x4.KET_LUAN, GetNgayThang(x4.NGAY_KQ), maBS, ttBS[0], ttBS[1]
                                );

                    }

                    SetAutoFeatures(xlSheetXML4, cols, 8, 10, 11, -14);
                    #endregion

                    #region Sheet 5: XML5


                    ISheet xlSheetXML5 = workbook.CreateSheet("DS XML5");
                    rows = 2;
                    cols = CreateHeader(xlSheetXML5, cellStyles, "Danh sách tổng hợp XML5",
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                //"Mã Khoa", "Tên Khoa" //"Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                //"Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                //"Mã Vật tư", "Tên Vật tư",
                                //"ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
                                //"Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");
                                //"Mã dịch vụ", "Mã chỉ số", "Tên chỉ số", "Giá Trị", "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả", "Mã BS Đọc kết quả", "Tên BS Đọc KQ", "Chức danh"
                                "Diễn biến LS","Giai đoạn bệnh","Hội chẩn","Phẫu thuật","Thời điểm diễn biến lâm sàng","Mã người thực hiện","Tên người thực hiện","Chức danh"
                                );

                    foreach (var x5 in data5)
                    {
                        string maBS = x5.NGUOI_THUC_HIEN;
                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheetXML5, cellStyles, rows++,
                                rows - 2, x5.XML01.MA_LK, x5.XML01.MA_BN, x5.XML01.HO_TEN,
                                x5.XML01.MA_THE_BHYT, GetNgayThang(x5.XML01.NGAY_SINH, true), GetGioiTinh(x5.XML01.GIOI_TINH),
                                //x4.MA_KHOA, string.Format(fmKhoa, x4.MA_KHOA), //maBS, ttBS[0], ttBS[1],
                                //x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                //x3.MA_VAT_TU, x3.TEN_VAT_TU,
                                //x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.THANH_TIEN_BV,
                                //GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN)
                                //x5.MA_DICH_VU, x4.MA_CHI_SO, x4.TEN_CHI_SO, x4.GIA_TRI, x4.DON_VI_DO, x4.MO_TA, x4.KET_LUAN, GetNgayThang(x4.NGAY_KQ), maBS, ttBS[0], ttBS[1]
                                x5.DIEN_BIEN_LS,x5.GIAI_DOAN_BENH,x5.HOI_CHAN,x5.PHAU_THUAT, GetNgayThang(x5.THOI_DIEM_DBLS), maBS, ttBS[0], ttBS[1]
                                );

                    }

                    SetAutoFeatures(xlSheetXML5, cols, 8, 10, 11, -14);
                    #endregion

                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;

            }
            catch (Exception ex)
            {
                string stk = ex.StackTrace;
            }

            return null;
        }

        static public byte[] Download_BaoCaoChiDinhDV(IEnumerable<XML01> data1, IEnumerable<ChiDinhDongThoi> dmDongThois)
        {
            try
            {
                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                var data3 = data1.SelectMany(x => x.XML03s);

                byte[] result = null;
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
                            var dichVus = data3.Where(x => x.XML01ID == xml1.XML01ID && Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU) && x.NGAY_YL != null);
                            var dvKhacs = data3.Where(x => x.XML01ID == xml1.XML01ID && Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null);
                            var allDates = dichVus.Select(x => x.NGAY_YL.Value.Date).Distinct().ToList();
                            allDates.AddRange(dvKhacs.Select(x => x.NGAY_YL.Value.Date).Distinct().ToList());

                            foreach (var dte in allDates)
                            {
                                var matches1 = dichVus.Where(x => x.NGAY_YL.Value.Date == dte).ToList();
                                var matches2 = dvKhacs.Where(x => x.NGAY_YL.Value.Date == dte).ToList();

                                var cnt1 = matches1.Count();
                                var cnt2 = matches2.Count();

                                int cnt = Math.Max(cnt1, cnt2);
                                for (int i = 0; i < cnt; i++)
                                {
                                    var m1 = i < cnt1 ? matches1[i] : new XML03();
                                    var m2 = i < cnt2 ? matches2[i] : new XML03();

                                    AppendRow(xlSheet930, cellStyles, rows++,
                                                rows - 2, xml1.MA_LK, xml1.MA_BN, xml1.HO_TEN,
                                                xml1.MA_THE_BHYT, GetNgayThang(xml1.NGAY_SINH, true), GetGioiTinh(xml1.GIOI_TINH),
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
                            var dvMatches = data3.Where(x => x.XML01ID == xml1.XML01ID && string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU));
                            var vtMatches = data3.Where(x => x.XML01ID == xml1.XML01ID && Uti.CodeMatch(x.MA_VAT_TU, rule.VAT_TU_BHYT));

                            if (!vtMatches.Any())
                            {
                                foreach (var x3 in dvMatches)
                                {
                                    if (errDVs.Contains(x3)) continue;
                                    if (dongThoi_err1004.Any(r => Uti.CodeMatch(x3.MA_DICH_VU, r.MA_DICH_VU) && Uti.CodeMatch(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

                                    errDVs.Add(x3);
                                }
                            }
                            else if (!dvMatches.Any())
                            {
                                foreach (var x3 in vtMatches)
                                {
                                    if (errVTs.Contains(x3)) continue;
                                    if (dongThoi_err1004.Any(r => Uti.CodeMatch(x3.MA_DICH_VU, r.MA_DICH_VU) && Uti.CodeMatch(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

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
                                x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetGioiTinh(x3.XML01.GIOI_TINH),
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
                                x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetGioiTinh(x3.XML01.GIOI_TINH),
                                x3.MA_VAT_TU, x3.TEN_VAT_TU, x3.NGAY_YL, x3.NGAY_KQ,
                                "Chỉ định vật tư kỹ thuật không có dịch vụ kỹ thuật đồng thời"
                            );
                    }

                    SetAutoFeatures(xlSheetVT1004, cols);
                    #endregion

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;

            }
            catch { }

            return null;
        }

        static public byte[] Download_BaoCaoThongkeLoi(IEnumerable<XML01> data1, IEnumerable<XMLError> dataE, IEnumerable<XMLErrorInfo> dmErrors)
        {
            try
            {
                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                var allKhoas = data1.Select(x => x.MA_KHOA).Distinct().OrderBy(x => x).ToList();
                dmErrors = dmErrors.OrderBy(e => e.error_file);

                byte[] result = null;
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

                    var grpErrs = dataE.Select(e => new { e.XML01ID, e.XML01.MA_KHOA, e.XMLEID, e.MA_LOI, e.STT })
                                        .GroupBy(e => new { e.MA_KHOA, e.MA_LOI })
                                        .Select(g => new { g.Key.MA_KHOA, g.Key.MA_LOI, SO_LOI = g.Count() });

                    foreach (var e in dmErrors)
                    {
                        var lst = grpErrs.Where(x => x.MA_LOI == e.error_code);
                        if (!lst.Any()) continue;

                        List<object> values = new()
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

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                string stk = ex.StackTrace;
            }

            return null;
        }

        static public byte[] Download_BaoCaoTrungMaMayXml4(IEnumerable<XML01> data1)//IEnumerable<DMDichVu> dataDV, IEnumerable<XML03> data3, IEnumerable<XML04> data4)
        {
            try
            {
                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                //var allXml4 = data4.Where(x => !string.IsNullOrWhiteSpace(x.TEN_CHI_SO?.Trim()) && Uti.StrExists(x.TEN_CHI_SO.Substring(0, 2), true, "CL", "CT", "MR", "SA", "XQ"));
                //string fmSoSanh = "$=COUNTIFS(I:I,I{0},N:N,N{0})+COUNTIFS(I:I,I{0},N:N,N{0}-TIME(0,{1},0))+COUNTIFS(I:I,I{0},N:N,N{0}+TIME(0,{1},0))";

                byte[] result = null;
                using (var fs = new MemoryStream())
                {
                    #region Sheet 1: Bang filter loi
                    ISheet xlSheetFilter = workbook.CreateSheet("DS Trùng");
                    int rows = 2;
                    int cols = CreateHeader(xlSheetFilter, cellStyles,
                                "Danh sách Trùng mã máy và giờ kết quả tại XML4", // Sheet title
                                "STT", "Tên file", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ",
                                "Mã Dịch vụ", "Tên dịch vụ", "Mã Máy", "Mã Nhóm", //"Mã Chỉ số", "Tên Chỉ số",
                                                                                  //"Giá trị", "Mô tả", "Kết luận",
                                "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    IRow row = xlSheetFilter.CreateRow(2);
                    ICell cell = row.CreateCell(0);
                    //cell.SetCellValue($"=FILTER('XML4'!A3:M{(allXml4.Count() + 2)},'XML4'!O3:O{(allXml4.Count() + 2)}>=2,\"\")");
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

                    foreach (var t in dataJoin)//allXml4)
                    {
                        //string mmSoSanh = string.Format(fmSoSanh, rows + 1, 2);
                        //if (t.TEN_CHI_SO.StartsWith("CL") || t.TEN_CHI_SO.StartsWith("CT"))
                        //{
                        //    mmSoSanh = string.Format(fmSoSanh, rows + 1, 4);
                        //}
                        //else if (t.TEN_CHI_SO.StartsWith("MR"))
                        //{
                        //    mmSoSanh = string.Format(fmSoSanh, rows + 1, 9);
                        //}

                        //if (t.MA_DICH_VU != maDV)
                        //{
                        //    maDV = t.MA_DICH_VU;
                        //    tenDV = (dataDV.FirstOrDefault(x => x.MA_DICH_VU == t.MA_DICH_VU)?.TEN_DICH_VU) ?? "";
                        //}

                        AppendRow(xlSheetFilter, cellStyles, rows++,
                                rows - 2, t.XML01.FileName, t.XML01.MA_LK, t.XML01.MA_BN, t.XML01.HO_TEN, t.XML01.MA_THE_BHYT,
                                t.MA_DICH_VU, t.TEN_DICH_VU, t.TEN_CHI_SO, t.MA_NHOM, //t.MA_CHI_SO, t.TEN_CHI_SO,
                                                                                      //t.GIA_TRI, t.MO_TA, t.KET_LUAN,
                                t.NGAY_KQ, t.XML01.NGAY_VAO, t.XML01.NGAY_RA, t.XML01.NGAY_TTOAN,
                                string.Format("{0:yyyy/MM/dd HH:mm:ss}", t.NGAY_KQ)//, mmSoSanh
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
                                "Mã Dịch vụ", "Tên dịch vụ", "Mã Máy", "Mã Nhóm", //"Mã Chỉ số", "Tên Chỉ số",
                                                                       //"Giá trị", "Mô tả", "Kết luận",
                                "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    //string maDV = "";
                    //string tenDV = "";
                    var allXml4 = data1.SelectMany(x => x.XML04s).OrderBy(x => x.XML01ID).ToList();
                    foreach (var t in allXml4)
                    {
                        //string mmSoSanh = string.Format(fmSoSanh, rows + 1, 2);
                        //if (t.TEN_CHI_SO.StartsWith("CL") || t.TEN_CHI_SO.StartsWith("CT"))
                        //{
                        //    mmSoSanh = string.Format(fmSoSanh, rows + 1, 4);
                        //}
                        //else if (t.TEN_CHI_SO.StartsWith("MR"))
                        //{
                        //    mmSoSanh = string.Format(fmSoSanh, rows + 1, 9);
                        //}

                        //if (t.MA_DICH_VU != maDV)
                        //{
                        //    maDV = t.MA_DICH_VU;
                        //    tenDV = (dataDV.FirstOrDefault(x => x.MA_DICH_VU == t.MA_DICH_VU)?.TEN_DICH_VU) ?? "";
                        //}

                        //AppendRow(xlSheetX4, cellStyles, rows++,
                        //        rows - 2, t.XML01.FileName, t.XML01.MA_LK, t.XML01.MA_BN, t.XML01.HO_TEN, t.XML01.MA_THE_BHYT,
                        //        t.MA_DICH_VU, t.TEN_DICH_VU, t.TEN_CHI_SO, t.MA_NHOM, //t.MA_CHI_SO, t.TEN_CHI_SO,
                        //                                       //t.GIA_TRI, t.MO_TA, t.KET_LUAN,
                        //        t.NGAY_KQ, t.XML01.NGAY_VAO, t.XML01.NGAY_RA, t.XML01.NGAY_TTOAN,
                        //        string.Format("{0:yyyy/MM/dd HH:mm:ss}", t.NGAY_KQ)//, mmSoSanh
                        //    );
                    }

                    SetAutoFeatures(xlSheetX4, cols);
                    #endregion

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;

            }
            catch { }

            return null;
        }

        static public byte[] Download_BaoCaoChamCong(IEnumerable<XML02> data2, IEnumerable<XML03> data3, IEnumerable<XML04> data4, IEnumerable<XML05> data5, IEnumerable<DMBacSi> dmBacsis, DateTime dStart, DateTime dEnd)
        {
            string[] dows = new string[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };
            string[] cdnn = new string[] { "", "Bác sĩ", "Y sĩ", "Điều dưỡng", "Hộ sinh", "Kỹ thuật viên", "Cử nhân X-quang", "Dược sĩ đại học", "Dược sĩ trung cấp" };

            //var data2 = data1.SelectMany(x => x.XML02s);
            //var data3 = data1.SelectMany(x => x.XML03s);

            try
            {
                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                var allBS = dmBacsis.OrderBy(b => b.MA_KHOA);
                //var notBS = dmBacsis.Where(b => b.CHUCDANH_NN != "1").OrderBy(b => b.MA_KHOA);

                //Lấy MA_BS & NGAY_YL của BS đó từ XML2
                var data2YL = data2.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date });

                //Lấy MA_BS & NGAY_YL của BS đó từ XML3
                var data3YL = data3.Select(x => new { MA_BAC_SI = allBS.FirstOrDefault(b => x.MA_BAC_SI == b.MA_CCHN || x.MA_BAC_SI.StartsWith(b.MA_CCHN + ";"))?.MA_CCHN, NGAY_LV = x.NGAY_YL })
                                    .Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI))
                                    .GroupBy(x => new { x.MA_BAC_SI, x.NGAY_LV.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date });

                //var data3VT = data3.Where(x => Uti.NumExists(x.MA_NHOM, 10, 11) && x.NGAY_YL < dEnd);
                //var data3KQ = data3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11) && x.NGAY_KQ >= dStart);

                //Lấy NGUOI_THUC_HIEN & NGAY_THYL của NGUOI_THUC_HIEN đó từ XML3
                var data3THYL =data3.Where(x=> !Uti.NumExists(x.MA_NHOM, 10, 11))
                                               .Select(x => new { MA_BAC_SI = allBS.FirstOrDefault(b => x.NGUOI_THUC_HIEN == b.MA_CCHN || x.NGUOI_THUC_HIEN.Contains(b.MA_CCHN + ";"))?.MA_CCHN, NGAY_LV = x.NGAY_TH_YL })
                                               .Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI))
                                               .GroupBy(x => new { x.MA_BAC_SI, x.NGAY_LV.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date });

                //Lấy MA_BS & NGAY_KQ của BS đó từ XML4
                var data4KQ = data4.Select(x => new { MA_BAC_SI = allBS.FirstOrDefault(b => x.MA_BS_DOC_KQ == b.MA_CCHN || x.MA_BS_DOC_KQ.Contains(b.MA_CCHN + ";"))?.MA_CCHN, NGAY_LV = x.NGAY_KQ })
                                               .Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI))
                                               .GroupBy(x => new { x.MA_BAC_SI, x.NGAY_LV.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date });

                //Lấy NGUOI_THUC_HIEN & THOI_DIEM_DBLS của BS đó từ XML5
                var data5TD = data5.Select(x => new { MA_BAC_SI = allBS.FirstOrDefault(b => x.NGUOI_THUC_HIEN == b.MA_CCHN || x.NGUOI_THUC_HIEN.Contains(b.MA_CCHN + ";"))?.MA_CCHN, NGAY_LV = x.THOI_DIEM_DBLS })
                                               .Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI))
                                               .GroupBy(x => new { x.MA_BAC_SI, x.NGAY_LV.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date });

                byte[] result = null;
                using (var fs = new MemoryStream())
                {
                    var dataBS = data2YL
                                    .Union(data3YL)
                                    .Union(data3THYL)
                                    .Union(data4KQ)
                                    .Union(data5TD)
                                    .Distinct()
                                    .GroupBy(x => x.NGAY_LV, (x, g) => new { MA_BAC_SI = ";" + string.Join(';', g.Select(x => x.MA_BAC_SI).Distinct().ToArray()) + ";", NGAY_LV = x })
                                    .ToList();

                    //var dataKT = data3KQ.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_KQ.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date })
                    //                .Union(data3VT.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date }))
                    //                .Distinct()
                    //                .GroupBy(x => x.NGAY_LV, (x, g) => new { MA_BAC_SI = ";" + string.Join(';', g.Select(x => x.MA_BAC_SI).Distinct().ToArray()) + ";", NGAY_LV = x })
                    //                .ToList();

                    ISheet xlSheetBS = workbook.CreateSheet("Bác sĩ");
                    int colBS = CreateHeaderForWorkDays(xlSheetBS, dStart, cellStyles);
                    //ISheet xlSheetKT = workbook.CreateSheet("Kỹ thuật viên");
                    //int colKT = CreateHeaderForWorkDays(xlSheetKT, dStart, cellStyles);

                    //Add data for each bacsi
                    int rows = 5;
                    foreach (var bs in allBS)
                    {
                        //cds = 0;
                        //int.TryParse(bs.CHUCDANH_NN, out cds);
                        //AppendRow(xlSheetBS, cellStyles, rows++, rows - 5, bs.MACCHN, bs.HO_TEN, bs.CHUCDANH_NN, bs.MA_KHOA, bs.TEN_KHOA);
                        AppendRow(xlSheetBS, cellStyles, rows++, rows - 5, bs.MA_CCHN, bs.HO_TEN, bs.CHUC_DANH, bs.MA_KHOA, bs.TEN_KHOA);
                    }

                    ////Add data for each kythuatvien
                    //rows = 5;
                    //foreach (var bs in notBS)
                    //{
                    //    cds = 0;
                    //    int.TryParse(bs.CHUCDANH_NN, out cds);
                    //    AppendRow(xlSheetKT, cellStyles, rows++, rows - 5, bs.MACCHN, bs.HO_TEN, cds >= cdnn.Length ? cds : cdnn[cds], bs.MA_KHOA, bs.TEN_KHOA);
                    //}

                    ICell xcel;
                    ICellStyle celStyle;
                    int cols = colBS;
                    DateTime dte = dStart;
                    while (dte < dEnd)
                    {
                        cols++;
                        celStyle = (dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday) ? cellStyles[2] : cellStyles[1];

                        //Add header for date
                        AddDateHeaderForWorkDays(xlSheetBS, cols, dte, dows[(int)dte.DayOfWeek], celStyle);
                        //AddDateHeaderForWorkDays(xlSheetKT, cols, dte, dows[(int)dte.DayOfWeek], celStyle);

                        //Add work days for each bacsi
                        rows = 5;
                        celStyle = (dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday) ? cellStyles[6] : cellStyles[3];
                        var maBS = dte > DateTime.Now ? "" : dataBS.FirstOrDefault(w => w.NGAY_LV == dte)?.MA_BAC_SI;
                        foreach (var bs in allBS)
                        {
                            xcel = xlSheetBS.GetRow(rows++).CreateCell(cols);
                            xcel.CellStyle = celStyle;
                            if (dte > DateTime.Now)
                            {
                                xcel.SetCellValue("");
                            }
                            else
                            {
                                xcel.SetCellValue(maBS.Contains($";{bs.MA_CCHN};") ? "Đ" : "N");
                            }
                        }

                        ////Add work days for each kythuatvien
                        //rows = 5;
                        //var maKT = dte > DateTime.Now ? "" : dataKT.FirstOrDefault(w => w.NGAY_LV == dte)?.MA_BAC_SI;
                        //foreach (var bs in notBS)
                        //{
                        //    xcel = xlSheetKT.GetRow(rows++).CreateCell(cols);
                        //    xcel.CellStyle = celStyle;
                        //    if (dte > DateTime.Now)
                        //    {
                        //        xcel.SetCellValue("");
                        //    }
                        //    else
                        //    {
                        //        xcel.SetCellValue(maKT.Contains($";{bs.MACCHN};") ? "Đ" : "N");
                        //    }
                        //}

                        dte = dte.AddDays(1);
                    }

                    xlSheetBS.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
                    xlSheetBS.AddMergedRegion(new CellRangeAddress(1, 1, 0, cols));
                    SetAutoFeatures(xlSheetBS, cols + 1);

                    //xlSheetKT.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
                    //xlSheetKT.AddMergedRegion(new CellRangeAddress(1, 1, 0, cols));
                    //SetAutoFeatures(xlSheetKT, cols + 1);

                    workbook.Write(fs);
                    result = fs.ToArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                string stk = ex.StackTrace;
            }

            return null;
        }
    }
}