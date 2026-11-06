using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMThuocInfoModel
    {
        public int THID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_CSKCB { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_THUOC), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_THUOC { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TEN_THUOC), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_THUOC { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Hoat_CHAT), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_HOAT_CHAT { get; set; }

        [Display(Name = nameof(DisplayFieldResource.LOAI_THUOC), ResourceType = typeof(DisplayFieldResource))]
        public string Loai_THUOC { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_DUONG_DUNG { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
        public string Duong_DUNG { get; set; }

        [Display(Name = nameof(DisplayFieldResource.HAM_LUONG), ResourceType = typeof(DisplayFieldResource))]
        public string Ham_LUONG { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DANG_BAO_CHE), ResourceType = typeof(DisplayFieldResource))]
        public string Dang_BAO_CHE { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
        public string Don_VI_TINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
        public double? Don_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
        public double? Don_GIA_BH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
        public double? So_LUONG { get; set; }

        [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
        public string Quy_CACH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.SO_DANG_KY), ResourceType = typeof(DisplayFieldResource))]
        public string So_DANG_KY { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_DVKT), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_DVKT { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
        public string Nuoc_SX { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
        public string Nha_SX { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Nha_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.NHOM_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Nhom_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Loai_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Tt_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.HT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Ht_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
        public byte? MaNHOM_9324 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
        public byte? MaNHOM_5937 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string Tu_NGAY { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string Den_NGAY { get; set; }

        [Display(Name = nameof(DisplayFieldResource.NGAY_DE_NGHI), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_DE_NGHI { get; set; }

        [Display(Name = nameof(DisplayFieldResource.PP_CHE_BIEN), ResourceType = typeof(DisplayFieldResource))]
        public string Pp_CHE_BIEN { get; set; }
    }
}
