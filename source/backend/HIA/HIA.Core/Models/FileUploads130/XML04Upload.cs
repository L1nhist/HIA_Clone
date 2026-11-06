using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHITIEU_CHITIET_DICHVUCANLAMSANG", IsNullable = false)]
public partial class CHITIEU_XML04
{
    [XmlElement("DSACH_CHI_TIET_CLS")]
    public DSACH_DVCLS? DANH_SACH { get; set; }
}

public class DSACH_DVCLS
{
    [XmlElement("CHI_TIET_CLS")]
    public HOSO_XML04[]? CHI_TIETs { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIET_CLS", IsNullable = false)]
public class HOSO_XML04 : IXMLFile<XML04>
{
    public string? MA_LK { get; set; }
    public string? STT { get; set; }
    public string? MA_DICH_VU { get; set; }
    public string? MA_CHI_SO { get; set; }
    public string? TEN_CHI_SO { get; set; }
    public string? GIA_TRI { get; set; }
    public string? DON_VI_DO { get; set; }
    public string? MO_TA { get; set; }
    public string? KET_LUAN { get; set; }
    public string? NGAY_KQ { get; set; }
    public string? MA_BS_DOC_KQ { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML04()
    { }

    public XML04 Export()
    {
        return new XML04()
        {
            MA_LK = MA_LK,
            STT = EvalHelper.Cast<int>(STT ?? string.Empty),
            MA_DICH_VU = MA_DICH_VU,
            MA_CHI_SO = MA_CHI_SO,
            TEN_CHI_SO = TEN_CHI_SO,
            GIA_TRI = GIA_TRI,
            DON_VI_DO = DON_VI_DO,
            MO_TA = MO_TA,
            KET_LUAN = KET_LUAN,
            NGAY_KQ = EvalHelper.ToDateSql(NGAY_KQ ?? string.Empty),
            MA_BS_DOC_KQ = MA_BS_DOC_KQ,
            DU_PHONG = DU_PHONG,
        };
    }
}
