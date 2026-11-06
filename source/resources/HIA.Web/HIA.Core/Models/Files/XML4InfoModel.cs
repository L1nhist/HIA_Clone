using HIA.Core.Languages.DisplayField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Files
{
    public class XML4InfoModel
    {
        //Linhist changed 26/12/22
        //Thay doi het kieu DateTime thanh string de hien thi dung dinh dang

        public int XML4ID { get; set; }
        public int? XML1ID { get; set; }
        public bool HasError { get; set; }

        [Display(Name = nameof(DisplayFieldResource.NO), ResourceType = typeof(DisplayFieldResource))]
        public int? Stt { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_LK), ResourceType = typeof(DisplayFieldResource))]
        public string MA_LK { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_DICH_VU), ResourceType = typeof(DisplayFieldResource))]
        public string MA_DICH_VU { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_CHI_SO), ResourceType = typeof(DisplayFieldResource))]
        public string MA_CHI_SO { get; set; }
        [Display(Name = nameof(DisplayFieldResource.TEN_CHI_SO), ResourceType = typeof(DisplayFieldResource))]
        public string Ten_CHI_SO { get; set; }
        [Display(Name = nameof(DisplayFieldResource.GIA_TRI), ResourceType = typeof(DisplayFieldResource))]
        public string Gia_TRI { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MA_MAY), ResourceType = typeof(DisplayFieldResource))]
        public string MA_MAY { get; set; }
        [Display(Name = nameof(DisplayFieldResource.MO_TA), ResourceType = typeof(DisplayFieldResource))]
        public string MO_TA { get; set; }
        [Display(Name = nameof(DisplayFieldResource.KET_LUAN), ResourceType = typeof(DisplayFieldResource))]
        public string Ket_LUAN { get; set; }
        [Display(Name = nameof(DisplayFieldResource.NGAY_KQ), ResourceType = typeof(DisplayFieldResource))]
        public string Ngay_KQ { get; set; }

    }
}
