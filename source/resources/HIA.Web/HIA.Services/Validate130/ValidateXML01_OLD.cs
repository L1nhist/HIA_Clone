//using HIA.Core.Base;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Files;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace HIA.Services.Validate130
//{
//    public class ValidateXML01 : BaseValidator<XML01>
//    {
//        private const string MA_BENH_SAI_31313 = "N70;N70.0;N70.1;N70.9;N71;N71.0;N71.1;N71.9;N72;N73;N73.0;N73.1;N73.2;N73.3;N73.4;N73.5;N73.6;N73.8;N73.9;N74*;N74.0*;N74.1*;N74.2*;N74.3*;N74.4*;N74.8*;N75;N75.0;N75.1;N75.8;N75.9;N76;N76.0;N76.1;N76.2;N76.3;N76.4;N76.5;N76.6;N76.8;N77*;N77.0*;N77.1*;N77.8*;N80;N80.0;N80.1;N80.2;N80.3;N80.4;N80.5;N80.6;N80.8;N80.9;N81;N81.0;N81.1;N81.2;N81.3;N81.4;N81.5;N81.6;N81.8;N81.9;N82;N82.0;N82.1;N82.2;N82.3;N82.4;N82.5;N82.8;N82.9;N83;N83.0;N83.1;N83.2;N83.3;N83.4;N83.5;N83.6;N83.7;N83.8;N83.9;N84;N84.0;N84.1;N84.2;N84.3;N84.8;N84.9;N85;N85.0;N85.1;N85.2;N85.3;N85.4;N85.5;N85.6;N85.7;N85.8;N85.9;N86;N87;N87.0;N87.1;N87.2;N87.9;N88;N88.0;N88.1;N88.2;N88.3;N88.4;N88.8;N88.9;N89;N89.0;N89.1;N89.2;N89.3;N89.4;N89.5;N89.6;N89.7;N89.8;N89.9;N90;N90.0;N90.1;N90.2;N90.3;N90.4;N90.5;N90.6;N90.7;N90.8;N90.9;N91;N91.0;N91.1;N91.2;N91.3;N91.4;N91.5;N92;N92.0;N92.1;N92.2;N92.3;N92.4;N92.5;N92.6;N93;N93.0;N93.8;N93.9;N94;N94.0;N94.1;N94.2;N94.3;N94.4;N94.5;N94.6;N94.8;N94.9;N95;N95.0;N95.1;N95.2;N95.3;N95.8;N95.9;N96;N97;N97.0;N97.1;N97.2;N97.3;N97.4;N97.8;N97.9;N98;N98.0;N98.1;N98.2;N98.3;N98.8;N98.9;O00;O00.0;O00.1;O00.2;O00.8;O00.9;O01;O01.0;O01.1;O01.9;O02;O02.0;O02.1;O02.8;O02.9;O03;O03.0;O03.1;O03.2;O03.3;O03.4;O03.5;O03.6;O03.7;O03.8;O03.9;O04;O04.0;O04.1;O04.2;O04.3;O04.4;O04.5;O04.6;O04.7;O04.8;O04.9;O05;O05.0;O05.1;O05.2;O05.3;O05.4;O05.5;O05.6;O05.7;O05.8;O05.9;O06;O06.0;O06.1;O06.2;O06.3;O06.4;O06.5;O06.6;O06.7;O06.8;O06.9;O07;O07.0;O07.1;O07.2;O07.3;O07.4;O07.5;O07.6;O07.7;O07.8;O07.9;O08;O08.0;O08.1;O08.2;O08.3;O08.4;O08.5;O08.6;O08.7;O08.8;O08.9;O10.0;O10.1;O10.2;O10.3;O10.4;O10.9;O11;O12;O12.0;O12.1;O12.2;O13;O14;O14.0;O14.1;O14.2;O14.9;O15;O15.0;O15.1;O15.2;O15.9;O16;O20;O20.0;O20.8;O20.9;O21;O21.0;O21.1;O21.2;O21.8;O21.9;O22;O22.0;O22.1;O22.2;O22.3;O22.4;O22.5;O22.8;O22.9;O23;O23.0;O23.1;O23.2;O23.3;O23.4;O23.5;O23.9;O25;O26;O26.0;O26.1;O26.2;O26.3;O26.4;O26.5;O26.6;O26.7;O26.8;O26.9;O28;O28.0;O28.1;O28.2;O28.3;O28.4;O28.5;O28.8;O28.9;O29;O29.0;O29.1;O29.2;O29.3;O29.4;O29.5;O29.6;O29.8;O29.9;O30;O30.0;O30.1;O30.2;O30.8;O30.9;O31;O31.0;O31.1;O31.2;O31.8;O32;O32.0;O32.1;O32.2;O32.3;O32.4;O32.5;O32.6;O32.8;O32.9;O33;O33.0;O33.1;O33.2;O33.3;O33.4;O33.5;O33.6;O33.7;O33.8;O33.9;O34;O34.0;O34.1;O34.2;O34.3;O34.4;O34.5;O34.6;O34.7;O34.8;O34.9;O35;O35.0;O35.1;O35.2;O35.3;O35.4;O35.5;O35.6;O35.7;O35.8;O35.9;O36;O36.0;O36.1;O36.2;O36.3;O36.4;O36.5;O36.6;O36.7;O36.8;O36.9;O40;O41;O41.0;O41.1;O41.8;O41.9;O42;O42.0;O42.1;O42.2;O42.9;O43;O43.0;O43.1;O43.2;O43.8;O43.9;O44;O44.0;O44.1;O45;O45.0;O45.8;O45.9;O46;O46.0;O46.8;O46.9;O47;O47.0;O47.1;O47.9;O48;O60;O60.0;O60.1;O60.2;O60.3;O61;O61.0;O61.1;O61.8;O61.9;O62;O62.0;O62.1;O62.2;O62.3;O62.4;O62.8;O62.9;O63;O63.0;O63.1;O63.2;O63.9;O64;O64.0;O64.1;O64.2;O64.3;O64.4;O64.5;O64.8;O64.9;O65;O65.0;O65.1;O65.2;O65.3;O65.4;O65.5;O65.8;O65.9;O66;O66.0;O66.1;O66.2;O66.3;O66.4;O66.5;O66.8;O66.9;O67;O67.0;O67.8;O67.9;O68;O68.0;O68.1;O68.2;O68.3;O68.8;O68.9;O69;O69.0;O69.1;O69.2;O69.3;O69.4;O69.5;O69.8;O69.9;O70;O70.0;O70.1;O70.2;O70.3;O70.9;O71;O71.0;O71.1;O71.2;O71.3;O71.4;O71.5;O71.6;O71.7;O71.8;O71.9;O72;O72.0;O72.1;O72.2;O72.3;O73;O73.0;O73.1;O74;O74.0;O74.1;O74.2;O74.3;O74.4;O74.5;O74.6;O74.7;O74.8;O74.9;O75;O75.0;O75.1;O75.2;O75.3;O75.4;O75.5;O75.6;O75.7;O75.8;O75.9;O80;O80.0;O80.1;O80.8;O80.9;O81;O81.0;O81.1;O81.2;O81.3;O81.4;O81.5;O82;O82.0;O82.1;O82.2;O82.8;O82.9;O83;O83.0;O83.1;O83.2;O83.3;O83.4;O83.8;O83.9;O84;O84.0;O84.1;O84.2;O84.8;O84.9;O85;O86;O86.0;O86.1;O86.2;O86.3;O86.4;O86.8;O87;O87.0;O87.1;O87.2;O87.3;O87.8;O87.9;O88;O88.0;O88.1;O88.2;O88.3;O88.8;O89;O89.0;O89.1;O89.2;O89.3;O89.4;O89.5;O89.6;O89.8;O89.9;O90;O90.0;O90.1;O90.2;O90.3;O90.4;O90.5;O90.8;O90.9;O91;O91.0;O91.1;O91.2;O92;O92.0;O92.1;O92.2;O92.3;O92.4;O92.5;O92.6;O92.7;O94;O95;O96;O96.0;O96.1;O96.9;O97;O97.0;O97.1;O97.9;O98;O98.0;O98.1;O98.2;O98.3;O98.4;O98.5;O98.6;O98.7;O98.8;O98.9;O99;O99.0;O99.1;O99.2;O99.3;O99.4;O99.5;O99.6;O99.7;O99.8;P00;P00.0;P00.1;P00.2;P00.3;P00.4;P00.5;P00.6;P00.7;P00.8;P00.9;P01;P01.0;P01.1;P01.2;P01.3;P01.4;P01.5;P01.6;P01.7;P01.8;P01.9;P02;P02.0;P02.1;P02.2;P02.3;P02.4;P02.5;P02.6;P02.7;P02.8;P02.9;P03;P03.0;P03.1;P03.2;P03.3;P03.4;P03.5;P03.6;P03.8;P03.9;P04;P04.0;P04.1;P04.2;P04.3;P04.4;P04.5;P04.6;P04.8;P04.9;P05;P05.0;P05.1;P05.2;P05.9;P07;P07.0;P07.1;P07.2;P07.3;P08;P08.0;P08.1;P08.2;P10;P10.0;P10.1;P10.2;P10.3;P10.4;P10.8;P10.9;P11;P11.0;P11.1;P11.2;P11.3;P11.4;P11.5;P11.9;P12;P12.0;P12.1;P12.2;P12.3;P12.4;P12.8;P12.9;P13;P13.0;P13.1;P13.2;P13.3;P13.4;P13.8;P13.9;P14;P14.0;P14.1;P14.2;P14.3;P14.8;P14.9;P15;P15.0;P15.1;P15.2;P15.3;P15.4;P15.5;P15.6;P15.8;P15.9;P20;P20.0;P20.1;P20.9;P21;P21.0;P21.1;P21.9;P22;P22.0;P22.1;P22.8;P22.9;P23;P23.0;P23.1;P23.2;P23.3;P23.4;P23.5;P23.6;P23.8;P23.9;P24;P24.0;P24.1;P24.2;P24.3;P24.8;P24.9;P25;P25.0;P25.1;P25.2;P25.3;P25.8;P26;P26.0;P26.1;P26.8;P26.9;P27;P27.0;P27.1;P27.8;P27.9;P28;P28.0;P28.1;P28.2;P28.3;P28.4;P28.5;P28.8;P28.9;P29;P29.0;P29.1;P29.2;P29.3;P29.4;P29.8;P29.9;P35;P35.0;P35.1;P35.2;P35.3;P35.4;P35.8;P35.9;P36;P36.0;P36.1;P36.2;P36.3;P36.4;P36.5;P36.8;P36.9;P37;P37.0;P37.1;P37.2;P37.3;P37.4;P37.5;P37.8;P37.9;P38;P39;P39.0;P39.1;P39.2;P39.3;P39.4;P39.8;P39.9;P50;P50.0;P50.1;P50.2;P50.3;P50.4;P50.5;P50.8;P50.9;P51;P51.0;P51.8;P51.9;P52;P52.0;P52.1;P52.2;P52.3;P52.4;P52.5;P52.6;P52.8;P52.9;P53;P54;P54.0;P54.1;P54.2;P54.3;P54.4;P54.5;P54.6;P54.8;P54.9;P55;P55.0;P55.1;P55.8;P55.9;P56;P56.0;P56.9;P57;P57.0;P57.8;P57.9;P58;P58.0;P58.1;P58.2;P58.3;P58.4;P58.5;P58.8;P58.9;P59;P59.0;P59.1;P59.2;P59.3;P59.8;P59.9;P60;P61;P61.0;P61.1;P61.2;P61.3;P61.4;P61.5;P61.6;P61.8;P61.9;P70;P70.0;P70.1;P70.2;P70.3;P70.4;P70.8;P70.9;P71;P71.0;P71.1;P71.2;P71.3;P71.4;P71.8;P71.9;P72;P72.0;P72.1;P72.2;P72.8;P72.9;P74;P74.0;P74.1;P74.2;P74.3;P74.4;P74.5;P74.8;P74.9;P75*;P76;P76.0;P76.1;P76.2;P76.8;P76.9;P77;P78;P78.0;P78.1;P78.2;P78.3;P78.8;P78.9;P80;P80.0;P80.8;P80.9;P81;P81.0;P81.8;P81.9;P83;P83.0;P83.1;P83.2;P83.3;P83.4;P83.5;P83.6;P83.8;P83.9;P90;P91;P91.0;P91.1;P91.2;P91.3;P91.4;P91.5;P91.6;P91.7;P91.8;P91.9;P92;P92.0;P92.1;P92.2;P92.3;P92.4;P92.5;P92.8;P92.9;P93;P94;P94.0;P94.1;P94.2;P94.8;P94.9;P95;P96;P96.0;P96.1;P96.2;P96.3;P96.4;P96.5;P96.8;P96.9;Z00.0;Z10;Z10.0;Z10.8;Z27.2;Z30;Z30.1;Z30.5\r\n";

//        //Linhist changed 20/03/26
//        //private readonly IEnumerable<Hospital> dmHospital;
//        private readonly TimePeriod _officialTime;

//        private readonly IXml01Repository _hosoRepository;
//        private readonly IEnumerable<DMYHDTICD10> dmYHCT;
//        private readonly IEnumerable<DMBacSi> dmBACSI;
//        //private readonly IEnumerable<DMKhoa> dmKHOA;
//        private readonly IEnumerable<DMKhoaBV> dmKHOABV;
//        private readonly IEnumerable<DMChung130> dmCHUNG;
//        private readonly Dictionary<string, string> dmDacBiet;
//        private readonly IEnumerable<DMError130> dmError130;
//        private readonly IEnumerable<DMErrorMHTC> _dMErrorMHTC;
//        private readonly ILogger<dynamic> _logger;

//        private XMLCommonInfo Info;

//        public ValidateXML01
//        (
//            //Linhist changed 20/03/26
//            //IEnumerable<Hospital> DmHospital,
//            TimePeriod officialTime,

//            IXml01Repository hosoRepository,
//            Dictionary<string, string> dMDacBiets,
//            IEnumerable<DMYHDTICD10> DMYhct,
//            IEnumerable<DMBacSi> DMBacSis,
//            IEnumerable<DMKhoaBV> DMKhoaBVs,
//            IEnumerable<DMChung130> DMChungs,
//            IEnumerable<DMError130> dMError130s,
//            IEnumerable<DMErrorMHTC> dMErrorMHTC,
//            ILogger<dynamic> logger = null
//        ) : base(logger)
//        {
//            //Linhist changed 20/03/26
//            //dmHospital = DmHospital;
//            _officialTime = officialTime;

//            _hosoRepository = hosoRepository;
//            dmYHCT = DMYhct;
//            dmBACSI = DMBacSis;
//            dmKHOABV = DMKhoaBVs;
//            dmCHUNG = DMChungs;
//            dmDacBiet = dMDacBiets;
//            dmError130 = dMError130s;
//            _dMErrorMHTC = dMErrorMHTC;
//            _logger = logger;
//        }

//        private static string[] SplitTimeToCheck(string timetoCheck)
//        {
//            if (string.IsNullOrEmpty(timetoCheck)) return Array.Empty<string>();

//            // cố gắng match chính xác [part0][part1]rest
//            var m = Regex.Match(timetoCheck, @"^\[([^\]]+)\]\[([^\]]+)\](.*)$");
//            if (m.Success)
//            {
//                return new[] { m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value.Trim() };
//            }

//            // fallback: lấy mọi phần trong ngoặc [] rồi phần còn lại
//            var matches = Regex.Matches(timetoCheck, @"\[([^\]]*)\]");
//            var list = matches.Cast<System.Text.RegularExpressions.Match>().Select(x => x.Groups[1].Value).ToList();
//            var lastIdx = timetoCheck.LastIndexOf(']');
//            if (lastIdx >= 0 && lastIdx < timetoCheck.Length - 1)
//            {
//                var tail = timetoCheck.Substring(lastIdx + 1).Trim();
//                if (!string.IsNullOrEmpty(tail)) list.Add(tail);
//            }

//            return list.ToArray();
//        }

//        private static bool IsDateInRange(DateTime dateToCheck, string dateRange)
//        {
//            // dateRange expected format "dd/MM-dd/MM" (ngày/tháng)
//            if (string.IsNullOrWhiteSpace(dateRange)) return false;
//            var parts = dateRange.Split('-', StringSplitOptions.RemoveEmptyEntries);
//            if (parts.Length != 2) return false;

//            if (!TryParseDayMonth(parts[0].Trim(), out int sd, out int sm)) return false;
//            if (!TryParseDayMonth(parts[1].Trim(), out int ed, out int em)) return false;

//            // build start/end year-aware
//            var year = dateToCheck.Year;
//            var start = new DateTime(year, sm, sd);
//            var end = new DateTime(year, em, ed);

//            // nếu end trước start => phạm vi bắc qua năm (ví dụ 01/09 - 30/05)
//            if (end < start) end = end.AddYears(1);

//            // chuẩn hóa dateToCheck cũng có thể phải shift nếu nằm đầu năm và range cross-year
//            var check = dateToCheck.Date;
//            if (check < start && end.Year > start.Year)
//            {
//                // có thể check thuộc phần sau năm (ví dụ date 2026-01-10 và start 2025-09-01)
//                check = check.AddYears(1);
//            }

//            return check >= start.Date && check <= end.Date;
//        }

//        private static bool TryParseDayMonth(string s, out int day, out int month)
//        {
//            day = 0; month = 0;
//            if (DateTime.TryParseExact(s, new[] { "d/M", "dd/MM", "d/MM", "dd/M" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
//            {
//                day = dt.Day;
//                month = dt.Month;
//                return true;
//            }
//            // fallback try split
//            var parts = s.Split('/', StringSplitOptions.RemoveEmptyEntries);
//            if (parts.Length == 2 && int.TryParse(parts[0], out day) && int.TryParse(parts[1], out month))
//            {
//                return true;
//            }
//            return false;
//        }

//        private static bool IsTimeInAnyRange(DateTime sessionTime, string schedule)
//        {
//            if (string.IsNullOrWhiteSpace(schedule)) return false;
//            var ranges = schedule.Split(';', StringSplitOptions.RemoveEmptyEntries);
//            foreach (var r in ranges)
//            {
//                var trimmed = r.Trim();
//                if (string.IsNullOrEmpty(trimmed)) continue;
//                var parts = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
//                if (parts.Length != 2) continue;
//                if (TryParseTimeOfDay(parts[0].Trim(), out TimeSpan start) && TryParseTimeOfDay(parts[1].Trim(), out TimeSpan end))
//                {
//                    var t = sessionTime.TimeOfDay;
//                    // inclusive boundaries: >= start && <= end
//                    if (t >= start && t <= end) return true;
//                }
//            }
//            return false;
//        }

//        private static bool TryParseTimeOfDay(string s, out TimeSpan ts)
//        {
//            ts = TimeSpan.Zero;
//            if (TimeSpan.TryParseExact(s, new[] { @"h\:mm", @"hh\:mm", @"m\:ss" }, CultureInfo.InvariantCulture, out ts))
//            {
//                return true;
//            }
//            // hỗ trợ "H:mm" dạng 7:30
//            if (DateTime.TryParseExact(s, new[] { "H:mm", "HH:mm", "H:m" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
//            {
//                ts = dt.TimeOfDay;
//                return true;
//            }
//            // fallback: try TimeSpan.Parse
//            return TimeSpan.TryParse(s, out ts);
//        }

//        private  bool Check_TheTE_ChuaDiHoc(string maThe, DateTime? ngaySinh)
//        {
//            if (string.IsNullOrEmpty(maThe) || !maThe.StartsWith("TE1")) return false;
//            if (ngaySinh == null || !ngaySinh.HasValue) return false;

//            double tuoiBN = (double)((DateTime.Now - ngaySinh.Value).Days / 365.242199);
//            DateTime dMax = new DateTime(DateTime.Now.Year, 9, 30).AddDays(1);

//            if(DateTime.Now.Year-ngaySinh.Value.Year <=6)
//            {
//                if (tuoiBN < 6 || (tuoiBN < 7 && DateTime.Now < dMax)) return true;
//            }
//            else
//            {
//                if (tuoiBN < 6) return true;
//            }
//            //if (tuoiBN < 6 || (tuoiBN < 7 && DateTime.Now < dMax)) return true;
            
//            return false;
//        }

//        public bool isOfficeTime(string TimetoCheck, DateTime sessionTime)
//        {
//            if (string.IsNullOrWhiteSpace(TimetoCheck)) return false;

//            var parts = SplitTimeToCheck(TimetoCheck);
//            // Nếu chỉ có 1 phần trong ngoặc, áp dụng quanh năm
//            if (parts.Length == 1)
//            {
//                var schedule = parts[0].Trim();
//                // special-case áp dụng quanh năm
//                if (string.Equals(schedule, "6:30-11:30;13:00-16:00", StringComparison.OrdinalIgnoreCase))
//                {
//                    return IsTimeInAnyRange(sessionTime, schedule);
//                }

//                // Một bộ giờ áp dụng quanh năm
//                return IsTimeInAnyRange(sessionTime, schedule);
//            }

//            // Nếu có 3 phần: parts[0], parts[1], parts[2] (date range)
//            if (parts.Length >= 3)
//            {
//                var summerSchedule = parts[0].Trim();
//                var winterSchedule = parts[1].Trim();
//                var dateRange = parts[2].Trim();

//                // Nếu summerSchedule là special-case áp dụng quanh năm => xử lý ngay
//                if (string.Equals(summerSchedule, "6:30-11:30;13:00-16:00", StringComparison.OrdinalIgnoreCase))
//                {
//                    return IsTimeInAnyRange(sessionTime, summerSchedule);
//                }

//                var inSummer = IsDateInRange(sessionTime.Date, dateRange);
//                return IsTimeInAnyRange(sessionTime, inSummer ? summerSchedule : winterSchedule);
//            }

//            // fallback: không parse được
//            return false;
//            //if (!String.IsNullOrEmpty(TimetoCheck))
//            //{
//            //    if (checkString(TimetoCheck))
//            //    {
//            //        string[] parts = TimetoCheck.Split(new string[] { "][" }, StringSplitOptions.None);
//            //        // Remove the leading '[' from the first part and the trailing ']' from the last part
//            //        parts[0] = parts[0].TrimStart('[');
//            //        parts[2] = parts[2].TrimEnd(']');
//            //        // Kiểm tra là giờ mùa hè
//            //        if (checkSessionTime(sessionTime, parts[2]) == "1")
//            //        {
//            //            //Đúng giờ mùa hè kiểm tra có intime hay không?
//            //            if (checkTimepan(sessionTime, parts[0]) == "1")
//            //            {
//            //                return true;
//            //            }
//            //            else return false;
//            //        }
//            //        else
//            //        {
//            //            //Giờ mùa đông kiểm tra xem có intime hay không?
//            //            if (checkTimepan(sessionTime, parts[1]) == "1")
//            //            {
//            //                return true;
//            //            }
//            //            else return false;
//            //        }

//            //    }
//            //    else
//            //    {
//            //        string[] parts = TimetoCheck.Split(new string[] { "][" }, StringSplitOptions.None);
//            //        // Remove the leading '[' from the first part and the trailing ']' from the last part
//            //        parts[0] = parts[0].TrimStart('[');
//            //        parts[0] = parts[0].TrimEnd(']');
//            //        if (checkTimepan(sessionTime, parts[0]) == "1")
//            //        {
//            //            return true;
//            //        }
//            //        else return false;
//            //    }
//            //}

//            //return false;
//        }

//        public  bool checkString(string TimetoCheck)
//        {
//            string[] parts = TimetoCheck.Split(new string[] { "][" }, StringSplitOptions.None);
//            return parts.Length == 3;
//        }

//        public  string checkTimepan(DateTime sessionTime, string partTime)
//        {
//            TimeSpan timecheck = sessionTime.TimeOfDay;
//            string[] rangers = partTime.Split(";");
//            foreach (var ranger in rangers)
//            {
//                string[] time = ranger.Split("-");
//                TimeSpan startTime = TimeSpan.Parse(time[0]);
//                TimeSpan endTime = TimeSpan.Parse(time[1]);
//                if (timecheck >= startTime && timecheck <= endTime)
//                {
//                    return "1"; // onTime
//                }
//            }
//            return "-1"; // out time
//        }

//        public  string checkSessionTime(DateTime sessionTime, string partTime)
//        {
//            string[] dates = partTime.Split('-');

//            // Extract day and month for the start date
//            string[] startDateParts = dates[0].Split('/');
//            int startDay = int.Parse(startDateParts[0]);
//            int startMonth = int.Parse(startDateParts[1]);

//            // Extract day and month for the end date
//            string[] endDateParts = dates[1].Split('/');
//            int endDay = int.Parse(endDateParts[0]);
//            int endMonth = int.Parse(endDateParts[1]);

//            // Create DateTime objects (using year 1 as a placeholder)
//            DateTime startDate = new DateTime(1, startMonth, startDay);
//            DateTime endDate = new DateTime(1, endMonth, endDay);

//            DateTime checkedDate = new DateTime(1, sessionTime.Month, sessionTime.Day);
//            if (checkedDate >= startDate && checkedDate <= endDate)
//            {
//                return "1"; // Giờ mùa hè
//            }
//            if (checkedDate < startDate || checkedDate > endDate)
//            {
//                return "2"; // Giờ mùa đông
//            }
//            return "3";
//        }
//        public async Task<bool> Validate(XML01 xml, XMLCommonInfo info)
//        {
//            Info = info;
//            return await Validate(xml);
//        }

//        public override async Task<bool> Validate(XML01 xml)
//        {
//            try
//            {
//                // Kiem tra neu XML null thi ko chay tiep nua
//                if (!await base.Validate(xml)) return false;

//                //Stopwatch _watch = Stopwatch.StartNew();
//                //AddNewLog($"Bat dau giam dinh ho so XML01");

//                //MA_LOAI_KCB QĐ 824 BYT
//                //1 Khám Bệnh
//                //2 Ngoại trú
//                //3 Nội trú
//                //4 Nội trú ban ngày
//                //5 Ngoại trú các bệnh mãn tính dài ngày, khám bệnh và lấy thuốc
//                //6 Điều trị lưu tại Trạm y tế xã, phòng khám đa khoa khu vực
//                //7 Nhận thuốc theo hẹn
//                //8 Ngoại trú các bệnh mãn tính dài ngày, khám bệnh và lấy thuốc, có thực hiện các dịch vụ kỹ thuật
//                //9 Điều trị nội trú dưới 4h
//                //10 các TH khác

//                //MA_DOITUONG_KCB QĐ 824 BYT
//                //1 Đúng tuyến
//                //1.1 Người tham gia bảo hiểm y tế đến khám bệnh, chữa bệnh đúng cơ sở khám bệnh, chữa bệnh ban đầu.
//                //1.2 Người tham gia bảo hiểm y tế đăng ký khám bệnh, chữa bệnh ban đầu tại cơ sở khám bệnh, chữa bệnh tuyến Xã hoặc Phòng khám đa khoa hoặc Bệnh viện tuyến Huyện được quyền khám bệnh, chữa bệnh bảo hiểm y tế tại Trạm y tế tuyến xã hoặc Phòng khám đa khoa hoặc Bệnh viện tuyến huyện khác trong cùng địa bàn tỉnh
//                //1.3 Người tham gia bảo hiểm y tế được chuyển tuyến khám bệnh, chữa bệnh bảo hiểm y tế theo quy định
//                //1.4 Người tham gia bảo hiểm y tế có giấy tờ chứng minh đang ở tại địa phương khác trong thời gian đi công tác, làm việc lưu động, học tập trung theo các hình thức đào tạo, chương trình đào tạo, tạm trú và khám bệnh, chữa bệnh ban đầu tại cơ sở khám bệnh, chữa bệnh cùng tuyến hoặc tương đương với cơ sở đăng ký khám bệnh,
//                //1.5 Người có giấy hẹn khám lại trong trường hợp đã được chuyển tuyến
//                //1.6 Người đã hiến bộ phận cơ thể của mình phải điều trị ngay sau khi hiến bộ phận cơ thể.
//                //1.7 Trẻ sơ sinh phải điều trị1 ngay sau khi sinh ra.
//                //1.8 Các trường hợp KBCB theo quy định đối với người mắc bệnh lao theo quy định tại
//                //1.9 Các trường hợp khám bệnh, chữa bệnh theo quy định đối với người mắc bệnh HIV/AIDS.
//                //1.10 Các trường hợp khám bệnh, chữa bệnh theo quy định đối với người mắc bệnh COVID - 19.
//                //1.11
//                //1.12
//                //1.13
//                //1.14
//                //1.15
//                //1.16
//                //1.17
//                //1.18
//                //2. Cấp cứu
//                //3. Trái tuyến
//                //3.1 Các trường hợp tự đi khám bệnh, chữa bệnh được chỉ định điều trị nội trú, nội trú ban ngày tại các bệnh viện tuyến trung ương
//                // (Bỏ)3.2 Các trường hợp tự đi khám bệnh, chữa bệnh được chỉ định điều trị nội trú, nội trú ban ngày tại các bệnh viện tuyến tỉnh
//                //3.3 Các trường hợp tự đi khám bệnh, chữa bệnh tại các bệnh viện tuyến huyện trên phạm vi cả nước
//                //3.4(Bỏ) Trường hợp tự đi khám bệnh, chữa bệnh ngoại trú tại cơ sở khám bệnh, chữa bệnh tuyến trung ương (mức hưởng 0%, hay nói cách khác: quỹ bảo hiểm y tế không thanh toán chi phí khám bệnh, chữa bệnh bảo hiểm y
//                //3.5 (Bỏ) Trường hợp tự đi khám bệnh, chữa bệnh ngoại trú tại cơ sở khám bệnh, chữa bệnh tuyến tỉnh (mức hưởng 0%, hay nói cách khác: quỹ bảo hiểm y tế không thanh toán chi phí khám bệnh, chữa bệnh bảo hiểm y tế
//                //3.6 
//                //3.7 (Bỏ)
//                //7. Lĩnh thuốc theo giấy hẹn trong trường hợp dịch bệnh hoặc bất khả kháng hoặc do bất kỳ nguyên nhân nào (mức hưởng 100% theo phạm vi quyền lợi mức hưởng)
//                //7.1
//                //7.2
//                //7.2
//                //7.4
//                //8 Thu hồi đề nghị thanh 8
//                //9 Khám dịch vụ
//                //"1";"1.1";"1.2";"1.3";"1.4";"1.5";"1.6";"1.7";"1.8";"1.9";"1.10";"2";"3";"3.1";"3.2";"3.3";"3.4";"3.5";"3.6";"3.7";"7";"7.1";"7.2";"7.3";"7.4";"8";"9"

//                //Bệnh nhẹ kê hồi sức cấp cứu: P59;P59.9;D64;E87.6;A04.9;I79.1*;R40.2;T78.2;J96.0;J16.8;R17;J44.1;I50;R10;I64;T46.5;I61;X45;J81;J18.0;T88.6;R57.2;T40.6;D64.9;P22;T78.4;K92.2;I27.8;I10;T43.2;Z93.0;I63;T62;R00.1;K29;R00.0;J18;F44;K85;X44;Y18;N18.5;K25;R56;P36;C34;E16.1;
//                // Giường hồi sức CC: K02.1907;K02.1907.1;K02.1953;K02.1953.1;K03.1906;K04.1905;K04.1906;K07.1907.1;K08.1904;K08.1905;K09.1906.1;K11.1905;K12.1907;K13.1906;K14.1905.1;K16.1905;K16.1905;K16.1906.1;K18.1904;K18.1905;K20.1905;K20.1906.1;K21.1904;K21.1907.1;K22.1904;K22.1907;K23.1905;K24.1904;K24.1905;K27.1905;K27.1906;K27.1906;K28.1904;K28.1906.1;K29.1905;K29.1906;K31.1905;K33.1904;K02.1906;K02.1907.10;K02.1907.7;K02.1907.8;K02.1908;K03.1906.18;K05.1905;K06.1906;K06.1907.6;K07.1904;K07.1907.7;K08.1905;K11.1904;K11.1907;K11.1907.7;K12.1907.1;K14.1905;K14.1907;K14.1907.1;K16.1906;K18.1905;K18.1906;K18.1907.1;K19.1905.2;K19.1906.1;K19.1907.1;K21.1905;K21.1906.1;K23.1905.1;K23.1906.1;K27.1906;K27.1907;K27.1907.1;K29.1905;K31.1904;K31.1905;K33.1905.1;K36.1906;K36.1906.2;K02.1905;K02.1906;K02.1906;K02.1906.18;K02.1907.11;K02.1907.17;K02.1907.4;K02.1952.1;K03.1904;K03.1905;K03.1905;K03.1905;K03.1907.1;K04.1905;K04.1907.1;K04.1907.8;K05.1904;K05.1907.7;K07.1905;K08.1907.7;K09.1907;K11.1905;K13.1907;K14.1905;K14.1906.1;K15.1906;K15.1907.1;K16.1905.1;K16.1907.1;K18.1907;K24.1906;K24.1907;K24.1907.1;K25.1906;K27.1906;K27.1906.1;K29.1906.1;K31.1906;K33.1905;K36.1906.1;K36.1907.1;K49.1905;K49.1906;K49.1907;K33.1906.1;K33.1906.2;K36.1907;K48.1904;K48.1905;K48.1905;K48.1906;K48.1906;K49.1905;K50.1904;K50.1907.1;K02.1906;K02.1906;K02.1907.13;K02.1907.20;K02.1907.5;K03.1905;K03.1906.1;K04.1904;K05.1907.1;K06.1906.1;K06.1907;K06.1907.1;K07.1905;K07.1906;K09.1904;K09.1907.1;K12.1904;K12.1906.1;K15.1906.1;K16.1907;K18.1906;K18.1906.1;K19.1907;K22.1907.1;K23.1905;K27.1904;K27.1906.18;K28.1906;K30.1907.1;K31.1906.1;K31.1907;K33.1905;K33.1906;K36.1904;K36.1905;K48.1907;K49.1906.1;K49.1907.1;K50.1906.1;K50.1906.2;K02.1906;K02.1906.1;K02.1907.12;K02.1907.18;K02.1907.19;K02.1907.3;K04.1906.1;K06.1904;K06.1905;K06.1905;K07.1907;K08.1906;K08.1907;K13.1905;K13.1906.1;K13.1907.1;K14.1904;K14.1906;K15.1904;K18.1906.18;K19.1905.1;K19.1906.2;K19.1907.5;K22.1906.1;K23.1905.2;K23.1907;K24.1905;K24.1906;K25.1904;K25.1907;K28.1905;K28.1906.2;K30.1905;K30.1907;K31.1907.1;K33.1907.1;K49.1904;K02.1904;K02.1907.16;K02.1907.9;K02.1951;K02.1952;K03.1906;K03.1907;K03.1907;K05.1905;K05.1906;K05.1907;K11.1906;K11.1906.1;K13.1905;K13.1906.2;K13.1907.4;K15.1907.3;K16.1904;K18.1907.5;K19.1906;K20.1904;K20.1905.2;K20.1906;K20.1907;K20.1907.1;K20.1907.5;K23.1906;K23.1907.1;K24.1906.1;K27.1905;K27.1905;K29.1907;K30.1906;K33.1907;K48.1906.1;K50.1906;;;K02.1905;K02.1906.10;K02.1907.14;K02.1907.15;K02.1907.6;K04.1906;K04.1907;K08.1906.1;K08.1907.1;K09.1905;K12.1906.2;K13.1904;K15.1905;K19.1905;K19.1905;K20.1905.1;K21.1906.2;K21.1907;K22.1906;K23.1906.2;K25.1907.1;K27.1906;K27.1906;K27.1906.2;K28.1907;K28.1907.1;K29.1907.1;K30.1904;K30.1905;K31.1906.2;K49.1904;K49.1906.2;K02.1906.2;K02.1907.2;K03.1907;K03.1907.7;K05.1906.1;K07.1906.1;K09.1906;K09.1907.7;K11.1907.1;K12.1905;K12.1906;K14.1907.3;K15.1907;K16.1907.5;K18.1905;K18.1905;K19.1904;K19.1906.19;K21.1906;K21.1907.5;K22.1905;K23.1904;K25.1905;K25.1906.1;K28.1905;K29.1904;K30.1906.1;K48.1906.2;K48.1907.1;K50.1904;K50.1905;K50.1905;K50.1906;K50.1907;
//                //21/9/2025 Bigupdate XMLerror
//                //ErrorDecs
//                //Coding for XML1

//                //string ErrDecs = "";

//                //26/9/2025 Khai báo Biến toàn cục

//                var errorlist = dmError130.ToDictionary(x => x.ErrorCode, x => x.Message);
//                var benhchinhInfo = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH)?.TEN ?? "Không có thông tin bệnh";

//                #region Chuyên đề bảo hiểm BHXH Việt Nam
//                //4046	4	XML1	MA_LOAI_KCB	[3176] Chuyên đề Bảo hiểm KT144: Không thanh toán DVKT tiêm bắp, tiêm tĩnh mạch, truyền tĩnh mạch, tháo bột, cắt chỉ, thay băng trong điều trị nội trú
//                //Linhist changed 16/07/26
//                //if(File.XML03s.Any(x=> Uti.HCodeContain(x.MA_DICH_VU, "15.0303.0200;10.9003.0200;10.9003.0200_BS;03.3911.0200;07.0225.0200")))
//                if ((File.MA_LOAI_KCB == "03" || File.MA_LOAI_KCB == "09") && File.XML03s.Any(x=> Uti.HCodeContain(x.MA_DICH_VU, "15.0303.0200;10.9003.0200;10.9003.0200_BS;03.3911.0200;07.0225.0200")))
//                {
//                    RaiseError(true,
//                                4046,
//                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(4046, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Hồ sơ có chứa dịch vụ kỹ thuật thuộc danh mục DVKT tiêm bắp, tiêm tĩnh mạch, truyền tĩnh mạch, tháo bột, cắt chỉ, thay băng trong điều trị nội trú theo chuyên đề Bảo hiểm KT144 của BHXH Việt Nam, đề nghị kiểm tra lại"
//                        );
//                }    
//                #endregion
//                #region Update code từ 4210 qua
//                //3796	3	XML1	MA_DOITUONG_KCB	[Bắc Ninh CDC] Mã đối tượng KCB không nằm trong phạm vi của Đơn vị

//                if (File.MA_DOITUONG_KCB != "1.1" && File.MA_DOITUONG_KCB != "1.2")
//                {
//                    RaiseError(true,
//                            3796,
//                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                            $"Nội dung lỗi: {(errorlist.TryGetValue(3796, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                            $"Chi tiết:" +
//                            $"Bệnh nhân có mã đối tượng KCB:{File.MA_DOITUONG_KCB}, không nằm trong phạm vi KCB của Bệnh viện, kiểm tra lại"
//                        );
//                }


//                //3785	3	XML1	NGAY_TTOAN	[3176] Ngày thanh toán đang lớn hơn ngày tạo, kiểm tra lại
//                if (File.NGAY_LAP != null && File.NGAY_TTOAN != null)
//                {
//                    if (File.NGAY_LAP < File.NGAY_TTOAN)
//                    {
//                        RaiseError(true,
//                                3785,
//                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3785, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân khởi tạo hồ sơ đề nghị thanh toán vào lúc: {File.NGAY_LAP.Value.ToString("dd/MM/yyyy HH:mm")}, tuy nhiên ngày Thanh toán lại là:{File.NGAY_TTOAN.Value.ToString("dd/MM/yyyyy HH:mm")} không hợp lý đề nghị kiểm tra lại"
//                            );
//                    }
//                }
//                //3845	3	XML1	MA_DOITUONG_KCB	[3176] Mã đối tượng KCB không phù hợp theo Quyết định 3276/QĐ-BYT 2025
//                if (File.MA_DOITUONG_KCB == "1.1" && !File.MA_DKBD.StartsWith(File.MA_CSKCB))
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD} khác nhau, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                if (File.MA_DOITUONG_KCB == "1.3" && (File.MA_DKBD.StartsWith(File.MA_CSKCB) || String.IsNullOrEmpty(File.GIAY_CHUYEN_TUYEN)))
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD} khác nhau, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                if ((File.MA_DOITUONG_KCB == "1.4" || File.MA_DOITUONG_KCB == "1.11") && File.MA_DKBD.StartsWith(File.MA_CSKCB))
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD} trùng nhau, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                //3846	3	XML1	MA_DOITUONG_KCB	[3176] Thái Bình: Mã đối tượng KCB không phù hợp theo Quyết định 3276/QĐ-BYT 2025
//                if ((File.MA_DOITUONG_KCB == "3.1" || File.MA_DOITUONG_KCB == "3.2") && File.MA_DKBD.StartsWith(File.MA_CSKCB))
//                {
//                    RaiseError(true,
//                        3846,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3846, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD} trùng nhau, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                if (File.MA_DOITUONG_KCB == "1.5" && (File.MA_DKBD.StartsWith(File.MA_CSKCB) || String.IsNullOrEmpty(File.GIAY_CHUYEN_TUYEN)))
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD}, Giấy chuyển tuyến:{File.GIAY_CHUYEN_TUYEN}, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );

//                }
//                if (File.MA_DOITUONG_KCB == "1.7" && Info.NGAY_TUOI_BN > 28)
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD}, Bệnh nhân {Info.NGAY_TUOI_BN} >28 ngày tuổi, không phải trẻ sơ sinh, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                if ((File.MA_DOITUONG_KCB == "1.13" || File.MA_DOITUONG_KCB == "1.14") && (File.MA_DKBD.StartsWith(File.MA_CSKCB) || !"02;05;08".Contains(File.MA_LOAI_KCB)))
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD}, Mã loại KCB: {File.MA_LOAI_KCB}, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                if (File.MA_DOITUONG_KCB == "1.15" && (File.MA_DKBD.StartsWith(File.MA_CSKCB) || !"03;04;09".Contains(File.MA_LOAI_KCB)))
//                {
//                    RaiseError(true,
//                        3845,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3845, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân có mã Đăng ký KCB:{File.MA_CSKCB} và Mã Đăng ký Ban đầu:{File.MA_DKBD}, Mã loại KCB: {File.MA_LOAI_KCB}, Mã đối tượng KCB:{File.MA_DOITUONG_KCB} không đúng với hướng dẫn tại QĐ 3276/QĐ-BYT năm 2025"
//                                );
//                }
//                //3776	4	XML1	NGAY_RA	[3176] - Chuyên đề bảo hiểm: Ngày ra viện không trùng với ngày vào viện (Trường hợp BN đi khám viện)
//                if (File.MA_LOAI_KCB == "01")
//                {
//                    if (File.NGAY_RA != null && File.NGAY_VAO != null)
//                    {
//                        if (File.NGAY_RA.Value.Date != File.NGAY_VAO.Value.Date)
//                        {
//                            RaiseError(true,
//                                3776,
//                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                $"Nội dung lỗi: {(errorlist.TryGetValue(3776, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                $"Chi tiết:" +
//                                $"Bệnh nhân khám bệnh, có Mã Loại KCB:{File.MA_LOAI_KCB}, có ngày vào viện: {File.NGAY_VAO.Value.ToString("dd/MM/yyyy HH:mm")}, ngày ra viện: {File.NGAY_RA.Value.ToString("dd/MM/yyyy HH:mm")} là không hợp lý, đề nghị kiểm tra lại"
//                            );
//                        }
//                    }
//                }
//                //3342    3   XML1 MA_BENH_CHINH[3176] Bắc Giang: Kê giường hồi sức cấp cứu cho Bệnh nhân Nội trú chưa phù hợp với mức độ, trình trạng bệnh
//                //[3176] Bắc Giang: Kê giường hồi sức cấp cứu cho Bệnh nhân Nội trú chưa phù hợp với mức độ, trình trạng bệnh (Chuyên đề bảo hiểm KT35)
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 3442))
//                {
//                    if (File.MA_LOAI_KCB == "03")
//                    {
//                        if (Uti.HCodeContain(File.MA_BENH_CHINH, "P59;P59.9;D64;E87.6;A04.9;I79.1*;R40.2;T78.2;J96.0;J16.8;R17;J44.1;I50;R10;I64;T46.5;I61;X45;J81;J18.0;T88.6;R57.2;T40.6;D64.9;P22;T78.4;K92.2;I27.8;I10;T43.2;Z93.0;I63;T62;R00.1;K29;R00.0;J18;F44;K85;X44;Y18;N18.5;K25;R56;P36;C34;E16.1;") && File.XML03s.Any(x => Uti.HCodeContain(x.MA_DICH_VU, "K02.1907;K02.1907.1;K02.1953;K02.1953.1;K03.1906;K04.1905;K04.1906;K07.1907.1;K08.1904;K08.1905;K09.1906.1;K11.1905;K12.1907;K13.1906;K14.1905.1;K16.1905;K16.1905;K16.1906.1;K18.1904;K18.1905;K20.1905;K20.1906.1;K21.1904;K21.1907.1;K22.1904;K22.1907;K23.1905;K24.1904;K24.1905;K27.1905;K27.1906;K27.1906;K28.1904;K28.1906.1;K29.1905;K29.1906;K31.1905;K33.1904;K02.1906;K02.1907.10;K02.1907.7;K02.1907.8;K02.1908;K03.1906.18;K05.1905;K06.1906;K06.1907.6;K07.1904;K07.1907.7;K08.1905;K11.1904;K11.1907;K11.1907.7;K12.1907.1;K14.1905;K14.1907;K14.1907.1;K16.1906;K18.1905;K18.1906;K18.1907.1;K19.1905.2;K19.1906.1;K19.1907.1;K21.1905;K21.1906.1;K23.1905.1;K23.1906.1;K27.1906;K27.1907;K27.1907.1;K29.1905;K31.1904;K31.1905;K33.1905.1;K36.1906;K36.1906.2;K02.1905;K02.1906;K02.1906;K02.1906.18;K02.1907.11;K02.1907.17;K02.1907.4;K02.1952.1;K03.1904;K03.1905;K03.1905;K03.1905;K03.1907.1;K04.1905;K04.1907.1;K04.1907.8;K05.1904;K05.1907.7;K07.1905;K08.1907.7;K09.1907;K11.1905;K13.1907;K14.1905;K14.1906.1;K15.1906;K15.1907.1;K16.1905.1;K16.1907.1;K18.1907;K24.1906;K24.1907;K24.1907.1;K25.1906;K27.1906;K27.1906.1;K29.1906.1;K31.1906;K33.1905;K36.1906.1;K36.1907.1;K49.1905;K49.1906;K49.1907;K33.1906.1;K33.1906.2;K36.1907;K48.1904;K48.1905;K48.1905;K48.1906;K48.1906;K49.1905;K50.1904;K50.1907.1;K02.1906;K02.1906;K02.1907.13;K02.1907.20;K02.1907.5;K03.1905;K03.1906.1;K04.1904;K05.1907.1;K06.1906.1;K06.1907;K06.1907.1;K07.1905;K07.1906;K09.1904;K09.1907.1;K12.1904;K12.1906.1;K15.1906.1;K16.1907;K18.1906;K18.1906.1;K19.1907;K22.1907.1;K23.1905;K27.1904;K27.1906.18;K28.1906;K30.1907.1;K31.1906.1;K31.1907;K33.1905;K33.1906;K36.1904;K36.1905;K48.1907;K49.1906.1;K49.1907.1;K50.1906.1;K50.1906.2;K02.1906;K02.1906.1;K02.1907.12;K02.1907.18;K02.1907.19;K02.1907.3;K04.1906.1;K06.1904;K06.1905;K06.1905;K07.1907;K08.1906;K08.1907;K13.1905;K13.1906.1;K13.1907.1;K14.1904;K14.1906;K15.1904;K18.1906.18;K19.1905.1;K19.1906.2;K19.1907.5;K22.1906.1;K23.1905.2;K23.1907;K24.1905;K24.1906;K25.1904;K25.1907;K28.1905;K28.1906.2;K30.1905;K30.1907;K31.1907.1;K33.1907.1;K49.1904;K02.1904;K02.1907.16;K02.1907.9;K02.1951;K02.1952;K03.1906;K03.1907;K03.1907;K05.1905;K05.1906;K05.1907;K11.1906;K11.1906.1;K13.1905;K13.1906.2;K13.1907.4;K15.1907.3;K16.1904;K18.1907.5;K19.1906;K20.1904;K20.1905.2;K20.1906;K20.1907;K20.1907.1;K20.1907.5;K23.1906;K23.1907.1;K24.1906.1;K27.1905;K27.1905;K29.1907;K30.1906;K33.1907;K48.1906.1;K50.1906;;;K02.1905;K02.1906.10;K02.1907.14;K02.1907.15;K02.1907.6;K04.1906;K04.1907;K08.1906.1;K08.1907.1;K09.1905;K12.1906.2;K13.1904;K15.1905;K19.1905;K19.1905;K20.1905.1;K21.1906.2;K21.1907;K22.1906;K23.1906.2;K25.1907.1;K27.1906;K27.1906;K27.1906.2;K28.1907;K28.1907.1;K29.1907.1;K30.1904;K30.1905;K31.1906.2;K49.1904;K49.1906.2;K02.1906.2;K02.1907.2;K03.1907;K03.1907.7;K05.1906.1;K07.1906.1;K09.1906;K09.1907.7;K11.1907.1;K12.1905;K12.1906;K14.1907.3;K15.1907;K16.1907.5;K18.1905;K18.1905;K19.1904;K19.1906.19;K21.1906;K21.1907.5;K22.1905;K23.1904;K25.1905;K25.1906.1;K28.1905;K29.1904;K30.1906.1;K48.1906.2;K48.1907.1;K50.1904;K50.1905;K50.1905;K50.1906;K50.1907;")))
//                        {
//                            //string errorInfo =errorlist.TryGetValue(3342, out errorInfo) ? errorInfo : "Không tìm thấy nội dung lỗi";
//                            //string desc = "Bệnh thuộc nhóm bệnh nhẹ kê giường hồi sức cấp cứu (Chuyên đề Bảo hiểm KT35). Vui lòng kiểm tra lại.";
//                            //RaiseError(true, 3342,Uti.getErrorDescription(File.MA_BN,File.HO_TEN,"3342",errorInfo,File.MA_BENH_CHINH,benhchinhInfo,desc));

//                            RaiseError(
//                                    true,
//                                    3342,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3342, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết:" +
//                                        "Bệnh thuộc nhóm bệnh nhẹ kê giường hồi sức cấp cứu (Chuyên đề Bảo hiểm KT35). Vui lòng kiểm tra lại."
//                                    );


//                        }
//                    }
//                }
//                //3328	3	XML3	MA_DICH_VU	[3176] Chuyên đề bảo hiểm: Phát sinh chi phí công khám đối với các trường hợp nhận thuốc theo lịch hẹn (Mã loại khám chữa bệnh = 7) Quyết định số 824/QĐ-BYT ngày 15 tháng 02 năm 2023
//                if (Uti.HCodeContain(File.MA_LOAI_KCB, "07"))
//                {
//                    if (File.XML03s.Any(x => x.MA_NHOM == 13))
//                    {
//                        //string errorInfo = errorlist.TryGetValue(3328, out errorInfo) ? errorInfo : "Không tìm thấy nội dung lỗi";
//                        //string desc = "Phát sinh chi phí công khám đối với các trường hợp nhận thuốc theo lịch hẹn. Căn cứ Quyết định số 824/QĐ-BYT ngày 15 tháng 02 năm 2023";
//                        //RaiseError(true, 3228, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "3228", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(true,
//                            3328,
//                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3328, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết:" +
//                                        "Bệnh nhân này đến nhận thuốc theo lịch hẹn (Mã Loại KCB=7) nên không được phát sinh công khám (Căn cứ quyết định 824/QĐ BYT ngày 15/02/2023."
//                            );

//                    }
//                }
//                //3732	3	XML1	MA_BENH_KT	[3176] Khám nghĩa vụ quân sự không thanh toán tiền BHYT (Z00)
//                if (File.MA_BENH_KT.Contains("Z00"))
//                {
//                    RaiseError(
//                                    true,
//                                    3732,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3732, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết:" +
//                                        $"{File.MA_BENH_KT}, có mã Bệnh Z00 thuộc nhóm bệnh khám nghĩa vụ quân sự, không thanh toán Bảo hiểm Y Tế"
//                                    );
//                }
//                //418 Hồ sơ khám sức khỏe không được thanh toán BHYT
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 418))
//                {
//                    if (Uti.HCodeContain(File.MA_BENH_CHINH, "Z00.*;Z01.*;Z02.*;Z10.0;Z10.1"))
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 418);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = "Thuộc nhóm bệnh đi khám sức khỏe ICD10, không thanh toán Bảo hiểm Y Tế";
//                        //RaiseError(true, 418, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "418", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(
//                                        true,
//                                        418,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(418, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết:" +
//                                            $"{File.MA_BENH_CHINH} thuộc nhóm bệnh đi khám sức khỏe ICD10, không thanh toán Bảo hiểm Y Tế"
//                                        );

//                    }
//                }
//                //3283	4	XML1	MA_BENH_CHINH	[3176] Chi phí đề nghị TT ngoài phạm vi chi trả BHYT (của HSBA có mã chẩn đoán) Z30 theo Luật BHYT 2008
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 3283))
//                {
//                    if (Uti.CodeContain(File.MA_BENH_CHINH, "Z30.*"))
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3283);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = "Chi phí đề nghị thanh toán cho Bệnh nằm ngoài pham vi chi trả BHYT. Căn cứ Luật bảo hiểm y tế năm 2018";
//                        //RaiseError(true, 3283, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "3283", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(
//                                        true,
//                                        3283,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(3283, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết:" +
//                                            $"Chi phí đề nghị thanh toán cho Bệnh nằm ngoài pham vi chi trả BHYT. Căn cứ Luật bảo hiểm y tế năm 2018"
//                                        );

//                    }
//                }
//                //3284	4	XML1	MA_BENH_CHINH	[3176]  Sử dụng kỹ thuật hỗ trợ sinh sản, dịch vụ kế hoạch hóa gia đình, nạo hút thai, phá thai ngoài phạm vi thanh toán của BHYT
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 3284))
//                {
//                    if (Uti.CodeContain(File.MA_BENH_CHINH, "Z31.*;O04.4;O05.2;O05.3;O05.4;O05.9;O06.2;O06.3;O06.4;O06.8;O06.9;O07.2;O07.4;O07.9"))
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3284);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = "Sử dụng kỹ thuật hỗ trợ sinh sản, dịch vụ kế hoạch hóa gia đình, nạo hút thai, phá thai ngoài phạm vi thanh toán của BHYT. Căn cứ Luật bảo hiểm y tế năm 2018";
//                        //RaiseError(true, 3284, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "3284", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(
//                                        true,
//                                        3284,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(3284, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết:" +
//                                            $"Sử dụng kỹ thuật hỗ trợ sinh sản, dịch vụ kế hoạch hóa gia đình, nạo hút thai, phá thai ngoài phạm vi thanh toán của BHYT. Căn cứ Luật bảo hiểm y tế năm 2018"
//                                        );

//                    }
//                }
//                //2506	3	XML1	MA_LOAI_KCB	[130] Bệnh nhân có thực hiện dịch vụ >1 ngày từ thời điểm thực hiện Y Lệnh, kiểm tra xem đi Khám bệnh hay Ngoại trú
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 2506))
//                {

//                    if (File.MA_LOAI_KCB == "01")
//                    {
//                        var xml03Infor = File.XML03s.FirstOrDefault(x => x.NGAY_KQ.HasValue && x.NGAY_YL.HasValue && x.NGAY_YL.Value.Date < x.NGAY_KQ.Value.Date);
//                        if (xml03Infor != null)
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2506);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                            //string dichvu = xml03Infor.MA_DICH_VU;
//                            //string dichvuName = xml03Infor.TEN_DICH_VU;
//                            //DateTime timeKQ = xml03Infor.NGAY_KQ.HasValue ? xml03Infor.NGAY_KQ.Value : DateTime.MinValue;
//                            //DateTime timeYL = xml03Infor.NGAY_YL.HasValue ? xml03Infor.NGAY_YL.Value : DateTime.MinValue;

//                            //string desc = $"Bệnh nhân có sử dụng dịch vụ:{dichvu}:{dichvuName} có thời gian y lệnh là:{timeYL}, thời gian trả kết quả {timeKQ}, vậy thời gian thực hiện là: {(timeKQ-timeYL).Days} là không phù hợp, Đề nghị kiểm tra lại";
//                            //RaiseError(true, 2506, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2506", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            //RaiseError(true, 2506);
//                            RaiseError(true,
//                                        2506,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2506, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết:" +
//                                        $"Bệnh nhân có sử dụng dịch vụ: {xml03Infor.MA_DICH_VU} - {xml03Infor.TEN_DICH_VU} có thời gian y lệnh là: {xml03Infor.NGAY_YL:dd/MM/yyyy}, thời gian trả kết quả {xml03Infor.NGAY_KQ:dd/MM/yyyy}, vậy thời gian thực hiện là: {(xml03Infor.NGAY_KQ.Value.Date - xml03Infor.NGAY_YL.Value.Date).Days} ngày là không phù hợp, Đề nghị kiểm tra lại"
//                                        );
//                        }
//                    }
//                }
//                //1241	1	XML1	KET_QUA_DTRI	Điều trị ngoại trú không được phép kết quả ra viện là Khỏi
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 1241))
//                {
//                    if (Uti.HCodeContain(File.MA_LOAI_KCB, "01;02;05;07;08") && File.KET_QUA_DTRI == 1)
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1241);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";

//                        //string desc = $"Bệnh nhân đến khám bệnh, với các bệnh Mãn tính như Tiểu Đường, Tăng Huyết Áp, hoặc mới khám bệnh chưa dùng thuốc. kết quả điều trị KHỎI là không phù hợp, Đề nghị kiểm tra lại";
//                        //RaiseError(true, 1241, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1241", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        //RaiseError(true, 1241);
//                        RaiseError(true,
//                                    1241,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(1241, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân đến khám bệnh, với các bệnh Mãn tính như Tiểu Đường, Tăng Huyết Áp, hoặc mới khám bệnh chưa dùng thuốc. kết quả điều trị KHỎI là không phù hợp, Đề nghị kiểm tra lại"
//                                    );
//                    }
//                }
//                //1227  1   XML1    MA_BENH     Điều trị nội trú không cần thiết: Bệnh nhẹ mà vào điều trị Nội Trú
//                if (_dMErrorMHTC.Any(x => x.EMErrorCode == 1227))
//                {
//                    if (Uti.HCodeContain(File.MA_LOAI_KCB, "03;04") && Uti.CodeMatch(File.MA_BENH_CHINH, "A04.*;A08.*;A09.*;H10.*;H11.1;H11.2;H11.3;H11.4;H11;8;H11.9;H81.*;J00.*;J01.*;J02.*;J03.*;J20.*;K01.*;K21.*;K29.*;L23.*;M13.*;M17.*;M18.*;M25.*;M47.*;M54.*;R51.*"))
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1227);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";

//                        //string desc = $"Chuyên đề Bảo hiểm: Kiểm tra lại Bệnh {benhchinhInfo} cho điều trị nội trú có hợp lý không? Đề nghị kiểm tra lại";
//                        //RaiseError(true, 1227, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1227", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        //RaiseError(true, 1227);
//                        RaiseError(true,
//                                    1227,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(1227, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Chuyên đề Bảo hiểm: Kiểm tra lại Bệnh {benhchinhInfo} cho điều trị nội trú có hợp lý không? Đề nghị kiểm tra lại"
//                                    );
//                    }
//                }
//                //1987	0	XML1	MA_KHOA	Mã khoa nằm ngoài danh mục được Bảo hiểm phê dyệt
//                //*** Thêm Danh Mục Khoa

//                //4115	3	XML1	MA_BENH_CHINH	[ĐKQT Hải Phòng] Mã bệnh không thuộc phạm vi thanh toán BHYT
//                if (File.MA_CSKCB == "31313")
//                {
//                    if (Uti.CodeContain(File.MA_BENH_CHINH, MA_BENH_SAI_31313) || Uti.CodeContain(File.MA_BENH_KT, MA_BENH_SAI_31313))
//                    {
//                        RaiseError(
//                                true,
//                                4115,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4115, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH}, có mã Bệnh không thuộc phạm vi thanh toán BHYT"
//                                );
//                    }
//                    if (Uti.CodeContain(File.MA_BENH_CHINH, MA_BENH_SAI_31313) || Uti.CodeContain(File.MA_BENH_KT, MA_BENH_SAI_31313))
//                    {
//                        RaiseError(
//                                true,
//                                4115,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4115, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Mã bệnh kt: {File.MA_BENH_KT}, có mã Bệnh không thuộc phạm vi thanh toán BHYT"
//                                );
//                    }
//                }

//                #endregion

//                #region Kiểm tra các lỗi cơ bản
//                //13	XML1	MA_LK	Mã liên kết không được để trống
//                //14	XML1	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
//                Field(x => x.MA_LK).IsEmpty().ThenError(13).LengthOver(100).ThenError(14);

//                //15	XML1	STT	Số thứ tự để trống hoặc phải có giá trị từ 1 đến 999999999
//                Field(x => x.STT).IsEmpty().IsOver(999999999).ThenError(15);

//                //16	XML1	MA_BN	Mã bệnh nhân không được để trống
//                //17	XML1	MA_BN	Mã bệnh nhân không được lớn hơn 100 ký tự
//                Field(x => x.MA_BN).IsEmpty().ThenError(16).LengthOver(100).ThenError(16);

//                //18	XML1	HO_TEN	Họ tên không được để trống
//                //19	XML1	HO_TEN	Họ tên không được lớn hơn 255 ký tự
//                Field(x => x.HO_TEN).IsEmpty().ThenError(18).LengthOver(255).ThenError(19);

//                //21	XML1	SO_CCCD	Số CCCD không nhiều hơn 15 ký tự
//                Field(x => x.SO_CCCD).LengthOver(15).ThenError(21);
//                //2292    1   XML1 SO_CCCD[130] Số CCCD không đúng định dạng
//                if (Field(x => x.SO_CCCD).NotEmpty())
//                {
//                    RaiseError(!Uti.checkCCCD(File.SO_CCCD), 2292);
//                }
//                //20	XML1	NGAY_SINH	Ngày sinh không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
//                //139	XML1	NGAY_SINH	Ngày sinh không được lớn hơn thời gian hiện tại
//                //140	XML1	NGAY_SINH	Ngày sinh không được lớn hơn thời gian vào viện
//                // if (!Field(x => x.MA_LOAI_KCB).HasValue("2", "5", "7", "02", "05", "07"))
//                //{
//                Field(x => x.NGAY_SINH).IsEmpty().ThenError(20).IsOver(DateTime.Now).ThenError(139).IsOver(Field(x => x.NGAY_VAO)).ThenError(140);
//                RaiseError(string.IsNullOrEmpty(File.NGAY_SINH.ToString().Trim()), 20);
//                //}    
//                //else
//                //{
//                //    Field(x => x.NGAY_SINH).IsEmpty().ThenError(20).IsOver(Field(x => x.NGAY_VAO)).ThenError(140);

//                //}
//                //

//                //22	XML1	GIOI_TINH	Giới tính không được để trống và phải là số nguyên có giá trị từ 1 đến 3
//                Field(x => x.GIOI_TINH).IsEmpty().IsUnder(1).IsOver(3).ThenError(22);

//                //1260	XML1	MA_QUOCTICH	Mã quốc tịch không được để trống ???
//                //*** mã quốc tịch của người bệnh theo quy định tại Phụ lục 2 Thông tư số 07/2016/TT-BCA ***//
//                Field(x => x.MA_QUOCTICH).IsEmpty().LengthOver(3).ThenError(1260);

//                //1261	XML1	MA_DANTOC	Mã dân tộc không được để trống ???
//                //*** mã quốc tịch của người bệnh theo quy định tại Phụ lục 2 Thông tư số 07/2016/TT-BCA ***//
//                Field(x => x.MA_DANTOC).IsEmpty().LengthOver(2).ThenError(1261);

//                //1262	XML1	MA_NGHE_NGHIEP	Mã nghề nghiệp không được để trống ???
//                //*** mã nghề nghiệp theo quy định tại Quyết định số 34/2020/QĐ-TTg ngày 26 tháng 11 năm 2020 của Thủ tướng Chính phủ ***//
//                // Khong co MA_NGHE_NGHIEP ghi gia tri "00000"
//                Field(x => x.MA_NGHE_NGHIEP).IsEmpty().LengthOver(5).ThenError(1262);

//                //24	XML1	DIA_CHI	Địa chỉ không được để trống ???
//                Field(x => x.DIA_CHI).IsEmpty().LengthOver(1024).ThenError(24);

//                //1263	XML1	MATINH_CU_TRU	Mã tỉnh cư trú không được để trống ???
//                //*** Phụ lục 1 Thông tư số 07/2016/TT-BCA ***//
//                Field(x => x.MATINH_CU_TRU).IsEmpty().LengthOver(3).ThenError(1263);

//                //1264	XML1	MAHUYEN_CU_TRU	Mã huyện cư trú không được để trống ???
//                //*** Quyết định số 124/2004/QĐ-TTg ngày 08/7/2004 của Thủ tướng Chính phủ ***//
//                Field(x => x.MAHUYEN_CU_TRU).IsEmpty().LengthOver(3).ThenError(1264);

//                //1265	XML1	MAXA_CU_TRU	Mã xã cư trú không được để trống ???
//                //*** Quyết định số 124/2004/QĐ-TTg ngày 08/7/2004 của Thủ tướng Chính phủ ***//
//                Field(x => x.MAXA_CU_TRU).IsEmpty().LengthOver(5).ThenError(1265);

//                //237	XML1	NGAY_MIEN_CCT	Ngày miễn cùng chi trả phải đúng định dạng ngày tháng yyyymmddHHMM hoặc yyyymmdd
//                Field(x => x.NGAY_MIEN_CCT).NotDateTimeFormat().IgnoreEmpty().ThenError(237);

//                //23	XML1	LY_DO_VV	    Lý do vào viện không được để trống
//                Field(x => x.LY_DO_VV).IsEmpty().ThenError(23);

//                //44	XML1	CHAN_DOAN_VAO	Chẩn đoán vào viện không được để trống
//                Field(x => x.CHAN_DOAN_VAO).IsEmpty().ThenError(44);

//                //45	XML1	CHAN_DOAN_RV	Chẩn đoán ra viện không được để trống
//                Field(x => x.CHAN_DOAN_RV).IsEmpty().ThenError(45);

//                //Linhist added 24/03/26
//                //3955	XML1	CHAN_DOAN_RV	Khám sức khỏe không được thanh toán tiền BHYT
//                if (!string.IsNullOrWhiteSpace(File.CHAN_DOAN_RV?.Trim()) && File.CHAN_DOAN_RV.Contains("khám sức khỏe", StringComparison.OrdinalIgnoreCase) || File.CHAN_DOAN_RV.Contains("z00", StringComparison.OrdinalIgnoreCase))
//                {
//                    RaiseError(true, 3955,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3936, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết:" +
//                                        $"{File.CHAN_DOAN_RV} có chẩn đoán thuộc nhóm khám sức khỏe ICD10, không thanh toán Bảo hiểm Y Tế"
//                                    );
//                }

//                //37	XML1	MA_BENH_CHINH	Mã bệnh không được để trống 
//                //38	XML1	MA_BENH_CHINH	Mã bệnh phải gồm từ 3 đến 7 ký tự, không được tồn tại khoảng trắng ở giữa
//                //*** Kiem tra DM, co ca ma YHCT ???
//                Field(x => x.MA_BENH_CHINH).IsEmpty().ThenError(37).HasSpaceAny().LengthOver(7).ThenError(38);

//                //2020    XML1  MA_BENH_KT    Chỉ được ghi tối đa 12 mã bệnh kèm theo cách nhau bởi dấu ';' và không vượt quá 100 ký tự
//                //*** Kiem tra DM, co ca ma YHCT ???
//                Field(x => x.MA_BENH_KT).LengthOver(100).EachCountOver(";", 12).IgnoreEmpty().ThenError(2020);

//                //2021    XML1  MA_BENH_YHCT    Chỉ được ghi tối đa 12 mã bệnh kèm theo cách nhau bởi dấu ';' và không vượt quá 255 ký tự
//                //??? Kiem tra DM, co ca ma YHCT ???
//                Field(x => x.MA_BENH_YHCT).LengthOver(255).EachCountOver(";", 12).IgnoreEmpty().ThenError(2021);

//                //79	XML1	MA_PTTT_QT	Mã phẫu thuật thủ thuật phải gồm các chuỗi cách nhau bởi dấu ';' và nhỏ hơn 125 ký tự
//                //??? Chi ghi khi co Phau thuat
//                //??? Kiem tra theo DM ICD9-CM
//                Field(x => x.MA_PTTT_QT).LengthOver(125).IgnoreEmpty().ThenError(79);

//                //35	XML1	MA_DOITUONG_KCB	Mã đối tượng KCB không được để trống
//                ////"1";"1.1";"1.2";"1.3";"1.4";"1.5";"1.6";"1.7";"1.8";"1.9";"1.10";"2";"3";"3.1";"3.2";"3.3";"3.4";"3.5";"3.6";"3.7";"7";"7.1";"7.2";"7.3";"7.4";"8";"9"
//                ///2355	0	XML1	MA_DOITUONG_KCB	[130] Mã đối tượng KCB không nằm trong Danh mục
//                Field(x => x.MA_DOITUONG_KCB).IsEmpty().LengthOver(4).ThenError(35);
//                if (!Uti.HCodeContain(File.MA_DOITUONG_KCB, ServiceConstants.MA_DOITUONG_KCB))
//                {
//                    //RaiseError(, 2355);
//                    RaiseError(true,
//                        2355,
//                         $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                        $"Nội dung lỗi: {(errorlist.TryGetValue(2355, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                        $"Chi tiết:" +
//                        $"Mã Đối tượng KCB:{File.MA_DOITUONG_KCB}, không nằm trong QĐ 2010 BYT, ngày 19/06/2025."
//                        );
//                }

//                //3724	3	XML1	MA_DOITUONG_KCB        [3176] Mã đối tượng khám chữa bệnh không nằm trong Danh mục quy định(Quyết định 3276 ngày 17 / 10 / 2025)
//                //3725    3   XML1 MA_DOITUONG_KCB    [3176] Thái Bình: Mã đối tượng KCB không thuộc Mã đối tượng KCB tại Bệnh viện quản lý
//                if (!Uti.HCodeContain(File.MA_DOITUONG_KCB, ServiceConstants.MA_DOITUONG_KCB_3276))
//                {
//                    RaiseError(true,
//                        3724,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                        $"Nội dung lỗi: {(errorlist.TryGetValue(3724, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                        $"Chi tiết:" +
//                        $"Mã Đối tượng KCB:{File.MA_DOITUONG_KCB}, không nằm trong QĐ 3276 BYT, ngày 17/10/2025."
//                        );
//                }
//                if (!Uti.HCodeMach(File.MA_DOITUONG_KCB, "2;1.6;1.5;3.6;1.4;1.7;1.18;1.17;9;1.1;1.3;3.2"))
//                {
//                    RaiseError(true,
//                        3725,
//                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                        $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                        $"Nội dung lỗi: {(errorlist.TryGetValue(3725, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                        $"Chi tiết:" +
//                        $"Mã Đối tượng KCB:{File.MA_DOITUONG_KCB}, không thuộc Mã đối tượng KCB tại Bệnh viện quản lý."
//                        );
//                }

//                //42	XML1	MA_NOI_DI	Mã nơi đi phải bao gồm 5 ký tự, không được tồn tại khoảng trắng ở giữa
//                //145	XML1	MA_NOI_DI	Mã nơi đi không được trùng với mã bệnh viện
//                //2498	3	XML1	GIAY_CHUYEN_TUYEN	[130] Bệnh nhân chuyển tuyến (Có mã nơi đi) nhưng chưa nhập giấy chuyển tuyến hoặc giấy hẹn khám lại
//                //2499    3   XML1 GIAY_CHUYEN_TUYEN[130] Bệnh nhân hẹn khám lại(Lý do vào viện có hẹn khám) nhưng không có nhập giấy khám lại
//                //Field(x => x.MA_NOI_DI).HasSpaceAny().LengthOver(5).IgnoreEmpty().ThenError(42).IsEqual(Field(x => x.MA_CSKCB)).ThenError(145);
//                if (Field(x => x.MA_NOI_DI).NotEmpty())
//                {
//                    RaiseError(File.MA_NOI_DI.Length < 5, 42);
//                    if (_dMErrorMHTC.Any(x => x.EMErrorCode == 145))
//                    {
//                        if (File.MA_NOI_DI == File.MA_CSKCB)
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 145);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";

//                            //string desc = $"Bệnh nhân có Mã nơi đi: {File.MA_NOI_DI} trùng với Mã Bệnh viện:{File.MA_CSKCB}, Không đúng hướng dẫn 3716 và 4750, Đề nghị kiểm tra lại";
//                            //RaiseError(true, 145, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "145", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            //RaiseError(true, 145);
//                            RaiseError(true,
//                                        145,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(145, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết:" +
//                                            $"Bệnh nhân có Mã nơi đi: {File.MA_NOI_DI} trùng với Mã Bệnh viện:{File.MA_CSKCB}, Không đúng hướng dẫn 3716 và 4750, Đề nghị kiểm tra lại"
//                                        );
//                        }
//                    }
//                    if (_dMErrorMHTC.Any(x => x.EMErrorCode == 2498))
//                    {
//                        if (Field(x => x.GIAY_CHUYEN_TUYEN).IsEmpty())
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2498);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";

//                            //string desc = $"Bệnh nhân có Mã nơi đi: {File.MA_NOI_DI} (Bệnh nhân chuyển tuyến, hoặc hẹn hám lại), Nhưng không có thông tin giấy chuyển tuyến, Không đúng hướng dẫn 3716 và 4750, Đề nghị kiểm tra lại";
//                            //RaiseError(true, 2498, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2498", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            RaiseError(true,
//                                        2498,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2498, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết:" +
//                                            $"Bệnh nhân có Mã nơi đi: {File.MA_NOI_DI} (Bệnh nhân chuyển tuyến, hoặc hẹn hám lại), Nhưng không có thông tin giấy chuyển tuyến, Không đúng hướng dẫn 3716 và 4750, Đề nghị kiểm tra lại"
//                                        );
//                        }
//                    }
//                }



//                //41	XML1	MA_NOI_DEN	Mã nơi đến phải bao gồm 5 ký tự, không được tồn tại khoảng trắng ở giữa
//                //141	XML2	MA_NOI_DEN	Mã nơi đến không được trùng với mã bệnh viện
//                //Field(x => x.MA_NOI_DEN).HasSpaceAny().LengthOver(5).IgnoreEmpty().ThenError(41).IsEqual(Field(x => x.MA_CSKCB)).ThenError(141);
//                if (Field(x => x.MA_NOI_DEN).NotEmpty())
//                {
//                    RaiseError(File.MA_NOI_DEN.Length < 5, 41);
//                    if (_dMErrorMHTC.Any(x => x.EMErrorCode == 141))
//                    {
//                        if (File.MA_NOI_DEN == File.MA_CSKCB)
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 141);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";

//                            //string desc = $"Bệnh nhân có Mã nơi đến: {File.MA_NOI_DEN} trùng với mã CSKCB: {File.MA_CSKCB} , Không đúng hướng dẫn 3716 và 4750, Đề nghị kiểm tra lại";
//                            //RaiseError(true, 141, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "141", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            //RaiseError(true, 141);
//                            RaiseError(true,
//                                        141,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(141, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết:" +
//                                            $"Bệnh nhân có Mã nơi đến: {File.MA_NOI_DEN} trùng với mã CSKCB: {File.MA_CSKCB} , Không đúng hướng dẫn 3716 và 4750, Đề nghị kiểm tra lại"
//                                        );
//                        }
//                    }
//                }


//                //146	XML1	MA_TAI_NAN	Mã tai nạn phải có giá trị từ 0 đến 8 hoặc để trống
//                Field(x => x.MA_TAI_NAN).LengthOver(8).IgnoreEmpty().ThenError(146);

//                //43	XML1	NGAY_VAO	Ngày vào không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
//                //147	XML1	NGAY_VAO	Ngày vào không được lớn hơn thời gian hiện tại
//                Field(x => x.NGAY_VAO).IsEmpty().ThenError(43).IsOver(DateTime.Now).ThenError(147);

//                //47	XML1	NGAY_RA	Ngày ra không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
//                //148	XML1	NGAY_RA	Ngày ra không lớn hơn thời gian hiện tại
//                //149	XML1	NGAY_RA	Ngày ra không được nhở hơn thời gian vào
//                // Code loi update lai code nong dan
//                //2387	3	XML7	NGAY_RA	[130] Ngày ra không được nhỏ hơn ngày vào nội trú
//                //2458	3	XML1	NGAY_VAO	[130] Ngày vào không được nhỏ hơn ngày vào nội trú
//                Field(x => x.NGAY_RA).IsEmpty().ThenError(47);
//                if (Field(x => x.MA_LOAI_KCB).NotValue("02", "07", "05"))
//                {
//                    Field(x => x.NGAY_RA).IsOver(DateTime.Now).ThenError(148);
//                }
//                Field(x => x.NGAY_RA).IsUnder(File.NGAY_VAO).ThenError(149);
//                if (Field(x => x.NGAY_VAO_NOI_TRU).NotEmpty())
//                {
//                    if (File.NGAY_VAO_NOI_TRU != DateTime.MinValue || File.NGAY_VAO_NOI_TRU.Value.Year != 1753)
//                    {
//                        if (_dMErrorMHTC.Any(x => x.EMErrorCode == 2387))
//                        {
//                            if (File.NGAY_RA < File.NGAY_VAO_NOI_TRU)
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2387);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có Ngày ra viện: {File.NGAY_RA:yyyyMMdd} < Ngày vào nội trú: {File.NGAY_VAO_NOI_TRU:yyyyMMdd:hhmm}, Không hợp lý về mặt logic thời gian, Đề nghị kiểm tra lại";
//                                //RaiseError(true, 2387, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2387", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                //RaiseError(true, 2387);
//                                RaiseError(true,
//                                            2387,
//                                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2387, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                $"Chi tiết:" +
//                                                $"Bệnh nhân có Ngày ra viện: {File.NGAY_RA:yyyyMMdd} < Ngày vào nội trú: {File.NGAY_VAO_NOI_TRU:yyyyMMdd:hhmm}, Không hợp lý về mặt logic thời gian, Đề nghị kiểm tra lại"
//                                            );
//                            }
//                        }
//                        if (_dMErrorMHTC.Any(x => x.EMErrorCode == 2458))
//                        {

//                            if (File.NGAY_VAO > File.NGAY_VAO_NOI_TRU && File.MA_LOAI_KCB == "03" && File.NGAY_VAO_NOI_TRU != null)
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2458);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có Ngày vào viện: {File.NGAY_VAO:yyyyMMdd} > Ngày vào nội trú: {File.NGAY_VAO_NOI_TRU:yyyyMMdd:hhmm}, Không hợp lý về mặt logic thời gian, Đề nghị kiểm tra lại";
//                                //RaiseError(true, 2458, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2458", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                            2458,
//                                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2458, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                $"Chi tiết:" +
//                                                $"Bệnh nhân có Ngày vào viện: {File.NGAY_VAO:yyyyMMdd} > Ngày vào nội trú: {File.NGAY_VAO_NOI_TRU:yyyyMMdd:hhmm}, Không hợp lý về mặt logic thời gian, Đề nghị kiểm tra lại"
//                                            );
//                            }
//                        }
//                    }
//                    //Field(x => x.NGAY_RA).IsUnder(File.NGAY_VAO_NOI_TRU).ThenError(2387);
//                    //Field(x => x.NGAY_VAO).IsOver(File.NGAY_VAO_NOI_TRU).ThenError(2458);
//                }


//                //2022    XML1    GIAY_CHUYEN_TUYEN   Giấy chuyển tuyến không được vượt quá 50 ký tự hoặc để trống
//                Field(x => x.GIAY_CHUYEN_TUYEN).LengthOver(50).IgnoreEmpty().ThenError(2022);

//                //2022    XML1    GIAY_CHUYEN_TUYEN   Giấy chuyển tuyến không được vượt quá 50 ký tự hoặc để trống
//                //??? Ghi phương pháp điều trị theo đúng hướng dẫn tại Thông tư số 18/2022/TT-BYT của Bộ trưởng Bộ Y tế
//                //Field(x => x.PP_DIEU_TRI).IsEmpty().ThenError(999999);

//                //50	XML1	KET_QUA_DTRI	Kết quả điểu trị không được để trống và phải là số nguyên có giá trị từ 1 đến 8
//                Field(x => x.KET_QUA_DTRI).Not_KQ_DTRI().ThenError(50);

//                //51	XML1	MA_LOAI_RV	Tình trạng ra viện không được để trống và phải là số nguyên có giá trị từ 1 đến 5
//                Field(x => x.MA_LOAI_RV).Not_LOAI_RV().ThenError(51);

//                //52	XML1	NGAY_TTOAN	Ngày thanh toán phải đúng định dạng ngày tháng yyyymmddHHMM hoặc để trống
//                //152	XML1	NGAY_TTOAN	Ngày thanh toán không được nhỏ hơn ngày ra viện
//                //153	XML1	NGAY_TTOAN	Ngày thanh toán không được lớn hơn thời gian hiện tại
//                Field(x => x.NGAY_TTOAN).NotDateTimeFormat().IsEmpty().ThenError(52);
//                if (!Field(x => x.MA_LOAI_KCB).HasValue("02", "05", "07"))
//                {
//                    Field(x => x.NGAY_TTOAN).NotDateTimeFormat().IsUnder(Field(x => x.NGAY_RA)).ThenError(152);
//                }
//                var tongThuoc = 0d;
//                var tongVTYT = 0d;
//                var tongDV_VT = 0d;
//                var tongBH = 0d;
//                var tongBNTT = 0d;
//                var tongBNCCT = 0d;
//                var tongNgKhac = 0d;
//                var tongGVT = 0d;
//                try
//                {
//                    foreach (var x2 in File.XML02s)
//                    {
//                        tongThuoc += x2.THANH_TIEN_BV ?? 0d;
//                        tongBH += x2.THANH_TIEN_BH ?? 0d;
//                        tongBNTT += x2.T_BNTT ?? 0d;
//                        tongBNCCT += x2.T_BNCCT ?? 0d;
//                        tongNgKhac += x2.T_NGUONKHAC ?? 0d;

//                        if (x2.MA_PTTT != null && x2.MA_PTTT == 1)
//                        {
//                            tongGVT += x2.T_BHTT ?? 0d;
//                        }
//                    }
//                    foreach (var x3 in File.XML03s)
//                    {
//                        tongDV_VT += x3.THANH_TIEN_BV ?? 0d;
//                        if (x3.MA_NHOM == 10 || x3.MA_NHOM == 11)
//                        {
//                            tongVTYT += x3.THANH_TIEN_BV ?? 0d;
//                        }

//                        tongBH += x3.THANH_TIEN_BH ?? 0d;
//                        tongBNTT += x3.T_BNTT ?? 0d;
//                        tongBNCCT += x3.T_BNCCT ?? 0d;
//                        tongNgKhac += x3.T_NGUONKHAC ?? 0d;

//                        if (x3.MA_PTTT != null && x3.MA_PTTT == 1)
//                        {
//                            tongGVT += x3.T_BHTT ?? 0d;
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Uti.HandleCronJobLog(_logger, $"Validate130: Lỗi check Tiền: MaBN: {File.MA_BN} MaCSKCB: {File.MA_CSKCB}, MABN: {File.MA_BN} XML1 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
//                }

//                //2025	XML1	T_THUOC	    Tiền thuốc không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2179   4   XML1 T_THUOC[130] Tiền thuốc không bằng tổng tiền thuốc trong XML2
//                //Field(x => x.T_THUOC).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(2025).NotRoundedEquals(tongThuoc, 2).ThenError(2179);
//                Field(x => x.T_THUOC).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(2025);

//                //Linhist changed 26/06/26
//                //if (Math.Round((decimal)File.T_THUOC, 2) != Math.Round((decimal)tongThuoc, 2))
//                if (!Uti.IsEqualUp(File.T_THUOC, tongThuoc))
//                {
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2179);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tiền thuốc tại XML1: {File.T_THUOC:N2}  khác với Tính tổng tiền thuốc tại XML2:  {tongThuoc:N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2179, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2179", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2179,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2179, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tiền thuốc tại XML1: {File.T_THUOC:N2}  khác với Tính tổng tiền thuốc tại XML2:  {tongThuoc:N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //59	XML1	T_VTYT	Tiền vật tư y tế không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2180    4   XML1 T_VTYT[130] Tiền VTYT không bằng tổng tiền VTYT trong XML3
//                //Field(x => x.T_VTYT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(59).NotRoundedEquals(tongVTYT, 2).ThenError(2180);
//                Field(x => x.T_VTYT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(59);

//                //Linhist changed 26/06/26
//                //if (File.T_VTYT != null && Math.Round((decimal)File.T_VTYT, 2) != Math.Round((decimal)tongVTYT, 2))
//                if (File.T_VTYT != null && !Uti.IsEqualUp(File.T_VTYT, tongVTYT))
//                {
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2180);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tiền VTYT tại XML1: {File.T_VTYT:N2}  khác với Tính tổng tiền Vật tư tại XML3:  {tongVTYT:N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2180, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2180", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2180,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2180, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tiền VTYT tại XML1: {File.T_VTYT:N2}  khác với Tính tổng tiền Vật tư tại XML3:  {tongVTYT:N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //61	XML1	T_TONGCHI_BV	Tổng chi BV không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2181    4   XML1 T_TONGCHI_BV[130] Tổng chi BV không bằng tổng tiền thuốc và VTYT trong XML2 và XML3
//                //Field(x => x.T_TONGCHI_BV).IsEmpty().NotNumberFormat().IsUnderOrEqual(0).ThenError(61).NotRoundedEquals(tongThuoc + tongDV_VT, 2).ThenError(2181);
//                Field(x => x.T_TONGCHI_BV).IsEmpty().NotNumberFormat().IsUnderOrEqual(0).ThenError(61);

//                //Linhist changed 26/06/26
//                //if (Math.Round((decimal)File.T_TONGCHI_BV, 2) != Math.Round((decimal)(tongThuoc + tongDV_VT), 2))
//                if (!Uti.IsEqualUp(File.T_TONGCHI_BV, tongThuoc + tongDV_VT))
//                {
//                    ///Xử lý lỗi 2181
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2181);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tổng chi BV tại XML1: {File.T_TONGCHI_BV:N2}  khác với mức tính Tổng chi tại XML2,3:  {(tongThuoc+tongDV_VT):N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2181, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2181", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2181,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2181, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tổng chi BV tại XML1: {File.T_TONGCHI_BV:N2}  khác với mức tính Tổng chi tại XML2,3:  {(tongThuoc + tongDV_VT):N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //62	XML1	T_TONGCHI_BH	Tổng chi BH phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.') hoặc để trống
//                //2182    4   XML1 T_TONGCHI_BH[130] Tổng chi BH không bằng tổng tiền thuốc và VTYT trong XML2 và XML3
//                //Field(x => x.T_TONGCHI_BH).NotNumberFormat().IsUnder(0).IgnoreEmpty().ThenError(62).NotRoundedEquals(tongBH, 2).ThenError(2182);
//                Field(x => x.T_TONGCHI_BH).NotNumberFormat().IsUnder(0).IgnoreEmpty().ThenError(62);

//                //Linhist changed 26/06/26
//                //if (Math.Round((decimal)File.T_TONGCHI_BH, 2) != Math.Round((decimal)tongBH, 2))
//                if (!Uti.IsEqualUp(File.T_TONGCHI_BH, tongBH))
//                {
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2182);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tổng chi BH tại XML1: {File.T_TONGCHI_BH:N2}  khác với mức tính Tổng chi BH tại XML2,3:  {(tongBH):N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2182, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2182", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2182,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2182, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tổng chi BH tại XML1: {File.T_TONGCHI_BH:N2}  khác với mức tính Tổng chi BH tại XML2,3:  {(tongBH):N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //63	XML1	T_BNTT	Tiền bệnh nhân thanh toán không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2183    4   XML1 T_BNTT[130] Tiền BNTT không bằng tổng tiền T_BNTT trong XML2 & XML3
//                //Field(x => x.T_BNTT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(63).NotRoundedEquals(tongBNTT, 2).ThenError(2183);
//                Field(x => x.T_BNTT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(63);

//                //Linhist changed 26/06/26
//                //if (Math.Round((decimal)File.T_BNTT, 2) != Math.Round((decimal)tongBNTT, 2))
//                if (!Uti.IsEqualUp(File.T_BNTT, tongBNTT))
//                {
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2183);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tổng Bệnh nhân TT tại XML1: {File.T_BNTT:N2}  khác với mức tính Tổng Bệnh nhân TT tại XML2,3:  {(tongBNTT):N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2183, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2183", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2183,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2183, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tổng Bệnh nhân TT tại XML1: {File.T_BNTT:N2}  khác với mức tính Tổng Bệnh nhân TT tại XML2,3:  {(tongBNTT):N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //64	XML1	T_BNCCT	Tiền bệnh nhân cùng chi trả không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2184    4   XML1 T_BNCCT[130] Tiền BNCCT không bằng tổng tiền T_BNCCT trong XML2 & XML3
//                //Field(x => x.T_BNCCT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(64).NotRoundedEquals(tongBNCCT, 2).ThenError(2184);
//                Field(x => x.T_BNCCT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(64);

//                //Linhist changed 26/06/26
//                //if (Math.Round((decimal)File.T_BNCCT, 2) != Math.Round((decimal)tongBNCCT, 2))
//                if (!Uti.IsEqualUp(File.T_BNCCT, tongBNCCT))
//                {
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2184);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tổng Bệnh nhân CCT tại XML1: {File.T_BNCCT:N2}  khác với mức tính Tổng Bệnh nhân CCT tại XML2,3:  {(tongBNCCT):N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2184, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2184", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2184,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2184, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tổng Bệnh nhân CCT tại XML1: {File.T_BNCCT:N2}  khác với mức tính Tổng Bệnh nhân CCT tại XML2,3:  {(tongBNCCT):N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //67	XML1	T_NGUONKHAC	Tiền nguồn khác không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2185    4   XML1 T_NGUONKHAC[130] Tiền nguồn khác không bằng tổng tiền T_NGUONKHAC trong XML2 &XML3
//                //Field(x => x.T_NGUONKHAC).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(67).NotRoundedEquals(tongNgKhac, 2).ThenError(2185);
//                Field(x => x.T_NGUONKHAC).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(67);

//                //Linhist changed 26/06/26
//                //if (Math.Round((decimal)File.T_NGUONKHAC,2) != Math.Round((decimal)tongNgKhac, 2))
//                if (!Uti.IsEqualUp(File.T_NGUONKHAC, tongNgKhac))
//                {
//                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2185);
//                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                    //string desc = $"Bệnh nhân có Tổng Nguồn khác tại XML1: {File.T_NGUONKHAC:N2}  khác với mức tính Tổng Nguồn khác tại XML2,3:  {(tongNgKhac):N2}, Đề nghị kiểm tra lại";
//                    //RaiseError(true, 2185, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2185", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                    RaiseError(true,
//                                2185,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2185, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết:" +
//                                    $"Bệnh nhân có Tổng Nguồn khác tại XML1: {File.T_NGUONKHAC:N2}  khác với mức tính Tổng Nguồn khác tại XML2,3:  {(tongNgKhac):N2}, Đề nghị kiểm tra lại"
//                                );
//                }

//                //68	XML1	T_BHTT_GDV	Tiền bảo hiểm thanh toán ngoài định suất không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2186	4	XML1	T_BHTT_GVT	[130] Tiền ngoài định suất không bằng tổng tiền T_BHTT với MA_PTTT = 1 trong XML2 & XML3
//                Field(x => x.T_BHTT_GDV).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(68);//.NotRoundedEquals(tongGVT, 2).ThenError(2186);

//                //70	XML1	NAM_QT	Năm quyết toán không được để trống và phải là số nguyên lớn hơn 2000
//                Field(x => x.NAM_QT).IsEmpty().IsUnder(2000).ThenError(70);

//                //71	XML1	THANG_QT	Tháng quyết toán không được để trống và phải là số nguyên có giá trị từ 1 đến 12
//                //08/09/2025 không hiểu sao tự nhiên phát sinh lỗi Null obbject, cần kiểm tra thêm
//                // Field(x => x.THANG_QT).IsEmpty().NotBetweenOrEqual(1, 12).ThenError(71);
//                if (File.THANG_QT == null) RaiseError(true, 71);
//                else
//                {
//                    RaiseError(!Uti.IsNumber(File.THANG_QT.ToString()), 71);
//                    if (Uti.IsNumber(File.THANG_QT.ToString()))
//                    {
//                        RaiseError(int.Parse(File.THANG_QT.ToString()) < 1 || int.Parse(File.THANG_QT.ToString()) > 12, 71);
//                    }
//                }

//                //75	XML1	MA_KHOA	Mã khoa không được để trống
//                //74	XML1	MA_KHOA	Mã khoa không được lớn hơn 50 ký tự
//                Field(x => x.MA_KHOA).IsEmpty().ThenError(75).LengthOver(50).ThenError(74);

//                //76	XML1	MA_CSKCB	Mã CSKCB không được để trống
//                //77	XML1	MA_CSKCB	Mã CSKCB phải gồm 5 ký tự, không tồn tại khoảng trắng ở giữa
//                Field(x => x.MA_CSKCB).IsEmpty().ThenError(76).LengthNotEqual(5).HasSpaceAny().ThenError(77);

//                //78	XML1	MA_KHUVUC	Mã khu vực để trống hoặc phải có giá trị là K1, K2, K3
//                //Field(x => x.MA_KHUVUC).NotValueOrEmpty("K1", "K2", "K3").ThenError(78);
//                if (Field(x => x.MA_KHUVUC).NotEmpty())
//                {
//                    RaiseError(!Uti.CodeMatch(File.MA_KHUVUC, "K1;K2;K3"), 78);
//                }

//                //80	XML1	CAN_NANG	Cân nặng không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
//                //2393	4	XML1	CAN_NANG	[130] Cân năng không để bằng 0
//                //4002	3	XML1	CAN_NANG	[3176] Cân nặng đang để quá 200kg, đề nghị kiểm tra lại
//                Field(x => x.CAN_NANG).IsEmpty().IsUnderOrEqual(0).ThenError(80).HasValue(0).ThenError(2393).IsOver(200).ThenError(4002);



//                //2027    XML1    CAN_NANG_CON    Cân nặng con không vượt quá 100 ký tự và cách nhau bởi dấu chấm phẩy “;” hoặc để trống
//                Field(x => x.CAN_NANG_CON).LengthOver(100).ThenError(2027);

//                //72	XML1	MA_HSBA	Mã hồ sơ bệnh án không được để trống ???
//                Field(x => x.MA_HSBA).IsEmpty().LengthOver(100).ThenError(72);
//                #endregion


//                #region Kiểm tra các lỗi có liên quan đến các trường khác
//                //1299 	1	XML1	NGAY_VAO	Bệnh nhân không cấp cứu vào Viện ngoài giờ hành chính
//                //haimaster tam ngung de kiem tra them 06092025


//                //Linhist changed20/03/26
//                //string officeTime = dmHospital.Select(x => x.OfficeTime).FirstOrDefault();
//                //if (!string.IsNullOrEmpty(officeTime))
//                //{
//                //    if (File.MA_DOITUONG_KCB != "2")
//                //    {

//                //        //RaiseError(!isOfficeTime(officeTime, (DateTime)File.NGAY_VAO), 1299);
//                //        if (!isOfficeTime(officeTime, (DateTime)File.NGAY_VAO))
//                //        {
//                //            RaiseError(true,
//                //                1299,
//                //                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                //                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                //                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2185, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                //                $"Chi tiết:" +
//                //                $"Bệnh nhận vào viện {File.NGAY_VAO:dd/MM/yyyy HH:mm}, Giờ đăng ký khám của Viện là: {officeTime}"
//                //                );
//                //        }
//                //    }
//                //}
//                if (File.MA_DOITUONG_KCB != "2" && _officialTime != null && _officialTime.IsValid)
//                {
//                    //RaiseError(!isOfficeTime(officeTime, (DateTime)File.NGAY_VAO), 1299);
//                    if (!_officialTime.InRange(File.NGAY_VAO))
//                    {
//                        RaiseError(true,
//                            1299,
//                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                            $"Nội dung lỗi: {(errorlist.TryGetValue(1299, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                            $"Chi tiết:" +
//                            $"Bệnh nhận vào viện {File.NGAY_VAO:dd/MM/yyyy HH:mm}, Giờ đăng ký khám của Viện là: {_officialTime}"
//                            );
//                    }
//                }

//                //09062025 Haimaster tam stop de nghien cuu
//                //1982	3	XML3	SO_LUONG	BV ĐK Quốc Tế Hải Phòng không kê giường lẻ theo TT39
//                if (File.MA_CSKCB == "31313")
//                {
//                    var allNgayGiuong = File.XML03s.Where(x => Uti.NumExists(x.MA_NHOM, 14, 15) && x.SO_LUONG != null);
//                    if (allNgayGiuong != null && allNgayGiuong.Any())
//                    {
//                        var sumNgayGiuong = allNgayGiuong.Sum(x => x.SO_LUONG) ?? 0d;
//                        if (sumNgayGiuong % 1d != 0d)
//                        {
//                            var lastNgayGiuong = allNgayGiuong.Where(x => x.NGAY_YL != null).OrderByDescending(x => x.NGAY_YL).FirstOrDefault();
//                            if (lastNgayGiuong != null)
//                            {
//                                RaiseError(true, 1982);
//                            }
//                        }
//                    }
//                }

//                if (File.NGAY_RA != null && File.NGAY_VAO != null && File.NGAY_VAO_NOI_TRU != null)
//                {
//                    //439 1   XML3   SO_LUONG    Tổng ngày giường lớn hơn hướng dẫn của BYT(TT39)
//                    //440 1   XML3   SO_LUONG    Tổng ngày giường nhỏ hơn hướng dẫn của BYT(TT39)
//                    //884 1   XML3   SO_LUONG    Không thanh toán tiền giường điều trị nội trú của BN nằm viện < 4h
//                    double tongGioDtri = (File.NGAY_RA.Value - File.NGAY_VAO.Value).TotalHours;
//                    int tongNgayDtri = (int)(File.NGAY_RA.Value.Date - File.NGAY_VAO.Value.Date).TotalDays;
//                    int tongNgayDtri3176 = (int)((File.NGAY_RA?.Date - File.NGAY_VAO_NOI_TRU?.Date)?.TotalDays ?? 0);
//                    int tongGioDtri3716 = (int)((File.NGAY_RA?.Date - File.NGAY_VAO_NOI_TRU?.Date)?.TotalHours ?? 0);

//                    if (File.MA_LOAI_KCB == "03")
//                    {
//                        int ngayGiuong = 0;
//                        int ngayGuuong3176 = 0;
//                        if (tongGioDtri >= 4d && tongGioDtri <= 24d)
//                        {
//                            ngayGiuong = 1;
//                        }
//                        //Linhist changed 12/01/23
//                        //else if ((xml1.TINH_TRANG_RV == 2 && xml1.KET_QUA_DTRI > 1 && xml1.KET_QUA_DTRI < 5) || (xml1.TINH_TRANG_RV == 4 && xml1.KET_QUA_DTRI > 2 && xml1.KET_QUA_DTRI < 5))
//                        else if (
//                                    // Ket qua dieu tri KET_QUA_DTRI theo 3176
//                                    //                -Mã "1": Khỏi; 
//                                    //                -Mã "2": Đỡ;
//                                    //                -Mã "3": Không thay đổi;
//                                    //                -Mã "4": Nặng hơn;
//                                    //                -Mã "5": Tử vong tại cơ sở KBCB;
//                                    //                -Mã "6": Tiên lượng nặng xin về;
//                                    //                -Mã "7": Chưa xác định(không thuộc một trong các mã kết quả điều trị nêu trên);
//                                    //                -Mã "8": Tử vong ngoại viện;
//                                    // 
//                                    //MA_LOAI_RV theo 3176
//                                    //                -Mã "1": Ra viện; 
//                                    //                -Mã "2": Chuyển tuyến/Chuyển cơ sở KBCB theo yêu cầu chuyên môn;
//                                    //                -Mã "3": Trốn viện;
//                                    //                -Mã "4": Xin ra viện;
//                                    //                -Mã "5": Chuyển tuyến/Chuyển cơ sở KBCB theo yêu cầu người bệnh.
//                                    //

//                                    // Trích dẫn TT39-BYT-2024 Điều 4c
//                                    // Số ngày điều trị nội trú bằng ngày ra viện trừ (-) ngày vào viện cộng (+) 1: áp dụng đối với các trường hợp:

//                                    // Người bệnh đang điều trị nội trú tử vong (KET_QUA_DTRI = 5 ) hoặc diễn biến nặng lên (KET_QUA_DTRI = 4) và gia đình xin về MA_LOAI_RV = 4)  hoặc chuyển cơ sở khám bệnh, chữa bệnh (MA_LOAI_RV = 2 hoặc 5);

//                                    // Người bệnh đã được điều trị qua giai đoạn cấp cứu nhưng vẫn cần tiếp tục điều trị nội trú được chuyển đến cơ sở khám bệnh, chữa bệnh khác;

//                                    (File.KET_QUA_DTRI == 5 || File.KET_QUA_DTRI == 4 && (File.MA_LOAI_RV == 4 || File.MA_LOAI_RV == 5 || File.MA_LOAI_RV == 2)) ||
//                                    //  (File.KET_QUA_DTRI == 2 && File.MA_LOAI_RV > 1 && File.MA_LOAI_RV <= 5) ||
//                                    (File.MA_LOAI_RV == 2 || File.MA_LOAI_RV == 5) ||
//                                    (File.KET_QUA_DTRI == 5)
//                            )
//                        {
//                            ngayGiuong = tongNgayDtri + 1;
//                            ngayGuuong3176 = tongNgayDtri3176 + 1;
//                        }
//                        else
//                        {
//                            ngayGiuong = tongNgayDtri;
//                            ngayGuuong3176 = tongNgayDtri3176; 
//                        }

//                        var allGiuongs = File.XML03s.Where(x => x.MA_NHOM == 14 || x.MA_NHOM == 15);
//                        double? sumGiuongs = allGiuongs.Any() ? allGiuongs.Sum(x => x.SO_LUONG) : 0;
//                        //Linhist changed 26/06/26
//                        //int sumNgayGiuong = sumGiuongs == null || !sumGiuongs.HasValue ? 0 : (int)Math.Round(sumGiuongs.Value, 0);
//                        int sumNgayGiuong = sumGiuongs == null || !sumGiuongs.HasValue ? 0 : (int)Uti.RoundUp(sumGiuongs, 0);
//                        if (sumNgayGiuong != ngayGiuong)
//                        {
//                            //3708	3	XML1	MA_BN	[3176] Bệnh nhân nằm nội trú nhưng không kê giường Bệnh
//                            if (sumNgayGiuong == 0)
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3708);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                                //desc = desc + $"\n Bệnh nhân vào viện:{File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ngày vào nội trú:{File.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}";
//                                //desc = desc + $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  lớn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại";
//                                //RaiseError(true, 3708, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "3708", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                            3708,
//                                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                $"Nội dung lỗi: {(errorlist.TryGetValue(3708, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                $"Chi tiết:" +
//                                                $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                                $"\n Bệnh nhân vào viện:{File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ngày vào nội trú:{File.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}" +
//                                                $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  lớn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại"
//                                            );
//                            }
//                            else
//                            {
//                                foreach (var xml3 in allGiuongs)
//                                {
//                                    if (sumNgayGiuong > ngayGiuong)
//                                    {
//                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 439);
//                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                        //string desc = $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                                        //desc = desc + $"\n Bệnh nhân vào viện:{File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}";
//                                        //desc = desc + $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  lớn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại";
//                                        //xml3.RaiseError(439, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "439", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                        xml3.RaiseError(439,
//                                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                            $"Nội dung lỗi: {(errorlist.TryGetValue(439, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                            $"Chi tiết:" +
//                                                            $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                                            $"\n Bệnh nhân vào viện:{File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}" +
//                                                            $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  lớn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại"
//                                                         );
//                                        //xml3.RaiseError(439);
//                                    }
//                                    if (sumNgayGiuong < ngayGiuong)
//                                    {
//                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 440);
//                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                        //string desc = $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                                        //desc = desc + $"\n Bệnh nhân vào viện:{File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}";
//                                        //desc = desc + $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  nhỏ hơn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại";
//                                        //xml3.RaiseError(440, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "440", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                        //xml3.RaiseError(440);
//                                        xml3.RaiseError(440,
//                                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                            $"Nội dung lỗi: {(errorlist.TryGetValue(440, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                            $"Chi tiết:" +
//                                                            $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                                            $"\n Bệnh nhân vào viện:{File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}" +
//                                                            $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  nhỏ hơn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại"
//                                                         );
//                                    }
//                                    //3690	4	XML3	MA_DICH_VU	[3176] Hòa Bình: Tổng số ngày giường nhỏ hơn Hướng dẫn thông tư 39 BYT
//                                    //3691    4   XML3 MA_DICH_VU[3176] Hòa Bình: Tổng số ngày giường lớn hơn Hướng dẫn thông tư 39 BYT
//                                    if (sumNgayGiuong > ngayGuuong3176)
//                                    {
//                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3691);
//                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                        //string desc = $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                                        //desc = desc + $"\n Bệnh nhân vào Nội trú:{File.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}";
//                                        //desc = desc + $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  lớn hơn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại";
//                                        //xml3.RaiseError(3691, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "3691", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                        xml3.RaiseError(3691,
//                                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                            $"Nội dung lỗi: {(errorlist.TryGetValue(3691, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                            $"Chi tiết:" +
//                                                            $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                                            $"\n Bệnh nhân vào Nội trú:{File.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}" +
//                                                            $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  lớn hơn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại"
//                                                         );

//                                        //xml3.RaiseError(3691);
//                                    }
//                                    if (sumNgayGiuong < ngayGuuong3176)
//                                    {
//                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3690);
//                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                        //string desc = $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                                        //desc = desc + $"\n Bệnh nhân vào Nội trú:{File.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}";
//                                        //desc = desc + $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  nhỏ hơn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại";
//                                        //xml3.RaiseError(3690, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "3690", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                        //xml3.RaiseError(3690);
//                                        xml3.RaiseError(3690,
//                                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                            $"Nội dung lỗi: {(errorlist.TryGetValue(3690, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                            $"Chi tiết:" +
//                                                            $"Bệnh nhân kế quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                                            $"\n Bệnh nhân vào Nội trú:{File.NGAY_VAO_NOI_TRU:dd/MM/yyyy HH:mm:ss}, ngày ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}" +
//                                                            $"\n Bệnh nhân có Tổng số ngày giường tại XML: {sumNgayGiuong}  nhỏ hơn hơn công thức tính ngày giường:  {ngayGiuong}, Đề nghị kiểm tra lại"
//                                                         );
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }

//                //2500	3	XML1	MA_DOITUONG_KCB      [130] Mã đối tượng KCB là 9, không thanh toán BH y tế
//                if (File.MA_DOITUONG_KCB == "9")
//                {
//                    if (File.T_BHTT != 0)
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2500);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có mã đối tượng: {File.MA_DOITUONG_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.KET_QUA_DTRI.ToString())}, Đối tượng không thanh toán BHYT";

//                        //RaiseError(true,2500, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2500", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        //RaiseError(true, 2500);
//                        RaiseError(true,
//                                        2500,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã đối tượng: {File.MA_DOITUONG_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.KET_QUA_DTRI.ToString())}, Đối tượng không thanh toán BHYT\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2500, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã đối tượng: {File.MA_DOITUONG_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.KET_QUA_DTRI.ToString())}, Đối tượng không thanh toán BHYT"
//                                        );
//                    }
//                }
//                //1585	3	XML1	MA_DOITUONG_KCB	Bệnh nhân có mã ĐK KCB Ban đầu khác với mã Viện đang khám, lý do vào Viện không để Đúng Tuyến. Đề nghị sửa lại Mã đối tượng KCB
//                //2495	3	XML1	MA_DOITUONG_KCB	[130] Mã đăng ký KCB Ban đầu trùng với Mã của Viện, Mã Đối tượng KCB phải là đúng tuyến
//                if (File.MA_CSKCB == File.MA_DKBD)
//                {
//                    if (Uti.CodeMatch(File.MA_DOITUONG_KCB, "3.1;3.3;3.6;"))
//                    {
//                        RaiseError(true, 2495);
//                    }
//                }
//                if ((File.MA_CSKCB?.Length ?? 0) >= 2 && (File.MA_DKBD?.Length ?? 0) >= 2)
//                {
//                    if (File.MA_CSKCB.Substring(0, 2) != File.MA_DKBD.Substring(0, 2))
//                    {
//                        if (Uti.CodeMatch(File.MA_DOITUONG_KCB, "1;1.1;1.2;1.3;1.4;1.5;1.6;1.7;1.8;1.9;1.10;1.11;1.12;1.13;1.14;1.15;1.16;1.17;1.18;"))
//                        {
//                            if (!File.LY_DO_VV.ToLower().Contains("chuyển tuyến") && !File.LY_DO_VV.ToLower().Contains("giấy hẹn"))
//                            {
//                                if (String.IsNullOrEmpty(File.GIAY_CHUYEN_TUYEN) && String.IsNullOrEmpty(File.MA_NOI_DI))
//                                {
//                                    RaiseError(true, 1585);
//                                }
//                            }
//                        }
//                    }
//                }

//                //Bệnh nhân chuyển tuyến MA_LOAI_RV=2 hoac 5

//                //4203	2	XML1	MA_LOAI_RV	Bệnh nhân chuyển tuyến không có Biên bản hội chẩn (với điều trị nội trú)
//                //if (File.MA_LOAI_RV == 2 && (File.MA_LOAI_KCB == "03" || File.MA_LOAI_KCB == "09")
//                //{
                    
//                //}


//                //2223	4	XML1	GIAY_CHUYEN_TUYEN	[130] Giấy chuyển tuyến không có khi Bệnh nhân chuyển tuyến
//                //2224    4   XML1 MA_LOAI_RV[130] Không có thông tin XML13 khi Bệnh nhân chuyển tuyến
//                //2225    4   XML1 MA_NOI_DEN[130] Phải nhập thông tin mã nơi đến khi Bệnh nhân chuyển tuyến
//                //476	1	XML1	TINH_TRANG_RV	[130] Bệnh nhân chuyển tuyến không có Biên bản hội chẩn



//                if (Field(x => x.MA_LOAI_RV).HasValue(2, 5))
//                {
//                    //Field(x => x.GIAY_CHUYEN_TUYEN).IsEmpty().ThenError(2223);
//                    if (string.IsNullOrEmpty(File.GIAY_CHUYEN_TUYEN))
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2223);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                        //desc = desc + "\n Bệnh nhân chuyển tuyến nhưng không có Giấy chuyển tuyến, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";
//                        //RaiseError(true, 2223, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2223", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(true,
//                                        2223,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2223, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                            $"\n Bệnh nhân chuyển tuyến nhưng không có Giấy chuyển tuyến, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );

//                    }

//                    //Field(x => x.MA_NOI_DEN).IsEmpty().ThenError(2225);
//                    if (string.IsNullOrEmpty(File.MA_NOI_DEN))
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2225);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                        //desc = desc + "\n Bệnh nhân chuyển tuyến nhưng không có Mã nơi đến, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";
//                        //RaiseError(true, 2225, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2225", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(true,
//                                        2225,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2225, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                            $"\n Bệnh nhân chuyển tuyến nhưng không có Mã nơi đến, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );
//                    }
//                    if (File.XML13s is null || File.XML13s.Count == 0)
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2224);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                        //desc = desc + "\n Bệnh nhân chuyển tuyến nhưng không có thông tin tại XML13, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";

//                        //RaiseError(true, 2224, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2224", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        //RaiseError(true, 2224);
//                        RaiseError(true,
//                                        2224,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2224, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                            $"\n Bệnh nhân chuyển tuyến nhưng không có thông tin tại XML13, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );
//                    }
//                    var xml05Info = File.XML05s.Where(x => x.HOI_CHAN != null && Uti.StrExists(x.HOI_CHAN.ToLower(), true, "chuyển viện", "Chuyển BV", "Chuyển Bệnh viện", "chuyển")).ToList();
//                    if (!xml05Info.Any())
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 476);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                        //desc = desc + "\n Bệnh nhân chuyển tuyến nhưng không có thông tin chuyển tuyến tại XML5-Hội Chẩn , Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";

//                        //RaiseError(true, 476, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "476", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(true,
//                                        476,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(476, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã ra viện: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                            $"\n Bệnh nhân chuyển tuyến nhưng không có thông tin chuyển tuyến tại XML5-Hội Chẩn , Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );
//                        //RaiseError(true, 476);
//                    }

//                }
//                #endregion
//                //Kiểm tra lý do vào viện
//                //23	23	0	XML1	LY_DO_VV	Lý do vào viện không được để trống
//                if (String.IsNullOrWhiteSpace(File.LY_DO_VV) || String.IsNullOrEmpty(File.LY_DO_VV))
//                {
//                    RaiseError(true,
//                        23,
//                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                            $"Nội dung lỗi: {(errorlist.TryGetValue(23, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                            $"Chi tiết: " +
//                            $"Lý do vào viện không được để trống"
//                        );

//                }

//                //Có MA_THE_BHYT -> Kiểm tra các thông tin liên quan: MA_THE_BHYT, MA_DKBD, GT_THE_TU, GT_THE_DEN, NAM_NAM_LIEN_TUC 
//                if (Field(x => x.MA_THE_BHYT).NotEmpty())
//                {
//                    //26	XML1	MA_THE_BHYT	Mã thẻ phải gồm các chuỗi cách nhau bằng dấu ';' và mỗi chuỗi gồm 10 hoặc 15 ký tự, không tồn tại khoảng trắng ở giữa
//                    Field(x => x.MA_THE_BHYT).EachAny(";", t => t.Contains(" ") || (t.Length != 10 && t.Length != 15)).ThenError(26);
//                    //// 1240	1	XML3	MA_THE	Sai thông tin thẻ Trẻ em (Sai mã Tỉnh)
//                    ///2364	0	XML1	NGAY_SINH	[130] Bệnh nhân không phải đối tượng hưởng thẻ TE
//                    ///2472	4	XML1	MA_THE_BHYT	[130] Thẻ ghép bị thiếu thông tin Mã thẻ, đề nghị kiểm tra lại
//                    ///2473	4	XML1	GT_THE_TU	[130] Thẻ ghép bị thiếu thông tin Giá trị thẻ từ, đề nghị kiểm tra lại
//                    //2474    4   XML1 GT_THE_DEN[130] Thẻ ghép bị thiếu thông tin Giá trị thẻ đến, đề nghị kiểm tra lại
//                    //2593	3	XML1	MA_DKBD		[130] Thẻ ghép bị thiếu thông tin Mã ĐK KCBBĐ, đề nghị kiểm tra

//                    if (File.MA_THE_BHYT != null && File.MA_THE_BHYT.Length > 0 && File.MA_DKBD != null && File.MA_DKBD.Length > 0)
//                    {
//                        var maThes = File.MA_THE_BHYT.Split(";");
//                        var gtTheTu = File.GT_THE_TU.Split(";");
//                        var gtTheDen = File.GT_THE_DEN.Split(";");
//                        if (maThes.Count() == 1)
//                        {
//                            if (gtTheDen.Count() > 1)
//                            {
//                                RaiseError(true, 2472);
//                            }
//                            else if (gtTheTu.Count() > 1) RaiseError(true, 2472);
//                        }
//                        else
//                        {
//                            if (gtTheDen.Count() == 1)
//                            {
//                                RaiseError(true, 2474);
//                            }
//                            if (gtTheTu.Count() == 1)
//                            {
//                                RaiseError(true, 2473);
//                            }
//                            if (!string.IsNullOrEmpty(File.MA_DKBD))
//                            {
//                                var KCBBD = File.MA_DKBD.Split(";");
//                                if (KCBBD.Count() == 1)
//                                {
//                                    RaiseError(true, 2593);
//                                }
//                            }
//                        }
//                        foreach (var thex in maThes)
//                        {
//                            if (!dmCHUNG.Any(x => x.LOAI == "TINH" && x.MA == File.MA_THE_BHYT.Substring(3, 2)))
//                            {
//                                if (!Uti.HCodeContain(thex.PadLeft(2), "QN;HC;LS;XK;CY;CA;"))
//                                {
//                                    RaiseError(true, 1240);
//                                }
//                            }
//                            if (File.MA_THE_BHYT.Substring(0, 2) == "TE")
//                            {

//                                RaiseError(!Check_TheTE_ChuaDiHoc(thex, File.NGAY_SINH), 2364);
//                            }
//                            //2476	4	XML1	MA_THE_BHYT	[130] Hai ký tự đầu của thẻ bị sai (QĐ 1351QĐ-BHXH
//                            var Loaithe = "DN;HX;CH;NN;TK;HC;XK;HT;TB;NO;CT;XB;TN;CS;QN;CA;CY;XN;MS;CC;CK;CB;KC;HD;TE;BT;HN;DT;DK;XD;TS;TC;TQ;TA;TY;HG;LS;PV;CN;HS;SV;BG;GD";
//                            if (Uti.HCodeMach(File.MA_THE_BHYT.PadLeft(2), Loaithe))
//                            {
//                                RaiseError(true, 2476);
//                            }

//                        }
//                        //27	XML1	MA_DKBD	Mã đăng ký ban đầu không được để trống
//                        //28	XML1	MA_DKBD	Mã đăng ký ban đầu phải gồm các chuỗi cách nhau bằng dấu ';' và mỗi chuỗi gồm 5 ký tự, không tồn tại khoảng trắng ở giữa
//                        //*** Trường hợp người bệnh chưa có thẻ BHYT nhưng được cơ quan BHXH cấp mã thẻ tạm thời: Ghi theo 02 ký tự cuối của mã đơn vị hành chính của tỉnh, thành phố trực thuộc Trung ương nơi người bệnh cư trú (Quy định tại Phụ lục 1 Thông tư số 07/2016/TT-BCA ngày 01 tháng 2 năm 2016 của Bộ trưởng Bộ Công an) + 000. Ví dụ: Hà Nội thì ghi là 01000.
//                        Field(x => x.MA_DKBD).IsEmpty().ThenError(27).EachAny(";", t => t.Contains(" ") || t.Length != 5).ThenError(28);

//                        //29	XML1	GT_THE_TU	Giá trị thẻ từ không được để trống
//                        //30	XML1	GT_THE_TU	Các giá trị thẻ từ phải gồm các chuỗi cách nhau bằng dấu ';' và mỗi chuỗi phải đúng định dạng ngày tháng yyyymmddHHMM
//                        //*** Trường hợp người bệnh KBCB BHYT nhưng chưa có thẻ BHYT: Thay thời điểm thẻ BHYT có giá trị bằng thời gian người bệnh vào cơ sở KBCB (gồm 08 ký tự theo định dạng yyyymmdd);
//                        Field(x => x.GT_THE_TU).IsEmpty().ThenError(29).EachAny(";", t => EvalHelper.ToDateSql(t) == null).ThenError(30);

//                        //33	XML1	GT_THE_DEN	Giá trị thẻ đến không được để trống
//                        //34	XML1	GT_THE_DEN	Giá trị thẻ đến phải gồm các chuỗi cách nhau bằng dấu ';' và mỗi chuỗi phải đúng định dạng ngày tháng yyyymmddHHMM
//                        //*** Trường hợp người bệnh KBCB BHYT nhưng chưa có thẻ BHYT: Thay thời điểm thẻ BHYT có giá trị bằng thời gian người bệnh vào cơ sở KBCB (gồm 08 ký tự theo định dạng yyyymmdd);
//                        Field(x => x.GT_THE_DEN).IsEmpty().ThenError(33).EachAny(";", t => EvalHelper.ToDateSql(t) == null).ThenError(34);

//                        //2017	XML1	NAM_NAM_LIEN_TUC	Thời điểm năm năm liên tục không được để trống và phải đúng định dạng ngày tháng yyyymmdd
//                        Field(x => x.NAM_NAM_LIEN_TUC).NotDateTimeFormat().ThenError(2017);
//                    }

//                    //73	XML1	MA_LOAI_KCB	    Mã loại KCB không được để trống và phải là số nguyên có giá trị từ 1 đến 10
//                    //Có MA_LOAI_KCB = 03, 09,04 (Điều trị nội trú, Điều trị nội trú dưới 4 tiếng) -> Kiểm tra các thông tin liên quan: LY_DO_VNT, MA_LY_DO_VNT, NGAY_VAO_NOI_TRU
//                    if (!Field(x => x.MA_LOAI_KCB).Not_LOAI_KCB().ThenError(73))
//                    {
//                        if (Field(x => x.MA_LOAI_KCB).HasValue("03", "09", "04"))
//                        {
//                            //2018    XML1    LY_DO_VNT       Lý do vào nội trú không được để trống
//                            //Field(x => x.LY_DO_VNT).IsEmpty().ThenError(2018);
//                            if (String.IsNullOrEmpty(File.LY_DO_VNT))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2018);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}";
//                                //desc = desc + "\n Bệnh nhân không có thông tin Lý do vào nội trú. Đề nghị kiểm tra lại, Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";

//                                //RaiseError(true, 2018, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2018", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        2018,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2018, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}" +
//                                            $"\n Bệnh nhân không có thông tin Lý do vào nội trú. Đề nghị kiểm tra lại, Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );
//                            }
//                            //2019    XML1    MA_LY_DO_VNT    Mã lý do vào nội trú không được để trống
//                            //*** Kiem tra trong DM
//                            //Field(x => x.MA_LY_DO_VNT).IsEmpty().ThenError(2019);

//                            //46	XML1	NGAY_VAO_NOI_TRU	Ngày vào nội trú phải đúng định dạng ngày tháng yyyymmddHHMM
//                            Field(x => x.NGAY_VAO_NOI_TRU).IsEmpty().NotDateTimeFormat().ThenError(46);
//                        }
//                    }

//                    //Linhist added 17/07/26
//                    //4169	2	XML1	MA_LY_DO_VNT	[3176] MA_LY_DO_VNT không được quá 5 kí tự
//                    if (!string.IsNullOrEmpty(File.MA_LY_DO_VNT) && File.MA_LY_DO_VNT.Length > 5)
//                    {
//                        RaiseError(true, 4169);
//                    }

//                    //2191	4	XML1	MA_LOAI_KCB	[130] Khám chữa bệnh nội trú bắt buộc phải có giấy ra viện (XML7)
//                    if (Field(x => x.MA_LOAI_KCB).HasValue("03", "04", "09"))
//                    {
//                        if (Field(x => x.KET_QUA_DTRI).NotValue(5))
//                        {
//                            if (Field(x => x.MA_LOAI_RV).NotValue(3))
//                            {
//                                if (File.XML07s is null || File.XML07s.Count == 0)
//                                {
//                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2191);
//                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                    //string desc = $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}, Kết quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}";
//                                    //desc = desc + "\n Bệnh nhân không có thông tin Giấy ra viện tại XML7, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";
//                                    //RaiseError(true, 2191, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2191", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                    //RaiseError(true, 2191);
//                                    RaiseError(true,
//                                        2191,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}, Kết quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2191, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}, Kết quả điều trị: {File.KET_QUA_DTRI}|{ServiceConstants.GetValue(ServiceConstants.ketquadieutri, File.KET_QUA_DTRI.ToString())} và có Tình trạng ra Viện:{File.MA_LOAI_RV}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.MA_LOAI_RV.ToString())}" +
//                                            $"\n Bệnh nhân không có thông tin Giấy ra viện tại XML7, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );
//                                }
//                            }
//                        }
//                    }
//                    ////2470	4	XML1	MA_LOAI_KCB	[130] Bắt buộc phải nhập nội dung tóm tắt hồ sơ Bệnh án (XML8) với những loại KCB là 03, 04 và 06
//                    ///
//                    if (Field(x => x.MA_LOAI_KCB).HasValue("03", "04", "06"))
//                    {


//                        if (File.XML08s is null || File.XML08s.Count == 0)
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2470);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                            //string desc = $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}";
//                            //desc= desc + "\n Bệnh nhân không có thông tin Giấy ra viện tại XML7, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế";
//                            //RaiseError(true, 2470, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2470", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));                            //RaiseError(true, 2470);
//                            RaiseError(true,
//                                        2470,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2470, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}" +
//                                            $"\n Bệnh nhân không có thông tin Giấy ra viện tại XML7, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );

//                        }


//                    }
                    
//                    //Linhist added 14/04/26
//                    //3988   2   XML1 MA_LOAI_KCB   [3176] Chi tiết diễn biến lâm sàng(XML5) bắt buộc phải nhập khi điều trị ngoại trú(02; 05; 08), điều trị nội trú, điều trị nội trú ban ngày, điều trị nội trú<4h hoặc điều trị lưu tại TYT tuyến xã, PKĐKKV
//                    if (Field(x => x.MA_LOAI_KCB).HasValue("02", "03", "04", "05", "06", "08", "09"))
//                    {
//                        if (File.XML05s is null || File.XML05s.Count == 0)
//                        {
//                            RaiseError(true,
//                                        3988,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(3988, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.LoaiKhamBenh, File.MA_LOAI_KCB.ToString())}" +
//                                            $"\n Bệnh nhân không có thông tin Chi tiết diễn biến lâm sàng tại XML5, Đề nghị kiểm tra lại. Căn cứ Quyết định 3176 và Quyết định 4750 Bộ Y tế"
//                                        );
//                        }
//                    }    
                    
                    
//                    //49	XML1	SO_NGAY_DTRI	Số ngày điều trị không được để trống và phải là số nguyên có giá trị từ 0 đến 999
//                    Field(x => x.SO_NGAY_DTRI).IsEmpty().NotBetweenOrEqual(0, 999).ThenError(49);
//                    if (Field(x => x.NGAY_RA).NotEmpty().Field(x => x.NGAY_VAO).NotEmpty())
//                    {
//                        //2188	4	XML1	NGAY_RA	[130] Thời gian khám bệnh từ lúc đến viện đến lúc ra viện nhỏ hơn 5 phút, cần kiểm tra
//                        if ((File.NGAY_RA - File.NGAY_VAO)?.TotalMinutes < 5)
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2188);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                            //string desc = $"Bệnh nhân vào viện: {File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}, thời gian nhỏ hơn 5 phút, cần kiểm tra lại";
//                            //RaiseError(true, 2188, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2188", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            //RaiseError(true, 2188);
//                            RaiseError(true,
//                                        2188,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2188, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân vào viện: {File.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, ra viện:{File.NGAY_RA:dd/MM/yyyy HH:mm:ss}, thời gian nhỏ hơn 5 phút, cần kiểm tra lại"
//                                        );
//                        }


//                        if (!Field(x => x.SO_NGAY_DTRI).IsEmpty())
//                        {
//                            //2023    XML1    SO_NGAY_DTRI    Chỉ ghi 0 nếu MA_LOAI_KCB có các mã "01", "07", "09"
//                            if (Field(x => x.MA_LOAI_KCB).HasValue("01", "07", "09"))
//                            {
//                                if (File.SO_NGAY_DTRI != 0)
//                                {
//                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2023);
//                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                    //string desc = $"Bệnh nhân có loại khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.MA_DOITUONG_KCB)}";
//                                    //desc = desc + "\n Số ngày điều trị phải nhập là 0, Căn cứ Quyết định 4705 và Quyết định 3176 Bộ Y Tế";
//                                    //RaiseError(true, 2023, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2023", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                    //Field(x => x.SO_NGAY_DTRI).NotValue(0).ThenError(2023);
//                                    RaiseError(true,
//                                        2023,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.MA_DOITUONG_KCB)}\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2023, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có loại khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.MA_DOITUONG_KCB)}" +
//                                            $"\n Số ngày điều trị phải nhập là 0, Căn cứ Quyết định 4705 và Quyết định 3176 Bộ Y Tế"
//                                        );
//                                }
//                            }
//                            //2024    XML1    SO_NGAY_DTRI    SO_NGAY_DTRI = NGAY_RA - NGAY_VAO + 1 nếu MA_LOAI_KCB có các mã "02", "03", "04", "06"
//                            else
//                            {
//                                if (Field(x => x.MA_LOAI_KCB).HasValue("02", "03", "04", "06"))
//                                {
//                                    //DateTime ngayra = new DateTime(File.NGAY_RA.Value.Year, File.NGAY_RA.Value.Month, File.NGAY_RA.Value.Day);
//                                    //DateTime ngayvao = new DateTime(File.NGAY_VAO.Value.Year, File.NGAY_VAO.Value.Month, File.NGAY_VAO.Value.Day);
//                                    //var ts = ngayra - ngayvao;
//                                    //double d = ts.TotalDays + 1;
//                                    //d = Math.Round(d, 0);

//                                    //Linhist changed 26/06/26
//                                    //double songay = File.NGAY_RA.HasValue && File.NGAY_VAO.HasValue
//                                    //        ? Math.Round((File.NGAY_RA.Value.Date - File.NGAY_VAO.Value.Date).TotalDays + 1, 0)
//                                    //        : 0;
//                                    var songay = File.NGAY_RA.HasValue && File.NGAY_VAO.HasValue
//                                            ? (int)Uti.RoundUp((File.NGAY_RA?.Date - File.NGAY_VAO?.Date)?.TotalDays + 1d, 0)
//                                            : 0;
//                                    //Field(x => x.SO_NGAY_DTRI).NotValue((int)d).ThenError(2024);
//                                    if (File.SO_NGAY_DTRI != songay)
//                                    {
//                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2024);
//                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                        //string desc = $"Bệnh nhân có loại khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.MA_DOITUONG_KCB)}";
//                                        //desc = desc + "\n Số ngày điều trị phải nhập là 0, Căn cứ Quyết định 4705 và Quyết định 3176 Bộ Y Tế";
//                                        //RaiseError(true, 2024, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2024", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                        RaiseError(true,
//                                                2024,
//                                                $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                                $"Mã khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.MA_DOITUONG_KCB)}\n" +
//                                                $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2024, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                                $"Chi tiết: " +
//                                                $"Bệnh nhân có loại khám chữa bệnh: {File.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.MaDoiTuongKCB, File.MA_DOITUONG_KCB)}" +
//                                                $"\n Số ngày điều trị phải nhập là 0, Căn cứ Quyết định 4705 và Quyết định 3176 Bộ Y Tế"
//                                            );
//                                    }
//                                }
//                                //??? Đối với Trường thông tin MA_LOAI_KCB có mã ""05"" thì trường thông tin SO_NGAY_DTRI = Số ngày dùng thuốc;
//                                //2414	3	XML1	SO_NGAY_DTRI	[130] MA_LOAI_KCB có mã ""05"" thì trường thông tin SO_NGAY_DTRI = Số ngày dùng thuốc

//                                else if (Field(x => x.MA_LOAI_KCB).HasValue("05"))
//                                {
//                                    var songaydungT = File.XML02s.Where(x => x.NGAY_YL != null && x.MA_NHOM == 4).Select(x => x.NGAY_YL).ToList();
//                                    int countsongaydT = 0;
//                                    if (songaydungT.Any())
//                                    {
//                                        var dngay = DateTime.Now.Date.AddDays(10);
//                                        foreach (var ngay in songaydungT)
//                                        {
//                                            if (ngay.Value.Date != dngay.Date)
//                                            {
//                                                countsongaydT++;
//                                                dngay = ngay.Value;
//                                            }
//                                        }
//                                    }
//                                    Field(x => x.SO_NGAY_DTRI).NotValue(countsongaydT).ThenError(2414);
//                                }
//                                //??? Đối với Trường thông tin MA_LOAI_KCB có mã ""08"": trường thông tin SO_NGAY_DTRI = Số ngày thực tế có sử dụng DVKT.
//                                //2415    3   XML1 SO_NGAY_DTRI[130] MA_LOAI_KCB có mã ""08"": trường thông tin SO_NGAY_DTRI = Số ngày thực tế có sử dụng DVKT.
//                                else if (Field(x => x.MA_LOAI_KCB).HasValue("08"))
//                                {
//                                    var songaydungDV = File.XML03s.Where(x => x.NGAY_TH_YL != null).Select(x => x.NGAY_TH_YL).OrderByDescending(g => g.Value).ToList();

//                                    int countsongaydT = 0;
//                                    if (songaydungDV.Any())
//                                    {
//                                        var dngay = DateTime.Now.Date.AddDays(10);
//                                        foreach (var ngay in songaydungDV)
//                                        {
//                                            if (ngay.Value.Date != dngay.Date)
//                                            {
//                                                countsongaydT++;
//                                                dngay = ngay.Value;
//                                            }
//                                        }
//                                    }
//                                    //Uti.HandleCronJobLog(logger, $"[130] Kết thúc giam dinh ho so XML01", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML01)} time" + dt.ToString());
//                                    Field(x => x.SO_NGAY_DTRI).NotValue(countsongaydT).ThenError(2415);
//                                }
//                            }
//                        }
//                    }

//                    //65	XML1	T_BHTT	Tiền bảo hiểm thanh toán không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.'
//                    if (!Field(x => x.T_BHTT).IsEmpty().NotNumberFormat().IsUnder(0).ThenError(65))
//                    {
//                        //Kiểm tra T_BHTT theo Công thức: T_BHTT = T_TONGCHI_BH - T_BNCCT
//                        //2026    XML1    T_BHTT  Tổng số tiền đề nghị cơ quan bảo hiểm xã hội thanh toán theo công thức sau: T_BHTT = T_TONGCHI_BH - T_BNCCT
//                        //if (Field(x => x.T_TONGCHI_BH).Result() && Field(x => x.T_BNCCT).Result())
//                        //{
//                        //    var tongBHTT = Math.Round(File.T_TONGCHI_BH.Value - File.T_BNCCT.Value, 2);
//                        //    Field(x => x.T_BHTT).NotValue(tongBHTT).ThenError(2026);
//                        //}
//                        //Linhist changed 26/06/26
//                        //Field(x => x.T_TONGCHI_BH)
//                        //    .Field(x => x.T_BNCCT)
//                        //    .Field(x => x.T_BHTT).NotValue(Math.Round(File.T_TONGCHI_BH.Value - File.T_BNCCT.Value, 2)).ThenError(2026);
//                        Field(x => x.T_TONGCHI_BH)
//                            .Field(x => x.T_BNCCT)
//                            .Field(x => x.T_BHTT).NotValue((double)Uti.RoundUp(File.T_TONGCHI_BH - File.T_BNCCT)).ThenError(2026);
//                    }

//                    //2090	4	XML1	MA_BENH_CHINH	[130] Mã bệnh chính phải là mã ICD10, không nhập thêm mã Bệnh YHCT

//                    #region Kiểm tra mã bệnh
//                    // 2993    3   XML1 MA_BENH_CHINH   [3176] Gia Lai: Mã bệnh chính và mã bệnh kèm theo trùng ICD(Mã cha, mã con), không hợp lý về chuyên môn
//                    //2994    3   XML1 MA_BENH_KT[3176] Gia Lai: Mã bệnh kèm theo trùng ICD(Mã cha, mã con), không hợp lý về chuyên môn
//                    if (Field(x => x.MA_BENH_KT).NotEmpty())
//                    {
//                        var maBenhKT = File.MA_BENH_KT.Split(";");
//                        var dubplicateICD = new HashSet<string>();
//                        foreach (var m in maBenhKT)
//                        {
//                            if (File.MA_BENH_CHINH.ToString().Substring(0, 3) == m.ToString().Substring(0, 3))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2993);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có Mã bệnh chính: {File.MA_BENH_CHINH}| Mã bệnh kèm theo:{File.MA_BENH_KT}";
//                                //desc = desc + $"\n Trong mã bệnh kèm theo có Mã bệnh:{m.ToString()}, trùng với mã bệnh chính, không hợp lý về chuyên môn";
//                                //RaiseError(true, 2993, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2993", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        2993,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2993, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có Mã bệnh chính: {File.MA_BENH_CHINH}| Mã bệnh kèm theo:{File.MA_BENH_KT}" +
//                                            $"\n Trong mã bệnh kèm theo có Mã bệnh:{m.ToString()}, trùng với mã bệnh chính, không hợp lý về chuyên môn"
//                                        );
//                                //RaiseError(true, 2993);
//                            }
//                            if (!dubplicateICD.Add(m.Substring(0, 3)))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2994);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có Mã bệnh kèm theo:{File.MA_BENH_KT}";
//                                //desc = desc + $"\n Trong mã bệnh kèm theo có Mã bệnh:{m.ToString()} rồi, khai báo 2 lần 1 mã bệnh trong Bệnh kèm theo, không hợp lý về chuyên môn";
//                                //RaiseError(true, 2994, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2994", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                //RaiseError(true, 2994);
//                                RaiseError(true,
//                                        2994,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2994, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có Mã bệnh kèm theo:{File.MA_BENH_KT}" +
//                                            $"\n Trong mã bệnh kèm theo có Mã bệnh:{m.ToString()} rồi, khai báo 2 lần 1 mã bệnh trong Bệnh kèm theo, không hợp lý về chuyên môn"
//                                        );
//                            }
//                        }
//                    }
//                    //2321	2321	0	XML1	MA_BENH_CHINH	[130] Mã ICD10 Bệnh chính không có trong Bảng mã ICD10 kiểm tra lại
//                    //2322    2322    0   XML1 MA_BENH_KT[130] Mã ICD10 tại Mã bệnh kèm theo không có trong DS Bệnh ICD10
//                    //2487	3	XML1	MA_BENH_YHCT	[13] Không tồn mã Mã Bệnh YHCT trong Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế
//                    //2494	3	XML1	MA_BENH_YHCT	[130] Mã bệnh YHCT phải cách nhau bằng dấu chấm phẩy ";"
//                    if (Field(x => x.MA_BENH_YHCT).NotEmpty())
//                    {
//                        if (File.MA_BENH_YHCT.Contains(","))
//                        {
//                            RaiseError(true, 2494);
//                        }
//                        var mabenhyhct = File.MA_BENH_YHCT.Split(";");
//                        foreach (var m in mabenhyhct)
//                        {
//                            var benhYHCTinfo = dmYHCT.FirstOrDefault(x => Uti.HCodeContain(m, x.YHDTCode));
//                            if (benhYHCTinfo == null)
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2487);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có Mã bệnh YHCT:{m}, Không nằm trong Bảng Mã Bệnh YHCT - Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế ";

//                                //RaiseError(true, 2487, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2487", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        2487,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2487, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có Mã bệnh YHCT:{m}, Không nằm trong Bảng Mã Bệnh YHCT - Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế "
//                                        );
//                                //RaiseError(!dmYHCT.Any(x => Uti.CodeContain(m, x.YHDTCode)), 2487);
//                            }
//                        }
//                    }
//                    RaiseError(dmYHCT.Any(x => Uti.HCodeContain(File.MA_BENH_CHINH, x.YHDTCode)), 2090);

//                    //2321	2321	0	XML1	MA_BENH_CHINH	[130] Mã ICD10 Bệnh chính không có trong Bảng mã ICD10 kiểm tra lại
//                    //2322    2322    0   XML1 MA_BENH_KT[130] Mã ICD10 tại Mã bệnh kèm theo không có trong DS Bệnh ICD10
//                    //2091    4   XML1 MA_BENH_KT[130] Mã bệnh kèm theo phải là mã ICD10, không nhập thêm mã Bệnh YHCT
//                    var benhchinh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICDCHINH" && x.MA == File.MA_BENH_CHINH);
//                    if (benhchinh == null)
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2321);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có Mã bệnh chính:{File.MA_BENH_CHINH}, Không nằm trong phụ lục ICD10 - Quyết định 4469 Bộ Y Tế";

//                        //RaiseError(true, 2321, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2321", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(true,
//                                        2321,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2321, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có Mã bệnh chính:{File.MA_BENH_CHINH}, Không nằm trong phụ lục ICD10 - Quyết định 4469 Bộ Y Tế"
//                                        );
//                    }
//                    //RaiseError(dmCHUNG.Any(x => x.LOAI == "ICDOCHINH" && x.MA == File.MA_BENH_CHINH), 2321);
//                    if (!string.IsNullOrEmpty(File.MA_BENH_KT))
//                    {
//                        var benhKT = File.MA_BENH_KT.Split(';');
//                        foreach (var item in benhKT)
//                        {
//                            RaiseError(dmYHCT.Any(x => x.YHDTCode == item), 2091);
//                            //RaiseError(!dmCHUNG.Any(x => x.LOAI == "ICD10" && x.MA == item), 2322);
//                            var benhkemtheoinfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == item);
//                            if (benhkemtheoinfor == null)
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2322);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //string desc = $"Bệnh nhân có Mã bệnh kèm theo:{item}, Không mằm trong Phụ lục ICD10: Các mã bệnh ICD10 - Quyết định 4469 Bộ Y Tế";
//                                //RaiseError(true, 2322, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2322", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        2322,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2322, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có Mã bệnh kèm theo:{item}, Không mằm trong Phụ lục ICD10: Các mã bệnh ICD10 - Quyết định 4469 Bộ Y Tế"
//                                        );
//                            }
//                        }
//                    }
//                    //4087	3	XML1	MA_BENH_CHINH	[3176] TT06/2026 Mã Bệnh không được dùng làm bệnh chính, đề nghị kiểm tra lại
//                    //4088    3   XML1 MA_BENH_CHINH[3176] TT06 / 2026 Mã bệnh không khuyến khích dùng làm mã Bệnh chính, đề nghị kiểm tra lại
//                    //4089    3   XML1 MA_BENH_CHINH[3176] TT06 / 2026 Mã bệnh không được sử dụng vì đã có mã bệnh 4 ký tự hoặc 5 ký tự cụ thể hơn(Bệnh chính)
//                    //4090    3   XML1 MA_BENH_KT[3176] TT06 / 2026 Mã bệnh không được sử dụng vì đã có mã bệnh 4 ký tự hoặc 5 ký tự cụ thể hơn(Bệnh kèm theo)
//                    //4091    3   XML1 MA_BENH_CHINH[3176] TT06 / 2026 Mã bệnh chỉ mã hóa cho nguyên nhân tử vong(Bệnh chính)
//                    //4092    3   XML1 MA_BENH_KT[3176] TT06 / 2026 Mã bệnh chỉ mã hóa cho nguyên nhân tử vong(Bệnh phụ)
//                    //4093    3   XML1 MA_BENH_CHINH[3176] TT06 / 2026 Mã bệnh chỉ có hoặc chủ yếu ở Nữ giới(Bệnh chính)
//                    //4094    3   XML1 MA_BENH_KT[3176] TT06 / 2026 Mã bệnh chỉ có hoặc chủ yếu ở Nữ giới(Bệnh phụ)
//                    //4095    3   XML1 MA_BENH_CHINH[3176] TT06 / 2026 Mã bệnh chỉ có hoặc chủ yếu ở Nam giới(Bệnh chính)
//                    //4096    3   XML1 MA_BENH_KT[3176] TT06 / 2026 Mã bệnh chỉ có hoặc chủ yếu ở Nam giới(Bệnh phụ)

//                    if (!string.IsNullOrEmpty(File.MA_BENH_CHINH))
//                    {
//                        foreach (var benh in File.MA_BENH_CHINH.Split(";"))
//                        {
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MAKOBENHCHINH" && x.TEN.Contains(benh)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MAKOBENHCHINH" && x.TEN.Contains(benh)))
//                            {
//                                RaiseError(true,
//                                    4087,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4087, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết: " +
//                                    $"Mã Bệnh {benh} không được dùng làm bệnh chính, Theo thông tư 06/2026 của Bộ Y Tế"
//                                    );
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MAKOKKBENHCHINH" && x.TEN.Contains(benh)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MAKOKKBENHCHINH" && x.TEN.Contains(benh)))
//                            {
//                                RaiseError(true,
//                                    4088,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4088, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết: " +
//                                    $"Mã Bệnh {benh} không khuyến khích dùng làm mã Bệnh chính, Theo thông tư 06/2026 của Bộ Y Tế"
//                                    );
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MAKOSD" && x.TEN.Contains(benh)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MAKOSD" && x.TEN.Contains(benh)))
//                            {
//                                RaiseError(true,
//                                    4089,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4089, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết: " +
//                                    $"Mã Bệnh {benh} không được sử dụng vì đã có mã bệnh 4 ký tự hoặc 5 ký tự cụ thể hơn(Bệnh chính), Theo thông tư 06/2026 của Bộ Y Tế"
//                                    );
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MAHOATV" && x.TEN.Contains(benh)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MAHOATV" && x.TEN.Contains(benh)))
//                            {
//                                if (File.KET_QUA_DTRI != 5 || File.KET_QUA_DTRI != 8)
//                                {
//                                    RaiseError(true,
//                                        4091,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4091, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Mã Bệnh {benh} không được sử dụng vì đây là mã bệnh mã hóa cho nguyên nhân tử vong, Theo thông tư 06/2026 của Bộ Y Tế"
//                                        );
//                                }
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MABENHNU" && x.TEN.Contains(benh)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MABENHNU" && x.TEN.Contains(benh)))
//                            {
//                                if (File.GIOI_TINH != 2)
//                                {
//                                    RaiseError(true,
//                                        4093,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4093, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Mã Bệnh {benh} không được sử dụng vì đây là mã bệnh cho bệnh nhân nữ giới, Theo thông tư 06/2026 của Bộ Y Tế"
//                                        );
//                                }
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MABENHNAM" && x.TEN.Contains(benh)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MABENHNAM" && x.TEN.Contains(benh)))
//                            {
//                                if (File.GIOI_TINH != 1)
//                                {
//                                    RaiseError(true,
//                                        4095,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4095, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Mã Bệnh {benh} không được sử dụng vì đây là mã bệnh cho bệnh nhân nam giới, Theo thông tư 06/2026 của Bộ Y Tế"
//                                        );
//                                }
//                            }

//                            //Linhist added 06/07/26
//                            //4124    2   XML1 MA_BENH_CHINH[3176] Mã bệnh chính không nằm trong danh mục mã bệnh của TT06
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MABENHCHUNG" && !x.TEN.Contains(benh)))
//                            {
//                                RaiseError(true,
//                                    4124,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã bệnh chính: {File.MA_BENH_CHINH}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4124, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết: " +
//                                    $"Mã Bệnh {benh} không nằm trong danh mục mã bệnh, Theo thông tư 06/2026 của Bộ Y Tế"
//                                    );
//                            }
//                        }
//                    }

//                    if (!string.IsNullOrEmpty(File.MA_BENH_KT))
//                    {
//                        foreach (var benhkt in File.MA_BENH_KT.Split(";"))
//                        {
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MAKOSD" && x.TEN.Contains(benhkt)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MAKOSD" && x.TEN.Contains(benhkt)))
//                            {
//                                RaiseError(true,
//                                    4090,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã bệnh kèm theo: {File.MA_BENH_KT}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4090, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết: " +
//                                    $"Mã Bệnh {benhkt} không được sử dụng vì đã có mã bệnh 4 ký tự hoặc 5 ký tự cụ thể hơn(Bệnh kèm theo), Theo thông tư 06/2026 của Bộ Y Tế"
//                                    );
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MAHOATV" && x.TEN.Contains(benhkt)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MAHOATV" && x.TEN.Contains(benhkt)))
//                            {
//                                if (File.KET_QUA_DTRI != 5 || File.KET_QUA_DTRI != 8)
//                                {
//                                    RaiseError(true,
//                                        4092,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh kèm theo: {File.MA_BENH_KT}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4092, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Mã Bệnh {benhkt} không được sử dụng vì đây là mã bệnh mã hóa cho nguyên nhân tử vong, Theo thông tư 06/2026 của Bộ Y Tế"
//                                        );
//                                }
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MABENHNU" && x.TEN.Contains(benhkt)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MABENHNU" && x.TEN.Contains(benhkt)))
//                            {
//                                if (File.GIOI_TINH != 2)
//                                {
//                                    RaiseError(true,
//                                        4094,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh kèm theo: {File.MA_BENH_KT}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4094, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Mã Bệnh {benhkt} không được sử dụng vì đây là mã bệnh cho bệnh nhân nữ giới, Theo thông tư 06/2026 của Bộ Y Tế"
//                                        );
//                                }
//                            }
//                            //Linhist changed 06/07/26
//                            //if (dmCHUNG.Any(x => x.MA == "TT06/2026" && x.LOAI == "MABENHNAM" && x.TEN.Contains(benhkt)))
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MABENHNAM" && x.TEN.Contains(benhkt)))
//                            {
//                                if (File.GIOI_TINH != 1)
//                                {
//                                    RaiseError(true,
//                                        4096,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh kèm theo: {File.MA_BENH_KT}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4096, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Mã Bệnh {benhkt} không được sử dụng vì đây là mã bệnh cho bệnh nhân nam giới, Theo thông tư 06/2026 của Bộ Y Tế"
//                                        );
//                                }
//                            }

//                            //Linhist added 06/07/26
//                            //4125	2	XML1	MA_BENH_KT	[3176] Mã bệnh chính không nằm trong danh mục mã bệnh của TT06
//                            if (dmCHUNG.Any(x => x.LOAI == "TT06/2026" && x.MA == "MABENHCHUNG" && !x.TEN.Contains(benhkt)))
//                            {
//                                RaiseError(true,
//                                    4125,
//                                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                    $"Mã bệnh kèm theo: {File.MA_BENH_KT}\n" +
//                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4125, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                    $"Chi tiết: " +
//                                    $"Mã Bệnh {benhkt} không nằm trong danh mục mã bệnh, Theo thông tư 06/2026 của Bộ Y Tế"
//                                    );
//                            }
//                        }
//                    }




//                    #endregion

//                    #region Thong tin hanh chinh Quoc Tich, Tinh, Xa Huyen, Dan toc
//                    //2323	2323	0	XML1	MA_QUOCTICH	[130] Mã quốc tịch không nằm trong Danh mục phê duyệt
//                    //2324	2324	0	XML1	MA_NGHE_NGHIEP	[130] Mã nghề nghiệp ko nằm trong danh mục phê duyệt
//                    //2325    2325    0   XML1 MATINH_CU_TRU[130] Mã tỉnh cư trú không nằm trong Danh mục
//                    //2326    2326    0   XML1 MAHUYEN_CU_TRU[130] Mã huyện cứ trú không có trong danh mục
//                    //2330	2340	0	XML1	MAXA_CU_TRU	[130] Mã xã cư trú không có trong Danh mục phê duyệt
//                    //2673	3	XML1	MA_NGHE_NGHIEP	[3176] Mã nghề nghiệp không đúng trong Danh mục Phê duyệt
//                    RaiseError(!dmCHUNG.Any(x => x.LOAI == "QT" && x.MA == File.MA_QUOCTICH), 2323);
//                    RaiseError(!dmCHUNG.Any(x => x.LOAI == "NNG" && x.MA == File.MA_NGHE_NGHIEP), 2324);
//                    RaiseError(!dmCHUNG.Any(x => x.LOAI == "TINH" && x.MA == File.MATINH_CU_TRU), 2325);
//                    RaiseError(!dmCHUNG.Any(x => x.LOAI == "HUYEN" && x.MA == File.MAHUYEN_CU_TRU), 2326);
//                    RaiseError(!dmCHUNG.Any(x => x.LOAI == "XA" && x.MA == File.MAXA_CU_TRU), 2340);
//                    RaiseError(!dmCHUNG.Any(x => x.LOAI == "NNG3176" && x.MA == File.MA_NGHE_NGHIEP), 2673);

//                    #endregion

//                    //2675	3	XML1	DU_PHONG	[3176] Thông tin chiều cao bắt buộc phải nhập
//                    //2676    3   XML1 DU_PHONG[3176] Giá trị nhập có vấn đề, đề nghị kiểm tra lại
//                    if (File.DU_PHONG == null)
//                    {
//                        RaiseError(true, 2675);
//                    }
//                    if (File.DU_PHONG != null)
//                    {
//                        if (!double.TryParse(File.DU_PHONG.ToString(), out double d))
//                        {
//                            RaiseError(true, 2676);
//                        }
//                        else
//                        {
//                            double chieucao = double.Parse(File.DU_PHONG.ToString());
//                            if (chieucao < 0 || chieucao > 250)
//                            {
//                                RaiseError(true, 2676);
//                            }
//                        }

//                    }

//                    //2092	4	XML1	MA_TTDV	[130] Số định danh người đứng đầu không được để trống
//                    //2093  4   XML1    MA_TTDV[130] Số định dang không được lớn hơn 10 ký tự
//                    //2094  4   XML1    MA_TTDV[130] Số định danh không đúng trong Danh mục được phê duyệt
//                    //2158	4	XML1	MA_TTDV	[130] CCHN trong Mã định danh y tế không có vị trí 1 hoặc 2 hoặc 3

//                    //2674	3	XML1	MA_TTDV	[3716] Mã người đứng đầu (CCHN) Không khớp trong DM phê duyệt
//                    Field(x => x.MA_TTDV).IsEmpty().ThenError(2092).LengthOver(10).ThenError(2093);

//                    if (Field(x => x.MA_TTDV).NotEmpty())
//                    {
//                        //RaiseError(!dmBACSI.Any(x => x.MA_CSKCB == File.MA_CSKCB && x.VI_TRI == "1" && x.MA_BHXH == File.MA_TTDV), 2094);
//                        //RaiseError(!dmBACSI.Any(x => x.MA_BHXH == File.MA_TTDV), 2094);
//                        //RaiseError(!dmBACSI.Any(x => !string.IsNullOrEmpty(x.VI_TRI) && Uti.CodeContain("1;2;3",File.MA_TTDV) && x.MA_BHXH == File.MA_TTDV), 2158);

//                        var dmTTDVs = dmBACSI.Where(x => x.MA_BHXH == File.MA_TTDV);
//                        var dmTTDV3716 = dmBACSI.Where(x => x.MACCHN == File.MA_TTDV);
//                        //if (!RaiseError(!dmTTDVs.Any(), 2094))
//                        if (!dmTTDVs.Any())
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2094);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                            //string desc = $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}, Không tồn tại trong DM nhân viên, đề nghị kiểm tra lại Dữ liệu trên cổng và đồng bộ lại Danh mục";
//                            //RaiseError(true, 2094, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2094", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            RaiseError(true,
//                                        2094,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2094, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}, Không tồn tại trong DM nhân viên, đề nghị kiểm tra lại Dữ liệu trên cổng và đồng bộ lại Danh mục"
//                                        );
//                            //Chỉ kiểm tra khi có BS trong danh mục
//                            //RaiseError(!dmTTDVs.Any(x => Uti.HCodeContain(x.VI_TRI, "1;4;3")), 2158);

//                        }
//                        else
//                        //Linhist removed 10/02/26
//                        //{
//                        //    if (String.IsNullOrEmpty(dmTTDVs.FirstOrDefault().VI_TRI) || Uti.CodeContain(dmTTDVs.FirstOrDefault().VI_TRI, "1;3;4"))
//                        //    {
//                        //        //desc = $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}|Vị trí:{dmTTDVs.FirstOrDefault().VI_TRI}, Không có vị trí 1 hoặc 3 hoặc 4";
//                        //        //desc = $"Vị trí người đúng đầu phải là 1,3,4 theo đúng Quyết định 3176 và Quyết định 4750 Bộ Y tế, Quyết định 3618 Bảo hiểm xã hội. Kiểm tra lại giá trị Vị trí trên cổng Bảo hiểm và đồng bộ lại Danh mục nhân viên";
//                        //        //RaiseError(true, 2158, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2158", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        //        RaiseError(true,
//                        //                2158,
//                        //                    $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                        //                    $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                        //                    $"Nội dung lỗi: {(errorlist.TryGetValue(2158, out var Erinfor) ? Erinfor : "Không tìm thấy nội dung lỗi")}\n" +
//                        //                    $"Chi tiết: " +
//                        //                    $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}|Vị trí:{dmTTDVs.FirstOrDefault().VI_TRI}, Không có vị trí 1 hoặc 3 hoặc 4" +
//                        //                    $"\n Vị trí người đúng đầu phải là 1,3,4 theo đúng Quyết định 3176 và Quyết định 4750 Bộ Y tế, Quyết định 3618 Bảo hiểm xã hội. Kiểm tra lại giá trị Vị trí trên cổng Bảo hiểm và đồng bộ lại Danh mục nhân viên"
//                        //                );
//                        //    }
//                        //}
//                        //if (!RaiseError(!dmTTDV3716.Any(), 2674))
//                        if (!dmTTDV3716.Any())
//                        {
//                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2674);
//                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                            //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                            //string desc = $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}, Không tồn tại trong DM nhân viên, đề nghị kiểm tra lại Dữ liệu trên cổng và đồng bộ lại Danh mục";
//                            //RaiseError(true, 2674, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2674", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                            RaiseError(true,
//                                        2674,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2674, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}, Không tồn tại trong DM nhân viên, đề nghị kiểm tra lại Dữ liệu trên cổng và đồng bộ lại Danh mục"
//                                        );
//                            //Chỉ kiểm tra khi có BS trong danh mục
//                            //RaiseError(!dmTTDVs.Any(x => Uti.HCodeContain(x.VI_TRI, "1;4;3")), 2158);

//                        }
//                        else
//                        {
//                            var vitri = dmTTDVs.FirstOrDefault()?.VI_TRI;
//                            if (string.IsNullOrEmpty(vitri) || Uti.CodeContain(vitri, "1;3;4"))
//                            {
//                                //desc = $"Mã TTDV Người đúng đầu đơn vị:{File.MA_TTDV}|Vị trí:{dmTTDVs.FirstOrDefault().VI_TRI}, Không có vị trí 1 hoặc 3 hoặc 4";
//                                //desc = $"Vị trí người đúng đầu phải là 1,3,4 theo đúng Quyết định 3176 và Quyết định 4750 Bộ Y tế, Quyết định 3618 Bảo hiểm xã hội. Kiểm tra lại giá trị Vị trí trên cổng Bảo hiểm và đồng bộ lại Danh mục nhân viên";
//                                //RaiseError(true, 2158, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2158", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        2158,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2158, out var Erinfor) ? Erinfor : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Mã TTDV Người đúng đầu đơn vị: {File.MA_TTDV}|Vị trí: {vitri}, Không có vị trí 1 hoặc 3 hoặc 4" +
//                                            $"\n Vị trí người đúng đầu phải là 1,3,4 theo đúng Quyết định 3176 và Quyết định 4750 Bộ Y tế, Quyết định 3618 Bảo hiểm xã hội. Kiểm tra lại giá trị Vị trí trên cổng Bảo hiểm và đồng bộ lại Danh mục nhân viên"
//                                        );
//                            }
//                            //Chỉ kiểm tra khi có BS trong danh mục
//                            //RaiseError(!dmTTDVs.Any(x => Uti.HCodeContain(x.VI_TRI, "1;4;3")), 2158);
//                        }
//                    }

//                    //2327	2327	0	XML1	MA_KHOA	[130] Mã khoa không tồn tại trong Danh mục phê duyệt
//                    var Khoa = File.MA_KHOA.Split(';');
//                    foreach (var K in Khoa)
//                    {
//                        RaiseError(!dmKHOABV.Any(x => x.MA_KHOA == K), 2327);
//                    }

//                    //2336	2346	0	XML1	NGAY_TAI_KHAM	[130] Số lượng ngày tái khám không đúng với thông tin khai báo tại XML14
//                    //2337    2347    0   XML1 NGAY_TAI_KHAM[130] Ngày tái khám bắt buộc phải nhập khi khai báo XML14
//                    //2338    2348    0   XML1 NGAY_TAI_KHAM[130] Có thông tin ngày tái khám nhưng không có XML14
//                    //2339	2349	0	XML1	NGAY_TAI_KHAM	[130] Ngày tái khám không khớp với thông tin khai báo tại XML14
//                    //Nhờ Linh check, đoạn này XML1 Ngày tái khám thì yyyymmdd còn XML14 thì lại là yyyymmddhhmm
//                    if (Field(x => x.NGAY_TAI_KHAM).NotEmpty())
//                    {
//                        if (xml.XML14s is null || xml.XML14s.Count == 0)
//                        {
//                            RaiseError(true, 2338);
//                        }
//                        else
//                        {
//                            var ngaytk = File.NGAY_TAI_KHAM.Split(';');
//                            if (ngaytk.Count() != xml.XML14s.Count)
//                            {
//                                RaiseError(true, 2336);
//                            }
//                            else
//                            {

//                                foreach (var ngay in ngaytk)
//                                {
//                                    //if (!xml.XML14s.Any(x => x.NGAY_HEN_KL == ngay))
//                                    //{
//                                    //    RaiseError(true, 2339);
//                                    //}
//                                }
//                            }

//                        }
//                    }
//                    else if (xml.XML14s is not null && xml.XML14s.Count > 0)
//                    {
//                        RaiseError(true, 2337);
//                    }


//                    #region Validate QĐ 4400 và QĐ 4469
//                    //1600	1	XML1	GIOI_TINH	Mã Bệnh Nam Giới chỉ phù hợp với Bệnh nhân Nam (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (File.GIOI_TINH != 1 && dmDacBiet.ContainsKey("benhnamgioi"))
//                    {
//                        //Kiểm tra giới tính, Mã bệnh xem có phù hợp với bệnh nhân là Nam giới
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhnamgioi"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1600);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân có mã Bệnh:{b}|{binfor.TEN} là bệnh dành riêng cho Nam giới. Giới tính Bệnh nhân:{File.GIOI_TINH}|{ServiceConstants.GetValue(ServiceConstants.gioitinh,File.GIOI_TINH.ToString())}.Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1600, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1600", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                //RaiseError(true, 1600);
//                                RaiseError(true,
//                                        1600,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1600, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã Bệnh:{b}| là bệnh dành riêng cho Nam giới. Giới tính Bệnh nhân:{File.GIOI_TINH}|{ServiceConstants.GetValue(ServiceConstants.gioitinh, File.GIOI_TINH.ToString())}.Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );
//                            }
//                        }

//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhnamgioi"]))
//                        //{
//                        //    RaiseError(true, 1600);
//                        //}
//                    }

//                    //1635	1	XML1	GIOI_TINH	Mã Bệnh Nữ Giới chỉ phù hợp với Bệnh nhân Nữ (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (File.GIOI_TINH != 2 && dmDacBiet.ContainsKey("benhphunu"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhphunu"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1635);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân có mã Bệnh:{b}|{binfor.TEN} là bệnh dành riêng cho Phụ Nữ. Giới tính Bệnh nhân:{File.GIOI_TINH}|{ServiceConstants.GetValue(ServiceConstants.gioitinh, File.GIOI_TINH.ToString())}.Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1635, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1635", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1635,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1635, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có mã Bệnh:{b}| là bệnh dành riêng cho Phụ Nữ. Giới tính Bệnh nhân:{File.GIOI_TINH}|{ServiceConstants.GetValue(ServiceConstants.gioitinh, File.GIOI_TINH.ToString())}.Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );
//                            }
//                        }

//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhphunu"]))
//                        //{
//                        //    RaiseError(true, 1635);
//                        //}
//                    }
//                    //1601	1	XML1	GIOI_TINH	Mã Bệnh Sản Phụ Khoa chỉ phù hợp với Phụ nữ từ 9 - 60 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)	
//                    if (!(File.GIOI_TINH == 2 && Info.TUOI_BN >= 9d && Info.TUOI_BN <= 60d) && dmDacBiet.ContainsKey("benhsanphu"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhsanphu"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1601);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN},  là bệnh dành riêng cho Mã Bệnh Sản Phụ Khoa chỉ phù hợp với Phụ nữ từ 9 - 60. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1601, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1601", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1601,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1601, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Mã Bệnh Sản Phụ Khoa chỉ phù hợp với Phụ nữ từ 9 - 60. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhsanphu"]))
//                        //{

//                        //    RaiseError(true, 1601);
//                        //}
//                    }

//                    //1626	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Sơ Sinh chỉ phù hợp với Trẻ em từ 0 - 365 ngày tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.NGAY_TUOI_BN > 365d && dmDacBiet.ContainsKey("benhtreduoi1"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhtreduoi1"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1626);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN},  là bệnh dành riêng cho Trẻ Sơ Sinh chỉ phù hợp với Trẻ em từ 0 - 365 ngày tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1626, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1626", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1626,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1626, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Trẻ Sơ Sinh chỉ phù hợp với Trẻ em từ 0 - 365 ngày tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhtreduoi1"]))
//                        //{
//                        //    RaiseError(true, 1626);
//                        //}
//                    }
//                    //1630	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Sơ Sinh chỉ phù hợp với Trẻ em trên 27 ngày tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.NGAY_TUOI_BN < 27d && dmDacBiet.ContainsKey("benhtretren27n"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhtretren27n"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1630);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.NGAY_TUOI_BN} ngày tuổi có mã Bệnh:{b}|{binfor.TEN},  là bệnh dành riêng cho Trẻ Sơ Sinh chỉ phù hợp với Trẻ em trên 27 ngày tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1630, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1630", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1630,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1630, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.NGAY_TUOI_BN} ngày tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Trẻ Sơ Sinh chỉ phù hợp với Trẻ em trên 27 ngày tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhtretren27n"]))
//                        //{
//                        //    RaiseError(true, 1630);
//                        //}
//                    }

//                    //1627	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Nhỏ chỉ phù hợp với Trẻ em từ 0 - 2 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.TUOI_BN > 2d && dmDacBiet.ContainsKey("benhtreduoi2"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhtreduoi2"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1627);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN},  là bệnh dành riêng cho Trẻ Nhỏ chỉ phù hợp với Trẻ em từ 0 - 2 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1627, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1627", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1627,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1627, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Trẻ Nhỏ chỉ phù hợp với Trẻ em từ 0 - 2 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhtreduoi2"]))
//                        //{
//                        //    RaiseError(true, 1627);
//                        //}
//                    }
//                    //1628	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Lớn chỉ phù hợp với Trẻ em từ 0 - 10 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.TUOI_BN > 10d && dmDacBiet.ContainsKey("benhtreduoi10"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhtreduoi10"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1628);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN},  là bệnh dành riêng cho Trẻ Lớn chỉ phù hợp với Trẻ em từ 0 - 10 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1628, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1628", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1628,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1628, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Trẻ Lớn chỉ phù hợp với Trẻ em từ 0 - 10 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhtreduoi10"]))
//                        //{
//                        //    RaiseError(true, 1628);
//                        //}
//                    }
//                    //1629	1	XML1	GIOI_TINH	Mã Bệnh Thanh Thiếu Niên chỉ phù hợp với Thiếu niên từ 0 - 19 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.TUOI_BN > 19d && dmDacBiet.ContainsKey("benhtreduoi19"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhtreduoi19"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1629);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN}, là bệnh dành riêng cho Thanh Thiếu Niên chỉ phù hợp với Thiếu niên từ 0 - 19 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1629, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1629", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1629,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1629, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Thanh Thiếu Niên chỉ phù hợp với Thiếu niên từ 0 - 19 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhtreduoi19"]))
//                        //{
//                        //    RaiseError(true, 1629);

//                        //}
//                    }
//                    //1632	1	XML1	GIOI_TINH	Mã Bệnh Tuổi Dậy Thì chỉ phù hợp với Thiếu niên từ 8 - 19 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if ((Info.TUOI_BN < 8d || Info.TUOI_BN > 19d) && dmDacBiet.ContainsKey("benhdaythi"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhdaythi"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1632);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN } tuổi có mã Bệnh:{b}|{binfor.TEN}, là bệnh dành riêng cho Tuổi Dậy Thì chỉ phù hợp với Thiếu niên từ 8 - 19 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1632, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1632", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1632,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1632, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Tuổi Dậy Thì chỉ phù hợp với Thiếu niên từ 8 - 19 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhdaythi"]))
//                        //{
//                        //    RaiseError(true, 1632);
//                        //}
//                    }
//                    //1631	1	XML7	GIOI_TINH	Mã Bệnh Trẻ em chỉ phù hợp với Trẻ em trên 1 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.TUOI_BN < 1d && dmDacBiet.ContainsKey("benhtretren1"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhtretren1"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1631);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN}, là bệnh dành riêng cho Trẻ em chỉ phù hợp với Trẻ em trên 1 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1631, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1631", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1631,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1631, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Trẻ em chỉ phù hợp với Trẻ em trên 1 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhtretren1"]))
//                        //{
//                        //    RaiseError(true, 1631);
//                        //}
//                    }
//                    //1633	1	XML9	GIOI_TINH	Mã Bệnh Người Trưởng Thành chỉ phù hợp với Bệnh nhân trên 15 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.TUOI_BN < 15d && dmDacBiet.ContainsKey("benhlontren15"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhlontren15"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1633);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN}, là bệnh dành riêng cho Người Trưởng Thành chỉ phù hợp với Bệnh nhân trên 15 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1633, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1633", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1633,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1633, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Người Trưởng Thành chỉ phù hợp với Bệnh nhân trên 15 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );
//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhlontren15"]))
//                        //{
//                        //    RaiseError(true, 1633);
//                        //}
//                    }
//                    //1634	1	XML10	GIOI_TINH	Mã Bệnh Người Lớn chỉ phù hợp với Bệnh nhân trên 30 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
//                    if (Info.TUOI_BN < 30d && dmDacBiet.ContainsKey("benhlontren30"))
//                    {
//                        var mabenhinfo = Info.MA_BENH_ICD10.Split(";");
//                        foreach (var b in mabenhinfo)
//                        {
//                            if (Uti.CodeMatch(b, dmDacBiet["benhlontren30"]))
//                            {
//                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1634);
//                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                //var binfor = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == b) ?? new DMChung130 { MA = b, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" };
//                                //string desc = $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}|{binfor.TEN}, là bệnh dành riêng cho Người Lớn chỉ phù hợp với Bệnh nhân trên 30 tuổi. Đề nghị kiểm tra lại.";
//                                //desc = desc + $"\n Căn cứ Quyết định 4469 của Bộ Y Tế";
//                                //RaiseError(true, 1634, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "1634", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                RaiseError(true,
//                                        1634,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1634, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân {Info.TUOI_BN} tuổi có mã Bệnh:{b}| là bệnh dành riêng cho Người Lớn chỉ phù hợp với Bệnh nhân trên 30 tuổi. Đề nghị kiểm tra lại." +
//                                            $"\n Căn cứ Quyết định 4469 của Bộ Y Tế"
//                                        );

//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["benhlontren30"]))
//                        //{
//                        //    RaiseError(true, 1634);

//                        //}
//                    }
//                    //2343    0   XML1 MA_BENH_CHINH[130] [130] Mã bệnh không được nằm trong Mã Bệnh chính (Phụ lục A2 QĐ 4469 BYT)
//                    benhchinh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICDOCHINH" && x.MA == File.MA_BENH_CHINH);
//                    if (benhchinh != null)
//                    {
//                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2343);
//                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                        //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                        //string desc = $"Bệnh nhân có Mã bệnh chính:{File.MA_BENH_CHINH}, Không nằm trong Phụ lục A2: Các mã bệnh sử dụng là bệnh chính - Quyết định 4469 Bộ Y Tế";

//                        //RaiseError(true, 2343, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2343", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                        RaiseError(true,
//                                        2343,
//                                            $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                            $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2343, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                            $"Chi tiết: " +
//                                            $"Bệnh nhân có Mã bệnh chính:{File.MA_BENH_CHINH}, Không nằm trong Phụ lục A2: Các mã bệnh sử dụng là bệnh chính - Quyết định 4469 Bộ Y Tế"
//                                        );
//                    }
//                    //RaiseError(dmCHUNG.Any(x => x.LOAI == "ICDOCHINH" && File.MA_BENH_CHINH == x.MA), 2343);

//                    //if (dmDacBiet.ContainsKey("benhkolabenhchinh"))
//                    //{
//                    //    if (Uti.CodeMatch(File.MA_BENH_CHINH, dmDacBiet["benhkolabenhchinh"]))
//                    //    {
//                    //        RaiseError(true, 2343);
//                    //    }
//                    //}
//                    //2365	0	XML1	MA_BENH_CHINH	[130] Bệnh mãn tính dài ngày, mỗi lần khám bệnh của Bệnh nhân không nhỏ hơn 30 ngày
//                    if (dmDacBiet.ContainsKey("BenhManTinhDaiNgay"))
//                    {
//                        var mabenhs = dmDacBiet["BenhManTinhDaiNgay"].ToString();
//                        var hoso30Ngays = await _hosoRepository.GetListAsync(x => x.XML01ID != File.XML01ID && x.IsDelete != true && x.MA_CSKCB == File.MA_CSKCB && x.MA_BN == File.MA_BN && x.MA_LK != File.MA_LK && x.NGAY_TTOAN != null && x.NGAY_TTOAN.Value.AddDays(30) <= File.NGAY_TTOAN, query => query.OrderBy(x => x.NGAY_TTOAN));
//                        var hscheck = hoso30Ngays.Where(x => Uti.CodeMatch(x.MA_BENH_CHINH, mabenhs) || (x.MA_BENH_KT != null && Uti.CodeMatch(x.MA_BENH_KT, mabenhs))).ToList();
//                        if (hscheck.Any())
//                        {
//                            foreach (var mabenhInfor in Info.MA_BENH_ICD10.Split(";"))
//                            {
//                                if (Uti.CodeContain(mabenhInfor, dmDacBiet["BenhManTinhDaiNgay"]))
//                                {

//                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2365);
//                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
//                                    //var benh = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.MA_BENH_CHINH);
//                                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
//                                    //var mabenhinforname = dmCHUNG.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == mabenhInfor) ?? new DMChung130 { MA = mabenhInfor, TEN = "Không tìm thấy tên bệnh", LOAI = "ICD10" }; ;

//                                    //string desc = $"Bệnh nhân có mã bệnh mãn tính:{mabenhInfor}|{mabenhinforname.TEN}, đã đi khám vào ngày:{hscheck.FirstOrDefault().NGAY_VAO:dd/MM/yyyy HH:mm:ss}, khoảng cách: {(File.NGAY_TTOAN - hscheck.FirstOrDefault().NGAY_TTOAN).Value.TotalDays:F0}, chưa đủ 30 ngày. Kiểm tra lại";
//                                    //RaiseError(true, 2365, Uti.getErrorDescription(File.MA_BN, File.HO_TEN, "2365", errorInfo, File.MA_BENH_CHINH, benhchinhInfo, desc));
//                                    RaiseError(true,
//                                        2365,
//                                        $"Bệnh nhân: {File.HO_TEN} (Mã BN: {File.MA_BN})\n" +
//                                        $"Mã bệnh chính: {File.MA_BENH_CHINH} - {benhchinhInfo}\n" +
//                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2365, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
//                                        $"Chi tiết: " +
//                                        $"Bệnh nhân có mã bệnh mãn tính:{mabenhInfor}|, đã đi khám vào ngày:{hscheck.FirstOrDefault().NGAY_VAO:dd/MM/yyyy HH:mm:ss}, khoảng cách: {(File.NGAY_TTOAN - hscheck.FirstOrDefault().NGAY_TTOAN).Value.TotalDays:F0}, chưa đủ 30 ngày. Kiểm tra lại"
//                                        );

//                                }
//                            }
//                        }
//                        //if (Uti.CodeMatch(Info.MA_BENH_ICD10, dmDacBiet["BenhManTinhDaiNgay"]))
//                        //{
//                        //    var mabenhs = dmDacBiet["BenhManTinhDaiNgay"].ToString();

//                        //    RaiseError(hoso30Ngays.Any(x => Uti.CodeMatch(x.MA_BENH_CHINH, mabenhs) || Uti.CodeMatch(x.MA_BENH_KT, mabenhs)), 2365);
//                        //}
//                    }
//                    #endregion
//                }
//                //_watch.Stop();
//                //if (File.MA_CSKCB == "25001")
//                //{
//                //    Uti.HandleCronJobLog(_logger, "Giám định xong XML1, thời gian:" + _watch.Elapsed.TotalSeconds.ToString(), nameof(Validate));
//                //}
            

//                //Uti.HandleCronJobLog(_logger, $"[130] Kết thúc giam dinh ho so XML01", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML01)} time" + dt.ToString());

//                //Kiểm tra lỗi giờ hành chính
//            }
//            catch (Exception ex)
//            {
//                var fullError = new StringBuilder();
//                fullError.AppendLine($"❌ Validate130:XML1 lỗi tại MaBN: {File.MA_BN}, XML01ID: {File.XML01ID}, MaCSKCB: {File.MA_CSKCB}");
//                fullError.AppendLine($"⏱️ Thời điểm: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
//                fullError.AppendLine($"📄 Message: {ex.Message}");
//                fullError.AppendLine($"📌 StackTrace: {ex.StackTrace}");

//                //if (ex.InnerException != null)
//                //{
//                //    fullError.AppendLine($"🔍 InnerException: {ex.InnerException.Message}");
//                //    fullError.AppendLine($"🔍 Inner StackTrace: {ex.InnerException.StackTrace}");
//                //}

//                Uti.HandleCronJobLog(_logger, fullError.ToString(), nameof(Validate));

//            }



//            return true;
//        }
//    }
//}