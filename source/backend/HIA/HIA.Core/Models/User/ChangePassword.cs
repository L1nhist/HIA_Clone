using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.User;

public class ChangePassword
{
    [Display(Name = nameof(DisplayFieldResource.CURRENT_PASSWORD), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_CURRENT_PASSWORD))]
    [RegularExpression(RegexValidation.REGEX_PASSWORD, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_PASSWORD))]
    public string CurrentPassword { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.NEW_PASSWORD), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_NEW_PASSWORD ))]
    [RegularExpression(RegexValidation.REGEX_PASSWORD, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_PASSWORD))]
    public string NewPassword { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.CONFIRM_PASSWORD), ResourceType = typeof(DisplayFieldResource))]
    [Compare(nameof(NewPassword), ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_NOT_MATCH_NEW_PASSWORD))]
    public string? ConfirmPassword { get; set; }
}
