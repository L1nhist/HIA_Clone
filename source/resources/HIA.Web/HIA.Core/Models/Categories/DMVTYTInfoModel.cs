using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMVTYTInfoModel
    {
        public int VTID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_CSKCB { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_VAT_TU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
        public string Nhom_VAT_TU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TEN_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_VAT_TU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_HIEU), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_HIEU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
        public string Quy_CACH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
        public string Don_VI_TINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
        public double? Don_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
        public double? Don_GIA_BH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
        public double? So_LUONG { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
        public string Nuoc_SX { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
        public string Nha_SX { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Nha_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Loai_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Tt_THAU { get; set; }

        //[Display(Name = nameof(DisplayFieldResource.HT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string Ht_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
        public string Quyet_DINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DINH_MUC), ResourceType = typeof(DisplayFieldResource))]
        public int? Dinh_MUC { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
        public string Tyle_TT_BH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
        public string MaNHOM_9324 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
        public string MaNHOM_5937 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string Tu_NGAY { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string Den_NGAY { get; set; }

        //[Display(Name = nameof(DisplayFieldResource.NGAY_DE_NGHI), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_DE_NGHI { get; set; }
    }

    //public class DMVTYTInfoModel
    //{
    //    public int Vtytid { get; set; }
    //    //public int? Vtytmhtcid { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_NHOM_VTYT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_NHOM_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_NHOM_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ten_NHOM_VTYT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ten_NHOM_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_HIEU), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_HIEU { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_VTYT_BV { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ten_VTYT_BV { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
    //    public string Quy_CACH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    //    public string Nuoc_SX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
    //    public string Hang_SX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    //    public string Don_VI_TINH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    //    public double? Don_GIA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Don_GIA_TT), ResourceType = typeof(DisplayFieldResource))]
    //    public double? Don_GIA_TT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Gia_TRAN), ResourceType = typeof(DisplayFieldResource))]
    //    public double? Gia_TRAN { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
    //    public string Nha_THAU { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    //    public string Quyet_DINH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.CONG_BO), ResourceType = typeof(DisplayFieldResource))]
    //    public string Cong_BO { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DINH_MUC), ResourceType = typeof(DisplayFieldResource))]
    //    public int? Dinh_MUC { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    //    public double? SO_LUONG { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_CSKCB { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
    //    public string Loai_THAU { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    //    public string MaNHOM_9324 { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.HIEULUC), ResourceType = typeof(DisplayFieldResource))]
    //    public string HieuLuc { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.KETQUA), ResourceType = typeof(DisplayFieldResource))]
    //    public string KetQua { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.LYDOTUCHOI), ResourceType = typeof(DisplayFieldResource))]
    //    public string LyDoTuChoi { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    //    public string MaNHOM_5937 { get; set; }


    //    public string Stt { get; set; }
    //    public string Ma_VAT_TU { get; set; }
    //    public string Nhom_VAT_TU { get; set; }
    //    public string Ten_VAT_TU { get; set; }
    //    public string Don_GIA_BH { get; set; }
    //    public string Tyle_TT_BH { get; set; }
    //    public string Tt_THAU { get; set; }
    //    public string Tu_NGAY { get; set; }
    //    public string Den_NGAY { get; set; }
    //    public string Ht_THAU { get; set; }
    //    public string Id { get; set; }
    //}
}