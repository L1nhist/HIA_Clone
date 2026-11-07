using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Records;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Files;

public class XML2InfoModel : IMapable<XML02>
{
    //Linhist changed 26/12/22
    //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

    public int XML2ID { get; set; }
    public int? XML1ID { get; set; }
    public bool HasError { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
    public int? Stt { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_LK { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_THUOC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_NHOM), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_NHOM { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_THUOC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Don_VI_TINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.HAM_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public string? Ham_LUONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? Duong_DUNG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.LIEU_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? Lieu_DUNG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.SO_DANG_KY), ResourceType = typeof(DisplayFieldResource))]
    public string? SO_DANG_KY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? TT_THAU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.PHAM_VI), ResourceType = typeof(DisplayFieldResource))]
    public byte? Pham_VI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public byte? Tyle_TT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public double? SO_LUONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THANH_TIEN), ResourceType = typeof(DisplayFieldResource))]
    public double? Thanh_TIEN { get; set; }
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
    [Display(Name = nameof(DisplayFieldResource.MA_BAC_SI), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BAC_SI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_YL), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_YL { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_PTTT), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_PTTT { get; set; }

    // Map cap: XML02 <-> XML2InfoModel
    public void MapFrom(XML02? item)
    {
        if (item == null) return;
        Stt = item.STT;
        MA_LK = item.MA_LK;
        MA_THUOC = item.MA_THUOC;
        MA_NHOM = item.MA_NHOM;
        Ten_THUOC = item.TEN_THUOC;
        Don_VI_TINH = item.DON_VI_TINH;
        Ham_LUONG = item.HAM_LUONG;
        Duong_DUNG = item.DUONG_DUNG;
        Lieu_DUNG = item.LIEU_DUNG;
        SO_DANG_KY = item.SO_DANG_KY;
        TT_THAU = item.TT_THAU;
        Pham_VI = item.PHAM_VI;
        SO_LUONG = item.SO_LUONG;
        Don_GIA = item.DON_GIA;
        Muc_HUONG = (byte?)item.MUC_HUONG;
        T_NGUONKHAC = item.T_NGUONKHAC;
        T_BNTT = item.T_BNTT;
        T_BHTT = item.T_BHTT;
        T_BNCCT = item.T_BNCCT;
        MA_KHOA = item.MA_KHOA;
        MA_BAC_SI = item.MA_BAC_SI;
        Ngay_YL = item.NGAY_YL?.ToString();
        MA_PTTT = item.MA_PTTT;
    }

    // Map cap: XML02 <-> XML2InfoModel
    public XML02? MapTo()
    {
        return new XML02
        {
            STT = Stt,
            MA_LK = MA_LK,
            MA_THUOC = MA_THUOC,
            MA_NHOM = MA_NHOM,
            TEN_THUOC = Ten_THUOC,
            DON_VI_TINH = Don_VI_TINH,
            HAM_LUONG = Ham_LUONG,
            DUONG_DUNG = Duong_DUNG,
            LIEU_DUNG = Lieu_DUNG,
            SO_DANG_KY = SO_DANG_KY,
            TT_THAU = TT_THAU,
            PHAM_VI = Pham_VI,
            SO_LUONG = SO_LUONG,
            DON_GIA = Don_GIA,
            MUC_HUONG = Muc_HUONG,
            T_NGUONKHAC = T_NGUONKHAC,
            T_BNTT = T_BNTT,
            T_BHTT = T_BHTT,
            T_BNCCT = T_BNCCT,
            MA_KHOA = MA_KHOA,
            MA_BAC_SI = MA_BAC_SI,
            NGAY_YL = MapConvert.ToDate(Ngay_YL),
            MA_PTTT = MA_PTTT,
        };
    }
}
