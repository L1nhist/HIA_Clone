using HIA.Core.Models.Pagination;
using HIA.Data.Rules;
using HIA.Repositories.Base;

namespace HIA.Repositories.HDSD;

public interface IChiDinhDongThoiRepository : IBaseRepository<ChiDinhDongThoi>
{
    Task<IEnumerable<ChiDinhDongThoi>> GetActiveRules(string currentCSKCB = "", bool isMaster = false);
    Task<PaginationResponseModel<ChiDinhDongThoi>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false);
}
