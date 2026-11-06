using System;

namespace HIA.Core.Models.Files
{
    public class XML1DownloadModel
    {
        public int XML1ID { get; set; }
        public string HospitalId { get; set; }
        public int? MHTCID { get; set; }
        public byte? CHECKED { get; set; }
        public string DanhSachLoi { get; set; }
        public string FileName { get; set; }
        public DateTime? NGAYLAP { get; set; }
        public string MA_LK { get; set; }
        public int? STT { get; set; }
        public string MA_BN { get; set; }
        public string HO_TEN { get; set; }
        public DateTime? NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string DIA_CHI { get; set; }
        public string MA_THE { get; set; }
        public string MA_DKBD { get; set; }
        public string GT_THE_TU { get; set; }
        public string GT_THE_DEN { get; set; }
        public string MIEN_CUNG_CT { get; set; }
        public string TEN_BENH { get; set; }
        public string MA_BENH { get; set; }
        public string MA_BENHKHAC { get; set; }
        public byte? MA_LYDO_VVIEN { get; set; }
        public string MA_NOI_CHUYEN { get; set; }
        public byte? MA_TAI_NAN { get; set; }
        public DateTime? NGAY_VAO { get; set; }
        public DateTime? NGAY_RA { get; set; }
        public int? SO_NGAY_DTRI { get; set; }
        public byte? KET_QUA_DTRI { get; set; }
        public byte? TINH_TRANG_RV { get; set; }
        public DateTime? NGAY_TTOAN { get; set; }
        public double? T_THUOC { get; set; }
        public double? T_VTYT { get; set; }
        public double? T_TONGCHI { get; set; }
        public double? T_BNTT { get; set; }
        public double? T_BNCCT { get; set; }
        public double? T_BHTT { get; set; }
        public double? T_NGUONKHAC { get; set; }
        public double? T_NGOAIDS { get; set; }
        public int? NAM_QT { get; set; }
        public byte? THANG_QT { get; set; }
        public byte? MA_LOAI_KCB { get; set; }
        public string MA_KHOA { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_KHUVUC { get; set; }
        public string MA_PTTT_QT { get; set; }
        public double? CAN_NANG { get; set; }
        public bool? isDelete { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? Creator { get; set; }
        public string MA_NOI_DI { get; set; }
        public string MA_NOI_DEN { get; set; }
    }
}
