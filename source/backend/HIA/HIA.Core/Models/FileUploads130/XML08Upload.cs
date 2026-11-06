using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_DU_LIEU_TOM_TAT_HO_SO_BENH_AN", IsNullable = false)]
public partial class HOSO_XML08 : IXMLFile<XML08>
{
    public string? MA_LK { get; set; }
    public string? MA_LOAI_KCB { get; set; }
    public string? HO_TEN_CHA { get; set; }
    public string? HO_TEN_ME { get; set; }
    public string? NGUOI_GIAM_HO { get; set; }
    public string? DON_VI { get; set; }
    public string? NGAY_VAO { get; set; }
    public string? NGAY_RA { get; set; }
    public string? CHAN_DOAN_VAO { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? QT_BENHLY { get; set; }
    public string? TOMTAT_KQ { get; set; }
    public string? PP_DIEUTRI { get; set; }
    public string? NGAY_SINHCON { get; set; }
    public string? NGAY_CONCHET { get; set; }
    public string? SO_CONCHET { get; set; }
    public string? KET_QUA_DTRI { get; set; }
    public string? GHI_CHU { get; set; }
    public string? MA_TTDV { get; set; }
    public string? NGAY_CT { get; set; }
    public string? MA_THE_TAM { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML08()
    { }

    public XML08 Export()
    {
        return new XML08()
        {
            MA_LK = MA_LK,
            STT = 1,
            MA_LOAI_KCB = MA_LOAI_KCB,
            HO_TEN_CHA = HO_TEN_CHA,
            HO_TEN_ME = HO_TEN_ME,
            NGUOI_GIAM_HO = NGUOI_GIAM_HO,
            DON_VI = DON_VI,
            NGAY_VAO = EvalHelper.ToDateSql(NGAY_VAO ?? string.Empty),
            NGAY_RA = EvalHelper.ToDateSql(NGAY_RA ?? string.Empty),
            CHAN_DOAN_VAO = CHAN_DOAN_VAO,
            CHAN_DOAN_RV = CHAN_DOAN_RV,
            QT_BENHLY = QT_BENHLY,
            TOMTAT_KQ = TOMTAT_KQ,
            PP_DIEUTRI = PP_DIEUTRI,
            NGAY_SINHCON = EvalHelper.ToDateSql(NGAY_SINHCON ?? string.Empty),
            NGAY_CONCHET = EvalHelper.ToDateSql(NGAY_CONCHET ?? string.Empty),
            SO_CONCHET = EvalHelper.Cast<byte>(SO_CONCHET ?? string.Empty),
            KET_QUA_DTRI = EvalHelper.Cast<byte>(KET_QUA_DTRI ?? string.Empty),
            GHI_CHU = GHI_CHU,
            MA_TTDV = MA_TTDV,
            NGAY_CT = EvalHelper.ToDateSql(NGAY_CT ?? string.Empty),
            MA_THE_TAM = MA_THE_TAM,
            DU_PHONG = DU_PHONG,
        };
    }
}
