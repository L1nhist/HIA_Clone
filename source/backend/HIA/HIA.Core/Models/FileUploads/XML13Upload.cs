
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;
namespace HIA.Core.Models.FileUploads;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("CHI_TIEU_GIAYCHUYENTUYEN", IsNullable = false)]
public class HOSO_XML13 : IXMLFile<XML13>
{
    public string? MA_LK { get; set; }
    public string? SO_HOSO { get; set; }
    public string? SO_CHUYENTUYEN { get; set; }
    public string? GIAY_CHUYEN_TUYEN { get; set; }
    public string? MA_CSKCB { get; set; }
    public string? MA_NOI_DI { get; set; }
    //Linhist added 06/02/24 - thay MA_NOI_DI
    public string? MA_CSKCB_DI { get; set; }
    public string? MA_NOI_DEN { get; set; }
    //Linhist added 06/02/24 - thay MA_NOI_DEN
    public string? MA_CSKCB_DEN { get; set; }
    public string? HO_TEN { get; set; }
    public string? NGAY_SINH { get; set; }
    public byte? GIOI_TINH { get; set; }
    public string? MA_QUOCTICH { get; set; }
    public string? MA_DANTOC { get; set; }
    public string? MA_NGHE_NGHIEP { get; set; }
    public string? DIA_CHI { get; set; }
    public string? MA_THE_BHYT { get; set; }
    public string? GT_THE_DEN { get; set; }
    public string? NGAY_VAO { get; set; }
    public string? NGAY_VAO_NOI_TRU { get; set; }
    public string? NGAY_RA { get; set; }
    public string? DAU_HIEU_LS { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? QT_BENHLY { get; set; }
    public string? TOMTAT_KQ { get; set; }
    public string? PP_DIEUTRI { get; set; }
    public string? MA_BENH_CHINH { get; set; }
    public string? MA_BENH_KT { get; set; }
    public string? MA_BENH_YHCT { get; set; }
    public string? TEN_DICH_VU { get; set; }
    public string? TEN_THUOC { get; set; }
    public string? PP_DIEU_TRI { get; set; }
    //Linhist added 06/02/24 - thay PP_DIEU_TRI
    public string? TINH_TRANG_CT { get; set; }
    public string? MA_LOAI_RV { get; set; }
    public string? MA_LYDO_CT { get; set; }
    public string? HUONG_DIEU_TRI { get; set; }
    public string? PHUONGTIEN_VC { get; set; }
    public string? HOTEN_NGUOI_HT { get; set; }
    public string? CHUCDANH_NGUOI_HT { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? MA_TTDV { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML13()
    { }

    public XML13 Export()
    {
        return new XML13()
        {
            MA_LK = MA_LK,
            STT = 1,
            SO_HOSO = SO_HOSO,
            SO_CHUYENTUYEN = SO_CHUYENTUYEN,
            GIAY_CHUYEN_TUYEN = GIAY_CHUYEN_TUYEN,
            MA_CSKCB = MA_CSKCB ?? string.Empty,
            MA_NOI_DI = MA_NOI_DI,
            //Linhist added 06/02/24 - thay MA_NOI_DI
            MA_CSKCB_DI = MA_CSKCB_DI,
            MA_NOI_DEN = MA_NOI_DEN,
            //Linhist added 06/02/24 - thay MA_NOI_DEN
            MA_CSKCB_DEN = MA_CSKCB_DEN,
            HO_TEN = HO_TEN,
            NGAY_SINH = EvalHelper.ToDateSql(NGAY_SINH ?? string.Empty),
            GIOI_TINH = GIOI_TINH,
            MA_QUOCTICH = MA_QUOCTICH,
            MA_DANTOC = MA_DANTOC,
            MA_NGHE_NGHIEP = MA_NGHE_NGHIEP,
            DIA_CHI = DIA_CHI,
            MA_THE_BHYT = MA_THE_BHYT,
            GT_THE_DEN = GT_THE_DEN,
            NGAY_VAO = EvalHelper.ToDateSql(NGAY_VAO ?? string.Empty),
            NGAY_VAO_NOI_TRU = EvalHelper.ToDateSql(NGAY_VAO_NOI_TRU ?? string.Empty),
            NGAY_RA = EvalHelper.ToDateSql(NGAY_RA ?? string.Empty),
            DAU_HIEU_LS = DAU_HIEU_LS,
            CHAN_DOAN_RV = CHAN_DOAN_RV,
            QT_BENHLY = QT_BENHLY,
            TOMTAT_KQ = TOMTAT_KQ,
            PP_DIEUTRI = PP_DIEUTRI,
            MA_BENH_CHINH = MA_BENH_CHINH,
            MA_BENH_KT = MA_BENH_KT,
            MA_BENH_YHCT = MA_BENH_YHCT,
            TEN_DICH_VU = TEN_DICH_VU,
            TEN_THUOC = TEN_THUOC,
            PP_DIEU_TRI = PP_DIEU_TRI,
            //Linhist added 06/02/24 - thay PP_DIEU_TRI
            TINH_TRANG_CT = TINH_TRANG_CT,
            MA_LOAI_RV = EvalHelper.Cast<byte>(MA_LOAI_RV ?? string.Empty),
            MA_LYDO_CT = EvalHelper.Cast<byte>(MA_LYDO_CT ?? string.Empty),
            HUONG_DIEU_TRI = HUONG_DIEU_TRI,
            PHUONGTIEN_VC = PHUONGTIEN_VC,
            HOTEN_NGUOI_HT = HOTEN_NGUOI_HT,
            CHUCDANH_NGUOI_HT = CHUCDANH_NGUOI_HT,
            MA_BAC_SI = MA_BAC_SI,
            MA_TTDV = MA_TTDV,
            DU_PHONG = DU_PHONG,
        };
    }
}
