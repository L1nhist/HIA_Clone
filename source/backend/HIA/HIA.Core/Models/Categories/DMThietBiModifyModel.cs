using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMThietBiModifyModel : IMapable<DMThietBi>
{
    public int TBID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_MAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.KY_HIEU), ResourceType = typeof(DisplayFieldResource))]
    public string? KY_HIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_THIET_BI), ResourceType = typeof(DisplayFieldResource))]
    public string? TEN_THIET_BI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CONGTY_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? CONGTY_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NUOC_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? NAM_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SD), ResourceType = typeof(DisplayFieldResource))]
    public string? NAM_SD { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUU_HANH), ResourceType = typeof(DisplayFieldResource))]
    public string? SO_LUU_HANH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? HD_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_DEN), ResourceType = typeof(DisplayFieldResource))]
    public string? HD_DEN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? TU_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? DEN_NGAY { get; set; }

    // Map cap: DMThietBi <-> DMThietBiModifyModel
    public void MapFrom(DMThietBi? item)
    {
        if (item == null) return;
        TBID = item.TBID;
        MA_CSKCB = item.MA_CSKCB;
        MA_MAY = item.MA_MAY;
        KY_HIEU = item.KY_HIEU;
        TEN_THIET_BI = item.TEN_THIET_BI;
        CONGTY_SX = item.CONGTY_SX;
        NUOC_SX = item.NUOC_SX;
        NAM_SX = item.NAM_SX;
        NAM_SD = item.NAM_SD;
        SO_LUU_HANH = item.SO_LUU_HANH;
        HD_TU = item.HD_TU;
        HD_DEN = item.HD_DEN;
        TU_NGAY = item.TU_NGAY;
        DEN_NGAY = item.DEN_NGAY;
    }

    public DMThietBi? MapTo()
    {
        return new DMThietBi
        {
            TBID = TBID,
            MA_CSKCB = MA_CSKCB,
            MA_MAY = MA_MAY,
            KY_HIEU = KY_HIEU,
            TEN_THIET_BI = TEN_THIET_BI,
            CONGTY_SX = CONGTY_SX,
            NUOC_SX = NUOC_SX,
            NAM_SX = NAM_SX,
            NAM_SD = NAM_SD,
            SO_LUU_HANH = SO_LUU_HANH,
            HD_TU = HD_TU,
            HD_DEN = HD_DEN,
            TU_NGAY = TU_NGAY,
            DEN_NGAY = DEN_NGAY
        };
    }
}
