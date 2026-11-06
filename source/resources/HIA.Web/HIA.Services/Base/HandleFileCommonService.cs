//using HIA.Core.Base;
//using HIA.Core.Models.Categories;
//using HIA.DatabaseModel.Entites;
//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using NPOI.SS.UserModel;
//using NPOI.SS.Util;
//using NPOI.XSSF.UserModel;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;

//namespace HIA.Services.Base
//{
//    public class HandleFileCommonService
//    {
//        private static readonly Dictionary<Type, string[][]> FieldMappings;

//        static HandleFileCommonService()
//        {
//            FieldMappings = new()
//            {
//                {
//                    typeof(DMThuoc),
//                    new string[][]
//                    {
//                        "DEN_NGAY,DENNGAY".Split(','),
//                        "DEN,DEN_NGAY".Split(',')
//                    }
//                },
//                {
//                    typeof(DMDichVu),
//                    new string[][]
//                    {
//                        "MA_TUONG_DUONG,TEN_DVKT_PHEDUYET,QUY_TRINH,TUNGAY,DENNGAY".Split(','),
//                        "MA_DICH_VU,TEN_DICH_VU,QUYET_DINH,TU_NGAY,DEN_NGAY".Split(','),
//                    }
//                },
//                {
//                    typeof(DMDVThuCong),
//                    new string[][]
//                    {
//                        "MA_TUONG_DUONG,TEN_DVKT_PHEDUYET,QUY_TRINH,TUNGAY,DENNGAY".Split(','),
//                        "MA_DICH_VU,TEN_DICH_VU,QUYET_DINH,TU_NGAY,DEN_NGAY".Split(','),
//                    }
//                },
//                {
//                    typeof(DMVTYT),
//                    new string[][]
//                    {
//                        "HANG_SX,DENNGAY".Split(','),
//                        "NHA_SX,DEN_NGAY".Split(','),
//                    }
//                },
//                {
//                    typeof(DMBacSi),
//                    new string[][]
//                    {
//                        "".Split(','),
//                        "".Split(',')
//                    }
//                },
//                {
//                    typeof(DMKhoaBV),
//                    new string[][]
//                    {
//                        "MA,TEN".Split(','),
//                        "MA_KHOA,TEN_KHOA".Split (','),
//                    }
//                },
//                {
//                    typeof(DMThietBi),
//                    new string[][]
//                    {
//                        "TEN_TB".Split(','),
//                        "TEN_THIET_BI".Split(','),
//                    }
//                }
//            };
//        }

//        static public List<T> UploadExcel<T>(IFormFile request, List<string> keysString)
//        {
//            List<T> data;

//            DataTable dtTable = new DataTable();
//            List<string> rowList = new List<string>();
//            ISheet sheet;
//            using (var stream = new MemoryStream())
//            {
//                request.CopyTo(stream);
//                stream.Position = 0;
//                XSSFWorkbook xssWorkbook = new XSSFWorkbook(stream);
//                sheet = xssWorkbook.GetSheetAt(0);
//                IRow headerRow = sheet.GetRow(0);
//                int cellCount = headerRow.LastCellNum;
//                List<int> ignoreColumns = new List<int>();

//                //Linhist added 09/01/23
//                //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
//                int colNhom9 = -1;
//                int colNhom5 = -1;
//                int colHamLuong = -1;

//                string[][] flds = FieldMappings[typeof(T)];
//                for (int j = 0; j < cellCount; j++)
//                {
//                    ICell cell = headerRow.GetCell(j);
//                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
//                    if (keysString.Contains(cell.ToString()))
//                    {
//                        ignoreColumns.Add(j);
//                        continue;
//                    }

//                    string capt = Uti.ReplaceValuePairs(cell.ToString(), flds);
//                    if (!dtTable.Columns.Contains(capt))
//                    {
//                        dtTable.Columns.Add(capt);
//                    }

//                    //Linhist added 09/01/23
//                    //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
//                    if (Uti.IsEqual(cell.ToString().Trim(), "MANHOM_9324", ignoreCase: true))
//                    {
//                        colNhom9 = dtTable.Columns.Count - 1;
//                    }
//                    if (Uti.IsEqual(cell.ToString().Trim(), "MANHOM_5937", ignoreCase: true))
//                    {
//                        colNhom5 = dtTable.Columns.Count - 1;
//                    }

//                    //Linhist added 09/01/23
//                    if (Uti.IsEqual(cell.ToString().Trim(), "HAM_LUONG", ignoreCase: true))
//                    {
//                        colHamLuong = j;
//                    }

//                    //Linhist removed 06/03/24
//                    //Linhist added 08/03/23
//                    //Dat la ten cot MA --> MA_KHOA, TEN --> TEN_KHOA trong bang DMKhoaBV
//                    //if (Uti.IsEqual(cell.ToString().Trim(), "MA", ignoreCase: true))
//                    //{
//                    //    dtTable.Columns[dtTable.Columns.Count - 1].ColumnName = "MA_KHOA";
//                    //}
//                    //if (Uti.IsEqual(cell.ToString().Trim(), "TEN", ignoreCase: true))
//                    //{
//                    //    dtTable.Columns[dtTable.Columns.Count - 1].ColumnName = "TEN_KHOA";
//                    //}
//                }

//                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
//                {
//                    IRow row = sheet.GetRow(i);
//                    if (row == null) continue;
//                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
//                    for (int j = row.FirstCellNum; j < cellCount; j++)
//                    {
//                        if (ignoreColumns.Contains(j)) continue;

//                        if (colHamLuong > -1 && j == colHamLuong)
//                        {
//                            rowList.Add(row.GetCell(j)?.ToString().Trim());
//                        }
//                        else if (row.GetCell(j) != null)
//                        {
//                            var cellValue = row.GetCell(j)?.ToString().Trim();
//                            if (double.TryParse(cellValue, out double numericValue))
//                                cellValue = cellValue.Replace(',', '.');

//                            rowList.Add(cellValue);
//                        }
//                        else
//                        {
//                            rowList.Add(null);
//                        }
//                    }
//                    if (rowList.Count > 0)
//                    {
//                        //Linhist added 09/01/23
//                        //Add code fix MANHOM_5937 empty (replace with MANHOM_9234)
//                        if (colNhom5 != -1 && colNhom9 != -1)
//                        {
//                            if (rowList[colNhom5] == null || rowList[colNhom5].ToString() == "")
//                            {
//                                rowList[colNhom5] = rowList[colNhom9];
//                            }
//                        }

//                        dtTable.Rows.Add(rowList.ToArray());
//                    }
//                    rowList.Clear();
//                }
//            }

//            return JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(dtTable));
//        }

//        static public byte[] DownloadExcel<T>(IEnumerable<T> data)
//        {
//            DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data), (typeof(DataTable)));
//            byte[] result;
//            using (var fs = new MemoryStream())
//            {
//                // xlsx: XSSFWorkbook  --- xls: HSSFWorkbook  
//                IWorkbook workbook = new XSSFWorkbook();
//                ISheet excelSheet = workbook.CreateSheet("Sheet1");

//                List<string> columns = new List<string>();
//                IRow row = excelSheet.CreateRow(0);
//                int columnIndex = 0;

//                foreach (DataColumn column in table.Columns)
//                {
//                    columns.Add(column.ColumnName);
//                    row.CreateCell(columnIndex).SetCellValue(column.ColumnName);
//                    columnIndex++;
//                }

//                int rowIndex = 1;
//                foreach (DataRow dsrow in table.Rows)
//                {
//                    row = excelSheet.CreateRow(rowIndex);
//                    int cellIndex = 0;
//                    foreach (string col in columns)
//                    {
//                        row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
//                        cellIndex++;
//                    }

//                    rowIndex++;
//                }
//                workbook.Write(fs);
//                result = fs.ToArray();
//            }

//            return result;
//        }

//        #region Linhist removed 19/10/23
//        //#region Linhist added 10/03/22
//        //private static void BindTableToSheet(ISheet excelSheet, DataTable table, ICellStyle boldStyle, int rowIdx = 0)
//        //{
//        //    List<string> columns = new List<string>();
//        //    IRow row = excelSheet.CreateRow(rowIdx++);

//        //    int idx = 0;
//        //    foreach (DataColumn column in table.Columns)
//        //    {
//        //        columns.Add(column.ColumnName);
//        //        ICell cell = row.CreateCell(idx++);
//        //        cell.SetCellValue(column.ColumnName.Replace("_", " "));
//        //        cell.CellStyle = boldStyle;
//        //    }

//        //    foreach (DataRow dsrow in table.Rows)
//        //    {
//        //        row = excelSheet.CreateRow(rowIdx++);
//        //        int cellIndex = 0;
//        //        foreach (string col in columns)
//        //        {
//        //            row.CreateCell(cellIndex++).SetCellValue(dsrow[col].ToString());
//        //        }
//        //    }

//        //    for (int i = 0; i < idx; i++)
//        //    {
//        //        excelSheet.AutoSizeColumn(i);
//        //    }
//        //}

//        //private static void BindTableToSheet(ISheet excelSheet, DataTable table, ICellStyle boldStyle, string formatVlookupKhoa, int rowIdx = 0)
//        //{
//        //    List<string> columns = new List<string>();
//        //    IRow row = excelSheet.CreateRow(rowIdx++);

//        //    int idx = 0;
//        //    foreach (DataColumn column in table.Columns)
//        //    {
//        //        columns.Add(column.ColumnName);
//        //        ICell cell = row.CreateCell(idx++);
//        //        cell.SetCellValue(column.ColumnName.Replace("_", " "));
//        //        cell.CellStyle = boldStyle;
//        //    }

//        //    int idxKhoa = -1;
//        //    foreach (DataRow dsrow in table.Rows)
//        //    {
//        //        row = excelSheet.CreateRow(rowIdx++);
//        //        int cellIndex = 0;
//        //        foreach (string col in columns)
//        //        {
//        //            var cel = row.CreateCell(cellIndex++);

//        //            if (formatVlookupKhoa.Equals(dsrow[col]))
//        //            {
//        //                idxKhoa = cellIndex;
//        //                cel.SetCellFormula(string.Format(formatVlookupKhoa, "E", rowIdx));
//        //            }
//        //            else
//        //            {
//        //                cel.SetCellValue(dsrow[col].ToString());
//        //            }
//        //        }
//        //    }

//        //    for (int i = 0; i < idx; i++)
//        //    {
//        //        if (i == idxKhoa)
//        //        {
//        //            excelSheet.SetColumnWidth(i, _excelColMild);
//        //        }
//        //        else
//        //        {
//        //            excelSheet.AutoSizeColumn(i);
//        //        }
//        //    }
//        //}
//        //#endregion

//        //Linhist sua doan nay, them DMKhoa
//        //static public byte[] DownloadExcelReport6Sheets(IEnumerable<XML1> data1, IEnumerable<DMError> DMErrors)
//        //static public byte[] DownloadExcelReport6Sheets(IEnumerable<XML1> data1, IEnumerable<DMError> DMErrors, IEnumerable<DMKhoa> DMKhoas)
//        //{
//        //    //Linhist them format cho cellfunction
//        //    string formatVlookupKhoa = "VLOOKUP({0}{1}," + $"'DM Khoa'!A2:B{DMKhoas.Count() + 1},2,FALSE)";

//        //    byte[] result;
//        //    using (var fs = new MemoryStream())
//        //    {
//        //        string SoHoSo = data1.Count().ToString();

//        //        //Linhist sua doan nay them Ten_Khoa
//        //        //var newDta = from x in data1 select new { TênFile =x.FileName,Mã_Bệnh_Nhân=x.MA_BN,Mã_CSKCB=x.MA_CSKCB,Mã_Khoa = x.MA_KHOA, Tên_Bệnh_Nhân=x.HO_TEN,Địa_Chỉ=x.DIA_CHI,Ngày_Vào=x.NGAY_VAO,Ngày_Ra = x.NGAY_RA, Ngày_Lập= x.CreateDate };

//        //        var newDta = from x in data1
//        //                     select new
//        //                     {
//        //                         TênFile = x.FileName,
//        //                         Mã_Bệnh_Nhân = x.MA_BN,
//        //                         Mã_CSKCB = x.MA_CSKCB,
//        //                         Mã_Lượt_Khám = x.MA_LK,
//        //                         Mã_Khoa = x.MA_KHOA,
//        //                         Tên_Khoa = formatVlookupKhoa,
//        //                         Tên_Bệnh_Nhân = x.HO_TEN,
//        //                         Địa_Chỉ = x.DIA_CHI,
//        //                         Ngày_Vào = x.NGAY_VAO,
//        //                         Ngày_Ra = x.NGAY_RA,
//        //                         Ngày_Lập = x.CreateDate
//        //                     };
//        //        newDta.AsEnumerable();

//        //        DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(newDta), (typeof(DataTable)));
//        //        // xlsx: XSSFWorkbook  --- xls: HSSFWorkbook  
//        //        IWorkbook workbook = new XSSFWorkbook();
//        //        ISheet excelSheet = workbook.CreateSheet("DS File Báo cáo");

//        //        IFont boldFont = workbook.CreateFont();
//        //        boldFont.IsBold = true;
//        //        ICellStyle boldStyle = workbook.CreateCellStyle();
//        //        boldStyle.SetFont(boldFont);

//        //        boldStyle.BorderBottom = BorderStyle.Medium;
//        //        boldStyle.BorderRight = BorderStyle.Medium;
//        //        boldStyle.BorderTop = BorderStyle.Medium;
//        //        boldStyle.BorderLeft = BorderStyle.Medium;
//        //        boldStyle.FillForegroundColor = IndexedColors.Lime.Index;
//        //        boldStyle.FillPattern = FillPattern.SolidForeground;

//        //        List<string> columns = new List<string>();
//        //        IRow row = excelSheet.CreateRow(1);

//        //        ICellStyle style = workbook.CreateCellStyle();
//        //        IFont boldFontTitle = workbook.CreateFont();
//        //        boldFontTitle.IsBold = true;
//        //        boldFontTitle.FontHeightInPoints = 30;

//        //        var cra = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, 8);
//        //        excelSheet.AddMergedRegion(cra);

//        //        IRow rowHeader = excelSheet.CreateRow(0);
//        //        ICell cell1 = rowHeader.CreateCell(0);
//        //        cell1.SetCellValue("Báo Cáo Tổng Hợp " + SoHoSo + " Hồ Sơ - " + System.DateTime.Now.ToShortDateString());
//        //        style.SetFont(boldFontTitle);
//        //        cell1.CellStyle = style;

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet, table, boldStyle, formatVlookupKhoa, 1);

//        //        #region Linhist removed 10/03/22
//        //        //int columnIndex = 0;

//        //        //foreach (DataColumn column in table.Columns)
//        //        //{
//        //        //    columns.Add(column.ColumnName);
//        //        //    ICell cell = row.CreateCell(columnIndex);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex++;
//        //        //}

//        //        //int rowIndex = 2;
//        //        //foreach (DataRow dsrow in table.Rows)
//        //        //{
//        //        //    row = excelSheet.CreateRow(rowIndex);
//        //        //    int cellIndex = 0;
//        //        //    foreach (string col in columns)
//        //        //    {
//        //        //        row.CreateCell(cellIndex).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex++;
//        //        //    }

//        //        //    rowIndex++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex; i++)
//        //        //{
//        //        //    excelSheet.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        var data2 = data1.SelectMany(x => x.XML1Errors).GroupBy(x => x.XEErrorCode, (key, g) => new { XEErrorCode = key, Count = g.Count() }).AsEnumerable();

//        //        var dataJoin = data2.Join(DMErrors, x => x.XEErrorCode, y => y.ErrorCode, (x, y) => new  // result selector
//        //        {
//        //            Loại_File_Lỗi = y.ErrorAtXMLFile,
//        //            Mã_Lỗi = y.ErrorCode,
//        //            Lỗi_Tại_Thẻ = y.ErrorAtXMLTag,
//        //            Số_Lỗi = x.Count,
//        //            Chi_Tiết_Lỗi = y.ErrorContent
//        //        });
//        //        DataTable table2 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataJoin), (typeof(DataTable)));
//        //        ISheet excelSheet2 = workbook.CreateSheet("Tổng Hợp Số Lỗi");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet2, table2, boldStyle);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns2 = new List<string>();
//        //        //IRow row2 = excelSheet2.CreateRow(0);
//        //        //int columnIndex2 = 0;

//        //        //foreach (DataColumn column in table2.Columns)
//        //        //{
//        //        //    columns2.Add(column.ColumnName);
//        //        //    ICell cell = row2.CreateCell(columnIndex2);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex2++;
//        //        //}

//        //        //int rowIndex2 = 1;
//        //        //foreach (DataRow dsrow in table2.Rows)
//        //        //{
//        //        //    row2 = excelSheet2.CreateRow(rowIndex2);
//        //        //    int cellIndex2 = 0;
//        //        //    foreach (string col in columns2)
//        //        //    {
//        //        //        row2.CreateCell(cellIndex2).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex2++;
//        //        //    }

//        //        //    rowIndex2++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex2; i++)
//        //        //{
//        //        //    excelSheet2.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        //var data3 = data1.SelectMany(x => x.XML1Errors);

//        //        //var errorXmlAll = ;
//        //        var errorXmlAllJoinDM = data1.SelectMany(x => x.XML1Errors).
//        //            Join(DMErrors, x => x.XEErrorCode, y => y.ErrorCode, (x, y) => new { x, y }).
//        //            Join(data1, z => z.x.XEXML1ID, u => u.XML1ID, (z, u) => new { z, u });

//        //        //xml1 failse
//        //        var XML1Fails = data1.Join(errorXmlAllJoinDM.Where(o => o.z.y.ErrorAtXMLFile == "XML1").ToList(), z => z.XML1ID, u => u.z.x.XEXML1ID, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.STT == p.u.z.x.XEErrorRow).Select(f => new {
//        //            Chi_Tiết_Lỗi = f.u.z.y.ErrorContent,
//        //            Mã_Bệnh_Nhân = f.u.u.MA_BN,
//        //            Tên_Bệnh_Nhân = f.u.u.HO_TEN,
//        //            Mã_Lượt_Khám = f.u.u.MA_LK,
//        //            Mã_Khoa = f.u.u.MA_KHOA,
//        //            Tên_Khoa = formatVlookupKhoa,
//        //            Ngày_Vào = f.u.u.NGAY_VAO,
//        //            Ngày_Ra = f.u.u.NGAY_RA,
//        //            Ngày_Thanh_Toán = f.u.u.NGAY_TTOAN,
//        //            Chuẩn_Đoán_Chính = f.u.u.MA_BENH,
//        //            Chuẩn_Đoán_Kèm_Theo = f.u.u.MA_BENHKHAC,
//        //            Tên_Bệnh = f.u.u.TEN_BENH,
//        //            STT = f.z.STT,
//        //        }).ToList();

//        //        DataTable table1 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML1Fails), (typeof(DataTable)));
//        //        ISheet excelSheet1 = workbook.CreateSheet("Lỗi XML1");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet1, table1, boldStyle, formatVlookupKhoa);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns1= new List<string>();
//        //        //IRow row1 = excelSheet1.CreateRow(0);
//        //        //int columnIndex1 = 0;

//        //        //foreach (DataColumn column in table1.Columns)
//        //        //{
//        //        //    columns1.Add(column.ColumnName);
//        //        //    ICell cell = row1.CreateCell(columnIndex1);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex1++;
//        //        //}

//        //        //int rowIndex1 = 1;
//        //        //foreach (DataRow dsrow in table1.Rows)
//        //        //{
//        //        //    row1 = excelSheet1.CreateRow(rowIndex1);
//        //        //    int cellIndex1 = 0;
//        //        //    foreach (string col in columns1)
//        //        //    {
//        //        //        row1.CreateCell(cellIndex1).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex1++;
//        //        //    }

//        //        //    rowIndex1++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex1; i++)
//        //        //{
//        //        //    excelSheet1.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        var XML2Fails = data1.SelectMany(m => m.XML2s).Join(errorXmlAllJoinDM.Where(o => o.z.y.ErrorAtXMLFile == "XML2").ToList(), z => z.XML1ID, u => u.z.x.XEXML1ID, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.STT == p.u.z.x.XEErrorRow).Select(f => new {
//        //            Chi_Tiết_Lỗi = f.u.z.y.ErrorContent,
//        //            Mã_Bệnh_Nhân = f.u.u.MA_BN,
//        //            Tên_Bệnh_Nhân = f.u.u.HO_TEN,
//        //            Mã_Lượt_Khám = f.u.u.MA_LK,
//        //            Mã_Khoa = f.u.u.MA_KHOA,
//        //            Tên_Khoa = formatVlookupKhoa,
//        //            Ngày_Vào = f.u.u.NGAY_VAO,
//        //            Ngày_Ra = f.u.u.NGAY_RA,
//        //            Ngày_Thanh_Toán = f.u.u.NGAY_TTOAN,
//        //            Chuẩn_Đoán_Chính = f.u.u.MA_BENH,
//        //            Chuẩn_Đoán_Kèm_Theo = f.u.u.MA_BENHKHAC,
//        //            Tên_Bệnh = f.u.u.TEN_BENH,
//        //            Mã_Bác_Sĩ = f.z.MA_BAC_SI,
//        //            STT = f.z.STT,
//        //            Mã_Thuốc = f.z.MA_THUOC,
//        //            Mã_Nhóm = f.z.MA_NHOM,
//        //            Tên_Thuốc = f.z.TEN_THUOC,
//        //            ĐVT = f.z.DON_VI_TINH,
//        //            Số_Đăng_Ký = f.z.SO_DANG_KY,
//        //            TT_Thầu = f.z.TT_THAU,
//        //            Đơn_Giá = f.z.DON_GIA,
//        //            Tỷ_Lệ_TT = f.z.TYLE_TT,
//        //            Thành_Tiền = f.z.THANH_TIEN,
//        //            Ngày_Y_Lệnh = f.z.NGAY_YL

//        //        }).ToList();

//        //        //var XML2s = data1.SelectMany(m => m.XML2s).ToList();
//        //        //var khoi2 = from x in XML2s
//        //        //            join y in khoi
//        //        //            on new { X1 = x.XML1ID, X2 = x.STT } equals new { X1 = y.XML1ID, X2 = y.Dòng_Lỗi }
//        //        //            select new {x,y };
//        //        //on new { x.XML1ID, x.STT } equals new { y.XML1ID, y.Dòng_Lỗi };




//        //        DataTable table3 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML2Fails), (typeof(DataTable)));
//        //        ISheet excelSheet3 = workbook.CreateSheet("Lỗi XML2");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet3, table3, boldStyle, formatVlookupKhoa);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns3 = new List<string>();
//        //        //IRow row3 = excelSheet3.CreateRow(0);
//        //        //int columnIndex3 = 0;

//        //        //foreach (DataColumn column in table3.Columns)
//        //        //{
//        //        //    columns3.Add(column.ColumnName);
//        //        //    ICell cell = row3.CreateCell(columnIndex3);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex3++;
//        //        //}

//        //        //int rowIndex3 = 1;
//        //        //foreach (DataRow dsrow in table3.Rows)
//        //        //{
//        //        //    row3 = excelSheet3.CreateRow(rowIndex3);
//        //        //    int cellIndex3 = 0;
//        //        //    foreach (string col in columns3)
//        //        //    {
//        //        //        row3.CreateCell(cellIndex3).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex3++;
//        //        //    }

//        //        //    rowIndex3++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex3; i++)
//        //        //{
//        //        //    excelSheet3.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        var XML3Fails = data1.SelectMany(m => m.XML3s).Join(errorXmlAllJoinDM.Where(o => o.z.y.ErrorAtXMLFile == "XML3").ToList(), z => z.XML1ID, u => u.z.x.XEXML1ID, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.STT == p.u.z.x.XEErrorRow).Select(f => new {
//        //            Chi_Tiết_Lỗi = f.u.z.y.ErrorContent,
//        //            Mã_Bệnh_Nhân = f.u.u.MA_BN,
//        //            Tên_Bệnh_Nhân = f.u.u.HO_TEN,
//        //            Mã_Lượt_Khám = f.u.u.MA_LK,
//        //            Mã_Khoa = f.u.u.MA_KHOA,
//        //            Tên_Khoa = formatVlookupKhoa,
//        //            Mã_Dịch_Vụ = f.z.MA_DICH_VU,
//        //            Tên_Dịch_Vụ = f.z.TEN_DICH_VU,
//        //            Mã_Vật_Tư = f.z.MA_VAT_TU,
//        //            Tên_Vật_Tư = f.z.TEN_VAT_TU,

//        //            Ngày_Vào = f.u.u.NGAY_VAO,
//        //            Ngày_Ra = f.u.u.NGAY_RA,
//        //            Ngày_Thanh_Toán = f.u.u.NGAY_TTOAN,
//        //            Chuẩn_Đoán_Chính = f.u.u.MA_BENH,
//        //            Chuẩn_Đoán_Kèm_Theo = f.u.u.MA_BENHKHAC,
//        //            Tên_Bệnh = f.u.u.TEN_BENH,
//        //            Mã_Bác_Sĩ = f.z.MA_BAC_SI,
//        //            STT = f.z.STT,

//        //            ĐVT = f.z.DON_VI_TINH,

//        //            TT_Thầu = f.z.TT_THAU,
//        //            Đơn_Giá = f.z.DON_GIA,
//        //            Tỷ_Lệ_TT = f.z.TYLE_TT,
//        //            Thành_Tiền = f.z.THANH_TIEN,
//        //            Ngày_Y_Lệnh = f.z.NGAY_YL,
//        //            Ngày_Kết_quả = f.z.NGAY_KQ

//        //        }).ToList();
//        //        DataTable table4 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML3Fails), (typeof(DataTable)));
//        //        ISheet excelSheet4 = workbook.CreateSheet("Lỗi XML3");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet4, table4, boldStyle, formatVlookupKhoa);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns4 = new List<string>();
//        //        //IRow row4 = excelSheet4.CreateRow(0);
//        //        //int columnIndex4 = 0;

//        //        //foreach (DataColumn column in table4.Columns)
//        //        //{
//        //        //    columns4.Add(column.ColumnName);
//        //        //    ICell cell = row4.CreateCell(columnIndex4);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex4++;
//        //        //}

//        //        //int rowIndex4 = 1;
//        //        //foreach (DataRow dsrow in table4.Rows)
//        //        //{
//        //        //    row4 = excelSheet4.CreateRow(rowIndex4);
//        //        //    int cellIndex4 = 0;
//        //        //    foreach (string col in columns4)
//        //        //    {
//        //        //        row4.CreateCell(cellIndex4).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex4++;
//        //        //    }

//        //        //    rowIndex4++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex4; i++)
//        //        //{
//        //        //    excelSheet4.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        var XML4Fails = data1.SelectMany(m => m.XML4s).Join(errorXmlAllJoinDM.Where(o => o.z.y.ErrorAtXMLFile == "XML4").ToList(), z => z.XML1ID, u => u.z.x.XEXML1ID, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.STT == p.u.z.x.XEErrorRow).Select(f => new {
//        //            Chi_Tiết_Lỗi = f.u.z.y.ErrorContent,
//        //            Mã_Bệnh_Nhân = f.u.u.MA_BN,
//        //            Tên_Bệnh_Nhân = f.u.u.HO_TEN,
//        //            Mã_Lượt_Khám = f.u.u.MA_LK,
//        //            Mã_Khoa = f.u.u.MA_KHOA,
//        //            Tên_Khoa = formatVlookupKhoa,
//        //            Ngày_Vào = f.u.u.NGAY_VAO,
//        //            Ngày_Ra = f.u.u.NGAY_RA,
//        //            Ngày_Thanh_Toán = f.u.u.NGAY_TTOAN,
//        //            Chuẩn_Đoán_Chính = f.u.u.MA_BENH,
//        //            Chuẩn_Đoán_Kèm_Theo = f.u.u.MA_BENHKHAC,
//        //            Tên_Bệnh = f.u.u.TEN_BENH,
//        //            STT = f.z.STT,
//        //        }).ToList();

//        //        DataTable table5 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML4Fails), (typeof(DataTable)));
//        //        ISheet excelSheet5 = workbook.CreateSheet("Lỗi XML4");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet5, table5, boldStyle, formatVlookupKhoa);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns5 = new List<string>();
//        //        //IRow row5 = excelSheet5.CreateRow(0);
//        //        //int columnIndex5 = 0;

//        //        //foreach (DataColumn column in table5.Columns)
//        //        //{
//        //        //    columns5.Add(column.ColumnName);
//        //        //    ICell cell = row5.CreateCell(columnIndex5);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex5++;
//        //        //}

//        //        //int rowIndex5 = 1;
//        //        //foreach (DataRow dsrow in table5.Rows)
//        //        //{
//        //        //    row5 = excelSheet5.CreateRow(rowIndex5);
//        //        //    int cellIndex5 = 0;
//        //        //    foreach (string col in columns5)
//        //        //    {
//        //        //        row5.CreateCell(cellIndex5).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex5++;
//        //        //    }

//        //        //    rowIndex5++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex5; i++)
//        //        //{
//        //        //    excelSheet5.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        var XML5Fails = data1.SelectMany(m => m.XML5s).Join(errorXmlAllJoinDM.Where(o => o.z.y.ErrorAtXMLFile == "XML5").ToList(), z => z.XML1ID, u => u.z.x.XEXML1ID, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.STT == p.u.z.x.XEErrorRow).Select(f => new {
//        //            Chi_Tiết_Lỗi = f.u.z.y.ErrorContent,
//        //            Mã_Bệnh_Nhân = f.u.u.MA_BN,
//        //            Tên_Bệnh_Nhân = f.u.u.HO_TEN,
//        //            Mã_Lượt_Khám = f.u.u.MA_LK,
//        //            Mã_Khoa = f.u.u.MA_KHOA,
//        //            Tên_Khoa = formatVlookupKhoa,
//        //            Ngày_Vào = f.u.u.NGAY_VAO,
//        //            Ngày_Ra = f.u.u.NGAY_RA,
//        //            Ngày_Thanh_Toán = f.u.u.NGAY_TTOAN,
//        //            Chuẩn_Đoán_Chính = f.u.u.MA_BENH,
//        //            Chuẩn_Đoán_Kèm_Theo = f.u.u.MA_BENHKHAC,
//        //            Tên_Bệnh = f.u.u.TEN_BENH,
//        //            STT = f.z.STT,
//        //        }).ToList();

//        //        DataTable table6 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML5Fails), (typeof(DataTable)));
//        //        ISheet excelSheet6 = workbook.CreateSheet("Lỗi XML5");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet6, table6, boldStyle, formatVlookupKhoa);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns6 = new List<string>();
//        //        //IRow row6 = excelSheet6.CreateRow(0);
//        //        //int columnIndex6 = 0;

//        //        //foreach (DataColumn column in table6.Columns)
//        //        //{
//        //        //    columns6.Add(column.ColumnName);
//        //        //    ICell cell = row6.CreateCell(columnIndex6);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex6++;
//        //        //}

//        //        //int rowIndex6 = 1;
//        //        //foreach (DataRow dsrow in table6.Rows)
//        //        //{
//        //        //    row6 = excelSheet6.CreateRow(rowIndex6);
//        //        //    int cellIndex6 = 0;
//        //        //    foreach (string col in columns6)
//        //        //    {
//        //        //        row6.CreateCell(cellIndex6).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex6++;
//        //        //    }

//        //        //    rowIndex6++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex6; i++)
//        //        //{
//        //        //    excelSheet6.AutoSizeColumn(i);
//        //        //}
//        //        #endregion


//        //        var XML3FailsNhom8 = data1.SelectMany(m => m.XML3s).Join(data1.SelectMany(m => m.XML3s).ToList(), z => z.MA_BAC_SI, u => u.MA_BAC_SI, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.MA_NHOM == 8
//        //        && p.u.MA_NHOM == 8
//        //        && p.z.MA_DICH_VU != p.u.MA_DICH_VU
//        //        && p.z.MA_LK != p.u.MA_LK
//        //        && p.z.NGAY_KQ != null
//        //        && p.z.NGAY_YL <= p.u.NGAY_YL
//        //        && p.z.NGAY_KQ >= p.u.NGAY_YL
//        //        ).Select(f => new {
//        //            MA_BS_1 = f.z.MA_BAC_SI,
//        //            XML1ID_1 = f.z.XML1ID,
//        //            Ma_LK_1 = f.z.MA_LK,
//        //            Ten_Dich_Vu_1 = f.z.TEN_DICH_VU,
//        //            Ngay_YL_1 = f.z.NGAY_YL,
//        //            Ngay_KQ_1 = f.z.NGAY_KQ,

//        //            MA_BS_2 = f.u.MA_BAC_SI,
//        //            XML1ID_2 = f.u.XML1ID,
//        //            Ma_LK_2 = f.u.MA_LK,
//        //            Ten_Dich_Vu_2 = f.u.TEN_DICH_VU,
//        //            Ngay_YL_2 = f.u.NGAY_YL,
//        //            Ngay_KQ_2 = f.u.NGAY_KQ,


//        //        }).Join(data1.ToList(), z1 => z1.XML1ID_1, u1 => u1.XML1ID, (z1, u1) => new
//        //        {
//        //            z1,
//        //            u1
//        //        }).Join(data1.ToList(), z2 => z2.z1.XML1ID_2, u2 => u2.XML1ID, (z2, u2) => new
//        //        {
//        //            Mã_BS_1 = z2.z1.MA_BS_1,
//        //            Mã_Liên_Kết_1 = z2.z1.Ma_LK_1,
//        //            Tên_Dịch_Vụ_1 = z2.z1.Ten_Dich_Vu_1,
//        //            Ngày_Kết_Quả_1 = z2.z1.Ngay_KQ_1,
//        //            Ngày_Y_Lệnh_1 = z2.z1.Ngay_YL_1,
//        //            Họ_Tên_1 = z2.u1.HO_TEN,
//        //            Mã_Thẻ_1 = z2.u1.MA_THE,



//        //            Mã_BS_2 = z2.z1.MA_BS_2,
//        //            Mã_Liên_Kết_2 = z2.z1.Ma_LK_2,
//        //            Tên_Dịch_Vụ_2 = z2.z1.Ten_Dich_Vu_2,
//        //            Ngày_Kết_Quả_2 = z2.z1.Ngay_KQ_2,
//        //            Ngày_Y_Lệnh_2 = z2.z1.Ngay_YL_2,
//        //            Họ_Tên_2 = u2.HO_TEN,
//        //            Mã_Thẻ_2 = u2.MA_THE,





//        //        }).ToList();


//        //        DataTable table7 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML3FailsNhom8), (typeof(DataTable)));
//        //        ISheet excelSheet7 = workbook.CreateSheet("Lỗi BS Dịch Vụ Trùng TG-Nhom 8");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet7, table7, boldStyle);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns7 = new List<string>();
//        //        //IRow row7 = excelSheet7.CreateRow(0);
//        //        //int columnIndex7 = 0;

//        //        //foreach (DataColumn column in table7.Columns)
//        //        //{
//        //        //    columns7.Add(column.ColumnName);
//        //        //    ICell cell = row7.CreateCell(columnIndex7);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex7++;
//        //        //}

//        //        //int rowIndex7 = 1;
//        //        //foreach (DataRow dsrow in table7.Rows)
//        //        //{
//        //        //    row7 = excelSheet7.CreateRow(rowIndex7);
//        //        //    int cellIndex7 = 0;
//        //        //    foreach (string col in columns7)
//        //        //    {
//        //        //        row7.CreateCell(cellIndex7).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex7++;
//        //        //    }

//        //        //    rowIndex7++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex7; i++)
//        //        //{
//        //        //    excelSheet7.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        var XML3FailsNhom2 = data1.SelectMany(m => m.XML3s).Join(data1.SelectMany(m => m.XML3s).ToList(), z => z.MA_BAC_SI, u => u.MA_BAC_SI, (z, u) => new
//        //        {
//        //            z,
//        //            u
//        //        }).Where(p => p.z.MA_NHOM == 2
//        //        && p.u.MA_NHOM == 2
//        //        && p.z.MA_DICH_VU != p.u.MA_DICH_VU
//        //        && p.z.MA_LK != p.u.MA_LK
//        //        && p.z.NGAY_KQ != null
//        //        && p.z.NGAY_YL <= p.u.NGAY_YL
//        //        && p.z.NGAY_KQ >= p.u.NGAY_YL
//        //        ).Select(f => new {
//        //            MA_BS_1 = f.z.MA_BAC_SI,
//        //            XML1ID_1 = f.z.XML1ID,
//        //            Ma_LK_1 = f.z.MA_LK,
//        //            Ten_Dich_Vu_1 = f.z.TEN_DICH_VU,
//        //            Ngay_YL_1 = f.z.NGAY_YL,
//        //            Ngay_KQ_1 = f.z.NGAY_KQ,

//        //            MA_BS_2 = f.u.MA_BAC_SI,
//        //            XML1ID_2 = f.u.XML1ID,
//        //            Ma_LK_2 = f.u.MA_LK,
//        //            Ten_Dich_Vu_2 = f.u.TEN_DICH_VU,
//        //            Ngay_YL_2 = f.u.NGAY_YL,
//        //            Ngay_KQ_2 = f.u.NGAY_KQ,


//        //        }).Join(data1.ToList(), z1 => z1.XML1ID_1, u1 => u1.XML1ID, (z1, u1) => new
//        //        {
//        //            z1,
//        //            u1
//        //        }).Join(data1.ToList(), z2 => z2.z1.XML1ID_2, u2 => u2.XML1ID, (z2, u2) => new
//        //        {
//        //            Mã_BS_1 = z2.z1.MA_BS_1,
//        //            Mã_Liên_Kết_1 = z2.z1.Ma_LK_1,
//        //            Tên_Dịch_Vụ_1 = z2.z1.Ten_Dich_Vu_1,
//        //            Ngày_Kết_Quả_1 = z2.z1.Ngay_KQ_1,
//        //            Ngày_Y_Lệnh_1 = z2.z1.Ngay_YL_1,
//        //            Họ_Tên_1 = z2.u1.HO_TEN,
//        //            Mã_Thẻ_1 = z2.u1.MA_THE,



//        //            Mã_BS_2 = z2.z1.MA_BS_2,
//        //            Mã_Liên_Kết_2 = z2.z1.Ma_LK_2,
//        //            Tên_Dịch_Vụ_2 = z2.z1.Ten_Dich_Vu_2,
//        //            Ngày_Kết_Quả_2 = z2.z1.Ngay_KQ_2,
//        //            Ngày_Y_Lệnh_2 = z2.z1.Ngay_YL_2,
//        //            Họ_Tên_2 = u2.HO_TEN,
//        //            Mã_Thẻ_2 = u2.MA_THE,





//        //        }).ToList();


//        //        DataTable table8 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(XML3FailsNhom2), (typeof(DataTable)));
//        //        ISheet excelSheet8 = workbook.CreateSheet("Lỗi BS Dịch Vụ Trùng TG-Nhom 2");

//        //        //Linhist bo doan ben duoi thay bang function BindTableToSheet
//        //        BindTableToSheet(excelSheet8, table8, boldStyle);

//        //        #region Linhist removed 10/03/22
//        //        //List<string> columns8 = new List<string>();
//        //        //IRow row8 = excelSheet8.CreateRow(0);
//        //        //int columnIndex8 = 0;

//        //        //foreach (DataColumn column in table8.Columns)
//        //        //{
//        //        //    columns8.Add(column.ColumnName);
//        //        //    ICell cell = row8.CreateCell(columnIndex8);
//        //        //    cell.SetCellValue(column.ColumnName);
//        //        //    cell.CellStyle = boldStyle;
//        //        //    columnIndex8++;
//        //        //}

//        //        //int rowIndex8 = 1;
//        //        //foreach (DataRow dsrow in table8.Rows)
//        //        //{
//        //        //    row8 = excelSheet8.CreateRow(rowIndex8);
//        //        //    int cellIndex8 = 0;
//        //        //    foreach (string col in columns8)
//        //        //    {
//        //        //        row8.CreateCell(cellIndex8).SetCellValue(dsrow[col].ToString());
//        //        //        cellIndex8++;
//        //        //    }

//        //        //    rowIndex8++;
//        //        //}
//        //        //for (int i = 0; i < columnIndex8; i++)
//        //        //{
//        //        //    excelSheet8.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        #region Linhist added 10/03/22
//        //        //Linhist them DM Khoa
//        //        var khoas = DMKhoas.Select(k => new { Mã_Khoa = k.KhoaCode, Tên_Khoa = k.KhoaName }).ToList();
//        //        DataTable table9 = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(khoas), typeof(DataTable));
//        //        ISheet excelSheet9 = workbook.CreateSheet("DM Khoa");
//        //        BindTableToSheet(excelSheet9, table9, boldStyle);
//        //        #endregion

//        //        workbook.Write(fs);
//        //        result = fs.ToArray();
//        //    }

//        //    return result;
//        //}
//        #endregion

//        #region Linhist added 11/03/22
//        private const int _excelColWide = 256 * 72;
//        private const int _excelColMild = 256 * 28;
//        //private static ICellStyle _captCellStyle = null;
//        //private static ICellStyle _headCellStyle = null;

//        #region Linhist removed 28/04/22
//        //private static void AddCaptionRow(ISheet xlSheet, int rowIdx = 0, string content = "", int colMerge = 0)
//        //{
//        //    if (colMerge > 0)
//        //    {
//        //        var cellRanged = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, colMerge);
//        //        xlSheet.AddMergedRegion(cellRanged);
//        //    }

//        //    IRow row = xlSheet.CreateRow(rowIdx);
//        //    ICell cell = row.CreateCell(rowIdx);

//        //    cell.SetCellValue(content);
//        //    cell.CellStyle = _captCellStyle;
//        //}

//        //private static void AddHeaderRow(ISheet xlSheet, int rowIdx = 0, params string[] columns)
//        //{
//        //    if (columns == null || columns.Length == 0) return;

//        //    IRow row = xlSheet.CreateRow(rowIdx);

//        //    for (int i = 0; i < columns.Length; i++)
//        //    {
//        //        ICell cell = row.CreateCell(i);
//        //        cell.SetCellValue(columns[i]);
//        //        cell.CellStyle = _headCellStyle;
//        //    }
//        //}
//        #endregion

//        #region Linhist removed 19/10/23
//        //private static void AddRow(ISheet xlSheet, int rowIdx = 0, params object[] values)
//        //{
//        //    if (values == null || values.Length == 0) return;

//        //    string val = "";
//        //    IRow row = xlSheet.CreateRow(rowIdx);
//        //    for (int i = 0; i < values.Length; i++)
//        //    {
//        //        ICell cell = row.CreateCell(i);

//        //        val = $"{values[i]}";
//        //        if (val.StartsWith("$="))
//        //        {
//        //            cell.SetCellFormula(val[2..]);
//        //        }
//        //        else
//        //        {
//        //            if (val.EndsWith("SA") && val.Contains(" 12:"))
//        //            {
//        //                val = val.Replace(" 12:", " 0:");
//        //            }

//        //            cell.SetCellValue(val);
//        //        }
//        //    }
//        //}

//        //static public byte[] DownloadExcelReport9Sheets(IEnumerable<XML1> data1, IEnumerable<DMError> DMErrors, IEnumerable<DMKhoa> DMKhoas, IEnumerable<XML1> XML1HaveXML4Error = null)
//        //{
//        //    //Create new excel
//        //    IWorkbook workbook = new XSSFWorkbook();

//        //    //Create some predefined style for cell
//        //    IFont boldFontTitle = workbook.CreateFont();
//        //    boldFontTitle.IsBold = true;
//        //    boldFontTitle.FontHeightInPoints = 30;
//        //    IFont boldFont = workbook.CreateFont();
//        //    boldFont.IsBold = true;

//        //    _captCellStyle = workbook.CreateCellStyle();
//        //    _captCellStyle.SetFont(boldFontTitle);

//        //    _headCellStyle = workbook.CreateCellStyle();
//        //    _headCellStyle.SetFont(boldFont);
//        //    _headCellStyle.BorderBottom = BorderStyle.Medium;
//        //    _headCellStyle.BorderRight = BorderStyle.Medium;
//        //    _headCellStyle.BorderTop = BorderStyle.Medium;
//        //    _headCellStyle.BorderLeft = BorderStyle.Medium;
//        //    _headCellStyle.FillForegroundColor = IndexedColors.Lime.Index;
//        //    _headCellStyle.FillPattern = FillPattern.SolidForeground;

//        //    //format cho cell function
//        //    string reportCaption = $"Báo Cáo Tổng Hợp {data1.Count()} Hồ Sơ - {System.DateTime.Now.ToShortDateString()}";
//        //    string formularKhoa = $"$=IFERROR(VLOOKUP({{0}}{{1}},'DM Khoa'!A2:B{(DMKhoas.Count() + 1)},2,FALSE),\"Chưa cập nhật\")";
//        //    string formularError = "$=IFERROR(VLOOKUP({0}{1},'Tổng Hợp Số Lỗi'!B2:D###,3,FALSE),\"\")";

//        //    byte[] result = null;
//        //    using (var fs = new MemoryStream())
//        //    {
//        //        #region Sheet 1: DS File Bao cao
//        //        int rowIdxF = 0;
//        //        ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");
//        //        AddCaptionRow(xlSheetF, rowIdxF++, reportCaption, 10);
//        //        AddHeaderRow(xlSheetF, rowIdxF++, "Tên File", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã MA_CSKCB", "Mã lượt khám",
//        //                    "Mã Khoa", "Tên Khoa", "Địa chỉ", "Ngày vào", "Ngày ra", "Ngày lập");

//        //        foreach (var x in data1)
//        //        {
//        //            AddRow(xlSheetF, rowIdxF++, x.FileName, x.MA_BN, x.HO_TEN, x.MA_CSKCB, x.MA_LK, x.MA_KHOA,
//        //                    string.Format(formularKhoa, "F", rowIdxF), x.DIA_CHI, x.NGAY_VAO, x.NGAY_RA, x.CreateDate);
//        //        }

//        //        for (int i = 0; i < 11; i++)
//        //        {
//        //            if (i == 6)
//        //            {
//        //                xlSheetF.SetColumnWidth(i, _excelColMild);
//        //            }
//        //            else
//        //            {
//        //                xlSheetF.AutoSizeColumn(i);
//        //            }
//        //        }
//        //        #endregion


//        //        var allErrors = data1.SelectMany(x => x.XML1Errors);

//        //        #region Sheet 2: Tong hop Loi
//        //        var grpErrors =
//        //            allErrors.GroupBy(x => x.XEErrorCode, (k, g) => new { ErrorCode = k, Count = g.Count() })
//        //            .Join(DMErrors, x => x.ErrorCode, y => y.ErrorCode, (x, y) => new
//        //            {
//        //                y.ErrorAtXMLFile,
//        //                y.ErrorCode,
//        //                y.ErrorAtXMLTag,
//        //                x.Count,
//        //                y.ErrorContent
//        //            });


//        //        int rowIdxE = 0;
//        //        ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Số Lỗi");
//        //        AddHeaderRow(xlSheetE, rowIdxE++, "Loại File lỗi", "Mã lỗi", "Lỗi tại Thẻ", "Chi tiết lỗi", "Số lỗi");

//        //        foreach (var x2 in grpErrors)
//        //        {
//        //            AddRow(xlSheetE, rowIdxE++, x2.ErrorAtXMLFile, x2.ErrorCode, x2.ErrorAtXMLTag, x2.ErrorContent, x2.Count);
//        //        }

//        //        for (int i = 0; i < 5; i++)
//        //        {
//        //            if (i == 3)
//        //            {
//        //                xlSheetE.SetColumnWidth(i, _excelColWide);
//        //            }
//        //            else
//        //            {
//        //                xlSheetE.AutoSizeColumn(i);
//        //            }
//        //        }
//        //        formularError = formularError.Replace("###", rowIdxE.ToString());
//        //        #endregion


//        //        #region Sheet 3: Loi XML1
//        //        List<int?> xml1ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML1").Select(e => e.ErrorCode).ToList();
//        //        var xml1Errors = allErrors.Where(a => xml1ErCodes.Contains(a.XEErrorCode));

//        //        int rowIdx1 = 0;
//        //        ISheet xlSheet1 = workbook.CreateSheet("Lỗi XML1");
//        //        AddHeaderRow(xlSheet1, rowIdx1++, "Mã lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã lượt khám",
//        //                    "Mã Khoa", "Tên Khoa", "Ngày vào", "Ngày ra", "Ngày Thanh toán", "Chuẩn đoán chính",
//        //                    "Chuẩn đoán kèm theo", "Tên Bệnh", "STT");

//        //        foreach (var x in xml1Errors)
//        //        {
//        //            AddRow(xlSheet1, rowIdx1++, x.XEErrorCode, string.Format(formularError, "A", rowIdx1), x.XEXML1.MA_BN,
//        //                    x.XEXML1.HO_TEN, x.XEXML1.MA_LK, x.XEXML1.MA_KHOA, string.Format(formularKhoa, "F", rowIdx1),
//        //                    x.XEXML1.NGAY_VAO, x.XEXML1.NGAY_RA, x.XEXML1.NGAY_TTOAN, x.XEXML1.MA_BENH, x.XEXML1.MA_BENHKHAC,
//        //                    x.XEXML1.TEN_BENH, x.XEXML1.STT);
//        //        }

//        //        for (int i = 0; i < 14; i++)
//        //        {
//        //            if (i == 1)
//        //            {
//        //                xlSheet1.SetColumnWidth(i, _excelColWide);
//        //            }
//        //            else if (i == 6)
//        //            {
//        //                xlSheet1.SetColumnWidth(i, _excelColMild);
//        //            }
//        //            else
//        //            {
//        //                xlSheet1.AutoSizeColumn(i);
//        //            }
//        //        }
//        //        #endregion


//        //        #region Sheet 4: Loi XML2
//        //        List<int?> xml2ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML2").Select(e => e.ErrorCode).ToList();
//        //        var xml2Errors = allErrors.Where(a => xml2ErCodes.Contains(a.XEErrorCode));

//        //        var data2 =
//        //            data1.SelectMany(x => x.XML2s)
//        //            .Join(xml2Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml2 = x, XmlE = e })
//        //            .Where(x => x.Xml2.STT == x.XmlE.XEErrorRow);

//        //        int rowIdx2 = 0;
//        //        ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML2");
//        //        AddHeaderRow(xlSheet2, rowIdx2++, "Mã lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã lượt khám", "Mã Khoa",
//        //                    "Tên Khoa", "Ngày vào", "Ngày ra", "Ngày Thanh toán", "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                    "STT", "Mã Bác sĩ", "Khoa y lệnh", "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký", "TT Thầu", "Đơn giá",
//        //                    "Số lượng", "Tỷ lệ TT", "Thành tiền", "Ngày Y lệnh");

//        //        foreach (var x in data2)
//        //        {
//        //            AddRow(xlSheet2, rowIdx2++, x.XmlE.XEErrorCode, string.Format(formularError, "A", rowIdx2), x.Xml2.XML1.MA_BN,
//        //                    x.Xml2.XML1.HO_TEN, x.Xml2.XML1.MA_LK, x.Xml2.XML1.MA_KHOA, string.Format(formularKhoa, "F", rowIdx2),
//        //                    x.Xml2.XML1.NGAY_VAO, x.Xml2.XML1.NGAY_RA, x.Xml2.XML1.NGAY_TTOAN, x.Xml2.XML1.MA_BENH,
//        //                    x.Xml2.XML1.MA_BENHKHAC, x.Xml2.XML1.TEN_BENH, x.Xml2.STT, x.Xml2.MA_BAC_SI, x.Xml2.MA_KHOA, x.Xml2.MA_THUOC,
//        //                    x.Xml2.MA_NHOM, x.Xml2.TEN_THUOC, x.Xml2.DON_VI_TINH, x.Xml2.LIEU_DUNG, x.Xml2.SO_DANG_KY, x.Xml2.TT_THAU,
//        //                    x.Xml2.DON_GIA, x.Xml2.SO_LUONG, x.Xml2.TYLE_TT, x.Xml2.THANH_TIEN, x.Xml2.NGAY_YL);
//        //        }

//        //        for (int i = 0; i < 25; i++)
//        //        {
//        //            if (i == 1)
//        //            {
//        //                xlSheet2.SetColumnWidth(i, _excelColWide);
//        //            }
//        //            else if (i == 6)
//        //            {
//        //                xlSheet2.SetColumnWidth(i, _excelColMild);
//        //            }
//        //            else
//        //            {
//        //                xlSheet2.AutoSizeColumn(i);
//        //            }
//        //        }
//        //        #endregion


//        //        #region Sheet 5: Loi XML3
//        //        var data3 = data1.SelectMany(x => x.XML3s);
//        //        if (data3.Any())
//        //        {
//        //            List<int?> xml3ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML3").Select(e => e.ErrorCode).ToList();
//        //            var xml3Errors = allErrors.Where(a => xml3ErCodes.Contains(a.XEErrorCode));

//        //            var data3Join =
//        //                data3.Join(xml3Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml3 = x, XmlE = e })
//        //                .Where(x => x.Xml3.STT == x.XmlE.XEErrorRow);

//        //            int rowIdx3 = 0;
//        //            ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML3");
//        //            AddHeaderRow(xlSheet3, rowIdx3++, "Mã lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã lượt khám", "Mã Khoa",
//        //                        "Tên Khoa", "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh", "STT", "Mã Bác sĩ", "Khoa y lệnh", "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT",
//        //                        "Thành tiền", "Ngày Y lệnh", "Ngày Kết quả");

//        //            foreach (var x in data3Join)
//        //            {
//        //                AddRow(xlSheet3, rowIdx3++, x.XmlE.XEErrorCode, string.Format(formularError, "A", rowIdx3), x.Xml3.XML1.MA_BN,
//        //                        x.Xml3.XML1.HO_TEN, x.Xml3.XML1.MA_LK, x.Xml3.XML1.MA_KHOA, string.Format(formularKhoa, "F", rowIdx3),
//        //                        x.Xml3.MA_DICH_VU, x.Xml3.TEN_DICH_VU, x.Xml3.MA_VAT_TU, x.Xml3.TEN_VAT_TU, x.Xml3.XML1.NGAY_VAO,
//        //                        x.Xml3.XML1.NGAY_RA, x.Xml3.XML1.NGAY_TTOAN, x.Xml3.XML1.MA_BENH, x.Xml3.XML1.MA_BENHKHAC, x.Xml3.XML1.TEN_BENH,
//        //                        x.Xml3.STT, x.Xml3.MA_BAC_SI, x.Xml3.MA_KHOA, x.Xml3.DON_VI_TINH, x.Xml3.TT_THAU, x.Xml3.DON_GIA, x.Xml3.TYLE_TT,
//        //                        x.Xml3.THANH_TIEN, x.Xml3.NGAY_YL, x.Xml3.NGAY_KQ);
//        //            }

//        //            for (int i = 0; i < 26; i++)
//        //            {
//        //                if (i == 1)
//        //                {
//        //                    xlSheet3.SetColumnWidth(i, _excelColWide);
//        //                }
//        //                else if (i == 6)
//        //                {
//        //                    xlSheet3.SetColumnWidth(i, _excelColMild);
//        //                }
//        //                else
//        //                {
//        //                    xlSheet3.AutoSizeColumn(i);
//        //                }
//        //            }
//        //        }
//        //        #endregion


//        //        #region Sheet 6: Loi XML4
//        //        var data4 = data1.SelectMany(x => x.XML4s);
//        //        if (data4.Any())
//        //        {
//        //            List<int?> xml4ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//        //            var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.XEErrorCode));




//        //            var data4Join =
//        //                data4.Join(xml4Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml4 = x, XmlE = e })
//        //                .Where(x => x.Xml4.STT == x.XmlE.XEErrorRow);

//        //            int rowIdx4 = 0;
//        //            ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML4");
//        //            AddHeaderRow(xlSheet4, rowIdx4++, "Mã lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã lượt khám", "Mã Khoa",
//        //                        "Tên Khoa", "Ngày vào", "Ngày ra", "Ngày Thanh toán", "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "STT");

//        //            foreach (var x in data4Join)
//        //            {
//        //                AddRow(xlSheet4, rowIdx4++, x.XmlE.XEErrorCode, string.Format(formularError, "A", rowIdx4), x.Xml4.XML1.MA_BN,
//        //                        x.Xml4.XML1.HO_TEN, x.Xml4.XML1.MA_LK, x.Xml4.XML1.MA_KHOA, string.Format(formularKhoa, "F", rowIdx4),
//        //                        x.Xml4.XML1.NGAY_VAO, x.Xml4.XML1.NGAY_RA, x.Xml4.XML1.NGAY_TTOAN, x.Xml4.XML1.MA_BENH,
//        //                        x.Xml4.XML1.MA_BENHKHAC, x.Xml4.XML1.TEN_BENH, x.Xml4.STT);
//        //            }

//        //            for (int i = 0; i < 14; i++)
//        //            {
//        //                if (i == 1)
//        //                {
//        //                    xlSheet4.SetColumnWidth(i, _excelColWide);
//        //                }
//        //                else if (i == 6)
//        //                {
//        //                    xlSheet4.SetColumnWidth(i, _excelColMild);
//        //                }
//        //                else
//        //                {
//        //                    xlSheet4.AutoSizeColumn(i);
//        //                }
//        //            }
//        //        }
//        //        #endregion


//        //        #region Sheet 7: Loi XML5
//        //        var data5 = data1.SelectMany(x => x.XML5s);
//        //        if (data5.Any())
//        //        {
//        //            List<int?> xml5ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//        //            var xml4Errors = allErrors.Where(a => xml5ErCodes.Contains(a.XEErrorCode));

//        //            var data5Join =
//        //                data5.Join(xml4Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml5 = x, XmlE = e })
//        //                .Where(x => x.Xml5.STT == x.XmlE.XEErrorRow);

//        //            int rowIdx5 = 0;
//        //            ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML5");
//        //            AddHeaderRow(xlSheet5, rowIdx5++, "Mã lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã lượt khám", "Mã Khoa",
//        //                        "Tên Khoa", "Ngày vào", "Ngày ra", "Ngày Thanh toán", "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "STT");

//        //            foreach (var x in data5Join)
//        //            {
//        //                AddRow(xlSheet5, rowIdx5++, x.XmlE.XEErrorCode, string.Format(formularError, "A", rowIdx5), x.Xml5.XML1.MA_BN,
//        //                        x.Xml5.XML1.HO_TEN, x.Xml5.XML1.MA_LK, x.Xml5.XML1.MA_KHOA, string.Format(formularKhoa, "F", rowIdx5),
//        //                        x.Xml5.XML1.NGAY_VAO, x.Xml5.XML1.NGAY_RA, x.Xml5.XML1.NGAY_TTOAN, x.Xml5.XML1.MA_BENH,
//        //                        x.Xml5.XML1.MA_BENHKHAC, x.Xml5.XML1.TEN_BENH, x.Xml5.STT);
//        //            }

//        //            for (int i = 0; i < 14; i++)
//        //            {
//        //                if (i == 1)
//        //                {
//        //                    xlSheet5.SetColumnWidth(i, _excelColWide);
//        //                }
//        //                else if (i == 6)
//        //                {
//        //                    xlSheet5.SetColumnWidth(i, _excelColMild);
//        //                }
//        //                else
//        //                {
//        //                    xlSheet5.AutoSizeColumn(i);
//        //                }
//        //            }
//        //        }
//        //        #endregion

//        //        #region Linhist removed 13/04/22 giam so luong join bang
//        //        //var dataNhom8 = data3.Where(x => x.MA_NHOM == 8 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //        //var XML3FailsNhom8 = data1.SelectMany(m => m.XML3s).Join(data1.SelectMany(m => m.XML3s).ToList(), z => z.MA_BAC_SI, u => u.MA_BAC_SI, (z, u) => new
//        //        //{
//        //        //    z,
//        //        //    u
//        //        //}).Where(p => p.z.MA_NHOM == 8
//        //        //&& p.u.MA_NHOM == 8
//        //        //&& p.z.MA_DICH_VU != p.u.MA_DICH_VU
//        //        //&& p.z.MA_LK != p.u.MA_LK
//        //        //&& p.z.NGAY_KQ != null
//        //        //&& p.z.NGAY_YL <= p.u.NGAY_YL
//        //        //&& p.z.NGAY_KQ >= p.u.NGAY_YL
//        //        //).Select(f => new {
//        //        //    MA_BS_1 = f.z.MA_BAC_SI,
//        //        //    XML1ID_1 = f.z.XML1ID,
//        //        //    Ma_LK_1 = f.z.MA_LK,
//        //        //    Ten_Dich_Vu_1 = f.z.TEN_DICH_VU,
//        //        //    Ngay_YL_1 = f.z.NGAY_YL,
//        //        //    Ngay_KQ_1 = f.z.NGAY_KQ,

//        //        //    MA_BS_2 = f.u.MA_BAC_SI,
//        //        //    XML1ID_2 = f.u.XML1ID,
//        //        //    Ma_LK_2 = f.u.MA_LK,
//        //        //    Ten_Dich_Vu_2 = f.u.TEN_DICH_VU,
//        //        //    Ngay_YL_2 = f.u.NGAY_YL,
//        //        //    Ngay_KQ_2 = f.u.NGAY_KQ,


//        //        //}).Join(data1.ToList(), z1 => z1.XML1ID_1, u1 => u1.XML1ID, (z1, u1) => new
//        //        //{
//        //        //    z1,
//        //        //    u1
//        //        //}).Join(data1.ToList(), z2 => z2.z1.XML1ID_2, u2 => u2.XML1ID, (z2, u2) => new
//        //        //{
//        //        //    Mã_BS_1 = z2.z1.MA_BS_1,
//        //        //    Mã_Liên_Kết_1 = z2.z1.Ma_LK_1,
//        //        //    Tên_Dịch_Vụ_1 = z2.z1.Ten_Dich_Vu_1,
//        //        //    Ngày_Kết_Quả_1 = z2.z1.Ngay_KQ_1,
//        //        //    Ngày_Y_Lệnh_1 = z2.z1.Ngay_YL_1,
//        //        //    Họ_Tên_1 = z2.u1.HO_TEN,
//        //        //    Mã_Thẻ_1 = z2.u1.MA_THE,



//        //        //    Mã_BS_2 = z2.z1.MA_BS_2,
//        //        //    Mã_Liên_Kết_2 = z2.z1.Ma_LK_2,
//        //        //    Tên_Dịch_Vụ_2 = z2.z1.Ten_Dich_Vu_2,
//        //        //    Ngày_Kết_Quả_2 = z2.z1.Ngay_KQ_2,
//        //        //    Ngày_Y_Lệnh_2 = z2.z1.Ngay_YL_2,
//        //        //    Họ_Tên_2 = u2.HO_TEN,
//        //        //    Mã_Thẻ_2 = u2.MA_THE,

//        //        //}).ToList();

//        //        //int rowIdxG8 = 0;
//        //        //ISheet xlSheetG8 = workbook.CreateSheet("Lỗi BS Dịch Vụ Trùng TG-Nhóm 8");
//        //        //AddHeaderRow(xlSheetG8, rowIdxG8++, "Mã BS 1", "Mã Liên kết 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //        //            "Họ tên 1", "Mã Thẻ 1", "Mã BS 2", "Mã Liên kết 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2",
//        //        //            "Họ tên 2", "Mã Thẻ 2");

//        //        //foreach (var x in XML3FailsNhom8)
//        //        //{
//        //        //    AddRow(xlSheetG8, rowIdxG8++, x.Mã_BS_1, x.Mã_Liên_Kết_1, x.Tên_Dịch_Vụ_1, x.Ngày_Kết_Quả_1, x.Ngày_Y_Lệnh_1,
//        //        //            x.Họ_Tên_1, x.Mã_Thẻ_1, x.Mã_BS_2, x.Mã_Liên_Kết_2, x.Tên_Dịch_Vụ_2, x.Ngày_Kết_Quả_2, x.Ngày_Y_Lệnh_2,
//        //        //            x.Họ_Tên_2, x.Mã_Thẻ_2);
//        //        //}

//        //        //for (int i = 0; i < 14; i++)
//        //        //{
//        //        //    xlSheetG8.AutoSizeColumn(i);
//        //        //}
//        //        #endregion


//        //        var allXMl3 = data1.SelectMany(x => x.XML3s);

//        //        #region Sheet 8: Loi BS DV Trung TG - Nhom 8
//        //        // Linhist added 13/04/22 bo qua cac dich vu khong can check
//        //        var dataNhom8 = allXMl3.Where(x => x.MA_NHOM == 8 && !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0133.0209") && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //        var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//        //                                .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //        int rowIdxG8 = 0;
//        //        ISheet xlSheetG8 = workbook.CreateSheet("Lỗi BS Dịch Vụ Trùng TG-Nhóm 8");
//        //        AddHeaderRow(xlSheetG8, rowIdxG8++, "Mã BS 1", "Mã Liên kết 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                    "Họ tên 1", "Mã Khoa 1", "Mã Thẻ 1", "Mã BS 2", "Mã Liên kết 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2",
//        //                    "Họ tên 2", "Mã Khoa 2", "Mã Thẻ 2");

//        //        foreach (var t in xml3Nhom8Trung)
//        //        {
//        //            AddRow(xlSheetG8, rowIdxG8++, t.x1.MA_BAC_SI, t.x1.MA_LK, t.x1.TEN_DICH_VU, t.x1.NGAY_KQ, t.x1.NGAY_YL,
//        //                    t.x1.XML1.HO_TEN, t.x1.MA_KHOA, t.x1.XML1.MA_THE, t.x2.MA_BAC_SI, t.x2.MA_LK, t.x2.TEN_DICH_VU, t.x2.NGAY_KQ,
//        //                    t.x2.NGAY_YL, t.x2.XML1.HO_TEN, t.x2.MA_KHOA, t.x2.XML1.MA_THE);
//        //        }

//        //        for (int i = 0; i < 14; i++)
//        //        {
//        //            xlSheetG8.AutoSizeColumn(i);
//        //        }
//        //        #endregion


//        //        #region Linhist removed 13/04/22 giam so luong join bang
//        //        //var XML3FailsNhom2 = data1.SelectMany(m => m.XML3s).Join(data1.SelectMany(m => m.XML3s).ToList(), z => z.MA_BAC_SI, u => u.MA_BAC_SI, (z, u) => new
//        //        //{
//        //        //    z,
//        //        //    u
//        //        //}).Where(p => p.z.MA_NHOM == 2
//        //        //&& p.u.MA_NHOM == 2
//        //        //&& p.z.MA_DICH_VU != p.u.MA_DICH_VU
//        //        //&& p.z.MA_LK != p.u.MA_LK
//        //        //&& p.z.NGAY_KQ != null
//        //        //&& p.z.NGAY_YL <= p.u.NGAY_YL
//        //        //&& p.z.NGAY_KQ >= p.u.NGAY_YL
//        //        //).Select(f => new {
//        //        //    MA_BS_1 = f.z.MA_BAC_SI,
//        //        //    XML1ID_1 = f.z.XML1ID,
//        //        //    Ma_LK_1 = f.z.MA_LK,
//        //        //    Ten_Dich_Vu_1 = f.z.TEN_DICH_VU,
//        //        //    Ngay_YL_1 = f.z.NGAY_YL,
//        //        //    Ngay_KQ_1 = f.z.NGAY_KQ,

//        //        //    MA_BS_2 = f.u.MA_BAC_SI,
//        //        //    XML1ID_2 = f.u.XML1ID,
//        //        //    Ma_LK_2 = f.u.MA_LK,
//        //        //    Ten_Dich_Vu_2 = f.u.TEN_DICH_VU,
//        //        //    Ngay_YL_2 = f.u.NGAY_YL,
//        //        //    Ngay_KQ_2 = f.u.NGAY_KQ,


//        //        //}).Join(data1.ToList(), z1 => z1.XML1ID_1, u1 => u1.XML1ID, (z1, u1) => new
//        //        //{
//        //        //    z1,
//        //        //    u1
//        //        //}).Join(data1.ToList(), z2 => z2.z1.XML1ID_2, u2 => u2.XML1ID, (z2, u2) => new
//        //        //{
//        //        //    Mã_BS_1 = z2.z1.MA_BS_1,
//        //        //    Mã_Liên_Kết_1 = z2.z1.Ma_LK_1,
//        //        //    Tên_Dịch_Vụ_1 = z2.z1.Ten_Dich_Vu_1,
//        //        //    Ngày_Kết_Quả_1 = z2.z1.Ngay_KQ_1,
//        //        //    Ngày_Y_Lệnh_1 = z2.z1.Ngay_YL_1,
//        //        //    Họ_Tên_1 = z2.u1.HO_TEN,
//        //        //    Mã_Thẻ_1 = z2.u1.MA_THE,



//        //        //    Mã_BS_2 = z2.z1.MA_BS_2,
//        //        //    Mã_Liên_Kết_2 = z2.z1.Ma_LK_2,
//        //        //    Tên_Dịch_Vụ_2 = z2.z1.Ten_Dich_Vu_2,
//        //        //    Ngày_Kết_Quả_2 = z2.z1.Ngay_KQ_2,
//        //        //    Ngày_Y_Lệnh_2 = z2.z1.Ngay_YL_2,
//        //        //    Họ_Tên_2 = u2.HO_TEN,
//        //        //    Mã_Thẻ_2 = u2.MA_THE,

//        //        //}).ToList();


//        //        //int rowIdxG2 = 0;
//        //        //ISheet xlSheetG2 = workbook.CreateSheet("Lỗi BS Dịch Vụ Trùng TG-Nhóm 2");
//        //        //AddHeaderRow(xlSheetG2, rowIdxG2++, "Mã BS 1", "Mã Liên kết 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //        //            "Họ tên 1", "Mã Thẻ 1", "Mã BS 2", "Mã Liên kết 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2",
//        //        //            "Họ tên 2", "Mã Thẻ 2");

//        //        //foreach (var x in XML3FailsNhom2)
//        //        //{
//        //        //    AddRow(xlSheetG2, rowIdxG2++, x.Mã_BS_1, x.Mã_Liên_Kết_1, x.Tên_Dịch_Vụ_1, x.Ngày_Kết_Quả_1, x.Ngày_Y_Lệnh_1,
//        //        //            x.Họ_Tên_1, x.Mã_Thẻ_1, x.Mã_BS_2, x.Mã_Liên_Kết_2, x.Tên_Dịch_Vụ_2, x.Ngày_Kết_Quả_2, x.Ngày_Y_Lệnh_2,
//        //        //            x.Họ_Tên_2, x.Mã_Thẻ_2);
//        //        //}

//        //        //for (int i = 0; i < 14; i++)
//        //        //{
//        //        //    xlSheetG2.AutoSizeColumn(i);
//        //        //}
//        //        #endregion

//        //        #region Sheet 9: Loi BS DV Trung TG - Nhom 2
//        //        var dataNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //        var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//        //                                .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //        int rowIdxG2 = 0;
//        //        ISheet xlSheetG2 = workbook.CreateSheet("Lỗi BS Dịch Vụ Trùng TG-Nhóm 2");
//        //        AddHeaderRow(xlSheetG2, rowIdxG2++, "Mã BS 1", "Mã Liên kết 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                    "Họ tên 1", "Mã Khoa 1", "Mã Thẻ 1", "Mã BS 2", "Mã Liên kết 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2",
//        //                    "Họ tên 2", "Mã Khoa 2", "Mã Thẻ 2");

//        //        foreach (var t in xml3Nhom2Trung)
//        //        {
//        //            AddRow(xlSheetG2, rowIdxG2++, t.x1.MA_BAC_SI, t.x1.MA_LK, t.x1.TEN_DICH_VU, t.x1.NGAY_KQ, t.x1.NGAY_YL,
//        //                    t.x1.XML1.HO_TEN, t.x1.MA_KHOA, t.x1.XML1.MA_THE, t.x2.MA_BAC_SI, t.x2.MA_LK, t.x2.TEN_DICH_VU, t.x2.NGAY_KQ,
//        //                    t.x2.NGAY_YL, t.x2.XML1.HO_TEN, t.x2.MA_KHOA, t.x2.XML1.MA_THE);
//        //        }

//        //        for (int i = 0; i < 14; i++)
//        //        {
//        //            xlSheetG2.AutoSizeColumn(i);
//        //        }
//        //        #endregion

//        //        #region Sheet phu: DM Khoa
//        //        var dataK = DMKhoas.OrderBy(k => k.KhoaCode);

//        //        int rowIdxK = 0;
//        //        ISheet xlSheetK = workbook.CreateSheet("DM Khoa");
//        //        AddHeaderRow(xlSheetK, rowIdxK++, "Mã Khoa", "Tên Khoa");

//        //        foreach (var x in dataK)
//        //        {
//        //            AddRow(xlSheetK, rowIdxK++, x.KhoaCode, x.KhoaName);
//        //        }

//        //        for (int i = 0; i < 2; i++)
//        //        {
//        //            xlSheetK.AutoSizeColumn(i);
//        //        }
//        //        #endregion

//        //        workbook.Write(fs);
//        //        result = fs.ToArray();
//        //    }

//        //    return result;
//        //}
//        #endregion
//        #endregion

//        #region Linhist added 28/04/22
//        //private static ICellStyle _lftCellStyle = null;
//        //private static ICellStyle _midCellStyle = null;
//        //private static ICellStyle _rgtCellStyle = null;

//        private static string LoaiBS(string CHUCDANH_NN)
//        {
//            int loai = 0;
//            if (int.TryParse(CHUCDANH_NN, out loai))
//            {
//                switch (loai)
//                {
//                    case 1: return "Bác sĩ";
//                    case 2: return "Y sĩ";
//                    case 3: return "Điều dưỡng";
//                    case 4: return "Hộ sinh";
//                    case 5: return "Kỹ thuật viên";
//                    case 6: return "Cử nhân X-quang";
//                    case 7: return "Dược sĩ Đại học";
//                    case 8: return "Dược sĩ Trung cấp";
//                }
//            }
//            return "";
//        }

//        #region Linhist removed 19/10/23
//        //private static void AddCaptionRow(ISheet xlSheet, int rowIdx = 0, string content = "", int colMerge = 0)
//        //{
//        //    if (colMerge > 0)
//        //    {
//        //        var cellRanged = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, colMerge);
//        //        xlSheet.AddMergedRegion(cellRanged);
//        //    }

//        //    IRow row = xlSheet.CreateRow(rowIdx);
//        //    row.HeightInPoints = 36f;

//        //    ICell cell = row.CreateCell(rowIdx);

//        //    cell.SetCellValue(content?.ToUpper());
//        //    cell.CellStyle = _captCellStyle;
//        //}

//        //private static void AddHeaderRow(ISheet xlSheet, int rowIdx = 0, params string[] columns)
//        //{
//        //    if (columns == null || columns.Length == 0) return;

//        //    IRow row = xlSheet.CreateRow(rowIdx);
//        //    row.HeightInPoints = 24f;

//        //    for (int i = 0; i < columns.Length; i++)
//        //    {
//        //        ICell cell = row.CreateCell(i);
//        //        cell.SetCellValue(columns[i]?.ToUpper());
//        //        cell.CellStyle = _headCellStyle;
//        //    }
//        //}

//        //private static void AddRow(ISheet xlSheet, int rowIdx = 0, string aligns = "", params object[] values)
//        //{
//        //    if (values == null || values.Length == 0) return;
//        //    if (string.IsNullOrWhiteSpace(aligns))
//        //    {
//        //        aligns = "l";
//        //    }

//        //    char alg = 'l';
//        //    string val = "";
//        //    IRow row = xlSheet.CreateRow(rowIdx);
//        //    for (int i = 0; i < values.Length; i++)
//        //    {
//        //        ICell cell = row.CreateCell(i);

//        //        alg = aligns.Length > i ? aligns[i] : aligns[0];
//        //        cell.CellStyle = (alg == 'r' ? _rgtCellStyle : (alg == 'c' ? _midCellStyle : _lftCellStyle));

//        //        val = values[i] is DateTime ? $"{values[i]:dd/MM/yyyy HH:mm:ss tt}" : $"{values[i]}";
//        //        if (val.StartsWith("$="))
//        //        {
//        //            cell.SetCellFormula(val[2..]);
//        //        }
//        //        else
//        //        {
//        //            cell.SetCellValue(val);
//        //        }
//        //    }
//        //}

//        //private static string GetError(Dictionary<int, string> dicErrors, int? MA_LOI)
//        //{
//        //    if (MA_LOI == null) return "";

//        //    string err = "";
//        //    dicErrors.TryGetValue(MA_LOI.Value, out err);
//        //    return err;
//        //}

//        //private static string GetKhoa(IEnumerable<DMKhoa> allKhoas, Dictionary<string, string> dicKhoas, string MA_KHOA)
//        //{
//        //    if (string.IsNullOrEmpty(MA_KHOA)) return "";

//        //    string khoaName = "";
//        //    if (dicKhoas.TryGetValue(MA_KHOA, out khoaName))
//        //    {
//        //        return khoaName;
//        //    }

//        //    var khoa = allKhoas.FirstOrDefault(k => k.KhoaCode == MA_KHOA);
//        //    if (khoa != null)
//        //    {
//        //        khoaName = khoa.KhoaName;
//        //        dicKhoas.Add(MA_KHOA, khoaName);
//        //    }
//        //    return khoaName;
//        //}

//        //private static string GetOneBS(IEnumerable<DMBacSi> allBacsis, Dictionary<string, string> dicBacsis, string MA_BS)
//        //{
//        //    string bsName = null;
//        //    if (dicBacsis.TryGetValue(MA_BS, out bsName))
//        //    {
//        //        return bsName;
//        //    }

//        //    var bs = allBacsis.FirstOrDefault(b => b.MACCHN == MA_BS);
//        //    if (bs != null)
//        //    {
//        //        bsName = $"{bs.HO_TEN} ({LoaiBS(bs.CHUCDANH_NN)})";
//        //        dicBacsis.Add(MA_BS, bsName);
//        //    }
//        //    return bsName;
//        //}

//        //private static string GetBacSi(IEnumerable<DMBacSi> allBacsis, Dictionary<string, string> dicBacsis, ref string MA_BACSI)
//        //{
//        //    if (string.IsNullOrEmpty(MA_BACSI)) return "";

//        //    string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
//        //    MA_BACSI = "";
//        //    string bacsi = "";
//        //    string bs = "";

//        //    if (lstBs.Length == 1)
//        //    {
//        //        MA_BACSI = lstBs[0];
//        //        return GetOneBS(allBacsis, dicBacsis, lstBs[0]);
//        //    }

//        //    for (int i = 0; i < lstBs.Length; i++)
//        //    {
//        //        if (i > 0)
//        //        {
//        //            MA_BACSI += Environment.NewLine;
//        //            bacsi += Environment.NewLine;
//        //        }
//        //        if (i + 1 < lstBs.Length)
//        //        {
//        //            bs = GetOneBS(allBacsis, dicBacsis, $"{lstBs[i]};{lstBs[i + 1]}");
//        //            if (bs != null)
//        //            {
//        //                MA_BACSI += $"{lstBs[i]};{lstBs[i + 1]}";
//        //                bacsi += bs;
//        //                continue;
//        //            }
//        //        }

//        //        MA_BACSI += lstBs[i];
//        //        bacsi += GetOneBS(allBacsis, dicBacsis, lstBs[i]);

//        //    }
//        //    return bacsi;
//        //}

//        //static public byte[] DownloadExcelReport8Sheets(IEnumerable<XML1> data1, IEnumerable<DMError> DMErrors, IEnumerable<DMKhoa> DMKhoas, IEnumerable<DMBacSi> DMBacsis)
//        //{
//        //    //Create new excel
//        //    IWorkbook workbook = new XSSFWorkbook();

//        //    //Create some predefined style for cell
//        //    IFont boldFontTitle = workbook.CreateFont();
//        //    boldFontTitle.IsBold = true;
//        //    boldFontTitle.FontHeightInPoints = 20;
//        //    IFont boldFont = workbook.CreateFont();
//        //    boldFont.IsBold = true;
//        //    boldFont.FontHeightInPoints = 13;

//        //    _captCellStyle = workbook.CreateCellStyle();
//        //    _captCellStyle.SetFont(boldFontTitle);
//        //    _captCellStyle.Alignment = HorizontalAlignment.Center;
//        //    _captCellStyle.VerticalAlignment = VerticalAlignment.Center;

//        //    _headCellStyle = workbook.CreateCellStyle();
//        //    _headCellStyle.SetFont(boldFont);
//        //    _headCellStyle.BorderBottom = BorderStyle.Medium;
//        //    _headCellStyle.BorderRight = BorderStyle.Medium;
//        //    _headCellStyle.BorderTop = BorderStyle.Medium;
//        //    _headCellStyle.BorderLeft = BorderStyle.Medium;
//        //    _headCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;
//        //    _headCellStyle.FillPattern = FillPattern.SolidForeground;
//        //    _headCellStyle.Alignment = HorizontalAlignment.Center;
//        //    _headCellStyle.VerticalAlignment = VerticalAlignment.Center;

//        //    _lftCellStyle = workbook.CreateCellStyle();
//        //    _lftCellStyle.BorderBottom = BorderStyle.Medium;
//        //    _lftCellStyle.BorderRight = BorderStyle.Medium;
//        //    _lftCellStyle.BorderTop = BorderStyle.Medium;
//        //    _lftCellStyle.BorderLeft = BorderStyle.Medium;
//        //    _lftCellStyle.Alignment = HorizontalAlignment.Left;
//        //    _lftCellStyle.VerticalAlignment = VerticalAlignment.Top;
//        //    _lftCellStyle.Alignment = HorizontalAlignment.Left;
//        //    _lftCellStyle.WrapText = true;

//        //    _midCellStyle = workbook.CreateCellStyle();
//        //    _midCellStyle.BorderBottom = BorderStyle.Medium;
//        //    _midCellStyle.BorderRight = BorderStyle.Medium;
//        //    _midCellStyle.BorderTop = BorderStyle.Medium;
//        //    _midCellStyle.BorderLeft = BorderStyle.Medium;
//        //    _midCellStyle.Alignment = HorizontalAlignment.Left;
//        //    _midCellStyle.VerticalAlignment = VerticalAlignment.Top;
//        //    _midCellStyle.Alignment = HorizontalAlignment.Center;
//        //    _midCellStyle.WrapText = true;

//        //    _rgtCellStyle = workbook.CreateCellStyle();
//        //    _rgtCellStyle.BorderBottom = BorderStyle.Medium;
//        //    _rgtCellStyle.BorderRight = BorderStyle.Medium;
//        //    _rgtCellStyle.BorderTop = BorderStyle.Medium;
//        //    _rgtCellStyle.BorderLeft = BorderStyle.Medium;
//        //    _rgtCellStyle.Alignment = HorizontalAlignment.Left;
//        //    _rgtCellStyle.VerticalAlignment = VerticalAlignment.Top;
//        //    _rgtCellStyle.Alignment = HorizontalAlignment.Right;
//        //    _rgtCellStyle.WrapText = true;

//        //    //format cho cell function
//        //    string reportCaption = $"Báo Cáo Tổng Hợp {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}";

//        //    #region Linhist added 26/04/22 them dictionary cho cac loai danh muc
//        //    var allErrors = data1.SelectMany(x => x.XML1Errors);
//        //    var allErrCodes = allErrors.GroupBy(x => x.XEErrorCode, (k, g) => new { ErrorCode = k, Count = g.Count() });

//        //    Dictionary<int, string> dicErrors = new Dictionary<int, string>();
//        //    foreach (var eg in allErrCodes)
//        //    {
//        //        var err = DMErrors.FirstOrDefault(e => e.ErrorCode == eg.ErrorCode);
//        //        if (err != null)
//        //        {
//        //            dicErrors.Add(err.ErrorCode.Value, err.ErrorContent);
//        //        }
//        //    }

//        //    Dictionary<string, string> dicKhoas = new Dictionary<string, string>();
//        //    Dictionary<string, string> dicBacsis = new Dictionary<string, string>();
//        //    #endregion

//        //    int stt = 1;
//        //    byte[] result = null;
//        //    using (var fs = new MemoryStream())
//        //    {
//        //        #region Sheet 1: DS File Bao cao
//        //        int rowIdxF = 0;
//        //        var grpFiles = data1.OrderBy(x => x.FileName);
//        //        ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");
//        //        AddCaptionRow(xlSheetF, rowIdxF++, reportCaption, 11);
//        //        AddHeaderRow(xlSheetF, rowIdxF++, "STT",
//        //                    "Tên File", "Mã MA_CSKCB",
//        //                    "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Địa chỉ",
//        //                    "Mã Khoa", "Tên Khoa",
//        //                    "Ngày vào", "Ngày ra", "Ngày lập");

//        //        foreach (var x in grpFiles)
//        //        {
//        //            AddRow(xlSheetF, rowIdxF++, "clcccllclccc", stt++,
//        //                    x.FileName, x.MA_CSKCB,
//        //                    x.MA_LK, x.MA_BN, x.HO_TEN, x.DIA_CHI,
//        //                    x.MA_KHOA, GetKhoa(DMKhoas, dicKhoas, x.MA_KHOA),
//        //                    x.NGAY_VAO, x.NGAY_RA, x.CreateDate);
//        //        }

//        //        for (int i = 0; i < 12; i++)
//        //        {
//        //            xlSheetF.AutoSizeColumn(i);
//        //        }
//        //        xlSheetF.SetAutoFilter(CellRangeAddress.ValueOf("A2:L2"));
//        //        #endregion



//        //        #region Sheet 2: Tong hop Loi
//        //        stt = 1;
//        //        var grpErrors = allErrCodes.Join(DMErrors, x => x.ErrorCode, y => y.ErrorCode, (x, y) => new
//        //        {
//        //            y.ErrorCode,
//        //            y.ErrorAtXMLFile,
//        //            y.ErrorAtXMLTag,
//        //            x.Count,
//        //            y.ErrorContent
//        //        }).OrderBy(e => e.ErrorAtXMLFile);


//        //        int rowIdxE = 0;
//        //        ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Số Lỗi");
//        //        AddCaptionRow(xlSheetE, rowIdxE++, "Tổng hợp số lỗi trong hồ sơ", 6);
//        //        AddHeaderRow(xlSheetE, rowIdxE++, "STT", "Mã lỗi", "Loại File lỗi", "Lỗi tại Thẻ", "Chi tiết lỗi", "Số lỗi");

//        //        foreach (var x2 in grpErrors)
//        //        {
//        //            AddRow(xlSheetE, rowIdxE++, "cccclr", stt++, x2.ErrorCode, x2.ErrorAtXMLFile, x2.ErrorAtXMLTag, x2.ErrorContent, x2.Count);
//        //        }

//        //        for (int i = 0; i < 6; i++)
//        //        {
//        //            xlSheetE.AutoSizeColumn(i);
//        //        }

//        //        xlSheetE.SetAutoFilter(CellRangeAddress.ValueOf("A2:F2"));
//        //        #endregion


//        //        #region Sheet 3: Loi XML1
//        //        stt = 1;
//        //        List<int?> xml1ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML1").Select(e => e.ErrorCode).ToList();
//        //        var xml1Errors = allErrors.Where(a => xml1ErCodes.Contains(a.XEErrorCode));

//        //        int rowIdx1 = 0;
//        //        ISheet xlSheet1 = workbook.CreateSheet("Lỗi XML1");
//        //        AddCaptionRow(xlSheet1, rowIdx1++, "Danh sách lỗi của XML1", 13);
//        //        AddHeaderRow(xlSheet1, rowIdx1++, "STT",
//        //                    "Mã lỗi", "Chi tiết lỗi",
//        //                    "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                    "Mã Khoa", "Tên Khoa",
//        //                    "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                    "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //        foreach (var x in xml1Errors)
//        //        {
//        //            AddRow(xlSheet1, rowIdx1++, "cclcclclcclccc", stt++,
//        //                    x.XEErrorCode, GetError(dicErrors, x.XEErrorCode),
//        //                    x.XEXML1.MA_LK, x.XEXML1.MA_BN, x.XEXML1.HO_TEN,
//        //                    x.XEXML1.MA_KHOA, GetKhoa(DMKhoas, dicKhoas, x.XEXML1.MA_KHOA),
//        //                    x.XEXML1.MA_BENH, x.XEXML1.MA_BENHKHAC, x.XEXML1.TEN_BENH,
//        //                    x.XEXML1.NGAY_VAO, x.XEXML1.NGAY_RA, x.XEXML1.NGAY_TTOAN);
//        //        }

//        //        for (int i = 0; i < 14; i++)
//        //        {
//        //            xlSheet1.AutoSizeColumn(i);
//        //        }
//        //        xlSheet1.SetAutoFilter(CellRangeAddress.ValueOf("A2:N2"));
//        //        #endregion


//        //        #region Sheet 4: Loi XML2
//        //        stt = 1;
//        //        List<int?> xml2ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML2").Select(e => e.ErrorCode).ToList();
//        //        var xml2Errors = allErrors.Where(a => xml2ErCodes.Contains(a.XEErrorCode));

//        //        var data2 =
//        //            data1.SelectMany(x => x.XML2s)
//        //            .Join(xml2Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml2 = x, XmlE = e })
//        //            .Where(x => x.Xml2.STT == x.XmlE.XEErrorRow);

//        //        int rowIdx2 = 0;
//        //        ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML2");
//        //        AddCaptionRow(xlSheet2, rowIdx2++, "Danh sách lỗi của XML2", 28);
//        //        AddHeaderRow(xlSheet2, rowIdx2++, "STT",
//        //                    "Mã lỗi", "Chi tiết lỗi",
//        //                    "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                    "Mã Khoa", "Tên Khoa",
//        //                    "Mã Bác sĩ", "Tên Bác sĩ", "Khoa y lệnh",
//        //                    "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                    "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
//        //                    "TT Thầu", "Đơn giá", "Số lượng", "Tỷ lệ TT", "Thành tiền",
//        //                    "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //        foreach (var x in data2)
//        //        {
//        //            string maBS = x.Xml2.MA_BAC_SI;
//        //            string tenBS = GetBacSi(DMBacsis, dicBacsis, ref maBS);
//        //            AddRow(xlSheet2, rowIdx2++, "cclcclclclccclcclclccrrrrcccc", stt++,
//        //                    x.XmlE.XEErrorCode, GetError(dicErrors, x.XmlE.XEErrorCode),
//        //                    x.Xml2.XML1.MA_LK, x.Xml2.XML1.MA_BN, x.Xml2.XML1.HO_TEN,
//        //                    x.Xml2.XML1.MA_KHOA, GetKhoa(DMKhoas, dicKhoas, x.Xml2.XML1.MA_KHOA),
//        //                    maBS, tenBS, x.Xml2.MA_KHOA,
//        //                    x.Xml2.XML1.MA_BENH, x.Xml2.XML1.MA_BENHKHAC, x.Xml2.XML1.TEN_BENH,
//        //                    x.Xml2.MA_THUOC, x.Xml2.MA_NHOM, x.Xml2.TEN_THUOC, x.Xml2.DON_VI_TINH, x.Xml2.LIEU_DUNG, x.Xml2.SO_DANG_KY,
//        //                    x.Xml2.TT_THAU, x.Xml2.DON_GIA, x.Xml2.SO_LUONG, x.Xml2.TYLE_TT, x.Xml2.THANH_TIEN,
//        //                    x.Xml2.NGAY_YL, x.Xml2.XML1.NGAY_VAO, x.Xml2.XML1.NGAY_RA, x.Xml2.XML1.NGAY_TTOAN);
//        //        }

//        //        for (int i = 0; i < 29; i++)
//        //        {
//        //            xlSheet2.AutoSizeColumn(i);
//        //        }
//        //        xlSheet2.SetAutoFilter(CellRangeAddress.ValueOf("A2:AC2"));
//        //        #endregion


//        //        #region Sheet 5: Loi XML3
//        //        stt = 1;
//        //        var data3 = data1.SelectMany(x => x.XML3s);
//        //        if (data3.Any())
//        //        {
//        //            List<int?> xml3ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML3").Select(e => e.ErrorCode).ToList();
//        //            var xml3Errors = allErrors.Where(a => xml3ErCodes.Contains(a.XEErrorCode));

//        //            var data3Join =
//        //                data3.Join(xml3Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml3 = x, XmlE = e })
//        //                .Where(x => x.Xml3.STT == x.XmlE.XEErrorRow);

//        //            int rowIdx3 = 0;
//        //            ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML3");
//        //            AddCaptionRow(xlSheet3, rowIdx3++, "Danh sách lỗi của XML3", 27);
//        //            AddHeaderRow(xlSheet3, rowIdx3++, "STT",
//        //                        "Mã lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Mã Bác sĩ", "Tên Bác sĩ", "Khoa y lệnh",
//        //                        "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
//        //                        "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //            foreach (var x in data3Join)
//        //            {
//        //                string maBS = x.Xml3.MA_BAC_SI;
//        //                string tenBS = GetBacSi(DMBacsis, dicBacsis, ref maBS);
//        //                AddRow(xlSheet3, rowIdx3++, "cclcclclclcclclcclccrrrccccc", stt++,
//        //                        x.XmlE.XEErrorCode, GetError(dicErrors, x.XmlE.XEErrorCode),
//        //                        x.Xml3.XML1.MA_LK, x.Xml3.XML1.MA_BN, x.Xml3.XML1.HO_TEN,
//        //                        x.Xml3.XML1.MA_KHOA, GetKhoa(DMKhoas, dicKhoas, x.Xml3.XML1.MA_KHOA),
//        //                        maBS, tenBS, x.Xml3.MA_KHOA,
//        //                        x.Xml3.MA_DICH_VU, x.Xml3.TEN_DICH_VU, x.Xml3.MA_VAT_TU, x.Xml3.TEN_VAT_TU,
//        //                        x.Xml3.XML1.MA_BENH, x.Xml3.XML1.MA_BENHKHAC, x.Xml3.XML1.TEN_BENH,
//        //                        x.Xml3.DON_VI_TINH, x.Xml3.TT_THAU, x.Xml3.DON_GIA, x.Xml3.TYLE_TT,
//        //                        x.Xml3.THANH_TIEN, x.Xml3.NGAY_YL, x.Xml3.NGAY_KQ, x.Xml3.XML1.NGAY_VAO, x.Xml3.XML1.NGAY_RA, x.Xml3.XML1.NGAY_TTOAN);
//        //            }

//        //            for (int i = 0; i < 28; i++)
//        //            {
//        //                xlSheet3.AutoSizeColumn(i);
//        //            }
//        //            xlSheet3.SetAutoFilter(CellRangeAddress.ValueOf("A2:AB2"));
//        //        }
//        //        #endregion


//        //        #region Sheet 6: Loi XML4
//        //        stt = 1;
//        //        var data4 = data1.SelectMany(x => x.XML4s);
//        //        if (data4.Any())
//        //        {
//        //            List<int?> xml4ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//        //            var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.XEErrorCode));

//        //            var data4Join =
//        //                data4.Join(xml4Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml4 = x, XmlE = e })
//        //                .Where(x => x.Xml4.STT == x.XmlE.XEErrorRow);

//        //            int rowIdx4 = 0;
//        //            ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML4");
//        //            AddCaptionRow(xlSheet4, rowIdx4++, "Danh sách lỗi của XML4", 13);
//        //            AddHeaderRow(xlSheet4, rowIdx4++, "STT",
//        //                        "Mã lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //            foreach (var x in data4Join)
//        //            {
//        //                AddRow(xlSheet4, rowIdx4++, "cclcclclcclccc", stt++,
//        //                        x.XmlE.XEErrorCode, GetError(dicErrors, x.XmlE.XEErrorCode),
//        //                        x.Xml4.XML1.MA_LK, x.Xml4.XML1.MA_BN, x.Xml4.XML1.HO_TEN,
//        //                        x.Xml4.XML1.MA_KHOA, GetKhoa(DMKhoas, dicKhoas, x.Xml4.XML1.MA_KHOA),
//        //                        x.Xml4.XML1.MA_BENH, x.Xml4.XML1.MA_BENHKHAC, x.Xml4.XML1.TEN_BENH,
//        //                        x.Xml4.XML1.NGAY_VAO, x.Xml4.XML1.NGAY_RA, x.Xml4.XML1.NGAY_TTOAN);
//        //            }

//        //            for (int i = 0; i < 14; i++)
//        //            {
//        //                xlSheet4.AutoSizeColumn(i);
//        //            }
//        //            xlSheet4.SetAutoFilter(CellRangeAddress.ValueOf("A2:N2"));
//        //        }
//        //        #endregion


//        //        #region Sheet 7: Loi XML5
//        //        stt = 1;
//        //        var data5 = data1.SelectMany(x => x.XML5s);
//        //        if (data5.Any())
//        //        {
//        //            List<int?> xml5ErCodes = DMErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//        //            var xml4Errors = allErrors.Where(a => xml5ErCodes.Contains(a.XEErrorCode));

//        //            var data5Join =
//        //                data5.Join(xml4Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml5 = x, XmlE = e })
//        //                .Where(x => x.Xml5.STT == x.XmlE.XEErrorRow);

//        //            int rowIdx5 = 0;
//        //            ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML5");
//        //            AddCaptionRow(xlSheet5, rowIdx5++, "Danh sách lỗi của XML5", 13);
//        //            AddHeaderRow(xlSheet5, rowIdx5++, "STT",
//        //                        "Mã lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //            foreach (var x in data5Join)
//        //            {
//        //                AddRow(xlSheet5, rowIdx5++, "cclcclclcclccc", stt++,
//        //                        x.XmlE.XEErrorCode, GetError(dicErrors, x.XmlE.XEErrorCode),
//        //                        x.Xml5.XML1.MA_LK, x.Xml5.XML1.MA_KHOA, GetKhoa(DMKhoas, dicKhoas, x.Xml5.XML1.MA_KHOA),
//        //                        x.Xml5.XML1.MA_BN, x.Xml5.XML1.HO_TEN,
//        //                        x.Xml5.XML1.MA_BENH, x.Xml5.XML1.MA_BENHKHAC, x.Xml5.XML1.TEN_BENH,
//        //                        x.Xml5.XML1.NGAY_VAO, x.Xml5.XML1.NGAY_RA, x.Xml5.XML1.NGAY_TTOAN);
//        //            }

//        //            for (int i = 0; i < 14; i++)
//        //            {
//        //                xlSheet5.AutoSizeColumn(i);
//        //            }
//        //            xlSheet5.SetAutoFilter(CellRangeAddress.ValueOf("A2:N2"));
//        //        }
//        //        #endregion



//        //        var allXMl3 = data1.SelectMany(x => x.XML3s);

//        //        #region Sheet 8: Loi BS DV Trung TG - Nhom 8
//        //        stt = 1;
//        //        var dataNhom8 = allXMl3.Where(x => x.MA_NHOM == 8 && !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0133.0209") && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //        var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//        //                                .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //        int rowIdxG8 = 0;
//        //        ISheet xlSheetG8 = workbook.CreateSheet("Lỗi BS&DV Trùng Nhóm 8");
//        //        AddCaptionRow(xlSheetG8, rowIdxG8++, "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8", 20);
//        //        AddHeaderRow(xlSheetG8, rowIdxG8++, "STT",
//        //                    "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1",
//        //                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1", "Mã Thẻ 1",
//        //                    "Khoa Y lệnh 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                    "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2",
//        //                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2", "Mã Thẻ 2",
//        //                    "Khoa Y lệnh 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//        //        foreach (var t in xml3Nhom8Trung)
//        //        {
//        //            string maBS1 = t.x1.MA_BAC_SI;
//        //            string tenBS1 = GetBacSi(DMBacsis, dicBacsis, ref maBS1);
//        //            string maBS2 = t.x2.MA_BAC_SI;
//        //            string tenBS2 = GetBacSi(DMBacsis, dicBacsis, ref maBS2);

//        //            AddRow(xlSheetG8, rowIdxG8++, "ccclclcclcccclclcclcc", stt++,
//        //                    t.x1.MA_LK, maBS1, tenBS1,
//        //                    t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN, t.x1.XML1.MA_THE,
//        //                    t.x1.MA_KHOA, t.x1.TEN_DICH_VU, t.x1.NGAY_KQ, t.x1.NGAY_YL,
//        //                    t.x2.MA_LK, maBS2, tenBS2,
//        //                    t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN, t.x2.XML1.MA_THE,
//        //                    t.x2.MA_KHOA, t.x2.TEN_DICH_VU, t.x2.NGAY_KQ, t.x2.NGAY_YL);
//        //        }

//        //        for (int i = 0; i < 21; i++)
//        //        {
//        //            xlSheetG8.AutoSizeColumn(i);
//        //        }
//        //        xlSheetG8.SetAutoFilter(CellRangeAddress.ValueOf("A2:U2"));
//        //        #endregion


//        //        #region Sheet 9: Loi BS DV Trung TG - Nhom 2
//        //        stt = 1;
//        //        var dataNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //        var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//        //                                .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //        //List<XML3> trungNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null).ToList();

//        //        int rowIdxG2 = 0;
//        //        ISheet xlSheetG2 = workbook.CreateSheet("Lỗi BS&DV Trùng Nhóm 2");
//        //        AddCaptionRow(xlSheetG2, rowIdxG2++, "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 2", 20);
//        //        AddHeaderRow(xlSheetG2, rowIdxG2++, "STT",
//        //                    "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1",
//        //                    "Mã Bệnh nhân 1", "Tên Bệnh nhân 1", "Mã Thẻ 1",
//        //                    "Khoa Y lệnh 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                    "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2",
//        //                    "Mã Bệnh nhân 2", "Tên Bệnh nhân 2", "Mã Thẻ 2",
//        //                    "Khoa Y lệnh 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//        //        foreach (var t in xml3Nhom2Trung)
//        //        {
//        //            string maBS1 = t.x1.MA_BAC_SI;
//        //            string tenBS1 = GetBacSi(DMBacsis, dicBacsis, ref maBS1);
//        //            string maBS2 = t.x2.MA_BAC_SI;
//        //            string tenBS2 = GetBacSi(DMBacsis, dicBacsis, ref maBS2);

//        //            AddRow(xlSheetG2, rowIdxG2++, "ccclclcclcccclclcclcc", stt++,
//        //                t.x1.MA_LK, maBS1, tenBS1,
//        //                t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN, t.x1.XML1.MA_THE,
//        //                t.x1.MA_KHOA, t.x1.TEN_DICH_VU, t.x1.NGAY_KQ, t.x1.NGAY_YL,
//        //                t.x2.MA_LK, maBS2, tenBS2,
//        //                t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN, t.x2.XML1.MA_THE,
//        //                t.x2.MA_KHOA, t.x2.TEN_DICH_VU, t.x2.NGAY_KQ, t.x2.NGAY_YL);
//        //        }

//        //        for (int i = 0; i < 21; i++)
//        //        {
//        //            xlSheetG2.AutoSizeColumn(i);
//        //        }
//        //        xlSheetG2.SetAutoFilter(CellRangeAddress.ValueOf("A2:U2"));
//        //        #endregion

//        //        workbook.Write(fs);
//        //        result = fs.ToArray();
//        //    }

//        //    return result;
//        //}
//        #endregion
//        #endregion

//        #region Linhist added 03/05/22
//        private static IWorkbook CreateWorkBook(out ICellStyle[] cellStyles)
//        {
//            //Create new excel
//            IWorkbook workbook = new XSSFWorkbook();

//            //Create some predefined style for cell
//            IFont boldFontTitle = workbook.CreateFont();
//            boldFontTitle.IsBold = true;
//            boldFontTitle.FontHeightInPoints = 20;
//            IFont boldFont = workbook.CreateFont();
//            boldFont.IsBold = true;
//            boldFont.FontHeightInPoints = 13;

//            cellStyles = new ICellStyle[] { workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle(), workbook.CreateCellStyle() };

//            //CellStyle 0 for Page Title
//            cellStyles[0].SetFont(boldFontTitle);

//            //CellStyle 1, 2 for Row Caption
//            for (int i = 1; i < 3; i++)
//            {
//                cellStyles[i].SetFont(boldFont);
//                cellStyles[i].FillPattern = FillPattern.SolidForeground;
//            }

//            cellStyles[1].FillForegroundColor = NPOI.HSSF.Util.HSSFColor.SkyBlue.Index;

//            //CellStyle 2 for Row Caption in Sat & Sun
//            cellStyles[2].FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;


//            //Other CellStyle 3 to 5 for Row data
//            //CellStyle 4 for align left, CellStyle 5 for align right
//            for (int i = 0; i < cellStyles.Length; i++)
//            {
//                cellStyles[i].Alignment = (i == 4 ? HorizontalAlignment.Left : (i == 5 ? HorizontalAlignment.Right : HorizontalAlignment.Center));
//                cellStyles[i].VerticalAlignment = (i < 3 ? VerticalAlignment.Center : VerticalAlignment.Top);
//                cellStyles[i].WrapText = true;

//                if (i > 0)
//                {
//                    cellStyles[i].BorderBottom = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
//                    cellStyles[i].BorderRight = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
//                    cellStyles[i].BorderTop = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
//                    cellStyles[i].BorderLeft = i < 3 ? BorderStyle.Medium : BorderStyle.Thin;
//                }
//            }

//            //Other CellStyle 6 for Row data in Sat & Sun
//            cellStyles[6].FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
//            cellStyles[6].FillPattern = FillPattern.SolidForeground;

//            return workbook;
//        }

//        private static int CreateHeader(ISheet sheet, ICellStyle[] cellStyles, string title, params string[] columns)
//        {
//            int rIdx = 0;
//            if (columns == null || columns.Length == 0) return 0;

//            if (!string.IsNullOrEmpty(title))
//            {
//                var cellRanged = new NPOI.SS.Util.CellRangeAddress(0, 0, 0, columns.Length - 1);
//                sheet.AddMergedRegion(cellRanged);

//                IRow rCap = sheet.CreateRow(rIdx++);
//                rCap.HeightInPoints = 36f;

//                ICell cell = rCap.CreateCell(0);
//                cell.SetCellValue(title?.ToUpper());
//                cell.CellStyle = cellStyles[0];
//            }

//            IRow row = sheet.CreateRow(rIdx);
//            row.HeightInPoints = 24f;

//            for (int i = 0; i < columns.Length; i++)
//            {
//                ICell cell = row.CreateCell(i);
//                cell.SetCellValue(columns[i]?.ToUpper());
//                cell.CellStyle = cellStyles[1];

//                ICellStyle style = sheet.Workbook.CreateCellStyle();
//                style.Alignment = HorizontalAlignment.Center;
//                if (Uti.StrExists(columns[i], true, "tên", "địa chỉ", "chi tiết", "chuẩn đoán", "liều dùng"))
//                {
//                    style.Alignment = HorizontalAlignment.Left;
//                }
//                if (Uti.StrExists(columns[i], true, "giá", "lượng", "tiền"))
//                {
//                    style.Alignment = HorizontalAlignment.Right;
//                }

//                sheet.SetDefaultColumnStyle(i, style);
//            }

//            return columns.Length;
//        }

//        private static void AppendRow(ISheet sheet, ICellStyle[] cellStyles, int rowIdx = 0, params object[] values)
//        {
//            if (values == null || values.Length == 0) return;

//            IRow row = sheet.CreateRow(rowIdx);
//            for (int i = 0; i < values.Length; i++)
//            {
//                ICell cell = row.CreateCell(i);

//                ICellStyle style = (cellStyles == null || cellStyles.Length < 1 ? null : (cellStyles.Length > 5 ? cellStyles[3] : cellStyles?[0]));
//                ICellStyle oStyle = sheet.GetColumnStyle(i);

//                if (cellStyles != null && cellStyles.Length > 5)
//                {
//                    if (oStyle.Alignment == HorizontalAlignment.Left)
//                    {
//                        style = cellStyles[4];
//                    }
//                    else if (oStyle.Alignment == HorizontalAlignment.Right)
//                    {
//                        style = cellStyles[5];
//                    }
//                }
//                cell.CellStyle = style;

//                string val = $"{values[i]}";
//                if (val.StartsWith("$="))
//                {
//                    cell.SetCellFormula(val[2..]);
//                }
//                else
//                {
//                    cell.SetCellValue(val);
//                }
//            }
//        }

//        #region Linhist removed 19/10/23
//        //private static string GetFormularBacSi(string fmBacsi, ref string MA_BACSI)
//        //{
//        //    if (string.IsNullOrEmpty(MA_BACSI)) return "";

//        //    //Loai tru trng hop MA_BS co 2 chuoi
//        //    MA_BACSI = MA_BACSI.Replace("; ", "$");
//        //    string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
//        //    string[] lstFm = new string[lstBs.Length];
//        //    for (int i = 0; i < lstBs.Length; i++)
//        //    {
//        //        lstBs[i] = lstBs[i].Trim().Replace("$", "; ");
//        //        lstFm[i] = string.Format(fmBacsi, lstBs[i]);
//        //    }

//        //    MA_BACSI = string.Join(Environment.NewLine, lstBs);
//        //    return "$=" + string.Join(" & CHAR(10) & ", lstFm);
//        //}
//        #endregion

//        private static string[] GetFormularBacSi_New(string[] fmBacsis, ref string MA_BACSI)
//        {
//            string[] results = new string[fmBacsis.Length];
//            for (int i = 0; i < results.Length; i++)
//            {
//                results[i] = "";
//            }

//            if (string.IsNullOrWhiteSpace(MA_BACSI)) return results;

//            //Loai tru trng hop MA_BS co 2 chuoi
//            MA_BACSI = MA_BACSI.Replace("; ", "$");
//            string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
//            for (int i = 0; i < lstBs.Length; i++)
//            {
//                lstBs[i] = lstBs[i].Trim().Replace("$", "; ");
//                for (int k = 0; k < fmBacsis.Length; k++)
//                {
//                    results[k] += (i == 0 ? "$=" : " & CHAR(10) & ") + string.Format(fmBacsis[k], lstBs[i]);
//                }
//            }

//            MA_BACSI = string.Join(Environment.NewLine, lstBs);
//            return results;
//        }

//        private static string GetFormularKhoaTH(string fmBacsi, string MA_BACSI)
//        {
//            if (string.IsNullOrWhiteSpace(MA_BACSI)) return "";

//            //Loai tru trng hop MA_BS co 2 chuoi
//            MA_BACSI = MA_BACSI.Replace("; ", "$");
//            string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
//            return "$=" + string.Format(fmBacsi, lstBs.Length > 0 ? lstBs[0] : "");
//        }

//        private static string GetGioiTinh(byte? gioiTinh)
//        {
//            if (gioiTinh == 1) return "Nam";
//            if (gioiTinh == 2) return "Nữ";
//            return "";
//        }

//        private static string GetNgayThang(DateTime? ngayThang, bool noTime = false)
//        {
//            return (noTime ? $"{ngayThang:dd/MM/yyyy}" : $"{ngayThang:dd/MM/yyyy HH:mm:ss}");
//        }

//        private static void SetAutoFeatures(ISheet sheet, int cols, params int[] widens)
//        {
//            for (int i = 0; i < cols; i++)
//            {
//                if (widens != null)
//                {
//                    if (widens.Contains(i))
//                    {
//                        sheet.SetColumnWidth(i, _excelColMild);
//                        continue;
//                    }
//                    else if (widens.Contains(-i))
//                    {
//                        sheet.SetColumnWidth(i, _excelColWide);
//                        continue;
//                    }
//                }

//                sheet.AutoSizeColumn(i);
//            }

//            // Add Auto Filter slow down performance
//            //string ranged = "";
//            //while (cols > 0)
//            //{
//            //    ranged = (char)('A' + cols % 26) + ranged;
//            //    cols /= 26;
//            //}

//            //sheet.SetAutoFilter(CellRangeAddress.ValueOf($"A2:{ranged}2"));
//        }

//        //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//        private static void CreateLookupSheet(IWorkbook workbook, ICellStyle[] cellStyles, IEnumerable<DMBacSi> dmBacsis, IEnumerable<DMKhoaBV> dmKhoas)
//        {
//            int rows = 1, cols = 0;
//            #region Sheet phu: DM Khoa
//            if (dmKhoas != null)
//            {
//                var dataK = dmKhoas.OrderBy(k => k.MA_KHOA);

//                ISheet xlSheetK = workbook.CreateSheet("DM Khoa");
//                rows = 1;
//                cols = CreateHeader(xlSheetK, cellStyles, "", "Mã Khoa", "Tên Khoa");

//                foreach (var x in dataK)
//                {
//                    AppendRow(xlSheetK, cellStyles, rows++, x.MA_KHOA, x.TEN_KHOA);
//                }
//                SetAutoFeatures(xlSheetK, cols);
//            }
//            #endregion

//            #region Sheet phu: DM Bac si
//            if (dmBacsis != null)
//            {
//                var dataBS = dmBacsis.OrderBy(b => b.MACCHN);

//                ISheet xlSheetB = workbook.CreateSheet("DM Bác sĩ");
//                rows = 2;
//                cols = CreateHeader(xlSheetB, cellStyles, "", "Mã CNHN", "Tên Bác sĩ", "Chức danh", "Mã Khoa");

//                foreach (var x in dataBS)
//                {
//                    AppendRow(xlSheetB, cellStyles, rows++, x.MACCHN, x.HO_TEN, LoaiBS(x.CHUCDANH_NN), x.MA_KHOA);
//                }
//                SetAutoFeatures(xlSheetB, cols);
//            }
//            #endregion
//        }

//        #region Linhist removed 19/10/23
//        //Linhist changed 15/11/22
//        //static public byte[] DownloadExcelTotalErrors(IEnumerable<XML1> data1, IEnumerable<DMError> dmErrors, IEnumerable<DMKhoa> dmKhoas, IEnumerable<DMBacSi> dmBacsis,IEnumerable<XML1?> xml1HaveXml4error = null)
//        //static public byte[] DownloadExcelTotalErrors(IEnumerable<XML1> data1, IEnumerable<DMErrorInfoModel> dmErrors, IEnumerable<DMKhoa> dmKhoas, IEnumerable<DMBacSi> dmBacsis, IEnumerable<XML4> allXml4s = null)
//        //{
//        //    try
//        //    {
//        //        //Create new excel
//        //        ICellStyle[] cellStyles = null;
//        //        IWorkbook workbook = CreateWorkBook(out cellStyles);

//        //        var allErrors = data1.SelectMany(x => x.XML1Errors);
//        //        var allErrCodes = allErrors.GroupBy(x => x.XEErrorCode, (k, g) => new { ErrorCode = k, Count = g.Count() });
//        //        var grpErrors = allErrCodes.Join(dmErrors, x => x.ErrorCode, y => y.ErrorCode, (x, y) => new
//        //        {
//        //            y.ErrorCode,
//        //            y.ErrorAtXMLFile,
//        //            y.ErrorAtXMLTag,
//        //            x.Count,
//        //            y.ErrorContent,
//        //            y.IsAlarm
//        //        }).OrderBy(e => e.ErrorAtXMLFile);

//        //        string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
//        //        string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
//        //        string fmKhoaTH = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
//        //        string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";
//        //        string fmError = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:D{(grpErrors.Count() + 5)},3,FALSE),\"\")";
//        //        string fmEtype = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:C{(grpErrors.Count() + 5)},2,FALSE),\"\")";

//        //        byte[] result = null;
//        //        using (var fs = new MemoryStream())
//        //        {
//        //            #region Sheet 1: DS File Bao cao
//        //            var grpFiles = data1.OrderBy(x => x.FileName);
//        //            ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");

//        //            int rows = 2;
//        //            int cols = CreateHeader(xlSheetF, cellStyles,
//        //                        $"Báo Cáo Tổng Hợp {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
//        //                        "STT", "Tên File", "Mã CSKCB",
//        //                        "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính", "Địa chỉ",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Ngày vào", "Ngày ra", "Ngày lập");

//        //            foreach (var x in grpFiles)
//        //            {
//        //                AppendRow(xlSheetF, rows++,
//        //                            rows - 2, x.FileName, x.MA_CSKCB,
//        //                            x.MA_LK, x.MA_BN, x.HO_TEN,
//        //                            x.MA_THE, GetNgayThang(x.NGAY_SINH, true), GetGioiTinh(x.GIOI_TINH), x.DIA_CHI,
//        //                            x.MA_KHOA, string.Format(fmKhoa, x.MA_KHOA),
//        //                            GetNgayThang(x.NGAY_VAO), GetNgayThang(x.NGAY_RA), GetNgayThang(x.CreateDate));
//        //            }

//        //            SetAutoFeatures(xlSheetF, cols, -1, 11);
//        //            #endregion


//        //            #region Sheet 2: Tong hop Loi
//        //            ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Lỗi");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheetE, cellStyles,
//        //                                "Tổng hợp số lỗi trong file hồ sơ", // Sheet title
//        //                                "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi", "Loại File lỗi", "Lỗi tại Thẻ", "Số lỗi");

//        //            foreach (var x2 in grpErrors)
//        //            {
//        //                AppendRow(xlSheetE, rows++,
//        //                            rows - 2, x2.ErrorCode, x2.IsAlarm ? "CẢNH BÁO" : "BÁO LỖI", x2.ErrorContent, x2.ErrorAtXMLFile, x2.ErrorAtXMLTag, x2.Count);
//        //            }

//        //            SetAutoFeatures(xlSheetE, cols, -2);
//        //            #endregion


//        //            #region Sheet 3: Loi XML1
//        //            List<int?> xml1ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML1").Select(e => e.ErrorCode).ToList();
//        //            var xml1Errors = allErrors.Where(a => xml1ErCodes.Contains(a.XEErrorCode));

//        //            ISheet xlSheet1 = workbook.CreateSheet("Lỗi XML1");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheet1, cellStyles,
//        //                        "Danh sách lỗi của XML1", // Sheet title
//        //                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//        //                        "Tổng Chi");

//        //            foreach (var x in xml1Errors)
//        //            {
//        //                AppendRow(xlSheet1, rows++,
//        //                        rows - 2, x.XEErrorCode, string.Format(fmEtype, x.XEErrorCode), string.Format(fmError, x.XEErrorCode),
//        //                        x.XEXML1.MA_LK, x.XEXML1.MA_BN, x.XEXML1.HO_TEN,
//        //                        x.XEXML1.MA_THE, GetNgayThang(x.XEXML1.NGAY_SINH, true), GetGioiTinh(x.XEXML1.GIOI_TINH),
//        //                        x.XEXML1.MA_KHOA, string.Format(fmKhoa, x.XEXML1.MA_KHOA),
//        //                        x.XEXML1.MA_BENH, x.XEXML1.MA_BENHKHAC, x.XEXML1.TEN_BENH,
//        //                        GetNgayThang(x.XEXML1.NGAY_VAO), GetNgayThang(x.XEXML1.NGAY_RA), GetNgayThang(x.XEXML1.NGAY_TTOAN),
//        //                        string.Format("{0:##,##0.0#}", x.XEXML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//        //            }

//        //            SetAutoFeatures(xlSheet1, cols, 2, -3, 11, -14);
//        //            #endregion


//        //            #region Sheet 4: Loi XML2
//        //            List<int?> xml2ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML2").Select(e => e.ErrorCode).ToList();
//        //            var xml2Errors = allErrors.Where(a => xml2ErCodes.Contains(a.XEErrorCode));

//        //            var data2 =
//        //                data1.SelectMany(x => x.XML2s)
//        //                .Join(xml2Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml2 = x, XmlE = e })
//        //                .Where(x => x.Xml2.STT == x.XmlE.XEErrorRow);

//        //            ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML2");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheet2, cellStyles,
//        //                        "Danh sách lỗi của XML2", // Sheet title
//        //                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//        //                        "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
//        //                        "TT Thầu", "Đơn giá", "Số lượng", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
//        //                        "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//        //                        "Tổng Chi");

//        //            foreach (var x in data2)
//        //            {
//        //                string maBS = x.Xml2.MA_BAC_SI;
//        //                string tenBS = GetFormularBacSi(fmBacsi, ref maBS);
//        //                string chucVu = GetFormularBacSi(fmChucvu, ref maBS);
//        //                AppendRow(xlSheet2, rows++,
//        //                        rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//        //                        x.Xml2.XML1.MA_LK, x.Xml2.XML1.MA_BN, x.Xml2.XML1.HO_TEN,
//        //                        x.Xml2.XML1.MA_THE, GetNgayThang(x.Xml2.XML1.NGAY_SINH, true), GetGioiTinh(x.Xml2.XML1.GIOI_TINH),
//        //                        x.Xml2.XML1.MA_KHOA, string.Format(fmKhoa, x.Xml2.XML1.MA_KHOA),
//        //                        maBS, tenBS, chucVu, x.Xml2.MA_KHOA,
//        //                        x.Xml2.XML1.MA_BENH, x.Xml2.XML1.MA_BENHKHAC, x.Xml2.XML1.TEN_BENH,
//        //                        x.Xml2.MA_THUOC, x.Xml2.MA_NHOM, x.Xml2.TEN_THUOC, x.Xml2.DON_VI_TINH, x.Xml2.LIEU_DUNG, x.Xml2.SO_DANG_KY,
//        //                        x.Xml2.TT_THAU, x.Xml2.DON_GIA, x.Xml2.SO_LUONG, x.Xml2.MUC_HUONG, x.Xml2.TYLE_TT, x.Xml2.THANH_TIEN,
//        //                        GetNgayThang(x.Xml2.NGAY_YL), GetNgayThang(x.Xml2.XML1.NGAY_VAO), GetNgayThang(x.Xml2.XML1.NGAY_RA), GetNgayThang(x.Xml2.XML1.NGAY_TTOAN),
//        //                        string.Format("{0:##,##0.0#}", x.Xml2.XML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//        //            }

//        //            SetAutoFeatures(xlSheet2, cols, 2, -3, 11, 13, 14, -18);
//        //            #endregion


//        //            #region Sheet 5: Loi XML3
//        //            var data3 = data1.SelectMany(x => x.XML3s);
//        //            List<int?> xml3ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML3").Select(e => e.ErrorCode).ToList();
//        //            var xml3Errors = allErrors.Where(a => xml3ErCodes.Contains(a.XEErrorCode));

//        //            var data3Join =
//        //                data3.Join(xml3Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml3 = x, XmlE = e })
//        //                .Where(x => x.Xml3.STT == x.XmlE.XEErrorRow);

//        //            ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML3");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheet3, cellStyles,
//        //                        "Danh sách lỗi của XML3", // Sheet title
//        //                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh", "Khoa Thực hiện",
//        //                        "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
//        //                        "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//        //                        "Tổng Chi");

//        //            foreach (var x in data3Join)
//        //            {
//        //                string maBS = x.Xml3.MA_BAC_SI;
//        //                string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
//        //                string tenBS = GetFormularBacSi(fmBacsi, ref maBS);
//        //                string chucVu = GetFormularBacSi(fmChucvu, ref maBS);
//        //                AppendRow(xlSheet3, rows++,
//        //                        rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//        //                        x.Xml3.XML1.MA_LK, x.Xml3.XML1.MA_BN, x.Xml3.XML1.HO_TEN,
//        //                        x.Xml3.XML1.MA_THE, GetNgayThang(x.Xml3.XML1.NGAY_SINH, true), GetGioiTinh(x.Xml3.XML1.GIOI_TINH),
//        //                        x.Xml3.XML1.MA_KHOA, string.Format(fmKhoa, x.Xml3.XML1.MA_KHOA),
//        //                        maBS, tenBS, chucVu, x.Xml3.MA_KHOA, tenKTH,
//        //                        x.Xml3.MA_DICH_VU, x.Xml3.TEN_DICH_VU, x.Xml3.MA_VAT_TU, x.Xml3.TEN_VAT_TU,
//        //                        x.Xml3.XML1.MA_BENH, x.Xml3.XML1.MA_BENHKHAC, x.Xml3.XML1.TEN_BENH,
//        //                        x.Xml3.DON_VI_TINH, x.Xml3.TT_THAU, x.Xml3.DON_GIA, x.Xml3.MUC_HUONG, x.Xml3.TYLE_TT, x.Xml3.THANH_TIEN,
//        //                        GetNgayThang(x.Xml3.NGAY_YL), GetNgayThang(x.Xml3.NGAY_KQ), GetNgayThang(x.Xml3.XML1.NGAY_VAO), GetNgayThang(x.Xml3.XML1.NGAY_RA), GetNgayThang(x.Xml3.XML1.NGAY_TTOAN),
//        //                        string.Format("{0:##,##0.0#}", x.Xml3.XML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//        //            }

//        //            SetAutoFeatures(xlSheet3, cols, 2, -3, 11, 13, 14, 16, -23);
//        //            #endregion


//        //            #region Sheet 6: Loi XML4
//        //            //Linhist changed 15/11/22
//        //            //var data4 = xml1HaveXml4error.SelectMany(x => x.XML4s);
//        //            //List<int?> xml4ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//        //            //var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.XEErrorCode));
//        //            var data4 = allXml4s;
//        //            List<int?> xml4ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//        //            var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.XEErrorCode));

//        //            var data4Join =
//        //                data4.Join(xml4Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml4 = x, XmlE = e })
//        //                .Where(x => x.Xml4.STT == x.XmlE.XEErrorRow).OrderBy(x => x.Xml4.XML1ID);

//        //            ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML4");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheet4, cellStyles,
//        //                        "Danh sách lỗi của XML4", // Sheet title
//        //                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Mã Dịch vụ", "Mã Máy", "Giá trị",
//        //                        "Mã Chỉ số", "Tên Chỉ số",
//        //                        "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//        //                        "Tổng Chi");

//        //            XML1 xml1 = null;
//        //            foreach (var x in data4Join)
//        //            {
//        //                if (xml1 == null || xml1.XML1ID != x.Xml4.XML1ID)
//        //                {
//        //                    xml1 = data1.FirstOrDefault(x1 => x1.XML1ID == x.Xml4.XML1ID);
//        //                }

//        //                AppendRow(xlSheet4, rows++,
//        //                        rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//        //                        xml1?.MA_LK, xml1?.MA_BN, xml1?.HO_TEN,
//        //                        xml1?.MA_KHOA, string.Format(fmKhoa, xml1?.MA_KHOA),
//        //                        xml1?.MA_BENH, xml1?.MA_BENHKHAC, xml1?.TEN_BENH,
//        //                        x.Xml4.MA_DICH_VU, x.Xml4.MA_MAY, x.Xml4.GIA_TRI,
//        //                        x.Xml4.MA_CHI_SO, x.Xml4.TEN_CHI_SO,
//        //                        GetNgayThang(x.Xml4.NGAY_KQ), GetNgayThang(xml1?.NGAY_VAO), GetNgayThang(xml1?.NGAY_RA), GetNgayThang(xml1?.NGAY_TTOAN),
//        //                        string.Format("{0:##,##0.0#}", xml1?.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//        //            }

//        //            SetAutoFeatures(xlSheet4, cols, 2, -3, 7);
//        //            #endregion


//        //            #region Sheet 7: Loi XML5
//        //            var data5 = data1.SelectMany(x => x.XML5s);
//        //            List<int?> xml5ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML5").Select(e => e.ErrorCode).ToList();
//        //            var xml5Errors = allErrors.Where(a => xml5ErCodes.Contains(a.XEErrorCode));

//        //            var data5Join =
//        //                data5.Join(xml5Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml5 = x, XmlE = e })
//        //                .Where(x => x.Xml5.STT == x.XmlE.XEErrorRow);

//        //            ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML5");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheet5, cellStyles,
//        //                        "Danh sách lỗi của XML5", // Sheet title
//        //                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//        //                        "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Khoa", "Tên Khoa",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//        //                        "Tổng Chi");

//        //            foreach (var x in data5Join)
//        //            {
//        //                AppendRow(xlSheet5, rows++,
//        //                        rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//        //                        x.Xml5.XML1.MA_LK, x.Xml5.XML1.MA_BN, x.Xml5.XML1.HO_TEN,
//        //                        x.Xml5.XML1.MA_KHOA, string.Format(fmKhoa, x.Xml5.XML1.MA_KHOA),
//        //                        x.Xml5.XML1.MA_BENH, x.Xml5.XML1.MA_BENHKHAC, x.Xml5.XML1.TEN_BENH,
//        //                        GetNgayThang(x.Xml5.NGAY_YL), GetNgayThang(x.Xml5.XML1.NGAY_VAO), GetNgayThang(x.Xml5.XML1.NGAY_RA), GetNgayThang(x.Xml5.XML1.NGAY_TTOAN),
//        //                        string.Format("{0:##,##0.0#}", x.Xml5.XML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//        //            }

//        //            SetAutoFeatures(xlSheet5, cols, 2, -3, 8);
//        //            #endregion

//        //            CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

//        //            workbook.Write(fs);
//        //            result = fs.ToArray();
//        //        }

//        //        return result;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string stk = ex.StackTrace;
//        //    }

//        //    return null;
//        //}

//        //static public byte[] DownloadExcelDublicated(IEnumerable<XML1> data1, IEnumerable<DMKhoa> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
//        //{
//        //    try
//        //    {
//        //        //Create new excel
//        //        ICellStyle[] cellStyles = null;
//        //        IWorkbook workbook = CreateWorkBook(out cellStyles);

//        //        string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
//        //        string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
//        //        string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";

//        //        var allXMl3 = data1.SelectMany(x => x.XML3s);

//        //        byte[] result = null;
//        //        using (var fs = new MemoryStream())
//        //        {
//        //            #region Sheet 1: Loi BS DV Trung TG - Nhom 8
//        //            //Linhist added 08/06/22 bo qua ma dich vu ko kiem tra loi nay
//        //            //var dataNhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18) && !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0133.0209;01.0175.0196;02.0209.0194") && x.NGAY_YL != null && x.NGAY_KQ != null);
//        //            var dataNhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18) && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null &&
//        //                                                !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232"));

//        //            var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//        //                                    .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //            ISheet xlSheetG8 = workbook.CreateSheet("Trùng Nhóm 8");
//        //            int rows = 2;
//        //            int cols = CreateHeader(xlSheetG8, cellStyles,
//        //                        "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8", // Sheet title
//        //                        "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//        //                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//        //                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//        //                        "Mã Khoa 1", "Tên Khoa 1",
//        //                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                        "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//        //                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//        //                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//        //                        "Mã Khoa 2", "Tên Khoa 2",
//        //                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//        //            foreach (var t in xml3Nhom8Trung)
//        //            {
//        //                string maBS1 = t.x1.MA_BAC_SI;
//        //                string tenBS1 = GetFormularBacSi(fmBacsi, ref maBS1);
//        //                string chucVu1 = GetFormularBacSi(fmChucvu, ref maBS1);
//        //                string maBS2 = t.x2.MA_BAC_SI;
//        //                string tenBS2 = GetFormularBacSi(fmBacsi, ref maBS2);
//        //                string chucVu2 = GetFormularBacSi(fmChucvu, ref maBS2);

//        //                AppendRow(xlSheetG8, rows++,
//        //                        rows - 2, t.x1.MA_LK, maBS1, tenBS1,
//        //                        t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN, chucVu1,
//        //                        t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//        //                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
//        //                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//        //                        t.x2.MA_LK, maBS2, tenBS2, chucVu2,
//        //                        t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//        //                        t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//        //                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
//        //                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL)
//        //                    );
//        //            }

//        //            SetAutoFeatures(xlSheetG8, cols, 3, 4, 11, 18, 19, 26);
//        //            #endregion

//        //            #region Sheet 2: Loi BS DV Trung TG - Nhom 2
//        //            var dataNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //            var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//        //                                    .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //            ISheet xlSheetG2 = workbook.CreateSheet("Trùng Nhóm 2");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheetG2, cellStyles,
//        //                                "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8", // Sheet title
//        //                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//        //                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//        //                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//        //                                "Mã Khoa 1", "Tên Khoa 1",
//        //                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//        //                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//        //                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//        //                                "Mã Khoa 2", "Tên Khoa 2",
//        //                                "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//        //            foreach (var t in xml3Nhom2Trung)
//        //            {
//        //                string maBS1 = t.x1.MA_BAC_SI;
//        //                string tenBS1 = GetFormularBacSi(fmBacsi, ref maBS1);
//        //                string chucVu1 = GetFormularBacSi(fmChucvu, ref maBS1);
//        //                string maBS2 = t.x2.MA_BAC_SI;
//        //                string tenBS2 = GetFormularBacSi(fmBacsi, ref maBS2);
//        //                string chucVu2 = GetFormularBacSi(fmChucvu, ref maBS2);

//        //                AppendRow(xlSheetG2, rows++,
//        //                        rows - 2, t.x1.MA_LK, maBS1, tenBS1, chucVu1,
//        //                        t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//        //                        t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//        //                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
//        //                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//        //                        t.x2.MA_LK, maBS2, tenBS2, chucVu2,
//        //                        t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//        //                        t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//        //                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
//        //                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL)
//        //                    );
//        //            }

//        //            SetAutoFeatures(xlSheetG2, cols, 3, 4, 11, 18, 19, 26);
//        //            #endregion

//        //            //Linhist added 03/09/22
//        //            #region Sheet 3: Loi DV Trung Ngay Y Lenh cua Noi Tru & Ngoai Tru (loc loai KCB & loai tru vat tu hay giuong benh)

//        //            #region Linhist removed 20/09/22
//        //            //var groupYLs = allXMl3.Where(x => x.XML1.MA_LOAI_KCB > 1 && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15)).GroupBy(x => (x.NGAY_YL, x.MA_BAC_SI));

//        //            //var xml3TrungYL = groupYLs.Where(g => g.Key.NGAY_YL != null && g.Key.NGAY_YL.HasValue && g.Count() > 1 && g.Select(x => x.MA_LK).Distinct().Count() > 1)
//        //            //                            .SelectMany(g => g.ToList()).OrderBy(x => x.NGAY_YL);

//        //            //ISheet xlSheetYL = workbook.CreateSheet("Trùng Y Lệnh");
//        //            //rows = 2;
//        //            //cols = CreateHeader(xlSheetYL, cellStyles,
//        //            //            "Danh sách Dịch vụ Nội - Ngoại trú trùng thời gian y lệnh", // Sheet title
//        //            //            "STT", "Mã lượt khám", "Mã Bác sĩ", "Tên Bác sĩ",
//        //            //            "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //            //            "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //            //            "Mã Khoa", "Tên Khoa",
//        //            //            "Mã Dịch vụ", "Tên Dịch vụ", "Ngày Y lệnh", "Ngày Kết quả");

//        //            //foreach (var t in xml3TrungYL)
//        //            //{
//        //            //    string maBS = t.MA_BAC_SI;
//        //            //    string tenBS = GetFormularBacSi(fmBacsi, ref maBS);

//        //            //    AppendRow(xlSheetYL, rows++,
//        //            //            rows - 2, t.MA_LK, maBS, tenBS,
//        //            //            t.XML1.MA_BN, t.XML1.HO_TEN,
//        //            //            t.XML1.MA_THE, GetNgayThang(t.XML1.NGAY_SINH, true), GetGioiTinh(t.XML1.GIOI_TINH),
//        //            //            t.MA_KHOA, string.Format(fmKhoa, t.MA_KHOA),
//        //            //            t.MA_DICH_VU, t.TEN_DICH_VU, GetNgayThang(t.NGAY_YL), GetNgayThang(t.NGAY_KQ)
//        //            //        );
//        //            //}

//        //            //SetAutoFeatures(xlSheetYL, cols, 3, 10);
//        //            #endregion

//        //            var dataTrungYL = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15) && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null);

//        //            //var xml3TrungYL = dataTrungYL.Join(dataTrungYL,
//        //            //                                    x1 => x1.MA_BAC_SI.Substring(0, (x1.MA_BAC_SI.IndexOf(";") >= 0 ? x1.MA_BAC_SI.IndexOf(";") : x1.MA_BAC_SI.Length)),
//        //            //                                    x2 => x2.MA_BAC_SI.Substring(0, (x2.MA_BAC_SI.IndexOf(";") >= 0 ? x2.MA_BAC_SI.IndexOf(";") : x2.MA_BAC_SI.Length)),
//        //            //                                    (x1, x2) => new { x1, x2 })
//        //            //                        .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL);

//        //            //haimaster update 18/10/2022
//        //            var xml3TrungYL = dataTrungYL.Join(dataTrungYL,
//        //                                                x1 => x1.NGAY_YL,
//        //                                                x2 => x2.NGAY_YL,
//        //                                                (x1, x2) => new { x1, x2 })
//        //                                    //Linhist changed 26/10/22 sua ham kiem tra ma bac si dau tien
//        //                                    //.Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && ((a.x1.MA_BAC_SI.Contains(a.x2.MA_BAC_SI))||(a.x2.MA_BAC_SI.Contains(a.x1.MA_BAC_SI))));
//        //                                    .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && Uti.FirstMatch(a.x1.MA_BAC_SI, a.x2.MA_BAC_SI));
//        //            //Them case kiem tra A.ma_bacsi like B.ma_bacsi (vd: select * from xml3 where XML3ID in ('44469994','43771413'))

//        //            ISheet xlSheetYL = workbook.CreateSheet("Trùng Y Lệnh");
//        //            rows = 2;
//        //            //Linhist changed 26/10/22
//        //            //cols = CreateHeader(xlSheetYL, cellStyles,
//        //            //            "Danh sách Dịch vụ Nội - Ngoại trú trùng thời gian y lệnh", // Sheet title
//        //            //            "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1",
//        //            //            "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//        //            //            "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//        //            //            "Mã Khoa 1", "Tên Khoa 1",
//        //            //            "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //            //            "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2",
//        //            //            "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//        //            //            "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//        //            //            "Mã Khoa 2", "Tên Khoa 2",
//        //            //            "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");
//        //            cols = CreateHeader(xlSheetYL, cellStyles,
//        //                        "Danh sách Dịch vụ Nội - Ngoại trú trùng thời gian y lệnh", // Sheet title
//        //                        "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1", "Mã Bệnh nhân 1",
//        //                        "Mã Khoa 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1",
//        //                        "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 1", "Chức danh 1", "Mã Bệnh nhân 2",
//        //                        "Mã Khoa 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

//        //            foreach (var t in xml3TrungYL)
//        //            {
//        //                string maBS1 = t.x1.MA_BAC_SI;
//        //                string tenBS1 = GetFormularBacSi(fmBacsi, ref maBS1);
//        //                string chucVu1 = GetFormularBacSi(fmChucvu, ref maBS1);
//        //                string maBS2 = t.x2.MA_BAC_SI;
//        //                string tenBS2 = GetFormularBacSi(fmBacsi, ref maBS2);
//        //                string chucVu2 = GetFormularBacSi(fmChucvu, ref maBS2);

//        //                //Linhist changed 26/10/22
//        //                //AppendRow(xlSheetYL, rows++,
//        //                //        rows - 2, t.x1.MA_LK, maBS1, tenBS1,
//        //                //        t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//        //                //        t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//        //                //        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
//        //                //        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//        //                //        t.x2.MA_LK, maBS2, tenBS2,
//        //                //        t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//        //                //        t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//        //                //        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
//        //                //        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL)
//        //                //    );
//        //                AppendRow(xlSheetYL, rows++,
//        //                        rows - 2, t.x1.MA_LK, maBS1, tenBS1, chucVu1, t.x1.XML1.MA_BN,
//        //                        t.x1.MA_KHOA, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL),
//        //                        t.x2.MA_LK, maBS2, tenBS2, chucVu2, t.x2.XML1.MA_BN,
//        //                        t.x2.MA_KHOA, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL)
//        //                    );
//        //            }

//        //            //Linhist changed 26/10/22
//        //            //SetAutoFeatures(xlSheetYL, cols, 3, 10, 17, 24);
//        //            SetAutoFeatures(xlSheetYL, cols, 3, 4, 10, 11);
//        //            #endregion

//        //            #region Sheet 3: Loi Trung Giuong
//        //            var dataGiuong = allXMl3.Where(x => (x.MA_NHOM == 14 || x.MA_NHOM == 15) && !string.IsNullOrEmpty(x.MA_GIUONG) && x.NGAY_YL != null && x.NGAY_KQ != null);

//        //            var xml3GiuongTrung = dataGiuong.Join(dataGiuong, x1 => x1.MA_GIUONG, x2 => x2.MA_GIUONG, (x1, x2) => new { x1, x2 })
//        //                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//        //            ISheet xlSheetGG = workbook.CreateSheet("Trùng Giường");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheetGG, cellStyles,
//        //                        "Danh sách Dịch vụ Giường trùng thời gian", // Sheet caption
//        //                        "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//        //                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//        //                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//        //                        "Mã Khoa 1", "Tên Khoa 1",
//        //                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Giường 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                        "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 1",
//        //                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//        //                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//        //                        "Mã Khoa 2", "Tên Khoa 2",
//        //                        "Mã Dịch vụ 1", "Tên Dịch vụ 2", "Giường 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//        //            foreach (var t in xml3GiuongTrung)
//        //            {
//        //                string maBS1 = t.x1.MA_BAC_SI;
//        //                string tenBS1 = GetFormularBacSi(fmBacsi, ref maBS1);
//        //                string chucVu1 = GetFormularBacSi(fmChucvu, ref maBS1);
//        //                string maBS2 = t.x2.MA_BAC_SI;
//        //                string tenBS2 = GetFormularBacSi(fmBacsi, ref maBS2);
//        //                string chucVu2 = GetFormularBacSi(fmChucvu, ref maBS2);

//        //                AppendRow(xlSheetGG, rows++,
//        //                    rows - 2, t.x1.MA_LK, maBS1, tenBS1, chucVu1,
//        //                    t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//        //                    t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//        //                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
//        //                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, t.x1.MA_GIUONG, GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//        //                    t.x2.MA_LK, maBS2, tenBS2, chucVu2,
//        //                    t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//        //                    t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//        //                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
//        //                    t.x2.MA_KHOA, t.x2.TEN_DICH_VU, t.x2.MA_GIUONG, GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL));
//        //            }

//        //            SetAutoFeatures(xlSheetGG, cols, 3, 4, 11, 19, 20, 27);
//        //            #endregion


//        //            //Linhist added 26/10/22
//        //            #region Sheet 3: Loi Trung Ngay Y Lenh cua Thuoc & DV
//        //            var xml2TRG = data1.SelectMany(x => x.XML2s).Where(x => x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));
//        //            var xml3TRG = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15) && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));

//        //            var allTRG_YL = xml2TRG.Join(xml3TRG, x2 => x2.NGAY_YL,
//        //                                                x3 => x3.NGAY_YL,
//        //                                                (x2, x3) => new { x2, x3 })
//        //                                    .Where(a => a.x2.MA_LK != a.x3.MA_LK && Uti.FirstMatch(a.x2.MA_BAC_SI, a.x3.MA_BAC_SI));

//        //            ISheet xlSheetTRG = workbook.CreateSheet("Trùng Thuốc DV");
//        //            rows = 2;
//        //            //Linhist changed 26/10/22
//        //            //cols = CreateHeader(xlSheetTRG, cellStyles,
//        //            //            "Danh sách trùng thời điểm bác sĩ y lệnh Thuốc với Dịch vụ", // Sheet title
//        //            //            "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1",
//        //            //            "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//        //            //            "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//        //            //            "Mã Khoa 1", "Tên Khoa 1",
//        //            //            "Mã Thuốc 1", "Tên Thuốc 1", "Ngày Y lệnh 1",
//        //            //            "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2",
//        //            //            "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//        //            //            "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//        //            //            "Mã Khoa 2", "Tên Khoa 2",
//        //            //            "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");
//        //            cols = CreateHeader(xlSheetTRG, cellStyles,
//        //                        "Danh sách trùng thời điểm bác sĩ y lệnh Thuốc với Dịch vụ", // Sheet title
//        //                        "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1", "Mã Bệnh nhân 1",
//        //                        "Mã Khoa 1", "Tên Thuốc 1", "Ngày Y lệnh 1",
//        //                        "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 1", "Chức danh 1", "Mã Bệnh nhân 2",
//        //                        "Mã Khoa 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

//        //            foreach (var t in allTRG_YL)
//        //            {
//        //                string maBS1 = t.x2.MA_BAC_SI;
//        //                string tenBS1 = GetFormularBacSi(fmBacsi, ref maBS1);
//        //                string chucVu1 = GetFormularBacSi(fmChucvu, ref maBS1);
//        //                string maBS2 = t.x3.MA_BAC_SI;
//        //                string tenBS2 = GetFormularBacSi(fmBacsi, ref maBS2);
//        //                string chucVu2 = GetFormularBacSi(fmChucvu, ref maBS2);

//        //                //Linhist changed 26/10/22
//        //                //AppendRow(xlSheetTRG, rows++,
//        //                //        rows - 2, t.x2.MA_LK, maBS1, tenBS1,
//        //                //        t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//        //                //        t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//        //                //        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
//        //                //        t.x2.MA_THUOC, t.x2.TEN_THUOC, GetNgayThang(t.x2.NGAY_YL),
//        //                //        t.x3.MA_LK, maBS2, tenBS2,
//        //                //        t.x3.XML1.MA_BN, t.x3.XML1.HO_TEN,
//        //                //        t.x3.XML1.MA_THE, GetNgayThang(t.x3.XML1.NGAY_SINH, true), GetGioiTinh(t.x3.XML1.GIOI_TINH),
//        //                //        t.x3.MA_KHOA, string.Format(fmKhoa, t.x3.MA_KHOA),
//        //                //        t.x3.MA_DICH_VU, t.x3.TEN_DICH_VU, GetNgayThang(t.x3.NGAY_YL)
//        //                //    );
//        //                AppendRow(xlSheetTRG, rows++,
//        //                        rows - 2, t.x2.MA_LK, maBS1, tenBS1, chucVu1, t.x2.XML1.MA_BN,
//        //                        t.x2.MA_KHOA, t.x2.TEN_THUOC, GetNgayThang(t.x2.NGAY_YL),
//        //                        t.x3.MA_LK, maBS2, tenBS2, chucVu2, t.x3.XML1.MA_BN,
//        //                        t.x3.MA_KHOA, t.x3.TEN_DICH_VU, GetNgayThang(t.x3.NGAY_YL)
//        //                    );
//        //            }

//        //            //Linhist changed 26/10/22
//        //            //SetAutoFeatures(xlSheetTRG, cols, 3, 10, 16, 23);
//        //            SetAutoFeatures(xlSheetTRG, cols, 3, 4, 11, 12);
//        //            #endregion

//        //            CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

//        //            workbook.Write(fs);
//        //            result = fs.ToArray();
//        //        }

//        //        return result;

//        //    }
//        //    catch { }

//        //    return null;
//        //}
//        //static public byte[] DownloadExcelResources(IEnumerable<XML1> data1, IEnumerable<DMKhoa> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
//        //{
//        //    try
//        //    {
//        //        //Create new excel
//        //        ICellStyle[] cellStyles = null;
//        //        IWorkbook workbook = CreateWorkBook(out cellStyles);

//        //        var data2 = data1.SelectMany(x => x.XML2s);
//        //        var data3 = data1.SelectMany(x => x.XML3s);

//        //        string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
//        //        string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";

//        //        byte[] result = null;
//        //        using (var fs = new MemoryStream())
//        //        {
//        //            #region Sheet 1: Thuoc XML2
//        //            ISheet xlSheet2 = workbook.CreateSheet("DS Thuốc");
//        //            int rows = 2;
//        //            int cols = CreateHeader(xlSheet2, cellStyles,
//        //                        "Danh sách tổng hợp Thuốc XML2", // Sheet title
//        //                        "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
//        //                        "TT Thầu", "Đơn giá", "Số lượng", "Tỷ lệ TT", "Thành tiền",
//        //                        "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //            foreach (var x2 in data2)
//        //            {
//        //                string maBS = x2.MA_BAC_SI;
//        //                string tenBS = GetFormularBacSi(fmBacsi, ref maBS);
//        //                AppendRow(xlSheet2, rows++,
//        //                        rows - 2, x2.XML1.MA_LK, x2.XML1.MA_BN, x2.XML1.HO_TEN,
//        //                        x2.XML1.MA_THE, GetNgayThang(x2.XML1.NGAY_SINH, true), GetGioiTinh(x2.XML1.GIOI_TINH),
//        //                        x2.MA_KHOA, string.Format(fmKhoa, x2.MA_KHOA), maBS, tenBS,
//        //                        x2.XML1.MA_BENH, x2.XML1.MA_BENHKHAC, x2.XML1.TEN_BENH,
//        //                        x2.MA_THUOC, x2.MA_NHOM, x2.TEN_THUOC, x2.DON_VI_TINH, x2.LIEU_DUNG, x2.SO_DANG_KY,
//        //                        x2.TT_THAU, x2.DON_GIA, x2.SO_LUONG, x2.TYLE_TT, x2.THANH_TIEN,
//        //                        GetNgayThang(x2.NGAY_YL), GetNgayThang(x2.XML1.NGAY_VAO), GetNgayThang(x2.XML1.NGAY_RA), GetNgayThang(x2.XML1.NGAY_TTOAN));
//        //            }

//        //            SetAutoFeatures(xlSheet2, cols, 8, 10, -13);
//        //            #endregion


//        //            #region Sheet 2: Dich vu XML3
//        //            var data3DVs = data3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.NumExists(x.MA_NHOM, 10, 11));

//        //            ISheet xlSheetDV = workbook.CreateSheet("DS Dịch vụ");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheetDV, cellStyles,
//        //                        "Danh sách tổng hợp Dịch vụ XML3", // Sheet caption
//        //                        "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Mã Dịch vụ", "Tên Dịch vụ",
//        //                        "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
//        //                        "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //            foreach (var x3 in data3DVs)
//        //            {
//        //                string maBS = x3.MA_BAC_SI;
//        //                string tenBS = GetFormularBacSi(fmBacsi, ref maBS);
//        //                AppendRow(xlSheetDV, rows++,
//        //                        rows - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//        //                        x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//        //                        x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, tenBS,
//        //                        x3.XML1.MA_BENH, x3.XML1.MA_BENHKHAC, x3.XML1.TEN_BENH,
//        //                        x3.MA_DICH_VU, x3.TEN_DICH_VU,
//        //                        x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA, x3.TYLE_TT, x3.THANH_TIEN,
//        //                        GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML1.NGAY_VAO), GetNgayThang(x3.XML1.NGAY_RA), GetNgayThang(x3.XML1.NGAY_TTOAN));
//        //            }
//        //            SetAutoFeatures(xlSheetDV, cols, 8, 10, -13);
//        //            #endregion


//        //            #region Sheet 3: Vat tu XML3
//        //            var data3VTs = data3.Where(x => !string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.NumExists(x.MA_NHOM, 10, 11));

//        //            ISheet xlSheetVT = workbook.CreateSheet("DS VTYT");
//        //            rows = 2;
//        //            cols = CreateHeader(xlSheetVT, cellStyles, "Danh sách tổng hợp Vật tư XML3",
//        //                        "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ",
//        //                        "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//        //                        "Mã Vật tư", "Tên Vật tư",
//        //                        "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
//        //                        "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//        //            foreach (var x3 in data3VTs)
//        //            {
//        //                string maBS = x3.MA_BAC_SI;
//        //                string tenBS = GetFormularBacSi(fmBacsi, ref maBS);
//        //                AppendRow(xlSheetVT, rows++,
//        //                        rows - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//        //                        x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//        //                        x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, tenBS,
//        //                        x3.XML1.MA_BENH, x3.XML1.MA_BENHKHAC, x3.XML1.TEN_BENH,
//        //                        x3.MA_VAT_TU, x3.TEN_VAT_TU,
//        //                        x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA, x3.TYLE_TT, x3.THANH_TIEN,
//        //                        GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML1.NGAY_VAO), GetNgayThang(x3.XML1.NGAY_RA), GetNgayThang(x3.XML1.NGAY_TTOAN));
//        //            }

//        //            SetAutoFeatures(xlSheetVT, cols, 8, 10, -13);
//        //            #endregion

//        //            CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

//        //            workbook.Write(fs);
//        //            result = fs.ToArray();
//        //        }

//        //        return result;

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string stk = ex.StackTrace;
//        //    }

//        //    return null;
//        //}
//        #endregion

//        //Linhist added 27/02/23
//        //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//        static public byte[] DownloadExcelDublicated_New(IEnumerable<XML2> data2, IEnumerable<XML3> data3, IEnumerable<DMKhoaBV> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
//        {
//            try
//            {
//                //Create new excel
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
//                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
//                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";

//                var allXMl3 = data3;

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    #region Sheet 1: Loi BS DV Trung TG - Nhom 8
//                    var dataNhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041;03.3826.2047")) &&
//                                                        !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null &&
//                                                        !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232"));

//                    //var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//                    //                        .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);
//                    var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//                                            .Where(a =>  a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//                    ISheet xlSheetG8 = workbook.CreateSheet("Trùng Nhóm 8 - 18");
//                    int rows = 2;
//                    int cols = CreateHeader(xlSheetG8, cellStyles,
//                                "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8 - 18", // Sheet title
//                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//                                "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
//                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//                                "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
//                                "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//                    foreach (var t in xml3Nhom8Trung)
//                    {
//                        string maBS1 = t.x1.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x2.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetG8, cellStyles, rows++,
//                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//                                t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
//                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//                                t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
//                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL)
//                            );
//                    }

//                    SetAutoFeatures(xlSheetG8, cols, 3, 4, 11, 18, 19, 26);
//                    #endregion

//                    #region Sheet 2: Loi BS DV Trung TG - Nhom 2
//                    var dataNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null);

//                    var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, x1 => x1.MA_BAC_SI, x2 => x2.MA_BAC_SI, (x1, x2) => new { x1, x2 })
//                                            .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL);

//                    ISheet xlSheetG2 = workbook.CreateSheet("Trùng Nhóm 2");
//                    rows = 2;
//                    cols = CreateHeader(xlSheetG2, cellStyles,
//                                        "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 2", // Sheet title
//                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
//                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//                                        "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
//                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//                    foreach (var t in xml3Nhom2Trung)
//                    {
//                        string maBS1 = t.x1.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x2.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetG2, cellStyles, rows++,
//                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//                                t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//                                t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
//                                t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//                                t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//                                t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
//                                t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL)
//                            );
//                    }

//                    SetAutoFeatures(xlSheetG2, cols, 3, 4, 11, 18, 19, 26);
//                    #endregion

//                    #region Sheet 3: Loi DV Trung Ngay Y Lenh cua Noi Tru & Ngoai Tru (loc loai KCB & loai tru vat tu hay giuong benh)

//                    var dataTrungYL = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15) && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null);
//                    var xml3TrungYL = dataTrungYL.Join(dataTrungYL,
//                                                        x1 => x1.NGAY_YL,
//                                                        x2 => x2.NGAY_YL,
//                                                        (x1, x2) => new { x1, x2 })
//                                            .Where(a => a.x1.MA_DICH_VU != a.x2.MA_DICH_VU && a.x1.MA_LK != a.x2.MA_LK && Uti.FirstMatch(a.x1.MA_BAC_SI, a.x2.MA_BAC_SI));

//                    ISheet xlSheetYL = workbook.CreateSheet("Trùng Y Lệnh");
//                    rows = 2;
//                    cols = CreateHeader(xlSheetYL, cellStyles,
//                                "Danh sách Dịch vụ Nội - Ngoại trú trùng thời gian y lệnh", // Sheet title
//                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1",
//                                "Mã Khoa 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1",
//                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2",
//                                "Mã Khoa 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

//                    foreach (var t in xml3TrungYL)
//                    {
//                        string maBS1 = t.x1.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x2.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetYL, cellStyles, rows++,
//                                rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//                                t.x1.MA_KHOA, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL),
//                                t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//                                t.x2.MA_KHOA, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL)
//                            );
//                    }

//                    SetAutoFeatures(xlSheetYL, cols, 3, 4, 12, 13);
//                    #endregion

//                    #region Sheet 4: Loi Trung Giuong
//                    var dataGiuong = allXMl3.Where(x => (x.MA_NHOM == 14 || x.MA_NHOM == 15) && !string.IsNullOrEmpty(x.MA_GIUONG) && x.NGAY_YL != null && x.NGAY_KQ != null);

//                    var xml3GiuongTrung = dataGiuong.Join(dataGiuong, x1 => x1.MA_GIUONG, x2 => x2.MA_GIUONG, (x1, x2) => new { x1, x2 })
//                                            .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL && a.x1.MA_KHOA == a.x2.MA_KHOA && (a.x1.TYLE_TT>50||a.x2.TYLE_TT>50));

//                    ISheet xlSheetGG = workbook.CreateSheet("Trùng Giường");
//                    rows = 2;
//                    cols = CreateHeader(xlSheetGG, cellStyles,
//                                "Danh sách Dịch vụ Giường trùng thời gian", // Sheet caption
//                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
//                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
//                                "Mã Khoa 1", "Tên Khoa 1",
//                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Giường 1", "Tỷ lệ thanh toán 1",
//                                "Ngày Kết quả 1", "Ngày Y lệnh 1",
//                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
//                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
//                                "Mã Khoa 2", "Tên Khoa 2",
//                                "Mã Dịch vụ 1", "Tên Dịch vụ 2", "Giường 2", "Tỷ lệ thanh toán 2",
//                                "Ngày Kết quả 2", "Ngày Y lệnh 2");

//                    foreach (var t in xml3GiuongTrung)
//                    {
//                        string maBS1 = t.x1.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x2.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetGG, cellStyles, rows++,
//                            rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                            t.x1.XML1.MA_BN, t.x1.XML1.HO_TEN,
//                            t.x1.XML1.MA_THE, GetNgayThang(t.x1.XML1.NGAY_SINH, true), GetGioiTinh(t.x1.XML1.GIOI_TINH),
//                            t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
//                            t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, t.x1.MA_GIUONG, t.x1.TYLE_TT,
//                            GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL),
//                            t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                            t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN,
//                            t.x2.XML1.MA_THE, GetNgayThang(t.x2.XML1.NGAY_SINH, true), GetGioiTinh(t.x2.XML1.GIOI_TINH),
//                            t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
//                            t.x2.MA_KHOA, t.x2.TEN_DICH_VU, t.x2.MA_GIUONG, t.x2.TYLE_TT,
//                            GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL));
//                    }

//                    SetAutoFeatures(xlSheetGG, cols, 3, 4, 11, 19, 20, 27);
//                    #endregion

//                    #region Sheet 5: Loi Trung Ngay Y Lenh cua Thuoc & DV
//                    var xml2TRG = data2.Where(x => x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));
//                    var xml3TRG = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15) && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));

//                    var allTRG_YL = xml2TRG.Join(xml3TRG, x2 => x2.NGAY_YL,
//                                                        x3 => x3.NGAY_YL,
//                                                        (x2, x3) => new { x2, x3 })
//                                            .Where(a => a.x2.MA_LK != a.x3.MA_LK && Uti.FirstMatch(a.x2.MA_BAC_SI, a.x3.MA_BAC_SI));

//                    ISheet xlSheetTRG = workbook.CreateSheet("Trùng Thuốc DV");
//                    rows = 2;

//                    cols = CreateHeader(xlSheetTRG, cellStyles,
//                                "Danh sách trùng thời điểm bác sĩ y lệnh Thuốc với Dịch vụ", // Sheet title
//                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1", "Mã Nhóm 1",
//                                "Mã Khoa 1", "Tên Thuốc 1", "Ngày Y lệnh 1",
//                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2", "Mã Nhóm 2",
//                                "Mã Khoa 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

//                    foreach (var t in allTRG_YL)
//                    {
//                        string maBS1 = t.x2.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x3.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetTRG, cellStyles, rows++,
//                                rows - 2, t.x2.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN, t.x2.MA_NHOM,
//                                t.x2.MA_KHOA, t.x2.TEN_THUOC, GetNgayThang(t.x2.NGAY_YL),
//                                t.x3.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x3.XML1.MA_BN, t.x3.XML1.HO_TEN, t.x3.MA_NHOM,
//                                t.x3.MA_KHOA, t.x3.TEN_DICH_VU, GetNgayThang(t.x3.NGAY_YL)
//                            );
//                    }

//                    SetAutoFeatures(xlSheetTRG, cols, 3, 4, 12, 13);
//                    #endregion

//                    var xml2EVR = xml2TRG; //data2.Where(x => x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));
//                    var xml3EVR = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && string.IsNullOrWhiteSpace(x.MA_VAT_TU) && x.NGAY_YL != null && !string.IsNullOrEmpty(x.MA_BAC_SI));

//                    #region Sheet 6: Canh bao Trung Thuoc & DV (Tat ca)
//                    var allEVR_22 = xml2EVR.Join(xml2EVR, x21 => x21.NGAY_YL,
//                                                        x22 => x22.NGAY_YL,
//                                                        (x21, x22) => new { x21, x22 })
//                                            .Where(a => a.x21.MA_LK != a.x22.MA_LK && Uti.FirstMatch(a.x21.MA_BAC_SI, a.x22.MA_BAC_SI));

//                    var allEVR_23 = xml2EVR.Join(xml3EVR, x2 => x2.NGAY_YL,
//                                                        x3 => x3.NGAY_YL,
//                                                        (x2, x3) => new { x2, x3 })
//                                            .Where(a => a.x2.MA_LK != a.x3.MA_LK && Uti.FirstMatch(a.x2.MA_BAC_SI, a.x3.MA_BAC_SI));

//                    var allEVR_33 = xml3EVR.Join(xml3EVR, x31 => x31.NGAY_YL,
//                                                        x32 => x32.NGAY_YL,
//                                                        (x31, x32) => new { x31, x32 })
//                                            .Where(a => a.x31.MA_LK != a.x32.MA_LK && Uti.FirstMatch(a.x31.MA_BAC_SI, a.x32.MA_BAC_SI));

//                    ISheet xlSheetEVR = workbook.CreateSheet("Cảnh báo Trùng Y Lệnh");
//                    rows = 2;

//                    cols = CreateHeader(xlSheetEVR, cellStyles,
//                                "Danh sách Cảnh báo trùng thời điểm bác sĩ y lệnh Thuốc với Dịch vụ", // Sheet title
//                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1", "Mã Nhóm 1",
//                                "Mã Khoa 1", "Loại KCB 1", "Loại 1", "Tên 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2", "Mã Nhóm 2",
//                                "Mã Khoa 2", "Loại KCB 2", "Loại 2", "Tên 2", "Ngày Kết quả 1", "Ngày Y lệnh 2");

//                    foreach (var t in allEVR_22)
//                    {
//                        string maBS1 = t.x21.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x22.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetEVR, cellStyles, rows++,
//                                rows - 2, t.x21.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x21.XML1.MA_BN, t.x21.XML1.HO_TEN, t.x21.MA_NHOM,
//                                t.x21.MA_KHOA, t.x21.XML1.MA_LOAI_KCB, "Thuốc", t.x21.TEN_THUOC, "", GetNgayThang(t.x21.NGAY_YL), 
//                                t.x22.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x22.XML1.MA_BN, t.x22.XML1.HO_TEN, t.x22.MA_NHOM,
//                                t.x22.MA_KHOA, t.x22.XML1.MA_LOAI_KCB, "Thuốc", t.x22.TEN_THUOC, "", GetNgayThang(t.x22.NGAY_YL)
//                            );
//                    }

//                    foreach (var t in allEVR_23)
//                    {
//                        string maBS1 = t.x2.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x3.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetEVR, cellStyles, rows++,
//                                rows - 2, t.x2.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x2.XML1.MA_BN, t.x2.XML1.HO_TEN, t.x2.MA_NHOM,
//                                t.x2.MA_KHOA, t.x2.XML1.MA_LOAI_KCB, "Thuốc", t.x2.TEN_THUOC, "", GetNgayThang(t.x2.NGAY_YL),
//                                t.x3.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x3.XML1.MA_BN, t.x3.XML1.HO_TEN, t.x3.MA_NHOM,
//                                t.x3.MA_KHOA, t.x3.XML1.MA_LOAI_KCB, "Dịch vụ", t.x3.TEN_DICH_VU, GetNgayThang(t.x3.NGAY_KQ), GetNgayThang(t.x3.NGAY_YL)
//                            );
//                    }

//                    foreach (var t in allEVR_33)
//                    {
//                        string maBS1 = t.x31.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x32.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetEVR, cellStyles, rows++,
//                                rows - 2, t.x31.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x31.XML1.MA_BN, t.x31.XML1.HO_TEN, t.x31.MA_NHOM,
//                                t.x31.MA_KHOA, t.x31.XML1.MA_LOAI_KCB, "Dịch vụ", t.x31.TEN_DICH_VU, GetNgayThang(t.x31.NGAY_KQ), GetNgayThang(t.x31.NGAY_YL),
//                                t.x32.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x32.XML1.MA_BN, t.x32.XML1.HO_TEN, t.x32.MA_NHOM,
//                                t.x32.MA_KHOA, t.x32.XML1.MA_LOAI_KCB, "Dịch vụ", t.x32.TEN_DICH_VU, GetNgayThang(t.x32.NGAY_KQ), GetNgayThang(t.x32.NGAY_YL)
//                            );
//                    }

//                    SetAutoFeatures(xlSheetEVR, cols, 3, 4, 15, 16);
//                    #endregion

//                    #region Sheet 7: Canh bao Trung Ngay KQ DV
//                    var allNgayKQ = xml3EVR.Join(xml3EVR, x31 => x31.NGAY_KQ,
//                                                        x32 => x32.NGAY_KQ,
//                                                        (x31, x32) => new { x31, x32 })
//                                            .Where(a => a.x31.MA_LK != a.x32.MA_LK && Uti.FirstMatch(a.x31.MA_BAC_SI, a.x32.MA_BAC_SI));

//                    ISheet xlSheetKQ = workbook.CreateSheet("Cảnh báo Trùng Ngày KQ");
//                    rows = 2;

//                    cols = CreateHeader(xlSheetKQ, cellStyles,
//                                "Danh sách Cảnh báo trùng thời điểm Kết quả Dịch vụ", // Sheet title
//                                "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
//                                "Mã Bệnh nhân 1", "Tên Bệnh Nhân 1", "Mã Nhóm 1",
//                                "Mã Khoa 1", "Loại KCB 1", "Loại 1", "Tên 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//                                "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
//                                "Mã Bệnh nhân 2", "Tên Bệnh Nhân 2", "Mã Nhóm 2",
//                                "Mã Khoa 2", "Loại KCB 2", "Loại 2", "Tên 2", "Ngày Kết quả 1", "Ngày Y lệnh 2");

//                    foreach (var t in allNgayKQ)
//                    {
//                        string maBS1 = t.x31.MA_BAC_SI;
//                        string[] ttBS1 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS1);
//                        string maBS2 = t.x32.MA_BAC_SI;
//                        string[] ttBS2 = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS2);

//                        AppendRow(xlSheetKQ, cellStyles, rows++,
//                                rows - 2, t.x31.MA_LK, maBS1, ttBS1[0], ttBS1[1],
//                                t.x31.XML1.MA_BN, t.x31.XML1.HO_TEN, t.x31.MA_NHOM,
//                                t.x31.MA_KHOA, t.x31.XML1.MA_LOAI_KCB, "Dịch vụ", t.x31.TEN_DICH_VU, GetNgayThang(t.x31.NGAY_KQ), GetNgayThang(t.x31.NGAY_YL),
//                                t.x32.MA_LK, maBS2, ttBS2[0], ttBS2[1],
//                                t.x32.XML1.MA_BN, t.x32.XML1.HO_TEN, t.x32.MA_NHOM,
//                                t.x32.MA_KHOA, t.x32.XML1.MA_LOAI_KCB, "Dịch vụ", t.x32.TEN_DICH_VU, GetNgayThang(t.x32.NGAY_KQ), GetNgayThang(t.x32.NGAY_YL)
//                            );
//                    }

//                    SetAutoFeatures(xlSheetKQ, cols, 3, 4, 15, 16);
//                    #endregion

//                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;

//            }
//            catch { }

//            return null;
//        }

//        //Linhist added 27/02/23
//        //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//        static public byte[] DownloadExcelResources_New(IEnumerable<XML2> data2, IEnumerable<XML3> data3, IEnumerable<DMKhoaBV> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
//        {
//            try
//            {
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
//                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
//                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    #region Sheet 1: Thuoc XML2
//                    ISheet xlSheet2 = workbook.CreateSheet("DS Thuốc");
//                    int rows = 2;
//                    int cols = CreateHeader(xlSheet2, cellStyles,
//                                "Danh sách tổng hợp Thuốc XML2", // Sheet title
//                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
//                                "TT Thầu", "Đơn giá", "Số lượng", "Tỷ lệ TT", "Thành tiền",
//                                "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//                    foreach (var x2 in data2)
//                    {
//                        string maBS = x2.MA_BAC_SI;
//                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

//                        AppendRow(xlSheet2, cellStyles, rows++,
//                                rows - 2, x2.XML1.MA_LK, x2.XML1.MA_BN, x2.XML1.HO_TEN,
//                                x2.XML1.MA_THE, GetNgayThang(x2.XML1.NGAY_SINH, true), GetGioiTinh(x2.XML1.GIOI_TINH),
//                                x2.MA_KHOA, string.Format(fmKhoa, x2.MA_KHOA), maBS, ttBS[0], ttBS[1],
//                                x2.XML1.MA_BENH, x2.XML1.MA_BENHKHAC, x2.XML1.TEN_BENH,
//                                x2.MA_THUOC, x2.MA_NHOM, x2.TEN_THUOC, x2.DON_VI_TINH, x2.LIEU_DUNG, x2.SO_DANG_KY,
//                                x2.TT_THAU, x2.DON_GIA, x2.SO_LUONG, x2.TYLE_TT, x2.THANH_TIEN,
//                                GetNgayThang(x2.NGAY_YL), GetNgayThang(x2.XML1.NGAY_VAO), GetNgayThang(x2.XML1.NGAY_RA), GetNgayThang(x2.XML1.NGAY_TTOAN));
//                    }

//                    SetAutoFeatures(xlSheet2, cols, 8, 10, 11, -14);
//                    #endregion


//                    #region Sheet 2: Dich vu XML3
//                    var data3DVs = data3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.NumExists(x.MA_NHOM, 10, 11));

//                    ISheet xlSheetDV = workbook.CreateSheet("DS Dịch vụ");
//                    rows = 2;
//                    cols = CreateHeader(xlSheetDV, cellStyles,
//                                "Danh sách tổng hợp Dịch vụ XML3", // Sheet caption
//                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Mã Dịch vụ", "Tên Dịch vụ",
//                                "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
//                                "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//                    foreach (var x3 in data3DVs)
//                    {
//                        string maBS = x3.MA_BAC_SI;
//                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

//                        AppendRow(xlSheetDV, cellStyles, rows++,
//                                rows - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//                                x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//                                x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1],
//                                x3.XML1.MA_BENH, x3.XML1.MA_BENHKHAC, x3.XML1.TEN_BENH,
//                                x3.MA_DICH_VU, x3.TEN_DICH_VU,
//                                x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA, x3.TYLE_TT, x3.THANH_TIEN,
//                                GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML1.NGAY_VAO), GetNgayThang(x3.XML1.NGAY_RA), GetNgayThang(x3.XML1.NGAY_TTOAN));
//                    }
//                    SetAutoFeatures(xlSheetDV, cols, 8, 10, 11, -14);
//                    #endregion


//                    #region Sheet 3: Vat tu XML3
//                    var data3VTs = data3.Where(x => !string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.NumExists(x.MA_NHOM, 10, 11));

//                    ISheet xlSheetVT = workbook.CreateSheet("DS VTYT");
//                    rows = 2;
//                    cols = CreateHeader(xlSheetVT, cellStyles, "Danh sách tổng hợp Vật tư XML3",
//                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Mã Vật tư", "Tên Vật tư",
//                                "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
//                                "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//                    foreach (var x3 in data3VTs)
//                    {
//                        string maBS = x3.MA_BAC_SI;
//                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

//                        AppendRow(xlSheetVT, cellStyles, rows++,
//                                rows - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//                                x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//                                x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1],
//                                x3.XML1.MA_BENH, x3.XML1.MA_BENHKHAC, x3.XML1.TEN_BENH,
//                                x3.MA_VAT_TU, x3.TEN_VAT_TU,
//                                x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA, x3.TYLE_TT, x3.THANH_TIEN,
//                                GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML1.NGAY_VAO), GetNgayThang(x3.XML1.NGAY_RA), GetNgayThang(x3.XML1.NGAY_TTOAN));
//                    }

//                    SetAutoFeatures(xlSheetVT, cols, 8, 10, 11, -14);
//                    #endregion

//                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;

//            }
//            catch (Exception ex)
//            {
//                string stk = ex.StackTrace;
//            }

//            return null;
//        }
//        #endregion

//        #region Linhist removed 19/10/23
//        //static public byte[] DownloadExcelIndication(IEnumerable<XML1> data1, IEnumerable<ChiDinhDongThoi> dmDongThois)
//        //{
//        //    try
//        //    {
//        //        //Create new excel
//        //        ICellStyle[] cellStyles = null;
//        //        IWorkbook workbook = CreateWorkBook(out cellStyles);

//        //        byte[] result = null;
//        //        using (var fs = new MemoryStream())
//        //        {
//        //            #region Sheet 1: DVKT Tuong Duong Trung gio - Loi 930
//        //            ISheet xlSheet930 = workbook.CreateSheet("DVKT Tương đương");
//        //            int rows = 2;
//        //            int cols = CreateHeader(xlSheet930, cellStyles,
//        //                        "Danh sách DVKT tương đương bị trùng ngày", // Sheet title
//        //                        "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//        //                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//        //            var dongThoi_err930 = dmDongThois.Where(x => x.MA_LOI == 930);
//        //            foreach (var rule in dongThoi_err930)
//        //            {
//        //                foreach (var xml1 in data1)
//        //                {
//        //                    var dichVus = xml1.XML3s.Where(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU) && x.NGAY_YL != null);
//        //                    var dvKhacs = xml1.XML3s.Where(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null);
//        //                    var allDates = dichVus.Select(x => x.NGAY_YL.Value.Date).Distinct().ToList();
//        //                    allDates.AddRange(dvKhacs.Select(x => x.NGAY_YL.Value.Date).Distinct().ToList());

//        //                    foreach (var dte in allDates)
//        //                    {
//        //                        var matches1 = dichVus.Where(x => x.NGAY_YL.Value.Date == dte).ToList();
//        //                        var matches2 = dvKhacs.Where(x => x.NGAY_YL.Value.Date == dte).ToList();

//        //                        int cnt = Math.Max(matches1.Count, matches2.Count);
//        //                        for (int i = 0; i < cnt; i++)
//        //                        {
//        //                            var m1 = i < matches1.Count ? matches1[i] : new XML3();
//        //                            var m2 = i < matches2.Count ? matches2[i] : new XML3();

//        //                            AppendRow(xlSheet930, rows++,
//        //                                        rows - 2, xml1.MA_LK, xml1.MA_BN, xml1.HO_TEN,
//        //                                        xml1.MA_THE, GetNgayThang(xml1.NGAY_SINH, true), GetGioiTinh(xml1.GIOI_TINH),
//        //                                        m1.MA_DICH_VU, m1.TEN_DICH_VU, m1.NGAY_YL, m1.NGAY_KQ,
//        //                                        m2.MA_DICH_VU, m2.TEN_DICH_VU, m2.NGAY_YL, m2.NGAY_KQ
//        //                                    );
//        //                        }
//        //                    }
//        //                }
//        //            }
//        //            SetAutoFeatures(xlSheet930, cols);
//        //            #endregion


//        //            #region Sheet 2: Chi dinh thieu DV, VT - Loi 1004
//        //            var dongThoi_err1004 = dmDongThois.Where(x => x.MA_LOI == 1004);

//        //            List<XML3> errVTs = new List<XML3>();
//        //            List<XML3> errDVs = new List<XML3>();

//        //            foreach (var rule in dongThoi_err1004)
//        //            {
//        //                foreach (var xml1 in data1)
//        //                {
//        //                    var dvMatches = xml1.XML3s.Where(x => string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU));
//        //                    var vtMatches = xml1.XML3s.Where(x => Uti.CodeMatch(x.MA_VAT_TU, rule.VAT_TU_BHYT));

//        //                    if (!vtMatches.Any())
//        //                    {
//        //                        foreach (var x3 in dvMatches)
//        //                        {
//        //                            if (errDVs.Contains(x3)) continue;
//        //                            if (dongThoi_err1004.Any(r => Uti.CodeMatch(x3.MA_DICH_VU, r.MA_DICH_VU) && Uti.CodeMatch(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

//        //                            errDVs.Add(x3);
//        //                        }
//        //                    }
//        //                    else if (!dvMatches.Any())
//        //                    {
//        //                        foreach (var x3 in vtMatches)
//        //                        {
//        //                            if (errVTs.Contains(x3)) continue;
//        //                            if (dongThoi_err1004.Any(r => Uti.CodeMatch(x3.MA_DICH_VU, r.MA_DICH_VU) && Uti.CodeMatch(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

//        //                            errVTs.Add(x3);
//        //                        }
//        //                    }
//        //                }
//        //            }

//        //            ISheet xlSheetDV1004 = workbook.CreateSheet("DV thiếu VT");
//        //            int rowsDV = 2;
//        //            cols = CreateHeader(xlSheetDV1004, cellStyles,
//        //                                "Danh sách Chỉ định DVKT thiếu VTYT", // Sheet title
//        //                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                                "Mã Dịch vụ", "Tên Dịch vụ", "Ngày Kết quả DV", "Ngày Y lệnh DV", "Nội dung lỗi");

//        //            foreach (var x3 in errDVs)
//        //            {
//        //                AppendRow(xlSheetDV1004, rowsDV++,
//        //                        rowsDV - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//        //                        x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//        //                        x3.MA_DICH_VU, x3.TEN_DICH_VU, x3.NGAY_YL, x3.NGAY_KQ,
//        //                        "Chỉ định dịch vụ kỹ thuật không có vật tư kỹ thuật đồng thời"
//        //                    );
//        //            }

//        //            SetAutoFeatures(xlSheetDV1004, cols);


//        //            ISheet xlSheetVT1004 = workbook.CreateSheet("VT thiếu DV");
//        //            int rowsVT = 2;
//        //            cols = CreateHeader(xlSheetVT1004, cellStyles,
//        //                                "Danh sách Chỉ định VTYT thiếu DVKT", // Sheet title
//        //                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//        //                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//        //                                "Mã Vật tư", "Tên Vật tư", "Ngày Kết quả VT", "Ngày Y lệnh VT", "Nội dung lỗi");

//        //            foreach (var x3 in errVTs)
//        //            {
//        //                AppendRow(xlSheetVT1004, rowsVT++,
//        //                        rowsVT - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//        //                        x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//        //                        x3.MA_VAT_TU, x3.TEN_VAT_TU, x3.NGAY_YL, x3.NGAY_KQ,
//        //                        "Chỉ định vật tư kỹ thuật không có dịch vụ kỹ thuật đồng thời"
//        //                    );
//        //            }

//        //            SetAutoFeatures(xlSheetVT1004, cols);
//        //            #endregion

//        //            workbook.Write(fs);
//        //            result = fs.ToArray();
//        //        }

//        //        return result;

//        //    }
//        //    catch { }

//        //    return null;
//        //}
//        //static public byte[] DownloadExcelSumErrors(IEnumerable<XML1> data1, IEnumerable<DMErrorInfoModel> dmErrors)
//        //{
//        //    try
//        //    {
//        //        //Create new excel
//        //        ICellStyle[] cellStyles = null;
//        //        IWorkbook workbook = CreateWorkBook(out cellStyles);

//        //        var allKhoas = data1.Select(x => x.MA_KHOA).Distinct().OrderBy(x => x).ToList();
//        //        dmErrors = dmErrors.OrderBy(e => e.ErrorAtXMLFile);

//        //        byte[] result = null;
//        //        using (var fs = new MemoryStream())
//        //        {
//        //            #region Sheet 1: DS File Bao cao
//        //            ISheet xlSheetF = workbook.CreateSheet("Thống kê lỗi");

//        //            int rows = 2;

//        //            string[] hVals = new string[allKhoas.Count + 4];
//        //            hVals[0] = "Mã Lỗi";
//        //            hVals[1] = "Loại lỗi";
//        //            hVals[2] = "Nội dung lỗi";
//        //            hVals[3] = "Hồ sơ lỗi";
//        //            allKhoas.CopyTo(hVals, 4);

//        //            int cols = CreateHeader(xlSheetF, cellStyles,
//        //                        $"Báo Cáo Thống kê {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
//        //                        hVals);

//        //            var grpErrs = data1.SelectMany(x => x.XML1Errors.Select(e => new { x.XML1ID, x.MA_KHOA, e.XML1ERID, e.XEErrorCode, e.XEErrorRow }))
//        //                                .GroupBy(e => new { e.MA_KHOA, e.XEErrorCode })
//        //                                .Select(g => new { g.Key.MA_KHOA, MA_LOI = g.Key.XEErrorCode, SO_LOI = g.Count() });

//        //            foreach (var e in dmErrors)
//        //            {
//        //                var lst = grpErrs.Where(x => x.MA_LOI == e.ErrorCode);
//        //                if (!lst.Any()) continue;

//        //                List<object> values = new List<object>
//        //                {
//        //                    e.ErrorCode,
//        //                    e.IsAlarm ? "CẢNH BÁO" : "BÁO LỖI",
//        //                    e.ErrorContent,
//        //                    e.ErrorAtXMLFile,
//        //                };

//        //                foreach (var k in allKhoas)
//        //                {
//        //                    values.Add(lst.FirstOrDefault(e => e.MA_KHOA == k)?.SO_LOI ?? 0);
//        //                }

//        //                AppendRow(xlSheetF, rows++, values.ToArray());
//        //            }

//        //            SetAutoFeatures(xlSheetF, cols, -2);
//        //            #endregion

//        //            workbook.Write(fs);
//        //            result = fs.ToArray();
//        //        }

//        //        return result;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string stk = ex.StackTrace;
//        //    }

//        //    return null;
//        //}
//        #endregion


//        #region Linhist added 08/06/22

//        //Linhist added 27/02/23
//        static public byte[] DownloadExcelIndication_New(IEnumerable<XML1> data1, IEnumerable<XML3> data3, IEnumerable<ChiDinhDongThoi> dmDongThois)
//        {
//            try
//            {
//                //Create new excel
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    #region Sheet 1: DVKT Tuong Duong Trung gio - Loi 930
//                    ISheet xlSheet930 = workbook.CreateSheet("DVKT Tương đương");
//                    int rows = 2;
//                    int cols = CreateHeader(xlSheet930, cellStyles,
//                                "Danh sách DVKT tương đương bị trùng ngày", // Sheet title
//                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Kết quả 1", "Ngày Y lệnh 1",
//                                "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Kết quả 2", "Ngày Y lệnh 2");

//                    var dongThoi_err930 = dmDongThois.Where(x => x.MA_LOI == 930);
//                    foreach (var rule in dongThoi_err930)
//                    {
//                        foreach (var xml1 in data1)
//                        {
//                            var dichVus = data3.Where(x => x.XML1ID == xml1.XML1ID && Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU) && x.NGAY_YL != null);
//                            var dvKhacs = data3.Where(x => x.XML1ID == xml1.XML1ID && Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null);
//                            var allDates = dichVus.Select(x => x.NGAY_YL.Value.Date).Distinct().ToList();
//                            allDates.AddRange(dvKhacs.Select(x => x.NGAY_YL.Value.Date).Distinct().ToList());

//                            foreach (var dte in allDates)
//                            {
//                                var matches1 = dichVus.Where(x => x.NGAY_YL.Value.Date == dte).ToList();
//                                var matches2 = dvKhacs.Where(x => x.NGAY_YL.Value.Date == dte).ToList();

//                                int cnt = Math.Max(matches1.Count, matches2.Count);
//                                for (int i = 0; i < cnt; i++)
//                                {
//                                    var m1 = i < matches1.Count ? matches1[i] : new XML3();
//                                    var m2 = i < matches2.Count ? matches2[i] : new XML3();

//                                    AppendRow(xlSheet930, cellStyles, rows++,
//                                                rows - 2, xml1.MA_LK, xml1.MA_BN, xml1.HO_TEN,
//                                                xml1.MA_THE, GetNgayThang(xml1.NGAY_SINH, true), GetGioiTinh(xml1.GIOI_TINH),
//                                                m1.MA_DICH_VU, m1.TEN_DICH_VU, m1.NGAY_YL, m1.NGAY_KQ,
//                                                m2.MA_DICH_VU, m2.TEN_DICH_VU, m2.NGAY_YL, m2.NGAY_KQ
//                                            );
//                                }
//                            }
//                        }
//                    }
//                    SetAutoFeatures(xlSheet930, cols);
//                    #endregion


//                    #region Sheet 2: Chi dinh thieu DV, VT - Loi 1004
//                    var dongThoi_err1004 = dmDongThois.Where(x => x.MA_LOI == 1004);

//                    List<XML3> errVTs = new List<XML3>();
//                    List<XML3> errDVs = new List<XML3>();

//                    foreach (var rule in dongThoi_err1004)
//                    {
//                        foreach (var xml1 in data1)
//                        {
//                            var dvMatches = data3.Where(x => x.XML1ID == xml1.XML1ID && string.IsNullOrEmpty(x.MA_VAT_TU) && Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU));
//                            var vtMatches = data3.Where(x => x.XML1ID == xml1.XML1ID && Uti.CodeMatch(x.MA_VAT_TU, rule.VAT_TU_BHYT));

//                            if (!vtMatches.Any())
//                            {
//                                foreach (var x3 in dvMatches)
//                                {
//                                    if (errDVs.Contains(x3)) continue;
//                                    if (dongThoi_err1004.Any(r => Uti.CodeMatch(x3.MA_DICH_VU, r.MA_DICH_VU) && Uti.CodeMatch(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

//                                    errDVs.Add(x3);
//                                }
//                            }
//                            else if (!dvMatches.Any())
//                            {
//                                foreach (var x3 in vtMatches)
//                                {
//                                    if (errVTs.Contains(x3)) continue;
//                                    if (dongThoi_err1004.Any(r => Uti.CodeMatch(x3.MA_DICH_VU, r.MA_DICH_VU) && Uti.CodeMatch(x3.MA_VAT_TU, r.VAT_TU_BHYT))) continue;

//                                    errVTs.Add(x3);
//                                }
//                            }
//                        }
//                    }

//                    ISheet xlSheetDV1004 = workbook.CreateSheet("DV thiếu VT");
//                    int rowsDV = 2;
//                    cols = CreateHeader(xlSheetDV1004, cellStyles,
//                                        "Danh sách Chỉ định DVKT thiếu VTYT", // Sheet title
//                                        "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                        "Mã Dịch vụ", "Tên Dịch vụ", "Ngày Kết quả DV", "Ngày Y lệnh DV", "Nội dung lỗi");

//                    foreach (var x3 in errDVs)
//                    {
//                        AppendRow(xlSheetDV1004, cellStyles, rowsDV++,
//                                rowsDV - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//                                x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//                                x3.MA_DICH_VU, x3.TEN_DICH_VU, x3.NGAY_YL, x3.NGAY_KQ,
//                                "Chỉ định dịch vụ kỹ thuật không có vật tư kỹ thuật đồng thời"
//                            );
//                    }

//                    SetAutoFeatures(xlSheetDV1004, cols);


//                    ISheet xlSheetVT1004 = workbook.CreateSheet("VT thiếu DV");
//                    int rowsVT = 2;
//                    cols = CreateHeader(xlSheetVT1004, cellStyles,
//                                        "Danh sách Chỉ định VTYT thiếu DVKT", // Sheet title
//                                        "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                        "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                        "Mã Vật tư", "Tên Vật tư", "Ngày Kết quả VT", "Ngày Y lệnh VT", "Nội dung lỗi");

//                    foreach (var x3 in errVTs)
//                    {
//                        AppendRow(xlSheetVT1004, cellStyles, rowsVT++,
//                                rowsVT - 2, x3.XML1.MA_LK, x3.XML1.MA_BN, x3.XML1.HO_TEN,
//                                x3.XML1.MA_THE, GetNgayThang(x3.XML1.NGAY_SINH, true), GetGioiTinh(x3.XML1.GIOI_TINH),
//                                x3.MA_VAT_TU, x3.TEN_VAT_TU, x3.NGAY_YL, x3.NGAY_KQ,
//                                "Chỉ định vật tư kỹ thuật không có dịch vụ kỹ thuật đồng thời"
//                            );
//                    }

//                    SetAutoFeatures(xlSheetVT1004, cols);
//                    #endregion

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;

//            }
//            catch { }

//            return null;
//        }
//        #endregion


//        #region Linhist added 09/10/22

//        //Linhist added 27/02/23
//        static public byte[] DownloadExcelSumErrors_New(IEnumerable<XML1> data1, IEnumerable<XML1Error> dataE, IEnumerable<DMErrorInfoModel> dmErrors)
//        {
//            try
//            {
//                //Create new excel
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                var allKhoas = data1.Select(x => x.MA_KHOA).Distinct().OrderBy(x => x).ToList();
//                dmErrors = dmErrors.OrderBy(e => e.ErrorAtXMLFile);

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    #region Sheet 1: DS File Bao cao
//                    ISheet xlSheetF = workbook.CreateSheet("Thống kê lỗi");

//                    int rows = 2;

//                    string[] hVals = new string[allKhoas.Count + 4];
//                    hVals[0] = "Mã Lỗi";
//                    hVals[1] = "Loại lỗi";
//                    hVals[2] = "Nội dung lỗi";
//                    hVals[3] = "Hồ sơ lỗi";
//                    allKhoas.CopyTo(hVals, 4);

//                    int cols = CreateHeader(xlSheetF, cellStyles,
//                                $"Báo Cáo Thống kê {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
//                                hVals);

//                    var grpErrs = dataE.Select(e => new { e.XEXML1ID, e.XEXML1.MA_KHOA, e.XML1ERID, e.XEErrorCode, e.XEErrorRow })
//                                        .GroupBy(e => new { e.MA_KHOA, e.XEErrorCode })
//                                        .Select(g => new { g.Key.MA_KHOA, MA_LOI = g.Key.XEErrorCode, SO_LOI = g.Count() });

//                    foreach (var e in dmErrors)
//                    {
//                        var lst = grpErrs.Where(x => x.MA_LOI == e.ErrorCode);
//                        if (!lst.Any()) continue;

//                        List<object> values = new List<object>
//                        {
//                            e.ErrorCode,
//                            e.IsAlarm ? "CẢNH BÁO" : "BÁO LỖI",
//                            e.ErrorContent,
//                            e.ErrorAtXMLFile,
//                        };

//                        foreach (var k in allKhoas)
//                        {
//                            values.Add(lst.FirstOrDefault(e => e.MA_KHOA == k)?.SO_LOI ?? 0);
//                        }

//                        AppendRow(xlSheetF, cellStyles, rows++, values.ToArray());
//                    }

//                    SetAutoFeatures(xlSheetF, cols, 1, -2);
//                    #endregion

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                string stk = ex.StackTrace;
//            }

//            return null;
//        }
//        #endregion



//        //Linhist added 24/02/23
//        //split list xml1,2,3,4,5,error
//        //Linhist changed 08/05/24 - sua DMKhoa thanh DMKhoaBV
//        static public byte[] DownloadExcelTotalErrors_New(IEnumerable<XML1> data1, IEnumerable<XML2> data2, IEnumerable<XML3> data3, IEnumerable<XML4> data4, IEnumerable<XML5> data5, IEnumerable<XML1Error> dataE, IEnumerable<DMErrorInfoModel> dmErrors, IEnumerable<DMKhoaBV> dmKhoas, IEnumerable<DMBacSi> dmBacsis)
//        {
//            try
//            {
//                //Create new excel
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                var allErrors = dataE;
//                var allErrCodes = allErrors.GroupBy(x => x.XEErrorCode, (k, g) => new { ErrorCode = k, Count = g.Count() });
//                var grpErrors = allErrCodes.Join(dmErrors, x => x.ErrorCode, y => y.ErrorCode, (x, y) => new
//                {
//                    y.ErrorCode,
//                    y.ErrorAtXMLFile,
//                    y.ErrorAtXMLTag,
//                    x.Count,
//                    y.ErrorContent,
//                    y.IsAlarm,
//                }).OrderBy(e => e.ErrorAtXMLFile);

//                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{(dmBacsis.Count() + 5)},2,FALSE),\"\")";
//                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{(dmBacsis.Count() + 5)},3,FALSE),\"\")";
//                string fmKhoaTH = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:D{(dmBacsis.Count() + 5)},4,FALSE),\"\")";
//                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{(dmKhoas.Count() + 5)},2,FALSE),\"\")";
//                string fmError = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:D{(grpErrors.Count() + 5)},3,FALSE),\"\")";
//                string fmEtype = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:C{(grpErrors.Count() + 5)},2,FALSE),\"\")";

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    #region Sheet 1: DS File Bao cao
//                    var grpFiles = data1.OrderBy(x => x.FileName);
//                    ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");

//                    int rows = 2;
//                    int cols = CreateHeader(xlSheetF, cellStyles,
//                                $"Báo Cáo Tổng Hợp {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
//                                "STT", "Tên File", "Mã CSKCB",
//                                "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính", "Địa chỉ",
//                                "Mã Khoa", "Tên Khoa",
//                                "Ngày vào", "Ngày ra", "Ngày lập");

//                    foreach (var x in grpFiles)
//                    {
//                        AppendRow(xlSheetF, cellStyles, rows++,
//                                    rows - 2, x.FileName, x.MA_CSKCB,
//                                    x.MA_LK, x.MA_BN, x.HO_TEN,
//                                    x.MA_THE, GetNgayThang(x.NGAY_SINH, true), GetGioiTinh(x.GIOI_TINH), x.DIA_CHI,
//                                    x.MA_KHOA, string.Format(fmKhoa, x.MA_KHOA),
//                                    GetNgayThang(x.NGAY_VAO), GetNgayThang(x.NGAY_RA), GetNgayThang(x.CreateDate));
//                    }

//                    SetAutoFeatures(xlSheetF, cols, -1, 11);
//                    #endregion


//                    #region Sheet 2: Tong hop Loi
//                    ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Lỗi");
//                    rows = 2;
//                    cols = CreateHeader(xlSheetE, cellStyles,
//                                        "Tổng hợp số lỗi trong file hồ sơ", // Sheet title
//                                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi", "Loại File lỗi", "Lỗi tại Thẻ", "Số lỗi");

//                    foreach (var x2 in grpErrors)
//                    {
//                        AppendRow(xlSheetE, cellStyles, rows++,
//                                    rows - 2, x2.ErrorCode, x2.IsAlarm ? "CẢNH BÁO" : "BÁO LỖI", x2.ErrorContent, x2.ErrorAtXMLFile, x2.ErrorAtXMLTag, x2.Count);
//                    }

//                    SetAutoFeatures(xlSheetE, cols, 2, -3);
//                    #endregion


//                    #region Sheet 3: Loi XML1
//                    List<int?> xml1ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML1").Select(e => e.ErrorCode).ToList();
//                    var xml1Errors = allErrors.Where(a => xml1ErCodes.Contains(a.XEErrorCode));

//                    ISheet xlSheet1 = workbook.CreateSheet("Lỗi XML1");
//                    rows = 2;
//                    cols = CreateHeader(xlSheet1, cellStyles,
//                                "Danh sách lỗi của XML1", // Sheet title
//                                "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Khoa", "Tên Khoa",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//                                "Tổng Chi");

//                    foreach (var x in xml1Errors)
//                    {
//                        AppendRow(xlSheet1, cellStyles, rows++,
//                                rows - 2, x.XEErrorCode, string.Format(fmEtype, x.XEErrorCode), string.Format(fmError, x.XEErrorCode),
//                                x.XEXML1.MA_LK, x.XEXML1.MA_BN, x.XEXML1.HO_TEN,
//                                x.XEXML1.MA_THE, GetNgayThang(x.XEXML1.NGAY_SINH, true), GetGioiTinh(x.XEXML1.GIOI_TINH),
//                                x.XEXML1.MA_KHOA, string.Format(fmKhoa, x.XEXML1.MA_KHOA),
//                                x.XEXML1.MA_BENH, x.XEXML1.MA_BENHKHAC, x.XEXML1.TEN_BENH,
//                                GetNgayThang(x.XEXML1.NGAY_VAO), GetNgayThang(x.XEXML1.NGAY_RA), GetNgayThang(x.XEXML1.NGAY_TTOAN),
//                                string.Format("{0:##,##0.0#}", x.XEXML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//                    }

//                    SetAutoFeatures(xlSheet1, cols, 2, -3, 11, -14);
//                    #endregion


//                    #region Sheet 4: Loi XML2
//                    List<int?> xml2ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML2").Select(e => e.ErrorCode).ToList();
//                    var xml2Errors = allErrors.Where(a => xml2ErCodes.Contains(a.XEErrorCode));

//                    var data2Join =
//                        data2
//                        .Join(xml2Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml2 = x, XmlE = e })
//                        .Where(x => x.Xml2.STT == x.XmlE.XEErrorRow);

//                    ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML2");
//                    rows = 2;
//                    cols = CreateHeader(xlSheet2, cellStyles,
//                                "Danh sách lỗi của XML2", // Sheet title
//                                "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//                                "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Khoa", "Tên Khoa",
//                                "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký",
//                                "TT Thầu", "Đơn giá", "Số lượng", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
//                                "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//                                "Tổng Chi");

//                    foreach (var x in data2Join)
//                    {
//                        string maBS = x.Xml2.MA_BAC_SI;
//                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

//                        AppendRow(xlSheet2, cellStyles, rows++,
//                                rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//                                x.Xml2.XML1.MA_LK, x.Xml2.XML1.MA_BN, x.Xml2.XML1.HO_TEN,
//                                x.Xml2.XML1.MA_THE, GetNgayThang(x.Xml2.XML1.NGAY_SINH, true), GetGioiTinh(x.Xml2.XML1.GIOI_TINH),
//                                x.Xml2.XML1.MA_KHOA, string.Format(fmKhoa, x.Xml2.XML1.MA_KHOA),
//                                maBS, ttBS[0], ttBS[1], x.Xml2.MA_KHOA,
//                                x.Xml2.XML1.MA_BENH, x.Xml2.XML1.MA_BENHKHAC, x.Xml2.XML1.TEN_BENH,
//                                x.Xml2.MA_THUOC, x.Xml2.MA_NHOM, x.Xml2.TEN_THUOC, x.Xml2.DON_VI_TINH, x.Xml2.LIEU_DUNG, x.Xml2.SO_DANG_KY,
//                                x.Xml2.TT_THAU, x.Xml2.DON_GIA, x.Xml2.SO_LUONG, x.Xml2.MUC_HUONG, x.Xml2.TYLE_TT, x.Xml2.THANH_TIEN,
//                                GetNgayThang(x.Xml2.NGAY_YL), GetNgayThang(x.Xml2.XML1.NGAY_VAO), GetNgayThang(x.Xml2.XML1.NGAY_RA), GetNgayThang(x.Xml2.XML1.NGAY_TTOAN),
//                                string.Format("{0:##,##0.0#}", x.Xml2.XML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//                    }

//                    SetAutoFeatures(xlSheet2, cols, 2, -3, 11, 13, 14, -18);
//                    #endregion


//                    #region Sheet 5: Loi XML3
//                    List<int?> xml3ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML3").Select(e => e.ErrorCode).ToList();
//                    var xml3Errors = allErrors.Where(a => xml3ErCodes.Contains(a.XEErrorCode));

//                    var data3Join =
//                        data3.Join(xml3Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml3 = x, XmlE = e })
//                        .Where(x => x.Xml3.STT == x.XmlE.XEErrorRow);

//                    ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML3");
//                    rows = 2;
//                    cols = CreateHeader(xlSheet3, cellStyles,
//                                "Danh sách lỗi của XML3", // Sheet title
//                                "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Thẻ", "Ngày sinh", "Giới tính",
//                                "Mã Khoa", "Tên Khoa",
//                                "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Khoa y lệnh", "Khoa Thực hiện",
//                                "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
//                                "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//                                "Tổng Chi");

//                    foreach (var x in data3Join)
//                    {
//                        string maBS = x.Xml3.MA_BAC_SI;
//                        string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
//                        string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

//                        AppendRow(xlSheet3, cellStyles, rows++,
//                                rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//                                x.Xml3.XML1.MA_LK, x.Xml3.XML1.MA_BN, x.Xml3.XML1.HO_TEN,
//                                x.Xml3.XML1.MA_THE, GetNgayThang(x.Xml3.XML1.NGAY_SINH, true), GetGioiTinh(x.Xml3.XML1.GIOI_TINH),
//                                x.Xml3.XML1.MA_KHOA, string.Format(fmKhoa, x.Xml3.XML1.MA_KHOA),
//                                maBS, ttBS[0], ttBS[1], x.Xml3.MA_KHOA, tenKTH,
//                                x.Xml3.MA_DICH_VU, x.Xml3.TEN_DICH_VU, x.Xml3.MA_VAT_TU, x.Xml3.TEN_VAT_TU,
//                                x.Xml3.XML1.MA_BENH, x.Xml3.XML1.MA_BENHKHAC, x.Xml3.XML1.TEN_BENH,
//                                x.Xml3.DON_VI_TINH, x.Xml3.TT_THAU, x.Xml3.DON_GIA, x.Xml3.MUC_HUONG, x.Xml3.TYLE_TT, x.Xml3.THANH_TIEN,
//                                GetNgayThang(x.Xml3.NGAY_YL), GetNgayThang(x.Xml3.NGAY_KQ), GetNgayThang(x.Xml3.XML1.NGAY_VAO), GetNgayThang(x.Xml3.XML1.NGAY_RA), GetNgayThang(x.Xml3.XML1.NGAY_TTOAN),
//                                string.Format("{0:##,##0.0#}", x.Xml3.XML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//                    }

//                    SetAutoFeatures(xlSheet3, cols, 2, -3, 11, 13, 14, 16, -23);
//                    #endregion


//                    #region Sheet 6: Loi XML4
//                    //Linhist changed 15/11/22
//                    //var data4 = xml1HaveXml4error.SelectMany(x => x.XML4s);
//                    //List<int?> xml4ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//                    //var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.XEErrorCode));
//                    List<int?> xml4ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML4").Select(e => e.ErrorCode).ToList();
//                    var xml4Errors = allErrors.Where(a => xml4ErCodes.Contains(a.XEErrorCode));

//                    var data4Join =
//                        data4.Join(xml4Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml4 = x, XmlE = e })
//                        .Where(x => x.Xml4.STT == x.XmlE.XEErrorRow).OrderBy(x => x.Xml4.XML1ID);

//                    ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML4");
//                    rows = 2;
//                    cols = CreateHeader(xlSheet4, cellStyles,
//                                "Danh sách lỗi của XML4", // Sheet title
//                                "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Khoa", "Tên Khoa",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Mã Dịch vụ", "Mã Máy", "Giá trị",
//                                "Mã Chỉ số", "Tên Chỉ số",
//                                "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//                                "Tổng Chi");

//                    XML1 xml1 = null;
//                    foreach (var x in data4Join)
//                    {
//                        if (xml1 == null || xml1.XML1ID != x.Xml4.XML1ID)
//                        {
//                            xml1 = data1.FirstOrDefault(x1 => x1.XML1ID == x.Xml4.XML1ID);
//                        }

//                        AppendRow(xlSheet4, cellStyles, rows++,
//                                rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//                                xml1?.MA_LK, xml1?.MA_BN, xml1?.HO_TEN,
//                                xml1?.MA_KHOA, string.Format(fmKhoa, xml1?.MA_KHOA),
//                                xml1?.MA_BENH, xml1?.MA_BENHKHAC, xml1?.TEN_BENH,
//                                x.Xml4.MA_DICH_VU, x.Xml4.MA_MAY, x.Xml4.GIA_TRI,
//                                x.Xml4.MA_CHI_SO, x.Xml4.TEN_CHI_SO,
//                                GetNgayThang(x.Xml4.NGAY_KQ), GetNgayThang(xml1?.NGAY_VAO), GetNgayThang(xml1?.NGAY_RA), GetNgayThang(xml1?.NGAY_TTOAN),
//                                string.Format("{0:##,##0.0#}", xml1?.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//                    }

//                    SetAutoFeatures(xlSheet4, cols, 2, -3, 7);
//                    #endregion


//                    #region Sheet 7: Loi XML5
//                    List<int?> xml5ErCodes = dmErrors.Where(e => e.ErrorAtXMLFile == "XML5").Select(e => e.ErrorCode).ToList();
//                    var xml5Errors = allErrors.Where(a => xml5ErCodes.Contains(a.XEErrorCode));

//                    var data5Join =
//                        data5.Join(xml5Errors, x => x.XML1ID, e => e.XEXML1ID, (x, e) => new { Xml5 = x, XmlE = e })
//                        .Where(x => x.Xml5.STT == x.XmlE.XEErrorRow);

//                    ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML5");
//                    rows = 2;
//                    cols = CreateHeader(xlSheet5, cellStyles,
//                                "Danh sách lỗi của XML5", // Sheet title
//                                "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi",
//                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
//                                "Mã Khoa", "Tên Khoa",
//                                "Chuẩn đoán chính", "Chuẩn đoán kèm theo", "Tên Bệnh",
//                                "Ngày Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
//                                "Tổng Chi");

//                    foreach (var x in data5Join)
//                    {
//                        AppendRow(xlSheet5, cellStyles, rows++,
//                                rows - 2, x.XmlE.XEErrorCode, string.Format(fmEtype, x.XmlE.XEErrorCode), string.Format(fmError, x.XmlE.XEErrorCode),
//                                x.Xml5.XML1.MA_LK, x.Xml5.XML1.MA_BN, x.Xml5.XML1.HO_TEN,
//                                x.Xml5.XML1.MA_KHOA, string.Format(fmKhoa, x.Xml5.XML1.MA_KHOA),
//                                x.Xml5.XML1.MA_BENH, x.Xml5.XML1.MA_BENHKHAC, x.Xml5.XML1.TEN_BENH,
//                                GetNgayThang(x.Xml5.NGAY_YL), GetNgayThang(x.Xml5.XML1.NGAY_VAO), GetNgayThang(x.Xml5.XML1.NGAY_RA), GetNgayThang(x.Xml5.XML1.NGAY_TTOAN),
//                                string.Format("{0:##,##0.0#}", x.Xml5.XML1.T_TONGCHI).Replace(".", "$").Replace(",", ".").Replace("$", ","));
//                    }

//                    SetAutoFeatures(xlSheet5, cols, 2, -3, 8);
//                    #endregion

//                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                string stk = ex.StackTrace;
//            }

//            return null;
//        }

//        //Linhist added 27/07/23
//        static public byte[] DownloadExcelXml4Dublicated_New(IEnumerable<DMDichVu> dataDV, IEnumerable<XML4> data4)
//        {
//            try
//            {
//                //Create new excel
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                var allXml4 = data4.Where(x => !string.IsNullOrWhiteSpace(x.MA_MAY?.Trim()) && Uti.StrExists(x.MA_MAY.Substring(0, 2), true, "CL", "CT", "MR", "SA", "XQ"));
//                string fmSoSanh = "$=COUNTIFS(I:I,I{0},N:N,N{0})+COUNTIFS(I:I,I{0},N:N,N{0}-TIME(0,{1},0))+COUNTIFS(I:I,I{0},N:N,N{0}+TIME(0,{1},0))";

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    #region Sheet 1: Bang filter loi
//                    ISheet xlSheetFilter = workbook.CreateSheet("DS Trùng");
//                    int cols = CreateHeader(xlSheetFilter, cellStyles,
//                                "Danh sách Trùng mã máy và giờ kết quả tại XML4", // Sheet title
//                                "STT", "Tên file", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ",
//                                "Mã Dịch vụ", "Tên dịch vụ", "Mã Máy", //"Mã Chỉ số", "Tên Chỉ số",
//                                                                       //"Giá trị", "Mô tả", "Kết luận",
//                                "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//                    IRow row = xlSheetFilter.CreateRow(2);
//                    ICell cell = row.CreateCell(0);
//                    cell.SetCellValue($"=FILTER('XML4'!A3:M{(allXml4.Count() + 2)},'XML4'!O3:O{(allXml4.Count() + 2)}>=2,\"\")");

//                    SetAutoFeatures(xlSheetFilter, cols);
//                    #endregion


//                    #region Sheet 1: Danh sach all XML4
//                    ISheet xlSheetX4 = workbook.CreateSheet("XML4");
//                    int rows = 2;
//                    cols = CreateHeader(xlSheetX4, cellStyles,
//                                "Danh sách Mã máy và giờ kết quả tại XML4", // Sheet title
//                                "STT", "Tên file", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ",
//                                "Mã Dịch vụ", "Tên dịch vụ", "Mã Máy", //"Mã Chỉ số", "Tên Chỉ số",
//                                                                       //"Giá trị", "Mô tả", "Kết luận",
//                                "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

//                    string maDV = "";
//                    string tenDV = "";

//                    foreach (var t in allXml4)
//                    {
//                        string mmSoSanh = string.Format(fmSoSanh, rows + 1, 2);
//                        if (t.MA_MAY.StartsWith("CL") || t.MA_MAY.StartsWith("CT"))
//                        {
//                            mmSoSanh = string.Format(fmSoSanh, rows + 1, 4);
//                        }
//                        else if (t.MA_MAY.StartsWith("MR"))
//                        {
//                            mmSoSanh = string.Format(fmSoSanh, rows + 1, 9);
//                        }

//                        if (t.MA_DICH_VU != maDV)
//                        {
//                            maDV = t.MA_DICH_VU;
//                            tenDV = (dataDV.FirstOrDefault(x => x.MA_DICH_VU == t.MA_DICH_VU)?.TEN_DICH_VU) ?? "";
//                        }

//                        AppendRow(xlSheetX4, cellStyles, rows++,
//                                rows - 2, t.XML1.FileName, t.MA_LK, t.XML1.MA_BN, t.XML1.HO_TEN, t.XML1.MA_THE,
//                                t.MA_DICH_VU, tenDV, t.MA_MAY, //t.MA_CHI_SO, t.TEN_CHI_SO,
//                                                               //t.GIA_TRI, t.MO_TA, t.KET_LUAN,
//                                t.NGAY_KQ, t.XML1.NGAY_VAO, t.XML1.NGAY_RA, t.XML1.NGAY_TTOAN,
//                                string.Format("{0:yyyy/MM/dd HH:mm:ss}", t.NGAY_KQ), mmSoSanh
//                            );
//                    }

//                    SetAutoFeatures(xlSheetX4, cols);
//                    #endregion

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;

//            }
//            catch { }

//            return null;
//        }

//        //Linhist removed 20/11/23
//        //#region Linhist added 19/10/23
//        ////Linhist added 18/10/23
//        //static public byte[] DownloadExcelWorkDays_New(IEnumerable<XML2> data2, IEnumerable<XML3> data3, IEnumerable<DMBacSi> dmBacsis, DateTime dStart, DateTime dEnd)
//        //{
//        //    string[] dows = new string[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

//        //    try
//        //    {
//        //        //Create new excel
//        //        ICellStyle[] cellStyles = null;
//        //        IWorkbook workbook = CreateWorkBook(out cellStyles);

//        //        var allWorks = data2.GroupBy(x => new { x.MA_BAC_SI, NGAY_YL = x.NGAY_YL.Value.Date }, (x, g) => new { x.NGAY_YL, x.MA_BAC_SI })
//        //                            .Union(data3.GroupBy(x => new { x.MA_BAC_SI, NGAY_YL = x.NGAY_YL.Value.Date }, (x, g) => new { x.NGAY_YL, x.MA_BAC_SI }))
//        //                            .Distinct().OrderBy(w => w.NGAY_YL);

//        //        byte[] result = null;
//        //        using (var fs = new MemoryStream())
//        //        {
//        //            ISheet xlSheet = workbook.CreateSheet("Chi tiết");
//        //            var flds = new List<string>() { };

//        //            int rows = 5;
//        //            IRow xrow = xlSheet.CreateRow(0);
//        //            xrow.HeightInPoints = 36f;

//        //            ICell xcel = xrow.CreateCell(0);
//        //            xcel.SetCellValue($"Bảng Chấm công trong Tháng {dStart:MM} năm {dStart.Year}".ToUpper());
//        //            xcel.CellStyle = cellStyles[0];

//        //            xrow = xlSheet.CreateRow(1);
//        //            xrow = xlSheet.CreateRow(2);
//        //            xcel = xrow.CreateCell(0);
//        //            xcel.SetCellValue("Ghi chú: [Đ]: Đi làm, [N]: Nghỉ");
//        //            xlSheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 3));

//        //            xrow = xlSheet.CreateRow(3);
//        //            xrow.HeightInPoints = 24f;

//        //            string[] caps = new string[] { "STT", "Mã CCHN", "Họ tên", "Tên khoa" };
//        //            dmBacsis = dmBacsis.OrderBy(b => b.MA_KHOA).ThenBy(b => b.HO_TEN).ToList();
//        //            for (int i = 0; i < caps.Length; i++)
//        //            {
//        //                xcel = xrow.CreateCell(i);
//        //                xcel.SetCellValue(caps[i].ToUpper());
//        //                xcel.CellStyle = cellStyles[1];

//        //                ICellStyle style = xlSheet.Workbook.CreateCellStyle();
//        //                style.Alignment = HorizontalAlignment.Center;
//        //                if (i == 2 || i == 3)
//        //                {
//        //                    style.Alignment = HorizontalAlignment.Left;
//        //                }

//        //                xlSheet.SetDefaultColumnStyle(i, style);
//        //            }

//        //            xrow = xlSheet.CreateRow(4);
//        //            xrow.HeightInPoints = 24f;
//        //            for (int i = 0; i < caps.Length; i++)
//        //            {
//        //                xcel = xrow.CreateCell(i);
//        //                xcel.CellStyle = cellStyles[1];
//        //                xlSheet.AddMergedRegion(new CellRangeAddress(3, 4, i, i));
//        //            }

//        //            foreach (var bs in dmBacsis)
//        //            {
//        //                var vals = new List<string>() { (rows - 5).ToString(), bs.MACCHN, bs.HO_TEN, bs.TEN_KHOA };
//        //                AppendRow(xlSheet, cellStyles, rows++, vals.ToArray());
//        //            }

//        //            int cols = caps.Length - 1;
//        //            while (dStart < dEnd)
//        //            {
//        //                cols++;
//        //                //Add header for date
//        //                xcel = xlSheet.GetRow(3).CreateCell(cols);
//        //                var celStyle = (dStart.DayOfWeek == DayOfWeek.Saturday || dStart.DayOfWeek == DayOfWeek.Sunday) ? cellStyles[2] : cellStyles[1];
//        //                xcel.SetCellValue($"{dStart:dd}");
//        //                xcel.CellStyle = celStyle;

//        //                xcel = xlSheet.GetRow(4).CreateCell(cols);
//        //                xcel.SetCellValue($"{dows[(int)dStart.DayOfWeek]}");
//        //                xcel.CellStyle = celStyle;

//        //                //Add data for each bacsi
//        //                rows = 5;
//        //                celStyle = (dStart.DayOfWeek == DayOfWeek.Saturday || dStart.DayOfWeek == DayOfWeek.Sunday) ? cellStyles[6] : cellStyles[3];
//        //                var mabs = string.Join(';', allWorks.Where(w => w.NGAY_YL == dStart).Select(w => w.MA_BAC_SI).ToArray());
//        //                foreach (var bs in dmBacsis)
//        //                {
//        //                    xcel = xlSheet.GetRow(rows).CreateCell(cols);
//        //                    xcel.CellStyle = celStyle;
//        //                    if (dStart > DateTime.Now)
//        //                    {
//        //                        xcel.SetCellValue("");
//        //                    }
//        //                    else
//        //                    {
//        //                        xcel.SetCellValue(mabs.Contains(bs.MACCHN) ? "Đ" : "N");
//        //                    }
//        //                    rows++;
//        //                }

//        //                dStart = dStart.AddDays(1);
//        //                rows++;
//        //            }

//        //            xlSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
//        //            SetAutoFeatures(xlSheet, cols + 1);
//        //            workbook.Write(fs);
//        //            result = fs.ToArray();
//        //        }

//        //        return result;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        string stk = ex.StackTrace;
//        //    }

//        //    return null;
//        //}
//        //#endregion




//        //Linhist added 20/11/23
//        static private int CreateHeaderForWorkDays(ISheet xlSheet, DateTime dStart, ICellStyle[] cellStyles)
//        {
//            IRow xrow = xlSheet.CreateRow(0);
//            xrow.HeightInPoints = 36f;

//            ICell xcel = xrow.CreateCell(0);
//            xcel.SetCellValue($"Bảng Chấm công trong Tháng {dStart:MM} năm {dStart.Year}".ToUpper());
//            xcel.CellStyle = cellStyles[0];

//            xrow = xlSheet.CreateRow(1);
//            xcel = xrow.CreateCell(0);
//            xcel.SetCellValue("Ghi chú: [Đ]: Đi làm, [N]: Nghỉ");

//            xlSheet.CreateRow(2);
//            xrow = xlSheet.CreateRow(3);
//            xrow.HeightInPoints = 24f;

//            string[] caps = new string[] { "STT", "Mã CCHN", "Họ tên", "Chức danh", "Mã khoa", "Tên khoa" };
//            for (int i = 0; i < caps.Length; i++)
//            {
//                xcel = xrow.CreateCell(i);
//                xcel.SetCellValue(caps[i].ToUpper());
//                xcel.CellStyle = cellStyles[1];

//                ICellStyle style = xlSheet.Workbook.CreateCellStyle();
//                style.Alignment = HorizontalAlignment.Center;
//                if (i == 2 || i == 3)
//                {
//                    style.Alignment = HorizontalAlignment.Left;
//                }

//                xlSheet.SetDefaultColumnStyle(i, style);
//            }

//            xrow = xlSheet.CreateRow(4);
//            xrow.HeightInPoints = 24f;
//            for (int i = 0; i < caps.Length; i++)
//            {
//                xcel = xrow.CreateCell(i);
//                xcel.CellStyle = cellStyles[1];
//                xlSheet.AddMergedRegion(new CellRangeAddress(3, 4, i, i));
//            }

//            return caps.Length - 1;
//        }

//        static private void AddDateHeaderForWorkDays(ISheet xlSheet, int col, DateTime dte, string dayOfWeek, ICellStyle celStyle)
//        {
//            //Add header for date
//            ICell xcel = xlSheet.GetRow(3).CreateCell(col);
//            xcel.SetCellValue($"{dte:dd}");
//            xcel.CellStyle = celStyle;

//            xcel = xlSheet.GetRow(4).CreateCell(col);
//            xcel.SetCellValue($"{dayOfWeek}");
//            xcel.CellStyle = celStyle;
//        }

//        static public byte[] DownloadExcelWorkDays_New(IEnumerable<XML2> data2, IEnumerable<XML3> data3, IEnumerable<DMBacSi> dmBacsis, DateTime dStart, DateTime dEnd)
//        {
//            string[] dows = new string[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };
//            string[] cdnn = new string[] { "", "Bác sĩ", "Y sĩ", "Điều dưỡng", "Hộ sinh", "Kỹ thuật viên", "Cử nhân X-quang", "Dược sĩ đại học", "Dược sĩ trung cấp", "Lương y", "Cử nhân xét nghiệm" };

//            try
//            {
//                //Create new excel
//                ICellStyle[] cellStyles = null;
//                IWorkbook workbook = CreateWorkBook(out cellStyles);

//                var allBS = dmBacsis.Where(b => b.CHUCDANH_NN == "1").OrderBy(b => b.MA_KHOA);
//                var notBS = dmBacsis.Where(b => b.CHUCDANH_NN != "1").OrderBy(b => b.MA_KHOA);

//                var data3YL = data3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11))
//                                                .Select(x => new { MA_BAC_SI = allBS.FirstOrDefault(b => x.MA_BAC_SI == b.MACCHN || x.MA_BAC_SI.StartsWith(b.MACCHN + ";"))?.MACCHN, x.NGAY_YL })
//                                                .Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI));
//                var data3VT = data3.Where(x => Uti.NumExists(x.MA_NHOM, 10, 11) && x.NGAY_YL < dEnd);
//                var data3KQ = data3.Where(x => !Uti.NumExists(x.MA_NHOM, 10, 11) && x.NGAY_KQ >= dStart);

//                byte[] result = null;
//                using (var fs = new MemoryStream())
//                {
//                    var dataBS = data2.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date })
//                                    .Union(data3YL.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date }))
//                                    .Distinct()
//                                    .GroupBy(x => x.NGAY_LV, (x, g) => new { MA_BAC_SI = ";" + string.Join(';', g.Select(x => x.MA_BAC_SI).Distinct().ToArray()) + ";", NGAY_LV = x })
//                                    .ToList();

//                    var dataKT = data3KQ.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_KQ.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date })
//                                    .Union(data3VT.GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date }, (x, g) => new { x.MA_BAC_SI, NGAY_LV = x.Date }))
//                                    .Distinct()
//                                    .GroupBy(x => x.NGAY_LV, (x, g) => new { MA_BAC_SI = ";" + string.Join(';', g.Select(x => x.MA_BAC_SI).Distinct().ToArray()) + ";", NGAY_LV = x })
//                                    .ToList();

//                    ISheet xlSheetBS = workbook.CreateSheet("Bác sĩ");
//                    int colBS = CreateHeaderForWorkDays(xlSheetBS, dStart, cellStyles);
//                    ISheet xlSheetKT = workbook.CreateSheet("Kỹ thuật viên");
//                    int colKT = CreateHeaderForWorkDays(xlSheetKT, dStart, cellStyles);

//                    //Add data for each bacsi
//                    int rows = 5;
//                    int cds;
//                    foreach (var bs in allBS)
//                    {
//                        cds = 0;
//                        int.TryParse(bs.CHUCDANH_NN, out cds);
//                        AppendRow(xlSheetBS, cellStyles, rows++, rows - 5, bs.MACCHN, bs.HO_TEN, cdnn[cds], bs.MA_KHOA, bs.TEN_KHOA);
//                    }

//                    //Add data for each kythuatvien
//                    rows = 5;
//                    foreach (var bs in notBS)
//                    {
//                        cds = 0;
//                        int.TryParse(bs.CHUCDANH_NN, out cds);
//                        AppendRow(xlSheetKT, cellStyles, rows++, rows - 5, bs.MACCHN, bs.HO_TEN, cds >= cdnn.Length ? cds : cdnn[cds], bs.MA_KHOA, bs.TEN_KHOA);
//                    }

//                    ICell xcel;
//                    ICellStyle celStyle;
//                    int cols = colBS;
//                    DateTime dte = dStart;
//                    while (dte < dEnd)
//                    {
//                        cols++;
//                        celStyle = (dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday) ? cellStyles[2] : cellStyles[1];

//                        //Add header for date
//                        AddDateHeaderForWorkDays(xlSheetBS, cols, dte, dows[(int)dte.DayOfWeek], celStyle);
//                        AddDateHeaderForWorkDays(xlSheetKT, cols, dte, dows[(int)dte.DayOfWeek], celStyle);

//                        //Add work days for each bacsi
//                        rows = 5;
//                        celStyle = (dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday) ? cellStyles[6] : cellStyles[3];
//                        var maBS = dte > DateTime.Now ? "" : dataBS.FirstOrDefault(w => w.NGAY_LV == dte)?.MA_BAC_SI;
//                        foreach (var bs in allBS)
//                        {
//                            xcel = xlSheetBS.GetRow(rows++).CreateCell(cols);
//                            xcel.CellStyle = celStyle;
//                            if (dte > DateTime.Now)
//                            {
//                                xcel.SetCellValue("");
//                            }
//                            else
//                            {
//                                xcel.SetCellValue(maBS.Contains($";{bs.MACCHN};") ? "Đ" : "N");
//                            }
//                        }

//                        //Add work days for each kythuatvien
//                        rows = 5;
//                        var maKT = dte > DateTime.Now ? "" : dataKT.FirstOrDefault(w => w.NGAY_LV == dte)?.MA_BAC_SI;
//                        foreach (var bs in notBS)
//                        {
//                            xcel = xlSheetKT.GetRow(rows++).CreateCell(cols);
//                            xcel.CellStyle = celStyle;
//                            if (dte > DateTime.Now)
//                            {
//                                xcel.SetCellValue("");
//                            }
//                            else
//                            {
//                                xcel.SetCellValue(maKT.Contains($";{bs.MACCHN};") ? "Đ" : "N");
//                            }
//                        }

//                        dte = dte.AddDays(1);
//                    }

//                    xlSheetBS.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
//                    xlSheetBS.AddMergedRegion(new CellRangeAddress(1, 1, 0, cols));
//                    SetAutoFeatures(xlSheetBS, cols + 1);

//                    xlSheetKT.AddMergedRegion(new CellRangeAddress(0, 0, 0, cols));
//                    xlSheetKT.AddMergedRegion(new CellRangeAddress(1, 1, 0, cols));
//                    SetAutoFeatures(xlSheetKT, cols + 1);

//                    workbook.Write(fs);
//                    result = fs.ToArray();
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                string stk = ex.StackTrace;
//            }

//            return null;
//        }
//    }
//}