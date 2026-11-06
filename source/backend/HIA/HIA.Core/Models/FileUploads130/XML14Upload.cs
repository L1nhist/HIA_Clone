using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_GIAYHEN_KHAMLAI", IsNullable = false)]
public class HOSO_XML14 : IXMLFile<XML14>
{
    public string? MA_LK { get; set; }
    public string? SO_GIAYHEN_KL { get; set; }
    public string? MA_CSKCB { get; set; }
    public string? HO_TEN { get; set; }
    public string? NGAY_SINH { get; set; }
    public string? GIOI_TINH { get; set; }
    public string? DIA_CHI { get; set; }
    public string? MA_THE_BHYT { get; set; }
    public string? GT_THE_DEN { get; set; }
    public string? NGAY_VAO { get; set; }
    public string? NGAY_VAO_NOI_TRU { get; set; }
    public string? NGAY_RA { get; set; }
    public string? NGAY_HEN_KL { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? MA_BENH_CHINH { get; set; }
    public string? MA_BENH_KT { get; set; }
    public string? MA_BENH_YHCT { get; set; }
    public string? MA_DOITUONG_KCB { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? MA_TTDV { get; set; }
    public string? NGAY_CT { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML14()
    { }

    public XML14 Export()
    {
        return new XML14()
        {
            MA_LK = MA_LK,
            STT = 1,
            SO_GIAYHEN_KL = SO_GIAYHEN_KL,
            MA_CSKCB = MA_CSKCB ?? string.Empty,
            HO_TEN = HO_TEN,
            NGAY_SINH = EvalHelper.ToDateSql(NGAY_SINH ?? string.Empty),
            GIOI_TINH = EvalHelper.Cast<byte>(GIOI_TINH ?? string.Empty),
            DIA_CHI = DIA_CHI,
            MA_THE_BHYT = MA_THE_BHYT,
            GT_THE_DEN = GT_THE_DEN,
            NGAY_VAO = EvalHelper.ToDateSql(NGAY_VAO ?? string.Empty),
            NGAY_VAO_NOI_TRU = EvalHelper.ToDateSql(NGAY_VAO_NOI_TRU ?? string.Empty),
            NGAY_RA = EvalHelper.ToDateSql(NGAY_RA ?? string.Empty),
            NGAY_HEN_KL = EvalHelper.ToDateSql(NGAY_HEN_KL ?? string.Empty),
            CHAN_DOAN_RV = CHAN_DOAN_RV,
            MA_BENH_CHINH = MA_BENH_CHINH,
            MA_BENH_KT = MA_BENH_KT,
            MA_BENH_YHCT = MA_BENH_YHCT,
            MA_DOITUONG_KCB = MA_DOITUONG_KCB,
            MA_BAC_SI = MA_BAC_SI,
            MA_TTDV = MA_TTDV,
            NGAY_CT = EvalHelper.ToDateSql(NGAY_CT ?? string.Empty),
            DU_PHONG = DU_PHONG,

        };
    }
}
