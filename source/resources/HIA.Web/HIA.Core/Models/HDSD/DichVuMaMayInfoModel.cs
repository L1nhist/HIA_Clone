#nullable disable

using HIA;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD
{
    public partial class DichVuMaMayInfoModel
    {
        public int DvmmID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_CSKCB { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_DICH_VU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_MAY { get; set; }
    }
}