using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.Files130
{
    public class XML04Info : IXMLModel<XML04>
    {
        public long xml04id { get; set; }
        public long xml01id { get; set; }
        public string ma_cskcb { get; set; }
        public string ma_lk { get; set; }
        public int? stt { get; set; }
        public string ma_dich_vu { get; set; }
        public string ma_chi_so { get; set; }
        public string ten_chi_so { get; set; }
        public string gia_tri { get; set; }
        public string don_vi_do { get; set; }
        public string mo_ta { get; set; }
        public string ket_luan { get; set; }
        public string ngay_kq { get; set; }
        public string ma_bs_doc_kq { get; set; }
        public string du_phong { get; set; }

        public XML04Info(XML04 entity = null)
        {
            if (entity != null) Import(entity);
        }

        public void Import(XML04 entity)
        {
            xml04id = entity.XML04ID;
            xml01id = entity.XML01ID;
            ma_cskcb = entity.MA_CSKCB;
            ma_lk = entity.MA_LK;
            stt = entity.STT;
            ma_dich_vu = entity.MA_DICH_VU;
            ma_chi_so = entity.MA_CHI_SO;
            ten_chi_so = entity.TEN_CHI_SO;
            gia_tri = entity.GIA_TRI;
            don_vi_do = entity.DON_VI_DO;
            mo_ta = entity.MO_TA;
            ket_luan = entity.KET_LUAN;
            ngay_kq = Uti.ToDateView(entity.NGAY_KQ);
            ma_bs_doc_kq = entity.MA_BS_DOC_KQ;
            du_phong = entity.DU_PHONG;
        }
    }
}