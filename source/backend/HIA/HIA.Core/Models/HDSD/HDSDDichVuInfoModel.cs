using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD;

public partial class HDSDDichVuInfoModel
{
    public int Id { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_DICH_VU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_DICH_VU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CHI_DINH), ResourceType = typeof(DisplayFieldResource))]
    public bool? Chi_DINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DO_TUOI), ResourceType = typeof(DisplayFieldResource))]
    public string? Do_TUOI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CO_THAI), ResourceType = typeof(DisplayFieldResource))]
    public bool? Co_THAI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CHO_CON_BU), ResourceType = typeof(DisplayFieldResource))]
    public bool? Cho_CON_BU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_BENH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.THUOC_KHAC), ResourceType = typeof(DisplayFieldResource))]
    public string? Thuoc_KHAC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_NHOM), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_NHOM { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DICH_VU_KHAC), ResourceType = typeof(DisplayFieldResource))]
    public string? Dich_VU_KHAC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TIEN_SU_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Tien_SU_BENH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public string? Tyle_TT { get; set; }

    [Display(Name = nameof(DisplayFieldResource.THOI_GIAN_TOI_THIEU), ResourceType = typeof(DisplayFieldResource))]
    public double? Thoi_GIAN_TOI_THIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.THOI_GIAN_THUC_HIEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Thoi_GIAN_THUC_HIEN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
    public string? Noi_DUNG_LOI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HIEULUC), ResourceType = typeof(DisplayFieldResource))]
    public bool? Hieu_LUC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_LOAI_KCB), ResourceType = typeof(DisplayFieldResource))]
    public int? Ma_LOAI_KCB { get; set; }

    public int? So_NGUOI_TOI_THIEU { get; set; }

    public string? Ma_DICH_VU_KO_CHECK { get; set; }
}
