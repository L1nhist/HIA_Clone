using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Rules;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD;

public partial class DichVuMaMayInfoModel : IMapable<DichVuMaMay>
{
    public int DvmmID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_DICH_VU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_MAY { get; set; }

    // Map cap: DichVuMaMay <-> DichVuMaMayInfoModel
    public void MapFrom(DichVuMaMay? item)
    {
        if (item == null) return;
        DvmmID = item.DVMMID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_DICH_VU = item.MA_DICH_VU;
        Ma_MAY = item.MA_MAY;
    }

    // Map cap: DichVuMaMay <-> DichVuMaMayInfoModel
    public DichVuMaMay? MapTo()
    {
        return new DichVuMaMay
        {
            DVMMID = DvmmID,
            MA_CSKCB = Ma_CSKCB ?? string.Empty,
            MA_DICH_VU = Ma_DICH_VU,
            MA_MAY = Ma_MAY,
        };
    }
}
