namespace HIA.Data.Records;

public partial class XML03 : IRecord<XML01>
{
    public long XML03ID { get; set; }
    public long XML01ID { get; set; }
    public string MA_CSKCB { get; set; } = string.Empty;
    public string? MA_LK { get; set; }
    public int? STT { get; set; }

    public string? MA_DICH_VU { get; set; }
    public string? MA_PTTT_QT { get; set; }
    public string? MA_VAT_TU { get; set; }
    public byte? MA_NHOM { get; set; }
    public string? GOI_VTYT { get; set; }
    public string? TEN_VAT_TU { get; set; }
    public string? TEN_DICH_VU { get; set; }
    public string? MA_XANG_DAU { get; set; }
    public string? DON_VI_TINH { get; set; }
    public byte? PHAM_VI { get; set; }
    public double? SO_LUONG { get; set; }
    public double? DON_GIA_BV { get; set; }
    public double? DON_GIA_BH { get; set; }
    public string? TT_THAU { get; set; }
    public byte? TYLE_TT_DV { get; set; }
    public byte? TYLE_TT_BH { get; set; }
    public double? THANH_TIEN_BV { get; set; }
    public double? THANH_TIEN_BH { get; set; }
    public double? T_TRANTT { get; set; }
    public double? MUC_HUONG { get; set; }
    public double? T_NGUONKHAC_NSNN { get; set; }
    public double? T_NGUONKHAC_VTNN { get; set; }
    public double? T_NGUONKHAC_VTTN { get; set; }
    public double? T_NGUONKHAC_CL { get; set; }
    public double? T_NGUONKHAC { get; set; }
    public double? T_BNTT { get; set; }
    public double? T_BNCCT { get; set; }
    public double? T_BHTT { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_GIUONG { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? NGUOI_THUC_HIEN { get; set; }
    public string? MA_BENH { get; set; }
    public string? MA_BENH_YHCT { get; set; }
    public DateTime? NGAY_YL { get; set; }
    public DateTime? NGAY_TH_YL { get; set; }
    public DateTime? NGAY_KQ { get; set; }
    public byte? MA_PTTT { get; set; }
    public byte? VET_THUONG_TP { get; set; }
    public byte? PP_VO_CAM { get; set; }
    public int? VI_TRI_TH_DVKT { get; set; }
    public string? MA_MAY { get; set; }
    public string? MA_HIEU_SP { get; set; }
    public byte? TAI_SU_DUNG { get; set; }
    public string? DU_PHONG { get; set; }

    public virtual XML01 XML01 { get; set; } = null!;

    public void AppendTo(XML01 x01)
    {
        XML01 = x01;
        XML01ID = x01.XML01ID;
        if (STT == null || x01.XML03s.Any(x => x.STT == STT))
        {
            STT = (x01.XML03s.Max(x => x.STT) ?? 0) + 1;
        }

        x01.XML03s.Add(this);
    }

    public void RaiseError(int MA_LOI, string? errDesc = null)
    {
        XML01.Errors.Add(new XMLError()
        {
            XML01 = XML01,
            XML01ID = XML01ID,
            MA_CSKCB = MA_CSKCB,
            STT = STT,
            MA_LOI = MA_LOI,
            ErrorDesc = errDesc,
            ErrorDate = DateTime.Now,
        });
    }
}