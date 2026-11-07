using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMKhoaBVInfoBasicModel : IMapable<DMKhoaBV>
{
    public int Kbvid { get; set; }
    public string? Ma_CSKCB { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string Ma_KHOA { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string Ten_KHOA { get; set; } = string.Empty;
    public string? Tu_NGAY { get; set; }
    public string? Den_NGAY { get; set; }

    // Map cap: DMKhoaBV <-> DMKhoaBVInfoBasicModel
    public void MapFrom(DMKhoaBV? item)
    {
        if (item == null) return;
        Kbvid = item.KBVID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_KHOA = item.MA_KHOA ?? string.Empty;
        Ten_KHOA = item.TEN_KHOA ?? string.Empty;
        Tu_NGAY = item.TU_NGAY;
        Den_NGAY = item.DEN_NGAY;
    }

    public DMKhoaBV? MapTo()
    {
        return new DMKhoaBV
        {
            KBVID = Kbvid,
            MA_CSKCB = Ma_CSKCB ?? string.Empty,
            MA_KHOA = Ma_KHOA,
            TEN_KHOA = Ten_KHOA,
            TU_NGAY = Tu_NGAY,
            DEN_NGAY = Den_NGAY
        };
    }
}
