using System.Collections.Generic;

namespace HIA.Core.Base
{
    public class MA_LOAI_KCB
    {
        public int Id { get; set; }
        public string Value { get; set; }

    }
    public class ServiceConstants
    {
        //MA_LOAI_KCB QĐ 824 BYT
        //1 Khám Bệnh
        //2 Ngoại trú
        //3 Nội trú
        //4 Nội trú ban ngày
        //5 Ngoại trú các bệnh mãn tính dài ngày, khám bệnh và lấy thuốc
        //6 Điều trị lưu tại Trạm y tế xã, phòng khám đa khoa khu vực
        //7 Nhận thuốc theo hẹn
        //8 Ngoại trú các bệnh mãn tính dài ngày, khám bệnh và lấy thuốc, có thực hiện các dịch vụ kỹ thuật
        //9 Điều trị nội trú dưới 4h
        //10 các TH khác

        //MA_DOITUONG_KCB QĐ 824 BYT
        //1 Đúng tuyến
        //1.1 Người tham gia bảo hiểm y tế đến khám bệnh, chữa bệnh đúng cơ sở khám bệnh, chữa bệnh ban đầu.
        //1.2 Người tham gia bảo hiểm y tế đăng ký khám bệnh, chữa bệnh ban đầu tại cơ sở khám bệnh, chữa bệnh tuyến Xã hoặc Phòng khám đa khoa hoặc Bệnh viện tuyến Huyện được quyền khám bệnh, chữa bệnh bảo hiểm y tế tại Trạm y tế tuyến xã hoặc Phòng khám đa khoa hoặc Bệnh viện tuyến huyện khác trong cùng địa bàn tỉnh
        //1.3 Người tham gia bảo hiểm y tế được chuyển tuyến khám bệnh, chữa bệnh bảo hiểm y tế theo quy định
        //1.4 Người tham gia bảo hiểm y tế có giấy tờ chứng minh đang ở tại địa phương khác trong thời gian đi công tác, làm việc lưu động, học tập trung theo các hình thức đào tạo, chương trình đào tạo, tạm trú và khám bệnh, chữa bệnh ban đầu tại cơ sở khám bệnh, chữa bệnh cùng tuyến hoặc tương đương với cơ sở đăng ký khám bệnh,
        //1.5 Người có giấy hẹn khám lại trong trường hợp đã được chuyển tuyến
        //1.6 Người đã hiến bộ phận cơ thể của mình phải điều trị ngay sau khi hiến bộ phận cơ thể.
        //1.7 Trẻ sơ sinh phải điều trị1 ngay sau khi sinh ra.
        //1.8 Các trường hợp KBCB theo quy định đối với người mắc bệnh lao theo quy định tại
        //1.9 Các trường hợp khám bệnh, chữa bệnh theo quy định đối với người mắc bệnh HIV/AIDS.
        //1.10 Các trường hợp khám bệnh, chữa bệnh theo quy định đối với người mắc bệnh COVID - 19.
        //1.11
        //1.12
        //1.13
        //1.14
        //1.15
        //1.16
        //1.17
        //1.18
        //2. Cấp cứu
        //3. Trái tuyến
        //3.1 Các trường hợp tự đi khám bệnh, chữa bệnh được chỉ định điều trị nội trú, nội trú ban ngày tại các bệnh viện tuyến trung ương
        // (Bỏ)3.2 Các trường hợp tự đi khám bệnh, chữa bệnh được chỉ định điều trị nội trú, nội trú ban ngày tại các bệnh viện tuyến tỉnh
        //3.3 Các trường hợp tự đi khám bệnh, chữa bệnh tại các bệnh viện tuyến huyện trên phạm vi cả nước
        //3.4(Bỏ) Trường hợp tự đi khám bệnh, chữa bệnh ngoại trú tại cơ sở khám bệnh, chữa bệnh tuyến trung ương (mức hưởng 0%, hay nói cách khác: quỹ bảo hiểm y tế không thanh toán chi phí khám bệnh, chữa bệnh bảo hiểm y
        //3.5 (Bỏ) Trường hợp tự đi khám bệnh, chữa bệnh ngoại trú tại cơ sở khám bệnh, chữa bệnh tuyến tỉnh (mức hưởng 0%, hay nói cách khác: quỹ bảo hiểm y tế không thanh toán chi phí khám bệnh, chữa bệnh bảo hiểm y tế
        //3.6 
        //3.7 (Bỏ)
        //7. Lĩnh thuốc theo giấy hẹn trong trường hợp dịch bệnh hoặc bất khả kháng hoặc do bất kỳ nguyên nhân nào (mức hưởng 100% theo phạm vi quyền lợi mức hưởng)
        //7.1
        //7.2
        //7.2
        //7.4
        //8 Thu hồi đề nghị thanh 8
        //9 Khám dịch vụ
        //"1";"1.1";"1.2";"1.3";"1.4";"1.5";"1.6";"1.7";"1.8";"1.9";"1.10";"2";"3";"3.1";"3.2";"3.3";"3.4";"3.5";"3.6";"3.7";"7";"7.1";"7.2";"7.3";"7.4";"8";"9"

        public const string MA_LOAI_KCB = "01;02;03;04;05;06;07;08;09;10";
        public const string DANG_HUONG_CHE_DO = "1;2;3;4;5;6;7";
        public const string MA_LOAI_RV = "1;2;3;4;5";
        public const string KET_QUA_DTRI = "1;2;3;4;5;6;7";
        public const string MA_TAI_NAN = "0;1;2;3;4;5;6;7;8";
        public const string MA_DOITUONG_KCB = "1;1.1;1.2;1.3;1.4;1.5;1.6;1.7;1.8;1.9;1.10;1.11;1.12;1.13;1.14;1.15;1.16;1.17;1.18;2;3.1;3.3;3.6;7;7.1;7.2;7.3;7.4;8;9;";

        public const string MA_DOITUONG_KCB_3276 = "1.1;1.2;1.3;1.4;1.5;1.6;1.7;1.11;1.12;1.13;1.14;1.15;1.16;1.17;1.18;2;3.1;3.2;3.3;3.6;7;7.2;7.3;7.4;8;9;10;";
        public const string MA_DOITUONG_THAI_BINH = "1.1;1.18;1.17;1.3;1.4;1.5;1.6;1.7;2;3.2;3.6;9";

        public static readonly string[] ALL_MA_LOAI_KCB = MA_LOAI_KCB.Split(';');
        public static readonly string[] ALL_DANG_HUONG_CHE_DO = DANG_HUONG_CHE_DO.Split(';');
        public static readonly string[] ALL_MA_LOAI_RV = MA_LOAI_RV.Split(';');
        public static readonly string[] ALL_KET_QUA_DTRI = KET_QUA_DTRI.Split(';');
        public static readonly string[] ALL_MA_TAI_NAN = MA_TAI_NAN.Split(';');
        public static readonly string[] ALL_MA_DOITUONG_KCB = MA_DOITUONG_KCB.Split(';');
        public static readonly string[] ALL_MA_DOITUONG_KCB_3276 = MA_DOITUONG_KCB_3276.Split(';');


        public static readonly string[] ALL_CHUC_DANH = { "Bác sĩ", "Y sĩ", "Điều dưỡng", "Hộ sinh", "Kỹ thuật viên", "Cử nhân X-quang", "Dược sĩ Đại học", "Dược sĩ Trung cấp" };
        public static readonly string[] DAYS_OF_WEEK = { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

        public static readonly Dictionary<string, string> LoaiKhamBenh = new Dictionary<string, string>
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
        public static readonly Dictionary<string, string> Loairavien = new Dictionary<string, string>
        {
            { "1", "Ra viện" },
            { "2", "Chuyển tuyến/Chuyển cơ sở KBCB theo yêu cầu chuyên môn" },
            { "3", "Trốn viện" },
            { "4", "Xin ra viện" },
            { "5", "Chuyển tuyến/Chuyển cơ sở KBCB theo yêu cầu người bệnh" }
        };
        public static readonly Dictionary<string, string> ketquadieutri = new Dictionary<string, string>
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
        public static readonly Dictionary<string, string> matainan = new Dictionary<string, string>
        {
            { "0", "Không xác định" },
            { "1", "Tai nạn giao thông" },
            { "2", "Tai nạn lao động" },
            { "3", "Tai nạn dưới nước" },
            { "4", "Bỏng" },
            { "5", "Bạo lực, xung đột" },
            { "6", "Tự tử" },
            { "7", "Ngộ độc các loại" },
            { "8", "Khác" }
        };
        public static readonly Dictionary<string, string> gioitinh = new Dictionary<string, string>
        {
            { "1", "Nam" },
            { "2", "Nữ" },
            { "3", "Chưa xác định" }
            
        };
        public static string GetValue(Dictionary<string, string> dict, string id)
        {
            return dict.TryGetValue(id, out var value) ? value : "Không xác định";
        }
        public static readonly Dictionary<string, string> MaDoiTuongKCB = new Dictionary<string, string>
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

        public static readonly Dictionary<string, string> MA_DOI_TUONG_KCB_3276 = new Dictionary<string, string>
        {
            { "1.1", "Đến KCB đúng cơ sở KCB nơi đăng ký KCB BHYT ban đầu (Trừ trường hợp quy định tại mã 1.2)" },
            { "1.2", "Đi KCB tại cơ sở KCB cấp ban đầu bao gồm trạm y tế, cơ sở KCB y học gia đình, trạm y tế quân - dân y, trung tâm y tế huyện... (bao gồm cả đúng và không đúng nơi đăng ký BHYT ban đầu)" },
            { "1.3", "Đến KCB có phiếu chuyển cơ sở KCB (Bao gồm Phiếu chuyển cơ sở KCB có giá trị 1 năm)" },
            { "1.4", "KCB khi thay đổi nơi lưu trú, nơi cư trú theo quy định tại Điều 4 Thông tư số 01/2025/TT-BYT" },
            { "1.5", "Đến KCB theo phiếu hẹn khám lại theo quy định tại Điều 11 Thông tư số 01/2025/TT-BYT" },
            { "1.6", "Người đã hiến bộ phận cơ thể phải điều trị ngay sau khi hiến" },
            { "1.7", "Trẻ sơ sinh phải điều trị ngay sau khi sinh ra" },
            { "1.11", "Tự đến KCB tại cơ sở KCB cấp ban đầu còn lại không thuộc mã 1.2" },
            { "1.12", "Tự đến KCB ngoại trú tại cơ sở KCB cấp cơ bản đạt dưới 50 điểm hoặc tạm xếp cấp cơ bản (trừ trường hợp tuyến tỉnh/trung ương trước 01/01/2025)" },
            { "1.13", "Tự đến KCB ngoại trú tại cơ sở KCB cấp cơ bản đạt từ 50 - 70 điểm (trừ bệnh thuộc Phụ lục II)" },
            { "1.14", "Tự đến KCB ngoại trú tại cơ sở KCB cấp cơ bản tuyến tỉnh/trung ương trước 01/01/2025 (trừ bệnh thuộc Phụ lục II)" },
            { "1.15", "Tự đến KCB nội trú tại cơ sở KCB cấp cơ bản" },
            { "1.16", "Tự đến KCB tại cơ sở KCB cấp cơ bản đối với bệnh thuộc Phụ lục II" },
            { "1.17", "Tự đến KCB tại cơ sở KCB cấp chuyên sâu đối với bệnh thuộc Phụ lục I" },
            { "1.18", "Tự đến KCB ngoại trú tại cơ sở KCB cấp chuyên sâu tuyến tỉnh trước 01/01/2025 (trừ bệnh thuộc Phụ lục I)" },
            { "2", "Cấp cứu" },
            { "3.1", "Tự đến KCB tại cơ sở KCB cấp chuyên sâu tuyến trung ương trước 01/01/2025 (trừ đối tượng mã 1.17)" },
            { "3.2", "Tự đến KCB nội trú tại cơ sở KCB cấp chuyên sâu tuyến tỉnh trước 01/01/2025 (trừ đối tượng mã 1.17)" },
            { "3.3", "Tự đến KCB tại cơ sở KCB cấp cơ bản/chuyên sâu tuyến huyện trước 01/01/2025" },
            { "3.6", "Người dân tộc thiểu số, hộ nghèo tại vùng khó khăn, xã đảo, huyện đảo đến KCB nội trú tại cơ sở KCB cấp chuyên sâu" },
            { "7", "Lĩnh thuốc theo giấy hẹn (chỉ lĩnh thuốc, không khám bệnh) trong dịch bệnh nhóm A hoặc bất khả kháng" },
            { "7.2", "Người bệnh uỷ quyền cho người khác đến cơ sở KCB lĩnh thuốc" },
            { "7.3", "Người bệnh lĩnh thuốc tại cơ sở KCB khác do không thể đến nơi cấp giấy hẹn (dịch bệnh nhóm A)" },
            { "7.4", "Cơ sở KCB chuyển thuốc đến cho người bệnh (dịch bệnh nhóm A)" },
            { "8", "Thu hồi đề nghị thanh toán" },
            { "9", "Người bệnh không KCB BHYT" },
            { "10", "Người bệnh đến cơ sở KCB lĩnh thuốc theo giấy hẹn (chỉ lĩnh thuốc, không khám bệnh, trừ trường hợp mã 7)" }
        };




    }
}