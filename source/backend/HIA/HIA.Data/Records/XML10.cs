namespace HIA.Data.Records;

public partial class XML10 : IRecord<XML01>
{
    public long XML10ID { get; set; }
    public long XML01ID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_LK { get; set; }
    public int? STT { get; set; }

    public string? SO_SERI { get; set; }
    public string? SO_CT { get; set; }
    public int? SO_NGAY { get; set; }
    public string? DON_VI { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public DateTime? TU_NGAY { get; set; }
    public DateTime? DEN_NGAY { get; set; }
    public string? MA_TTDV { get; set; }
    public string? TEN_BS { get; set; }
    public string? MA_BS { get; set; }
    public DateTime? NGAY_CT { get; set; }
    public string? DU_PHONG { get; set; }

    public virtual XML01 XML01 { get; set; } = null!;

    public void AppendTo(XML01 x01)
    {
        XML01 = x01;
        XML01ID = x01.XML01ID;
        if (STT == null || x01.XML10s.Any(x => x.STT == STT))
        {
            STT = (x01.XML10s.Max(x => x.STT) ?? 0) + 1;
        }

        x01.XML10s.Add(this);
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