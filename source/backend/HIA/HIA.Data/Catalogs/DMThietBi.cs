namespace HIA.Data.Catalogs;

public partial class DMThietBi : ICatalog
{
    long ICatalog.ID
    {
        get => TBID;
        set => TBID = (int)value;
    }

    string? ICatalog.MA_DM
    {
        get => MA_MAY;
        set => MA_MAY = value;
    }

    string ICatalog.MA_CSKCB
    {
        get => MA_CSKCB ?? string.Empty;
        set => MA_CSKCB = value;
    }

    public int TBID { get; set; }
    public string? MA_CSKCB { get; set; }
    public string? MA_MAY { get; set; }
    public string? KY_HIEU { get; set; }
    public string? TEN_THIET_BI { get; set; }
    public string? CONGTY_SX { get; set; }
    public string? NUOC_SX { get; set; }
    public string? NAM_SX { get; set; }
    public string? NAM_SD { get; set; }
    public string? SO_LUU_HANH { get; set; }
    public string? HD_TU { get; set; }
    public string? HD_DEN { get; set; }
    public string? TU_NGAY { get; set; }
    public string? DEN_NGAY { get; set; }
}