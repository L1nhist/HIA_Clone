using System.Text.Json.Serialization;

namespace HIA.Core.Models.Diagram;

/// <summary>
/// Response gốc của GET /api/mdinsight/vien/{ma_cskcb}.
/// Toàn bộ số liệu đã được tính sẵn phía server — không tự suy công thức khác.
/// </summary>
public class AIStatModel
{
    /// <summary>Mã CSKCB đã tra. Giữ nguyên dạng chuỗi (có thể có số 0 đứng đầu) — KHÔNG ép sang số.</summary>
    [JsonPropertyName("ma_cskcb")]
    public string MaCskcb { get; set; } = string.Empty;

    [JsonPropertyName("ten_vien")]
    public string TenVien { get; set; } = string.Empty;

    /// <summary>
    /// Ngày dữ liệu "hôm nay" phản ánh, theo LỊCH (có thể lùi ngày nếu vài ngày gần nhất
    /// chưa có dữ liệu mới). Dùng cho Day/HoatDong/ChatLuong/DayLai.
    /// KHÁC với TopLoiLapNgayNeo (xem bên dưới).
    /// </summary>
    [JsonPropertyName("ngay_bao_cao")]
    public string NgayBaoCao { get; set; } = string.Empty;

    /// <summary>
    /// true = Viện có thật nhưng không tìm được ngày nào có dữ liệu trong cửa sổ đối chiếu.
    /// Khi true: Day/HoatDong/ChatLuong/DayLai sẽ là object rỗng, TopLoiLap là mảng rỗng.
    /// </summary>
    [JsonPropertyName("khong_co_du_lieu")]
    public bool KhongCoDuLieu { get; set; }

    /// <summary>
    /// Ngày LÀM VIỆC gần nhất có dữ liệu — neo "hôm nay" riêng cho TopLoiLap[].*_hom_nay/*_binh_quan/*_pct.
    /// Bỏ qua Thứ Bảy/Chủ Nhật/lễ VN, dù có dữ liệu rơi vào ngày đó.
    /// null nếu không tìm được ngày làm việc nào có dữ liệu trong cửa sổ (hiếm, VD trọn kỳ nghỉ Tết) —
    /// khi đó các trường *_hom_nay/*_binh_quan/*_pct trong từng TopLoiLapItem sẽ vắng mặt (null).
    /// </summary>
    [JsonPropertyName("top_loi_lap_ngay_neo")]
    public string? TopLoiLapNgayNeo { get; set; }

    /// <summary>Số ngày làm việc thật dùng tính "bình quân" cho TopLoiLap. 0 khi TopLoiLapNgayNeo = null.</summary>
    [JsonPropertyName("top_loi_lap_so_ngay_binh_quan")]
    public int TopLoiLapSoNgayBinhQuan { get; set; }

    /// <summary>
    /// Tổng số hồ sơ (không lọc mã lỗi) Viện đẩy vào đúng ngày TopLoiLapNgayNeo —
    /// dùng để tự đánh giá độ tin cậy của % xu hướng khi khối lượng dữ liệu ngày đó thấp.
    /// 0 khi TopLoiLapNgayNeo = null.
    /// </summary>
    [JsonPropertyName("top_loi_lap_so_ho_so_ngay_neo")]
    public int TopLoiLapSoHoSoNgayNeo { get; set; }

    [JsonPropertyName("day")]
    public DayInfo Day { get; set; } = new();

    [JsonPropertyName("hoat_dong")]
    public HoatDongInfo HoatDong { get; set; } = new();

    [JsonPropertyName("chat_luong")]
    public ChatLuongInfo ChatLuong { get; set; } = new();

    [JsonPropertyName("day_lai")]
    public DayLaiInfo DayLai { get; set; } = new();

    /// <summary>Top mã lỗi lặp lại nhiều ngày liên tiếp, tối đa 10 phần tử, sắp theo SoLuot giảm dần.</summary>
    [JsonPropertyName("top_loi_lap")]
    public List<TopLoiLapItem> TopLoiLap { get; set; } = new();

    [JsonPropertyName("dataLines")]
    public List<DataPoints> DataLines { get; set; } = new();

    [JsonPropertyName("dataBars")]
    public List<DataPoints> DataBars { get; set; } = new();

    [JsonPropertyName("dataPies")]
    public List<DataPointsPie> DataPies { get; set; } = new();
}

/// <summary>Thống kê đẩy hồ sơ (mục "day").</summary>
public class DayInfo
{
    /// <summary>Số hồ sơ bệnh án đã đẩy, tính tới ngày NgayBaoCao.</summary>
    [JsonPropertyName("so_ho_so")]
    public int SoHoSo { get; set; }

    /// <summary>Tổng số LẦN đẩy (1 hồ sơ có thể đẩy lại nhiều lần).</summary>
    [JsonPropertyName("so_lan_day")]
    public int SoLanDay { get; set; }

    /// <summary>SoLanDay / SoHoSo — trung bình mỗi hồ sơ bị đẩy lại mấy lần.</summary>
    [JsonPropertyName("lan_day_moi_ho_so")]
    public double LanDayMoiHoSo { get; set; }

    [JsonPropertyName("so_ho_so_hom_qua")]
    public int SoHoSoHomQua { get; set; }

    [JsonPropertyName("so_lan_day_hom_qua")]
    public int SoLanDayHomQua { get; set; }

    /// <summary>Trung bình cộng trong cửa sổ vài ngày gần nhất (xu hướng, khác "hôm qua").</summary>
    [JsonPropertyName("so_ho_so_bq")]
    public double SoHoSoBinhQuan { get; set; }

    /// <summary>% thay đổi so với hôm qua. null khi hôm qua chưa có dữ liệu để so (không phải 0%).</summary>
    [JsonPropertyName("thay_doi_ho_so_vs_hom_qua_pct")]
    public double? ThayDoiHoSoVsHomQuaPct { get; set; }

    /// <summary>% thay đổi so với bình quân. null khi bình quân chưa có dữ liệu để so.</summary>
    [JsonPropertyName("thay_doi_ho_so_vs_bq_pct")]
    public double? ThayDoiHoSoVsBinhQuanPct { get; set; }

    [JsonPropertyName("lan_day_moi_ho_so_bq")]
    public double LanDayMoiHoSoBinhQuan { get; set; }

    [JsonPropertyName("theo_ngay")]
    public List<TheoNgayInfo> TheoNgay { get; set; } = new();
}

public class TheoNgayInfo
{
    [JsonPropertyName("ngay")]
    public string Ngay { get; set; } = string.Empty;

    [JsonPropertyName("so_ho_so")]
    public int SoHoSo { get; set; }

    [JsonPropertyName("so_lan_day")]
    public double SoLanDay { get; set; }
}

/// <summary>Thống kê hoạt động người dùng (mục "hoat_dong").</summary>
public class HoatDongInfo
{
    /// <summary>Tổng số người dùng ĐĂNG KÝ của Viện trên MDinsight (không phân biệt có dùng hay không).</summary>
    [JsonPropertyName("tong_so_nguoi_dung")]
    public int TongSoNguoiDung { get; set; }

    /// <summary>Số người có ít nhất 1 hành động bất kỳ trong ngày (đăng nhập/xem file/đẩy dữ liệu/...).</summary>
    [JsonPropertyName("nguoi_co_hoat_dong")]
    public int NguoiCoHoatDong { get; set; }

    /// <summary>Số người lấy báo cáo hoặc hỏi trợ lý AI trong ngày — tập con của NguoiCoHoatDong.</summary>
    [JsonPropertyName("nguoi_tuong_tac")]
    public int NguoiTuongTac { get; set; }

    /// <summary>NguoiTuongTac / NguoiCoHoatDong * 100.</summary>
    [JsonPropertyName("ty_le_tuong_tac_pct")]
    public double TyLeTuongTacPct { get; set; }

    [JsonPropertyName("nguoi_co_hoat_dong_bq")]
    public double NguoiCoHoatDongBinhQuan { get; set; }

    [JsonPropertyName("nguoi_tuong_tac_bq")]
    public double NguoiTuongTacBinhQuan { get; set; }

    [JsonPropertyName("ty_le_tuong_tac_bq_pct")]
    public double TyLeTuongTacBinhQuanPct { get; set; }

    /// <summary>Danh sách người CÓ hoạt động đúng ngày NgayBaoCao. Rỗng khi KhongCoDuLieu = true.</summary>
    [JsonPropertyName("nguoi_dung_hoat_dong")]
    public List<NguoiDung> NguoiDungHoatDong { get; set; } = new();

    /// <summary>
    /// Danh sách người KHÔNG có hoạt động đúng ngày đó.
    /// Tổng số phần tử của 2 danh sách LUÔN bằng TongSoNguoiDung.
    /// </summary>
    [JsonPropertyName("nguoi_dung_khong_hoat_dong")]
    public List<NguoiDung> NguoiDungKhongHoatDong { get; set; } = new();
}

public class NguoiDung
{
    [JsonPropertyName("ten")]
    public string Ten { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>Thống kê chất lượng dữ liệu (mục "chat_luong").</summary>
public class ChatLuongInfo
{
    /// <summary>Tổng số lỗi validate phát hiện trong ngày NgayBaoCao.</summary>
    [JsonPropertyName("tong_loi")]
    public int TongLoi { get; set; }

    [JsonPropertyName("tong_loi_hom_qua")]
    public int TongLoiHomQua { get; set; }

    [JsonPropertyName("tong_loi_bq")]
    public double TongLoiBinhQuan { get; set; }

    [JsonPropertyName("thay_doi_loi_vs_bq_pct")]
    public double? ThayDoiLoiVsBinhQuanPct { get; set; }

    /// <summary>% hồ sơ có ít nhất 1 lỗi.</summary>
    [JsonPropertyName("pct_ho_so_co_loi")]
    public double PctHoSoCoLoi { get; set; }

    [JsonPropertyName("pct_ho_so_co_loi_bq")]
    public double PctHoSoCoLoiBinhQuan { get; set; }

    /// <summary>TongLoi / SoLanDay — trung bình mỗi lần đẩy sinh bao nhiêu lỗi.</summary>
    [JsonPropertyName("loi_moi_lan_day")]
    public double LoiMoiLanDay { get; set; }

    [JsonPropertyName("loi_moi_lan_day_bq")]
    public double LoiMoiLanDayBinhQuan { get; set; }
}

/// <summary>Thống kê đẩy lại hồ sơ đã từng lỗi (mục "day_lai").</summary>
public class DayLaiInfo
{
    /// <summary>Số hồ sơ đã từng bị lỗi rồi được đẩy lại trong cửa sổ đối chiếu.</summary>
    [JsonPropertyName("tong_so_ca")]
    public int TongSoCa { get; set; }

    /// <summary>Đẩy lại và lỗi ÍT ĐI — "sửa có kết quả".</summary>
    [JsonPropertyName("so_ca_cai_thien")]
    public int SoCaCaiThien { get; set; }

    /// <summary>Đẩy lại nhưng lỗi Y HỆT lần trước.</summary>
    [JsonPropertyName("so_ca_y_het")]
    public int SoCaYHet { get; set; }

    /// <summary>Đẩy lại nhưng lỗi NHIỀU HƠN.</summary>
    [JsonPropertyName("so_ca_xau_hon")]
    public int SoCaXauHon { get; set; }


    [JsonPropertyName("danh_sach_khong_chinh_sua")]
    public List<HosoDayLaiInfo> DSKoChinhSua { get; set; } = new();


    [JsonPropertyName("danh_sach_y_het")]
    public List<HosoDayLaiInfo> DSYHet { get; set; } = new();


    [JsonPropertyName("danh_sach_cai_thien")]
    public List<HosoDayLaiInfo> DSCaiThien { get; set; } = new();


    [JsonPropertyName("danh_sach_xau_hon")]
    public List<HosoDayLaiInfo> DSXauHon { get; set; } = new();
}

/// <summary>Thống kê đẩy lại hồ sơ đã từng lỗi (mục "day_lai").</summary>
public class HosoDayLaiInfo
{
    [JsonPropertyName("ma_lk")]
    public string MA_LK { get; set; } = string.Empty;

    [JsonPropertyName("so_loi")]
    public int SO_LOI { get; set; }

    [JsonPropertyName("ho_ten")]
    public string HO_TEN { get; set; } = string.Empty;

    [JsonPropertyName("so_bhyt")]
    public string SO_BHYT { get; set; } = string.Empty;

    [JsonPropertyName("ngay_sinh")]
    public string NGAY_SINH { get; set; } = string.Empty;

    [JsonPropertyName("ma_chan_doan")]
    public string MA_CHAN_DOAN { get; set; } = string.Empty;

    [JsonPropertyName("ngay_vao")]
    public string NGAY_VAO { get; set; } = string.Empty;

    [JsonPropertyName("ngay_ra")]
    public string NGAY_RA { get; set; } = string.Empty;

    [JsonPropertyName("ma_khoa")]
    public string MA_KHOA { get; set; } = string.Empty;

    [JsonPropertyName("nguoi_gui")]
    public NguoiDung? NGUOI_GUI { get; set; }
}

/// <summary>Một mã lỗi lặp lại trong "top_loi_lap".</summary>
public class TopLoiLapItem
{
    /// <summary>Mã lỗi (errorCode của MDinsight).</summary>
    [JsonPropertyName("ma_loi")]
    public string MaLoi { get; set; } = string.Empty;

    /// <summary>Tên lỗi dễ đọc. null nếu tra từ điển tên lỗi tạm thời lỗi (hiếm).</summary>
    [JsonPropertyName("ten_loi")]
    public string? TenLoi { get; set; }

    [JsonPropertyName("file_xml")]
    public string FileXml { get; set; } = string.Empty;

    /// <summary>Tổng số LƯỢT lỗi xuất hiện, trong cửa sổ đối chiếu THEO LỊCH (không liên quan nhóm field xu hướng bên dưới).</summary>
    [JsonPropertyName("so_luot")]
    public int SoLuot { get; set; }

    /// <summary>Số hồ sơ KHÁC NHAU dính lỗi này, cùng cửa sổ theo lịch.</summary>
    [JsonPropertyName("so_ho_so_dinh")]
    public int SoHoSoDinh { get; set; }

    // ----- Nhóm field xu hướng, neo theo NGÀY LÀM VIỆC (TopLoiLapNgayNeo ở response gốc) -----
    // Vắng mặt (null) khi TopLoiLapNgayNeo = null ở response gốc.

    /// <summary>Số lượt mã lỗi này xuất hiện vào ngày TopLoiLapNgayNeo (ngày làm việc).</summary>
    [JsonPropertyName("so_luot_hom_nay")]
    public int? SoLuotHomNay { get; set; }

    /// <summary>
    /// Bình quân số lượt/ngày, tính trực tiếp từ dữ liệu thô trong cửa sổ TopLoiLapSoNgayBinhQuan
    /// ngày làm việc trước TopLoiLapNgayNeo. KHÔNG phải SoLuot chia số ngày (2 cửa sổ khác nhau).
    /// </summary>
    [JsonPropertyName("so_luot_binh_quan")]
    public double? SoLuotBinhQuan { get; set; }

    /// <summary>% thay đổi lượt lỗi: hôm nay (ngày làm việc neo) so với bình quân. Dương = tăng (xấu đi).</summary>
    [JsonPropertyName("thay_doi_so_luot_pct")]
    public double? ThayDoiSoLuotPct { get; set; }

    /// <summary>Số hồ sơ dính lỗi này vào ngày TopLoiLapNgayNeo.</summary>
    [JsonPropertyName("so_ho_so_dinh_hom_nay")]
    public int? SoHoSoDinhHomNay { get; set; }

    /// <summary>Bình quân số hồ sơ dính/ngày, cùng cửa sổ ngày làm việc nói trên.</summary>
    [JsonPropertyName("so_ho_so_dinh_binh_quan")]
    public double? SoHoSoDinhBinhQuan { get; set; }

    /// <summary>% thay đổi số hồ sơ dính: hôm nay (ngày làm việc neo) so với bình quân.</summary>
    [JsonPropertyName("thay_doi_so_ho_so_dinh_pct")]
    public double? ThayDoiSoHoSoDinhPct { get; set; }
}
