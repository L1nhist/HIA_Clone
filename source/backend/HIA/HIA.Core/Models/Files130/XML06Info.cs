using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;

namespace HIA.Core.Models.Files130;

public class XML06Info : IXMLModel<XML06>
{
    public long xml06id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? ma_the_bhyt { get; set; }
    public string? so_cccd { get; set; }
    public string? ngay_sinh { get; set; }
    public byte? gioi_tinh { get; set; }
    public string? dia_chi { get; set; }
    public string? matinh_cu_tru { get; set; }
    public string? mahuyen_cu_tru { get; set; }
    public string? maxa_cu_tru { get; set; }
    public string? ngaykd_hiv { get; set; }
    public string? noi_lay_mau_xn { get; set; }
    public string? noi_xn_kd { get; set; }
    public string? noi_bddt_arv { get; set; }
    public string? bddt_arv { get; set; }
    public string? ma_phac_do_dieu_tri_bd { get; set; }
    public byte? ma_bac_phac_do_bd { get; set; }
    public byte? ma_lydo_dtri { get; set; }
    public byte? loai_dtri_lao { get; set; }
    public byte? sang_loc_lao { get; set; }
    public byte? phacdo_dtri_lao { get; set; }
    public string? ngaybd_dtri_lao { get; set; }
    public string? ngaykt_dtri_lao { get; set; }
    public byte? kq_dtri_lao { get; set; }
    public byte? ma_lydo_xntl_vr { get; set; }
    public string? ngay_xn_tlvr { get; set; }
    public byte? kq_xntl_vr { get; set; }
    public string? ngay_kq_xn_tlvr { get; set; }
    public byte? ma_loai_bn { get; set; }
    public byte? giai_doan_lam_sang { get; set; }
    public byte? nhom_doi_tuong { get; set; }
    public string? ma_tinh_trang_dk { get; set; }
    public byte? lan_xn_pcr { get; set; }
    public string? ngay_xn_pcr { get; set; }
    public string? ngay_kq_xn_pcr { get; set; }
    public byte? ma_kq_xn_pcr { get; set; }
    public string? ngay_nhan_tt_mang_thai { get; set; }
    public string? ngay_bat_dau_dt_ctx { get; set; }
    public string? ma_xu_tri { get; set; }
    public string? ngay_bat_dau_xu_tri { get; set; }
    public string? ngay_ket_thuc_xu_tri { get; set; }
    public string? ma_phac_do_dieu_tri { get; set; }
    public byte? ma_bac_phac_do { get; set; }
    public int? so_ngay_cap_thuoc_arv { get; set; }
    public string? ngay_cchuyen_phac_do { get; set; }
    public byte? ly_do_chuyen_phac_do { get; set; }
    public string? du_phong { get; set; }

    public XML06Info(XML06? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML06 entity)
    {
        xml06id = entity.XML06ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        ma_the_bhyt = entity.MA_THE_BHYT;
        so_cccd = entity.SO_CCCD;
        ngay_sinh = Uti.ToDateView(entity.NGAY_SINH);
        dia_chi = entity.DIA_CHI;
        matinh_cu_tru = entity.MATINH_CU_TRU;
        mahuyen_cu_tru = entity.MAHUYEN_CU_TRU;
        maxa_cu_tru = entity.MAXA_CU_TRU;
        ngaykd_hiv = Uti.ToDateView(entity.NGAYKD_HIV);
        noi_lay_mau_xn = entity.NOI_LAY_MAU_XN;
        noi_xn_kd = entity.NOI_XN_KD;
        noi_bddt_arv = entity.NOI_BDDT_ARV;
        bddt_arv = Uti.ToDateView(entity.BDDT_ARV);
        ma_phac_do_dieu_tri_bd = entity.MA_PHAC_DO_DIEU_TRI_BD;
        ma_bac_phac_do_bd = entity.MA_BAC_PHAC_DO_BD;
        ma_lydo_dtri = entity.MA_LYDO_DTRI;
        loai_dtri_lao = entity.LOAI_DTRI_LAO;
        sang_loc_lao = entity.SANG_LOC_LAO;
        phacdo_dtri_lao = entity.PHACDO_DTRI_LAO;
        ngaybd_dtri_lao = Uti.ToDateView(entity.NGAYBD_DTRI_LAO);
        ngaykt_dtri_lao = Uti.ToDateView(entity.NGAYKT_DTRI_LAO);
        kq_dtri_lao = entity.KQ_DTRI_LAO;
        ma_lydo_xntl_vr = entity.MA_LYDO_XNTL_VR;
        ngay_xn_tlvr = Uti.ToDateView(entity.NGAY_XN_TLVR);
        kq_xntl_vr = entity.KQ_XNTL_VR;
        ngay_kq_xn_tlvr = Uti.ToDateView(entity.NGAY_KQ_XN_TLVR);
        ma_loai_bn = entity.MA_LOAI_BN;
        giai_doan_lam_sang = entity.GIAI_DOAN_LAM_SANG;
        nhom_doi_tuong = entity.NHOM_DOI_TUONG;
        ma_tinh_trang_dk = entity.MA_TINH_TRANG_DK;
        lan_xn_pcr = entity.LAN_XN_PCR;
        ngay_xn_pcr = Uti.ToDateView(entity.NGAY_XN_PCR);
        ngay_kq_xn_pcr = Uti.ToDateView(entity.NGAY_KQ_XN_PCR);
        ma_kq_xn_pcr = entity.MA_KQ_XN_PCR;
        ngay_nhan_tt_mang_thai = Uti.ToDateView(entity.NGAY_NHAN_TT_MANG_THAI);
        ngay_bat_dau_dt_ctx = Uti.ToDateView(entity.NGAY_BAT_DAU_DT_CTX);
        ma_xu_tri = entity.MA_XU_TRI;
        ngay_bat_dau_xu_tri = Uti.ToDateView(entity.NGAY_BAT_DAU_XU_TRI);
        ngay_ket_thuc_xu_tri = Uti.ToDateView(entity.NGAY_KET_THUC_XU_TRI);
        ma_phac_do_dieu_tri = entity.MA_PHAC_DO_DIEU_TRI;
        ma_bac_phac_do = entity.MA_BAC_PHAC_DO;
        so_ngay_cap_thuoc_arv = entity.SO_NGAY_CAP_THUOC_ARV;
        ngay_cchuyen_phac_do = Uti.ToDateView(entity.NGAY_CHUYEN_PHAC_DO);
        ly_do_chuyen_phac_do = entity.LY_DO_CHUYEN_PHAC_DO;
        du_phong = entity.DU_PHONG;
    }
}
