using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class DMKhac
    {
        public int STDVID { get; set; }
        public string STDVCode { get; set; }
        public string STDVName { get; set; }
        public bool? STDVIsBlood { get; set; }
        public double? STDVPrice { get; set; }
        public DateTime? STDVCreateDate { get; set; }
    }
}
