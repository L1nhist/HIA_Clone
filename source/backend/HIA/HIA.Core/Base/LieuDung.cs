using System.Text;

namespace HIA.Core.Base;
/// <summary>
/// Phân tích chuỗi liều dùng thuốc (dạng text tự do theo 5 định dạng cố định) thành dữ liệu có cấu
/// trúc. KHÔNG dùng Regex — chỉ quét từng ký tự (index thủ công) và so sánh từ khoá không cấp phát
/// chuỗi con (string.Compare theo vị trí), số được cộng dồn trực tiếp từ mã ký tự thay vì cắt
/// Substring rồi decimal.Parse — tối ưu hiệu năng, gần như không cấp phát bộ nhớ trong lúc parse
/// (ngoại trừ chính object LieuDung kết quả).
///
/// DonVi được truyền vào TRƯỚC khi parse (biết trước đơn vị cần khớp trong chuỗi — vd "viên", "ml"),
/// so sánh không phân biệt hoa/thường (áp dụng chung cho mọi từ khoá trong chuỗi, không chỉ riêng
/// DonVi — dùng StringComparison.OrdinalIgnoreCase). Dấu '*' và 'x'/'X' dùng thay nhau được, khoảng
/// trắng linh hoạt.
///
/// 5 định dạng nhận diện được:
///   1) "<SoLuong> g|gam|gram x <SoLan> thang x <SoNgay> ngày"
///   2) "<SoLan> lần/ngày x <SoNgay> ngày"
///   3) "<SoLuong> DonVi/lần x <SoLan> lần/ngày x <SoNgay> ngày [<TrongNgay> DonVi/ngày]"
///   4) "Sáng: <Sang> DonVi, Trưa: <Trua> DonVi, Chiều: <Chieu> DonVi, Tối: <Toi> DonVi [<TrongNgay> DonVi/ngày]"
///   5) Giống (4) nhưng có thêm "x <SoNgay> ngày" trước phần TrongNgay tuỳ chọn.
/// Định dạng (4)/(5): Sáng/Trưa/Chiều/Tối có thể chỉ xuất hiện 1, 2, hoặc 3 trong 4 mốc — mốc nào
/// vắng mặt giữ giá trị mặc định 0. CÁC MỐC CÓ MẶT PHẢI ĐÚNG THỨ TỰ Sáng→Trưa→Chiều→Tối trong chuỗi
/// (bỏ mốc nào cũng được, nhưng không đảo thứ tự những mốc còn lại).
///
/// TongSo và TrongNgay (khi chuỗi không nêu rõ) là giá trị TỰ TÍNH theo từng định dạng — xem phần
/// trả lời kèm theo lúc giao code để biết công thức, đây là điểm cần xác nhận lại vì đề bài không
/// nêu rõ.
/// </summary>
public class LieuDung
{
    private static readonly string[] _donviLoaitru = new string[] { "chai", "lọ", "túi" };

    public string DonVi { get; private set; } = string.Empty;
    public decimal SoLuong { get; private set; }
    public decimal SoLan { get; private set; }
    public decimal SoNgay { get; private set; }
    public decimal TrongNgay { get; private set; }
    public decimal TongSo { get; private set; }
    public decimal Sang { get; private set; }
    public decimal Trua { get; private set; }
    public decimal Chieu { get; private set; }
    public decimal Toi { get; private set; }

    public int Format { get; private set; } = 0;

    /// <summary>Chuỗi gốc đã parse thành công — giữ lại để tham chiếu/debug.</summary>
    public string RawText { get; private set; } = string.Empty;

    private LieuDung() { }

    /// <summary>Phân tích 1 chuỗi liều dùng. donVi là đơn vị đã biết trước (vd "viên", "ml") — so
    /// sánh không phân biệt hoa/thường với đơn vị xuất hiện trong chuỗi.</summary>
    /// <returns>0 nếu khớp được 1 trong 5 định dạng, ngược lại trả về mã lỗi.</returns>
    //2345	0	XML2	LIEU_DUNG	[130] Liều dùng không đúng định dạng 130 (Số viên/ lần * số lần/ ngày * số ngày [Viên/ngày]
    public static int TryParse(string text, string donVi, out LieuDung? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(donVi)) return 357;

        var t = text.Trim();
        var dv = donVi.Trim();

        if (text.Contains("thang"))
        {
            return TryParseFormat1(t, dv, out result);
        }
        if (text.Contains("sáng") || text.Contains("trưa") || text.Contains("chiều") || text.Contains("tối"))
        {
            return TryParseFormat45(t, dv, out result);
        }

        if (text.Contains("lần"))
        {
            if (text.Contains("[")) return TryParseFormat3(t, dv, out result);
            return TryParseFormat2(t, dv, out result);
        }

        return 2345;
    }

    // ---------------- Định dạng 1: <SoLuong> g|gam|gram x <SoLan> thang x <SoNgay> ngày ----------------
    // 4042	4	XML2	LIEU_DUNG	[3176] Liều dùng thuốc Y Học Cổ truyền không đúng định dạng theo QĐ 3176
    private static int TryParseFormat1(string s, string dv, out LieuDung? result)
    {
        result = null;
        var i = 0;

        //3997	3	XML2	LIEU_DUNG	[3176] [Thái Bình] Trong Liều Dùng tại XML2, số lượng ghi trong liều dùng phải là số nguyên (Đối với thuốc YHCT)
        if (!TryReadInt(s, ref i, out var soLuong)) return 3997;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soLuong == 0) return 886;

        SkipWs(s, ref i);
        // Thử "gram"/"gam" trước "g" (dài nhất trước) để không nuốt nhầm 1 ký tự rồi kẹt lại.
        if (!TryMatchLiteral(s, ref i, "gram") && !TryMatchLiteral(s, ref i, "gam") && !TryMatchLiteral(s, ref i, "g")) return 4042;
        if (!TryMatchSep(s, ref i)) return 4042;

        //3997	3	XML2	LIEU_DUNG	[3176] [Thái Bình] Trong Liều Dùng tại XML2, số lượng ghi trong liều dùng phải là số nguyên (Đối với thuốc YHCT)
        if (!TryReadInt(s, ref i, out var soLan)) return 3997;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soLan == 0) return 886;

        SkipWs(s, ref i);
        if (!TryMatchLiteral(s, ref i, "thang")) return 4042;
        if (!TryMatchSep(s, ref i)) return 4042;

        //3997	3	XML2	LIEU_DUNG	[3176] [Thái Bình] Trong Liều Dùng tại XML2, số lượng ghi trong liều dùng phải là số nguyên (Đối với thuốc YHCT)
        if (!TryReadNumber(s, ref i, out var soNgay)) return 3997;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soNgay == 0) return 886;

        SkipWs(s, ref i);
        if (!TryMatchLiteral(s, ref i, "ngày")) return 4042;
        SkipWs(s, ref i);

        //4042	4	XML2	LIEU_DUNG	[3176] Liều dùng thuốc Y Học Cổ truyền không đúng định dạng theo QĐ 3176
        if (i != s.Length) return 4042;

        result = new LieuDung
        {
            RawText = s,
            Format = 1,
            DonVi = dv,
            SoLuong = soLuong,
            SoLan = soLan,
            SoNgay = soNgay,
            TrongNgay = soLuong * soLan,
            TongSo = soLuong * soLan * soNgay,
        };
        return 0;
    }

    // ---------------- Định dạng 2: <SoLan> lần/ngày x <SoNgay> ngày ----------------
    //4201	2	XML2	LIEU_DUNG	[3176] Liều dùng không đúng định dạng (Số viên/ lần * số ngày)
    private static int TryParseFormat2(string s, string dv, out LieuDung? result)
    {
        result = null;
        var i = 0;

        if (!TryReadNumber(s, ref i, out var soLan)) return 4201;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soLan == 0m) return 886;

        SkipWs(s, ref i);
        if (!TryMatchSlash(s, ref i, "lần", "ngày")) return 4201;
        if (!TryMatchSep(s, ref i)) return 4201;

        //4201	2	XML2	LIEU_DUNG	[3176] Liều dùng không đúng định dạng (Số viên/ lần * số ngày)
        if (!TryReadNumber(s, ref i, out var soNgay)) return 4201;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soNgay == 0m) return 886;

        SkipWs(s, ref i);
        if (!TryMatchLiteral(s, ref i, "ngày")) return 4201;
        SkipWs(s, ref i);

        //4201	2	XML2	LIEU_DUNG	[3176] Liều dùng không đúng định dạng (Số viên/ lần * số ngày)
        if (i != s.Length) return 4201;

        result = new LieuDung
        {
            RawText = s,
            Format = 2,
            DonVi = dv,
            SoLan = soLan,
            SoNgay = soNgay,
            TrongNgay = soLan * soNgay,
            TongSo = soLan * soNgay,
        };
        return 0;
    }

    // ---------------- Định dạng 3: <SoLuong> DonVi/lần x <SoLan> lần/ngày x <SoNgay> ngày [<TrongNgay> DonVi/ngày] ----------------

    //2345	0	XML2	LIEU_DUNG	[130] Liều dùng không đúng định dạng 130 (Số viên/ lần * số lần/ ngày * số ngày [Viên/ngày]
    private static int TryParseFormat3(string s, string dv, out LieuDung? result)
    {
        result = null;
        var i = 0;

        if (!TryReadNumber(s, ref i, out var soLuong)) return 2345;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soLuong == 0m) return 886;

        SkipWs(s, ref i);
        //2394	4	XML2	LIEU_DUNG	[130] Đơn vị tính trong liều dùng không đúng với Đơn vị tính của thuốc
        if (!TryMatchSlash(s, ref i, dv, "lần")) return 2394;
        if (!TryMatchSep(s, ref i)) return 2345;

        if (!TryReadNumber(s, ref i, out var soLan)) return 2345;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soLan == 0m) return 886;

        SkipWs(s, ref i);
        if (!TryMatchSlash(s, ref i, "lần", "ngày")) return 2345;
        if (!TryMatchSep(s, ref i)) return 2345;

        if (!TryReadNumber(s, ref i, out var soNgay)) return 2345;
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (soNgay == 0m) return 886;

        SkipWs(s, ref i);
        if (!TryMatchLiteral(s, ref i, "ngày")) return 2345;

        //2395	4	XML2	LIEU_DUNG	[130] Đơn vị tính của số lần và Đơn vị tính tổng số lần trong ngày khác nhau, đề nghị xem lại
        if (!TryMatchTrongNgaySuffix(s, ref i, dv, out var trongNgay, out var err)) return err > 0 ? err : 2395;
        //2350	0	XML2	LIEU_DUNG	[130] Liều dùng sai công thức hướng dẫn tại QĐ 4750
        else if (trongNgay != soLuong * soLan) return 2350;

        SkipWs(s, ref i);
        if (i != s.Length) return 2345;

        result = new LieuDung
        {
            RawText = s,
            Format = 3,
            DonVi = dv,
            SoLuong = soLuong,
            SoLan = soLan,
            SoNgay = soNgay,
            TrongNgay = trongNgay,
            TongSo = soLuong * soLan * soNgay,
        };
        return 0;
    }

    // ---------------- Định dạng 4/5: Sáng/Trưa/Chiều/Tối (1-4 mốc, đúng thứ tự) ----------------
    //4041	4	XML2	LIEU_DUNG	[3176] Liều dùng ko đúng định dạng 3176 (Sáng: xx Viên, Chiều: xx Viên, Tối:xx Viên [xx viên/ngày]
    private static int TryParseFormat45(string s, string dv, out LieuDung? result)
    {
        result = null;
        var i = 0;
        var err = 0;

        var coSang = TryMatchMoc(s, ref i, "Sáng", dv, out var sang, out err);
        if (err > 0) return err;
        var coTrua = TryMatchMoc(s, ref i, "Trưa", dv, out var trua, out err);
        if (err > 0) return err;
        var coChieu = TryMatchMoc(s, ref i, "Chiều", dv, out var chieu, out err);
        if (err > 0) return err;
        var coToi = TryMatchMoc(s, ref i, "Tối", dv, out var toi, out err);
        if (err > 0) return err;

        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        if ((coSang && sang == 0m) || (coTrua && trua == 0m) || (coChieu && chieu == 0m) || (coToi && toi == 0m))
        {
            return 886;
        }
        //4049	3	XML2	LIEU_DUNG	[3176] Thái Bình - Trần Lãm: Nếu chỉ kê số lượng thuốc cần lần trong ngày bằng nhau => Đề nghị chuyển sang dạng kê Số Viên / Lần * Số Lần/ ngày * số ngày [Tổng số thuốc/ngày])
        else
        {
            var g = 0m;
            var t = 0m;
            var c = 0m;
            var o = 0m;
            if (coSang)
            {
                g = sang;
                t = trua;
                c = chieu;
                o = toi;
            }
            if (coTrua)
            {
                g = g > 0m ? g : trua;
                t = trua;
                c = c > 0m ? c : trua;
                o = o > 0m ? o : trua;
            }
            if (coChieu)
            {
                g = g > 0m ? g : chieu;
                t = t > 0m ? t : chieu;
                c = chieu;
                o = o > 0m ? o : chieu;
            }
            if (coToi)
            {
                g = g > 0m ? g : toi;
                t = t > 0m ? t : toi;
                c = c > 0m ? c : toi;
                o = toi;
            }

            if (g != 0m && g == t && t == c && c == o)
            {
                return 4049;
            }
        }

        // Định dạng 5: có thêm "x <SoNgay> ngày" — thử khớp, rollback nếu không đủ cả 2 phần.
        decimal soNgay = 0;
        var coSoNgay = false;
        var save = i;
        if (TryMatchSep(s, ref i))
        {
            if (TryReadNumber(s, ref i, out soNgay))
            {
                SkipWs(s, ref i);
                if (TryMatchLiteral(s, ref i, "ngày"))
                {
                    coSoNgay = true;
                }
                else
                {
                    i = save;
                }
            }
            else
            {
                i = save;
            }
        }


        if (!TryMatchTrongNgaySuffix(s, ref i, dv, out var trongNgay, out err)) return 4041;
        else if (err != 0) return err;
        //2350	0	XML2	LIEU_DUNG	[130] Liều dùng sai, Tổng số thuốc trong ngày không khớp với số viên/ lần * số lần/ngày
        else if (trongNgay != sang + trua + chieu + toi) return 2350;

        SkipWs(s, ref i);
        if (i != s.Length) return 4041;


        result = new LieuDung
        {
            RawText = s,
            Format = coSoNgay ? 5 : 4,
            DonVi = dv,
            Sang = sang,
            Trua = trua,
            Chieu = chieu,
            Toi = toi,
            SoNgay = soNgay,
            TrongNgay = trongNgay,
            TongSo = (sang + trua + chieu + toi) * (coSoNgay ? soNgay : 1m),
        };
        return 0;
    }

    // ---------------- Nguyên liệu quét chuỗi thủ công (không Regex, không cấp phát Substring) ----------------

    private static void SkipWs(string s, ref int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
    }

    /// <summary>Đọc 1 số (nguyên hoặc thập phân, dấu thập phân . hoặc ,) bắt đầu từ vị trí i (đã tự
    /// bỏ qua khoảng trắng đầu). Cộng dồn trực tiếp từ mã ký tự — KHÔNG cắt Substring rồi
    /// decimal.Parse.</summary>
    private static bool TryReadInt(string s, ref int i, out int value)
    {
        SkipWs(s, ref i);
        value = 0;
        var start = i;
        while (i < s.Length && s[i] >= '0' && s[i] <= '9')
        {
            value = value * 10 + (s[i] - '0');
            i++;
        }
        return i > start;
    }

    /// <summary>Đọc 1 số (nguyên hoặc thập phân, dấu thập phân . hoặc ,) bắt đầu từ vị trí i (đã tự
    /// bỏ qua khoảng trắng đầu). Cộng dồn trực tiếp từ mã ký tự — KHÔNG cắt Substring rồi
    /// decimal.Parse.</summary>
    private static bool TryReadNumber(string s, ref int i, out decimal value)
    {
        SkipWs(s, ref i);
        var start = i;
        decimal intPart = 0;
        while (i < s.Length && s[i] >= '0' && s[i] <= '9')
        {
            intPart = intPart * 10 + (s[i] - '0');
            i++;
        }
        if (i == start)
        {
            value = 0;
            return false;
        }

        decimal fracPart = 0;
        if (i < s.Length && (s[i] == '.' || s[i] == ','))
        {
            var afterSep = i + 1;
            var scale = 0.1m;
            var j = afterSep;
            while (j < s.Length && s[j] >= '0' && s[j] <= '9')
            {
                fracPart += (s[j] - '0') * scale;
                scale *= 0.1m;
                j++;
            }
            // Chỉ coi là phần thập phân nếu có ít nhất 1 chữ số theo sau dấu . hoặc , — tránh nuốt
            // nhầm dấu phẩy phân cách trong chuỗi (vd "5, ngày") thành dấu thập phân.
            if (j > afterSep) i = j;
        }

        value = intPart + fracPart;
        return true;
    }

    /// <summary>So khớp 1 từ khoá cố định tại vị trí i (đã tự bỏ qua khoảng trắng đầu), không phân
    /// biệt hoa/thường, KHÔNG cấp phát chuỗi con (dùng string.Compare theo vị trí).</summary>
    private static bool TryMatchLiteral(string s, ref int i, string literal)
    {
        SkipWs(s, ref i);
        if (i + literal.Length > s.Length) return false;
        if (string.Compare(s, i, literal, 0, literal.Length, StringComparison.OrdinalIgnoreCase) != 0) return false;
        i += literal.Length;
        return true;
    }

    /// <summary>So khớp dấu phân cách nhân — 'x'/'X'/'*'.</summary>
    private static bool TryMatchSep(string s, ref int i)
    {
        SkipWs(s, ref i);
        if (i < s.Length && (s[i] == 'x' || s[i] == 'X' || s[i] == '*'))
        {
            i++;
            SkipWs(s, ref i);
            return true;
        }
        return false;
    }

    /// <summary>So khớp dấu phân cách nhóm — '['.</summary>
    private static bool TryMatchGroup(string s, bool start, ref int i)
    {
        SkipWs(s, ref i);
        if (i < s.Length && (s[i] == (start ? '[' : ']')))
        {
            i++;
            SkipWs(s, ref i);
            return true;
        }
        return false;
    }

    /// <summary>So khớp "left/right" (vd "lần/ngày", "DonVi/lần"), khoảng trắng quanh dấu / linh
    /// hoạt. Rollback về vị trí ban đầu nếu không khớp trọn vẹn.</summary>
    private static bool TryMatchSlash(string s, ref int i, string left, string right)
    {
        var save = i;
        if (!TryMatchLiteral(s, ref i, left)) { i = save; return false; }
        SkipWs(s, ref i);
        if (i >= s.Length || s[i] != '/') { i = save; return false; }
        i++;
        SkipWs(s, ref i);
        if (!TryMatchLiteral(s, ref i, right)) { i = save; return false; }
        return true;
    }

    /// <summary>So khớp 1 mốc "Nhãn: Số DonVi[,]" (định dạng 4/5). Rollback nếu không khớp trọn vẹn
    /// — để các mốc phía sau (nếu có) không bị lệch vị trí quét.</summary>
    private static bool TryMatchMoc(string s, ref int i, string label, string dv, out decimal value, out int error)
    {
        value = 0;
        error = 0;
        var save = i;
        if (!TryMatchLiteral(s, ref i, label)) { i = save; return false; }
        SkipWs(s, ref i);
        if (i >= s.Length || s[i] != ':') { i = save; return false; }
        i++;
        if (!TryReadNumber(s, ref i, out value)) { i = save; return false; }
        //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        else if (value == 0m) { i = save; error = 886; return false; }
        SkipWs(s, ref i);

        //2394	4	XML2	LIEU_DUNG	[130] Đơn vị tính trong liều dùng không đúng với Đơn vị tính của thuốc
        if (!TryMatchLiteral(s, ref i, dv)) { i = save; error = 2394; return false; }
        SkipWs(s, ref i);
        if (i < s.Length && s[i] == ',')
        {
            i++;
            SkipWs(s, ref i);
        }
        return true;
    }

    /// <summary>So khớp phần đuôi tuỳ chọn "<TrongNgay> DonVi/ngày" (dùng chung cho định dạng 3 và
    /// 4/5). Nếu khớp thì ghi đè trongNgay; nếu không thì rollback, giữ nguyên giá trị mặc định
    /// caller đã truyền vào.</summary>
    private static bool TryMatchTrongNgaySuffix(string s, ref int i, string dv, out decimal trongNgay, out int error)
    {
        trongNgay = 0;
        error = 0;
        var save = i;

        SkipWs(s, ref i);
        if (!TryMatchGroup(s, true, ref i)) { i = save; return false; }
        SkipWs(s, ref i);
        if (!TryReadNumber(s, ref i, out var tn)) { i = save; return false; }
        SkipWs(s, ref i);

        //2395	4	XML2	LIEU_DUNG	[130] Đơn vị tính của số lần và Đơn vị tính tổng số lần trong ngày khác nhau, đề nghị xem lại
        if (!TryMatchSlash(s, ref i, dv, "ngày")) { i = save; error = 2395; return false; }
        SkipWs(s, ref i);
        if (!TryMatchGroup(s, false, ref i)) { i = save; return false; }

        trongNgay = tn;
        return true;
    }
}
