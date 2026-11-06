using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class DMBenhPhanLoai
    {
        public DMBenhPhanLoai()
        {
            DMBenhs = new HashSet<DMBenh>();
        }

        public int LBID { get; set; }
        public string LBCode { get; set; }
        public string LBTenLoai_EN { get; set; }
        public string LBTenLoai_VN { get; set; }
        public int? LBNBID { get; set; }
        public string NBCode { get; set; }

        public virtual ICollection<DMBenh> DMBenhs { get; set; }
    }
}
