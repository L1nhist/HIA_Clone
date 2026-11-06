using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_DU_LIEU_GIAY_RA_VIEN", IsNullable = false)]
public partial class HOSO_XML07 : IXMLFile<XML07>
{
    public string? MA_LK { get; set; }
    public string? SO_LUU_TRU { get; set; }
    public string? MA_YTE { get; set; }
    public string? MA_KHOA_RV { get; set; }
    public string? NGAY_VAO { get; set; }
    public string? NGAY_RA { get; set; }
    public string? MA_DINH_CHI_THAI { get; set; }
    public string? NGUYENNHAN_DINHCHI { get; set; }
    public string? THOIGIAN_DINHCHI { get; set; }
    public string? TUOI_THAI { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? PP_DIEUTRI { get; set; }
    public string? GHI_CHU { get; set; }
    public string? MA_TTDV { get; set; }
    public string? MA_BS { get; set; }
    public string? TEN_BS { get; set; }
    public string? NGAY_CT { get; set; }
    public string? MA_CHA { get; set; }
    public string? MA_ME { get; set; }
    public string? MA_THE_TAM { get; set; }
    public string? HO_TEN_CHA { get; set; }
    public string? HO_TEN_ME { get; set; }
    public string? SO_NGAY_NGHI { get; set; }
    public string? NGOAITRU_TUNGAY { get; set; }
    public string? NGOAITRU_DENNGAY { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML07()
    { }

    public XML07 Export()
    {
        return new XML07()
        {
            MA_LK = MA_LK,
            STT = 1,
            SO_LUU_TRU = SO_LUU_TRU,
            MA_YTE = MA_YTE,
            MA_KHOA_RV = MA_KHOA_RV,
            NGAY_VAO = EvalHelper.ToDateSql(NGAY_VAO ?? string.Empty),
            NGAY_RA = EvalHelper.ToDateSql(NGAY_RA ?? string.Empty),
            MA_DINH_CHI_THAI = EvalHelper.Cast<byte>(MA_DINH_CHI_THAI ?? string.Empty),
            NGUYENNHAN_DINHCHI = NGUYENNHAN_DINHCHI,
            THOIGIAN_DINHCHI = EvalHelper.ToDateSql(THOIGIAN_DINHCHI ?? string.Empty),
            TUOI_THAI = EvalHelper.Cast<byte>(TUOI_THAI ?? string.Empty),
            CHAN_DOAN_RV = CHAN_DOAN_RV,
            PP_DIEUTRI = PP_DIEUTRI,
            GHI_CHU = GHI_CHU,
            MA_TTDV = MA_TTDV,
            MA_BS = MA_BS,
            TEN_BS = TEN_BS,
            NGAY_CT = EvalHelper.ToDateSql(NGAY_CT ?? string.Empty),
            MA_CHA = MA_CHA,
            MA_ME = MA_ME,
            MA_THE_TAM = MA_THE_TAM,
            HO_TEN_CHA = HO_TEN_CHA,
            HO_TEN_ME = HO_TEN_ME,
            SO_NGAY_NGHI = EvalHelper.Cast<byte>(SO_NGAY_NGHI ?? string.Empty),
            NGOAITRU_TUNGAY = EvalHelper.ToDateSql(NGOAITRU_TUNGAY ?? string.Empty),
            NGOAITRU_DENNGAY = EvalHelper.ToDateSql(NGOAITRU_DENNGAY ?? string.Empty),
            DU_PHONG = DU_PHONG,
        };
    }
}
