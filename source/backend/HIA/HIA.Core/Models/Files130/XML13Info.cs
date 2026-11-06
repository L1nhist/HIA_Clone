using HIA.Core.Base;
using HIA.Data.Base;
using HIA.Data.Entities;

namespace HIA.Core.Models.Files130;

public class XML13Info : IXMLModel<XML13>
{
    public long xml13id { get; set; }
    public long xml01id { get; set; }
    public string? ma_cskcb { get; set; }
    public string? ma_lk { get; set; }
    public int? stt { get; set; }
    public string? so_hoso { get; set; }
    public string? so_chuyentuyen { get; set; }
    public string? giay_chuyen_tuyen { get; set; }
    public string? ma_noi_di { get; set; }
    //Linhist added 06/02/24 - thay ma_noi_di
    public string? ma_cskcb_di { get; set; }
    public string? ma_noi_den { get; set; }
    //Linhist added 06/02/24 - thay ma_noi_den
    public string? ma_cskcb_den { get; set; }
    public string? ho_ten { get; set; }
    public string? ngay_sinh { get; set; }
    public byte? gioi_tinh { get; set; }
    public string? ma_quoctich { get; set; }
    public string? ma_dantoc { get; set; }
    public string? ma_nghe_nghiep { get; set; }
    public string? dia_chi { get; set; }
    public string? ma_the_bhyt { get; set; }
    public string? gt_the_den { get; set; }
    public string? ngay_vao { get; set; }
    public string? ngay_vao_noi_tru { get; set; }
    public string? ngay_ra { get; set; }
    public string? dau_hieu_ls { get; set; }
    public string? chan_doan_rv { get; set; }
    public string? qt_benhly { get; set; }
    public string? tomtat_kq { get; set; }
    public string? pp_dieutri { get; set; }
    public string? ma_benh_chinh { get; set; }
    public string? ma_benh_kt { get; set; }
    public string? ma_benh_yhct { get; set; }
    public string? ten_dich_vu { get; set; }
    public string? ten_thuoc { get; set; }
    public string? pp_dieu_tri { get; set; }
    //Linhist added 06/02/24 - pp_dieu_tri
    public string? tinh_trang_ct { get; set; }
    public byte? ma_loai_rv { get; set; }
    public byte? ma_lydo_ct { get; set; }
    public string? huong_dieu_tri { get; set; }
    public string? phuongtien_vc { get; set; }
    public string? hoten_nguoi_ht { get; set; }
    public string? chucdanh_nguoi_ht { get; set; }
    public string? ma_bac_si { get; set; }
    public string? ma_ttdv { get; set; }
    public string? du_phong { get; set; }

    public XML13Info(XML13? entity = null)
    {
        if (entity != null) Import(entity);
    }

    public void Import(XML13 entity)
    {
        xml13id = entity.XML13ID;
        xml01id = entity.XML01ID;
        ma_cskcb = entity.MA_CSKCB;
        ma_lk = entity.MA_LK;
        stt = entity.STT;
        so_hoso = entity.SO_HOSO;
        so_chuyentuyen = entity.SO_CHUYENTUYEN;
        giay_chuyen_tuyen = entity.GIAY_CHUYEN_TUYEN;
        ma_cskcb = entity.MA_CSKCB;
        ma_noi_di = entity.MA_NOI_DI;
        //Linhist added 06/02/24 - thay ma_noi_di
        ma_cskcb_di = entity.MA_CSKCB_DI;
        ma_noi_den = entity.MA_NOI_DEN;
        //Linhist added 06/02/24 - thay ma_noi_den
        ma_cskcb_den = entity.MA_CSKCB_DEN;
        ho_ten = entity.HO_TEN;
        ngay_sinh = EvalHelper.ToDateView(entity.NGAY_SINH);
        gioi_tinh = entity.GIOI_TINH;
        ma_quoctich = entity.MA_QUOCTICH;
        ma_dantoc = entity.MA_DANTOC;
        ma_nghe_nghiep = entity.MA_NGHE_NGHIEP;
        dia_chi = entity.DIA_CHI;
        ma_the_bhyt = entity.MA_THE_BHYT;
        gt_the_den = entity.GT_THE_DEN;
        ngay_vao = EvalHelper.ToDateView(entity.NGAY_VAO);
        ngay_vao_noi_tru = EvalHelper.ToDateView(entity.NGAY_VAO_NOI_TRU);
        ngay_ra = EvalHelper.ToDateView(entity.NGAY_RA);
        dau_hieu_ls = entity.DAU_HIEU_LS;
        chan_doan_rv = entity.CHAN_DOAN_RV;
        qt_benhly = entity.QT_BENHLY;
        tomtat_kq = entity.TOMTAT_KQ;
        pp_dieutri = entity.PP_DIEUTRI;
        ma_benh_chinh = entity.MA_BENH_CHINH;
        ma_benh_kt = entity.MA_BENH_KT;
        ma_benh_yhct = entity.MA_BENH_YHCT;
        ten_dich_vu = entity.TEN_DICH_VU;
        ten_thuoc = entity.TEN_THUOC;
        pp_dieu_tri = entity.PP_DIEU_TRI;
        //Linhist added 06/02/24 - thay pp_dieu_tri
        tinh_trang_ct = entity.TINH_TRANG_CT;
        ma_loai_rv = entity.MA_LOAI_RV;
        ma_lydo_ct = entity.MA_LYDO_CT;
        huong_dieu_tri = entity.HUONG_DIEU_TRI;
        phuongtien_vc = entity.PHUONGTIEN_VC;
        hoten_nguoi_ht = entity.HOTEN_NGUOI_HT;
        chucdanh_nguoi_ht = entity.CHUCDANH_NGUOI_HT;
        ma_bac_si = entity.MA_BAC_SI;
        ma_ttdv = entity.MA_TTDV;
        du_phong = entity.DU_PHONG;
    }
}
