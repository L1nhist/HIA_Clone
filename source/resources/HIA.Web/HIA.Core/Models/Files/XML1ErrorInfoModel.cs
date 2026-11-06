using HIA.Core.Languages.DisplayField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Files
{
    public class XML1ErrorInfoModel
    {
        public int XML1ERID { get; set; }
        public long XEXML1ID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.XEErrorCode), ResourceType = typeof(DisplayFieldResource))]
        public int? XeErrorCode { get; set; }
        [Display(Name = nameof(DisplayFieldResource.XEErrorRow), ResourceType = typeof(DisplayFieldResource))]
        public int? XeErrorRow { get; set; }

        public int? ErrorType { get; set; }

        [Display(Name = nameof(DisplayFieldResource.ErrorAtXMLFile), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorAtXMLFile { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ErrorAtXMLTag), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorAtXMLTag { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorContent { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ErrorComment), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorComment { get; set; }
        public string ErrorDesc { get; set; }

        public string FileName { get; set; }

        public string ErrorConts { get; set; }

        public bool ErrorMode { get; set; }

        public string MA_AX { get; set; }
        public  string TenAX { get; set; }
        public string Ma_BS_ChiDinh { get; set; }
        public string Ma_BS_YL { get; set; }
    }
    public class XML1ErrorInfoModelAPI
    {
        public int XML1ERID { get; set; }
        public int? XEXML1ID { get; set; }

        [Display(Name = nameof(DisplayFieldResource.XEErrorCode), ResourceType = typeof(DisplayFieldResource))]
        public int? XeErrorCode { get; set; }
        [Display(Name = nameof(DisplayFieldResource.XEErrorRow), ResourceType = typeof(DisplayFieldResource))]
        public int? XeErrorRow { get; set; }

        public int? ErrorType { get; set; }

        [Display(Name = nameof(DisplayFieldResource.ErrorAtXMLFile), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorAtXMLFile { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ErrorAtXMLTag), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorAtXMLTag { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ErrorContent), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorContent { get; set; }
        [Display(Name = nameof(DisplayFieldResource.ErrorComment), ResourceType = typeof(DisplayFieldResource))]
        public string ErrorComment { get; set; }

        public string FileName { get; set; }

        public string Ma_AX { get; set; }

        public string Ten_AX { get; set; }

        public string CCHN { get; set; }

        public DatabaseModel.Entites.DMThuoc DMThuoc { get; set; }
        public DatabaseModel.Entites.DMVTYT DMVTYT { get; set; }

        public DatabaseModel.Entites.DMDichVu DMDichVu { get; set; }
        //public string XML2_Ten_AX { get; set; }

        //public string XML2_Don_Gia { get; set; }

        //public string XML2_MA_NHOM_AX { get; set; }
    }

    public class XML1ErrorQuantityModel
    {
        public int XeErrorCode { get; set; }
        public int Total { get; set; }
    }

    public class SelectErrorsItems
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public int Quantity { get; set; }
    }
}
