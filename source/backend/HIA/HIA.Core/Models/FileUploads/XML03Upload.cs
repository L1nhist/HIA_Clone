
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;
namespace HIA.Core.Models.FileUploads;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHITIEU_CHITIET_DVKT_VTYT", IsNullable = false)]
public class CHITIEU_XML03
{
    [XmlElement("DSACH_CHI_TIET_DVKT")]
    public DSACH_DVKT? DANH_SACH { get; set; }
}

public class DSACH_DVKT
{
    [XmlElement("CHI_TIET_DVKT")]
    public HOSO_XML03[]? CHI_TIETs { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIET_DVKT", IsNullable = false)]
public class HOSO_XML03 : IXMLFile<XML03>
{
    public string? MA_LK { get; set; }
    public string? STT { get; set; }
    public string? MA_DICH_VU { get; set; }
    public string? MA_PTTT_QT { get; set; }
    public string? MA_VAT_TU { get; set; }
    public string? MA_NHOM { get; set; }
    public string? GOI_VTYT { get; set; }
    public string? TEN_VAT_TU { get; set; }
    public string? TEN_DICH_VU { get; set; }
    public string? MA_XANG_DAU { get; set; }
    public string? DON_VI_TINH { get; set; }
    public string? PHAM_VI { get; set; }
    public string? SO_LUONG { get; set; }
    public string? DON_GIA_BV { get; set; }
    public string? DON_GIA_BH { get; set; }
    public string? TT_THAU { get; set; }
    public string? TYLE_TT_DV { get; set; }
    public string? TYLE_TT_BH { get; set; }
    public string? THANH_TIEN_BV { get; set; }
    public string? THANH_TIEN_BH { get; set; }
    public string? T_TRANTT { get; set; }
    public string? MUC_HUONG { get; set; }
    public string? T_NGUONKHAC_NSNN { get; set; }
    public string? T_NGUONKHAC_VTNN { get; set; }
    public string? T_NGUONKHAC_VTTN { get; set; }
    public string? T_NGUONKHAC_CL { get; set; }
    public string? T_NGUONKHAC { get; set; }
    public string? T_BNTT { get; set; }
    public string? T_BNCCT { get; set; }
    public string? T_BHTT { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_GIUONG { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? NGUOI_THUC_HIEN { get; set; }
    public string? MA_BENH { get; set; }
    public string? MA_BENH_YHCT { get; set; }
    public string? NGAY_YL { get; set; }
    public string? NGAY_TH_YL { get; set; }
    public string? NGAY_KQ { get; set; }
    public string? MA_PTTT { get; set; }
    public string? VET_THUONG_TP { get; set; }
    public string? PP_VO_CAM { get; set; }
    public string? VI_TRI_TH_DVKT { get; set; }
    public string? MA_MAY { get; set; }
    public string? MA_HIEU_SP { get; set; }
    public string? TAI_SU_DUNG { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML03()
    { }

    public XML03 Export()
    {
        return new XML03()
        {
            MA_LK = MA_LK,
            STT = EvalHelper.Cast<int>(STT ?? string.Empty),
            MA_DICH_VU = MA_DICH_VU,
            MA_PTTT_QT = MA_PTTT_QT,
            MA_VAT_TU = MA_VAT_TU,
            MA_NHOM = EvalHelper.Cast<byte>(MA_NHOM ?? string.Empty),
            GOI_VTYT = GOI_VTYT,
            TEN_VAT_TU = TEN_VAT_TU,
            TEN_DICH_VU = TEN_DICH_VU,
            MA_XANG_DAU = MA_XANG_DAU,
            DON_VI_TINH = DON_VI_TINH,
            PHAM_VI = EvalHelper.Cast<byte>(PHAM_VI ?? string.Empty),
            SO_LUONG = EvalHelper.ToDouble(SO_LUONG ?? string.Empty),
            DON_GIA_BV = EvalHelper.ToDouble(DON_GIA_BV ?? string.Empty),
            DON_GIA_BH = EvalHelper.ToDouble(DON_GIA_BH ?? string.Empty),
            TT_THAU = TT_THAU,
            TYLE_TT_DV = EvalHelper.Cast<byte>(TYLE_TT_DV ?? string.Empty),
            TYLE_TT_BH = EvalHelper.Cast<byte>(TYLE_TT_BH ?? string.Empty),
            THANH_TIEN_BV = EvalHelper.ToDouble(THANH_TIEN_BV ?? string.Empty),
            THANH_TIEN_BH = EvalHelper.ToDouble(THANH_TIEN_BH ?? string.Empty),
            T_TRANTT = EvalHelper.ToDouble(T_TRANTT ?? string.Empty),
            MUC_HUONG = EvalHelper.ToDouble(MUC_HUONG ?? string.Empty),
            T_NGUONKHAC_NSNN = EvalHelper.ToDouble(T_NGUONKHAC_NSNN ?? string.Empty),
            T_NGUONKHAC_VTNN = EvalHelper.ToDouble(T_NGUONKHAC_VTNN ?? string.Empty),
            T_NGUONKHAC_VTTN = EvalHelper.ToDouble(T_NGUONKHAC_VTTN ?? string.Empty),
            T_NGUONKHAC_CL = EvalHelper.ToDouble(T_NGUONKHAC_CL ?? string.Empty),
            T_NGUONKHAC = EvalHelper.ToDouble(T_NGUONKHAC ?? string.Empty),
            T_BNTT = EvalHelper.ToDouble(T_BNTT ?? string.Empty),
            T_BNCCT = EvalHelper.ToDouble(T_BNCCT ?? string.Empty),
            T_BHTT = EvalHelper.ToDouble(T_BHTT ?? string.Empty),
            MA_KHOA = MA_KHOA,
            MA_GIUONG = MA_GIUONG,
            MA_BAC_SI = MA_BAC_SI,
            NGUOI_THUC_HIEN = NGUOI_THUC_HIEN,
            MA_BENH = MA_BENH,
            MA_BENH_YHCT = MA_BENH_YHCT,
            NGAY_YL = EvalHelper.ToDateSql(NGAY_YL ?? string.Empty),
            NGAY_TH_YL = EvalHelper.ToDateSql(NGAY_TH_YL ?? string.Empty),
            NGAY_KQ = EvalHelper.ToDateSql(NGAY_KQ ?? string.Empty),
            MA_PTTT = EvalHelper.Cast<byte>(MA_PTTT ?? string.Empty),
            VET_THUONG_TP = EvalHelper.Cast<byte>(VET_THUONG_TP ?? string.Empty),
            PP_VO_CAM = EvalHelper.Cast<byte>(PP_VO_CAM ?? string.Empty),
            VI_TRI_TH_DVKT = EvalHelper.Cast<int>(VI_TRI_TH_DVKT ?? string.Empty),
            MA_MAY = MA_MAY,
            MA_HIEU_SP = MA_HIEU_SP,
            TAI_SU_DUNG = EvalHelper.Cast<byte>(TAI_SU_DUNG ?? string.Empty),
            DU_PHONG = DU_PHONG,
        };
    }
}
