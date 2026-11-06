using HIA.Core.Models.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Files
{
    public class FilesPaginationRequest : PaginationRequestModel
    {
        #region Linhist removed 10/03/22
        //Linhist them truong vao class cu & thay doi construction
        //public List<string> DateFilters { get; set; }
        //public List<string> Departments { get; set; }
        //public List<string> ErrFilter { get; set; }
        //public string FilterFromDate { get; set; }
        //public string FilterToDate { get; set; }

        //public FilesPaginationRequest()
        //{

        //}
        //public FilesPaginationRequest(PaginationRequestModel requestModel, List<string> dateFilter,
        //        List<string> departments, List<string> errFilter, string filterFromDate, string filterToDate)
        //{
        //    Draw = requestModel.Draw;
        //    OrderType = requestModel.OrderType;
        //    OrderByColumn = requestModel.OrderByColumn;
        //    PageIndex = requestModel.PageIndex;
        //    PageNumber = requestModel.PageNumber;
        //    SearchKey = requestModel.SearchKey;


        //    DateFilters = dateFilter;
        //    Departments = departments;
        //    ErrFilter = errFilter;
        //    FilterFromDate = filterFromDate;
        //    FilterToDate = filterToDate;
        //}
        #endregion

        //Linhist added 16/06/22
        public string HospitalId { get; set; }

        #region Linhist added 19/04/22
        public DateTime? FromDatePaid { get; set; }
        public DateTime? ToDatePaid { get; set; }
        public string? TypeKCB { get; set; }
        #endregion

        #region Linhist added 10/03/22
        public DateTime? FromDateIn { get; set; }
        public DateTime? ToDateIn { get; set; }
        public DateTime? FromDateOut { get; set; }
        public DateTime? ToDateOut { get; set; }
        public DateTime? FromDateCreate { get; set; }
        public DateTime? ToDateCreate { get; set; }
        public List<string> Departments { get; set; }
        public List<string> Errors { get; set; }
        public int? FromHour { get; set; }
        public int? FromMin { get; set; }
        public int? ToHour { get; set; }
        public int? ToMin { get; set; }
        public int? Status { get; set; }

        public FilesPaginationRequest()
        {

        }

        //Linhist changed 16/06/22 them string hospitalId
        public FilesPaginationRequest(PaginationRequestModel requestModel, List<string> dateIns, List<string> dateOuts, List<string> dateCreates, List<string> datePaids,
                List<string> departments, List<string> errors, List<int?> timeRanges, string typeKCB, int? status, string hospitalId = "")
        {
            //Linhist changed 16/06/22 them string hospitalId
            HospitalId = hospitalId;

            Draw = requestModel.Draw;
            OrderType = requestModel.OrderType;
            OrderByColumn = requestModel.OrderByColumn;
            PageIndex = requestModel.PageIndex;
            PageNumber = requestModel.PageNumber;
            SearchKey = requestModel.SearchKey;

            Departments = departments;

            DateTime dtm = DateTime.MinValue;
            if (dateIns?.Count > 0 && DateTime.TryParse($"{dateIns[0]}", out dtm))
            {
                FromDateIn = dtm.Date;
            }
            if (dateIns?.Count > 1 && DateTime.TryParse($"{dateIns[1]}", out dtm))
            {
                ToDateIn = dtm.Date;
            }

            if (dateOuts?.Count > 0 && DateTime.TryParse($"{dateOuts[0]}", out dtm))
            {
                FromDateOut = dtm.Date;
            }
            if (dateOuts?.Count > 1 && DateTime.TryParse($"{dateOuts[1]}", out dtm))
            {
                ToDateOut = dtm.Date;
            }

            if (dateCreates?.Count > 0 && DateTime.TryParse($"{dateCreates[0]}", out dtm))
            {
                FromDateCreate = dtm.Date;
            }
            if (dateCreates?.Count > 1 && DateTime.TryParse($"{dateCreates[1]}", out dtm))
            {
                ToDateCreate = dtm.Date;
            }

            #region Linhist added 19/04/22
            if (datePaids?.Count > 0 && DateTime.TryParse($"{datePaids[0]}", out dtm))
            {
                FromDatePaid = dtm.Date;
            }
            if (datePaids?.Count > 1 && DateTime.TryParse($"{datePaids[1]}", out dtm))
            {
                ToDatePaid = dtm.Date;
            }

            TypeKCB = typeKCB;
            #endregion

            //Linhist added 07/04/26
            FromHour = timeRanges?.Count > 0 ? timeRanges[0] : null;
            FromMin = timeRanges?.Count > 1 ? timeRanges[1] : null;
            ToHour = timeRanges?.Count > 2 ? timeRanges[2] : null;
            ToMin = timeRanges?.Count > 3 ? timeRanges[3] : null;

            Status = status;
            Errors = 1.Equals(Status) ? new List<string>() : errors;
        }
        #endregion
    }
}