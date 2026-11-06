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
    public class ValidateXML04 : BaseValidator<XML04>
    {
        //Linhist changed 20/03/26
        //private readonly IEnumerable<Hospital> dMhospital;
        private readonly TimePeriod _officialTime;

        private readonly IEnumerable<DMDichVu> dmDICHVU;
        private readonly IEnumerable<DMBacSi> dmBACSI;
        private string curMADICHVU;
        private IEnumerable<DMDichVu> cungMADICHVUs;
        private readonly IEnumerable<DMError130> dmError130;
        private readonly IEnumerable<DMChung130> DMChung;

        public ValidateXML04
        (
            //Linhist changed 20/03/26
            //IEnumerable<Hospital> hospitals,
            TimePeriod officialTime,

            IEnumerable<DMDichVu> DMDichVus,
            IEnumerable<DMBacSi> DMBacSis,
            IEnumerable<DMError130> dMError130s,
            IEnumerable<DMChung130> dMChung130s,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            //Linhist changed 20/03/26
            //dMhospital = hospitals;
            _officialTime = officialTime;

            dmDICHVU = DMDichVus;
            dmBACSI = DMBacSis;
            curMADICHVU = null;
            cungMADICHVUs = null;
            dmError130 = dMError130s;
            DMChung = dMChung130s;
        }
        private readonly Stopwatch _watch = Stopwatch.StartNew();
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

        public override async Task<bool> Validate(XML04 xml)
        {
            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;
            _watch.Restart();

            if (xml.MA_DICH_VU != curMADICHVU)
            {
                curMADICHVU = xml.MA_DICH_VU;
                if (!string.IsNullOrEmpty(curMADICHVU))
                {
                    cungMADICHVUs = dmDICHVU.Where(t => t.MA_DICH_VU == curMADICHVU).ToList();
                }
            }

            #region Kiểm tra các lỗi cơ bản
            //337	XML4	MA_LK	Mã liên kết không được để trống
            //1270	XML4	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
            Field(x => x.MA_LK).IsEmpty().ThenError(337).LengthOver(100).ThenError(1270);
            //1271	XML4	STT	Số thứ tự không được để trống và phải có giá trị từ 0 đến 999999999
            Field(x => x.STT).IsEmpty().IsOver(999999999).ThenError(1271);
            //1272	XML4	MA_DICH_VU	Mã dịch vụ không được để trống
            //1273	XML4	MA_DICH_VU	Mã dịch vụ không được lớn hơn 15 ký tự
            //Linhist changed 250926
            //Field(x => x.MA_DICH_VU).IsEmpty().ThenError(1272).ThenError(1273);
            Field(x => x.MA_DICH_VU).IsEmpty().ThenError(1272).LengthOver(50).ThenError(1273);
            //1274	XML4	MA_CHI_SO	Mã chỉ số không được để trống
            //1275	XML4	MA_CHI_SO	Mã chỉ số không được lớn hơn 50 ký tự
            Field(x => x.MA_CHI_SO).IsEmpty().ThenError(1274).LengthOver(50).ThenError(1275);
            //1276	XML4	TEN_CHI_SO	Tên chỉ số không được để trống
            //1277	XML4	TEN_CHI_SO	Tên chỉ số không được lớn hơn 255 ký tự
            Field(x => x.TEN_CHI_SO).IsEmpty().ThenError(1276).LengthOver(255).ThenError(1277);
            //1223   1   XML4 NGAY_KQ Ngày Kết quả không được lớn hơn thời điểm hiện tại
            Field(x => x.NGAY_KQ).IsOver(DateTime.Now).ThenError(1223);
            //2356	0	XML4	MA_BS_DOC_KQ	[130] Mã BS đọc kết quả không được để trống
            Field(x => x.MA_BS_DOC_KQ).IsEmpty().ThenError(2356);

            //2105    4   XML4 NGAY_KQ[130] Ngày kết quả không được để trống
            //2344	0	XML4	GIA_TRI	[130] Trường giá trị không nhập quá 50 ký tự
            if (!string.IsNullOrEmpty(File.GIA_TRI))
            {
                Field(x => x.NGAY_KQ).IsEmpty().ThenError(2105);
                Field(x => x.GIA_TRI).LengthOver(255).ThenError(2344);
            }

            //Linhist changed 20/03/26
            if (!string.IsNullOrEmpty(File.MO_TA) || !string.IsNullOrEmpty(File.KET_LUAN))
            {
                Field(x => x.NGAY_KQ).IsEmpty().ThenError(2105);
            }
            #endregion

            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML04", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML04)}");
            var errorlist = dmError130.ToDictionary(x => x.ErrorCode, x => x.Message);
            var benhchinhInfo = DMChung.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH)?.TEN ?? "Không có thông tin bệnh";


            #region kiểm tra đặc biệt cho XML4
            //431 	1 	XML4 	KET_LUAN 	Dịch vụ Giải phẫu bệnh nhuộm hai màu không có kết luận Trợt, loét, polyp, U hoặc Khối
            //const string strDVXml4 = "20.0059.0140;20.0059.0140;02.0295.0498;02.0295.0498;02.0295.0498;02.0295.0498;02.0286.0497;02.0286.0497;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0282.0318;02.0282.0318;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0271.0140;02.0271.0140;20.0076.0140;01.0232.0140;01.0232.0140;02.0307.0136;02.0307.0136;02.0307.0136;02.0307.0136;02.0307.0136;02.0262.0136;02.0262.0136;20.0053.0105;20.0053.0105;02.0217.0183;02.0369.0185;01.0101.0125;02.0038.0125;02.0283.0141;02.0284.0141;02.0275.0141;02.0275.0141;02.0275.0141;02.0275.0141;20.0055.0496;20.0056.0141;02.0036.0127;02.0036.0127;02.0036.0128;02.0036.0128;02.0045.0130;02.0045.0130;02.0045.0132;02.0045.0132;02.0043.0127;02.0043.0127;02.0304.0134;02.0304.0134;02.0304.0134;02.0304.0134;02.0304.0134;02.0309.0138;02.0309.0138;02.0309.0138;20.0072.0191;20.0044.0503;03.2340.0157;03.2340.0157";
            //    if (File.XML01.XML04s.Any(x => Uti.CodeContain(x.MA_DICH_VU, strDVXml4)))
            //    {

            //        RaiseError(!File.XML01.XML05s.Any(x => Uti.StrExists(x.DIEN_BIEN_LS, true, "Trợt", "loét", "polyp", "U hoặc Khối")), 431);
            //    }


            //3786	3	XML4	MO_TA	[3716] Trường MÔ TẢ đang nhập lớn hơn 4000 ký tự, chỉnh sửa lại
            if (File.MO_TA.Length >= 3000)
            {
                RaiseError(true,
                               3786, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                               $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                               $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                               $"Nội dung lỗi: {(errorlist.TryGetValue(3786, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                               $"Chi tiết:" +
                               $"Bệnh nhân có Trường Mô tả quá dài, lớn:{File.MO_TA.Length} ký tự, Cổng BH sẽ không tiếp nhận, đề nghị kiểm tra lại."
                               );
            }
            //3820	4	XML4	MA_CHI_SO	[3176] Độ dài cột mã chỉ số không lớn hơn 255 ký tự
            if (File.MA_CHI_SO.Length > 255)
            {
                RaiseError(true,
                    3820, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    $"Nội dung lỗi: {(errorlist.TryGetValue(3820, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    $"Chi tiết:" +
                    $"Bệnh nhân có Trường Mã chỉ số quá dài, lớn:{File.MA_CHI_SO.Length} ký tự, Cổng BH sẽ không tiếp nhận, đề nghị kiểm tra lại."
                    );
            }

            #region Linhist changed 20/03/26
            ////2520	3	XML4	NGAY_KQ	[130] Ngày KQ ngoài thời gian làm việc hành chính của Viện
            //Hospital currentHospital = new Hospital();
            //if (dMhospital.Any())
            //{
            //    currentHospital = dMhospital.FirstOrDefault(x => !String.IsNullOrEmpty(x.OfficeTime));
            //    if (currentHospital != null)
            //    {
            //        TimePeriod gio_Hanh_Chinh_BV = new TimePeriod(currentHospital.OfficeTime);

            //        if (Field(x => x.NGAY_KQ).NotEmpty())
            //        {
            //            if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_KQ))
            //            {
            //                //RaiseError(true, 2520);
            //                RaiseError(true,
            //                    2520, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
            //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
            //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
            //                    $"Nội dung lỗi: {(errorlist.TryGetValue(2520, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
            //                    $"Chi tiết:" +
            //                    $"Bệnh nhân có ngày trả kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm}, Không khớp với thời gian làm việc hành chính của Viện, đề nghị kiểm tra lại."
            //                    );
            //            }
            //        }
            //    }
            //}

            //2520	3	XML4	NGAY_KQ	[130] Ngày KQ ngoài thời gian làm việc hành chính của Viện
            if (_officialTime != null && _officialTime.IsValid && !_officialTime.InRange(File.NGAY_KQ))
            {
                RaiseError(true, 2520,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2520, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân có ngày trả kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm}, Không khớp với thời gian làm việc hành chính của Viện, đề nghị kiểm tra lại."
                );
            }
            #endregion

            //2097	4	XML4	NGAY_KQ	[130] Ngày kết quả tại XML4 không được nhỏ hơn ngày vào
            //Field(x => x.NGAY_KQ).IsUnder(File.XML01.NGAY_VAO).ThenError(2097);
            if (File.NGAY_KQ <= File.XML01.NGAY_VAO)
            {
                RaiseError(true,
                    2097,
                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    $"Nội dung lỗi: {(errorlist.TryGetValue(2097, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    $"Chi tiết:" +
                    $"Bệnh nhân có ngày trả kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm}, ngày vào Viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm}. Không hợp lý về mặt logic thời gian"
                    );
            }

            //2098	4	XML4	NGAY_KQ	[130] Ngày kết quả tại XML4 không trùng với ngày kết quả tại XML3
            //RaiseError(!File.XML01.XML03s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && x.NGAY_KQ == File.NGAY_KQ), 2098);
            if (!File.XML01.XML03s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && x.NGAY_KQ == File.NGAY_KQ))
            {
                RaiseError(true,
                    2098,
                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    $"Nội dung lỗi: {(errorlist.TryGetValue(2098, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    $"Chi tiết:" +
                    $"Bệnh nhân có ngày trả kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm}, Tuy nhiên thời gian trả kết quả cho dịch vụ {File.MA_DICH_VU} tại XML3 lại không khớp với thời gian này."
                    );
            }

            //2099	4	XML4	MA_BS_DOC_KQ	[130] Mã BS đọc kết quả không có trong DM phê duyệt
            if (!File.MA_DICH_VU.Contains("K"))
            {
                //if (File.MA_BS_DOC_KQ.IndexOf(";")>-1)
                //{
                //    var mabs = File.MA_BS_DOC_KQ.Split(";");
                //    foreach (var mabs2 in mabs)
                //    {
                //        if (!dmBACSI.Any(x => x.MA_CSKCB == File.MA_CSKCB && x.MACCHN == mabs2) && !dmBACSI.Any(x => x.MA_CSKCB == File.MA_CSKCB && x.MA_BHXH == mabs2))
                //        {
                //            RaiseError(true, 2099);
                //        }
                //    }

                //}
                //else 
                if (!dmBACSI.Any(x => x.MA_CSKCB == File.MA_CSKCB && x.MA_CCHN == File.MA_BS_DOC_KQ) && !dmBACSI.Any(x => x.MA_CSKCB == File.MA_CSKCB && x.MA_BHXH == File.MA_BS_DOC_KQ))
                {
                    //RaiseError(true, 2099);
                    RaiseError(true,
                        2099,
                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(2099, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bác sĩ: {File.MA_BS_DOC_KQ} không tồn tại trong DM Bác sĩ được bảo hiểm phê duyệt, đề nghị kiểm tra lại danh mục Bác sĩ trên cổng, đồng bộ lại Danh mục để sử dụng."
                        );
                }
                //2693	3	XML4	MA_BS_DOC_KQ	[3176] Mã BS Đọc kết quả phải mã hóa theo CCHN và nằm trong DM được phê duyệt
                if (!dmBACSI.Any(x => x.MA_CSKCB == File.MA_CSKCB && x.MA_CCHN == File.MA_BS_DOC_KQ))
                {
                    RaiseError(true,
                        2693,
                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(2693, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bác sĩ: {File.MA_BS_DOC_KQ} không tồn tại trong DM Bác sĩ được bảo hiểm phê duyệt (Mã Chứng chỉ hành nghề), đề nghị kiểm tra lại danh mục Bác sĩ trên cổng, đồng bộ lại Danh mục để sử dụng."
                        );

                    //RaiseError(true, 2963);
                }
            }
            #endregion

            #region Kiểm tra các lỗi có liên quan đến các Danh mục
            //3257	3	XML4	NGAY_KQ	3176: Ngày trả kết quả không nằm trong khoảng thời gian đăng ký làm việc Tuần của nhân viên
            //3272	3	XML4	NGAY_KQ	3176: Ngày trả kết quả không nằm trong khoảng thời gian đăng ký làm việc Ngày của nhân viên
            if (Field(x => x.NGAY_KQ).NotEmpty())
            {
                var tuanlamviec = ParseWeekdays(dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.MA_BS_DOC_KQ)?.TG_TUAN);
                if (tuanlamviec != null)
                {
                    if (!tuanlamviec.Contains(File.NGAY_KQ.Value.DayOfWeek))
                    {
                        //RaiseError(true, 3257);
                        RaiseError(true,
                            3257,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3257, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bác sĩ: {File.MA_BS_DOC_KQ}, Trả kết quả vào lúc {File.NGAY_KQ:dd/MM/yyyy HH:mm}, BS chỉ đăng ký làm việc các ngày:" +
                            $"{string.Join(", ", tuanlamviec.Select(d => d.ToString()))}."
                        );
                    }
                }
                var ngaylamviec = dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.MA_BS_DOC_KQ)?.TG_NGAY;
                if (ngaylamviec != null)
                {
                    if (!IsInTimeRanges(ngaylamviec, File.NGAY_KQ.Value))
                    {
                        //RaiseError(true, 3272);
                        RaiseError(true,
                            3272,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3272, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bác sĩ: {File.MA_BS_DOC_KQ}, Trả kết quả vào lúc {File.NGAY_KQ:dd/MM/yyyy HH:mm}, chỉ đăng ký làm việc các khung giờ:" +
                            $"{ngaylamviec}."
                        );

                    }
                }
            }
            #endregion
            #region Kiểm tra các chỉ số trong XML4
            //1961	0	XML4	MA_DICH_VU	Dịch vụ Chẩn đoán hình ảnh không có đủ Mô tả & Kết luận trong XML4
            //if (Uti.CodeMatch(File.MA_DICH_VU, "18.0150.0041;18.0151.0041;18.0156.0041;18.0159.0041;18.0192.0041;18.0222.0041;18.0256.0041;18.0258.0041;18.0260.0041;18.0262.0041;18.0263.0041;18.0265.0041;18.0297.0065;18.0298.0066;18.0299.0065;18.0300.0066;18.0301.0065;18.0302.0065;18.0304.0065;18.0311.0065;18.0314.0065;18.0317.0065;18.0319.0066;18.0320.0065;18.0323.0065;18.0325.0065;18.0329.0065;18.0333.0067;18.0335.0065;18.0337.0065;18.0339.0065;18.0341.0065;18.0342.0065;18.0344.0065;18.0346.0065;18.0353.0065;18.0355.0065;18.0359.0065;18.0360.0065;18.0361.0065"))
            //{
            //    if (string.IsNullOrEmpty(File.MO_TA) || File.MO_TA.Length < 5 || string.IsNullOrEmpty(File.KET_LUAN) || File.KET_LUAN?.Length < 5)
            //    {
            //        RaiseError(true, 1691);
            //    }
            //}
            //2471	4	XML4	KET_LUAN 	[130] Trường kết luận không nhập nội dung cho các Dịch vụ CĐHA (Mã nhóm =2)
            //2521	3	XML4	MA_DICH_VU	[130] Dịch vụ xét nghiệm không nhập giá trị và kết quả
            if (!string.IsNullOrEmpty(File.MA_DICH_VU))
            {

                if (File.XML01.XML03s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && x.MA_NHOM == 2))
                {
                    if (string.IsNullOrEmpty(File.MO_TA) || File.MO_TA.Length < 5 || string.IsNullOrEmpty(File.KET_LUAN) || File.KET_LUAN?.Length < 5)
                    {
                        //RaiseError(true, 1691);
                        RaiseError(true,
                            1961,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN:{File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_RV}| {ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH}) - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(1961, out var Erifo) ? Erifo:"Không tim thấy nội dung lỗi")}\n" +
                            $"Chi tiết:"+
                            $"Bệnh nhân được kê: {File.MA_DICH_VU} với Mã nhóm =2 (Chuẩn đoán hình ảnh), tuy nhiên lại không nhập MÔ TẢ hoặc KẾT LUẬN tại XML4"
                            );
                    }
                    if (string.IsNullOrEmpty(File.KET_LUAN) || File.KET_LUAN?.Length < 5)
                    {
                        //RaiseError(true, 2471);
                        RaiseError(true,
                            2471,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN:{File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_RV}| {ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH}) - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2471, out var Erifo) ? Erifo : "Không tim thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU} với Mã nhóm =2 (Chuẩn đoán hình ảnh), tuy nhiên lại không nhập KẾT LUẬN tại XML4"
                            );

                    }

                }
                if (File.XML01.XML03s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && x.MA_NHOM == 1))
                {
                    if (string.IsNullOrEmpty(File.GIA_TRI))
                    {
                        //RaiseError(true, 2521);
                        RaiseError(true,
                            2521,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN:{File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_RV}| {ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH}) - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2521, out var Erifo) ? Erifo : "Không tim thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU} với Mã nhóm =1 (Xét nghiệm), tuy nhiên lại không nhập GIÁ TRỊ tại XML4"
                            );
                    }
                }
                //2586	3	XML4	MA_DICH_VU	[130] DV Xét nghiệm, CĐHA, thăm dò chức năng (mã nhóm 1,2,3), không có kết quả tại XML 4
                
                if (File.XML01.XML03s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && (x.MA_NHOM == 1 || x.MA_NHOM == 2 ||x.MA_NHOM==3)))
                {
                    if (string.IsNullOrEmpty(File.GIA_TRI) && string.IsNullOrEmpty(File.MO_TA) && string.IsNullOrEmpty(File.KET_LUAN))
                    {
                        //RaiseError(true, 2586);
                        RaiseError(true,
                            2586,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN:{File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_RV}| {ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH}) - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2586, out var Erifo) ? Erifo : "Không tim thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU} với Mã nhóm =1 (Xét nghiệm) hoặc 2 (Chuẩn đoán hình ảnh) hoặc 3 (Thăm dò chức năng), tuy nhiên lại không nhập GIÁ TRỊ hoặc MÔ TẢ hoặc KẾT LUẬN tại XML4"
                            );
                    }
                }    

            }

            _watch.Stop();
            //Uti.HandleCronJobLog(_logger, "Giám định xong XML1, thời gian:" + _watch.Elapsed.TotalSeconds.ToString(), nameof(Validate));
            #endregion

            return true;
        }
    }
}