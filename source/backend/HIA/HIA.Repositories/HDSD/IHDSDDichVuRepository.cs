using HIA.Core.Models.Pagination;
using HIA.Data.Entities;
using HIA.Repositories.Base;

namespace HIA.Repositories.HDSD;

public interface IHDSDDichVuRepository : IBaseRepository<HDSDDichVu>
{
    Task<IEnumerable<HDSDDichVu>> GetActiveRules(string currentCSKCB = "", bool isMaster = false);
    Task<PaginationResponseModel<HDSDDichVu>> GetListPaginationRules(PaginationRequestModel request, string currentCSKCB = "", bool isMaster = false);
}
