using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMKhoaModifyModel
{
    public int KhoaID { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string KhoaName { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string KhoaCode { get; set; } = string.Empty;
}

public class DMKhoaBVModifyModel
{
    public List<DMKhoaModifyModel> Data { get; set; } = new();
}
