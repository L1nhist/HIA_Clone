using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.Files130
{
    public class XML11Info : IXMLModel<XML11>
    {
        public long xml11id { get; set; }
        public long xml01id { get; set; }
        public string ma_cskcb { get; set; }
        public string ma_lk { get; set; }
        public int? stt { get; set; }
        public string so_ct { get; set; }
        public string so_seri { get; set; }
        public string so_kcb { get; set; }
        public string don_vi { get; set; }
        public string ma_bhxh { get; set; }
        public string ma_the_bhyt { get; set; }
        public string chan_doan_rv { get; set; }
        public string pp_dieutri { get; set; }
        public byte? ma_dinh_chi_thai { get; set; }
        public string nguyennhan_dinhchi { get; set; }
        public byte? tuoi_thai { get; set; }
        public byte? so_ngay_nghi { get; set; }
        public string tu_ngay { get; set; }
        public string den_ngay { get; set; }
        public string ho_ten_cha { get; set; }
        public string ho_ten_me { get; set; }
        public string ma_ttdv { get; set; }
        public string ma_bs { get; set; }
        public string ngay_ct { get; set; }
        public string ma_the_tam { get; set; }
        public string mau_so { get; set; }
        public string du_phong { get; set; }

        public XML11Info(XML11 entity = null)
        {
            if (entity != null) Import(entity);
        }

        public void Import(XML11 entity)
        {
            xml11id = entity.XML11ID;
            xml01id = entity.XML01ID;
            ma_cskcb = entity.MA_CSKCB;
            ma_lk = entity.MA_LK;
            stt = entity.STT;
            so_ct = entity.SO_CT;
            so_seri = entity.SO_SERI;
            so_kcb = entity.SO_KCB;
            don_vi = entity.DON_VI;
            ma_bhxh = entity.MA_BHXH;
            ma_the_bhyt = entity.MA_THE_BHYT;
            chan_doan_rv = entity.CHAN_DOAN_RV;
            pp_dieutri = entity.PP_DIEUTRI;
            ma_dinh_chi_thai = entity.MA_DINH_CHI_THAI;
            nguyennhan_dinhchi = entity.NGUYENNHAN_DINHCHI;
            tuoi_thai = entity.TUOI_THAI;
            so_ngay_nghi = entity.SO_NGAY_NGHI;
            tu_ngay = Uti.ToDateView(entity.TU_NGAY);
            den_ngay = Uti.ToDateView(entity.DEN_NGAY);
            ho_ten_cha = entity.HO_TEN_CHA;
            ho_ten_me = entity.HO_TEN_ME;
            ma_ttdv = entity.MA_TTDV;
            ma_bs = entity.MA_BS;
            ngay_ct = Uti.ToDateView(entity.NGAY_CT);
            ma_the_tam = entity.MA_THE_TAM;
            mau_so = entity.MAU_SO;
            du_phong = entity.DU_PHONG;
        }
    }
}