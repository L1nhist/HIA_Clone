//using HIA.Core.Base;
//using HIA.Core.Models.Pagination;
//using HIA.DatabaseModel.Entites;
//using HIA.Repositories.Base;
//using LinqKit;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Query;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using Z.EntityFramework.Plus;
//namespace HIA.Repositories.FileAPI
//{
//    public class Xml1APIRepository : BaseRepository<XML1_API>, IXml1APIRepository
//    {
//        private readonly DbFactory _dbFactory;
//        private DbSet<XML1_API> _dbSet;
//        private readonly IHttpContextAccessor _httpContext;

//        public Xml1APIRepository(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
//        {

//            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
//            _dbSet = _dbFactory.DbContext.Set<XML1_API>();
//            _httpContext = httpContext;
//        }

//        public async Task<IEnumerable<XML1_API>> GetAllListXmlReferenceLeString(List<string> idsLe)
//        {
//            //bo asknotracking vào time out
//            var query = _dbSet.Where(x => idsLe.Contains(x.FileName) && (x.MHTCID == null || x.CHECKED == 99))
//                 .Include(i => i.XML2_APIs).AsSplitQuery()
//                 .Include(i1 => i1.XML3_APIs).AsSplitQuery()
//                 //.Include(i2 => i2.XML4_APIs).AsSplitQuery()
//                 //.Include(i3 => i3.XML5_APIs).AsSplitQuery()
//                 .Include(i4 => i4.XML1Error_APIs).AsSplitQuery();
//            var result = await query.ToListAsync();
//            return result;
//        }
//    }
//}
