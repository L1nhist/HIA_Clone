using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMThuocModifyModel : IMapable<DMThuoc>
{
    public int THID { get; set; }

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string MA_CSKCB { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string MA_THUOC { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.TEN_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string TEN_THUOC { get; set; } = string.Empty;

    [Display(Name = nameof(DisplayFieldResource.Hoat_CHAT), ResourceType = typeof(DisplayFieldResource))]
    public string? TEN_HOAT_CHAT { get; set; }

    [Display(Name = nameof(DisplayFieldResource.LOAI_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? LOAI_THUOC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_DUONG_DUNG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? DUONG_DUNG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HAM_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public string? HAM_LUONG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DANG_BAO_CHE), ResourceType = typeof(DisplayFieldResource))]
    public string? DANG_BAO_CHE { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public string? DON_VI_TINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? DON_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
    public double? DON_GIA_BH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public double? SO_LUONG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
    public string? QUY_CACH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_DANG_KY), ResourceType = typeof(DisplayFieldResource))]
    public string? SO_DANG_KY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DVKT), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_DVKT { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NUOC_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NHA_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? NHA_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NHOM_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? NHOM_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? LOAI_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? TT_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? HT_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public byte? MANHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public byte? MANHOM_5937 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? TU_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? DEN_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NGAY_DE_NGHI), ResourceType = typeof(DisplayFieldResource))]
    public string? NGAY_DE_NGHI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.PP_CHE_BIEN), ResourceType = typeof(DisplayFieldResource))]
    public string? PP_CHE_BIEN { get; set; }

    // Map cap: DMThuoc <-> DMThuocModifyModel
    public void MapFrom(DMThuoc? item)
    {
        if (item == null) return;
        THID = item.THID;
        MA_CSKCB = item.MA_CSKCB;
        MA_THUOC = item.MA_THUOC ?? string.Empty;
        TEN_THUOC = item.TEN_THUOC ?? string.Empty;
        TEN_HOAT_CHAT = item.TEN_HOAT_CHAT;
        LOAI_THUOC = item.LOAI_THUOC;
        MA_DUONG_DUNG = item.MA_DUONG_DUNG;
        DUONG_DUNG = item.DUONG_DUNG;
        HAM_LUONG = item.HAM_LUONG;
        DANG_BAO_CHE = item.DANG_BAO_CHE;
        DON_VI_TINH = item.DON_VI_TINH;
        DON_GIA = item.DON_GIA;
        DON_GIA_BH = item.DON_GIA_BH;
        SO_LUONG = item.SO_LUONG;
        QUY_CACH = item.QUY_CACH;
        SO_DANG_KY = item.SO_DANG_KY;
        MA_DVKT = item.MA_DVKT;
        NUOC_SX = item.NUOC_SX;
        NHA_SX = item.NHA_SX;
        NHA_THAU = item.NHA_THAU;
        NHOM_THAU = item.NHOM_THAU;
        LOAI_THAU = item.LOAI_THAU;
        TT_THAU = item.TT_THAU;
        HT_THAU = item.HT_THAU;
        MANHOM_9324 = item.MANHOM_9324;
        MANHOM_5937 = item.MANHOM_5937;
        TU_NGAY = item.TU_NGAY;
        DEN_NGAY = item.DEN_NGAY;
        NGAY_DE_NGHI = item.NGAY_DE_NGHI;
        PP_CHE_BIEN = item.PP_CHEBIEN;
    }

    public DMThuoc? MapTo()
    {
        return new DMThuoc
        {
            THID = THID,
            MA_CSKCB = MA_CSKCB,
            MA_THUOC = MA_THUOC,
            TEN_THUOC = TEN_THUOC,
            TEN_HOAT_CHAT = TEN_HOAT_CHAT,
            LOAI_THUOC = LOAI_THUOC,
            MA_DUONG_DUNG = MA_DUONG_DUNG,
            DUONG_DUNG = DUONG_DUNG,
            HAM_LUONG = HAM_LUONG,
            DANG_BAO_CHE = DANG_BAO_CHE,
            DON_VI_TINH = DON_VI_TINH,
            DON_GIA = DON_GIA,
            DON_GIA_BH = DON_GIA_BH,
            SO_LUONG = SO_LUONG,
            QUY_CACH = QUY_CACH,
            SO_DANG_KY = SO_DANG_KY,
            MA_DVKT = MA_DVKT,
            NUOC_SX = NUOC_SX,
            NHA_SX = NHA_SX,
            NHA_THAU = NHA_THAU,
            NHOM_THAU = NHOM_THAU,
            LOAI_THAU = LOAI_THAU,
            TT_THAU = TT_THAU,
            HT_THAU = HT_THAU,
            MANHOM_9324 = MANHOM_9324,
            MANHOM_5937 = MANHOM_5937,
            TU_NGAY = TU_NGAY,
            DEN_NGAY = DEN_NGAY,
            NGAY_DE_NGHI = NGAY_DE_NGHI,
            PP_CHEBIEN = PP_CHE_BIEN
        };
    }
}
