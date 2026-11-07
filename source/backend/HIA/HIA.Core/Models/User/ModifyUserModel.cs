using EntityUser = HIA.Data.Auths.User;
using HIA.Data.Base;
using HIA.Core.Base;
using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.User;

public class ModifyUserModel : IMapable<EntityUser>
{
    public string? Id { get; set; }
    public bool IsFirstTimeLogin { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_HOSPITAL))]
    public string HospitalId { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
    public string? HospitalName { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ACTIVE), ResourceType = typeof(DisplayFieldResource))]
    public bool IsActivated { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ROLE), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_ROLE))]
    public string RoleId { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.ROLE), ResourceType = typeof(DisplayFieldResource))]
    public string? RoleName { get; set; }
    [Display(Name = nameof(DisplayFieldResource.EMAIL), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_EMAIL))]
    [RegularExpression(RegexValidation.REGEX_EMAIL, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_EMAIL))]
    public string Email { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.PASSWORD), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_PASSWORD))]
    [RegularExpression(RegexValidation.REGEX_PASSWORD, ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_INVALID_PASSWORD))]
    public string Password { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.FULLNAME), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = nameof(MessageResource.ERR_400_REQUIRED_FULLNAME))]
    public string FullName { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.PHONE_NUMBER), ResourceType = typeof(DisplayFieldResource))]
    public string? PhoneNumber { get; set; }

    // Map cap: User <-> ModifyUserModel
    public void MapFrom(EntityUser? item)
    {
        if (item == null) return;
        Id = item.Id;
        IsFirstTimeLogin = item.IsFirstTimeLogin;
        HospitalId = item.HospitalId ?? string.Empty;
        IsActivated = item.IsActivated;
        Email = item.Email ?? string.Empty;
        Password = item.Password;
        FullName = item.FullName ?? string.Empty;
        PhoneNumber = item.PhoneNumber;
    }

    public EntityUser? MapTo()
    {
        return new EntityUser
        {
            Id = Id ?? string.Empty,
            IsFirstTimeLogin = IsFirstTimeLogin,
            HospitalId = HospitalId,
            IsActivated = IsActivated,
            Email = Email,
            Password = Password,
            FullName = FullName,
            PhoneNumber = PhoneNumber
        };
    }

    // Map cap: ModifyUserModel <-> NewUserModel (ham tuong minh)
    public NewUserModel ToNewUserModel()
    {
        return new NewUserModel
        {
            Id = Id,
            IsFirstTimeLogin = IsFirstTimeLogin,
            HospitalId = HospitalId,
            HospitalName = HospitalName,
            RoleId = RoleId,
            RoleName = RoleName,
            IsActivated = IsActivated,
            Email = Email,
            Password = Password,
            FullName = FullName,
            PhoneNumber = PhoneNumber
        };
    }
}
