using HIA.Core.Languages.DisplayField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Files
{
    public class XML5InfoModel
    {
        //Linhist changed 26/12/22
        //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

        public int XML5ID { get; set; }
        public int? XML1ID { get; set; }
        public bool HasError { get; set; }

        [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
        public int? Stt { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
        public string MA_LK { get; set; }
        [Display(Name = nameof(DisplayFieldResource.DIEN_BIEN), ResourceType = typeof(DisplayFieldResource))]
        public string Dien_BIEN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.HOI_CHAN), ResourceType = typeof(DisplayFieldResource))]
        public string Hoi_CHAN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.PHAU_THUAT), ResourceType = typeof(DisplayFieldResource))]
        public string Phau_THUAT { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_YL), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_YL { get; set; }
    }
}
