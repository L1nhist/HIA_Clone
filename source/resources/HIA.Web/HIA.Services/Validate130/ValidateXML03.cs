using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.Record.Chart;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HIA.Services.Validate130
{
    public class ValidateXML03 : BaseValidator<XML03>
    {
        private bool isDichVu = true;

        //Linhist changed 20/03/26
        //private readonly IEnumerable<Hospital> dMhospital;
        private readonly TimePeriod _officialTime;

        private XMLCommonInfo Info;
        private readonly IEnumerable<DMYHDTICD10> dmYHCT;
        private readonly IEnumerable<DMDichVu> dmDICHVU;
        private readonly IEnumerable<DMVTYT> dmVTYT;
        private readonly IEnumerable<DMBacSi> dmBACSI;
        private readonly IEnumerable<HDSDDichVu> hdsdDV;
        private readonly IEnumerable<DMKhoaBV> dmKhoaBV;
        private readonly IEnumerable<DMDVThuCong> dmTHUCONG;
        private readonly IEnumerable<DMThietBi> dmTHIETBI;
        private readonly IEnumerable<DMChung130> dmChung130;
        private readonly IEnumerable<ChiDinhDongThoi> ruleDongthoi;
        private readonly IEnumerable<DMError130> dmError130;
        private readonly IXml01Repository _hosoRepository;
        private readonly IBaseRepository<XML03> _xml03Repository;
        private readonly IBaseRepository<XML02> _xml02Repository;
        private readonly IEnumerable<DMErrorMHTC> _dmErrorMHTCs;
        private readonly IEnumerable<DichVuMaMay> _dvuMamays;
        private readonly ILogger<dynamic> _logger;

        private string curMADICHVU;
        private IEnumerable<DMDichVu> cungMADICHVUs;
        private IEnumerable<HDSDDichVu> cungHDSDs;

        private string curMAVATTU;
        private IEnumerable<DMVTYT> cungMAVATTUs;

        private Dictionary<int, string> errorlist;
        string benhchinhInfo;

        public class TT042017 
        { 
            public string ID { get; set; } 
            public long Value { get; set; } 
        }
        public class DVTrung
        {
            public string MA_DV1 { get; set; }
            public string MA_DV2 { get; set; }
        }
        public class QD5937
        {
            public string MA_DV { get; set; }
            public string Value { get; set; }
        }
        public string TT_CUT(string strin)
        {
            if(strin.IndexOf("[")!=-1&& strin.IndexOf("]")!=-1)
            {
                strin = strin.Substring(0, strin.IndexOf("["));
                //Uti.HandleCronJobLog(_logger, $"TEN_DV:,{strin}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML03)}");

            }   
            return strin.TrimEnd();
        }
        public int isGayMeGayTe(string tenDV)
        {
            //1 là gây mê
            //2 là gây tê
            //3 là vừa gây mê tây tê
            //-1 không phải gây mê gây tê
            if (tenDV.Contains("gây mê") && !tenDV.Contains("gây tê")) return 1;
            if (!tenDV.Contains("gây mê") && tenDV.Contains("gây tê")) return 2;
            if (tenDV.Contains("gây mê") && tenDV.Contains("gây tê")) return 3;
            return -1;

        }
        

        public ValidateXML03(
            //Linhist changed 20/03/26
            //IEnumerable<Hospital> DMhospital,
            TimePeriod officialTime,

            IEnumerable<DMYHDTICD10> DmYHCT,
            IEnumerable<DMChung130> DMchung130,
            IEnumerable<ChiDinhDongThoi> RuleDongthoi,
            IEnumerable<DMKhoaBV> dMKhoaBVs,
            IEnumerable<DMDichVu> DMDichVus,
            IEnumerable<DMVTYT> DMVtyts,
            IEnumerable<DMBacSi> DMBacSis,
            IEnumerable<DMDVThuCong> DMThuCongs,
            IEnumerable<DMThietBi> DMThietBis,
            IEnumerable<HDSDDichVu> HDSDDichVus,
            IEnumerable<DMError130> dMError130s,
            IXml01Repository hosoRepository,
            IBaseRepository<XML03> xml03Repository,
            IBaseRepository<XML02> xml02Repository,
            IEnumerable<DMErrorMHTC> dMErrorMHTCs,
            IEnumerable<DichVuMaMay> dvuMamays,
            ILogger<dynamic> logger = null
        ) : base(logger)
        {
            //Linhist changed 20/03/26
            //dMhospital = DMhospital;
            _officialTime = officialTime;

            dmYHCT = DmYHCT;
            ruleDongthoi = null;
            curMADICHVU = null;
            cungMADICHVUs = null;
            curMAVATTU = null;
            cungMAVATTUs = null;
            dmDICHVU = DMDichVus;
            dmVTYT = DMVtyts;
            dmBACSI = DMBacSis;
            dmTHUCONG = DMThuCongs;
            dmTHIETBI = DMThietBis;
            hdsdDV = HDSDDichVus;
            dmKhoaBV = dMKhoaBVs;
            ruleDongthoi = RuleDongthoi;
            dmChung130 = DMchung130;
            dmError130 = dMError130s;
            _hosoRepository = hosoRepository;
            _xml03Repository = xml03Repository;
            _xml02Repository = xml02Repository;
            _dmErrorMHTCs = dMErrorMHTCs;
            _dvuMamays = dvuMamays;
            _logger = logger;
        }

        public  List<DayOfWeek> ParseWeekdays(string input)
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
        public bool checkMA_MAY(string MA_MAY, string MA_CSKCB)
        {
            //MA_MAY phai theo dinh dang XX.n.YYYYY.ZZZZZZ
            //XX: Ma nhom (viet tat ky tu dau toi da 3 ky tu)
            //n: Ma nguon kinh phi (gia tri: 1,2,3)
            //YYYYY: Ma CSKCB cua benh vien hien hanh
            //ZZZZZZ: Serie cua may ( co the co nhieu so serie cach nhau bang dau ';')
            var maMays = MA_MAY.Split('.');
            //if (maMays.Length != 4) return false;
            if (maMays.Count() < 4) return false;
            

            //code XX: ma nhom may (vd: HH, VS, SA, MRI,...)
            if (maMays[0] == "" || maMays[0].Contains(" ") || maMays[0].Length > 3) return false;

            //code n: nguon kinh phi (1: ngan sach, 2: xa hoi hoa, 3: khac)
            if (maMays[1].Length != 1 || !"1234".Contains(maMays[1])) return false;

            //code YYYYY: ma cskcb
            if (!Uti.CodeMatch(maMays[2],MA_CSKCB)) return false;

            //code ZZZZZZ: so serie may (6 chu so)
            if (maMays[3].IndexOf(";") != -1)
            {
                if (maMays[3].Contains(" "))
                {
                    return false;
                }
            }
            else
            {
                string[] seriesMays = maMays[3].Split(';');
                foreach (string s in seriesMays)
                {
                    if (s.Contains(" "))
                    {
                        return false;
                    }
                }
                
            }

            return true;
        }
        public bool checkMaGiuong(string magiuong)
        {
            if (string.IsNullOrEmpty(magiuong)) return false;
            if (magiuong.Trim().Length > 4) return false;
            if (magiuong.Trim().Length < 4) return false;
            string MG = magiuong.Length>1? magiuong.Substring(0,1): magiuong;
            string sogiuong = magiuong.Length > 3 ? magiuong.Substring(1, 3) : magiuong;
            if (sogiuong == "000") return false;
            bool isNumber = int.TryParse(sogiuong, out int result);
            if (!isNumber) return false;
            if (MG != "H" && MG != "T" && MG != "C" && MG != "K") return false;
            return true;

        }
        public string cut_TT_VT(string TT_THAU)
        {
            if(string.IsNullOrEmpty(TT_THAU)) return "";
            string[] mts = TT_THAU.Split(';');
            if (mts.Count() == 4) return TT_THAU;
            if (mts.Count() > 4)
            {
                return (TT_THAU = $"{mts[0]};{mts[1]};{mts[2]};{mts[3]}");
            }
            return TT_THAU;
        }

        public bool Check_TT_THAU(string TT_THAU)
        {
            string[] mts = TT_THAU.Split(';');
            if (mts.Length == 4) return true;
            if (mts.Length == 5) return true;
            return false;
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
        private  bool TryParseTime(string input, out TimeSpan result)
        {
            return TimeSpan.TryParseExact(input, new[] { "hhmm", "hmm", "h\\:mm", "hh\\:mm" }, null, out result);
        }

        //Linhist added 16/07/26
        public static bool CoHieuLuc(string tuNgay, DateTime? date)
        {
            if (string.IsNullOrEmpty(tuNgay) || tuNgay.Length != 8 || date == null) return true;

            try
            {
                var yy = tuNgay.Substring(0, 4);
                var mm = tuNgay.Substring(4, 2);
                var dd = tuNgay.Substring(6, 2);
                return DateTime.Parse($"{yy}/{mm}/{dd}") <= date.Value;
            }
            catch { }

            return true;
        }

        public async Task<bool> Validate(XML03 xml, XMLCommonInfo xmlCommon)
        {
            Info = xmlCommon;
            return await Validate(xml);
        }

        public override async Task<bool> Validate(XML03 xml)
        {
            //Stopwatch _watch = Stopwatch.StartNew();
            //Stopwatch _wa = Stopwatch.StartNew();

            //Kiem tra neu XML null thi ko chay tiep nua
            if (!await base.Validate(xml)) return false;

            //Linhist added 011026 - bệnh nhân không thanh toán BHYT thì không check lỗi
            if (File.PHAM_VI == 2) return true;

            //Khởi tạo Data check QĐ 5937
            List<QD5937> qD5937s = new List<QD5937>
            {
                new QD5937{ MA_DV="03.0708.0253", Value="Tần số;Cường độ;Chế độ;Vùng chỉ định"},
                new QD5937{ MA_DV="17.0011.0237", Value="Công suất;Khoảng cách;Vùng chỉ định"},
                new QD5937{ MA_DV="03.0774.0237", Value="Công suất;Khoảng cách;Vùng chỉ định"},
                new QD5937{ MA_DV="17.0026.0220", Value="Vùng kéo;Lực kéo;Lực giữ;Chế độ;Thời gian:thời gian giữ giây/thời gian nghỉ giây"},
                new QD5937{ MA_DV="03.0773.0234", Value="Dòng;Cường độ;Vùng đặt điện cực"},
                new QD5937{ MA_DV="17.0007.0234", Value="Dòng;Cường độ;Vùng đặt điện cực"},
                new QD5937{ MA_DV="1701600245", Value="Dòng;Cường độ;Vùng đặt điện cực"}
            };
            //Khởi tạo data cho DV làm trùng nhau
            List<DVTrung> dvTrung =new List<DVTrung>
            {
                new DVTrung{ MA_DV1="03.0774.0237", MA_DV2="03.0773.0234"},
                new DVTrung{ MA_DV1="17.0011.0237", MA_DV2="17.0007.0234"},
                new DVTrung{ MA_DV1="01.0158.0074", MA_DV2="01.0032.0299"},
                new DVTrung{ MA_DV1="01.0182.0118", MA_DV2="01.0055.0114"},
                new DVTrung{ MA_DV1="01.0128.0209", MA_DV2="01.0055.0114"},
                new DVTrung{ MA_DV1="01.0132.0209", MA_DV2="01.0055.0114"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="03.0091.0300"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="13.0192.0103"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="03.2191.0898"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="02.0150.0114"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="03.2191.0898"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="13.0192.0103"},
                new DVTrung{ MA_DV1="03.0082.0209", MA_DV2="02.0150.0114"},
            };
            //Khởi tạo data cho TT04
            List<TT042017> dataTT04 = new List<TT042017> 
            { 
                new TT042017 { ID = "N06.02.010", Value = 20000000 }, 
                new TT042017 { ID = "N06.02.020", Value = 36000000 }, 
                new TT042017 { ID = "N06.02.030", Value = 20000000 }, 
                new TT042017 { ID = "N06.02.040", Value = 20000000 }, 
                new TT042017 { ID = "N06.02.050", Value = 30000000 }, 
                new TT042017 { ID = "N06.02.060", Value = 260000000 }, 
                new TT042017 { ID = "N06.02.070", Value = 280000000 }, 
                new TT042017 { ID = "N06.02.080", Value = 220000000 }, 
                new TT042017 { ID = "N06.02.090", Value = 70000000 }, 
                new TT042017 { ID = "N06.03.010", Value = 3000000 }, 
                new TT042017 { ID = "N06.04.051", Value = 45000000 }, 
                new TT042017 { ID = "N06.04.052", Value = 35000000 }, 
                new TT042017 { ID = "N06.04.053", Value = 45000000 }, 
                new TT042017 { ID = "N06.04.054", Value = 35000000 }, 
                new TT042017 { ID = "N07.01.401", Value = 42000000 }, 
                new TT042017 { ID = "N07.01.402", Value = 70000000 }, 
                new TT042017 { ID = "N07.01.403", Value = 260000000 } };
            //Uti.HandleCronJobLog(_logger, $"[130] Bat dau giam dinh ho so XML03", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML03)}");

            //Xác định đây là dịch vụ hay vật tư
            //Code doan nay co ve khong chay
            //isDichVu = Field(x => x.MA_NHOM).NotValue(10, 11).Result();
            if (Field(x => x.MA_NHOM).HasValue(10, 11))
            {
                isDichVu = false;
            }
            else isDichVu = true;

            if (isDichVu)
            {
                Validate_DICHVU(xml);
            }
            else
            {
                Validate_VATTU(xml);
            }

            errorlist = dmError130
                .Where(x => x.ErrorCode.HasValue) // Filter out entries where ErrorCode is null
                .ToDictionary(x => x.ErrorCode.Value, x => x.Message); // Use Value to access the non-nullable int
            //errorlist = dmError130.ToDictionary(x => x.ErrorCode, x => x.Message);
            benhchinhInfo = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH)?.TEN ?? "Không có thông tin bệnh";

            #region Kiểm tra các lỗi cơ bản
            //109	XML3	MA_LK	Mã liên kết không được để trống
            //110	XML3	MA_LK	Mã liên kết không được lớn hơn 100 ký tự
            Field(x => x.MA_LK).IsEmpty().ThenError(109).LengthOver(100).ThenError(110);

            //111	XML3	STT	Số thứ tự không được để trống và phải có giá trị từ 0 đến 999999999
            Field(x => x.STT).IsEmpty().IsOver(999999999).ThenError(111);

            //122	XML3	DON_VI_TINH	Đơn vị tính không được để trống
            //187	XML3	DON_VI_TINH	Đơn vị tính không được lớn hơn 50 ký tự
            Field(x => x.DON_VI_TINH).IsEmpty().ThenError(122).LengthOver(50).ThenError(187);

            //159	XML3	PHAM_VI	Phạm vi không được để trống và phải có giá trị từ 1 đến 3
            Field(x => x.PHAM_VI).IsEmpty().NotBetweenOrEqual(1, 3).ThenError(159);

            //123	XML3	SO_LUONG	Số lượng không được để trống  và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.SO_LUONG).IsEmpty().IsUnder(0).ThenError(123);

            //125	XML3	DON_GIA_BV	Đơn giá BV không được để trống  và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.DON_GIA_BV).IsEmpty().IsUnder(0).ThenError(125);

            //126	XML3	DON_GIA_BH	Đơn giá BH không được để trống  và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.DON_GIA_BH).IsEmpty().IsUnder(0).ThenError(126);

            //127	XML3	TYLE_TT_DV	Tỷ lệ thanh toán DV không được để trống và phải có giá trị từ 0 đến 100
            Field(x => x.TYLE_TT_DV).IsEmpty().NotBetweenOrEqual(0, 100).ThenError(127);

            //128	XML3	TYLE_TT_BH	Tỷ lệ thanh toán BH không được để trống và phải có giá trị từ 0 đến 100
            //*** Code nhập bằng 0 vẫn báo lỗi
            Field(x => x.TYLE_TT_BH).IsEmpty().NotBetweenOrEqual(0, 100).ThenError(128);

            //129	XML3	THANH_TIEN_BV	Thành tiền BV không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.THANH_TIEN_BV).IsEmpty().IsUnder(0).ThenError(129);

            //130	XML3	THANH_TIEN_BH	Thành tiền BH không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.THANH_TIEN_BH).IsEmpty().IsUnder(0).ThenError(130);

            //162	XML3	MUC_HUONG	Mức hưởng không được để trống và phải có giá trị từ 0 đến 100
            Field(x => x.MUC_HUONG).IsEmpty().NotBetweenOrEqual(0, 100).ThenError(162);

            //1816	XML3	T_NGUONKHAC_NSNN	Tiền chi trả từ NSNN không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_NGUONKHAC_NSNN).IsEmpty().IsUnder(0).ThenError(1816);

            //1817	XML3	T_NGUONKHAC_VTNN	Tiền chi trả từ nguồn nước ngoài không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_NGUONKHAC_VTNN).IsEmpty().IsUnder(0).ThenError(1817);

            //1818	XML3	T_NGUONKHAC_VTTN	Tiền chi trả từ nguồn trong nước không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_NGUONKHAC_VTTN).IsEmpty().IsUnder(0).ThenError(1818);

            //1819	XML3	T_NGUONKHAC_CL	Tiền chi trả từ nguồn khác không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_NGUONKHAC_CL).IsEmpty().IsUnder(0).ThenError(1819);

            //1820	XML3	T_NGUONKHAC	Tổng tiền chi trả nguồn khác không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_NGUONKHAC).IsEmpty().IsUnder(0).ThenError(1820);

            //1821	XML3	T_BNTT	Tiền bệnh nhân thanh toán không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_BNTT).IsEmpty().IsUnder(0).ThenError(1821);

            //1822	XML3	T_BNCCT	Tiền bệnh nhân cùng chi trả không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_BNCCT).IsEmpty().IsUnder(0).ThenError(1822);

            //1823	XML3	T_BHTT	Tiền bảo hiểm thanh toán không được để trống và phải là số thập phân lớn hơn hoặc bằng 0 và làm tròn đến 2 đơn vị thập phân (phân cách bởi dấu '.')
            Field(x => x.T_BHTT).IsEmpty().IsUnder(0).ThenError(1823);

            //131	XML3	MA_KHOA	Mã khoa không được để trống
            //132	XML3	MA_KHOA	Mã khoa không được lớn hơn 20 ký tự
            //Linhist changed 250926
            //Field(x => x.MA_KHOA).IsEmpty().ThenError(131).LengthOver(20).ThenError(132);
            Field(x => x.MA_KHOA).IsEmpty().ThenError(131).LengthOver(50).ThenError(132);
            //4003	3	XML3	MA_KHOA	[3176] Mã khoa điều trị tại XML3, phải được liệt kê đẩy đủ trong Mã khoa tại XML1
            if (!Uti.CodeMatch(File.MA_KHOA, File.XML01.MA_KHOA))
            {
                RaiseError(true, 4003, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4004, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Mã khoa điều trị tại XML3:{File.MA_KHOA}, phải được liệt kê đẩy đủ trong Mã khoa tại XML1:{File.XML01.MA_KHOA}"
                        );
            }    

            //133	XML3	MA_BAC_SI	Mã bác sĩ không được để trống
            //184	XML3	MA_BAC_SI	Mã bác sĩ không được lớn hơn 255 ký tự
            Field(x => x.MA_BAC_SI).IsEmpty().ThenError(133).LengthOver(255).ThenError(184);

            //2079	4	XML3	NGUOI_THUC_HIEN	[130] Mã CCHN người thực hiện không được để trống
            //2082    4   XML3 NGUOI_THUC_HIEN[130] Người thực hiện có độ dài lớn hơn 255 ký tự
            //Field(x=>x.MA_NHOM).HasValue(1,2,3,8,18).Field(x => x.NGUOI_THUC_HIEN).IsEmpty().ThenError(2079).LengthOver(255).ThenError(2082);
            //if(File.MA_NHOM.HasValue(1,2,3,8,10))
            //{
            //    Field(x => x.NGUOI_THUC_HIEN).IsEmpty().ThenError(2079).LengthOver(255).ThenError(2079);
            //}    
            if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 8, 18))
            {
                Field(x => x.NGUOI_THUC_HIEN).IsEmpty().ThenError(2079).LengthOver(255).ThenError(2082);
            }
            //134	XML3	MA_BENH	Mã bệnh không được để trống
            //186	XML3	MA_BENH	Mã bệnh không được lớn hơn 255 ký tự
            if (File.MA_NHOM != 10)
            {
                //Linhist changed 25/03/26
                //if (String.IsNullOrEmpty(File.MA_BENH))
                //{
                //    RaiseError(true, 134);
                //}
                //else if (File.MA_BENH.Length > 255)
                //{
                //    RaiseError(true, 186);
                //}
                //Linhist changed 250926
                //Field(x => x.MA_BENH).IsEmpty().ThenError(134).LengthOver(255).ThenError(186);
                Field(x => x.MA_BENH).IsEmpty().ThenError(134).LengthOver(100).ThenError(186);
            }

            //135	XML3	NGAY_YL	Ngày y lệnh không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
            //Linhist changed 170926
            //if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
            if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 12))
            {
                Field(x => x.NGAY_YL).IsEmpty().ThenError(135);
            }

            ////136	XML3	NGAY_KQ	Ngày kết quả không được để trống và phải đúng định dạng ngày tháng yyyymmddHHMM
            ///Ngày KQ ngày giường không được để trống.=> them cong kham
            if (Field(x => x.MA_NHOM).HasValue(14, 15, 16, 13))
            {
                Field(x => x.NGAY_KQ).IsEmpty().ThenError(136);
            }

            //Linhist added 110826
            //4214	1	XML3	NGAY_KQ	Ngày kết quả của vật tư không được để trống 
            if (Field(x => x.MA_NHOM).HasValue(10, 11))
            {
                Field(x => x.NGAY_KQ).IsEmpty().ThenError(4214);
            }

            //137	XML3	MA_PTTT	Mã PTTT không được để trống và phải là số nguyên có giá trị từ 0 đến 3
            Field(x => x.MA_PTTT).IsEmpty().NotBetweenOrEqual(0, 3).ThenError(137);
            //112	XML3	MA_NHOM	Mã nhóm không được để trống
            //113	XML3	MA_NHOM	Mã nhóm phải là số nguyên có giá trị khác với 4, 5, 6, 7
            Field(x => x.MA_NHOM).IsEmpty().ThenError(112).IsUnder(0).BetweenOrEqual(4, 7).ThenError(113);

            //2072	4	XML3	NGAY_TH_YL	[130] Ngày thực hiện y lệnh không được để trống với các mã nhóm 1,2,3,8,18 
            //Linhist changed 170926
            //if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 8, 18))
            if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 8, 12, 18))
            {
                //if (File.MA_DICH_VU.IndexOf("K") == -1)
                //{
                Field(x => x.NGAY_TH_YL).IsEmpty().ThenError(2072);
                //}
            }

            //2075    4   XML3 TT_THAU[130] Thông tin thầu phải nhập khi mã nhóm bằng 10
            if (Field(x => x.MA_NHOM).HasValue(10, 11))
            {
                Field(x => x.TT_THAU).IsEmpty().ThenError(2075);

            }
            //2074	4	XML3	MA_XANG_DAU	[130] Mã Xăng dầu bắt buộc phải nhập khi mã nhóm là 12
            if (Field(x => x.MA_NHOM).HasValue(12))
            {
                Field(x => x.MA_XANG_DAU).IsEmpty().ThenError(2074);
            }
            //2187	4	XML3	VET_THUONG_TP	[130] Vết thương tái phát nếu có chỉ nhập giá trị bằng 1
            if (Field(x => x.VET_THUONG_TP).NotEmpty())
            {
                Field(x => x.VET_THUONG_TP).NotValue(1).ThenError(2187);
            }

            // Kiểm tra đây là XML03 Vật tư (MA_NHOM = 10, 11)
            if (Field(x => x.MA_NHOM).HasValue(10, 11))
            {
                //116	XML3	MA_VAT_TU	Mã vật tư không được để trống
                //117	XML3	MA_VAT_TU	Mã vật tư không được lớn hơn 255 ký tự
                Field(x => x.MA_VAT_TU).IsEmpty().ThenError(116).LengthOver(255).ThenError(117);

                //120	XML3	TEN_VAT_TU	Tên vật tư không được để trống
                //121	XML3	TEN_VAT_TU	Tên vật tư không được lớn hơn 255 ký tự
                //Linhist changed 250926
                //Field(x => x.TEN_VAT_TU).IsEmpty().ThenError(120).LengthOver(255).ThenError(121);
                Field(x => x.TEN_VAT_TU).IsEmpty().ThenError(120).LengthOver(1024).ThenError(121);
            }
            // Kiểm tra đây là XML03 Dịch vụ (MA_NHOM khác 10, 11)
            else
            {
                //114	XML3	MA_DICH_VU	Mã dịch vụ không được để trống
                //115	XML3	MA_DICH_VU	Mã dịch vụ không được lớn hơn 255 ký tự
                //Linhist changed 250926
                //Field(x => x.MA_DICH_VU).IsEmpty().ThenError(114).LengthOver(255).ThenError(115);
                Field(x => x.MA_DICH_VU).IsEmpty().ThenError(114).LengthOver(50).ThenError(115);

                //118	XML3	TEN_DICH_VU	Tên dịch vụ không được để trống
                //119	XML3	TEN_DICH_VU	Tên dịch vụ không được lớn hơn 255 ký tự
                //Linhist changed 250926
                //Field(x => x.TEN_DICH_VU).IsEmpty().ThenError(118).LengthOver(255).ThenError(119);
                Field(x => x.TEN_DICH_VU).IsEmpty().ThenError(118).LengthOver(1024).ThenError(119);

                //Linhist added 16/07/26
                //204 1   XML3 MA_DICH_VU  Mã dịch vụ không có hiệu lực trong danh mục phê duyệt
                if (File.MA_NHOM != 12 && !dmDICHVU.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && CoHieuLuc(x.TU_NGAY, File.NGAY_YL)))
                {
                    RaiseError(true
                            , 204
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(204, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Không tìm thấy mã dịch vụ hoặc mã dịch vụ có hiệu lực từ ngày lớn hơn ngày y lệnh."
                        );
                }
            }
            //2892	3	XML3	SO_LUONG	[3176] Tuyên Quang: Dịch vụ nhóm 18 số lượng không được lớn hơn 1
            if (Field(x => x.MA_NHOM).HasValue(18))
            {
                RaiseError(File.SO_LUONG > 1
                            , 2892
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2892, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm: {File.MA_NHOM} số lượng không được lớn hơn 1."
                );
            }
            #endregion

            try
            {
                #region Chuyên đề bảo hiểm
                //4098	3	XML3	MA_MAY	[3176] Nội dung mã máy không khớp với thông tin dịch vụ kỹ thuật thực hiện, đề nghị kiểm tra lại mã máy
                //Linhist changed 16/07/26
                //if(string.IsNullOrEmpty(File.MA_MAY) && !string.IsNullOrEmpty(File.MA_DICH_VU))
                //{
                //    var thongtin = dmChung130.Where(x=>x.LOAI==File.MA_CSKCB && x.MA==File.MA_DICH_VU).FirstOrDefault();
                //    if (thongtin != null && File.MA_MAY != thongtin.TEN)
                //    {
                //        RaiseError(true, 4098, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                    $"Nội dung lỗi: {(errorlist.TryGetValue(4098, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                    $"Chi tiết:" +
                //                    $"Mã máy tại XML3: {File.MA_MAY} không khớp với thông tin dịch vụ kỹ thuật thực hiện: {thongtin.TEN}, đề nghị kiểm tra lại mã máy."
                //        );
                //    }
                //}
                if (!string.IsNullOrEmpty(File.MA_DICH_VU))
                {
                    var thongtin = _dvuMamays.FirstOrDefault(x => Uti.MaMayContain(File.MA_DICH_VU, x.MA_DICH_VU));
                    if (thongtin != null && (string.IsNullOrEmpty(thongtin.MA_MAY?.Trim()) ? !string.IsNullOrEmpty(File.MA_MAY?.Trim()) : !Uti.MaMayContain(File.MA_MAY, thongtin.MA_MAY)))
                    {
                        RaiseError(true, 4098, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4098, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Mã máy tại XML3: {File.MA_MAY} không khớp với thông tin dịch vụ kỹ thuật thực hiện: {thongtin.MA_MAY}, đề nghị kiểm tra lại mã máy."
                        );
                    }
                }

                //3795	3	XML3	NGAY_KQ	[3176] Các dịch vụ ngày giường có thời gian KQ - Thời gian TH >24h, đề nghị xem lại
                ////3823	3	XML3	MA_DICH_VU	Thái Bình: Vật tư này bắt buộc phải đình kèm dịch vụ (Bắt buộc nhập mã Dịch vụ tại XML3)
                ///


                if (File.MA_NHOM == 10 || File.MA_NHOM == 11)
                {
                    if (!string.IsNullOrEmpty(File.MA_VAT_TU.ToString()) && "N07.02.060;N02.04.030.0466.109.0001;N02.04.050;N03.03.010;N03.03.010.0079.292.0001;N03.03.010.2463.279.0001;N03.03.010.3782.279.0001;N03.03.090.2238.272.0002;N03.03.090.2238.272.0012;N03.03.130;N03.03.130.2061.125.0001;N03.03.130.3626.174.0003;N03.03.130.3626.174.0007;N04.01.010.3047.213.0001;N04.01.010.3047.213.0009;N04.01.010.4321.175.0051;N04.01.010.4321.175.0056;N04.01.090;N04.01.090.2413.279.0017;N04.01.090.4389.279.0004;N04.03.020;N04.03.020.0547.242.0001;N04.03.090;N04.04.010;N04.04.010.0280.232.0009;N04.04.010.0280.232.0014;N04.04.010.1507.000.0007;N04.04.010.2238.272.0002;N04.04.010.4263.175.0005;N04.04.010.4263.175.0008;N04.04.020;N04.04.020.0585.175.0001;N04.04.020.0585.175.0007;N04.04.020.3047.213.0001;N04.04.020.3061.175.0001;N04.04.030;N04.04.030.0272.271.0014;N04.04.030.0272.271.0018;N04.04.030.0280.232.0002;N04.04.030.0280.232.0003;N04.04.030.0280.232.0004;N04.04.030.0587.183.0002;N04.04.030.3492.232.0001;N04.04.030.4191.232.0019;N05.03.060;N05.03.060.1685.213.0002;N05.03.060.3606.155.0006;N05.03.060.5609.279.0001;N05.03.060.5609.279.0002;N05.03.060.6415.279.0001;N05.03.090;N05.03.090.1971.175.0001;N05.03.090.4733.279.0002;N06.01.010.0093.118.0007;N06.01.010.0093.118.0016;N06.01.010.0093.118.0018;N06.01.010.5320.252.0001;N06.01.030;N06.01.030.3045.175.0002;N06.01.030.3045.175.0003;N06.01.030.5745.175.0002;N06.01.040;N06.02.020;N06.02.020.0052.183.0004;N06.02.020.0052.183.0009;N06.02.020.0342.155.0001;N06.02.020.0549.274.0002;N06.02.020.0587.183.0014;N06.02.020.0587.183.0015;N06.02.020.0587.183.0018;N06.02.020.0641.155.0001;N06.02.020.0972.000.0004;N06.02.020.0972.000.0005;N06.02.020.0972.000.0006;N06.02.020.3046.183.0005;N06.02.020.3202.115.0001;N06.02.020.3377.173.0002;N06.02.020.4263.175.0001;N06.02.020.4263.175.0006;N06.02.020.5153.183.0001;N06.02.020.5153.183.0002;N06.02.020.5171.131.0001;N06.02.040;N06.02.040.0585.175.0001;N06.02.040.2043.175.0005;N06.02.050;N06.02.060.2665.155.0008;N06.02.070.2665.155.0005;N06.02.100;N06.02.100.0972.000.0001;N06.03.010;N06.03.010.2060.107.0005;N06.03.010.2457.155.0011;N06.03.010.2978.177.0002;N06.03.010.3490.125.0010;N06.03.010.4957.115.0007;N06.04.020;N06.04.020.3493.175.0010;N06.04.020.4911.115.0001;N06.04.020.4997.177.0011;N06.04.051;N06.04.051.0544.240.0019;N06.04.051.2513.155.0020;N06.04.051.2513.155.0024;N06.04.051.3857.183.0032;N06.04.051.3857.183.0048;N06.04.051.3857.183.0051;N06.04.052;N06.04.052.2513.155.0013;N06.04.052.3115.175.0002;N06.04.052.3115.175.0015;N06.04.052.3857.183.0005;N06.04.052.3857.183.0011;N06.04.053;N06.04.053.3461.155.0016;N06.05.020;N06.05.020.3881.125.0004;N06.05.030.3403.257.0002;N06.05.040;N07.01.060.0093.118.0001;N07.01.100;N07.01.100.2685.232.0001;N07.01.100.3061.175.0001;N07.01.110;N07.01.110.1507.000.0006;N07.01.110.2941.175.0002;N07.01.110.3024.155.0005;N07.01.110.3024.155.0007;N07.01.110.4263.175.0004;N07.01.110.4263.175.0005;N07.01.190.0048.175.0014;N07.01.200;N07.01.210;N07.01.220;N07.01.230.0972.000.0005;N07.01.230.0972.000.0009;N07.01.230.0972.000.0013;N07.01.230.3024.155.0005;N07.01.240;N07.01.240.0052.146.0006;N07.01.240.0070.274.0008;N07.01.240.0269.240.0004;N07.01.240.0269.240.0010;N07.01.240.0549.274.0006;N07.01.240.0549.274.0007;N07.01.240.0549.274.0012;N07.01.240.0585.175.0024;N07.01.240.0587.183.0008;N07.01.240.0641.155.0002;N07.01.240.0972.000.0002;N07.01.240.0972.000.0005;N07.01.240.0972.000.0007;N07.01.240.0972.000.0010;N07.01.240.2026.155.0004;N07.01.240.2377.155.0001;N07.01.240.2685.232.0005;N07.01.240.2685.232.0006;N07.01.240.3046.183.0001;N07.01.240.3047.213.0002;N07.01.240.3047.213.0004;N07.01.240.3377.173.0004;N07.01.240.3377.173.0005;N07.01.240.4263.175.0003;N07.01.240.4263.175.0004;N07.01.250.3079.213.0002;N07.01.270;N07.01.270.0585.146.0013;N07.01.270.1122.000.0005;N07.01.270.1243.000.0005;N07.01.270.1507.000.0001;N07.01.270.1507.000.0002;N07.01.270.2104.232.0001;N07.01.270.4263.175.0009;N07.01.280.0048.175.0010;N07.01.280.0048.175.0014;N07.01.290.2043.175.0002;N07.01.310.2685.232.0001;N07.01.401.3056.257.0008;N07.01.401.3056.257.0010;N07.01.401.3937.175.0008;N07.01.401.3937.175.0010;N07.01.402;N07.01.402.0587.183.0001;N07.01.402.3056.257.0013;N07.01.402.3056.257.0017;N07.01.402.3937.175.0010;N07.01.402.3939.205.0011;N07.01.403.3937.175.0014;N07.01.404.3937.175.0012;N07.01.404.3937.175.0013;N07.01.430;N07.01.430.0587.183.0005;N07.01.440;N07.01.440.0479.107.0002;N07.01.440.0587.183.0002;N07.01.440.0587.183.0004;N07.01.460;N07.01.460.0272.271.0037;N07.01.460.0272.271.0038;N07.01.460.0280.232.0003;N07.01.460.0585.146.0006;N07.01.460.3492.232.0001;N07.01.460.4191.232.0002;N07.02.040;N07.02.040.2181.240.0002;N07.02.040.2181.240.0004;N07.02.060.2181.240.0001;N07.02.060.2181.240.0005;N07.02.060.4818.240.0001;N07.02.080;N07.02.080.0158.155.0006;N07.02.080.0445.292.0008;N07.02.080.0445.292.0009;N07.02.080.2136.155.0016;N07.04.040;N07.04.040.1712.175.0001;N07.04.040.5667.279.0002;N07.04.050.1712.175.0026;N07.04.070.3894.240.0082;N07.04.070.3894.240.0082;N07.05.020.2238.272.0002;N07.06.030;N07.06.040;N07.06.040;N07.06.040.0090.272.0009;N07.06.040.0324.272.0002;N07.06.040.0324.272.0007;N07.06.040.0324.272.0038;N07.06.040.0324.272.0060;N07.06.040.0324.272.0108;N07.06.040.0324.272.0112;N07.06.040.0324.272.0135;N07.06.040.0324.272.0136;N07.06.040.0324.272.0144;N07.06.040.0324.272.0153;N07.06.040.0324.272.0184;N07.06.040.0324.272.0310;N07.06.040.0461.118.0014;N07.06.040.0461.118.0031;N07.06.040.0461.118.0047;N07.06.040.0461.118.0049;N07.06.040.0461.118.0050;N07.06.040.0461.118.0052;N07.06.040.0461.118.0066;N07.06.040.0461.118.0086;N07.06.040.0461.118.0091;N07.06.040.0461.118.0115;N07.06.040.0461.118.0115;N07.06.040.0461.118.0117;N07.06.040.0461.118.0120;N07.06.040.0461.118.0127;N07.06.040.0461.118.0128;N07.06.040.0461.118.0133;N07.06.040.0461.118.0142;N07.06.040.0461.118.0143;N07.06.040.0461.118.0145;N07.06.040.0461.118.0146;N07.06.040.0714.279.0058;N07.06.040.1681.175.0012;N07.06.040.1681.175.0020;N07.06.040.1681.175.0022;N07.06.040.1681.175.0026;N07.06.040.1711.240.0005;N07.06.040.1711.240.0008;N07.06.040.1711.240.0014;N07.06.040.1861.155.0042;N07.06.040.1861.155.0062;N07.06.040.1861.155.0063;N07.06.040.2;N07.06.040.2264.174.0006;N07.06.040.2264.174.0009;N07.06.040.2613.174.0017;N07.06.040.2613.174.0019;N07.06.040.2613.174.0028;N07.06.040.2613.174.0042;N07.06.040.2613.174.0060;N07.06.040.2613.174.0060;N07.06.040.2613.174.0061;N07.06.040.2613.174.0061;N07.06.040.3055.175.0019;N07.06.040.3059.175.0017;N07.06.040.3059.175.0032;N07.06.040.3387.155.0033;N07.06.040.3493.175.0021;N07.06.040.3493.175.0040;N07.06.040.3493.175.0041;N07.06.040.3493.175.0042;N07.06.040.3493.175.0043;N07.06.040.3493.175.0044;N07.06.040.3493.175.0053;N07.06.040.3599.155.0009;N07.06.040.3599.155.0010;N07.06.040.3599.155.0026;N07.06.040.3599.155.0027;N07.06.040.3599.155.0033;N07.06.040.3599.155.0034;N07.06.040.3599.155.0036;N07.06.040.3599.155.0037;N07.06.040.3599.155.0063;N07.06.040.3599.155.0066;N07.06.040.3599.155.0072;N07.06.040.3599.155.0073;N07.06.040.3599.155.0076;N07.06.040.3599.155.0087;N07.06.040.3599.155.0094;N07.06.040.3599.155.0099;N07.06.040.3599.155.0126;N07.06.040.3599.155.0129;N07.06.040.3599.155.0187;N07.06.040.3639.175.0006;N07.06.040.3856.175.0009;N07.06.040.3856.175.0009;N07.06.040.3856.175.0010;N07.06.040.4132.240.0007;N07.06.040.4253.174.0031;N07.06.040.4834.174.0001;N07.06.040.4834.174.0005;N07.06.040.4834.174.0032;N07.06.040.4911.115.0014;N07.06.040.4997.177.0155;N07.06.040.4997.177.0249;N07.06.040.4997.177.0251;N07.06.040.4997.177.0256;N07.06.040.4997.177.0291;N07.06.040.4997.177.0305;N07.06.040.5083.234.0201;N07.06.040.5083.234.0243;N07.06.040.5083.234.0283;N07.06.040.5083.234.0293;N07.06.040.5083.234.0310;N07.06.040.5083.234.0350;N07.06.040.5083.234.0353;N07.06.040.5083.234.0356;N07.06.040.5083.234.0359;N07.06.040.5083.234.0366;N07.06.040.5083.234.0367;N07.06.040.5083.234.0368;N07.06.040.5083.234.0369;N07.06.040.5083.234.0373;N07.06.040.5083.234.0374;N07.06.040.5083.234.0376;N07.06.040.5083.234.0379;N07.06.040.5083.234.0385;N07.06.040.5083.234.0419;N07.06.040.5083.234.0424;N07.06.040.5083.234.0497;N07.06.040.5083.234.0540;N07.06.040.5083.234.0551;N07.06.040.5083.234.0586;N07.06.040.5091.155.0003;N07.06.040.5091.155.0004;N07.06.040.5157.115.0030;N07.06.040.5157.115.0030;N07.06.040.5157.115.0094;N07.06.040.5157.115.0112;N07.06.040.5957.279.0012;N07.06.040.5957.279.0039;N07.06.040.6240.234.0001;N07.06.040.6240.234.0004;N07.06.040.6240.234.0006;N07.06.040.6240.234.0007;N07.06.040.6240.234.0008;N07.06.040.6240.234.0021;N07.06.040.6240.234.0022;N07.06.040.6240.234.0026;N07.06.040.6240.234.0028;N07.06.040.6240.234.0050;N07.06.040.6240.234.0077;N07.06.040.6240.234.0079;N07.06.050.4829.234.0002;N07.06.080.6193.155.0001;N07.06.080.6193.155.0002;N08.00.010;N08.00.010.1712.175.0030;N08.00.010.1712.175.0073;N08.00.010.1712.175.0078;N08.00.010.1712.175.0079;N08.00.010.1712.175.0081;N08.00.080;N08.00.150.4607.240.0008;N08.00.260;N08.00.330;N08.00.330.0324.272.0002;N08.00.330.1681.175.0002;N08.00.360".Contains(File.MA_VAT_TU))
                    {
                        if (string.IsNullOrEmpty(File.MA_DICH_VU))
                        {
                            RaiseError(true,
                                3823,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3823, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân có vật tư y tế {File.MA_VAT_TU}, nhưng không có mã dịch vụ kèm theo, đề nghị xem lại"
                                );
                        }
                    }
                }
                //469	1	XML3	MA_BAC_SI	Chỉ bác sĩ mới được thực hiện Khám chữa bệnh và ra chỉ định
                if (File.MA_NHOM == 13 || File.MA_NHOM == 8 || File.MA_NHOM == 18)
                {
                    if (!string.IsNullOrEmpty(File.MA_BAC_SI))
                    {
                        foreach(var s in File.MA_BAC_SI.Split(";"))
                        {
                            var basi = dmBACSI.Where(x => x.MA_CCHN == s).FirstOrDefault();
                            if (basi != null)
                            {
                                if (basi.CHUC_DANH_NN != "1")
                                {
                                    RaiseError(true,
                                        469,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(469, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bác sĩ có CCHN:{s}, không phải là bác sĩ, cần phải kiểm tra lại"
                                        );
                                }
                            }
                        }
                        //if (!dmBACSI.Any(x => x.MACCHN == File.MA_BAC_SI && x.CHUCDANH_NN == "1"))
                        //{
                        //    RaiseError(true,
                        //        469,
                        //        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //        $"Nội dung lỗi: {(errorlist.TryGetValue(469, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //        $"Chi tiết:" +
                        //        $"Nhaan vieen {File.MA_BAC_SI}, không phải là bác sĩ, cần phải kiểm tra lại"
                        //        );
                        //}
                    }
                }

                #region Linhist changed 23/03/26
                //3840	3	XML3	 NGUOI_THUC_HIEN	[3176] TT 49-2018 BYT: Chỉ bác sĩ chuyên khoa xét nghiệm hoặc kỹ thuật viên có trình độ đại học mới được thực hiện các dịch vụ xét nghiệm
                //if (File.MA_NHOM == 1)
                //{
                //    if(!string.IsNullOrEmpty(File.NGUOI_THUC_HIEN))
                //    {
                //        foreach(var s in File.NGUOI_THUC_HIEN.Split(";"))
                //        {
                //            var basi = dmBACSI.Where(x => x.MACCHN == s).FirstOrDefault();
                //            if (basi != null)
                //            {
                //                //Linhist changed 06/03/26 - (yc Phuong) - bổ sung 501;502;503;504 thành 122;123;124;125;501;502;503;504
                //                if ((basi.CHUCDANH_NN != "1" || basi.CHUCDANH_NN != "5") || (basi.CHUCDANH_NN == "5" && !string.IsNullOrEmpty(basi.PHAMVI_CM) && !"122;123;124;125;501;502;503;504".Contains(basi.PHAMVI_CM)))
                //                {
                //                    RaiseError(true,
                //                        3840,
                //                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                    $"Nội dung lỗi: {(errorlist.TryGetValue(3840, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                    $"Chi tiết:" +
                //                    $"Bác sĩ:{s} không được phép thực hiện các dịch vụ xét nghiệm, kiểm tra lại."
                //                    );
                //                }
                //            }
                //        }    
                //    }    
                //}
                if (File.MA_NHOM == 1 && !string.IsNullOrEmpty(File.NGUOI_THUC_HIEN) && !Uti.HCodeMach(File.MA_DICH_VU, "22.0502.1267;22.0502.1268;01.0281.1510"))
                {

                    if (!dmBACSI.Any(x => File.NGUOI_THUC_HIEN.Contains(x.MA_CCHN) && Uti.CodeMatch(x.PHAMVI_CM, "122;123;124;125;501;502;503;504")))
                    {
                       
                        RaiseError(true,
                                3840,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3840, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Không có người thực hiện nào được phép thực hiện các dịch vụ xét nghiệm, kiểm tra lại."
                            );

                    }
                }
                #endregion

                if (Uti.CodeContain(File.MA_NHOM.ToString(), "14;15;16"))
                {
                    if(File.NGAY_KQ !=null && File.NGAY_TH_YL !=null)
                    {
                        if (File.NGAY_KQ.Value-File.NGAY_TH_YL.Value >TimeSpan.FromHours(24))
                        {
                            RaiseError(true,
                                3795,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3795, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM}, có thời gian Thực hiện y lệnh:{File.NGAY_TH_YL.Value.ToString("dd/MM/yyyy HH:mm")}, thời gian Kết quả:{File.NGAY_KQ.Value.ToString("dd/MM/yyyy HH:mm")}, thời gian kê:{(File.NGAY_KQ.Value-File.NGAY_TH_YL.Value).TotalHours} quá 24h, đề nghị xem lại"
                                );
                        }    
                    }    
                }    


                //3777    4   XML3 NGUOI_THUC_HIEN[3716] Bác sĩ thực hiện không đúng phạm vi chuyên môn và phân công công việc, căn cứ TT32 - PL5
                if (File.NGUOI_THUC_HIEN != null && File.MA_DICH_VU.Length >= 7 && File.TEN_DICH_VU != null && ( File.MA_NHOM!=14 && File.MA_NHOM!=15 && File.MA_NHOM!=16)) 
                {
                    foreach (var bs in File.NGUOI_THUC_HIEN.Split(";"))
                    {
                        
                        var bsinfor = dmBACSI.Where(x => x.MA_CSKCB == File.MA_CSKCB && x.MA_CCHN == bs).FirstOrDefault();
                        if (bsinfor != null)
                        {
                            if (bsinfor.PHAMVI_CM == "129" || bsinfor.PHAMVI_CM == "132")
                            {
                                //kiểm tra xem có dịch vụ khác hay không? nếu có dv khác

                                if(string.IsNullOrEmpty(bsinfor.DVKT_KHAC))
                                {
                                   
                                    if (!dmChung130.Any(x => x.LOAI == "TT32-PL5" && (x.MA == File.MA_DICH_VU.Substring(0, 7)) || x.TEN.Contains(File.TEN_DICH_VU)))
                                    {
                                        RaiseError(true,
                                          3777,
                                          $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                          $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                          $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                          $"Nội dung lỗi: {(errorlist.TryGetValue(3777, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                          $"Chi tiết:" +
                                          $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM} được thực hiện bởi BS:{bsinfor.MA_CCHN ?? ""}|{bsinfor.HO_TEN ?? ""} Không đúng với hướng dẫn tại Phục lục 5 TT32 ngày 32/12/2023 của BYT, đề nghị xem lại\n" +
                                          $"Dịch vụ khác của BS này đang để trống"
                                          );
                                    }
                                }    
                                if(!string.IsNullOrEmpty(bsinfor.DVKT_KHAC))
                                {
                                    var hasErr = true;
                                    foreach (var dv in bsinfor.DVKT_KHAC.Split(";"))
                                    {
                                                                              
                                            //Kiem tra co phai ma hoa theo Ma nhom theo PL01 7603
                                            var nhomDV = dmChung130.Where(x => x.LOAI == "DVKHAC" && x.MA == dv).Select(x => x.TEN).FirstOrDefault();
                                            if (!String.IsNullOrEmpty(nhomDV))
                                            {
                                                foreach (var dvkhac in nhomDV.Split(";"))
                                                {
                                                    if (File.MA_DICH_VU.Substring(0, 7) == dvkhac)
                                                    {
                                                        hasErr = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (File.MA_DICH_VU.Substring(0, 7) == dv)
                                                {
                                                    hasErr = false;
                                                }
                                            }
                                    }

                                    if (hasErr)
                                    {
                                        RaiseError(true,
                                              3777,
                                              $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                              $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                              $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                              $"Nội dung lỗi: {(errorlist.TryGetValue(3777, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                              $"Chi tiết:" +
                                              $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM} được thực hiện bởi BS:{bsinfor.MA_CCHN ?? ""}|{bsinfor.HO_TEN ?? ""} Không đúng với hướng dẫn tại Phục lục 5 TT32 ngày 32/12/2023 của BYT, đề nghị xem lại" 
                                              );
                                    }
                                }
                                
                            }
                        } 
                        
                    }    
                }


                //3727	3	XML3	NGAY_TH_YL	[3176] Thời điểm thực hiện y lệnh trùng với thời điểm Thực hiện Y lệnh cho Bệnh nhân khác (Nhóm 8 - 18)
                //3730    3   XML3 NGAY_TH_YL[3176] Trùng thời điểm Thực hiện Y Lệnh, Bệnh nhân có thời điểm thực hiện y lệnh trùng với Bệnh nhân khác

                var excludedCodes = new List<string> {
                "08.0022.0252", "01.0032.0299", "01.0066.1888", "01.0055.0114", "03.0091.0300", "03.0092.0299",
                "13.0192.0103", "03.2191.0898", "02.0150.0114", "02.0495.0196", "02.0209.0194", "02.0496.0195",
                "01.0132.0209", "08.0015.0252", "01.0131.0209", "01.0133.0209", "01.0175.0196", "03.0463.0230",
                "03.0464.0230", "03.0467.0230", "03.0478.0230", "03.0479.0230", "03.0483.0230", "03.0484.0230",
                "03.0497.0230", "03.0522.0230", "03.0527.0230", "03.0529.0230", "03.0530.0230", "17.0026.0220",
                "17.0007.0234", "17.0008.0253", "17.0001.0254", "08.0293.0230", "08.0301.0230", "08.0313.0230",
                "17.0011.0237", "17.0006.0231", "17.0004.0232"
                    };

                var includedCodes = "02.0094.0321;02.0111.1798;03.1245.1823;03.1652.0751;03.2126.0884;03.2381.0305;14.0224.0751;14.0241.0864;14.0245.0864;14.0246.0742;14.0247.0864;14.0248.0864;14.0249.0844;14.0250.0852;14.0251.0852;14.0252.0801;14.0253.0757;14.0254.0757;14.0255.0755;14.0256.0843;14.0257.0848;14.0258.0754;14.0259.0753;14.0262.0751;14.0263.0751;14.0264.0751;14.0265.0751;14.0266.0865;14.0267.0750;14.0268.0752;14.0269.0750;14.0270.0750;14.0271.0865;14.0272.0744;14.0273.0747;14.0274.0747;14.0275.0758;14.0276.0752;14.0277.0865;14.0278.0865;18.0064.0177;18.0134.0019;18.0626.0608;18.0627.0146;18.0628.0081;18.0636.0171;18.0637.0171;18.0638.0171;18.0639.0172;18.0640.0171;18.0641.0171;18.0644.0171;18.0645.0171;18.0646.0171;18.0648.0172;18.0650.0088;18.0651.0088;19.0378.1823;19.0379.1823;19.0380.1823;19.0381.1823;19.0382.1823;19.0383.1823;19.0384.1823;19.0385.1823;19.0386.1823;19.0387.1823;19.0388.1823;19.0389.1823;19.0390.1823;21.0060.0890;21.0062.0891;21.0064.0885;21.0065.0887;21.0066.0886;21.0068.0888;21.0079.0801;21.0082.0843;22.0126.0092;22.0127.0091;24.0225.2041"
                    .Split(';')
                    .Distinct()
                    .ToList();

                //Linhist added 060826
                if (_dmErrorMHTCs.Any(x => x.EMErrorCode == 3727))
                {
                    if (File.NGAY_TH_YL != null && File.NGAY_YL != null && File.NGAY_KQ != null)
                    {
                        if (Uti.HCodeContain(File.MA_NHOM.ToString(), "8;18"))
                        {


                            #region Trùng thời điểm thực hiện PT và TT (nhóm 8 và nhóm 18)
                            if (_dmErrorMHTCs.Any(x => x.EMErrorCode == 3727))
                            {
                                var xml3Lichsu = await _xml03Repository.GetItem(x =>
                                                        x.MA_CSKCB == File.MA_CSKCB
                                                        && x.MA_LK != File.MA_LK
                                                        && (x.MA_NHOM == 8 || x.MA_NHOM == 18 || includedCodes.Contains(x.MA_DICH_VU))
                                                        && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null
                                                        && !excludedCodes.Contains(x.MA_DICH_VU)
                                                        && x.NGUOI_THUC_HIEN == File.NGUOI_THUC_HIEN
                                                        && !(x.NGAY_KQ < File.NGAY_TH_YL || x.NGAY_TH_YL > File.NGAY_KQ)
                                                    );
                                if (xml3Lichsu != null)
                                {
                                    var x01h = await _hosoRepository.GetItem(x => x.XML01ID == xml3Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB);
                                    //XML01History bnKhac = .Select(x => new XML01History { XML01ID = x.XML01ID, HO_TEN = x.HO_TEN, MA_BN = x.MA_BN, NGAY_VAO = x.NGAY_VAO, NGAY_RA = x.NGAY_RA }).FirstOrDefault();
                                    if (x01h != null)
                                    {

                                        RaiseError(true,
                                            3727,
                                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(3727, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM} có thời điểm thực hiện y lệnh trùng với Bệnh nhân khác: Mã BN: {x01h.MA_BN}, Tên BN:{x01h.HO_TEN}, Thời gian vào Viện: {x01h.NGAY_VAO?.ToString("dd/MM/yyyy HH:mm")}, Thời gian ra Viện: {x01h.NGAY_RA?.ToString("dd/MM/yyyy HH:mm")} \n" +
                                            $"Thời điểm thực hiện y lệnh trùng: {File.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} Ngày KQ: {File.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Người thực hiện {File.NGUOI_THUC_HIEN}\n" +
                                            $"Thời điểm thực hiện BN khác trùng: {xml3Lichsu.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} Ngày KQ {xml3Lichsu.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Người thực hiện {xml3Lichsu.NGUOI_THUC_HIEN}\n" +
                                            $"Theo QĐ 3176 hoặc QĐ 4750, Các dịch vụ nhóm 8, 18 (Phẫu thuật và Thủ thuật), ghi thời điểm bắt đầu thực hiện và kết thúc thực hiện => Không thể thực hiện trùng khoảng thời gian giữa các đợt Phẫu thuât, thủ thuật trên 2 BN khác nhau"

                                            );
                                    }
                                }
                            }
                        }
                        //_wa.Stop();
                        //if (_wa.Elapsed.TotalSeconds > .3d)
                        //{
                        //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - XMLLichSu, MA_CSKCB {File.MA_CSKCB} MA LOI: 3727, MA LK {File.MA_LK}, XML03ID {File.XML03ID}  thời gian: {_wa.Elapsed.TotalSeconds}", nameof(Validate));
                        //}
                        //_wa.Restart();
                        #endregion
                        #region Đang phẫu thuật thủ thuật ra y lệnh cho BN khác
                        //3733	3	XML3	MA_DICH_VU	[3176] Bác sĩ phát sinh Y lệnh dịch vụ trong quá trình thực hiện Phẫu thuật và Thủ thuật
                        if (_dmErrorMHTCs.Any(x => x.EMErrorCode == 3733))
                        {
                            var xml3Lichsu = await _xml03Repository.GetItem(x =>
                                                x.MA_CSKCB == File.MA_CSKCB
                                                && x.MA_LK != File.MA_LK
                                                && (x.MA_NHOM == 1 || x.MA_NHOM == 2 || x.MA_NHOM == 3 || x.MA_NHOM==4 || x.MA_NHOM==5 || x.MA_NHOM==13 || x.MA_NHOM==14 || x.MA_NHOM==15)
                                                && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_KQ != null && x.NGAY_TH_YL != null
                                                && !excludedCodes.Contains(x.MA_DICH_VU)
                                                && x.NGUOI_THUC_HIEN.Contains(File.MA_BAC_SI.Trim())
                                                && x.NGAY_TH_YL <= File.NGAY_YL && x.NGAY_KQ >= File.NGAY_YL
                                            );
                            if (xml3Lichsu != null)
                            {
                                var x01h = await _hosoRepository.GetItem(x => x.XML01ID == xml3Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB);
                                //XML01History bnKhac = _hosoRepository.DbSet.AsNoTracking().Where(x => x.XML01ID == xml3Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB).Select(x => new XML01History { XML01ID = x.XML01ID, HO_TEN = x.HO_TEN, MA_BN = x.MA_BN, NGAY_VAO = x.NGAY_VAO, NGAY_RA = x.NGAY_RA }).FirstOrDefault();
                                if (x01h != null)
                                {

                                    RaiseError(true,
                                        3733,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3733, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM} có thời điểm thực hiện y lệnh trùng với Bệnh nhân khác: Mã BN: {x01h.MA_BN}, Tên BN:{x01h.HO_TEN}, Thời gian vào Viện: {x01h.NGAY_VAO?.ToString("dd/MM/yyyy HH:mm")}, Thời gian ra Viện: {x01h.NGAY_RA?.ToString("dd/MM/yyyy HH:mm")} \n" +
                                        $"Thời điểm thực hiện y lệnh trùng: {File.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} Ngày KQ: {File.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Bác sĩ {File.MA_BAC_SI}\n" +
                                        $"Thời điểm thực hiện BN khác trùng: {xml3Lichsu.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} Ngày KQ {xml3Lichsu.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Người thực hiện {xml3Lichsu.NGUOI_THUC_HIEN}\n" +
                                        $"Theo QĐ 3176 hoặc QĐ 4750, Các dịch vụ nhóm 8, 18 (Phẫu thuật và Thủ thuật), ghi thời điểm bắt đầu thực hiện và kết thúc thực hiện => Không thể ra Y lệnh cho BN khác trong quá trình thực hiện Phẫu Thuật và Thủ thuật"
                                        );
                                }
                            }
                        }
                        //_wa.Stop();
                        //if (_wa.Elapsed.TotalSeconds > .3d)
                        //{
                        //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - XMLLichSu,MA_CSKCB {File.MA_CSKCB} MA LOI: 3733, MA LK {File.MA_LK}, XML03ID {File.XML03ID}  thời gian: {_wa.Elapsed.TotalSeconds}", nameof(Validate));
                        //}
                        //_wa.Restart();
                        #endregion

                        #region Ra y lệnh thuốc trong quá trình thực hiện phẫu thuật, thủ thật
                        //3734    3   XML3 MA_DICH_VU[3176] Bác sĩ phát sinh y lệnh thuốc trong quá trình thực hiện Phẫu thuật, Thủ thật
                        if (_dmErrorMHTCs.Any(x => x.EMErrorCode == 3734))
                        {
                            var xml02Lichsu = await _xml02Repository.GetItem(x =>
                                            x.MA_CSKCB == File.MA_CSKCB
                                            && x.MA_LK != File.MA_LK
                                            && !string.IsNullOrEmpty(x.MA_THUOC)
                                            && x.NGAY_YL != null
                                            && File.NGUOI_THUC_HIEN.Contains(x.MA_BAC_SI.Trim())
                                            && (x.NGAY_YL >= File.NGAY_TH_YL && x.NGAY_YL <= File.NGAY_KQ)
                                        );
                            if (xml02Lichsu != null)
                            {
                                var x01h = await _hosoRepository.GetItem(x => x.XML01ID == xml02Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB);
                                //XML01History bnKhac = _hosoRepository.DbSet.AsNoTracking().Where(x => x.XML01ID == xml02Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB).Select(x => new XML01History { XML01ID = x.XML01ID, HO_TEN = x.HO_TEN, MA_BN = x.MA_BN, NGAY_VAO = x.NGAY_VAO, NGAY_RA = x.NGAY_RA }).FirstOrDefault();
                                if (x01h != null)
                                {
                                    RaiseError(true,
                                        3734,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3734, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM} có thời điểm thực hiện y lệnh trùng với Bệnh nhân khác: Mã BN: {x01h.MA_BN}, Tên BN:{x01h.HO_TEN}, Thời gian vào Viện: {x01h.NGAY_VAO?.ToString("dd/MM/yyyy HH:mm")}, Thời gian ra Viện: {x01h.NGAY_RA?.ToString("dd/MM/yyyy HH:mm")} \n" +
                                        $"Thời điểm thực hiện y lệnh trùng: {File.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} Ngày KQ: {File.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Bác sĩ {File.MA_BAC_SI}\n" +
                                        $"Thời điểm thực hiện BN khác trùng: {xml02Lichsu.NGAY_YL?.ToString("dd/MM/yyyy HH:mm")}, Người thực hiện {xml02Lichsu.MA_BAC_SI}\n" +
                                        $"Theo QĐ 3176 hoặc QĐ 4750, Các dịch vụ nhóm 8, 18 (Phẫu thuật và Thủ thuật), ghi thời điểm bắt đầu thực hiện và kết thúc thực hiện => Không thể ra Y lệnh thuốc cho BN khác trong quá trình thực hiện Phẫu Thuật và Thủ thuật"
                                        );
                                }
                                
                            }
                        }
                        //_wa.Stop();
                        //if (_wa.Elapsed.TotalSeconds > .3d)
                        //{
                        //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - XMLLichSu,MA_CSKCB {File.MA_CSKCB} MA LOI: 3734, MA LK {File.MA_LK}, XML03ID {File.XML03ID}  thời gian: {_wa.Elapsed.TotalSeconds}", nameof(Validate));
                        //}
                        //_wa.Restart();
                        #endregion
                    }
                    
                    if (Uti.HCodeContain(File.MA_NHOM.ToString(), "2;3"))
                    {
                        #region Trùng thời gian thực hiện y lệnh các nhóm 2,3
                        if (_dmErrorMHTCs.Any(x => x.EMErrorCode == 3730))
                        {
                            var xml3Lichsu = await _xml03Repository.GetItem(x => x.MA_CSKCB == File.MA_CSKCB &&
                                                    x.MA_LK != File.MA_LK
                                                    && (x.MA_NHOM == 8 || x.MA_NHOM == 18 || x.MA_NHOM == 2 || x.MA_NHOM == 3)
                                                    && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_KQ != null && x.NGAY_TH_YL != null
                                                    && x.NGUOI_THUC_HIEN == File.NGUOI_THUC_HIEN
                                                    && x.NGAY_TH_YL == File.NGAY_TH_YL
                                                );
                            if (xml3Lichsu != null)
                            {
                                var x01h = await _hosoRepository.GetItem(x => x.XML01ID == xml3Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB);
                                //XML01History xml01lichsu = _hosoRepository.DbSet.AsNoTracking().Where(x => x.XML01ID == xml3Lichsu.XML01ID && x.MA_CSKCB == File.MA_CSKCB).Select(x => new XML01History { XML01ID = x.XML01ID, HO_TEN = x.HO_TEN, MA_BN = x.MA_BN, NGAY_VAO = x.NGAY_VAO, NGAY_RA = x.NGAY_RA }).FirstOrDefault();
                                if (x01h != null)
                                {
                                    RaiseError(true,
                                        3730,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(3730, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân có dịch vụ {File.MA_DICH_VU}|{File.MA_NHOM} có thời điểm thực hiện y lệnh trùng với Bệnh nhân khác: Mã BN: {x01h.MA_BN}, Tên BN:{x01h.HO_TEN}, Thời gian vào: {x01h.NGAY_VAO?.ToString("dd/MM/yyyy HH:mm")}, Thời gian ra: {x01h.NGAY_RA?.ToString("dd/MM/yyyy HH:mm")} \n" +
                                        $"Thời điểm thực hiện y lệnh trùng: {File.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} - {File.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Người thực hiện {File.NGUOI_THUC_HIEN}\n" +
                                        $"Thời điểm thực hiện BN khác trùng: {xml3Lichsu.NGAY_TH_YL?.ToString("dd/MM/yyyy HH:mm")} - {xml3Lichsu.NGAY_KQ?.ToString("dd/MM/yyyy HH:mm")}, Người thực hiện {xml3Lichsu.NGUOI_THUC_HIEN}\n" +
                                        $"Theo QĐ 3176 hoặc QĐ 4750, Các dịch vụ nhóm 2,3 (Cận lâm sàng, Chẩn đoán hình ảnh), ghi thời điểm thực hiện y lệnh => Không thể thực hiện trùng thời điểm thực hiện y lệnh giữa các bệnh nhân khác nhau"
                                        );

                                }
                            }
                        }
                        //_wa.Stop();
                        //if (_wa.Elapsed.TotalSeconds > .3d)
                        //{
                        //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - XMLLichSu,MA_CSKCB {File.MA_CSKCB} MA LOI: 3730, MA LK {File.MA_LK}, XML03ID {File.XML03ID}  thời gian: {_wa.Elapsed.TotalSeconds}", nameof(Validate));
                        //}
                        //_wa.Restart();
                        #endregion
                    }
                }

                //_wa.Stop();
                //if (_wa.Elapsed.TotalSeconds > .3d)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - XMLLichSu , thời gian: {_wa.Elapsed.TotalSeconds.ToString()}", nameof(Validate));
                //}
                //_wa.Restart();



                //	1250	1250	1	XML3	MA_DICH_VU	Bệnh nhân Khám Bệnh nhưng kê giường Bệnh

                if (File.XML01.MA_LOAI_KCB=="01")
                {
                    if (Field(x => x.MA_NHOM).HasValue(14, 15))
                    {
                        RaiseError(true,
                            1250,
                                   $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                   $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                   $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                   $"Nội dung lỗi: {(errorlist.TryGetValue(2943, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                   $"Chi tiết:" +
                                   $"Bệnh nhân đi khám bệnh, Tuy nhiên lại được kê giường bệnh: {File.TEN_DICH_VU}, Bệnh nhân đi khám không được kê giường bệnh."
                            );
                        //RaiseError(true, 1250);
                    }
                }

                //3622	3	XML3	MA_DICH_VU	[3176] Thanh Hóa: Điều Dưỡng không được phép y lệnh Giường cho Bệnh nhân (Chuyên đề bảo hiểm)
                if (Field(x=>x.MA_NHOM).HasValue(14,15))
                {
                    //Linhist changed 19/03/26
                    if (!File.MA_BAC_SI.Contains(";") && !string.IsNullOrEmpty(File.MA_BAC_SI))
                    {
                        var basi = dmBACSI.Where(x => x.MA_CCHN == File.MA_BAC_SI).FirstOrDefault();
                        //301 Điều dưỡng có trình độ từ DH trở lên
                        //302 Điều dưỡng có trình độ dưới ĐH
                        //303 Điều dưỡng có VB Hồi sức cấp cứu
                        //304 DD có VB nội khoa
                        //305 DD có VB Da Liễu
                        //306 DD có văn bằng Tâm Thần
                        //307 DD có văn bằng Nội tiết
                        //308 DD có văn bằng Y học cổ truyền
                        //309 DD có văn bằng Gây mê
                        //310 DD có văn bằng Ngoại
                        //311 DD có văn bằng Bỏng
                        //312 DD có văn Bằng khoa U Biếu
                        //313 DD có VB khoa Sản, phujk hoa
                        //314 DD có VB khoa Mắt
                        //315 DD có VB khoa Tai mũi
                        //316 DD có VB khoa Răng
                        //317 DD có VB Khoa Phục hồi chức năng
                        //318 DD có VB khoa Thăm dò chức năng
                        //319 DD có VB khoa Huyết học
                        if (basi != null)
                        {
                            //B1. Kiểm tra xem có phải điều dưỡng hay không?
                            var phamvichuyenmon = basi.PHAMVI_CM.ToString().Split(";");
                            
                            foreach (var s in phamvichuyenmon)
                            {
                                if (Uti.CodeMatch(s, "301;302;303;304;305;306;307;308;309;310;311;312;323;314;315;316;317;318;319"))
                                {
                                    //Gans neeu la dieu duong
                                    RaiseError(true,
                                        3622,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2943, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}, Người ra y lệnh:{File.MA_BAC_SI}, phạm vi chuyên môn:{s}, là điều dưỡng, không được phép y lệnh ngày giường."
                                        );


                                    //3622	3	XML3	MA_DICH_VU	[3176] Thanh Hóa: Điều Dưỡng không được phép y lệnh Giường cho Bệnh nhân (Chuyên đề bảo hiểm)
                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3622);
                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                    //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}, Người ra y lệnh:{File.MA_BAC_SI}, phạm vi chuyên môn:{s}, là điều dưỡng, không được phép y lệnh ngày giường.";
                                    //RaiseError(true, 3622, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3622", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                    //RaiseError(true, 3622);
                                }
                            }
                        }
                    }
                }

                //3617	3	XML3	MA_DICH_VU	[3176] Bệnh nhân chuyển viện, nhưng không nhập mã Nơi Đến
                //3618	3	XML3	MA_DICH_VU	[3176] Mã CSKCB trong vận chuyển không khớp với Mã Nới đến (Sai hướng dẫn 3176)
                //3619    3   XML3 MA_DICH_VU[3176] Mã Vận chuyển không đúng hướng dẫn của 3176
                if (File.MA_DICH_VU.Contains("VC"))
                {
                    if(String.IsNullOrEmpty(File.XML01.MA_NOI_DEN))
                    {


                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3617);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, nhưng không nhập mã Nơi Đến trong hồ sơ.";
                        //RaiseError(true, 3617, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3617", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true,
                                   3617,
                                   $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                   $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                   $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                   $"Nội dung lỗi: {(errorlist.TryGetValue(2943, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                   $"Chi tiết:" +
                                   $"Bệnh nhân được y lệnh Dịch vụ vận chuyển:{File.MA_DICH_VU}|{File.TEN_DICH_VU},nhưng không nhập mã Nơi Đến trong hồ sơ XML đề nghị thanh toán."
                                   );
                        //RaiseError(true, 3617);
                    }
                    if (File.MA_DICH_VU.Length != 8)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3619);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã Vận chuyển không đúng định dạng (8 ký tự).";
                        //RaiseError(true, 3619, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3619", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true,
                            3619,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3619, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh dịch vụ vẫn chuyển:{File.MA_DICH_VU}|{File.TEN_DICH_VU},mã Vận chuyển không đúng định dạng (8 ký tự)."
                            );
                        //RaiseError(true, 3619);
                        var mavc = File.MA_DICH_VU.Split(".");
                        if (mavc.Length != 2)
                        {
                            //var error1 = dmError130.FirstOrDefault(x => x.ErrorCode == 3619);
                            //string errorInfo1 = error1 != null ? error1.Message : "Không tìm thấy nội dung lỗi";
                            //var benh1 = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //string benhchinhInfo1 = benh1 != null ? benh1.TEN : "Không tìm thấy tên bệnh";
                            //string desc1 = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã Vận chuyển không đúng định dạng (không đúng số lượng phần mã).";
                            //RaiseError(true, 3619, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3619", errorInfo1, File.XML01.MA_BENH_CHINH, benhchinhInfo1, desc1));
                            RaiseError(true,
                                3619,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(3619, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được y lệnh:{File.MA_DICH_VU}|{File.TEN_DICH_VU}, mã Vận chuyển không đúng định dạng (không đúng số lượng phần mã)."
                                );
                            //RaiseError(true, 3619);
                        }
                        else
                        {
                            if (mavc[0].Length != 2)
                            {
                                //var error1 = dmError130.FirstOrDefault(x => x.ErrorCode == 3619);
                                //string errorInfo1 = error1 != null ? error1.Message : "Không tìm thấy nội dung lỗi";
                                //var benh1 = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo1 = benh1 != null ? benh1.TEN : "Không tìm thấy tên bệnh";
                                //string desc1 = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã Vận chuyển không đúng định dạng (Phần mã nhóm:{mavc[0]} không đúng 2 ký tự).";
                                //RaiseError(true, 3619, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3619", errorInfo1, File.XML01.MA_BENH_CHINH, benhchinhInfo1, desc1));
                                RaiseError(true,
                                    3619,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3619, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh:{File.MA_DICH_VU}|{File.TEN_DICH_VU}, mã Vận chuyển không đúng định dạng (Phần mã nhóm:{mavc[0]} không đúng 2 ký tự)."
                                    );
                                //RaiseError(true, 3619);
                            }
                            if (mavc[1].Length != 5)
                            {
                                //var error1 = dmError130.FirstOrDefault(x => x.ErrorCode == 3619);
                                //string errorInfo1 = error1 != null ? error1.Message : "Không tìm thấy nội dung lỗi";
                                //var benh1 = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo1 = benh1 != null ? benh1.TEN : "Không tìm thấy tên bệnh";
                                //string desc1 = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã Vận chuyển không đúng định dạng (Phần mã CSKCB:{mavc[1]} không đúng 5 ký tự).";
                                //RaiseError(true, 3619, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3619", errorInfo1, File.XML01.MA_BENH_CHINH, benhchinhInfo1, desc1));
                                RaiseError(true,
                                    3619,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3619, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh:{File.MA_DICH_VU}|{File.TEN_DICH_VU}, mã Vận chuyển không đúng định dạng (Phần mã CSKCB:{mavc[1]} không đúng 5 ký tự)."
                                    );
                                //RaiseError(true, 3619);
                            }
                        }
                    }
                    else
                    {
                        if (File.MA_DICH_VU.Length > 8)
                        {
                            var macs = File.MA_DICH_VU.Substring(3, 5);
                            if (macs != File.XML01.MA_NOI_DEN)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3618);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc1 = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã CSKCB trong vận chuyển:{macs} không khớp với Mã Nơi đến:{File.XML01.MA_NOI_DEN}.";
                                //RaiseError(true, 3618, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3618", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc1));
                                RaiseError(true,
                                    3618,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3618, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh:{File.MA_DICH_VU}|{File.TEN_DICH_VU}, mã CSKCB trong vận chuyển:{macs} không khớp với Mã Nơi đến:{File.XML01.MA_NOI_DEN}."
                                    );
                                //RaiseError(true, 3618);
                            }
                        }
                    }    
                    
                    
                }

                //3465    3   XML3 MA_DICH_VU[3176] Chuyên đề BH: căn cứ quy trình KT quy định tại QĐ 5737 / QĐ - BYT đợt 2, thực hiện từ chối thanh toán các DVKT không có chỉ định bước sóng laser, công suất phát, chế độ phát, cường độ chùm tia
                if (qD5937s.Any(x=>x.MA_DV==File.MA_DICH_VU))
                {
                    var gtkiemtra = qD5937s.Where(x => x.MA_DV == File.MA_DICH_VU).FirstOrDefault();
                    if(gtkiemtra != null)
                    {
                        var value = gtkiemtra.Value;
                        foreach(var item in value.Split(';'))
                        {
                            var check = File.XML01.XML05s.Where(x => x.PHAU_THUAT.ToLower().Contains(item.ToLower()) && File.NGAY_YL != null && x.THOI_DIEM_DBLS != null && x.THOI_DIEM_DBLS <= File.NGAY_YL);
                            if (check == null || !check.Any())
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3465);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, nhưng không có chỉ định {item} trong hồ sơ.";
                                //RaiseError(true, 3465, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3465", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true,
                                    3465,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2952, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, nhưng không có chỉ định {item} trong hồ sơ."
                                    );
                                //RaiseError(true, 3465);

                                break;
                            }

                                //if(!File.XML01.XML05s.Any(x=>x.PHAU_THUAT.ToLower().Contains(item.ToLower()) && x.THOI_DIEM_DBLS<=File.NGAY_YL))
                                //{
                                //    RaiseError(true, 3465);
                                //}
                        }
                    }
                }    
                #endregion

                //3463	3	XML3	MA_DICH_VU	[3176] Phát sinh chi phí trùng nhau (SL >1 ) trong 1 lần KCB
                if (Uti.HCodeContain(File.MA_NHOM.ToString(),"13;1;2;3;8;18") && !Uti.HCodeContain(File.MA_DICH_VU,"2.202.791.269;2.202.911.280;2.205.021.267;2.202.681.330;2.202.691.329"))
                {
                    if (File.SO_LUONG > 1)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3463);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có số lượng SL:{File.SO_LUONG} >1 trong 1 lần KCB.";
                        //RaiseError(true, 3463, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3463", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true,
                            3463,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3463, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được y lệnh: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có số lượng SL:{File.SO_LUONG} >1 trong 1 lần KCB."
                            );
                        //RaiseError(true, 3463);
                    }
                    else
                    {
                        if (File.NGAY_TH_YL != null)
                        {
                            var duplicates = File.XML01.XML03s
                                            .Where(x => x.MA_DICH_VU == File.MA_DICH_VU
                                                     && x.NGAY_YL != null
                                                     && x.NGAY_TH_YL != null
                                                     && x.NGAY_YL.Value == File.NGAY_YL.Value)
                                            .ToList();
                            if (duplicates.Any())
                            {
                                //Linhist added 19/06/26
                                //4086  XML3	MA_DICH_VU	[BVĐK Trần Lãm] [3176] Dịch vụ kỹ thuật chỉ định 2 lần trong cùng một thời điểm y lệnh
                                if (File.MA_CSKCB == "34002")
                                {
                                    var matchXML03 = duplicates.Where(x => x.XML03ID != File.XML03ID && x.TEN_DICH_VU == File.TEN_DICH_VU).FirstOrDefault();
                                    //haimaster update code 22/6/2026 => matchXML03 = null vẫn xử lý lệnh RaiseErro => báo lỗi
                                    if (matchXML03 != null)
                                    {
                                        RaiseError(true,
                                                4086,
                                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(4086, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y lệnh: {File.STT}|{File.TEN_DICH_VU}|{File.MA_DICH_VU}- Thời điểm y lệnh: {File.NGAY_YL}.\n" +
                                                $"Thời điểm y lệnh trùng:{matchXML03.STT}|{matchXML03.TEN_DICH_VU}|{matchXML03.MA_DICH_VU} - Thời điểm y lệnh: {matchXML03.NGAY_YL}"
                                            );
                                    }
                                }
                                //3737	3	XML3	MA_DICH_VU	[3176] Dịch vụ kỹ thuật chỉ định 2 lần trong cùng một thời điểm y lệnh
                                else
                                {
                                    var matchXML03 = duplicates.Where(x => x.XML03ID != File.XML03ID).FirstOrDefault();
                                    if (matchXML03 != null)

                                    {
                                        RaiseError(true,
                                                3737,
                                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(3737, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y lệnh: {File.STT}|{File.TEN_DICH_VU}|{File.MA_DICH_VU}- Thời điểm y lệnh: {File.NGAY_YL}.\n" +
                                                $"Thời điểm y lệnh trùng: {matchXML03.STT}|{matchXML03.TEN_DICH_VU}|{matchXML03.MA_DICH_VU} - Thời điểm y lệnh: {matchXML03.NGAY_YL}"
                                            );
                                    }
                                }
                            }

                            //Linhist added 24/07/26
                            //4184	2	XML3	NGAY_TH_YL	Các dịch vụ kỹ thuật có thời gian thực hiện cách nhau dưới 3 phút
                            var maNhomKiemtras = new byte?[] { 2, 3, 8, 13, 18 };
                            var dupliateTHYL = duplicates
                                            .Where(x => x.XML03ID != File.XML03ID
                                                     && x.NGAY_YL != null
                                                     && x.NGAY_TH_YL != null
                                                     && maNhomKiemtras.Contains(x.MA_NHOM) 
                                                     && x.NGAY_TH_YL.Value >= File.NGAY_TH_YL.Value.AddMinutes(-3) && x.NGAY_TH_YL.Value <= File.NGAY_TH_YL.Value.AddMinutes(3))
                                            .ToList();
                            if (dupliateTHYL.Any())
                            {
                                var matchXML03 = dupliateTHYL.FirstOrDefault();
                                if (matchXML03 != null)
                                {
                                    RaiseError(true,
                                        4184,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4184, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được y lệnh: {File.STT}|{File.TEN_DICH_VU}|{File.MA_DICH_VU}- Thời điểm TH y lệnh: {File.NGAY_TH_YL}.\n" +
                                        $"Trùng với:{matchXML03.STT}|{matchXML03.TEN_DICH_VU}|{matchXML03.MA_DICH_VU} - Thời điểm TH y lệnh: {matchXML03.NGAY_TH_YL}"
                                        );
                                }
                            }

                            //Linhist added 24/07/26
                            //3738    3   XML3 MA_DICH_VU[3176] Dịch vụ kỹ thuật trùng nhau và trùng thời gian thực hiện
                            var matchTrungGio = dupliateTHYL.Where(x => x.NGAY_TH_YL.Value == File.NGAY_TH_YL.Value).FirstOrDefault();
                            if (matchTrungGio != null)
                            {
                                RaiseError(true,
                                    3738,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3738, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y lệnh: {File.STT}|{File.TEN_DICH_VU}|{File.MA_DICH_VU}- Thời điểm y lệnh: {File.NGAY_TH_YL}.\n" +
                                    $"Thời điểm y lệnh trùng: {matchTrungGio.STT}|{matchTrungGio.TEN_DICH_VU}|{matchTrungGio.MA_DICH_VU} - Thời điểm y lệnh: {matchTrungGio.NGAY_TH_YL}"
                                    );
                            }
                        }
                    }
                }    
                
                
                if (Field(x => x.MA_NHOM).NotValue(10, 11))
                {
                    //Linhist added 03/08/26
                    //4204	2	XML3	MA_NHOM	Ngày y lệnh, ngày thực hiện y lệnh, ngày kết quả của vật tư không được để trống
                    if (File.NGAY_YL == null || File.NGAY_YL == DateTime.MinValue || File.NGAY_TH_YL == null || File.NGAY_TH_YL == DateTime.MinValue || File.NGAY_KQ == null || File.NGAY_KQ == DateTime.MinValue)
                    {
                        RaiseError(true, 4204);
                    }

                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            if (File.NGAY_TH_YL.Value.AddMinutes(5) > File.NGAY_KQ.Value)
                            {
                                //373	373	1	XML3	NGAY_KQ	Ngày kết quả dịch vụ không được nhỏ hơn 5 phút tính từ thời điểm thực hiện y lệnh
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 373);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ} nhỏ hơn 5 phút tính từ thời điểm thực hiện y lệnh:{File.NGAY_TH_YL}.";
                                //RaiseError(true, 373, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "373", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 373
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(373, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ} nhỏ hơn 5 phút tính từ thời điểm thực hiện y lệnh:{File.NGAY_TH_YL}."
                                    );
                                //RaiseError(true, 373);
                            }
                            //2953	3	XML3	NGAY_TH_YL    [3176] Ngày thực hiện Y lệnh nhỏ hơn 3 phút so với ngày Kết quả(Kiểm tra lại)
                            if (File.NGAY_TH_YL.Value.AddMinutes(3) > File.NGAY_KQ.Value)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2953);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ} nhỏ hơn 3 phút tính từ thời điểm thực hiện y lệnh:{File.NGAY_TH_YL}.";
                                //RaiseError(true, 2953, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2953", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2953
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2953, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ} nhỏ hơn 3 phút tính từ thời điểm thực hiện y lệnh:{File.NGAY_TH_YL}."
                                    );
                                //RaiseError(true, 2953);
                            }
                        }
                    }


                }

                ////346 1   XML3 MA_DICH_VU  Có dịch vụ xét nghiệm nhưng không kê thuốc

                if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
                {

                    if (!File.XML01.XML02s.Any(x => x.MA_NHOM == 4))
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 346);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không có kê thuốc BH trong cho Bệnh nhân.";
                        //RaiseError(true, 346, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "346", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 346
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(346, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không có kê thuốc BH trong cho Bệnh nhân."
                            );
                        //RaiseError(true, 346);
                    }
                }
                
                //2887	3	XML3	MA_NHOM	[3176] Thái Bình: Chỉ Khoa Y Học Cổ truyền và PHCN mới kê giường nội trú ban ngày
                if (Field(x => x.MA_NHOM).HasValue(14))
                {
                    if (!(File.MA_KHOA.Contains("K16") || File.MA_KHOA.Contains("K31")))
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2887);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU},Khoa:{File.MA_KHOA} thực hiện kê giường nội trú ban ngày";
                        //RaiseError(true, 2887, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2887", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 2887
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2887, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU},Khoa:{File.MA_KHOA} thực hiện kê giường nội trú ban ngày."
                            );
                        //RaiseError(true, 2887);
                    }
                }

                #region Linhist changed 20/03/26
                ////2847	3	XML3	NGAY_YL	[130] Lào Cai: Ngày Y Lệnh khoa Khám Bệnh ngoài giờ hành chính
                ////2848    3   XML3 NGAY_TH_YL[130] Lào Cai: Ngày TH Y Lệnh khoa Khám Bệnh ngoài giờ hành chính
                ////2849    3   XML3 NGAY_KQ[130] Lào Cai: Ngày KQ khoa Khám Bệnh ngoài giờ hành chính
                //if (File.XML01.MA_DOITUONG_KCB != "2")
                //{
                //    if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
                //    {
                //        Hospital currentHospital = new Hospital();
                //        if (dMhospital.Any())
                //        {
                //            currentHospital = dMhospital.FirstOrDefault(x => !String.IsNullOrEmpty(x.OfficeTime));
                //            if (currentHospital != null && currentHospital.OfficeTime!=null)
                //            {
                //                TimePeriod gio_Hanh_Chinh_BV = new TimePeriod(currentHospital.OfficeTime);
                //                if (File.XML01.MA_LOAI_KCB=="01")
                //                {
                //                    if (Field(x => x.NGAY_YL).NotEmpty())
                //                    {
                //                        if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_YL))
                //                        {
                //                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2847);
                //                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                            //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh.";
                //                            //RaiseError(true, 2847, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2847", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                            RaiseError(true
                //                                , 2847
                //                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                                $"Nội dung lỗi: {(errorlist.TryGetValue(2847, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                                $"Chi tiết:" +
                //                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh."
                //                                );
                //                            //RaiseError(true, 2847);
                //                        }
                //                    }

                //                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                //                    {
                //                        if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_TH_YL))
                //                        {
                //                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2848);
                //                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                            //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh.";
                //                            //RaiseError(true, 2848, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2848", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                            RaiseError(true
                //                                , 2848
                //                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                                $"Nội dung lỗi: {(errorlist.TryGetValue(2848, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                                $"Chi tiết:" +
                //                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh."
                //                                );
                //                            //RaiseError(true, 2848);
                //                        }
                //                    }
                //                    if (Field(x => x.NGAY_KQ).NotEmpty())
                //                    {
                //                        if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_KQ))
                //                        {
                //                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2849);
                //                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                            //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                            //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh.";
                //                            //RaiseError(true, 2849, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2849", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                            RaiseError(true
                //                                , 2849
                //                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                                $"Nội dung lỗi: {(errorlist.TryGetValue(2849, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                                $"Chi tiết:" +
                //                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh."
                //                                );
                //                            //RaiseError(true, 2949);
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                ////2502	3	XML3	NGAY_YL	[130] Ngày y lệnh ngoài thời gian làm việc hành chính
                ////2503    3   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh ngoài thời gian làm việc hành chính
                ////2519	3	XML3	NGAY_KQ	[130] Ngày KQ ngoài thời gian làm việc hành chính của Viện
                ////2886	3	XML3	NGAY_KQ	[3176] Ngày kết quả dịch vụ không được ngoài giờ hành chính
                //if (File.XML01.MA_DOITUONG_KCB != "2")
                //{
                //    Hospital currentHospital = new Hospital();
                //    if (dMhospital.Any())
                //    {
                //        currentHospital = dMhospital.FirstOrDefault(x => !String.IsNullOrEmpty(x.OfficeTime));
                //        if (currentHospital != null && currentHospital.OfficeTime!=null)
                //        {
                //            TimePeriod gio_Hanh_Chinh_BV = new TimePeriod(currentHospital.OfficeTime);
                //            if (Field(x => x.MA_NHOM).HasValue(14))
                //            {
                //                if (Field(x => x.NGAY_KQ).NotEmpty())
                //                {
                //                    if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_KQ))
                //                    {
                //                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2886);
                //                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{currentHospital.OfficeTime} của Viện.";
                //                        //RaiseError(true, 2886, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2886", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                        RaiseError(true
                //                            , 2886
                //                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                            $"Nội dung lỗi: {(errorlist.TryGetValue(2886, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                            $"Chi tiết:" +
                //                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{currentHospital.OfficeTime} của Viện."
                //                            );
                //                        //RaiseError(true, 2886);
                //                    }
                //                }
                //            }

                //            if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
                //            {

                //                if (Field(x => x.NGAY_YL).NotEmpty())
                //                {
                //                    if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_YL))
                //                    {
                //                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2502);
                //                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh.";
                //                        //RaiseError(true, 2502, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2502", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                        RaiseError(true
                //                            , 2502
                //                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                            $"Nội dung lỗi: {(errorlist.TryGetValue(2502, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                            $"Chi tiết:" +
                //                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh."
                //                            );
                //                        //RaiseError(true, 2502);
                //                    }
                //                }
                //                if (Field(x => x.NGAY_TH_YL).NotEmpty())
                //                {
                //                    if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_TH_YL))
                //                    {
                //                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2503);
                //                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh.";
                //                        //RaiseError(true, 2503, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2503", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                        RaiseError(true
                //                            , 2503
                //                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                            $"Nội dung lỗi: {(errorlist.TryGetValue(2503, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                            $"Chi tiết:" +
                //                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh."
                //                            );
                //                        //RaiseError(true, 2503);
                //                    }
                //                }
                //                if (Field(x => x.NGAY_KQ).NotEmpty())
                //                {
                //                    if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(File.NGAY_KQ))
                //                    {
                //                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2519);
                //                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                //                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                //                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                //                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh.";
                //                        //RaiseError(true, 2519, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2519", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                //                        RaiseError(true
                //                            , 2519
                //                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                            $"Nội dung lỗi: {(errorlist.TryGetValue(2519, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                            $"Chi tiết:" +
                //                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{currentHospital.OfficeTime} của Khoa Khám Bệnh."
                //                            );
                //                        //RaiseError(true, 2519);
                //                    }
                //                }
                //            }

                //        }
                //    }
                //}

                if (File.XML01.MA_DOITUONG_KCB != "2" && _officialTime != null && _officialTime.IsValid)
                {
                    //2847	3	XML3	NGAY_YL	[130] Lào Cai: Ngày Y Lệnh khoa Khám Bệnh ngoài giờ hành chính
                    //2848    3   XML3 NGAY_TH_YL[130] Lào Cai: Ngày TH Y Lệnh khoa Khám Bệnh ngoài giờ hành chính
                    //2849    3   XML3 NGAY_KQ[130] Lào Cai: Ngày KQ khoa Khám Bệnh ngoài giờ hành chính
                    if (File.XML01.MA_LOAI_KCB == "01")
                    {
                        if (!_officialTime.InRange(File.NGAY_YL))
                        {
                            RaiseError(true, 2847,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2847, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL} ngoài giờ hành chính:{_officialTime} của Khoa Khám Bệnh."
                            );
                        }
                        if (!_officialTime.InRange(File.NGAY_TH_YL))
                        {
                            RaiseError(true, 2848,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2848, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} ngoài giờ hành chính:{_officialTime} của Khoa Khám Bệnh."
                            );
                        }
                        if (!_officialTime.InRange(File.NGAY_KQ))
                        {
                            RaiseError(true, 2849,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2849, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{_officialTime} của Khoa Khám Bệnh."
                            );
                        }
                    }

                    //2886	3	XML3	NGAY_KQ	[3176] Ngày kết quả dịch vụ không được ngoài giờ hành chính
                    if (File.MA_NHOM == 14 && !_officialTime.InRange(File.NGAY_KQ))
                    {
                        RaiseError(true, 2886,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2886, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{_officialTime} của Viện."
                        );
                    }

                    //2502	3	XML3	NGAY_YL	[130] Ngày y lệnh ngoài thời gian làm việc hành chính
                    //2503    3   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh ngoài thời gian làm việc hành chính
                    //2519	3	XML3	NGAY_KQ	[130] Ngày KQ ngoài thời gian làm việc hành chính của Viện
                    if (File.MA_NHOM == 1 || File.MA_NHOM == 2 || File.MA_NHOM == 3)
                    {
                        if (!_officialTime.InRange(File.NGAY_YL))
                        {
                            RaiseError(true, 2502,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2502, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL} ngoài giờ hành chính:{_officialTime} của Khoa Khám Bệnh."
                            );
                        }
                            
                        if (!_officialTime.InRange(File.NGAY_TH_YL))
                        {
                            RaiseError(true, 2503,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2503, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} ngoài giờ hành chính:{_officialTime} của Khoa Khám Bệnh."
                            );
                        }
                        
                        if (!_officialTime.InRange(File.NGAY_KQ))
                        {
                            RaiseError(true, 2519,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2519, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ} ngoài giờ hành chính:{_officialTime} của Khoa Khám Bệnh."
                            );
                        }
                    }
                }
                #endregion

                //1294	1	XML3	NGAY_YL	BV Lào Cai: Bệnh nhân chạy thận nhân tạo không được phép thực hiện trước 06h hoặc sau 20h
                if (Uti.CodeMatch(File.MA_DICH_VU, "01.0175.0196") && File.MA_CSKCB == "10061" && File.MA_KHOA == "K35" && Uti.NumExists(File.XML01.MA_DOITUONG_KCB, 2))
                {
                    if (File.NGAY_TH_YL.Value.Hour < 6 || File.NGAY_TH_YL.Value.Hour > 20)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1294);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} trước 06h hoặc sau 20h.";
                        //RaiseError(true, 1294, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1294", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 1294
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(1294, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL} trước 06h hoặc sau 20h."
                            );
                        //RaiseError(true, 1294);
                    }
                }

                //2501	3	XML3	NGAY_KQ	[130] Ngày kết quả đang để trống (Mã nhóm 8,18,14,15 không được để trống)
                if (Field(x => x.MA_NHOM).HasValue(8, 18, 14, 15))
                {
                    //Field(x => x.NGAY_KQ).IsEmpty().ThenError(2501);
                    if(!File.NGAY_KQ.HasValue)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2501);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không có ngày Kết quả dịch vụ.";
                        //RaiseError(true, 2501, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2501", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 2501
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2501, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không có ngày Kết quả dịch vụ."
                            );
                    }    
                } 

                //2294	2294	0	XML3	MA_GIUONG	[130] Mã giường không đúng định dạng 4750
                if (Field(x => x.MA_GIUONG).NotEmpty()) 
                {
                    if(!checkMaGiuong(File.MA_GIUONG))
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2294);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã giường:{File.MA_GIUONG} không đúng định dạng.";
                        //RaiseError(true, 2294, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2294", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 2294
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2294, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã giường:{File.MA_GIUONG} không đúng định dạng."
                            );
                    }    
                }
                
                //2475	4	XML3	MA_GIUONG	[130] Dịch vụ giường nhưng không khai báo mã giường, đề nghị kiểm tra
                //2896	3	XML3	MA_KHOA	[130] Hải Phòng: Thanh toán giường nội trú tại khoa không điều trị nội trú sai quy định
                if (Field(x => x.MA_NHOM).HasValue(14, 15))
                {
                    //3778	4	XML3	NGAY_TH_YL	[3176] Kê giường bệnh cho Bệnh nhân 2 lần trên cùng một khoảng thời gian, đề nghị kiểm tra lại
                    if (File.NGAY_TH_YL!=null && File.NGAY_KQ != null)
                    {
                        var giuongtrung = File.XML01.XML03s
                             .Where(x => !string.IsNullOrEmpty(x.MA_NHOM.ToString())
                                      && Uti.CodeContain(x.MA_NHOM.ToString(), "14;15")
                                      && x.NGAY_TH_YL <= File.NGAY_KQ
                                      && x.NGAY_KQ >= File.NGAY_TH_YL
                                      && x.XML03ID != File.XML03ID)
                             .FirstOrDefault();
                        if (giuongtrung != null)
                        {
                            RaiseError(true
                                ,3778
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3778, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê:{File.STT}|{File.TEN_DICH_VU}|{File.MA_DICH_VU}, thời điểm nằm giường của BN là :{File.NGAY_TH_YL.Value.ToString("dd/MM/yyyy HH:mm")} kết thúc lúc:{File.NGAY_KQ.Value.ToString("dd/MM/yyyy HH:mm")}\n" +
                            $"Trùng với y lệnh có STT:{giuongtrung.STT}, kê:{giuongtrung.TEN_DICH_VU}|{giuongtrung.MA_DICH_VU}, thời điểm nằm giường của BN là :{giuongtrung.NGAY_TH_YL.Value.ToString("dd/MM/yyyy HH:mm")} kết thúc lúc:{giuongtrung.NGAY_KQ.Value.ToString("dd/MM/yyyy HH:mm")}"
                            );
                            //Uti.HandleCronJobLog(_logger, $"Validate XML3 Lỗi 3778 Trùng với y lệnh có STT:{giuongtrung.STT}, kê:{giuongtrung.TEN_DICH_VU}|{giuongtrung.MA_DICH_VU}, thời điểm nằm giường của BN là :{giuongtrung.NGAY_TH_YL.Value.ToString("dd/MM/yyyy HH:mm")} kết thúc lúc:{giuongtrung.NGAY_KQ.Value.ToString("dd/MM/yyyy HH:mm")}", nameof(ValidateXML03));
                        }
                    }

                    //Linhist removed 29/06/26
                    //if (File.NGAY_KQ>File.XML01.NGAY_RA)
                    //{
                    //    //199 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được lớn hơn hoặc bằng ngày ra
                    //    //Field(x => x.NGAY_KQ).IsOverOrEqual(File.XML01.NGAY_RA).ThenError(199);
                    //    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 199);
                    //    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                    //    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                    //    //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                    //    //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn ngày ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}.";
                    //    //RaiseError(true, 199, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "199", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                    //    RaiseError(true
                    //        , 199
                    //        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //        $"Nội dung lỗi: {(errorlist.TryGetValue(199, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //        $"Chi tiết:" +
                    //        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn ngày ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                    //        );
                    //}    
                   
                    if(File.NGAY_YL<File.XML01.NGAY_VAO)
                    {
                        //Field(x => x.NGAY_YL).IsUnder(File.XML01.NGAY_VAO).ThenError(194);
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 194);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                        //RaiseError(true, 194, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "194", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 194
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(194, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                            );
                    }    
                    

                    if (Uti.HCodeContain(File.MA_KHOA, "K01;K31;K3940;K41;K44;K47;K35"))
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2896);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã khoa:{File.MA_KHOA} không đúng quy định thanh toán giường nội trú.";
                        //RaiseError(true, 2896, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2896", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 2896
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2896, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã khoa:{File.MA_KHOA} không đúng quy định thanh toán giường nội trú."
                            );
                        //RaiseError(true, 2896);
                    }
                    if(string.IsNullOrEmpty(File.MA_GIUONG))
                    {
                        //Field(x => x.MA_GIUONG).IsEmpty().ThenError(2475);
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2475);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không có mã giường dịch vụ giường nội trú.";
                        //RaiseError(true, 2475, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2475", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 2475
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2475, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không có mã giường dịch vụ giường nội trú."
                            );
                    }

                    //2898 [130] Ngày bắt đầu nằm giường hoặc ngày kết thúc nằm giường không đúng hướng dẫn 130, 4750 và 3176
                    if (Field(x => x.MA_NHOM).HasValue(15))
                    {
                        if (File.NGAY_KQ != null && File.NGAY_TH_YL != null  && File.XML01.NGAY_VAO_NOI_TRU != null&& File.XML01.NGAY_RA != null)
                        {
                            if (File.XML01.XML03s.Count(Count => Count.MA_NHOM == 15) > 1)
                            {
                                if ((File.NGAY_TH_YL.Value.Date != File.XML01.NGAY_VAO_NOI_TRU.Value.Date) && (File.NGAY_KQ.Value.Date != File.XML01.NGAY_RA.Value.Date))
                                {
                                    //Linhist changed 201026 -> ko được dùng .Date để lấy .Hour
                                    //if (File.NGAY_TH_YL.Value.Date.Hour != 0 || File.NGAY_KQ.Value.Date.Hour != 23)
                                    if (File.NGAY_TH_YL.Value.Hour != 0 || File.NGAY_KQ.Value.Hour != 23)
                                    {

                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2898);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} không đúng hướng dẫn QĐ 130, QĐ 4750 và QĐ 3176.";
                                        //RaiseError(true, 2898, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2898", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                                , 2898
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2898, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} không đúng hướng dẫn QĐ 130, QĐ 4750 và QĐ 3176."
                                            );
                                        //RaiseError(true, 2898);
                                    }
                                }

                                if (File.NGAY_TH_YL.Value.Date == File.XML01.NGAY_VAO_NOI_TRU.Value.Date)
                                {
                                    if (File.NGAY_KQ.Value.Hour != 23)
                                    {
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2898);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} không đúng hướng dẫn QĐ 130, QĐ 4750 và QĐ 3176.";
                                        //RaiseError(true, 2898, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2898", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                                , 2898
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2898, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} không đúng hướng dẫn QĐ 130, QĐ 4750 và QĐ 3176."
                                            );
                                        //RaiseError(true, 2898);
                                    }
                                }

                                if (File.NGAY_TH_YL.Value.Date == File.XML01.NGAY_RA.Value.Date)
                                {
                                    //Linhist changed 201026 -> ko được dùng .Date để lấy .Hour
                                    //if (File.NGAY_TH_YL.Value.Date.Hour != 0)
                                    if (File.NGAY_TH_YL.Value.Hour != 0)
                                    {
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2898);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} không đúng hướng dẫn QĐ 130, QĐ 4750 và QĐ 3176.";
                                        //RaiseError(true, 2898, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2898", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                                , 2898
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2898, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} không đúng hướng dẫn QĐ 130, QĐ 4750 và QĐ 3176."
                                            );
                                        //RaiseError(true, 2898);
                                    }
                                }
                            }
                        }
                    }
                }

                if (Field(x => x.MA_NHOM).HasValue(13))
                {
                    //2914	3	XML3	NGAY_KQ	[3176] Dịch vụ Khám Bệnh không được thực hiện dưới 3p
                    if (Field(x => x.NGAY_KQ).NotEmpty())
                    {
                        if (Field(x => x.NGAY_TH_YL).NotEmpty())
                        {
                            if (File.NGAY_KQ.Value.Subtract(File.NGAY_TH_YL.Value).TotalMinutes<3)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2914);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //string benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} không được thực hiện dưới 3 phút so với ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 2914, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2914", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2914
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2914, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} không được thực hiện dưới 3 phút so với ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                                //RaiseError(true, 2914);
                            }
                        }
                    }

                    //Linhist added 220926
                    //4229   2   XML3 NGAY_YL[3176] [Thái Bình] Công khám được y lệnh ngoài giờ hành chính
                    //Linhist changed 250926
                    //if (File.XML01.MA_LOAI_KCB == "01" || File.XML01.MA_LOAI_KCB == "02")
                    if (File.XML01.MA_DOITUONG_KCB != "2" && (File.XML01.MA_LOAI_KCB == "01" || File.XML01.MA_LOAI_KCB == "02"))
                    {
                        if (File.NGAY_YL != null && !_officialTime.InRange(File.NGAY_YL))
                        {
                            RaiseError(true
                                , 4229
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2914, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân khám bệnh: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày y lệnh: {File.NGAY_YL:dd/MM/yyyy HH:mm:ss} ngoài giờ hành chính."
                                );
                        }
                    }    
                }

                if (Field(x => x.MA_NHOM).NotValue(14, 15))
                {
                    if (File.NGAY_KQ != null && File.XML01.NGAY_RA != null)
                    {
                        //199 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được lớn hơn ngày ra
                        RaiseError(File.NGAY_KQ > File.XML01.NGAY_RA,
                            199,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(199, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                            );

                        //Linhist added 250926 - ko tính công khám
                        if (File.MA_NHOM != 13)
                        {
                            //4208	2	XML3	NGAY_KQ	Ngày kết quả dịch vụ không được bằng ngày ra
                            RaiseError(File.NGAY_KQ == File.XML01.NGAY_RA,
                                4208,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(4208, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                                );
                        }
                    }
                }

                #region Kiểm tra các lỗi ngày tháng
                //1590	1	XML3	MA_DICH_VU	Dịch vụ kỹ thuật chỉ định trước ra viện 5 phút
                //Field(x => x.NGAY_YL).TimeUnderMinute(File.XML01.NGAY_RA, 5).ThenError(1590);
                //*** Để lại code cũ vì code mới nghi ngờ lỗi

                //Kiểm tra các lỗi cần NGAY_YL bắt buộc phải có giá trị
                if (Uti.IsValidDate(File.NGAY_YL))
                {
                    if (Uti.IsValidDate(File.XML01.NGAY_RA))
                    {
                        RaiseError(Uti.NumExists(File.MA_NHOM, 1, 2, 3) && File.NGAY_YL.Value.AddMinutes(5) >= File.XML01.NGAY_RA.Value, 1590);
                    }

                    //Kiểm tra thời gian tuan
                    //3254 3   XML3 NGAY_YL 3176: Ngày y lệnh không nằm trong khoảng thời gian đăng ký làm việc Tuần của nhân viên
                    //3270	3	XML3	NGAY_YL	[3176] Ngày y lệnh không nằm trong khoảng thời gian khai báo ngày làm việc của Nhân viên
                    //Linhist removed 25/03/26
                    //if (Field(x => x.NGAY_YL).NotEmpty())
                    //{
                    var tuanlamviec = ParseWeekdays(dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.MA_BAC_SI)?.TG_TUAN);
                    if (tuanlamviec != null && !tuanlamviec.Contains(File.NGAY_YL.Value.DayOfWeek))
                    {
                        RaiseError(true
                                    , 3254
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3254, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} BS chỉ đăng ký làm việc các ngày:" +
                                    $"{string.Join(", ", tuanlamviec.Select(d => d.ToString()))}."
                        );
                    }

                    var ngaylamviec = dmBACSI.FirstOrDefault(x => x.MA_CCHN == File.MA_BAC_SI)?.TG_NGAY;
                    if (ngaylamviec != null && !IsInTimeRanges(ngaylamviec, File.NGAY_YL.Value))
                    {
                        RaiseError(true
                            , 3270
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3270, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} BS chỉ đăng ký làm việc các khung giờ:" +
                            $"{ngaylamviec}."
                            );
                    }
                }

                //Linhist removed 29/06/26
                //Kiểm tra các lỗi cần NGAY_KQ bắt buộc phải có giá trị
                //if (Uti.IsValidDate(File.NGAY_KQ))
                //{
                //    if (File.NGAY_KQ > File.XML01.NGAY_RA && File.MA_NHOM == 13)
                //    {
                //        RaiseError(true
                //                    , 199
                //                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                    $"Nội dung lỗi: {(errorlist.TryGetValue(199, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                    $"Chi tiết:" +
                //                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày KQ:{File.NGAY_KQ} lớn hơn ngày ra viện:{File.XML01.NGAY_RA}."
                //        );
                //    }
                //}

                //Kiểm tra các lỗi cần NGAY_TH_YL bắt buộc phải có giá trị
                if (Uti.IsValidDate(File.NGAY_TH_YL))
                {    
                    #region Linhist changed 24/03/26: Thay đổi kiểm tra người thực hiện, ko phải bác sĩ
                    //3255    3   XML3 NGAY_TH_YL  3176: Ngày TH  y lệnh không nằm trong khoảng thời gian đăng ký làm việc Tuần của nhân viên
                    //3271    3   XML3 NGAY_TH_YL[3176] Ngày TH  y lệnh không nằm trong khoảng thời gian khai báo ngày làm việc của Nhân viên
                    //if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    //{
                    //    var MaBS = File.MA_BAC_SI.Split(";");
                    //    foreach (var BS in MaBS)
                    //    {
                    //        var tuanlamviec = ParseWeekdays(dmBACSI.FirstOrDefault(x => x.MACCHN == BS)?.THOIGIAN_TUAN);
                    //        if (tuanlamviec != null)
                    //        {
                    //            if (!tuanlamviec.Contains(File.NGAY_TH_YL.Value.DayOfWeek))
                    //            {
                    //                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3255);
                    //                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                    //                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                    //                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                    //                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} không nằm trong khoảng thời gian đăng ký làm việc Tuần: {tuanlamviec} của bác sĩ:{BS}.";
                    //                //RaiseError(true, 3255, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3255", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                    //                RaiseError(true
                    //                    , 3255
                    //                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //                    $"Nội dung lỗi: {(errorlist.TryGetValue(3255, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //                    $"Chi tiết:" +
                    //                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} BS chỉ đăng ký làm việc các ngày:" +
                    //                    $"{string.Join(", ", tuanlamviec.Select(d => d.ToString()))}."
                    //                    );
                    //                //RaiseError(true, 3255);
                    //            }
                    //        }
                    //        var ngaylamviec = dmBACSI.FirstOrDefault(x => x.MACCHN == BS)?.THOIGIAN_NGAY;
                    //        if (ngaylamviec != null)
                    //        {
                    //            //Linhist changed 19/03/26
                    //            //if (!IsInTimeRanges(ngaylamviec, File.NGAY_YL.Value))
                    //            if (!IsInTimeRanges(ngaylamviec, File.NGAY_TH_YL.Value))
                    //            {
                    //                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 3271);
                    //                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                    //                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                    //                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                    //                //var desc = $"Bệnh Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} không nằm trong khoảng thời gian đăng ký làm việc Tuần: {tuanlamviec} của bác sĩ:{BS}.";
                    //                //RaiseError(true, 3271, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "3271", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                    //                RaiseError(true
                    //                    , 3271
                    //                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //                    $"Nội dung lỗi: {(errorlist.TryGetValue(3271, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //                    $"Chi tiết:" +
                    //                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} BS chỉ đăng ký làm việc các khung giờ:" +
                    //                    $"{ngaylamviec}."
                    //                    );
                    //                //RaiseError(true, 3271);
                    //            }
                    //        }
                    //    } 
                    //}
                    var ngths = dmBACSI.Where(x => File.NGUOI_THUC_HIEN.Contains(x.MA_CCHN)).ToList();
                    foreach (var ng in ngths)
                    {
                        var tuanlamviec = ParseWeekdays(ng.TG_TUAN);
                        if (tuanlamviec != null && !tuanlamviec.Contains(File.NGAY_TH_YL.Value.DayOfWeek))
                        {
                            RaiseError(true, 3255,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3255, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss};" +
                                    $"Người thực hiện {ng.MA_CCHN} chỉ đăng ký làm việc các ngày: {string.Join(", ", tuanlamviec.Select(d => d.ToString()))}."
                            );
                        }
                        var ngaylamviec = ng.TG_NGAY;
                        if (ngaylamviec != null && !IsInTimeRanges(ngaylamviec, File.NGAY_TH_YL.Value))
                        {
                            RaiseError(true, 3271,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(3271, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss};" +
                                    $"Người thực hiện {ng.MA_CCHN} chỉ đăng ký làm việc các khung giờ: {ngaylamviec}."
                            );
                        }
                    }
                    #endregion
                }

                //Kiểm tra các lỗi cần NGÀY_TH_YL và NGÀY_KQ bắt buộc phải có giá trị
                if (Uti.IsValidDate(File.NGAY_TH_YL, File.NGAY_KQ))
                {
                    string Erinfo = null;
                    //2995	3	XML3	MA_DICH_VU	[3176] Bắc Ninh: Dịch vụ CĐHA thực hiện ko được nhỏ hơn 2 phút
                    //Linhist changed 25/03/26
                    //if(Field(x => x.MA_NHOM).HasValue(2))
                    //{
                    //    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    //    {
                    //        if (Field(x => x.NGAY_KQ).NotEmpty())
                    //        {
                    //            if (File.NGAY_KQ.Value.AddMinutes(-2) < File.NGAY_TH_YL.Value)
                    //            {
                    if (File.MA_NHOM == 2)
                    {
                        RaiseError(File.NGAY_KQ.Value.AddMinutes(-2) < File.NGAY_TH_YL.Value
                                , 2995
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2995, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} không được thực hiện dưới 2 phút so với ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                        );
                    }

                    //2996    3   XML3 MA_DICH_VU[3176] Bắc Ninh: Thời gian khám bệnh không nhỏ hơn 2 phút
                    //Linhist changed 25/03/26
                    //if (Field(x => x.MA_NHOM).HasValue(13))
                    //{
                    //    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    //    {
                    //        if (Field(x => x.NGAY_KQ).NotEmpty())
                    //        {
                    //            if (File.NGAY_KQ.Value.AddMinutes(-2) < File.NGAY_TH_YL.Value)
                    //            {
                    if (File.MA_NHOM == 13)
                    {
                        RaiseError(File.NGAY_KQ.Value.AddMinutes(-2) < File.NGAY_TH_YL.Value
                                    , 2996
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2996, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} không được thực hiện dưới 2 phút so với ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                        );
                    }
                    
                    //Linhist added 06/04/26
                    //3975   2   XML3 NGAY_KQ[3176] Ngày thực hiện y lệnh vật tư trùng ngày kết quả
                    if (File.MA_NHOM == 10)
                    {
                        RaiseError(File.NGAY_TH_YL == File.NGAY_KQ, 3975);
                    }
                }

                //Kiểm tra các lỗi cần NAY_YL, NGÀY_TH_YL và NGÀY_KQ bắt buộc phải có giá trị
                if (Uti.IsValidDate(File.NGAY_YL, File.NGAY_TH_YL, File.NGAY_KQ))
                {
                    string Erinfo = null;
                    //2496	3	XML3	NGAY_TH_YL	[130] Thái Bình: Ngày Thực hiện y lệnh quá 72h từ ngày Y lệnh
                    //2499	3	XML3	NGAY_KQ	[130] Thái Bình: Ngày KQ >72h so với ngày THYL
                    //2845	3	XML3	NGAY_TH_YL	[130] Lào Cai: Ngày Thực hiện y lệnh quá 24h từ ngày Y lệnh
                    //2846    3   XML3 NGAY_KQ[130] Lào Cai: Ngày KQ > 24h so với ngày THYL
                    //2479	4	XML3	NGAY_KQ	[130] ĐK Tỉnh Tuyên Quang: Thời gian KQ - Thời gian thực hiện >=99p (Kiểm tra lại)
                    //Linhist changed 25/03/26
                    //if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    //{
                    //    if (Field(x => x.NGAY_KQ).NotEmpty())
                    //    {
                    //        if (Field(x => x.NGAY_YL).NotEmpty())
                    //        {
                    //            if (File.NGAY_YL.Value.AddHours(72) < File.NGAY_TH_YL.Value)
                    //            {
                    RaiseError(File.NGAY_YL.Value.AddHours(72) < File.NGAY_TH_YL.Value
                                , 2496
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2496, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} quá 72 giờ so với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                    );

                    RaiseError(File.NGAY_TH_YL.Value.AddHours(72) < File.NGAY_KQ.Value
                                , 2499
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2499, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} quá 72 giờ so với ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                    );

                    if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 8, 18))
                    {
                        RaiseError(File.NGAY_YL.Value.AddHours(24) < File.NGAY_TH_YL.Value
                                    , 2845
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2845, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} quá 24 giờ so với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                        );

                        RaiseError(File.NGAY_TH_YL.Value.AddHours(24) < File.NGAY_KQ.Value
                                    , 2846
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2846, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} quá 24 giờ so với ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                        );

                        RaiseError(File.NGAY_TH_YL.Value.AddMinutes(99) < File.NGAY_KQ.Value
                                    , 2479
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2479, out Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} - Ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} >= 99 phút."
                        );
                    }
                }
                #endregion

                #region Kiểm tra các lỗi có liên quan đến các trường khác
                //1007	1	XML3	MA_VAT_TU	Phát sinh chi phí trùng thời điểm vào Viện
                //1008    1   XML3 MA_DICH_VU  Phát sinh chi phí trùng thời điểm vào Viện
                if (File.XML01.MA_LOAI_KCB != "02")
                {
                    if (Field(x => x.MA_NHOM).HasValue(10, 11))
                    {
                        //Field(x => x.NGAY_YL).HasValue(File.XML01.NGAY_VAO).ThenError(1007);
                        if (File.NGAY_YL.HasValue && File.NGAY_YL.Value == File.XML01.NGAY_VAO)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1007);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} trùng với ngày vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 1007, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1007", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                , 1007
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(1007, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} trùng với ngày vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                                );
                            //RaiseError(true, 1007);
                        }
                    }
                    else
                    {
                        //Field(x => x.NGAY_YL).HasValue(File.XML01.NGAY_VAO).ThenError(1008);
                        if(File.NGAY_YL.HasValue && File.NGAY_YL.Value == File.XML01.NGAY_VAO)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1008);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} trùng với ngày vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 1008, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1008", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                , 1008
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(1008, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} trùng với ngày vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                                );
                            //RaiseError(true, 1008);
                        }
                    }
                }
                #endregion

                #region Kiểm tra các lỗi có liên quan đến các Danh mục
                //4202	2	XML3	MA_DICH_VU	Khám Tai Mũi Họng làm cùng ngày y lệnh với các dịch vụ Nội soi Tai Mũi Họng (Công văn 4861/BHXH-CSYT ngày 06/12/2016)
                if (File.MA_DICH_VU == "15.1898" || File.MA_DICH_VU == "15.28")
                {
                    var lstNS = new string[] { "03.1001.2048", "03.1002.2048", "03.1003.2048", "15.9001.2048", "20.0013.0933", "20.0013.2048" };
                    if (File.XML01.XML03s.Any(x => lstNS.Contains(x.MA_DICH_VU) && x.NGAY_YL == File.NGAY_YL))
                    {
                        RaiseError(true, 4202);
                    }
                }  

                //2329    2329    0   XML3 MA_KHOA[130] Mã khoa không tồn tại trong Danh mục phê duyệt
                var dmKhoa = dmKhoaBV.Where(x => x.MA_CSKCB == File.XML01.MA_CSKCB).ToList();
                if(!dmKhoa.Any())
                {
                    //RaiseError(!dmKhoaBV.Any(x => x.MA_KHOA == File.MA_KHOA), 2329);
                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2329);
                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                    //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã khoa:{File.MA_KHOA} không tồn tại trong Danh mục khoa của cơ sở y tế.";
                    //RaiseError(true, 2329, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2329", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                    RaiseError(true
                        , 2329
                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        $"Nội dung lỗi: {(errorlist.TryGetValue(2329, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        $"Chi tiết:" +
                        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã khoa:{File.MA_KHOA} không tồn tại trong Danh mục khoa của cơ sở y tế."
                        );
                }

                //RaiseError(!dmKhoaBV.Any(x => x.MA_KHOA == File.MA_KHOA), 2329);


                if (Field(x => x.MA_DICH_VU).NotEmpty())
                {
                    //243	1	XML3	MA_DICH_VU	Dịch vụ trẻ em không được chỉ định cho người lớn (Sử dụng mã dịch vụ Khoa Nhi dành cho bệnh nhân lớn tuổi, sai TT 39/2018/TT-BYT)
                    if (File.MA_DICH_VU.StartsWith("03."))
                    {
                        //haimaster update 10.4.2023 them cho Lao Cai
                        if (!Uti.CodeMatch(File.MA_DICH_VU.ToString(), "03.1848.1012;03.1848.1013;03.1848.1014;03.1848.1015;"))
                        {
                            var dvtuongduong = dmDICHVU.Where(d => !d.MA_DICH_VU.StartsWith("03.") && d.TEN_DICH_VU == File.TEN_DICH_VU).FirstOrDefault();
                            if (Info.TUOI_BN > 16d && dvtuongduong != null) /*&& d.HIEULUC == "Có"))*/
                            {
                                RaiseError(true
                                    , 243
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(243, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã dịch vụ bắt đầu bằng '03.' nhưng tuổi bệnh nhân:{Info.TUOI_BN} > 16 tuổi.\n" +
                                    $"Dịch vụ tương đương dành cho người lớn là: {dvtuongduong.MA_DICH_VU}|{dvtuongduong.TEN_DICH_VU}."
                                    );
                                //RaiseError(true, 243);
                            }
                        }
                    }

                    //Linhist added 250926
                    //4232	2	XML3	MA_DICH_VU	YHCT Hòa Bình] Dịch vụ YHCT người lớn nhưng chỉ định cho bệnh nhân < 16 tuổi (có mã nhi khoa tương đương)
                    if (File.MA_DICH_VU.StartsWith("08."))
                    {
                        var dvtuongduong = dmDICHVU.Where(d => d.MA_DICH_VU.StartsWith("03.") && d.TEN_DICH_VU == File.TEN_DICH_VU).FirstOrDefault();
                        if (Info.TUOI_BN < 16d && dvtuongduong != null)
                        {
                            RaiseError(true
                                , 4232
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(243, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã dịch vụ bắt đầu bằng '08.' nhưng tuổi bệnh nhân:{Info.TUOI_BN} < 16 tuổi.\n" +
                                $"Dịch vụ tương đương dành cho trẻ em là: {dvtuongduong.MA_DICH_VU}|{dvtuongduong.TEN_DICH_VU}."
                                );
                            //RaiseError(true, 243);
                        }
                    }
                }


                //Vật tư
                //10.7.2024 nếu Vật tư có ngày TH_YL và ngày YL thì phải kiểm tra
                if (Field(x => x.MA_NHOM).HasValue(10, 11))
                {
                    //Linhist added 250826
                    //206 1   XML3 MA_VAT_TU   Mã vật tư không có hiệu lực trong danh mục phê duyệt
                    if (File.NGAY_YL != null)
                    {
                        var ngayYL = File.NGAY_YL.Value.Year * 10000 + File.NGAY_YL.Value.Month * 100 + File.NGAY_YL.Value.Day;
                        if (dmVTYT.Any(x => x.MA_VAT_TU == File.MA_VAT_TU && x.TT_THAU == File.TT_THAU && (string.IsNullOrEmpty(x.TU_NGAY) || (int.TryParse(x.TU_NGAY, out var dte) && dte >= ngayYL))))
                        {
                            RaiseError(true, 206);
                        }
                    }

                    //190 1   XML3 DON_GIA Đơn giá không khớp với đơn giá trong danh mục phê duyệt
                    //191 1   XML3 DON_GIA Đơn giá nhỏ hơn đơn giá trong danh mục phê duyệt
                    //207 1   XML3 DON_GIA Đơn giá lớn hơn đơn giá trong danh mục phê duyệt
                    //1587	2	XML3	DON_GIA_BH	Đơn giá BH chêch lệch quá 1 đồng với đơn giá trong danh mục phê duyệt
                    //4173	3	XML3	DON_GIA_BH	Đơn giá BH chêch lệch ít hơn 1 đồng với đơn giá tối thiểu trong danh mục phê duyệt
                    if (File.DON_GIA_BH != null)
                    {
                        try
                        {
                            if (!cungMAVATTUs.Any(x => x.DON_GIA == File.DON_GIA_BH))
                            {
                                double? maxGia = cungMAVATTUs.Where(x => x.DON_GIA != null && Uti.IsEqual(File.TEN_VAT_TU, x.TEN_VAT_TU, ignoreBracket: true)).Max(x => x.DON_GIA);
                                double? minGia = cungMAVATTUs.Where(x => x.DON_GIA != null && Uti.IsEqual(File.TEN_VAT_TU, x.TEN_VAT_TU, ignoreBracket: true)).Min(x => x.DON_GIA);

                                if (maxGia != null && Uti.IsOverUp(File.DON_GIA_BH, maxGia))
                                {
                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 207);
                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                    //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} lớn hơn đơn giá tối đa:{maxGia} trong danh mục phê duyệt.";
                                    //RaiseError(true, 207, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "207", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                    RaiseError(true
                                        , 207
                                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(207, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} lớn hơn đơn giá tối đa:{maxGia} trong danh mục phê duyệt."
                                        );
                                    //RaiseError(true, 207);
                                }
                                else if (minGia != null && Uti.IsUnderUp(File.DON_GIA_BH, minGia))
                                {
                                    if (File.MA_VAT_TU.Length >= 10)
                                    {
                                        if (!dataTT04.Any(x => x.ID == File.MA_VAT_TU.Substring(0, 10)))
                                        {
                                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 191);
                                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                            //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} nhỏ hơn đơn giá tối thiểu:{minGia} trong danh mục phê duyệt.";
                                            //RaiseError(true, 191, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "191", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                            RaiseError(true
                                                , 191
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(191, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} nhỏ hơn đơn giá tối thiểu:{minGia} trong danh mục phê duyệt."
                                                );
                                            //RaiseError(true, 191);
                                        }
                                        else
                                        {
                                            try
                                            {
                                                if (File.XML01.XML03s.Count(x => !string.IsNullOrEmpty(x.MA_VAT_TU) &&
                                                                        !string.IsNullOrEmpty(File.MA_VAT_TU) &&
                                                                        x.MA_VAT_TU.Length >= 10 &&
                                                                        File.MA_VAT_TU.Length >= 10 &&
                                                                        x.MA_VAT_TU.Substring(0, 10) == File.MA_VAT_TU.Substring(0, 10)) == 2)
                                                {
                                                    if (dataTT04.Any(x => x.ID == File.MA_VAT_TU.Substring(0, 10) && File.DON_GIA_BH != x.Value))
                                                    {
                                                        if (dataTT04.Any(x => x.ID == File.MA_VAT_TU.Substring(0, 10) && File.DON_GIA_BH != double.Parse(x.Value.ToString()) / 2))
                                                        {
                                                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 190);
                                                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                                            //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} không đúng quy định của TT04/2023/TT-BYT với mã VT:{File.MA_VAT_TU.Substring(0, 10)}.";
                                                            //RaiseError(true, 190, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "190", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                                            RaiseError(true
                                                                , 190
                                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                                $"Nội dung lỗi: {(errorlist.TryGetValue(190, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                                $"Chi tiết:" +
                                                                $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} không đúng quy định của TT04/2023/TT-BYT với mã VT:{File.MA_VAT_TU.Substring(0, 10)}."
                                                                );
                                                            //RaiseError(true, 190);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (dataTT04.Any(x => x.ID == File.MA_VAT_TU.Substring(0, 10) && File.DON_GIA_BH != x.Value))
                                                    {
                                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 190);
                                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                                        //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} không đúng quy định của TT04/2023/TT-BYT với mã VT:{File.MA_VAT_TU.Substring(0, 10)}.";
                                                        //RaiseError(true, 190, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "190", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                                        RaiseError(true
                                                                , 190
                                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                                $"Nội dung lỗi: {(errorlist.TryGetValue(190, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                                $"Chi tiết:" +
                                                                $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} không đúng quy định của TT04/2023/TT-BYT với mã VT:{File.MA_VAT_TU.Substring(0, 10)}."
                                                                );
                                                        //RaiseError(true, 190);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Uti.HandleCronJobLog(_logger, $"Validate130: MaBN: {File.XML01.MA_BN} MaCSKCB: {File.XML01.MA_CSKCB}; STT Bị Lỗi: {File.STT} XML3 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 190);
                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                    //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} không khớp với đơn giá trong danh mục phê duyệt.";
                                    //RaiseError(true, 190, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "190", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                    RaiseError(true
                                                , 190
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(190, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} không đúng quy định của TT04/2023/TT-BYT với mã VT:{(File.MA_VAT_TU.Length > 10 ? File.MA_VAT_TU.Substring(0, 10) : File.MA_VAT_TU)}."
                                                );
                                    //RaiseError(true, 190);
                                }

                                maxGia = cungMAVATTUs.Where(x => x.DON_GIA != null && File.TT_THAU == x.TT_THAU).Max(x => x.DON_GIA);
                                minGia = cungMAVATTUs.Where(x => x.DON_GIA != null && File.TT_THAU == x.TT_THAU).Min(x => x.DON_GIA);
                                if (maxGia != null && Uti.IsOverUp((decimal?)File.DON_GIA_BH, (decimal?)maxGia + 1m))
                                {
                                    RaiseError(true
                                        , 1587
                                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(1587, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} lớn hơn đơn giá tối đa:{maxGia} trong danh mục phê duyệt."
                                        );
                                }
                                else if (minGia != null && Uti.IsUnderUp((decimal?)File.DON_GIA_BH, (decimal?)minGia - 1m))
                                {
                                    RaiseError(true
                                        , 4173
                                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(4173, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có đơn giá:{File.DON_GIA_BH} nhỏ hơn đơn giá tối thiểu:{maxGia} trong danh mục phê duyệt."
                                        );
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: MaBN: {File.XML01.MA_BN} MaCSKCB: {File.XML01.MA_CSKCB}; STT Bị Lỗi: {File.STT} XML3 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
                        }
                    }

                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        //2062	4	XML3	NGAY_TH_YL	[130] Ngày thực hiện y lệnh  không được lớn hơn thời điểm hiện tại
                        Field(x => x.NGAY_TH_YL).IsOver(DateTime.Now).ThenError(2062);
                        //2295    2295    0   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được nhỏ hơn ngày y lệnh  NULL
                        //2296    2296    0   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được lớn hơn ngày kết quả NULL
                        if (Field(x => x.NGAY_YL).NotEmpty())
                        {
                            //Field(x => x.NGAY_TH_YL).IsUnder(File.NGAY_YL).ThenError(2295);
                            if (File.NGAY_KQ != null && File.NGAY_TH_YL < File.NGAY_YL)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2295);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 2295, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2295", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2295
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2295, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                                //RaiseError(true, 2295);
                            }
                        }
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            //Field(x => x.NGAY_TH_YL).IsOver(File.NGAY_KQ).ThenError(2296);
                            if (File.NGAY_KQ != null && File.NGAY_TH_YL > File.NGAY_KQ)
                            {
                                var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2296);
                                string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss}.";
                                RaiseError(true, 2296, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2296", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //RaiseError(true, 2296);
                            }
                            //196	1	XML3	NGAY_KQ	Ngày kết quả dịch vụ không được lớn hơn thời gian hiện tại
                            Field(x => x.NGAY_KQ).IsOver(DateTime.Now).ThenError(196);
                            //198 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được nhỏ hơn ngày y lệnh
                            //Field(x => x.NGAY_KQ).IsUnder(File.NGAY_YL).ThenError(198);
                            if (File.NGAY_KQ != null && File.NGAY_YL != null && File.NGAY_KQ < File.NGAY_YL)
                            {
                                RaiseError(true,
                                    198,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(198, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );

                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 198);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 198, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "198", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //RaiseError(true, 198);
                            }
                            //2067	4	XML3	NGAY_KQ	[130] Ngày kết quả không được nhỏ hơn ngày thực hiện y lệnh
                            //Field(x => x.NGAY_KQ).IsUnder(File.NGAY_TH_YL).ThenError(2067);
                            if (File.NGAY_KQ != null && File.NGAY_TH_YL != null && File.NGAY_KQ < File.NGAY_TH_YL)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2067);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 2067, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2067", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true,
                                    2067,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2067, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                                //RaiseError(true, 2067);
                            }
                            //1188	1	XML3	NGAY_KQ	Thời gian Kết quả trùng Thời điểm Y Lệnh
                            //Field(x => x.NGAY_KQ).HasValue(File.NGAY_YL).ThenError(1188);
                            if (File.NGAY_KQ != null && File.NGAY_YL != null && File.NGAY_KQ == File.NGAY_YL)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1188);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 1188, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1188", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //RaiseError(true, 1188);
                                RaiseError(true,
                                    1188,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(1188, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                            }
                            //2068	4	XML3	NGAY_KQ	[130] Ngày kết quả không được nhỏ hơn ngày vào
                            //Field(x => x.NGAY_KQ).IsUnder(File.XML01.NGAY_VAO).ThenError(2068);
                            if (File.NGAY_KQ != null && File.XML01.NGAY_VAO != null && File.NGAY_KQ < File.XML01.NGAY_VAO)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2068);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 2068, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2068", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true,
                                    2068,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2068, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                                    );
                                //RaiseError(true, 2068);
                            }

                            //Linhist removed 29/06/26
                            //// //199 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được lớn hơn hoặc bằng ngày ra
                            ////Field(x => x.NGAY_KQ).IsOverOrEqual(File.XML01.NGAY_RA).ThenError(199);
                            //if (File.NGAY_KQ != null && File.XML01.NGAY_RA != null && File.NGAY_KQ > File.XML01.NGAY_RA)
                            //{
                            //    RaiseError(true,
                            //        199,
                            //        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            //        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            //        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            //        $"Nội dung lỗi: {(errorlist.TryGetValue(199, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            //        $"Chi tiết:" +
                            //        $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                            //        );
                            //    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 199);
                            //    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //    //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}.";
                            //    //RaiseError(true, 199, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "199", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            //    //RaiseError(true, 199);

                            //}
                        }
                    }
                }

                //Dịch vụ
                if (Field(x => x.MA_NHOM).NotValue(10, 11))
                {
                    //Linhist removed 29/06/26
                    // //199 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được lớn hơn hoặc bằng ngày ra
                    ////Field(x => x.NGAY_KQ).IsOverOrEqual(File.XML01.NGAY_RA).ThenError(199);
                    //if (File.NGAY_KQ != null && File.XML01.NGAY_RA != null && File.NGAY_KQ >= File.XML01.NGAY_RA && Uti.HCodeContain(File.MA_NHOM.ToString(),"1;2;3"))
                    //{
                    //    RaiseError(true,
                    //        199,
                    //        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //        $"Nội dung lỗi: {(errorlist.TryGetValue(199, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //        $"Chi tiết:" +
                    //        $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                    //        );
                    //    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 199);
                    //    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                    //    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                    //    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                    //    //var desc = $"Bệnh nhân được kê: {File.TEN_VAT_TU}|{File.MA_VAT_TU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}.";
                    //    //RaiseError(true, 199, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "199", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                    //    //RaiseError(true, 199);

                    //}

                    ////180 1   XML3 MA_NHOM Mã nhóm không khớp với mã nhóm trong danh mục phê duyệt
                    ///
                    RaiseError(cungMADICHVUs != null && !cungMADICHVUs.Any(x => x.MANHOM_5937 == File.MA_NHOM), 180);
                    //2841	3	XML3	SO_LUONG	[130] Lào Cai: Dịch vụ hút đờm SL chỉ được là 1
                    //01.0055.0114;03.0076.0114;03.0092.0299;01.0054.0114;01.0056.0300;02.0150.0114;03.0091.0300;

                    if (Uti.HCodeContain(File.MA_DICH_VU, "01.0055.0114;03.0076.0114;03.0092.0299;01.0054.0114;01.0056.0300;02.0150.0114;03.0091.0300;"))
                    {
                        
                        //Field(x => x.SO_LUONG).NotValue(1).ThenError(2841);
                        if(File.SO_LUONG>1)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2841);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có số lượng:{File.SO_LUONG} > 1.";
                            //RaiseError(true, 2841, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2841", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 2841
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2841, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có số lượng:{File.SO_LUONG} > 1."
                                    );
                        }    
                    }

                    //190 1   XML3 DON_GIA Đơn giá không khớp với đơn giá trong danh mục phê duyệt
                    //191 1   XML3 DON_GIA Đơn giá nhỏ hơn đơn giá trong danh mục phê duyệt
                    //207 1   XML3 DON_GIA Đơn giá lớn hơn đơn giá trong danh mục phê duyệt
                    if (Field(x => x.DON_GIA_BH).NotEmpty())
                    {
                        if (!cungMADICHVUs.Any(x => x.DON_GIA == File.DON_GIA_BH))
                        {

                            double? maxGia = cungMADICHVUs.Where(x => x.DON_GIA != null && Uti.IsEqual(File.TEN_DICH_VU, x.TEN_DICH_VU, ignoreBracket: true)).Max(x => x.DON_GIA);
                            double? minGia = cungMADICHVUs.Where(x => x.DON_GIA != null && Uti.IsEqual(File.TEN_DICH_VU, x.TEN_DICH_VU, ignoreBracket: true)).Min(x => x.DON_GIA);

                            if (maxGia != null && File.DON_GIA_BH > maxGia)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 207);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có đơn giá:{File.DON_GIA_BH} lớn hơn đơn giá tối đa:{maxGia} trong danh mục phê duyệt.";
                                //RaiseError(true, 207, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "207", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 207
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(207, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có đơn giá:{File.DON_GIA_BH} lớn hơn đơn giá tối đa:{maxGia} trong danh mục phê duyệt."
                                    );
                                //RaiseError(true, 207);
                            }
                            else if (minGia != null && File.DON_GIA_BH < minGia)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 191);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có đơn giá:{File.DON_GIA_BH} nhỏ hơn đơn giá tối thiểu:{minGia} trong danh mục phê duyệt.";
                                //RaiseError(true, 191, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "191", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 191
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(191, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có đơn giá:{File.DON_GIA_BH} nhỏ hơn đơn giá tối thiểu:{minGia} trong danh mục phê duyệt."
                                    );

                                //RaiseError(true, 191);
                            }
                            else
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 190);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có đơn giá:{File.DON_GIA_BH} không khớp với đơn giá trong danh mục phê duyệt.";
                                //RaiseError(true, 190, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "190", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 190
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(190, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có đơn giá:{File.DON_GIA_BH} không đúng quy định của TT04/2023/TT-BYT với mã VT:{(File.MA_VAT_TU.Length>10? File.MA_VAT_TU.Substring(0, 10) : File.MA_VAT_TU)}."
                                    );

                                //RaiseError(true, 190);
                            }
                        }
                    }

                    //Linhist changed 091026
                    //if (cungMADICHVUs == null || cungMADICHVUs.Count() == 0)
                    if (!cungMADICHVUs.Any())
                    {
                        if (!File.MA_DICH_VU.Contains("VC"))
                        {
                            //203	1	XML3	MA_DICH_VU	Mã dịch vụ không được tìm thấy trong danh mục phê duyệt
                            var dichvutuongduong = dmDICHVU.Where(x =>Uti.NormalizeUnicode(x.MA_DICH_VU) == Uti.NormalizeUnicode(File.MA_DICH_VU)).FirstOrDefault();
                            var madichvutuongduong = dichvutuongduong == null ? "Không có mã dịch vụ" : dichvutuongduong.MA_DICH_VU;
                            var tendichvutuongduong = dichvutuongduong == null ? "Không có tên dịch vụ" : dichvutuongduong.TEN_DICH_VU;
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 203);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không tìm thấy mã dịch vụ trong danh mục phê duyệt.";
                            ////desc += $" \nMã dịch vụ tương đương gợi ý: {madichvutuongduong}|{tendichvutuongduong}";
                            //desc += $" \nMã dịch vụ tương đương gợi ý: {madichvutuongduong}|độ dài:{madichvutuongduong.Length}|Unicode form:{Uti.GetUnicodeForm(madichvutuongduong)}";   
                            //desc += $" \nMã dịch vụ trong XML: {File.MA_DICH_VU}|độ dài:{File.MA_DICH_VU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_DICH_VU)}";   
                            //////desc= desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                            ////                        desc = desc + $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}";
                            //RaiseError(true, 203, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "203", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 203
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(203, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, không tìm thấy mã dịch vụ trong danh mục phê duyệt.\n" +
                                    $"Mã dịch vụ tương đương gợi ý: {madichvutuongduong?? ""}|độ dài:{madichvutuongduong?.Length?? 0}|Unicode form:{Uti.GetUnicodeForm(madichvutuongduong?? "")}\n" +
                                    $"Mã dịch vụ trong XML: {File.MA_DICH_VU}|độ dài:{File.MA_DICH_VU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_DICH_VU)}"
                                    );
                            //RaiseError(true, 203);
                        }
                        
                        //2319	2319	0	XML3	MA_DICH_VU	[130] Mã dịch vụ thừa khoảng trắng (Dấu cách)
                        var madv = dmDICHVU.FirstOrDefault(x => x.MA_DICH_VU == File.MA_DICH_VU.Trim());
                        if (madv != null)
                        {
                            RaiseError(true,
                                2319,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2319, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã dịch vụ thừa khoảng trắng (Dấu cách).\n" +
                                $"Dịch vụ được kê: {File.MA_DICH_VU}, có độ dài: {File.MA_DICH_VU.Length}\n" +
                                $"Dịch vụ trong DM: {madv.MA_DICH_VU}, có độ dài:{madv.MA_DICH_VU.Length}"
                                );
                        }
                    }
                    //Linhist removed 091026 - đã chuyển lên trên
                    //else
                    //{
                        
                    //    //RaiseError(!cungMADICHVUs.Any(), 203);
                    //    //2319	2319	0	XML3	MA_DICH_VU	[130] Mã dịch vụ thừa khoảng trắng (Dấu cách)
                    //    if (!cungMADICHVUs.Any())
                    //    {
                    //        var madv = dmDICHVU.FirstOrDefault(x => x.MA_DICH_VU == File.MA_DICH_VU.Trim()).MA_DICH_VU;
                    //        if(madv !=null)
                    //        {
                    //            RaiseError(true,
                    //                2319,
                    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //                $"Nội dung lỗi: {(errorlist.TryGetValue(2319, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //                $"Chi tiết:" +
                    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, mã dịch vụ thừa khoảng trắng (Dấu cách).\n" +
                    //                $"Dịch vụ được kê: {File.MA_DICH_VU}, có độ dài: {File.MA_DICH_VU.Length}\n"+
                    //                $"Dịch vụ trong DM: {madv}, có độ dài:{madv.Length}"
                    //                );
                    //        }    
                    //        //RaiseError(dmDICHVU.Any(x => x.MA_DICH_VU == File.MA_DICH_VU.Trim()), 2319);
                    //    }
                    //    //183 1   XML3 TEN_DICH_VU Tên dịch vụ không khớp với tên dịch vụ trong danh mục phê duyệt
                    //    //2176	4	XML3	TEN_DICH_VU	[130] Tên dịch vụ thừa khoảng trắng (Dấu cách), có độ dài không đúng độ dài trong danh mục phê duyệt
                    //    //2331	2341	0	XML3	TEN_DICH_VU	[130] Tên dịch vụ không khớp với Danh mục phê duyệt (Không phân biệt chữ hoa chữ thường)
                    //    //TT_CUT là cắt bỏ các đoạn trong đấu [] mô tả cho tên Dịch vụ

                    //    //Linhist changed 13/04/26
                    //    //if (!cungMADICHVUs.Any(t => Uti.IsEqual(t.TEN_DICH_VU, File.TEN_DICH_VU)))
                    //    //{
                    //    //    if (File.TEN_DICH_VU != null)
                    //    //    {
                    //    //        //var TenDVGanDung = dmDICHVU.FirstOrDefault(x => x.TEN_DICH_VU != null & x.TEN_DICH_VU.ToLower().Trim() == TT_CUT(File.TEN_DICH_VU).ToLower().Trim()).TEN_DICH_VU;
                    //    //        var target = dmDICHVU.FirstOrDefault(x =>
                    //    //                    x.TEN_DICH_VU != null &&
                    //    //                    TT_CUT(File?.TEN_DICH_VU)?.ToLower()?.Trim() == x.TEN_DICH_VU.ToLower().Trim()
                    //    //                );

                    //    //        var TenDVGanDung = target?.TEN_DICH_VU;
                    //    //        if (!cungMADICHVUs.Any(t => Uti.IsEqual(t.TEN_DICH_VU, TT_CUT(File.TEN_DICH_VU))))
                    //    //        {
                    //    //            RaiseError(true,
                    //    //                183,
                    //    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //    //                $"Nội dung lỗi: {(errorlist.TryGetValue(183, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //    //                $"Chi tiết:" +
                    //    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, tên dịch vụ không khớp với Danh mục phê duyệt .\n" +
                    //    //                $"Tên dịch vụ gần đúng gợi ý: {(TenDVGanDung ?? "")}|độ dài:{(TenDVGanDung?.Length ?? 0)}"
                    //    //                );
                    //    //        }
                    //    //        if (!dmDICHVU.Any(t => Uti.IsEqual(t.TEN_DICH_VU.ToUpper(), TT_CUT(File.TEN_DICH_VU).ToUpper())))
                    //    //        {
                    //    //            RaiseError(true,
                    //    //                2341,
                    //    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //    //                $"Nội dung lỗi: {(errorlist.TryGetValue(2341, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //    //                $"Chi tiết:" +
                    //    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, tên dịch vụ không khớp với Danh mục phê duyệt (Không phân biệt chữ hoa chữ thường).\n" +
                    //    //                $"Tên dịch vụ trên XML{File.TEN_DICH_VU}| độ dài: {File.TEN_DICH_VU.Length}\n" +
                    //    //                $"Tên dịch vụ gần đúng gợi ý: {(TenDVGanDung ?? "")}|độ dài:{(TenDVGanDung?.Length ?? 0)}"
                    //    //                );
                    //    //        }
                    //    //        if (!dmDICHVU.Any(t => t.TEN_DICH_VU.Length == TT_CUT(File.TEN_DICH_VU).Length))
                    //    //        {
                    //    //            RaiseError(true,
                    //    //                2176,
                    //    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //    //                $"Nội dung lỗi: {(errorlist.TryGetValue(2176, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //    //                $"Chi tiết:" +
                    //    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, tên dịch vụ thừa khoảng trắng (Dấu cách), có độ dài không đúng độ dài trong danh mục phê duyệt.\n" +
                    //    //                $"Độ dài tên dịch vụ trong XML: {TT_CUT(File.TEN_DICH_VU).Length}\n" +
                    //    //                $"Tên dịch vụ gần đúng gợi ý: {(TenDVGanDung ?? "")}|độ dài:{(TenDVGanDung?.Length ?? 0)}"
                    //    //                );
                    //    //        }
                    //    //    }    

                    //    //}
                    //    if (!string.IsNullOrWhiteSpace(File.TEN_DICH_VU))
                    //    {
                    //        var TenDVGanDung = Uti.ReplaceQuote(File.TEN_DICH_VU, true, false);
                    //        if (!cungMADICHVUs.Any(t => Uti.IsEqual(t.TEN_DICH_VU, File.TEN_DICH_VU, ignoreBracket: true)))
                    //        {
                    //            RaiseError(true,
                    //                183,
                    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //                $"Nội dung lỗi: {(errorlist.TryGetValue(183, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //                $"Chi tiết:" +
                    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, tên dịch vụ không khớp với Danh mục phê duyệt.\n" +
                    //                $"Tên dịch vụ gần đúng gợi ý: {(TenDVGanDung ?? "")}|độ dài:{(TenDVGanDung?.Length ?? 0)}"
                    //                );
                    //        }
                    //        if (!dmDICHVU.Any(t => Uti.IsEqual(t.TEN_DICH_VU, File.TEN_DICH_VU, ignoreCase: true, ignoreBracket: true)))
                    //        {
                    //            RaiseError(true,
                    //                2341,
                    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //                $"Nội dung lỗi: {(errorlist.TryGetValue(2341, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //                $"Chi tiết:" +
                    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, tên dịch vụ không khớp với Danh mục phê duyệt (Không phân biệt chữ hoa chữ thường).\n" +
                    //                $"Tên dịch vụ trên XML{File.TEN_DICH_VU}| độ dài: {File.TEN_DICH_VU.Length}\n" +
                    //                $"Tên dịch vụ gần đúng gợi ý: {(TenDVGanDung ?? "")}|độ dài:{(TenDVGanDung?.Length ?? 0)}"
                    //                );
                    //        }
                    //        if (!dmDICHVU.Any(t => Uti.ReplaceQuote(t.TEN_DICH_VU, true, false).Length == Uti.ReplaceQuote(File.TEN_DICH_VU, true, false).Length))
                    //        {
                    //            RaiseError(true,
                    //                2176,
                    //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                    //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                    //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                    //                $"Nội dung lỗi: {(errorlist.TryGetValue(2176, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                    //                $"Chi tiết:" +
                    //                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, tên dịch vụ thừa khoảng trắng (Dấu cách), có độ dài không đúng độ dài trong danh mục phê duyệt.\n" +
                    //                $"Độ dài tên dịch vụ trong XML: {TT_CUT(File.TEN_DICH_VU).Length}\n" +
                    //                $"Tên dịch vụ gần đúng gợi ý: {(TenDVGanDung ?? "")}|độ dài:{(TenDVGanDung?.Length ?? 0)}"
                    //                );
                    //        }
                    //    }
                    //}

                    //193 1   XML3 NGAY_YL Ngày y lệnh dịch vụ không được lớn hơn thời gian hiện tại
                    Field(x => x.NGAY_YL).IsOver(DateTime.Now).ThenError(193);
                    //2062	4	XML3	NGAY_TH_YL	[130] Ngày thực hiện y lệnh  không được lớn hơn thời điểm hiện tại
                    Field(x => x.NGAY_TH_YL).IsOver(DateTime.Now).ThenError(2062);

                    // 2295    2295    0   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được nhỏ hơn ngày y lệnh  NULL
                    //2296    2296    0   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được lớn hơn ngày kết quả NULL
                    //2840	3	XML3	NGAY_TH_YL	[130] Ngày thực hiện Y lệnh trùng với thời điểm Y lệnh
                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        //Field(x => x.NGAY_TH_YL).IsUnder(File.NGAY_YL).ThenError(2295);
                        if(File.NGAY_YL != null && File.NGAY_TH_YL < File.NGAY_YL)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2295);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 2295, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2295", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 2295
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2295, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                            //RaiseError(true, 2295);
                        }
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            if(File.NGAY_KQ != null && File.NGAY_TH_YL > File.NGAY_KQ)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2296);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 2296, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2296", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2296
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2296, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss}."
                                    );
                                //RaiseError(true, 2296);
                            }
                            //Field(x => x.NGAY_TH_YL).IsOver(File.NGAY_KQ).ThenError(2296);
                        }
                        if (Field(x => x.MA_NHOM).NotValue(14, 15, 13))
                        {

                            //Field(x => x.NGAY_TH_YL).HasValue(File.NGAY_YL).ThenError(2840);
                            if(File.NGAY_YL != null && File.NGAY_TH_YL == File.NGAY_YL)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2840);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} trùng với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                                //RaiseError(true, 2840, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2840", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2840
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2840, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} trùng với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                                //RaiseError(true, 2840);
                            }
                        }
                    }

                    if (!Field(x => x.NGAY_KQ).IsEmpty())
                    {
                        //196	1	XML3	NGAY_KQ	Ngày kết quả dịch vụ không được lớn hơn thời gian hiện tại
                        Field(x => x.NGAY_KQ).IsOver(DateTime.Now).ThenError(196);
                        //198 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được nhỏ hơn ngày y lệnh
                        //Field(x => x.NGAY_KQ).IsUnder(File.NGAY_YL).ThenError(198);
                        if(File.NGAY_KQ != null && File.NGAY_YL != null && File.NGAY_KQ < File.NGAY_YL)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 198);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 198, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "198", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 198
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(198, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                            //RaiseError(true, 198);
                        }
                        //2067	4	XML3	NGAY_KQ	[130] Ngày kết quả không được nhỏ hơn ngày thực hiện y lệnh
                        //Field(x => x.NGAY_KQ).IsUnder(File.NGAY_TH_YL).ThenError(2067);
                        if(File.NGAY_KQ != null && File.NGAY_TH_YL != null && File.NGAY_KQ < File.NGAY_TH_YL)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2067);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 2067, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2067", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 2067
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2067, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                            //RaiseError(true, 2067);
                        }
                        //1188	1	XML3	NGAY_KQ	Thời gian Kết quả trùng Thời điểm Y Lệnh
                        //Field(x => x.NGAY_KQ).HasValue(File.NGAY_YL).ThenError(1188);
                        if(File.NGAY_KQ != null && File.NGAY_YL != null && File.NGAY_KQ == File.NGAY_YL)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1188);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 1188, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1188", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 1188
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(1188, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng với ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}."
                                    );
                            //RaiseError(true, 1188);
                        }
                        //2068	4	XML3	NGAY_KQ	[130] Ngày kết quả không được nhỏ hơn ngày vào
                        //Field(x => x.NGAY_KQ).IsUnder(File.XML01.NGAY_VAO).ThenError(2068);
                        if(File.NGAY_KQ != null && File.XML01.NGAY_VAO != null && File.NGAY_KQ < File.XML01.NGAY_VAO)
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2068);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                            //RaiseError(true, 2068, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2068", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 2068
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2068, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                                    );
                            //RaiseError(true, 2068);
                        }

                        ////Linhist changed 29/06/26
                        //// //199 1   XML3 NGAY_KQ Ngày kết quả dịch vụ không được lớn hơn hoặc bằng ngày ra
                        //if (File.MA_NHOM != 13)
                        //{
                        //    //Field(x => x.NGAY_KQ).IsOverOrEqual(File.XML01.NGAY_RA).ThenError(199);
                        //    if(File.NGAY_KQ != null && File.XML01.NGAY_RA != null && File.NGAY_KQ >= File.XML01.NGAY_RA)
                        //    {
                        //        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 199);
                        //        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}.";
                        //        //RaiseError(true, 199, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "199", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        //        RaiseError(true
                        //            , 199
                        //            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //            $"Nội dung lỗi: {(errorlist.TryGetValue(199, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //            $"Chi tiết:" +
                        //            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Kết Quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} lớn hơn hoặc bằng ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                        //            );
                        //        //RaiseError(true, 199);
                        //    }
                        //}

                    }



                    //Update nếu có K nghĩa là Dịch vụ được thực hiện tại CS Khác, không kiểm tra ngày Y Lệnh và ngày thực hiện y lệnh
                    //Linhist changed 02/12/22
                    //if (File.MA_DICH_VU.IndexOf("K") > -1)
                    //if (File.MA_DICH_VU.IndexOf("K") > -1)
                    //194	1	XML3	NGAY_YL	Ngày y lệnh dịch vụ không được nhỏ hơn ngày vào
                    //Field(x => x.NGAY_YL).IsUnder(File.XML01.NGAY_VAO).ThenError(194);
                    if(File.NGAY_YL != null && File.XML01.NGAY_VAO !=null && File.NGAY_YL<File.XML01.NGAY_VAO)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 194);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                        //RaiseError(true, 194, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "194", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                                    , 194
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(194, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                                    );
                    }    
                    //195 1   XML3 NGAY_YL Ngày y lệnh dịch vụ không được lớn hơn ngày ra
                    //Field(x => x.NGAY_YL).IsOver(File.XML01.NGAY_RA).ThenError(195);
                    if(File.NGAY_YL != null && File.XML01.NGAY_RA != null && File.NGAY_YL > File.XML01.NGAY_RA)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 195);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}.";
                        //RaiseError(true, 195, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "195", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                                    , 195
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(195, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                                    );
                    }
                    // 2063    4   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được nhỏ hơn ngày vào
                    //Field(x => x.NGAY_TH_YL).IsUnder(File.XML01.NGAY_VAO).ThenError(2063);
                    if(File.NGAY_TH_YL != null && File.XML01.NGAY_VAO != null && File.NGAY_TH_YL < File.XML01.NGAY_VAO)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2063);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}.";
                        //RaiseError(true, 2063, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2063", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                                    , 2063
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2063, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} nhỏ hơn ngày Vào viện:{File.XML01.NGAY_VAO:dd/MM/yyyy HH:mm:ss}."
                                    );
                        //RaiseError(true, 2063);
                    }
                    //2064    4   XML3 NGAY_TH_YL[130] Ngày thực hiện y lệnh không được lớn hơn ngày ra
                    //Field(x => x.NGAY_TH_YL).IsOver(File.XML01.NGAY_RA).ThenError(2064);
                    if(File.NGAY_TH_YL != null && File.XML01.NGAY_RA != null && File.NGAY_TH_YL > File.XML01.NGAY_RA)
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2064);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}.";
                        //RaiseError(true, 2064, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2064", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                                    , 2064
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2064, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có ngày Thực hiện Y Lệnh:{File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} lớn hơn ngày Ra viện:{File.XML01.NGAY_RA:dd/MM/yyyy HH:mm:ss}."
                                    );
                        //RaiseError(true, 2064);
                    }

                    //Kiểm tra cac truong hop ko phai cap cuu
                    if (!Uti.CodeMatch(File.XML01.MA_LOAI_KCB.ToString(), "02"))
                    {
                        ////2897	3	XML3	MA_KHOA	[130] Sơn La: Bệnh nhân có y lệnh từ khoa Cấp cứu nhưng không chọn là BN Cấp cứu
                        ///
                        if (File.MA_KHOA == "K02")
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2897);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã khoa:{File.MA_KHOA} là khoa Cấp cứu nhưng không chọn loại KCB là Cấp cứu, Mã Loại KCB:{File.XML01.MA_LOAI_KCB}.";
                            //RaiseError(true, 2897, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2897", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                    , 2897
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2897, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã khoa:{File.MA_KHOA} là khoa Cấp cứu nhưng không chọn loại KCB là Cấp cứu, Mã Loại KCB:{File.XML01.MA_LOAI_KCB}."
                                    );

                            //RaiseError(true, 2897);
                        }
                        //1228	1	XML3	NGAY_YL	Thời gian y lệnh cận lâm sàng sau thời gian chỉ định thuốc
                        //1228  1   XML3    NGAY_YL     Thời gian y lệnh cận lâm sàng sau thời gian chỉ định thuốc
                        if (File.XML01.MA_LOAI_KCB == "01" && Uti.NumExists(File.MA_NHOM, 1, 2, 3))
                        {
                            if (File.XML01.XML02s.Any())
                            {
                                var MinYL_Xml2_CLS = File.XML01.XML02s.Min(x => x.NGAY_YL);

                                if (MinYL_Xml2_CLS != DateTime.MaxValue && File.NGAY_YL != null && File.NGAY_YL.HasValue && File.NGAY_YL.Value > MinYL_Xml2_CLS)
                                {
                                    //Linhist added 03/12/22
                                    var  minxmlthuoc = File.XML01.XML02s.Where(x => x.NGAY_YL == MinYL_Xml2_CLS).FirstOrDefault();
                                    // 1297   1   XML3 MA_DICH_VU  Cảnh báo khám lần thứ 2 trở lên không kê thuốc bảo hiểm y tế
                                    //haimaster 25.9.2025 đoạn code này chưa chuẩn, tạm chấp nhận nhưng nếu xem kỹ lại sẽ phải chỉnh sửa.
                                    if (File.XML01.XML03s.Count(x => x.MA_NHOM == 13) > 1)
                                    {
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1297);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có thời gian Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} sau thời gian Y Lệnh thuốc:{minxmlthuoc.MA_THUOC}, y lệnh lúc:{MinYL_Xml2_CLS:dd/MM/yyyy HH:mm:ss}.";
                                        //RaiseError(true, 1297, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1297", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true,
                                            1297,
                                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1297, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có thời gian Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} sau thời gian Y Lệnh thuốc:{minxmlthuoc.MA_THUOC}, y lệnh lúc:{MinYL_Xml2_CLS:dd/MM/yyyy HH:mm:ss}."
                                            );

                                        //RaiseError(true, 1297);
                                    }
                                    else
                                    {
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 1228);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có thời gian Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} sau thời gian Y Lệnh thuốc:{minxmlthuoc.MA_THUOC}, y lệnh lúc:{MinYL_Xml2_CLS:dd/MM/yyyy HH:mm:ss}.";
                                        //RaiseError(true, 1228, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "1228", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 1228
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(1228, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có thời gian Y Lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss} sau thời gian Y Lệnh thuốc:{minxmlthuoc.MA_THUOC}, y lệnh lúc:{MinYL_Xml2_CLS:dd/MM/yyyy HH:mm:ss}."
                                            );
                                        //RaiseError(true, 1228);
                                    }

                                }
                            }
                        }
                    }

                    //Kiem tra CCHN
                    var dmCungBS = dmBACSI.Where(x => x.MA_CSKCB == File.XML01.MA_CSKCB);
                    if (Field(x => x.MA_BAC_SI).NotEmpty())
                    {
                        //774 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
                        foreach (var bs in File.MA_BAC_SI.Split(";"))
                        {
                            if (string.IsNullOrEmpty(bs))
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 774);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã bác sĩ:{File.MA_BAC_SI} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy.";
                                //RaiseError(true, 774, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "774", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 774
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(774, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã bác sĩ:{File.MA_BAC_SI} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy."
                                    );
                                //RaiseError(true, 774);
                            }
                        }
                        if (File.MA_BAC_SI.Contains(";"))
                        {

                            if (!dmCungBS.Any(x => x.MA_CCHN == File.MA_BAC_SI))
                            {
                                string[] bacsis = File.MA_BAC_SI.Split(';');
                                //359	1	XML3	MA_BAC_SI	Sai thông tin mã bác sĩ, không nằm trong mã CCHN được cấp
                                //133	XML3	MA_BAC_SI	Mã bác sĩ không được để trống
                                //2352	0	XML3	MA_BAC_SI	[130] Mã bác sĩ (Mã BHXH) không khớp thông tin danh mục phê duyệt
                                foreach (var Ds in bacsis)
                                {
                                    if (Ds.Trim().Length == 0)
                                    {
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 133);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã bác sĩ:{Ds} Đang không có dữ liệu .";
                                        //RaiseError(true, 133, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "133", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 133
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(133, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã bác sĩ:{Ds} Đang không có dữ liệu ."
                                            );
                                        //RaiseError(true, 133);
                                    }
                                    //RaiseError(!dmCungBS.Any(x => x.MACCHN == Ds.ToString()), 359);
                                    if (!dmCungBS.Any(x => x.MA_CCHN == Ds.ToString()))
                                    {
                                        var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_CCHN).Contains(Uti.NormalizeUnicode(File.MA_BAC_SI))).Select(x => x.MA_CCHN).FirstOrDefault();
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 359);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                        //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                        //desc = desc + $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}";
                                        //RaiseError(true, 359, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "359", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 359
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(359, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                            (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                            $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}"
                                            );
                                    }
                                    //RaiseError(!dmCungBS.Any(x => x.MA_BHXH == Ds.ToString()), 2352);
                                    if (!dmCungBS.Any(x => x.MA_BHXH == Ds.ToString()))
                                    {
                                        var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(File.MA_BAC_SI))).Select(x => x.MA_BHXH).FirstOrDefault();
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2352);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                        //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                        //desc = desc + $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}";
                                        //RaiseError(true, 2352, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2352", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 2352
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2352, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                            (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                            $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}"
                                            );
                                    }
                                }
                            }
                        }
                        else
                        {
                            //RaiseError(!dmCungBS.Any(x => x.MACCHN == File.MA_BAC_SI), 359);
                            if(!dmCungBS.Any(x=> x.MA_CCHN == File.MA_BAC_SI))
                            {
                                var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_CCHN).Contains(Uti.NormalizeUnicode(File.MA_BAC_SI))).Select(x => x.MA_CCHN).FirstOrDefault();
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 359);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //desc = desc + $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}";
                                //RaiseError(true, 359, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "359", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 359
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(359, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                    (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                    $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}"
                                    );

                            }    
                            //RaiseError(!dmCungBS.Any(x => x.MA_BHXH == File.MA_BAC_SI), 2352);
                            if(!dmCungBS.Any(x => x.MA_BHXH == File.MA_BAC_SI))
                            {
                                var bsgandung = dmBACSI.Where(x => x.MA_BHXH !=null && Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(File.MA_BAC_SI))).Select(x => x.MA_BHXH).FirstOrDefault();
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2352);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //desc = desc + $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}";
                                //RaiseError(true, 2352, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2352", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2352
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2352, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ y lệnh:{File.MA_BAC_SI}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                    (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                    $"\n, Mã bác sĩ trong XML:{File.MA_BAC_SI}|đồ dài:{File.MA_BAC_SI.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_BAC_SI)}"
                                    );
                            }
                        }
                    }
                    if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 8, 18).Field(x => x.NGUOI_THUC_HIEN).NotEmpty())
                    {
                        //2080    4   XML3 NGUOI_THUC_HIEN[130] Trống mã người thực hiện, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy

                        if (File.NGUOI_THUC_HIEN.Trim() == ";")
                        {
                            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2080);
                            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //var desc = $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy.";
                            //RaiseError(true, 2080, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2080", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            RaiseError(true
                                , 2080
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2080, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy."
                                );
                            //RaiseError(true, 2080);
                        }
                        if (File.NGUOI_THUC_HIEN.Contains(";"))
                        {
                            var dataBS = dmBACSI.Where(x => x.MA_CCHN.Contains(";")).Select(x=>x.MA_CCHN).ToList();
                            string Bscochamphay="";
                            string Bskhongchamphay="";

                            if (dataBS!=null)
                            {
                                foreach(var bs in dataBS)
                                {
                                    if(File.NGUOI_THUC_HIEN.Contains(bs))
                                    {
                                        Bscochamphay = File.NGUOI_THUC_HIEN.Substring(File.NGUOI_THUC_HIEN.IndexOf(bs),bs.Length);
                                        Bskhongchamphay= File.NGUOI_THUC_HIEN.Substring(File.NGUOI_THUC_HIEN.IndexOf(bs), bs.Length).TrimStart(';');
                                        break;
                                    }    
                                }    
                                if(String.IsNullOrEmpty(Bscochamphay) && string.IsNullOrEmpty(Bskhongchamphay))
                                {
                                    foreach(var bs in File.NGUOI_THUC_HIEN.Split(";"))
                                    {
                                        if (!dmBACSI.Any(x => x.MA_CCHN == bs.ToString()))
                                        {
                                            var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(bs))).Select(x => x.MA_BHXH).FirstOrDefault();
                                            
                                            RaiseError(true
                                                , 2081
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2081, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{bs}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                                (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                                $"\n, Mã bác sĩ trong XML:{bs}|đồ dài:{bs.Length}|Unicode form:{Uti.GetUnicodeForm(bs)}"
                                                );
                                        }

                                    }
                                }
                                if(!String.IsNullOrEmpty(Bskhongchamphay))
                                {
                                    foreach (var bs in Bskhongchamphay.Split(";"))
                                    {
                                        if (!dmBACSI.Any(x => x.MA_CCHN == bs.ToString()))
                                        {
                                            var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(bs))).Select(x => x.MA_BHXH).FirstOrDefault();

                                            RaiseError(true
                                                , 2081
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2081, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{bs}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                                (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                                $"\n, Mã bác sĩ trong XML:{bs}|đồ dài:{bs.Length}|Unicode form:{Uti.GetUnicodeForm(bs)}"
                                                );
                                        }

                                    }
                                }
                                    
                                //if (BsInValid.Any())
                                //{
                                //    foreach (var ds in BsInValid)
                                //    {
                                //        if (ds.Trim().Length == 0)
                                //        {
                                //            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2079);
                                //            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã người thực hiện:{Ds} Đang không có dữ liệu .";
                                //            //RaiseError(true, 2079, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2079", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2079
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2079, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{ds} Đang không có dữ liệu ."
                                //                );
                                //            //RaiseError(true, 2079);
                                //            //desc = "";
                                //            //desc = $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy.";
                                //            //RaiseError(true, 2080, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2080", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2080
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2080, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy."
                                //                );
                                //            //RaiseError(true, 2080);
                                //        }
                                //        if (!dmBACSI.Any(x => x.MACCHN == ds.ToString()))
                                //        {
                                //            var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(ds))).Select(x => x.MA_BHXH).FirstOrDefault();
                                //            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2081);
                                //            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //            //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //            //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //            //desc = desc + $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}";
                                //            //RaiseError(true, 2081, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2081", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2081
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2081, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                //                (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                //                $"\n, Mã bác sĩ trong XML:{ds}|đồ dài:{ds.Length}|Unicode form:{Uti.GetUnicodeForm(ds)}"
                                //                );
                                //        }
                                //        //RaiseError(!dmCungBS.Any(x => x.MACCHN == Ds.ToString()), 2081);
                                //        if (!dmBACSI.Any(x => x.MA_BHXH == ds.ToString()))
                                //        {
                                //            var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(ds))).Select(x => x.MA_BHXH).FirstOrDefault();
                                //            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2353);
                                //            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //            //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //            //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //            //desc = desc + $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}";
                                //            //RaiseError(true, 2353, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2353", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2353
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2353, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                //                (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                //                $"\n, Mã bác sĩ trong XML:{ds}|đồ dài:{ds.Length}|Unicode form:{Uti.GetUnicodeForm(ds)}"
                                //                );

                                //        }
                                //    }
                                //}
                                //if (BsValid.Any())
                                //{
                                //    foreach (var ds in BsValid)
                                //    {
                                //        if (ds.Trim().Length == 0)
                                //        {
                                //            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2079);
                                //            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //            //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã người thực hiện:{Ds} Đang không có dữ liệu .";
                                //            //RaiseError(true, 2079, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2079", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2079
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2079, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{ds} Đang không có dữ liệu ."
                                //                );
                                //            //RaiseError(true, 2079);
                                //            //desc = "";
                                //            //desc = $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy.";
                                //            //RaiseError(true, 2080, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2080", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2080
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2080, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy."
                                //                );
                                //            //RaiseError(true, 2080);
                                //        }
                                //        if (!dmBACSI.Any(x => x.MACCHN == ds.ToString()))
                                //        {
                                //            var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(ds))).Select(x => x.MA_BHXH).FirstOrDefault();
                                //            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2081);
                                //            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //            //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //            //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //            //desc = desc + $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}";
                                //            //RaiseError(true, 2081, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2081", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2081
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2081, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                //                (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                //                $"\n, Mã bác sĩ trong XML:{ds}|đồ dài:{ds.Length}|Unicode form:{Uti.GetUnicodeForm(ds)}"
                                //                );
                                //        }
                                //        //RaiseError(!dmCungBS.Any(x => x.MACCHN == Ds.ToString()), 2081);
                                //        if (!dmBACSI.Any(x => x.MA_BHXH == ds.ToString()))
                                //        {
                                //            var bsgandung = dmBACSI.Where(x => Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(ds))).Select(x => x.MA_BHXH).FirstOrDefault();
                                //            //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2353);
                                //            //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //            //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //            //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //            //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //            //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //            //desc = desc + $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}";
                                //            //RaiseError(true, 2353, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2353", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                //            RaiseError(true
                                //                , 2353
                                //                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                //                $"Nội dung lỗi: {(errorlist.TryGetValue(2353, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                //                $"Chi tiết:" +
                                //                $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                //                (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                //                $"\n, Mã bác sĩ trong XML:{ds}|đồ dài:{ds.Length}|Unicode form:{Uti.GetUnicodeForm(ds)}"
                                //                );

                                //        }
                                //    }
                                //}
                            }    

                            if (dataBS==null)
                            {

                                string[] ngthuchien = File.NGUOI_THUC_HIEN.Split(";");
                                //2079	4	XML3	NGUOI_THUC_HIEN	[130] Mã CCHN người thực hiện không được để trống
                                //2081    4   XML3 NGUOI_THUC_HIEN[130] CCHN người thực hiện không có trong danh mục phê duyệt
                                //2353	0	XML3	NGUOI_THUC_HIEN	[130] Mã người thực hiện (Mã BHXH) không khớp thông tin danh mục phê duyệt
                                foreach (var Ds in ngthuchien)
                                {
                                    if (Ds.Trim().Length == 0)
                                    {
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2079);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã người thực hiện:{Ds} Đang không có dữ liệu .";
                                        //RaiseError(true, 2079, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2079", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 2079
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2079, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{Ds} Đang không có dữ liệu ."
                                            );
                                        //RaiseError(true, 2079);
                                        //desc = "";
                                        //desc = $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy.";
                                        //RaiseError(true, 2080, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2080", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 2080
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2080, out var Erinfo1) ? Erinfo1 : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy."
                                            );
                                        //RaiseError(true, 2080);
                                    }
                                    if(!dmBACSI.Any(x => x.MA_CCHN == Ds.ToString()))
                                    {
                                        var bsgandung = dmBACSI.Where(x => x.MA_BHXH != null && Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(Ds))).Select(x => x.MA_BHXH).FirstOrDefault();
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2081);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                        //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                        //desc = desc + $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}";
                                        //RaiseError(true, 2081, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2081", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 2081
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2081, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                            (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                            $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}"
                                            );
                                    }
                                    //RaiseError(!dmCungBS.Any(x => x.MACCHN == Ds.ToString()), 2081);
                                    if(!dmBACSI.Any(x => x.MA_BHXH == Ds.ToString()))
                                    {
                                        var bsgandung = dmBACSI.Where(x => x.MA_BHXH != null && Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(Ds))).Select(x => x.MA_BHXH).FirstOrDefault();
                                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2353);
                                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                        //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                        //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                        //desc = desc + $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}";
                                        //RaiseError(true, 2353, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2353", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                        RaiseError(true
                                            , 2353
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2353, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{Ds}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                            (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                            $"\n, Mã bác sĩ trong XML:{Ds}|đồ dài:{Ds.Length}|Unicode form:{Uti.GetUnicodeForm(Ds)}"
                                            );

                                    }
                                    //RaiseError(!dmCungBS.Any(x => x.MA_BHXH == Ds.ToString()), 2353);
                                }
                            }
                        }
                        else
                        {
                            if(!dmCungBS.Any(x => x.MA_CCHN == File.NGUOI_THUC_HIEN))
                            {
                                var bsgandung = dmBACSI.Where(x => x.MA_BHXH != null && Uti.NormalizeUnicode(x.MA_CCHN).Contains(Uti.NormalizeUnicode(File.NGUOI_THUC_HIEN))).Select(x => x.MA_CCHN).FirstOrDefault();
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2081);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{File.NGUOI_THUC_HIEN}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //desc = desc + $"\n, Mã bác sĩ trong XML:{File.NGUOI_THUC_HIEN}|đồ dài:{File.NGUOI_THUC_HIEN.Length}|Unicode form:{Uti.GetUnicodeForm(File.NGUOI_THUC_HIEN)}";
                                //RaiseError(true, 2081, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2081", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2081
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2081, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{File.NGUOI_THUC_HIEN}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                    //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                    (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                    $"\n, Mã bác sĩ trong XML:{File.NGUOI_THUC_HIEN}|đồ dài:{File.NGUOI_THUC_HIEN.Length}|Unicode form:{Uti.GetUnicodeForm(File.NGUOI_THUC_HIEN)}"
                                    );
                            }
                            //RaiseError(!dmCungBS.Any(x => x.MACCHN == File.NGUOI_THUC_HIEN), 2081);
                            if(!dmCungBS.Any(x => x.MA_BHXH == File.NGUOI_THUC_HIEN))
                            {
                                var bsgandung = dmBACSI.Where(x => x.MA_BHXH != null && Uti.NormalizeUnicode(x.MA_BHXH).Contains(Uti.NormalizeUnicode(File.NGUOI_THUC_HIEN))).Select(x => x.MA_BHXH).FirstOrDefault();
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2353);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //string desc = $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{File.NGUOI_THUC_HIEN}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ";
                                //desc = desc + (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "");
                                //desc = desc + $"\n, Mã bác sĩ trong XML:{File.NGUOI_THUC_HIEN}|đồ dài:{File.NGUOI_THUC_HIEN.Length}|Unicode form:{Uti.GetUnicodeForm(File.NGUOI_THUC_HIEN)}";
                                //RaiseError(true, 2353, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2353", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2353
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2353, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được y:{File.MA_DICH_VU}|{File.MA_VAT_TU}, Mã bác sĩ TH y lệnh:{File.NGUOI_THUC_HIEN}, Không tìm thấy mã bác sĩ trong danh mục phê duyệt, kiểm tra lại thông tin mã bác sĩ" +
                                    (bsgandung != null ? $"\n Mã bác sĩ đúng có thể là:{bsgandung}|đồ dài:{bsgandung.Length}|Unicode form:{Uti.GetUnicodeForm(bsgandung)}" : "") +
                                    $"\n, Mã bác sĩ trong XML:{File.NGUOI_THUC_HIEN}|đồ dài:{File.NGUOI_THUC_HIEN.Length}|Unicode form:{Uti.GetUnicodeForm(File.NGUOI_THUC_HIEN)}"
                                    );
                            }
                            //RaiseError(!dmCungBS.Any(x => x.MA_BHXH == File.NGUOI_THUC_HIEN), 2353);

                        }
                        //2685	3	XML3	NGUOI_THUC_HIEN	[130] Người thực hiện tối thiểu phải là 2 người
                        if (Field(x => x.MA_NHOM).HasValue(2, 3))
                        {
                            string[] ngthuchien = File.NGUOI_THUC_HIEN.Split(';');
                            if(ngthuchien.Length<2)
                            {
                                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2685);
                                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy.";
                                //RaiseError(true, 2685, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2685", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                RaiseError(true
                                    , 2685
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Chi tiết:" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2685, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có mã người thực hiện:{File.NGUOI_THUC_HIEN} không đúng định dạng, hiển thị cách nhau bằng hai ký tự dấu chấm phẩy."
                                    );

                                // RaiseError(ngthuchien.Length < 2, 2685);
                            }    
                            
                        }

                    }
                }

                if (Field(x => x.MA_NHOM).HasValue(10, 11))
                {
                    try
                    {
                        //Linhist changed 091026
                        //if (!cungMAVATTUs.Any())
                        //{
                        //    if (File.MA_VAT_TU != null)
                        //    {
                        //        var mavattugandung = dmVTYT.Where(x => x.MA_VAT_TU != null && Uti.NormalizeUnicode(x.MA_VAT_TU).Contains(Uti.NormalizeUnicode(File.MA_VAT_TU.Trim().ToLower()))).Select(x => x.MA_VAT_TU).FirstOrDefault();
                        //        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 205);
                        //        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã vật tư:{File.MA_VAT_TU} không tìm thấy trong danh mục phê duyệt.";
                        //        //desc = desc + (mavattugandung != null ? $"\n Mã vật tư đúng có thể là:{mavattugandung}|đồ dài:{mavattugandung.Length}|Unicode form:{Uti.GetUnicodeForm(mavattugandung)}" : "");
                        //        //desc = desc + $"\n, Mã vật tư trong XML:{File.MA_VAT_TU}|đồ dài:{File.MA_VAT_TU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_VAT_TU)}";
                        //        //RaiseError(true, 205, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "205", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        //        RaiseError(true
                        //            , 205
                        //            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //            $"Nội dung lỗi: {(errorlist.TryGetValue(205, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //            $"Chi tiết:" +
                        //            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã vật tư:{File.MA_VAT_TU} không tìm thấy trong danh mục phê duyệt." +
                        //            (mavattugandung != null ? $"\n Mã vật tư đúng có thể là:{mavattugandung}|đồ dài:{mavattugandung.Length}|Unicode form:{Uti.GetUnicodeForm(mavattugandung)}" : "") +
                        //            $"\n, Mã vật tư trong XML:{File.MA_VAT_TU}|đồ dài:{File.MA_VAT_TU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_VAT_TU)}"
                        //            );
                        //        //RaiseError(true, 205);
                        //    } 
                        //}
                        //else
                        //{
                        //    if (cungMAVATTUs.Count() == 0 )
                        //    {
                        //        var mavattugandung = dmVTYT.Where(x => Uti.NormalizeUnicode(x.MA_VAT_TU).Contains(Uti.NormalizeUnicode(File.MA_VAT_TU.Trim().ToLower()))).Select(x => x.MA_VAT_TU).FirstOrDefault();
                        //        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 205);
                        //        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã vật tư:{File.MA_VAT_TU} không tìm thấy trong danh mục phê duyệt.";
                        //        //desc = desc + (mavattugandung != null ? $"\n Mã vật tư đúng có thể là:{mavattugandung}|đồ dài:{mavattugandung.Length}|Unicode form:{Uti.GetUnicodeForm(mavattugandung)}" : "");
                        //        //desc = desc + $"\n, Mã vật tư trong XML:{File.MA_VAT_TU}|đồ dài:{File.MA_VAT_TU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_VAT_TU)}";
                        //        //RaiseError(true, 205, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "205", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        //        RaiseError(true
                        //            , 205
                        //            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //            $"Nội dung lỗi: {(errorlist.TryGetValue(205, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //            $"Chi tiết:" +
                        //            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã vật tư:{File.MA_VAT_TU} không tìm thấy trong danh mục phê duyệt."
                        //            + (mavattugandung != null ? $"\n Mã vật tư đúng có thể là:{mavattugandung}|đồ dài:{mavattugandung.Length}|Unicode form:{Uti.GetUnicodeForm(mavattugandung)}" : "") +
                        //            $"\n, Mã vật tư trong XML:{File.MA_VAT_TU}|đồ dài:{File.MA_VAT_TU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_VAT_TU)}"
                        //            );
                        //        //RaiseError(true, 205);
                        //    }
                        //    else
                        //    {
                        //        //RaiseError(!cungMAVATTUs.Any(), 205);
                        //        //2320	2320	0	XML3	MA_VAT_TU	[130] Mã vật tư thừa khoảng trắng (Dấu cách)
                        //        if (!cungMAVATTUs.Any())
                        //        {
                        //            RaiseError(dmVTYT.Any(x => x.MA_VAT_TU == File.MA_VAT_TU.Trim()), 2320);
                        //        }
                        //        if (cungMAVATTUs.Any())
                        //        {
                        //            //185	1	XML3	TEN_VAT_TU	Tên vật tư không khớp với tên vật tư trong danh mục phê duyệt
                        //            //2177	4	XML3	TEN_VAT_TU	[130] Tên vật tư thừa khoảng trắng (Dấu cách), có độ dài không đúng độ dài trong danh mục phê duyệt
                        //            //Linhist changed 16/06/26
                        //            //RaiseError(!cungMAVATTUs.Any(t => Uti.IsEqual(t.TEN_VAT_TU, Regex.Replace(File.TEN_VAT_TU, @"\[\s*.*?\s*\]", ""))), 185);
                        //            //RaiseError(!cungMAVATTUs.Any(t => t.TEN_VAT_TU.Length == Regex.Replace(File.TEN_VAT_TU, @"\[\s*.*?\s*\]", "").Length), 2177);
                        //            RaiseError(!cungMAVATTUs.Any(t => Uti.IsEqual(t.TEN_VAT_TU, File.TEN_VAT_TU, ignoreBracket: true)), 185);
                        //            RaiseError(!cungMAVATTUs.Any(t => Uti.ReplaceBracketSpace(t.TEN_VAT_TU).Length == Uti.ReplaceBracketSpace(File.TEN_VAT_TU).Length), 2177);

                        //            //2922	3	XML3	MA_HIEU_SP	[130] Thái Bình: Mã hiệu Sản phẩm bắt buộc phải nhập với một số Vật tư có mã hiệu sản phẩm
                        //            Field(x => x.MA_HIEU_SP).IsEmpty().ThenError(2922);

                        //            //2106	4	XML3	MA_HIEU_SP	[130] Mã hiệu sản phẩm không đúng trong Danh mục phê duyệt
                        //            if (Field(x => x.MA_HIEU_SP).NotEmpty())
                        //            {
                        //                RaiseError(!cungMAVATTUs.Any(t => t.MA_HIEU == File.MA_HIEU_SP), 2106);
                        //            }
                        //            //2107    4   XML3 TT_THAU[130] Thông tin thầu không khớp với Thông tin thầu vật tư được phê duyệt
                        //            //RaiseError(!cungMAVATTUs.Any(t => TT_THAU_RT(t.TT_THAU) == File.TT_THAU), 2107);

                        //            RaiseError(!cungMAVATTUs.Any(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU), 2107);
                        //            //2392	4	XML3	TT_THAU	[130] Thông tin thầu khớp trong Danh mục phê duyệt, tuy nhiên không đúng định dạng 4750, các anh chị lưu ý
                        //            //2359    0   XML3 TT_THAU[130] Thông tin thầu không đúng định dạng
                        //            if (cungMAVATTUs.Any(t => !string.IsNullOrEmpty(t.TT_THAU.Trim()) && cut_TT_VT(t.TT_THAU) == File.TT_THAU))
                        //            {
                        //                RaiseError(!Check_TT_THAU(File.TT_THAU), 2359);
                        //            }
                        //            else
                        //            {
                        //                RaiseError(!Check_TT_THAU(File.TT_THAU), 2392);
                        //            }

                        //            //4001	3	XML3	DON_GIA_BH	[3176] Đơn giá (Vật tư) lớn hơn với đơn giá trong Danh mục phê duyệt
                        //            //4019	3	XML3	DON_GIA_BH	[3176] Đơn giá (Vật tư) nhỏ hơn với đơn giá trong Danh mục phê duyệt
                        //            var maxDonGia = cungMAVATTUs
                        //                            .Where(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU
                        //                             /*&& t.DON_GIA == File.DON_GIA_BH*/)
                        //                            .Max(t => t.DON_GIA);

                        //            // Lấy giá trị nhỏ nhất DON_GIA
                        //            var minDonGia = cungMAVATTUs
                        //                                .Where(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU
                        //                                /*&& t.DON_GIA == File.DON_GIA_BH*/)
                        //                                .Min(t => t.DON_GIA);
                        //            if (!cungMAVATTUs.Any(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU && t.DON_GIA == File.DON_GIA_BH))
                        //            {
                        //                if (maxDonGia!=null && File.DON_GIA_BH > maxDonGia)
                        //                {
                        //                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 4001);
                        //                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //                    //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} lớn hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {maxDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư.";
                        //                    //RaiseError(true, 4001, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "4001", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        //                    RaiseError(true
                        //                       , 4001
                        //                       , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                       $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                       $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                       $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                       $"Chi tiết:" +
                        //                       $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} lớn hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {maxDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư."
                        //                       );
                        //                    //RaiseError(true , 4001, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                    //                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                    //                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                    //                        $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                    //                        $"Chi tiết:" +
                        //                    //                        $"Đơn giá: {File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} không khớp với đơn giá trong DM Vật tư được Bảo hiểm phê duyệt!"
                        //                    //);
                        //                }
                        //                if (minDonGia!=null && File.DON_GIA_BH < minDonGia)
                        //                {
                        //                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 4001);
                        //                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //                    //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} lớn hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {maxDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư.";
                        //                    //RaiseError(true, 4001, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "4001", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        //                    RaiseError(true
                        //                       , 4019
                        //                       , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                       $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                       $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                       $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                       $"Chi tiết:" +
                        //                       $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} nhỏ hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {minDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư."
                        //                       );
                        //                    //RaiseError(true , 4001, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                    //                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                    //                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                    //                        $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                    //                        $"Chi tiết:" +
                        //                    //                        $"Đơn giá: {File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} không khớp với đơn giá trong DM Vật tư được Bảo hiểm phê duyệt!"
                        //                    //);
                        //                }

                        //            }
                        //        }
                        //    }
                        //}

                        if (cungMAVATTUs.Any())
                        {
                            //185	1	XML3	TEN_VAT_TU	Tên vật tư không khớp với tên vật tư trong danh mục phê duyệt
                            //2177	4	XML3	TEN_VAT_TU	[130] Tên vật tư thừa khoảng trắng (Dấu cách), có độ dài không đúng độ dài trong danh mục phê duyệt
                            //Linhist changed 16/06/26
                            //RaiseError(!cungMAVATTUs.Any(t => Uti.IsEqual(t.TEN_VAT_TU, Regex.Replace(File.TEN_VAT_TU, @"\[\s*.*?\s*\]", ""))), 185);
                            //RaiseError(!cungMAVATTUs.Any(t => t.TEN_VAT_TU.Length == Regex.Replace(File.TEN_VAT_TU, @"\[\s*.*?\s*\]", "").Length), 2177);
                            RaiseError(!cungMAVATTUs.Any(t => Uti.IsEqual(t.TEN_VAT_TU, File.TEN_VAT_TU, ignoreBracket: true)), 185);
                            RaiseError(!cungMAVATTUs.Any(t => Uti.ReplaceBracketSpace(t.TEN_VAT_TU).Length == Uti.ReplaceBracketSpace(File.TEN_VAT_TU).Length), 2177);

                            //2922	3	XML3	MA_HIEU_SP	[130] Thái Bình: Mã hiệu Sản phẩm bắt buộc phải nhập với một số Vật tư có mã hiệu sản phẩm
                            Field(x => x.MA_HIEU_SP).IsEmpty().ThenError(2922);

                            //2106	4	XML3	MA_HIEU_SP	[130] Mã hiệu sản phẩm không đúng trong Danh mục phê duyệt
                            if (Field(x => x.MA_HIEU_SP).NotEmpty())
                            {
                                RaiseError(!cungMAVATTUs.Any(t => t.MA_HIEU == File.MA_HIEU_SP), 2106);
                            }
                            //2107    4   XML3 TT_THAU[130] Thông tin thầu không khớp với Thông tin thầu vật tư được phê duyệt
                            //RaiseError(!cungMAVATTUs.Any(t => TT_THAU_RT(t.TT_THAU) == File.TT_THAU), 2107);

                            RaiseError(!cungMAVATTUs.Any(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU), 2107);
                            //2392	4	XML3	TT_THAU	[130] Thông tin thầu khớp trong Danh mục phê duyệt, tuy nhiên không đúng định dạng 4750, các anh chị lưu ý
                            //2359    0   XML3 TT_THAU[130] Thông tin thầu không đúng định dạng
                            if (cungMAVATTUs.Any(t => !string.IsNullOrEmpty(t.TT_THAU.Trim()) && cut_TT_VT(t.TT_THAU) == File.TT_THAU))
                            {
                                RaiseError(!Check_TT_THAU(File.TT_THAU), 2359);
                            }
                            else
                            {
                                RaiseError(!Check_TT_THAU(File.TT_THAU), 2392);
                            }

                            //4001	3	XML3	DON_GIA_BH	[3176] Đơn giá (Vật tư) lớn hơn với đơn giá trong Danh mục phê duyệt
                            //4019	3	XML3	DON_GIA_BH	[3176] Đơn giá (Vật tư) nhỏ hơn với đơn giá trong Danh mục phê duyệt
                            if (!cungMAVATTUs.Any(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU && t.DON_GIA_BH == File.DON_GIA_BH))
                            {
                                var maxDonGia = cungMAVATTUs.Where(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU/*&& t.DON_GIA == File.DON_GIA_BH*/)
                                                .Max(t => t.DON_GIA_BH);

                                if (maxDonGia != null && File.DON_GIA_BH > maxDonGia)
                                {
                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 4001);
                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                    //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} lớn hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {maxDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư.";
                                    //RaiseError(true, 4001, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "4001", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                    RaiseError(true
                                       , 4001
                                       , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                       $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                       $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                       $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                       $"Chi tiết:" +
                                       $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} lớn hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {maxDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư."
                                       );
                                    //RaiseError(true , 4001, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    //                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    //                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    //                        $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    //                        $"Chi tiết:" +
                                    //                        $"Đơn giá: {File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} không khớp với đơn giá trong DM Vật tư được Bảo hiểm phê duyệt!"
                                    //);
                                }

                                // Lấy giá trị nhỏ nhất DON_GIA
                                var minDonGia = cungMAVATTUs.Where(t => cut_TT_VT(t.TT_THAU) == File.TT_THAU/*&& t.DON_GIA == File.DON_GIA_BH*/)
                                                    .Min(t => t.DON_GIA_BH);
                                if (minDonGia != null && File.DON_GIA_BH < minDonGia)
                                {
                                    //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 4001);
                                    //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                                    //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                                    //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                                    //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} lớn hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {maxDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư.";
                                    //RaiseError(true, 4001, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "4001", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                                    RaiseError(true
                                       , 4019
                                       , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                       $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                       $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                       $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                       $"Chi tiết:" +
                                       $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có đơn giá BH:{File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} nhỏ hơn đơn giá trong DM Vật tư được Bảo hiểm phê duyệt là {minDonGia}, kiểm tra lại thông tin đơn giá BH của vật tư."
                                       );
                                    //RaiseError(true , 4001, $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    //                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    //                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    //                        $"Nội dung lỗi: {(errorlist.TryGetValue(4001, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    //                        $"Chi tiết:" +
                                    //                        $"Đơn giá: {File.DON_GIA_BH} của vật tư: {File.MA_VAT_TU} không khớp với đơn giá trong DM Vật tư được Bảo hiểm phê duyệt!"
                                    //);
                                }

                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(File.MA_VAT_TU))
                            {
                                //2320	2320	0	XML3	MA_VAT_TU	[130] Mã vật tư thừa khoảng trắng (Dấu cách)
                                RaiseError(dmVTYT.Any(x => x.MA_VAT_TU == File.MA_VAT_TU.Trim()), 2320);

                                //205 1   XML3 MA_VAT_TU   Mã vật tư không tìm thấy trong danh mục phê duyệt
                                var mavattugandung = dmVTYT.Where(x => x.MA_VAT_TU != null && Uti.NormalizeUnicode(x.MA_VAT_TU).Contains(Uti.NormalizeUnicode(File.MA_VAT_TU.Trim().ToLower()))).Select(x => x.MA_VAT_TU).FirstOrDefault();
                                RaiseError(true
                                    , 205
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(205, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã vật tư:{File.MA_VAT_TU} không tìm thấy trong danh mục phê duyệt." +
                                    (mavattugandung != null ? $"\n Mã vật tư đúng có thể là:{mavattugandung}|đồ dài:{mavattugandung.Length}|Unicode form:{Uti.GetUnicodeForm(mavattugandung)}" : "") +
                                    $"\n, Mã vật tư trong XML:{File.MA_VAT_TU}|đồ dài:{File.MA_VAT_TU.Length}|Unicode form:{Uti.GetUnicodeForm(File.MA_VAT_TU)}"
                                    );
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: MaBN: {File.XML01.MA_BN} MaCSKCB: {File.XML01.MA_CSKCB}; STT Bị Lỗi: {File.STT} XML3 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
                    }
                }


                //_wa.Stop();
                //if (_wa.Elapsed.TotalSeconds > .3d)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - thông tin cơ bản, thời gian: {_wa.Elapsed.TotalSeconds.ToString()}", nameof(Validate));
                //}
                //_wa.Restart();
                #endregion


                #region kiểm tra tiền XML3
                //Kiem tra tien
                //2083	4	XML3	THANH_TIEN_BV	[130] Thành tiền Bệnh viện khác Số lượng* Đơn giá
                if (Field(x => x.THANH_TIEN_BV).NotEmpty().Field(x => x.SO_LUONG).NotEmpty().Field(x => x.DON_GIA_BV).NotEmpty().Field(x => x.TYLE_TT_DV).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    //double thanhtienBV = Math.Round(File.SO_LUONG.Value * File.DON_GIA_BV.Value * File.TYLE_TT_DV.Value / 100, 2);
                    //if(Math.Round((decimal)File.THANH_TIEN_BV,2)!=Math.Round((decimal)thanhtienBV,2))
                    var thanhtienBV = Uti.RoundUp((decimal)File.SO_LUONG.Value * (decimal)File.DON_GIA_BV.Value * (decimal)File.TYLE_TT_DV.Value / 100m);
                    if (!Uti.IsEqualUp((decimal?)File.THANH_TIEN_BV, thanhtienBV))
                    {
                        //var error = dmError130.FirstOrDefault(x => x.ErrorCode == 2083);
                        //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                        //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                        //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                        //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có số lượng:{File.SO_LUONG}, đơn giá BV:{File.DON_GIA_BV}, tỷ lệ thanh toán DV:{File.TYLE_TT_DV}%, thành tiền BV trong XML3:{File.THANH_TIEN_BV} không đúng công thức tính Số lượng* Đơn giá* Tỷ lệ thanh toán dịch vụ/100={thanhtienBV}";
                        //RaiseError(true, 2083, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "2083", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                        RaiseError(true
                            , 2083
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2083, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có số lượng:{File.SO_LUONG}, đơn giá BV:{File.DON_GIA_BV}, tỷ lệ thanh toán DV:{File.TYLE_TT_DV}%, thành tiền BV trong XML3:{File.THANH_TIEN_BV} không đúng công thức tính Số lượng* Đơn giá* Tỷ lệ thanh toán dịch vụ/100={thanhtienBV}"
                            );
                        //Field(x => x.THANH_TIEN_BV).NotValue(thanhtienBV).ThenError(2083);
                    }
                    
                }

                //2084	4	XML3	THANH_TIEN_BH	[130] Thành tiền Bảo hiểm khác Số lượng * Đơn giá * Tỷ lệ Bảo hiểm /100
                //2085    4   XML3 THANH_TIEN_BH    [130] Thành tiền Bảo hiểm khác Số lượng* Đơn giá* Tỷ lệ Bảo hiểm / 100 * tỷ lệ thanh toán dịch vụ / 100
                if (Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.SO_LUONG).NotEmpty().Field(x => x.DON_GIA_BH).NotEmpty().Field(x => x.TYLE_TT_BH).NotEmpty())
                {
                    //Linhist changed 091026
                    //if (Field(x => x.TYLE_TT_DV).IsOver(0))
                    if (File.TYLE_TT_DV > 0)
                    {
                        //Linhist changed 26/06/26
                        //double thanhtienBH = Math.Round(File.SO_LUONG.Value * File.DON_GIA_BH.Value * File.TYLE_TT_BH.Value / 100 * File.TYLE_TT_DV.Value / 100, 2);
                        //if (Math.Round((decimal)File.THANH_TIEN_BH,2)!= Math.Round((decimal)thanhtienBH,2))
                        var thanhtienBH = Uti.RoundUp((decimal)File.SO_LUONG.Value * (decimal)File.DON_GIA_BH.Value * (decimal)File.TYLE_TT_BH.Value / 100m * (decimal)File.TYLE_TT_DV.Value / 100m, 2);
                        if (!Uti.IsEqualUp((decimal?)File.THANH_TIEN_BH, thanhtienBH))
                        {
                            RaiseError(true
                                , 2085
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2085, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có số lượng:{File.SO_LUONG}, đơn giá BH:{File.DON_GIA_BH}, tỷ lệ thanh toán BH:{File.TYLE_TT_BH}%, tỷ lệ thanh toán DV:{File.TYLE_TT_DV}%, thành tiền BH trong XML3:{File.THANH_TIEN_BH} không đúng công thức tính Số lượng* Đơn giá* Tỷ lệ Bảo hiểm / 100 * tỷ lệ thanh toán dịch vụ / 100={thanhtienBH}"
                                );
                            //Field(x => x.THANH_TIEN_BH).NotValue(thanhtienBH).ThenError(2085);
                        }
                    }
                    //Linhist changed 091026
                    //if (Field(x => x.TYLE_TT_DV).Equals(0))
                    if (File.TYLE_TT_DV == 0)
                    {
                        //Linhist changed 26/06/26
                        //double thanhtienBH = Math.Round(File.SO_LUONG.Value * File.DON_GIA_BH.Value * File.TYLE_TT_BH.Value / 100, 2);
                        //if(Math.Round((decimal)File.THANH_TIEN_BH,2)!=Math.Round((decimal)thanhtienBH,2))
                        var thanhtienBH = Uti.RoundUp((decimal)File.SO_LUONG.Value * (decimal)File.DON_GIA_BH.Value * (decimal)File.TYLE_TT_BH.Value / 100m, 2);
                        if(!Uti.IsEqualUp((decimal?)File.THANH_TIEN_BH, thanhtienBH))
                        {
                            RaiseError(true
                                , 2084
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2084, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có số lượng:{File.SO_LUONG}, đơn giá BH:{File.DON_GIA_BH}, tỷ lệ thanh toán BH:{File.TYLE_TT_BH}%, thành tiền BH trong XML3:{File.THANH_TIEN_BH} không đúng công thức tính Số lượng* Đơn giá* Tỷ lệ Bảo hiểm / 100={thanhtienBH}"
                                );

                        }
                        //Field(x => x.THANH_TIEN_BH).NotValue(thanhtienBH).ThenError(2084);
                    }

                }
                //2086	4	XML3	T_NGUONKHAC	[130] Tiền nguồn khác sai công thức tính trong 4750
                if (Field(x => x.T_NGUONKHAC).NotEmpty().Field(x => x.T_NGUONKHAC_NSNN).NotEmpty().Field(x => x.T_NGUONKHAC_VTNN).NotEmpty().Field(x => x.T_NGUONKHAC_VTTN).NotEmpty().Field(x => x.T_NGUONKHAC_CL).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    //double tnguonkhac = Math.Round(File.T_NGUONKHAC_NSNN.Value + File.T_NGUONKHAC_VTTN.Value + File.T_NGUONKHAC_VTNN.Value + File.T_NGUONKHAC_CL.Value, 2);
                    //if(Math.Round((decimal)File.T_NGUONKHAC, 2) != Math.Round((decimal)tnguonkhac, 2))
                    var tnguonkhac = Uti.RoundUp((decimal)File.T_NGUONKHAC_NSNN.Value + (decimal)File.T_NGUONKHAC_VTTN.Value + (decimal)File.T_NGUONKHAC_VTNN.Value + (decimal)File.T_NGUONKHAC_CL.Value);
                    if (!Uti.IsEqualUp((decimal?)File.T_NGUONKHAC, tnguonkhac))
                    {
                        RaiseError(true
                            , 2086
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2086, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền nguồn khác trong XML3:{File.T_NGUONKHAC} không đúng công thức tính Tổng tiền nguồn khác = Tiền nguồn khác NSNN + Tiền nguồn khác VTTN + Tiền nguồn khác VTNN + Tiền nguồn khác CL={tnguonkhac}"
                            );
                    }
                    //Field(x => x.T_NGUONKHAC).NotValue(tnguonkhac).ThenError(2086);

                }
                //2087	4	XML3	T_BNTT	[130] Tiền Bệnh nhân thanh toán bị sai, xem lại công thức
                if (Field(x => x.THANH_TIEN_BV).NotEmpty().Field(x => x.THANH_TIEN_BH).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    //double t_bntt = Math.Round(File.THANH_TIEN_BV.Value - File.THANH_TIEN_BH.Value, 2);
                    var t_bntt = Uti.RoundUp((decimal)File.THANH_TIEN_BV.Value - (decimal)File.THANH_TIEN_BH.Value);
                    if (File.T_NGUONKHAC.Value != 0)
                    {
                        //Linhist changed 26/06/26
                        //if (File.T_NGUONKHAC.Value < t_bntt)
                        //{
                        //    t_bntt = Math.Round(t_bntt - File.T_NGUONKHAC.Value, 2);
                        //}
                        //else if (File.T_NGUONKHAC > t_bntt)
                        //{
                        //    t_bntt = 0;
                        //}
                        if (Uti.IsUnderUp((decimal?)File.T_NGUONKHAC, t_bntt))
                        {
                            t_bntt = Uti.RoundUp(t_bntt - (decimal)File.T_NGUONKHAC.Value);
                        }
                        else if (Uti.IsOverUp((decimal?)File.T_NGUONKHAC, t_bntt))
                        {
                            t_bntt = 0;
                        }
                    }

                    //Linhist changed 26/06/26
                    //if (Math.Round((decimal)File.T_BNTT, 2) != Math.Round((decimal)t_bntt, 2))
                    if (!Uti.IsEqualUp((decimal?)File.T_BNTT, t_bntt))
                    {
                        RaiseError(true
                            , 2087
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2087, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền Bệnh nhân thanh toán trong XML3:{File.T_BNTT} không đúng công thức tính Thành tiền BV - Thành tiền BH - Tiền nguồn khác = Tiền BN thanh toán={t_bntt}"
                            );
                    }
                    //Field(x => x.T_BNTT).NotValue(t_bntt).ThenError(2087);

                }
                //2689	3	XML3	T_BNTT	[3176] XML3 T_BNTT sai so với công thức của 3176

                if (Field(x => x.THANH_TIEN_BV).NotEmpty().Field(x => x.THANH_TIEN_BH).NotEmpty())
                {
                    //Linhist changed 26/06/26
                    //double t_bntt = Math.Round(File.THANH_TIEN_BV.Value - File.THANH_TIEN_BH.Value, 2);
                    var t_bntt = Uti.RoundUp((decimal)File.THANH_TIEN_BV.Value - (decimal)File.THANH_TIEN_BH.Value);
                    if (File.T_NGUONKHAC.Value > 0)
                    {
                        //Linhist changed 26/06/26
                        //t_bntt = Math.Round(t_bntt - File.T_NGUONKHAC.Value, 2);
                        t_bntt = Uti.RoundUp(t_bntt - (decimal)File.T_NGUONKHAC.Value);
                        if (t_bntt < 0)
                        {
                            t_bntt = 0;
                        }
                    }

                    //Linhist changed 26/06/26
                    //if (Math.Round((decimal)File.T_BNTT,2)!=Math.Round((decimal)t_bntt,2))
                    if (!Uti.IsEqualUp((decimal?)File.T_BNTT, t_bntt))
                    {
                        RaiseError(true
                            , 2689
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2689, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền Bệnh nhân thanh toán trong XML3:{File.T_BNTT} không đúng công thức tính Thành tiền BV - Thành tiền BH - Tiền nguồn khác = Tiền BN thanh toán={t_bntt}"
                            );
                    }    
                    //Field(x => x.T_BNTT).NotValue(t_bntt).ThenError(2689);

                }
                //2690    3   XML3 T_BNCTT[3176] XML3 T_BNCTT không đúng hướng dẫn của 3716
                if (Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.MUC_HUONG).NotEmpty().Field(x => x.T_BNCCT).NotEmpty().Field(x => x.T_BNTT).NotEmpty())
                {
                    //Linhist changed 250926 - có công thức THANH_TIEN_BH - T_BHTT
                    //Linhist changed 26/06/26
                    //double t_bnctt = EvalHelper.Round2Digit(File.THANH_TIEN_BH.Value - File.T_BNTT.Value);
                    //var t_bnctt = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value - (decimal)File.T_BNTT.Value);

                    //if (File.T_NGUONKHAC.Value > 0)
                    //{
                    //    //Linhist changed 26/06/26
                    //    //var tienconlai = EvalHelper.Round2Digit(File.T_NGUONKHAC.Value - File.T_BNTT.Value);
                    //    var tienconlai = Uti.RoundUp((decimal)File.T_NGUONKHAC.Value - (decimal)File.T_BNTT.Value);
                    //    if (tienconlai > 0)
                    //    {
                    //        //Linhist changed 26/06/26
                    //        //t_bnctt = EvalHelper.Round2Digit(t_bnctt - tienconlai);
                    //        t_bnctt = Uti.RoundUp(t_bnctt - tienconlai);
                    //    }
                    //    if (t_bnctt < 0)
                    //    {
                    //        t_bnctt = 0;
                    //    }
                    //}
                    var t_bncct = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value - (decimal)File.T_BHTT.Value);

                    //Linhist changed 26/06/26
                    //if (Math.Round((decimal)File.T_BNCCT, 2) != Math.Round((decimal)t_bnctt, 2))
                    if (!Uti.IsEqualUp((decimal?)File.T_BNCCT, t_bncct))
                    {
                        RaiseError(true
                            , 2690
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2690, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền Bệnh nhân cùng chi trả trong XML3:{File.T_BNCCT} không đúng công thức tính Thành tiền BH - Tiền BN thanh toán - Tiền nguồn khác = Tiền BN cùng chi trả={t_bncct}"
                            );
                    }
                    //Field(x => x.T_BNCCT).NotValue(t_bnctt).ThenError(2690);
                }
                //2691    3   XML3 T_BHTT[3176] XML3 T_BHTT không đúng so với công thức 3176
                //Linhist changed 25/03/26
                //if (Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.MUC_HUONG).NotEmpty().Field(x => x.T_BHTT).NotEmpty())
                if (File.THANH_TIEN_BH != null && File.MUC_HUONG != null && File.T_BHTT != null)
                {
                    //Linhist changed 26/06/26
                    //double t_bhtt = EvalHelper.Round2Digit(File.THANH_TIEN_BH.Value * File.MUC_HUONG.Value / 100);
                    var t_bhtt = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value * (decimal)File.MUC_HUONG.Value / 100m);

                    if (File.T_NGUONKHAC.Value > 0)
                    {
                        //Linhist changed 26/06/26
                        //var tienconlai = EvalHelper.Round2Digit(File.T_NGUONKHAC.Value - File.T_BNTT.Value - File.T_BNCCT.Value);
                        var tienconlai = Uti.RoundUp((decimal)File.T_NGUONKHAC.Value - (decimal)File.T_BNTT.Value - (decimal)File.T_BNCCT.Value);
                        if (tienconlai > 0)
                        {
                            //Linhist changed 26/06/26
                            //t_bhtt = EvalHelper.Round2Digit(t_bhtt - tienconlai);
                            t_bhtt = Uti.RoundUp(t_bhtt - tienconlai);
                        }
                        if (t_bhtt < 0)
                        {
                            t_bhtt = 0;
                        }
                    }

                    //Linhist changed 26/06/26
                    //if (Math.Round((decimal)File.T_BHTT, 2) != Math.Round((decimal)t_bhtt, 2))
                    if (!Uti.IsEqualUp((decimal?)File.T_BHTT, t_bhtt))
                    {
                        RaiseError(true
                            , 2691
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2691, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền Bảo hiểm thanh toán trong XML3:{File.T_BHTT} không đúng công thức tính Thành tiền BH * Mức hưởng - Tiền nguồn khác = Tiền BH thanh toán={t_bhtt}"
                            );
                    }
                    //Field(x => x.T_BHTT).NotValue(t_bhtt).ThenError(2691);

                    //}

                    //2088	4	XML3	T_BHTT	[130] Tiền Bảo hiểm thanh toán bị sai so với công thức hướng dẫn 4750
                    //Linhist changed 25/03/26 - connect with upper condition
                    //if (Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.MUC_HUONG).NotEmpty().Field(x => x.T_BHTT).NotEmpty())
                    //{
                    //double t_bhtt = EvalHelper.Round2Digit(File.THANH_TIEN_BH.Value * File.MUC_HUONG.Value / 100);

                    //Linhist changed 26/06/26
                    //double t_bnctttamtinh = EvalHelper.Round2Digit(File.THANH_TIEN_BH.Value - t_bhtt);
                    //double t_bntttamtinh = EvalHelper.Round2Digit(File.THANH_TIEN_BV.Value - File.THANH_TIEN_BH.Value);
                    var t_bnctttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value - t_bhtt);
                    var t_bntttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BV.Value - (decimal)File.THANH_TIEN_BH.Value);
                    if (File.T_NGUONKHAC.Value != 0)
                    {
                        //Linhist changed 26/06/26
                        //if (File.T_NGUONKHAC.Value > t_bnctttamtinh + t_bntttamtinh)
                        //{
                        //    t_bhtt = Math.Round(t_bhtt - (File.T_NGUONKHAC.Value - t_bnctttamtinh) - t_bntttamtinh, 2);
                        //}
                        //else if (File.T_NGUONKHAC < t_bnctttamtinh + t_bntttamtinh)
                        //{

                        //}
                        if (Uti.IsOverUp((decimal?)File.T_NGUONKHAC, t_bnctttamtinh + t_bntttamtinh))
                        {
                            t_bhtt = Uti.RoundUp(t_bhtt - ((decimal)File.T_NGUONKHAC.Value - t_bnctttamtinh) - t_bntttamtinh, 2);
                        }
                        //else if (Uti.IsUnderUp((decimal?)File.T_NGUONKHAC, t_bnctttamtinh + t_bntttamtinh))
                        //{

                        //}
                    }

                    //Linhist changed 26/06/26
                    //if (Math.Round((decimal)File.T_BHTT, 2) != Math.Round((decimal)t_bhtt, 2))
                    if (!Uti.IsEqualUp((decimal?)File.T_BHTT, t_bhtt))
                    {
                        RaiseError(true
                            , 2088
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2088, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền Bảo hiểm thanh toán trong XML3:{File.T_BHTT} không đúng công thức tính Thành tiền BH * Mức hưởng - Tiền nguồn khác = Tiền BH thanh toán={t_bhtt}"
                            );
                        //Field(x => x.T_BHTT).NotValue(t_bhtt).ThenError(2088);

                    }
                }
                //2089    4   XML3 T_BNCTT[130] Tiền Bệnh nhân cùng chi trả bị sai so với công thức hướng dẫn 4750
                //Linhist changed 25/03/26
                //if (Field(x => x.THANH_TIEN_BH).NotEmpty().Field(x => x.MUC_HUONG).NotEmpty().Field(x => x.T_BNCCT).NotEmpty())
                if (File.THANH_TIEN_BH != null && File.MUC_HUONG != null && File.T_BNCCT != null)
                {
                    //Linhist changed 26/06/26
                    //double t_bntttamtinh = EvalHelper.Round2Digit(File.THANH_TIEN_BV.Value - File.THANH_TIEN_BH.Value);
                    //double t_bhtttamtinh = EvalHelper.Round2Digit(File.THANH_TIEN_BH.Value * File.MUC_HUONG.Value / 100);
                    //double t_bnctt = EvalHelper.Round2Digit(File.THANH_TIEN_BH.Value - t_bhtttamtinh);

                    //if (File.T_NGUONKHAC > t_bntttamtinh)
                    //{
                    //    if (t_bnctt > File.T_NGUONKHAC.Value - t_bntttamtinh)
                    //    {
                    //        t_bnctt = Math.Round(t_bnctt - (File.T_NGUONKHAC.Value - t_bntttamtinh), 2);
                    //    }
                    //    else
                    //    {
                    //        t_bnctt = 0;
                    //    }
                    //}

                    //if (Math.Round((decimal)File.T_BNCCT, 2) != Math.Round((decimal)t_bnctt, 2))
                    var t_bntttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BV.Value - (decimal)File.THANH_TIEN_BH.Value);
                    var t_bhtttamtinh = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value * (decimal)File.MUC_HUONG.Value / 100m);
                    var t_bnctt = Uti.RoundUp((decimal)File.THANH_TIEN_BH.Value - t_bhtttamtinh);

                    if (Uti.IsOverUp((decimal?)File.T_NGUONKHAC, t_bntttamtinh))
                    {
                        if (Uti.IsOverUp(t_bnctt, (decimal)File.T_NGUONKHAC.Value - t_bntttamtinh))
                        {
                            t_bnctt = Uti.RoundUp(t_bnctt - ((decimal)File.T_NGUONKHAC.Value - t_bntttamtinh));
                        }
                        else
                        {
                            t_bnctt = 0;
                        }
                    }

                    if (!Uti.IsEqualUp((decimal?)File.T_BNCCT, t_bnctt))
                    {
                        RaiseError(true
                            , 2089
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2089, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.MA_VAT_TU}, có mã nhóm:{File.MA_NHOM}, có tiền Bệnh nhân cùng chi trả trong XML3:{File.T_BNCCT} không đúng công thức tính Thành tiền BH - Tiền BH thanh toán - Tiền nguồn khác = Tiền BN cùng chi trả={t_bnctt}"
                            );
                    }
                    //Field(x => x.T_BNCCT).NotValue(t_bnctt).ThenError(2089);

                }
                #endregion

                #region kiểm tra mã máy
                //2189	4	XML3	MA_MAY	[130] ĐK Tỉnh Lào Cai: Thiếu thông tin Mã máy tại XML3
                //2190    4   XML3 MA_THAY[130] ĐK Tỉnh Lào Cai: Định dạng thông tin Mã Máy không đúng yêu cầu 4750
                if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
                {
                    //Linhist added 02/02/26
                    if (!dmTHUCONG.Any(x => x.MA_DICH_VU == File.MA_DICH_VU))
                    {
                        if (!Uti.CodeMatch(File.MA_DICH_VU, "22.0149.1594;22.0279.1269;22.0280.1269;01.0281.1510;01.0284.1269;03.0191.1510;14.0255.0755;22.0019.1348;22.0021.1219;22.0126.0092;22.0127.0091;22.0129.1415;22.0133.1409;22.0136.1363;22.0149.1594;22.0149.1594;22.0152.1609;22.0154.1735;22.0268.1330;22.0274.1326;22.0279.1269;22.0279.1269;22.0279.1269;22.0280.1269;22.0283.1269;22.0285.1267;22.0286.1268;22.0290.1275;22.0291.1280;22.0291.1280;22.0291.1280;22.0292.1280;22.0304.1306;22.0308.1306;22.0502.1267;22.0502.1268;23.0019.1493;23.0029.1473;23.0030.1472;23.0058.1487;23.0103.1531;23.0166.1494;23.0173.1575;23.0173.1575;23.0188.1586;23.0193.1589;23.0194.1589;23.0194.1589;23.0194.1589;23.0195.1589;23.0209.1606;23.0209.1606;23.0220.1608;23.0234.1510;23.0234.1510;23.0244.1544;23.0244.1544;24.0001.1714;24.0001.1714;24.0003.1715;24.0003.1715;24.0003.1715;24.0003.1715;24.0004.1716;24.0005.1716;24.0006.1723;24.0006.1723;24.0006.1723;24.0006.1723;24.0006.1723;24.0007.1723;24.0008.1722;24.0016.1712;24.0016.1712;24.0017.1714;24.0019.1685;24.0019.1685;24.0032.1687;24.0042.1714;24.0049.1714;24.0060.1627;24.0060.1627;24.0094.1623;24.0094.1623;24.0098.1720;24.0098.1720;24.0108.1720;24.0117.1646;24.0117.1646;24.0137.1650;24.0144.1621;24.0144.1621;24.0152.1653;24.0155.1696;24.0155.1696;24.0164.1696;24.0169.1616;24.0169.1616;24.0180.1662;24.0183.1637;24.0183.1637;24.0187.1637;24.0187.1637;24.0235.1719;24.0243.1671;24.0243.1671;24.0254.1701;24.0263.1665;24.0263.1665;24.0265.1674;24.0267.1674;24.0268.1674;24.0269.1674;24.0289.1694;24.0289.1694;24.0289.1694;24.0305.1674;24.0307.1674;24.0309.1674;24.0309.1674;24.0317.1674;24.0319.1674;24.0319.1674;24.0321.1674;24.0322.1724;24.0322.1724;24.0322.1724;24.0322.1724;24.0323.1716;24.0326.1722;25.0007.1758;25.0013.1758;25.0015.1758;25.0019.1758;25.0030.1751;25.0035.1753;25.0037.1751;25.0061.1746.K.01929;25.0074.1736;25.0079.1744;25.0089.1735;25.0090.1757;"))
                        {
                            if (String.IsNullOrEmpty(File.MA_MAY))
                            {
                                RaiseError(true
                                , 2189
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2189, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có mã máy theo quy định của Sở Y tế Lào Cai"
                                );
                                //Field(x => x.MA_MAY).IsEmpty().ThenError(2189);
                            }
                        }
                    }
                }
                if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
                {
                    if (!Uti.CodeMatch(File.MA_DICH_VU, "22.0149.1594;22.0279.1269;22.0280.1269;22.0283.1269;22.0127.0091;22.0126.0092;22.0279.1269;22.0291.1280;25.0035.1753;22.0291.1280;22.0279.1269;24.0321.1674;22.0154.1735;24.0108.1720;22.0274.1326;22.0268.1330;23.0234.1510;25.0061.1746.K.01929;23.0244.1544;24.0117.1646;24.0155.1696;24.0144.1621;24.0169.1616;24.0094.1623;24.0060.1627;24.0183.1637;24.0098.1720;24.0187.1637;24.0243.1671;24.0289.1694;24.0254.1701;23.0173.1575;23.0194.1589;24.0289.1694;24.0016.1712;24.0004.1716;24.0019.1685;24.0322.1724;24.0006.1723;24.0003.1715;24.0006.1723;24.0008.1722;24.0326.1722;24.0322.1724;23.0194.1589;23.0103.1531;24.0322.1724;24.0323.1716;24.0006.1723;24.0003.1715;24.0007.1723;24.0005.1716;24.0032.1687;24.0137.1650;24.0180.1662;24.0152.1653;24.0235.1719;23.0234.1510;14.0255.0755;24.0006.1723;24.0003.1715;24.0001.1714;22.0021.1219;22.0019.1348;22.0149.1594;24.0319.1674;24.0263.1665;24.0309.1674;23.0166.1494;23.0030.1472;23.0029.1473;23.0019.1493;23.0058.1487;22.0136.1363;22.0129.1415;22.0133.1409;22.0304.1306;22.0308.1306;23.0220.1608;23.0209.1606;23.0209.1606;25.0007.1758;22.0279.1269;22.0285.1267;22.0290.1275;22.0286.1268;22.0280.1269;22.0149.1594;01.0281.1510;03.0191.1510;22.0152.1609;25.0037.1751;25.0030.1751;25.0074.1736;25.0089.1735;25.0019.1758;25.0015.1758;25.0013.1758;25.0079.1744;25.0090.1757;22.0502.1268;01.0284.1269;22.0502.1267;22.0291.1280;22.0292.1280;23.0244.1544;24.0117.1646;24.0155.1696;24.0144.1621;24.0169.1616;24.0094.1623;24.0060.1627;24.0183.1637;24.0098.1720;24.0187.1637;24.0243.1671;24.0289.1694;24.0319.1674;24.0001.1714;24.0267.1674;24.0049.1714;24.0317.1674;24.0017.1714;24.0268.1674;24.0305.1674;24.0263.1665;24.0309.1674;24.0307.1674;24.0269.1674;24.0265.1674;24.0042.1714;24.0016.1712;24.0019.1685;24.0322.1724;24.0006.1723;24.0003.1715;23.0188.1586;23.0194.1589;23.0193.1589;23.0173.1575;23.0195.1589;"))
                    {
                        if (!checkMA_MAY(File.MA_MAY,File.MA_CSKCB))
                        {
                            RaiseError(true
                                , 2190
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2190, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã máy trong XML3:{File.MA_MAY} không đúng định dạng quy định theo 4750"
                                );
                            //RaiseError(!checkMA_MAY((File.MA_MAY), File.MA_CSKCB), 2190);
                        }
                    
                    }
                }
                //2559	3	XML3	MA_MAY	[130] Dịch vụ Phẫu thuật không nhập thông tin mã máy
                //2560    3   XML3 MA_MAY[130] Dịch vụ Thủ thuật không nhập mã máy
                if (Field(x => x.MA_NHOM).HasValue(8))
                {
                    if(!dmTHUCONG.Any(x=>x.MA_DICH_VU == File.MA_DICH_VU))
                    {
                        if(string.IsNullOrEmpty(File.MA_MAY))
                        {
                            RaiseError(true
                                , 2559
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2559, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có mã máy theo quy định của QĐ 4750"
                                );
                        }
                        //Field(x => x.MA_MAY).IsEmpty().ThenError(2559);
                    }
                }
                if (Field(x => x.MA_NHOM).HasValue(18))
                {
                    //Linhist added 02/02/26
                    if (!dmTHUCONG.Any(x => x.MA_DICH_VU == File.MA_DICH_VU))
                    {
                        if (string.IsNullOrEmpty(File.MA_MAY))
                        {
                            RaiseError(true
                                , 2560
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2560, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có mã máy theo quy định của QĐ 4750"
                                );
                            //Field(x => x.MA_MAY).IsEmpty().ThenError(2560);
                        }
                    }
                }
                if (Field(x => x.MA_NHOM).HasValue(8, 18,2,3,1))
                {
                    if (dmTHIETBI.Any())
                    {
                        if (Field(x => x.MA_DICH_VU).NotEmpty())
                        {
                            if (Field(x => x.MA_MAY).NotEmpty())
                            {
                                if (!dmTHIETBI.Any(x => File.MA_MAY == x.MA_MAY))
                                {
                                    var raiseA = false;
                                    var raiseB = false;
                                    var maMays = File.MA_MAY.Split(';');
                                    foreach (var mm in maMays)
                                    {
                                        //2561    3   XML3 MA_MAY[130] Mã máy không đúng trong DM Trang thiết bị y tế
                                        if (!dmTHIETBI.Any(x => x.MA_MAY == mm || Uti.CodeContain(mm, x.MA_MAY)))
                                        {
                                            RaiseError(true
                                                , 2561
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2561, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã máy trong XML3:{File.MA_MAY} không có Trong Danh mục Mã máy phê duyệt"
                                                );
                                            raiseA = true;
                                        }

                                        //4209	2	XML3	MA_MAY	[130] Mã máy không đúng trong DM Trang thiết bị y tế (không tính dấu cách)
                                        if (!dmTHIETBI.Any(x => x.MA_MAY == mm.Trim() || Uti.CodeContain(mm.Trim(), x.MA_MAY)))
                                        {
                                            RaiseError(true
                                                , 4209
                                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2561, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã máy trong XML3:{File.MA_MAY} không có Trong Danh mục Mã máy phê duyệt"
                                                );
                                            raiseB = true;
                                        }

                                        if (raiseA && raiseB) break;
                                    }
                                }
                            }
                        }
                    }
                }

                //2095	4	XML3	MA_MAY	[130] Thiếu thông tin Mã máy trong bảng XML3
                //2096	4	XML3	MA_MAY	[130] Mã máy không đúng định dạng quy định theo 4750
                if (Field(x => x.MA_NHOM).HasValue(1, 2, 3, 8, 18))
                {
                    if (!File.MA_DICH_VU.Contains("K."))
                    {
                        if (dmTHUCONG.Any())
                        {
                            if (!dmTHUCONG.Any(x => x.MA_DICH_VU == File.MA_DICH_VU))
                            {
                                //Field(x => x.MA_MAY).IsEmpty().ThenError(2095);
                                if (string.IsNullOrEmpty(File.MA_MAY))
                                {
                                    RaiseError(true
                                        , 2095
                                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2095, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có mã máy theo quy định của QĐ 4750"
                                        );
                                    //RaiseError(true, 2095);
                                }
                                else
                                {
                                    if(!checkMA_MAY((File.MA_MAY), File.MA_CSKCB))
                                    {
                                        RaiseError(true
                                            , 2096
                                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(2096, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã máy trong XML3:{File.MA_MAY} không đúng định dạng quy định theo 4750"
                                            );
                                    }
                                    //RaiseError(!checkMA_MAY((File.MA_MAY), File.MA_CSKCB), 2096);
                                }

                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(File.MA_MAY))
                            {
                                RaiseError(true
                                    , 2095
                                    , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2095, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có mã máy theo quy định của QĐ 4750"
                                    );
                                //Field(x => x.MA_MAY).IsEmpty().ThenError(2095);
                            }
                            if (Field(x => x.MA_MAY).NotEmpty())
                            {
                                if(!checkMA_MAY((File.MA_MAY), File.MA_CSKCB))
                                {
                                    RaiseError(true
                                        , 2096
                                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2096, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã máy trong XML3:{File.MA_MAY} không đúng định dạng quy định theo 4750"
                                        );
                                }
                                //RaiseError(!checkMA_MAY((File.MA_MAY), File.MA_CSKCB), 2096);
                                //var mamay = File.MA_MAY.Split(";");
                                //foreach (var m in mamay)
                                //{
                                //    RaiseError(!checkMA_MAY((m), File.MA_CSKCB), 2096);
                                //}
                                //RaiseError(!checkMA_MAY((File.MA_MAY), File.MA_CSKCB), 2096);
                            }

                        }
                    }
                }
                if (Field(x => x.MA_NHOM).HasValue(1, 2, 3))
                {


                    //2401	3	XML3	MA_MAY	[130] Mã máy không có trong danh mục phê duyệt
                    if (dmTHIETBI.Any())
                    {
                        if (Field(x => x.MA_DICH_VU).NotEmpty())
                        {
                            if (Field(x => x.MA_MAY).NotEmpty())
                            {
                                if (!dmTHIETBI.Any(x => x.MA_MAY == File.MA_MAY))
                                {
                                    RaiseError(true
                                        , 2401
                                        , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2401, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, có mã máy trong XML3:{File.MA_MAY} không có Trong Danh mục Mã máy phê duyệt"
                                        );
                                    //RaiseError(!dmTHIETBI.Any(x => x.MA_MAY == File.MA_MAY), 2401);
                                }
                                //var mamay = File.MA_MAY.Split(";");
                                //foreach (var m in mamay)
                                //{
                                //    RaiseError(!dmTHIETBI.Any(x => x.MA_MAY == m), 2401);
                                //}
                            }
                        }
                    }

                    //356	1	XML3	NGAY_KQ	Không tìm thấy kết quả CLS tại bảng XML4 (Bệnh nhân chưa thực hiện CLS)

                    if (!File.XML01.XML04s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU))
                    {
                        RaiseError(true
                            , 356
                            , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(356, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có kết quả CLS tại bảng XML4 tương ứng cho mã dịch vụ:{File.MA_DICH_VU} đó (Bệnh nhân chưa thực hiện CLS)"
                            );
                        //RaiseError(true, 356);
                    }
                    // 2455	3	XML3	MA_DICH_VU	[130] Dịch vụ Nhuộm hai màu chưa có thông tin kết luận XML4     
                    if (File.MA_DICH_VU == "25.0037.1751")
                    {
                        if (File.XML01.XML04s.Any(x => x.MA_DICH_VU == File.MA_DICH_VU && string.IsNullOrEmpty(x.KET_LUAN)))
                        {
                            
                            //RaiseError(true, 2455);
                        }
                    }


                }
                #endregion

                #region các mã lỗi lẻ từ 4210

                //884 Không thanh toán tiền giường điều trị nội trú của BN nằm viện < 4h

                if (Field(x => x.MA_NHOM).HasValue(15))
                {
                    if (File.XML01.MA_LOAI_KCB!=null && File.XML01.MA_LOAI_KCB == "03" && File.XML01.NGAY_RA != null)
                    {
                        if (File.XML01.NGAY_VAO_NOI_TRU !=null && File.XML01.NGAY_VAO_NOI_TRU.Value.AddHours(4) > File.XML01.NGAY_RA.Value)
                        {
                            RaiseError(true,
                                884,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(884, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng thời gian nằm viện < 4 giờ"
                                );
                           // RaiseError(true, 884);
                        }
                    }
                }
                if (Field(x => x.MA_NHOM).HasValue(14))
                {
                    if (File.XML01.MA_LOAI_KCB == "04")
                    {
                        if (File.XML01.NGAY_VAO_NOI_TRU.Value.AddHours(4) > File.XML01.NGAY_RA.Value)
                        {
                            RaiseError(true,
                                884,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(884, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng thời gian nằm viện < 4 giờ"
                                );
                            //RaiseError(true, 884);
                        }
                    }
                }
                //2885	3	XML3	MA_DICH_VU	[3176] Bệnh nhân điều trị nội trú ban ngày bắt buộc phải sử dụng thủ thuật mới được kê ngày giường
                if (Field(x => x.MA_NHOM).HasValue(14))
                {
                    if (File.XML01.MA_LOAI_KCB == "04")
                    {
                        if (!File.XML01.XML03s.Any(x => x.MA_NHOM == 18))
                        {
                            RaiseError(true
                                , 2885
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2885, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có thủ thuật trong hồ sơ XML3"
                                );
                            //RaiseError(true, 2885);
                        }

                    }
                }

                //463 	1 	XML3 	MA_DICH_VU 	Không có Biên bản tường trình cho Bệnh nhân Phẫu Thuật
                //477 	1 	XML3 	MA_DICH_VU 	Không có Biên bản tường trình cho bệnh nhân thực hiện thủ thật
                //462	1	XML3	MA_DICH_VU	Dịch vụ phẫu thuật chưa có biên bản hội chẩn trong hồ sơ XML đề nghị thanh toán
                if (Field(x => x.MA_NHOM).HasValue(8, 18))
                {
                    var xml5CungNgay = File.XML01.XML05s.Where(x => x.THOI_DIEM_DBLS >= File.NGAY_TH_YL);
                    if(!xml5CungNgay.Any())
                    {
                        if (Field(x => x.MA_NHOM).HasValue(8))
                        {
                            RaiseError(true,
                                463,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(463, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có Biên bản tường trình cho Bệnh nhân Phẫu Thuật\n" +

                                $"(Lưu ý: Biên bản tường trình phải có thời điểm lập biên bản >= thời điểm thực hiện thủ thuật, phẫu thuật của bệnh nhân)"
                                );
                            //RaiseError(true, 463);
                        }
                        if (Field(x => x.MA_NHOM).HasValue(18))
                        {
                            RaiseError(true,
                                477,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(477, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có Biên bản tường trình cho bệnh nhân thực hiện thủ thuật\n"+
                                $"(Lưu ý: Biên bản tường trình phải có thời điểm lập biên bản >= thời điểm thực hiện thủ thuật, phẫu thuật của bệnh nhân)"
                                );
                            //RaiseError(true, 477);
                        }
                    }
                    else if (!xml5CungNgay.Any(x => x.PHAU_THUAT.Length > 3))
                    {
                        if (Field(x => x.MA_NHOM).HasValue(8))
                        {
                            RaiseError(true,
                                463,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(463, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có Biên bản tường trình cho Bệnh nhân Phẫu Thuật\n" +
                                $"(Lưu ý: Nội dung biên bản tường trình quá ngắn, không phục vụ cho mục đích tường trình)"
                                );
                            //RaiseError(true, 463);
                        }
                        if (Field(x => x.MA_NHOM).HasValue(18))
                        {
                            RaiseError(true,
                                477,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(477, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có Biên bản tường trình cho bệnh nhân thực hiện thủ thuật\n" +
                                $"(Lưu ý: Nội dung biên bản tường trình quá ngắn, không phục vụ cho mục đích tường trình)"
                                );
                            //RaiseError(true, 477);
                        } 
                            

                    }
                    if (Field(x => x.MA_NHOM).HasValue(8))
                    {
                        if (!File.XML01.XML05s.Any(x => x.THOI_DIEM_DBLS <= File.NGAY_TH_YL && Uti.StrExists(x.HOI_CHAN.ToLower(), true, "Phẫu Thuật", "Phương pháp vô cảm", "Kết luận", "Mổ cấp cứu", "Cắt", "Cắt", "Mổ", "vô cảm", "nội soi")))
                        {
                            RaiseError(true,
                                462,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(462, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng không có Biên bản hội chẩn cho Bệnh nhân Phẫu Thuật\n" +
                                $"(Lưu ý: Biên bản hội chẩn phải có thời điểm lập biên bản <= thời điểm y lệnh thực hiện phẫu thuật của bệnh nhân và có nội dung liên quan đến phẫu thuật)"
                                );
                            //RaiseError(true, 462);
                        }
                    }
                }
                //2486	3	XML3	MA_BAC_SI	[130] Bác sĩ vừa y lệnh và thực hiện y lệnh cho các nhóm 1,2,3 (Đề nghị kiểm tra lại)
                if (Uti.NumExists(File.MA_NHOM, 1, 2, 3) && !"18.0036.0001;18.0034.0001;18.0018.0001;18.0020.0001;18.0035.0001;18.0031.0003;18.0030.0001;01.0002.1778;02.0085.1778".Contains(File.MA_DICH_VU))
                {
                    if (!String.IsNullOrEmpty(File.NGUOI_THUC_HIEN) && !String.IsNullOrEmpty(File.MA_BAC_SI))
                    {
                        if (File.NGUOI_THUC_HIEN.Contains(File.MA_BAC_SI))
                        {
                            RaiseError(true
                                , 2486
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2486, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng Bác sĩ {File.MA_BAC_SI} vừa y lệnh và thực hiện y lệnh cho các nhóm 1,2,3 (Đề nghị kiểm tra lại)"
                                );
                            //RaiseError(true, 2486);
                        }
                    }
                }
                //1189  1189    1   XML3 NGAY_KQ Thời Gian Trả Kết Quả Không Hợp Lý(Xét nghiệm &CĐHA)
                if (Uti.NumExists(File.MA_NHOM, 2, 3))
                {
                    if (Field(x => x.NGAY_KQ).NotEmpty())
                    {
                        var tSpan = File.NGAY_KQ - File.NGAY_YL;
                        if (tSpan != null && (tSpan?.TotalHours >= 48d))
                        {
                            RaiseError(true
                                , 1189
                                , $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(1189, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, nhưng Thời Gian Trả Kết Quả Không Hợp Lý (Thời gian trả kết quả >= 48 giờ so với thời điểm y lệnh)\n"+
                                $"Thời điểm y lệnh:{File.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Thời điểm trả kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss}, Khoảng thời gian trả kết quả: {tSpan?.TotalHours} giờ"
                                );
                            RaiseError(true, 1189);
                        }
                    }

                }
                //1988	0	XML3	MA_DICH_VU	ĐK Tỉnh Lào Cai: Điện tim thực hiện trùng thời điểm với các Dịch vụ nhóm 8,18,2,3
                if (isDichVu && Uti.NumExists(File.MA_NHOM, 8, 18) && File.NGAY_YL != null && File.NGAY_KQ != null && Uti.CodeMatch(File.MA_DICH_VU, "01.0002.1778;03.0019.1798"))
                {
                    //Kiem tra cac xml3 nhom 2, 3, 8 & 18 con lai co trung khoang thoi gian khong
                    try
                    {
                        var x3TrungGios = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 8, 18) && !Uti.CodeMatch(x.MA_DICH_VU, "01.0002.1778;03.0019.1798") && x.NGAY_TH_YL <= File.NGAY_KQ && x.NGAY_KQ >= File.NGAY_TH_YL).ToList();
                        if (x3TrungGios.Any())
                        {
                            var dictrunggio = x3TrungGios.ToDictionary(k => k.XML03ID, v => new
                            {
                                v.MA_DICH_VU,
                                v.TEN_DICH_VU,
                                v.MA_NHOM,
                                v.NGAY_TH_YL,
                                v.NGAY_YL,
                                v.NGAY_KQ
                            });
                            var loiTrungGio = string.Join("\n", dictrunggio.Select(item =>

                                $"Dịch vụ trùng: {item.Value.MA_DICH_VU}|{item.Value.TEN_DICH_VU} có mã nhóm:{item.Value.MA_NHOM}, thực hiện y lệnh:{item.Value.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{item.Value.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                                ));
                            RaiseError(true,
                                    1988,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +

                                    $"Nội dung lỗi: {(errorlist.TryGetValue(1988, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3:\n" +
                                    loiTrungGio.Substring(0,Math.Min(3000, loiTrungGio.Length))
                                    );
                            // RaiseError(true, 1988);

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi kiểm tra lỗi 1988 cho bệnh nhân {File.XML01.MA_BN} - {File.XML01.HO_TEN}, Mã CSKCB:{File.MA_CSKCB}, Dòng XML3:{File.STT}, Mã:{File.MA_DICH_VU}");

                    }

                   
                }

                ////***RISK: đoạn này phải xem lại mã loại trừ & mã bắt buộc (nếu xem lại check thêm lỗi 4181)
                #region Linhist removed 23/07/26 - chuyển sang check cùng lỗi 4181
                ////421 Kiểm tra các dịch vụ mã nhóm 8,18 (Không bao gồm dịch vụ đặt ống thở 24h) để so sánh trùng
                ////Linhist changed 11/03/26
                ////if (Uti.NumExists(File.MA_NHOM, 8, 18) && !Uti.HCodeMach(File.MA_DICH_VU, "01.0008.0100;01.0135.0209;08.0015.0252;08.0022.0252;03.0082.0209;03.0058.0209;17.0007.0234;17.0011.0237;08.0022.0252;01.0008.0100;01.0009.0098;08.0022.0252;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0144.0209;01.0136.0209;01.0137.0209;01.0138.0209"))

                ////Linhist added 25/05/26 - thêm mã
                ////if (Uti.NumExists(File.MA_NHOM, 8, 18) && !Uti.HCodeMach(File.MA_DICH_VU, "01.0008.0100;01.0009.0098;01.0087.0898;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0136.0209;01.0137.0209;01.0138.0209;01.0144.0209;01.0175.0196;02.0150.0114;03.0082.0209;03.0058.0209;08.0022.0252;08.0015.0252;17.0007.0234;17.0011.0237"))
                ////Linhist changed 14/07/26
                ////var maLoaiTru = "01.0008.0100;01.0009.0098;01.0087.0898;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0136.0209;01.0137.0209;01.0138.0209;01.0142.0209;01.0144.0209;01.0175.0196;02.0150.0114;03.0082.0209;03.0058.0209;08.0022.0252;08.0015.0252;17.0007.0234;17.0011.0237";
                //var maLoaiTru = "01.0008.0100;01.0009.0098;01.0087.0898;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0136.0209;01.0137.0209;01.0138.0209;01.0142.0209;01.0144.0209;01.0175.0196;02.0150.0114;02.0305.0135;03.0082.0209;03.0058.0209;08.0022.0252;08.0015.0252;09.9000.1894_BS;17.0007.0234;17.0011.0237";
                ////Linhist changed 23/07/26
                ////if (Uti.NumExists(File.MA_NHOM, 8, 18) && !Uti.HCodeMach(File.MA_DICH_VU, maLoaiTru))
                //if (Uti.NumExists(File.MA_NHOM, 8, 18) && File.TYLE_TT_BH == 100 && !Uti.HCodeMach(File.MA_DICH_VU, maLoaiTru))
                //{
                //    try
                //    {
                //        //Linhist changed 11/03/26
                //        //var x3TrungGios = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 8, 18) &&
                //        //                                          !Uti.HCodeMach(x.MA_DICH_VU, "01.0008.0100;01.0135.0209;08.0022.0252;08.0015.0252;03.0082.0209;03.0058.0209;17.0007.0234;17.0011.0237;08.0022.0252;01.0008.0100;01.0009.0098;08.0022.0252;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0144.0209;01.0136.0209;01.0137.0209;01.0138.0209") &&
                //        //                                          (!(x.NGAY_KQ <= File.NGAY_TH_YL || x.NGAY_TH_YL >= File.NGAY_KQ) || (x.NGAY_TH_YL <= File.NGAY_TH_YL && x.NGAY_KQ >= File.NGAY_KQ))
                //        //                                          ).ToList();

                //        //Linhist added 25/05/26 - thêm mã
                //        //var x3TrungGios = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 8, 18) &&
                //        //                                                !Uti.HCodeMach(x.MA_DICH_VU, "01.0008.0100;01.0009.0098;01.0087.0898;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0136.0209;01.0137.0209;01.0138.0209;01.0144.0209;01.0175.0196;02.0150.0114;03.0082.0209;03.0058.0209;08.0022.0252;08.0015.0252;17.0007.0234;17.0011.0237") &&
                //        //                                                x.NGAY_TH_YL <= File.NGAY_KQ && x.NGAY_KQ >= File.NGAY_TH_YL).ToList();
                //        var x3TrungGios = File.XML01.XML03s.Where(x =>
                //                                x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) &&
                //                                Uti.NumExists(x.MA_NHOM, 8, 18) && !Uti.HCodeMach(x.MA_DICH_VU, maLoaiTru) &&
                //                                x.NGAY_TH_YL <= File.NGAY_KQ && x.NGAY_KQ >= File.NGAY_TH_YL &&
                //                                File.TYLE_TT_BH == 100).ToList();

                //        if (x3TrungGios.Any())
                //        {
                //            //Linhist changed 15/04/26 - Chỉ báo lỗi 1 lần, cho lỗi đầu tiên
                //            //var dictrunggio = x3TrungGios.ToDictionary(k => k.XML03ID, v => new
                //            var dictrunggio = x3TrungGios.Take(1).ToDictionary(k => k.XML03ID, v => new
                //            {
                //                v.MA_DICH_VU,
                //                v.TEN_DICH_VU,
                //                v.MA_NHOM,
                //                v.NGAY_TH_YL,
                //                v.NGAY_YL,
                //                v.NGAY_KQ
                //            });

                //            var loiTrungGio = string.Join("\n", dictrunggio.Select(item =>
                //                $"Dịch vụ trùng: {item.Value.MA_DICH_VU}|{item.Value.TEN_DICH_VU} có mã nhóm:{item.Value.MA_NHOM}, thực hiện y lệnh:{item.Value.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{item.Value.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                //                ));
                //            RaiseError(true,
                //                    421,
                //                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                    $"Nội dung lỗi: {(errorlist.TryGetValue(421, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                    $"Chi tiết:" +
                //                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3::\n" +
                //                    loiTrungGio.Substring(0, Math.Min(3000, loiTrungGio.Length))
                //                    );
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, $"Lỗi khi kiểm tra lỗi 421 cho bệnh nhân {File.XML01.MA_BN} - {File.XML01.HO_TEN}, Mã CSKCB:{File.MA_CSKCB}, Dòng XML3:{File.STT}, Mã:{File.MA_DICH_VU}");
                //    }
                //}

                //// Chỉ lấy các dịch vụ đặt ống thở 24h, để xem có trùng với dịch vụ đặt ống thở nào khác hay không?
                ////Linhist changed 23/07/26
                //var maBatBuoc = "03.0082.0209;03.0058.0209;17.0007.0234;17.0011.0237;0;01.0009.0098;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0144.0209;01.0136.0209;01.0137.0209;01.0138.0209";
                //if (Uti.NumExists(File.MA_NHOM, 8, 18) && File.TYLE_TT_BH == 100 && Uti.HCodeMach(File.MA_DICH_VU, maBatBuoc))
                //{
                //    try
                //    {
                //        var x3TrungGios = File.XML01.XML03s.Where(x =>
                //                                x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) &&
                //                                Uti.NumExists(x.MA_NHOM, 8, 18) && x.MA_DICH_VU == File.MA_DICH_VU &&
                //                                (!(x.NGAY_KQ <= File.NGAY_TH_YL || x.NGAY_TH_YL >= File.NGAY_KQ) ||
                //                                (x.NGAY_TH_YL <= File.NGAY_TH_YL && x.NGAY_KQ >= File.NGAY_KQ)) &&
                //                                File.TYLE_TT_BH == 100).ToList();
                //        if (x3TrungGios.Any())
                //        {
                //            var dictrunggio = x3TrungGios.ToDictionary(k => k.XML03ID, v => new
                //            {
                //                v.MA_DICH_VU,
                //                v.TEN_DICH_VU,
                //                v.MA_NHOM,
                //                v.NGAY_TH_YL,
                //                v.NGAY_YL,
                //                v.NGAY_KQ
                //            });
                //            var loiTrungGio = string.Join("\n", dictrunggio.Select(item =>
                //                $"Dịch vụ trùng: {item.Value.MA_DICH_VU}|{item.Value.TEN_DICH_VU} có mã nhóm:{item.Value.MA_NHOM}, thực hiện y lệnh:{item.Value.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{item.Value.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                //                ));
                //            RaiseError(true,
                //                    421,
                //                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                //                    $"Nội dung lỗi: {(errorlist.TryGetValue(421, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                //                    $"Chi tiết:" +
                //                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3::\n" +
                //                   loiTrungGio.Substring(0, Math.Min(3000, loiTrungGio.Length))
                //                    );
                //        }
                //    }
                //    catch(Exception ex)
                //    {
                //        _logger.LogError(ex, $"Lỗi khi kiểm tra lỗi 421 cho bệnh nhân {File.XML01.MA_BN} - {File.XML01.HO_TEN}, Mã CSKCB:{File.MA_CSKCB}, Dòng XML3:{File.STT}, Mã:{File.MA_DICH_VU}");
                //    }

                //    //if (File.XML01.XML03s.Any(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 8, 18) && Uti.HCodeMach(x.MA_DICH_VU, "03.0082.0209;03.0058.0209;17.0007.0234;17.0011.0237;01.0008.0100;01.0009.0098;01.0128.0209;01.0130.0209;01.0131.0209;01.0132.0209;01.0133.0209;01.0134.0209;01.0135.0209;01.0144.0209;01.0136.0209;01.0137.0209;01.0138.0209") && (!(x.NGAY_KQ <= File.NGAY_TH_YL || x.NGAY_TH_YL >= File.NGAY_KQ) || (x.NGAY_TH_YL <= File.NGAY_TH_YL && x.NGAY_KQ >= File.NGAY_KQ))))
                //    //{
                //    //    RaiseError(true, 421);
                //    //}
                //}
                #endregion


                //Linhist changed 170826
                //mã cskcb 34004 - 2 dv được làm trùng nhau
                //08.0005.0230 Điện châm[kim ngắn]
                //08.0022.0252 Sắc thuốc thang

                
                //2454	3	XML3	NGAY_TH_YL	[130] Thời gian thực hiện y lệnh trùng thời điểm với dịch vụ khác
                //Linhist changed 240826
                //if (Uti.NumExists(File.MA_NHOM, 2, 3, 8, 18) && !Uti.CodeMatch(File.MA_DICH_VU, "08.0022.0252;08.0005.0230;08.0006.0271"))
                var dvSacThuocs = "03.0276.0252;03.0284.0252;08.0005.0230;08.0006.0271;08.0015.0252;08.0022.0252";
                if (Uti.NumExists(File.MA_NHOM, 2, 3, 8, 18) && !Uti.CodeMatch(File.MA_DICH_VU, dvSacThuocs))
                {
                    try
                    {
                        if (!dvTrung.Any(x => x.MA_DV1 == File.MA_DICH_VU && x.MA_DV2 == File.MA_DICH_VU))
                        {
                            var dvtrungthyl = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && !Uti.CodeMatch(x.MA_DICH_VU, dvSacThuocs) && Uti.NumExists(x.MA_NHOM, 2, 3, 8, 18) && x.NGAY_TH_YL == File.NGAY_TH_YL);

                            if (dvtrungthyl.Any())
                            {
                                var dictrungthyl = dvtrungthyl.ToDictionary(k => k.XML03ID, v => new
                                {
                                    v.MA_DICH_VU,
                                    v.TEN_DICH_VU,
                                    v.MA_NHOM,
                                    v.NGAY_TH_YL,
                                    v.NGAY_YL,
                                    v.NGAY_KQ
                                });
                                var loitrungthyl = string.Join("\n", dictrungthyl.Select(item =>
                                    $"dịch vụ trùng: {item.Value.MA_DICH_VU}|{item.Value.TEN_DICH_VU} có mã nhóm:{item.Value.MA_NHOM}, thực hiện y lệnh:{item.Value.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{item.Value.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                                    ));
                                RaiseError(true,
                                    2454,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"nội dung lỗi: {(errorlist.TryGetValue(2454, out var erinfo) ? erinfo : "không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm th y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} trùng thời điểm với dịch vụ khác trong hồ sơ xml3:\n" +
                                    loitrungthyl.Substring(0, Math.Min(3000, loitrungthyl.Length))
                                    );
                                //raiseerror(true, 2454);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi kiểm tra lỗi 2454 cho bệnh nhân {File.XML01.MA_BN} - {File.XML01.HO_TEN}, Mã CSKCB:{File.MA_CSKCB}, Dòng XML3:{File.STT}, Mã:{File.MA_DICH_VU}");
                    }

                }
                //2671	3	XML3	MA_DICH_VU	[130] Dịch vụ kỹ thuật thực hiện trùng thời điểm (Các nhóm 1,2,3 và 8,18 thực hiện trùng thời gian)
                if (Uti.NumExists(File.MA_NHOM, 1, 2, 3, 8, 18))
                {
                    try
                    {
                        //(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ)
                        if (!dvTrung.Any(x => x.MA_DV1 == File.MA_DICH_VU && x.MA_DV2 == File.MA_DICH_VU))
                        {
                            //Linhist changed 01/04/26
                            //var trungs = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !Uti.CodeMatch(File.MA_DICH_VU, "08.0022.0252;08.0006.0271") &&
                            //                                            !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 1, 2, 3, 8, 18) &&
                            //                                            (!(x.NGAY_KQ <= File.NGAY_TH_YL || x.NGAY_TH_YL >= File.NGAY_KQ) || (x.NGAY_TH_YL <= File.NGAY_TH_YL && x.NGAY_KQ >= File.NGAY_KQ))
                            //                                        ).ToList();
                            //Linhist changed 240826 - theo dvSacThuocs
                            //var trungs = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !Uti.CodeMatch(File.MA_DICH_VU, "08.0022.0252;08.0006.0271") &&
                            var trungs = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !Uti.CodeMatch(x.MA_DICH_VU, dvSacThuocs) &&
                                                                        !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 1, 2, 3, 8, 18) &&
                                                                        x.NGAY_TH_YL <= File.NGAY_KQ && x.NGAY_KQ >= File.NGAY_TH_YL).ToList();
                            if (trungs.Any())
                            {
                                var dictrungthoidiem = trungs.ToDictionary(k => k.XML03ID, v => new
                                {
                                    v.MA_DICH_VU,
                                    v.TEN_DICH_VU,
                                    v.MA_NHOM,
                                    v.NGAY_TH_YL,
                                    v.NGAY_YL,
                                    v.NGAY_KQ
                                });
                                var loiTrungthoidiem = string.Join("\n", dictrungthoidiem.Select(item =>
                                    $"Dịch vụ trùng: {item.Value.MA_DICH_VU}|{item.Value.TEN_DICH_VU} có mã nhóm:{item.Value.MA_NHOM}, thực hiện y lệnh:{item.Value.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{item.Value.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                                    ));
                                //var dicTrungMaDV = dvtrungthoidiem
                                //    .GroupBy(x => x.MA_DICH_VU)
                                //    .ToDictionary(
                                //        g => g.Key,
                                //        g => g.Select(v => new
                                //        {
                                //            v.TEN_DICH_VU,
                                //            v.MA_NHOM,
                                //            v.NGAY_TH_YL,
                                //            v.NGAY_YL,
                                //            v.NGAY_KQ
                                //        }).ToList());
                                //var loiTrungMaDV = string.Join("\n", dictrungthoidiem
                                //        .Where(item => item.Value.Count > 1) // chỉ lấy những mã bị trùng
                                //        .Select(item =>
                                //            $"Dịch vụ trùng: {item.Key}|{item.Value.First().TEN_DICH_VU} có {item.Value.Count} lần thực hiện, nhóm:{item.Value.First().MA_NHOM}, ngày y lệnh:{item.Value.First().NGAY_TH_YL:dd/MM/yyyy HHmm}, ngày kết quả:{item.Value.First().NGAY_KQ:dd/MM/yyyy HHmm}"
                                //        ));
                                RaiseError(true,
                                    2671,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2671, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} trùng khoảng thời gian thực hiện với dịch vụ khác trong hồ sơ XML3:\n" +
                                    loiTrungthoidiem.Substring(0, Math.Min(3000, loiTrungthoidiem.Length))
                                    );
                                //RaiseError(true, 2671);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi kiểm tra lỗi 2671 cho bệnh nhân {File.XML01.MA_BN} - {File.XML01.HO_TEN}, Mã CSKCB:{File.MA_CSKCB}, Dòng XML3:{File.STT}, Mã:{File.MA_DICH_VU}");
                    }

                }
                //2939	3	XML3	NGUOI_THUC_HIEN    [3176] Trùng người thực hiện dịch vụ tại cùng một thời điểm
                if (Uti.NumExists(File.MA_NHOM, 2, 3, 8, 18))
                {
                    var dichvutrungnguoithuchien = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 2, 3, 8, 18) && x.NGAY_TH_YL == File.NGAY_TH_YL && (x.NGUOI_THUC_HIEN.Split(';').Intersect(File.NGUOI_THUC_HIEN.Split(';')).Any()));
                    if(dichvutrungnguoithuchien.Any())
                    {
                        foreach (var dv in dichvutrungnguoithuchien)
                        {
                            var nguoiTrung = dv.NGUOI_THUC_HIEN.Split(";")
                                .Intersect(File.NGUOI_THUC_HIEN.Split(";"));

                            var tenNguoiTrung = string.Join(", ", nguoiTrung);

                            RaiseError(true,
                                2939,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2939, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} trùng người thực hiện dịch vụ với dịch vụ khác trong hồ sơ XML3:\n" +
                                $"Dịch vụ trùng: {dv.MA_DICH_VU}|{dv.TEN_DICH_VU}|{File.NGUOI_THUC_HIEN} có mã nhóm:{dv.MA_NHOM}, thực hiện y lệnh:{dv.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{dv.NGAY_KQ:dd/MM/yyyy HH:mm:ss}\n" +
                                $"Người thực hiện trùng: {tenNguoiTrung}|{dv.MA_DICH_VU}"
                                );
                        }
                    }    
                    //if (File.XML01.XML03s.Any(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 2, 3, 8, 18) && x.NGAY_TH_YL == File.NGAY_TH_YL && (x.NGUOI_THUC_HIEN.Split(';').Intersect(File.NGUOI_THUC_HIEN.Split(';')).Any())/* && xml.MA_DICH_VU == File.MA_DICH_VU*/)
                    //{
                    //    RaiseError(true, 2939);
                    //}
                }
                //2940    3   XML3 NGUOI_THUC_HIEN    [3176] Trùng người thực hiện dịch vụ tại cùng một thời điểm (Bao gồm cả nhóm 8, 18, 2, 3)
                //Linhist changed 18/03/26
                //if (Uti.NumExists(File.MA_NHOM, 1, 2, 3, 8, 18))
                if (Uti.NumExists(File.MA_NHOM, 2, 3, 8, 18))
                {
                    //Linhist changed 01/04/26 - Cho phép dv sắc thuốc thang 08.0022.0252 được làm trùng
                    //var trungs = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) &&
                    //                                            Uti.NumExists(x.MA_NHOM, 2, 3, 8, 18) &&
                    //                                            (!(x.NGAY_KQ <= File.NGAY_TH_YL || x.NGAY_TH_YL >= File.NGAY_KQ) || (x.NGAY_TH_YL <= File.NGAY_TH_YL && x.NGAY_KQ >= File.NGAY_KQ)) &&
                    //                                            (x.NGUOI_THUC_HIEN.Split(';').Intersect(File.NGUOI_THUC_HIEN.Split(';')).Any()));
                    var trungs = File.XML01.XML03s.Where(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) &&
                                                                !Uti.CodeMatch(File.MA_DICH_VU, "08.0022.0252") &&
                                                                Uti.NumExists(x.MA_NHOM, 2, 3, 8, 18) &&
                                                                x.NGAY_TH_YL <= File.NGAY_KQ && x.NGAY_KQ >= File.NGAY_TH_YL &&
                                                                (x.NGUOI_THUC_HIEN.Split(';').Intersect(File.NGUOI_THUC_HIEN.Split(';')).Any()));
                    if (trungs.Any())
                    {
                        //Linhist changed 01/04/26 - Chỉ báo lỗi 1 lần
                        //foreach (var dv in trungs)
                        //{
                        //    var nguoiTrung = dv.NGUOI_THUC_HIEN.Split(";")
                        //        .Intersect(File.NGUOI_THUC_HIEN.Split(";"));

                        //    var tenNguoiTrung = string.Join(", ", nguoiTrung);

                        //    RaiseError(true,
                        //        2940,
                        //        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //        $"Nội dung lỗi: {(errorlist.TryGetValue(2940, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //        $"Chi tiết:" +
                        //        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} trùng người thực hiện dịch vụ với dịch vụ khác trong hồ sơ XML3:\n" +
                        //        $"Dịch vụ trùng: {dv.MA_DICH_VU}|{dv.TEN_DICH_VU}|{File.NGUOI_THUC_HIEN} có mã nhóm:{dv.MA_NHOM}, thực hiện y lệnh:{dv.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{dv.NGAY_KQ:dd/MM/yyyy HH:mm:ss}\n" +
                        //        $"Người thực hiện trùng: {tenNguoiTrung}|{dv.MA_DICH_VU}"
                        //        );
                        //}

                        //Linhist added 15/04/26 - Chỉ báo lỗi 1 lần, cho dv đầu tiên
                        trungs = trungs.Take(1);

                        var nguoiTrung = trungs.SelectMany(x => x.NGUOI_THUC_HIEN.Split(';').Intersect(File.NGUOI_THUC_HIEN.Split(';'))).Distinct();
                        var tenNguoiTrung = string.Join(", ", nguoiTrung);
                        var dvus = trungs.Select(x => $"Dịch vụ trùng: {x.MA_DICH_VU}|{x.TEN_DICH_VU}|{x.NGUOI_THUC_HIEN} có mã nhóm:{x.MA_NHOM}, thực hiện y lệnh:{x.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, ngày kết quả:{x.NGAY_KQ:dd/MM/yyyy HH:mm:ss}").Distinct();
                        var dvsTrung = string.Join("\n", dvus);

                        RaiseError(true,
                            2940,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2940, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} trùng người thực hiện dịch vụ với dịch vụ khác trong hồ sơ XML3:\n" +
                            $"{dvsTrung}\n" +
                            $"Người thực hiện trùng: {tenNguoiTrung}"
                            );
                    }
                    //(a.x2.NGAY_KQ < a.x1.NGAY_TH_YL || a.x2.NGAY_TH_YL > a.x1.NGAY_KQ)
                    //if (File.XML01.XML03s.Any(x => x.XML03ID != File.XML03ID && !string.IsNullOrEmpty(x.MA_DICH_VU) && Uti.NumExists(x.MA_NHOM, 1, 2, 3, 8, 18) && x.NGUOI_THUC_HIEN == File.NGUOI_THUC_HIEN && (!(x.NGAY_KQ <= File.NGAY_TH_YL || x.NGAY_TH_YL >= File.NGAY_KQ) || (x.NGAY_TH_YL <= File.NGAY_TH_YL && x.NGAY_KQ >= File.NGAY_KQ))))
                    //{
                    //    RaiseError(true, 2940);
                    //}

                }


                //2390	4	XML3	NGAY_KQ	[130] Các dịch vụ CĐHA (Nhóm 2) thực hiện nhỏ hơn 3 phút
                //2452	3	XML3	NGAY_KQ	[130] Thời gian thực hiện xét nghiệm (Nhóm 1) không nhỏ hơn 3p
                //2453    3   XML3 NGAY_KQ[130] Thời gian thực hiện TDCN(Nhóm 3) không nhỏ hơn 3p
                if (File.MA_NHOM == 2)
                {
                    if(Field(x=>x.NGAY_TH_YL).NotEmpty())
                    { 
                        if(Field(x=>x.NGAY_KQ).NotEmpty())
                        {
                            if (File.NGAY_TH_YL.Value.AddMinutes(3) >= File.NGAY_KQ)
                            {
                                RaiseError(true,
                                    2390,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2390, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} < 3 phút"
                                    );
                                //RaiseError(true, 2390);
                            }
                        }
                    }
                }
                if (File.MA_NHOM == 1)
                {
                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            if (File.NGAY_TH_YL.Value.AddMinutes(3) >= File.NGAY_KQ)
                            {
                                RaiseError(true,
                                    2452,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2452, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} < 3 phút"
                                    );

                                //RaiseError(true, 2452);
                            }
                        }
                    }
                }
                if (File.MA_NHOM == 3)
                {
                    if(Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            if (File.NGAY_TH_YL.Value.AddMinutes(3) >= File.NGAY_KQ)
                            {
                                RaiseError(true,
                                    2453,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2453, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, thời điểm TH y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss} ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss} < 3 phút"
                                    );
                                //RaiseError(true, 2453);
                            }
                        }
                    }
                    
                }

                #endregion
                #region các lỗi còn lại
                //2192	4	XML3	PP_VO_CAM	[130] Bệnh nhân thực hiện Phẫu thuật, phải nhập Phương pháp Vô Cảm
                //2193	0	XML3	PP_VO_CAM	[130] Xem lại mã Phương pháp vô cảm, nội dung không đúng với loại PT (Phải là Gây tê)
                //2194	0	XML3	PP_VO_CAM	[130] Xem lại mã Phương pháp vô cảm, nội dung không đúng với loại PT (Phải là Gây mê)
                //- Mã "1": Gây mê; 
                //-Mã "2": Gây tê;
                //-Mã "3": Châm tê;
                //-Mã "4": các phương pháp vô cảm khác. 
                //isGayMeGayTe
                ////1 là gây mê
                //2 là gây tê
                //3 là vừa gây mê tây tê
                //-1 không phải gây mê gây tê
                //2073	0	XML3	PP_VO_CAM	[130] Phương pháp vô cảm phải nhập thông tin với dịch vụ PT hoặc TT và phải có giá trị từ 1 đến 4
                if (Field(x => x.MA_NHOM).HasValue(8))
                {
                    Field(x => x.PP_VO_CAM).IsEmpty().ThenError(2192);
                    if (Field(x => x.PP_VO_CAM).NotEmpty())
                    {
                        Field(x => x.PP_VO_CAM).NotValue(1, 2, 3, 4).ThenError(2073);
                        if (isGayMeGayTe(File.TEN_DICH_VU.ToLower()) == 1 && File.PP_VO_CAM != 1)
                        {
                            RaiseError(true,
                                2194,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2194, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Dịch vụ có PP Vô cảm là gây mê, nhưng lại nhập PP Vô cảm:{File.PP_VO_CAM}. Đề nghị kiểm tra lại "
                                );
                            //RaiseError(true, 2194);

                        }
                        else if (isGayMeGayTe(File.TEN_DICH_VU.ToLower()) == 2 && File.PP_VO_CAM != 2)
                        {
                            RaiseError(true,
                                2193,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2193, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Dịch vụ có PP Vô cảm là gây tê, nhưng lại nhập PP Vô cảm:{File.PP_VO_CAM}. Đề nghị kiểm tra lại "
                                );
                            //RaiseError(true, 2193);
                        }
                        else if (isGayMeGayTe(File.TEN_DICH_VU.ToLower()) == -1 && File.MA_DICH_VU.PadRight(2) == "GT" && File.PP_VO_CAM != 2)
                        {
                            RaiseError(true,
                                2193,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2193, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Dịch vụ có PP Vô cảm là gây tê, nhưng lại nhập PP Vô cảm:{File.PP_VO_CAM}. Đề nghị kiểm tra lại "
                                );
                            //RaiseError(true, 2193);
                        }
                    }
                }
                //2196	4	XML3	PP_VO_CAM	[130] Dịch vụ thủ thuật có sử dụng phương pháp vô cảm phải nhập mã phương pháp vô cảm
                if (Field(x => x.MA_NHOM).HasValue(18))
                {
                    if (Field(x => x.PP_VO_CAM).IsEmpty())
                    {
                        if (isGayMeGayTe(File.TEN_DICH_VU.ToLower()) == 1 || isGayMeGayTe(File.TEN_DICH_VU.ToLower()) == 2 || isGayMeGayTe(File.TEN_DICH_VU.ToLower()) == 3)
                        {
                            RaiseError(true,
                                2196,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2196, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân sử dụng thủ thuật có phương pháp vô cảm, nhưng không nhập mã PP Vô cảm. Đề nghị kiểm tra lại "
                                );
                            //RaiseError(true, 2196);
                        }
                        else if (File.MA_DICH_VU.PadRight(2) == "GT")
                        {
                            RaiseError(true,
                                2196,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2196, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân sử dụng thủ thuật có phương pháp vô cảm, nhưng không nhập mã PP Vô cảm. Đề nghị kiểm tra lại "
                                );
                            //RaiseError(true, 2196);
                        }
                    }
                    else
                    {
                        Field(x => x.PP_VO_CAM).NotValue(1, 2, 3, 4).ThenError(2073);
                    }
                }
                //2610	3	XML3	PP_VO_CAM	[130] Phương pháp vô cảm chỉ nhập khi thực hiện PT hoặc TT có xâm lấn
                if (!Field(x => x.MA_NHOM).HasValue(8, 18))
                {
                    if (File.PP_VO_CAM != null)
                    {
                        RaiseError(true,
                            2610,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(2610, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phải PT hoặc TT có xâm lấn nhưng lại nhập PP Vô cảm:{File.PP_VO_CAM}. Đề nghị kiểm tra lại "
                            );
                        //RaiseError(true, 2610);
                    }
                }
                //2417	3	XML3	MA_BENH	[130] Mã ICD không nằm trong bảng ICD của quyết định 4469
                if (Field(x => x.MA_BENH).NotEmpty())
                {
                    var mabenhs = File.MA_BENH.Split(";");
                    foreach (var item in mabenhs)
                    {
                        if (!dmChung130.Any(x => x.LOAI == "ICD10" && x.MA == item))
                        {
                            RaiseError(true,
                                2417,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2417, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, mã bệnh kèm theo:{item} không tồn tại trong danh mục ICD10 của quyết định 4469/QĐ-BYT ngày 28/12/2015 của Bộ trưởng Bộ Y tế. Đề nghị kiểm tra lại "
                                );
                           // RaiseError(true, 2417);
                        }
                    }

                }
                //2488	3	XML3	MA_BENH_YHCT	[13] Không tồn mã Mã Bệnh YHCT trong Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế

                if (Field(x => x.MA_BENH_YHCT).NotEmpty())
                {
                    var mabenhs = File.MA_BENH_YHCT.Split(";");
                    foreach (var item in mabenhs)
                    {
                        if (!dmYHCT.Any(x => x.YHDTCode == item))
                        {
                            RaiseError(true,
                                2488,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(2488, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, mã bệnh YHCT kèm theo:{item} không tồn tại trong danh mục Mã Bệnh YHCT trong Quyết định số 6061/QĐ-BYT ngày 29/12/2017 của Bộ trưởng Bộ Y tế. Đề nghị kiểm tra lại "
                                );
                            //RaiseError(true, 2488);
                        }
                    }

                }

                ////942	1	XML3	MA_BAC_SI	Khám bệnh không đúng chuyên môn của CCHN bác sĩ
                if (Field(x => x.MA_NHOM).HasValue(13))
                {
                    if (dmChung130.Any(x => x.LOAI == "KHAMCN" && Uti.CodeMatch(File.MA_DICH_VU, x.TEN)))
                    {
                        var strChungDanhNN = dmChung130.Where(x => x.LOAI == "KHAMCN" && Uti.CodeMatch(File.MA_DICH_VU, x.TEN)).Select(x => x.MA).FirstOrDefault();
                        foreach (var item in File.MA_BAC_SI.Split(";"))
                        {
                            var bacsis = dmBACSI.Where(bs => bs.MA_CCHN != null && bs.MA_CCHN.Contains(item) && Uti.IsNumEqual(bs.CHUC_DANH_NN, 1)); //bs.LOAI != null && bs.LOAI.Contains("BS"));
                            //Linhist changed 27/12/23 -> them check PHAMVI_CM
                            //if (!bacsis.Any(bs => Uti.CodeMatch(bs.MA_CHUYENNGANH, strChungDanhNN) || Uti.CodeMatch(bs.PHAMVI_CM, strChungDanhNN) || Uti.CodeMatch(bs.MA_KHOA, File.MA_DICH_VU)))
                            if (!bacsis.Any(bs => Uti.CodeMatch(bs.PHAMVI_CM, strChungDanhNN) || Uti.CodeMatch(bs.PHAMVI_CMBS, File.MA_DICH_VU) || Uti.CodeMatch(bs.MA_KHOA, File.MA_DICH_VU)))
                            {
                                RaiseError(true,
                                    942,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(942, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, mã bác sĩ:{item} không đúng chuyên môn của CCHN bác sĩ. Đề nghị kiểm tra lại "
                                    );
                                //RaiseError(true, 942);
                            }
                        }

                    }
                }

                //2911	3	XML3	NGUOI_THUC_HIEN	Sơn La: Các Dịch vụ Nội soi cần có tối thiểu 3 người thực hiện, Trong đó phải có 1 Bác Sĩ
                if (Field(x => x.MA_DICH_VU).NotEmpty())
                {
                    if (Uti.HCodeContain(File.MA_DICH_VU, "01.0232.0140;02.0252.0502;02.0253.0135;02.0255.0319;02.0259.0137;02.0261.0319;02.0262.0136;02.0264.0140;02.0265.0140;02.0266.0157;02.0267.0140;02.0271.0140;02.0285.0140;02.0286.0497;02.0294.0137;02.0295.0498;02.0296.0500;02.0298.0140;02.0304.0134;02.0305.0135;02.0306.0137;02.0307.0136;03.0154.0502;03.0155.0140;03.0157.0140;03.1040.0497;03.1049.0140;03.1056.0140;03.1057.0140;03.1059.0500;03.1061.0134;03.1061.0135;03.1062.0137;03.1063.0500;03.1064.0184;03.1066.0136;03.1067.0498;15.0235.0926;15.0235.0928;20.0067.0140;20.0070.0500;20.0071.0184;20.0073.0136;20.0079.0134;20.0080.0135;20.0081.0137;"))
                    {
                        Field(x => x.NGUOI_THUC_HIEN).IsEmpty().ThenError(2911);
                        if (Field(x => x.NGUOI_THUC_HIEN).NotEmpty())
                        {
                            var nguoithuchien = File.NGUOI_THUC_HIEN.Split(";");
                            if (nguoithuchien.Count() < 3)
                            {
                                RaiseError(true,
                                    2911,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2911, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, dịch vụ nội soi cần có tối thiểu 3 người thực hiện, Trong đó phải có 1 Bác Sĩ. Đề nghị kiểm tra lại\n " +
                                    $"Số người thực hiện hiện tại là:{nguoithuchien.Count()}. Cần bổ sung thêm"
                                    );
                                //RaiseError(true, 2911);
                            }
                            else
                            {
                                var hasBS = false;
                                foreach (var m in nguoithuchien)
                                {
                                    if (dmBACSI.Any(x => x.MA_CCHN == m && Uti.IsNumEqual(x.CHUC_DANH_NN, 1)))
                                    {
                                        hasBS = true;
                                    }
                                }
                                if (!hasBS)
                                {
                                    RaiseError(true,
                                        2911,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2911, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, dịch vụ nội soi cần có tối thiểu 3 người thực hiện, Trong đó phải có 1 Bác Sĩ. Đề nghị kiểm tra lại\n "+
                                        $"Trong danh sách thực hiện không có ai là Bác sĩ"
                                        );
                                    //RaiseError(true, 2911);
                                }
                            }
                        }

                    }
                }
                //2611	3	XML3	NGUOI_THUC_HIEN     [130] Điều dưỡng thực hiện dịch vụ kỹ thuật nằm ngoài Phục lục 12 TT 32 / 2023 / TT - BYT
                //2612    3   XML3 NGUOI_THUC_HIEN     [130] Điều dưỡng thực hiện dịch vụ kỹ thuật(Dấu*) khi chưa có trình độ DH, không đúng Phục lục 12 TT 32 / 2023 / TT - BYT
                if (!File.NGUOI_THUC_HIEN.Contains(";"))
                {
                        var basi = dmBACSI.Where(x => x.MA_CCHN == File.NGUOI_THUC_HIEN).FirstOrDefault();
                        //301 Điều dưỡng có trình độ từ DH trở lên
                        //302 Điều dưỡng có trình độ dưới ĐH
                        //303 Điều dưỡng có VB Hồi sức cấp cứu
                        //304 DD có VB nội khoa
                        //305 DD có VB Da Liễu
                        //306 DD có văn bằng Tâm Thần
                        //307 DD có văn bằng Nội tiết
                        //308 DD có văn bằng Y học cổ truyền
                        //309 DD có văn bằng Gây mê
                        //310 DD có văn bằng Ngoại
                        //311 DD có văn bằng Bỏng
                        //312 DD có văn Bằng khoa U Biếu
                        //313 DD có VB khoa Sản, phujk hoa
                        //314 DD có VB khoa Mắt
                        //315 DD có VB khoa Tai mũi
                        //316 DD có VB khoa Răng
                        //317 DD có VB Khoa Phục hồi chức năng
                        //318 DD có VB khoa Thăm dò chức năng
                        //319 DD có VB khoa Huyết học
                        if (basi != null)
                        {
                            //B1. Kiểm tra xem có phải điều dưỡng hay không?
                            var phamvichuyenmon = basi.PHAMVI_CM.ToString().Split(";");
                            var isDD = false;
                            var isDDdH = false;
                            foreach (var s in phamvichuyenmon)
                            {
                                if (Uti.CodeMatch(s, "301;302;303;304;305;306;307;308;309;310;311;312;323;314;315;316;317;318;319"))
                                {
                                    //Gans neeu la dieu duong
                                    isDD = true;
                                }
                                if (Uti.CodeMatch(s, "301"))
                                {
                                    //Gans neu la dieu duong dai hoc
                                    isDDdH = true;
                                }

                            }

                        if (isDD)
                        {

                            //Linhist removed 19/03/26 - lỗi 3622 chỉ kiểm tra bs, ko kiểm tra người thực hiện
                            ////3622	3	XML3	MA_DICH_VU	[3176] Thanh Hóa: Điều Dưỡng không được phép y lệnh Giường cho Bệnh nhân (Chuyên đề bảo hiểm)
                            //if (Field(x => x.MA_NHOM).HasValue(14, 15))
                            //{
                            //    RaiseError(true,
                            //        3622,
                            //        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            //        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            //        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            //        $"Nội dung lỗi: {(errorlist.TryGetValue(3622, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            //        $"Chi tiết:" +
                            //        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, dịch vụ giường bệnh do điều dưỡng thực hiện. Đề nghị kiểm tra lại\n "+
                            //        $"{File.NGUOI_THUC_HIEN} là Điều Dưỡng"
                            //        );
                            ////RaiseError(true, 3622);
                            //}

                            //Dich vu khong co trong PL 12, =>DD Thuc hien Bao Loi
                            //Regex.Replace(input, @"\s*\[.*?\]", "");

                            if (!dmChung130.Any(x => x.LOAI == "TT32" && x.MA == "PL12" && x.TEN.Contains(Regex.Replace(File.TEN_DICH_VU, @"\s*\[.*?\]", ""))))
                            {
                                if(basi.DVKT_KHAC !=null && basi.DVKT_KHAC.Length>7 && !basi.DVKT_KHAC.Contains(File.MA_DICH_VU.Substring(0,7)))
                                {
                                        RaiseError(true,
                                        2611,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(2611, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, dịch vụ kỹ thuật không nằm trong Phục lục 12 Thông tư 32/2023/TT-BYT do Điều dưỡng thực hiện. Đề nghị kiểm tra lại\n " +
                                        $"{File.NGUOI_THUC_HIEN} là Điều Dưỡng"
                                        );
                                }
                                //RaiseError(true, 2611);
                            }
                                
                                //Dich vu dau *, Dieu duong ko phai dai hoc tro len thuc hien bao loi
                                if (dmChung130.Any(x => x.LOAI == "TT32" && x.MA == "DDDaiHoc" && x.TEN.ToLower().Trim()== File.TEN_DICH_VU.ToLower().Trim()))
                                {
                                    if (!isDDdH)
                                    {
                                        if (basi.DVKT_KHAC != null && basi.DVKT_KHAC.Length > 7 && !basi.DVKT_KHAC.Contains(File.MA_DICH_VU.Substring(0, 7)))
                                        {
                                            RaiseError(true,
                                                2612,
                                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(2612, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, dịch vụ kỹ thuật(Dấu*) do Điều dưỡng thực hiện khi chưa có trình độ Đại học trở lên. Đề nghị kiểm tra lại\n " +
                                                $"{File.NGUOI_THUC_HIEN} là Điều Dưỡng, có trình độ dưới Đại Học"
                                                );
                                        }
                                    //RaiseError(true, 2612);
                                    }
                                }

                            }
                        }
                }
                
                #endregion
                #region check các lỗi chuyên đề BH 2025
                //927	5	XML3	MA_DICH_VU	(Chuyên đề Bảo hiểm): Định lượng Pro calcitionin không được phép chỉ định đồng thời với DVKT Định lượng CRP/CRP hs (theo TT35/2016/TT-BYT)
                if(File.MA_DICH_VU== "23.0130.1549")
                {
                    if(File.XML01.XML03s.Any(x=>Uti.HCodeContain(x.MA_DICH_VU, "23.0130.1483;23.0050.1484")))
                    {
                        RaiseError(true,
                            927,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(927, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Dịch vụ định lượng Pro calcitionin không được phép chỉ định đồng thời với DVKT Định lượng CRP/CRP hs. Đề nghị kiểm tra lại "
                            );
                        //RaiseError(true, 927);
                    }
                }    
                //3343	3	XML3	MA_DICH_VU	[3176] Bắc Giang: Dịch vụ kỹ thuật “Phục hồi cổ răng bằng Composite” chỉ định quá 01 lần trong Hồ sơ bệnh án (Chuyên đề bảo hiểm KT40)
                if (Uti.HCodeContain(File.MA_DICH_VU, "16.0071.1018;16.0072.1018"))
                {
                    if (File.SO_LUONG > 1)
                    {
                        RaiseError(true,
                            3343,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3343, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Dịch vụ kỹ thuật “Phục hồi cổ răng bằng Composite” chỉ định quá 01 lần trong Hồ sơ bệnh án. Đề nghị kiểm tra lại "
                            );
                        //RaiseError(true, 3343);
                    }
                    if(File.XML01.XML03s.Count(x=>Uti.HCodeContain(x.MA_DICH_VU, "16.0071.1018;16.0072.1018"))>1)
                    {
                        RaiseError(true,
                            3343,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(3343, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết:" +
                            $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Dịch vụ kỹ thuật “Phục hồi cổ răng bằng Composite” chỉ định quá 01 lần trong Hồ sơ bệnh án. Đề nghị kiểm tra lại "
                            );
                        //RaiseError(true, 3343);
                    }
                }    
                // //2391	4	XML3	NGAY_TH_YL	[130] Thời gian thực hiện y lệnh trùng thời gian kết quả
                if (Field(x => x.MA_NHOM).NotValue(10))
                {
                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            if (File.NGAY_TH_YL == File.NGAY_KQ)
                            {
                                RaiseError(true,
                                    2391,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2391, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Thời gian thực hiện y lệnh trùng thời gian kết quả. Đề nghị kiểm tra lại \n"+
                                    $"Ngày thực hiện y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Ngày kết quả: {File.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                                    );
                               // RaiseError(true, 2391);
                            }
                        }
                    }

                }
                //4006	3	XML3	MA_DICH_VU	[3176] [Hưng Yên] Giường nội trú ban ngày phải được thực hiện trong giờ hành chính (6h45 - 17h00)
                if (File.MA_NHOM==14)
                {
                    if (Field(x => x.NGAY_TH_YL).NotEmpty())
                    {
                        if (Field(x => x.NGAY_KQ).NotEmpty())
                        {
                            if (File.NGAY_TH_YL.Value.TimeOfDay < new TimeSpan(6,45,0) || File.NGAY_KQ.Value.TimeOfDay > new TimeSpan(17,0,0))
                            {
                                RaiseError(true,
                                    4006,
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(4006, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Giường nội trú ban ngày phải được thực hiện trong giờ hành chính (6h45 - 17h00). Đề nghị kiểm tra lại \n" +
                                    $"Ngày thực hiện y lệnh: {File.NGAY_TH_YL:dd/MM/yyyy HH:mm:ss}, Ngày Kết quả:{File.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                                    );
                                //RaiseError(true, 4006);
                            }
                        }
                    }
                }
                //3327	3	XML3	MA_DICH_VU	[3176] Chuyên đề bảo hiểm:  Siêu âm ổ bụng sau PT  ko phù hợp tình trạng bệnh
                //haimaster 27/9/2025 update lai code
                if (File.MA_NHOM==18)
                //if (Uti.CodeContain(File.MA_DICH_VU, "18.0015.0001;02.0314.0001"))
                {
                    if (File.NGAY_YL.HasValue)
                    {
                        var xmlPT = File.XML01.XML03s.Where(x => Uti.HCodeContain(x.MA_DICH_VU, "27.0191.0451;27.0187.2039;27.0188.2039;03.4078.0451;03.4071.2039;10.0510.0459") && x.NGAY_KQ!=null && x.NGAY_TH_YL>File.NGAY_YL).ToList();
                        if (xmlPT.Any())
                        {
                            foreach (var x in xmlPT)
                            {
                                if (File.NGAY_YL >=x.NGAY_KQ)
                                {
                                    RaiseError(true,
                                        3327,
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(2391, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Bệnh nhân được y lệnh Siêu âm ổ Bụng sau Phẫu thuật là không hợp lý \n" +
                                    $"{x.MA_DICH_VU}|{x.TEN_DICH_VU}, Ngày thực hiện y lệnh: {x.NGAY_YL:dd/MM/yyyy HH:mm:ss}, Ngày kết quả: {x.NGAY_KQ:dd/MM/yyyy HH:mm:ss}"
                                    );
                                    //iseError(true, 3327);
                                }    
                            }
                        }
                    }
                }

                #endregion

                //_wa.Stop();
                //if (_wa.Elapsed.TotalSeconds > .3d)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 - Các chuyên đề, thời gian: {_wa.Elapsed.TotalSeconds.ToString()}", nameof(Validate));
                //}
                //_wa.Restart();

                #region Check HDSD Dịch vụ & dịch vụ đồng thời, sẽ kiểm tra thêm TH có 3 dịch vụ khác nhau cùng 1 lúc

                //Update 7.8.2025 Haimaster recode
                // Sẽ tách làm 2 luồng code, luồng check DV và Vật tư, Luồng 2 kiểm tra vật tư rồi check ngược xem có kê DV hay không?
                if (isDichVu)
                {
                    await Validate_HDSDDV();

                    foreach (var rule in ruleDongthoi)
                   {
                        #region Case Chỉ Định
                        if (rule.CHI_DINH == true)
                        {
                            if (Uti.CodeContain(File.MA_DICH_VU, rule.MA_DICH_VU)) //Đúng mã dịch vụ cần kiểm tra
                            {
                                if (rule.CUNG_HO_SO == true)// Kiểm tra cùng hồ sơ
                                {
                                    //Kiểm tra dịch vụ khác
                                    if (!String.IsNullOrEmpty(rule.DICH_VU_KHAC)) //Nếu dịch vụ khác có nội dung
                                    {
                                        //Case kiểm tra xem có chỉ định cùng DV khac shay khong? nếu ko có báo lỗi 
                                        //if (!File.XML01.XML03s.Any(x => Uti.CodeContain(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL.HasValue && x.NGAY_TH_YL.HasValue && (x.NGAY_YL.Value.Date == File.NGAY_YL.Value.Date || x.NGAY_TH_YL.Value.Date == File.NGAY_TH_YL.Value.Date)))
                                        if (!File.XML01.XML03s.Any(x => Uti.CodeContain(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
                                        {
                                            if (!File.XML01.Errors.Any(x => x.MA_LOI == rule.MA_LOI && x.STT == File.STT))
                                            {
                                                RaiseError(true,
                                                    (rule.MA_LOI ?? 0),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI ?? 0, out var ErInfo) ? ErInfo : "Không tìm thấy nội dung lỗi")}\n"+
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}.\n" +
                                                    $"Dịch vụ {rule.MA_DICH_VU}|{File.TEN_DICH_VU} cần phải chỉ định đồng thời với dịch vụ {rule.DICH_VU_KHAC}. Đề nghị kiểm tra lại "
                                                    );
                                                //RaiseError(true, (int)rule.MA_LOI); //Gán lỗi
                                            }
                                        }
                                    }
                                    //Kiểm tra vật tư khác (kê dịch vụ nhưng ko có vật tư)
                                    if (!String.IsNullOrEmpty(rule.VAT_TU_BHYT)) //Nếu vật tư khác có nội dung
                                    {
                                        //if (!File.XML01.XML03s.Any(x => Uti.CodeContain(x.MA_VAT_TU, rule.VAT_TU_BHYT) && x.NGAY_YL.HasValue && x.NGAY_TH_YL.HasValue && (x.NGAY_YL.Value.Date == File.NGAY_YL.Value.Date || x.NGAY_TH_YL.Value.Date == File.NGAY_TH_YL.Value.Date))))
                                        //Kiểm tra kê dịch vụ nhưng ko có vật tư
                                        if (!File.XML01.XML03s.Any(x => Uti.CodeContain(x.MA_VAT_TU, rule.VAT_TU_BHYT)))
                                        {
                                            if (!File.XML01.Errors.Any(x => x.MA_LOI == rule.MA_LOI && x.STT == File.STT))
                                            {
                                                RaiseError(true,
                                                    (rule.MA_LOI ?? 0),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI ?? 0, out var ErInfo) ? ErInfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}.\n" +
                                                    $"Dịch vụ {rule.MA_DICH_VU}|{File.TEN_DICH_VU} cần phải chỉ định đồng thời với vật tư khác như {rule.VAT_TU_BHYT}. Đề nghị kiểm tra lại "
                                                    );
                                                //RaiseError(true, (int)rule.MA_LOI); //Gán lỗi
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Case Chống chỉ định
                        if(rule.CHI_DINH==false)
                        {
                           if(File.NGAY_TH_YL.HasValue && File.NGAY_YL.HasValue)//Kiểm tra ngày y lệnh và ngày thực hiện y lệnh có giá trị hay không
                           {
                             if (Uti.CodeContain(rule.MA_DICH_VU,File.MA_DICH_VU)) //Đúng mã dịch vụ cần kiểm tra
                             {
                                 if (!String.IsNullOrEmpty(rule.DICH_VU_KHAC)) //Nếu dịch vụ khác có nội dung
                                 {
                                    if (rule.CUNG_THOI_DIEM == true) //Không được phép chỉ định 2 dịch vụ cùng thời điểm =>Update cùng thời điểm là cùng ngày và cùng giờ
                                    {
                                       if (File.XML01.XML03s.Any(x => Uti.CodeContain(rule.DICH_VU_KHAC,x.MA_DICH_VU) && x.NGAY_YL.HasValue && x.NGAY_TH_YL.HasValue && (x.NGAY_YL.Value == File.NGAY_YL.Value || x.NGAY_TH_YL.Value == File.NGAY_TH_YL.Value)))
                                       {
                                          if (!File.XML01.Errors.Any(x => x.MA_LOI == rule.MA_LOI && x.STT == File.STT))
                                          {
                                                    RaiseError(true,
                                                    (rule.MA_LOI ?? 0),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI ?? 0, out var ErInfo) ? ErInfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}.\n" +
                                                    $"Dịch vụ {rule.MA_DICH_VU}|{File.TEN_DICH_VU} chống chỉ định đồng thời điểm với  {rule.DICH_VU_KHAC}. Đề nghị kiểm tra lại "
                                                    );
                                                    //RaiseError(true, (int)rule.MA_LOI); //Gán lỗi
                                          }
                                       }
                                    }
                                    if(rule.CUNG_NGAY_YL == true) 
                                    {
                                        if(!rule.DICH_VU_KHAC.Contains(";"))
                                        {
                                            if (File.XML01.XML03s.Any(x => Uti.CodeContain(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL.HasValue && x.NGAY_TH_YL.HasValue && (x.NGAY_YL.Value.Date == File.NGAY_YL.Value.Date || x.NGAY_TH_YL.Value.Date == File.NGAY_TH_YL.Value.Date)))
                                            {
                                                if (!File.XML01.Errors.Any(x => x.MA_LOI == rule.MA_LOI && x.STT == File.STT))
                                                {
                                                        RaiseError(true,
                                                    (rule.MA_LOI ?? 0),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI ?? 0, out var ErInfo) ? ErInfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}.\n" +
                                                    $"Dịch vụ {rule.MA_DICH_VU}|{File.TEN_DICH_VU} cần phải chỉ định cùng ngày với dịch vụ {rule.DICH_VU_KHAC}. Đề nghị kiểm tra lại "
                                                    );

                                                        //RaiseError(true, (int)rule.MA_LOI); //Gán lỗi
                                                }
                                            }
                                        }
                                        else
                                        {
                                                if (File.XML01.XML03s.Count(x => Uti.CodeContain(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL.HasValue && x.NGAY_TH_YL.HasValue && (x.NGAY_YL.Value.Date == File.NGAY_YL.Value.Date || x.NGAY_TH_YL.Value.Date == File.NGAY_TH_YL.Value.Date))>=3)
                                                {
                                                    if (!File.XML01.Errors.Any(x => x.MA_LOI == rule.MA_LOI && x.STT == File.STT))
                                                    {
                                                        RaiseError(true,
                                                    (rule.MA_LOI ?? 0),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(rule.MA_LOI ?? 0, out var ErInfo) ? ErInfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}.\n" +
                                                    $"Dịch vụ {rule.MA_DICH_VU}|{File.TEN_DICH_VU} cần phải chỉ định trong cùng một đợt điều trị với dịch vụ {rule.DICH_VU_KHAC}. Đề nghị kiểm tra lại "
                                                    );
                                                        //RaiseError(true, (int)rule.MA_LOI); //Gán lỗi
                                                    }
                                                }
                                        }    
                                    } 
                                            
                                 }
                             }
                           }    
                        }    
                        #endregion
                   }


                    #region code cũ
                    //List<string> errX3IDs = new List<string>();


                    //foreach (var rule in ruleDongthoi)
                    //{
                    //    //Linhist added 04/06/22
                    //    //rule == null return la sai & them kiem tra MA_LOI, de gan 
                    //    //if (rule == null) return;
                    //    //if (string.IsNullOrEmpty(rule.MA_DICH_VU)) continue;
                    //    if (rule == null || string.IsNullOrEmpty(rule.MA_DICH_VU) || rule.MA_LOI == null) continue;

                    //    var matchDVs = xml.XML01.XML03s.Where(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU));
                    //    //var matchVTs = xml.XML01.XML03s.Where(x => Uti.CodeContain(x.MA_VAT_TU, rule.VAT_TU_BHYT));
                    //    // 20.7/2022 haimaster sua oode => Tach check dich vu rieng, vat tu ring
                    //    //Case co CHI DINH cho dich vu:
                    //    if (rule.CHI_DINH == true)
                    //    {
                    //        //Kiem tra DVKT vs VTBH
                    //        if (!string.IsNullOrEmpty(rule.VAT_TU_BHYT))
                    //        {
                    //            if (rule.CUNG_HO_SO == true)
                    //            {
                    //                var matchVTs = xml.XML01.XML03s.Where(x => Uti.CodeMatch(x.MA_VAT_TU, rule.VAT_TU_BHYT));

                    //                //Linhist changed 16/07/22 thay doi kiem tra chi co 1 trong 2 DV hoac VT (ko the co ca hai)
                    //                //haimaster doi lai
                    //                if (!matchDVs.Any() || !matchVTs.Any())

                    //                //if (matchDVs.Any() != matchVTs.Any())

                    //                {

                    //                    foreach (var xml3 in matchDVs)
                    //                    {
                    //                        RaiseError(true, (int)rule.MA_LOI);
                    //                        if(!File.XML01.Errors.Any(x => x.XML01ID  == xml3.XML01ID && x.MA_LOI == rule.MA_LOI && x.STT == xml3.STT))
                    //                        {
                    //                            RaiseError(true, (int)rule.MA_LOI);
                    //                        }
                    //                        //if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
                    //                        //{
                    //                        //    xml3.XML1.XML1Errors.Add(new XML1Error()
                    //                        //    {
                    //                        //        //XEXML1ID =xml3.XML1ID,
                    //                        //        XEErrorCode = rule.MA_LOI,
                    //                        //        XEErrorRow = xml3.STT
                    //                        //    });
                    //                        //}
                    //                    }
                    //                    //20.7.2022 haimaster them code
                    //                    //var haimasterGetDVtheoVT = ruleDongThois.Where(x=> Uti.CodeMatch(x.VAT_TU_BHYT,a)
                    //                    foreach (var xml3 in allVatTus)
                    //                    {
                    //                        var haimasterGetDVtheoVT = ruleDongThois.Where(x => Uti.CodeMatch(x.VAT_TU_BHYT, xml3.MA_VAT_TU) && Uti.CodeMatch(x.MA_DICH_VU, allDVsHaimaster));
                    //                        if (!haimasterGetDVtheoVT.Any())
                    //                        {

                    //                            if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
                    //                            {
                    //                                xml3.XML1.XML1Errors.Add(new XML1Error()
                    //                                {
                    //                                    //XEXML1ID =xml3.XML1ID,
                    //                                    XEErrorCode = rule.MA_LOI,
                    //                                    XEErrorRow = xml3.STT
                    //                                });
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }

                    //        //Kiem tra DVKT vs DVKT khac
                    //        if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC))
                    //        {
                    //            if (rule.CUNG_NGAY_YL == true)
                    //            {
                    //                var dates = matchDVs.Select(x => x.NGAY_YL == null ? (DateTime?)null : x.NGAY_YL.Value.Date).Distinct();

                    //                foreach (var dte in dates)
                    //                {
                    //                    if (dte == null) continue;

                    //                    if (!allDvKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
                    //                    {
                    //                        foreach (var xml3 in matchDVs.Where(x => x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
                    //                        {
                    //                            if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
                    //                            {
                    //                                xml3.XML1.XML1Errors.Add(new XML1Error()
                    //                                {
                    //                                    //XEXML1ID =xml3.XML1ID,
                    //                                    XEErrorCode = rule.MA_LOI,
                    //                                    XEErrorRow = xml3.STT
                    //                                });
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    //Case chong CHI DINH:
                    //    else
                    //    {
                    //        //Kiem tra DVKT vs DVKT khac cung thoi diem
                    //        if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC))
                    //        {
                    //            if (rule.CUNG_THOI_DIEM == true)
                    //            {
                    //                foreach (var xml3 in matchDVs)
                    //                {
                    //                    if (xml3.NGAY_YL == null) continue;

                    //                    if (allDvKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null && x.NGAY_YL == xml3.NGAY_YL))
                    //                    {
                    //                        if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
                    //                        {
                    //                            xml3.XML1.XML1Errors.Add(new XML1Error()
                    //                            {
                    //                                //XEXML1ID =xml3.XML1ID,
                    //                                XEErrorCode = rule.MA_LOI,
                    //                                XEErrorRow = xml3.STT
                    //                            });
                    //                        }
                    //                    }
                    //                }
                    //            }

                    //            //Linhist added 04/06/22
                    //            //Kiem tra DVKT vs DVKT khac cung ngay YL
                    //            if (rule.CUNG_NGAY_YL == true)
                    //            {
                    //                var dates = matchDVs.Select(x => x.NGAY_YL == null ? (DateTime?)null : x.NGAY_YL.Value.Date).Distinct();

                    //                foreach (var dte in dates)
                    //                {
                    //                    if (dte == null) continue;

                    //                    if (allDvKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
                    //                    {
                    //                        foreach (var xml3 in matchDVs.Where(x => x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
                    //                        {
                    //                            if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
                    //                            {
                    //                                xml3.XML1.XML1Errors.Add(new XML1Error()
                    //                                {
                    //                                    //XEXML1ID =xml3.XML1ID,
                    //                                    XEErrorCode = rule.MA_LOI,
                    //                                    XEErrorRow = xml3.STT
                    //                                });
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                }

                #region Tạm bỏ code check Vật tư dịch vụ cùng thời điểm
                //haimaster tạm bỏ
                //if (!isDichVu)
                //{
                //    foreach (var rule in ruleDongthoi)
                //    {
                //        #region Case Chỉ Định
                //        if (rule.CHI_DINH == true)
                //        {
                //            if (Uti.CodeContain(File.MA_VAT_TU, rule.VAT_TU_BHYT)) //Đúng mã dịch vụ cần kiểm tra
                //            {
                //                if (rule.CUNG_HO_SO == true)// Kiểm tra cùng hồ sơ
                //                {
                //                    if (!File.XML01.XML03s.Any(x => Uti.CodeContain(x.MA_DICH_VU, rule.MA_DICH_VU)))
                //                    {
                //                        if (!File.XML01.Errors.Any(x => x.MA_LOI == rule.MA_LOI && x.STT == File.STT))
                //                        {
                //                            RaiseError(true, (int)rule.MA_LOI); //Gán lỗi
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //        #endregion
                //    }
                //}
                #endregion
                #endregion

                //_wa.Stop();
                //if (_wa.Elapsed.TotalMilliseconds > 500)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 {File.XML03ID} - Ma CSKCB {File.MA_CSKCB} - Ma LK {File.MA_LK} - XML01ID {File.XML01ID} - HDSD Dịch vụ , thời gian: {_wa.Elapsed.TotalMilliseconds.ToString()}", nameof(Validate));
                //}

                //_watch.Stop();
                //if(_watch.Elapsed.TotalMilliseconds > 500)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 {File.XML03ID} - Ma LK {File.XML01.MA_LK} - XML01ID {File.XML01ID}, thời gian: {_watch.Elapsed.TotalMilliseconds}", nameof(Validate));
                //}

                #region Lỗi mới
                //Linhist added 110826
                //4213	2	XML3	MA_DICH_VU	Thanh toán siêu âm hệ tiết niệu, siêu âm tử cung phần phụ có ngày y lệnh sau thời gian siêu âm ổ bụng không phù hợp theo Quyết định số 3805/QĐ-BYT ngày 25/9/2014
                if (Uti.CodeContain(File.MA_DICH_VU, _SieuAmLienQuan) && File.NGAY_YL != null)
                {
                    var saOB = File.XML01.XML03s.FirstOrDefault(x => Uti.CodeContain(x.MA_DICH_VU, _SieuAmOBungs) && x.NGAY_YL != null && x.NGAY_YL.HasValue && x.NGAY_YL.Value.Date == File.NGAY_YL.Value.Date && x.NGAY_YL.Value <= File.NGAY_YL.Value);
                    if (saOB != null)
                    {
                        RaiseError(true, 4213,
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(4213, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, Ngày y lệnh: {File.NGAY_YL:dd/MM/yyyy HH:mm:ss}. \n" +
                                $"Nhưng đã có y lệnh siêu âm ổ bụng cùng ngày trước đó: {saOB.MA_DICH_VU}|{saOB.TEN_DICH_VU}, Ngày y lệnh: {saOB.NGAY_YL:dd/MM/yyyy HH:mm:ss}"
                            );
                        //RaiseError(true, 4213);
                    }
                }
                #endregion

                //Linhist added 27/08/26 - cho lỗi mức hưởng set vào XMLInfo (ko kiểm tra cho vật tư)
                //762   1   XML3    MUC_HUONG   Sai mức hưởng BHYT
                if (!Uti.IsValidDate(File.XML01.NGAY_MIEN_CCT))
                {
                    if ((File.MA_NHOM != 10 && File.MA_NHOM != 11) && Info.MAX_MUC_HUONG > 0d && !Uti.IsEqualUp(File.MUC_HUONG, Info.MAX_MUC_HUONG))
                    {
                        RaiseError(true,
                            762,
                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            $"Nội dung lỗi: {(errorlist.TryGetValue(761, out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            $"Chi tiết: Mức hưởng BHYT của bệnh nhân là {File.MUC_HUONG}, không đúng với mức hưởng quy định là {Info.MAX_MUC_HUONG}, đề nghị xem lại"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: MaBN: {File.XML01.MA_BN} MaCSKCB: {File.XML01.MA_CSKCB}; STT Bị Lỗi: {File.STT} XML3 Giám định có lỗi: {ex.ToString()} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
            }
            return true;
        }

        private const string _SieuAmOBungs = "02.0314.0001;18.0015.0001";
        private const string _SieuAmLienQuan = "18.0016.0001;18.0018.0001;18.0019.0001;18.0030.0001";

        private void Validate_DICHVU(XML03 xml)
        {
            if (xml.MA_DICH_VU != curMADICHVU)
            {
                curMADICHVU = xml.MA_DICH_VU;
                if (!string.IsNullOrEmpty(curMADICHVU))
                {
                    cungMADICHVUs = dmDICHVU.Where(t => t.MA_DICH_VU == curMADICHVU).ToList();
                    //cungHDSDs = hdsdDV.Where(t => t.MA_DICH_VU.Contains(curMADICHVU)).ToList();

                    //Linhist changed 25/03/26 - thêm trg hợp MA_DICH_VU == ""
                    //cungHDSDs = hdsdDV
                    //            .Where(t => (t.MA_DICH_VU ?? "")
                    //            .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    //            .Any(ma => ma.Trim() == curMADICHVU))
                    //            .ToList();
                    cungHDSDs = hdsdDV.Where(t => string.IsNullOrEmpty(t.MA_DICH_VU) || Uti.CodeMatch(curMADICHVU, t.MA_DICH_VU)).ToList();
                }
            }

        }


        private void Validate_VATTU(XML03 xml)
        {
            try
            {
                if (xml.MA_VAT_TU != curMAVATTU)
                {
                    //if (xml.MA_VAT_TU.Substring(0,24) != curMAVATTU)
                    //{
                    //    curMAVATTU = xml.MA_VAT_TU.Substring(0,24).Trim();
                    //}else curMAVATTU = xml.MA_VAT_TU;
                    //cungMAVATTUs = dmVTYT.Where(t => t.MA_VAT_TU.StartsWith(curMAVATTU)).ToList();
                    //if(cungMAVATTUs.Count() == 0) { cungMAVATTUs = dmVTYT.Where(t => t.MA_VAT_TU == xml.MA_VAT_TU).ToList(); } 
                    if (xml.MA_VAT_TU.Length > 24)
                    {
                        if (xml.MA_VAT_TU.Substring(0, 24) != curMAVATTU)
                        {
                            curMAVATTU = xml.MA_VAT_TU.Substring(0, 24);
                        }
                    }
                    else
                    {
                        curMAVATTU = xml.MA_VAT_TU;
                    }

                    cungMAVATTUs = dmVTYT.Where(t => t.MA_VAT_TU == curMAVATTU || t.MA_VAT_TU==xml.MA_VAT_TU).ToList();

                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private async Task Validate_HDSDDV()
        {
            //Linhist added 07/07/26
            if (File.PHAM_VI != 1) return;

            if (cungHDSDs == null || !cungHDSDs.Any()) return;
            //Kiem tra rieng case CHONG CHI DINH, co THOI GIAN TOI THIEU:

            // cần mã bệnh ICD10 --> Info.MA_BENH_ICD10
            // Cần có thai --> Info.CO_THAI
            // Cần cho con bú --> Info.CHO_CON_BU
            // Cần tuổi BN --> Info.TUOI_BN
            var dichvuKhacs = File.XML01?.XML03s.Where(x => x.STT != File.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));
            //haimaster update 26.9.2025 Update check phan Rule Chong CHi Dinh Thoi Gian Toi thieu
            //var ruleToiThieus = cungHDSDs.Where(x => x.CHI_DINH == false && x.THOI_GIAN_TOI_THIEU != null && x.THOI_GIAN_TOI_THIEU != 0 && !string.IsNullOrEmpty(x.MA_BENH) && Uti.CodeMatch(Info.MA_BENH_ICD10, x.MA_BENH));
            //if (!ruleToiThieus.Any())
            //{
            //    ruleToiThieus = cungHDSDs.Where(x => x.CHI_DINH == false && x.THOI_GIAN_TOI_THIEU != null && x.THOI_GIAN_TOI_THIEU != 0 && string.IsNullOrEmpty(x.MA_BENH));
            //}

            //if (ruleToiThieus.Any() && File.NGAY_YL != null)
            //{
            //    foreach (var rule in ruleToiThieus)
            //    {
            //        if (rule == null) continue;

            //        try
            //        {
            //            //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua
            //            if (rule.MA_LOAI_KCB != null && rule.MA_LOAI_KCB != 0 && File.XML01.MA_LOAI_KCB != null)
            //            {
            //                string RuleMA_LOAI_KCB = "0" + rule.MA_LOAI_KCB.ToString();
            //                if (RuleMA_LOAI_KCB == File.XML01.MA_LOAI_KCB.ToString()) continue;
            //            }

            //            //Kiem tra cac MA_KHOA DICH VU khac trong cung ho so xem co chi dinh 2 lan trong THOI_GIAN TOI THIEU khong:
            //            DateTime dMin = File.NGAY_YL.Value.AddDays(-rule.THOI_GIAN_TOI_THIEU.Value);
            //            if (dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU) && x.MA_LK != File.MA_LK && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL < File.NGAY_YL.Value))
            //            {
            //                RaiseError(true, int.Parse(rule.MA_LOI));

            //            }
            //            //Neu khong chi dinh dich vu trong cung ho so thi vao case kiem tra THOI_GIAN_TOI_THIEU rieng
            //            else
            //            {
            //                Owner.All_XML03_ToiThieus.Add(File);
            //                Owner.All_HDSD_ToiThieus.Add(rule);
            //            }
            //        }
            //        catch { }
            //    }

            //}

            foreach (var hdsd in cungHDSDs)
            {
                // Kiểm tra hướng dẫn sử dụng dịch vụ
                //...
                if (hdsd == null) continue;
                //if (hdsd.MA_LOAI_KCB != null && hdsd.MA_LOAI_KCB != 0 && xml.XML01.MA_LOAI_KCB != null)
                //{
                //    string RuleMA_LOAI_KCB = "0" + hdsd.MA_LOAI_KCB.ToString();
                //    if (RuleMA_LOAI_KCB == xml.XML01.MA_LOAI_KCB.ToString()) continue;
                //}
                try
                {
                    bool hasErr = false;

                    //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua

                    if (hdsd.MA_LOAI_KCB != null && hdsd.MA_LOAI_KCB != 0 && File.XML01.MA_LOAI_KCB != null)
                    {
                        string RuleMA_LOAI_KCB = "0" + hdsd.MA_LOAI_KCB.ToString();
                        if (RuleMA_LOAI_KCB == File.XML01.MA_LOAI_KCB.ToString()) continue;
                    }

                    bool hasDoTuoi = !string.IsNullOrEmpty(hdsd.TUOI);
                    bool hasMaBenh = !string.IsNullOrEmpty(hdsd.MA_BENH);
                    bool hasMaNhom = !string.IsNullOrEmpty(hdsd.MA_NHOM);
                    bool hasDichVu = !string.IsNullOrEmpty(hdsd.DICH_VU_KHAC);
                    //26/12/2022 haimaster update
                    bool hasThuocKhac = !string.IsNullOrEmpty(hdsd.THUOC_KHAC);

                    //Linhist removed 14/06/22
                    //var dichvuKhacs = xml.XML01.xmls.Where(x => x.STT != xml.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));

                    //Case CHI DINH:
                    if (hdsd.CHI_DINH == true)
                    {
                        //Kiem tra DO_TUOI: neu tuoi BN ko phu hop dieu kien --> bao loi
                        if (hasDoTuoi && !Uti.NumMatch(Info.TUOI_BN, hdsd.TUOI))
                        {
                            RaiseError(true,
                                int.Parse(hdsd.MA_LOI),
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với độ tuổi quy định của dịch vụ" +
                                $"(Độ tuổi quy định của dịch vụ: {hdsd.TUOI})\n" +
                                $"(Đô tuôi bệnh nhân: {Uti.RoundUp(Info.TUOI_BN, 0)})"
                                );
                            //hasErr = true;
                            //goto RAISE_ERROR_HDSD_DICHVU;
                        }

                        //Linhist added 04/11/22
                        //Trg hop co MA_NHOM:
                        if (hasMaBenh && hasMaNhom)
                        {
                            //Neu XML01 ko co MA_BENH tuong ung hoac xml khong co MA_DICH_VU tuong ung --> bao loi
                            if (!Uti.CodeMatch($"{Info.MA_BENH_ICD10};{File.MA_BENH}", hdsd.MA_BENH) && !File.XML01.XML03s.Any(x => $";{hdsd.MA_NHOM};".Contains($";{x.MA_NHOM};")))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với mã bệnh và mã nhóm quy định của dịch vụ " +
                                    $"(Mã bệnh quy định của dịch vụ: {hdsd.MA_BENH})\n" +
                                    $"(Mã bệnh của bệnh nhân: {Info.MA_BENH_ICD10})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;

                            }
                        }
                        else if (hasMaNhom)
                        {
                            if (!File.XML01.XML03s.Any(x => $";{hdsd.MA_NHOM};".Contains($";{x.MA_NHOM};")))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với mã nhóm quy định của dịch vụ " +
                                    $"(Mã nhóm quy định của dịch vụ: {hdsd.MA_NHOM})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;
                            }
                        }

                        //Kiem tra MA_BENH & DICH_VU_KHAC:
                        //Trg hop co MA_BENH & co DICH_VU_KHAC:
                        if (hasMaBenh && hasDichVu)
                        {
                            //Neu XML01 ko co MA_BENH tuong ung hoac xml khong co MA_DICH_VU tuong ung --> bao loi
                            if (!Uti.CodeMatch($"{Info.MA_BENH_ICD10};{File.MA_BENH}", hdsd.MA_BENH) && !dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, hdsd.DICH_VU_KHAC)))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với mã bệnh và dịch vụ khác quy định của dịch vụ " +
                                    $"(Mã bệnh quy định của dịch vụ: {hdsd.MA_BENH})\n" +
                                    $"(Mã bệnh của bệnh nhân: {Info.MA_BENH_ICD10})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;
                            }
                        }
                        //Trg hop chi co DICH_VU_KHAC:
                        else if (hasDichVu)
                        {
                            //Neu xml khong co MA_DICH_VU tuong ung --> bao loi
                            if (!dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, hdsd.DICH_VU_KHAC)))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với dịch vụ khác quy định của dịch vụ " +
                                    $"(Dịch vụ khác quy định của dịch vụ: {hdsd.DICH_VU_KHAC})\n"
                                    );
                                //    hasErr = true;
                                //    goto RAISE_ERROR_HDSD_DICHVU;
                            }
                        }


                        //Trg hop chi co MA_BENH:
                        //haimaster 6/6/22 bo doan check thua MA_DICH_VU_KHAC
                        if (hasMaBenh && !hasMaNhom && !hasDichVu)
                        {
                            //Neu XML01 ko co MA_BENH tuong ung --> bao loi
                            if (!Uti.CodeMatch($"{Info.MA_BENH_ICD10};{File.MA_BENH}", hdsd.MA_BENH))
                            {
                                //haimaster update code 15.2.2023
                                if (!(File.XML01.MA_CSKCB == "10061" && (File.MA_KHOA.Trim().ToString().ToUpper() == "K03" || File.MA_KHOA.Trim().ToUpper().ToString() == "K11") && (hdsd.HDSDDVID.ToString() == "131" || hdsd.HDSDDVID.ToString() == "132" || hdsd.HDSDDVID.ToString() == "133")))
                                {
                                    RaiseError(true,
                                        int.Parse(hdsd.MA_LOI),
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với mã bệnh quy định của dịch vụ " +
                                        $"(Mã bệnh quy định của dịch vụ: {hdsd.MA_BENH})\n" +
                                        $"(Mã bệnh của bệnh nhân: {Info.MA_BENH_ICD10})\n"
                                        );
                                    //hasErr = true;
                                    //goto RAISE_ERROR_HDSD_DICHVU;
                                }
                            }
                        }

                        //26/12/2022 Case co THUOC_KHAC
                        if (hasThuocKhac)
                        {
                            //neu co dich vu trong HDSD Dich vu & co thuoc khac
                            //kiem tra neu xml2 khong co thuoc khac => bao loi
                            if (!File.XML01.XML02s.Any(x => Uti.CodeMatch(x.MA_THUOC, hdsd.THUOC_KHAC)))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thuốc khác quy định của dịch vụ "
                                    //$"(Thuốc khác quy định của dịch vụ: {hdsd.TH    UOC_KHAC})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;
                            }
                        }
                    }
                    //Case CHONG CHI DINH:
                    else
                    {
                        //Update kiểm tra TH mã nhóm 

                        //Kiem tra DO_TUOI: neu tuoi BN phu hop dieu kien --> bao loi
                        if (hasDoTuoi && Uti.NumMatch(Info.TUOI_BN, hdsd.TUOI) && !hasMaBenh)
                        {
                            RaiseError(true,
                                int.Parse(hdsd.MA_LOI),
                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                $"Chi tiết:" +
                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với độ tuổi quy định của dịch vụ " +
                                $"(Độ tuổi quy định của dịch vụ: {hdsd.TUOI})\n" +
                                $"(Đô tuôi bệnh nhân: {Uti.RoundUp(Info.TUOI_BN, 0)})"
                                );
                            //hasErr = Uti.NumMatch(Info.TUOI_BN, hdsd.TUOI);
                            //goto RAISE_ERROR_HDSD_DICHVU;
                        }
                        //Update thêm TH có cả tuổi và Bệnh.
                        if (hasDoTuoi && hasMaBenh)
                        {
                            if (Uti.NumMatch(Info.TUOI_BN, hdsd.TUOI))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với độ tuổi quy định của dịch vụ " +
                                    $"(Độ tuổi quy định của dịch vụ: {hdsd.TUOI})\n" +
                                    $"(Đô tuôi bệnh nhân: {Uti.RoundUp(Info.TUOI_BN, 0)})"
                                    );
                            }
                            string mabenhloi = "";
                            foreach (var mabenh in Info.MA_BENH_ICD10.Split(";"))
                            {
                                if (Uti.CodeMatch(mabenh, hdsd.MA_BENH))
                                {
                                    mabenhloi = mabenh;
                                    break;
                                }
                            }
                            if (mabenhloi != "")
                            {
                                if (Uti.CodeMatch(Info.MA_BENH_ICD10, hdsd.MA_BENH))
                                {
                                    RaiseError(true,
                                        int.Parse(hdsd.MA_LOI),
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, Có mã bệnh Chống chỉ định theo quy định của dịch vụ " +
                                        $"(Mã bệnh quy định của dịch vụ: {hdsd.MA_BENH})\n" +
                                        $"(Mã bệnh của bệnh nhân: {mabenhloi})\n"
                                        );
                                }
                            }

                            //hasErr = Uti.NumMatch(Info.TUOI_BN, hdsd.TUOI);
                            //goto RAISE_ERROR_HDSD_DICHVU;
                        }
                        // 1/2/2025 update cho so nguoi toi thieu thuc hien dich vu
                        if (hdsd.SO_NGUOI_TOI_THIEU != null)
                        {
                            if (File.MA_DICH_VU.Trim() == hdsd.MA_DICH_VU.Trim())
                            {
                                var songuoi = File.NGUOI_THUC_HIEN.Split(";");
                                if (songuoi.Count() != hdsd.SO_NGUOI_TOI_THIEU)
                                {
                                    RaiseError(true,
                                        int.Parse(hdsd.MA_LOI),
                                        $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                        $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                        $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                        $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                        $"Chi tiết:" +
                                        $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với số người tối thiểu thực hiện quy định của dịch vụ " +
                                        $"(Số người tối thiểu thực hiện quy định của dịch vụ: {hdsd.SO_NGUOI_TOI_THIEU})\n" +
                                        $"(Số người thực hiện của bệnh nhân: {songuoi.Count()})"
                                        );
                                    //hasErr = true;
                                    //goto RAISE_ERROR_HDSD_DICHVU;
                                }
                            }
                        }

                        //Linhist changed 240926
                        //11.12.2025 update them case moi co ma nhom, thoi gian thuc hien va ma dich vu loai tru
                        //Kiểm tra thời gian thực hiện với mã nhóm và mã dịch vụ không kiểm tra (mã dịch vụ phải là null không check mã dịch vụ)
                        //if (!string.IsNullOrEmpty(hdsd.THOI_GIAN_THUC_HIEN) && !string.IsNullOrEmpty(hdsd.MA_NHOM) && !string.IsNullOrEmpty(hdsd.MA_DICH_VU_KO_CHECK) && string.IsNullOrEmpty(hdsd.MA_DICH_VU))
                        //{
                        //    if (File.NGAY_KQ != null && File.NGAY_TH_YL != null && byte.TryParse(hdsd.MA_NHOM, out var maNhom) && File.MA_NHOM == maNhom && !Uti.HCodeContain(hdsd.MA_DICH_VU_KO_CHECK, File.MA_DICH_VU))
                        //    {
                        //        double thucHien = 0d;
                        //        string hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN;
                        //        //Update chuyển từ thời gian y lệnh thành thời gian thực hiện ly lệnh
                        //        TimeSpan tmeDim = (File.NGAY_KQ.Value - File.NGAY_TH_YL.Value);

                        //        if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p"))
                        //        {
                        //            hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //            thucHien = tmeDim.TotalMinutes;
                        //        }
                        //        else if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h"))
                        //        {
                        //            hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //            thucHien = tmeDim.TotalHours;
                        //        }
                        //        else
                        //        {
                        //            thucHien = tmeDim.TotalDays;
                        //        }

                        //        hasErr = Uti.NumMatch(thucHien, hdsdThucHien);
                        //        if (hasErr)
                        //        {
                        //            RaiseError(true,
                        //                int.Parse(hdsd.MA_LOI),
                        //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                $"Chi tiết:" +
                        //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                        //                $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                        //                $"(Thời gian thực hiện của bệnh nhân: {thucHien}{(hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p") ? " phút" : hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h") ? " giờ" : " ngày")})"
                        //                );
                        //        }
                        //    }
                        //}
                        ////20.7/2024 Update thêm trường hợp không check mã Dịch vụ, chỉ check Mã nhóm cho thời gian thực hiện
                        ////Kiểm tra với thời gian thực hiện, mã nhóm (Không check mã dịch vụ và mã dịch vụ ko check)
                        //if (!string.IsNullOrEmpty(hdsd.THOI_GIAN_THUC_HIEN) && !string.IsNullOrEmpty(hdsd.MA_NHOM) && string.IsNullOrEmpty(hdsd.MA_DICH_VU_KO_CHECK) && string.IsNullOrEmpty(hdsd.MA_DICH_VU))
                        //{
                        //    if (File.NGAY_KQ != null && File.NGAY_TH_YL != null && byte.TryParse(hdsd.MA_NHOM, out var maNhom) && File.MA_NHOM == maNhom)
                        //    {
                        //        double thucHien = 0d;
                        //        string hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN;
                        //        //Update chuyển từ thời gian y lệnh thành thời gian thực hiện ly lệnh
                        //        TimeSpan tmeDim = (File.NGAY_KQ.Value - File.NGAY_TH_YL.Value);

                        //        if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p"))
                        //        {
                        //            hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //            thucHien = tmeDim.TotalMinutes;
                        //        }
                        //        else if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h"))
                        //        {
                        //            hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //            thucHien = tmeDim.TotalHours;
                        //        }
                        //        else
                        //        {
                        //            thucHien = tmeDim.TotalDays;
                        //        }

                        //        hasErr = Uti.NumMatch(thucHien, hdsdThucHien);
                        //        if (hasErr)
                        //        {
                        //            RaiseError(true,
                        //                int.Parse(hdsd.MA_LOI),
                        //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                $"Chi tiết:" +
                        //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                        //                $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                        //                $"(Thời gian thực hiện của bệnh nhân: {thucHien}{(hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p") ? " phút" : hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h") ? " giờ" : " ngày")})"
                        //                );
                        //        }
                        //    }

                        //}
                        ////Kiểm tra THOI_GIAN_THUC_HIEN neu co MA_DICH_VU
                        ////Kiểm tra thời gian tối thiểu với mã dịch vụ (Không kiểm tra mã nhóm và mã dịch vụ ko kiểm tra)
                        //if (!string.IsNullOrEmpty(hdsd.THOI_GIAN_THUC_HIEN) && string.IsNullOrEmpty(hdsd.MA_NHOM) && !string.IsNullOrEmpty(hdsd.MA_DICH_VU) && string.IsNullOrEmpty(hdsd.MA_DICH_VU_KO_CHECK))
                        //{
                        //    if (File.NGAY_KQ != null && File.NGAY_TH_YL != null)
                        //    {
                        //        if (hdsd.MA_DICH_VU.Contains(File.MA_DICH_VU))
                        //        {
                        //            //Uti.HandleCronJobLog(_logger, $"Validate130:HDSD Dich vu: MA_CSKCB:{File.MA_CSKCB},Ma LK:{File.MA_LK},Thoi gian thuc hien{hdsd.THOI_GIAN_THUC_HIEN}, Ma DV:{File.MA_DICH_VU},   [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
                        //            double thucHien = 0d;
                        //            string hdsdThucHien = "";
                        //            //var hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN;
                        //            //Update chuyển từ thời gian y lệnh thành thời gian thực hiện ly lệnh
                        //            TimeSpan tmeDim = (File.NGAY_KQ.Value - File.NGAY_TH_YL.Value);

                        //            if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p"))
                        //            {
                        //                hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //                thucHien = tmeDim.TotalMinutes;
                        //            }
                        //            else if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h"))
                        //            {
                        //                hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //                thucHien = tmeDim.TotalHours;
                        //            }
                        //            else
                        //            {
                        //                thucHien = tmeDim.TotalDays;
                        //            }
                        //            hasErr = Uti.NumMatch(thucHien, hdsdThucHien);
                        //            if (hasErr)
                        //            {
                        //                RaiseError(true,
                        //                    int.Parse(hdsd.MA_LOI),
                        //                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                    $"Chi tiết:" +
                        //                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                        //                    $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                        //                    $"(Thời gian thực hiện của bệnh nhân: {thucHien}{(hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p") ? " phút" : hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h") ? " giờ" : " ngày")})"
                        //                    );
                        //            }
                        //            //Uti.HandleCronJobLog(_logger, $"Validate130:HDSD Dich vu: MA_CSKCB:{File.MA_CSKCB},Ma LK:{File.MA_LK},Thoi gian thuc hien{hdsd.THOI_GIAN_THUC_HIEN},Thu hien{thucHien}, Ma DV:{File.MA_DICH_VU} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(Validate));
                        //        }

                        //    }
                        //    //goto RAISE_ERROR_HDSD_DICHVU;
                        //}
                        if (!string.IsNullOrEmpty(hdsd.THOI_GIAN_THUC_HIEN) && File.NGAY_KQ != null && File.NGAY_TH_YL != null)
                        {
                            var ruleTGTH = hdsd.THOI_GIAN_THUC_HIEN.ToLower();
                            var isHour = ruleTGTH.EndsWith("h");
                            var isMinut = ruleTGTH.EndsWith("p");
                            ruleTGTH = isHour || isMinut ? ruleTGTH[0..^1] : ruleTGTH;

                            var tmeDim = (File.NGAY_KQ.Value - File.NGAY_TH_YL.Value);
                            var tgThucHien = isMinut ? tmeDim.TotalMinutes : isHour ? tmeDim.TotalHours : tmeDim.TotalDays;

                            if (!string.IsNullOrEmpty(hdsd.MA_NHOM))
                            {
                                if (string.IsNullOrEmpty(hdsd.MA_DICH_VU))
                                {
                                    if (string.IsNullOrEmpty(hdsd.MA_DICH_VU_KO_CHECK))
                                    {
                                        if (File.NGAY_KQ != null && File.NGAY_TH_YL != null && byte.TryParse(hdsd.MA_NHOM, out var maNhom) && File.MA_NHOM == maNhom)
                                        {
                                            hasErr = Uti.NumMatch(tgThucHien, ruleTGTH);
                                            if (hasErr)
                                            {
                                                RaiseError(true,
                                                    int.Parse(hdsd.MA_LOI),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                                                    $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                                                    $"(Thời gian thực hiện của bệnh nhân: {tgThucHien}{(isMinut ? " phút" : isHour ? " giờ" : " ngày")})"
                                                    );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (byte.TryParse(hdsd.MA_NHOM, out var maNhom) && File.MA_NHOM == maNhom && !Uti.HCodeContain(hdsd.MA_DICH_VU_KO_CHECK, File.MA_DICH_VU))
                                        {
                                            hasErr = Uti.NumMatch(tgThucHien, ruleTGTH);
                                            if (hasErr)
                                            {
                                                RaiseError(true,
                                                    int.Parse(hdsd.MA_LOI),
                                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                    $"Chi tiết:" +
                                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                                                    $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                                                    $"(Thời gian thực hiện của bệnh nhân: {tgThucHien}{(isMinut ? " phút" : isHour ? " giờ" : " ngày")})"
                                                    );
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(hdsd.MA_DICH_VU) && string.IsNullOrEmpty(hdsd.MA_DICH_VU_KO_CHECK))
                                {
                                    if (hdsd.MA_DICH_VU.Contains(File.MA_DICH_VU))
                                    {
                                        hasErr = Uti.NumMatch(tgThucHien, ruleTGTH);
                                        if (hasErr)
                                        {
                                            RaiseError(true,
                                                int.Parse(hdsd.MA_LOI),
                                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                                                $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                                                $"(Thời gian thực hiện của bệnh nhân: {tgThucHien}{(isMinut ? " phút" : isHour ? " giờ" : " ngày")})"
                                                );
                                        }
                                    }
                                }
                            }
                        }

                        //Kiem tra THOI_GIAN_THUC_HIEN: neu thoi gian thuc hien phu hop dieu kien --> bao loi
                        //if (!string.IsNullOrEmpty(hdsd.THOI_GIAN_THUC_HIEN) && string.IsNullOrEmpty(hdsd.MA_NHOM) && string.IsNullOrEmpty(hdsd.MA_DICH_VU_KO_CHECK))
                        //{
                        //    if (File.NGAY_KQ != null && File.NGAY_TH_YL != null)
                        //    {

                        //        double thucHien = 0d;
                        //        string hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN;
                        //        //Update chuyển từ thời gian y lệnh thành thời gian thực hiện ly lệnh
                        //        TimeSpan tmeDim = (File.NGAY_KQ.Value - File.NGAY_TH_YL.Value);

                        //        if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p"))
                        //        {
                        //            hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //            thucHien = tmeDim.TotalMinutes;
                        //        }
                        //        else if (hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h"))
                        //        {
                        //            hdsdThucHien = hdsd.THOI_GIAN_THUC_HIEN[0..^1];
                        //            thucHien = tmeDim.TotalHours;
                        //        }
                        //        else
                        //        {
                        //            thucHien = tmeDim.TotalDays;
                        //        }
                        //        hasErr = Uti.NumMatch(thucHien, hdsdThucHien);
                        //        if (hasErr)
                        //        {
                        //            RaiseError(true,
                        //                int.Parse(hdsd.MA_LOI),
                        //                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                        //                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                        //                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                        //                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                        //                $"Chi tiết:" +
                        //                $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với thời gian thực hiện quy định của dịch vụ " +
                        //                $"(Thời gian thực hiện quy định của dịch vụ: {hdsd.THOI_GIAN_THUC_HIEN})\n" +
                        //                $"(Thời gian thực hiện của bệnh nhân: {thucHien}{(hdsd.THOI_GIAN_THUC_HIEN.EndsWith("p") ? " phút" : hdsd.THOI_GIAN_THUC_HIEN.EndsWith("h") ? " giờ" : " ngày")})"
                        //                );
                        //        }


                            //}
                            //goto RAISE_ERROR_HDSD_DICHVU;
                        //}

                        //30/6/23 update dich vu khac
                        if (!string.IsNullOrEmpty(hdsd.DICH_VU_KHAC) && string.IsNullOrEmpty(hdsd.MA_BENH))
                        {
                            if (dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, hdsd.DICH_VU_KHAC)))
                            {

                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Chi tiết:" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với dịch vụ khác quy định của dịch vụ " 
                                    //$"(Dịch vụ khác quy định của dịch vụ: {hdsd.DICH_VU_KHAC})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;
                            }
                        }

                        //Trg hop chi co MA_BENH:
                        if (hasMaBenh && hdsd.THOI_GIAN_TOI_THIEU is null)
                        {
                            //Neu XML01 co MA_BENH tuong ung --> bao loi
                            if (Uti.CodeMatch($"{Info.MA_BENH_ICD10};{File.MA_BENH}", hdsd.MA_BENH))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Chi tiết:" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, có mã nhóm:{File.MA_NHOM}, không phù hợp với mã bệnh quy định của dịch vụ " +
                                    //$"(Mã bệnh quy định của dịch vụ: {hdsd.MA_BENH})\n" +
                                    $"(Mã bệnh của bệnh nhân: {Info.MA_BENH_ICD10})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;
                            }
                        }

                        //Stopwatch _ws = Stopwatch.StartNew();

                        //25.9.2025 haimaster update thêm trường hợp thời gian tối thiêu => Nếu không đủ thời gian tối thiểu báo lỗi
                        //Thoi gian toi thieu co 3 case
                        //Case 1: Thoi gian toi thieu cho Dich VỤ(Sau dung khoang thoi gian day mio duoc y lenh tiep
                        //Case 2: Thoi gian toi thieu cho DV nhưng kèm theo điều kiện Bệnh
                        //Case 3: Thời gian tối thiểu cho DV này chỉ định kèm dịch vụ khác(Nếu để 0 = 0 ngày, để 1 = 1 ngày => làm trong TH Ko chi dinh dong thoi hoac ko chi dinh kem dịch vu khác
                        //Coding case 1:
                        try
                        {
                            #region Linhist removed 15/07/26
                            //if (hdsd.THOI_GIAN_TOI_THIEU != null && hdsd.MA_BENH == null && hdsd.DICH_VU_KHAC == null && File.NGAY_YL != null)
                            //{
                            //    try
                            //    {
                            //        var day = hdsd.THOI_GIAN_TOI_THIEU;


                            //        //var xml1Lichsu = await _hosoRepository.GetAllByMaLK(x => x.XML01ID != File.XML01.XML01ID && x.IsDelete != true && x.MA_BN == File.XML01.MA_BN && x.MA_CSKCB == File.XML01.MA_CSKCB && x.MA_LK != File.XML01.MA_LK && x.NGAY_VAO != null && x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(double)day));
                            //        var xml1lichsu = (await _hosoRepository.GetListAsync(x => x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU) && x.MA_CSKCB == File.MA_CSKCB &&
                            //                                (
                            //                                    !string.IsNullOrEmpty(x.MA_THE_BHYT) &&
                            //                                    !string.IsNullOrEmpty(File.XML01.MA_THE_BHYT) &&
                            //                                    x.MA_THE_BHYT.Length >= 15 &&
                            //                                    x.MA_THE_BHYT.Substring(4, 11) == File.XML01.MA_THE_BHYT.Substring(4, 11)
                            //                                )
                            //                                && x.MA_LK != File.XML01.MA_LK)).Select(y => y.XML01ID).ToList();

                            //        if (xml1lichsu.Any())
                            //        {
                            //            //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                            //            //var xml3Lichsu = await _xml03Repository.GetListAsync(x => x.MA_LK != File.MA_LK && hdsd.MA_DICH_VU.Contains(x.MA_DICH_VU));
                            //            //var xml3Lichsu =  _xml03Repository.DbSet.AsNoTracking().Where(x => xml1IDs.Contains(x.XML01ID) && x.MA_LK != File.MA_LK && hdsd.MA_DICH_VU.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(ma => ma.Trim() == x.MA_DICH_VU)).Take(1).ToList();
                            //            // Tách danh sách mã dịch vụ ra trước để tránh lỗi LINQ
                            //            var maDichVuList = hdsd.MA_DICH_VU
                            //                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            //                .Select(ma => ma.Trim())
                            //                .ToList();

                            //            // Truy vấn XML03 lịch sử
                            //            var xml3Lichsu = await _xml03Repository.GetItem(x => x.MA_CSKCB == File.MA_CSKCB &&
                            //                    xml1lichsu.Contains(x.XML01ID) &&
                            //                    x.MA_LK != File.MA_LK &&
                            //                    maDichVuList.Contains(x.MA_DICH_VU));


                            //            if (xml3Lichsu != null)
                            //            {

                            //                //    int maloi;
                            //                //    maloi = int.TryParse(hdsd.MA_LOI, out int temp) ? temp : 0;
                            //                //    var error = dmError130.FirstOrDefault(x => x.ErrorCode == maloi);
                            //                //    string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //                //    var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //                //    var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //                //    var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}";
                            //                //    desc = desc + $"\n Bệnh nhân đã thực hiện dịch vụ này:{xml3.MA_DICH_VU}, y lệnh {xml3.NGAY_YL}, ngày thực hiện:{xml3.NGAY_TH_YL}";
                            //                //    desc = desc + $"\n, khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL-xml3.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}";
                            //                //    RaiseError(true, maloi, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "205", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            //                RaiseError(true,
                            //                    int.Parse(hdsd.MA_LOI),
                            //                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            //                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            //                    $"Chi tiết:" +
                            //                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}" +
                            //                    $"\n Bệnh nhân đã thực hiện dịch vụ này:{xml3Lichsu.MA_DICH_VU}, y lệnh {xml3Lichsu.NGAY_YL}, ngày thực hiện:{xml3Lichsu.NGAY_TH_YL}" +
                            //                    $"\n Khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - xml3Lichsu.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}" +
                            //                    $"\n Mã LK: {xml3Lichsu.MA_LK}"
                            //                    );



                            //            }
                            //        }
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        _logger.LogError(ex, "Validate130: MaBN: {MaBN} MaCSKCB: {MaCSKCB}; STT Bị Lỗi: {STT}; DBset{Dbset} XML3 Giám định có lỗi [{Time}]", File.XML01.MA_BN, File.XML01.MA_CSKCB, File.STT, _hosoRepository.DbSet.GetType().FullName, DateTime.Now.ToString("dd/MM HH:mm:ss"));
                            //    }
                            //}
                            ////coding case 2: Co benh kem theo
                            //if (hdsd.THOI_GIAN_TOI_THIEU != null && hdsd.MA_BENH != null && hdsd.DICH_VU_KHAC == null && File.NGAY_YL != null)
                            //{
                            //    try
                            //    {
                            //        //var day = hdsd.THOI_GIAN_TOI_THIEU;
                            //        //var xml1lichsu = await _hosoRepository.DbSet.AsNoTracking().Where(x => x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU) && x.MA_CSKCB == File.MA_CSKCB && x.MA_BN == File.XML01.MA_BN && x.MA_LK != File.XML01.MA_LK).Take(1).ToListAsync();
                            //        //List<long> xml1IDs = xml1lichsu.Select(x => x.XML01ID).ToList();
                            //        //update lại code cho phù hợp với EF6
                            //        var day = hdsd.THOI_GIAN_TOI_THIEU;
                            //        //var xml1Lichsu = await _hosoRepository.GetAllByMaLK(x => x.XML01ID != File.XML01.XML01ID && x.IsDelete != true && x.MA_BN == File.XML01.MA_BN && x.MA_CSKCB == File.XML01.MA_CSKCB && x.MA_LK != File.XML01.MA_LK && x.NGAY_VAO != null && x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(double)day));
                            //        var xml1lichsu = (await _hosoRepository.GetListAsync(x => x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU) && x.MA_CSKCB == File.MA_CSKCB && (
                            //                                    !string.IsNullOrEmpty(x.MA_THE_BHYT) &&
                            //                                    !string.IsNullOrEmpty(File.XML01.MA_THE_BHYT) &&
                            //                                    x.MA_THE_BHYT.Length >= 15 &&
                            //                                    x.MA_THE_BHYT.Substring(4, 11) == File.XML01.MA_THE_BHYT.Substring(4, 11)
                            //                                ) && x.MA_LK != File.XML01.MA_LK)).Select(x=>x.XML01ID).ToList();
                                    
                            //        if (xml1lichsu != null)
                            //        {
                            //            //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                            //            //var xml3Lichsu = await _xml03Repository.DbSet.AsNoTracking().Where(x => xml1IDs.Contains(x.XML01ID) && x.MA_LK != File.MA_LK && hdsd.MA_DICH_VU.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(ma => ma.Trim() == x.MA_DICH_VU)).Take(1).ToListAsync();
                            //            // Tách danh sách mã dịch vụ ra trước để tránh lỗi LINQ
                            //            var maDichVuList = hdsd.MA_DICH_VU
                            //                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            //                .Select(ma => ma.Trim())
                            //                .ToList();

                            //            // Truy vấn XML03 lịch sử
                            //            var xml3Lichsu = await _xml03Repository.GetItem(x => x.MA_CSKCB == File.MA_CSKCB &&
                            //                    xml1lichsu.Contains(x.XML01ID) &&
                            //                    x.MA_LK != File.MA_LK &&
                            //                    maDichVuList.Contains(x.MA_DICH_VU));

                            //            if (xml3Lichsu != null)
                            //            {
                                            
                            //                    var checkbenh = xml3Lichsu.XML01.MA_BENH_CHINH + ";" + xml3Lichsu.XML01.MA_BENH_KT;
                            //                    string benhloi = "";
                            //                    foreach (var bloi in checkbenh)
                            //                    {
                            //                        if (hdsd.MA_BENH.Contains(bloi))
                            //                        {
                            //                            benhloi += bloi + ";";
                            //                        }
                            //                    }

                            //                //maloi = int.TryParse(hdsd.MA_LOI, out int temp) ? temp : 0;
                            //                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == maloi);
                            //                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}";
                            //                //desc = desc + $"\n Bệnh nhân đã thực hiện dịch vụ này:{xml3.MA_DICH_VU}, y lệnh {xml3.NGAY_YL}, ngày thực hiện:{xml3.NGAY_TH_YL}";
                            //                //desc = desc + $"\n Bệnh nhân đã thực hiện dịch vụ này với bệnh:{benhloi}";
                            //                //desc = desc + $"\n, khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - xml3.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}";
                            //                //RaiseError(true, maloi, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "205", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            //                RaiseError(true,
                            //                    int.Parse(hdsd.MA_LOI),
                            //                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            //                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            //                    $"Chi tiết:" +
                            //                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}" +
                            //                    $"\n Bệnh nhân đã thực hiện dịch vụ này:{xml3Lichsu.MA_DICH_VU}, y lệnh {xml3Lichsu.NGAY_YL}, ngày thực hiện:{xml3Lichsu.NGAY_TH_YL}" +
                            //                    $"\n Bệnh nhân đã thực hiện dịch vụ này với bệnh:{benhloi}" +
                            //                    $"\n  Khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - xml3Lichsu.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}" +
                            //                    $"\n Mã LK: {xml3Lichsu.MA_LK}"
                            //                    );
                            //                }
                            //        }
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        _logger.LogError(ex, "Validate130: MaBN: {MaBN} MaCSKCB: {MaCSKCB}; STT Bị Lỗi: {STT} XML3 Giám định có lỗi [{Time}]",
                            //   File.XML01.MA_BN,
                            //   File.XML01.MA_CSKCB,
                            //   File.STT,
                            //   DateTime.Now.ToString("dd/MM HH:mm:ss"));

                            //    }
                            //}
                            ////coding case 3: dich vu khac
                            //if (hdsd.THOI_GIAN_TOI_THIEU != null && hdsd.MA_BENH == null && hdsd.DICH_VU_KHAC != null && File.NGAY_YL != null)
                            //{
                            //    try
                            //    {
                            //        //case nay co the tach lam 2 TH
                            //        //var day = hdsd.THOI_GIAN_TOI_THIEU;
                            //        //var xml1lichsu = await _hosoRepository.DbSet.AsNoTracking().Where(x => x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU) && x.MA_CSKCB == File.MA_CSKCB && x.MA_BN == File.XML01.MA_BN && x.MA_LK != File.XML01.MA_LK).Take(1).ToListAsync();
                            //        //List<long> xml1IDs = xml1lichsu.Select(x => x.XML01ID).ToList();
                            //        //Update lại code cho phù hợp với EF 6
                            //        //var day = hdsd.THOI_GIAN_TOI_THIEU;
                            //        //var xml1lichsu = await _hosoRepository.DbSet.AsNoTracking().Where(x => x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU) && x.MA_CSKCB == File.MA_CSKCB && x.MA_BN == File.XML01.MA_BN && x.MA_LK != File.XML01.MA_LK).Take(1).ToListAsync();
                            //        //List<long> xml1IDs = xml1lichsu.Select(x => x.XML01ID).ToList();
                            //        //update lại code cho phù hợp với EF6
                            //        var day = hdsd.THOI_GIAN_TOI_THIEU;
                            //        //var xml1Lichsu = await _hosoRepository.GetAllByMaLK(x => x.XML01ID != File.XML01.XML01ID && x.IsDelete != true && x.MA_BN == File.XML01.MA_BN && x.MA_CSKCB == File.XML01.MA_CSKCB && x.MA_LK != File.XML01.MA_LK && x.NGAY_VAO != null && x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(double)day));
                            //        var xml1lichsu = (await _hosoRepository.GetListAsync(x => x.NGAY_VAO >= File.XML01.NGAY_VAO.Value.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU) && x.MA_CSKCB == File.MA_CSKCB && (
                            //                                    !string.IsNullOrEmpty(x.MA_THE_BHYT) &&
                            //                                    !string.IsNullOrEmpty(File.XML01.MA_THE_BHYT) &&
                            //                                    x.MA_THE_BHYT.Length >= 15 &&
                                                                
                            //                                    x.MA_THE_BHYT.Substring(4, 11) == File.XML01.MA_THE_BHYT.Substring(4, 11)
                            //                                ) && x.MA_LK != File.XML01.MA_LK)).Select(x=>x.XML01ID).ToList();

                            //        if (xml1lichsu != null)
                            //        {
                            //            //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                            //            //var xml3Lichsu = await _xml03Repository.DbSet.AsNoTracking().Where(x => xml1IDs.Contains(x.XML01ID) && x.MA_LK != File.MA_LK && hdsd.DICH_VU_KHAC.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(ma => ma.Trim() == x.MA_DICH_VU)).Take(1).ToListAsync();
                            //            // Tách danh sách mã dịch vụ ra trước để tránh lỗi LINQ
                            //            var maDichVuList = hdsd.MA_DICH_VU
                            //                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            //                .Select(ma => ma.Trim())
                            //                .ToList();

                            //            // Truy vấn XML03 lịch sử
                            //            var xml3Lichsu = await _xml03Repository.GetItem(x => x.MA_CSKCB == File.MA_CSKCB &&
                            //                    xml1lichsu.Contains(x.XML01ID)&&
                            //                    x.MA_LK != File.MA_LK &&
                            //                    maDichVuList.Contains(x.MA_DICH_VU));

                            //            if (xml3Lichsu != null)
                            //            {
                            //                //int maloi;
                            //                //maloi = int.TryParse(hdsd.MA_LOI, out int temp) ? temp : 0;
                            //                //var error = dmError130.FirstOrDefault(x => x.ErrorCode == maloi);
                            //                //string errorInfo = error != null ? error.Message : "Không tìm thấy nội dung lỗi";
                            //                //var benh = dmChung130.FirstOrDefault(x => x.LOAI == "ICD10" && x.MA == File.XML01.MA_BENH_CHINH);
                            //                //var benhchinhInfo = benh != null ? benh.TEN : "Không tìm thấy tên bệnh";
                            //                //var desc = $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}";
                            //                //desc = desc + $"\n Bệnh nhân đã thực hiện dịch vụ này:{xml3.MA_DICH_VU}, y lệnh {xml3.NGAY_YL}, ngày thực hiện:{xml3.NGAY_TH_YL}";
                            //                //desc = desc + $"\n, khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - xml3.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}";
                            //                //desc = desc + $"\n Dịch vụ khác quy định trong HDSD là:{hdsd.DICH_VU_KHAC}";
                            //                //RaiseError(true, maloi, Uti.getErrorDescription(File.XML01.MA_BN, File.XML01.HO_TEN, "205", errorInfo, File.XML01.MA_BENH_CHINH, benhchinhInfo, desc));
                            //                RaiseError(true,
                            //                    int.Parse(hdsd.MA_LOI),
                            //                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                            //                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                            //                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                            //                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                            //                    $"Chi tiết:" +
                            //                    $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}" +
                            //                    $"\n Bệnh nhân đã thực hiện dịch vụ này:{xml3Lichsu.MA_DICH_VU}, y lệnh {xml3Lichsu.NGAY_YL}, ngày thực hiện:{xml3Lichsu.NGAY_TH_YL}" +
                            //                    $"\n, khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - xml3Lichsu.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}" +
                            //                    $"\n Dịch vụ khác quy định trong HDSD là:{hdsd.DICH_VU_KHAC}" +
                            //                    $"\n Mã LK: {xml3Lichsu.MA_LK}"
                            //                    );
                            //            }
                            //        }    
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        _logger.LogError(ex, "Validate130: MaBN: {MaBN} MaCSKCB: {MaCSKCB}; STT Bị Lỗi: {STT} XML3 Giám định có lỗi [{Time}]",
                            //          File.XML01.MA_BN,
                            //          File.XML01.MA_CSKCB,
                            //          File.STT,
                            //          DateTime.Now.ToString("dd/MM HH:mm:ss"));
                            //    }
                            //}

                            //_ws.Stop();
                            //if (_ws.Elapsed.TotalSeconds > .3d)
                            //{
                            //    Uti.HandleCronJobLog(_logger, $"Giám định xong XML3 {File.XML03ID} - CSKCB {File.MA_CSKCB} - MA LK {File.MA_LK} - HDSD Dich vu (check lich su) - MA_LOI {hdsd.MA_LOI}, thời gian: {_ws.Elapsed.TotalSeconds}", nameof(Validate));
                            //}
                            #endregion

                            #region Linhist added 15/07/26
                            //Stopwatch wa = Stopwatch.StartNew();

                            //Linhist added 060826
                            if (_dmErrorMHTCs.Any(x => x.EMErrorCode?.ToString() == hdsd.MA_LOI))
                            {
                                if (hdsd.THOI_GIAN_TOI_THIEU != null && File.NGAY_TH_YL != null)
                                {
                                    // Tách danh sách mã dịch vụ ra trước để tránh lỗi LINQ
                                    var maDichVus = hdsd.MA_DICH_VU.Split(';', StringSplitOptions.RemoveEmptyEntries);
                                    var tuNgay = File.NGAY_TH_YL.Value.Date.AddDays(-(int)hdsd.THOI_GIAN_TOI_THIEU);
                                    var denNgay = File.NGAY_TH_YL.Value.Date.AddDays((int)hdsd.THOI_GIAN_TOI_THIEU + 1);
                                    var subMaThe = string.IsNullOrEmpty(File.XML01.MA_THE_BHYT) || File.XML01.MA_THE_BHYT.Length < 15 ? string.Empty : File.XML01.MA_THE_BHYT.Substring(4, 11);

                                    if (string.IsNullOrEmpty(hdsd.MA_BENH) && string.IsNullOrEmpty(hdsd.DICH_VU_KHAC))
                                    {
                                        //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                                        var x3ToiThieu = await _xml03Repository.GetItem(x => x.MA_LK != File.MA_LK && x.MA_CSKCB == File.MA_CSKCB && x.XML01.IsDelete != true && x.XML01.MA_CSKCB == File.MA_CSKCB && x.XML01.MA_LK != File.MA_LK &&
                                                                                !string.IsNullOrEmpty(x.XML01.MA_THE_BHYT) && x.XML01.MA_THE_BHYT.Length >= 15 && x.XML01.MA_THE_BHYT.Substring(4, 11) == subMaThe &&
                                                                                maDichVus.Contains(x.MA_DICH_VU) && x.NGAY_TH_YL > tuNgay && x.NGAY_TH_YL < denNgay && (hdsd.MA_LOAI_KCB == null || x.XML01.MA_LOAI_KCB.Contains(hdsd.MA_LOAI_KCB.ToString())));

                                        RaiseError(x3ToiThieu != null,
                                                int.Parse(hdsd.MA_LOI), x3ToiThieu == null ? "" :
                                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                $"Chi tiết:" +
                                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}" +
                                                $"\n Bệnh nhân đã thực hiện dịch vụ này:{x3ToiThieu.MA_DICH_VU}, y lệnh {x3ToiThieu.NGAY_YL}, ngày thực hiện:{x3ToiThieu.NGAY_TH_YL}" +
                                                $"\n Khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - x3ToiThieu.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}" +
                                                $"\n Mã LK: {x3ToiThieu.MA_LK}"
                                            );
                                    }
                                    else if (!string.IsNullOrEmpty(hdsd.MA_BENH))
                                    {
                                        var maBenhs = hdsd.MA_BENH.Split(';', StringSplitOptions.RemoveEmptyEntries);

                                        //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                                        var x3ToiThieus = await _xml03Repository.GetListAsync(x => x.MA_LK != File.MA_LK && x.MA_CSKCB == File.MA_CSKCB && x.XML01.IsDelete != true && x.XML01.MA_CSKCB == File.MA_CSKCB && x.XML01.MA_LK != File.MA_LK &&
                                                                                !string.IsNullOrEmpty(x.XML01.MA_THE_BHYT) && x.XML01.MA_THE_BHYT.Length >= 15 && x.XML01.MA_THE_BHYT.Substring(4, 11) == subMaThe &&
                                                                                maDichVus.Contains(x.MA_DICH_VU) && x.NGAY_TH_YL > tuNgay && x.NGAY_TH_YL < denNgay && (hdsd.MA_LOAI_KCB == null || x.XML01.MA_LOAI_KCB.Contains(hdsd.MA_LOAI_KCB.ToString())));

                                        if (x3ToiThieus != null && x3ToiThieus.Any())
                                        {
                                            var x1Ids = x3ToiThieus.Select(x => x.XML01ID).Distinct().ToArray();
                                            var x1ToiThieus = await _hosoRepository.GetAllByListID(File.MA_CSKCB, x1Ids);

                                            foreach (var x1 in x1ToiThieus)
                                            {
                                                foreach (var mb in maBenhs)
                                                {
                                                    if (x1.MA_BENH_CHINH == mb || x1.MA_BENH_KT.Split(';').Contains(mb))
                                                    {
                                                        var x3ToiThieu = x3ToiThieus.FirstOrDefault(x => x.XML01ID == x1.XML01ID);
                                                        RaiseError(x3ToiThieu != null,
                                                                int.Parse(hdsd.MA_LOI), x3ToiThieu == null ? "" :
                                                                $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                                                $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                                                $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                                                $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                                                $"Chi tiết:" +
                                                                $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}" +
                                                                $"\n Bệnh nhân đã thực hiện dịch vụ này:{x3ToiThieu.MA_DICH_VU}, y lệnh {x3ToiThieu.NGAY_YL}, ngày thực hiện:{x3ToiThieu.NGAY_TH_YL}" +
                                                                $"\n Bệnh nhân đã thực hiện dịch vụ này với bệnh: {mb}" +
                                                                $"\n  Khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - x3ToiThieu.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}" +
                                                                $"\n Mã LK: {x3ToiThieu.MA_LK}"
                                                            );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(hdsd.DICH_VU_KHAC))
                                    {
                                        var maDvKhacs = hdsd.DICH_VU_KHAC.Split(';', StringSplitOptions.RemoveEmptyEntries);
                                        //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                                        var x3ToiThieu = await _xml03Repository.GetItem(x => x.MA_LK != File.MA_LK && x.MA_CSKCB == File.MA_CSKCB && x.XML01.IsDelete != true && x.XML01.MA_CSKCB == File.MA_CSKCB && x.XML01.MA_LK != File.MA_LK &&
                                                                                !string.IsNullOrEmpty(x.XML01.MA_THE_BHYT) && x.XML01.MA_THE_BHYT.Length >= 15 && x.XML01.MA_THE_BHYT.Substring(4, 11) == subMaThe &&
                                                                                maDvKhacs.Contains(x.MA_DICH_VU) && x.NGAY_TH_YL > tuNgay && x.NGAY_TH_YL < denNgay && (hdsd.MA_LOAI_KCB == null || x.XML01.MA_LOAI_KCB.Contains(hdsd.MA_LOAI_KCB.ToString())));

                                        RaiseError(x3ToiThieu != null,
                                            int.Parse(hdsd.MA_LOI), x3ToiThieu == null ? "" :
                                            $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                            $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                            $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                            $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                            $"Chi tiết:" +
                                            $"Bệnh nhân được kê: {File.TEN_DICH_VU}|{File.MA_DICH_VU}, y lệnh:{File.NGAY_YL}, ngày thực hiện:{File.NGAY_TH_YL}" +
                                            $"\n Bệnh nhân đã thực hiện dịch vụ này:{x3ToiThieu.MA_DICH_VU}, y lệnh {x3ToiThieu.NGAY_YL}, ngày thực hiện:{x3ToiThieu.NGAY_TH_YL}" +
                                            $"\n, khoảng cách giữa 2 lần thực hiện là:{(File.NGAY_YL - x3ToiThieu.NGAY_YL).Value.TotalDays} < thời gian tối thiêu:{hdsd.THOI_GIAN_TOI_THIEU}" +
                                            $"\n Dịch vụ khác quy định trong HDSD là:{x3ToiThieu.MA_DICH_VU}" +
                                            $"\n Mã LK: {x3ToiThieu.MA_LK}"
                                            );
                                    }
                                }
                            }
                            //wa.Stop();
                            //if (wa.Elapsed.TotalMilliseconds > 300)
                            //{
                            //    Uti.HandleCronJobLog(_logger, $"HDSD Dịch vụ Tối thiểu {File.XML01ID} - MA CSKCB {File.MA_CSKCB} - Ma LK {File.MA_LK}, thời gian: {wa.Elapsed.TotalMilliseconds}", nameof(Validate));
                            //}
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Validate130: MaBN: {MaBN} MaCSKCB: {MaCSKCB}; STT Bị Lỗi: {STT} XML3 Giám định có lỗi [{Time}]",
                               File.XML01.MA_BN,
                               File.XML01.MA_CSKCB,
                               File.STT,
                               DateTime.Now.ToString("dd/MM HH:mm:ss"));
                        }

                        //Linhist added 06/07/26
                        if (!string.IsNullOrEmpty(hdsd.THUOC_KHAC))
                        {
                            if (File.XML01.XML02s.Any(x => Uti.CodeMatch(x.MA_THUOC, hdsd.THUOC_KHAC)))
                            {
                                RaiseError(true,
                                    int.Parse(hdsd.MA_LOI),
                                    $"Bệnh nhân: {File.XML01.HO_TEN} (Mã BN: {File.XML01.MA_BN})\n" +
                                    $"Mã ra viện: {File.XML01.MA_LOAI_KCB}|{ServiceConstants.GetValue(ServiceConstants.Loairavien, File.XML01.MA_LOAI_RV.ToString())}\n" +
                                    $"Mã bệnh chính: {File.XML01.MA_BENH_CHINH} - {benhchinhInfo}\n" +
                                    $"Chi tiết:" +
                                    $"Nội dung lỗi: {(errorlist.TryGetValue(int.Parse(hdsd.MA_LOI), out var Erinfo) ? Erinfo : "Không tìm thấy nội dung lỗi")}\n" +
                                    $"Bệnh nhân được kê: {File.MA_DICH_VU}|{File.TEN_DICH_VU}, đã bao gồm thuốc {hdsd.THUOC_KHAC} trong dịch vụ, không phải kê khai thêm thuốc ngoài"
                                    //$"(Dịch vụ khác quy định của dịch vụ: {hdsd.DICH_VU_KHAC})\n"
                                    );
                                //hasErr = true;
                                //goto RAISE_ERROR_HDSD_DICHVU;
                            }

                        }    
                    }
                    

                //RAISE_ERROR_HDSD_DICHVU:
                //        RaiseError(hasErr, int.Parse(hdsd.MA_LOI));
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Validate130: MaBN: {MaBN} MaCSKCB: {MaCSKCB}; STT Bị Lỗi: {STT} XML3 Giám định có lỗi [{Time}]",
                        File.XML01.MA_BN,
                        File.XML01.MA_CSKCB,
                        File.STT,
                        DateTime.Now.ToString("dd/MM HH:mm:ss"));
                }

            }
        }
    }
}
