using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMThietBiModifyModel
{
    public int TBID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_MAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.KY_HIEU), ResourceType = typeof(DisplayFieldResource))]
    public string? KY_HIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_THIET_BI), ResourceType = typeof(DisplayFieldResource))]
    public string? TEN_THIET_BI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CONGTY_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? CONGTY_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NUOC_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NAM_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SD), ResourceType = typeof(DisplayFieldResource))]
    public string? NAM_SD { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUU_HANH), ResourceType = typeof(DisplayFieldResource))]
    public string? SO_LUU_HANH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? HD_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_DEN), ResourceType = typeof(DisplayFieldResource))]
    public string? HD_DEN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? TU_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? DEN_NGAY { get; set; }
}
