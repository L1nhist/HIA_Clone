using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_DU_LIEU_GIAY_CHUNG_SINH", IsNullable = false)]
public partial class CHITIEU_XML09
{
    [XmlElement("DSACH_GIAYCHUNGSINH")]
    public DSACH_GCHS? DANH_SACH { get; set; }
}

public class DSACH_GCHS
{
    [XmlElement("DU_LIEU_GIAY_CHUNG_SINH")]
    public HOSO_XML09[]? CHI_TIETs { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("DU_LIEU_GIAY_CHUNG_SINH", IsNullable = false)]
public partial class HOSO_XML09 : IXMLFile<XML09>
{
    public string? MA_LK { get; set; }
    public string? MA_BHXH_NND { get; set; }
    public string? MA_THE_NND { get; set; }
    public string? HO_TEN_NND { get; set; }
    public string? NGAYSINH_NND { get; set; }
    public string? MA_DANTOC_NND { get; set; }
    public string? SO_CCCD_NND { get; set; }
    public string? NGAYCAP_CCCD_NND { get; set; }
    public string? NOICAP_CCCD_NND { get; set; }
    public string? NOI_CU_TRU_NND { get; set; }
    public string? MA_QUOCTICH { get; set; }
    public string? MATINH_CU_TRU { get; set; }
    public string? MAHUYEN_CU_TRU { get; set; }
    public string? MAXA_CU_TRU { get; set; }
    public string? HO_TEN_CHA { get; set; }
    public string? MA_THE_TAM { get; set; }
    public string? HO_TEN_CON { get; set; }
    public string? GIOI_TINH_CON { get; set; }
    public string? SO_CON { get; set; }
    public string? LAN_SINH { get; set; }
    public string? SO_CON_SONG { get; set; }
    public string? CAN_NANG_CON { get; set; }
    public string? NGAY_SINH_CON { get; set; }
    public string? NOI_SINH_CON { get; set; }
    public string? TINH_TRANG_CON { get; set; }
    public string? SINHCON_PHAUTHUAT { get; set; }
    public string? SINHCON_DUOI32TUAN { get; set; }
    public string? GHI_CHU { get; set; }
    public string? NGUOI_DO_DE { get; set; }
    public string? NGUOI_GHI_PHIEU { get; set; }
    public string? NGAY_CT { get; set; }
    public string? SO { get; set; }
    public string? QUYEN_SO { get; set; }
    public string? MA_TTDV { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML09()
    { }

    public XML09 Export()
    {
        return new XML09()
        {
            MA_LK = MA_LK,
            MA_BHXH_NND = MA_BHXH_NND,
            MA_THE_NND = MA_THE_NND,
            HO_TEN_NND = HO_TEN_NND,
            NGAYSINH_NND = EvalHelper.ToDateSql(NGAYSINH_NND ?? string.Empty),
            MA_DANTOC_NND = MA_DANTOC_NND,
            SO_CCCD_NND = SO_CCCD_NND,
            NGAYCAP_CCCD_NND = EvalHelper.ToDateSql(NGAYCAP_CCCD_NND ?? string.Empty),
            NOICAP_CCCD_NND = NOICAP_CCCD_NND,
            NOI_CU_TRU_NND = NOI_CU_TRU_NND,
            MA_QUOCTICH = MA_QUOCTICH,
            MATINH_CU_TRU = MATINH_CU_TRU,
            MAHUYEN_CU_TRU = MAHUYEN_CU_TRU,
            MAXA_CU_TRU = MAXA_CU_TRU,
            HO_TEN_CHA = HO_TEN_CHA,
            MA_THE_TAM = MA_THE_TAM,
            HO_TEN_CON = HO_TEN_CON,
            GIOI_TINH_CON = EvalHelper.Cast<byte>(GIOI_TINH_CON ?? string.Empty),
            SO_CON = EvalHelper.Cast<byte>(SO_CON ?? string.Empty),
            LAN_SINH = EvalHelper.Cast<byte>(LAN_SINH ?? string.Empty),
            SO_CON_SONG = EvalHelper.Cast<byte>(SO_CON_SONG ?? string.Empty),
            CAN_NANG_CON = EvalHelper.Cast<int>(CAN_NANG_CON ?? string.Empty),
            NGAY_SINH_CON = EvalHelper.ToDateSql(NGAY_SINH_CON ?? string.Empty),
            NOI_SINH_CON = NOI_SINH_CON,
            TINH_TRANG_CON = TINH_TRANG_CON,
            SINHCON_PHAUTHUAT = EvalHelper.Cast<byte>(SINHCON_PHAUTHUAT ?? string.Empty),
            SINHCON_DUOI32TUAN = EvalHelper.Cast<byte>(SINHCON_DUOI32TUAN ?? string.Empty),
            GHI_CHU = GHI_CHU,
            NGUOI_DO_DE = NGUOI_DO_DE,
            NGUOI_GHI_PHIEU = NGUOI_GHI_PHIEU,
            NGAY_CT = EvalHelper.ToDateSql(NGAY_CT ?? string.Empty),
            SO = SO,
            QUYEN_SO = QUYEN_SO,
            MA_TTDV = MA_TTDV,
            DU_PHONG = DU_PHONG,
        };
    }
}
