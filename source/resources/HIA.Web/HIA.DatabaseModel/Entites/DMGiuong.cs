using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class DMGiuong
    {
        public int GiuongID { get; set; }
        public string MA_CSKCB { get; set; }
        public string GiuongCode { get; set; }
        public string GiuongName { get; set; }
        public string GiuongNameTT37 { get; set; }
        public byte? GiuongClass { get; set; }
        public byte? GiuongType { get; set; }
        public byte? GiuongKhoa { get; set; }
        public string GiuongMaKhoa { get; set; }
    }
}
