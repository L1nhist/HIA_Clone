using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMDichVuModifyModel
{
    public int DVID { get; set; }

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string MA_CSKCB { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string MA_DICH_VU { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string TEN_DICH_VU { get; set; } = string.Empty;

    [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? DON_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    public string? QUYET_DINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public byte? MANHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public byte? MANHOM_5937 { get; set; }
}
