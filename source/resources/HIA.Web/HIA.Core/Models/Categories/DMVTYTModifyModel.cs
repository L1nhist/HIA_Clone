using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMVTYTModifyModel
    {
        public int VTID { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CSKCB { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.MA_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
        public string MA_VAT_TU { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
        public string NHOM_VAT_TU { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.TEN_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
        public string TEN_VAT_TU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_HIEU), ResourceType = typeof(DisplayFieldResource))]
        public string MA_HIEU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
        public string QUY_CACH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
        public string DON_VI_TINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
        public double? DON_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA_BH), ResourceType = typeof(DisplayFieldResource))]
        public double? DON_GIA_BH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
        public double? SO_LUONG { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
        public string NUOC_SX { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
        public string NHA_SX { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string NHA_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string LOAI_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string TT_THAU { get; set; }

        //[Display(Name = nameof(DisplayFieldResource.HT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string HT_THAU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
        public string QUYET_DINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DINH_MUC), ResourceType = typeof(DisplayFieldResource))]
        public int? DINH_MUC { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
        public string TYLE_TT_BH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
        public string MANHOM_9324 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
        public string MANHOM_5937 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string TU_NGAY { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string DEN_NGAY { get; set; }

        //[Display(Name = nameof(DisplayFieldResource.NGAY_DE_NGHI), ResourceType = typeof(DisplayFieldResource))]
        public string NGAY_DE_NGHI { get; set; }
    }

    //public class DMVTYTModifyModel
    //{
    //    public int VTYTID { get; set; }
    //    //public int? VTYTMHTCID { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_NHOM_VTYT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_NHOM_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_NHOM_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_VTYT), ResourceType = typeof(DisplayFieldResource))]
    //    public string TEN_NHOM_VTYT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_NHOM_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string TEN_NHOM_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_HIEU), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_HIEU { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
    //    [Required]
    //    public string MA_VTYT_BV { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_VTYT_BV), ResourceType = typeof(DisplayFieldResource))]
    //    [Required]
    //    public string Ten_VTYT_BV { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.QUY_CACH), ResourceType = typeof(DisplayFieldResource))]
    //    public string QUY_CACH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    //    public string NUOC_SX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Hang_SX), ResourceType = typeof(DisplayFieldResource))]
    //    public string HANG_SX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
    //    public string DON_VI_TINH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    //    public double? DON_GIA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Don_GIA_TT), ResourceType = typeof(DisplayFieldResource))]
    //    public double? DON_GIA_TT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Gia_TRAN), ResourceType = typeof(DisplayFieldResource))]
    //    public double? GIA_TRAN { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Nha_THAU), ResourceType = typeof(DisplayFieldResource))]
    //    public string NHA_THAU { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    //    public string QUYET_DINH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.CONG_BO), ResourceType = typeof(DisplayFieldResource))]
    //    public string CONG_BO { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DINH_MUC), ResourceType = typeof(DisplayFieldResource))]
    //    public int? DINH_MUC { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
    //    public double? SO_LUONG { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_CSKCB { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.LOAI_THAU), ResourceType = typeof(DisplayFieldResource))]
    //    public string LOAI_THAU { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    //    public string MANHOM_9324 { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.HIEULUC), ResourceType = typeof(DisplayFieldResource))]
    //    public string HIEULUC { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.KETQUA), ResourceType = typeof(DisplayFieldResource))]
    //    public string KETQUA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.LYDOTUCHOI), ResourceType = typeof(DisplayFieldResource))]
    //    public string LYDOTUCHOI { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    //    public string MaNHOM_5937 { get; set; }
    //}
}