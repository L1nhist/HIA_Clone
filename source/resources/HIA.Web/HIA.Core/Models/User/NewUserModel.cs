using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.User
{
    public class NewUserModel
    {
        public string Id { get; set; }
        public bool IsFirstTimeLogin { get; set; }

        [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
        [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_HOSPITAL))]
        public string HospitalId { get; set; }
        [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
        public string HospitalName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ROLE), ResourceType = typeof(DisplayFieldResource))]
        [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_ROLE))]
        public string RoleId { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ROLE), ResourceType = typeof(DisplayFieldResource))]
        public string RoleName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.EMAIL), ResourceType = typeof(DisplayFieldResource))]
        [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_EMAIL))]
        [RegularExpression(RegexValidation.REGEX_EMAIL, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_EMAIL))]
        public string Email { get; set; }
        public string Password { get; set; }
        [Display(Name = nameof(DisplayFieldResource.FULLNAME), ResourceType = typeof(DisplayFieldResource))]
        [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_FULLNAME))]
        public string FullName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.PHONE_NUMBER), ResourceType = typeof(DisplayFieldResource))]
        public string PhoneNumber { get; set; }

        [Display(Name = nameof(DisplayFieldResource.ACTIVE), ResourceType = typeof(DisplayFieldResource))]
        public bool IsActivated { get; set; }

        //Linhist added 200826
        public bool IsValidated { get; set; }
    }
}
