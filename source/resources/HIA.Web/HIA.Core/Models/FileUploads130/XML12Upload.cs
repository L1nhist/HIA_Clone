using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("CHI_TIEU_GIAM_DINH_YKHOA", IsNullable = false)]
    public partial class XML12_DanhSach
    {
        [XmlArrayItem("DSACH_GIAM_DINH_YKHOA", IsNullable = false)]
        [XmlArrayItem("GIAM_DINH_YKHOA", IsNullable = false, NestingLevel = 1)]
        public XML12_GiamDinhYKhoa[] GIAM_DINH_YKHOA { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("GIAM_DINH_YKHOA", IsNullable = false)]
    public partial class XML12_GiamDinhYKhoa
    {
        public string NGUOI_CHU_TRI { get; set; }
        public string CHUC_VU { get; set; }
        public string NGAY_HOP { get; set; }
        public string HO_TEN { get; set; }
        public string NGAY_SINH { get; set; }
        public string SO_CCCD { get; set; }
        public string NGAY_CAP_CCCD { get; set; }
        public string NOI_CAP_CCCD { get; set; }
        public string DIA_CHI { get; set; }
        public string MATINH_CU_TRU { get; set; }
        public string MAHUYEN_CU_TRU { get; set; }
        public string MAXA_CU_TRU { get; set; }
        public string MA_BHXH { get; set; }
        public string MA_THE_BHYT { get; set; }
        public string NGHE_NGHIEP { get; set; }
        public string DIEN_THOAI { get; set; }
        public string MA_DOI_TUONG { get; set; }
        public string KHAM_GIAM_DINH { get; set; }
        public string SO_BIEN_BAN { get; set; }
        public string TYLE_TTCT_CU { get; set; }
        public string DANG_HUONG_CHE_DO { get; set; }
        public string NGAY_CHUNG_TU { get; set; }
        public string SO_GIAY_GIOI_THIEU { get; set; }
        public string NGAY_DE_NGHI { get; set; }
        public string MA_DONVI { get; set; }
        public string GIOI_THIEU_CUA { get; set; }
        public string KET_QUA_KHAM { get; set; }
        public string SO_VAN_BAN_CAN_CU { get; set; }
        public string TYLE_TTCT_MOI { get; set; }
        public string TONG_TYLE_TTCT { get; set; }
        public string DANG_KHUYETTAT { get; set; }
        public string MUC_DO_KHUYETTAT { get; set; }
        public string DE_NGHI { get; set; }
        public string DUOC_XACDINH { get; set; }
        public string DU_PHONG { get; set; }
    }
}