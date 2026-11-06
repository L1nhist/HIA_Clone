namespace HIA.Data.Catalogs;

public partial class DMMaGiuong : ICatalog
{
    long ICatalog.ID
    {
        get => GID;
        set => GID = value;
    }

    string? ICatalog.MA_DM
    {
        get => MA_GIUONG;
        set => MA_GIUONG = value;
    }

    public long GID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_GIUONG { get; set; }
    public string? TEN_GIUONG { get; set; }
    public string? LOAI_GIUONG { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MIEU_TA { get; set; }
    public string? TU_NGAY { get; set; }
    public string? DEN_NGAY { get; set; }
    public bool? HIEU_LUC { get; set; }
}