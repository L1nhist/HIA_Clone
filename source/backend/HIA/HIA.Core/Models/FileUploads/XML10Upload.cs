
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;
namespace HIA.Core.Models.FileUploads;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_DU_LIEU_GIAY_NGHI_DUONG_THAI", IsNullable = false)]
public class HOSO_XML10 : IXMLFile<XML10>
{
    public string? MA_LK { get; set; }
    public string? SO_SERI { get; set; }
    public string? SO_CT { get; set; }
    public string? SO_NGAY { get; set; }
    public string? DON_VI { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? TU_NGAY { get; set; }
    public string? DEN_NGAY { get; set; }
    public string? MA_TTDV { get; set; }
    public string? TEN_BS { get; set; }
    public string? MA_BS { get; set; }
    public string? NGAY_CT { get; set; }

    public HOSO_XML10()
    { }

    public XML10 Export()
    {
        return new XML10()
        {
            MA_LK = MA_LK,
            STT = 1,
            SO_SERI = SO_SERI,
            SO_CT = SO_CT,
            SO_NGAY = EvalHelper.Cast<int>(SO_NGAY ?? string.Empty),
            DON_VI = DON_VI,
            CHAN_DOAN_RV = CHAN_DOAN_RV,
            TU_NGAY = EvalHelper.ToDateSql(TU_NGAY ?? string.Empty),
            DEN_NGAY = EvalHelper.ToDateSql(DEN_NGAY ?? string.Empty),
            MA_TTDV = MA_TTDV,
            TEN_BS = TEN_BS,
            MA_BS = MA_BS,
            NGAY_CT = EvalHelper.ToDateSql(NGAY_CT ?? string.Empty),
        };
    }
}
