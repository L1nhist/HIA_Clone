using HIA.Data.Base;

namespace HIA.Data.Entities
{
    public partial class XML11 : IXMLEntity<XML01>
    {
        public long XML11ID { get; set; }
        public long XML01ID { get; set; }
        public string MA_CSKCB { get; set; } = string.Empty;
        public string? MA_LK { get; set; }
        public int? STT { get; set; }

        public string? SO_CT { get; set; }
        public string? SO_SERI { get; set; }
        public string? SO_KCB { get; set; }
        public string? DON_VI { get; set; }
        public string? MA_BHXH { get; set; }
        public string? MA_THE_BHYT { get; set; }
        public string? CHAN_DOAN_RV { get; set; }
        public string? PP_DIEUTRI { get; set; }
        public byte? MA_DINH_CHI_THAI { get; set; }
        public string? NGUYENNHAN_DINHCHI { get; set; }
        public byte? TUOI_THAI { get; set; }
        public byte? SO_NGAY_NGHI { get; set; }
        public DateTime? TU_NGAY { get; set; }
        public DateTime? DEN_NGAY { get; set; }
        public string? HO_TEN_CHA { get; set; }
        public string? HO_TEN_ME { get; set; }
        public string? MA_TTDV { get; set; }
        public string? MA_BS { get; set; }
        public DateTime? NGAY_CT { get; set; }
        public string? MA_THE_TAM { get; set; }
        public string? MAU_SO { get; set; }
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

            x01.XML11s.Add(this);
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