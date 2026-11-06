
using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class DMThuoc : ICategoryData
    {
        long ICategoryData.ID
        {
            get => THID;
            set => THID = (int)value;
        }

        string? ICategoryData.MA_DM
        {
            get => MA_THUOC;
            set => MA_THUOC = value;
        }

        public int THID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_THUOC { get; set; }
        public string? TEN_THUOC { get; set; }
        public string? TEN_HOAT_CHAT { get; set; }
        public string? LOAI_THUOC { get; set; }
        public string? MA_DUONG_DUNG { get; set; }
        public string? DUONG_DUNG { get; set; }
        public string? HAM_LUONG { get; set; }
        public string? DANG_BAO_CHE { get; set; }
        public string? DON_VI_TINH { get; set; }
        public double? DON_GIA { get; set; }
        public double? DON_GIA_BH { get; set; }
        public double? SO_LUONG { get; set; }
        public string? QUY_CACH { get; set; }
        public string? SO_DANG_KY { get; set; }
        public string? MA_DVKT { get; set; }
        public string? NUOC_SX { get; set; }
        public string? NHA_SX { get; set; }
        public string? NHA_THAU { get; set; }
        public string? NHOM_THAU { get; set; }
        public string? LOAI_THAU { get; set; }
        public string? TT_THAU { get; set; }
        public string? HT_THAU { get; set; }
        public byte? MANHOM_9324 { get; set; }
        public byte? MANHOM_5937 { get; set; }
        public string? TU_NGAY { get; set; }
        public string? DEN_NGAY { get; set; }
        public string? NGAY_DE_NGHI { get; set; }
        public string? PP_CHEBIEN { get; set; }
    }
}