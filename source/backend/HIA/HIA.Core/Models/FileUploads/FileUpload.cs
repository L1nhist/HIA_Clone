using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.FileUploads;

public class FileUpload
{
    [Required]
    [Display(Name = "File")]
    public List<IFormFile> FormFiles { get; set; } = new(); // convert to list
    public string? SuccessMessage { get; set; }
}

public class DocumentViewmodel
{
    public int Id { get; set; }
    [MaxLength(250)]
    public string? FileName { get; set; }
    [MaxLength(100)]
    public string? FileType { get; set; }
    [MaxLength]
    public byte[]? FileData { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}

//xml 5
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class DSACH_CHI_TIET_DIEN_BIEN_BENH
{
    [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_DIEN_BIEN_BENH")]
    public DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH[]? CHI_TIET_DIEN_BIEN_BENH { get; set; }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class DSACH_CHI_TIET_DIEN_BIEN_BENHCHI_TIET_DIEN_BIEN_BENH
{
    public string? MA_LK { get; set; }
    public int STT { get; set; }
    public string? DIEN_BIEN { get; set; }
    public string? HOI_CHAN { get; set; }
    public string? PHAU_THUAT { get; set; }
    public string? NGAY_YL { get; set; }
}

//XML 4
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class DSACH_CHI_TIET_CLS
{
    [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_CLS")]
    public DSACH_CHI_TIET_CLSCHI_TIET_CLS[]? CHI_TIET_CLS { get; set; }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class DSACH_CHI_TIET_CLSCHI_TIET_CLS
{
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string? MA_LK { get; set; }
    public int STT { get; set; }
    public string? MA_DICH_VU { get; set; }
    public string? MA_CHI_SO { get; set; }
    public string? TEN_CHI_SO { get; set; }
    public string? GIA_TRI { get; set; }
    public string? MA_MAY { get; set; }
    public string? MO_TA { get; set; }
    public string? KET_LUAN { get; set; }
    public string? NGAY_KQ { get; set; }
}

//xml3
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class GIAMDINHHS
{
    public GIAMDINHHSTHONGTINDONVI? THONGTINDONVI { get; set; }
    public GIAMDINHHSTHONGTINHOSO? THONGTINHOSO { get; set; }
    public object? CHUKYDONVI { get; set; }

}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GIAMDINHHSTHONGTINDONVI
{
    public double MACSKCB { get; set; }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GIAMDINHHSTHONGTINHOSO
{
    //haimaster convert NgAYLAP to String
    public string? NGAYLAP { get; set; }
    public ushort SOLUONGHOSO { get; set; }
    [System.Xml.Serialization.XmlArrayItemAttribute("HOSO", IsNullable = false)]
    [System.Xml.Serialization.XmlArrayItemAttribute("FILEHOSO", IsNullable = false, NestingLevel = 1)]
    public GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO[][]? DANHSACHHOSO { get; set; }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class GIAMDINHHSTHONGTINHOSOHOSOFILEHOSO
{
    public string? LOAIHOSO { get; set; }
    public string? NOIDUNGFILE { get; set; }
}

//xml2
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public class DSACH_CHI_TIET_THUOC
{
    [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_THUOC")]
    public DSACH_CHI_TIET_THUOCCHI_TIET_THUOC[]? CHI_TIET_THUOC { get; set; }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class DSACH_CHI_TIET_THUOCCHI_TIET_THUOC
{

    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string? MA_LK { get; set; }
    public int STT { get; set; }
    public string? MA_THUOC { get; set; }
    public byte MA_NHOM { get; set; }
    public string? TEN_THUOC { get; set; }
    public string? DON_VI_TINH { get; set; }
    public string? HAM_LUONG { get; set; }
    public string? DUONG_DUNG { get; set; }
    public string? LIEU_DUNG { get; set; }
    public string? SO_DANG_KY { get; set; }
    public string? TT_THAU { get; set; }
    public byte PHAM_VI { get; set; }
    public double TYLE_TT { get; set; }
    public double SO_LUONG { get; set; }
    public double DON_GIA { get; set; }
    public double THANH_TIEN { get; set; }
    public double MUC_HUONG { get; set; }
    public string? T_NGUONKHAC { get; set; }
    public double T_BNTT { get; set; }
    public double T_BHTT { get; set; }
    public double T_BNCCT { get; set; }
    public string? T_NGOAIDS { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? MA_BENH { get; set; }
    public string? NGAY_YL { get; set; }
    public byte MA_PTTT { get; set; }
}

//xml3
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public class DSACH_CHI_TIET_DVKT
{
    [System.Xml.Serialization.XmlElementAttribute("CHI_TIET_DVKT")]
    public DSACH_CHI_TIET_DVKTCHI_TIET_DVKT[]? CHI_TIET_DVKT { get; set; }
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class DSACH_CHI_TIET_DVKTCHI_TIET_DVKT
{

    //24.12.2021 haimaster comment Doi double to string
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string? MA_LK { get; set; }
    public int STT { get; set; }
    public string? MA_DICH_VU { get; set; }
    public string? MA_VAT_TU { get; set; }
    public byte MA_NHOM { get; set; }
    public string? GOI_VTYT { get; set; }
    public string? TEN_VAT_TU { get; set; }
    public string? TEN_DICH_VU { get; set; }
    public string? DON_VI_TINH { get; set; }
    public byte PHAM_VI { get; set; }
    public double SO_LUONG { get; set; }
    public double DON_GIA { get; set; }
    public string? TT_THAU { get; set; }
    public double TYLE_TT { get; set; }
    public double THANH_TIEN { get; set; }
    public string? T_TRANTT { get; set; }
    public double MUC_HUONG { get; set; }
    public string? T_NGUONKHAC { get; set; }
    public double T_BNTT { get; set; }
    public double T_BHTT { get; set; }
    public double T_BNCCT { get; set; }
    public string? T_NGOAIDS { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_GIUONG { get; set; }
    public string? MA_BAC_SI { get; set; }
    public string? MA_BENH { get; set; }
    public string? NGAY_YL { get; set; }
    public string? NGAY_KQ { get; set; }
    public byte MA_PTTT { get; set; }
}

//xml 1
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]

public partial class TONG_HOP
{

    //</TONG_HOP>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string? MA_LK { get; set; }
    public int STT { get; set; }
    public string? MA_BN { get; set; }
    public string? HO_TEN { get; set; }
    public string? NGAY_SINH { get; set; }
    public byte GIOI_TINH { get; set; }
    public string? DIA_CHI { get; set; }
    public string? MA_THE { get; set; }
    public string? MA_DKBD { get; set; }
    public string? GT_THE_TU { get; set; }
    public string? GT_THE_DEN { get; set; }
    public string? MIEN_CUNG_CT { get; set; }
    public string? TEN_BENH { get; set; }
    public string? MA_BENH { get; set; }
    public string? MA_BENHKHAC { get; set; }
    public string? MA_LYDO_VVIEN { get; set; }
    public string? MA_NOI_CHUYEN { get; set; }
    public string? MA_TAI_NAN { get; set; }
    public string? NGAY_VAO { get; set; }
    public string? NGAY_RA { get; set; }
    public int SO_NGAY_DTRI { get; set; }
    public byte KET_QUA_DTRI { get; set; }
    public byte TINH_TRANG_RV { get; set; }
    public string? NGAY_TTOAN { get; set; }
    public string? T_THUOC { get; set; }
    public string? T_VTYT { get; set; }
    public string? T_TONGCHI { get; set; }
    public string? T_BNTT { get; set; }
    public string? T_BNCCT { get; set; }
    public string? T_BHTT { get; set; }
    public string? T_NGUONKHAC { get; set; }
    public string? T_NGOAIDS { get; set; }
    public int NAM_QT { get; set; }
    public byte THANG_QT { get; set; }
    public byte MA_LOAI_KCB { get; set; }
    public string? MA_KHOA { get; set; }
    public string? MA_CSKCB { get; set; }
    public string? MA_KHUVUC { get; set; }
    public string? MA_PTTT_QT { get; set; }
    public string? CAN_NANG { get; set; }
}
