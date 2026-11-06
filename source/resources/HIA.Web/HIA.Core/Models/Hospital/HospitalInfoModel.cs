using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Hospital
{
    public class HospitalInfoModel
    {
        public string Id { get; set; }
        [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
        public string Name { get; set; }
        [Display(Name = nameof(DisplayFieldResource.STATUS), ResourceType = typeof(DisplayFieldResource))]
        public bool IsActivated { get; set; }
        [Display(Name = nameof(DisplayFieldResource.Address), ResourceType = typeof(DisplayFieldResource))]
        public string Address { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CSKCB { get; set; }
        [Display(Name = nameof(DisplayFieldResource.Loai_Tuyen), ResourceType = typeof(DisplayFieldResource))]
        public int Loai_Tuyen { get; set; }
        [Display(Name = nameof(DisplayFieldResource.USERNAME_MINISTRY_HEALTH), ResourceType = typeof(DisplayFieldResource))]
        public string Username { get; set; }
        [Display(Name = nameof(DisplayFieldResource.PASSWORD_MINISTRY_HEALTH), ResourceType = typeof(DisplayFieldResource))]
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }

        public string DateTimeNow { get; set; }

        public int? FileAPICount { get; set; }

        public int? MaxFilePerDate { get; set; }

        public int? LevelId { get; set; }
        
        public string SuperHospitalId { get; set; }

        public string HospitalCons { get; set; }
    }
}
