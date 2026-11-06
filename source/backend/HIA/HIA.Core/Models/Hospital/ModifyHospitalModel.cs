using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIA.Core.Models.Hospital;

public class ModifyHospitalModel
{
    public string? Id { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = "ERR_400_REQUIRED_HOSPITAL")]
    public string Name { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.ACTIVE), ResourceType = typeof(DisplayFieldResource))]
    public bool IsActivated { get; set; }
    [Display(Name = nameof(DisplayFieldResource.USERNAME_MINISTRY_HEALTH), ResourceType = typeof(DisplayFieldResource))]
    public string? Username { get; set; }
    [Display(Name = nameof(DisplayFieldResource.PASSWORD_MINISTRY_HEALTH), ResourceType = typeof(DisplayFieldResource))]
    public string? Password { get; set; }
    [Display(Name = nameof(DisplayFieldResource.Address), ResourceType = typeof(DisplayFieldResource))]
    public string? Address { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_CSKCB { get; set; }
    [Display(Name = nameof(DisplayFieldResource.Loai_Tuyen), ResourceType = typeof(DisplayFieldResource))]
    public int Loai_Tuyen { get; set; }
    public int MaxFilePerDate { get; set; }

    public int LevelId { get; set; }

    public string? SuperHospitalId { get; set; }

    public string? OfficeTime { get; set; }

    [NotMapped]
    public bool PredefError { get; set; }

}
