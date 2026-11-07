using HIA.Core.Languages.DisplayField;
using HIA.Data.Base;
using HIA.Data.Records;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Files;

public class XML1InfoModel : IMapable<XML01>
{
    //Linhist changed 26/12/22
    //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

    public int XML1ID { get; set; }
    public bool HasError { get; set; }
    public List<XML1ErrorInfoModel> ErrorList { get; set; } = new();

    [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
    public int? Stt { get; set; }

    public string? HospitalId { get; set; }
    [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
    public string? HospitalName { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_KHOA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
    public string? KhoaName { get; set; }

    [Display(Name = nameof(DisplayFieldResource.HO_TEN), ResourceType = typeof(DisplayFieldResource))]
    public string? HO_TEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_THE), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_THE { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CAN_NANG), ResourceType = typeof(DisplayFieldResource))]
    public double? Can_NANG { get; set; }
    [Display(Name = nameof(DisplayFieldResource.DIA_CHI), ResourceType = typeof(DisplayFieldResource))]
    public string? Dia_CHI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.GIOI_TINH), ResourceType = typeof(DisplayFieldResource))]
    public byte? Gioi_TINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_SINH), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_SINH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.GT_THE_TU), ResourceType = typeof(DisplayFieldResource))]
    public string? GT_THE_TU { get; set; }
    [Display(Name = nameof(DisplayFieldResource.GT_THE_DEN), ResourceType = typeof(DisplayFieldResource))]
    public string? GT_THE_DEN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.KET_QUA_DTRI), ResourceType = typeof(DisplayFieldResource))]
    public byte? Ket_QUA_DTRI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.Ten_BENH), ResourceType = typeof(DisplayFieldResource))]
    public string? Ten_BENH { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_BENHKHAC), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BENHKHAC { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MA_BN), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_BN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_CSKCB { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_DKBD), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_DKBD { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_KHUVUC), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_KHUVUC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_LK { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_LOAI_KCB), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_LOAI_KCB { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_LYDO_VVIEN), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_LYDO_VVIEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_NOI_CHUYEN), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_NOI_CHUYEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_PTTT_QT), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_PTTT_QT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_TAI_NAN), ResourceType = typeof(DisplayFieldResource))]
    public byte? MA_TAI_NAN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.MIEN_CUNG_CT), ResourceType = typeof(DisplayFieldResource))]
    public string? Mien_CUNG_CT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NAM_QT), ResourceType = typeof(DisplayFieldResource))]
    public int? Nam_QT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_VAO), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_VAO { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_RA), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_RA { get; set; }
    [Display(Name = nameof(DisplayFieldResource.SO_NGAY_DTRI), ResourceType = typeof(DisplayFieldResource))]
    public int? SO_NGAY_DTRI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_TTOAN), ResourceType = typeof(DisplayFieldResource))]
    public string? Ngay_TTOAN { get; set; }

    [Display(Name = nameof(DisplayFieldResource.THANG_QT), ResourceType = typeof(DisplayFieldResource))]
    public byte? Thang_QT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.TINH_TRANG_RV), ResourceType = typeof(DisplayFieldResource))]
    public byte? Tinh_TRANG_RV { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_BHTT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_BHTT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_BNCCT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_BNCCT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_BNTT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_BNTT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_VTYT), ResourceType = typeof(DisplayFieldResource))]
    public double? T_VTYT { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_TONGCHI), ResourceType = typeof(DisplayFieldResource))]
    public double? T_TONGCHI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_THUOC), ResourceType = typeof(DisplayFieldResource))]
    public double? T_THUOC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_NGUONKHAC), ResourceType = typeof(DisplayFieldResource))]
    public double? T_NGUONKHAC { get; set; }
    [Display(Name = nameof(DisplayFieldResource.T_NGOAIDS), ResourceType = typeof(DisplayFieldResource))]
    public double? T_NGOAIDS { get; set; }

    [Display(Name = nameof(DisplayFieldResource.FILE_NAME), ResourceType = typeof(DisplayFieldResource))]
    public string? FileName { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MHTCID), ResourceType = typeof(DisplayFieldResource))]
    public string? Mhtcid { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CHECKED), ResourceType = typeof(DisplayFieldResource))]
    public byte? Checked{ get; set; }
    [Display(Name = nameof(DisplayFieldResource.DANH_SACH_LOI), ResourceType = typeof(DisplayFieldResource))]
    public string? DanhSachLoi { get; set; }
    [Display(Name = nameof(DisplayFieldResource.NGAY_LAP), ResourceType = typeof(DisplayFieldResource))]
    public string? NgayLAP { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_NOI_DI), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_NOI_DI { get; set; }
    [Display(Name = nameof(DisplayFieldResource.MA_NOI_DEN), ResourceType = typeof(DisplayFieldResource))]
    public string? MA_NOI_DEN { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CREATOR), ResourceType = typeof(DisplayFieldResource))]
    public int? Creator { get; set; }
    [Display(Name = nameof(DisplayFieldResource.CREATED_DATE), ResourceType = typeof(DisplayFieldResource))]
    public string? CreateDate { get; set; }

    public List<XML1ErrorInfoModel> XML1Errors { get; set; } = new();

    // Map cap: XML01 <-> XML1InfoModel
    public void MapFrom(XML01? item)
    {
        if (item == null) return;
        XML1ID = (int)item.XML01ID;
        Stt = item.STT;
        HospitalId = item.HospitalID;
        MA_KHOA = item.MA_KHOA;
        HO_TEN = item.HO_TEN;
        Can_NANG = item.CAN_NANG;
        Dia_CHI = item.DIA_CHI;
        Gioi_TINH = item.GIOI_TINH;
        Ngay_SINH = item.NGAY_SINH?.ToString();
        GT_THE_TU = item.GT_THE_TU;
        GT_THE_DEN = item.GT_THE_DEN;
        Ket_QUA_DTRI = item.KET_QUA_DTRI;
        MA_BN = item.MA_BN;
        MA_CSKCB = item.MA_CSKCB;
        MA_DKBD = item.MA_DKBD;
        MA_KHUVUC = item.MA_KHUVUC;
        MA_LK = item.MA_LK;
        MA_LOAI_KCB = MapConvert.ToByte(item.MA_LOAI_KCB);
        MA_PTTT_QT = item.MA_PTTT_QT;
        MA_TAI_NAN = item.MA_TAI_NAN;
        Nam_QT = item.NAM_QT;
        Ngay_VAO = item.NGAY_VAO?.ToString();
        Ngay_RA = item.NGAY_RA?.ToString();
        SO_NGAY_DTRI = item.SO_NGAY_DTRI;
        Ngay_TTOAN = item.NGAY_TTOAN?.ToString();
        Thang_QT = item.THANG_QT;
        T_BHTT = item.T_BHTT;
        T_BNCCT = item.T_BNCCT;
        T_BNTT = item.T_BNTT;
        T_VTYT = item.T_VTYT;
        T_THUOC = item.T_THUOC;
        T_NGUONKHAC = item.T_NGUONKHAC;
        FileName = item.FileName;
        NgayLAP = item.NGAY_LAP?.ToString();
        MA_NOI_DI = item.MA_NOI_DI;
        MA_NOI_DEN = item.MA_NOI_DEN;
        CreateDate = item.CreateDate?.ToString();
    }

    // Map cap: XML01 <-> XML1InfoModel
    public XML01? MapTo()
    {
        return new XML01
        {
            XML01ID = XML1ID,
            STT = Stt,
            HospitalID = HospitalId,
            MA_KHOA = MA_KHOA,
            HO_TEN = HO_TEN,
            CAN_NANG = Can_NANG,
            DIA_CHI = Dia_CHI,
            GIOI_TINH = Gioi_TINH,
            NGAY_SINH = MapConvert.ToDate(Ngay_SINH),
            GT_THE_TU = GT_THE_TU,
            GT_THE_DEN = GT_THE_DEN,
            KET_QUA_DTRI = Ket_QUA_DTRI,
            MA_BN = MA_BN,
            MA_CSKCB = MA_CSKCB ?? string.Empty,
            MA_DKBD = MA_DKBD,
            MA_KHUVUC = MA_KHUVUC,
            MA_LK = MA_LK,
            MA_LOAI_KCB = MA_LOAI_KCB?.ToString(),
            MA_PTTT_QT = MA_PTTT_QT,
            MA_TAI_NAN = MA_TAI_NAN,
            NAM_QT = Nam_QT,
            NGAY_VAO = MapConvert.ToDate(Ngay_VAO),
            NGAY_RA = MapConvert.ToDate(Ngay_RA),
            SO_NGAY_DTRI = SO_NGAY_DTRI,
            NGAY_TTOAN = MapConvert.ToDate(Ngay_TTOAN),
            THANG_QT = Thang_QT,
            T_BHTT = T_BHTT,
            T_BNCCT = T_BNCCT,
            T_BNTT = T_BNTT,
            T_VTYT = T_VTYT,
            T_THUOC = T_THUOC,
            T_NGUONKHAC = T_NGUONKHAC,
            FileName = FileName,
            NGAY_LAP = MapConvert.ToDate(NgayLAP),
            MA_NOI_DI = MA_NOI_DI,
            MA_NOI_DEN = MA_NOI_DEN,
            CreateDate = MapConvert.ToDate(CreateDate),
        };
    }
}
