using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMBacSiInfoModel
{
    public int BSID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }
    public string? Ma_BHXH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.HO_TEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ho_TEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.GIOI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public byte? Gioi_TINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_KHOA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_KHOA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CHUCDANH), ResourceType = typeof(DisplayFieldResource))]
    public string? Chuc_DANH { get; set; }
    public string? Chuc_DANH_NN { get; set; }
    public string? Ma_VI_TRI { get; set; }
    public string? Vi_TRI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MACCHN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CCHN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAYCAP_CCHN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngaycap_CCHN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NOICAP_CCHN), ResourceType = typeof(DisplayFieldResource))]
    public string? Noicap_CCHN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.PHAM_VI), ResourceType = typeof(DisplayFieldResource))]
    public string? Phamvi_CM { get; set; }
    public string? Phamvi_CMBS { get; set; }
    public string? Dvkt_KHAC { get; set; }
    public string? Vb_PHANCONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOIGIAN_DK), ResourceType = typeof(DisplayFieldResource))]
    public string? Tg_DANGKY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOIGIAN_NGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tg_NGAY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOIGIAN_TUAN), ResourceType = typeof(DisplayFieldResource))]
    public string? Tg_TUAN { get; set; }
    public string? Cskcb_KHAC { get; set; }
    public string? Cskcb_CGKT { get; set; }
    public string? Qd_CGKT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }
}
