using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMDichVuInfoModel
    {
        public int DVID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_CSKCB { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_DICH_VU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_DICH_VU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
        public double? Don_GIA { get; set; }

        [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
        public string Quyet_DINH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
        public byte? MaNHOM_9324 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
        public byte? MaNHOM_5937 { get; set; }

        [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string Tu_NGAY { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
        public string Den_NGAY { get; set; }

        //[Display(Name = nameof(DisplayFieldResource.NGAY_DE_NGHI), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_DE_NGHI { get; set; }
    }

    //public class DMDichVuInfoModel
    //{
    //    public int DichvuID { get; set; }
    //    //public int? DichVuMHTCID { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_DVKT), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ma_DVKT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ma_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_DVKT), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ten_DVKT { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.TEN_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ten_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ma_GIA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    //    public double? Don_GIA { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.GIA_AX), ResourceType = typeof(DisplayFieldResource))]
    //    public double? Gia_AX { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Gia_TRAN), ResourceType = typeof(DisplayFieldResource))]
    //    public double? Gia_TRAN { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    //    public string Quyet_DINH { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.CONG_BO), ResourceType = typeof(DisplayFieldResource))]
    //    public string Cong_BO { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ma_CSKCB { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    //    public string Manhom_9324 { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.DVKTC), ResourceType = typeof(DisplayFieldResource))]
    //    public bool Dvktc { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.HIEULUC), ResourceType = typeof(DisplayFieldResource))]
    //    public string Hieuluc { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.KETQUA), ResourceType = typeof(DisplayFieldResource))]
    //    public string Ketqua { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.LYDOTUCHOI), ResourceType = typeof(DisplayFieldResource))]
    //    public string Lydotuchoi { get; set; }
    //    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    //    public string MaNHOM_5937 { get; set; }



        
        
        
        
        
        
    //    public string Stt { get; set; }
    //    public string Ma_DICH_VU { get; set; }
    //    public string Ten_DICH_VU { get; set; }
    //    public string Quy_TRINH { get; set; }
    //    public string Cskcb_CGKT { get; set; }
    //    public string Cskcb_CLS { get; set; }
    //    public string Id { get; set; }
    //}
}
