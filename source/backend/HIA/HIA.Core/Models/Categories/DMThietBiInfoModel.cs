using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMThietBiInfoModel
{
    public int TBID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_MAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.KY_HIEU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ky_HIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_THIET_BI), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_THIET_BI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CONGTY_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Congty_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nuoc_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nam_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SD), ResourceType = typeof(DisplayFieldResource))]
    public string? Nam_SD { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUU_HANH), ResourceType = typeof(DisplayFieldResource))]
    public string? So_LUU_HANH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? Hd_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_DEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Hd_DEN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }
}
