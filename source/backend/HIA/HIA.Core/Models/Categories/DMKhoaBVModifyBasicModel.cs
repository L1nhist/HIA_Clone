using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMKhoaBVModifyBasicModel : IMapable<DMKhoaBV>
{
    public int KBVID { get; set; }

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string MA_CSKCB { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string MA_KHOA { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string TEN_KHOA { get; set; } = string.Empty;

    public string? TU_NGAY { get; set; }

    public string? DEN_NGAY { get; set; }

    // Map cap: DMKhoaBV <-> DMKhoaBVModifyBasicModel
    public void MapFrom(DMKhoaBV? item)
    {
        if (item == null) return;
        KBVID = item.KBVID;
        MA_CSKCB = item.MA_CSKCB;
        MA_KHOA = item.MA_KHOA ?? string.Empty;
        TEN_KHOA = item.TEN_KHOA ?? string.Empty;
        TU_NGAY = item.TU_NGAY;
        DEN_NGAY = item.DEN_NGAY;
    }

    public DMKhoaBV? MapTo()
    {
        return new DMKhoaBV
        {
            KBVID = KBVID,
            MA_CSKCB = MA_CSKCB,
            MA_KHOA = MA_KHOA,
            TEN_KHOA = TEN_KHOA,
            TU_NGAY = TU_NGAY,
            DEN_NGAY = DEN_NGAY
        };
    }
}
