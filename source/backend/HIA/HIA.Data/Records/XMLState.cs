namespace HIA.Data.Records;

public partial class XMLState : IRecord<XML01>, IRecord
{
    [JsonIgnore]
    int? IRecord.STT => 1;

    public long XML01ID { get; set; }

    public string MA_CSKCB { get; set; } = string.Empty;

    public string? MA_LK { get; set; }

    public string? DS_LOI { get; set; }

    public int? TONG_SO_LOI { get; set; }

    public int? CANH_BAO { get; set; }

    public int? Counter { get; set; }

    public string? Checker { get; set; }

    public DateTime? CheckDate { get; set; }

    public DateTime? VerifyDate { get; set; }

    public bool? IsUpload { get; set; }

    public DateTime? CreatedDate { get; set; }

    [JsonIgnore]
    public virtual XML01 XML01 { get; set; } = null!;

    public void AppendTo(XML01 x01)
    {
        XML01 = x01;
        XML01ID = x01.XML01ID;
        CreatedDate = x01.CreateDate;
        x01.State = this;
    }

    public void RaiseError(int MA_LOI, string? errDesc = null)
        => XML01?.RaiseError(MA_LOI, errDesc);
}