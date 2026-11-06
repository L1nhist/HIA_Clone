using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class XML09 : IXMLEntity<XML01>
    {
        public long XML09ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_LK { get; set; }
        public int? STT { get; set; }

        public string? MA_BHXH_NND { get; set; }
        public string? MA_THE_NND { get; set; }
        public string? HO_TEN_NND { get; set; }
        public DateTime? NGAYSINH_NND { get; set; }
        public string? MA_DANTOC_NND { get; set; }
        public string? SO_CCCD_NND { get; set; }
        public DateTime? NGAYCAP_CCCD_NND { get; set; }
        public string? NOICAP_CCCD_NND { get; set; }
        public string? NOI_CU_TRU_NND { get; set; }
        public string? MA_QUOCTICH { get; set; }
        public string? MATINH_CU_TRU { get; set; }
        public string? MAHUYEN_CU_TRU { get; set; }
        public string? MAXA_CU_TRU { get; set; }
        public string? HO_TEN_CHA { get; set; }
        public string? MA_THE_TAM { get; set; }
        public string? HO_TEN_CON { get; set; }
        public byte? GIOI_TINH_CON { get; set; }
        public byte? SO_CON { get; set; }
        public byte? LAN_SINH { get; set; }
        public byte? SO_CON_SONG { get; set; }
        public int? CAN_NANG_CON { get; set; }
        public DateTime? NGAY_SINH_CON { get; set; }
        public string? NOI_SINH_CON { get; set; }
        public string? TINH_TRANG_CON { get; set; }
        public byte? SINHCON_PHAUTHUAT { get; set; }
        public byte? SINHCON_DUOI32TUAN { get; set; }
        public string? GHI_CHU { get; set; }
        public string? NGUOI_DO_DE { get; set; }
        public string? NGUOI_GHI_PHIEU { get; set; }
        public DateTime? NGAY_CT { get; set; }
        public string? SO { get; set; }
        public string? QUYEN_SO { get; set; }
        public string? MA_TTDV { get; set; }
        public string? DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; } = null!;

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML09s.Any(x => x.STT == STT))
            {
                STT = (x01.XML09s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML09s.Add(this);
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