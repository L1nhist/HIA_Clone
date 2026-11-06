using HIA.Core.Languages.DisplayField;
using System;
using System.ComponentModel.DataAnnotations;

namespace HIA.Core.Base
{
    public class BaseEntity
    {
        public string Id { get; set; }
        [Display(Name = "STATUS", ResourceType = typeof(DisplayFieldResource))]
        public bool IsActivated { get; set; }
        public bool IsDeleted { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string CreatedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
