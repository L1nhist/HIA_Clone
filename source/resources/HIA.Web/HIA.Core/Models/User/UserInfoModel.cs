using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.User
{
    public class UserInfoModel
    {
        public string Id { get; set; }
        public bool IsFirstTimeLogin { get; set; }
        [Display(Name = nameof(DisplayFieldResource.EMAIL), ResourceType = typeof(DisplayFieldResource))]
        public string Email { get; set; }
        [Display(Name = nameof(DisplayFieldResource.FULLNAME), ResourceType = typeof(DisplayFieldResource))]
        public string FullName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.PHONE_NUMBER), ResourceType = typeof(DisplayFieldResource))]
        public string PhoneNumber { get; set; }
        [Display(Name = nameof(DisplayFieldResource.STATUS), ResourceType = typeof(DisplayFieldResource))]
        public string RoleId { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ROLE), ResourceType = typeof(DisplayFieldResource))]
        public string RoleName { get; set; }
        public string HospitalId { get; set; }
        [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
        public string HospitalName { get; set; }
        public bool IsActivated { get; set; }
    }
}
