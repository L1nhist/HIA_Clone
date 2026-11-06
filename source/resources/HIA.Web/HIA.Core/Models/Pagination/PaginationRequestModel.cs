using HIA.Core.Models.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Pagination
{
    public class PaginationRequestModel
    {
        public int Draw { get; set; } // times number call from frontend and send back to frontend
        public string OrderByColumn { get; set; }
        public string OrderType { get; set; }
        public int PageIndex { get; set; }
        public int PageNumber { get; set; }
        public string SearchKey { get; set; }

        public PaginationRequestModel()
        {
        }

        public PaginationRequestModel(DataTableAjaxPostModel request)
        {
            Draw = request.draw;
            OrderType = request.order?[0].dir?.Trim() ?? "";
            OrderByColumn = request.columns?[request.order?[0].column ?? 0].data?.Trim() ?? "";
            PageIndex = request.length == 0 ? 0 : request.start / request.length;
            PageNumber = request.length;
            SearchKey = request.search?.value?.Trim();
        }
    }
}
