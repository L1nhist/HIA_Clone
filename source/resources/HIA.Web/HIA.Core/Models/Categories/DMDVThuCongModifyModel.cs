using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMDVThuCongModifyModel
    {
        public int DVTCID { get; set; }
        
        [Required]
        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CSKCB { get; set; }
        
        [Required]
        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string MA_DICH_VU { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string TEN_DICH_VU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
        public string MA_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
        public double? DON_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
        public string QUYET_DINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
        public byte? MANHOM_9324 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
        public byte? MANHOM_5937 { get; set; }
    }

    //public class DMDichVuModifyModel
    //{
    //    public int DichVuID { get; set; }
    //    //public int? DichVuMHTCID { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_DVKT), ResourceType = typeof(DisplayFieldResource))]
    //    [Required]
    //    public string MA_DVKT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_DVKT), ResourceType = typeof(DisplayFieldResource))]
    //    [Required]
    //    public string Ten_DVKT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string TEN_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_GIA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    //    public double? DON_GIA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.GIA_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public double? GIA_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Gia_TRAN), ResourceType = typeof(DisplayFieldResource))]
    //    public double? GIA_TRAN { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    //    public string QUYET_DINH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.CONG_BO), ResourceType = typeof(DisplayFieldResource))]
    //    public string CONG_BO { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    //    public string MA_CSKCB { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    //    public string MANHOM_9324 { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DVKTC), ResourceType = typeof(DisplayFieldResource))]
    //    public bool DVKTC { get; set; }
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
