using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMThuocInfoModel : IMapable<DMThuoc>
{
    public int THID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_THUOC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_THUOC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Hoat_CHAT), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_HOAT_CHAT { get; set; }

    [Display(Name = nameof(DisplayFieldResource.LOAI_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? Loai_THUOC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_DUONG_DUNG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? Duong_DUNG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HAM_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public string? Ham_LUONG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DANG_BAO_CHE), ResourceType = typeof(DisplayFieldResource))]
    public string? Dang_BAO_CHE { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Don_VI_TINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA_BH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    public double? So_LUONG { get; set; }

    [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
    public string? Quy_CACH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_DANG_KY), ResourceType = typeof(DisplayFieldResource))]
    public string? So_DANG_KY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DVKT), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_DVKT { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nuoc_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nha_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Nha_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NHOM_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Nhom_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Loai_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Tt_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HT_THAU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ht_THAU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public byte? MaNHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public byte? MaNHOM_5937 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NGAY_DE_NGHI), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_DE_NGHI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.PP_CHE_BIEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Pp_CHE_BIEN { get; set; }

    // Map cap: DMThuoc <-> DMThuocInfoModel
    public void MapFrom(DMThuoc? item)
    {
        if (item == null) return;
        THID = item.THID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_THUOC = item.MA_THUOC;
        Ten_THUOC = item.TEN_THUOC;
        Ten_HOAT_CHAT = item.TEN_HOAT_CHAT;
        Loai_THUOC = item.LOAI_THUOC;
        Ma_DUONG_DUNG = item.MA_DUONG_DUNG;
        Duong_DUNG = item.DUONG_DUNG;
        Ham_LUONG = item.HAM_LUONG;
        Dang_BAO_CHE = item.DANG_BAO_CHE;
        Don_VI_TINH = item.DON_VI_TINH;
        Don_GIA = item.DON_GIA;
        Don_GIA_BH = item.DON_GIA_BH;
        So_LUONG = item.SO_LUONG;
        Quy_CACH = item.QUY_CACH;
        So_DANG_KY = item.SO_DANG_KY;
        Ma_DVKT = item.MA_DVKT;
        Nuoc_SX = item.NUOC_SX;
        Nha_SX = item.NHA_SX;
        Nha_THAU = item.NHA_THAU;
        Nhom_THAU = item.NHOM_THAU;
        Loai_THAU = item.LOAI_THAU;
        Tt_THAU = item.TT_THAU;
        Ht_THAU = item.HT_THAU;
        MaNHOM_9324 = item.MANHOM_9324;
        MaNHOM_5937 = item.MANHOM_5937;
        Tu_NGAY = item.TU_NGAY;
        Den_NGAY = item.DEN_NGAY;
        Ngay_DE_NGHI = item.NGAY_DE_NGHI;
        Pp_CHE_BIEN = item.PP_CHEBIEN;
    }

    public DMThuoc? MapTo()
    {
        return new DMThuoc
        {
            THID = THID,
            MA_CSKCB = Ma_CSKCB ?? string.Empty,
            MA_THUOC = Ma_THUOC,
            TEN_THUOC = Ten_THUOC,
            TEN_HOAT_CHAT = Ten_HOAT_CHAT,
            LOAI_THUOC = Loai_THUOC,
            MA_DUONG_DUNG = Ma_DUONG_DUNG,
            DUONG_DUNG = Duong_DUNG,
            HAM_LUONG = Ham_LUONG,
            DANG_BAO_CHE = Dang_BAO_CHE,
            DON_VI_TINH = Don_VI_TINH,
            DON_GIA = Don_GIA,
            DON_GIA_BH = Don_GIA_BH,
            SO_LUONG = So_LUONG,
            QUY_CACH = Quy_CACH,
            SO_DANG_KY = So_DANG_KY,
            MA_DVKT = Ma_DVKT,
            NUOC_SX = Nuoc_SX,
            NHA_SX = Nha_SX,
            NHA_THAU = Nha_THAU,
            NHOM_THAU = Nhom_THAU,
            LOAI_THAU = Loai_THAU,
            TT_THAU = Tt_THAU,
            HT_THAU = Ht_THAU,
            MANHOM_9324 = MaNHOM_9324,
            MANHOM_5937 = MaNHOM_5937,
            TU_NGAY = Tu_NGAY,
            DEN_NGAY = Den_NGAY,
            NGAY_DE_NGHI = Ngay_DE_NGHI,
            PP_CHEBIEN = Pp_CHE_BIEN
        };
    }
}
