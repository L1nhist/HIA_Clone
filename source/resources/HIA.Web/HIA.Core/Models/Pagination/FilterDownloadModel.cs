using System;
using System.Collections.Generic;

namespace HIA.Core.Models.Categories
{
    public class FilterDownloadModel
    {
        //Linhist added 16/06/22
        public string HospitalId { get; set; }

        //Linhist added 27/02/23
        public string MaCSKCB { get; set; }

        public string SearchKey { get; set; }
        public string OrderByColumn { get; set; }
        public string OrderType { get; set; }

        #region Linhist removed 10/03/22
        //Linhist thay the cac thuoc tinh moi
        //public List<string> IgnoreKeysForSearch { get; set; }
        //public List<string> DateFilters { get; set; }
        //public List<string> Departments { get; set; }
        //public List<string> ErrFilter { get; set; }
        //public string FilterFromDate { get; set; }
        //public string FilterToDate { get; set; }
        #endregion

        #region Linhist added 10/03/22
        public DateTime? FromDateIn { get; set; }
        public DateTime? ToDateIn { get; set; }
        public DateTime? FromDateOut { get; set; }
        public DateTime? ToDateOut { get; set; }
        public DateTime? FromDateCreate { get; set; }
        public DateTime? ToDateCreate { get; set; }
        public int? Status { get; set; }

        public List<string> IgnoreKeysForSearch { get; set; }
        public List<string> Departments { get; set; }
        public List<string> Errors { get; set; }
        #endregion

        #region Linhist added 19/04/22
        public DateTime? FromDatePaid { get; set; }
        public DateTime? ToDatePaid { get; set; }


        public int? FromHour { get; set; }

        public int? FromMin { get; set; }

        public int? ToHour { get; set; }

        public int? ToMin { get; set; }

        public string TypeKCB { get; set; }
        #endregion
    }
}