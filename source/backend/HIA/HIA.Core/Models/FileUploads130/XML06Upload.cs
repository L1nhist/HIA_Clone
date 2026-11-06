using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_HO_SO_BENH_AN_CHAM_SOC_VA_DIEU_TRI_HIV_AIDS", IsNullable = false)]
public partial class CHITIEU_XML06
{
    [XmlElement("DSACH_HO_SO_BENH_AN_CHAM_SOC_VA_DIEU_TRI_HIV_AIDS")]
    public DSACH_HIVS? DANH_SACH { get; set; }
}

public class DSACH_HIVS
{
    [XmlElement("HO_SO_BENH_AN_CHAM_SOC_VA_DIEU_TRI_HIV_AIDS")]
    public HOSO_XML06[]? CHI_TIETs { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("HO_SO_BENH_AN_CHAM_SOC_VA_DIEU_TRI_HIV_AIDS", IsNullable = false)]
public class HOSO_XML06 : IXMLFile<XML06>
{
    public string? MA_LK { get; set; }
    public string? MA_THE_BHYT { get; set; }
    public string? SO_CCCD { get; set; }
    public string? NGAY_SINH { get; set; }
    public string? GIOI_TINH { get; set; }
    public string? DIA_CHI { get; set; }
    public string? MATINH_CU_TRU { get; set; }
    public string? MAHUYEN_CU_TRU { get; set; }
    public string? MAXA_CU_TRU { get; set; }
    public string? NGAYKD_HIV { get; set; }
    public string? NOI_LAY_MAU_XN { get; set; }
    public string? NOI_XN_KD { get; set; }
    public string? NOI_BDDT_ARV { get; set; }
    public string? BDDT_ARV { get; set; }
    public string? MA_PHAC_DO_DIEU_TRI_BD { get; set; }
    public string? MA_BAC_PHAC_DO_BD { get; set; }
    public string? MA_LYDO_DTRI { get; set; }
    public string? LOAI_DTRI_LAO { get; set; }
    public string? SANG_LOC_LAO { get; set; }
    public string? PHACDO_DTRI_LAO { get; set; }
    public string? NGAYBD_DTRI_LAO { get; set; }
    public string? NGAYKT_DTRI_LAO { get; set; }
    public string? KQ_DTRI_LAO { get; set; }
    public string? MA_LYDO_XNTL_VR { get; set; }
    public string? NGAY_XN_TLVR { get; set; }
    public string? KQ_XNTL_VR { get; set; }
    public string? NGAY_KQ_XN_TLVR { get; set; }
    public string? MA_LOAI_BN { get; set; }
    public string? GIAI_DOAN_LAM_SANG { get; set; }
    public string? NHOM_DOI_TUONG { get; set; }
    public string? MA_TINH_TRANG_DK { get; set; }
    public string? LAN_XN_PCR { get; set; }
    public string? NGAY_XN_PCR { get; set; }
    public string? NGAY_KQ_XN_PCR { get; set; }
    public string? MA_KQ_XN_PCR { get; set; }
    public string? NGAY_NHAN_TT_MANG_THAI { get; set; }
    public string? NGAY_BAT_DAU_DT_CTX { get; set; }
    public string? MA_XU_TRI { get; set; }
    public string? NGAY_BAT_DAU_XU_TRI { get; set; }
    public string? NGAY_KET_THUC_XU_TRI { get; set; }
    public string? MA_PHAC_DO_DIEU_TRI { get; set; }
    public string? MA_BAC_PHAC_DO { get; set; }
    public string? SO_NGAY_CAP_THUOC_ARV { get; set; }
    public string? NGAY_CHUYEN_PHAC_DO { get; set; }
    public string? LY_DO_CHUYEN_PHAC_DO { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML06()
    { }

    public XML06 Export()
    {
        return new XML06()
        {
            MA_LK = MA_LK,
            MA_THE_BHYT = MA_THE_BHYT,
            SO_CCCD = SO_CCCD,
            NGAY_SINH = EvalHelper.ToDateSql(NGAY_SINH ?? string.Empty),
            GIOI_TINH = EvalHelper.Cast<byte>(GIOI_TINH ?? string.Empty),
            DIA_CHI = DIA_CHI,
            MATINH_CU_TRU = MATINH_CU_TRU,
            MAHUYEN_CU_TRU = MAHUYEN_CU_TRU,
            MAXA_CU_TRU = MAXA_CU_TRU,
            NGAYKD_HIV = EvalHelper.ToDateSql(NGAYKD_HIV ?? string.Empty),
            BDDT_ARV = EvalHelper.ToDateSql(BDDT_ARV ?? string.Empty),
            NOI_LAY_MAU_XN = NOI_LAY_MAU_XN,
            NOI_XN_KD = NOI_XN_KD,
            NOI_BDDT_ARV = NOI_BDDT_ARV,
            MA_PHAC_DO_DIEU_TRI_BD = MA_PHAC_DO_DIEU_TRI_BD,
            MA_BAC_PHAC_DO_BD = EvalHelper.Cast<byte>(MA_BAC_PHAC_DO_BD ?? string.Empty),
            MA_LYDO_DTRI = EvalHelper.Cast<byte>(MA_LYDO_DTRI ?? string.Empty),
            LOAI_DTRI_LAO = EvalHelper.Cast<byte>(LOAI_DTRI_LAO ?? string.Empty),
            SANG_LOC_LAO = EvalHelper.Cast<byte>(SANG_LOC_LAO ?? string.Empty),
            PHACDO_DTRI_LAO = EvalHelper.Cast<byte>(PHACDO_DTRI_LAO ?? string.Empty),
            NGAYBD_DTRI_LAO = EvalHelper.ToDateSql(NGAYBD_DTRI_LAO ?? string.Empty),
            NGAYKT_DTRI_LAO = EvalHelper.ToDateSql(NGAYKT_DTRI_LAO ?? string.Empty),
            KQ_DTRI_LAO = EvalHelper.Cast<byte>(KQ_DTRI_LAO ?? string.Empty),
            MA_LYDO_XNTL_VR = EvalHelper.Cast<byte>(MA_LYDO_XNTL_VR ?? string.Empty),
            NGAY_XN_TLVR = EvalHelper.ToDateSql(NGAY_XN_TLVR ?? string.Empty),
            KQ_XNTL_VR = EvalHelper.Cast<byte>(KQ_XNTL_VR ?? string.Empty),
            NGAY_KQ_XN_TLVR = EvalHelper.ToDateSql(NGAY_KQ_XN_TLVR ?? string.Empty),
            MA_LOAI_BN = EvalHelper.Cast<byte>(MA_LOAI_BN ?? string.Empty),
            GIAI_DOAN_LAM_SANG = EvalHelper.Cast<byte>(GIAI_DOAN_LAM_SANG ?? string.Empty),
            NHOM_DOI_TUONG = EvalHelper.Cast<byte>(NHOM_DOI_TUONG ?? string.Empty),
            MA_TINH_TRANG_DK = MA_TINH_TRANG_DK,
            LAN_XN_PCR = EvalHelper.Cast<byte>(LAN_XN_PCR ?? string.Empty),
            NGAY_XN_PCR = EvalHelper.ToDateSql(NGAY_XN_PCR ?? string.Empty),
            NGAY_KQ_XN_PCR = EvalHelper.ToDateSql(NGAY_KQ_XN_PCR ?? string.Empty),
            MA_KQ_XN_PCR = EvalHelper.Cast<byte>(MA_KQ_XN_PCR ?? string.Empty),
            NGAY_NHAN_TT_MANG_THAI = EvalHelper.ToDateSql(NGAY_NHAN_TT_MANG_THAI ?? string.Empty),
            NGAY_BAT_DAU_DT_CTX = EvalHelper.ToDateSql(NGAY_BAT_DAU_DT_CTX ?? string.Empty),
            MA_XU_TRI = MA_XU_TRI,
            NGAY_BAT_DAU_XU_TRI = EvalHelper.ToDateSql(NGAY_BAT_DAU_XU_TRI ?? string.Empty),
            NGAY_KET_THUC_XU_TRI = EvalHelper.ToDateSql(NGAY_KET_THUC_XU_TRI ?? string.Empty),
            MA_PHAC_DO_DIEU_TRI = MA_PHAC_DO_DIEU_TRI,
            MA_BAC_PHAC_DO = EvalHelper.Cast<byte>(MA_BAC_PHAC_DO ?? string.Empty),
            SO_NGAY_CAP_THUOC_ARV = EvalHelper.Cast<int>(SO_NGAY_CAP_THUOC_ARV ?? string.Empty),
            NGAY_CHUYEN_PHAC_DO = EvalHelper.ToDateSql(NGAY_CHUYEN_PHAC_DO ?? string.Empty),
            LY_DO_CHUYEN_PHAC_DO = EvalHelper.Cast<byte>(LY_DO_CHUYEN_PHAC_DO ?? string.Empty),
            DU_PHONG = DU_PHONG,
        };
    }
}
