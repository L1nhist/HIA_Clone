using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.Files130
{
    public class XML05Info : IXMLModel<XML05>
    {
        public long xml05id { get; set; }
        public long xml01id { get; set; }
        public string ma_cskcb { get; set; }
        public string ma_lk { get; set; }
        public int? stt { get; set; }
        public string dien_bien_ls { get; set; }
        public string giai_doan_benh { get; set; }
        public string hoi_chan { get; set; }
        public string phau_thuat { get; set; }
        public string thoi_diem_dbls { get; set; }
        public string nguoi_thuc_hien { get; set; }
        public string du_phong { get; set; }

        public XML05Info(XML05 entity = null)
        {
            if (entity != null) Import(entity);
        }

        public void Import(XML05 entity)
        {
            xml05id = entity.XML05ID;
            xml01id = entity.XML01ID;
            ma_cskcb = entity.MA_CSKCB;
            ma_lk = entity.MA_LK;
            stt = entity.STT;
            dien_bien_ls = entity.DIEN_BIEN_LS;
            giai_doan_benh = entity.GIAI_DOAN_BENH;
            hoi_chan = entity.HOI_CHAN;
            phau_thuat = entity.PHAU_THUAT;
            thoi_diem_dbls = Uti.ToDateView(entity.THOI_DIEM_DBLS);
            nguoi_thuc_hien = entity.NGUOI_THUC_HIEN;
            du_phong = entity.DU_PHONG;
        }
    }
}