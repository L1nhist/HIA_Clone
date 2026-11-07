
using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Records;
namespace HIA.Core.Models.Files;

public class XML03Info : IXMLModel<XML03>
{
    public long xml03id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? ma_dich_vu { get; set; }
    public string? ma_pttt_qt { get; set; }
    public string? ma_vat_tu { get; set; }
    public byte? ma_nhom { get; set; }
    public string? goi_vtyt { get; set; }
    public string? ten_vat_tu { get; set; }
    public string? ten_dich_vu { get; set; }
    public string? ma_xang_dau { get; set; }
    public string? don_vi_tinh { get; set; }
    public byte? pham_vi { get; set; }
    public double? so_luong { get; set; }
    public double? don_gia_bv { get; set; }
    public double? don_gia_bh { get; set; }
    public string? tt_thau { get; set; }
    public byte? tyle_tt_dv { get; set; }
    public byte? tyle_tt_bh { get; set; }
    public double? thanh_tien_bv { get; set; }
    public double? thanh_tien_bh { get; set; }
    public double? t_trantt { get; set; }
    public double? muc_huong { get; set; }
    public double? t_nguonkhac_nsnn { get; set; }
    public double? t_nguonkhac_vtnn { get; set; }
    public double? t_nguonkhac_vttn { get; set; }
    public double? t_nguonkhac_cl { get; set; }
    public double? t_nguonkhac { get; set; }
    public double? t_bntt { get; set; }
    public double? t_bncct { get; set; }
    public double? t_bhtt { get; set; }
    public string? ma_khoa { get; set; }
    public string? ma_giuong { get; set; }
    public string? ma_bac_si { get; set; }
    public string? nguoi_thuc_hien { get; set; }
    public string? ma_benh { get; set; }
    public string? ma_benh_yhct { get; set; }
    public string? ngay_yl { get; set; }
    public string? ngay_th_yl { get; set; }
    public string? ngay_kq { get; set; }
    public byte? ma_pttt { get; set; }
    public byte? vet_thuong_tp { get; set; }
    public byte? pp_vo_cam { get; set; }
    public int? vi_tri_th_dvkt { get; set; }
    public string? ma_may { get; set; }
    public string? ma_hieu_sp { get; set; }
    public byte? tai_su_dung { get; set; }
    public string? du_phong { get; set; }

    public XML03Info(XML03? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML03 entity)
    {
        xml03id = entity.XML03ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        ma_dich_vu = entity.MA_DICH_VU;
        ma_pttt_qt = entity.MA_PTTT_QT;
        ma_vat_tu = entity.MA_VAT_TU;
        ma_nhom = entity.MA_NHOM;
        goi_vtyt = entity.GOI_VTYT;
        ten_vat_tu = entity.TEN_VAT_TU;
        ten_dich_vu = entity.TEN_DICH_VU;
        ma_xang_dau = entity.MA_XANG_DAU;
        don_vi_tinh = entity.DON_VI_TINH;
        pham_vi = entity.PHAM_VI;
        so_luong = entity.SO_LUONG;
        don_gia_bv = entity.DON_GIA_BV;
        don_gia_bh = entity.DON_GIA_BH;
        tt_thau = entity.TT_THAU;
        tyle_tt_dv = entity.TYLE_TT_DV;
        tyle_tt_bh = entity.TYLE_TT_BH;
        thanh_tien_bv = entity.THANH_TIEN_BV;
        thanh_tien_bh = entity.THANH_TIEN_BH;
        t_trantt = entity.T_TRANTT;
        muc_huong = entity.MUC_HUONG;
        t_nguonkhac_nsnn = entity.T_NGUONKHAC_NSNN;
        t_nguonkhac_vtnn = entity.T_NGUONKHAC_VTNN;
        t_nguonkhac_vttn = entity.T_NGUONKHAC_VTTN;
        t_nguonkhac_cl = entity.T_NGUONKHAC_CL;
        t_nguonkhac = entity.T_NGUONKHAC;
        t_bntt = entity.T_BNTT;
        t_bncct = entity.T_BNCCT;
        t_bhtt = entity.T_BHTT;
        ma_khoa = entity.MA_KHOA;
        ma_giuong = entity.MA_GIUONG;
        ma_bac_si = entity.MA_BAC_SI;
        nguoi_thuc_hien = entity.NGUOI_THUC_HIEN;
        ma_benh = entity.MA_BENH;
        ma_benh_yhct = entity.MA_BENH_YHCT;
        ngay_yl = EvalHelper.ToDateView(entity.NGAY_YL);
        ngay_th_yl = EvalHelper.ToDateView(entity.NGAY_TH_YL);
        ngay_kq = EvalHelper.ToDateView(entity.NGAY_KQ);
        ma_pttt = entity.MA_PTTT;
        vet_thuong_tp = entity.VET_THUONG_TP;
        pp_vo_cam = entity.PP_VO_CAM;
        vi_tri_th_dvkt = entity.VI_TRI_TH_DVKT;
        ma_may = entity.MA_MAY;
        ma_hieu_sp = entity.MA_HIEU_SP;
        tai_su_dung = entity.TAI_SU_DUNG;
        du_phong = entity.DU_PHONG;
    }
}
