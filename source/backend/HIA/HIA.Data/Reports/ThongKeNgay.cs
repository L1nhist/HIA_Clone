namespace HIA.Data.Reports;

public partial class ThongKeNgay
{
    public int TKID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public byte? TheLoai { get; set; }
    public double? KhamBenh { get; set; }
    public double? NoiTru { get; set; }
    public double? NgoaiTru { get; set; }
    public double? TongHop { get; set; }
    public int? TrongNgay { get; set; }
}
