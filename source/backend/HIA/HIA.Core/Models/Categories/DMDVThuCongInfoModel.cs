using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMDVThuCongInfoModel : IMapable<DMDVThuCong>
{
    public int DVTCID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_DICH_VU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_DICH_VU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_GIA), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
    public double? Don_GIA { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Quyet_DINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Quyet_DINH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_9324), ResourceType = typeof(DisplayFieldResource))]
    public byte? MaNHOM_9324 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MANHOM_5937), ResourceType = typeof(DisplayFieldResource))]
    public byte? MaNHOM_5937 { get; set; }

    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }

    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }

    public string? Ngay_DE_NGHI { get; set; }

    // Map cap: DMDVThuCong <-> DMDVThuCongInfoModel
    public void MapFrom(DMDVThuCong? item)
    {
        if (item == null) return;
        DVTCID = item.DVTCID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_DICH_VU = item.MA_DICH_VU;
        Ten_DICH_VU = item.TEN_DICH_VU;
        Ma_GIA = item.MA_GIA;
        Don_GIA = item.DON_GIA;
        Quyet_DINH = item.QUYET_DINH;
        MaNHOM_9324 = item.MANHOM_9324;
        MaNHOM_5937 = item.MANHOM_5937;
        Tu_NGAY = item.TU_NGAY;
        Den_NGAY = item.DEN_NGAY;
        Ngay_DE_NGHI = item.NGAY_DE_NGHI;
    }

    public DMDVThuCong? MapTo()
    {
        return new DMDVThuCong
        {
            DVTCID = DVTCID,
            MA_CSKCB = Ma_CSKCB ?? string.Empty,
            MA_DICH_VU = Ma_DICH_VU,
            TEN_DICH_VU = Ten_DICH_VU,
            MA_GIA = Ma_GIA,
            DON_GIA = Don_GIA,
            QUYET_DINH = Quyet_DINH,
            MANHOM_9324 = MaNHOM_9324,
            MANHOM_5937 = MaNHOM_5937,
            TU_NGAY = Tu_NGAY,
            DEN_NGAY = Den_NGAY,
            NGAY_DE_NGHI = Ngay_DE_NGHI
        };
    }
}
