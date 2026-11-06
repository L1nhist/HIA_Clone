using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories
{
    public class DMKhoaInfoModel
    {
        public int KhoaID { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string KhoaName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string KhoaCode { get; set; }
    }

    public class DMKhoaBVInfoModel
    {
        public int KhoaID { get; set; }
        public bool IsSelected { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string KhoaName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string KhoaCode { get; set; }
    }
}
