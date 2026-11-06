using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.Files130
{
    public class XML07Info : IXMLModel<XML07>
    {
        public long xml07id { get; set; }
        public long xml01id { get; set; }
        public string ma_cskcb { get; set; }
        public string ma_lk { get; set; }
        public int? stt { get; set; }
        public string so_luu_tru { get; set; }
        public string ma_yte { get; set; }
        public string ma_khoa_rv { get; set; }
        public string ngay_vao { get; set; }
        public string ngay_ra { get; set; }
        public byte? ma_dinh_chi_thai { get; set; }
        public string nguyennhan_dinhchi { get; set; }
        public string thoigian_dinhchi { get; set; }
        public byte? tuoi_thai { get; set; }
        public string chan_doan_rv { get; set; }
        public string pp_dieutri { get; set; }
        public string ghi_chu { get; set; }
        public string ma_ttdv { get; set; }
        public string ma_bs { get; set; }
        public string ten_bs { get; set; }
        public string ngay_ct { get; set; }
        public string ma_cha { get; set; }
        public string ma_me { get; set; }
        public string ma_the_tam { get; set; }
        public string ho_ten_cha { get; set; }
        public string ho_ten_me { get; set; }
        public byte? so_ngay_nghi { get; set; }
        public string ngoaitru_tungay { get; set; }
        public string ngoaitru_denngay { get; set; }
        public string du_phong { get; set; }

        public XML07Info(XML07 entity = null)
        {
            if (entity != null) Import(entity);
        }

        public void Import(XML07 entity)
        {
            xml07id = entity.XML07ID;
            xml01id = entity.XML01ID;
            ma_cskcb = entity.MA_CSKCB;
            ma_lk = entity.MA_LK;
            stt = entity.STT;
            so_luu_tru = entity.SO_LUU_TRU;
            ma_yte = entity.MA_YTE;
            ma_khoa_rv = entity.MA_KHOA_RV;
            ngay_vao = Uti.ToDateView(entity.NGAY_VAO);
            ngay_ra = Uti.ToDateView(entity.NGAY_RA);
            ma_dinh_chi_thai = entity.MA_DINH_CHI_THAI;
            nguyennhan_dinhchi = entity.NGUYENNHAN_DINHCHI;
            thoigian_dinhchi = Uti.ToDateView(entity.THOIGIAN_DINHCHI);
            tuoi_thai = entity.TUOI_THAI;
            chan_doan_rv = entity.CHAN_DOAN_RV;
            pp_dieutri = entity.PP_DIEUTRI;
            ghi_chu = entity.GHI_CHU;
            ma_ttdv = entity.MA_TTDV;
            ma_bs = entity.MA_BS;
            ten_bs = entity.TEN_BS;
            ngay_ct = Uti.ToDateView(entity.NGAY_CT);
            ma_cha = entity.MA_CHA;
            ma_me = entity.MA_ME;
            ma_the_tam = entity.MA_THE_TAM;
            ho_ten_cha = entity.HO_TEN_CHA;
            ho_ten_me = entity.HO_TEN_ME;
            so_ngay_nghi = entity.SO_NGAY_NGHI;
            ngoaitru_tungay = Uti.ToDateView(entity.NGOAITRU_TUNGAY);
            ngoaitru_denngay = Uti.ToDateView(entity.NGOAITRU_DENNGAY);
            du_phong = entity.DU_PHONG;
        }
    }
}