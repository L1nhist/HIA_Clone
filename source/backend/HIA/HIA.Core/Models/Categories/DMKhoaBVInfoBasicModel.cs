using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMKhoaBVInfoBasicModel
{
    public int Kbvid { get; set; }
    public string? Ma_CSKCB { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string Ma_KHOA { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string Ten_KHOA { get; set; } = string.Empty;
    public string? Tu_NGAY { get; set; }
    public string? Den_NGAY { get; set; }
}
