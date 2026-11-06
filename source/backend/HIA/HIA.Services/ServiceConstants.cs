namespace HIA.Core.Base;

public class ServiceConstants
{
    public const string MA_DOITUONG_KCB = "1;1.1;1.2;1.3;1.4;1.5;1.6;1.7;1.8;1.9;1.10;1.11;1.12;1.13;1.14;1.15;1.16;1.17;1.18;2;3.1;3.3;3.6;7;7.1;7.2;7.3;7.4;8;9;";
    public const string MA_DOITUONG_KCB_3276 = "1.1;1.2;1.3;1.4;1.5;1.6;1.7;1.11;1.12;1.13;1.14;1.15;1.16;1.17;1.18;2;3.1;3.2;3.3;3.6;7;7.2;7.3;7.4;8;9;10;";
    public const string MA_DOITUONG_THAI_BINH = "1.1;1.18;1.17;1.3;1.4;1.5;1.6;1.7;2;3.2;3.6;9";

    public static readonly string[] ALL_CHUC_DANH = { "Bác sĩ", "Y sĩ", "Điều dưỡng", "Hộ sinh", "Kỹ thuật viên", "Cử nhân X-quang", "Dược sĩ Đại học", "Dược sĩ Trung cấp" };
    public static readonly string[] DAYS_OF_WEEK = { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

    public static readonly Dictionary<string, string> LoaiKhamBenh = new()
    {
        { "01", "Khám Bệnh" },
        { "02", "Ngoại trú" },
        { "03", "Nội trú" },
        { "04", "Nội trú ban ngày" },
        { "05", "Ngoại trú các bệnh mãn tính dài ngày, khám bệnh và lấy thuốc" },
        { "06", "Điều trị lưu tại Trạm y tế xã, phòng khám đa khoa khu vực" },
        { "07", "Nhận thuốc theo hẹn" },
        { "08", "Ngoại trú các bệnh mãn tính dài ngày, khám bệnh và lấy thuốc, có thực hiện các dịch vụ kỹ thuật" },
        { "09", "Điều trị nội trú dưới 4h" },
        { "10", "Các trường hợp khác" }
    };

    public static readonly Dictionary<string, string> Loairavien = new()
    {
        { "1", "Ra viện" },
        { "2", "Chuyển tuyến/Chuyển cơ sở KBCB theo yêu cầu chuyên môn" },
        { "3", "Trốn viện" },
        { "4", "Xin ra viện" },
        { "5", "Chuyển tuyến/Chuyển cơ sở KBCB theo yêu cầu người bệnh" }
    };

    public static readonly Dictionary<string, string> ketquadieutri = new()
    {
        { "1", "Khỏi" },
        { "2", "Đỡ" },
        { "3", "Không thay đổi" },
        { "4", "Nặng hơn" },
        { "5", "Tử vong tại cơ sở KBCB" },
        { "6", "Tiên lượng nặng xin về" },
        { "7", "Chưa xác định (không thuộc một trong các mã kết quả điều trị nêu trên)" },
        { "8", "Tử vong ngoại viện" }
    };

    public static readonly Dictionary<string, string> gioitinh = new()
    {
        { "1", "Nam" },
        { "2", "Nữ" },
        { "3", "Chưa xác định" }
        
    };

    public static string GetValue(Dictionary<string, string> dict, string? id)
    {
        return id != null && dict.TryGetValue(id, out var value) ? value : "Không xác định";
    }

    public static readonly Dictionary<string, string> MaDoiTuongKCB = new()
    {
        { "1.1", "Đến KCB đúng cơ sở KCB nơi đăng ký KCB BHYT ban đầu" },
        { "1.2", "Tự đến KCB tại cơ sở KCB cấp ban đầu bao gồm trạm y tế, y học gia đình, quân - dân y, trung tâm y tế huyện..." },
        { "1.3", "Đến KCB có phiếu chuyển cơ sở KCB (bao gồm phiếu chuyển có giá trị 1 năm)" },
        { "1.4", "KCB khi thay đổi nơi lưu trú, cư trú theo quy định tại Điều 4 Thông tư 01/2025/TT-BYT" },
        { "1.5", "KCB theo phiếu hẹn khám lại theo Điều 11 Thông tư 01/2025/TT-BYT" },
        { "1.6", "Người đã hiến bộ phận cơ thể cần điều trị ngay sau khi hiến" },
        { "1.7", "Trẻ sơ sinh cần điều trị ngay sau khi sinh" },
        { "1.11", "Tự đến KCB tại cơ sở cấp ban đầu còn lại không thuộc mã 1.2" },
        { "1.12", "Tự đến KCB ngoại trú tại cơ sở cấp cơ bản đạt < 50 điểm hoặc tạm xếp cấp cơ bản" },
        { "1.13", "Tự đến KCB ngoại trú tại cơ sở cấp cơ bản đạt từ 50–70 điểm" },
        { "1.14", "Tự đến KCB ngoại trú tại cơ sở cấp cơ bản đã xác định là tuyến tỉnh/trung ương trước 01/01/2025" },
        { "1.15", "Tự đến KCB nội trú tại cơ sở cấp cơ bản" },
        { "1.16", "Tự đến KCB tại cơ sở cấp cơ bản với bệnh thuộc Phụ lục II" },
        { "1.17", "Tự đến KCB tại cơ sở cấp chuyên sâu với bệnh thuộc Phụ lục I" },
        { "1.18", "Tự đến KCB ngoại trú tại cơ sở cấp chuyên sâu đã xác định là tuyến tỉnh trước 01/01/2025" },
        { "2",    "Cấp cứu" },
        { "3.1",  "Tự đến KCB tại cơ sở cấp chuyên sâu đã xác định là tuyến trung ương" },
        { "3.3",  "Tự đến KCB tại cơ sở cấp cơ bản/chuyên sâu đã xác định là tuyến huyện" },
        { "3.6",  "Người dân tộc thiểu số, hộ nghèo, vùng khó khăn, xã đảo… đến KCB nội trú tại cơ sở chuyên sâu" },
        { "7",    "Lĩnh thuốc theo giấy hẹn trong trường hợp dịch bệnh hoặc bất khả kháng" },
        { "7.1",  "Người bệnh trực tiếp lĩnh thuốc" },
        { "7.2",  "Người bệnh ủy quyền người khác lĩnh thuốc" },
        { "7.3",  "Cơ sở KCB chuyển thuốc cho cơ sở KCB khác" },
        { "7.4",  "Cơ sở KCB chuyển thuốc đến cho người bệnh" },
        { "8",    "Thu hồi đề nghị thanh toán" },
        { "9",    "Người bệnh không KCB BHYT" }
    };
}
