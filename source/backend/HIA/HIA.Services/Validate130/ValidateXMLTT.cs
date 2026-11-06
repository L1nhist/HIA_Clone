using System.Globalization;
using System.Text;
using HIA.Core.Base;
using HIA.Data.Records;
using HIA.Repositories.CheckThe;
using HIA.Services.HosoManagement130;
using Microsoft.Extensions.Logging;

namespace HIA.Services.Validate130;

public class ValidateXMLTT : BaseValidator<XML01>
{
    private readonly static string[] erNums = "001,002,003,010,050,051,052,053,054,055,060,061,070,100,101,110,120,121,122,123,124,130,205,401".Split(',');
    private readonly static int[] erCods = new int[] {
        222, 223, 3454, 010, 214, 209, 2566, 2567, 2570, 2571, 215, 227, 216, 228, 221, 230, 231, 232, 233, 234, 235, 236, 2568, 2569
    };

    private readonly ICheckThe130Repository _checkTheRepository;

    public ValidateXMLTT
    (
        ICheckThe130Repository checkTheRepository,
        ILogger<dynamic>? logger = null
    ) : base(logger)
    {
        _checkTheRepository = checkTheRepository;
    }

    private static bool Check_TheTE_ChuaDiHoc(string maThe, DateTime? ngaySinh)
    {
        if (string.IsNullOrEmpty(maThe) || !maThe.StartsWith("TE1")) return false;
        if (ngaySinh == null || !ngaySinh.HasValue) return false;

        double tuoiBN = (double)((DateTime.Now - ngaySinh.Value).Days / 365.242199);
        DateTime dMax = new DateTime(DateTime.Now.Year, 9, 30).AddDays(1);

        if (DateTime.Now.Year - ngaySinh.Value.Year <= 6)
        {
            if (tuoiBN < 6 || (tuoiBN < 7 && DateTime.Now < dMax)) return true;
        }
        else
        {
            if (tuoiBN < 6) return true;
        }
        //if (tuoiBN < 6 || (tuoiBN < 7 && DateTime.Now < dMax)) return true;

        return false;
    }

    public override async Task<bool> Validate(XML01 xml)
    {
        // Kiem tra neu XML null thi ko chay tiep nua
        if (!await base.Validate(xml)) return false;

        try
        {
            //Linhist added 170826
            //Kiểm tra loại bỏ thẻ TE chưa đi học
            //Linhist changed 021026 - kiểm tra cả thẻ trẻ em chưa đi học
            //if (Check_TheTE_ChuaDiHoc(File.MA_THE_BHYT, File.NGAY_SINH)) return false;

            var checkThe = await _checkTheRepository.GetItemOrderBy(c => c.MA_CSKCB == File.MA_CSKCB && c.MA_BN == File.MA_BN && c.MA_THE_BHYT == File.MA_THE_BHYT, query => query.OrderByDescending(c => c.TG_CHECK_THE));
            if (checkThe == null) return false;

            //Linhist removed 30/07/26
            //if (checkThe.MA_DKBD != File.MA_DKBD || checkThe.NGAY_SINH != File.NGAY_SINH || checkThe.GIOI_TINH != File.GIOI_TINH || checkThe.GT_THE_DEN != File.GT_THE_DEN || checkThe.GT_THE_TU != File.GT_THE_TU || checkThe.GIOI_TINH != File.GIOI_TINH)
            //{
            //    //Nếu mã đăng ký ban đầu, ngày sinh, giới tính không khớp thì không cần verify
            //    return false;
            //}

            var maThes = (File.MA_THE_BHYT ?? string.Empty).Split(';');
            var gtTus = (File.GT_THE_TU ?? string.Empty).Split(';');
            var gtDens = (File.GT_THE_DEN ?? string.Empty).Split(';');
            var maLois = (checkThe.ERROR_CODE ?? string.Empty).Split(';');
            var maDKCBs = (checkThe.MA_DKBD ?? string.Empty).Split(';');
            var ndLois = new NhanLichSuKCB2025?[maThes.Length];

            try
            {
                var arrs = JsonDefaults.Deserialize<NhanLichSuKCB2025[][]>(checkThe.ERROR_CONTENTS ?? string.Empty)
                    ?? throw new InvalidOperationException("Empty ERROR_CONTENTS");
                for (var i = 0; i < arrs.Length && i < ndLois.Length; i++)
                {
                    ndLois[i] = arrs[i] != null && arrs[i].Length > 0 ? arrs[i][0] : null;
                    if (arrs[i] != null && arrs[i].Length > 1 && ndLois[i] is { } first)
                    {
                        first.dsLichSuKCB2025 = arrs[i][1].dsLichSuKCB2025;
                    }
                }
            }
            catch
            {
                try
                {
                    var arrs = JsonDefaults.Deserialize<NhanLichSuKCB2025[]>(checkThe.ERROR_CONTENTS ?? string.Empty)
                        ?? throw new InvalidOperationException("Empty ERROR_CONTENTS");
                    for (var i = 0; i < arrs.Length && i < ndLois.Length; i++)
                    {
                        ndLois[i] = arrs[i];
                    }
                }
                catch
                { }
            }

            var lstErrs = new List<int>();
            for (var i = 0; i < maThes.Length; i++)
            {
                var idx = Array.IndexOf(erNums, maLois[i]);
                if (idx >= 0 && !lstErrs.Contains(erCods[idx]))
                {
                    lstErrs.Add(erCods[idx]);
                    RaiseError(true, erCods[idx]);
                }
                else if (int.TryParse(maLois[i], out var erno) && erCods.Contains(erno))
                {
                    lstErrs.Add(erno);
                    RaiseError(true, erno);
                }

                var chkData = ndLois.Length > i && ndLois[i] != null ? ndLois[i] : null;
                if (chkData != null)
                {
                    if (!string.IsNullOrEmpty(chkData.maDKBD) && chkData.maDKBD != maDKCBs[i] && !lstErrs.Contains(218))
                    {
                        lstErrs.Add(218);
                        RaiseError(true, 218);
                    }

                    if (!string.IsNullOrEmpty(chkData.maDKBDMoi) && chkData.maDKBDMoi != maDKCBs[i] && !lstErrs.Contains(2585))
                    {
                        lstErrs.Add(2585);
                        RaiseError(true, 2585);
                    }
                    
                    //217 217 1   XML1 GIOI_TINH   Thẻ sai giới tính   LOI_THE
                    if (!string.IsNullOrEmpty(chkData.gioiTinh) && !Uti.IsMatchGender(chkData.gioiTinh, File.GIOI_TINH) && !lstErrs.Contains(217))
                    {
                        lstErrs.Add(217);
                        RaiseError(true, 217);
                    }

                    //Linhist removed 100826
                    ////Linhist added 30/07/26: Check ngaySinh khong khop voi ngaySinh trong lich su KCB
                    //if (!string.IsNullOrEmpty(chkData.ngaySinh) && File.NGAY_SINH?.ToString("dd/MM/yyyy") != chkData.ngaySinh && !lstErrs.Contains(216))
                    //{
                    //    lstErrs.Add(216);
                    //    RaiseError(true, 216);
                    //}

                    //Linhist added 100826
                    //216   1   XML1    NGAY_SINH   Thẻ sai ngày sinh
                    //4212	2	XML1	NGAY_SINH	[3176] Năm sinh đúng tuy nhiên BH trả về không đủ thông tin ngày tháng, viện cần cẩn thận
                    if (!string.IsNullOrEmpty(chkData.ngaySinh))
                    {
                        //Trường hợp có đủ ngày tháng năm sinh:
                        if (chkData.ngaySinh.Length == 10)
                        {
                            //Nếu đúng với ngày tháng năm sinh trong hồ sơ thì không báo lỗi
                            if (File.NGAY_SINH?.ToString("dd/MM/yyyy") == chkData.ngaySinh)
                            { }
                            //Nếu sai thì báo lỗi 216
                            else if (!lstErrs.Contains(216))
                            {
                                lstErrs.Add(216);
                                RaiseError(true, 216, $"Bệnh nhân có ngày sinh: {(File.NGAY_SINH?.ToString("dd/MM/yyyy") ?? "")} nhưng kết quả BH trả về: {chkData.ngaySinh}");
                            }
                        }
                        //Nếu chỉ có năm sinh:
                        else if (chkData.ngaySinh.Length == 4)
                        {
                            //Nếu đúng với năm sinh trong hồ sơ thì cảnh báo
                            if (File.NGAY_SINH?.Year.ToString() == chkData.ngaySinh)
                            {
                                if (!lstErrs.Contains(4212))
                                {
                                    lstErrs.Add(4212);
                                    RaiseError(true, 4212, $"Bệnh nhân có ngày sinh: {(File.NGAY_SINH?.ToString("dd/MM/yyyy") ?? "")} nhưng kết quả BH trả về: {chkData.ngaySinh}");
                                }
                            }
                            //Nếu sai thì báo lỗi 216
                            else if (!lstErrs.Contains(216))
                            {
                                lstErrs.Add(216);
                                RaiseError(true, 216, $"Bệnh nhân có ngày sinh: {(File.NGAY_SINH?.ToString("dd/MM/yyyy") ?? "")} nhưng kết quả BH trả về: {chkData.ngaySinh}");
                            }
                        }
                        else if (chkData.ngaySinh.Length == 7 && File.NGAY_SINH?.ToString("MM/yyyy") == chkData.ngaySinh)
                        {
                            var nams = chkData.ngaySinh.Split('/');

                            //Nếu đúng với năm sinh trong hồ sơ thì cảnh báo
                            if (nams.Length == 2 && File.NGAY_SINH?.Year.ToString() == nams[1] && (File.NGAY_SINH?.Month == 1 || File.NGAY_SINH?.Month.ToString("00") == nams[0]))
                            {
                                if (!lstErrs.Contains(4212))
                                {
                                    lstErrs.Add(4212);
                                    RaiseError(true, 4212, $"Bệnh nhân có ngày sinh: {(File.NGAY_SINH?.ToString("dd/MM/yyyy") ?? "")} nhưng kết quả BH trả về: {chkData.ngaySinh}");
                                }
                            }
                            //Nếu sai thì báo lỗi 216
                            else if (!lstErrs.Contains(216))
                            {
                                lstErrs.Add(216);
                                RaiseError(true, 216, $"Bệnh nhân có ngày sinh: {(File.NGAY_SINH?.ToString("dd/MM/yyyy") ?? "")} nhưng kết quả BH trả về: {chkData.ngaySinh}");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(chkData.gtTheTu))
                    {
                        //1183	1183	2	XML1	GT_THE_TU	Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về
                        if (chkData.gtTheTu != Uti.FixDateOrYear(gtTus[i]) && !lstErrs.Contains(1183))
                        {
                            lstErrs.Add(1183);
                            RaiseError(true, 1183);
                        }
                        //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
                        if (DateTime.TryParseExact(chkData.gtTheTu, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var gtd))
                        {
                            if (File.NGAY_VAO != null && File.NGAY_VAO.HasValue && File.NGAY_VAO.Value.Date < gtd.Date && !lstErrs.Contains(211))
                            {
                                lstErrs.Add(211);
                                RaiseError(true, 211);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(chkData.gtTheDen))
                    {
                        //1184	1184	2	XML1	GT_THE_DEN	Sai hạn thẻ: Giá trị thẻ đến sai so với giá trị trả về
                        if (chkData.gtTheDen != Uti.FixDateOrYear(gtDens[i]) && !lstErrs.Contains(1184))
                        {
                            lstErrs.Add(1184);
                            RaiseError(true, 1184);
                        }
                        //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
                        if (DateTime.TryParseExact(chkData.gtTheDen, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var gdd))
                        {
                            if (File.NGAY_RA != null && File.NGAY_RA.HasValue && File.NGAY_RA.Value.Date > gdd.Date && !lstErrs.Contains(212))
                            {
                                lstErrs.Add(212);
                                RaiseError(true, 212);
                            }
                        }
                    }
                }

                if (chkData != null && chkData.maKetQua == "000")
                {
                    var lstLS = new List<LichSuKhamChuaBenh>();
                    if (chkData.dsLichSuKCB2018 != null && chkData.dsLichSuKCB2018.Count > 0)
                    {
                        lstLS.AddRange(chkData.dsLichSuKCB2018);
                    }
                    if (chkData.dsLichSuKCB2025 != null && chkData.dsLichSuKCB2025.Count > 0)
                    {
                        lstLS.AddRange(chkData.dsLichSuKCB2025);
                    }

                    foreach (var ls in lstLS)
                    {
                        if (ls.maCSKCB == File.MA_CSKCB) continue;
                        if (!DateTime.TryParseExact(ls.ngayRa, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtOut)) continue;
                        if (File.NGAY_RA != null && File.NGAY_RA.HasValue && File.NGAY_VAO != null && File.NGAY_VAO.HasValue)
                        {
                            //2583	3	XML1	MA_THE_BHYT	[130] Bệnh nhân đã đi khám chữa bệnh tại đơn vị khác, ra viện cùng ngày
                            if (File.NGAY_RA.Value == dtOut && !lstErrs.Contains(2583))
                            {
                                lstErrs.Add(2583);
                                RaiseError(true, 2583);
                            }
                            //2584	3	XML1	MA_THE_BHYT	[130] Bệnh nhân có phát sinh chi phí KCB tại đơn vị khác trong thời gian nằm Viện
                            if (File.NGAY_VAO.Value <= dtOut.Date && File.NGAY_RA.Value >= dtOut && !lstErrs.Contains(2584))
                            {
                                lstErrs.Add(2584);
                                RaiseError(true, 2584);
                            }
                        }
                    }
                }
            }

            //Linhist added 30/07/26: Them thong tin da kiem tra TT
            //if (xml.State != null)
            //{
            //    xml.State.VerifyResult = checkThe.ERROR_CODE;
            //    xml.State.LastVerify = DateTime.Now;
            //}
            if (xml.State != null)
            {
                xml.State.VerifyDate = DateTime.Now;
            }
            return true;
        }
        catch (Exception ex)
        {
            var fullError = new StringBuilder();
            fullError.AppendLine($"❌ Validate130:XML1 lỗi tại MaBN: {File.MA_BN}, XML01ID: {File.XML01ID}, MaCSKCB: {File.MA_CSKCB}");
            fullError.AppendLine($"⏱️ Thời điểm: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            fullError.AppendLine($"📄 Message: {ex.Message}");
            fullError.AppendLine($"📌 StackTrace: {ex.StackTrace}");

            WriteLog(fullError.ToString(), nameof(Validate));
            return false;
        }

    }
}
