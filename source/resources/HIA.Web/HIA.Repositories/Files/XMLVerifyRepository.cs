//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using HIA.Core.Base;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using System.Linq;

//namespace HIA.Repositories.Files
//{
//    public class XMLVerifyRepository : BaseRepository<XMLVerify>, IXMLVerifyRepository
//    {
//        private readonly DbFactory _dbFactory;
//        private DbSet<XMLVerify> _dbSet;
//        private readonly IHttpContextAccessor _httpContext;

//        public XMLVerifyRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
//        {
//            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
//            _dbSet = _dbFactory.DbContext.Set<XMLVerify>();
//        }

//        public async Task<IEnumerable<long>> GetListIDToVerify(string maCSKCB, int max = 300)
//        {
//            var query = _dbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB).OrderByDescending(x => x.XML01ID).Select(x => x.XML01ID);
//            if (max > 0)
//            {
//                query = query.Take(max);
//            }
//            return await query.ToListAsync();
//        }
//        public async Task<IEnumerable<XMLVerify>> GetListToVerify(string maCSKCB, int max = 300)
//        {
//            var query = _dbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB).OrderByDescending(x => x.XML01ID).Take(max);
            
//            return await query.ToListAsync();
//        }
//        public async Task<IEnumerable<string>> GetLastMaCSKCBtoVerify(int max = 10)
//        {
//            return await _dbSet.OrderByDescending(x => x.XML01ID)
//                                .Select(x => x.MA_CSKCB)
//                                .Distinct().Take(max).ToListAsync();
//        }
       

//    }
//}