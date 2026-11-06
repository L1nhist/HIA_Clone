using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Pagination
{
    public class PaginationResponseModel<T>
    {
        public int TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; }

        public PaginationResponseModel()
        {
        }
        public PaginationResponseModel(int totalItems, IEnumerable<T> items)
        {
            TotalItems = totalItems;
            Items = items;
        }
    }
}
