#nullable disable

using HIA;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD
{
    public partial class ChiDinhDongThoiInfoModel
    {
        public int CddtID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_CSKCB { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string Ma_DICH_VU { get; set; }

        [Display(Name = nameof(DisplayFieldResource.DICH_VU_KHAC), ResourceType = typeof(DisplayFieldResource))]
        public string Dich_VU_KHAC { get; set; }

        public string Vat_TU_BHYT { get; set; }

        public string Vat_TU_KO_BH { get; set; }

        [Display(Name = nameof(DisplayFieldResource.CHI_DINH), ResourceType = typeof(DisplayFieldResource))]
        public bool? Chi_DINH { get; set; }

        public int? Cung_LOAI { get; set; }

        [Display(Name = nameof(DisplayFieldResource.XEErrorCode), ResourceType = typeof(DisplayFieldResource))]
        public int? Ma_LOI { get; set; }

        [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
        public string Noi_DUNG_LOI { get; set; }

        [Display(Name = nameof(DisplayFieldResource.HIEULUC), ResourceType = typeof(DisplayFieldResource))]
        public bool? Hieu_LUC { get; set; }
    }
}