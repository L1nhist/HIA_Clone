using HIA.Core.Base;
using HIA.Data.Records;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HIA.Repositories.Files;

public class XMLValidateRepository : BaseRepository<XMLValidate>, IXMLValidateRepository
{
    public XMLValidateRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    {
    }

    public async Task<IEnumerable<string>> GetLatestMaCSKCB(int max = 30)
    {
        // Uu tien cac vien it ho so di truoc (order by ascending)
        var query = DbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).OrderBy(g => g.Count()).Select(g => g.Key);
        if (max <= 0)
        {
            var total = await query.CountAsync();
            if (total < 3)
            {
                max = total;
            }
            else
            {
                max = total > 1 ? total / 2 : 1;
            }
        }

        return await query.Take(max).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetLatestMaCSKCBDesc(int max = 30)
    {
        // Uu tien cac vien nhieu ho so di truoc
        var query = DbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).OrderByDescending(g => g.Count()).Select(g => g.Key);
        if (max <= 0)
        {
            var total = await query.CountAsync();
            if (total < 2)
            {
                return Enumerable.Empty<string>();
            }
            else
            {
                max = total > 1 ? total / 2 : 0;
            }
        }

        return await query.Take(max).ToListAsync();
    }

    public async Task<IEnumerable<long>> GetListIDToValidate(string maCSKCB, int max = 1000)
    {
        var query = DbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB).OrderBy(x => x.XML01ID).Select(x => x.XML01ID);
        if (max > 0)
        {
            query = query.Take(max);
        }
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetTopCSKCB(int max = 5, bool ascending = true)
    {
        var query = DbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).Where(g => g.Count() > 15);
        if (ascending)
        {
            query = query.OrderByDescending(g => g.Count());
        }
        else
        {
            query = query.OrderBy(g => g.Count());
        }

        return await query.Select(g => g.Key).Take(max < 1 ? 1 : max).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetLessCSKCB(string skips)
    {
        return await DbSet.AsNoTracking()
                            .GroupBy(x => x.MA_CSKCB)
                            .Where(g => !skips.Contains(g.Key))
                            .Select(g => g.Key).ToListAsync();
    }
}
