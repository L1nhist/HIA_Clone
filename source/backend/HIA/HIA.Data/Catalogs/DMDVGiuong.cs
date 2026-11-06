namespace HIA.Data.Catalogs;

public partial class DMDVGiuong : ICatalog
{
    long ICatalog.ID
    {
        get => GID;
        set => GID = value;
    }

    string? ICatalog.MA_DM
    {
        get => MA_DICH_VU;
        set => MA_DICH_VU = value;
    }
    public long GID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_DICH_VU { get; set; }
    public string? TEN_DICH_VU { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_TT37 { get; set; }
    public float? DON_GIA { get; set; }
    public string? TU_NGAY { get; set; }
    public string? DEN_NGAY { get; set; }
    public bool? HIEU_LUC { get; set; }
}