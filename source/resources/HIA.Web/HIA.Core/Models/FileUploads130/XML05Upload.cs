using System;
using System.ComponentModel;
using System.Xml.Serialization;
using HIA.Core.Base;
using HIA.DatabaseModel.Entites;

namespace HIA.Core.Models.FileUploads130
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("CHITIEU_CHITIET_DIENBIENLAMSANG", IsNullable = false)]
    public partial class CHITIEU_XML05
    {
        [XmlElement("DSACH_CHI_TIET_DIEN_BIEN_BENH")]
        public DSACH_DBLS DANH_SACH { get; set; }
    }

    public class DSACH_DBLS
    {
        [XmlElement("CHI_TIET_DIEN_BIEN_BENH")]
        public HOSO_XML05[] CHI_TIETs { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot("CHI_TIET_DIEN_BIEN_BENH", IsNullable = false)]
    public class HOSO_XML05 : IXMLFile<XML05>
    {
        public string MA_LK { get; set; }
        public string STT { get; set; }
        public string DIEN_BIEN_LS { get; set; }
        public string GIAI_DOAN_BENH { get; set; }
        public string HOI_CHAN { get; set; }
        public string PHAU_THUAT { get; set; }
        public string THOI_DIEM_DBLS { get; set; }
        public string NGUOI_THUC_HIEN { get; set; }
        public string DU_PHONG { get; set; }

        public HOSO_XML05()
        { }

        public XML05 Export()
        {
            return new XML05()
            {
                MA_LK = MA_LK,
                STT = EvalHelper.Cast<int>(STT),
                DIEN_BIEN_LS = DIEN_BIEN_LS,
                GIAI_DOAN_BENH = GIAI_DOAN_BENH,
                HOI_CHAN = HOI_CHAN,
                PHAU_THUAT = PHAU_THUAT,
                THOI_DIEM_DBLS = EvalHelper.ToDateSql(THOI_DIEM_DBLS),
                NGUOI_THUC_HIEN = NGUOI_THUC_HIEN,
                DU_PHONG = DU_PHONG,
            };
        }
    }
}