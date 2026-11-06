using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMErrorInfoModel
{
    public int ErrorID { get; set; }
    public bool IsSelected { get; set; }

    [Display(Name = nameof(DisplayFieldResource.XEErrorCode), ResourceType = typeof(DisplayFieldResource))]
    public int? ErrorCode { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorType), ResourceType = typeof(DisplayFieldResource))]
    public int? ErrorType { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorAtXMLFile), ResourceType = typeof(DisplayFieldResource))]
    public string? ErrorAtXMLFile { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorAtXMLTag), ResourceType = typeof(DisplayFieldResource))]
    public string? ErrorAtXMLTag { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
    public string? ErrorContent { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorComment), ResourceType = typeof(DisplayFieldResource))]
    public string? ErrorComment { get; set; }

    public bool IsAlarm { get; set; }

    public bool IsInternal { get; set; }
}
