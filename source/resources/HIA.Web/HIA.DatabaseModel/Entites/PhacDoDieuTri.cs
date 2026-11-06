using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class PhacDoDieuTri
    {
        public int PhacDoID { get; set; }
        public string MA_CSKCB { get; set; }
        public string PhacDoMaBenh { get; set; }
        public string PhacDoMaDichVu { get; set; }
        public string PhacDoMaThuoc { get; set; }
        public int? PhacDoMaLoiDichVu { get; set; }
        public int? PhacDoMaLoiThuoc { get; set; }
    }
}
