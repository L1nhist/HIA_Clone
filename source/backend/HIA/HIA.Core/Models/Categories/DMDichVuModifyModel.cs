using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMDichVuModifyModel : IMapable<DMDichVu>
{
    public int DVID { get; set; }

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string MA_CSKCB { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string MA_DICH_VU { get; set; } = string.Empty;

    [Required]
    [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string TEN_DICH_VU { get; set; } = string.Empty;

    [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? DON_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    public string? QUYET_DINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public byte? MANHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public byte? MANHOM_5937 { get; set; }

    // Map cap: DMDichVu <-> DMDichVuModifyModel
    public void MapFrom(DMDichVu? item)
    {
        if (item == null) return;
        DVID = item.DVID;
        MA_CSKCB = item.MA_CSKCB;
        MA_DICH_VU = item.MA_DICH_VU ?? string.Empty;
        TEN_DICH_VU = item.TEN_DICH_VU ?? string.Empty;
        MA_GIA = item.MA_GIA;
        DON_GIA = item.DON_GIA;
        QUYET_DINH = item.QUYET_DINH;
        MANHOM_9324 = item.MANHOM_9324;
        MANHOM_5937 = item.MANHOM_5937;
    }

    public DMDichVu? MapTo()
    {
        return new DMDichVu
        {
            DVID = DVID,
            MA_CSKCB = MA_CSKCB,
            MA_DICH_VU = MA_DICH_VU,
            TEN_DICH_VU = TEN_DICH_VU,
            MA_GIA = MA_GIA,
            DON_GIA = DON_GIA,
            QUYET_DINH = QUYET_DINH,
            MANHOM_9324 = MANHOM_9324,
            MANHOM_5937 = MANHOM_5937
        };
    }
}
