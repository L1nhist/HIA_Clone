namespace HIA.Data.Catalogs;

public partial class DMKhoaBV : ICatalog
{
    long ICatalog.ID
    {
        get => KBVID;
        set => KBVID = (int)value;
    }

    string? ICatalog.MA_DM
    {
        get => MA_KHOA;
        set => MA_KHOA = value;
    }

    public int KBVID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_KHOA { get; set; }
    public string? TEN_KHOA { get; set; }
    public string? TU_NGAY { get; set; }
    public string? DEN_NGAY { get; set; }
}