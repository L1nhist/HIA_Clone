using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class DMNhomBenh
    {
        public int NBID { get; set; }
        public string NBCode { get; set; }
        public string NBTenNhom_EN { get; set; }
        public string NBTenNhom_VN { get; set; }
        public string NBCode_Sub_1 { get; set; }
        public string NBTenNhom_Phu_1_EN { get; set; }
        public string NBTenNhom_Phu_1_VN { get; set; }
        public string NBCode_Sub_2 { get; set; }
        public string NBTenNhom_Phu_2_EN { get; set; }
        public string NBTenNhom_Phu_2_VN { get; set; }
    }
}
