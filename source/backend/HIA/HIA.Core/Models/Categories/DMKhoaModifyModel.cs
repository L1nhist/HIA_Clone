using HIA.Data.Base;
using HIA.Data.Catalogs;
using HIA.Core.Languages.DisplayField;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMKhoaModifyModel : IMapable<DMKhoa>
{
    public int KhoaID { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string KhoaName { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    [Required]
    public string KhoaCode { get; set; } = string.Empty;

    // Map cap: DMKhoa <-> DMKhoaModifyModel
    public void MapFrom(DMKhoa? item)
    {
        if (item == null) return;
        KhoaID = item.KhoaID;
        KhoaName = item.KhoaName ?? string.Empty;
        KhoaCode = item.KhoaCode ?? string.Empty;
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

public class DMKhoaBVModifyModel
{
    public List<DMKhoaModifyModel> Data { get; set; } = new();
}
