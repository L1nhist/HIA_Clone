using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML05 : BaseValidator<XML05>
    {
        private readonly IEnumerable<DMBacSi> dmBACSI;
        private readonly IEnumerable<DMError130> dmError130;
        private readonly IEnumerable<DMChung130> DMChung;

        public ValidateXML05
        (
            IEnumerable<DMBacSi> dMBacSis,
            IEnumerable<DMError130> dMError130s,
            IEnumerable<DMChung130> dMChung130s,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            dmBACSI = dMBacSis;
            dmError130 = dMError130s;
            DMChung = dMChung130s;
        }
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        public static List<DayOfWeek> ParseWeekdays(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            // Bước 1: Chuẩn hóa chuỗi
            input = input.ToUpper()
                         .Replace("VÀ", ",")
                         .Replace(" ", "")
                         .Replace("CN", "T8"); // CN → T8 để xử lý đồng nhất

            // Bước 2: Tách chuỗi bằng Regex: tìm tất cả các cụm T2–T8 hoặc số 2–8
            var matches = Regex.Matches(input, @"T[2-8]|\b[2-8]\b");

            var result = new List<DayOfWeek>();
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string val = match.Value.Replace("T", "");
                if (val == "8") result.Add(DayOfWeek.Sunday);
                else if (int.TryParse(val, out int num) && num >= 2 && num <= 7)
                    result.Add((DayOfWeek)(((int)DayOfWeek.Monday + num - 2) % 7));
            }

            return result.Distinct().OrderBy(d => d).ToList();
        }

        public static bool IsInTimeRanges(string scheduleString, DateTime timeToCheck)
        {
            var entries = scheduleString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                string dayCode = "";
                string timePart = "";

                // Xác định phần ngày
                if (entry.StartsWith("CN"))
                {
                    dayCode = "CN";
                    timePart = entry.Substring(2);
                }
                else if (entry.StartsWith("T") && entry.Length >= 3 && char.IsDigit(entry[1]))
                {
                    dayCode = entry.Substring(0, 2);
                    timePart = entry.Substring(2);
                }
                else
                {
                    // Không có tiền tố ngày → áp dụng cho mọi ngày
                    dayCode = "ALL";
                    timePart = entry;
                }

                // Kiểm tra ngày
                if (dayCode != "ALL")
                {
                    DayOfWeek day;
                    if (dayCode == "CN")
                        day = DayOfWeek.Sunday;
                    else if (int.TryParse(dayCode.Substring(1), out int num) && num >= 2 && num <= 7)
                        day = (DayOfWeek)(((int)DayOfWeek.Monday + num - 2) % 7);
                    else
                        continue;

                    if (timeToCheck.DayOfWeek != day) continue;
                }

                // Tách nhiều khoảng thời gian trong 1 dòng (dấu , hoặc ;)
                var timeRanges = timePart.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var range in timeRanges)
                {
                    var times = range.Replace(" ", "").Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (times.Length != 2) continue;

                    if (!TryParseTime(times[0], out TimeSpan start)) continue;
                    if (!TryParseTime(times[1], out TimeSpan end)) continue;

                    TimeSpan current = timeToCheck.TimeOfDay;
                    if (current >= start && current <= end)
                        return true;
                }
            }

            return false;
        }

        // Hỗ trợ cả "0700", "7:00", "07:00", "19:30"
        private static bool TryParseTime(string input, out TimeSpan result)
        {
            return TimeSpan.TryParseExact(input, new[] { "hhmm", "hmm", "h\\:mm", "hh\\:mm" }, null, out result);
        }
        public override async Task<bool> Validate(XML05 xml)
        {
            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;
            var errorlist = dmError130.ToDictionary(x => x.ErrorCode, x => x.Message);
            var benhchinhInfo = DMChung.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH)?.TEN ?? "Không có thông tin bệnh";
            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML05", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML05)}");

            #region Kiểm tra các lỗi cơ bản

            //436 	1	XML5	DIEN_BIEN	Nội dung Diễn biến điều trị có kết quả trùng nhau
            if (File.XML01.XML05s.Any(x=>x.DIEN_BIEN_LS==File.DIEN_BIEN_LS && x.STT!=File.STT &&x.THOI_DIEM_DBLS!=File.THOI_DIEM_DBLS))
            {
                //RaiseError(true, 436);
                RaiseError(true,
                                436, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(436, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Nội dung Diễn biến bệnh trùng nhau giữa các thời điểm ghi Diễn biến lâm sàng là không hợp lý, Việc này chỉ có thể là copy Diễn biến lâm sàng và việc này không phản ánh đúng thực tế điều trị cho Bệnh nhân"
                                );
            }
            //	1824	XML5	MA_LK	Mã liên kết không được để trống
            //	1825	XML5	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
            Field(x => x.MA_LK).IsEmpty().ThenError(1824).LengthOver(100).ThenError(1825);
            //	1266	XML5	STT	Số thứ tự không được để trống và phải có giá trị từ 0 đến 999999999
            Field(x => x.STT).IsEmpty().IsOver(999999999).ThenError(1266);
            //	433	XML5	DIEN_BIEN_LS	Diễn biến bệnh không được để trống
            Field(x => x.DIEN_BIEN_LS).IsEmpty().ThenError(433);
            if (File.DIEN_BIEN_LS.Trim().Length == 0)
            {
                //RaiseError(true, 433);
                RaiseError(true,
                                433, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(433, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Nội dung Diễn biến lâm sàng không được để trống, căn cứ theo Quyết định 3176 Bộ Y Tế"
                                );
            }
            //1803	1	XML5	DIEN_BIEN	Lào Cai: Diễn biễn bệnh không được ít hơn 7 ký tự (Ghi rõ Thứ bẩy hoặc Chủ nhật)
            if(File.DIEN_BIEN_LS.Trim().Length <7)
            {
                RaiseError(true,
                                1803, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(1803, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Nội dung Diễn biến lâm sàng không được ít hơn 7 ký tự, căn cứ theo Yêu cầu của Bệnh viện Đa Khoa Lào Cai"
                                );
            }    
            //2101	4	XML5	THOI_DIEM_DBLS	[130] Thời điểm diễn biến lâm sàng không được lớn hơn hiện tại
            Field(x => x.THOI_DIEM_DBLS).IsOver(DateTime.Now).ThenError(2101);
            //2102    4   XML5 THOI_DIEM_DBLS[130] Thời điểm diễn biến lâm sàng không được lớn hơn ngày ra viện
            //2103    4   XML5 THOI_DIEM_DBLS[130] Thời điểm diễn biến lâm sàng không được trước ngày vào viện
            //2104    4   XML5 THOI_DIEM_DBLS[130] Thời điểm diễn biến lâm sàng không được để trống
            Field(x => x.THOI_DIEM_DBLS).IsOver(File.XML01.NGAY_RA).ThenError(2102);
            Field(x => x.THOI_DIEM_DBLS).IsUnder(File.XML01.NGAY_VAO).ThenError(2103);
            Field(x => x.THOI_DIEM_DBLS).IsEmpty().ThenError(2104);
           
            //	1278	XML5	THOI_DIEM_DBLS	Thời điểm DBLS không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
            Field(x => x.THOI_DIEM_DBLS).IsEmpty().ThenError(1278);
            //	1826	XML5	NGUOI_THUC_HIEN	Người thực hiện không được để trống
            //	1827	XML5	NGUOI_THUC_HIEN	Người thực hiện không được lớn hơn 255 ký tự
            Field(x => x.NGUOI_THUC_HIEN).IsEmpty().ThenError(1826).LengthOver(255).ThenError(1827);
            #endregion

            #region Kiểm tra các lỗi có liên quan đến các trường khác

            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục

            //3258	3	XML5	THOI_DIEM_DBLS	3176: Thời gian ghi diễn biến lâm sàng không nằm trong khoảng thời gian đăng ký làm việc Tuần của nhân viên
            //3273	3	XML5	THOI_DIEM_DBLS	3176: Thời gian ghi diễn biến lâm sàng không nằm trong khoảng thời gian đăng ký làm việc Ngày của nhân viên
            if (Field(x => x.THOI_DIEM_DBLS).NotEmpty())
            {
                var tuanlamviec = ParseWeekdays(dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.NGUOI_THUC_HIEN)?.TG_TUAN);
                if (tuanlamviec != null)
                {
                    if (!tuanlamviec.Contains(File.THOI_DIEM_DBLS.Value.DayOfWeek))
                    {
                        //RaiseError(true, 3258);
                        RaiseError(true,
                                3258, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3258, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Ngày ghi Diễn biến lâm sàng là ngày {File.THOI_DIEM_DBLS.Value:dd/MM/yyyy HH:mm}, trong khi nhân viên y tế: {File.NGUOI_THUC_HIEN} chỉ đăng ký làm việc các ngày: " +
                                $"{string.Join(", ", tuanlamviec.Select(d => d.ToString()))}."
                                );
                    }
                }
                var ngaylamviec = dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.NGUOI_THUC_HIEN)?.TG_NGAY;
                if (ngaylamviec != null)
                {
                    if (!IsInTimeRanges(ngaylamviec, File.THOI_DIEM_DBLS.Value))
                    {
                        //RaiseError(true, 3273);
                        RaiseError(true,
                                3273, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3273, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Thời gian ghi Diễn biến lâm sàng là {File.THOI_DIEM_DBLS.Value:dd/MM/yyyy HH:mm}, trong khi nhân viên y tế: {File.NGUOI_THUC_HIEN} chỉ đăng ký làm việc các khung giờ: " +
                                $"{ngaylamviec}."
                                );
                    }
                }
            }
            //2100	4	XML5	GUOI_THUC_HIEN	[130] Người thực hiện không nằm trong Danh sách nhân viên được phê duyệt
            if (File.NGUOI_THUC_HIEN.IndexOf(";") < 0)
            {
                if(!dmBACSI.Any(x => x.MA_CCHN == File.NGUOI_THUC_HIEN) && !dmBACSI.Any(x=>x.MA_BHXH==File.NGUOI_THUC_HIEN))
                {
                    //RaiseError(true, 2100);
                    RaiseError(true,
                                2100, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2100, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Người thực hiện ghi Diễn biến lâm sàng là mã số: {File.NGUOI_THUC_HIEN}, không có trong danh mục nhân viên y tế được phê duyệt của cơ sở KCB."
                                );
                }
            }else
            {
                if (!dmBACSI.Any(x => x.MA_CCHN == File.NGUOI_THUC_HIEN))
                {
                    var nguoiTH = File.NGUOI_THUC_HIEN.Split(";");
                    foreach (var nguoi in nguoiTH)
                    {
                        if (!dmBACSI.Any(x => x.MA_CCHN == nguoi) && !dmBACSI.Any(x => x.MA_BHXH == nguoi))
                        {
                            //RaiseError(true, 2100);
                            RaiseError(true,
                                        2100, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2100, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Người thực hiện ghi Diễn biến lâm sàng là mã số: {nguoi}, không có trong danh mục nhân viên y tế được phê duyệt của cơ sở KCB."
                                        );
                        }
                    }
                }
            }
            //2692    3   XML5 NGUOI_THUC_HIEN[3176] Người thực phải mã hóa theo GPHN và nằm trong DM được phê duyệt
            
            if (File.NGUOI_THUC_HIEN.IndexOf(";") < 0)
            {
                if (File.NGUOI_THUC_HIEN.ToString().Length < 12)
                {
                    if (!dmBACSI.Any(x => x.MA_CCHN == File.NGUOI_THUC_HIEN))
                    {
                        //RaiseError(true, 2692);
                        RaiseError(true,
                                2692, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2692, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Người thực hiện ghi Diễn biến lâm sàng là mã số: {File.NGUOI_THUC_HIEN}, không đúng định dạng mã số theo GPHN hoặc không có trong danh mục nhân viên y tế được phê duyệt của cơ sở KCB."
                                );
                    }
                }
            }
            else
            {
                var nguoiTH = File.NGUOI_THUC_HIEN.Split(";");
                foreach (var nguoi in nguoiTH)
                {
                    if (nguoi.ToString().Length < 12)
                    {
                        if (!dmBACSI.Any(x => x.MA_CCHN == nguoi))
                        {
                            //RaiseError(true, 2692);
                            RaiseError(true,
                                    2692, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2692, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Người thực hiện ghi Diễn biến lâm sàng là mã số: {nguoi}, không đúng định dạng mã số theo GPHN hoặc không có trong danh mục nhân viên y tế được phê duyệt của cơ sở KCB."
                                    );
                        }
                    }
                }
            }
            #endregion

            return true;
        }
    }
}