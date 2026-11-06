using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class XML13 : IXMLEntity<XML01>
    {
        public long XML13ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_LK { get; set; }
        public int? STT { get; set; }
        public string? SO_HOSO { get; set; }
        public string? SO_CHUYENTUYEN { get; set; }
        public string? GIAY_CHUYEN_TUYEN { get; set; }
        public string? MA_NOI_DI { get; set; }
        //Linhist added 06/02/24 - thay MA_NOI_DI
        public string? MA_CSKCB_DI { get; set; }
        public string? MA_NOI_DEN { get; set; }
        //Linhist added 06/02/24 - thay MA_NOI_DEN
        public string? MA_CSKCB_DEN { get; set; }
        public string? HO_TEN { get; set; }
        public DateTime? NGAY_SINH { get; set; }
        public byte? GIOI_TINH { get; set; }
        public string? MA_QUOCTICH { get; set; }
        public string? MA_DANTOC { get; set; }
        public string? MA_NGHE_NGHIEP { get; set; }
        public string? DIA_CHI { get; set; }
        public string? MA_THE_BHYT { get; set; }
        public string? GT_THE_DEN { get; set; }
        public DateTime? NGAY_VAO { get; set; }
        public DateTime? NGAY_VAO_NOI_TRU { get; set; }
        public DateTime? NGAY_RA { get; set; }
        public string? DAU_HIEU_LS { get; set; }
        public string? CHAN_DOAN_RV { get; set; }
        public string? QT_BENHLY { get; set; }
        public string? TOMTAT_KQ { get; set; }
        public string? PP_DIEUTRI { get; set; }
        //Linhist added 06/02/24 - thay PP_DIEU_TRI
        public string? TINH_TRANG_CT { get; set; }
        public string? MA_BENH_CHINH { get; set; }
        public string? MA_BENH_KT { get; set; }
        public string? MA_BENH_YHCT { get; set; }
        public string? TEN_DICH_VU { get; set; }
        public string? TEN_THUOC { get; set; }
        public string? PP_DIEU_TRI { get; set; }
        public byte? MA_LOAI_RV { get; set; }
        public byte? MA_LYDO_CT { get; set; }
        public string? HUONG_DIEU_TRI { get; set; }
        public string? PHUONGTIEN_VC { get; set; }
        public string? HOTEN_NGUOI_HT { get; set; }
        public string? CHUCDANH_NGUOI_HT { get; set; }
        public string? MA_BAC_SI { get; set; }
        public string? MA_TTDV { get; set; }
        public string? DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; } = null!;

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML11s.Any(x => x.STT == STT))
            {
                STT = (x01.XML11s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML13s.Add(this);
        }

        public void RaiseError(int MA_LOI, string? errDesc = null)
        {
            XML01.Errors.Add(new XMLError()
            {
                XML01 = XML01,
                XML01ID = XML01ID,
                MA_CSKCB = MA_CSKCB,
                STT = STT,
                MA_LOI = MA_LOI,
                ErrorDesc = errDesc,
                ErrorDate = DateTime.Now,
            });
        }
    }
}