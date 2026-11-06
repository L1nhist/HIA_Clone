using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class XML07 : IXMLEntity<XML01>
    {
        public long XML07ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_LK { get; set; }
        public int? STT { get; set; }

        public string? SO_LUU_TRU { get; set; }
        public string? MA_YTE { get; set; }
        public string? MA_KHOA_RV { get; set; }
        public DateTime? NGAY_VAO { get; set; }
        public DateTime? NGAY_RA { get; set; }
        public byte? MA_DINH_CHI_THAI { get; set; }
        public string? NGUYENNHAN_DINHCHI { get; set; }
        public DateTime? THOIGIAN_DINHCHI { get; set; }
        public byte? TUOI_THAI { get; set; }
        public string? CHAN_DOAN_RV { get; set; }
        public string? PP_DIEUTRI { get; set; }
        public string? GHI_CHU { get; set; }
        public string? MA_TTDV { get; set; }
        public string? MA_BS { get; set; }
        public string? TEN_BS { get; set; }
        public DateTime? NGAY_CT { get; set; }
        public string? MA_CHA { get; set; }
        public string? MA_ME { get; set; }
        public string? MA_THE_TAM { get; set; }
        public string? HO_TEN_CHA { get; set; }
        public string? HO_TEN_ME { get; set; }
        public byte? SO_NGAY_NGHI { get; set; }
        public DateTime? NGOAITRU_TUNGAY { get; set; }
        public DateTime? NGOAITRU_DENNGAY { get; set; }
        public string? DU_PHONG { get; set; }

        public virtual XML01 XML01 { get; set; } = null!;

        public void AppendTo(XML01 x01)
        {
            XML01 = x01;
            XML01ID = x01.XML01ID;
            if (STT == null || x01.XML07s.Any(x => x.STT == STT))
            {
                STT = (x01.XML07s.Max(x => x.STT) ?? 0) + 1;
            }

            x01.XML07s.Add(this);
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