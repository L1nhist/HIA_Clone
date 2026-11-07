using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Rules;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD;

public partial class DichVuMaMayModifyModel : IMapable<DichVuMaMay>
{
    public int DVMMID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_DICH_VU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_MAY { get; set; }

    // Map cap: DichVuMaMay <-> DichVuMaMayModifyModel
    public void MapFrom(DichVuMaMay? item)
    {
        if (item == null) return;
        DVMMID = item.DVMMID;
        MA_CSKCB = item.MA_CSKCB;
        MA_DICH_VU = item.MA_DICH_VU;
        MA_MAY = item.MA_MAY;
    }

    // Map cap: DichVuMaMay <-> DichVuMaMayModifyModel
    public DichVuMaMay? MapTo()
    {
        return new DichVuMaMay
        {
            DVMMID = DVMMID,
            MA_CSKCB = MA_CSKCB ?? string.Empty,
            MA_DICH_VU = MA_DICH_VU,
            MA_MAY = MA_MAY,
        };
    }
}
