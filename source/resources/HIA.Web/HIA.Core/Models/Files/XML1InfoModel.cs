using HIA.Core.Languages.DisplayField;
using HIA.DatabaseModel.Entites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Models.Files
{
    public class XML1InfoModel
    {
        //Linhist changed 26/12/22
        //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

        public int XML1ID { get; set; }
        public bool HasError { get; set; }
        public List<XML1ErrorInfoModel> ErrorList { get; set; }

        [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
        public int? Stt { get; set; }

        public string HospitalId { get; set; }
        [Display(Name = nameof(DisplayFieldResource.HOSPITAL), ResourceType = typeof(DisplayFieldResource))]
        public string HospitalName { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string MA_KHOA { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TEN_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string KhoaName { get; set; }

        [Display(Name = nameof(DisplayFieldResource.HO_TEN), ResourceType = typeof(DisplayFieldResource))]
        public string HO_TEN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_THE), ResourceType = typeof(DisplayFieldResource))]
        public string MA_THE { get; set; }
        [Display(Name = nameof(DisplayFieldResource.CAN_NANG), ResourceType = typeof(DisplayFieldResource))]
        public double? Can_NANG { get; set; }
        [Display(Name = nameof(DisplayFieldResource.DIA_CHI), ResourceType = typeof(DisplayFieldResource))]
        public string Dia_CHI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.GIOI_TINH), ResourceType = typeof(DisplayFieldResource))]
        public byte? Gioi_TINH { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_SINH), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_SINH { get; set; }
        [Display(Name = nameof(DisplayFieldResource.GT_THE_TU), ResourceType = typeof(DisplayFieldResource))]
        public string GT_THE_TU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.GT_THE_DEN), ResourceType = typeof(DisplayFieldResource))]
        public string GT_THE_DEN { get; set; }

        [Display(Name = nameof(DisplayFieldResource.KET_QUA_DTRI), ResourceType = typeof(DisplayFieldResource))]
        public byte? Ket_QUA_DTRI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
        public string MA_BENH { get; set; }
        [Display(Name = nameof(DisplayFieldResource.Ten_BENH), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_BENH { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_BENHKHAC), ResourceType = typeof(DisplayFieldResource))]
        public string MA_BENHKHAC { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MA_BN), ResourceType = typeof(DisplayFieldResource))]
        public string MA_BN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_CSKCB), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CSKCB { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_DKBD), ResourceType = typeof(DisplayFieldResource))]
        public string MA_DKBD { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_KHUVUC), ResourceType = typeof(DisplayFieldResource))]
        public string MA_KHUVUC { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
        public string MA_LK { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_LOAI_KCB), ResourceType = typeof(DisplayFieldResource))]
        public byte? MA_LOAI_KCB { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_LYDO_VVIEN), ResourceType = typeof(DisplayFieldResource))]
        public byte? MA_LYDO_VVIEN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_NOI_CHUYEN), ResourceType = typeof(DisplayFieldResource))]
        public string MA_NOI_CHUYEN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_PTTT_QT), ResourceType = typeof(DisplayFieldResource))]
        public string MA_PTTT_QT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_TAI_NAN), ResourceType = typeof(DisplayFieldResource))]
        public byte? MA_TAI_NAN { get; set; }

        [Display(Name = nameof(DisplayFieldResource.MIEN_CUNG_CT), ResourceType = typeof(DisplayFieldResource))]
        public string Mien_CUNG_CT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NAM_QT), ResourceType = typeof(DisplayFieldResource))]
        public int? Nam_QT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_VAO), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_VAO { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_RA), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_RA { get; set; }
        [Display(Name = nameof(DisplayFieldResource.SO_NGAY_DTRI), ResourceType = typeof(DisplayFieldResource))]
        public int? SO_NGAY_DTRI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_TTOAN), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_TTOAN { get; set; }

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
        public string FileName { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MHTCID), ResourceType = typeof(DisplayFieldResource))]
        public string Mhtcid { get; set; }
        [Display(Name = nameof(DisplayFieldResource.CHECKED), ResourceType = typeof(DisplayFieldResource))]
        public byte? Checked{ get; set; }
        [Display(Name = nameof(DisplayFieldResource.DANH_SACH_LOI), ResourceType = typeof(DisplayFieldResource))]
        public string DanhSachLoi { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_LAP), ResourceType = typeof(DisplayFieldResource))]
        public string NgayLAP { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_NOI_DI), ResourceType = typeof(DisplayFieldResource))]
        public string MA_NOI_DI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_NOI_DEN), ResourceType = typeof(DisplayFieldResource))]
        public string MA_NOI_DEN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.CREATOR), ResourceType = typeof(DisplayFieldResource))]
        public int? Creator { get; set; }
        [Display(Name = nameof(DisplayFieldResource.CREATED_DATE), ResourceType = typeof(DisplayFieldResource))]
        public string CreateDate { get; set; }

        public List<XML1ErrorInfoModel> XML1Errors { get; set; }
}

    //Linhist removed 29/03/25
    //public class XML1VerifyModel
    //{
    //    public List<XML1> XML1s { get; set; }
    //    public List<DatabaseModel.Entites.Hospital> Hospitals { get; set; }
    //}
}
