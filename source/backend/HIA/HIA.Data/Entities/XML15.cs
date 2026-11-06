using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class XML15 : IXMLEntity<XML01>
    {
        public long XML15ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_LK { get; set; }
        public int? STT { get; set; }
        public string? MA_BN { get; set; }
        public string? HO_TEN { get; set; }
        public string? SO_CCCD { get; set; }
        public byte? PHANLOAI_LAO_VITRI { get; set; }
        public byte? PHANLOAI_LAO_TS { get; set; }
        public byte? PHANLOAI_LAO_HIV { get; set; }
        public byte? PHANLOAI_LAO_VK { get; set; }
        public byte? PHANLOAI_LAO_KT { get; set; }
        public byte? LOAI_DTRI_LAO { get; set; }
        public DateTime? NGAYBD_DTRI_LAO { get; set; }
        public byte? PHACDO_DTRI_LAO { get; set; }
        public DateTime? NGAYKT_DTRI_LAO { get; set; }
        public byte? KET_QUA_DTRI_LAO { get; set; }
        public DateTime? NGAYKD_HIV { get; set; }
        public DateTime? BDDT_ARV { get; set; }
        public DateTime? NGAY_BAT_DAU_DT_CTX { get; set; }
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

            x01.XML15s.Add(this);
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