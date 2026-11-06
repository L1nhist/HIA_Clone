using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using HIA.Core.Base;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using System.Linq;
using Microsoft.Extensions.Logging;




namespace HIA.Repositories.Files
{
    public class XMLValidateRepository : BaseRepository<XMLValidate>, IXMLValidateRepository
    {
       
        private readonly DbFactory _dbFactory;
        private DbSet<XMLValidate> _dbSet;

        public XMLValidateRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _dbSet = _dbFactory.DbContext.Set<XMLValidate>();
            
        }
        public async Task<IEnumerable<string>> GetMiddleMaCSKCB(int max = 30)
        {
            //update 2 chỗ, 1 update lấy 10 CSKCB, hay Order by Accending (Ưu tiên các viện ít hồ sơ đi trước)
            // max = 10;
            var query = _dbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).OrderBy(g => g.Count()).ThenByDescendingDynamic(g => g.Key).Select(g => g.Key);
            if (max <= 0)
            {
                var total = await query.CountAsync();
                if (total >3 && total <= 8)
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

        public async Task<IEnumerable<string>> GetLatestMaCSKCB(int max = 30)
        {
            //update 2 chỗ, 1 update lấy 10 CSKCB, hay Order by Accending (Ưu tiên các viện ít hồ sơ đi trước)
           // max = 10;
            var query = _dbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).OrderBy(g => g.Count()).Select(g => g.Key);
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
            //update 2 chỗ, 1 update lấy 10 CSKCB, hay Order by Accending (Ưu tiên các viện nhieeuf hồ sơ đi trước)
            //max = 10;
            
            var query = _dbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).OrderByDescending(g => g.Count()).Select(g => g.Key);
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
            var query = _dbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB).OrderBy(x => x.XML01ID).Select(x => x.XML01ID);
            if (max > 0)
            {
                query = query.Take(max);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTopCSKCB(int max = 5, bool ascending = true)
        {
            var query = _dbSet.AsNoTracking().GroupBy(x => x.MA_CSKCB).Where(g => g.Count() > 15);
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
            return await _dbSet.AsNoTracking()
                                .GroupBy(x => x.MA_CSKCB)
                                .Where(g => !skips.Contains(g.Key))
                                .Select(g => g.Key).ToListAsync();
        }
    }
}