using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMVTYTModifyModel : IMapable<DMVTYT>
{
    public int VTID { get; set; }

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string MA_CSKCB { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    public string MA_VAT_TU { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    public string NHOM_VAT_TU { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.TEN_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
    public string TEN_VAT_TU { get; set; } = string.Empty;

    [Display(Name = nameof(DisplayFieldResource.MA_HIEU), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_HIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
    public string? QUY_CACH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public string? DON_VI_TINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? DON_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
    public double? DON_GIA_BH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public double? SO_LUONG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NUOC_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NHA_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? NHA_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? LOAI_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? TT_THAU { get; set; }

    public string? HT_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    public string? QUYET_DINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DINH_MUC), ResourceType = typeof(DisplayFieldResource))]
    public int? DINH_MUC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public string? TYLE_TT_BH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public string? MANHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public string? MANHOM_5937 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? TU_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? DEN_NGAY { get; set; }

    public string? NGAY_DE_NGHI { get; set; }

    // Map cap: DMVTYT <-> DMVTYTModifyModel
    public void MapFrom(DMVTYT? item)
    {
        if (item == null) return;
        VTID = item.VTID;
        MA_CSKCB = item.MA_CSKCB;
        MA_VAT_TU = item.MA_VAT_TU ?? string.Empty;
        NHOM_VAT_TU = item.NHOM_VAT_TU ?? string.Empty;
        TEN_VAT_TU = item.TEN_VAT_TU ?? string.Empty;
        MA_HIEU = item.MA_HIEU;
        QUY_CACH = item.QUY_CACH;
        DON_VI_TINH = item.DON_VI_TINH;
        DON_GIA = item.DON_GIA;
        DON_GIA_BH = item.DON_GIA_BH;
        SO_LUONG = item.SO_LUONG;
        NUOC_SX = item.NUOC_SX;
        NHA_SX = item.NHA_SX;
        NHA_THAU = item.NHA_THAU;
        LOAI_THAU = item.LOAI_THAU;
        TT_THAU = item.TT_THAU;
        HT_THAU = item.HT_THAU;
        QUYET_DINH = item.QUYET_DINH;
        DINH_MUC = (int?)item.DINH_MUC;
        TYLE_TT_BH = item.TYLE_TT_BH;
        MANHOM_9324 = item.MANHOM_9324?.ToString();
        MANHOM_5937 = item.MANHOM_5937?.ToString();
        TU_NGAY = item.TU_NGAY;
        DEN_NGAY = item.DEN_NGAY;
        NGAY_DE_NGHI = item.NGAY_DE_NGHI;
    }

    public DMVTYT? MapTo()
    {
        return new DMVTYT
        {
            VTID = VTID,
            MA_CSKCB = MA_CSKCB,
            MA_VAT_TU = MA_VAT_TU,
            NHOM_VAT_TU = NHOM_VAT_TU,
            TEN_VAT_TU = TEN_VAT_TU,
            MA_HIEU = MA_HIEU,
            QUY_CACH = QUY_CACH,
            DON_VI_TINH = DON_VI_TINH,
            DON_GIA = DON_GIA,
            DON_GIA_BH = DON_GIA_BH,
            SO_LUONG = SO_LUONG,
            NUOC_SX = NUOC_SX,
            NHA_SX = NHA_SX,
            NHA_THAU = NHA_THAU,
            LOAI_THAU = LOAI_THAU,
            TT_THAU = TT_THAU,
            HT_THAU = HT_THAU,
            QUYET_DINH = QUYET_DINH,
            DINH_MUC = DINH_MUC,
            TYLE_TT_BH = TYLE_TT_BH,
            MANHOM_9324 = byte.TryParse(MANHOM_9324, out var nhom9324) ? nhom9324 : null,
            MANHOM_5937 = byte.TryParse(MANHOM_5937, out var nhom5937) ? nhom5937 : null,
            TU_NGAY = TU_NGAY,
            DEN_NGAY = DEN_NGAY,
            NGAY_DE_NGHI = NGAY_DE_NGHI
        };
    }
}
