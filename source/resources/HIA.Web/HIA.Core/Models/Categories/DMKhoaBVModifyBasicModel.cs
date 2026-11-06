using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMKhoaBVModifyBasicModel
    {
        public int KBVID { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CSKCB { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string MA_KHOA { get; set; }

        [Required]
        [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string TEN_KHOA { get; set; }

        public string TU_NGAY { get; set; }

        public string DEN_NGAY { get; set; }
    }
}
