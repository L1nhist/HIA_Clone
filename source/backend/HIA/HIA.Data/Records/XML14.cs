namespace HIA.Data.Records;

public partial class XML14 : IRecord<XML01>
{
    public long XML14ID { get; set; }
    public long XML01ID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_LK { get; set; }
    public int? STT { get; set; }
    public string? SO_GIAYHEN_KL { get; set; }
    public string? HO_TEN { get; set; }
    public DateTime? NGAY_SINH { get; set; }
    public byte? GIOI_TINH { get; set; }
    public string? DIA_CHI { get; set; }
    public string? MA_THE_BHYT { get; set; }
    public string? GT_THE_DEN { get; set; }
    public DateTime? NGAY_VAO { get; set; }
    public DateTime? NGAY_VAO_NOI_TRU { get; set; }
    public DateTime? NGAY_RA { get; set; }
    public DateTime? NGAY_HEN_KL { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? MA_BENH_CHINH { get; set; }
    public string? MA_BENH_KT { get; set; }
    public string? MA_BENH_YHCT { get; set; }
    public string? MA_DOITUONG_KCB { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? MA_TTDV { get; set; }
    public DateTime? NGAY_CT { get; set; }
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

        x01.XML14s.Add(this);
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