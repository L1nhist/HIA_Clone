namespace HIA.Data.Catalogs;

public partial class DMBacSi : ICatalog
{
    long ICatalog.ID
    {
        get => BSID;
        set => BSID = (int)value;
    }

    string? ICatalog.MA_DM
    {
        get => MA_CCHN;
        set => MA_CCHN = value;
    }

    public int BSID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_BHXH { get; set; }
    public string? MA_LOAI_KCB { get; set; }
    public string? HO_TEN { get; set; }
    public byte? GIOI_TINH { get; set; }
    public string? SO_CCCD { get; set; }
    public string? MA_KHOA { get; set; }
    public string? TEN_KHOA { get; set; }
    public string? CHUC_DANH { get; set; }
    public string? CHUC_DANH_NN { get; set; }
    public string? MA_VI_TRI { get; set; }
    public string? VI_TRI { get; set; }
    public string? MA_CCHN { get; set; }
    public string? NGAYCAP_CCHN { get; set; }
    public string? NOICAP_CCHN { get; set; }
    public string? PHAMVI_CM { get; set; }
    public string? PHAMVI_CMBS { get; set; }
    public string? DVKT_KHAC { get; set; }
    public string? VB_PHANCONG { get; set; }
    public string? TG_DANGKY { get; set; }
    public string? TG_NGAY { get; set; }
    public string? TG_TUAN { get; set; }
    public string? CSKCB_KHAC { get; set; }
    public string? CSKCB_CGKT { get; set; }
    public string? QD_CGKT { get; set; }
    public string? TU_NGAY { get; set; }
    public string? DEN_NGAY { get; set; }
}