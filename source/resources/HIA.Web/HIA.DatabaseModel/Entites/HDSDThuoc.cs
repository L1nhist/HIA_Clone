using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class HDSDThuoc
    {
        public int HDSDThuocID { get; set; }
        public string MA_THUOC { get; set; }
        public string TEN_THUOC { get; set; }
        public string DUONG_DUNG { get; set; }
        public bool? CHI_DINH { get; set; }
        public string TUOI { get; set; }
        public bool? CO_THAI { get; set; }
        public bool? CHO_CON_BU { get; set; }
        public string MA_BENH { get; set; }
        public string TEN_BENH { get; set; }
        public string THUOC_KHAC { get; set; }
        public string DICH_VU { get; set; }
        public string TIEN_SU_BENH { get; set; }
        public string TYLE_TT { get; set; }
        public double? TRE_SO_SINH { get; set; }
        public double? THOI_GIAN_TOI_THIEU { get; set; }
        public int? MA_LOI { get; set; }
        public string MA_CSKCB { get; set; }
        public bool? HIEU_LUC { get; set; }
    }
}
