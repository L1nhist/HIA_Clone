using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Records;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Files;

public class XML3InfoModel : IMapable<XML03>
{
    //Linhist changed 26/12/22
    //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

    public int XML3ID { get; set; }
    public int? XML1ID { get; set; }
    public bool HasError { get; set; }
    public List<string> PropertiesError { get; set; } = new();
    public List<string> ContensError { get; set; } = new();

    [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
    public int? Stt { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_LK { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_DICH_VU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_VAT_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_VAT_TU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_NHOM), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_NHOM { get; set; }
    [Display(Name = nameof(DisplayFieldResource.GOI_VTYT), ResourceType = typeof(DisplayFieldResource))]
    public string? Goi_VTYT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_VAT_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_VAT_TU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_DICH_VU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Don_VI_TINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.PHAM_VI), ResourceType = typeof(DisplayFieldResource))]
    public byte? Pham_VI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public double? SO_LUONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? TT_THAU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public byte? Tyle_TT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THANH_TIEN), ResourceType = typeof(DisplayFieldResource))]
    public double? Thanh_TIEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_TRANTT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_TRANTT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MUC_HUONG), ResourceType = typeof(DisplayFieldResource))]
    public byte? Muc_HUONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_NGUONKHAC), ResourceType = typeof(DisplayFieldResource))]
    public double? T_NGUONKHAC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_BNTT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_BNTT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_BHTT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_BHTT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_BNCCT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_BNCCT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_NGOAIDS), ResourceType = typeof(DisplayFieldResource))]
    public double? T_NGOAIDS { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_KHOA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_GIUONG), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_GIUONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_BAC_SI), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BAC_SI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_YL), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_YL { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_KQ), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_KQ { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_PTTT), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_PTTT { get; set; }

    // Map cap: XML03 <-> XML3InfoModel
    public void MapFrom(XML03? item)
    {
        if (item == null) return;
        Stt = item.STT;
        MA_LK = item.MA_LK;
        MA_DICH_VU = item.MA_DICH_VU;
        MA_VAT_TU = item.MA_VAT_TU;
        MA_NHOM = item.MA_NHOM;
        Goi_VTYT = item.GOI_VTYT;
        Ten_VAT_TU = item.TEN_VAT_TU;
        Ten_DICH_VU = item.TEN_DICH_VU;
        Don_VI_TINH = item.DON_VI_TINH;
        Pham_VI = item.PHAM_VI;
        SO_LUONG = item.SO_LUONG;
        TT_THAU = item.TT_THAU;
        T_TRANTT = item.T_TRANTT;
        Muc_HUONG = (byte?)item.MUC_HUONG;
        T_NGUONKHAC = item.T_NGUONKHAC;
        T_BNTT = item.T_BNTT;
        T_BHTT = item.T_BHTT;
        T_BNCCT = item.T_BNCCT;
        MA_KHOA = item.MA_KHOA;
        MA_GIUONG = item.MA_GIUONG;
        MA_BAC_SI = item.MA_BAC_SI;
        MA_BENH = item.MA_BENH;
        Ngay_YL = item.NGAY_YL?.ToString();
        Ngay_KQ = item.NGAY_KQ?.ToString();
        MA_PTTT = item.MA_PTTT;
    }

    // Map cap: XML03 <-> XML3InfoModel
    public XML03? MapTo()
    {
        return new XML03
        {
            STT = Stt,
            MA_LK = MA_LK,
            MA_DICH_VU = MA_DICH_VU,
            MA_VAT_TU = MA_VAT_TU,
            MA_NHOM = MA_NHOM,
            GOI_VTYT = Goi_VTYT,
            TEN_VAT_TU = Ten_VAT_TU,
            TEN_DICH_VU = Ten_DICH_VU,
            DON_VI_TINH = Don_VI_TINH,
            PHAM_VI = Pham_VI,
            SO_LUONG = SO_LUONG,
            TT_THAU = TT_THAU,
            T_TRANTT = T_TRANTT,
            MUC_HUONG = Muc_HUONG,
            T_NGUONKHAC = T_NGUONKHAC,
            T_BNTT = T_BNTT,
            T_BHTT = T_BHTT,
            T_BNCCT = T_BNCCT,
            MA_KHOA = MA_KHOA,
            MA_GIUONG = MA_GIUONG,
            MA_BAC_SI = MA_BAC_SI,
            MA_BENH = MA_BENH,
            NGAY_YL = MapConvert.ToDate(Ngay_YL),
            NGAY_KQ = MapConvert.ToDate(Ngay_KQ),
            MA_PTTT = MA_PTTT,
        };
    }
}
