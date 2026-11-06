using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HIA.Repositories.HDSD
{
    public interface IChiDinhDongThoiRepository : IBaseRepository<ChiDinhDongThoi>
    {
        Task<IEnumerable<ChiDinhDongThoi>> GetActiveRules(string currentCSKCB = "", bool isMaster = false);
        Task<PaginationResponseModel<ChiDinhDongThoi>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false);
    }
}
