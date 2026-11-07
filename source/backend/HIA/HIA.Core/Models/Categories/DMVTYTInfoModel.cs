using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMVTYTInfoModel : IMapable<DMVTYT>
{
    public int VTID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_VAT_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    public string? Nhom_VAT_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_VAT_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_HIEU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_HIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
    public string? Quy_CACH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Don_VI_TINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA_BH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public double? So_LUONG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nuoc_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nha_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Nha_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Loai_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Tt_THAU { get; set; }

    public string? Ht_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Quyet_DINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DINH_MUC), ResourceType = typeof(DisplayFieldResource))]
    public double? Dinh_MUC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public string? Tyle_TT_BH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public string? MaNHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public string? MaNHOM_5937 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }

    public string? Ngay_DE_NGHI { get; set; }

    // Map cap: DMVTYT <-> DMVTYTInfoModel
    public void MapFrom(DMVTYT? item)
    {
        if (item == null) return;
        VTID = item.VTID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_VAT_TU = item.MA_VAT_TU;
        Nhom_VAT_TU = item.NHOM_VAT_TU;
        Ten_VAT_TU = item.TEN_VAT_TU;
        Ma_HIEU = item.MA_HIEU;
        Quy_CACH = item.QUY_CACH;
        Don_VI_TINH = item.DON_VI_TINH;
        Don_GIA = item.DON_GIA;
        Don_GIA_BH = item.DON_GIA_BH;
        So_LUONG = item.SO_LUONG;
        Nuoc_SX = item.NUOC_SX;
        Nha_SX = item.NHA_SX;
        Nha_THAU = item.NHA_THAU;
        Loai_THAU = item.LOAI_THAU;
        Tt_THAU = item.TT_THAU;
        Ht_THAU = item.HT_THAU;
        Quyet_DINH = item.QUYET_DINH;
        Dinh_MUC = item.DINH_MUC;
        Tyle_TT_BH = item.TYLE_TT_BH;
        MaNHOM_9324 = item.MANHOM_9324?.ToString();
        MaNHOM_5937 = item.MANHOM_5937?.ToString();
        Tu_NGAY = item.TU_NGAY;
        Den_NGAY = item.DEN_NGAY;
        Ngay_DE_NGHI = item.NGAY_DE_NGHI;
    }

    public DMVTYT? MapTo()
    {
        return new DMVTYT
        {
            VTID = VTID,
            MA_CSKCB = Ma_CSKCB ?? string.Empty,
            MA_VAT_TU = Ma_VAT_TU,
            NHOM_VAT_TU = Nhom_VAT_TU,
            TEN_VAT_TU = Ten_VAT_TU,
            MA_HIEU = Ma_HIEU,
            QUY_CACH = Quy_CACH,
            DON_VI_TINH = Don_VI_TINH,
            DON_GIA = Don_GIA,
            DON_GIA_BH = Don_GIA_BH,
            SO_LUONG = So_LUONG,
            NUOC_SX = Nuoc_SX,
            NHA_SX = Nha_SX,
            NHA_THAU = Nha_THAU,
            LOAI_THAU = Loai_THAU,
            TT_THAU = Tt_THAU,
            HT_THAU = Ht_THAU,
            QUYET_DINH = Quyet_DINH,
            DINH_MUC = Dinh_MUC,
            TYLE_TT_BH = Tyle_TT_BH,
            MANHOM_9324 = byte.TryParse(MaNHOM_9324, out var nhom9324) ? nhom9324 : null,
            MANHOM_5937 = byte.TryParse(MaNHOM_5937, out var nhom5937) ? nhom5937 : null,
            TU_NGAY = Tu_NGAY,
            DEN_NGAY = Den_NGAY,
            NGAY_DE_NGHI = Ngay_DE_NGHI
        };
    }
}
