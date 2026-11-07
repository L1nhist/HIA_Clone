using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMThietBiInfoModel : IMapable<DMThietBi>
{
    public int TBID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_MAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.KY_HIEU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ky_HIEU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_THIET_BI), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_THIET_BI { get; set; }

    [Display(Name = nameof(DisplayFieldResource.CONGTY_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Congty_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Nuoc_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nuoc_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SX), ResourceType = typeof(DisplayFieldResource))]
    public string? Nam_SX { get; set; }

    [Display(Name = nameof(DisplayFieldResource.NAM_SD), ResourceType = typeof(DisplayFieldResource))]
    public string? Nam_SD { get; set; }

    [Display(Name = nameof(DisplayFieldResource.SO_LUU_HANH), ResourceType = typeof(DisplayFieldResource))]
    public string? So_LUU_HANH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? Hd_TU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HD_DEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Hd_DEN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }

    // Map cap: DMThietBi <-> DMThietBiInfoModel
    public void MapFrom(DMThietBi? item)
    {
        if (item == null) return;
        TBID = item.TBID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_MAY = item.MA_MAY;
        Ky_HIEU = item.KY_HIEU;
        Ten_THIET_BI = item.TEN_THIET_BI;
        Congty_SX = item.CONGTY_SX;
        Nuoc_SX = item.NUOC_SX;
        Nam_SX = item.NAM_SX;
        Nam_SD = item.NAM_SD;
        So_LUU_HANH = item.SO_LUU_HANH;
        Hd_TU = item.HD_TU;
        Hd_DEN = item.HD_DEN;
        Tu_NGAY = item.TU_NGAY;
        Den_NGAY = item.DEN_NGAY;
    }

    public DMThietBi? MapTo()
    {
        return new DMThietBi
        {
            TBID = TBID,
            MA_CSKCB = Ma_CSKCB,
            MA_MAY = Ma_MAY,
            KY_HIEU = Ky_HIEU,
            TEN_THIET_BI = Ten_THIET_BI,
            CONGTY_SX = Congty_SX,
            NUOC_SX = Nuoc_SX,
            NAM_SX = Nam_SX,
            NAM_SD = Nam_SD,
            SO_LUU_HANH = So_LUU_HANH,
            HD_TU = Hd_TU,
            HD_DEN = Hd_DEN,
            TU_NGAY = Tu_NGAY,
            DEN_NGAY = Den_NGAY
        };
    }
}
