using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HIA.Repositories.HDSD
{
    public interface IHDSDDichVuRepository : IBaseRepository<HDSDDichVu>
    {
        Task<IEnumerable<HDSDDichVu>> GetActiveRules(string currentCSKCB = "", bool isMaster = false);
        Task<PaginationResponseModel<HDSDDichVu>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false);
    }
}
