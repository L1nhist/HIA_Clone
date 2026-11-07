using System.Data;
using System.Globalization;
using System.Reflection;
using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Rules;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace HIA.Services.Base;

public class HandleFile130CommonService
{
    private static readonly Dictionary<Type, string[][]> FieldMappings = new()
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
                "MA_KHOA,TEN_KHOA".Split(','),
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
                [],
                [],
            }
        }
    };

    public static List<T> UploadExcel<T>(IFormFile request, List<string> keysString) where T : class
    {
        var dtTable = new DataTable();
        var rowList = new List<string?>();
        using (var stream = new MemoryStream())
        {
            request.CopyTo(stream);
            stream.Position = 0;
            var xssWorkbook = new XSSFWorkbook(stream);
            ISheet sheet = xssWorkbook.GetSheetAt(0);
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;
            var ignoreColumns = new List<int>();

            //Linhist added 09/01/23
            //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
            int colNhom9 = -1;
            int colNhom5 = -1;
            int colHamLuong = -1;
            // Cot Excel co cot dich la DateTime: o ngay goc doi thanh ExcelDate.Format de ToEntities parse lai.
            var dateCols = new HashSet<int>();

            string[][] flds = FieldMappings[typeof(T)];
            for (int j = 0; j < cellCount; j++)
            {
                ICell cell = headerRow.GetCell(j);
                var header = cell?.ToString();
                if (string.IsNullOrWhiteSpace(header)) continue;
                if (keysString.Contains(header))
                {
                    ignoreColumns.Add(j);
                    continue;
                }

                string capt = Uti.ReplaceValuePairs(header, flds);
                if (!dtTable.Columns.Contains(capt))
                {
                    dtTable.Columns.Add(capt);
                }
                if (IsDateTarget<T>(capt)) dateCols.Add(j);

                if (Uti.IsEqual(header.Trim(), "MANHOM_9324", ignoreCase: true))
                {
                    colNhom9 = dtTable.Columns.Count - 1;
                }
                if (Uti.IsEqual(header.Trim(), "MANHOM_5937", ignoreCase: true))
                {
                    colNhom5 = dtTable.Columns.Count - 1;
                }

                if (Uti.IsEqual(header.Trim(), "HAM_LUONG", ignoreCase: true))
                {
                    colHamLuong = j;
                }
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
                        rowList.Add(row.GetCell(j)?.ToString()?.Trim());
                    }
                    else if (row.GetCell(j) != null)
                    {
                        var cellValue = CellText(row.GetCell(j), dateCols.Contains(j))?.Trim();
                        if (cellValue != null && double.TryParse(cellValue, out _))
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
                    if (colNhom5 != -1 && colNhom9 != -1)
                    {
                        if (string.IsNullOrEmpty(rowList[colNhom5]))
                        {
                            rowList[colNhom5] = rowList[colNhom9];
                        }
                    }

                    dtTable.Rows.Add(rowList.ToArray());
                }
                rowList.Clear();
            }
        }

        return ToEntities<T>(dtTable);
    }

    public static byte[] DownloadExcel<T>(IEnumerable<T> data) where T : class
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();

        using var fs = new MemoryStream();
        // xlsx: XSSFWorkbook  --- xls: HSSFWorkbook
        IWorkbook workbook = new XSSFWorkbook();
        ISheet excelSheet = workbook.CreateSheet("Sheet1");

        IRow row = excelSheet.CreateRow(0);
        for (int columnIndex = 0; columnIndex < props.Count; columnIndex++)
        {
            row.CreateCell(columnIndex).SetCellValue(props[columnIndex].Name);
        }

        int rowIndex = 1;
        foreach (var item in data)
        {
            row = excelSheet.CreateRow(rowIndex);
            for (int cellIndex = 0; cellIndex < props.Count; cellIndex++)
            {
                row.CreateCell(cellIndex).SetCellValue(ExportText(props[cellIndex].GetValue(item)));
            }

            rowIndex++;
        }
        workbook.Write(fs);
        return fs.ToArray();
    }

    // Map DataTable (all string cells) -> entities by case-insensitive property name, culture-invariant conversion.
    private static List<T> ToEntities<T>(DataTable table) where T : class
    {
        var props = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.CanWrite && p.GetIndexParameters().Length == 0) props.TryAdd(p.Name, p);
        }

        var columns = table.Columns.Cast<DataColumn>()
            .Select(c => props.TryGetValue(c.ColumnName, out var p) ? p : null)
            .ToArray();

        var result = new List<T>(table.Rows.Count);
        foreach (DataRow dataRow in table.Rows)
        {
            var item = Activator.CreateInstance(typeof(T)) as T
                ?? throw new InvalidOperationException($"Cannot create {typeof(T).Name}.");
            for (int c = 0; c < columns.Length; c++)
            {
                var prop = columns[c];
                if (prop == null || dataRow[c] is not string text) continue;

                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (type == typeof(string))
                {
                    prop.SetValue(item, text);
                    continue;
                }

                if (text.Length == 0) continue;
                if (type == typeof(DateTime))
                {
                    if (!ExcelDate.TryParse(text, out var date))
                        throw new FormatException($"Ngay khong hop le o cot {prop.Name}: '{text}'.");
                    prop.SetValue(item, date);
                    continue;
                }
                prop.SetValue(item, Convert.ChangeType(text, type, CultureInfo.InvariantCulture));
            }
            result.Add(item);
        }

        return result;
    }

    // Cot dich la DateTime (co the Nullable) -> true. Khop ten cot theo cach ToEntities khop thuoc tinh.
    private static bool IsDateTarget<T>(string column)
    {
        var prop = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.CanWrite && p.GetIndexParameters().Length == 0
                && string.Equals(p.Name, column, StringComparison.OrdinalIgnoreCase));
        return prop != null && (Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType) == typeof(DateTime);
    }

    // Ngay Excel khi chi co gio: NPOI tra 30/12/1899 + gio (serial < 1).
    private static readonly DateTime ExcelTimeOnlyDate = new(1899, 12, 30);

    // O ngay goc cua Excel (so co dinh dang ngay):
    //  - cot dich DateTime -> ExcelDate.Format (ToEntities parse lai);
    //  - cot dich chuoi, chi co gio -> "HH:mm:ss" (khong sinh ngay gia);
    //  - cot dich chuoi, ngay-gio hoac chi ngay -> ExcelDate.Format.
    // Cac o khac giu ToString().
    private static string? CellText(ICell? cell, bool toDate)
    {
        if (cell == null) return null;
        if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell) && cell.DateCellValue is DateTime date)
        {
            if (!toDate && date.Date == ExcelTimeOnlyDate)
                return date.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            return ExcelDate.Format(date);
        }
        return cell.ToString();
    }

    // Gia tri xuat ra o text: DateTime theo ExcelDate.OutputFormat (invariant), con lai ToString() nhu cu.
    private static string ExportText(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => ExcelDate.Format(date),
        _ => value.ToString() ?? string.Empty,
    };
}
