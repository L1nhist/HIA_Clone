using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class DMTuyenBV
    {
        public int TuyenBVID { get; set; }
        public string TuyenBVName { get; set; }
        public string TuyenBVCode { get; set; }
        public byte? TuyenBVTyleTraiTuyen { get; set; }
        public DateTime? TuyenBVCreateDate { get; set; }
        public int? TuyenBVCreator { get; set; }
    }
}
