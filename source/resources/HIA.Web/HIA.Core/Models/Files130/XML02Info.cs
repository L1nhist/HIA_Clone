using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.Files130
{
    public class XML02Info : IXMLModel<XML02>
    {
        public long xml02id { get; set; }
        public long xml01id { get; set; }
        public string ma_cskcb { get; set; }
        public string ma_lk { get; set; }
        public int? stt { get; set; }
        public string ma_thuoc { get; set; }
        public string ma_pp_chebien { get; set; }
        public string ma_cskcb_thuoc { get; set; }
        public byte? ma_nhom { get; set; }
        public string ten_thuoc { get; set; }
        public string don_vi_tinh { get; set; }
        public string ham_luong { get; set; }
        public string duong_dung { get; set; }
        public string dang_bao_che { get; set; }
        public string lieu_dung { get; set; }
        public string cach_dung { get; set; }
        public string so_dang_ky { get; set; }
        public string tt_thau { get; set; }
        public byte? pham_vi { get; set; }
        public byte? tyle_tt_bh { get; set; }
        public double? so_luong { get; set; }
        public double? don_gia { get; set; }
        public double? thanh_tien_bv { get; set; }
        public double? thanh_tien_bh { get; set; }
        public double? t_nguonkhac_nsnn { get; set; }
        public double? t_nguonkhac_vtnn { get; set; }
        public double? t_nguonkhac_vttn { get; set; }
        public double? t_nguonkhac_cl { get; set; }
        public double? t_nguonkhac { get; set; }
        public double? muc_huong { get; set; }
        public double? t_bntt { get; set; }
        public double? t_bncct { get; set; }
        public double? t_bhtt { get; set; }
        public string ma_khoa { get; set; }
        public string ma_bac_si { get; set; }
        public string ma_dich_vu { get; set; }
        public string ngay_th_yl { get; set; }
        public string ngay_yl { get; set; }
        public byte? ma_pttt { get; set; }
        public byte? nguon_ctra { get; set; }
        public byte? vet_thuong_tp { get; set; }
        public string du_phong { get; set; }

        public XML02Info(XML02 entity = null)
        {
            if (entity != null) Import(entity);
        }

        public void Import(XML02 entity)
        {
            xml02id = entity.XML02ID;
            xml01id = entity.XML01ID;
            ma_cskcb = entity.MA_CSKCB;
            ma_lk = entity.MA_LK;
            stt = entity.STT;
            ma_thuoc = entity.MA_THUOC;
            ma_pp_chebien = entity.MA_PP_CHEBIEN;
            ma_cskcb_thuoc = entity.MA_CSKCB_THUOC;
            ma_nhom = entity.MA_NHOM;
            ten_thuoc = entity.TEN_THUOC;
            don_vi_tinh = entity.DON_VI_TINH;
            ham_luong = entity.HAM_LUONG;
            duong_dung = entity.DUONG_DUNG;
            dang_bao_che = entity.DANG_BAO_CHE;
            lieu_dung = entity.LIEU_DUNG;
            cach_dung = entity.CACH_DUNG;
            so_dang_ky = entity.SO_DANG_KY;
            tt_thau = entity.TT_THAU;
            pham_vi = entity.PHAM_VI;
            tyle_tt_bh = entity.TYLE_TT_BH;
            so_luong = entity.SO_LUONG;
            don_gia = entity.DON_GIA;
            thanh_tien_bv = entity.THANH_TIEN_BV;
            thanh_tien_bh = entity.THANH_TIEN_BH;
            t_nguonkhac_nsnn = entity.T_NGUONKHAC_NSNN;
            t_nguonkhac_vtnn = entity.T_NGUONKHAC_VTNN;
            t_nguonkhac_vttn = entity.T_NGUONKHAC_VTTN;
            t_nguonkhac_cl = entity.T_NGUONKHAC_CL;
            t_nguonkhac = entity.T_NGUONKHAC;
            muc_huong = entity.MUC_HUONG;
            t_bntt = entity.T_BNTT;
            t_bncct = entity.T_BNCCT;
            t_bhtt = entity.T_BHTT;
            ma_khoa = entity.MA_KHOA;
            ma_bac_si = entity.MA_BAC_SI;
            ma_dich_vu = entity.MA_DICH_VU;
            ngay_th_yl = EvalHelper.ToDateView(entity.NGAY_TH_YL);
            ngay_yl = EvalHelper.ToDateView(entity.NGAY_YL);
            ma_pttt = entity.MA_PTTT;
            nguon_ctra = entity.NGUON_CTRA;
            vet_thuong_tp = entity.VET_THUONG_TP;
            du_phong = entity.DU_PHONG;
        }
    }
}