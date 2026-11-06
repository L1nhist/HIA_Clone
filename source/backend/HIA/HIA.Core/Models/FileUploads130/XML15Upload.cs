using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_DIEUTRI_BENHLAO", IsNullable = false)]
public partial class CHITIEU_XML15
{
    [XmlElement("DSACH_CHITIET_DIEUTRI_BENHLAO")]
    public DSACH_DTLS? DANH_SACH { get; set; }
}

public class DSACH_DTLS
{
    [XmlElement("CHITIET_DIEUTRI_BENHLAO")]
    public HOSO_XML15[]? CHI_TIETs { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHITIET_DIEUTRI_BENHLAO", IsNullable = false)]
public class HOSO_XML15 : IXMLFile<XML15>
{
    public string? MA_LK { get; set; }
    public string? STT { get; set; }
    public string? MA_BN { get; set; }
    public string? HO_TEN { get; set; }
    public string? SO_CCCD { get; set; }
    public string? PHANLOAI_LAO_VITRI { get; set; }
    public string? PHANLOAI_LAO_TS { get; set; }
    public string? PHANLOAI_LAO_HIV { get; set; }
    public string? PHANLOAI_LAO_VK { get; set; }
    public string? PHANLOAI_LAO_KT { get; set; }
    public string? LOAI_DTRI_LAO { get; set; }
    public string? NGAYBD_DTRI_LAO { get; set; }
    public string? PHACDO_DTRI_LAO { get; set; }
    public string? NGAYKT_DTRI_LAO { get; set; }
    public string? KET_QUA_DTRI_LAO { get; set; }
    public string? MA_CSKCB { get; set; }
    public string? NGAYKD_HIV { get; set; }
    public string? BDDT_ARV { get; set; }
    public string? NGAY_BAT_DAU_DT_CTX { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML15()
    { }

    public XML15 Export()
    {
        return new XML15()
        {
            MA_LK = MA_LK,
            STT = EvalHelper.Cast<int>(STT ?? string.Empty),
            MA_BN = MA_BN,
            HO_TEN = HO_TEN,
            SO_CCCD = SO_CCCD,
            PHANLOAI_LAO_VITRI = EvalHelper.Cast<byte>(PHANLOAI_LAO_VITRI ?? string.Empty),
            PHANLOAI_LAO_TS = EvalHelper.Cast<byte>(PHANLOAI_LAO_TS ?? string.Empty),
            PHANLOAI_LAO_HIV = EvalHelper.Cast<byte>(PHANLOAI_LAO_HIV ?? string.Empty),
            PHANLOAI_LAO_VK = EvalHelper.Cast<byte>(PHANLOAI_LAO_VK ?? string.Empty),
            PHANLOAI_LAO_KT = EvalHelper.Cast<byte>(PHANLOAI_LAO_KT ?? string.Empty),
            LOAI_DTRI_LAO = EvalHelper.Cast<byte>(LOAI_DTRI_LAO ?? string.Empty),
            NGAYBD_DTRI_LAO = EvalHelper.ToDateSql(NGAYBD_DTRI_LAO ?? string.Empty),
            PHACDO_DTRI_LAO = EvalHelper.Cast<byte>(PHACDO_DTRI_LAO ?? string.Empty),
            NGAYKT_DTRI_LAO = EvalHelper.ToDateSql(NGAYKT_DTRI_LAO ?? string.Empty),
            KET_QUA_DTRI_LAO = EvalHelper.Cast<byte>(KET_QUA_DTRI_LAO ?? string.Empty),
            MA_CSKCB = MA_CSKCB ?? string.Empty,
            NGAYKD_HIV = EvalHelper.ToDateSql(NGAYKD_HIV ?? string.Empty),
            BDDT_ARV = EvalHelper.ToDateSql(BDDT_ARV ?? string.Empty),
            NGAY_BAT_DAU_DT_CTX = EvalHelper.ToDateSql(NGAY_BAT_DAU_DT_CTX ?? string.Empty),
            DU_PHONG = DU_PHONG,

        };
    }
}
