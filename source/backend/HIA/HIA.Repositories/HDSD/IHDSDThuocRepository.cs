using HIA.Core.Models.Pagination;
using HIA.Data.Entities;
using HIA.Repositories.Base;

namespace HIA.Repositories.HDSD;

public interface IHDSDThuocRepository : IBaseRepository<HDSDThuoc>
{
    Task<IEnumerable<HDSDThuoc>> GetActiveRules(string currentCSKCB = "", bool isMaster = false);
    Task<PaginationResponseModel<HDSDThuoc>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false);
}
