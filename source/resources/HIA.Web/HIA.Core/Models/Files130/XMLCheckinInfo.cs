using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.Files130
{
    public class XMLCheckinInfo
    {
        public long xmlcid { get; set; }
        public string ma_lk { get; set; }
        public int? stt { get; set; }
        public string ma_bn { get; set; }
        public string ho_ten { get; set; }
        public string so_cccd { get; set; }
        public string ngay_sinh { get; set; }
        public byte? gioi_tinh { get; set; }
        public string ma_the_bhyt { get; set; }
        public string ma_dkbd { get; set; }
        public string gt_the_tu { get; set; }
        public string gt_the_den { get; set; }
        public byte? ma_doituong_kcb { get; set; }
        public string ngay_vao { get; set; }
        public byte? ma_loai_kcb { get; set; }
        public string ma_cskcb { get; set; }
        public string ma_dich_vu { get; set; }
        public string ten_dich_vu { get; set; }
        public string ngay_yl { get; set; }

        public XMLCheckinInfo()
        { }
    }
}