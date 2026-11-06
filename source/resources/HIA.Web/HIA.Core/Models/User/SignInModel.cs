using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.User
{
    public class SignInModel
    {
        [Display(Name = nameof(DisplayFieldResource.EMAIL), ResourceType = typeof(DisplayFieldResource))]
        [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_EMAIL))]
        [RegularExpression(RegexValidation.REGEX_EMAIL, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_EMAIL))]
        public string Email { get; set; }
        [Display(Name = nameof(DisplayFieldResource.PASSWORD), ResourceType = typeof(DisplayFieldResource))]
        [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_PASSWORD))]
        [RegularExpression(RegexValidation.REGEX_PASSWORD, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_PASSWORD))]
        public string Password { get; set; }
    }
}
