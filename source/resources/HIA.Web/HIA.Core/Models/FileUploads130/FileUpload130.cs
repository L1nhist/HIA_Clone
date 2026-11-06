using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HIA.Core.Models.FileUploads130
{
    #region Models for Matching File Total uploaded by Hospital
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot("GIAMDINHHS", IsNullable = false)]
    public partial class GIAMDINHHS130
    {
        public THONGTINDONVI THONGTINDONVI { get; set; }

        public THONGTINHOSO THONGTINHOSO { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class THONGTINDONVI
    {
        public string MACSKCB { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class THONGTINHOSO
    {
        public string NGAYLAP { get; set; }

        public int? SOLUONGHOSO { get; set; }

        [XmlArrayItem("HOSO", IsNullable = false)]
        [XmlArrayItem("FILEHOSO", IsNullable = false, NestingLevel = 1)]
        public FILEHOSO[][] DANHSACHHOSO { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class FILEHOSO
    {
        public string LOAIHOSO { get; set; }

        public string NOIDUNGFILE { get; set; }
    }
    #endregion
}