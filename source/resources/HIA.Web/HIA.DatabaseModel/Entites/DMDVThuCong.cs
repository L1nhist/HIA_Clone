#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class DMDVThuCong
    {
        public int DVTCID { get; set; }
        public string MA_CSKCB { get; set; }
        public string MA_DICH_VU { get; set; }
        public string TEN_DICH_VU { get; set; }
        public string MA_GIA { get; set; }
        public double? DON_GIA { get; set; }
        public string QUYET_DINH { get; set; }
        public byte? MANHOM_9324 { get; set; }
        public byte? MANHOM_5937 { get; set; }
        public string TU_NGAY { get; set; }
        public string DEN_NGAY { get; set; }
        public string NGAY_DE_NGHI { get; set; }
    }
}