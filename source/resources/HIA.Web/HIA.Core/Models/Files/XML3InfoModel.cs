using HIA.Core.Languages.DisplayField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Files
{
    public class XML3InfoModel
    {
        //Linhist changed 26/12/22
        //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

        public int XML3ID { get; set; }
        public int? XML1ID { get; set; }
        public bool HasError { get; set; }
        public List<string> PropertiesError { get; set; }
        public List<string> ContensError { get; set; }

        [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
        public int? Stt { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
        public string MA_LK { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string MA_DICH_VU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_VAT_TU), ResourceType = typeof(DisplayFieldResource))]
        public string MA_VAT_TU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_NHOM), ResourceType = typeof(DisplayFieldResource))]
        public byte? MA_NHOM { get; set; }
        [Display(Name = nameof(DisplayFieldResource.GOI_VTYT), ResourceType = typeof(DisplayFieldResource))]
        public string Goi_VTYT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TEN_VAT_TU), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_VAT_TU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TEN_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_DICH_VU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.DON_VI_TINH), ResourceType = typeof(DisplayFieldResource))]
        public string Don_VI_TINH { get; set; }
        [Display(Name = nameof(DisplayFieldResource.PHAM_VI), ResourceType = typeof(DisplayFieldResource))]
        public byte? Pham_VI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.SO_LUONG), ResourceType = typeof(DisplayFieldResource))]
        public double? SO_LUONG { get; set; }
        [Display(Name = nameof(DisplayFieldResource.DON_GIA), ResourceType = typeof(DisplayFieldResource))]
        public double? Don_GIA { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TT_THAU), ResourceType = typeof(DisplayFieldResource))]
        public string TT_THAU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TYLE_TT), ResourceType = typeof(DisplayFieldResource))]
        public byte? Tyle_TT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.THANH_TIEN), ResourceType = typeof(DisplayFieldResource))]
        public double? Thanh_TIEN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.T_TRANTT), ResourceType = typeof(DisplayFieldResource))]
        public double? T_TRANTT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MUC_HUONG), ResourceType = typeof(DisplayFieldResource))]
        public byte? Muc_HUONG { get; set; }
        [Display(Name = nameof(DisplayFieldResource.T_NGUONKHAC), ResourceType = typeof(DisplayFieldResource))]
        public double? T_NGUONKHAC { get; set; }
        [Display(Name = nameof(DisplayFieldResource.T_BNTT), ResourceType = typeof(DisplayFieldResource))]
        public double? T_BNTT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.T_BHTT), ResourceType = typeof(DisplayFieldResource))]
        public double? T_BHTT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.T_BNCCT), ResourceType = typeof(DisplayFieldResource))]
        public double? T_BNCCT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.T_NGOAIDS), ResourceType = typeof(DisplayFieldResource))]
        public double? T_NGOAIDS { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_KHOA), ResourceType = typeof(DisplayFieldResource))]
        public string MA_KHOA { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_GIUONG), ResourceType = typeof(DisplayFieldResource))]
        public string MA_GIUONG { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_BAC_SI), ResourceType = typeof(DisplayFieldResource))]
        public string MA_BAC_SI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_BENH), ResourceType = typeof(DisplayFieldResource))]
        public string MA_BENH { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_YL), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_YL { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_KQ), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_KQ { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_PTTT), ResourceType = typeof(DisplayFieldResource))]
        public byte? MA_PTTT { get; set; }
    }
}
