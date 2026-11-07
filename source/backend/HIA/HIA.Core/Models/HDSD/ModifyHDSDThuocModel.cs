using HIA.Core.Languages.DisplayField;
using HIA.Core.Languages.Message;
using HIA.Data.Base;
using HIA.Data.Rules;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD;

public class ModifyHDSDThuocModel : IMapable<HDSDThuoc>
{
    public int Id { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_THUOC), ResourceType = typeof(DisplayFieldResource))]
    [Required(ErrorMessageResourceType = typeof(MessageResource), ErrorMessageResourceName = "ERR_400_REQUIRED_HOSPITAL")]
    public string Ma_THUOC { get; set; } = string.Empty;
    [Display(Name = nameof(DisplayFieldResource.TEN_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_THUOC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DUONG_DUNG), ResourceType = typeof(DisplayFieldResource))]
    public string? Duong_DUNG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CHI_DINH), ResourceType = typeof(DisplayFieldResource))]
    public bool Chi_DINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DO_TUOI), ResourceType = typeof(DisplayFieldResource))]
    public string? Do_TUOI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CO_THAI), ResourceType = typeof(DisplayFieldResource))]
    public bool Co_THAI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CHO_CON_BU), ResourceType = typeof(DisplayFieldResource))]
    public bool Cho_CON_BU { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_BENH { get; set; }

    [Display(Name = nameof(DisplayFieldResource.Ten_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THUOC_KHAC), ResourceType = typeof(DisplayFieldResource))]
    public string? Thuoc_KHAC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Dich_VU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TIEN_SU_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Tien_SU_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public string? Tyle_TT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TRE_SO_SINH), ResourceType = typeof(DisplayFieldResource))]
    public double? Tre_SO_SINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOI_GIAN_TOI_THIEU), ResourceType = typeof(DisplayFieldResource))]
    public double? Thoi_GIAN_TOI_THIEU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.XEErrorCode), ResourceType = typeof(DisplayFieldResource))]
    public int? Ma_LOI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
    public string? Noi_DUNG_LOI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }

    // Map cap: HDSDThuoc <-> ModifyHDSDThuocModel
    public void MapFrom(HDSDThuoc? item)
    {
        if (item == null) return;
        Id = item.HDSDThuocID;
        Ma_THUOC = item.MA_THUOC ?? string.Empty;
        Ten_THUOC = item.TEN_THUOC;
        Duong_DUNG = item.DUONG_DUNG;
        Chi_DINH = item.CHI_DINH == true;
        Do_TUOI = item.TUOI;
        Co_THAI = item.CO_THAI == true;
        Cho_CON_BU = item.CHO_CON_BU == true;
        Ma_BENH = item.MA_BENH;
        Ten_BENH = item.TEN_BENH;
        Thuoc_KHAC = item.THUOC_KHAC;
        Dich_VU = item.DICH_VU;
        Tien_SU_BENH = item.TIEN_SU_BENH;
        Tyle_TT = item.TYLE_TT;
        Tre_SO_SINH = item.TRE_SO_SINH;
        Thoi_GIAN_TOI_THIEU = item.THOI_GIAN_TOI_THIEU;
        Ma_LOI = item.MA_LOI;
        Ma_CSKCB = item.MA_CSKCB;
    }

    // Map cap: HDSDThuoc <-> ModifyHDSDThuocModel
    public HDSDThuoc? MapTo()
    {
        return new HDSDThuoc
        {
            HDSDThuocID = Id,
            MA_THUOC = Ma_THUOC,
            TEN_THUOC = Ten_THUOC,
            DUONG_DUNG = Duong_DUNG,
            CHI_DINH = Chi_DINH,
            TUOI = Do_TUOI,
            CO_THAI = Co_THAI,
            CHO_CON_BU = Cho_CON_BU,
            MA_BENH = Ma_BENH,
            TEN_BENH = Ten_BENH,
            THUOC_KHAC = Thuoc_KHAC,
            DICH_VU = Dich_VU,
            TIEN_SU_BENH = Tien_SU_BENH,
            TYLE_TT = Tyle_TT,
            TRE_SO_SINH = Tre_SO_SINH,
            THOI_GIAN_TOI_THIEU = Thoi_GIAN_TOI_THIEU,
            MA_LOI = Ma_LOI,
            MA_CSKCB = Ma_CSKCB,
        };
    }
}
