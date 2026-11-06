namespace HIA.Data.Records;

public partial class XMLError
{
    public long XMLEID { get; set; }
    public long XML01ID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public int? STT { get; set; }
    public int? MA_LOI { get; set; }
    public string? ErrorDesc { get; set; }
    public DateTime? ErrorDate { get; set; } = DateTime.Now;

    public virtual XML01 XML01 { get; set; } = null!;
}