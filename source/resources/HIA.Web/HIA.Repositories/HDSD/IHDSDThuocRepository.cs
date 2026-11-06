using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HIA.Repositories.HDSD
{
    public interface IHDSDThuocRepository : IBaseRepository<HDSDThuoc>
    {
        Task<IEnumerable<HDSDThuoc>> GetActiveRules(string currentCSKCB = "", bool isMaster = false);
        Task<PaginationResponseModel<HDSDThuoc>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false);
    }
}
