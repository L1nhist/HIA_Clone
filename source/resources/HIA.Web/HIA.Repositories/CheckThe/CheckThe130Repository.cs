using HIA.Core.Base;
using HIA.Core.Models.Pagination;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIA.Repositories.CheckThe
{
    public class CheckThe130Repository : BaseRepository<CheckThe4210>, ICheckThe130Repository
    {
        private readonly DbFactory _dbFactory;
        private DbSet<CheckThe4210> _dbSet;
        private readonly IHttpContextAccessor _httpContext;

        public CheckThe130Repository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
        {

            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _dbSet = _dbFactory.DbContext.Set<CheckThe4210>();
            _httpContext = httpContext;
        }

        public async Task<IEnumerable<string>> GetLastMaCSKCB(int max = 5)
        {
            return await _dbSet.OrderByDescending(x => x.TG_CHECK_THE)
                                .Select(x => x.MA_CSKCB)
                                .Distinct().Take(max).ToListAsync();
        }
    }
}
