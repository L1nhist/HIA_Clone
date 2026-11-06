namespace HIA.Data.Rules;

public partial class ChiDinhDongThoi
{
    public int CDDTID { get; set; }
    public string? MA_DICH_VU { get; set; }
    public string? DICH_VU_KHAC { get; set; }
    public string? VAT_TU_BHYT { get; set; }
    public string? VAT_TU_KO_BH { get; set; }
    public bool? CHI_DINH { get; set; }
    public bool? CUNG_HO_SO { get; set; }
    public bool? CUNG_THOI_DIEM { get; set; }
    public bool? CUNG_NGAY_YL { get; set; }
    public int? MA_LOI { get; set; }
    public string? MA_CSKCB { get; set; }
    public bool? HIEU_LUC { get; set; }
}
