using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Rules;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.HDSD;

public partial class ModifyHDSDDichVuModel : IMapable<HDSDDichVu>
{
    public int Id { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_DICH_VU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_DICH_VU { get; set; }
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
    [Display(Name = nameof(DisplayFieldResource.THUOC_KHAC), ResourceType = typeof(DisplayFieldResource))]
    public string? Thuoc_KHAC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_NHOM), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_NHOM { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DICH_VU_KHAC), ResourceType = typeof(DisplayFieldResource))]
    public string? Dich_VU_KHAC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TIEN_SU_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Tien_SU_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
    public string? Tyle_TT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOI_GIAN_TOI_THIEU), ResourceType = typeof(DisplayFieldResource))]
    public double? Thoi_GIAN_TOI_THIEU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOI_GIAN_THUC_HIEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Thoi_GIAN_THUC_HIEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.XEErrorCode), ResourceType = typeof(DisplayFieldResource))]
    public int? Ma_LOI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
    public string? Noi_DUNG_LOI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_LOAI_KCB), ResourceType = typeof(DisplayFieldResource))]
    public int? Ma_LOAI_KCB { get; set; }

    public int? So_NGUOI_TOI_THIEU { get; set; }
    public string? Ma_DICH_VU_KO_CHECK { get; set; }

    // Map cap: HDSDDichVu <-> ModifyHDSDDichVuModel
    public void MapFrom(HDSDDichVu? item)
    {
        if (item == null) return;
        Id = item.HDSDDVID;
        Ma_DICH_VU = item.MA_DICH_VU;
        Ten_DICH_VU = item.TEN_DICH_VU;
        Chi_DINH = item.CHI_DINH == true;
        Do_TUOI = item.TUOI;
        Co_THAI = item.CO_THAI == true;
        Cho_CON_BU = item.CHO_CON_BU == true;
        Ma_BENH = item.MA_BENH;
        Thuoc_KHAC = item.THUOC_KHAC;
        Ma_NHOM = item.MA_NHOM;
        Dich_VU_KHAC = item.DICH_VU_KHAC;
        Tien_SU_BENH = item.TIEN_SU_BENH;
        Tyle_TT = item.TYLE_TT;
        Thoi_GIAN_TOI_THIEU = item.THOI_GIAN_TOI_THIEU;
        Thoi_GIAN_THUC_HIEN = item.THOI_GIAN_THUC_HIEN;
        Ma_LOI = MapConvert.ToInt(item.MA_LOI);
        Ma_LOAI_KCB = item.MA_LOAI_KCB;
        Ma_CSKCB = item.MA_CSKCB;
        So_NGUOI_TOI_THIEU = item.SO_NGUOI_TOI_THIEU;
        Ma_DICH_VU_KO_CHECK = item.MA_DICH_VU_KO_CHECK;
    }

    // Map cap: HDSDDichVu <-> ModifyHDSDDichVuModel
    public HDSDDichVu? MapTo()
    {
        return new HDSDDichVu
        {
            HDSDDVID = Id,
            MA_DICH_VU = Ma_DICH_VU,
            TEN_DICH_VU = Ten_DICH_VU,
            CHI_DINH = Chi_DINH,
            TUOI = Do_TUOI,
            CO_THAI = Co_THAI,
            CHO_CON_BU = Cho_CON_BU,
            MA_BENH = Ma_BENH,
            THUOC_KHAC = Thuoc_KHAC,
            MA_NHOM = Ma_NHOM,
            DICH_VU_KHAC = Dich_VU_KHAC,
            TIEN_SU_BENH = Tien_SU_BENH,
            TYLE_TT = Tyle_TT,
            THOI_GIAN_TOI_THIEU = Thoi_GIAN_TOI_THIEU,
            THOI_GIAN_THUC_HIEN = Thoi_GIAN_THUC_HIEN,
            MA_LOI = Ma_LOI?.ToString(),
            MA_LOAI_KCB = Ma_LOAI_KCB,
            MA_CSKCB = Ma_CSKCB,
            SO_NGUOI_TOI_THIEU = So_NGUOI_TOI_THIEU,
            MA_DICH_VU_KO_CHECK = Ma_DICH_VU_KO_CHECK,
        };
    }
}
