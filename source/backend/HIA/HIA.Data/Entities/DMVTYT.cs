
using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class DMVTYT : ICategoryData
    {
        long ICategoryData.ID
        {
            get => VTID;
            set => VTID = (int)value;
        }

        string? ICategoryData.MA_DM
        {
            get => MA_VAT_TU;
            set => MA_VAT_TU = value;
        }

        public int VTID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_VAT_TU { get; set; }
        public string? NHOM_VAT_TU { get; set; }
        public string? TEN_VAT_TU { get; set; }
        public string? MA_HIEU { get; set; }
        public string? QUY_CACH { get; set; }
        public string? DON_VI_TINH { get; set; }
        public double? DON_GIA { get; set; }
        public double? DON_GIA_BH { get; set; }
        public double? SO_LUONG { get; set; }
        public string? NUOC_SX { get; set; }
        public string? NHA_SX { get; set; }
        public string? NHA_THAU { get; set; }
        public string? LOAI_THAU { get; set; }
        public string? TT_THAU { get; set; }
        public string? HT_THAU { get; set; }
        public string? QUYET_DINH { get; set; }
        public double? DINH_MUC { get; set; }
        public string? TYLE_TT_BH { get; set; }
        public byte? MANHOM_9324 { get; set; }
        public byte? MANHOM_5937 { get; set; }
        public string? TU_NGAY { get; set; }
        public string? DEN_NGAY { get; set; }
        public string? NGAY_DE_NGHI { get; set; }
    }

    //public partial class DMVTYT
    //{
    //    public int VTYTID { get; set; }
    //    public string MA_CSKCB { get; set; }
    //    public string MA_NHOM_VTYT { get; set; }
    //    public string MA_NHOM_AX { get; set; }
    //    public string TEN_NHOM_VTYT { get; set; }
    //    public string TEN_NHOM_AX { get; set; }
    //    public string MA_HIEU { get; set; }
    //    public string MA_VTYT_BV { get; set; }
    //    public string TEN_VTYT_BV { get; set; }
    //    public string QUY_CACH { get; set; }
    //    public string NUOC_SX { get; set; }
    //    public string HANG_SX { get; set; }
    //    public string DON_VI_TINH { get; set; }
    //    public double? DON_GIA { get; set; }
    //    public double? DON_GIA_TT { get; set; }
    //    public double? GIA_TRAN { get; set; }
    //    public string NHA_THAU { get; set; }
    //    public string QUYET_DINH { get; set; }
    //    public string CONG_BO { get; set; }
    //    public double? DINH_MUC { get; set; }
    //    public double? SO_LUONG { get; set; }
    //    public string LOAI_THAU { get; set; }
    //    public string MANHOM_9324 { get; set; }
    //    public string HIEULUC { get; set; }
    //    public string KETQUA { get; set; }
    //    public string LYDOTUCHOI { get; set; }
    //    public string MANHOM_5937 { get; set; }
    //    public string STT { get; set; }
    //    public string MA_VAT_TU { get; set; }
    //    public string NHOM_VAT_TU { get; set; }
    //    public string TEN_VAT_TU { get; set; }
    //    public string DON_GIA_BH { get; set; }
    //    public string TYLE_TT_BH { get; set; }
    //    public string TT_THAU { get; set; }
    //    public string TU_NGAY { get; set; }
    //    public string DEN_NGAY { get; set; }
    //    public string HT_THAU { get; set; }
    //    public string ID { get; set; }
    //}
}