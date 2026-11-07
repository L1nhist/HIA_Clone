using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Catalogs;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Categories;

public class DMBacSiModifyModel : IMapable<DMBacSi>
{
    public int BSID { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CSKCB { get; set; }
    public string? Ma_BHXH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.HO_TEN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ho_TEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.GIOI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public byte? Gioi_TINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_KHOA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_KHOA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CHUCDANH), ResourceType = typeof(DisplayFieldResource))]
    public string? Chuc_DANH { get; set; }
    public string? Chuc_DANH_NN { get; set; }
    public string? Ma_VI_TRI { get; set; }
    public string? Vi_TRI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MACCHN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ma_CCHN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAYCAP_CCHN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngaycap_CCHN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NOICAP_CCHN), ResourceType = typeof(DisplayFieldResource))]
    public string? Noicap_CCHN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.PHAM_VI), ResourceType = typeof(DisplayFieldResource))]
    public string? Phamvi_CM { get; set; }
    public string? Phamvi_CMBS { get; set; }
    public string? Dvkt_KHAC { get; set; }
    public string? Vb_PHANCONG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOIGIAN_DK), ResourceType = typeof(DisplayFieldResource))]
    public string? Tg_DANGKY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOIGIAN_NGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tg_NGAY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.THOIGIAN_TUAN), ResourceType = typeof(DisplayFieldResource))]
    public string? Tg_TUAN { get; set; }
    public string? Cskcb_KHAC { get; set; }
    public string? Cskcb_CGKT { get; set; }
    public string? Qd_CGKT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TUNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Tu_NGAY { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DENNGAY), ResourceType = typeof(DisplayFieldResource))]
    public string? Den_NGAY { get; set; }

    // Map cap: DMBacSi <-> DMBacSiModifyModel
    public void MapFrom(DMBacSi? item)
    {
        if (item == null) return;
        BSID = item.BSID;
        Ma_CSKCB = item.MA_CSKCB;
        Ma_BHXH = item.MA_BHXH;
        Ho_TEN = item.HO_TEN;
        Gioi_TINH = item.GIOI_TINH;
        Ma_KHOA = item.MA_KHOA;
        Ten_KHOA = item.TEN_KHOA;
        Chuc_DANH = item.CHUC_DANH;
        Chuc_DANH_NN = item.CHUC_DANH_NN;
        Ma_VI_TRI = item.MA_VI_TRI;
        Vi_TRI = item.VI_TRI;
        Ma_CCHN = item.MA_CCHN;
        Ngaycap_CCHN = item.NGAYCAP_CCHN;
        Noicap_CCHN = item.NOICAP_CCHN;
        Phamvi_CM = item.PHAMVI_CM;
        Phamvi_CMBS = item.PHAMVI_CMBS;
        Dvkt_KHAC = item.DVKT_KHAC;
        Vb_PHANCONG = item.VB_PHANCONG;
        Tg_DANGKY = item.TG_DANGKY;
        Tg_NGAY = item.TG_NGAY;
        Tg_TUAN = item.TG_TUAN;
        Cskcb_KHAC = item.CSKCB_KHAC;
        Cskcb_CGKT = item.CSKCB_CGKT;
        Qd_CGKT = item.QD_CGKT;
        Tu_NGAY = item.TU_NGAY;
        Den_NGAY = item.DEN_NGAY;
    }

    // Map cap: DMBacSi <-> DMBacSiModifyModel
    public DMBacSi? MapTo()
    {
        return new DMBacSi
        {
            BSID = BSID,
            MA_CSKCB = Ma_CSKCB ?? string.Empty,
            MA_BHXH = Ma_BHXH,
            HO_TEN = Ho_TEN,
            GIOI_TINH = Gioi_TINH,
            MA_KHOA = Ma_KHOA,
            TEN_KHOA = Ten_KHOA,
            CHUC_DANH = Chuc_DANH,
            CHUC_DANH_NN = Chuc_DANH_NN,
            MA_VI_TRI = Ma_VI_TRI,
            VI_TRI = Vi_TRI,
            MA_CCHN = Ma_CCHN,
            NGAYCAP_CCHN = Ngaycap_CCHN,
            NOICAP_CCHN = Noicap_CCHN,
            PHAMVI_CM = Phamvi_CM,
            PHAMVI_CMBS = Phamvi_CMBS,
            DVKT_KHAC = Dvkt_KHAC,
            VB_PHANCONG = Vb_PHANCONG,
            TG_DANGKY = Tg_DANGKY,
            TG_NGAY = Tg_NGAY,
            TG_TUAN = Tg_TUAN,
            CSKCB_KHAC = Cskcb_KHAC,
            CSKCB_CGKT = Cskcb_CGKT,
            QD_CGKT = Qd_CGKT,
            TU_NGAY = Tu_NGAY,
            DEN_NGAY = Den_NGAY,
        };
    }
}
