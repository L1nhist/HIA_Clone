using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Categories;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Files130;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Services.Categories;
using HIA.Services.Files;
using HIA.Services.HosoManagement130;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HIA.Services.Report
{
    public class ReportHandleService : IReportHandleService
    {
        private const int Excel_ColWide = 256 * 72;
        private const int Excel_ColMild = 256 * 28;

        #region Private Static Methods
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

        private static void AppendRow(ISheet sheet, ICellStyle[] cellStyles, int rowIdx = 0, params object[] values)
        {
            if (values == null || values.Length == 0) return;

            IRow row = sheet.CreateRow(rowIdx);
            for (int i = 0; i < values.Length; i++)
            {
                ICell cell = row.CreateCell(i);

                ICellStyle style = cellStyles == null || cellStyles.Length < 1 ? null : cellStyles.Length > 5 ? cellStyles[3] : cellStyles?[0];
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

        private static int CreateHeader(ISheet sheet, ICellStyle[] cellStyles, string title, params string[] columns)
        {
            int rIdx = 0;
            if (columns == null || columns.Length == 0) return 0;

            if (!string.IsNullOrEmpty(title))
            {
                var cellRanged = new CellRangeAddress(0, 0, 0, columns.Length - 1);
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

        //Linhist changed 20/02/25
        //private static int CreateHeader_ForWorkDays(ISheet xlSheet, DateTime dStart, ICellStyle[] cellStyles)
        //{
        //    IRow xrow = xlSheet.CreateRow(0);
        //    xrow.HeightInPoints = 36f;

        //    ICell xcel = xrow.CreateCell(0);
        //    xcel.SetCellValue($"Bảng Chấm công trong Tháng {dStart:MM} năm {dStart.Year}".ToUpper());
        //    xcel.CellStyle = cellStyles[0];

        //    xrow = xlSheet.CreateRow(1);
        //    xcel = xrow.CreateCell(0);
        //    xcel.SetCellValue("Ghi chú: [Đ]: Đi làm, [N]: Nghỉ");

        //    xlSheet.CreateRow(2);
        //    xrow = xlSheet.CreateRow(3);
        //    xrow.HeightInPoints = 24f;

        //    string[] caps = new string[] { "STT", "Mã CCHN", "Họ tên", "Chức danh", "Mã khoa", "Tên khoa" };
        //    for (int i = 0; i < caps.Length; i++)
        //    {
        //        xcel = xrow.CreateCell(i);
        //        xcel.SetCellValue(caps[i].ToUpper());
        //        xcel.CellStyle = cellStyles[1];

        //        ICellStyle style = xlSheet.Workbook.CreateCellStyle();
        //        style.Alignment = HorizontalAlignment.Center;
        //        if (i == 2 || i == 3)
        //        {
        //            style.Alignment = HorizontalAlignment.Left;
        //        }

        //        xlSheet.SetDefaultColumnStyle(i, style);
        //    }

        //    xrow = xlSheet.CreateRow(4);
        //    xrow.HeightInPoints = 24f;
        //    for (int i = 0; i < caps.Length; i++)
        //    {
        //        xcel = xrow.CreateCell(i);
        //        xcel.CellStyle = cellStyles[1];
        //        xlSheet.AddMergedRegion(new CellRangeAddress(3, 4, i, i));
        //    }

        //    return caps.Length - 1;
        //}
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

        private static void CreateLookupSheet(IWorkbook workbook, ICellStyle[] cellStyles, IEnumerable<DMBacSi> dmBacsis, IEnumerable<DMKhoaBV> dmKhoas)
        {
            int rows = 1, cols = 0;
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
                    dKhoa[x.MA_KHOA] = x.TEN_KHOA;
                    AppendRow(xlSheetK, cellStyles, rows++, x.MA_KHOA, x.TEN_KHOA);
                }
                SetAutoFeatures(xlSheetK, cols);
            }
            #endregion

            #region Sheet phu: DM Bac si
            if (dmBacsis != null)
            {
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";
                var dataBS = dmBacsis.OrderBy(b => b.MA_CCHN);
                ISheet xlSheetB = workbook.CreateSheet("DM Bác sĩ");
                rows = 2;
                cols = CreateHeader(xlSheetB, cellStyles, "", "Mã CNHN", "Tên Bác sĩ", "Chức danh", "Mã Khoa", "Tên Khoa", "Mã BHXH");

                foreach (var x in dataBS)
                {
                    var khoa = (x.MA_KHOA ?? "").Split(';')[0].Trim();
                    khoa = dKhoa.TryGetValue(khoa, out string ten) ? ten : "";
                    //AppendRow(xlSheetB, cellStyles, rows++, x.MACCHN, x.HO_TEN, x.CHUCDANH_NN, x.MA_KHOA, khoa, x.MA_BHXH);
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
                var request = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
                selDte = new DateTime(request.FromDateCreate.Value.Year, request.FromDateCreate.Value.Month, 1);
            }
            catch { }

            return selDte;
        }

        private static string[] GetFormular_BacSi(string[] fmBacsis, ref string MA_BACSI)
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

        private static string GetFormular_KhoaTH(string fmBacsi, string MA_BACSI)
        {
            if (string.IsNullOrWhiteSpace(MA_BACSI)) return "";

            //Loai tru trng hop MA_BS co 2 chuoi
            MA_BACSI = MA_BACSI.Replace("; ", "$");
            string[] lstBs = MA_BACSI.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return "$=" + string.Format(fmBacsi, lstBs.Length > 0 ? lstBs[0] : "");
        }

        private static string GetFormular_TenKhoa(string fmKhoa, string MA_KHOA, bool getLast = false)
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

        private static string GetNgayThang(DateTime? ngayThang, bool noTime = false)
            => noTime ? $"{ngayThang:dd/MM/yyyy}" : $"{ngayThang:dd/MM/yyyy HH:mm:ss}";

        private static string GetType_BacSi(string CHUCDANH_NN)
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
        private static string FixPeriod(string value)
        {
            if (string.IsNullOrEmpty(value)) return "N";

            var res = "";
            if (value.Contains("S")) res = "S";
            if (value.Contains("C")) res += (res.Length > 0 ? "-" : "") + "C";
            if (value.Contains("T")) res += (res.Length > 0 ? "-" : "") + "T";
            return res;
        }

        private static void SetAutoFeatures(ISheet sheet, int cols, params int[] widens)
        {
            for (int i = 0; i < cols; i++)
            {
                if (widens != null)
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
                }

                sheet.AutoSizeColumn(i);
            }
        }
        #endregion

        private readonly IMapper _mapper;
        private readonly ILogger<ReportHandleService> _logger;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;

        private readonly IBaseRepository<Hospital> _hospitalRepository;
        //private readonly IBaseRepository<DMDichVu> _dmDichVuRepository;
        private readonly IDMErrorService130 _dmErrorService;
        private readonly IBaseRepository<DMBacSi> _dmBacSiRepository;
        private readonly IBaseRepository<DMKhoaBV> _dmKhoaRepository;
        private readonly IBaseRepository<ChiDinhDongThoi> _dmDongThoiRepository;

        private readonly IXml01Repository _hosoRepository;
        //private readonly IBaseRepository<CheckChanges> _chgRepository;
        private readonly IBaseRepository<XMLState> _stateRepository;
        private readonly IBaseRepository<XML02> _xml02Repository;
        private readonly IBaseRepository<XML03> _xml03Repository;
        private readonly IBaseRepository<XML04> _xml04Repository;
        private readonly IBaseRepository<XML05> _xml05Repository;
        private readonly IBaseRepository<XML06> _xml06Repository;
        private readonly IBaseRepository<XML07> _xml07Repository;
        private readonly IBaseRepository<XML08> _xml08Repository;
        private readonly IBaseRepository<XML09> _xml09Repository;
        private readonly IBaseRepository<XML10> _xml10Repository;
        private readonly IBaseRepository<XML11> _xml11Repository;
        private readonly IBaseRepository<XML13> _xml13Repository;
        private readonly IBaseRepository<XML14> _xml14Repository;
        private readonly IBaseRepository<XML15> _xml15Repository;
        private readonly IBaseRepository<XMLError> _errorRepository;
        private readonly IBaseRepository<User> _userRepository;
        private readonly IBaseRepository<UserLogs> _ulogRepository;

        public ReportHandleService(
            IMapper mapper,
            ILogger<ReportHandleService> logger,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,

            IBaseRepository<Hospital> hospitalRepository,
            IDMErrorService130 dmErrorService,
            IBaseRepository<DMKhoaBV> dmKhoaRepository,
            IBaseRepository<DMBacSi> dmBacSiRepository,
            IBaseRepository<ChiDinhDongThoi> dmDongThoiRepository,

            IXml01Repository hosoRepository,
            //IBaseRepository<CheckChanges> chgRepository,
            IBaseRepository<XMLState> xml00Repository,
            IBaseRepository<XML02> xml02Repository,
            IBaseRepository<XML03> xml03Repository,
            IBaseRepository<XML04> xml04Repository,
            IBaseRepository<XML05> xml05Repository,
            IBaseRepository<XML06> xml06Repository,
            IBaseRepository<XML07> xml07Repository,
            IBaseRepository<XML08> xml08Repository,
            IBaseRepository<XML09> xml09Repository,
            IBaseRepository<XML10> xml10Repository,
            IBaseRepository<XML11> xml11Repository,
            IBaseRepository<XML13> xml13Repository,
            IBaseRepository<XML14> xml14Repository,
            IBaseRepository<XML15> xml15Repository,
            IBaseRepository<XMLError> errorRepository,
            IBaseRepository<User> userRepository,
            IBaseRepository<UserLogs> ulogRepository
        )
        {
            _mapper = mapper;
            _logger = logger;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;

            _hospitalRepository = hospitalRepository;
            _dmErrorService = dmErrorService;
            _dmBacSiRepository = dmBacSiRepository;
            _dmKhoaRepository = dmKhoaRepository;
            _dmDongThoiRepository = dmDongThoiRepository;

            _hosoRepository = hosoRepository;
            //_chgRepository = chgRepository;
            _stateRepository = xml00Repository;
            _xml02Repository = xml02Repository;
            _xml03Repository = xml03Repository;
            _xml04Repository = xml04Repository;
            _xml05Repository = xml05Repository;
            _xml06Repository = xml06Repository;
            _xml07Repository = xml07Repository;
            _xml08Repository = xml08Repository;
            _xml09Repository = xml09Repository;
            _xml10Repository = xml10Repository;
            _xml11Repository = xml11Repository;
            _xml13Repository = xml13Repository;
            _xml14Repository = xml14Repository;
            _xml15Repository = xml15Repository;
            _errorRepository = errorRepository;
            _userRepository = userRepository;
            _ulogRepository = ulogRepository;
        }

        #region Private Methods
        //Mac dinh includes se lay tat ca XML tu 02-11: "2;3;4;5;6;7;8;9;10;11;13;14;15"
        //Neu chi dinh ro [inners] thi se lay XML theo so chi dinh vd: "2;3;5;7"
        private async Task AttachInnerXML(IEnumerable<XML01> xmls, string innerList = null)
        {
            innerList = innerList?.Trim();
            string[] inners = "2;3;4;5;6;7;8;9;10;11;13;14;15".Split(';');
            if (!string.IsNullOrEmpty(innerList))
            {
                var lst = innerList.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (lst.Length > 0)
                {
                    inners = lst;
                }
            }

            try
            {
                var lstIds = xmls.Select(x => x.XML01ID).ToList();
                string maCSKCB = xmls.FirstOrDefault().MA_CSKCB;
                IEnumerable<XML02> xml02s = null;
                IEnumerable<XML03> xml03s = null;
                IEnumerable<XML04> xml04s = null;
                IEnumerable<XML05> xml05s = null;
                IEnumerable<XML06> xml06s = null;
                IEnumerable<XML07> xml07s = null;
                IEnumerable<XML08> xml08s = null;
                IEnumerable<XML09> xml09s = null;
                IEnumerable<XML10> xml10s = null;
                IEnumerable<XML11> xml11s = null;
                IEnumerable<XML13> xml13s = null;
                IEnumerable<XML14> xml14s = null;
                IEnumerable<XML15> xml15s = null;
                try
                {
                    xml02s = !inners.Contains("2") ? null : await _xml02Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML2-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML02");

                }
                try
                {
                    xml03s = !inners.Contains("3") ? null : await _xml03Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML3-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count}  nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML03");
                }
                try
                {
                    xml04s = !inners.Contains("4") ? null : await _xml04Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML4-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML04");
                }
                try
                {
                    xml05s = !inners.Contains("5") ? null : await _xml05Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML5-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML05");
                }
                try
                {
                    xml06s = !inners.Contains("6") ? null : await _xml06Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML6-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML06");
                }
                try
                {
                    xml07s = !inners.Contains("7") ? null : await _xml07Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML7-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML07");
                }
                try
                {
                    xml08s = !inners.Contains("8") ? null : await _xml08Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML8-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML08");
                }
                try
                {
                    xml09s = !inners.Contains("9") ? null : await _xml09Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khí lấy thông tin XML9-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML09");
                }
                try
                {
                    xml10s = !inners.Contains("10") ? null : await _xml10Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khí lấy thông tin XML10-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML10");
                }
                try
                {
                    xml11s = !inners.Contains("11") ? null : await _xml11Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch (Exception ex)
                { 
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML11-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML11");
                }
                try
                {
                    xml13s = !inners.Contains("13") ? null : await _xml13Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML13-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML13");
                }
                try
                {
                    xml14s = !inners.Contains("14") ? null : await _xml14Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML14-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML14");
                }
                try
                {
                    xml15s = !inners.Contains("15") ? null : await _xml15Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));
                }
                catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin XML15-BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{lstIds.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML15");
                }

                foreach (var xml01 in xmls)
                {
                    var x02 = xml02s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x02 != null) xml01.AddRange(x02);

                    var x03 = xml03s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x03 != null) xml01.AddRange(x03);

                    var x04 = xml04s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x04 != null) xml01.AddRange(x04);

                    var x05 = xml05s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x05 != null) xml01.AddRange(x05);

                    var x06 = xml06s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x06 != null) xml01.AddRange(x06);

                    var x07 = xml07s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x07 != null) xml01.AddRange(x07);

                    var x08 = xml08s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x08 != null) xml01.AddRange(x08);

                    var x09 = xml09s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x09 != null) xml01.AddRange(x09);

                    var x10 = xml10s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x10 != null) xml01.AddRange(x10);

                    var x11 = xml11s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x11 != null) xml01.AddRange(x11);

                    var x13 = xml13s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x13 != null) xml01.AddRange(x13);

                    var x14 = xml14s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x14 != null) xml01.AddRange(x14);

                    var x15 = xml15s?.Where(x => x.XML01ID == xml01.XML01ID);
                    if (x15 != null) xml01.AddRange(x15);
                }
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)}");
            }
        }

        private async Task<IEnumerable<XMLError>> AttachXMLError(string ma_CSKCB, IEnumerable<XML01> x01s)
        {
            List<long> lstXMLIDs = x01s.Select(x => x.XML01ID).ToList();
            //IEnumerable<XMLError> dataE = new IEnumerable<XMLError>();
            IEnumerable<XMLError> dataE = new List<XMLError>();
            try
            {
                dataE = await _errorRepository.GetListAsync(x => x.MA_CSKCB == ma_CSKCB && lstXMLIDs.Contains(x.XML01ID));
            }
            catch(Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin BC Tổng hợp, Đơn vị:{ma_CSKCB}-Số lượng hồ sơ:{lstXMLIDs.Count} nội dung lỗi:" + ex.Message.ToString(), $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - XML-Error");
            }

            //Gan XML1 vao cac XMLErrors
            //foreach (var x1 in x01s)
            //{
            //    var dtE = dataE?.Where(x => x.XML01ID == x1.XML01ID);
            //    if (dtE == null || !dtE.Any()) continue;
            //    foreach (var xe in dtE)
            //    {
            //        xe.XML01 = x1;
            //        x1.Errors.Add(xe);
            //    }
            //}
            return dataE;
        }

        private FilterQueryableModel<XML01> BuildQuery_XML01(FilterDownloadModel request, bool hasIncludes = true)
        {
            var result = new FilterQueryableModel<XML01>();

            #region Build Filter Query
            var filter = PredicateBuilder.New<XML01>(x => x.IsDelete != true);
            if (!string.IsNullOrEmpty(request.HospitalId))
            {
                filter = filter.And(x => x.MA_CSKCB == request.HospitalId );
            }
            if (!string.IsNullOrEmpty(request.MaCSKCB))
            {
                filter = filter.And(x => x.MA_CSKCB == request.MaCSKCB);
            }
            if (!string.IsNullOrWhiteSpace(request.SearchKey))
            {
                var key = request.SearchKey.ToLower();
                string pattern = request.SearchKey;

                var filterSearch = PredicateBuilder.New<XML01>();
                filterSearch = filterSearch.Or(x => x.MA_THE_BHYT != null && x.MA_THE_BHYT.Contains(pattern))
                                            .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern))
                                            .Or(x => x.MA_BN != null && x.MA_BN.Contains(pattern))
                                            .Or(x => x.HO_TEN != null && x.HO_TEN.Contains(pattern));

                filter = filter.And(filterSearch);
            }
            //MA KHOA Filter
            //if (request.Departments != null && request.Departments.Any())
            //{
            //    //18.7.2024 haimaster update thử nghiệm
            //    var filterDep = PredicateBuilder.New<XML01>(true);
            //    foreach (var maKhoa in request.Departments)
            //    {
            //        //filterDep = filterDep.Or(x => x.MA_KHOA.EndsWith(maKhoa)).Or(x=>x.MA_KHOA.StartsWith(maKhoa));
            //        filterDep = filterDep.Or(x => x.MA_KHOA.EndsWith(maKhoa.Trim())).Or(x=>x.MA_KHOA.StartsWith(maKhoa.Trim()));
            //    }
            //    filter = filter.And(x => x.MA_KHOA != null).And(filterDep);
            //}
            //MA KHOA filter 30/12/2024
            //if (request.Departments != null && request.Departments.Any())
            //{
            //    var filterDep = PredicateBuilder.New<XML01>(false);
            //    foreach (var maKhoa in request.Departments)
            //    {
            //        string temp = maKhoa.Trim();
            //        filterDep = filterDep.Or(x => x.MA_KHOA.Contains(temp));
            //    }
            //    filter = PredicateBuilder.New<XML01>(x => x.MA_KHOA != null).And(filterDep);
            //}
            //Linhist changed 02/01/25
            if (request.Departments != null && request.Departments.Any())
            {
                var allDeps = ";" + string.Join(";", request.Departments.ToArray()) + ";";
                var filterDep = PredicateBuilder.New<XML01>(x => allDeps.Contains(";" + x.MA_KHOA + ";"));

                foreach (var maKhoa in request.Departments)
                {
                    filterDep = filterDep.Or(x => (";" + x.MA_KHOA + ";").Contains(";" + maKhoa + ";"));
                }
                filter = filter.And(x => x.MA_KHOA != null).And(filterDep);
            }
            // Create Date filter
            //if ((request.FromDateCreate != null && request.FromDateCreate.HasValue) || (request.ToDateCreate != null && request.ToDateCreate.HasValue))
            //{
            //    var filterCreate = PredicateBuilder.New<XML01>(x => x.CreateDate != null);
            //    if (request.FromDateCreate != null && request.FromDateCreate.HasValue)
            //    {
            //        DateTime dayFrom = request.FromDateCreate.Value.Date;
            //        filterCreate = filterCreate.And(x => x.CreateDate.Value >= dayFrom);
            //    }
            //    if (request.ToDateCreate != null && request.ToDateCreate.HasValue)
            //    {
            //        DateTime dayTo = request.ToDateCreate.Value.Date.AddDays(1);
            //        //if (dayTo > DateTime.Now) dayTo = DateTime.Now.AddHours(-2);

            //        filterCreate = filterCreate.And(x => x.CreateDate.Value < dayTo);

            //    }
            //    filter = filter.And(filterCreate);
            //}
            if (request.FromDateCreate != null && request.FromDateCreate.HasValue)
            {
                DateTime dayFrom = request.FromDateCreate.Value.Date;
                filter = filter.And(x => x.CreateDate.Value >= dayFrom);
            }
            if (request.ToDateCreate != null && request.ToDateCreate.HasValue)
            {
                DateTime dayTo = request.ToDateCreate.Value.Date.AddDays(1);
                filter = filter.And(x => x.CreateDate.Value < dayTo);

            }

            // NGAY_VAO filter
            //if (request.FromDateIn != null && request.FromDateIn.HasValue || request.ToDateIn != null && request.ToDateIn.HasValue)
            //{
            //    var filterIn = PredicateBuilder.New<XML01>(x => x.NGAY_VAO != null);
            //    if (request.FromDateIn != null && request.FromDateIn.HasValue)
            //    {
            //        DateTime dayInFrom = request.FromDateIn.Value.Date;
            //        filterIn = filterIn.And(x => x.NGAY_VAO.Value >= dayInFrom);
            //    }
            //    if (request.ToDateIn != null && request.ToDateIn.HasValue)
            //    {
            //        DateTime dayInTo = request.ToDateIn.Value.Date.AddDays(1);
            //        //if (dayInTo > DateTime.Now) dayInTo = DateTime.Now.AddHours(-2);
            //        filterIn = filterIn.And(x => x.NGAY_VAO.Value < dayInTo);
            //    }
            //    filter = filter.And(filterIn);
            //}
            if (request.FromDateIn != null && request.FromDateIn.HasValue)
            {
                DateTime dayInFrom = request.FromDateIn.Value.Date;
                filter = filter.And(x => x.NGAY_VAO.Value >= dayInFrom);
            }
            if (request.ToDateIn != null && request.ToDateIn.HasValue)
            {
                DateTime dayInTo = request.ToDateIn.Value.Date.AddDays(1);
                filter = filter.And(x => x.NGAY_VAO.Value < dayInTo);
            }

            // NGAY_RA filter
            //if (request.FromDateOut != null && request.FromDateOut.HasValue || request.ToDateOut != null && request.ToDateOut.HasValue)
            //{
            //    var filterOut = PredicateBuilder.New<XML01>(x => x.NGAY_RA != null);
            //    if (request.FromDateOut != null && request.FromDateOut.HasValue)
            //    {
            //        DateTime dayOutFrom = request.FromDateOut.Value.Date;
            //        filterOut = filterOut.And(x => x.NGAY_RA.Value >= dayOutFrom);
            //    }
            //    if (request.ToDateOut != null && request.ToDateOut.HasValue)
            //    {
            //        DateTime dayOutTo = request.ToDateOut.Value.Date.AddDays(1);
            //        //if (dayOutTo > DateTime.Now) dayOutTo = DateTime.Now.AddHours(-2);
            //        filterOut = filterOut.And(x => x.NGAY_RA.Value < dayOutTo);
            //    }
            //    filter = filter.And(filterOut);
            //}
            if (request.FromDateOut != null && request.FromDateOut.HasValue)
            {
                DateTime dayOutFrom = request.FromDateOut.Value.Date;
                filter = filter.And(x => x.NGAY_RA.Value >= dayOutFrom);
            }
            if (request.ToDateOut != null && request.ToDateOut.HasValue)
            {
                DateTime dayOutTo = request.ToDateOut.Value.Date.AddDays(1);
                filter = filter.And(x => x.NGAY_RA.Value < dayOutTo);
            }

            // NGAY_TTOAN filter
            //if (request.FromDatePaid != null && request.FromDatePaid.HasValue || request.ToDatePaid != null && request.ToDatePaid.HasValue)
            //{
            //    var filterPaid = PredicateBuilder.New<XML01>(x => x.NGAY_TTOAN != null);
            //    if (request.FromDatePaid != null && request.FromDatePaid.HasValue)
            //    {
            //        DateTime dayPaidFrom = request.FromDatePaid.Value.Date;
            //        filterPaid = filterPaid.And(x => x.NGAY_TTOAN.Value >= dayPaidFrom);
            //    }
            //    if (request.ToDatePaid != null && request.ToDatePaid.HasValue)
            //    {
            //        DateTime dayPaidTo = request.ToDatePaid.Value.Date.AddDays(1);
            //        //if (dayPaidTo > DateTime.Now) dayPaidTo = DateTime.Now.AddHours(-2);
            //        filterPaid = filterPaid.And(x => x.NGAY_TTOAN.Value < dayPaidTo);
            //    }
            //    filter = filter.And(filterPaid);
            //}
            if (request.FromDatePaid != null && request.FromDatePaid.HasValue)
            {
                DateTime dayPaidFrom = request.FromDatePaid.Value.Date;
                filter = filter.And(x => x.NGAY_TTOAN.Value >= dayPaidFrom);
            }
            if (request.ToDatePaid != null && request.ToDatePaid.HasValue)
            {
                DateTime dayPaidTo = request.ToDatePaid.Value.Date.AddDays(1);
                filter = filter.And(x => x.NGAY_TTOAN.Value < dayPaidTo);
            }

            // MA_LOAI_KCB filter
            if (!string.IsNullOrEmpty(request.TypeKCB?.Trim()))
            {
                filter = filter.And(x => x.MA_LOAI_KCB == request.TypeKCB.ToString() || x.MA_LOAI_KCB == "0" + request.TypeKCB.ToString());
            }

            // Error status filter
            var filterError = PredicateBuilder.New<XML01>(true);
            if (request.Errors != null && request.Errors.Any())
            {
                List<int?> errors = request.Errors.Select(e => Uti.ToNum(e)).Where(e => e != null).ToList();
                filterError = filterError.Or(x => x.Errors.Any(y => errors.Contains(y.MA_LOI)));
            }
            else if (true.Equals(request.Status))
            {
                //filterError = filterError.Or(x => (x.State.TotalErrors ?? 0) + (x.State.TotalAlerts ?? 0) == 0);
                filterError = filterError.Or(x => (x.State.TONG_SO_LOI ?? 0) + (x.State.CANH_BAO ?? 0) == 0);
            }
            else if (false.Equals(request.Status))
            {
                //filterError = filterError.Or(x => (x.State.TotalErrors ?? 0) + (x.State.TotalAlerts ?? 0) > 0);
                filterError = filterError.Or(x => (x.State.TONG_SO_LOI ?? 0) + (x.State.CANH_BAO ?? 0) > 0);
            }

            filter = filter.And(filterError);
            result.FilterQueryable = filter;
            #endregion

            #region Build Sort Query
            result.OrderQueryable = new List<Func<IOrderedQueryable<XML01>, IOrderedQueryable<XML01>>>
            {
                x => x.OrderByDescending(x => x.CreateDate)
            };
            #endregion

            #region Includes
            //if (hasIncludes)
            //{
            //    result.IncludeQueryable = null;
            //}
            #endregion

            return result;
        }

        private FilterQueryableModel<XML02> BuildQuery_XML02(FilterDownloadModel request, bool hasIncludes = true)
        {
            var result = new FilterQueryableModel<XML02>();

            #region Build Filter Query
            var filter = PredicateBuilder.New<XML02>(x => x.XML01.IsDelete != true);
            if (!string.IsNullOrEmpty(request.HospitalId))
            {
                filter = filter.And(x => x.MA_CSKCB == request.HospitalId);
            }
            //if (!string.IsNullOrWhiteSpace(request.SearchKey))
            //{
            //    var key = request.SearchKey.ToLower();
            //    string pattern = request.SearchKey;

            //    var filterSearch = PredicateBuilder.New<XML02>();
            //    filterSearch = filterSearch.Or(x => x.XML01.MA_THE_BHYT != null && x.XML01.MA_THE_BHYT.Contains(pattern))
            //                                .Or(x => x.XML01.MA_BN != null && x.XML01.MA_BN.Contains(pattern))
            //                                .Or(x => x.XML01.HO_TEN != null && x.XML01.HO_TEN.Contains(pattern))
            //                                .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern));

            //    filter = filter.And(filterSearch);
            //}

            //if (request.Departments != null && request.Departments.Any())
            //{
            //    var allDeps = ";" + string.Join(";", request.Departments.ToArray()) + ";";
            //    var filterDep = PredicateBuilder.New<XML02>(x => allDeps.Contains(";" + x.XML01.MA_KHOA + ";"));

            //    foreach (var maKhoa in request.Departments)
            //    {
            //        filterDep = filterDep.Or(x => (";" + x.XML01.MA_KHOA + ";").Contains(";" + maKhoa + ";"));
            //    }
            //    filter = filter.And(x => x.XML01.MA_KHOA != null).And(filterDep);
            //}

            var filterDate = PredicateBuilder.New<XML02>(x => x.NGAY_YL != null);
            var dates = new DateTime?[] { request.FromDateCreate, request.FromDateIn, request.FromDateOut, request.FromDatePaid };
            DateTime? dateBy = dates.Where(d => d != null && d.HasValue).Min();
            dates = new DateTime?[] { request.ToDateCreate, request.ToDateIn, request.ToDateOut, request.ToDatePaid };
            DateTime? dateTo = dates.Where(d => d != null && d.HasValue).Max();
            if (dateBy != null)
            {
                DateTime dayBy = dateBy.Value.Date;
                //filterDate = filterDate.And(x => x.NGAY_YL.Value >= dayBy || x.NGAY_TH_YL >= dayBy);
                filterDate = filterDate.And(x => x.NGAY_YL.Value >= dayBy);
            }
            if (dateTo != null)
            {
                DateTime dayTo = dateTo.Value.Date.AddDays(1);
                //filterDate = filterDate.And(x => x.NGAY_YL.Value < dayTo || x.NGAY_TH_YL < dayTo);
                filterDate = filterDate.And(x => x.NGAY_YL.Value < dayTo);
            }
            filter = filter.And(filterDate);

            // MA_LOAI_KCB filter
            //if (!string.IsNullOrEmpty(request.TypeKCB?.Trim()))
            //{
            //    filter = filter.And(x => x.XML01.MA_LOAI_KCB == request.TypeKCB.ToString() || x.XML01.MA_LOAI_KCB == "0" + request.TypeKCB.ToString());
            //}

            result.FilterQueryable = filter;
            #endregion

            return result;
        }

        private FilterQueryableModel<XML03> BuildQuery_XML03(FilterDownloadModel request, bool hasIncludes = true)
        {
            var result = new FilterQueryableModel<XML03>();

            #region Build Filter Query
            var filter = PredicateBuilder.New<XML03>(x => x.XML01.IsDelete != true);
            if (!string.IsNullOrEmpty(request.HospitalId))
            {
                filter = filter.And(x => x.MA_CSKCB == request.HospitalId);
            }
            //if (!string.IsNullOrWhiteSpace(request.SearchKey))
            //{
            //    var key = request.SearchKey.ToLower();
            //    string pattern = request.SearchKey;

            //    var filterSearch = PredicateBuilder.New<XML03>();
            //    filterSearch = filterSearch.Or(x => x.XML01.MA_THE_BHYT != null && x.XML01.MA_THE_BHYT.Contains(pattern))
            //                                .Or(x => x.XML01.MA_BN != null && x.XML01.MA_BN.Contains(pattern))
            //                                .Or(x => x.XML01.HO_TEN != null && x.XML01.HO_TEN.Contains(pattern))
            //                                .Or(x => x.MA_LK != null && x.MA_LK.Contains(pattern));

            //    filter = filter.And(filterSearch);
            //}

            //if (request.Departments != null && request.Departments.Any())
            //{
            //    var allDeps = ";" + string.Join(";", request.Departments.ToArray()) + ";";
            //    var filterDep = PredicateBuilder.New<XML03>(x => allDeps.Contains(";" + x.XML01.MA_KHOA + ";"));

            //    foreach (var maKhoa in request.Departments)
            //    {
            //        filterDep = filterDep.Or(x => (";" + x.XML01.MA_KHOA + ";").Contains(";" + maKhoa + ";"));
            //    }
            //    filter = filter.And(x => x.XML01.MA_KHOA != null).And(filterDep);
            //}

            var filterDate = PredicateBuilder.New<XML03>(x => x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null);
            var dates = new DateTime?[] { request.FromDateCreate, request.FromDateIn, request.FromDateOut, request.FromDatePaid };
            var dateBy = dates.Where(d => d != null && d.HasValue).Min();
            dates = new DateTime?[] { request.ToDateCreate, request.ToDateIn, request.ToDateOut, request.ToDatePaid };
            var dateTo = dates.Where(d => d != null && d.HasValue).Max();
            if (dateBy != null)
            {
                DateTime dayBy = dateBy.Value.Date;
                filterDate = filterDate.And(x => x.NGAY_YL.Value >= dayBy || x.NGAY_KQ.Value >= dayBy || x.NGAY_TH_YL.Value >= dayBy);
                dateTo = dateTo ?? dateBy.Value.AddDays(1);
            }
            if (dateTo != null)
            {
                DateTime dayTo = dateTo.Value.Date.AddDays(1);
                filterDate = filterDate.And(x => x.NGAY_YL.Value < dayTo || x.NGAY_KQ.Value < dayTo || x.NGAY_TH_YL.Value < dayTo);
            }
            filter = filter.And(filterDate);

            // MA_LOAI_KCB filter
            //if (!string.IsNullOrEmpty(request.TypeKCB?.Trim()))
            //{
            //    filter = filter.And(x => x.XML01.MA_LOAI_KCB == request.TypeKCB.ToString() || x.XML01.MA_LOAI_KCB == "0" + request.TypeKCB.ToString());
            //}

            result.FilterQueryable = filter;
            #endregion

            return result;
        }

        private async Task<IEnumerable<XMLErrorInfo>> GetDMErrors(string ma_CSKCB, IEnumerable<int?> errCodes)
        {
            //Linhist changed 110926
            //List<XMLErrorInfo> dmErrors = new();
            //IEnumerable<DMError130> lstErrors = null;
            //if (errCodes == null)
            //{
            //    lstErrors = await _dmErrorService.GetListAsync(x => x.ErrorCode != null);
            //}
            //else
            //{
            //    lstErrors = await _dmErrorService.GetListAsync(x => errCodes.Contains(x.ErrorCode));
            //}

            //var userErrList = await _dmErrorMHTCRepository.GetListAsync(x => x.MA_CSKCB == ma_CSKCB, null, null);

            ////Linhist changed 27/07/26
            ////foreach (var e in lstErrors)
            ////{
            ////    var ue = userErrList.FirstOrDefault(u => u.EMErrorCode == e.ErrorId);
            ////    dmErrors.Add(new XMLErrorInfo
            ////    {
            ////        error_code = e.ErrorCode,
            ////        error_file = e.ErrorFile,
            ////        error_tag = e.ErrorTag,
            ////        message = e.Message,
            ////        comment = e.Comment,
            ////        is_alert = ue?.EMEnabled == false,
            ////    });
            ////}
            ////return dmErrors;
            //return lstErrors.Join(userErrList, e => e.ErrorCode, u => u.EMErrorCode, (e, u) => new XMLErrorInfo
            //{
            //    error_code = e.ErrorCode,
            //    error_file = e.ErrorFile,
            //    error_tag = e.ErrorTag,
            //    message = e.Message,
            //    comment = e.Comment,
            //    is_alert = u.EMEnabled == false,
            //}).Distinct().ToList();

            var errors = await _dmErrorService.GetAllByHospital(ma_CSKCB);
            return errors.Select(e => new XMLErrorInfo
            {
                error_code = e.ErrorCode,
                error_file = e.ErrorAtXMLFile,
                error_tag = e.ErrorAtXMLTag,
                message = e.ErrorContent,
                comment = e.ErrorComment,
                is_alert = e.IsAlarm,
            }).ToList();
        }

        #endregion

        public async Task<ResponseModel<byte[]>> BaoCao_TongHop(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            try
            {
                var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
                filterModel ??= new FilterDownloadModel()
                {
                    FromDateCreate = DateTime.Now.Date,
                    ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
                    MaCSKCB = maCSKCB
                };

                if (string.IsNullOrEmpty(filterModel.MaCSKCB))
                {
                    filterModel.MaCSKCB = maCSKCB;
                }

                var queryable = FilesService130.BuildQueryToGetListData(filterModel);
                var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));

                //update xử lý các TH lấy báo cáo quá lớn
                if (data1.Count() >= 50000)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin BC Tổng hợp, Đơn vị:{maCSKCB}-Số lượng hồ sơ:{data1.Count()}, người lấy báo cáo:", $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - BC Tổng hợp");
                    return new ResponseModel<byte[]>(
                        HttpStatusCode.RequestEntityTooLarge,
                        $"Report too large: {data1.Count()} records. Narrow your filter or export in smaller batches.",
                        null
                    );
                    
                }
                await AttachInnerXML(data1);

                var errors = await AttachXMLError(maCSKCB, data1);
                var errCodes = errors.Select(e => e.MA_LOI).Distinct().ToList();
                var dmErrors = await GetDMErrors(maCSKCB, errCodes);
                var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
                var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

                //Create new excel
                IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

                var grpErrors = errors.GroupBy(x => x.MA_LOI, (k, g) => new { ErrorCode = k, Count = g.Count(), Files = g.Select(e => e.XML01ID).Distinct().Count() })
                                        .Join(dmErrors, x => x.ErrorCode, y => y.error_code, (x, y) => new
                                        {
                                            y.error_code,
                                            y.error_file,
                                            y.error_tag,
                                            y.message,
                                            y.is_alert,
                                            x.Count,
                                            x.Files
                                        })
                                        .Where(e => e.error_code != null)
                                        .OrderBy(e => e.error_file);

                //Linhist changed 09/02/26
                //Change VLOOKUP to XLOOKUP for better performance
                //string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{dmBacsis.Count() + 5},2,FALSE),\"\")";
                //string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{dmBacsis.Count() + 5},3,FALSE),\"\")";
                //string fmKhoaTH = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:D{dmBacsis.Count() + 5},4,FALSE),\"\")";
                //string fmKhoaTen = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:E{dmBacsis.Count() + 5},5,FALSE),\"\")";
                //string fmBHXH = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:F{dmBacsis.Count() + 5},6,FALSE),\"\")";
                //string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";
                //string fmError = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:D{grpErrors.Count() + 5},3,FALSE),\"\")";
                //string fmEtype = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:C{grpErrors.Count() + 5},2,FALSE),\"\")";
                //string fmEtag = $"$=IFERROR(VLOOKUP(\"{{0}}\",'Tổng Hợp Lỗi'!B3:F{grpErrors.Count() + 5},5,FALSE),\"\")";

                string fmBacsi = $"IFERROR(INDEX('DM Bác sĩ'!B:B, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
                string fmChucvu = $"IFERROR(INDEX('DM Bác sĩ'!C:C, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
                string fmKhoaTH = $"IFERROR(INDEX('DM Bác sĩ'!D:D, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
                string fmKhoaTen = $"IFERROR(INDEX('DM Bác sĩ'!E:E, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";
                string fmBHXH = $"IFERROR(INDEX('DM Bác sĩ'!F:F, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\")";

                string fmMaNV = $"IFERROR(INDEX('DM Bác sĩ'!A:A, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),\"{{0}}\")";
                string fmTenNV = $"IFERROR(INDEX('DM Bác sĩ'!B:B, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!B:B, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
                string fmChucNV = $"IFERROR(INDEX('DM Bác sĩ'!C:C, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!C:C, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
                string fmKhoaNV = $"IFERROR(INDEX('DM Bác sĩ'!D:D, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!D:D, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
                string fmTKNV = $"IFERROR(INDEX('DM Bác sĩ'!E:E, MATCH(\"{{0}}\", 'DM Bác sĩ'!F:F, 0)),IFERROR(INDEX('DM Bác sĩ'!E:E, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"\"))";
                string fmBHNV = $"IFERROR(INDEX('DM Bác sĩ'!F:F, MATCH(\"{{0}}\", 'DM Bác sĩ'!A:A, 0)),\"{{0}}\")";

                string fmKhoa = $"$=IFERROR(INDEX('DM Khoa'!B:B, MATCH(\"{{0}}\", 'DM Khoa'!A:A, 0)),\"\")";

                string fmError = $"$=IFERROR(INDEX('Tổng Hợp Lỗi'!D:D, MATCH(\"{{0}}\", 'Tổng Hợp Lỗi'!B:B, 0)),\"\")";
                string fmEtype = $"$=IFERROR(INDEX('Tổng Hợp Lỗi'!C:C, MATCH(\"{{0}}\", 'Tổng Hợp Lỗi'!B:B, 0)),\"\")";
                string fmEtag = $"$=IFERROR(INDEX('Tổng Hợp Lỗi'!F:F, MATCH(\"{{0}}\", 'Tổng Hợp Lỗi'!B:B, 0)),\"\")";

                using (var fs = new MemoryStream())
                {
                    #region Sheet 1: DS File Bao cao
                    var grpFiles = data1.OrderBy(x => x.FileName);
                    ISheet xlSheetF = workbook.CreateSheet("DS File Báo cáo");

                    int rows = 2;
                    int cols = CreateHeader(xlSheetF, cellStyles,
                                $"Báo Cáo Tổng Hợp {data1.Count()} Hồ Sơ - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                                "STT", "Tên File", "Số lỗi", "Mã CSKCB", "Mã đối tượng KCB",
                                "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã bệnh chính", "Mã bệnh kèm theo", "Mã bệnh YHCT",
                                "Mã Thẻ", "Ngày sinh", "Giới tính", "Địa chỉ",
                                "Mã Khoa", "Tên Khoa",
                                "Ngày vào", "Ngày ra", "Ngày lập");

                    foreach (var x in grpFiles)
                    {
                        AppendRow(xlSheetF, cellStyles, rows++,
                                    rows - 2, x.FileName, errors?.Count(e => e.XML01ID == x.XML01ID) ?? 0, x.MA_CSKCB,x.MA_DOITUONG_KCB,
                                    x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_BENH_CHINH, x.MA_BENH_KT,x.MA_BENH_YHCT,
                                    x.MA_THE_BHYT, GetNgayThang(x.NGAY_SINH, true), GetType_GioiTinh(x.GIOI_TINH), x.DIA_CHI,
                                    x.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.MA_KHOA, true),
                                    GetNgayThang(x.NGAY_VAO), GetNgayThang(x.NGAY_RA), GetNgayThang(x.CreateDate));
                    }

                    SetAutoFeatures(xlSheetF, cols, -1, 9, 14);
                    #endregion

                    #region Sheet 2: Tong hop Loi
                    ISheet xlSheetE = workbook.CreateSheet("Tổng Hợp Lỗi");
                    rows = 2;
                    cols = CreateHeader(xlSheetE, cellStyles,
                                        "Tổng hợp số lỗi trong file hồ sơ", // Sheet title
                                        "STT", "Mã lỗi", "Loại lỗi", "Chi tiết lỗi", "Loại File lỗi", "Lỗi tại Thẻ", "Số lỗi", "Số File lỗi");

                    foreach (var x2 in grpErrors)
                    {
                        AppendRow(xlSheetE, cellStyles, rows++,
                                    rows - 2, x2.error_code, x2.is_alert ? "CẢNH BÁO" : "BÁO LỖI", x2.message, x2.error_file, x2.error_tag, x2.Count, x2.Files);
                    }

                    SetAutoFeatures(xlSheetE, cols, 2, -3);
                    #endregion

                    //var x01IdErrs = errors.Select(e => e.XML01ID).Distinct().ToList();
                    //var x01Errors = data1.Where(x => x01IdErrs.Contains(x.XML01ID)).ToList();
                    //var x01Codes = dmErrors.Where(e => e.error_file == "XML1").Select(e => e.error_code).ToList();
                    //var x01Errors = data1.Where(x => x.Errors?.Any(e => x01Codes.Contains(e.MA_LOI)) == true).ToList();

                    #region Sheet 3: DS Loi XML
                    ISheet xlSheet1 = workbook.CreateSheet("Lỗi XML1");
                    rows = 2;
                    cols = CreateHeader(xlSheet1, cellStyles,
                            "Danh sách lỗi XML1", // Sheet title
                            "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                            "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Số CCCD", "Ngày sinh", "Giới tính", "Nhóm máu",
                            "Mã quốc tịch", "Mã dân tộc", "Mã nghề nghiệp", "Địa chỉ",
                            "Mã tỉnh cư trú", "Mã Huyện", "Mã xã cư trú", "Điện thoại",
                            "Mã Thẻ", "Mã ĐKKCBBĐ", "Thẻ từ", "Thẻ đến", "Ngày miễn CCT",
                            "Lý do vào viện", "Lý do vào viện nội trú", "Mã lý do vào viện nội trú",
                            "Chẩn đoán vào", "Chẩn đoán ra", "Mã bệnh chính", "Mã bệnh kèm theo", "Mã bệnh YHCT",
                            "Mã PTTT QT", "Mã đối tượng KCB", "Mã nơi đi", "Mã nơi đến", "Mã tai nạn",
                            "Ngày vào", "Ngày vào nội trú", "Ngày ra", "Giấy chuyển tuyến",
                            "Số ngày điều trị", "PP Điều trị", "Kết quả DT", "Mã loại ra viện", "Ghi chú",
                            "Ngày thanh toán", "Tiền thuốc", "Tiền VTTY",
                            "Tổng chi BV", "Tổng chi BH", "Tiền BNTT",
                            "Tiền BNCTT", "Tiền BHTT", "Tiền Nguồn khác", "Tiền BHTT GDV",
                            "Năm QT", "Tháng QT", "Mã loại KCB",
                            "Mã Khoa", "Tên Khoa", "Mã CSKCB", "Mã khu vực",
                            "Cân nặng", "Cân nặng con", "Năm năm liên tục", "ngày tái khám", "Mã hồ sơ bệnh án", "Mã TTDV"
                        );

                    //List<int?> xmlErCodes = dmErrors.Where(e => e.error_file == "XML1").Select(e => e.error_code).ToList();
                    //var errLst = errors.Where(a => xmlErCodes.Contains(a.MA_LOI));

                    var x1Codes = dmErrors.Where(e => e.error_file == "XML1").Select(e => e.error_code).ToList();
                    var x1Errors = errors.Where(a => x1Codes.Contains(a.MA_LOI)).ToList();
                    var x1Join = data1.Join(x1Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X01 = x, XER = e })
                                            .Distinct().ToList();

                    //foreach (var x in errLst)
                    foreach (var x in x1Join)
                    {
                        AppendRow(xlSheet1, cellStyles, rows++,
                                x.X01.STT, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X01.MA_LK, x.X01.MA_BN, x.X01.HO_TEN, x.X01.SO_CCCD, GetNgayThang(x.X01.NGAY_SINH), x.X01.GIOI_TINH, x.X01.NHOM_MAU,
                                x.X01.MA_QUOCTICH, x.X01.MA_DANTOC, x.X01.MA_NGHE_NGHIEP, x.X01.DIA_CHI,
                                x.X01.MATINH_CU_TRU, x.X01.MAHUYEN_CU_TRU, x.X01.MAXA_CU_TRU, x.X01.DIEN_THOAI,
                                x.X01.MA_THE_BHYT, x.X01.MA_DKBD, x.X01.GT_THE_TU, x.X01.GT_THE_DEN, x.X01.NGAY_MIEN_CCT,
                                x.X01.LY_DO_VV, x.X01.LY_DO_VNT, x.X01.MA_LY_DO_VNT,
                                x.X01.CHAN_DOAN_VAO, x.X01.CHAN_DOAN_RV, x.X01.MA_BENH_CHINH, x.X01.MA_BENH_KT, x.X01.MA_BENH_YHCT,
                                x.X01.MA_PTTT_QT, x.X01.MA_DOITUONG_KCB, x.X01.MA_NOI_DI, x.X01.MA_NOI_DEN, x.X01.MA_TAI_NAN,
                                GetNgayThang(x.X01.NGAY_VAO), GetNgayThang(x.X01.NGAY_VAO_NOI_TRU), GetNgayThang(x.X01.NGAY_RA), x.X01.GIAY_CHUYEN_TUYEN,
                                x.X01.SO_NGAY_DTRI, x.X01.PP_DIEU_TRI, x.X01.KET_QUA_DTRI, x.X01.MA_LOAI_RV, x.X01.GHI_CHU,
                                GetNgayThang(x.X01.NGAY_TTOAN), Uti.GetMoneyVN(x.X01.T_THUOC), Uti.GetMoneyVN(x.X01.T_VTYT),
                                Uti.GetMoneyVN(x.X01.T_TONGCHI_BV), Uti.GetMoneyVN(x.X01.T_TONGCHI_BH), Uti.GetMoneyVN(x.X01.T_BNTT),
                                Uti.GetMoneyVN(x.X01.T_BNCCT), Uti.GetMoneyVN(x.X01.T_BHTT), Uti.GetMoneyVN(x.X01.T_NGUONKHAC), Uti.GetMoneyVN(x.X01.T_BHTT_GDV),
                                x.X01.NAM_QT, x.X01.THANG_QT, x.X01.MA_LOAI_KCB,
                                x.X01.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.X01.MA_KHOA), x.X01.MA_CSKCB, x.X01.MA_KHUVUC,
                                x.X01.CAN_NANG, x.X01.CAN_NANG_CON, x.X01.NAM_NAM_LIEN_TUC, x.X01.NGAY_TAI_KHAM, x.X01.MA_HSBA, x.X01.MA_TTDV
                        );
                    }

                    SetAutoFeatures(xlSheet1, cols, 2, -4, 12, -15);
                    #endregion

                    #region Sheet 4: DS Loi X02
                    //List<int?> X02ErCodes = dmErrors.Where(e => e.error_file == "XML2").Select(e => e.error_code).ToList();
                    //var X02Errors = errors.Where(a => X02ErCodes.Contains(a.MA_LOI));

                    //var data2Join = X02Errors.Select(e => e.XML01)
                    //                    .SelectMany(x => x.XML02s)
                    //                    .Join(X02Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X02 = x, XER = e })
                    //                    .Where(x => x.X02.XML01ID == x.XER.XML01ID && x.X02.STT == x.XER.STT);

                    var x2Codes = dmErrors.Where(e => e.error_file == "XML2").Select(e => e.error_code).ToList();
                    var x2Errors = errors.Where(a => x2Codes.Contains(a.MA_LOI)).ToList();
                    var x2All = data1.Where(x => x2Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML02s).ToList();
                    var x2Join = x2All.Join(x2Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X02 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet2 = workbook.CreateSheet("Lỗi XML02");
                    rows = 2;
                    cols = CreateHeader(xlSheet2, cellStyles,
                                "Danh sách lỗi của XML02", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Thứ tự", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa",
                                "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Mã BHXH", "Khoa y lệnh",
                                "Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện",
                                "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng", "Số Đăng ký","Cách dùng","Dạng bào chế","Mã Dịch Vụ", "Đường Dùng",
                                "TT Thầu", "Đơn giá", "Số lượng", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh", "Ngày thực hiên Y Lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                                "Tổng Chi");
                    
                    //foreach (var x in data2Join)
                    foreach (var x in x2Join)
                    {
                        string maBS = x.X02.MA_BAC_SI;
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu, fmBHXH }, ref maBS);

                        AppendRow(xlSheet2, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X02.STT, x.X02.XML01.MA_LK, x.X02.XML01.MA_BN, x.X02.XML01.HO_TEN,
                                x.X02.XML01.MA_THE_BHYT, GetNgayThang(x.X02.XML01.NGAY_SINH, true), GetType_GioiTinh(x.X02.XML01.GIOI_TINH),
                                x.X02.XML01.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.X02.XML01.MA_KHOA),
                                maBS, ttBS[0], ttBS[1], ttBS[2], x.X02.MA_KHOA,
                                x.X02.XML01.MA_BENH_CHINH, x.X02.XML01.MA_BENH_KT, x.X02.XML01.CHAN_DOAN_RV,
                                x.X02.MA_THUOC, x.X02.MA_NHOM, x.X02.TEN_THUOC, x.X02.DON_VI_TINH, x.X02.LIEU_DUNG, x.X02.SO_DANG_KY, x.X02.CACH_DUNG,x.X02.DANG_BAO_CHE,x.X02.MA_DICH_VU, x.X02.DUONG_DUNG,
                                x.X02.TT_THAU, x.X02.DON_GIA, x.X02.SO_LUONG, x.X02.MUC_HUONG, x.X02.TYLE_TT_BH, x.X02.THANH_TIEN_BV,
                                GetNgayThang(x.X02.NGAY_YL), GetNgayThang(x.X02.NGAY_TH_YL), GetNgayThang(x.X02.XML01.NGAY_VAO), GetNgayThang(x.X02.XML01.NGAY_RA), GetNgayThang(x.X02.XML01.NGAY_TTOAN),
                                string.Format("{0:##,##0.0#}", x.X02.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","));
                    }

                    SetAutoFeatures(xlSheet2, cols, 2, -4, 12, 14, 15, -19);
                    #endregion

                    #region Sheet 5: Loi XML3
                    //List<int?> xml3ErCodes = dmErrors.Where(e => e.error_file == "XML3").Select(e => e.error_code).ToList();
                    //var xml3Errors = errors.Where(a => xml3ErCodes.Contains(a.MA_LOI));

                    //var data3Join = data1.SelectMany(x => x.XML03s)
                    //                    .Join(xml3Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X03 = x, XER = e })
                    //                    .Where(x => x.X03.STT == x.XER.STT);
                    var x3Codes = dmErrors.Where(e => e.error_file == "XML3").Select(e => e.error_code).ToList();
                    var x3Errors = errors.Where(a => x3Codes.Contains(a.MA_LOI)).ToList();
                    var x3Join = data1.Where(x => x3Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML03s)
                                        .Join(x3Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X03 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet3 = workbook.CreateSheet("Lỗi XML03");
                    rows = 2;
                    cols = CreateHeader(xlSheet3, cellStyles,
                                "Danh sách lỗi của XML03", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Thứ tự", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính",
                                "Mã Khoa", "Tên Khoa", "Phạm vi",
                                "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh", "Mã BHXH", "Khoa y lệnh", "Khoa Thực hiện", "Mã Người thực hiện", "Tên người thực hiện", "Mã BHXH",
                                "Mã Dịch vụ", "Tên Dịch vụ", "Mã Vật tư", "Tên Vật tư", "Mã nhóm", "Mã máy",
                                "Mã bệnh chính", "Mã bệnh kèm theo", "Chuẩn đoán ra viện", "Số lượng", "Mã giường",
                                "ĐVT", "TT Thầu", "Đơn giá", "Mức hưởng", "Tỷ lệ TT", "Thành tiền",
                                "Ngày Y lệnh", "Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán",
                                "Tổng Chi");
                    
                    //foreach (var x in data3Join)
                    foreach (var x in x3Join)
                    {
                        string maBS = x.X03.MA_BAC_SI;
                        string maBSTH = x.X03.NGUOI_THUC_HIEN;
                        string tenKTH = GetFormular_KhoaTH(fmKhoaTH, maBS);
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu, fmBHXH }, ref maBS);
                        string[] ttBStt = GetFormular_BacSi(new string[] { fmBacsi, fmBHXH }, ref maBSTH);
                        AppendRow(xlSheet3, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X03.STT, x.X03.XML01.MA_LK, x.X03.XML01.MA_BN, x.X03.XML01.HO_TEN,
                                x.X03.XML01.MA_THE_BHYT, GetNgayThang(x.X03.XML01.NGAY_SINH, true), GetType_GioiTinh(x.X03.XML01.GIOI_TINH),
                                x.X03.XML01.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.X03.XML01.MA_KHOA), x.X03.PHAM_VI,
                                maBS, ttBS[0], ttBS[1], ttBS[2], x.X03.MA_KHOA, tenKTH, maBSTH, ttBStt[0], ttBStt[1],
                                x.X03.MA_DICH_VU, x.X03.TEN_DICH_VU, x.X03.MA_VAT_TU, x.X03.TEN_VAT_TU, x.X03.MA_NHOM, x.X03.MA_MAY,
                                x.X03.XML01.MA_BENH_CHINH, x.X03.XML01.MA_BENH_KT, x.X03.XML01.CHAN_DOAN_RV, x.X03.SO_LUONG, x.X03.MA_GIUONG,
                                x.X03.DON_VI_TINH, x.X03.TT_THAU, x.X03.DON_GIA_BV, x.X03.MUC_HUONG, x.X03.TYLE_TT_BH, x.X03.THANH_TIEN_BV,
                                GetNgayThang(x.X03.NGAY_YL), GetNgayThang(x.X03.NGAY_TH_YL), GetNgayThang(x.X03.NGAY_KQ), GetNgayThang(x.X03.XML01.NGAY_VAO), GetNgayThang(x.X03.XML01.NGAY_RA), GetNgayThang(x.X03.XML01.NGAY_TTOAN),
                                string.Format("{0:##,##0.0#}", x.X03.XML01.T_TONGCHI_BV).Replace(".", "$").Replace(",", ".").Replace("$", ","));
                    }

                    SetAutoFeatures(xlSheet3, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Sheet 6: Loi XML4
                    //List<int?> xml4ErCodes = dmErrors.Where(e => e.error_file == "XML4").Select(e => e.error_code).ToList();
                    //var xml4Errors = errors.Where(a => xml4ErCodes.Contains(a.MA_LOI));

                    //var data4Join =
                    //    data1.SelectMany(x => x.XML04s)
                    //    .Join(xml4Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X04 = x, XER = e })
                    //    .Where(x => x.X04.STT == x.XER.STT);
                    var x4Codes = dmErrors.Where(e => e.error_file == "XML4").Select(e => e.error_code).ToList();
                    var x4Errors = errors.Where(a => x4Codes.Contains(a.MA_LOI)).ToList();
                    var x4Join = data1.Where(x => x4Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML04s)
                                        .Join(x4Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X04 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet4 = workbook.CreateSheet("Lỗi XML04");
                    rows = 2;
                    cols = CreateHeader(xlSheet4, cellStyles,
                                "Danh sách lỗi của XML04", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính", "Mã Dịch vụ",
                                "Mã chỉ số", "Tên chỉ số", "Giá trị", "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả",
                                "Mã Bác sĩ đọc kết quả", "Tên bác sĩ đọc kết quả", "Chức Danh", "Mã BHXH",
                                "Mã Khoa", "Tên Khoa",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                            );
                    
                    //foreach (var x in data4Join)
                    foreach (var x in x4Join)
                    {
                        string maBS = x.X04.MA_BS_DOC_KQ;

                        string[] ttBS = GetFormular_BacSi(new string[] { fmMaNV, fmTenNV, fmChucNV, fmKhoaNV, fmTKNV, fmBHNV }, ref maBS);

                        AppendRow(xlSheet4, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X04.XML01.MA_LK, x.X04.XML01.MA_BN, x.X04.XML01.HO_TEN,
                                x.X04.XML01.MA_THE_BHYT, GetNgayThang(x.X04.XML01.NGAY_SINH, true), GetType_GioiTinh(x.X04.XML01.GIOI_TINH), x.X04.MA_DICH_VU,
                                x.X04.MA_CHI_SO, x.X04.TEN_CHI_SO, x.X04.GIA_TRI, x.X04.DON_VI_DO, x.X04.MO_TA, x.X04.KET_LUAN, x.X04.NGAY_KQ,
                                ttBS[0], ttBS[1], ttBS[2], ttBS[5], //maBS,
                                ttBS[3], ttBS[4],
                                //GetFormular_KhoaTH(fmKhoaTH, maBS), GetFormular_KhoaTH(fmKhoaTen, maBS),
                                GetNgayThang(x.X04.XML01.NGAY_VAO), GetNgayThang(x.X04.XML01.NGAY_RA), GetNgayThang(x.X04.XML01.NGAY_TTOAN)
                        );
                    }

                    SetAutoFeatures(xlSheet4, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Sheet 7: Loi XML5
                    //List<int?> xml5ErCodes = dmErrors.Where(e => e.error_file == "XML5").Select(e => e.error_code).ToList();
                    //var xml5Errors = errors.Where(a => xml5ErCodes.Contains(a.MA_LOI));

                    //var data5Join = data1.SelectMany(x => x.XML05s)
                    //                    .Join(xml5Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X05 = x, XER = e })
                    //                    .Where(x => x.X05.STT == x.XER.STT);
                    var x5Codes = dmErrors.Where(e => e.error_file == "XML5").Select(e => e.error_code).ToList();
                    var x5Errors = errors.Where(a => x5Codes.Contains(a.MA_LOI)).ToList();
                    var x5Join = data1.Where(x => x5Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML05s)
                                        .Join(x5Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X05 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet5 = workbook.CreateSheet("Lỗi XML05");
                    rows = 2;
                    cols = CreateHeader(xlSheet5, cellStyles,
                                "Danh sách lỗi của XML05", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Diễn biến Lâm Sàng", "Giai đoạn bệnh", "Hội chẩn",
                                "Phẫu thuật", "Thời điểm diễn biến lâm sàng",
                                "Mã Người thực hiện", "Tên Người thực hiện", "Mã BHXH",
                                "Mã Khoa", "Tên Khoa",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                                );
                    
                    //foreach (var x in data5Join)
                    foreach (var x in x5Join)
                    {
                        string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormular_KhoaTH(fmKhoaTH, maBS);
                        string[] ttBS = GetFormular_BacSi(new string[] { fmMaNV, fmTenNV, fmKhoaNV, fmTKNV, fmBHNV }, ref maBS);

                        AppendRow(xlSheet5, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X05.XML01.MA_LK, x.X05.XML01.MA_BN, x.X05.XML01.HO_TEN,
                                x.X05.DIEN_BIEN_LS, x.X05.GIAI_DOAN_BENH, x.X05.HOI_CHAN,
                                x.X05.PHAU_THUAT, GetNgayThang(x.X05.THOI_DIEM_DBLS),
                                ttBS[0], ttBS[1], ttBS[4], //maBS, x.X05.NGUOI_THUC_HIEN,
                                ttBS[2], ttBS[3], 
                                //GetFormular_KhoaTH(fmKhoaTH, maBS), GetFormular_KhoaTH(fmKhoaTen, maBS),
                                GetNgayThang(x.X05.XML01.NGAY_VAO), GetNgayThang(x.X05.XML01.NGAY_RA), GetNgayThang(x.X05.XML01.NGAY_TTOAN)
                        );
                    }

                    SetAutoFeatures(xlSheet5, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Báo cáo XML6
                    //List<int?> xml6ErCodes = dmErrors.Where(e => e.error_file == "XML6").Select(e => e.error_code).ToList();
                    //var xml6Errors = errors.Where(a => xml6ErCodes.Contains(a.MA_LOI));

                    //var data6Join =
                    //    data1.SelectMany(x => x.XML06s)
                    //    .Join(xml6Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X06 = x, XER = e })
                    //    .Where(x => x.X06.STT == x.XER.STT);
                    var x6Codes = dmErrors.Where(e => e.error_file == "XML6").Select(e => e.error_code).ToList();
                    var x6Errors = errors.Where(a => x6Codes.Contains(a.MA_LOI)).ToList();
                    var x6Join = data1.Where(x => x6Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML06s)
                                        .Join(x6Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X06 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet6 = workbook.CreateSheet("Lỗi XML06");
                    rows = 2;
                    cols = CreateHeader(xlSheet6, cellStyles,
                                "Danh sách lỗi của XML06", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi",
                                "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Thời điểm khẳng định HIV", "Thời điểm nhận thuốc ARV", "Mã phác đồ điều trị", "Mã bậc phác đồ điều trị", "Mã lý do đăng ký", "Mã loại điều trị lao", "Mã phác đồ điều trị bệnh lao",
                                "Mã Cơ sở lấy mẫu xét nghiệm", "Thời điểm bắt đầu điều trị lao", "Nơi làm kết quả xét nghiệm", "Thời điểm kết thúc điều trị lao", "Nơi đầu tiên nhận thuốc ARV", "Lý do chỉ định xét nghiệm đo tải lượng virus",
                                "Thời điểm lấy mẫu", "Kết quả xét nghiệm tải lượng", "Thời điểm kết quả xét nghiệm", "Mã đối tượng", "Mã tình trạng", "Phương pháp sàng lọc lao", "Mã lần thực hiện xét nghiệm PCR", "Ngày bệnh nhân thực hiện PCR",
                                "Ngày bệnh nhân có kết quả PCR", "Mã kết quả xét nghiệm PCR", "Mã kết quả điều trị Lao", "Thời điểm ghi nhận thông tin mang thai", "Thời điểm bắt đàu điều trị CTX", "Mã xử trí", "Ngày bắt đầu xử trí ARV", "Ngày kết thúc xử lý ARV",
                                "Mã phác đồ điều trị HIV", "Mã giai đoạn lâm sàng", "Mã bậc phác đồ điều trị", "Mã nhóm đối tượng", "Số ngày thuốc ARV", "Ngày chuyển phác đồ điều trị", "Mã lý do chuyển phác đồ",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data6Join)
                    foreach (var x in x6Join)
                    {
                        AppendRow(xlSheet6, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI),
                                x.X06.XML01.MA_LK, x.X06.XML01.MA_BN, x.X06.XML01.HO_TEN,
                                GetNgayThang(x.X06.NGAYKD_HIV), x.X06.BDDT_ARV, x.X06.MA_PHAC_DO_DIEU_TRI_BD, x.X06.MA_BAC_PHAC_DO_BD, x.X06.MA_LYDO_DTRI, x.X06.LOAI_DTRI_LAO, x.X06.PHACDO_DTRI_LAO,
                                x.X06.NOI_LAY_MAU_XN, GetNgayThang(x.X06.NGAYBD_DTRI_LAO), x.X06.NOI_XN_KD, GetNgayThang(x.X06.NGAYKT_DTRI_LAO), x.X06.NOI_BDDT_ARV, x.X06.MA_LYDO_XNTL_VR,
                                GetNgayThang(x.X06.NGAY_XN_TLVR), x.X06.KQ_XNTL_VR, GetNgayThang(x.X06.NGAY_KQ_XN_TLVR), x.X06.MA_LOAI_BN, x.X06.MA_TINH_TRANG_DK, x.X06.SANG_LOC_LAO, x.X06.LAN_XN_PCR, GetNgayThang(x.X06.NGAY_XN_PCR),
                                GetNgayThang(x.X06.NGAY_KQ_XN_PCR), x.X06.MA_KQ_XN_PCR, x.X06.KQ_DTRI_LAO, GetNgayThang(x.X06.NGAY_NHAN_TT_MANG_THAI), GetNgayThang(x.X06.NGAY_BAT_DAU_DT_CTX), x.X06.MA_XU_TRI, GetNgayThang(x.X06.NGAY_BAT_DAU_XU_TRI), GetNgayThang(x.X06.NGAY_KET_THUC_XU_TRI),
                                x.X06.MA_PHAC_DO_DIEU_TRI, x.X06.GIAI_DOAN_LAM_SANG, x.X06.MA_BAC_PHAC_DO, x.X06.NHOM_DOI_TUONG, x.X06.SO_NGAY_CAP_THUOC_ARV, x.X06.NGAY_CHUYEN_PHAC_DO, x.X06.LY_DO_CHUYEN_PHAC_DO,
                                GetNgayThang(x.X06.XML01.NGAY_VAO), GetNgayThang(x.X06.XML01.NGAY_RA), GetNgayThang(x.X06.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet6, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Báo cáo XML7
                    //List<int?> xml7ErCodes = dmErrors.Where(e => e.error_file == "XML7").Select(e => e.error_code).ToList();
                    //var xml7Errors = errors.Where(a => xml7ErCodes.Contains(a.MA_LOI));

                    //var data7Join =
                    //    data1.SelectMany(x => x.XML07s)
                    //    .Join(xml7Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X07 = x, XER = e })
                    //    .Where(x => x.X07.STT == x.XER.STT);
                    var x7Codes = dmErrors.Where(e => e.error_file == "XML7").Select(e => e.error_code).ToList();
                    var x7Errors = errors.Where(a => x7Codes.Contains(a.MA_LOI)).ToList();
                    var x7Join = data1.Where(x => x7Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML07s)
                                        .Join(x7Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X07 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet7 = workbook.CreateSheet("Lỗi XML07");
                    rows = 2;
                    cols = CreateHeader(xlSheet7, cellStyles,
                                "Danh sách lỗi của XML07", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số lưu trú", "Mã Y Tế", "Mã Khoa ra viện", "Ngày vào", "Ngày ra", "Mã đình chỉ thai", "Nguyên nhân đình chỉ thai", "Thời gian đình chỉ thai", "Tuổi thai", "Chẩn đoán ra viện",
                                "Phương pháp điều trị", "Ghi chú", "Mã TTDV", "Mã BS", "Tên BS", "Ngày Chứng từ", "Mã Cha", "Mã Mẹ", "Mã thẻ tạm", "Họ tên cha", "Họ tên mẹ", "Số ngày nghỉ", "Ngoại trú từ ngày", "Ngoại trú đến ngày",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data7Join)
                    foreach (var x in x7Join)
                    {
                        AppendRow(xlSheet7, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X07.XML01.MA_BN, x.X07.XML01.HO_TEN, x.X07.XML01.MA_LK, x.X07.XML01.MA_THE_BHYT,
                                x.X07.SO_LUU_TRU, x.X07.MA_YTE, x.X07.MA_KHOA_RV, GetNgayThang(x.X07.NGAY_VAO), GetNgayThang(x.X07.NGAY_RA), x.X07.MA_DINH_CHI_THAI, x.X07.NGUYENNHAN_DINHCHI, x.X07.THOIGIAN_DINHCHI, x.X07.TUOI_THAI, x.X07.CHAN_DOAN_RV,
                                x.X07.PP_DIEUTRI, x.X07.GHI_CHU, x.X07.MA_TTDV, x.X07.MA_BS, x.X07.TEN_BS, GetNgayThang(x.X07.NGAY_CT), x.X07.MA_CHA, x.X07.MA_ME, x.X07.MA_THE_TAM, x.X07.HO_TEN_CHA, x.X07.HO_TEN_ME, x.X07.SO_NGAY_NGHI, GetNgayThang(x.X07.NGOAITRU_TUNGAY), GetNgayThang(x.X07.NGOAITRU_DENNGAY),
                               GetNgayThang(x.X07.XML01.NGAY_VAO), GetNgayThang(x.X07.XML01.NGAY_RA), GetNgayThang(x.X07.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet7, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Báo cáo XML8
                    //List<int?> xml8ErCodes = dmErrors.Where(e => e.error_file == "XML8").Select(e => e.error_code).ToList();
                    //var xml8Errors = errors.Where(a => xml8ErCodes.Contains(a.MA_LOI));

                    //var data8Join =
                    //    data1.SelectMany(x => x.XML08s)
                    //    .Join(xml8Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X08 = x, XER = e })
                    //    .Where(x => x.X08.STT == x.XER.STT);
                    var x8Codes = dmErrors.Where(e => e.error_file == "XML8").Select(e => e.error_code).ToList();
                    var x8Errors = errors.Where(a => x8Codes.Contains(a.MA_LOI)).ToList();
                    var x8Join = data1.Where(x => x8Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML08s)
                                        .Join(x8Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X08 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet8 = workbook.CreateSheet("Lỗi XML08");
                    rows = 2;
                    cols = CreateHeader(xlSheet8, cellStyles,
                                "Danh sách lỗi của XML08", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Mã loại KCB", "Họ tên Cha", "Họ tên mẹ", "Người giám hộ", "Đơn vị", "Ngày vào", "Ngày ra", "Chẩn đoán vào", "Chẩn đoán ra viện", "Quá trình bệnh lý", "Tóm tắt KQ", "Phương pháp điều trị", "Ngày sinh con",
                                "Ngày con chết", "Số con chết", "Kết quả điều trị", "Ghi chú", "Mã TTDV", "Ngày chứng từ", "Mã thẻ tạm",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data8Join)
                    foreach (var x in x8Join)
                    {
                        //string maBS = x.X05.NGUOI_THUC_HIEN;
                        //string tenKTH = GetFormularKhoaTH(fmKhoaTH, maBS);
                        //string[] ttBS = GetFormularBacSi_New(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet8, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X08.XML01.MA_BN, x.X08.XML01.HO_TEN, x.X08.XML01.MA_LK, x.X08.XML01.MA_THE_BHYT,
                                x.X08.MA_LOAI_KCB, x.X08.HO_TEN_CHA, x.X08.HO_TEN_ME, x.X08.NGUOI_GIAM_HO, x.X08.DON_VI, GetNgayThang(x.X08.NGAY_VAO), GetNgayThang(x.X08.NGAY_RA), x.X08.CHAN_DOAN_VAO, x.X08.CHAN_DOAN_RV, x.X08.QT_BENHLY, x.X08.TOMTAT_KQ, x.X08.PP_DIEUTRI, GetNgayThang(x.X08.NGAY_SINHCON),
                                GetNgayThang(x.X08.NGAY_CONCHET), x.X08.SO_CONCHET, x.X08.KET_QUA_DTRI, x.X08.GHI_CHU, x.X08.MA_TTDV, GetNgayThang(x.X08.NGAY_CT), x.X08.MA_THE_TAM,
                                GetNgayThang(x.X08.XML01.NGAY_VAO), GetNgayThang(x.X08.XML01.NGAY_RA), GetNgayThang(x.X08.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet8, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Báo cáo XML9
                    //List<int?> xml9ErCodes = dmErrors.Where(e => e.error_file == "XML9").Select(e => e.error_code).ToList();
                    //var xml9Errors = errors.Where(a => xml9ErCodes.Contains(a.MA_LOI));

                    //var data9Join =
                    //    data1.SelectMany(x => x.XML09s)
                    //    .Join(xml9Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X09 = x, XER = e })
                    //    .Where(x => x.X09.STT == x.XER.STT);
                    var x9Codes = dmErrors.Where(e => e.error_file == "XML9").Select(e => e.error_code).ToList();
                    var x9Errors = errors.Where(a => x9Codes.Contains(a.MA_LOI)).ToList();
                    var x9Join = data1.Where(x => x9Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML09s)
                                        .Join(x9Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X09 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet9 = workbook.CreateSheet("Lỗi XML09");
                    rows = 2;
                    cols = CreateHeader(xlSheet9, cellStyles,
                                "Danh sách lỗi của XML09", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã loại KCB", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Mã BHXH người nuôi dưỡng", "Mã thẻ NND", "Họ tên NND", "Ngày sinh NND", "Mã DT NND", "Số CCCD NND", "Ngày cấp CCCD NND", "Nơi cấp CCCD NND", "Nơi cư trú NND", "Mã QT",
                                "Mã Tỉnh", "Mã huyện", "Mã xã", "Họ tên cha", "Mã thẻ tạm", "Họ tên con", "Giới tính", "Số con", "Lần sinh", "Số con sống", "Cân nặng con", "Ngày sinh con", "Nơi sinh con",
                                "Trình trạng con", "Sinh con phẫu thuật", "Sinh con dưới 32 tuần", "Ghi chú", "Người đỡ đẻ", "Người ghi phiếu", "Ngày chứng từ", "Số Chứng từ", "Quyển số", "Mã TTDV",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data9Join)
                    foreach (var x in x9Join)
                    {
                        AppendRow(xlSheet9, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X09.XML01.MA_LOAI_KCB, x.X09.XML01.MA_BN, x.X09.XML01.HO_TEN, x.X09.XML01.MA_LK, x.X09.XML01.MA_THE_BHYT,
                                x.X09.MA_BHXH_NND, x.X09.MA_THE_NND, x.X09.HO_TEN_NND, GetNgayThang(x.X09.NGAYSINH_NND), x.X09.MA_DANTOC_NND, x.X09.SO_CCCD_NND, GetNgayThang(x.X09.NGAYCAP_CCCD_NND), x.X09.NOICAP_CCCD_NND, x.X09.NOI_CU_TRU_NND, x.X09.MA_QUOCTICH,
                                x.X09.MATINH_CU_TRU, x.X09.MAHUYEN_CU_TRU, x.X09.MAXA_CU_TRU, x.X09.HO_TEN_CHA, x.X09.MA_THE_TAM, x.X09.HO_TEN_CON, x.X09.GIOI_TINH_CON, x.X09.SO_CON, x.X09.LAN_SINH, x.X09.SO_CON_SONG, x.X09.CAN_NANG_CON, GetNgayThang(x.X09.NGAY_SINH_CON), x.X09.NOI_SINH_CON,
                                x.X09.TINH_TRANG_CON, x.X09.SINHCON_PHAUTHUAT, x.X09.SINHCON_DUOI32TUAN, x.X09.GHI_CHU, x.X09.NGUOI_DO_DE, x.X09.NGUOI_GHI_PHIEU, GetNgayThang(x.X09.NGAY_CT), x.X09.SO, x.X09.QUYEN_SO, x.X09.MA_TTDV,
                                GetNgayThang(x.X09.XML01.NGAY_VAO), GetNgayThang(x.X09.XML01.NGAY_RA), GetNgayThang(x.X09.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet9, cols, 2, -4, 12, 14, 15, 17, -24);
                    #endregion

                    #region Báo cáo XML10
                    //List<int?> xml10ErCodes = dmErrors.Where(e => e.error_file == "XML10").Select(e => e.error_code).ToList();
                    //var xml10Errors = errors.Where(a => xml10ErCodes.Contains(a.MA_LOI));

                    //var data10Join =
                    //    data1.SelectMany(x => x.XML10s)
                    //    .Join(xml10Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X10 = x, XER = e })
                    //    .Where(x => x.X10.STT == x.XER.STT);
                    var x10Codes = dmErrors.Where(e => e.error_file == "XML10").Select(e => e.error_code).ToList();
                    var x10Errors = errors.Where(a => x10Codes.Contains(a.MA_LOI)).ToList();
                    var x10Join = data1.Where(x => x10Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML10s)
                                        .Join(x10Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X10 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet10 = workbook.CreateSheet("Lỗi XML10");
                    rows = 2;
                    cols = CreateHeader(xlSheet10, cellStyles,
                                "Danh sách lỗi của XML10", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số Seri", "Số chứng từ", "Số ngày", "Đơn vị", "Chẩn đoán ra viện", "Từ ngày", "Đến ngày", "Mã TTDV", "Tên BS", "Mã BS", "Ngày chứng từ",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data10Join)
                    foreach (var x in x10Join)
                    {
                        AppendRow(xlSheet10, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X10.XML01.MA_BN, x.X10.XML01.HO_TEN, x.X10.XML01.MA_LK, x.X10.XML01.MA_THE_BHYT,
                                x.X10.SO_SERI, x.X10.SO_CT, x.X10.SO_NGAY, x.X10.DON_VI, x.X10.CHAN_DOAN_RV, GetNgayThang(x.X10.TU_NGAY), GetNgayThang(x.X10.DEN_NGAY), x.X10.MA_TTDV, x.X10.TEN_BS, x.X10.MA_BS, x.X10.NGAY_CT,
                                GetNgayThang(x.X10.XML01.NGAY_VAO), GetNgayThang(x.X10.XML01.NGAY_RA), GetNgayThang(x.X10.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet10, cols, 2, -4, 12, 14, 15, 17, -24);

                    #endregion

                    #region Báo cáo XML11
                    //List<int?> xml11ErCodes = dmErrors.Where(e => e.error_file == "XML11").Select(e => e.error_code).ToList();
                    //var xml11Errors = errors.Where(a => xml11ErCodes.Contains(a.MA_LOI));

                    //var data11Join =
                    //    data1.SelectMany(x => x.XML11s)
                    //    .Join(xml11Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X11 = x, XER = e })
                    //    .Where(x => x.X11.STT == x.XER.STT);
                    var x11Codes = dmErrors.Where(e => e.error_file == "XML11").Select(e => e.error_code).ToList();
                    var x11Errors = errors.Where(a => x11Codes.Contains(a.MA_LOI)).ToList();
                    var x11Join = data1.Where(x => x11Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML11s)
                                        .Join(x11Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X11 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet11 = workbook.CreateSheet("Lỗi XML11");
                    rows = 2;
                    cols = CreateHeader(xlSheet11, cellStyles,
                                "Danh sách lỗi của XML11", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số Chứng Từ", "Số Seri", "Số KCB", "Đơn vị", "Mã BHXH", "Mã thẻ BHYT", "Chẩn đoán ra viện", "Phương pháp điều trị", "Mã đình chỉ thai", "Nguyên nhân đình chỉ", "Tuổi thai", "Số ngày nghỉ",
                                "Từ ngày", "Đến ngày", "Họ tên Cha", "Họ tên mẹ", "Mã TTDV", "Mã BS", "Ngày CT", "Mã thẻ tạm", "Mẫu số",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data11Join)
                    foreach (var x in x11Join)
                    {
                        AppendRow(xlSheet11, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X11.XML01.MA_BN, x.X11.XML01.HO_TEN, x.X11.XML01.MA_LK, x.X11.XML01.MA_THE_BHYT,
                                x.X11.SO_CT, x.X11.SO_SERI, x.X11.SO_KCB, x.X11.DON_VI, x.X11.MA_BHXH, x.X11.MA_THE_BHYT, x.X11.CHAN_DOAN_RV, x.X11.PP_DIEUTRI, x.X11.MA_DINH_CHI_THAI, x.X11.NGUYENNHAN_DINHCHI, x.X11.TUOI_THAI, x.X11.SO_NGAY_NGHI,
                                GetNgayThang(x.X11.TU_NGAY), GetNgayThang(x.X11.DEN_NGAY), x.X11.HO_TEN_CHA, x.X11.HO_TEN_ME, x.X11.MA_TTDV, x.X11.MA_BS, GetNgayThang(x.X11.NGAY_CT), x.X11.MA_THE_TAM, x.X11.MAU_SO,
                                GetNgayThang(x.X11.XML01.NGAY_VAO), GetNgayThang(x.X11.XML01.NGAY_RA), GetNgayThang(x.X11.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet11, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML13
                    //List<int?> xml13ErCodes = dmErrors.Where(e => e.error_file == "XML13").Select(e => e.error_code).ToList();
                    //var xml13Errors = errors.Where(a => xml13ErCodes.Contains(a.MA_LOI));

                    //var data13Join =
                    //    data1.SelectMany(x => x.XML13s)
                    //    .Join(xml13Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X13 = x, XER = e })
                    //    .Where(x => x.X13.STT == x.XER.STT);
                    var x13Codes = dmErrors.Where(e => e.error_file == "XML13").Select(e => e.error_code).ToList();
                    var x13Errors = errors.Where(a => x13Codes.Contains(a.MA_LOI)).ToList();
                    var x13Join = data1.Where(x => x13Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML13s)
                                        .Join(x13Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X13 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet13 = workbook.CreateSheet("Lỗi XML13");
                    rows = 2;
                    cols = CreateHeader(xlSheet13, cellStyles,
                                "Danh sách lỗi của XML13", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số hồ sơ", "Số chuyển tuyến", "Giấy chuyển tuyến", "Mã CSKCB", "Mã nơi đi", "Mã nơi đến", "Họ Tên", "Ngày sinh", "Giới tính", "Mã quốc tịch", "Mã dân tộc", "Mã nghề nghiệp", "Địa chỉ",
                                "Mã thẻ BHYT", "Giá trị thẻ đến", "Ngày vào", "Ngày vào nội trú", "Ngày ra", "Dấu hiệu LS", "Chẩn đoán ra viện", "Quá trình bệnh lý", "Tóm tắt KQ", "Phương pháp điều trị",
                                "Mã bệnh chính", "Mã bệnh YHCT", "Tên dịch vụ", "Tên thuốc", "Phương pháp điều trị", "Mã loại ra viện", "Mã lý do chuyển tuyến", "Hướng điều trị", "Phương tiện vận chuyển", "Họ tên người hộ tống",
                                "Chức dang người hộ tống", "Mã bác sĩ", "Mã TTDV",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data13Join)
                    foreach (var x in x13Join)
                    {
                        AppendRow(xlSheet13, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X13.XML01.MA_BN, x.X13.XML01.HO_TEN, x.X13.XML01.MA_LK, x.X13.XML01.MA_THE_BHYT,
                                x.X13.SO_HOSO, x.X13.SO_CHUYENTUYEN, x.X13.GIAY_CHUYEN_TUYEN, x.X13.MA_CSKCB, x.X13.MA_NOI_DI, x.X13.MA_NOI_DEN, x.X13.HO_TEN, GetNgayThang(x.X13.NGAY_SINH), x.X13.GIOI_TINH, x.X13.MA_QUOCTICH, x.X13.MA_DANTOC, x.X13.MA_NGHE_NGHIEP, x.X13.DIA_CHI,
                                x.X13.MA_THE_BHYT, x.X13.GT_THE_DEN, GetNgayThang(x.X13.NGAY_VAO), GetNgayThang(x.X13.NGAY_VAO_NOI_TRU), GetNgayThang(x.X13.NGAY_RA), x.X13.DAU_HIEU_LS, x.X13.CHAN_DOAN_RV, x.X13.QT_BENHLY, x.X13.TOMTAT_KQ, x.X13.PP_DIEUTRI,
                                x.X13.MA_BENH_CHINH, x.X13.MA_BENH_YHCT, x.X13.TEN_DICH_VU, x.X13.TEN_THUOC, x.X13.PP_DIEU_TRI, x.X13.MA_LOAI_RV, x.X13.MA_LYDO_CT, x.X13.HUONG_DIEU_TRI, x.X13.PHUONGTIEN_VC, x.X13.HOTEN_NGUOI_HT,
                                x.X13.CHUCDANH_NGUOI_HT, x.X13.MA_BAC_SI, x.X13.MA_TTDV,
                                GetNgayThang(x.X13.XML01.NGAY_VAO), GetNgayThang(x.X13.XML01.NGAY_RA), GetNgayThang(x.X13.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet13, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML14
                    //List<int?> xml14ErCodes = dmErrors.Where(e => e.error_file == "XML14").Select(e => e.error_code).ToList();
                    //var xml14Errors = errors.Where(a => xml14ErCodes.Contains(a.MA_LOI));

                    //var data14Join =
                    //    data1.SelectMany(x => x.XML14s)
                    //    .Join(xml14Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X14 = x, XER = e })
                    //    .Where(x => x.X14.STT == x.XER.STT);
                    var x14Codes = dmErrors.Where(e => e.error_file == "XML14").Select(e => e.error_code).ToList();
                    var x14Errors = errors.Where(a => x14Codes.Contains(a.MA_LOI)).ToList();
                    var x14Join = data1.Where(x => x14Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML14s)
                                        .Join(x14Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X14 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet14 = workbook.CreateSheet("Lỗi XML14");
                    rows = 2;
                    cols = CreateHeader(xlSheet14, cellStyles,
                                "Danh sách lỗi của XML14", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Số giấy hẹn khám lại", "Mã CSKCB", "Họ tên", "Ngày sinh", "Giới tính", "Địa chỉ", "Mã thẻ BHYT", "Giá trị thẻ đến", "Ngày vào", "ngày vào nội trú", "ngày ra", "ngày hẹn khám lại", "chẩn đoán ra viện",
                                "Mã bệnh chính", "Mã bệnh kèm theo", "Mã bệnh YHCT", "Mã đối tượng KCB", "Mã bác sĩ", "Mã TTDV", "Ngày chứng tử",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data14Join)
                    foreach (var x in x14Join)
                    {
                        AppendRow(xlSheet14, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X14.XML01.MA_BN, x.X14.XML01.HO_TEN, x.X14.XML01.MA_LK, x.X14.XML01.MA_THE_BHYT,
                                x.X14.SO_GIAYHEN_KL, x.X14.MA_CSKCB, x.X14.HO_TEN, GetNgayThang(x.X14.NGAY_SINH), x.X14.GIOI_TINH, x.X14.DIA_CHI, x.X14.MA_THE_BHYT, x.X14.GT_THE_DEN, GetNgayThang(x.X14.NGAY_VAO), GetNgayThang(x.X14.NGAY_VAO_NOI_TRU), GetNgayThang(x.X14.NGAY_RA), GetNgayThang(x.X14.NGAY_HEN_KL), x.X14.CHAN_DOAN_RV,
                                x.X14.MA_BENH_CHINH, x.X14.MA_BENH_KT, x.X14.MA_BENH_YHCT, x.X14.MA_DOITUONG_KCB, x.X14.MA_BAC_SI, x.X14.MA_TTDV, GetNgayThang(x.X14.NGAY_CT),
                                GetNgayThang(x.X14.XML01.NGAY_VAO), GetNgayThang(x.X14.XML01.NGAY_RA), GetNgayThang(x.X14.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet14, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    #region Báo cáo XML15
                    //List<int?> xml15ErCodes = dmErrors.Where(e => e.error_file == "XML15").Select(e => e.error_code).ToList();
                    //var xml15Errors = errors.Where(a => xml15ErCodes.Contains(a.MA_LOI));

                    //var data15Join =
                    //    data1.SelectMany(x => x.XML15s)
                    //    .Join(xml15Errors, x => x.XML01ID, e => e.XML01ID, (x, e) => new { X15 = x, XER = e })
                    //    .Where(x => x.X15.STT == x.XER.STT);
                    var x15Codes = dmErrors.Where(e => e.error_file == "XML15").Select(e => e.error_code).ToList();
                    var x15Errors = errors.Where(a => x15Codes.Contains(a.MA_LOI)).ToList();
                    var x15Join = data1.Where(x => x15Errors.Any(e => e.XML01ID == x.XML01ID))
                                        .SelectMany(e => e.XML15s)
                                        .Join(x15Errors, x => new { x.XML01ID, x.STT }, e => new { e.XML01ID, e.STT }, (x, e) => new { X15 = x, XER = e })
                                        .Distinct().ToList();

                    ISheet xlSheet15 = workbook.CreateSheet("Lỗi XML15");
                    rows = 2;
                    cols = CreateHeader(xlSheet15, cellStyles,
                                "Danh sách lỗi của XML15", // Sheet title
                                "STT", "Mã lỗi", "Loại lỗi", "Thẻ lỗi", "Chi tiết lỗi", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã LK", "Mã thẻ",
                                "Mã bệnh nhân", "Họ tên", "Số CCCD", "PHân loại loai theo VT", "Phân loại lao theo TS", "Phân loại lao HIV", "Phân loại Lao VK", "Loại điều trị Lao", "Ngày bắt đầu điều trị lao",
                                "Phác đồ điều trị lao", "Ngày kết thúc điều trị lao", "Kết quả điều trị Lao", "Mã CSKCB", "Ngày khẳng định HIV", "Thời điểm nhận thuốc ARV", "Ngày bắt đầu điều trị CTX",
                                "Ngày vào", "Ngày ra", "Ngày Thanh toán"
                               );
                    
                    //foreach (var x in data15Join)
                    foreach (var x in x15Join)
                    {
                        AppendRow(xlSheet15, cellStyles, rows++,
                                rows - 2, x.XER.MA_LOI, string.Format(fmEtype, x.XER.MA_LOI), string.Format(fmEtag, x.XER.MA_LOI), string.Format(fmError, x.XER.MA_LOI), x.X15.XML01.MA_BN, x.X15.XML01.HO_TEN, x.X15.XML01.MA_LK, x.X15.XML01.MA_THE_BHYT,
                                x.X15.MA_BN, x.X15.HO_TEN, x.X15.SO_CCCD, x.X15.PHANLOAI_LAO_VITRI, x.X15.PHANLOAI_LAO_TS, x.X15.PHANLOAI_LAO_HIV, x.X15.PHANLOAI_LAO_VK, GetNgayThang(x.X15.NGAYBD_DTRI_LAO),
                                x.X15.PHACDO_DTRI_LAO, GetNgayThang(x.X15.NGAYKT_DTRI_LAO), x.X15.KET_QUA_DTRI_LAO, x.X15.MA_CSKCB, x.X15.NGAYKD_HIV, GetNgayThang(x.X15.BDDT_ARV), GetNgayThang(x.X15.NGAY_BAT_DAU_DT_CTX),
                                GetNgayThang(x.X15.XML01.NGAY_VAO), GetNgayThang(x.X15.XML01.NGAY_RA), GetNgayThang(x.X15.XML01.NGAY_TTOAN)
                                );
                    }

                    SetAutoFeatures(xlSheet15, cols, 2, -4, 12, 14, 15, 17, -24);


                    #endregion

                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);
                    workbook.Write(fs);
                    res.Data = fs.ToArray();
                }
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_TongHop)}");
            }

            return res;
        }

        private static string NHOM8_THEM = "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041";
        private static string NHOM8_LOAI_1 = "08.0022.0252;01.0032.0299;01.0066.1888;01.0055.0114;01.0055.0114;01.0055.0114;03.0091.0300;03.0092.0299;13.0192.0103;03.2191.0898;02.0150.0114;03.2191.0898;13.0192.0103;02.0150.0114;02.0495.0196;02.0209.0194;02.0496.0195;01.0132.0209;08.0015.0252;01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232";
        private static string NHOM8_LOAI_2 = "01.0182.0118;01.0128.0209;01.0132.0209;01.0142.0209;01.0142.0209;01.0175.0196;02.0209.0194;01.0142.0209;03.0082.0209;03.0058.0209;08.0022.0252;01.0032.0299;01.0066.1888;01.0055.0114;01.0055.0114;01.0055.0114;03.0091.0300;03.0092.0299;13.0192.0103;03.2191.0898;02.0150.0114;03.2191.0898;13.0192.0103;02.0150.0114;02.0495.0196;02.0209.0194;02.0496.0195;01.0132.0209;08.0015.0252;01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232";

        public async Task<ResponseModel<byte[]>> BaoCao_TrungDichVu(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            try
            {
                #region Linhist changed 16/04/26
                //Uti.HandleCronJobLog(_logger,$"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                //var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
                //filterModel ??= new FilterDownloadModel()
                //{
                //    FromDateCreate = DateTime.Now.Date,
                //    ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
                //};

                //filterModel.MaCSKCB = maCSKCB;

                ////Linhist changed 03/01/25
                ////var queryable = BuildQuery_XML01(filterModel);
                ////var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));
                ////await AttachInnerXML(data1, "2;3");
                //var filter = BuildQuery_XML03(filterModel);
                //var filterXLL2 = BuildQuery_XML02(filterModel);
                //IEnumerable<XML03> allXMl3 = new List<XML03>();
                //IEnumerable<XML02> allXML2 = new List<XML02>();
                //IEnumerable<DMBacSi> dmBacsis = new List<DMBacSi>();
                //IEnumerable<DMKhoaBV> dmKhoas = new List<DMKhoaBV>();
                //try
                //{
                //     allXMl3 = await _xml03Repository.GetListAsync(filter.FilterQueryable, query => query.OrderBy(x => x.NGAY_YL), query => query.Include(x => x.XML01));
                //     allXML2 = await _xml02Repository.GetListAsync(filterXLL2.FilterQueryable, query => query.OrderBy(x => x.NGAY_YL), query => query.Include(x => x.XML01));

                //     dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
                //     dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);
                //}catch(Exception ex)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                //}
                #endregion

                var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
                filterModel ??= new FilterDownloadModel()
                {
                    FromDateCreate = DateTime.Now.Date,
                    ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
                    MaCSKCB = maCSKCB
                };

                if (string.IsNullOrEmpty(filterModel.MaCSKCB))
                {
                    filterModel.MaCSKCB = maCSKCB;
                }

                var queryable = FilesService130.BuildQueryToGetListData(filterModel);
                var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));

                //update xử lý các TH lấy báo cáo quá lớn
                if (data1.Count() >= 50000)
                {
                    Uti.HandleCronJobLog(_logger, $"Lỗi khi lấy thông tin BC Trùng DV, Đơn vị: {maCSKCB} - Số lượng hồ sơ: {data1.Count()}, người lấy báo cáo:", $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)} - BC Tổng hợp");
                    return new ResponseModel<byte[]>(
                        HttpStatusCode.RequestEntityTooLarge,
                        $"Report too large: {data1.Count()} records. Narrow your filter or export in smaller batches.",
                        null
                    );

                }
                await AttachInnerXML(data1, "2;3");
                var allXML2 = data1.SelectMany(x => x.XML02s);
                var allXMl3 = data1.SelectMany(x => x.XML03s);

                var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
                var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

                //Create new excel
                IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{dmBacsis.Count() + 5},2,FALSE),\"\")";
                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{dmBacsis.Count() + 5},3,FALSE),\"\")";
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";

                //Linhist removed 03/01/25
                //var allXML2 = data1.SelectMany(x => x.XML02s);
                //var allXMl3 = data1.SelectMany(x => x.XML03s);

                using (var fs = new MemoryStream())
                {
                    try
                    {
                        //Linhist added 270826 - bỏ dv 17.0011.0237 cho 17018
                        var skip_nhom8 = NHOM8_LOAI_1;
                        if (maCSKCB == "17018")
                        {
                            skip_nhom8 = skip_nhom8.Replace(";17.0011.0237;", ";");
                        }

                        var nhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, NHOM8_THEM)) &&
                                                            !Uti.CodeContain(x.MA_DICH_VU, skip_nhom8) &&
                                                            !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) &&
                                                            Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) &&
                                                            x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date
                                                        );

                        //Linhist added 06/04/26 - lấy lẻ từng người thực hiện
                        var dataNhom8 = nhom8.SelectMany(x => x.NGUOI_THUC_HIEN.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                        #region Sheet 1: Loi BS DV Trung TG - Nhom 8
                        try
                        {
                            //var dataNhom8 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, NHOM8_THEM)) &&
                            //                                    !Uti.CodeContain(x.MA_DICH_VU, NHOM8_LOAI_1) &&
                            //                                    !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) &&
                            //                                    x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date
                            //                                );
                            var xml3Nhom8Trung = dataNhom8.Join(dataNhom8, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { x1 = a.Data, x2 = b.Data, a.NGUOI_TH })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x2.NGAY_KQ >= a.x1.NGAY_TH_YL && a.x2.NGAY_TH_YL <= a.x1.NGAY_KQ)
                                                    .Distinct().ToList();

                            ISheet xlSheetG8 = workbook.CreateSheet("Trùng Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật)");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetG8, cellStyles,
                                        "Danh sách Bác sĩ & Dịch vụ trùng thời gian - Nhóm 8 (Phãu thuật) và nhóm 18 (Thủ Thuật)", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                        "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                        "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                            foreach (var t in xml3Nhom8Trung)
                            {
                                string maBS1 = t.x1.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.NGUOI_THUC_HIEN;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                                string maTrung = t.NGUOI_TH;
                                string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                                AppendRow(xlSheetG8, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                        t.NGUOI_TH, ttTrung[0],
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                                    );
                            }
                            SetAutoFeatures(xlSheetG8, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV - Nhóm 8 - 18, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Nhóm 8-18: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 2: Báo cáo trùng thời gian ra y lệnh khi đang thực hiện Phẫu thuật, thủ thuật
                        try
                        {
                            var nhom8A1 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, NHOM8_THEM)) &&
                                                                !Uti.CodeContain(x.MA_DICH_VU, NHOM8_LOAI_2) &&
                                                                !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) &&
                                                                x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date
                                                            );
                            //Linhist added 06/04/26 - lấy lẻ từng người thực hiện
                            var dataNhom8A1 = nhom8A1.SelectMany(x => x.NGUOI_THUC_HIEN.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                        .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                            //Linhist changed 06/04/26
                            //var dataNhomKhac = allXMl3.Where(x => /*(x.MA_NHOM != 8 && x.MA_NHOM != 18) && */x.NGAY_YL != null && !Uti.CodeContain(x.MA_DICH_VU, NHOM8_LOAI_2))
                            //                            .Distinct().ToList();
                            var dataNhomKhac = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI) && !Uti.CodeContain(x.MA_DICH_VU, NHOM8_LOAI_2) && Uti.IsValidDate(x.NGAY_YL))
                                                        .Distinct().ToList();

                            //Linhist changed 06/04/26
                            //var datatrungnhom8vaYL = dataNhom8A1.Join(dataNhomKhac, nhom8A1 => nhom8A1.NGUOI_THUC_HIEN.Split(";")[0].Trim(),
                            //                                    nhomKhac => nhomKhac.MA_BAC_SI.ToString().Trim(),
                            //                                    (nhom8A1, nhomKhac) => new { nhom8A1, nhomKhac }
                            //                                    ).Where(joined =>
                            //                                    joined.NhomK.NGAY_YL >= joined.Nhom8.NGAY_TH_YL &&
                            //                                    joined.NhomK.NGAY_YL <= joined.Nhom8.NGAY_KQ &&
                            //                                    joined.NhomK.MA_LK != joined.Nhom8.MA_LK // Loại trừ trường hợp cùng 1 lượt khám
                            //                                    ).ToList();
                            var datatrungnhom8vaYL = dataNhom8A1.Join(dataNhomKhac, a => a.NGUOI_TH, b => b.MA_BAC_SI.Trim(), (a, b) => new { Nhom8 = a.Data, NhomK = b })
                                                                .Where(g => g.NhomK.MA_LK != g.Nhom8.MA_LK && g.NhomK.NGAY_YL >= g.Nhom8.NGAY_TH_YL && g.NhomK.NGAY_YL <= g.Nhom8.NGAY_KQ)
                                                                .Distinct().ToList();

                            ISheet xlSheetG85 = workbook.CreateSheet("Bác sĩ ra Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật, Thủ thuật");
                            int rowss = 2;
                            int colss = CreateHeader(xlSheetG85, cellStyles,
                                        "Danh sách Bác sĩ Bác sĩ ra Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật, Thủ thuật", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                        "Mã Lượt khám 2", /*"/*Mã Bác sĩ TH 2"*/ "Mã bác sĩ Y lệnh 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Mã Vật Tư 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                            foreach (var t in datatrungnhom8vaYL)
                            {
                                string maBS1 = t.Nhom8.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.NhomK.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetG85, cellStyles, rowss++,
                                        rowss - 2, t.Nhom8.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.Nhom8.XML01.MA_BN, t.Nhom8.XML01.HO_TEN,
                                        t.Nhom8.XML01.MA_THE_BHYT, GetNgayThang(t.Nhom8.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Nhom8.XML01.GIOI_TINH),
                                        t.Nhom8.MA_KHOA, string.Format(fmKhoa, t.Nhom8.MA_KHOA), t.Nhom8.MA_NHOM,
                                        t.Nhom8.MA_DICH_VU, t.Nhom8.TEN_DICH_VU, GetNgayThang(t.Nhom8.NGAY_YL), GetNgayThang(t.Nhom8.NGAY_TH_YL), GetNgayThang(t.Nhom8.NGAY_KQ),
                                        t.NhomK.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.NhomK.XML01.MA_BN, t.NhomK.XML01.HO_TEN,
                                        t.NhomK.XML01.MA_THE_BHYT, GetNgayThang(t.NhomK.XML01.NGAY_SINH, true), GetType_GioiTinh(t.NhomK.XML01.GIOI_TINH),
                                        t.NhomK.MA_KHOA, string.Format(fmKhoa, t.NhomK.MA_KHOA), t.NhomK.MA_NHOM,
                                        t.NhomK.MA_DICH_VU, t.NhomK.TEN_DICH_VU, t.NhomK.MA_VAT_TU, GetNgayThang(t.NhomK.NGAY_YL), GetNgayThang(t.NhomK.NGAY_TH_YL), GetNgayThang(t.NhomK.NGAY_KQ)
                                    );
                            }
                            SetAutoFeatures(xlSheetG85, colss, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Y lệnh trong quá trình PT-TT: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 3: Báo cáo trùng thời gian thực hiện y lệnh khi đang thực hiện Phẫu thuật, thủ thuật
                        try
                        {
                            //Linhist changed 06/04/26 - dùng lại dataNhom8
                            //var dataNhom8A2 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, NHOM8_THEM)) &&
                            //                                    !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null &&
                            //                                    !Uti.CodeContain(x.MA_DICH_VU, NHOM8_LOAI_1)
                            //                                    && x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date
                            //                                );
                            //var dataNhomKhac2 = allXMl3.Where(x => (x.MA_NHOM == 8 && x.MA_NHOM == 18) && x.NGAY_TH_YL != null);

                            var dataNhomKhac2 = allXMl3.Where(x => !Uti.NumExists(x.MA_NHOM, 8, 13, 14, 15, 18) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) && Uti.IsValidDate(x.NGAY_TH_YL))
                                                        .SelectMany(x => x.NGUOI_THUC_HIEN.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                        .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                            //var datatrungnhom8vaYL2 = dataNhom8A2.Join(dataNhomKhac2, nhom8A1 => nhom8A1.NGUOI_THUC_HIEN.Split(";")[0].Trim(),
                            //                                    nhomKhac => nhomKhac.MA_BAC_SI.ToString().Trim(),
                            //                                    (nhom8A1, nhomKhac) => new { nhom8A1, nhomKhac }
                            //                                    ).Where(joined =>
                            //                                    joined.NhomK.NGAY_YL >= joined.Nhom8.NGAY_TH_YL &&
                            //                                    joined.NhomK.NGAY_YL <= joined.Nhom8.NGAY_KQ &&
                            //                                    joined.NhomK.MA_LK != joined.Nhom8.MA_LK // Loại trừ trường hợp cùng 1 lượt khám
                            //                                    ).ToList();
                            var datatrungnhom8vaYL2 = dataNhom8.Join(dataNhomKhac2, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { Nhom8 = a.Data, NhomK = b.Data, a.NGUOI_TH })
                                                                .Where(g => g.NhomK.MA_DICH_VU != g.Nhom8.MA_DICH_VU && g.NhomK.NGAY_KQ >= g.Nhom8.NGAY_TH_YL && g.NhomK.NGAY_TH_YL <= g.Nhom8.NGAY_KQ)
                                                                .Distinct().ToList();

                            ISheet xlSheetG86 = workbook.CreateSheet("Bác sĩ TH Y lệnh dịch vụ khác trong quá trình thực hiện Phẫu thuật, Thủ thuật");
                            int rowsss = 2;
                            int colsss = CreateHeader(xlSheetG86, cellStyles,
                                        "Danh sách Bác sĩ Bác sĩ thực hiện Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật, Thủ thuật", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Mã Vật tư 1", "Tên Vật tư 1",
                                        "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                        "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                        "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Mã Vật tư 2", "Tên Vật tư 2",
                                        "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2");

                            foreach (var t in datatrungnhom8vaYL2)
                            {
                                string maBS1 = t.Nhom8.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.NhomK.NGUOI_THUC_HIEN;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                                string maTrung = t.NGUOI_TH;
                                string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                                AppendRow(xlSheetG86, cellStyles, rowsss++,
                                        rowsss - 2, t.Nhom8.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.Nhom8.XML01.MA_BN, t.Nhom8.XML01.HO_TEN,
                                        t.Nhom8.XML01.MA_THE_BHYT, GetNgayThang(t.Nhom8.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Nhom8.XML01.GIOI_TINH),
                                        t.Nhom8.MA_KHOA, string.Format(fmKhoa, t.Nhom8.MA_KHOA), t.Nhom8.MA_NHOM,
                                        t.Nhom8.MA_DICH_VU, t.Nhom8.TEN_DICH_VU, t.Nhom8.MA_VAT_TU, t.Nhom8.TEN_VAT_TU,
                                        GetNgayThang(t.Nhom8.NGAY_YL), GetNgayThang(t.Nhom8.NGAY_TH_YL), GetNgayThang(t.Nhom8.NGAY_KQ),
                                        maTrung, ttTrung[0],
                                        t.NhomK.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.NhomK.XML01.MA_BN, t.NhomK.XML01.HO_TEN,
                                        t.NhomK.XML01.MA_THE_BHYT, GetNgayThang(t.NhomK.XML01.NGAY_SINH, true), GetType_GioiTinh(t.NhomK.XML01.GIOI_TINH),
                                        t.NhomK.MA_KHOA, string.Format(fmKhoa, t.NhomK.MA_KHOA), t.NhomK.MA_NHOM,
                                        t.NhomK.MA_DICH_VU, t.NhomK.TEN_DICH_VU, t.NhomK.MA_VAT_TU, t.NhomK.TEN_VAT_TU,
                                        GetNgayThang(t.NhomK.NGAY_YL), GetNgayThang(t.NhomK.NGAY_TH_YL), GetNgayThang(t.NhomK.NGAY_KQ)
                                    );
                            }
                            SetAutoFeatures(xlSheetG86, colsss, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet TH Y lệnh trong quá trình PT-TT: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 4: Báo cáo trùng thời gian ra y lệnh thuốc trong lúc khám bệnh
                        try
                        {
                            //Linhist changed 06/04/26
                            //var dataNhomKB = allXMl3.Where(x => x.MA_NHOM == 13 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_TH_YL != null && x.NGAY_KQ != null && x.NGUOI_THUC_HIEN != null);
                            //var datanhomThuoc = allXML2.Where(x => x.NGAY_YL != null && x.MA_BAC_SI != null);
                            //var dataTrungnhom = dataNhomKB.Join(datanhomThuoc, nhomKB => nhomKB.NGUOI_THUC_HIEN.Split(";")[0].Trim(),
                            //                                    nhomThuoc => nhomThuoc.MA_BAC_SI.ToString().Trim(),
                            //                                    (nhomKB, nhomThuoc) => new { nhomKB, nhomThuoc }
                            //                                    ).Where(joined =>
                            //                                    joined.Thuoc.NGAY_YL >= joined.Kham.NGAY_TH_YL &&
                            //                                    joined.Thuoc.NGAY_YL <= joined.Kham.NGAY_KQ &&
                            //                                    joined.Thuoc.MA_LK != joined.Kham.MA_LK
                            //                                    ).ToList();

                            var dataNhomKB = allXMl3.Where(x => x.MA_NHOM == 13 && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_TH_YL, x.NGAY_KQ))
                                                    .SelectMany(x => x.NGUOI_THUC_HIEN.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                    .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                            var datanhomThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL));

                            var dataTrungnhom = dataNhomKB.Join(datanhomThuoc, a => a.NGUOI_TH, b => b.MA_BAC_SI.Trim(), (a, b) => new { Kham = a.Data, Thuoc = b })
                                                            .Where(g => g.Kham.MA_LK != g.Thuoc.MA_LK && g.Thuoc.NGAY_YL >= g.Kham.NGAY_TH_YL && g.Thuoc.NGAY_YL <= g.Kham.NGAY_KQ)
                                                            .Distinct().ToList();

                            ISheet slSheetNhomKB = workbook.CreateSheet("Thực hiện y lệnh thuốc khi đang khám");
                            int rowssanhom = 2;
                            int colssathuoc = CreateHeader(slSheetNhomKB, cellStyles,
                                        "Danh sách Bác sĩ đang khám thực hiện y lệnh thuốc cho BN khác", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                        "Mã Lượt khám 2", /*"/*Mã Bác sĩ TH 2"*/ "Mã bác sĩ Y lệnh 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Thuốc 2", "Tên Thuốc 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2");

                            foreach (var t in dataTrungnhom)
                            {
                                string maBS1 = t.Kham.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.Thuoc.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(slSheetNhomKB, cellStyles, rowssanhom++,
                                        colssathuoc - 2, t.Kham.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.Kham.XML01.MA_BN, t.Kham.XML01.HO_TEN,
                                        t.Kham.XML01.MA_THE_BHYT, GetNgayThang(t.Kham.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Kham.XML01.GIOI_TINH),
                                        t.Kham.MA_KHOA, string.Format(fmKhoa, t.Kham.MA_KHOA), t.Kham.MA_NHOM,
                                        t.Kham.MA_DICH_VU, t.Kham.TEN_DICH_VU, GetNgayThang(t.Kham.NGAY_YL), GetNgayThang(t.Kham.NGAY_TH_YL), GetNgayThang(t.Kham.NGAY_KQ),
                                        t.Thuoc.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.Thuoc.XML01.MA_BN, t.Thuoc.XML01.HO_TEN,
                                        t.Thuoc.XML01.MA_THE_BHYT, GetNgayThang(t.Thuoc.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Thuoc.XML01.GIOI_TINH),
                                        t.Thuoc.MA_KHOA, string.Format(fmKhoa, t.Thuoc.MA_KHOA), t.Thuoc.MA_NHOM,
                                        t.Thuoc.MA_THUOC, t.Thuoc.TEN_THUOC, GetNgayThang(t.Thuoc.NGAY_YL), GetNgayThang(t.Thuoc.NGAY_TH_YL)
                                    );
                            }
                            SetAutoFeatures(slSheetNhomKB, colssathuoc, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Y lệnh trong quá trình khám bệnh: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }

                        #endregion

                        #region Sheet 5: Báo cáo trùng thời gian ra y lệnh thuốc khi đang thực hiện Phẫu thuật, thủ thuật
                        try
                        {
                            //Linhist changed 06/04/26 - dùng lại dataNhom8
                            //var dataNhom8A3 = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041")) &&
                            //                                    !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null &&
                            //                                    !Uti.CodeContain(x.MA_DICH_VU, "08.0022.0252;01.0032.0299;01.0066.1888;01.0055.0114;01.0055.0114;01.0055.0114;03.0091.0300;03.0092.0299;13.0192.0103;03.2191.0898;02.0150.0114;03.2191.0898;13.0192.0103;02.0150.0114;02.0495.0196;02.0209.0194;02.0496.0195;01.0132.0209;08.0015.0252;01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232")
                            //                                    && x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date
                            //                                );
                            //var dataNhomKhac3 = allXML2.Where(x => x.NGAY_YL != null && x.MA_BAC_SI != null);

                            //var datatrungnhom8vaYL3 = dataNhom8A3.Join(dataNhomKhac3, nhom8A1 => nhom8A1.NGUOI_THUC_HIEN.Split(";")[0].Trim(),
                            //                                    nhomKhac => nhomKhac.MA_BAC_SI.ToString().Trim(),
                            //                                    (nhom8A1, nhomKhac) => new { nhom8A1, nhomKhac }
                            //                                    ).Where(joined =>
                            //                                    joined.NhomK.NGAY_YL >= joined.Nhom8.NGAY_TH_YL &&
                            //                                    joined.NhomK.NGAY_YL <= joined.Nhom8.NGAY_KQ
                            //                                    ).ToList();
                            var dataNhomKhac3 = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL));

                            var datatrungnhom8vaYL3 = dataNhom8.Join(dataNhomKhac3, a => a.NGUOI_TH, b => b.MA_BAC_SI.Trim(), (a, b) => new { Nhom8 = a.Data, NhomK = b })
                                                                .Where(e => e.NhomK.NGAY_YL >= e.Nhom8.NGAY_TH_YL && e.NhomK.NGAY_YL <= e.Nhom8.NGAY_KQ)
                                                                .Distinct().ToList();

                            ISheet xlSheetG87 = workbook.CreateSheet("Bác sĩ ra Y lệnh thuốc trong quá trình thực hiện Phẫu thuật, Thủ thuật");
                            int rowssa = 2;
                            int colssa = CreateHeader(xlSheetG87, cellStyles,
                                        "Danh sách Bác sĩ Bác sĩ ra Y lệnh Thuốc trong quá trình thực hiện Phẫu thuật, Thủ thuật", // Sheet title
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1",
                                        "Mã Lượt khám 2", /*"/*Mã Bác sĩ TH 2"*/ "Mã bác sĩ Y lệnh 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                        "Mã Thuốc 2", "Tên Thuốc 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2");

                            foreach (var t in datatrungnhom8vaYL3)
                            {
                                string maBS1 = t.Nhom8.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.NhomK.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetG87, cellStyles, rowssa++,
                                        rowssa - 2, t.Nhom8.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.Nhom8.XML01.MA_BN, t.Nhom8.XML01.HO_TEN,
                                        t.Nhom8.XML01.MA_THE_BHYT, GetNgayThang(t.Nhom8.XML01.NGAY_SINH, true), GetType_GioiTinh(t.Nhom8.XML01.GIOI_TINH),
                                        t.Nhom8.MA_KHOA, string.Format(fmKhoa, t.Nhom8.MA_KHOA), t.Nhom8.MA_NHOM,
                                        t.Nhom8.MA_DICH_VU, t.Nhom8.TEN_DICH_VU, GetNgayThang(t.Nhom8.NGAY_YL), GetNgayThang(t.Nhom8.NGAY_TH_YL), GetNgayThang(t.Nhom8.NGAY_KQ),
                                        t.NhomK.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.NhomK.XML01.MA_BN, t.NhomK.XML01.HO_TEN,
                                        t.NhomK.XML01.MA_THE_BHYT, GetNgayThang(t.NhomK.XML01.NGAY_SINH, true), GetType_GioiTinh(t.NhomK.XML01.GIOI_TINH),
                                        t.NhomK.MA_KHOA, string.Format(fmKhoa, t.NhomK.MA_KHOA), t.NhomK.MA_NHOM,
                                        t.NhomK.MA_THUOC, t.NhomK.TEN_THUOC, GetNgayThang(t.NhomK.NGAY_YL), GetNgayThang(t.NhomK.NGAY_TH_YL)
                                    );
                            }
                            SetAutoFeatures(xlSheetG87, colssa, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Y lệnh trong quá trình PT-TT: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 6: Loi BS DV Trung TG - Nhom 2
                        try
                        {
                            //Linhist changed 06/04/26 - lấy lẻ từng người thực hiện
                            //var dataNhom2 = allXMl3.Where(x => x.MA_NHOM == 2 || x.MA_NHOM == 3 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && x.MA_MAY != null);

                            //var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, x1 => x1.NGUOI_THUC_HIEN, x2 => x2.NGUOI_THUC_HIEN, (x1, x2) => new { x1, x2 })
                            //                        .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.MA_MAY == a.x2.MA_MAY && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL);
                            var nhom2 = allXMl3.Where(x => (x.MA_NHOM == 2 || x.MA_NHOM == 3) &&
                                                            !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) &&
                                                            Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) && !string.IsNullOrWhiteSpace(x.MA_MAY));

                            var dataNhom2 = nhom2.SelectMany(x => x.NGUOI_THUC_HIEN.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                            .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH)).Distinct().ToList();

                            var xml3Nhom2Trung = dataNhom2.Join(dataNhom2, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { x1 = a.Data, x2 = b.Data, a.NGUOI_TH })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.MA_MAY.Trim() == a.x2.MA_MAY.Trim() && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetG2 = workbook.CreateSheet("Trùng thực hiện  Nhóm 2,3 - CĐHA -TDCN");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetG2, cellStyles,
                                                "Danh sách Bác sĩ thực hiện trên cùng mã máy - Nhóm 2,3 CĐHA -TDCN", // Sheet title
                                                "STT", "Mã lượt khám 1", "Mã Bác sĩ TH 1", "Tên Bác sĩ TH 1", "Chức danh 1",
                                                "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                                "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                                "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                                "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                                "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                                "Mã Lượt khám 2", "Mã Bác sĩ TH 2", "Tên Bác sĩ TH 2", "Chức danh 2",
                                                "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                                "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                                "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                                "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                            foreach (var t in xml3Nhom2Trung)
                            {
                                string maBS1 = t.x1.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.NGUOI_THUC_HIEN;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                                string maTrung = t.NGUOI_TH;
                                string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                                AppendRow(xlSheetG2, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                        maTrung, ttTrung[0],
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                    );
                            }

                            SetAutoFeatures(xlSheetG2, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thực hiện Nhóm 2,3: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 7: Báo cáo trùng thời gian thực hiện y lệnh dịch vụ
                        try
                        {
                            //Linhist changed 06/04/26 - lấy lẻ từng người thực hiện
                            //var dataTrungTHYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.CodeMatch(x.MA_DICH_VU, "08.0015.0252;08.0022.0252") && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15, 1, 13));

                            //var dataTrung = dataTrungTHYL.Join(dataTrungTHYL, x1 => x1.NGUOI_THUC_HIEN, x2 => x2.NGUOI_THUC_HIEN, (x1, x2) => new { x1, x2 })
                            //                        .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL);
                            var trungTHYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) &&
                                                                !Uti.CodeMatch(x.MA_DICH_VU, "08.0015.0252;08.0022.0252") &&
                                                                Uti.IsValidDate(x.NGAY_TH_YL) && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15, 1, 13));

                            var dataTrungTHYL = trungTHYL.SelectMany(x => x.NGUOI_THUC_HIEN.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(e => new { Data = x, NGUOI_TH = e.Trim() }))
                                                    .Where(e => !string.IsNullOrEmpty(e.NGUOI_TH))
                                                    .Distinct().ToList();

                            var dataTrung = dataTrungTHYL.Join(dataTrungTHYL, a => a.NGUOI_TH, b => b.NGUOI_TH, (a, b) => new { x1 = a.Data, x2 = b.Data, a.NGUOI_TH })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetG8TH = workbook.CreateSheet("Trùng thời gian thực hiện XML3 dịch vụ kỹ thuật");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetG8TH, cellStyles,
                                       "Danh sách Trùng thời gian thực hiện XML3 Thực hiện Y Lệnh", // Sheet title
                                       "STT", "Mã lượt khám 1", "Mã Bác sĩ thực hiện 1", "Tên Bác sĩ thực hiện 1", "Chức danh 1",
                                       "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                       "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                       "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                       "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1", "Ngày Kết quả 1", "Mã máy 1",
                                        "Mã CCHN trùng", "Tên Bác sĩ trùng",
                                       "Mã Lượt khám 2", "Mã Bác sĩ thực hiện 2", "Tên Bác sĩ thực hiện 2", "Chức danh 2",
                                       "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                       "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                       "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                       "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2", "Ngày thực hiện y lệnh 2", "Ngày Kết quả 2", "Mã máy 2");

                            foreach (var t in dataTrung)
                            {
                                string maBS1 = t.x1.NGUOI_THUC_HIEN;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.NGUOI_THUC_HIEN;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);
                                string maTrung = t.NGUOI_TH;
                                string[] ttTrung = GetFormular_BacSi(new string[] { fmBacsi }, ref maTrung);

                                AppendRow(xlSheetG8TH, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                        maTrung, ttTrung[0],
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                    );
                            }

                            SetAutoFeatures(xlSheetG8TH, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian thực hiện y lệnh dịch vụ: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        //Linhist changed 06/04/26 - chỉnh điều kiện
                        //var dataTrungKB = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && Uti.NumExists(x.MA_NHOM, 13));
                        var dataTrungKB = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ, x.NGAY_TH_YL) && Uti.NumExists(x.MA_NHOM, 13))
                                                    .Distinct().ToList();

                        #region Sheet 8: Báo cáo trùng thời gian thực hiện y lệnh khám bệnh (Trùng giờ Thực hiện)
                        try
                        {
                            //Linhist changed 06/04/26 - chỉnh điều kiện và cho ra ngoài
                            //var dataTrungKB = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && Uti.NumExists(x.MA_NHOM, 13));

                            var dataTrungx = dataTrungKB.Join(dataTrungKB, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetGxTH = workbook.CreateSheet("Trùng thời gian y lệnh Khám");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetGxTH, cellStyles,
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
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetGxTH, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                                    );
                            }

                            SetAutoFeatures(xlSheetGxTH, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh khám bệnh: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 9: Báo cáo trùng thời gian thực hiện y lệnh khám bệnh
                        try
                        {
                            //Linhist changed 06/04/26 - sử dụng dataTrungKB đã lấy ở trên
                            //var dataTrungKBx = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && Uti.NumExists(x.MA_NHOM, 13));

                            var dataTrungxx = dataTrungKB.Join(dataTrungKB, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetGxTHx = workbook.CreateSheet("Trùng thời gian thực hiện YL Khám");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetGxTHx, cellStyles,
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
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetGxTHx, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ),
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ)
                                    );
                            }
                            SetAutoFeatures(xlSheetGxTHx, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian thực hiện y lệnh khám bệnh: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }

                        #endregion

                        #region Sheet 10: Báo cáo lỗi trùng giường
                        try
                        {
                            //Linhist changed 06/04/26 - chỉnh điều kiện
                            //var dataGiuong = allXMl3.Where(x => (x.MA_NHOM == 14 || x.MA_NHOM == 15) && !string.IsNullOrEmpty(x.MA_GIUONG) && x.NGAY_YL != null && x.NGAY_KQ != null);
                            var dataGiuong = allXMl3.Where(x => (x.MA_NHOM == 14 || x.MA_NHOM == 15) && Uti.IsValidDate(x.NGAY_YL, x.NGAY_KQ))
                                                    .Distinct().ToList();

                            var xml3GiuongTrung = dataGiuong.Join(dataGiuong, x1 => x1.MA_GIUONG?.Trim(), x2 => x2.MA_GIUONG?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL <= a.x2.NGAY_YL && a.x1.NGAY_KQ >= a.x2.NGAY_YL &&
                                                                    a.x1.MA_KHOA == a.x2.MA_KHOA && (a.x1.TYLE_TT_DV > 50 || a.x2.TYLE_TT_DV > 50))
                                                    .Distinct().ToList();

                            ISheet xlSheetGG = workbook.CreateSheet("Trùng Giường");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetGG, cellStyles,
                                        "Danh sách Dịch vụ Giường trùng thời gian", // Sheet caption
                                        "STT", "Mã lượt khám 1", "Mã Bác sĩ 1", "Tên Bác sĩ 1", "Chức danh 1",
                                        "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                        "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                        "Mã Khoa 1", "Tên Khoa 1",
                                        "Mã Dịch vụ 1", "Tên Dịch vụ 1", "Giường 1", "Tỷ lệ thanh toán DV 1",
                                        "Ngày Kết quả 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1",
                                        "Mã Lượt khám 2", "Mã Bác sĩ 2", "Tên Bác sĩ 2", "Chức danh 2",
                                        "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                        "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                        "Mã Khoa 2", "Tên Khoa 2",
                                        "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Giường 2", "Tỷ lệ thanh toán DV 2",
                                        "Ngày Kết quả 2", "Ngày Y lệnh 2", "Ngày Thực hiện YL 2");

                            foreach (var t in xml3GiuongTrung)
                            {
                                string maBS1 = t.x1.MA_BAC_SI;
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetGG, cellStyles, rows++,
                                    rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                    t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                    t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                    t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA),
                                    t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, t.x1.MA_GIUONG, t.x1.TYLE_TT_DV,
                                    GetNgayThang(t.x1.NGAY_KQ), GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL),
                                    t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                    t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                    t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                    t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA),
                                    t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, t.x2.MA_GIUONG, t.x2.TYLE_TT_DV,
                                    GetNgayThang(t.x2.NGAY_KQ), GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL));
                            }

                            SetAutoFeatures(xlSheetGG, cols, 3, 4, 11, 19, 20, 27);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng Giường: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 11: Báo cáo trùng thời gian y lệnh dịch vụ
                        try
                        {
                            //Linhist changed 06/04/26 - chỉnh điều kiện
                            //var dataTrungYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15));
                            var dataTrungYL = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL) && !Uti.NumExists(x.MA_NHOM, 10, 11, 14, 15))
                                                    .Distinct().ToList();

                            var dataTrungTYL = dataTrungYL.Join(dataTrungYL, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetG9TH = workbook.CreateSheet("Trùng thời gian y lệnh XML3 dịch vụ kỹ thuật");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetG9TH, cellStyles,
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

                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetG9TH, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                    );
                            }

                            SetAutoFeatures(xlSheetG9TH, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh dịch vụ: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 12: Báo cáo trùng thời gian y lệnh thuốc
                        try
                        {
                            //Linhist changed 06/04/26 - chỉnh điều kiện
                            //var dataTrungYLThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_THUOC) && x.NGAY_YL != null && Uti.NumExists(x.MA_NHOM, 4));
                            var dataTrungYLThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_THUOC) && Uti.IsValidDate(x.NGAY_YL) && Uti.NumExists(x.MA_NHOM, 4))
                                                            .Distinct().ToList();

                            var dataTrungThuoc = dataTrungYLThuoc.Join(dataTrungYLThuoc, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetThuoc = workbook.CreateSheet("Trùng thời gian y lệnh XML2 Thuốc");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetThuoc, cellStyles,
                                       "Danh sách Trùng thời gian XML2 Y Lệnh", // Sheet title
                                       "STT", "Mã lượt khám 1", "Mã Bác sĩ YL 1", "Tên Bác sĩ YL 1", "Chức danh 1",
                                       "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                       "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                       "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                       "Mã Thuốc 1", "Tên Thuốc 1", "Ngày Y lệnh 1", "Ngày thực hiện Y lệnh 1",
                                       "Mã Lượt khám 2", "Mã Bác sĩ YL 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                       "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                       "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                       "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                       "Mã Thuốc 2", "Tên Thuốc 2", "Ngày Y lệnh 2", "Ngày thực hiện Y lệnh 2");

                            foreach (var t in dataTrungThuoc)
                            {
                                string maBS1 = t.x1.MA_BAC_SI;

                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetThuoc, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_THUOC, t.x1.TEN_THUOC, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL),
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_THUOC, t.x2.TEN_THUOC, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL)
                                    );
                            }

                            SetAutoFeatures(xlSheetThuoc, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh thuốc: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 13: Loi BS DV Trung TG - Nhom 2,3
                        try
                        {
                            //Linhist changed 06/04/26 - chỉnh điều kiện
                            //var dataNhom2MM = allXMl3.Where(x => x.MA_NHOM == 2 || x.MA_NHOM == 3 && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null && x.MA_MAY != null);
                            var dataNhom2MM = allXMl3.Where(x => (x.MA_NHOM == 2 || x.MA_NHOM == 3) && !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_MAY) && Uti.IsValidDate(x.NGAY_TH_YL))
                                                    .Distinct().ToList();

                            var xml3Nhom2TrungMM = dataNhom2MM.Join(dataNhom2MM, x1 => x1.MA_MAY?.Trim(), x2 => x2.MA_MAY?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_TH_YL == a.x2.NGAY_TH_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetG2MM = workbook.CreateSheet("Mã Máy  Nhóm 2,3 - CĐHA -TDCN");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetG2MM, cellStyles,
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
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.NGUOI_THUC_HIEN;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetG2MM, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                    );
                            }

                            SetAutoFeatures(xlSheetG2MM, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Mã Máy  Nhóm 2,3 - CĐHA -TDCN: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 14: Loi MM- Nhom 8
                        try
                        {
                            //Linhist changed 06/04/26 - sử dụng nhom8
                            //var dataNhom8MM = allXMl3.Where(x => (x.MA_NHOM == 8 || x.MA_NHOM == 18 || Uti.CodeContain(x.MA_DICH_VU, "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041")) &&
                            //                                    !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null &&
                            //                                    !string.IsNullOrEmpty(x.MA_MAY) &&
                            //                                    !Uti.CodeContain(x.MA_DICH_VU, "01.0131.0209;01.0132.0209;01.0133.0209;01.0175.0196;02.0209.0194;03.0463.0230;03.0464.0230;03.0467.0230;03.0478.0230;03.0479.0230;03.0483.0230;03.0484.0230;03.0497.0230;03.0522.0230;03.0527.0230;03.0529.0230;03.0530.0230;17.0026.0220;17.0007.0234;17.0008.0253;17.0001.0254;08.0293.0230;08.0301.0230;08.0313.0230;17.0011.0237;17.0006.0231;17.0004.0232")
                            //                                    && x.NGAY_TH_YL.Value.Date == x.NGAY_KQ.Value.Date
                            //                                );

                            //var xml3Nhom8TrungMM = dataNhom8MM.Join(dataNhom8MM, x1 => x1.MA_MAY, x2 => x2.MA_MAY, (x1, x2) => new { x1, x2 })
                            //                        .Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ));

                            var nhom8MM = nhom8.Where(x => !string.IsNullOrEmpty(x.MA_MAY));
                            var xml3Nhom8TrungMM = nhom8MM.Join(nhom8MM, x1 => x1.MA_MAY, x2 => x2.MA_MAY, (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && !(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ))
                                                    .Distinct().ToList();

                            ISheet xlSheetG8MM = workbook.CreateSheet("Trùng Mã Máy Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật)");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetG8MM, cellStyles,
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
                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.NGUOI_THUC_HIEN;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetG8MM, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_DICH_VU, t.x1.TEN_DICH_VU, GetNgayThang(t.x1.NGAY_YL), GetNgayThang(t.x1.NGAY_TH_YL), GetNgayThang(t.x1.NGAY_KQ), t.x1.MA_MAY,
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL), GetNgayThang(t.x2.NGAY_TH_YL), GetNgayThang(t.x2.NGAY_KQ), t.x2.MA_MAY
                                    );
                            }
                            SetAutoFeatures(xlSheetG8MM, cols, 3, 4, 11, 18, 19, 26);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng Mã Máy Nhóm 8 (Phẫu thuật) và nhóm 18 (Thủ thuật): {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        #region Sheet 15: Báo cáo trùng thời gian y lệnh thuốc & dịch vụ
                        //linhist added 131026
                        try
                        {
                            var dataYLThuoc = allXML2.Where(x => !string.IsNullOrEmpty(x.MA_THUOC) && Uti.IsValidDate(x.NGAY_YL) && x.MA_NHOM != 7)
                                                            .Distinct().ToList();
                            var dataYLDichvu = allXMl3.Where(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && !string.IsNullOrEmpty(x.MA_BAC_SI) && Uti.IsValidDate(x.NGAY_YL) && !Uti.NumExists(x.MA_NHOM, 10, 13, 14, 15))
                                                    .Distinct().ToList();

                            var dataYLTrung = dataYLThuoc.Join(dataYLDichvu, x1 => x1.MA_BAC_SI?.Trim(), x2 => x2.MA_BAC_SI?.Trim(), (x1, x2) => new { x1, x2 })
                                                    .Where(a => a.x1.MA_LK != a.x2.MA_LK && a.x1.NGAY_YL == a.x2.NGAY_YL)
                                                    .Distinct().ToList();

                            ISheet xlSheetYLT = workbook.CreateSheet("Trùng Y lệnh XML2 & XML3");
                            int rows = 2;
                            int cols = CreateHeader(xlSheetYLT, cellStyles,
                                       "Danh sách Trùng thời gian Y Lệnh Thuốc & Dịch vụ", // Sheet title
                                       "STT", "Mã lượt khám 1", "Mã Bác sĩ YL 1", "Tên Bác sĩ YL 1", "Chức danh 1",
                                       "Mã Bệnh nhân 1", "Tên Bệnh nhân 1",
                                       "Mã Thẻ 1", "Ngày sinh 1", "Giới tính 1",
                                       "Mã Khoa 1", "Tên Khoa 1", "Mã Nhóm 1",
                                       "Mã Thuốc 1", "Tên Thuốc 1", "Ngày Y lệnh 1",
                                       "Mã Lượt khám 2", "Mã Bác sĩ YL 2", "Tên Bác sĩ YL 2", "Chức danh 2",
                                       "Mã Bệnh nhân 2", "Tên Bệnh nhân 2",
                                       "Mã Thẻ 2", "Ngày sinh 2", "Giới tính 2",
                                       "Mã Khoa 2", "Tên Khoa 2", "Mã Nhóm 2",
                                       "Mã Dịch vụ 2", "Tên Dịch vụ 2", "Ngày Y lệnh 2");

                            foreach (var t in dataYLTrung)
                            {
                                string maBS1 = t.x1.MA_BAC_SI;

                                string[] ttBS1 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS1);
                                string maBS2 = t.x2.MA_BAC_SI;
                                string[] ttBS2 = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS2);

                                AppendRow(xlSheetYLT, cellStyles, rows++,
                                        rows - 2, t.x1.MA_LK, maBS1, ttBS1[0], ttBS1[1],
                                        t.x1.XML01.MA_BN, t.x1.XML01.HO_TEN,
                                        t.x1.XML01.MA_THE_BHYT, GetNgayThang(t.x1.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x1.XML01.GIOI_TINH),
                                        t.x1.MA_KHOA, string.Format(fmKhoa, t.x1.MA_KHOA), t.x1.MA_NHOM,
                                        t.x1.MA_THUOC, t.x1.TEN_THUOC, GetNgayThang(t.x1.NGAY_YL),
                                        t.x2.MA_LK, maBS2, ttBS2[0], ttBS2[1],
                                        t.x2.XML01.MA_BN, t.x2.XML01.HO_TEN,
                                        t.x2.XML01.MA_THE_BHYT, GetNgayThang(t.x2.XML01.NGAY_SINH, true), GetType_GioiTinh(t.x2.XML01.GIOI_TINH),
                                        t.x2.MA_KHOA, string.Format(fmKhoa, t.x2.MA_KHOA), t.x2.MA_NHOM,
                                        t.x2.MA_DICH_VU, t.x2.TEN_DICH_VU, GetNgayThang(t.x2.NGAY_YL)
                                    );
                            }

                            SetAutoFeatures(xlSheetYLT, cols, 3, 4, 11, 17, 18, 25);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Cơ sở:{maCSKCB} lấy báo cáo Trùng DV, thời điểm lấy: {DateTime.Now:dd/MM/yyyy HH:mm}, lỗi Sheet Trùng thời gian y lệnh thuốc: {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        }
                        #endregion

                        CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);

                        workbook.Write(fs);
                        res.Data = fs.ToArray();
                    }
                    catch (Exception ex)
                    {
                        res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                        Uti.HandleCronJobLog(_logger, $"Đơn vị: {maCSKCB} lấy báo cáo -LỖI - {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                    }
                }
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
                Uti.HandleCronJobLog(_logger, $"Đơn vị: {maCSKCB} lấy báo cáo -LỖI - {ex.ToString()}", $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungDichVu)}");
            }

            return res;
        }

        public async Task<ResponseModel<byte[]>> BaoCao_TrungThuocDV(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            try
            {

                var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
                filterModel ??= new FilterDownloadModel()
                {
                    FromDateCreate = DateTime.Now.Date,
                    ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
                };

                filterModel.MaCSKCB = maCSKCB;
                var queryable = BuildQuery_XML01(filterModel);
                var data1 = await _hosoRepository.GetListAsync(queryable.FilterQueryable, x => x.OrderByDescending(x1 => x1.XML01ID));
                await AttachInnerXML(data1, "2;3;4;5");

                var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
                var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

                IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

                string fmBacsi = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:B{dmBacsis.Count() + 5},2,FALSE),\"\")";
                string fmChucvu = $"IFERROR(VLOOKUP(\"{{0}}\",'DM Bác sĩ'!A2:C{dmBacsis.Count() + 5},3,FALSE),\"\")";
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";

                var data2 = data1.SelectMany(x => x.XML02s);
                var data3 = data1.SelectMany(x => x.XML03s);
                var data4 = data1.SelectMany(x => x.XML04s);
                var data5 = data1.SelectMany(x => x.XML05s);

                using (var fs = new MemoryStream())
                {
                    #region Sheet 1: Thuoc X02
                    ISheet xlSheet2 = workbook.CreateSheet("DS Thuốc");
                    int rows = 2;
                    int cols = CreateHeader(xlSheet2, cellStyles,
                                "Danh sách tổng hợp Thuốc XML02", // Sheet title
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính","Ngày vào nội trú",
                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                "Mã Thuốc", "Mã Nhóm", "Tên Thuốc", "ĐVT", "Liều dùng",
                                "Số Đăng ký", "Cách dùng", "Mã dịch vụ", "TT Thầu", "Đơn giá", "Số lượng",
                                "Tỷ lệ TT", "Tiền BHTT", "Tiền BNCCT", "Thành tiền",
                                "Ngày Y lệnh", "Ngày TH Y lệnh", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    foreach (var x2 in data2)
                    {
                        string maBS = x2.MA_BAC_SI;
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheet2, cellStyles, rows++,
                                rows - 2, x2.XML01.MA_LK, x2.XML01.MA_BN, x2.XML01.HO_TEN,
                                x2.XML01.MA_THE_BHYT, GetNgayThang(x2.XML01.NGAY_SINH, true), GetType_GioiTinh(x2.XML01.GIOI_TINH),GetNgayThang(x2.XML01.NGAY_VAO_NOI_TRU),
                                x2.MA_KHOA, string.Format(fmKhoa, x2.MA_KHOA), maBS, ttBS[0], ttBS[1],
                                x2.XML01.MA_BENH_CHINH, x2.XML01.MA_BENH_KT, x2.XML01.CHAN_DOAN_RV,
                                x2.MA_THUOC, x2.MA_NHOM, x2.TEN_THUOC, x2.DON_VI_TINH, x2.LIEU_DUNG,
                                x2.SO_DANG_KY, x2.CACH_DUNG,x2.MA_DICH_VU, x2.TT_THAU, x2.DON_GIA, x2.SO_LUONG,
                                x2.TYLE_TT_BH, x2.T_BHTT, x2.T_BNCCT, x2.THANH_TIEN_BV,
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
                                "Mã Thẻ", "Ngày sinh", "Giới tính", "Ngày vào nội trú",
                                "Mã Khoa", "Tên Khoa", "Mã Bác Y lệnh", "Tên Bác sĩ", "Chức danh",
                                "Mã Bác Sĩ Thực hiện", "Tên Bác sĩ thực hiện", "Chức danh",
                                "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                "Mã Dịch vụ", "Mã nhóm", "Tên Dịch vụ", "Mã giường", "Mã máy",
                                "ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Tiền BHTT", "Tiền BNCCT", "Thành tiền",
                                "Ngày Y lệnh", "Ngày thực hiện Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    foreach (var x3 in data3DVs)
                    {
                        string maBS = x3.MA_BAC_SI;
                        string mabsTH = x3.NGUOI_THUC_HIEN;
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);
                        string[] ttbsTH = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref mabsTH);

                        AppendRow(xlSheetDV, cellStyles, rows++,
                                rows - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                                x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetType_GioiTinh(x3.XML01.GIOI_TINH),GetNgayThang(x3.XML01.NGAY_VAO_NOI_TRU),
                                x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1], mabsTH, ttbsTH[0], ttbsTH[1],
                                x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                x3.MA_DICH_VU, x3.MA_NHOM, x3.TEN_DICH_VU, x3.MA_GIUONG, x3.MA_MAY,
                                x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.T_BHTT, x3.T_BNCCT, x3.THANH_TIEN_BV,
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
                                "Mã Thẻ", "Ngày sinh", "Giới tính","Ngày vào nội trú",
                                "Mã Khoa", "Tên Khoa", "Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                "Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                "Mã Vật tư", "Tên Vật tư", "ĐVT", "TT Thầu", "Đơn giá",
                                "Tỷ lệ TT", "Tiền BHTT", "Tiền BNCCT", "Thành tiền",
                                "Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");

                    foreach (var x3 in data3VTs)
                    {
                        string maBS = x3.MA_BAC_SI;
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheetVT, cellStyles, rows++,
                                rows - 2, x3.XML01.MA_LK, x3.XML01.MA_BN, x3.XML01.HO_TEN,
                                x3.XML01.MA_THE_BHYT, GetNgayThang(x3.XML01.NGAY_SINH, true), GetType_GioiTinh(x3.XML01.GIOI_TINH),GetNgayThang(x3.XML01.NGAY_VAO_NOI_TRU),
                                x3.MA_KHOA, string.Format(fmKhoa, x3.MA_KHOA), maBS, ttBS[0], ttBS[1],
                                x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                x3.MA_VAT_TU, x3.TEN_VAT_TU, x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV,
                                x3.TYLE_TT_BH, x3.T_BHTT, x3.T_BNCCT, x3.THANH_TIEN_BV,
                                GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN));
                    }

                    SetAutoFeatures(xlSheetVT, cols, 8, 10, 11, -14);
                    #endregion

                    #region Sheet 4: XML4
                    ISheet xlSheetXML4 = workbook.CreateSheet("DS XML4");
                    rows = 2;
                    cols = CreateHeader(xlSheetXML4, cellStyles, "Danh sách tổng hợp XML4",
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính", "Ngày vào nội trú",
                                "Mã dịch vụ", "Mã chỉ số", "Tên chỉ số", "Giá Trị",
                                "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả",
                                "Mã BS Đọc kết quả", "Tên BS Đọc KQ", "Chức danh");

                    foreach (var x4 in data4)
                    {
                        string maBS = x4.MA_BS_DOC_KQ;
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheetXML4, cellStyles, rows++,
                                rows - 2, x4.XML01.MA_LK, x4.XML01.MA_BN, x4.XML01.HO_TEN,
                                x4.XML01.MA_THE_BHYT, GetNgayThang(x4.XML01.NGAY_SINH, true), GetType_GioiTinh(x4.XML01.GIOI_TINH),GetNgayThang(x4.XML01.NGAY_VAO_NOI_TRU),
                                x4.MA_DICH_VU, x4.MA_CHI_SO, x4.TEN_CHI_SO, x4.GIA_TRI,
                                x4.DON_VI_DO, x4.MO_TA, x4.KET_LUAN, GetNgayThang(x4.NGAY_KQ),
                                maBS, ttBS[0], ttBS[1]
                            );
                    }

                    SetAutoFeatures(xlSheetXML4, cols, 8, 10, 11, -14);
                    #endregion

                    #region Sheet 5: XML5
                    ISheet xlSheetXML5 = workbook.CreateSheet("DS XML5");
                    rows = 2;
                    cols = CreateHeader(xlSheetXML5, cellStyles, "Danh sách tổng hợp XML5",
                                "STT", "Mã Lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân",
                                "Mã Thẻ", "Ngày sinh", "Giới tính","Ngày vào nội trú",
                                //"Mã Khoa", "Tên Khoa" //"Mã Bác sĩ", "Tên Bác sĩ", "Chức danh",
                                //"Chẩn đoán chính", "Chẩn đoán kèm theo", "Chẩn đoán ra viện",
                                //"Mã Vật tư", "Tên Vật tư",
                                //"ĐVT", "TT Thầu", "Đơn giá", "Tỷ lệ TT", "Thành tiền",
                                //"Ngày Y lệnh", "Ngày Kết quả", "Ngày vào", "Ngày ra", "Ngày Thanh toán");
                                //"Mã dịch vụ", "Mã chỉ số", "Tên chỉ số", "Giá Trị", "Đơn vị đo", "Mô tả", "Kết luận", "Ngày kết quả", "Mã BS Đọc kết quả", "Tên BS Đọc KQ", "Chức danh"
                                "Diễn biến LS", "Giai đoạn bệnh", "Hội chẩn", "Phẫu thuật", "Thời điểm diễn biến lâm sàng", "Mã người thực hiện", "Tên người thực hiện", "Chức danh"
                                );

                    foreach (var x5 in data5)
                    {
                        string maBS = x5.NGUOI_THUC_HIEN;
                        string[] ttBS = GetFormular_BacSi(new string[] { fmBacsi, fmChucvu }, ref maBS);

                        AppendRow(xlSheetXML5, cellStyles, rows++,
                                rows - 2, x5.XML01.MA_LK, x5.XML01.MA_BN, x5.XML01.HO_TEN,
                                x5.XML01.MA_THE_BHYT, GetNgayThang(x5.XML01.NGAY_SINH, true), GetType_GioiTinh(x5.XML01.GIOI_TINH), GetNgayThang(x5.XML01.NGAY_VAO_NOI_TRU),
                                //x4.MA_KHOA, string.Format(fmKhoa, x4.MA_KHOA), //maBS, ttBS[0], ttBS[1],
                                //x3.XML01.MA_BENH_CHINH, x3.XML01.MA_BENH_KT, x3.XML01.CHAN_DOAN_RV,
                                //x3.MA_VAT_TU, x3.TEN_VAT_TU,
                                //x3.DON_VI_TINH, x3.TT_THAU, x3.DON_GIA_BV, x3.TYLE_TT_BH, x3.THANH_TIEN_BV,
                                //GetNgayThang(x3.NGAY_YL), GetNgayThang(x3.NGAY_KQ), GetNgayThang(x3.XML01.NGAY_VAO), GetNgayThang(x3.XML01.NGAY_RA), GetNgayThang(x3.XML01.NGAY_TTOAN)
                                //x5.MA_DICH_VU, x4.MA_CHI_SO, x4.TEN_CHI_SO, x4.GIA_TRI, x4.DON_VI_DO, x4.MO_TA, x4.KET_LUAN, GetNgayThang(x4.NGAY_KQ), maBS, ttBS[0], ttBS[1]
                                x5.DIEN_BIEN_LS, x5.GIAI_DOAN_BENH, x5.HOI_CHAN, x5.PHAU_THUAT, GetNgayThang(x5.THOI_DIEM_DBLS), maBS, ttBS[0], ttBS[1]
                                );

                    }

                    SetAutoFeatures(xlSheetXML5, cols, 8, 10, 11, -14);
                    #endregion

                    #region Sheet 6: DS Tien BHXH
                    ISheet xlSheet6 = workbook.CreateSheet("Danh sách tiền BHXH");
                    rows = 2;
                    cols = CreateHeader(xlSheet6, cellStyles,
                            "Danh sách tiền BHXH", // Sheet title
                            "STT", "Mã lượt khám", "Mã Bệnh nhân", "Tên Bệnh nhân", "Tiền Bệnh nhân", "Tiền BHTT"
                        );

                    foreach (var x in data1)
                    {
                        AppendRow(xlSheet6, cellStyles, rows++,
                                x.STT, x.MA_LK, x.MA_BN, x.HO_TEN, Uti.GetMoneyVN(x.T_BNCCT), Uti.GetMoneyVN(x.T_BHTT)
                        );
                    }

                    SetAutoFeatures(xlSheet6, cols);
                    #endregion

                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);
                    workbook.Write(fs);
                    res.Data = fs.ToArray();
                }
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungThuocDV)}");
            }

            return res;
        }

        public async Task<ResponseModel<byte[]>> BaoCao_ChiDinhDV(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            try
            {
                var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
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

                //Create new excel
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
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_ChiDinhDV)}");
            }

            return res;
        }

        public async Task<ResponseModel<byte[]>> BaoCao_ThongkeLoi(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            try
            {
                var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
                filterModel ??= new FilterDownloadModel()
                {
                    FromDateCreate = DateTime.Now.Date,
                    ToDateCreate = DateTime.Now.Date.AddDays(1).AddTicks(-1),
                };

                filterModel.MaCSKCB = maCSKCB;

                //var queryable = BuildQuery_XML01(filterModel);
                var queryable = FilesService130.BuildQueryToGetListData(filterModel, hasDeleted: true);
                var data = await _hosoRepository.GetListAsync(queryable.FilterQueryable);
                var data1 = data.Where(x => x.IsDelete != true).ToList();

                var errors = await AttachXMLError(maCSKCB, data1);
                var errCodes = errors.Select(e => e.MA_LOI).Distinct().ToList();
                var dmErrors = await GetDMErrors(maCSKCB, errCodes);
                var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);
                var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);

                //Create new excel
                IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

                // lấy phần tử cuối cùng nếu không có ; thì giữ nguyên
                var allKhoas = data1.Select(x => x.MA_KHOA.Split(';').Last()).Distinct().OrderBy(x => x).ToList();
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

                    var xmlKhoa = data1.GroupBy(x => new { x.XML01ID, MA_KHOA = x.MA_KHOA.Split(";").Last() }, (k, g) => new { k.XML01ID, k.MA_KHOA }).Distinct().ToList();
                    var errCnts = errors.GroupBy(e => new { e.XML01ID, e.MA_LOI }, (k, g) => new { k.XML01ID, k.MA_LOI, Cnt = g.Count() })
                                        .Join(xmlKhoa, e => e.XML01ID, x => x.XML01ID, (e, x) => new { x.XML01ID, x.MA_KHOA, e.MA_LOI, e.Cnt })
                                        .GroupBy(x => new { x.MA_KHOA, x.MA_LOI }, (k, g) => new { k.MA_KHOA, k.MA_LOI, SO_LOI = g.Sum(e => e.Cnt) });

                    foreach (var e in dmErrors)
                    {
                        var lst = errCnts.Where(x => x.MA_LOI == e.error_code);
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

                    //Linhist added 20/02/25
                    #region SheetBuilder 2: DS Ho so day len & loi
                    string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";
                    var x01Ids = data1.Select(x => x.XML01ID).ToList();
                    var allLKs = data1.Select(x => x.MA_LK).Distinct().ToList();

                    var minDte = data1.Min(x => x.CreateDate).Value.Date;
                    var maxDte = data1.Max(x => x.CreateDate).Value.Date.AddDays(1);
                    //var files = await _chgRepository.GetListAsync(c => c.XML01ID != null && x01Ids.Contains(c.XML01ID.Value));
                    //var fGroups = files.GroupBy(c => new { c.XML01ID, c.NGAY_TAO })
                    //                    .Select(g => new { Xml01 = g.FirstOrDefault(), ErrCnt = g.Count(), g.Key.NGAY_TAO })
                    //                    .ToList();

                    //var files = await _chgRepository.DbSet.AsNoTracking()
                    //                    .Where(c => c.XML01ID != null && x01Ids.Contains(c.XML01ID.Value))
                    //                    .OrderBy(c => c.XML01ID) 
                    //                    .GroupBy(c => new { c.XML01ID, c.NGAY_TAO }, (g, f) => new { g.XML01ID, g.NGAY_TAO, ErrCnt = f.Count() })

                    //                    .ToListAsync();

                    #region Linhist changed 08/03/25
                    //var files = await _chgRepository.DbSet.AsNoTracking()
                    //                    .Where(c => c.XML01ID != null && x01Ids.Contains(c.XML01ID.Value))
                    //                    .OrderBy(c => c.MA_LK)
                    //                    .ThenBy(c => c.NGAY_TAO)
                    //                    .Select(c => new
                    //                    {
                    //                        c.ChgID,
                    //                        c.XML01ID,
                    //                        c.MA_CSKCB,
                    //                        c.MA_LK,
                    //                        c.MA_BN,
                    //                        c.MA_THE_BHYT,
                    //                        c.HO_TEN,
                    //                        c.MA_LOI,
                    //                        c.NGAY_TAO
                    //                    })
                    //                    .ToListAsync();
                    //var groupedFiles = files.GroupBy(f => f.XML01ID).ToList();


                    //if (files?.Any() == true)
                    //{
                    //    var result = new List<GroupedDataWithSTT>();

                    //    int stt = 0;
                    //    string MALK = "";

                    //    foreach (var group in groupedFiles)
                    //    {
                    //        var groupedData = new GroupedDataWithSTT
                    //        {
                    //            XML01IDs = (int)group.Key,
                    //            NGAY_TAO = group.First().NGAY_TAO.Value,
                    //            TotalErrors = group.Count(f => f.MA_LOI.HasValue),
                    //            Files = new List<GroupedDataWithSTT.FileData>()
                    //        };
                    //       //if(group.FirstOrDefault().MA_LK== "3657928")
                    //       // {
                    //       //     var i = 1;
                    //       // }    
                    //        if (group.FirstOrDefault().MA_LK != MALK)
                    //        {
                    //                MALK = group.FirstOrDefault().MA_LK;
                    //                stt = 1;
                    //        }
                    //        else
                    //        {
                    //                stt++;
                    //        }
                    //        groupedData.Files.Add(new GroupedDataWithSTT.FileData
                    //        {
                    //            ChgID = (int)group.FirstOrDefault().ChgID,
                    //            MA_CSKCB = group.FirstOrDefault().MA_CSKCB,
                    //            MA_LK = group.FirstOrDefault().MA_LK,
                    //            MA_BN = group.FirstOrDefault().MA_BN,
                    //            MA_THE_BHYT =  group.FirstOrDefault().MA_THE_BHYT,
                    //            HO_TEN = group.FirstOrDefault().HO_TEN,
                    //            MA_LOI = (int)group.FirstOrDefault().MA_LOI,
                    //            MA_KHOA = data1.FirstOrDefault(x => x.XML01ID == group.Key).MA_KHOA,
                    //        });
                    //        groupedData.STT = stt;
                    //        result.Add(groupedData);
                    //    }


                    //    ISheet xlSheetX = workbook.CreateSheet("Thống kê hồ sơ");

                    //    rows = 2;
                    //    cols = CreateHeader(xlSheetX, cellStyles,
                    //                $"Báo Cáo Thống kê Hồ sơ tải lên - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                    //                "STT", "Ngày kiểm tra","Số lần đẩy hồ sơ","Mã liên kết", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ","Mã Khoa", "Tổng số lỗi");


                    //    if(groupedFiles?.Any()==true)
                    //    {
                    //        int STT = 0;
                    //        foreach (var g in result)
                    //        {

                    //            AppendRow(xlSheetX, cellStyles, rows++, STT++, g.NGAY_TAO, g.STT, g.Files.FirstOrDefault().MA_LK, g.Files.FirstOrDefault().MA_BN, g.Files.FirstOrDefault().HO_TEN, g.Files.FirstOrDefault().MA_THE_BHYT, g.Files.FirstOrDefault().MA_KHOA, g.TotalErrors);
                    //        }
                    //    }
                    //    //if (gfile?.Any() == true)
                    //    //{
                    //    //    ////Thêm STT
                    //    //    if (MALK != x01.MA_LK)
                    //    //    {
                    //    //        MALK=x01.MA_LK;
                    //    //        STT = 1;
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        STT++;
                    //    //    }

                    //    //    foreach (var g in gfile)
                    //    //    {


                    //    //        AppendRow(xlSheetX, cellStyles, rows++, x01.STT, g.NGAY_TAO, STT, x01.MA_LK, x01.MA_BN, x01.HO_TEN, x01.MA_THE_BHYT,x01.MA_KHOA, g.ErrCnt);
                    //    //    }
                    //    //}
                    //    //else
                    //    //{
                    //    //    AppendRow(xlSheetX, cellStyles, rows++, x01.STT, x01.CreateDate, 0, x01.MA_LK, x01.MA_BN, x01.HO_TEN, x01.MA_THE_BHYT,x01.MA_KHOA, 0);
                    //    //}
                    #endregion
                    //var allChecks = await _chgRepository.GetListAsync(c => c.XML01ID != null && x01Ids.Contains(c.XML01ID.Value));
                    //var files = allChecks.GroupBy(c => new { c.MA_LK }, (g, f) => new { Data = f, ChkCnt = f.Select(c => new { c.XML01ID, c.NGAY_TAO }).Distinct().Count() })
                    //                    .SelectMany(c => c.Data.Select(f => new { f.XML01ID, f.MA_LK, f.MA_BN, f.MA_THE_BHYT, f.HO_TEN, f.NGAY_TAO, c.ChkCnt }))
                    //                    .GroupBy(c => new { c.XML01ID, c.NGAY_TAO }, (g, f) => new { Info = f.FirstOrDefault(), ErrCnt = f.Count() })
                    //                    .Select(c => new { c.Info.XML01ID, c.Info.MA_LK, c.Info.MA_BN, c.Info.MA_THE_BHYT, c.Info.HO_TEN, c.Info.NGAY_TAO, c.Info.ChkCnt, c.ErrCnt })
                    //                    .OrderBy(c => c.MA_LK)
                    //                    .ThenByDescending(c => c.NGAY_TAO)
                    //                    .ToList();
                    //var allChecks = await _chgRepository.GetListAsync(c => c.MA_CSKCB == maCSKCB && c.XML01ID != null && x01Ids.Contains(c.XML01ID.Value));
                    var allStates = await _stateRepository.GetListAsync(x => allLKs.Contains(x.MA_LK) && x.MA_CSKCB == maCSKCB && x.CheckDate >= minDte && x.CheckDate < maxDte);
                    var allX0s = allStates.GroupBy(x => new { x.MA_LK, x.MA_CSKCB, x.CheckDate }, (k, g) => new { k.MA_LK, k.CheckDate, ChkCnt = g.Count(), ErrCnt = g.Sum(c => c.TONG_SO_LOI) }).ToList();

                    if (allX0s != null && allX0s.Any())
                    {
                        //var cntChks = allChecks.GroupBy(c => c.MA_LK, (k, g) => new { MA_LK = k, ChkCnt = g.Select(c => new { c.XML01ID, c.NGAY_TAO }).Distinct().Count() }).Distinct().ToList();
                        //var cntErrs = allChecks.GroupBy(c => new { c.XML01ID, c.NGAY_TAO }, (k, g) => new { Data = g.FirstOrDefault(), ErrCnt = g.Count() }).Distinct().ToList();

                        //Linhist changed 11/06/26
                        //var allFiles = cntErrs.Join(cntChks, e => e.Data.MA_LK, c => c.MA_LK, (e, c) => new { e.Data, c.ChkCnt, e.ErrCnt })
                        //                        .Join(data1, e => e.Data.XML01ID, x => x.XML01ID, (e, x) => new { e.Data.XML01ID, e.Data.MA_LK, e.Data.MA_BN, e.Data.MA_THE_BHYT, e.Data.HO_TEN, x.MA_KHOA, e.Data.NGAY_TAO, e.ChkCnt, e.ErrCnt })
                        //                        .Distinct().OrderBy(c => c.MA_LK).ThenByDescending(c => c.NGAY_TAO).ToList();
                        //var allFiles = cntErrs.Join(cntChks, e => e.Data.MA_LK, c => c.MA_LK, (e, c) => new { e.Data.XML01ID, e.Data.MA_LK, e.Data.MA_BN, e.Data.MA_THE_BHYT, e.Data.HO_TEN, e.Data.MA_KHOA, e.Data.NGAY_TAO, c.ChkCnt, e.ErrCnt })
                        //                        .Distinct().OrderBy(c => c.MA_LK).ThenByDescending(c => c.NGAY_TAO).ToList();

                        ISheet xlSheetX = workbook.CreateSheet("Thống kê hồ sơ");

                        rows = 2;
                        cols = CreateHeader(xlSheetX, cellStyles,
                                    $"Báo Cáo Thống kê Hồ sơ tải lên - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                                    "STT", "Ngày kiểm tra", "Số lần đẩy hồ sơ", "Mã liên kết", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ", "Mã Khoa", "Tên Khoa", "Tổng số lỗi");

                        //var stt = 0;
                        //var maLK = "";
                        //foreach (var f in allFiles)
                        //{
                        //    if (f.MA_LK != maLK)
                        //    {
                        //        maLK = f.MA_LK;
                        //        stt = f.ChkCnt;
                        //    }
                        //    else
                        //    {
                        //        stt--;
                        //    }
                        var data1Join = data1.GroupBy(x => new { x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA }, (k, g) => k).Distinct().ToList();
                        var d01joiner = data1Join.Join(allX0s, d => d.MA_LK, x => x.MA_LK, (d, x) => new { d.MA_LK, d.MA_BN, d.HO_TEN, d.MA_THE_BHYT, d.MA_KHOA, x.CheckDate, x.ChkCnt, x.ErrCnt })
                                                    .Distinct().OrderBy(c => c.MA_LK).GroupBy(x => x.MA_LK, (k, g) => g.OrderByDescending(c => c.CheckDate)).ToList();
                        foreach (var grp in d01joiner)
                        {
                            //Linhist changed 08/03/25
                            //var x01 = data1.FirstOrDefault(x => x.XML01ID == f.XML01ID);
                            //AppendRow(xlSheetX, cellStyles, rows++, rows - 2, f.NGAY_TAO, $"Lần {stt}", f.MA_LK, f.MA_BN, f.HO_TEN, f.MA_THE_BHYT, string.Format(fmKhoa, x01?.MA_KHOA?.Contains(";") == true
                            //            ? x01.MA_KHOA.Split(';').Last()
                            //            : x01?.MA_KHOA
                            //              ), f.ErrCnt);
                            //AppendRow(xlSheetX, cellStyles, rows++, rows - 2, f.NGAY_TAO, $"Lần {stt}", f.MA_LK, f.MA_BN, f.HO_TEN, f.MA_THE_BHYT, f.MA_KHOA, GetFormular_TenKhoa(fmKhoa, f.MA_KHOA, true), f.ErrCnt);
                            var lan = grp.Count();
                            foreach (var x in grp)
                            {
                                AppendRow(xlSheetX, cellStyles, rows++, rows - 2, x.CheckDate, $"Lần {lan--}", x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.MA_KHOA, true), x.ErrCnt);
                            }
                        }

                        SetAutoFeatures(xlSheetX, cols, 5, 7);
                    }
                    #region Linhist removed 061026 - ko dùng check changes nữa
                    //}
                    //else
                    //{
                    //    var cntChgs = data.GroupBy(x => x.MA_LK, (k, g) => new { MA_LK = k, ChkCnt = g.Select(x => new { x.XML01ID, x.CreateDate }).Distinct().Count() }).Distinct().ToList();
                    //    var cntErrs = data1.GroupBy(x => new { x.XML01ID, x.CreateDate }, (k, g) => new { Data = g.FirstOrDefault(), ErrCnt = errors.Count(e => e.XML01ID == k.XML01ID) }).Distinct().ToList();

                    //    var allFiles = cntErrs.Join(cntChgs, e => e.Data.MA_LK, c => c.MA_LK, (e, c) => new { e.Data.XML01ID, e.Data.MA_LK, e.Data.MA_BN, e.Data.MA_THE_BHYT, e.Data.HO_TEN, e.Data.MA_KHOA, e.Data.CreateDate, c.ChkCnt, e.ErrCnt })
                    //                            .Distinct().OrderBy(c => c.MA_LK).ThenByDescending(c => c.CreateDate).ToList();

                    //    ISheet xlSheetY = workbook.CreateSheet("Thống kê hồ sơ");

                    //    rows = 2;
                    //    cols = CreateHeader(xlSheetY, cellStyles,
                    //                $"Báo Cáo Thống kê Hồ sơ tải lên - Ngày {DateTime.Now:dd/MM/yyyy}", // Sheet title
                    //                "STT", "Ngày kiểm tra", "Số lần đẩy hồ sơ", "Mã liên kết", "Mã Bệnh nhân", "Tên Bệnh nhân", "Mã thẻ", "Mã Khoa", "Tên Khoa", "Tổng số lỗi");

                    //    var stt = 0;
                    //    var maLK = "";
                    //    foreach (var f in allFiles)
                    //    {
                    //        if (f.MA_LK != maLK)
                    //        {
                    //            maLK = f.MA_LK;
                    //            stt = f.ChkCnt;
                    //        }
                    //        else
                    //        {
                    //            stt--;
                    //        }
                    //        AppendRow(xlSheetY, cellStyles, rows++, rows - 2, f.CreateDate, $"Lần {stt}", f.MA_LK, f.MA_BN, f.HO_TEN, f.MA_THE_BHYT, f.MA_KHOA, GetFormular_TenKhoa(fmKhoa, f.MA_KHOA, true), f.ErrCnt);
                    //    }

                    //    SetAutoFeatures(xlSheetY, cols, 5, 7);
                    //}
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
                            var arr = new object[] { rows - 2, x.MA_LK, x.MA_BN, x.HO_TEN, x.MA_THE_BHYT, x.MA_KHOA, GetFormular_TenKhoa(fmKhoa, x.MA_KHOA, true), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "" };
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
                    var khoaGroups = data1.GroupBy(x => new { MA_KHOA = x.MA_KHOA.Split(';').Last() }, (k, g) => new { k.MA_KHOA, Cnt = g.Count() }).OrderBy(x => x.MA_KHOA).Distinct().ToList();
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
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_ThongkeLoi)}");
            }

            return res;
        }

        public async Task<ResponseModel<byte[]>> BaoCao_TrungMaMay(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            try
            {
                var filterModel = JsonConvert.DeserializeObject<FilterDownloadModel>(filterString);
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

                //Create new excel
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

                    CreateLookupSheet(workbook, cellStyles, dmBacsis, dmKhoas);
                    workbook.Write(fs);
                    res.Data = fs.ToArray();
                }
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_TrungMaMay)}");
            }

            return res;
        }

        private static bool HasMaBS(string cchn, string allMaBS)
        {
            if (string.IsNullOrEmpty(cchn) || string.IsNullOrEmpty(allMaBS)) return false;
            return allMaBS == cchn || $";{allMaBS.Trim()};".Contains($";{cchn.Trim()};");
        }

        public async Task<ResponseModel<byte[]>> BaoCao_ChamCong(string filterString)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            var hospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;

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

                //Linhist added 24/01/26
                var hospital = await _hospitalRepository.GetById(hospitalId);
                var ofct = new TimePeriod(hospital.OfficeTime);


                //Linhist changed 21/02/25
                //var data2 = await _xml02Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null && x.NGAY_YL >= dStart && x.NGAY_YL < dEnd, o => o.OrderBy(x => x.NGAY_YL), d => d.Include(x => x.XML01));
                //var data3 = await _xml03Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null && x.NGAY_YL >= dStart && x.NGAY_KQ != null && x.NGAY_KQ < dEnd, o => o.OrderBy(x => x.NGAY_YL), d => d.Include(x => x.XML01));
                //var data4 = await _xml04Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BS_DOC_KQ) && x.NGAY_KQ != null && x.NGAY_KQ >= dStart && x.NGAY_KQ <= dEnd, o => o.OrderBy(x => x.NGAY_KQ), d => d.Include(x => x.XML01));
                //var data5 = await _xml05Repository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) && x.THOI_DIEM_DBLS != null && x.THOI_DIEM_DBLS >= dStart && x.THOI_DIEM_DBLS <= dEnd, o => o.OrderBy(x => x.THOI_DIEM_DBLS), d => d.Include(x => x.XML01));

                var x01set = _hosoRepository.DbSet.Where(x => x.MA_CSKCB == maCSKCB && x.IsDelete != true);

                var all2 = await x01set.SelectMany(x => x.XML02s)
                                    .Where(x2 => x2.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x2.MA_BAC_SI) && x2.NGAY_YL != null && x2.NGAY_YL >= dStart && x2.NGAY_YL < dEnd).ToListAsync();
                var data2 = all2.Select(x2 => new { XML2 = x2, Period = GetWorkPeriod(x2.NGAY_YL, ofct), x2.NGAY_YL.Value.Date})
                                    .GroupBy(x2 => new { x2.XML2.MA_BAC_SI, x2.Date, x2.Period }, (g, c) => c.FirstOrDefault())
                                    .Distinct().OrderBy(x2 => x2.XML2.NGAY_YL).ToList();
                //var data2 = await _xml02Repository.DbSet.Include(x => x.XML01)
                //                        .Where(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null && x.NGAY_YL >= dStart && x.NGAY_YL < dEnd)
                //                        .GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date, NGAY_LV = x.NGAY_YL }, (g, c) => g)
                //                        .Distinct()
                //                        .OrderBy(x => x.Date)
                //                        .ToListAsync();

                var all3a = await x01set.SelectMany(x => x.XML03s)
                                        .Where(x3 => x3.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x3.MA_BAC_SI) && x3.NGAY_YL != null && x3.NGAY_YL >= dStart && x3.NGAY_KQ != null && x3.NGAY_KQ < dEnd).ToListAsync();
                var data3a = all3a.Select(x3 => new { XML3 = x3, Period = GetWorkPeriod(x3.NGAY_YL, ofct), x3.NGAY_YL.Value.Date })
                                        .GroupBy(x3 => new { x3.XML3.MA_BAC_SI, x3.Date, x3.Period }, (g, c) => c.FirstOrDefault())
                                        .Distinct().OrderBy(x3 => x3.XML3.NGAY_YL).ToList();

                //var data3a = await _xml03Repository.DbSet.Include(x => x.XML01)
                //                        .Where(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_YL != null && x.NGAY_YL >= dStart && x.NGAY_KQ != null && x.NGAY_KQ < dEnd)
                //                        .GroupBy(x => new { x.MA_BAC_SI, x.NGAY_YL.Value.Date, NGAY_LV = x.NGAY_YL }, (g, c) => g)
                //                        .Distinct()
                //                        .OrderBy(x => x.Date)
                //                        .ToListAsync();

                var all3b = await x01set.SelectMany(x => x.XML03s)
                                        .Where(x3 => x3.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x3.NGUOI_THUC_HIEN) && x3.NGAY_TH_YL != null && x3.NGAY_TH_YL >= dStart && x3.NGAY_KQ != null && x3.NGAY_KQ < dEnd && (x3.MA_NHOM != 10 || x3.MA_NHOM != 11)).ToListAsync();
                var data3b = all3b.Select(x3 => new { XML3 = x3, Period = GetWorkPeriod(x3.NGAY_TH_YL, ofct), x3.NGAY_TH_YL.Value.Date })
                                        .GroupBy(x3 => new { x3.XML3.NGUOI_THUC_HIEN, x3.Date, x3.Period }, (g, c) => c.FirstOrDefault())
                                        .Distinct().OrderBy(x3 => x3.XML3.NGAY_TH_YL).ToList();

                //var data3b = await _xml03Repository.DbSet.Include(x => x.XML01)
                //                        .Where(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BAC_SI) && x.NGAY_TH_YL != null && x.NGAY_TH_YL >= dStart && x.NGAY_KQ != null && x.NGAY_KQ < dEnd && (x.MA_NHOM != 10 || x.MA_NHOM != 11))
                //                        .GroupBy(x => new { x.NGUOI_THUC_HIEN, x.NGAY_TH_YL.Value.Date, NGAY_LV = x.NGAY_TH_YL }, (g, c) => g)
                //                        .Distinct()
                //                        .OrderBy(x => x.Date)
                //                        .ToListAsync();

                var all4 = await x01set.Include(x => x.XML04s).SelectMany(x => x.XML04s)
                                        .Where(x4 => x4.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x4.MA_BS_DOC_KQ) && x4.NGAY_KQ != null && x4.NGAY_KQ >= dStart && x4.NGAY_KQ <= dEnd).ToListAsync();
                var data4 = all4.Select(x4 => new { XML4 = x4, Period = GetWorkPeriod(x4.NGAY_KQ, ofct), x4.NGAY_KQ.Value.Date })
                                        .GroupBy(x4 => new { x4.XML4.MA_BS_DOC_KQ, x4.Date, x4.Period }, (g, c) => c.FirstOrDefault())
                                        .Distinct().OrderBy(x4 => x4.XML4.NGAY_KQ).ToList();

                //var data4 = await _xml04Repository.DbSet.Include(x => x.XML01)
                //                        .Where(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.MA_BS_DOC_KQ) && x.NGAY_KQ != null && x.NGAY_KQ >= dStart && x.NGAY_KQ <= dEnd)
                //                        .GroupBy(x => new { x.MA_BS_DOC_KQ, x.NGAY_KQ.Value.Date, NGAY_LV = x.NGAY_KQ }, (g, c) => g)
                //                        .Distinct()
                //                        .OrderBy(x => x.Date)
                //                        .ToListAsync();

                var all5 = await x01set.Include(x => x.XML05s).SelectMany(x => x.XML05s)
                                        .Where(x5 => x5.MA_CSKCB == maCSKCB && !string.IsNullOrEmpty(x5.NGUOI_THUC_HIEN) && x5.THOI_DIEM_DBLS != null && x5.THOI_DIEM_DBLS >= dStart && x5.THOI_DIEM_DBLS <= dEnd).ToListAsync();
                var data5 = all5.Select(x5 => new { XML5 = x5, Period = GetWorkPeriod(x5.THOI_DIEM_DBLS, ofct), x5.THOI_DIEM_DBLS.Value.Date })
                                        .GroupBy(x5 => new { x5.XML5.NGUOI_THUC_HIEN, x5.Date, x5.Period }, (g, c) => c.FirstOrDefault())
                                        .Distinct().OrderBy(x5 => x5.XML5.THOI_DIEM_DBLS).ToList();

                //var data5 = await _xml05Repository.DbSet.Include(x => x.XML01)
                //                        .Where(x => x.MA_CSKCB == maCSKCB && x.XML01.IsDelete != true && !string.IsNullOrEmpty(x.NGUOI_THUC_HIEN) && x.THOI_DIEM_DBLS != null && x.THOI_DIEM_DBLS >= dStart && x.THOI_DIEM_DBLS <= dEnd)
                //                        .GroupBy(x => new { x.NGUOI_THUC_HIEN, x.THOI_DIEM_DBLS.Value.Date, NGAY_LV = x.THOI_DIEM_DBLS }, (g, c) => g)
                //                        .Distinct()
                //                        .OrderBy(x => x.Date)
                //                        .ToListAsync();

                var dmBacsis = await _dmBacSiRepository.GetListAsync(bs => bs.MA_CSKCB == maCSKCB);

                //Create new excel
                ICellStyle[] cellStyles = null;
                IWorkbook workbook = CreateWorkBook(out cellStyles);

                var allBS = dmBacsis.OrderBy(b => b.MA_KHOA);
                var lstX3Mores = new List<XML03>();

                //Linhist added 25/12/24
                var pivots = new Dictionary<DateTime, Dictionary<string, string>>();
                foreach (var bs in allBS)
                {
                    string perd;
                    DateTime dte;
                    Dictionary<string, string> lst;

                    //Kiem tra XML02 theo MA_BAC_SI & NGAY_YL
                    foreach (var x2 in data2)
                    {
                        //Linhist changed 21/02/25
                        //dte = x2.NGAY_YL.Value.Date;
                        dte = x2.Date;

                        //Linhist changed 24/01/26
                        //if (!pivots.TryGetValue(dte, out lst))
                        //{
                        //    lst = new List<string>();
                        //    pivots.Add(dte, lst);
                        //}

                        //if (x2.MA_BAC_SI == bs.MACCHN && !lst.Contains(bs.MACCHN))
                        //{
                        //    lst.Add(bs.MACCHN);
                        //}
                        if (!pivots.TryGetValue(dte, out lst))
                        {
                            lst = new Dictionary<string, string>();
                            pivots.Add(dte, lst);
                        }

                        if (x2.XML2.MA_BAC_SI == bs.MA_CCHN)
                        {
                            perd = x2.Period;
                            if (!lst.ContainsKey(bs.MA_CCHN))
                            {
                                lst.Add(bs.MA_CCHN, perd);
                            }
                            else
                            {
                                var str = lst[bs.MA_CCHN];
                                if (!str.Contains(perd))
                                {
                                    lst[bs.MA_CCHN] = str + perd;
                                }
                            }
                        }
                    }

                    //Kiem tra XML03 theo MA_BS & NGAY_YL
                    //Kiem tra XML03 theo NGUOI_THUC_HIEN & NGAY_THYL (chi danh cho MA_NHOM khac 10, 11)
                    //Linhist changed 21/02/25
                    //foreach (var x3 in data3)
                    //{
                    //    dte = x3.NGAY_YL.Value.Date;
                    //    if (!pivots.TryGetValue(dte, out lst))
                    //    {
                    //        lst = new List<string>();
                    //        pivots.Add(dte, lst);
                    //    }

                    //    if ((x3.MA_BAC_SI == bs.MACCHN || x3.MA_BAC_SI.StartsWith(bs.MACCHN + ";")) && !lst.Contains(bs.MACCHN))
                    //    {
                    //        lst.Add(bs.MACCHN);
                    //    }

                    //    if (!Uti.NumExists(x3.MA_NHOM, 10, 11))
                    //    {
                    //        if ((x3.NGUOI_THUC_HIEN == bs.MACCHN || x3.NGUOI_THUC_HIEN.Contains(bs.MACCHN + ";")) && !lst.Contains(bs.MACCHN))
                    //        {
                    //            lst.Add(bs.MACCHN);
                    //        }
                    //    }
                    //}
                    foreach (var x3 in data3a)
                    {
                        dte = x3.Date;

                        //Linhist changed 24/01/26
                        //if (!pivots.TryGetValue(dte, out lst))
                        //{
                        //    lst = new List<string>();
                        //    pivots.Add(dte, lst);
                        //}

                        //if ((x3.MA_BAC_SI == bs.MACCHN || x3.MA_BAC_SI.StartsWith(bs.MACCHN + ";")) && !lst.Contains(bs.MACCHN))
                        //{
                        //    lst.Add(bs.MACCHN);
                        //}
                        if (!pivots.TryGetValue(dte, out lst))
                        {
                            lst = new Dictionary<string, string>();
                            pivots.Add(dte, lst);
                        }

                        //Linhist changed 10/06/26
                        //if (x3.MA_BAC_SI == bs.MACCHN || x3.MA_BAC_SI.StartsWith(bs.MACCHN + ";"))
                        if (HasMaBS(bs.MA_CCHN, x3.XML3.MA_BAC_SI))
                        {
                            perd = x3.Period;
                            if (!lst.ContainsKey(bs.MA_CCHN))
                            {
                                lst.Add(bs.MA_CCHN, perd);
                            }
                            else
                            {
                                var str = lst[bs.MA_CCHN];
                                if (!str.Contains(perd))
                                {
                                    lst[bs.MA_CCHN] = str + perd;
                                }
                            }
                        }
                    }

                    foreach (var x3 in data3b)
                    {
                        dte = x3.Date;

                        //Linhist changed 24/01/26
                        //if (!pivots.TryGetValue(dte, out lst))
                        //{
                        //    lst = new List<string>();
                        //    pivots.Add(dte, lst);
                        //}

                        //if ((x3.NGUOI_THUC_HIEN == bs.MACCHN || x3.NGUOI_THUC_HIEN.Contains(bs.MACCHN + ";")) && !lst.Contains(bs.MACCHN))
                        //{
                        //    lst.Add(bs.MACCHN);
                        //}
                        if (!pivots.TryGetValue(dte, out lst))
                        {
                            lst = new Dictionary<string, string>();
                            pivots.Add(dte, lst);
                        }

                        //Linhist changed 10/06/26
                        //if (x3.NGUOI_THUC_HIEN == bs.MACCHN || x3.NGUOI_THUC_HIEN.Contains(bs.MACCHN + ";"))
                        //Linhist changed 170826
                        //if (HasMaBS(bs.MACCHN, x3.XML3.NGUOI_THUC_HIEN))
                        if (x3.XML3.MA_NHOM == 1)
                        {
                            if (bs.MA_CCHN == x3.XML3.NGUOI_THUC_HIEN || x3.XML3.NGUOI_THUC_HIEN.StartsWith($"{bs.MA_CCHN};"))
                            {
                                perd = x3.Period;
                                if (!lst.ContainsKey(bs.MA_CCHN))
                                {
                                    lst.Add(bs.MA_CCHN, perd);
                                }
                                else
                                {
                                    var str = lst[bs.MA_CCHN];
                                    if (!str.Contains(perd))
                                    {
                                        lst[bs.MA_CCHN] = str + perd;
                                    }
                                }
                            }

                            //MaCCHN thứ hai của XML3 nhóm 1 phải tính ngày kết quả
                            if (bs.MA_CCHN != x3.XML3.NGUOI_THUC_HIEN && (x3.XML3.NGUOI_THUC_HIEN.EndsWith($";{bs.MA_CCHN}") || x3.XML3.NGUOI_THUC_HIEN.EndsWith($";{bs.MA_CCHN};")) && x3.XML3.NGAY_KQ != null)
                            {
                                lstX3Mores.Add(x3.XML3);
                                var dte1 = x3.XML3.NGAY_KQ.Value;
                                if (!pivots.TryGetValue(dte1, out var lst1))
                                {
                                    lst1 = new Dictionary<string, string>();
                                    pivots.Add(dte1, lst1);
                                }

                                perd = GetWorkPeriod(x3.XML3.NGAY_KQ, ofct);
                                if (!lst1.ContainsKey(bs.MA_CCHN))
                                {
                                    lst1.Add(bs.MA_CCHN, perd);
                                }
                                else
                                {
                                    var str = lst1[bs.MA_CCHN];
                                    if (!str.Contains(perd))
                                    {
                                        lst1[bs.MA_CCHN] = str + perd;
                                    }
                                }
                            }
                        }
                        else if (HasMaBS(bs.MA_CCHN, x3.XML3.NGUOI_THUC_HIEN))
                        {
                            perd = x3.Period;
                            if (!lst.ContainsKey(bs.MA_CCHN))
                            {
                                lst.Add(bs.MA_CCHN, perd);
                            }
                            else
                            {
                                var str = lst[bs.MA_CCHN];
                                if (!str.Contains(perd))
                                {
                                    lst[bs.MA_CCHN] = str + perd;
                                }
                            }
                        }
                    }

                    //Kiem tra XML04 theo MA_BS_DOC_KQ & NGAY_KQ
                    foreach (var x4 in data4)
                    {
                        //Linhist changed 21/02/25
                        //dte = x4.NGAY_KQ.Value.Date;
                        dte = x4.Date;

                        //Linhist changed 24/01/26
                        //if (!pivots.TryGetValue(dte, out lst))
                        //{
                        //    lst = new List<string>();
                        //    pivots.Add(dte, lst);
                        //}

                        //if ((x4.MA_BS_DOC_KQ == bs.MACCHN || x4.MA_BS_DOC_KQ.Contains(bs.MACCHN + ";")) && !lst.Contains(bs.MACCHN))
                        //{
                        //    lst.Add(bs.MACCHN);
                        //}
                        if (!pivots.TryGetValue(dte, out lst))
                        {
                            lst = new Dictionary<string, string>();
                            pivots.Add(dte, lst);
                        }

                        //Linhist changed 10/06/26
                        //if (x4.MA_BS_DOC_KQ == bs.MACCHN || x4.MA_BS_DOC_KQ.Contains(bs.MACCHN + ";"))
                        if (HasMaBS(bs.MA_CCHN, x4.XML4.MA_BS_DOC_KQ))
                        {
                            perd = x4.Period;
                            if (!lst.ContainsKey(bs.MA_CCHN))
                            {
                                lst.Add(bs.MA_CCHN, perd);
                            }
                            else
                            {
                                var str = lst[bs.MA_CCHN];
                                if (!str.Contains(perd))
                                {
                                    lst[bs.MA_CCHN] = str + perd;
                                }
                            }
                        }
                    }

                    //Kiem tra XML05 theo NGUOI_THUC_HIEN & THOI_DIEM_DBLS
                    foreach (var x5 in data5)
                    {
                        //Linhist changed 21/02/25
                        //dte = x5.THOI_DIEM_DBLS.Value.Date;
                        dte = x5.Date;

                        //Linhist changed 24/01/26
                        //if (!pivots.TryGetValue(dte, out lst))
                        //{
                        //    lst = new List<string>();
                        //    pivots.Add(dte, lst);
                        //}

                        //if ((x5.NGUOI_THUC_HIEN == bs.MACCHN || x5.NGUOI_THUC_HIEN.Contains(bs.MACCHN + ";")) && !lst.Contains(bs.MACCHN))
                        //{
                        //    lst.Add(bs.MACCHN);
                        //}
                        if (!pivots.TryGetValue(dte, out lst))
                        {
                            lst = new Dictionary<string, string>();
                            pivots.Add(dte, lst);
                        }

                        //Linhist changed 10/06/26
                        //if (x5.NGUOI_THUC_HIEN == bs.MACCHN || x5.NGUOI_THUC_HIEN.Contains(bs.MACCHN + ";")
                        if (HasMaBS(bs.MA_CCHN, x5.XML5.NGUOI_THUC_HIEN))
                        {
                            perd = x5.Period;
                            if (!lst.ContainsKey(bs.MA_CCHN))
                            {
                                lst.Add(bs.MA_CCHN, perd);
                            }
                            else
                            {
                                var str = lst[bs.MA_CCHN];
                                if (!str.Contains(perd))
                                {
                                    lst[bs.MA_CCHN] = str + perd;
                                }
                            }
                        }
                    }
                }

                string fmBsCK = $"IFERROR(INDEX('Bác sĩ'!C:C, MATCH(\"{{0}}\", 'Bác sĩ'!B:B, 0)),\"\")";

                using (var fs = new MemoryStream())
                {
                    //Linhist removed 25/12/24
                    //var dataBS = data2YL.Union(data3YL).Union(data3THYL).Union(data4KQ).Union(data5TD).Distinct()
                    //                .GroupBy(x => x.NGAY_LV, (x, g) => new { MA_BAC_SI = ";" + string.Join(';', g.Select(x => x.MA_BAC_SI).Distinct().ToArray()) + ";", NGAY_LV = x })
                    //                .Distinct().ToList();

                    #region Sheet 1: Cham cong nhan vien BV
                    ISheet xlSheetBS = workbook.CreateSheet("Bác sĩ");
                    //Linhist changed 20/02/25
                    //int colBS = CreateHeader_ForWorkDays(xlSheetBS, dStart, cellStyles);
                    int colBS = CreateHeader_ForWorkDays(xlSheetBS, cellStyles,
                                        $"Bảng Chấm công trong Tháng {dStart:MM} năm {dStart.Year}",
                                        "STT", "Mã CCHN", "Họ tên", "Chức danh", "Mã khoa", "Tên khoa");

                    //Add data for each bacsi
                    int rows = 5;
                    foreach (var bs in allBS)
                    {
                        //Linhist removed 25/12/24
                        //cds = 0;
                        //int.TryParse(bs.CHUCDANH_NN, out cds);
                        //AppendRow(xlSheetBS, cellStyles, rows++, rows - 5, bs.MACCHN, bs.HO_TEN, GetType_BacSi(bs.CHUCDANH_NN), bs.MA_KHOA, bs.TEN_KHOA);
                        AppendRow(xlSheetBS, cellStyles, rows++, rows - 5, bs.MA_CCHN, bs.HO_TEN, bs.CHUC_DANH, bs.MA_KHOA, bs.TEN_KHOA);
                    }

                    ICell xcel;
                    ICellStyle celStyle;
                    int cols = colBS;
                    DateTime dte = dStart;
                    Dictionary<string, string> lstBS = null;
                    while (dte < dEnd)
                    {
                        cols++;
                        celStyle = dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday ? cellStyles[2] : cellStyles[1];

                        //Add header for date
                        AddDateHeader_ForWorkDays(xlSheetBS, cols, dte, ServiceConstants.DAYS_OF_WEEK[(int)dte.DayOfWeek], celStyle);

                        //Add work days for each bacsi
                        rows = 5;
                        celStyle = dte.DayOfWeek == DayOfWeek.Saturday || dte.DayOfWeek == DayOfWeek.Sunday ? cellStyles[6] : cellStyles[3];

                        //Linhist changed 25/12/24
                        //var maBS = dte > DateTime.Now ? "" : dataBS.FirstOrDefault(w => w.NGAY_LV == dte)?.MA_BAC_SI;
                        if (dte > DateTime.Now || !pivots.TryGetValue(dte, out lstBS))
                        {
                            lstBS = null;
                        }

                        foreach (var bs in allBS)
                        {
                            xcel = xlSheetBS.GetRow(rows++).CreateCell(cols);
                            xcel.CellStyle = celStyle;

                            //Linhist changed 24/01/26
                            //if (dte > DateTime.Now)
                            //{
                            //    xcel.SetCellValue("");
                            //}
                            //else
                            //{
                            //    //Linhist changed 25/12/24
                            //    //xcel.SetCellValue(maBS.Contains($";{bs.MACCHN};") ? "Đ" : "N");
                            //    xcel.SetCellValue(lstBS?.Contains(bs.MACCHN) == true ? "Đ" : "N");
                            //}
                            if (dte > DateTime.Now)
                            {
                                xcel.SetCellValue("");
                            }
                            else if (lstBS == null || !lstBS.ContainsKey(bs.MA_CCHN))
                            {
                                xcel.SetCellValue("N");
                            }
                            else
                            {
                                xcel.SetCellValue(FixPeriod(lstBS[bs.MA_CCHN]));
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

                        //Add data for each bacsi
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

                            //Add header for date
                            AddDateHeader_ForWorkDays(xlSheetUS, cols, dte, ServiceConstants.DAYS_OF_WEEK[(int)dte.DayOfWeek], celStyle);

                            //Add work days for each bacsi
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
                        var maBS = x.XML2.MA_BAC_SI;
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
                        var maBS = x.XML3.MA_BAC_SI;
                        var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                        AppendRow(xlSheetX3, cellStyles, rows++,
                                x.XML3.STT, x.XML3.MA_LK, //x.XML01.MA_BN, x.XML01.HO_TEN,
                                x.XML3.MA_DICH_VU, x.XML3.TEN_DICH_VU, x.XML3.MA_VAT_TU, x.XML3.TEN_VAT_TU,
                                maBS, fmBS[0], "", "",
                                x.XML3.NGAY_YL, x.Period);
                    }
                    foreach (var x in data3b)
                    {
                        var maBS = x.XML3.NGUOI_THUC_HIEN;
                        var fmBS = GetFormular_BacSi(new string[] { fmBsCK }, ref maBS);
                        AppendRow(xlSheetX3, cellStyles, rows++,
                                x.XML3.STT, x.XML3.MA_LK, //x.XML01.MA_BN, x.XML01.HO_TEN,
                                x.XML3.MA_DICH_VU, x.XML3.TEN_DICH_VU, x.XML3.MA_VAT_TU, x.XML3.TEN_VAT_TU,
                                "", "", maBS, fmBS[0],
                                x.XML3.NGAY_TH_YL, x.Period);
                    }
                    foreach (var x in lstX3Mores)
                    {
                        var maBS = x.NGUOI_THUC_HIEN;
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
                        var maBS = x.XML4.MA_BS_DOC_KQ;
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
                        var maBS = x.XML5.NGUOI_THUC_HIEN;
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
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_ChamCong)}");
            }

            return res;
        }

        public async Task<ResponseModel<byte[]>> BaoCaoDayDuLieu(IEnumerable<HosoDayLaiInfo> hosos, string maCSKCB, int type)
        {
            var res = new ResponseModel<byte[]>(HttpStatusCode.OK, "", null);

            try
            {
                var dmKhoas = await _dmKhoaRepository.GetListAsync(k => k.MA_CSKCB == maCSKCB);
                string fmKhoa = $"$=IFERROR(VLOOKUP(\"{{0}}\",'DM Khoa'!A2:B{dmKhoas.Count() + 5},2,FALSE),\"\")";

                //Create new excel
                IWorkbook workbook = CreateWorkBook(out ICellStyle[] cellStyles);

                // lấy phần tử cuối cùng nếu không có ; thì giữ nguyên
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
                res = Uti.HandleInternalServerError<byte[]>(ex, null, _logger, $"{nameof(ReportHandleService)} - {nameof(BaoCao_ThongkeLoi)}");
            }

            return res;
        }
    }

    //haiamster 28.2.2025
    public class GroupedDataWithSTT
    {
        public int XML01IDs { get; set; }
        public DateTime NGAY_TAO { get; set; }
        public int TotalErrors { get; set; } // New property to hold the total count of MA_LOI
        public int STT { get; set; } // Property to hold the STT for the group

        public List<FileData> Files { get; set; } // List to hold the file data
        public class FileData
        {
            public int ChgID { get; set; }
            public string MA_CSKCB { get; set; }
            public int STT { get; set; }
            public string MA_LK { get; set; }
            public string MA_BN { get; set; }
            public string MA_THE_BHYT { get; set; }
            public string HO_TEN { get; set; }
            public int MA_LOI { get; set; }
            public string MA_KHOA { get; set; }
        }
    }
}