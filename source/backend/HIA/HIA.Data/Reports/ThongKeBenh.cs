namespace HIA.Data.Reports;

public partial class ThongKeBenh
{
    public int TKID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MaBenh { get; set; }
    public string? TenBenh { get; set; }
    public byte? XepHang { get; set; }
    public int? SoLuong { get; set; }
    public DateTime? LastUpdate { get; set; }
}
