
using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class DMDichVu : ICategoryData
    {
        long ICategoryData.ID
        {
            get => DVID;
            set => DVID = (int)value;
        }

        string? ICategoryData.MA_DM
        {
            get => MA_DICH_VU;
            set => MA_DICH_VU = value;
        }

        public int DVID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_DICH_VU { get; set; }
        public string? TEN_DICH_VU { get; set; }
        public string? MA_GIA { get; set; }
        public double? DON_GIA { get; set; }
        public string? QUYET_DINH { get; set; }
        public byte? MANHOM_9324 { get; set; }
        public byte? MANHOM_5937 { get; set; }
        public string? TU_NGAY { get; set; }
        public string? DEN_NGAY { get; set; }
        public string? NGAY_DE_NGHI { get; set; }
    }

    //public partial class DMDichVu
    //{
    //    public int DichVuID { get; set; }
    //    public string MA_CSKCB { get; set; }
    //    public string MA_DVKT { get; set; }
    //    public string MA_AX { get; set; }
    //    public string TEN_DVKT { get; set; }
    //    public string TEN_AX { get; set; }
    //    public string MA_GIA { get; set; }
    //    public double? DON_GIA { get; set; }
    //    public double? GIA_AX { get; set; }
    //    public double? GIA_TRAN { get; set; }
    //    public string QUYET_DINH { get; set; }
    //    public string CONG_BO { get; set; }
    //    public string MANHOM_9324 { get; set; }
    //    public bool? DVKTC { get; set; }
    //    public string HIEULUC { get; set; }
    //    public string KETQUA { get; set; }
    //    public string LYDOTUCHOI { get; set; }
    //    public string MANHOM_5937 { get; set; }
    //    public string STT { get; set; }
    //    public string MA_DICH_VU { get; set; }
    //    public string TEN_DICH_VU { get; set; }
    //    public string QUY_TRINH { get; set; }
    //    public string CSKCB_CGKT { get; set; }
    //    public string CSKCB_CLS { get; set; }
    //    public string ID { get; set; }
    //}
}