using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;

namespace HIA.Core.Models.Files130;

public class XML01Info : IXMLModel<XML01>
{
    public long xml01id { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? ma_bn { get; set; }
    public string? ho_ten { get; set; }
    public string? so_cccd { get; set; }
    public string? ngay_sinh { get; set; }
    public byte? gioi_tinh { get; set; }
    public string? nhom_mau { get; set; }
    public string? ma_quoctich { get; set; }
    public string? ma_dantoc { get; set; }
    public string? ma_nghe_nghiep { get; set; }
    public string? dia_chi { get; set; }
    public string? matinh_cu_tru { get; set; }
    public string? mahuyen_cu_tru { get; set; }
    public string? maxa_cu_tru { get; set; }
    public string? dien_thoai { get; set; }
    public string? ma_the_bhyt { get; set; }
    public string? ma_dkbd { get; set; }
    public string? gt_the_tu { get; set; }
    public string? gt_the_den { get; set; }
    public string? ngay_mien_cct { get; set; }
    public string? ly_do_vv { get; set; }
    public string? ly_do_vnt { get; set; }
    public string? ma_ly_do_vnt { get; set; }
    public string? chan_doan_vao { get; set; }
    public string? chan_doan_rv { get; set; }
    public string? ma_benh_chinh { get; set; }
    public string? ma_benh_kt { get; set; }
    public string? ma_benh_yhct { get; set; }
    public string? ma_pttt_qt { get; set; }
    public string? ma_doituong_kcb { get; set; }
    public string? ma_noi_di { get; set; }
    public string? ma_noi_den { get; set; }
    public byte? ma_tai_nan { get; set; }
    public string? ngay_vao { get; set; }
    public string? ngay_vao_noi_tru { get; set; }
    public string? ngay_ra { get; set; }
    public string? giay_chuyen_tuyen { get; set; }
    public int? so_ngay_dtri { get; set; }
    public string? pp_dieu_tri { get; set; }
    public byte? ket_qua_dtri { get; set; }
    public byte? ma_loai_rv { get; set; }
    public string? ghi_chu { get; set; }
    public string? ngay_ttoan { get; set; }
    public double? t_thuoc { get; set; }
    public double? t_vtyt { get; set; }
    public double? t_tongchi_bv { get; set; }
    public double? t_tongchi_bh { get; set; }
    public double? t_bntt { get; set; }
    public double? t_bncct { get; set; }
    public double? t_bhtt { get; set; }
    public double? t_nguonkhac { get; set; }
    public double? t_bhtt_gdv { get; set; }
    public int? nam_qt { get; set; }
    public byte? thang_qt { get; set; }
    public string? ma_loai_kcb { get; set; }
    public string? ma_khoa { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_khuvuc { get; set; }
    public double? can_nang { get; set; }
    public string? can_nang_con { get; set; }
    public string? nam_nam_lien_tuc { get; set; }
    public string? ngay_tai_kham { get; set; }
    public string? ma_hsba { get; set; }
    public string? ma_ttdv { get; set; }
    public string? du_phong { get; set; }
    public string? ngay_lap { get; set; }
    public string? ngay_tao { get; set; }
    public string? file_name { get; set; }
    public string? has_files { get; set; }
    public bool? is_valid { get; set; }
    public bool? has_errors { get; set; }
    public bool? has_alerts { get; set; }

    public XML01Info(XML01? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML01 entity)
    {
        xml01id = entity.XML01ID;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        ma_bn = entity.MA_BN;
        ho_ten = entity.HO_TEN;
        so_cccd = entity.SO_CCCD;
        ngay_sinh = EvalHelper.ToDateView(entity.NGAY_SINH);
        gioi_tinh = entity.GIOI_TINH;
        nhom_mau = entity.NHOM_MAU;
        ma_quoctich = entity.MA_QUOCTICH;
        ma_dantoc = entity.MA_DANTOC;
        ma_nghe_nghiep = entity.MA_NGHE_NGHIEP;
        dia_chi = entity.DIA_CHI;
        matinh_cu_tru = entity.MATINH_CU_TRU;
        mahuyen_cu_tru = entity.MAHUYEN_CU_TRU;
        maxa_cu_tru = entity.MAXA_CU_TRU;
        dien_thoai = entity.DIEN_THOAI;
        ma_the_bhyt = entity.MA_THE_BHYT;
        ma_dkbd = entity.MA_DKBD;
        gt_the_tu = entity.GT_THE_TU;
        gt_the_den = entity.GT_THE_DEN;
        ngay_mien_cct = EvalHelper.ToDateView(entity.NGAY_MIEN_CCT);
        ly_do_vv = entity.LY_DO_VV;
        ly_do_vnt = entity.LY_DO_VNT;
        ma_ly_do_vnt = entity.MA_LY_DO_VNT;
        chan_doan_vao = entity.CHAN_DOAN_VAO;
        chan_doan_rv = entity.CHAN_DOAN_RV;
        ma_benh_chinh = entity.MA_BENH_CHINH;
        ma_benh_kt = entity.MA_BENH_KT;
        ma_benh_yhct = entity.MA_BENH_YHCT;
        ma_pttt_qt = entity.MA_PTTT_QT;
        ma_doituong_kcb = entity.MA_DOITUONG_KCB;
        ma_noi_di = entity.MA_NOI_DI;
        ma_noi_den = entity.MA_NOI_DEN;
        ma_tai_nan = entity.MA_TAI_NAN;
        ngay_vao = EvalHelper.ToDateView(entity.NGAY_VAO);
        ngay_vao_noi_tru = EvalHelper.ToDateView(entity.NGAY_VAO_NOI_TRU);
        ngay_ra = EvalHelper.ToDateView(entity.NGAY_RA);
        giay_chuyen_tuyen = entity.GIAY_CHUYEN_TUYEN;
        so_ngay_dtri = entity.SO_NGAY_DTRI;
        pp_dieu_tri = entity.PP_DIEU_TRI;
        ket_qua_dtri = entity.KET_QUA_DTRI;
        ma_loai_rv = entity.MA_LOAI_RV;
        ghi_chu = entity.GHI_CHU;
        ngay_ttoan = EvalHelper.ToDateView(entity.NGAY_TTOAN);
        t_thuoc = entity.T_THUOC;
        t_vtyt = entity.T_VTYT;
        t_tongchi_bv = entity.T_TONGCHI_BV;
        t_tongchi_bh = entity.T_TONGCHI_BH;
        t_bntt = entity.T_BNTT;
        t_bncct = entity.T_BNCCT;
        t_bhtt = entity.T_BHTT;
        t_nguonkhac = entity.T_NGUONKHAC;
        t_bhtt_gdv = entity.T_BHTT_GDV;
        nam_qt = entity.NAM_QT;
        thang_qt = entity.THANG_QT;
        ma_loai_kcb = entity.MA_LOAI_KCB;
        ma_khoa = entity.MA_KHOA;
        ma_cskcb = entity.MA_CSKCB;
        ma_khuvuc = entity.MA_KHUVUC;
        can_nang = entity.CAN_NANG;
        can_nang_con = entity.CAN_NANG_CON;
        nam_nam_lien_tuc = EvalHelper.ToDateView(entity.NAM_NAM_LIEN_TUC);
        ngay_tai_kham = entity.NGAY_TAI_KHAM;
        ma_hsba = entity.MA_HSBA;
        ma_ttdv = entity.MA_TTDV;
        du_phong = entity.DU_PHONG;
        ngay_lap = EvalHelper.ToDateView(entity.NGAY_LAP);
        ngay_tao = EvalHelper.ToDateFullView(entity.CreateDate);
        file_name = entity.FileName;
        has_files = entity.HasFiles;
        is_valid = false;
        has_errors = false;
        has_alerts = false;

            if (entity.State != null)
        {
            is_valid = entity.State.CheckDate != null && entity.State.CheckDate.HasValue;
            has_errors = entity.State.TONG_SO_LOI - entity.State.CANH_BAO > 0;
            has_alerts = entity.State.CANH_BAO > 0;
        }
    }
}
