#nullable disable

using HIA;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HIA.Core.Models.HDSD
{
    public partial class DichVuMaMayModifyModel
    {
        public int DVMMID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CSKCB { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string MA_DICH_VU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource)), AllowNull]
        public string MA_MAY { get; set; }
    }
}