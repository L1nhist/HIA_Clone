using System.Linq.Expressions;
using HIA.Core.Base;
using HIA.Core.Models.Token;
using HIA.Data.Base;
using HIA.Data.Records;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HIA.Repositories.Files;

public class Xml01Repository : BaseRepository<XML01>, IXml01Repository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<XML01> _dbSet;
    private readonly IHttpContextAccessor _httpContext;

    public Xml01Repository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
    {
        _dbContext = dbFactory.DbContext;
        _dbSet = _dbContext.Set<XML01>();
        _httpContext = httpContext;
    }

    public async Task<IEnumerable<XML01>> GetAllByMaLK(string maCSKCB, string Ma_LK)
    {
        var query = _dbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB && x.MA_LK == Ma_LK && x.IsDelete != true);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<XML01>> GetAllByListID(string maCSKCB, params long[] ids)
    {
        var query = _dbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB && ids.Contains(x.XML01ID) && x.IsDelete != true);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> GetInnersByListID<T>(string maCSKCB, params long[] ids)
        where T : class, IRecord
    {
        var query = _dbContext.Set<T>().AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB && ids.Contains(x.XML01ID));
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<XMLError>> GetErrorsByListID(string maCSKCB, params long[] ids)
    {
        var query = _dbContext.Set<XMLError>().AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB && ids.Contains(x.XML01ID));
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<XML01>> GetAllIncludedStateByListID(string maCSKCB, params long[] ids)
    {
        var x01s = await _dbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB && ids.Contains(x.XML01ID)).ToListAsync();

        var x00s = await _dbContext.Set<XMLState>().AsNoTracking()
            .Where(x => x.MA_CSKCB == maCSKCB && ids.Contains(x.XML01ID)).ToListAsync();

        foreach (var x01 in x01s)
        {
            x01.State = x00s.FirstOrDefault(x => x.XML01ID == x01.XML01ID);
        }

        return x01s;
    }

    public async Task<Dictionary<string, int>> CountTotalUploads(DateTime createDate)
    {
        var fromDate = createDate.Date;
        var endDate = fromDate.AddDays(1);

        var query = await _dbSet.AsNoTracking().Where(x => x.CreateDate >= fromDate && x.CreateDate < endDate)
                            .GroupBy(x => x.MA_CSKCB, (x, g) => new { MaCSKCB = x, Total = g.Count() }).ToListAsync();

        var res = new Dictionary<string, int>();
        foreach (var x in query)
        {
            res.Add(x.MaCSKCB, x.Total);
        }
        return res;
    }

    public async Task<IEnumerable<XML01>> GetAllListXmlReference(string HospitalID)
    {
        var currentMaCSKCB = _httpContext.HttpContext?.User.FindFirst(ClaimKey.MA_CSKCB)?.Value
            ?? throw new InvalidOperationException("Khong tim thay claim MA_CSKCB cua nguoi dung hien tai.");
        return await GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.HospitalID == HospitalID);
    }

    // Cac ham duoi day la stub tu ban cu (logic truy van da bi vo hieu hoa, luon tra ve null); giu nguyen hanh vi.
    public Task<IEnumerable<XML01>?> GetAllListXmlReferenceLe(int idsLe, string maCSKCB)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<string>?> GetAllListMaCSKCB()
        => Task.FromResult<IEnumerable<string>?>(null);

    public Task<IEnumerable<XML01>?> GetAllListXmlReferenceLeStringFile(List<string> idsLe)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<XML01>?> GetAllListXmlReferenceLeString(List<string> idsLe)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<XML01>?> GetAllListXmlReferenceSameDate(string currentHospitalId, DateTime mindate, DateTime MAxDate)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<XML01>?> GetAllListXmlReferenceKhoi(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<XML01>?> GetAllListXmlReference_WithXml4Only(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<XML01>?> GetListXmlReport3(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null, bool includedErrors = false)
        => Task.FromResult<IEnumerable<XML01>?>(null);

    public Task<IEnumerable<XML01>?> GetXML1AndErrorsOnly(string currentHospitalId, Expression<Func<XML01, bool>>? filter = null)
        => Task.FromResult<IEnumerable<XML01>?>(null);
}
