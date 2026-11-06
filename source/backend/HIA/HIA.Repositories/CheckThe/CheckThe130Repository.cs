using HIA.Core.Base;
using HIA.Data.Records;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HIA.Repositories.CheckThe;

public class CheckThe130Repository : BaseRepository<CheckThe4210>, ICheckThe130Repository
{
    public CheckThe130Repository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    {
    }

    public async Task<IEnumerable<string?>> GetLastMaCSKCB(int max = 5)
    {
        return await DbSet.OrderByDescending(x => x.TG_CHECK_THE)
                            .Select(x => x.MA_CSKCB)
                            .Distinct().Take(max).ToListAsync();
    }
}
