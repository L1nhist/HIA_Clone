using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class HDSDDichVu
    {
        public int HDSDDVID { get; set; }
        public string MA_DICH_VU { get; set; }
        public string TEN_DICH_VU { get; set; }
        public bool? CHI_DINH { get; set; }
        public string TUOI { get; set; }
        public bool? CO_THAI { get; set; }
        public bool? CHO_CON_BU { get; set; }
        public string MA_BENH { get; set; }
        public string THUOC_KHAC { get; set; }
        public string MA_NHOM { get; set; }
        public string DICH_VU_KHAC { get; set; }
        public string TIEN_SU_BENH { get; set; }
        public string TYLE_TT { get; set; }
        public double? THOI_GIAN_TOI_THIEU { get; set; }
        public string THOI_GIAN_THUC_HIEN { get; set; }
        public string MA_LOI { get; set; }
        public bool? HIEU_LUC { get; set; }
        public string MA_CSKCB { get; set; }
        public int? MA_LOAI_KCB { get; set; }
        public int? SO_NGUOI_TOI_THIEU { get; set; }

        public string MA_DICH_VU_KO_CHECK { get; set; }

    }
}