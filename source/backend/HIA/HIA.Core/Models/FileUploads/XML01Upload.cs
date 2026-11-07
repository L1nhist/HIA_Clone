
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
using System.Xml.Serialization;
namespace HIA.Core.Models.FileUploads;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot("TONG_HOP", IsNullable = false)]
public class HOSO_XML01 : IXMLFile<XML01>
{
    public string? MA_LK { get; set; }
    public string? STT { get; set; }
    public string? MA_BN { get; set; }
    public string? HO_TEN { get; set; }
    public string? SO_CCCD { get; set; }
    public string? NGAY_SINH { get; set; }
    public string? GIOI_TINH { get; set; }
    public string? NHOM_MAU { get; set; }
    public string? MA_QUOCTICH { get; set; }
    public string? MA_DANTOC { get; set; }
    public string? MA_NGHE_NGHIEP { get; set; }
    public string? DIA_CHI { get; set; }
    public string? MATINH_CU_TRU { get; set; }
    public string? MAHUYEN_CU_TRU { get; set; }
    public string? MAXA_CU_TRU { get; set; }
    public string? DIEN_THOAI { get; set; }
    public string? MA_THE_BHYT { get; set; }
    public string? MA_DKBD { get; set; }
    public string? GT_THE_TU { get; set; }
    public string? GT_THE_DEN { get; set; }
    public string? NGAY_MIEN_CCT { get; set; }
    public string? LY_DO_VV { get; set; }
    public string? LY_DO_VNT { get; set; }
    public string? MA_LY_DO_VNT { get; set; }
    public string? CHAN_DOAN_VAO { get; set; }
    public string? CHAN_DOAN_RV { get; set; }
    public string? MA_BENH_CHINH { get; set; }
    public string? MA_BENH_KT { get; set; }
    public string? MA_BENH_YHCT { get; set; }
    public string? MA_PTTT_QT { get; set; }
    public string? MA_DOITUONG_KCB { get; set; }
    public string? MA_NOI_DI { get; set; }
    public string? MA_NOI_DEN { get; set; }
    public string? MA_TAI_NAN { get; set; }
    public string? NGAY_VAO { get; set; }
    public string? NGAY_VAO_NOI_TRU { get; set; }
    public string? NGAY_RA { get; set; }
    public string? GIAY_CHUYEN_TUYEN { get; set; }
    public string? SO_NGAY_DTRI { get; set; }
    public string? PP_DIEU_TRI { get; set; }
    public string? KET_QUA_DTRI { get; set; }
    public string? MA_LOAI_RV { get; set; }
    public string? GHI_CHU { get; set; }
    public string? NGAY_TTOAN { get; set; }
    public string? T_THUOC { get; set; }
    public string? T_VTYT { get; set; }
    public string? T_TONGCHI_BV { get; set; }
    public string? T_TONGCHI_BH { get; set; }
    public string? T_BNTT { get; set; }
    public string? T_BNCCT { get; set; }
    public string? T_BHTT { get; set; }
    public string? T_NGUONKHAC { get; set; }
    public string? T_BHTT_GDV { get; set; }
    public string? NAM_QT { get; set; }
    public string? THANG_QT { get; set; }
    public string? MA_LOAI_KCB { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_CSKCB { get; set; }
    public string? MA_KHUVUC { get; set; }
    public string? CAN_NANG { get; set; }
    public string? CAN_NANG_CON { get; set; }
    public string? NAM_NAM_LIEN_TUC { get; set; }
    public string? NGAY_TAI_KHAM { get; set; }
    public string? MA_HSBA { get; set; }
    public string? MA_TTDV { get; set; }
    public string? DU_PHONG { get; set; }

    public HOSO_XML01()
    { }

    public XML01 Export()
    {
        return new XML01()
        {
            MA_LK = MA_LK,
            STT = EvalHelper.Cast<int>(STT ?? string.Empty),
            MA_BN = MA_BN,
            HO_TEN = HO_TEN,
            SO_CCCD = SO_CCCD,
            NGAY_SINH = EvalHelper.ToDateSql(NGAY_SINH ?? string.Empty),
            GIOI_TINH = EvalHelper.Cast<byte>(GIOI_TINH ?? string.Empty),
            NHOM_MAU = NHOM_MAU,
            MA_QUOCTICH = MA_QUOCTICH,
            MA_DANTOC = MA_DANTOC,
            MA_NGHE_NGHIEP = MA_NGHE_NGHIEP,
            DIA_CHI = DIA_CHI,
            MATINH_CU_TRU = MATINH_CU_TRU,
            MAHUYEN_CU_TRU = MAHUYEN_CU_TRU,
            MAXA_CU_TRU = MAXA_CU_TRU,
            DIEN_THOAI = DIEN_THOAI,
            MA_THE_BHYT = MA_THE_BHYT,
            MA_DKBD = MA_DKBD,
            GT_THE_TU = GT_THE_TU,
            GT_THE_DEN = GT_THE_DEN,
            NGAY_MIEN_CCT = EvalHelper.ToDateSql(NGAY_MIEN_CCT ?? string.Empty),
            LY_DO_VV = LY_DO_VV,
            LY_DO_VNT = LY_DO_VNT,
            MA_LY_DO_VNT = MA_LY_DO_VNT,
            CHAN_DOAN_VAO = CHAN_DOAN_VAO,
            CHAN_DOAN_RV = CHAN_DOAN_RV,
            MA_BENH_CHINH = MA_BENH_CHINH,
            MA_BENH_KT = MA_BENH_KT,
            MA_BENH_YHCT = MA_BENH_YHCT,
            MA_PTTT_QT = MA_PTTT_QT,
            MA_DOITUONG_KCB = MA_DOITUONG_KCB,
            MA_NOI_DI = MA_NOI_DI,
            MA_NOI_DEN = MA_NOI_DEN,
            MA_TAI_NAN = EvalHelper.Cast<byte>(MA_TAI_NAN ?? string.Empty),
            NGAY_VAO = EvalHelper.ToDateSql(NGAY_VAO ?? string.Empty),
            NGAY_VAO_NOI_TRU = EvalHelper.ToDateSql(NGAY_VAO_NOI_TRU ?? string.Empty),
            NGAY_RA = EvalHelper.ToDateSql(NGAY_RA ?? string.Empty),
            GIAY_CHUYEN_TUYEN = GIAY_CHUYEN_TUYEN,
            SO_NGAY_DTRI = EvalHelper.Cast<int>(SO_NGAY_DTRI ?? string.Empty),
            PP_DIEU_TRI = PP_DIEU_TRI,
            KET_QUA_DTRI = EvalHelper.Cast<byte>(KET_QUA_DTRI ?? string.Empty),
            MA_LOAI_RV = EvalHelper.Cast<byte>(MA_LOAI_RV ?? string.Empty),
            GHI_CHU = GHI_CHU,
            NGAY_TTOAN = EvalHelper.ToDateSql(NGAY_TTOAN ?? string.Empty),
            T_THUOC = EvalHelper.ToDouble(T_THUOC ?? string.Empty),
            T_VTYT = EvalHelper.ToDouble(T_VTYT ?? string.Empty),
            T_TONGCHI_BV = EvalHelper.ToDouble(T_TONGCHI_BV ?? string.Empty),
            T_TONGCHI_BH = EvalHelper.ToDouble(T_TONGCHI_BH ?? string.Empty),
            T_BNTT = EvalHelper.ToDouble(T_BNTT ?? string.Empty),
            T_BNCCT = EvalHelper.ToDouble(T_BNCCT ?? string.Empty),
            T_BHTT = EvalHelper.ToDouble(T_BHTT ?? string.Empty),
            T_NGUONKHAC = EvalHelper.ToDouble(T_NGUONKHAC ?? string.Empty),
            T_BHTT_GDV = EvalHelper.ToDouble(T_BHTT_GDV ?? string.Empty),
            NAM_QT = EvalHelper.Cast<int>(NAM_QT ?? string.Empty),
            THANG_QT = EvalHelper.Cast<byte>(THANG_QT ?? string.Empty),
            MA_LOAI_KCB = MA_LOAI_KCB,
            MA_KHOA = MA_KHOA,
            MA_CSKCB = MA_CSKCB ?? string.Empty,
            MA_KHUVUC = MA_KHUVUC,
            CAN_NANG = EvalHelper.ToDouble(CAN_NANG ?? string.Empty),
            CAN_NANG_CON = CAN_NANG_CON,
            NAM_NAM_LIEN_TUC = EvalHelper.ToDateSql(NAM_NAM_LIEN_TUC ?? string.Empty),
            NGAY_TAI_KHAM = NGAY_TAI_KHAM,
            MA_HSBA = MA_HSBA,
            MA_TTDV = MA_TTDV,
            DU_PHONG = DU_PHONG,
        };
    }
}
