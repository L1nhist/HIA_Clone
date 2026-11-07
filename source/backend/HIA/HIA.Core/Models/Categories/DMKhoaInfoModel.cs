using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMKhoaInfoModel : IMapable<DMKhoa>
{
    public int KhoaID { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? KhoaName { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? KhoaCode { get; set; }

    // Map cap: DMKhoa <-> DMKhoaInfoModel
    public void MapFrom(DMKhoa? item)
    {
        if (item == null) return;
        KhoaID = item.KhoaID;
        KhoaName = item.KhoaName;
        KhoaCode = item.KhoaCode;
    }

    public DMKhoa? MapTo()
    {
        return new DMKhoa
        {
            KhoaID = KhoaID,
            KhoaName = KhoaName,
            KhoaCode = KhoaCode
        };
    }
}

public class DMKhoaBVInfoModel
{
    public int KhoaID { get; set; }
    public bool IsSelected { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? KhoaName { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? KhoaCode { get; set; }
}
