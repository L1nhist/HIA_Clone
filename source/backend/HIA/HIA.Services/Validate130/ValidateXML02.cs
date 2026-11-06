using HIA.Core.Base;
using HIA.Data.Catalogs;
using HIA.Data.Records;
using HIA.Data.Rules;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace HIA.Services.Validate130;
public class ValidateXML02 : BaseValidator<XML02>
{
    //Linhist added 31/03/26 - Danh sách mã bệnh được kê thuốc ko quá 90 ngày theo TT26/2025
    private const string _MA_BENH_90_NGAY_TT26_2025 = "B18.1;B20.*;B21.*;B22.*;B23.*;B24.*;Z21;C50.*;C34.*;C73.*;D56.*;D57.*;D58.*;D59.*;D60.*;D61.*;D66.*;D67.*;D75.2;D76.2;D86.*;D89.2;E03.*;E05.*;E20.8;E21.*;E24.*;E26.*;E26.8;E27.*;E31.*;E83.0;E87.6;E89.0;E10;E10.2†;E10.3†E;10.4†;E10.5;E10.6;E10.7;E10.8;E10.9;E11;E11.2†;E11.3†;E11.4†;E11.5;E11.6;E11.7;E11.8;E11.9;E12;E12.2†;E12.3†;E12.4†;E12.5;E12.6;E12.7;E12.8;E12.9;E13;E13.2†;E13.3†;E13.4†;E13.5;E13.6;E13.7;E13.8;E13.9;E14;E14.2†;E14.3†;E14.4†;E14.5;E14.6;E14.7;E14.8;E14.9;E22.*;E23.0;E25.*;E27.1;E30.1;E35.1;E83.1;E85.*;G70.*;P05.1;Q87.1;F00.*;F02.*;F03.*;F04.*;F06.*;F07.*;F10.*;F20.*;F21.*;F22.*;F25.*;F31.*;F32.*;F33.*;F34.*;F40.*;F41.*;F42.*;F43.1;F43.2;F45.*;F60.*;F61.*;F62.*;F68.*;F70.*;F71.*;F72.*;F73.*;F74.*;F75.*;F76.*;F77.*;F78.*;F79.*;F80.*;F81.*;F82.*;F83.*;F84.*;F85.*;F86.*;F87.*;F88.*;F89.*;F90.*;F91.*;F92.*;F93.*;F94.*;F95.*;F96.*;F97.*;F98.*;F11.2; F11.3; F11.4; F11.5; F11.6; F11.7; F11.8; F11.9;F19.2; F19.3; F19.4; F19.5; F19.6; F19.7; F19.8;F01.*;G13.*;G20.*;G21.*;G24.*;G30.*;G35.*;G40.*;G70.0;G04.*;G09.*;G12.*;G36.0;G37.3;G50.0;G51.3;G53.0;G54.*;G61.0;G61.*;G71.*;G72.*;G80.*;I69.*;Q03.*;Q90.*;Q91.*;G04.*;G37.3;H46.*;Q85.0;H04.1;H16.*;H20.*;H21.*;H30.*;H33.4;H34.8;H35.*;H35.*;H40.*;H44.5;H44.1;H46.*;T26.2;Z94.7;I20.*;I21.*;I22.*;I23.*;I25.*;I31.1;I74.*;I80.*;I10.*;I42.*;Q25.7;Q26.*;Q25.7;I10.*;J45.*;J84.*;C96.6;J17.2;K75.4;K74.3;K59.0;K25.*;K26.*;Q44.7;S35.3;K76.6;E80.5;Q44.2;K76.0;E72.2;Q44.4;K80.*;I82.0;R18.*;K83.5;D73.1;K76.9;K71.0;I81.*;E74.0;R16.0;R77.0;K86.2;K83.5;Q44.5;Q44.6;K21.0;L10.*;L12.*;L13.0;L14.*;L20.*; L30.*;L26.*;L40.*;L44.0;L52.*;L88.*;L98.4;L41.*;L41.0;L41.1;L41.3;L41.4;L41.5;L41.8;L50.*;M32.*;M02.8;M02.9;M05.*;M07.3;M10.*;M11.*;M16.*;M17.*;M30.*;M31.*;M33.*;M34.*;M35.0;M43.*;M45.*;M47.*;M50.*;M75.0;M80.*;M84.1;M84.4;M85.0;M86.*;M87.0;M89.0;M90.7;M95.*;M00.*;M03.*;M06.*;M06.1;M08.*;M09.*;M13.*;M15.*;M18.*;M19.*;M24.*;M35.*;M36.*;M49.*;M51.*;M53.*;M54.*;M60.*;M61.*;M65.*;M75.*;M79.3;M79.7;M81.*;M82.*;M83.*;M88.*;N01.*;N02.*;N03.*;N04.*;N08.*;N18.*;N06.*;N07.*;N20.*;N25.*;N13.*;D69.0;N92.2;T95.*;Z69.64;Z69.65;Z94.*";

    private readonly ValidateAllXML23 Owner;
    //Linhist changed 20/03/26
    private readonly TimePeriod _officialTime;

    private readonly IEnumerable<DMThuoc> dmTHUOC;
    private readonly IEnumerable<DMKhoaBV>  dmKhoaBV;
    private readonly IEnumerable<DMBacSi> dmBACSI;
    private readonly IEnumerable<HDSDThuoc> hdsdTHUOC;
    private readonly IEnumerable<DMErrorMHTC> dmErrorMHTC;
    private readonly Dictionary<string, string> dmdacBiet;
    private XMLCommonInfo Info = new(new XML01());
    private readonly ILogger<dynamic>? _logger;

    private readonly IEnumerable<DMError130> dmError130;
    private readonly IEnumerable<DMChung130> DMChung;
    private string? curMATHUOC;
    private IEnumerable<DMThuoc>? cungMATHUOCs;
    private IEnumerable<HDSDThuoc>? cungHDSDs;
    private Dictionary<int, string> errorlist = new();
    string benhchinhInfo = string.Empty;

    private XML01 Xml01 => File.XML01 ?? new XML01();

    private Evaluation<XML02, string> StrField(Expression<Func<XML02, string?>> predicate)
        => (Evaluation<XML02, string>)(object)Field(predicate);

    public ValidateXML02
    (
        ValidateAllXML23 owner,
        //Linhist changed 20/03/26
        TimePeriod officialTime,

        Dictionary<string, string> dmDacBiet,
        IEnumerable<DMError130> dMError130s,
        IEnumerable<DMChung130> dMChung130s,
        IEnumerable<DMErrorMHTC> DMErrorMHTC,
        IEnumerable<DMKhoaBV> dMKhoaBVs,
        IEnumerable<DMThuoc> DMThuocs,
        IEnumerable<DMBacSi> DMBacSis,
        IEnumerable<HDSDThuoc> HDSDThuocs,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        Owner = owner;
        //Linhist changed 20/03/26
        _officialTime = officialTime;

        curMATHUOC = null;
        cungMATHUOCs = null;
        cungHDSDs = null;
        dmTHUOC = DMThuocs;
        dmBACSI = DMBacSis;
        hdsdTHUOC = HDSDThuocs;
        dmKhoaBV = dMKhoaBVs;
        dmErrorMHTC = DMErrorMHTC;
        dmdacBiet = dmDacBiet;
        dmError130 = dMError130s;
        DMChung = dMChung130s;
        _logger = logger;
    }

    public class TT222022
    {
        public string MA_THUOC { get; set; } = string.Empty;
        public string DUONG_DUNG { get; set; } = string.Empty;
    }

    public  bool IsInTimeRanges(string scheduleString, DateTime timeToCheck)
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
    private  bool TryParseTime(string input, out TimeSpan result)
    {
        return TimeSpan.TryParseExact(input, new[] { "hhmm", "hmm", "h\\:mm", "hh\\:mm" }, null, out result);
    }
    public  List<DayOfWeek>? ParseWeekdays(string? input)
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

    private int Validate_LieuDung130()
    {
        string? lieuDung = File.LIEU_DUNG;
        string donvi = File.DON_VI_TINH ?? "";
        double? soluong = File.SO_LUONG;

        if ("3.05;5.04;5.07;5.08;5.10;6.01;6.02;6.03;6.04;9.09;9.10;".Contains(File.DUONG_DUNG ?? string.Empty))
        {
            if(!(lieuDung ?? string.Empty).Contains("*") && !(lieuDung ?? string.Empty).Contains("x"))
            {
                return 2345;
            }
            return 0;
        }

        //Linhist added 21/04/22 bo kiem tra voi DON_VI_TINH la "Tub, Tuýp, Chai, Chai nhựa PPKB, Lọ, Chai/lọ, Bình xịt, Bình hít, Bơm tiêm"
        if (Uti.StrExists(donvi, true, "Chai nhựa PPKB", "Bình xịt", "Bình hít", "UI", "Đơn vị", "Bút tiêm")) return 0;
        //93  1   XML2 LIEU_DUNG   Liều dùng không được lớn hơn 255 ký tự
        //171 1   XML2 LIEU_DUNG   Liều dùng phải nằm trong thẻ<![CDATA[...]] >
        //2345	0	XML2	LIEU_DUNG	[13] Liều dùng không đúng định dạng 130
        if (string.IsNullOrEmpty(lieuDung?.Trim())) return 0;

        //Linhist added 07/07/22 bo kiem tra lieu dung voi thuoc 40.805

        //Linhist changed 27/05/26 -> sử dụng lại đoạn code:
        //update lại doan code
        if (donvi != "gram" && donvi != "gam" && donvi != "g")
        {
            lieuDung = lieuDung.ToLower().Replace(donvi.Trim().ToLower(), "");
        }
        lieuDung = lieuDung.ToLower().Replace(" ", "");
        //Linhist changed 31/08/23

        //Định dạng 1: Khám bệnh ngoại trú: 2 viên/lần * 2 lần/ngày * 5 ngày [4 viên/ngày]
        // Định dạng 2: Khám bệnh nội trú:  số lượng thuốc dùng trong một lần sử dụng * số lần trong ngày * 01 ngày [tổng số thuốc/ngày]

        //Linhist added 16/01/25
        //Định dạng 3: 12 gam * 1 thang * 1 ngày
        if (lieuDung.Contains("thang"))
        {
            return Validate_LieuDung130_YHCT_THANG_NGAY(lieuDung, donvi, soluong);
        }

        //Linhist removed 20/11/24
            if (lieuDung.Contains("lần"))
            {
                return Validate_LieuDung130_SO_LAN_NGAY(lieuDung, donvi, soluong);
            }

        // Định dạng 4: liều dùng của thuốc A trong ngày là: Sáng: 3 viên, Chiều: 2 viên, Tối: 1 viên [6 viên/ngày]
        // Định dạng 5: liều dùng của thuốc B trong 10 ngày là: Sáng: 2 viên, Chiều: 1 viên * 10 ngày [3 viên/ngày]
        if (lieuDung.Contains("sáng") || lieuDung.Contains("chiều") || lieuDung.Contains("tối") || lieuDung.Contains("trưa"))
        {
            if (lieuDung.Contains("sáng") && lieuDung.Contains("chiều") && lieuDung.Contains("tối") && lieuDung.Contains("trưa"))
            {
                return Validate_LieuDung130_SANG_TRUA_CHIEU_TOI(lieuDung,donvi,soluong);
            }    

            if (File.MA_CSKCB == "34014" || File.MA_CSKCB == "34002" || File.MA_CSKCB=="24017") return Validate_LieuDung130_SANG_CHIEU(lieuDung, donvi, soluong);
            if (File.MA_CSKCB == "34001" ) return Validate_LieuDung130_SANG_CHIEU_DKTB(lieuDung, donvi, soluong);
            if (lieuDung.Contains("sáng") && lieuDung.Contains("chiều") && lieuDung.Contains("tối"))
            {
                return Validate_LieuDung130_SANG_CHIEU_TOI(lieuDung, donvi, soluong);
            }
        }

        //Linhist changed 15/11/24 cap nhat bao loi lieu dung neu ko thuoc ca 3 dinh dang tren
        //2345	0	XML2	LIEU_DUNG	[13] Liều dùng không đúng định dạng 130
        if (lieuDung.ToLower().Contains("ui") || lieuDung.ToLower().Contains("iu")) return 0;
        return 0;
    }

    public int CountSubstring(string text, string value)
    {
        int count = 0;
        text = text.ToUpper();
        value = value.ToUpper();
        int minIndex = text.IndexOf(value, 0);
        while (minIndex != -1)
        {
            minIndex = text.IndexOf(value, minIndex + value.Length);
            count++;
        }
        return count;
    }
    public bool LDcheck2395(string lieudung)
    {
        //mg
        //ml
        var dvtmg = "mg";
        var dvtml = "ml";
        if (!lieudung.Contains(dvtml) || !lieudung.Contains(dvtmg)) return true;

        if (lieudung.Contains("lần"))
        {
            var lieus = lieudung.Split("*");
            if (lieus.Count() < 3) return true;

            if (lieus[0].Contains(dvtmg))
            {
                if (lieus[2].Contains("dvtmg"))
                {
                    return true;
                }
                else return false;

            }
            else
            {
                if (lieus[0].Contains(dvtml))
                {
                    if (lieus[2].Contains(dvtml))
                    {
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
        }

        if (lieudung.Contains("sáng"))
        {
            var lieus = lieudung.Split("*");
            var chk = "";
            foreach (var xl in lieus)
            {
                if(chk!="")
                {
                    if(!xl.Contains(chk)) return false;
                }    
                if (xl.Contains(dvtmg))
                {
                    chk = dvtmg;
                }
                else if(xl.Contains(dvtml))
                {
                    chk = dvtml;
                }
            }
            return true;
        }
        return false;
    }
    public bool checkCachDung (String cachdung, string dvt)
    {
        if (Uti.HCodeMach(dvt.Trim().ToLower(), "gói")) return true;
        
        if (!cachdung.Contains(dvt)) return false;
        return true;
     
    }
    public bool LDcheckDVT(string lieudung, string dvt)
    {
        if (Uti.HCodeMach(dvt.Trim().ToLower(),"gói")) return true;
        if (lieudung.Contains("lần"))
        {
            var lieus = lieudung.Split("*");
            if (lieus.Count() < 3) return true;
            if (lieus[0].Contains(dvt.ToLower().Trim()))
            {
                if (lieus[2].Contains(dvt.ToLower()))
                {
                    return true;
                }
                else return false;
                                       
            }
        }
        if (lieudung.Contains("sáng"))
        {
            var lieus = lieudung.Split("*");
            var chk = true;
            foreach(var xl in lieus)
            {
                if(!xl.Contains(dvt.ToLower().Trim())) chk= false;
            }
            return chk;
        }
        if(!lieudung.Contains(dvt)) return false;
        return true;
    }

    private int Validate_LieuDung130_SO_LAN_NGAY(string lieuDung, string donvi, double? soluong)
    {
        string[]? lieus = null;
        string[]? tongs = null;
        //1 Ống/lần * 2 lần/ngày * 1 ngày [2 Ống/ngày]
        //Định dạng 1: Khám bệnh ngoại trú: 2 viên/lần * 2 lần/ngày * 5 ngày [4 viên/ngày]
        //Định dạng 2: Khám bệnh ngoại trú: 2 viên/01lần * 2 lần/01ngày * 5 ngày [4 viên/ngày]
        int demNgay = 0;
        demNgay = CountSubstring(lieuDung, "ngày");
        if (demNgay != 3) return 2345;
        
        //2viên*2*5[4viên]
        lieus = lieuDung.Split("x*".ToCharArray());
        if (lieus.Length != 3) return 2345;
        if (lieus[2].Contains("`")) return 2345;
        lieus[0] = lieus[0].Replace("01lần", "").Replace("1lần", "").Replace("lần", "").Replace("01ngày", "").Replace("1ngày", "").Replace("ngày", "").Replace("/", "");
        lieus[1] = lieus[1].Replace("lần", "").Replace("01ngày", "").Replace("1ngày", "").Replace("ngày", "").Replace("/", "");
       
        lieus[0] = Uti.LeftNumberCharOnly(lieus[0]);
        lieus[1] = Uti.LeftNumberCharOnly(lieus[1]);
        //5 ngày [4 viên/ngày]
        lieus[2] = lieus[2].Replace("ngày", "").Replace("/", "");
        if (lieus[2].Contains("["))
        {

            tongs = lieus[2].Split("[".ToCharArray());
            if (tongs.Length != 2) return 2345;

            //5 ngày
            //4 viên
            tongs[0] = Uti.LeftNumberCharOnly(tongs[0]);
            tongs[1] = Uti.LeftNumberCharOnly(tongs[1]);
        }

        if (lieus == null) return 357;
        
        try
        {
            var soLuongCalc = decimal.Parse(lieus[0].Replace(",","."), CultureInfo.InvariantCulture) * decimal.Parse(lieus[1].Replace(",","."), CultureInfo.InvariantCulture);

            if (soLuongCalc == 0m) return 886;

            if (tongs != null)
            {
                var ngayCalc = decimal.Parse(tongs[0].Replace(",","."), CultureInfo.InvariantCulture);
                var tongCalc = decimal.Parse(tongs[1].Replace(",","."), CultureInfo.InvariantCulture);

                //2350	0	XML2	LIEU_DUNG	[130] Liều dùng sai công thức hướng dẫn tại QĐ 4750
                if (soLuongCalc != tongCalc) return 2350;

                //Linhist added 31/03/26
                //Linhist changed 140926
                if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2, 5, 6, 7, 8) &&  (Uti.CodeMatch(Xml01.MA_BENH_CHINH ?? string.Empty, _MA_BENH_90_NGAY_TT26_2025) || Uti.CodeMatch(Xml01.MA_BENH_KT ?? string.Empty, _MA_BENH_90_NGAY_TT26_2025)))
                {
                    if (ngayCalc > 90m) return 3969;
                }
                else

                //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
                if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2))
                {
                    if (!Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.263;40.30.268;40.277;40.269;40.30.270;20.20.273;40.268;40.269;40.277;"))
                    {
                        if (ngayCalc > 30m) return 1638;
                    }
                }
   
                soLuongCalc = soLuongCalc * ngayCalc;
            }

            if (soluong != null)
            {
                //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                //code riêng cho BV Thái Bình 
                if(File.MA_CSKCB == "34341" || File.MA_CSKCB == "27023")
                {
                    //Linhist changed 26/06/26
                    if (Uti.IsOverUp(soLuongCalc, (decimal?)soluong)) return 887;

                    //Linhist added 14/08/23
                    //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                    if (File.MA_KHOA != "K18")
                    {
                        //Linhist changed 26/06/26
                        if (Uti.IsUnderUp(soLuongCalc, (decimal?)soluong)) return 1636;
                    }
                }
                else if (donvi.ToLower() !="chai 60ml" && donvi.ToLower() != "túi" && donvi.ToLower()!="gam" && (File.DON_VI_TINH ?? string.Empty).ToLower() != "gói" && (File.DON_VI_TINH ?? string.Empty).ToLower() != "ống" && (File.DON_VI_TINH ?? string.Empty).ToLower() != "chai" && (File.DON_VI_TINH ?? string.Empty).ToLower().Normalize(NormalizationForm.FormC) != "lọ".Normalize(NormalizationForm.FormC))
                {
                    //Linhist changed 26/06/26
                    if (Uti.IsOverUp(soLuongCalc, (decimal?)soluong)) return 887;

                    //Linhist added 14/08/23
                    //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                    if (File.MA_KHOA != "K18")
                    {
                        //Linhist changed 26/06/26
                        if (Uti.IsUnderUp(soLuongCalc, (decimal?)soluong)) return 1636;
                    }
                }
            }
        }
        catch
        {
            return 2345;
        }

        return 0;
    }

    private int Validate_LieuDung130_SANG_CHIEU_DKTB(string lieuDung, string donvi, double? soluong)
    {
        //Áp dụng riêng cho ĐK Tỉnh Thái Bình
        // sáng 22IU, chiều 18IU [40IU/ngày]
        //4039	4	XML2	LIEU_DUNG	[3176] ĐK Tỉnh Thái Bình: Liều Dùng không đúng định dạng (Sáng: xx thuốc, Chiều:xx thuốc [Số lượng thuốc trong ngày]
        string[]? lieus = null;
        string[]? tongs = null;

        
        //Sáng: 22IU, Chiều 18IU [40IU/ngày]
        lieuDung = lieuDung.Replace("sáng", "").Replace("chiều", "").Replace("tối", "");
        
        //: 22IU,: 18IU [40IU/ngày]
        lieuDung = lieuDung.Replace(":", "").Replace(" ", "");
        
        //22IU,18IU [40IU/ngay]
        lieus = lieuDung.Split(",".ToCharArray());
        //22IU
        //18IU [40IU/ngày]
        if (lieus.Length < 2) return 4039;

        if (lieus.Length == 2)
        {
            lieus[0] = Uti.HLeftNumberCharOnly(lieus[0]);
            var lieumoiX = lieus[1];

            lieus[1] = Uti.HLeftNumberCharOnly(lieus[1]);
            
            lieumoiX = lieumoiX.Replace("ngày", "").Replace("/", "");
            if (lieumoiX.Contains("["))
            {

                tongs = lieumoiX.Split("[".ToCharArray());
                if (tongs.Length != 2) return 3039;

                tongs[0] = Uti.HLeftNumberCharOnly(tongs[0]);
                tongs[1] = Uti.HLeftNumberCharOnly(tongs[1]);

            }

        }

        try
        {
            var soLuongCalc = decimal.Parse(lieus[0].Replace(",", "."), CultureInfo.InvariantCulture) * decimal.Parse(lieus[1].Replace(",", "."), CultureInfo.InvariantCulture);

            if (soLuongCalc == 0m) return 886;

            if (tongs != null)
            {
                decimal ngayCalc = decimal.Parse(tongs[0].Replace(",", "."), CultureInfo.InvariantCulture);
                decimal tongCalc = decimal.Parse(tongs[1].Replace(",", "."), CultureInfo.InvariantCulture);

                //2350	0	XML2	LIEU_DUNG	[130] Liều dùng sai, Tổng số thuốc trong ngày không khớp với số viên/ lần * số lần/ngày
                if (tongCalc != decimal.Parse(lieus[0].Replace(",", ".")) + decimal.Parse(lieus[1].Replace(",", "."))) return 2350;

                //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
            }

            //    //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán

            //        //Linhist added 14/08/23
            //        //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
        }
        catch
        {
            return 4039;
        }

        return 0;
    }

    private int Validate_LieuDung130_SANG_TRUA_CHIEU_TOI (string lieudung, string donvi, double? soluong)
    {
        //Áp dụng cho Đại học Y Thái Bình và Trần Lãm
        string[]? lieus = null;
        string[]? tongs = null;
        decimal lieusang;
        decimal lieutrua;
        decimal lieuchieu;
        decimal lieutoi;

        //Sáng: 1 viên, Trưa: 0,5 viên, Chiều: 0,5 viên, Tối: 0,5 viên * 30 ngày [2,5 viên/ngày]

        lieudung = lieudung.Replace(",sáng", "$").Replace("sáng", "$").Replace(",chiều", "$").Replace("chiều", "$").Replace(",tối", "$").Replace("tối", "$").Replace(",trưa", "$").Replace("trưa", "$");
        //$:1$:0,5$:0,5$:0,5*30ngày[2,5/ngày]

        lieudung = lieudung.Replace(":", "").Replace(" ", "");
        // "$1$0,5$0,5$0,5*30ngày[2,5/ngày]"
        lieus = lieudung.Split("$".ToCharArray());
        
        if (lieus.Length < 4) return 4044;
        if(lieus.Length==5)
        {
            try
            {

                lieusang = decimal.Parse(lieus[1].Replace(",","."),CultureInfo.InvariantCulture);
                lieutrua = decimal.Parse(lieus[2].Replace(",", "."), CultureInfo.InvariantCulture);
                lieuchieu    = decimal.Parse(lieus[3].Replace(",", "."), CultureInfo.InvariantCulture);
                var lieumoi = lieus[4].Split("*");

                
                lieutoi = decimal.Parse(lieumoi[0].Replace(",", "."), CultureInfo.InvariantCulture);
                
                var lieumoiX = lieumoi[1];
                lieumoiX = lieumoiX.Replace("ngày", "").Replace("/", "");
                //"30ngày[2,5/ngày]"
                if (lieumoiX.Contains("["))
                {

                    tongs = lieumoiX.Split("[".ToCharArray());
                    if (tongs.Length != 2) return 4043;

                    tongs[0] = Uti.LeftNumberCharOnly(tongs[0]);
                    tongs[1] = Uti.LeftNumberCharOnly(tongs[1]);

                    if (lieutoi == 0 || lieusang == 0 || lieutrua == 0 || lieuchieu == 0) return 886;
                    var ngayCalc = decimal.Parse(tongs[0].Replace(",", "."), CultureInfo.InvariantCulture);
                    var tongCalc = decimal.Parse(tongs[1].Replace(",", "."), CultureInfo.InvariantCulture);

                    if (lieusang == lieutrua && lieusang == lieuchieu && lieusang == lieutoi)
                    {
                        return 4049;
                    }
                    if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2))
                    {
                        if (!Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.263;40.30.268;40.277;40.269;40.30.270;20.20.273;40.268;40.269;40.277;"))
                        {
                            if (ngayCalc > 30m) return 1638;
                        }
                    }
                    if ((decimal)(lieusang+ lieutrua+ lieuchieu+ lieutoi) != (decimal)tongCalc) return 2350;

                }
            }
            catch//(Exception ex)
            {
                return 4044;
            }

        }    

        return 0;
    }

    private int Validate_LieuDung130_SANG_CHIEU(string lieuDung, string donvi, double? soluong)
    {

        //haimaster update code 28/05/2026
        //// Regex cho tổng kết

        //4039	4	XML2	LIEU_DUNG	[3176] ĐK Tỉnh Thái Bình: Liều Dùng không đúng định dạng (Sáng: xx thuốc, Chiều:xx thuốc [Số lượng thuốc trong ngày]

        string[]? lieus = null;
        string[]? tongs = null;
        decimal lieu0;
        decimal lieu1;
        string lieus0;

        //Sáng: 3 viên, Chiều: 2 viên, Tối: 1 viên [6 viên/ngày]
        //Sáng: 22IU, Chiều 18IU [40IU/ngày]
        lieuDung = lieuDung.Replace(",sáng", "$").Replace("sáng", "$").Replace(",chiều", "$").Replace("chiều", "$").Replace(",tối", "$").Replace("tối", "$").Replace(",trưa", "$").Replace("trưa", "$");
        //: 3 viên, : 2 viên, : 1 viên [6 viên/ngày]
        //: 22IU,: 18IU * 30 ngày [40IU/ngày]
        lieuDung = lieuDung.Replace(":", "").Replace(" ", "");
        //3viên,2viên,1viên[6 viên/ngày]
        //22IU,18IU*30ngay[40IU/ngay]
        //Sáng: 0,02 lọ, Chiều: 0,015 lọ * 30 ngày [0,035 lọ/ngày]
        //"$:2$:1*30ngày[3/ngày]"
        lieus = lieuDung.Split("$".ToCharArray());
        //22IU
        //18IU*30 ngày [40IU/ngày]
        //"$2$1*30ngày[3/ngày]"
        if (lieus.Length < 2) return 4040;
        try
        {
            //case chỉ có Sáng hoặc trưa, hoặc chiều hoặc tối.
            try
            {
                if (lieus.Length == 2)
                {
                    var lieumoi = lieus[1].Split("*");

                    lieus0 = Uti.LeftNumberCharOnly(lieumoi[0]);
                    var lieumoiX = lieumoi[1];
                    lieumoiX = lieumoiX.Replace("ngày", "").Replace("/", "");
                    if (lieumoiX.Contains("["))
                    {

                        tongs = lieumoiX.Split("[".ToCharArray());
                        if (tongs.Length != 2) return 4043;

                        tongs[0] = Uti.LeftNumberCharOnly(tongs[0]);
                        tongs[1] = Uti.LeftNumberCharOnly(tongs[1]);
                        lieu0 = decimal.Parse(lieus0.Replace(",", "."), CultureInfo.InvariantCulture);
                        if (lieu0 == 0) return 886;
                        var ngayCalc = decimal.Parse(tongs[0].Replace(",", "."), CultureInfo.InvariantCulture);
                        var tongCalc = decimal.Parse(tongs[1].Replace(",", "."), CultureInfo.InvariantCulture);

                        if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2))
                        {
                            if (!Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.263;40.30.268;40.277;40.269;40.30.270;20.20.273;40.268;40.269;40.277;"))
                            {
                                if (ngayCalc > 30m) return 1638;
                            }
                        }
                        if ((decimal)lieu0 != (decimal)tongCalc) return 2350;

                    }

                }
            }catch//(Exception ex)
            {
                return 4043;
            }
            //case có đủ sáng chiều, sáng trưa hoặc sáng tối
            if (lieus.Length == 3)
            {
                lieus[1] = Uti.LeftNumberCharOnly(lieus[1]);

                var lieumoi = lieus[2].Split("*");

                lieus[2] = Uti.LeftNumberCharOnly(lieumoi[0]);
                var lieumoiX = lieumoi[1];
                lieumoiX = lieumoiX.Replace("ngày", "").Replace("/", "");
                if (lieumoiX.Contains("["))
                {

                    tongs = lieumoiX.Split("[".ToCharArray());
                    if (tongs.Length != 2) return 4040;

                    tongs[0] = Uti.LeftNumberCharOnly(tongs[0]);
                    tongs[1] = Uti.LeftNumberCharOnly(tongs[1]);
                }
                lieu0 = decimal.Parse(lieus[1].Replace(",", "."), CultureInfo.InvariantCulture);
                lieu1 = decimal.Parse(lieus[2].Replace(",", "."), CultureInfo.InvariantCulture);
                if(lieu0 == lieu1)
                {
                    return 4049;
                }
                try
                {
                    //Linhist changed 28/05/26 - sửa cách tính số lượng thuốc

                    var soLuongCalc = lieu0 + lieu1;

                    if (soLuongCalc == 0m) return 886;

                    if (tongs != null)
                    {
                        var ngayCalc = decimal.Parse(tongs[0].Replace(",", "."), CultureInfo.InvariantCulture);
                        var tongCalc = decimal.Parse(tongs[1].Replace(",", "."), CultureInfo.InvariantCulture);

                        //2350	0	XML2	LIEU_DUNG	[130] Liều dùng sai, Tổng số thuốc trong ngày không khớp với số viên/ lần * số lần/ngày
                        if (tongCalc != soLuongCalc) return 2350;

                        //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
                        if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2))
                        {
                            if (!Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.263;40.30.268;40.277;40.269;40.30.270;20.20.273;40.268;40.269;40.277;"))
                            {
                                if (ngayCalc > 30m) return 1638;
                            }
                        }
                        soLuongCalc = soLuongCalc * ngayCalc;
                    }

                    if (soluong != null)
                    {
                        //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                        if (donvi.ToLower() != "gói" && donvi.ToLower() != "túi" && donvi.ToLower() != "gam")
                        {
                            //Linhist changed 26/06/26
                            if (Uti.IsOverUp(soLuongCalc, (decimal?)soluong, 3)) return 887;

                            //Linhist added 14/08/23
                            //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                            if (File.MA_KHOA != "K18")
                            {
                                //Linhist changed 26/06/26
                                if (Uti.IsUnderUp(soLuongCalc, (decimal?)soluong, 3)) return 1636;
                            }
                        }
                    }
                }
                catch
                {
                    return 4040;
                }

            }
        }
        catch //(Exception ex)
        {
            return 4040;
        }

       
        return 0;
    }

    private int Validate_LieuDung130_SANG_CHIEU_TOI(string lieuDung, string donvi, double? soluong)
    {
        //4041	4	XML2	LIEU_DUNG	[3176] Liều dùng ko đúng định dạng 3176 (Sáng: xx Viên, Chiều: xx Viên, Tối:xx Viên [xx viên/ngày]

        string[]? lieus = null;
        string[]? tongs = null;
        if (!lieuDung.Contains(",")) return 4041;

        //Sáng: 3 viên, Chiều: 2 viên, Tối: 1 viên [6 viên/ngày]
        lieuDung = lieuDung.Replace("sáng", "").Replace("chiều", "").Replace("tối", "");
        //: 3 viên, : 2 viên, : 1 viên [6 viên/ngày]
        lieuDung = lieuDung.Replace(":", "").Replace(" ", "");
        //3viên,2viên,1viên[6 viên/ngày]
        lieus = lieuDung.Split(",".ToCharArray());
        //3viên
        //2viên
        //1viên[6 viên/ngày]
        if (lieus.Length < 2) return 4041;

        if ((lieus.Length==3))
        {
            lieus[0] = Uti.LeftNumberCharOnly(lieus[0]);
            lieus[1] = Uti.LeftNumberCharOnly(lieus[1]);

            //1viên[6viên/ngày]
            if (lieus[2].Contains("["))
            {
                lieus[2] = lieus[2].Replace("ngày", "");

                //1viên[6viên/ngày]
                tongs = lieus[2].Split("[".ToCharArray());
                if (tongs.Length != 2) return 4041;

                //1viên
                //6viên/ngày
                lieus[2] = Uti.LeftNumberCharOnly(tongs[0]);

                tongs[0] = "1";
                tongs[1] = Uti.LeftNumberCharOnly(tongs[1]);

            }   
        }

        if(lieus.Length==2)
        {
            //3viên
            
            //1viên[6 viên/ngày]
            lieus[0] = Uti.LeftNumberCharOnly(lieus[0]);

            //1viên[6viên/ngày]
            if (lieus[1].Contains("["))
            {
                lieus[1] = lieus[1].Replace("ngày", "");

                
                //1viên[6viên/]
                tongs = lieus[1].Split("[".ToCharArray());
                if (tongs.Length != 2) return 4041;

                //1viên
                //6viên/ngày
                lieus[1] = Uti.LeftNumberCharOnly(tongs[0]);

                tongs[0] = "1";
                tongs[1] = Uti.LeftNumberCharOnly(tongs[1]);

            }
        }

        try
        {
        var soLuongCalc = 0d;
            if (lieus.Length==3) {  soLuongCalc = double.Parse(lieus[0], CultureInfo.InvariantCulture) + double.Parse(lieus[1], CultureInfo.InvariantCulture) + double.Parse(lieus[2], CultureInfo.InvariantCulture); }
            if (lieus.Length == 2) { soLuongCalc = double.Parse(lieus[0], CultureInfo.InvariantCulture) + double.Parse(lieus[1], CultureInfo.InvariantCulture); }

            if (soLuongCalc == 0d) return 886;

            if (tongs != null)
            {
                double tongCalc = double.Parse(tongs[1], CultureInfo.InvariantCulture);
                //2350	0	XML2	LIEU_DUNG	[130] Liều dùng sai công thức hướng dẫn tại QĐ 4750
                if (soLuongCalc != tongCalc) return 2350;
            }

            if (soluong != null)
            {
                //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán

                if (donvi.ToLower() != "gói" && donvi.ToLower()!="lọ" && donvi.ToLower() != "túi" && donvi.ToLower()!="gam")
                {
                    //Linhist changed 26/06/26
                    if (Uti.IsOverUp(soLuongCalc, soluong ?? 0d)) return 887;
                }

                //Linhist added 14/08/23
                //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                if (Uti.NumExists(Xml01.MA_LOAI_KCB, 3) && File.MA_KHOA != "K18")
                {
                    //Linhist changed 26/06/26
                    if (Uti.IsUnderUp(soLuongCalc, soluong)) return 1636;
                }

                //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
                if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2))
                {
                    if (soluong / soLuongCalc > 30d) return 1638;
                }
            }
        }
        catch
        {
            return 4041;
        }

        return 0;
    }

    //Linhist added 16/01/25
    private int Validate_LieuDung130_YHCT_THANG_NGAY(string lieuDung, string donvi, double? soluong)
    {
        string[]? lieus = null;
        //1 gam * 2 thang * 1 ngày
        //1g * 2 thang * 1 ngày
        //4042	4	XML2	LIEU_DUNG	[3176] Liều dùng thuốc Y Học Cổ truyền không đúng định dạng theo QĐ 3176

        lieus = lieuDung.Replace("thang", "").Replace(donvi.ToLower(), "").Replace("gram", "").Replace("gam","").Replace("ngày", "").Replace("g", "").Split("x*".ToCharArray());
        // Sau khi đã chuẩn hóa chuỗi và gán vào lieus[0]
        string value = lieus[0].Trim();

        // Kiểm tra có phải số nguyên không
        if (!int.TryParse(value, out int result))
        {
            // Nếu không phải số nguyên thì báo lỗi
            return 3997; 
        }

        if (lieus.Length != 3) return 4042;

        try
        {
            var soLuongCalc = double.Parse(lieus[0], CultureInfo.InvariantCulture) * double.Parse(lieus[1], CultureInfo.InvariantCulture) * double.Parse(lieus[2], CultureInfo.InvariantCulture);

            if (soLuongCalc == 0d) return 886;

            if (soluong != null)
            {
                //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                if (donvi.ToLower() != "gói" && donvi.ToLower() != "lọ" && donvi.ToLower() != "túi")
                {
                    //Linhist changed 26/06/26
                    if (Uti.IsOverUp(soLuongCalc, soluong)) return 887;

                    //Linhist added 14/08/23
                    //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                    //Linhist changed 26/06/26
                    if (Uti.IsUnderUp(soLuongCalc, soluong)) return 1636;
                    
                    if (File.MA_KHOA != "K18")
                    {
                        //Linhist changed 26/06/26
                        if (Uti.IsUnderUp(soLuongCalc, soluong)) return 1636;
                    }
                }
            }
        }
        catch
        {
            return 4042;
        }

        return 0;
    }

    public string TT_THAU_RT(string TT_THAU, string MA_CSKCB, string LoaiThau)
    {
        string[] mts = TT_THAU.Split(';');
        if (mts.Length == 4) return TT_THAU;
        if (mts.Length == 5)
        {
            if (LoaiThau == "4")
            {
                return TT_THAU;
            }
            if(LoaiThau == "mua sắm theo kết quả trúng thầu đơn vị khác")
            {
                return TT_THAU;
            }
            else return (TT_THAU = $"{mts[0]};{mts[1]};{mts[2]};{mts[3]}");
        }
        return TT_THAU;
    }
    public bool Check_TT_THAU(string TT_THAU)
    {
        if(string.IsNullOrEmpty(TT_THAU)) return false;
        string[] mts = TT_THAU.Split(';');
        if (mts.Length == 4) return true;
        if(mts.Length==5) return true;
        return false;
    }

    public async Task<bool> Validate(XML02 xml, XMLCommonInfo info)
    {
        Info = info;
        return await Validate(xml);
    }

    //Linhist changed 31/07/26
    private static bool IsNumberOnly(string value)
    {
        foreach (var c in value.Trim())
        {
            if (!char.IsDigit(c) && c != '.' && c != ',') return true;
        }
        return false;
    }

    private static readonly string[] _boqua_YHCT = new string[] { "gói", "lọ", "túi" };
    private static readonly string[] _boqua_NGAY = new string[] { "chai 60ml", "chai", "gam", "ống" };
    private static readonly string[] _boqua_DONVI = new string[] { "chai", "lọ", "túi" };

    public void Validate_LieuDung()
    {
        var lieuDung = File.LIEU_DUNG?.Trim().ToLower() ?? string.Empty;
        var donVi = File.DON_VI_TINH?.Trim().ToLower() ?? string.Empty;
        var soLuong = File.SO_LUONG;

        //Nhung trường hợp sau đây thì không cần check liều dùng:
        if (string.IsNullOrEmpty(lieuDung)) return;
        if (Uti.StrExists(donVi, true, "chai nhựa ppkb", "bình xịt", "bình hít", "ui", "đơn vị", "bút tiêm")) return;
        //Linhist added 280826
        if (Uti.StrExists(donVi, true, "lọ", "bình", "bình xịt", "bình hít") && (lieuDung.Contains("liều xịt") || lieuDung.Contains("nhát xịt"))) return;

        //93  1   XML2 LIEU_DUNG   Liều dùng không được lớn hơn 255 ký tự
        //171 1   XML2 LIEU_DUNG   Liều dùng phải nằm trong thẻ<![CDATA[...]] >

        //2345	0	XML2	LIEU_DUNG	[13] Liều dùng không đúng định dạng 130
        if ("3.05;5.04;5.07;5.08;5.10;6.01;6.02;6.03;6.04;9.09;9.10;".Contains(File.DUONG_DUNG ?? string.Empty))
        {
            RaiseError(!lieuDung.Contains("*") && !(lieuDung ?? string.Empty).Contains("x"), 2345);
        }

        //4041	4	XML2	LIEU_DUNG	[3176] Liều dùng ko đúng định dạng 3176 (Sáng: xx Viên, Chiều: xx Viên, Tối:xx Viên [xx viên/ngày]
        var error = LieuDung.TryParse(lieuDung ?? string.Empty, donVi, out var chuanLD);
        if (error == 2394)
        {
            if (!(lieuDung ?? string.Empty).Contains("ui") && !(lieuDung ?? string.Empty).Contains("iu"))
            {
                //2394	4	XML2	LIEU_DUNG	[130] Đơn vị tính trong liều dùng không đúng với Đơn vị tính của thuốc
                if ((donVi != "ống" || Xml01.MA_CSKCB == "10007") && !_boqua_DONVI.Contains(donVi))
                {
                        RaiseError(true,
                            2394,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2394, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Liều dùng thuốc:{File.LIEU_DUNG}, Đơn vị tính thuốc:{File.DON_VI_TINH}, Không đúng với đơn vị tính của thuốc, đề nghị xem lại"
                            );
                }
            }
        }
        else if (error > 0)
        {
            RaiseError(!_boqua_YHCT.Contains(donVi) && !_boqua_NGAY.Contains(donVi), error);
        }

        //Không parse được liều dùng thì không cần check tiếp
        if (chuanLD == null || chuanLD.Format == 0)
        {
            return;
        }

        // Format: ## gam * ## thang * ## ngay
        if (chuanLD.Format == 1)
        {
            if (chuanLD.TongSo > 0m && soLuong != null && !_boqua_YHCT.Contains(donVi))
            {
                //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                RaiseError(Uti.IsOverUp(chuanLD.TongSo, (decimal?)soLuong), 887);

                //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                RaiseError(Uti.IsUnderUp(chuanLD.TongSo, (decimal?)soLuong), 1636);
            }
        }
        //Format: ## lần/ngày * ## ngày
        //Format: ## viên/lần * ## lần/ngày * ## ngày [## viên/ngày]
        else if (chuanLD.Format == 2 || chuanLD.Format == 3)
        {
            //Linhist changed 140926
            if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2, 5, 6, 7, 8) && (Uti.CodeMatch(Xml01.MA_BENH_CHINH ?? string.Empty, _MA_BENH_90_NGAY_TT26_2025) || Uti.CodeMatch(Xml01.MA_BENH_KT ?? string.Empty, _MA_BENH_90_NGAY_TT26_2025)))
            {
                RaiseError(chuanLD.SoNgay > 90m, 3969);
            }
            //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
            else if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2) && !Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.263;40.30.268;40.277;40.269;40.30.270;20.20.273;40.268;40.269;40.277;"))
            {
                RaiseError(chuanLD.SoNgay > 30m, 1638);
            }

            if (chuanLD.TongSo > 0 && soLuong != null && (File.MA_CSKCB == "34341" || File.MA_CSKCB == "27023" || File.MA_CSKCB == "10061" || (!_boqua_YHCT.Contains(donVi) && !_boqua_NGAY.Contains(donVi))))
            {
                //Linhist added 080926 - ko kiểm tra với đường dùng 3.05 (dùng ngoài) - trường hợp thuốc là Cồn
                //Linhist changed 140926
                if (File.DUONG_DUNG != "3.05" && File.DUONG_DUNG != "5.07" && File.DUONG_DUNG != "6.01")
                {
                    //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                    RaiseError(Uti.IsOverUp(chuanLD.TongSo, (decimal?)soLuong), 887);
                }

                //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                if (File.MA_KHOA != "K18")
                {
                    RaiseError(Uti.IsUnderUp(chuanLD.TongSo, (decimal?)soLuong), 1636);
                }
            }
        }

        //Format: Sáng: ## viên, Chiều: ## viên * ## ngày [## viên/ngày]
        //Format: Sáng: ## viên, Trưa: ## viên, Chiều: ## viên, Tối: ## viên * ## ngày [## viên/ngày]
        else if (chuanLD.Format == 4)
        {
            //Linhist changed 240926 - chỉ DKTB ko có số ngày 
            if (error == 0)
            {
                RaiseError(error == 0, 4041);

                if (chuanLD.TongSo > 0m && soLuong != null && !_boqua_YHCT.Contains(donVi))
                {
                    //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                    RaiseError(Uti.IsOverUp(chuanLD.TongSo, (decimal?)soLuong, 3), 887);

                    //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                    if (File.MA_KHOA != "K18")
                    {
                        RaiseError(Uti.IsUnderUp(chuanLD.TongSo, (decimal?)soLuong, 3), 1636);
                    }
                }
            }
            else
            {
                //Báo lỗi cho DKTB nếu ko parse đc
                RaiseError(error == 4041, 4039);
            }
        }
        else if (chuanLD.Format == 5)
        {
            //Linhist changed 240926
            RaiseError(chuanLD.SoNgay == 0, 886);

            //Linhist changed 240926
            var demBuoi = (chuanLD.Sang > 0 ? 1 : 0) + (chuanLD.Trua > 0 ? 1 : 0) + (chuanLD.Chieu > 0 ? 1 : 0) + (chuanLD.Toi > 0 ? 1 : 0);
            if (chuanLD.Trua > 0)
            {
                //Chỉ Trần lãm có trưa
                RaiseError(error == 0, 4041);

                //Báo lỗi cho Trần lãm nếu liều dùng parse ko đc
                    RaiseError(demBuoi == 1, 4043);
                    RaiseError(demBuoi == 2, 4040);
                    RaiseError(4044);
            }
            else
            {
                //Không đủ sáng chiều tối
                RaiseError(error == 0 && demBuoi != 3, 4041);
            }

            //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
            if (Uti.NumExists(Xml01.MA_LOAI_KCB, 1, 2) && !Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.263;40.30.268;40.277;40.269;40.30.270;20.20.273;40.268;40.269;40.277;"))
            {
                RaiseError(chuanLD.SoNgay > 30m, 1638);
            }

            if (chuanLD.TongSo > 0m && soLuong != null && !_boqua_YHCT.Contains(donVi))
            {
                //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                RaiseError(Uti.IsOverUp(chuanLD.TongSo, (decimal?)soLuong, 3), 887);

                //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
                if (File.MA_KHOA != "K18")
                {
                    RaiseError(Uti.IsUnderUp(chuanLD.TongSo, (decimal?)soLuong, 3), 1636);
                }
            }
        }
    }

    public override async Task<bool> Validate(XML02 xml)
    {

        List<TT222022> danhSachThuoc = new List<TT222022>()
        {
            new TT222022 { MA_THUOC = "40.186", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.187", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.188", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.189", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.30.247", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.249", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.251", DUONG_DUNG = "2.10;1.01;6.04" },
            new TT222022 { MA_THUOC = "40.253", DUONG_DUNG = "1.01;2.10" },
            new TT222022 { MA_THUOC = "40.257", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.265", DUONG_DUNG = "2.10;1.01" },
            new TT222022 { MA_THUOC = "Không tìm thấy", DUONG_DUNG = "1.01" },
            new TT222022 { MA_THUOC = "40.30.284", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "40.30.286", DUONG_DUNG = "2.10" },
            new TT222022 { MA_THUOC = "Không tìm thấy", DUONG_DUNG = "1.01" },
            new TT222022 { MA_THUOC = "40.30.303", DUONG_DUNG = "1.01" },
            new TT222022 { MA_THUOC = "40.253", DUONG_DUNG = "1.01" },
            new TT222022 { MA_THUOC = "40.1011", DUONG_DUNG = "2.15" },
            new TT222022 { MA_THUOC = "Không tìm thấy", DUONG_DUNG = "2.15" },
            new TT222022 { MA_THUOC = "40.1013.2", DUONG_DUNG = "2.15" },
            new TT222022 { MA_THUOC = "40.1013", DUONG_DUNG = "2.15" },
            new TT222022 { MA_THUOC = "40.1039", DUONG_DUNG = "1.01" }
        };
        //Kiem tra neu XML null thi ko chay tiep nua
        if (!await base.Validate(xml)) return false;
        var duongDungDict = new Dictionary<string, string>
            {
                { "1.01", "Uống" },
                { "1.02", "Ngậm" },
                { "1.03", "Nhai" },
                { "1.04", "Đặt dưới lưỡi" },
                { "1.05", "Ngậm dưới lưỡi" },
                { "2.01", "Tiêm bắp" },
                { "2.02", "Tiêm dưới da" },
                { "2.03", "Tiêm trong da" },
                { "2.04", "Tiêm tĩnh mạch" },
                { "2.05", "Tiêm truyền tĩnh mạch" },
                { "2.06", "Tiêm vào ổ khớp" },
                { "2.07", "Tiêm nội nhãn cầu" },
                { "2.08", "Tiêm trong dịch kính của mắt" },
                { "2.09", "Tiêm vào các khoang của cơ thể" },
                { "2.10", "Tiêm" },
                { "2.11", "Tiêm động mạch khối u" },
                { "2.12", "Tiêm vào khoang tự nhiên" },
                { "2.13", "Tiêm vào khối u" },
                { "2.14", "Truyền tĩnh mạch" },
                { "2.15", "Tiêm truyền" },
                { "3.01", "Bôi" },
                { "3.02", "Xoa ngoài" },
                { "3.03", "Dán trên da" },
                { "3.04", "Xịt ngoài da" },
                { "3.05", "Dùng ngoài" },
                { "4.01", "Đặt âm đạo" },
                { "4.02", "Đặt hậu môn" },
                { "4.03", "Thụt hậu môn - trực tràng" },
                { "4.04", "Đặt" },
                { "4.05", "Đặt tử cung" },
                { "4.06", "Thụt" },
                { "5.01", "Phun mù" },
                { "5.02", "Dạng hít" },
                { "5.03", "Bột hít" },
                { "5.04", "Xịt" },
                { "5.05", "Khí dung" },
                { "5.06", "Đường hô hấp" },
                { "5.07", "Xịt mũi" },
                { "5.08", "Xịt họng" },
                { "5.09", "Thuốc mũi" },
                { "5.10", "Nhỏ mũi" },
                { "5.11", "Đường nội khí quản" },
                { "6.01", "Nhỏ mắt" },
                { "6.02", "Tra mắt" },
                { "6.03", "Thuốc mắt" },
                { "6.04", "Nhỏ tai" },
                { "9.01", "Áp ngoài da" },
                { "9.02", "Áp sát khối u" },
                { "9.03", "Bình khí lỏng hoặc nén" },
                { "9.04", "Bình khí nén" },
                { "9.05", "Bôi trực tràng" },
                { "9.06", "Đánh tưa lưỡi" },
                { "9.07", "Cấy vào khối u" },
                { "9.08", "Chiếu ngoài" },
                { "9.09", "Dung dịch" },
                { "9.10", "Dung dịch rửa" },
                { "9.11", "Dung dịch thẩm phân" },
                { "9.12", "Phun" },
                { "9.13", "Túi" },
                { "9.14", "Hỗn dịch" },
                { "9.15", "Bột đông khô để pha hỗn dịch" },
                { "9.16", "Phức hợp lipid" },
                { "9.17", "Liposome" },
                { "9.18", "Polymeric micelle" },
                { "9.19", "Tại chỗ (ngâm vào khoang màng bụng)" }
            };

        try
        {
            benhchinhInfo = DMChung.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == Xml01.MA_BENH_CHINH)?.TEN ?? "Không có thông tin bệnh";

            if (File.MA_NHOM == 7) return true;

            if (!string.IsNullOrEmpty(File.MA_THUOC) && danhSachThuoc?.Any(x => x.MA_THUOC == File.MA_THUOC && x.DUONG_DUNG != null && x.DUONG_DUNG.Contains(File.DUONG_DUNG ?? string.Empty)) == true)
            {
                if (Xml01.MA_DOITUONG_KCB != "2")
                {
                    if (Xml01.XML05s?.Any(x => x.HOI_CHAN != null && x.HOI_CHAN.Length != 0 && x.THOI_DIEM_DBLS <= File.NGAY_YL) == true)
                    {
                        RaiseError(true,
                            4097,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(4097, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê thuốc {File.MA_THUOC}, nhưng không phải trường hợp cấp cứu, không có nội dung hội chẩn cho việc sử dụng thuốc trước thời điểm y lệnh thuốc"
                            );
                    }    
                }
            }

            //khai bao kiem tra thong tin ham luong & lieu dung null trong DM phe duyet
            var ichkHamLuong = false;
            var ichkDuongdung = false;
            var lieuDung = (File.LIEU_DUNG ?? string.Empty).Replace(" ", "");
            //2952	3	XML2	LIEU_DUNG	[3176] Liều dùng Oxy không đúng hướng dẫn của 4750 và 3176
            // Fix for CS0029: Ensure that nullable integers are handled correctly when converting to a non-nullable dictionary.
            errorlist = dmError130
                .Where(x => x.ErrorCode.HasValue) // Filter out entries where ErrorCode is null
                .ToDictionary(x => x.ErrorCode.GetValueOrDefault(), x => x.Message ?? string.Empty); // Use Value to access the non-nullable int

            if (File.MA_THUOC != curMATHUOC && File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
            {
                curMATHUOC = File.MA_THUOC;
                cungMATHUOCs = dmTHUOC.Where(t => t.MA_THUOC == curMATHUOC).ToList();
                cungHDSDs = hdsdTHUOC.Where(t => t.MA_THUOC == curMATHUOC).ToList();
            }
            else
            {
                //2952	3	XML2	LIEU_DUNG	[3176] Liều dùng Oxy không đúng hướng dẫn của 4750 và 3176
                if (File.MA_THUOC == "40.17")
                {
                    if (!(File.LIEU_DUNG ?? string.Empty).Contains("lít/phút") && !(File.LIEU_DUNG ?? string.Empty).Contains("l/p") && !(File.LIEU_DUNG ?? string.Empty).Contains("l/phút") &&
                        !lieuDung.Contains("lít/phút") && !lieuDung.Contains("l/p") && !lieuDung.Contains("l/phút"))
                    {
                        RaiseError(true,
                            2952,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2952, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê sử dụng Oxy, tuy nhiên liều dùng không có các đơn vị như lít / phút, l / p hoặc l / phút theo đúng Quyết định 3176, Quyết định 4750 Bộ Y Tế."
                            );
                    }
                }
            }

            //Linhist removed 100926 - chuyen sang ValidateXML23
            //3739	3	XML2	MA_THUOC	[3176] Kê thuốc trước khi có kết quả CLS

            //Linhist added 02/04/26
            //3972	2	XML2	MA_THUOC	[Hải Phòng] Thuốc điều trị tâm thần phải được kê bởi bác sĩ chuyên khoa tâm thần
            if (Uti.CodeContain(File.MA_THUOC ?? string.Empty, "40.944;40.949;40.953;40.955;40.956;40.966;40.968"))
            {
                RaiseError(dmBACSI.Where(x => (File.MA_BAC_SI ?? string.Empty).Contains(x.MA_CCHN ?? string.Empty) && (x.PHAMVI_CM ?? string.Empty).Split(';').Contains("106"))?.Any() != true, 3972);  
            }

            #region Kiểm tra các lỗi cơ bản
            //Linhist removed 100926 - chuyen sang ValidateXML23
            //3739	3	XML2	MA_THUOC	[3176] Kê thuốc trước khi có kết quả CLS
            //3625	3	XML2	MA_THUOC	[3176] Thực hiện y lệnh thuốc trước khi có kết quả CLS
            
            //2901	3	XML2	DANG_BAO_CHE	[3176] Thái Bình: Dạng bào chế không được để trống và lớn hơn 1024 ký tự
            if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
            {
                Field(x => x.DANG_BAO_CHE).IsEmpty().ThenError(2901).LengthOver(1024).ThenError(2901);
            }
            //2900	3	XML2	CACH_DUNG	[3176] Thái Bình: Cách dùng không được để trống và lớn hơn 1024 ký tự
            if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
            {
                Field(x => x.CACH_DUNG).IsEmpty().ThenError(2900).LengthOver(1024).ThenError(2900);
            }

            #region Linhist changed 20/03/26

            ////2504	3	XML2	NGAY_YL	[130] Ngày y lệnh ngoài thời gian làm việc hành chính
            ////2505    3   XML2 NGAY_TH_YL[130] Ngày thực hiện y lệnh ngoài thời gian làm việc hành chính

            if (Xml01.MA_LOAI_KCB == "01" && _officialTime != null && _officialTime.IsValid)
            {
                //2504    3   XML2 NGAY_YL[130] Ngày y lệnh ngoài thời gian làm việc hành chính
                //2505    3   XML2 NGAY_TH_YL[130] Ngày thực hiện y lệnh ngoài thời gian làm việc hành chính
                if (!_officialTime.InRange(File.NGAY_YL))
                {
                    RaiseError(true, 2504,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2504, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.TEN_THUOC}, thời gian y lệnh:{File.NGAY_YL} không nằm trong khoảng thời gian làm việc hành chính của viện:{_officialTime}. Không hợp lý về mặt logic thời gian"
                    );
                }

                if (!_officialTime.InRange(File.NGAY_TH_YL))
                {
                    RaiseError(true, 2505,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2505, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.TEN_THUOC}, thời gian TH Y lệnh:{File.NGAY_TH_YL} không nằm trong khoảng thời gian làm việc hành chính của viện:{_officialTime}. Không hợp lý về mặt logic thời gian"
                    );
                }

                if (!_officialTime.InRange(File.NGAY_YL))
                {
                    RaiseError(true, 2850,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2850, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.TEN_THUOC}, thời gian y lệnh:{File.NGAY_YL} không nằm trong khoảng thời gian làm việc hành chính của viện:{_officialTime}. Không hợp lý về mặt logic thời gian"
                    );
                }

                if (!_officialTime.InRange(File.NGAY_TH_YL))
                {
                    RaiseError(true, 2851,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2851, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.TEN_THUOC}, thời gian TH y lệnh:{File.NGAY_TH_YL} không nằm trong khoảng thời gian làm việc hành chính của viện:{_officialTime}. Không hợp lý về mặt logic thời gian"
                    );
                }
            }
            #endregion

            //201    0   XML2 MA_THUOC    Mã thuốc không tồn tại trong danh mục được Bảo hiểm phê duyệt
            if (cungMATHUOCs == null || !cungMATHUOCs.Any())
            {
                if (File.MA_NHOM == 4)
                {
                    RaiseError(true, 201,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(201, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thuốc không tìm thấy trong DM Thuốc, Kiểm tra Danh mục thuốc trên Cổng Bảo hiểm và Đồng bộ Danh mục"
                    );
                }
            }
            else
            {
                #region Kiểm tra xem có cần check các lỗi Hàm Lượng, Đường dùng hay không? Vì DM Bảo hiểm phê duyệt không có trường đó
                if (Field(x => x.MA_NHOM).HasValue(4))
                {
                    if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
                    {
                        var ischeckHamLuong = cungMATHUOCs.Where(t => TT_THAU_RT(t.TT_THAU ?? string.Empty, File.MA_CSKCB, t.LOAI_THAU ?? string.Empty) == File.TT_THAU).Select(x => x.HAM_LUONG).ToList();
                        var ischeckDuongDung = cungMATHUOCs.Where(t => TT_THAU_RT(t.TT_THAU ?? string.Empty, File.MA_CSKCB, t.LOAI_THAU ?? string.Empty) == File.TT_THAU).Select(x => x.MA_DUONG_DUNG).ToList();

                        //2362    0   XML2 HAM_LUONG[130] Hàm lượng trong danh mục được Bảo hiểm phê duyệt, ko có nội dung, sẽ bỏ qua kiểm tra
                        //2363    0   XML2 DUONG_DUNGg[130] Đường dùng trong danh mục được Bảo hiểm phê duyệt, ko có nội dung, sẽ bỏ qua kiểm tra

                        foreach (var t in ischeckDuongDung)
                        {
                            if ((t ?? string.Empty).Trim().Length > 0)
                            {
                                ichkDuongdung = true;
                            }
                        }
                        foreach (var t in ischeckHamLuong)
                        {
                            if ((t ?? string.Empty).Trim().Length > 0)
                            {
                                ichkHamLuong = true;
                            }
                        }
                        if (!ichkDuongdung)
                        {
                            RaiseError(true,
                                2363,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2363, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}|{File.DUONG_DUNG}, Đường Dùng không tìm thấy  trong DM Thuốc, Kiểm tra Danh mục thuốc trên Cổng Bảo hiểm và Đồng bộ Danh mục"
                                );
                        }
                        if (!ichkHamLuong)
                        {
                            RaiseError(true,
                                2362,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2362, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}|{File.HAM_LUONG}, Hàm lượng không tìm thấy  trong DM Thuốc, Kiểm tra Danh mục thuốc trên Cổng Bảo hiểm và Đồng bộ Danh mục"
                                );
                        }
                    }
                }
                #endregion

                //81	XML2	MA_LK	Mã liên kết không được để trống
                //82	XML2	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
                //***   XML2    MA_LK   Mã liên kết tại XML2 khác Mã liên kết tại XML1
                Field(x => x.MA_LK).IsEmpty().ThenError(81).LengthOver(100).ThenError(82);//.NotValue(Xml01.MA_LK).ThenError(99999);
                ////468	1	XML2	MA_BAC_SI	Chỉ bác sĩ mới được thực hiện Khám chữa bệnh và ra chỉ định
                if (File.MA_NHOM == 4)
                {
                    if (!string.IsNullOrEmpty(File.MA_BAC_SI))
                    {
                        if (!dmBACSI.Any(x => x.MA_CCHN == File.MA_BAC_SI && x.CHUC_DANH_NN == "1"))
                        {
                            RaiseError(true,
                                468,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(468, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Nhaan vieen {File.MA_BAC_SI}, không phải là bác sĩ, cần phải kiểm tra lại"
                                );
                        }
                    }
                }
                //83	XML2	STT	Số thứ tự không được để trống và phải có giá trị từ 1 đến 999999999
                Field(x => x.STT).IsEmpty().IsOver(999999999).ThenError(83);

                //86	XML2	MA_THUOC	Mã thuốc không được để trống
                //87	XML2	MA_THUOC	Mã thuốc không được lớn hơn 255 ký tự
                Field(x => x.MA_THUOC).IsEmpty().ThenError(86).LengthOver(255).ThenError(87);

                //2031   XML2    MA_CSKCB_THUOC Mã CSKCB thuốc không đúng định dạng 130
                //**** C.XXXXX, K.XXXXX, M.XXXXX, HC.XXXXX
                if (Field(x => x.MA_CSKCB_THUOC).NotEmpty())
                {
                    var thuoc = (File.MA_CSKCB_THUOC ?? string.Empty).ToString().Split(".");
                    if (thuoc.Length < 1)
                    {
                        RaiseError(true,
                            2301,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2031, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_CSKCB_THUOC}, Không theo đúng Quyết định 3176, Quyết định 4750 Bộ Y Tế.(Định dạng C.XXXXX, K.XXXXX, M.XXXXX, HC.XXXXX)"
                            );
                    }
                    else if (thuoc.Length > 1)
                    {
                        if (Uti.CodeContain("C;H;K;M;HC", thuoc[0]))
                        {
                            RaiseError(true,
                                2031,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2031, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_CSKCB_THUOC}, Không theo đúng Quyết định 3176, Quyết định 4750 Bộ Y Tế.(Định dạng: Không tồn tại các mã C,H,K,M,HC)"
                                );
                        }
                        if (thuoc[1].Length < 5)
                        {
                            RaiseError(true,
                                2031,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2031, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_CSKCB_THUOC}, Không theo đúng Quyết định 3176, Quyết định 4750 Bộ Y Tế.(Định dạng không đúng Mã CKSCB chuyển thuốc điều trị)"
                                );
                        }
                    }

                }

                //84	XML2	MA_NHOM	Mã nhóm không được để trống và phải là số nguyên có giá trị từ 4 đến 7
                Field(x => x.MA_NHOM).IsEmpty().NotValue(4, 7, 17).ThenError(84);

                //88	XML2	TEN_THUOC	Tên thuốc không được để trống
                //89	XML2	TEN_THUOC	Tên thuốc không được lớn hơn 1024 ký tự
                Field(x => x.TEN_THUOC).IsEmpty().ThenError(88).LengthOver(1024).ThenError(89);

                //90	XML2	DON_VI_TINH	Đơn vị tính không được để trống hoặc nhiều hơn 50 ký tự
                //141	XML2	DON_VI_TINH	Đơn vị tính không được lớn hơn 50 ký tự
                Field(x => x.DON_VI_TINH).IsEmpty().ThenError(90).LengthOver(50).ThenError(141);

                //91	XML2	HAM_LUONG	Hàm lượng không được để trống
                //168	XML2	HAM_LUONG	Hàm lượng không được lớn hơn 1024 ký tự
                if (Field(x => x.MA_NHOM).HasValue(4))
                {
                    if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573" && ichkHamLuong == true)
                    {
                        Field(x => x.HAM_LUONG).IsEmpty().ThenError(91).LengthOver(1024).ThenError(168);
                    }
                }

                //92	XML2	DUONG_DUNG	Mã đường dùng không được để trống
                //144	XML2	DUONG_DUNG	Mã đường dùng không được lớn hơn 4 ký tự
                if (Field(x => x.MA_NHOM).HasValue(4))
                {
                    if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
                    {
                        Field(x => x.DUONG_DUNG).IsEmpty().ThenError(92).LengthOver(4).ThenError(144);
                    }
                }

                //94	XML2	SO_DANG_KY	Số đăng ký không được lớn hơn 255 ký tự, không tồn tại khoảng trắng ở giữa
                StrField(x => x.SO_DANG_KY).HasSpaceAny().LengthOver(255).ThenError(94);

                //239	XML2	TT_THAU	Thông tin thầu để trống hoặc không được lớn hơn 50 ký tự
                //2178    4   XML2 TT_THAU[130] Thông tin thầu không dài hơn 50 ký tự
                if (Field(x => x.MA_NHOM).HasValue(4))
                {
                    if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
                    {
                        RaiseError(String.IsNullOrEmpty((File.TT_THAU ?? string.Empty).ToString()), 239);
                        RaiseError(string.IsNullOrEmpty(File.SO_DANG_KY), 94);
                        if ((File.TT_THAU ?? string.Empty).Length > 50)
                        {
                            RaiseError(true, 2178);
                        }
                    }
                }

                //*** Số lượng làm tròn đến 3 số sau số thập phân theo  130
                Field(x => x.SO_LUONG).IsEmpty().IsUnder(0).ThenError(96);

                //** Đơn giá làm tròn đến 3 số sau số thập phân theo 130
                Field(x => x.DON_GIA).IsEmpty().IsUnder(0).ThenError(97);

                Field(x => x.T_NGUONKHAC_NSNN).IsEmpty().IsUnder(0).ThenError(1808);

                Field(x => x.T_NGUONKHAC_VTNN).IsEmpty().IsUnder(0).ThenError(1809);

                Field(x => x.T_NGUONKHAC_VTTN).IsEmpty().IsUnder(0).ThenError(1810);

                Field(x => x.T_NGUONKHAC_CL).IsEmpty().IsUnder(0).ThenError(1811);

                Field(x => x.T_NGUONKHAC).IsEmpty().IsUnder(0).ThenError(1812);

                //102	XML2	MUC_HUONG	Mức hưởng không được để trống và phải có giá trị từ 0 đến 100
                Field(x => x.MUC_HUONG).IsEmpty().NotBetweenOrEqual(0, 100).ThenError(102);

                Field(x => x.T_BNTT).IsEmpty().IsUnder(0).ThenError(1813);

                Field(x => x.T_BNCCT).IsEmpty().IsUnder(0).ThenError(1814);

                Field(x => x.T_BHTT).IsEmpty().IsUnder(0).ThenError(1815);

                //103	XML2	MA_KHOA	Mã khoa không được để trống
                //104	XML2	MA_KHOA	Mã khoa không được lớn hơn 15 ký tự
                //Linhist changed 250925
                Field(x => x.MA_KHOA).IsEmpty().ThenError(103).LengthOver(50).ThenError(104);

                //4004    3   XML2 MA_KHOA[3176] Mã khoa điều trị tại XML2, phải được liệt kê đẩy đủ trong Mã khoa tại XML1
                
                    if (!Uti.CodeMatch(File.MA_KHOA ?? string.Empty, Xml01.MA_KHOA ?? string.Empty))
                    {
                            RaiseError(true, 4004,
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4004, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Mã khoa điều trị tại XML2:{File.MA_KHOA}, phải được liệt kê đẩy đủ trong Mã khoa tại XML1:{Xml01.MA_KHOA}"
                            );
                    }
                    
                //105	XML2	MA_BAC_SI	Mã bác sĩ không được để trống
                //106	XML2	MA_BAC_SI	Mã bác sĩ không được lớn hơn 255 ký tự
                Field(x => x.MA_BAC_SI).IsEmpty().ThenError(105).LengthOver(255).ThenError(106);

                //107	XML2	NGAY_YL	Ngày y lệnh không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
                Field(x => x.NGAY_YL).IsEmpty().ThenError(107);

                //108	XML2	MA_PTTT	Mã PTTT không được để trống và phải là số nguyên có giá trị từ 1 đến 3
                Field(x => x.MA_PTTT).IsEmpty().NotBetweenOrEqual(1, 3).ThenError(108);

                //95	XML2	NGUON_CTRA	Mã nguồn chi trả không được để trống và phải có giá trị từ 1 đến 4
                Field(x => x.NGUON_CTRA).IsEmpty().NotBetweenOrEqual(1, 4).ThenError(95);

                Field(x => x.THANH_TIEN_BH).IsEmpty().IsUnder(0).ThenError(100);
                #endregion

                #region Kiểm tra các lỗi có liên quan đến các trường khác
                //3464	3	XML2	MA_THUOC	[3176] Hải Phòng: Kê thuốc kháng sinh thiếu Phiếu yêu cầu sử dụng kháng sinh
                if (Uti.CodeContain("40.187;40.186;40.26;40.251;40.253;40.249;40.281;40.30.303;40.189;40.188;20.194;40.258;", File.MA_THUOC ?? string.Empty))
                {
                    if (!(Xml01.XML05s ?? Enumerable.Empty<XML05>()).Any(x => (x.HOI_CHAN ?? string.Empty).ToString().ToLower().Contains("kháng sinh") && x.THOI_DIEM_DBLS <= File.NGAY_YL))
                    {
                        RaiseError(true,
                            3464,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3464, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, XML5 không tồn tại nội dung HỘI CHẨN nào cho phép dùng thuốc kháng sinh và thời gian hội chẩn trước thời điểm y lệnh thuốc:{File.NGAY_YL}"
                            );
                    }
                }
                //3346	4	XML2	NGAY_TH_YL	[3176] Bắt buộc phải nhập ngày Thực hiện Y lệnh khi thuốc được sử dụng tại Cơ sở KCB
                if (Uti.CodeContain(Xml01.MA_LOAI_KCB ?? string.Empty, "03;04;09"))
                {
                    if (File.NGAY_TH_YL is null) RaiseError(true, 3346);

                }
                //2912	3	XML2	MA_DICH_VU	Thái Bình: Các vị thuốc này bắt buộc phải nhập mã dịch vụ đi kèm
                if ("05V.334;05V.333;05V.316;05V.258;05V.226;05V.296;05V.217;05V.97;05V.330;05V.328;05V.290;05V.236;05V.185;05V.331;05V.137;05V.91;05V.179;05V.301;05V.329;05V.53;05V.211;05V.298;05V.319;05V.221;05V.305;05V.17;05V.309;05V.212;05V.205;05V.1;05V.189;05V.311;05V.135;05V.213".Contains(File.MA_THUOC ?? string.Empty))
                {
                    if (string.IsNullOrEmpty((File.MA_DICH_VU ?? string.Empty).ToString()))
                    {
                        RaiseError(true,
                            3822,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3822, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Những vị thuốc YHCT bắt buộc phải đính kèm dịch vụ(Phải có mã Dịch vụ tại XML2)"
                            );
                    }
                }
                if ((File.MA_THUOC ?? string.Empty).Contains("C") || (File.MA_THUOC ?? string.Empty).Contains("V"))
                {
                    if ((File.DON_VI_TINH ?? string.Empty).ToLower().Contains("gam"))
                    {
                        Field(x => x.MA_DICH_VU).IsEmpty().ThenError(2912);
                    }
                }
                if (File.MA_THUOC == "40.1049" || (File.TEN_THUOC ?? string.Empty).Trim() == "Vinrovit 5000" || File.MA_THUOC == "40.716" || File.MA_THUOC == "40.980" || File.MA_THUOC == "40.642" || File.MA_THUOC == "40.973" || File.MA_THUOC == "40.981")
                {
                    Field(x => x.MA_DICH_VU).IsEmpty().ThenError(2912);
                }

                //99	XML2	TYLE_TT_BH	Tỷ lệ thanh toán không được để trống và phải là số nguyên có giá trị từ 0 đến 100
                Field(x => x.TYLE_TT_BH).IsEmpty().NotBetweenOrEqual(0, 100).ThenError(99);

                //85	XML2	PHAM_VI	Phạm vi không được để trống và phải là số nguyên có giá trị từ 1 đến 3
                if (!Field(x => x.PHAM_VI).IsEmpty().NotBetweenOrEqual(1, 3).ThenError(85))
                {
                    //*** với mã bằng 2 thì tỷ lệ này phải = 0
                    //2076    4   XML2 THANH_TIEN_BH[130] Phạm vi là 2, thuốc nằm ngoài phạm vi hưởng BHYT
                    //2077    4   XML2 TYLE_TT_BH[130] Phạm vi là 2, thuốc nằm ngoài phạm vi hưởng BHYT
                    if (Field(x => x.PHAM_VI).HasValue(2))
                    {
                        Field(x => x.THANH_TIEN_BH).IsOver(0).ThenError(2076);
                        Field(x => x.TYLE_TT_BH).IsOver(0).ThenError(2077);
                    }

                    //**** Với mã bằng 3 phải kiểm tra xem có phải thẻ có phải là thẻ Công an, Quân đội hay không (2 ký tự đầu thẻ là QN, HC, LS, XK, CY, CA)
                    if (Field(x => x.PHAM_VI).HasValue(3))
                    {
                        //2078	4	XML2	PHAM_VI	[130] Đối tượng không phải CA, QĐ, Cơ yếu theo NĐ 175/2015/NĐ-CP, Phạm vi không được để là 3
                        if (!Uti.HCodeContain((Xml01.MA_THE_BHYT ?? string.Empty).Substring(0, 2).ToString(), "QN;HC;LS;XK;CY;CA"))
                        {
                            RaiseError(true, 2078);
                        }
                    }
                }

                //2388    3   XML2 NGAY_YL[130] Kê thuốc trùng thời điểm ra viện
                //2389    3   XML2 NGAY_TH_YL[130] Kê thuốc trùng thời điểm ra viện

                Field(x => x.THANH_TIEN_BV).IsEmpty().IsUnder(0).ThenError(101);
                if (Field(x => x.SO_LUONG).NotEmpty().Field(x => x.DON_GIA).NotEmpty())
                {
                    Field(x => x.THANH_TIEN_BV).IsOver(File.SO_LUONG * File.DON_GIA + 1).ThenError(2023);
                }
                #endregion

                #region Kiểm tra các lỗi có liên quan đến các Danh mục
                //3256	3	XML2	NGAY_YL	3176: Ngày y lệnh không nằm trong khoảng thời gian đăng ký làm việc Tuần của nhân viên
                //3269	3	XML2	NGAY_YL	[3176] Ngày y lệnh không nằm trong khoảng thời gian khai báo ngày làm việc của Nhân viên

                if (Field(x => x.NGAY_YL).NotEmpty())
                {
                    var tuanlamviec = ParseWeekdays(dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.MA_BAC_SI)?.TG_TUAN);
                    if (tuanlamviec != null)
                    {
                        if (!tuanlamviec.Contains(File.NGAY_YL.GetValueOrDefault().DayOfWeek))
                        {
                            RaiseError(true,
                                3256,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3256, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Bác sĩ:{File.MA_BAC_SI} ra y lệnh:{File.NGAY_YL.GetValueOrDefault().DayOfWeek}, BS chỉ đăng ký làm việc các ngày:" +
                                $"{string.Join(", ", tuanlamviec.Select(d => d.ToString()))}."
                                );
                        }
                    }
                    var ngaylamviec = dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.MA_BAC_SI)?.TG_NGAY;
                    if (ngaylamviec != null)
                    {
                        if (!IsInTimeRanges(ngaylamviec, File.NGAY_YL.GetValueOrDefault()))
                        {
                            RaiseError(true,
                                3269,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3269, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Bác sĩ:{File.MA_BAC_SI} ra y lệnh:{File.NGAY_YL.GetValueOrDefault()}, BS chỉ đăng ký làm việc các khung giờ:" +
                                $"{ngaylamviec}."
                                );
                        }
                    }
                }

                //200 Mã thuốc ko tìm thấy trong DM được phê duyệt

                if (Field(x => x.MA_NHOM).HasValue(4))
                {
                    if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
                    {
                        //2574	3	XML2	LIEU_DUNG	[Hải Phòng]: 130 - Kê thuốc quá 30 ngày
                        //Linhist changed 130826
                            if ((File.LIEU_DUNG ?? string.Empty).ToString().Length >= 7)
                            {
                                string lieudungs = (File.LIEU_DUNG ?? string.Empty).ToString().Substring((File.LIEU_DUNG ?? string.Empty).Length - 7, 7);
                                if (lieudungs.ToLower().Contains("ngày"))
                                {
                                    string first2Chars = lieudungs.Substring(0, 2);
                                    bool areDigits = int.TryParse(first2Chars, out _);
                                    if (areDigits)
                                    {
                                        int songaydungthuoc = int.Parse(first2Chars);
                                        if (songaydungthuoc > 30)
                                        {
                                            RaiseError(true,
                                                2574,
                                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2574, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Số lượng thuốc kê trong đơn:{songaydungthuoc}>30 ngày, Kiểm tra lại"
                                                );
                                        }
                                    }
                                }
                            }

                        //201	201	4	XML2	MA_THUOC	Mã thuốc không tồn tại trong DM được phê duyệt

                        RaiseError(!cungMATHUOCs.Any(), 201);

                        if (cungMATHUOCs == null || !cungMATHUOCs.Any())
                        {
                            RaiseError(dmTHUOC.Any(x => x.MA_THUOC == (File.MA_THUOC ?? string.Empty).Trim()), 2318);
                        }

                        if (cungMATHUOCs != null && cungMATHUOCs.Any())
                        {

                            if (Field(x => x.TT_THAU).NotEmpty())
                            {
                                //3726	3	XML2	CACH_DUNG	[3176] Cách dùng của thuốc không đúng với Đường dùng của thuốc, đề nghị xem lại
                                if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
                                {
                                    if (!string.IsNullOrEmpty(File.CACH_DUNG))
                                    {
                                        string checkStr = duongDungDict.FirstOrDefault(x => x.Key == (File.DUONG_DUNG ?? string.Empty).Trim()).Value;
                                        if (!string.IsNullOrEmpty(checkStr))
                                        {
                                            string Str = checkStr.Contains(" ") ? checkStr.Substring(0, checkStr.IndexOf(" ")) : checkStr;
                                            if (!(File.CACH_DUNG ?? string.Empty).ToLower().Contains(Str.ToLower()) && !(File.CACH_DUNG ?? string.Empty).ToLower().Contains(checkStr.ToLower()))
                                            {
                                                RaiseError(true,
                                                    3726,
                                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3726, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}|{File.TT_THAU}, có Mã Đường dùng:{File.DUONG_DUNG}-{checkStr}" +
                                                    $"\nCách dùng thuôc là {File.CACH_DUNG} không hợp lý."
                                                    );
                                            }
                                        }
                                    }
                                }
                                var cungTTTHAU3618s = cungMATHUOCs.Where(t => !string.IsNullOrEmpty(t.TT_THAU) && t.TT_THAU == File.TT_THAU);
                                if (!cungTTTHAU3618s.Any())
                                {
                                    RaiseError(true,
                                        2552,
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2552, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}|{File.TT_THAU}, Không tim thấy TT_THAU:{File.TT_THAU} trong Danh mục, Kiểm tra Danh mục Thuốc tại cổng BHXH và Đồng bộ danh mục" +
                                        $"\n Đây là mã lỗi làm riêng cho các Cơ sở được Bảo hiểm địa phương yêu cầu TT_THAU phải nhập theo QĐ 3618 của Bảo hiểm Xã hội."
                                        );

                                }
                                //2317    2317    0   XML2 TT_THAU[130] Thông tin thầu không đúng định dạng 4750
                                RaiseError(!Check_TT_THAU(File.TT_THAU ?? string.Empty), 2317);
                                if (Check_TT_THAU(File.TT_THAU ?? string.Empty))
                                {
                                    //200  0   XML2 TT_THAU[130]Thông tin thầu không được tìm thấy trong danh mục phê duyệt, hệ thống sẽ không kiểm tra các thông tin Thuốc khác
                                    var cungTTTHAUs = cungMATHUOCs.Where(t => !string.IsNullOrEmpty((t.TT_THAU ?? string.Empty).Trim()) && TT_THAU_RT(t.TT_THAU ?? string.Empty, File.MA_CSKCB, t.LOAI_THAU ?? string.Empty) == File.TT_THAU);
                                    if (!cungTTTHAUs.Any())
                                    {
                                        RaiseError(true,
                                            200,
                                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(200, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}|{File.TT_THAU}, Không tim thấy TT_THAU:{File.TT_THAU} trong Danh mục, Kiểm tra Danh mục Thuốc tại cổng BHXH và Đồng bộ danh mục"
                                            );
                                    }
                                    if (dmErrorMHTC.Any(x => x.EMErrorCode == 2552))
                                    {
                                        cungTTTHAUs = cungTTTHAU3618s;
                                    }

                                    if (cungTTTHAUs.Any())
                                    {

                                        //327 Thông tin thầu không khớp với danh mục được phê duyệt bỏ lỗi

                                        //Nếu cần kiểm tra số đăng ký trước, nếu sai thì ko cần kiểm tra các thông tin còn lại của Thuốc, nếu đúng mới kiểm tra các thông tin còn lại
                                        if (Field(x => x.SO_DANG_KY).NotEmpty())
                                        {
                                            //241 Số đăng ký không đúng với danh mục được phê duyệt
                                            RaiseError(!cungTTTHAUs.Any(t => t.SO_DANG_KY == File.SO_DANG_KY), 241);
                                            RaiseError(!cungTTTHAUs.Any(t => Uti.IsEqual((t.SO_DANG_KY ?? string.Empty).Replace(" ", ""), (File.SO_DANG_KY ?? string.Empty).Replace(" ", ""), ignoreCase: true)), 173);
                                            RaiseError(!cungTTTHAUs.Any(t => Uti.IsEqual(t.SO_DANG_KY ?? string.Empty, File.SO_DANG_KY ?? string.Empty, ignoreCase: true)), 172);
                                        }

                                        //2938	3	XML2	MA_PP_CHEBIEN	[3176] Mã phương pháp chế biến không đúng trong Danh mục được Bảo hiểm phê duyệt
                                        if (cungTTTHAUs.Any(x => !string.IsNullOrEmpty(x.PP_CHEBIEN) && x.MA_THUOC == File.MA_THUOC))
                                        {
                                            RaiseError(!cungTTTHAUs.Any(t => t.PP_CHEBIEN == File.MA_PP_CHEBIEN), 2938);
                                        }

                                        //160 Mã nhóm không khớp với mã nhóm trong danh mục phê duyệt *** mã nhóm kiểm tra trong DM như 4210 DM không còn mã nhóm, bỏ lỗi
                                        //161 Tên thuốc không khớp với tên thuốc trong danh mục phê duyệt *** check trong DM nhu 4210
                                        //2175	4	XML2	TEN_THUOC	[130] Tên thuốc thừa khoảng trắng (Dấu cách), có độ dài không đúng độ dài trong danh mục phê duyệt
                                        //Linhist changed 06/03/26 - thêm kiểm tra cả số đăng ký
                                        //Linhist changed 31/03/26 - kiểm tra thêm tên thuốc đã bỏ phần trong ngoặc vuông []

                                        var tenTH = Uti.ReplaceQuote(File.TEN_THUOC ?? string.Empty, true, false);
                                        RaiseError(!cungTTTHAUs.Any(t => t.TEN_THUOC == tenTH && t.SO_DANG_KY == File.SO_DANG_KY), 161);
                                        RaiseError(!cungTTTHAUs.Any(t => Uti.IsEqual((t.TEN_THUOC ?? string.Empty).Replace(" ", ""), tenTH.Replace(" ", ""), ignoreCase: true) && t.SO_DANG_KY == File.SO_DANG_KY), 2051);
                                        RaiseError(!cungTTTHAUs.Any(t => (t.TEN_THUOC ?? string.Empty).ToLower() == tenTH.ToLower() && t.SO_DANG_KY == File.SO_DANG_KY), 2052);
                                        RaiseError(!cungTTTHAUs.Any(t => (t.TEN_THUOC ?? string.Empty).Length == tenTH.Length && t.SO_DANG_KY == File.SO_DANG_KY), 2175);

                                        if (!cungTTTHAUs.Any(x => x.DON_VI_TINH == File.DON_VI_TINH))
                                        {
                                            RaiseError(true,
                                                164,
                                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(164, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}|{File.TT_THAU}\n" +
                                                $"Đơn vị Tính:{File.DON_VI_TINH}|{(File.DON_VI_TINH ?? string.Empty).Length}\n" +
                                                $"Đơn vị Tính gần đúng: {string.Join(", ", cungTTTHAUs.Select(x => x.DON_VI_TINH + "|" + (x.DON_VI_TINH ?? string.Empty).Length).ToList())}"
                                                );
                                        }
                                        RaiseError(!cungTTTHAUs.Any(f => (f.DON_VI_TINH ?? string.Empty).ToLower() == (File.DON_VI_TINH ?? string.Empty).ToLower()), 2351);

                                        RaiseError(!cungTTTHAUs.Any(f => f.HAM_LUONG == File.HAM_LUONG), 166);

                                        //170 Mã đường dùng không khớp với danh mục phê duyệt

                                        if (ichkDuongdung)
                                        {
                                            RaiseError(!cungTTTHAUs.Any(f => f.MA_DUONG_DUNG == File.DUONG_DUNG), 170);
                                        }

                                        //2053    4   XML2 DANG_BAO_CHE[130] Dạng bào chế không đúng nội dung trong Danh mục phê duyệt
                                        if (!String.IsNullOrEmpty(File.DANG_BAO_CHE) && (File.MA_THUOC != "40.17" || File.MA_THUOC != "40.573"))
                                        {
                                            RaiseError(!cungTTTHAUs.Any(f => f.DANG_BAO_CHE == File.DANG_BAO_CHE), 2053);
                                        }

                                        //174 1   XML2 DON_GIA Đơn giá không khớp với đơn giá trong danh mục phê duyệt
                                        //175 1   XML2 DON_GIA Đơn giá nhỏ hơn đơn giá trong danh mục phê duyệt
                                        //202 1   XML2 DON_GIA Đơn giá lớn hơn đơn giá trong danh mục phê duyệt
                                        if (!cungTTTHAUs.Any(f => f.DON_GIA_BH == File.DON_GIA))
                                        {
                                            double? maxGia = cungTTTHAUs.Where(x => x.DON_GIA_BH != null && Uti.IsEqual(x.TEN_THUOC ?? string.Empty, File.TEN_THUOC ?? string.Empty, ignoreCase: true) &&
                                                                                    Uti.IsEqual(x.MA_DUONG_DUNG ?? string.Empty, File.DUONG_DUNG ?? string.Empty, ignoreCase: true) &&
                                                                                    Uti.IsEqual(x.DON_VI_TINH ?? string.Empty, File.DON_VI_TINH ?? string.Empty, ignoreCase: true)).Max(x => x.DON_GIA_BH);
                                            double? minGia = cungTTTHAUs.Where(x => x.DON_GIA_BH != null && Uti.IsEqual(x.TEN_THUOC ?? string.Empty, File.TEN_THUOC ?? string.Empty, ignoreCase: true) &&
                                                                                    Uti.IsEqual(x.MA_DUONG_DUNG ?? string.Empty, File.DUONG_DUNG ?? string.Empty, ignoreCase: true) &&
                                                                                    Uti.IsEqual(x.DON_VI_TINH ?? string.Empty, File.DON_VI_TINH ?? string.Empty, ignoreCase: true)).Min(x => x.DON_GIA_BH);

                                            if (maxGia != null && Uti.IsOverUp(File.DON_GIA, maxGia))
                                            {
                                                RaiseError(true,
                                                    202,
                                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(202, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Giá:{File.DON_GIA}, Giá trong Danh mục:{maxGia}, Kiểm tra Danh mục Thuốc tại cổng BHXH và Đồng bộ danh mục"
                                                    );
                                            }
                                            //Linhist added 280826
                                            else if (minGia != null && Uti.IsUnderUp(File.DON_GIA, minGia))
                                            {
                                                RaiseError(true,
                                                    175,
                                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(175, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Giá:{File.DON_GIA}, Giá trong Danh mục:{maxGia}, Kiểm tra Danh mục Thuốc tại cổng BHXH và Đồng bộ danh mục"
                                                    );
                                            }
                                            else
                                            {
                                                RaiseError(true,
                                                    174,
                                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(174, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Giá:{File.DON_GIA}, Giá trong Danh mục:{maxGia}, Kiểm tra Danh mục Thuốc tại cổng BHXH và Đồng bộ danh mục"
                                                    );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                //2054    4   XML2 THANH_TIEN_BV[130] Thành tiền Bệnh viện khác Số lượng* Đơn giá
                if (Field(x => x.THANH_TIEN_BV).NotEmpty().Field(x => x.SO_LUONG).NotEmpty().Field(x => x.DON_GIA).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    decimal thanhtienBV = Uti.RoundUp((decimal)File.SO_LUONG.GetValueOrDefault() * (decimal)File.DON_GIA.GetValueOrDefault());
                    if (!Uti.IsEqualUp((decimal?)File.THANH_TIEN_BV, thanhtienBV))
                    {
                        RaiseError(true,
                            2054,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2054, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thành tiền BV:{File.THANH_TIEN_BV}, Giá theo công thức:{thanhtienBV}, Kiểm tra lại công thức tính toán"
                            );
                    }
                }
                //2055	4	XML2	THANH_TIEN_BH	[130] Thành tiền Bảo hiểm khác Số lượng * Đơn giá * Tỷ lệ Bảo hiểm /100
                if (Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.SO_LUONG).NotEmpty().Field(x => x.DON_GIA).NotEmpty().Field(x => x.TYLE_TT_BH).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    decimal thanhtienBH = Uti.RoundUp((decimal)File.SO_LUONG.GetValueOrDefault() * (decimal)File.DON_GIA.GetValueOrDefault() * (decimal)File.TYLE_TT_BH.GetValueOrDefault() / 100);
                    if (!Uti.IsEqualUp((decimal?)File.THANH_TIEN_BH, thanhtienBH))
                    {
                        RaiseError(true,
                            2055,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2055, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thành tiền Bảo hiểm:{File.THANH_TIEN_BH}, Giá theo công thức:{thanhtienBH}, Kiểm tra lại công thức tính toán"
                            );
                    }

                }

                //2056	4	XML2	T_NGUONKHAC	[130] Tiền nguồn khác sai công thức tính trong 4750
                if (Field(x => x.T_NGUONKHAC).NotEmpty().Field(x => x.T_NGUONKHAC_NSNN).NotEmpty().Field(x => x.T_NGUONKHAC_VTNN).NotEmpty().Field(x => x.T_NGUONKHAC_VTTN).NotEmpty().Field(x => x.T_NGUONKHAC_CL).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    decimal tnguonkhac = Uti.RoundUp((decimal)File.T_NGUONKHAC_NSNN.GetValueOrDefault() + (decimal)File.T_NGUONKHAC_VTTN.GetValueOrDefault() + (decimal)File.T_NGUONKHAC_VTNN.GetValueOrDefault() + (decimal)File.T_NGUONKHAC_CL.GetValueOrDefault());
                    if (!Uti.IsEqualUp((decimal?)File.T_NGUONKHAC, tnguonkhac))
                    {
                        RaiseError(true,
                            2056,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2056, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền nguồn khác:{File.T_NGUONKHAC}, Giá theo công thức:{tnguonkhac}, Kiểm tra lại công thức tính toán"
                            );
                    }

                }

                //2057	4	XML2	T_BNTT	[130] Tiền Bệnh nhân thanh toán bị sai, xem lại công thức
                //2677	3	XML1	T_BNTT	[3176] T_BNTT sai so với công thức của 3176
                if (Field(x => x.THANH_TIEN_BV).NotEmpty().Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.T_NGUONKHAC).NotEmpty().Field(x => x.TYLE_TT_BH).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    decimal t_bntt = Uti.RoundUp((decimal)File.THANH_TIEN_BV.GetValueOrDefault() * (1 - (decimal)File.TYLE_TT_BH.GetValueOrDefault() / 100));
                    if (File.T_NGUONKHAC.GetValueOrDefault() > 0)
                    {
                        t_bntt = t_bntt - (decimal)File.T_NGUONKHAC.GetValueOrDefault();
                        if (t_bntt < 0)
                        {
                            t_bntt = 0;
                        }
                    }

                    //Linhist changed 26/06/26
                    if (!Uti.IsEqualUp((decimal?)File.T_BNTT, t_bntt))
                    {
                        RaiseError(true,
                            2677,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2677, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền Bệnh nhân thanh toán:{File.T_BNTT ?? 0f}, Giá theo công thức:{t_bntt}, Kiểm tra lại công thức tính toán"
                            );
                    }

                }
                if (Field(x => x.THANH_TIEN_BV).NotEmpty().Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.T_NGUONKHAC).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    decimal t_bntt = Uti.RoundUp((decimal)File.THANH_TIEN_BV.GetValueOrDefault() - (decimal)File.THANH_TIEN_BH.GetValueOrDefault());
                    if (File.T_NGUONKHAC.GetValueOrDefault() != 0)
                    {
                        //Linhist changed 26/06/26
                        if (Uti.IsUnderUp((decimal)File.T_NGUONKHAC.GetValueOrDefault(), t_bntt))
                        {
                            t_bntt = t_bntt - (decimal)File.T_NGUONKHAC.GetValueOrDefault();
                        }
                        else if ((decimal)File.T_NGUONKHAC.GetValueOrDefault() > t_bntt)
                        {
                            t_bntt = 0;
                        }
                    }

                    //Linhist changed 26/06/26
                    if (!Uti.IsEqualUp((decimal?)File.T_BNTT, t_bntt))
                    {
                        RaiseError(true,
                            2057,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2057, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền Bệnh nhân thanh toán:{File.T_BNTT ?? 0d}, Giá theo công thức:{t_bntt}, Kiểm tra lại công thức tính toán"
                            );
                    }
                }

                //2679	3	XML2	T_BHTT	[3176] T_BHTT không đúng so với công thức 3176
                //2058	4	XML2	T_BHTT	[130] Tiền Bảo hiểm thanh toán bị sai so với công thức hướng dẫn 4750
                if (File.THANH_TIEN_BH != null && File.MUC_HUONG != null && File.T_BHTT != null && File.T_NGUONKHAC != null && File.TYLE_TT_BH != null)
                {
                    //Linhist changed 26/06/26
                    decimal t_bhtt = Uti.RoundUp((decimal)File.THANH_TIEN_BV.GetValueOrDefault() * (decimal)File.TYLE_TT_BH.Value / 100 * (decimal)File.MUC_HUONG.Value / 100);

                    if (File.T_NGUONKHAC.Value > 0)
                    {
                        decimal tien_thua = (decimal)File.T_NGUONKHAC.GetValueOrDefault() - (decimal)File.T_BNTT.GetValueOrDefault() - (decimal)File.T_BNCCT.GetValueOrDefault();
                        if (tien_thua > 0)
                        {
                            t_bhtt = t_bhtt - tien_thua;
                            if (t_bhtt < 0)
                            {
                                t_bhtt = 0;
                            }
                        }
                    }

                    //Linhist changed 26/06/26
                    if (!Uti.IsEqualUp((decimal?)File.T_BHTT, t_bhtt))
                    {
                        RaiseError(true,
                            2058,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2058, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền Bảo hiểm thanh toán:{File.T_BHTT}, Giá theo công thức:{t_bhtt}, Kiểm tra lại công thức tính toán"
                            );
                    }

                }
                if (File.THANH_TIEN_BH != null && File.MUC_HUONG != null && File.T_BHTT != null && File.T_NGUONKHAC != null)
                {
                    //Linhist changed 26/06/26
                    decimal t_bhtt = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value * (decimal)File.MUC_HUONG.Value / 100);
                    decimal t_bnctttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value - (decimal)t_bhtt);
                    decimal t_bntttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BV.GetValueOrDefault() - (decimal)File.THANH_TIEN_BH.Value);

                    if (File.T_NGUONKHAC.Value != 0)
                    {
                        if ((decimal)File.T_NGUONKHAC.GetValueOrDefault() > t_bnctttamtinh + t_bntttamtinh)
                        {
                            //Linhist changed 26/06/26
                            t_bhtt = Uti.RoundUp(t_bhtt - ((decimal)File.T_NGUONKHAC.GetValueOrDefault() - t_bnctttamtinh) - t_bntttamtinh);
                        }
                        else if ((decimal)File.T_NGUONKHAC < t_bnctttamtinh + t_bntttamtinh)
                        {

                        }
                    }

                    //Linhist changed 26/06/26
                    if (!Uti.IsEqualUp((decimal?)File.T_BHTT, t_bhtt))
                    {
                        RaiseError(true,
                            2679,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2679, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền Bảo hiểm thanh toán:{File.T_BHTT ?? 0f}, Giá theo công thức:{t_bhtt}, Kiểm tra lại công thức tính toán"
                            );
                    }

                }

                //2678	3	XML2	T_BNCTT	[3176] T_BNCTT không đúng hướng dẫn của 3176
                //2059    4   XML2 T_BNCTT[130] Tiền Bệnh nhân cùng chi trả bị sai so với công thức hướng dẫn 4750
                if (File.THANH_TIEN_BV != null && File.MUC_HUONG != null && File.T_BNCCT != null && File.T_NGUONKHAC != null && File.T_BNTT != null)
                {
                    //Linhist changed 26/06/26

                    //Linhist changed 190826 - sửa lại THANH_TIEN_BV
                    decimal t_bnctt = Uti.RoundUp((decimal)File.THANH_TIEN_BV.Value - (decimal)File.T_BHTT.GetValueOrDefault() - (decimal)File.T_BNTT.Value);

                    if (File.T_NGUONKHAC > 0)
                    {
                        decimal t_thua = (decimal)File.T_BNTT.Value - (decimal)File.T_NGUONKHAC.GetValueOrDefault();
                        if (t_thua > 0)
                        {
                            t_bnctt -= t_thua;
                            if (t_bnctt < 0)
                            {
                                t_bnctt = 0;
                            }
                        }
                    }

                    //Linhist changed 26/06/26
                    if (!Uti.IsEqualUp((decimal)File.T_BNCCT, t_bnctt))
                    {
                        RaiseError(true,
                            2678,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2678, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền bệnh nhân cùng chi trả:{File.T_BNCCT ?? 0f}, Giá theo công thức:{t_bnctt}, Kiểm tra lại công thức tính toán"
                            );

                    }
                }
                if (File.THANH_TIEN_BH != null && File.THANH_TIEN_BV != null && File.MUC_HUONG != null && File.T_BNCCT != null && File.T_NGUONKHAC != null)
                {
                    //Linhist changed 26/06/26
                    decimal t_bntttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BV.Value - (decimal)File.THANH_TIEN_BH.Value);
                    decimal t_bhtttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value * (decimal)File.MUC_HUONG.Value / 100);
                    decimal t_bnctt = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value - t_bhtttamtinh);

                    if ((decimal)File.T_NGUONKHAC.GetValueOrDefault() > t_bntttamtinh)
                    {
                        if (t_bnctt > (decimal)File.T_NGUONKHAC.GetValueOrDefault() - t_bntttamtinh)
                        {
                            //Linhist changed 26/06/26
                            t_bnctt = Uti.RoundUp(t_bnctt - ((decimal)File.T_NGUONKHAC.GetValueOrDefault() - t_bntttamtinh));
                        }
                        else
                        {
                            t_bnctt = 0;
                        }
                    }

                    //Linhist changed 26/06/26
                    if (!Uti.IsEqualUp((decimal?)File.T_BNCCT, t_bnctt))
                    {
                        RaiseError(true,
                            2059,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2059, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tiền bệnh nhân cùng chi trả:{File.T_BNCCT}, Giá theo công thức:{t_bnctt}, Kiểm tra lại công thức tính toán"
                            );

                    }

                }

                //2328    2328    0   XML2 MA_KHOA[130] Mã khoa không tồn tại trong Danh mục phê duyệt
                RaiseError(!dmKhoaBV.Any(x => x.MA_KHOA == File.MA_KHOA), 2328);

                //1005	1	XML2	MA_THUOC	Phát sinh chi phí trùng thời điểm vào Viện
                if (Xml01.MA_LOAI_KCB != "02")
                {
                    if (File.NGAY_YL != null)
                    {
                        if (File.NGAY_YL == Xml01.NGAY_VAO)
                        {
                            RaiseError(true,
                                1005,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(1005, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} trùng với thời điểm Bệnh nhân vào viện:{Xml01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                                );
                        }
                    }
                }

                //2680	3	XML2	NGAY_TH_YL	[3176] Ngày Thực hiện Y Lệnh thuốc Bắt buộc phải nhập theo đúng HD 3176
                //2681	3	XML2	NGAY_TH_YL	[3176] Ngày thực hiện Y Lệnh thuốc nhỏ hơn thời gian Y Lệnh thuốc
                //2682    3   XML2 NGAY_TH_YL[3176] Ngày thực hiện Y Lệnh thuốc nhỏ hơn thời gian Vào viện
                //2683    3   XML2 NGAY_TH_YL[3176] Ngày thực hiện Y Lệnh thuốc lớn hơn thời gian ra Viện
                //2684    3   XML2 NGAY_TH_YL[3176] Ngày thực hiện Y lệnh thuốc trùng thời điểm vào viện
                if (File.MA_CSKCB_THUOC != null)
                {
                    Field(x => x.NGAY_TH_YL).IsEmpty().ThenError(2680);
                    if (File.NGAY_TH_YL != null)
                    {
                        if (File.NGAY_TH_YL < File.NGAY_YL)
                        {
                            RaiseError(true,
                                2681,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2681, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                                );
                        }
                        if (File.NGAY_TH_YL < Xml01.NGAY_VAO)
                        {
                            RaiseError(true,
                                2682,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2682, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian vào viện:{Xml01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                                );
                        }
                        if (File.NGAY_TH_YL > Xml01.NGAY_RA)
                        {
                            RaiseError(true,
                                2683,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2683, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian ra viện:{Xml01.NGAY_RA:dd/MM/yyyy HH:mm:ss}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                                );
                        }
                        if (File.NGAY_TH_YL == Xml01.NGAY_VAO)
                        {
                            RaiseError(true,
                                2684,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2684, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian vào viện:{Xml01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                                );
                        }

                    }
                }

                //179 1   XML2 NGAY_YL Ngày y lệnh thuốc không được lớn hơn ngày ra
                //2400	3	XML2	NGAY_YL	[130] Ngày Y lệnh thuốc không được bằng ngày ra viện
                if (File.NGAY_YL != null)
                {
                    if (File.NGAY_YL > Xml01.NGAY_RA)
                    {
                        RaiseError(true,
                            179,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(179, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian ra viện:{Xml01.NGAY_RA:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }
                    if (File.NGAY_YL == Xml01.NGAY_RA)
                    {
                        RaiseError(true,
                            2400,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2400, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian ra viện:{Xml01.NGAY_RA:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }

                    //178 1   XML2 NGAY_YL Ngày y lệnh thuốc không được nhỏ hơn ngày vào
                    if (File.NGAY_YL < Xml01.NGAY_VAO)
                    {
                        RaiseError(true,
                            178,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(178, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian vào viện:{Xml01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }

                    //2388    3   XML2 NGAY_YL[130] Kê thuốc trùng thời điểm ra viện
                    if (File.NGAY_YL == Xml01.NGAY_RA)
                    {
                        RaiseError(true,
                            2388,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2388, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian ra viện:{Xml01.NGAY_RA:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }
                }

                //328	1	XML2	NGAY_YL	Ngày y lệnh không được phép nhỏ hơn ngày công bố

                //2061	4	XML2	NGAY_TH_YL	[130] Ngày thực hiện y lệnh thuốc không được lớn hơn thời điểm công bố
                //358 1   XML2 MA_BAC_SI   Sai thông tin mã bác sĩ, không nằm trong mã CCHN được cấp
                //2354	0	XML2	MA_BAC_SI	[130] Mã bác sĩ (Mã BHXH) không khớp thông tin danh mục phê duyệt
                if (!string.IsNullOrEmpty(File.MA_BAC_SI))
                {
                    if (!dmBACSI.Any(x => Equals(x.MA_CCHN, File.MA_BAC_SI)))
                    {
                        RaiseError(true,
                            358,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(358, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bác sĩ y lệnh thuốc:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ"
                            );
                    }
                    if (!dmBACSI.Any(x => Equals(x.MA_CCHN, File.MA_BAC_SI)))
                    {
                        RaiseError(true,
                            2354,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2354, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bác sĩ y lệnh thuốc:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ"
                            );
                    }

                }

                //2060	4	XML2	NGAY_TH_YL	[130] Ngày thực hiện y lệnh thuốc không được lớn hơn thời điểm hiện tại
                //2297    2297    0   XML2 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được nhỏ hơn Ngày y lệnh
                if (File.NGAY_YL != null && File.NGAY_TH_YL != null)
                {
                    //177 1   XML2 NGAY_YL Ngày y lệnh thuốc không được lớn hơn thời gian hiện tại
                    if (File.NGAY_YL > DateTime.Now)
                    {
                        RaiseError(true,
                            177,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(177, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian hiện tại:{DateTime.Now:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }
                    if (File.NGAY_TH_YL > DateTime.Now)
                    {
                        RaiseError(true,
                            2060,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2060, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Thời gian hiện tại:{DateTime.Now:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }
                    if (File.NGAY_TH_YL < File.NGAY_YL)
                    {
                        RaiseError(true,
                            2297,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2297, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian Y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }
                    if (File.NGAY_TH_YL > Xml01.NGAY_RA)
                    {
                        RaiseError(true,
                            2069,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2069, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Thời gian ra viện:{Xml01.NGAY_RA:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );

                    }
                    //2069	4	XML2	NGAY_TH_YL	[130] Ngày thực hiện y lệnh thuốc không được lớn hơn ngày ra viện

                    //2070    4   XML2 NGAY_TH_YL[130] Ngày thực hiện y lệnh thuốc không được nhỏ hơn ngày vào viện
                    if (File.NGAY_TH_YL <= Xml01.NGAY_VAO)
                    {
                        RaiseError(true,
                            2070,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2070, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Thời gian vào viện:{Xml01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }

                    //2389    3   XML2 NGAY_TH_YL[130] Kê thuốc trùng thời điểm ra viện
                    if (File.NGAY_TH_YL == Xml01.NGAY_RA)
                    {
                        RaiseError(true,
                            2389,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2389, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian thực hiện y lệnh thuốc:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Thời gian ra viện:{Xml01.NGAY_RA:dd/MM/yyyy HH:mm:ss}, Không phù hợp logic thời gian"
                            );
                    }

                }
                //739 Liều dùng không đúng định dạng (Có các định dạng sau: 
                //Định dạng 1:  2 viên/ lần * 2 lần trên ngày * 5 ngày
                //Định dạng 2: sáng:3 viên, chiều: 2 viên, tối: 1 viên
                //Định dạng 3: 12g * 1 tháng * 5 ngày
                //887: Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
                //1636	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán

                //93	XML2	LIEU_DUNG	Liều dùng không được để trống
                //171	XML2	LIEU_DUNG	Liều dùng không được lớn hơn 1024 ký tự
                Field(x => x.LIEU_DUNG).IsEmpty().ThenError(93).LengthOver(1024).ThenError(171);

                //Linhist added 02/04/26
                //3973	2	XML2	SO_LUONG	Số lượng kê khi sử dụng Oxy bệnh nhân không khớp với số lượng trên liều dùng
                if (File.MA_THUOC == "40.17" && !string.IsNullOrEmpty(File.LIEU_DUNG) && File.SO_LUONG != null)
                {
                    //Định dạng: 02 lít/phút * 01 ngày [2880 lít/ ngày]
                    var lieu = File.LIEU_DUNG?.Replace(" ", "").ToLower();
                    lieu ??= string.Empty;
                    var idx = lieu.IndexOf("[");
                    if (idx > 0 && (lieu.Contains("lít/phút") || lieu.Contains("l/phút")) && lieu.Contains("ngày]"))
                    {
                        lieu = lieu[(idx + 1)..];
                        idx = lieu.IndexOf("lít/ngày]");
                        if (idx < 0) idx = lieu.IndexOf("l/ngày]");

                        if (idx > 0)
                        {
                            lieu = lieu[..idx];
                            if (double.TryParse(lieu, out var luong) && luong != File.SO_LUONG)
                            {
                                RaiseError(true,
                                    3973,
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3973, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc Oxy với liều dùng: {File.LIEU_DUNG}, Số lượng kê: {File.SO_LUONG}, Số lượng trên liều dùng tương ứng là: {luong}, Không khớp với số lượng kê, đề nghị xem lại"
                                );
                            }
                        }
                    }
                }

                if (File.MA_THUOC != "40.17" && File.MA_THUOC != "40.573")
                {
                    if (Field(x => x.LIEU_DUNG).NotEmpty())
                    {
                        //Linhist changed 03/08/26
                        Validate_LieuDung();
                    }

                    //2941	3	XML2	CACH_DUNG	[3176] Nội dung cách dùng bắt buộc phải nhập
                    Field(x => x.CACH_DUNG).IsEmpty().ThenError(2941);

                    //2942    3   XML2 CACH_DUNG[3176] Nội dung nhập quá ngắn hay chỉ là số, đề nghị kiểm tra
                    //2943    3   XML2 CACH_DUNG[3176] Đơn vị tính trong cách dùng không đúng với đơn vị tính của Thuốc, xem lại
                    if (!string.IsNullOrEmpty(File.CACH_DUNG))
                    {
                        if ((File.CACH_DUNG ?? string.Empty).ToString().Length < 4)
                        {
                            RaiseError(true,
                                2942,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2942, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Cách dùng thuốc:{File.CACH_DUNG}, ít hơn 4 ký tự, đề nghị xem lại"
                                );
                        }
                        if ((File.DON_VI_TINH ?? string.Empty).ToLower() != "gói")
                        {
                            if (!checkCachDung((File.CACH_DUNG ?? string.Empty).ToLower(), (File.DON_VI_TINH ?? string.Empty).ToLower()))
                            {
                                RaiseError(true,
                                    2943,
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2943, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Cách dùng thuốc:{File.CACH_DUNG}, Đơn vị tính thuốc:{File.DON_VI_TINH}, Không đúng với đơn vị tính của thuốc, đề nghị xem lại"
                                    );
                            }

                        }

                    }
                }

                string thuocVT = dmdacBiet.ContainsKey("thuocvuottuyen") ? dmdacBiet["thuocvuottuyen"] : "";
                //465 1   XML2 MA_THUOC    Chỉ định thuốc vượt tuyến thiếu biên bản hội chẩn
                if (File.NGAY_YL != null && Uti.CodeContain(File.MA_THUOC ?? string.Empty, thuocVT))
                {
                    var allHoiChans = (Xml01.XML05s ?? Enumerable.Empty<XML05>()).Where(x => x.THOI_DIEM_DBLS != null && x.THOI_DIEM_DBLS <= File.NGAY_YL);
                    if (!allHoiChans.Any(x => Uti.StrExists(x.HOI_CHAN ?? string.Empty, true, "Tuyến", "Vượt Tuyến", "Invanz", "Nimedine", "Tiepanem", "Colistimetato", "Fosmicin", "Linezolid", "Kidmin", "Amiparen", "MG - Tan", "Combilipid", "Doripenem", "Ertapenem", "Impienem", "Meropenem", "Colistin", "Fosfomycin", "Linezolid", "Teicoplanin", "Amphotericin", "Acid amin", "Hội chẩn thuốc", "Dùng thuốc", "Thuốc", "Nephrosteril")))
                    {
                        RaiseError(true,
                            465,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(465, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã thuốc vượt tuyến, Thời gian y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Không tìm thấy biên bản hội chẩn (Có nội dung hội chẩn chuẩn) có thời điểm trước thời điểm y lệnh thuốc, đề nghị kiểm tra lại"
                            );
                    }
                }

                #endregion

            }

            //Linhist added 27/02/26
            if (Field(x => x.MA_THUOC).NotEmpty())
            {
                //3884 XML2 MA_THUOC    Chỉ định dịch vụ Oxy theo giờ cho Bệnh nhân bị trùng khoảng thời gian
                if (File.MA_THUOC == "40.17" && File.NGAY_YL != null && File.SO_LUONG != null)
                {
                    var end_Oxi = File.NGAY_YL.Value.AddMinutes(File.SO_LUONG.Value * 60d);
                    var has_Oxi = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Any(x => x.MA_THUOC == "40.17" && x.XML02ID != File.XML02ID && x.NGAY_YL != null &&
                                                                x.SO_LUONG != null && x.NGAY_YL <= end_Oxi &&
                                                                x.NGAY_YL.Value.AddMinutes(x.SO_LUONG.Value * 60d) >= File.NGAY_YL);
                    RaiseError(has_Oxi, 3884);
                }

                //3885 XML2 MA_DICH_VU  Vị thuốc y học cổ truyền(Nguyên lieu thô) bắt buộc phải đi kèm với các mã dịch vụ sắc thuốc thang
                if ((File.MA_THUOC ?? string.Empty).StartsWith("05v", StringComparison.OrdinalIgnoreCase))
                {
                    Field(x => x.MA_DICH_VU).IsEmpty().NotValue("03.0276.0252", "03.0284.0252", "08.0015.0252", "08.0022.0252").ThenError(3885);
                }

                //3886 XML2 MA_DICH_VU  Các thuốc khí dung phải đi kèm với Dịch vụ Khí dung
                //Linhist changed 18/06/26
                if (Uti.CodeMatch(File.MA_THUOC ?? string.Empty, "40.980;40.973") && Uti.CodeContain(File.DUONG_DUNG ?? string.Empty, "5.05;5.06"))
                {
                    if (Xml01.XML03s.Any(x => Uti.HCodeContain(x.MA_DICH_VU ?? string.Empty, "01.0086.0898;01.0087.0898;02.0032.0898;09.0123.0898")))
                    {
                        
                        if (File.MA_DICH_VU == null || !Uti.CodeContain(File.MA_DICH_VU, "01.0086.0898;01.0087.0898;02.0032.0898;09.0123.0898"))
                        {
                            RaiseError(true,
                                3886,
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3886, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Các thuốc khí dung phải đi kèm với Dịch vụ Khí dung, tuy nhiên mã dịch vụ không đúng hoặc không có, đề nghị kiểm tra lại"
                            );
                        }
                    }
                }

                //3887 XML2 MA_DICH_VU  Bệnh nhân được thuốc cho dịch vụ thủy châm, tuy nhiên không nhập mã Dịch vụ thủy châm
                if (Uti.CodeMatch(File.MA_THUOC ?? string.Empty, "40.1049;40.1050") &&
                        (File.MA_KHOA ?? string.Empty).StartsWith("k16", StringComparison.OrdinalIgnoreCase) &&
                        ((File.CACH_DUNG ?? string.Empty).Contains("thủy châm", StringComparison.OrdinalIgnoreCase) || (File.CACH_DUNG ?? string.Empty).Contains("thuỷ châm")))
                {
                    //Linhist changed 29/05/26 - thêm mã 08.0006.0271
                    if (File.MA_DICH_VU ==null || !Uti.CodeMatch( File.MA_DICH_VU, "03.006.0271;08.0322.0271;08.0322.0271;08.0330.0271;08.0356.0271;08.0375.0271;08.0377.0271;08.0378.0271;08.0006.0271"))
                    {
                        RaiseError(true,
                            3887,
                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3887, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được thuốc cho dịch vụ thủy châm, tuy nhiên không nhập mã Dịch vụ thủy châm, đề nghị kiểm tra lại \n" +
                            $"(Mã dịch vụ thủy châm hợp lệ: 03.006.0271;08.0322.0271;08.0322.0271;08.0330.0271;08.0356.0271;08.0375.0271;08.0377.0271;08.0378.0271)"
                        );
                    }    
                }

                //3888 XML2 MA_DICH_VU  Các vị thuốc không cần phải nhập thông tin mã Dịch vụ
                if (!Uti.CodeMatch(File.MA_THUOC ?? string.Empty, "05V.298;05V.334;05V.53;05V.301;05V.226;05V.56;05V.295;05V.328;05V.303;05V.331;05V.319;05V.37;05V.296;05V.179;05V.315;05V.17;05V.330;05V.211;05V.151;05V.212;05V.35;05V.1;05V.333;05V.106;05V.217;05V.280;05V.91;05V.245;05V.181;05V.92;05V.282;05V.305;05V.45;05V.221;05V.124;05V.47;05V.6;05V.205;05V.135;05V.290;05V.185;05V.9;05V.29;05V.311;05V.279;05V.327;05V.189;05V.137;05V.329;05V.156;05V.258;40.980;40.973;40.1049;40.1050;05V.133;05V.171;05V.213;05V.218.P1;05V.226.S1;05V.236.S1;05V.27;05V.282.S1;05V.295.S1;05V.296.S1;05V.298.S1;05V.299;05V.309;05V.315.S1;05V.316.P1;05V.319.P1;05V.327.P1;05V.328.S1;05V.331.P1;05V.333.S1;05V.334.S1;05V.37.P1;05V.47.P1;05V.68;05V.91.P1;05V.97.P1"))
                {
                    Field(x => x.MA_DICH_VU).NotEmpty().ThenError(3888);
                }
            }

            //Linhist added 27/08/26 - cho lỗi mức hưởng set vào XMLInfo
            //761	1	XML2	MUC_HUONG	Sai mức hưởng BHYT
            if (!Uti.IsValidDate(Xml01.NGAY_MIEN_CCT))
            {
                if (Info.MAX_MUC_HUONG > 0d && !Uti.IsEqualUp(File.MUC_HUONG, Info.MAX_MUC_HUONG))
                {
                    RaiseError(true,
                        761,
                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(761, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết: Mức hưởng BHYT của bệnh nhân là {File.MUC_HUONG}, không đúng với mức hưởng quy định là {Info.MAX_MUC_HUONG}, đề nghị xem lại"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            if (_logger != null) Uti.HandleCronJobLog(_logger, $"Validate130: XML2 MaBN: {Xml01.MA_BN} MaCSKCB: {Xml01.MA_CSKCB}; STT Bị Lỗi: {File.STT} XML2 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
        }

        var result = Validate_HDSDTHUOC();

        return result;
    }

    private bool Validate_HDSDTHUOC()
    {
        // cần mã bệnh ICD10 --> Info.MA_BENH_ICD10
        // Cần có thai --> Info.CO_THAI
        // Cần cho con bú --> Info.CHO_CON_BU
        // Cần tuổi BN --> Info.TUOI_BN
        if (cungHDSDs == null) return false;
        
        if (File.MA_THUOC == "40.17" || File.MA_THUOC == "40.573") return false;
        foreach (HDSDThuoc rule in cungHDSDs)
        {
            if (rule == null) continue;

            try
            {
                bool hasErr = false;
                //Kiem tra DUONG_DUNG neu ko phu hop thi bo qua rule nay
                if (!string.IsNullOrEmpty(rule.DUONG_DUNG) && !Uti.CodeContain(File.DUONG_DUNG ?? string.Empty, rule.DUONG_DUNG)) continue;

                // Case chi dinh:
                if (rule.CHI_DINH == true)
                {
                    //Linhist added 100826
                    if (string.IsNullOrEmpty(rule.TEN_THUOC))
                    {
                        //Kiem tra MA_BENH: neu khong co MA_BENH theo ICD10 tuong ung --> bao loi
                        //haimaster 6/8/2022 update chi co ma benh, khong co thuoc khac
                        if (!string.IsNullOrEmpty(rule.MA_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC) && string.IsNullOrEmpty(rule.TEN_BENH))
                        {
                            hasErr = true;
                            foreach (var benhInfo in Info.MA_BENH_ICD10.Split(";"))
                            {
                                if (Uti.CodeMatch(benhInfo, rule.MA_BENH))
                                {
                                    hasErr = false;
                                    break;

                                }

                            }
                            if (hasErr)
                            {
                                RaiseError(true,
                                    rule.MA_LOI.GetValueOrDefault(),
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bệnh ICD10 của bệnh nhân, Không phù hợp với mã bệnh chỉ định sử dụng thuốc theo phê duyệt, đề nghị xem lại"
                                    );
                            }
                        }

                        //Kiem tra THUOC_KHAC: neu khong co it nhat 1 MA_THUOC trong cung ho so tuong ung voi THUOC_KHAC --> bao loi
                        //haimaster 6/8/2022 update chi co thuoc khac, khong co ma benh

                        if (!string.IsNullOrEmpty(rule.THUOC_KHAC) && File.XML01 != null && string.IsNullOrEmpty(rule.MA_BENH))
                        {
                            var thuockhac = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Where(x => Uti.CodeMatch(x.MA_THUOC ?? string.Empty, rule.THUOC_KHAC));
                            if (thuockhac == null)
                            {
                                hasErr = true;
                                RaiseError(true,
                                    rule.MA_LOI.GetValueOrDefault(),
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã thuốc khác chỉ định sử dụng cùng thuốc theo phê duyệt:, Không tìm thấy mã thuốc tương ứng trong hồ sơ bệnh án, đề nghị xem lại"
                                    );
                            }
                        }

                        //haimaster 6/8/2022: them truoc hop co ca thuoc khac va ma benh
                        if (!string.IsNullOrEmpty(rule.MA_BENH) && !string.IsNullOrEmpty(rule.THUOC_KHAC))
                        {
                            string benhinfordetain = "";
                            foreach (var benhinfor in Info.MA_BENH_ICD10)
                            {
                                if (!Uti.CodeMatch(benhinfor.ToString(), rule.MA_BENH))
                                {
                                    hasErr = true;
                                    benhinfordetain = benhinfor.ToString();
                                    break;

                                }
                            }
                            if (hasErr)
                            {
                                RaiseError(true,
                                        rule.MA_LOI.GetValueOrDefault(),
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bệnh ICD10 của bệnh nhân:{benhinfordetain}, Không phù hợp với mã bệnh chỉ định sử dụng thuốc theo phê duyệt:, Đề nghị xem lại"
                                        );
                            }
                            var thuockhac = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Where(x => Uti.CodeMatch(x.MA_THUOC ?? string.Empty, rule.THUOC_KHAC));
                            if (!thuockhac.Any())
                            {
                                hasErr = true;
                                RaiseError(true,
                                    rule.MA_LOI.GetValueOrDefault(),
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã thuốc khác chỉ định sử dụng cùng thuốc theo phê duyệt:, Không tìm thấy mã thuốc tương ứng trong hồ sơ bệnh án, đề nghị xem lại"
                                    );
                            }
                        }
                        //haimaster 25.11.2022 them Dich Vu, Chi dich thuoc phai co dich vu

                        if (!string.IsNullOrEmpty(rule.DICH_VU) && !(Xml01.XML03s ?? Enumerable.Empty<XML03>()).Any(y => Uti.CodeMatch(y.MA_DICH_VU ?? string.Empty, rule.DICH_VU)))
                        {
                            hasErr = true;
                            RaiseError(true,
                                rule.MA_LOI.GetValueOrDefault(),
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã dịch vụ chỉ định sử dụng cùng thuốc theo phê duyệt:, Không tìm thấy mã dịch vụ tương ứng trong hồ sơ bệnh án, đề nghị xem lại"
                                );
                        }

                        //Linhist added 10/06/24
                        //Kiem tra thuoc co TEN_BENH & MA_BENH:
                        if (!string.IsNullOrEmpty(rule.TEN_BENH))
                        {
                            if (!string.IsNullOrEmpty(rule.MA_BENH))
                            {
                                if (!Uti.CodeMatch(Info.MA_BENH_ICD10, rule.MA_BENH) || !(Xml01.CHAN_DOAN_RV ?? string.Empty).Contains(rule.TEN_BENH, StringComparison.OrdinalIgnoreCase))
                                {
                                    RaiseError(true,
                                        rule.MA_LOI.GetValueOrDefault(),
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bệnh ICD10 của bệnh nhân:{Info.MA_BENH_ICD10}, Tên bệnh của bệnh nhân:{Xml01.CHAN_DOAN_RV}, Không phù hợp với mã bệnh chỉ định sử dụng thuốc theo phê duyệt: và tên bệnh theo chỉ định sử dụng thuốc theo phê duyệt:, đề nghị xem lại"
                                        );
                                }
                            }
                            else
                            {
                                if (!(Xml01.CHAN_DOAN_RV ?? string.Empty).Contains(rule.TEN_BENH, StringComparison.OrdinalIgnoreCase))
                                {
                                    hasErr = true;
                                    RaiseError(true,
                                        rule.MA_LOI.GetValueOrDefault(),
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tên bệnh của bệnh nhân:{Xml01.CHAN_DOAN_RV}, Không phù hợp với tên bệnh theo chỉ định sử dụng thuốc theo phê duyệt:, đề nghị xem lại"
                                        );
                                }
                            }
                        }

                        //Linhist changed 12/06/24
                        // haimaster 26/05/2022: Kiem tra case cu ke thuoc la bao 25.11.2022 them DICHVU
                        //haimaster 23.9.2025 chưa hiểu code này làm gì? không có giá trị, Tạm REM lại co sgif tính toán sau
                        if (string.IsNullOrEmpty(rule.MA_BENH) && string.IsNullOrEmpty(rule.TEN_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC) && string.IsNullOrEmpty(rule.DICH_VU))
                        {
                            RaiseError(true, rule.MA_LOI.GetValueOrDefault());
                        }
                        //haimaster 11.09.2025 them case chi dinh cho TY LE Thanh toán
                        if (!string.IsNullOrEmpty(rule.TYLE_TT) && File.TYLE_TT_BH != null)
                        {
                            if ((File.TYLE_TT_BH == 0 && File.T_NGUONKHAC == File.THANH_TIEN_BH) && string.Compare((File.MA_THUOC ?? string.Empty).ToString(), "40.30.272") == 0 || string.Compare((File.MA_THUOC ?? string.Empty).ToString(), "40.30.273") == 0)
                            {

                            }
                            //29.11.2022 haimaster upate khong kiem tra cho Cong An va QUan Nhan
                            else if (((Xml01.MA_THE_BHYT ?? string.Empty).ToString().Substring(0, 2) == "CA") || ((Xml01.MA_THE_BHYT ?? string.Empty).ToString().Substring(0, 2) == "QN"))
                            {

                            }
                            if (Info.TUOI_BN < 6)
                            {

                            }
                            //Linhist changed 08/11/22: neu xml2 co ty le thanh toan phu hop dieu kien --> bao loi
                            else if (!Uti.NumMatch(File.TYLE_TT_BH.Value, rule.TYLE_TT))
                            {
                                hasErr = true;
                                RaiseError(true,
                                    rule.MA_LOI.GetValueOrDefault(),
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tỷ lệ thanh toán của thuốc:{File.TYLE_TT_BH}%, Không phù hợp với tỷ lệ thanh toán chỉ định sử dụng thuốc theo phê duyệt:{rule.TYLE_TT}%, đề nghị xem lại"
                                    );
                            }
                        }
                    }
                    else if (rule.MA_THUOC == File.MA_THUOC && rule.TEN_THUOC == File.TEN_THUOC)
                    {
                        //Kiem tra MA_BENH: neu khong co MA_BENH theo ICD10 tuong ung --> bao loi
                        //haimaster 6/8/2022 update chi co ma benh, khong co thuoc khac
                        if (!string.IsNullOrEmpty(rule.MA_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC) && string.IsNullOrEmpty(rule.TEN_BENH))
                        {
                            hasErr = true;
                            foreach (var benhInfo in Info.MA_BENH_ICD10.Split(";"))
                            {
                                if (Uti.CodeMatch(benhInfo, rule.MA_BENH))
                                {
                                    hasErr = false;
                                    break;

                                }

                            }
                            if (hasErr)
                            {
                                RaiseError(true,
                                    rule.MA_LOI.GetValueOrDefault(),
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bệnh ICD10 của bệnh nhân, Không phù hợp với mã bệnh chỉ định sử dụng thuốc theo phê duyệt, đề nghị xem lại"
                                    );
                            }
                        }

                        //Linhist added 10/06/24
                        //Kiem tra thuoc co TEN_BENH & MA_BENH:
                        if (!string.IsNullOrEmpty(rule.TEN_BENH))
                        {
                            if (!string.IsNullOrEmpty(rule.MA_BENH))
                            {
                                if (!Uti.CodeMatch(Info.MA_BENH_ICD10, rule.MA_BENH) || !(Xml01.CHAN_DOAN_RV ?? string.Empty).Contains(rule.TEN_BENH, StringComparison.OrdinalIgnoreCase))
                                {
                                    RaiseError(true,
                                        rule.MA_LOI.GetValueOrDefault(),
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bệnh ICD10 của bệnh nhân:{Info.MA_BENH_ICD10}, Tên bệnh của bệnh nhân:{Xml01.CHAN_DOAN_RV}, Không phù hợp với mã bệnh chỉ định sử dụng thuốc theo phê duyệt: và tên bệnh theo chỉ định sử dụng thuốc theo phê duyệt:, đề nghị xem lại"
                                        );
                                }
                            }
                            else
                            {
                                if (!(Xml01.CHAN_DOAN_RV ?? string.Empty).Contains(rule.TEN_BENH, StringComparison.OrdinalIgnoreCase))
                                {
                                    hasErr = true;
                                    RaiseError(true,
                                        rule.MA_LOI.GetValueOrDefault(),
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tên bệnh của bệnh nhân:{Xml01.CHAN_DOAN_RV}, Không phù hợp với tên bệnh theo chỉ định sử dụng thuốc theo phê duyệt:, đề nghị xem lại"
                                        );
                                }
                            }
                        }
                    }
                }
                // Case chong chi dinh:
                else
                {
                    if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
                    {
                        //Kiem tra TRE_SO_SINH: neu so ngay tuoi cua BN < dieu kien ngay tuoi TRE_SO_SINH ma ke THUOC_KHAC thi bao loi   
                        if (rule.TRE_SO_SINH != null && rule.TRE_SO_SINH != 0d)
                        {
                            if (Info.NGAY_TUOI_BN < rule.TRE_SO_SINH)
                            {
                                var thuockhac = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Where(x => Uti.CodeMatch(x.MA_THUOC ?? string.Empty, rule.THUOC_KHAC));
                                if (thuockhac.Any())
                                {
                                    hasErr = true;
                                    RaiseError(true,
                                        rule.MA_LOI.GetValueOrDefault(),
                                        $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                        $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tuổi bệnh nhân:{Info.TUOI_BN} ngày, Không phù hợp với độ tuổi chỉ định sử dụng thuốc khác:  theo phê duyệt (Dưới {rule.TRE_SO_SINH} ngày tuổi và kê thuốc: ), đề nghị xem lại"
                                        );

                                }
                            }
                        }

                        //Kiem tra THOI_GIAN_TOI_THIEU neu trong thoi gian toi thieu ke dong thoi voi THUOC_KHAC thi bao loi
                        else if (rule.THOI_GIAN_TOI_THIEU != null)
                        {
                            if (File.NGAY_YL != null)
                            {
                                if (rule.THOI_GIAN_TOI_THIEU == 0d)
                                {
                                    var thuoctoithieu = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Where(x => x.XML02ID != File.XML02ID && Uti.CodeMatch(x.MA_THUOC ?? string.Empty, rule.THUOC_KHAC) && x.NGAY_YL == File.NGAY_YL);
                                    if (thuoctoithieu.Any())
                                    {
                                        hasErr = true;
                                        RaiseError(true,
                                            rule.MA_LOI.GetValueOrDefault(),
                                            $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                            $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp với thời gian chỉ định sử dụng thuốc khác: theo phê duyệt (Cùng thời điểm y lệnh và kê thuốc: ), đề nghị xem lại"
                                            );
                                    }
                                }
                                else
                                {
                                    DateTime dMin = File.NGAY_YL.Value.AddHours(-rule.THOI_GIAN_TOI_THIEU.Value);
                                    DateTime dMax = File.NGAY_YL.Value.AddHours(rule.THOI_GIAN_TOI_THIEU.Value);
                                    if (File.XML01 != null)
                                    {
                                        var thuockhactoithieu = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Where(x => x.XML02ID != File.XML02ID && Uti.CodeMatch(x.MA_THUOC ?? string.Empty, rule.THUOC_KHAC) && x.NGAY_YL != null && x.NGAY_YL.Value > dMin && x.NGAY_YL.Value < dMax);
                                        if (thuockhactoithieu.Any())
                                        {
                                            hasErr = true;
                                            RaiseError(true,
                                                rule.MA_LOI.GetValueOrDefault(),
                                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Thời gian y lệnh thuốc:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Không phù hợp với thời gian chỉ định sử dụng thuốc khác:  theo phê duyệt (Trong khoảng {rule.THOI_GIAN_TOI_THIEU} giờ và kê thuốc:, đề nghị xem lại"
                                                );

                                        }
                                    }
                                }
                            }
                        }

                        //Kiem tra THUOC_KHAC: neu co it nhat 1 MA_THUOC trong cung ho so tuong ung voi THUOC_KHAC --> bao loi
                        else
                        {
                            var thuockhacs = (Xml01.XML02s ?? Enumerable.Empty<XML02>()).Where(x => Uti.CodeMatch(x.MA_THUOC ?? string.Empty, rule.THUOC_KHAC));
                            if (thuockhacs.Any())
                            {
                                RaiseError(true,
                                    rule.MA_LOI.GetValueOrDefault(),
                                    $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                    $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Không phù hợp với chỉ định sử dụng thuốc khác:  theo phê duyệt (Kê thuốc: ), đề nghị xem lại"
                                    );
                            }

                        }
                    }

                    //Kiem tra DO_TUOI: tuoi cua benh nhan phu hop voi dieu kien DO_TUOI --> bao loi 
                    if (!string.IsNullOrEmpty(rule.TUOI))
                    {
                        if (Uti.NumMatch(Info.TUOI_BN, rule.TUOI))
                        {
                            RaiseError(true,
                                rule.MA_LOI.GetValueOrDefault(),
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tuổi bệnh nhân:{Info.TUOI_BN} tuổi, Không phù hợp với độ tuổi chỉ định sử dụng thuốc theo phê duyệt:{rule.TUOI}, đề nghị xem lại"
                                );

                        }

                    }

                    if (rule.CO_THAI == true)
                    {
                        if (Info.CO_THAI == true)
                        {
                            hasErr = true;
                            RaiseError(true,
                                rule.MA_LOI.GetValueOrDefault(),
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Bệnh nhân là phụ nữ có thai, Không phù hợp với chỉ định sử dụng thuốc theo phê duyệt không dùng cho phụ nữ có thai, đề nghị xem lại"
                                );

                        }

                    }

                    if (rule.CHO_CON_BU == true)
                    {
                        if (Info.CHO_CON_BU == true)
                        {
                            hasErr = true;
                            RaiseError(true,
                                rule.MA_LOI.GetValueOrDefault(),
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Bệnh nhân là phụ nữ đang cho con bú, Không phù hợp với chỉ định sử dụng thuốc theo phê duyệt không dùng cho phụ nữ đang cho con bú, đề nghị xem lại"
                                );
                        }
                    }

                    //Kiem tra MA_BENH: neu co MA_BENH theo ICD10 tuong ung --> bao loi
                    if (!string.IsNullOrEmpty(rule.MA_BENH))
                    {
                        hasErr = false;
                        List<string> benhloi = new List<string>();
                        foreach (var b in Info.MA_BENH_ICD10.Split(";"))
                        {
                            if (Uti.CodeMatch(b, rule.MA_BENH))
                            {
                                hasErr = true;
                                benhloi.Add(b);

                            }
                        }
                        if (hasErr)
                        {
                            var error = dmError130.FirstOrDefault(x => x.ErrorCode == rule.MA_LOI);
                            RaiseError(true,
                                rule.MA_LOI.GetValueOrDefault(),
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Mã bệnh ICD10 của bệnh nhân:{string.Join(",", benhloi)}, Phù hợp với mã bệnh chỉ định không sử dụng thuốc theo phê duyệt:, Đề nghị xem lại"
                                );
                        }

                    }

                    //Linhist removed 30/05/22 chuyen len tren, kiem tra cung case khac
                    //Kiem tra THUOC_KHAC: neu co it nhat 1 MA_THUOC trong cung ho so tuong ung voi THUOC_KHAC --> bao loi

                    //Kiem tra TYLE_TT: neu xml2 co ty le thanh toan khong phu hop dieu kien --> bao loi
                    //haimaster: 26/5/2022 chinh sua lai dieu kien: Bo dau ! khi so sanh
                    //Linhist changed 08/11/22

                    if (!string.IsNullOrEmpty(rule.TYLE_TT) && File.TYLE_TT_BH != null)
                    {
                        if ((File.TYLE_TT_BH == 0 && File.T_NGUONKHAC == File.THANH_TIEN_BH) && string.Compare((File.MA_THUOC ?? string.Empty).ToString(), "40.30.272") == 0 || string.Compare((File.MA_THUOC ?? string.Empty).ToString(), "40.30.273") == 0)
                        {

                        }
                        //29.11.2022 haimaster upate khong kiem tra cho Cong An va QUan Nhan
                        else if (((Xml01.MA_THE_BHYT ?? string.Empty).ToString().Substring(0, 2) == "CA") || ((Xml01.MA_THE_BHYT ?? string.Empty).ToString().Substring(0, 2) == "QN"))
                        {

                        }
                        if (Info.TUOI_BN < 6)
                        {

                        }
                        //Linhist changed 08/11/22: neu xml2 co ty le thanh toan phu hop dieu kien --> bao loi
                        else if (Uti.NumMatch(File.TYLE_TT_BH.Value, rule.TYLE_TT))
                        {
                            hasErr = true;
                            RaiseError(true,
                                rule.MA_LOI.GetValueOrDefault(),
                                $"Bệnh nhân: {Xml01.HO_TEN} (Mã BN: {Xml01.MA_BN})\n" +
                                $"Mã ra viện: {Xml01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, Xml01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {Xml01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI.GetValueOrDefault(), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh thuốc:{File.MA_THUOC}|{File.TEN_THUOC}, Tỷ lệ thanh toán của thuốc:{File.TYLE_TT_BH}%, Phù hợp với tỷ lệ thanh toán chỉ định không sử dụng thuốc theo phê duyệt:{rule.TYLE_TT}%, đề nghị xem lại"
                                );
                        }
                    }
                }

                //RAISE_ERROR_HDSD_THUOC:
            }
            catch (Exception ex) 
            {
                if (_logger != null) Uti.HandleCronJobLog(_logger, $"Validate130: XML2 HDSD Thuoc MaBN: {Xml01.MA_BN} MaCSKCB: {Xml01.MA_CSKCB}; STT Bị Lỗi: {File.STT} XML3 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
            }
        }

        return true;
    }
}
