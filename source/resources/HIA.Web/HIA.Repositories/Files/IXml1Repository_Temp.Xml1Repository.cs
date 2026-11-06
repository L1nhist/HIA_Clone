//using HIA.Core.Base;
//using HIA.Core.Models.Pagination;
//using HIA.Core.Models.Token;
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
//namespace HIA.Repositories.Files
//{
//    public class Xml1Repository_Temp : BaseRepository<XML1_Temp>, IXml1Repository_Temp
//    {
//        private readonly DbFactory _dbFactory;
//        private DbSet<XML1_Temp> _dbSet;
//        private readonly IHttpContextAccessor _httpContext;

//        public Xml1Repository_Temp(DbFactory dbFactory, IHttpContextAccessor httpContext) : base(dbFactory, httpContext)
//        {

//            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
//            _dbSet = _dbFactory.DbContext.Set<XML1_Temp>();
//            _httpContext = httpContext;
//        }

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReference(string currentHospitalId)
//        {
//            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
//            //bo asknotracking vào time out
//            var query = _dbSet.AsNoTracking().Where(x => x.HospitalId == currentHospitalId && x.CreateDate.HasValue && x.CreateDate.Value.Date == System.DateTime.Now.Date && x.MHTCID == null && x.MA_CSKCB == currentMaCSKCB)
//                .Include(i => i.XML2s).AsSplitQuery()
//                .Include(i1 => i1.XML3s).AsSplitQuery()
//                .Include(i2 => i2.XML4s).AsSplitQuery()
//                .Include(i3 => i3.XML5s).AsSplitQuery();
//                //.Include(i4 => i4.XML1Errors).AsSplitQuery();
//            var result = await query.ToListAsync();
//            return result;
//        }

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceLe(int idsLe,string maCSKCB)
//        {
//            //bo asknotracking vào time out
//            //5.2.2022 haimaster bo asnotracking vao test
//            var query = _dbSet.AsNoTracking().Where(x =>
//            x.MA_CSKCB == maCSKCB
//            && (x.CHECKED == FileStatusVerification.RECALL_VALIDATE_CHECK || x.MHTCID == null)
//            && x.isDelete == false/* && x.CreateDate<DateTime.Now.AddSeconds(-60)*/).OrderBy(x => x.NGAYLAP).Take(idsLe)
//                 .Include(i => i.XML2s).AsSplitQuery()
//                 .Include(i1 => i1.XML3s).AsSplitQuery()
//                 .Include(i2 => i2.XML4s).AsSplitQuery()
//                 .Include(i3 => i3.XML5s).AsSplitQuery();
//                 //.Include(i4 => i4.XML1Errors).AsSplitQuery();
//            var result = await query.ToListAsync();
//            return result;
//        }



//        public async Task<IEnumerable<string>> GetAllListMaCSKCB()
//        {
//            //bo asknotracking vào time out
//            var query = _dbSet.AsNoTracking().Where(x => x.MA_CSKCB != null &&
//                                                       (x.CHECKED == FileStatusVerification.RECALL_VALIDATE_CHECK || x.MHTCID == null) 
//                                                       && x.isDelete == false && x.CreateDate < DateTime.Now.AddSeconds(-60))
//                .OrderBy(x => x.NGAYLAP)
//                .Select(x => x.MA_CSKCB).Distinct();
            
//            var result = await query.ToListAsync();
//            return result;
//        }

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceLeStringFile(List<string> idsLe)
//        {
//            //bo asknotracking vào time out
//            var query = _dbSet.Where(x => idsLe.Contains(x.MA_LK)&&false.Equals(x.isDelete));
                 
//            var result = await query.ToListAsync();
//            return result;
//        }

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceLeString(List<string> idsLe)
//        {
//            //bo asknotracking vào time out
//            var query = _dbSet.Where(x => idsLe.Contains(x.FileName) && (x.MHTCID == null || x.CHECKED == 99))
//                 .Include(i => i.XML2s).AsSplitQuery()
//                 .Include(i1 => i1.XML3s).AsSplitQuery()
//                 .Include(i2 => i2.XML4s).AsSplitQuery()
//                 .Include(i3 => i3.XML5s).AsSplitQuery();
//                 //.Include(i4 => i4.XML1Errors).AsSplitQuery();
//            var result = await query.ToListAsync();
//            return result;
//        }

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceSameDate(string currentHospitalId,DateTime mindate,DateTime MAxDate)
//        {
//            //bo asknotracking vào time out
//            var query = _dbSet.Where(x => x.HospitalId == currentHospitalId && x.NGAY_VAO.HasValue && x.NGAY_VAO.Value.Date >= mindate.Date&&x.NGAY_RA.HasValue&& x.NGAY_RA.Value.Date <= MAxDate.Date)
//                 .Include(i1 => i1.XML3s).AsSplitQuery();
//            var result = await query.ToListAsync();
//            return result;
//        }

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceHaveXML23()
//        {
//            DateTime timeNow = System.DateTime.Now.AddSeconds(-120);//khoikhoi Neu check chua co xml2 hoac 3 thi save xml1 xong 120s sau moi cau? len
                                                                    


//            var query = _dbSet.AsNoTracking().AsSplitQuery().Where(x =>
//            (
//                (x.XML2s.Any())
//               //             ||
//               // ((!x.XML2s.Any() || !x.XML3s.Any()) && x.CreateDate.HasValue && timeNow > x.CreateDate.Value)
//            )
//            && x.isDelete == false
//            && (x.CHECKED == FileStatusVerification.RECALL_VALIDATE_CHECK || x.MHTCID == null))
//                .Include(i => i.XML2s)
//                .Include(i1 => i1.XML3s);

//            var result = await query.ToListAsync();
//            return result;
//        }

        

//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReferenceKhoi(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null)
//        {
//            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

//            //Linhist changed 06/05/22
//            //var query = _dbSet.AsNoTracking().AsSplitQuery().Where(x => x.HospitalId == currentHospitalId && false.Equals(x.isDelete)).Where(filter)
//            var query = _dbSet.AsNoTracking().AsSplitQuery().Where(filter).Where(x => x.MA_CSKCB == currentMaCSKCB)
//                .Include(i => i.XML2s)
//                .Include(i1 => i1.XML3s)
//                .Include(i2 => i2.XML4s)
//                .Include(i3 => i3.XML5s);
//                //.Include(i4 => i4.XML1Errors);

//            //var query = _dbSet.Where(x => x.HospitalId == currentHospitalId).Where(filter)
//            //    .IncludeOptimized(i => i.XML2s)
//            //    .IncludeOptimized(i1 => i1.XML3s)
//            //    .IncludeOptimized(i2 => i2.XML4s)
//            //    .IncludeOptimized(i3 => i3.XML5s)
//            //    .IncludeOptimized(i4 => i4.XML1Errors);

//            var result = await query.ToListAsync();
//            return result;
//        }

//        #region Linhist added 15/11/22
//        public async Task<IEnumerable<XML1_Temp>> GetAllListXmlReference_WithXml4Only(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null)
//        {
//            //Linhist changed 06/05/22
//            //var query = _dbSet.AsNoTracking().AsSplitQuery().Where(x => x.HospitalId == currentHospitalId && false.Equals(x.isDelete)).Where(filter)
//            var query = _dbSet.AsNoTracking().AsSplitQuery().Where(filter)
//                //.Include(i => i.XML2s)
//                //.Include(i1 => i1.XML3s)
//                .Include(i2 => i2.XML4s);
//                //.Include(i3 => i3.XML5s)
//                //.Include(i4 => i4.XML1Errors);

//            //var query = _dbSet.Where(x => x.HospitalId == currentHospitalId).Where(filter)
//            //    .IncludeOptimized(i => i.XML2s)
//            //    .IncludeOptimized(i1 => i1.XML3s)
//            //    .IncludeOptimized(i2 => i2.XML4s)
//            //    .IncludeOptimized(i3 => i3.XML5s)
//            //    .IncludeOptimized(i4 => i4.XML1Errors);

//            var result = await query.ToListAsync();
//            return result;
//        }
//        #endregion

//        #region Linhist added 09/06/22
//        public async Task<IEnumerable<XML1_Temp>> GetListXmlReport3(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null, bool includedErrors = false)
//        {
//            IEnumerable<XML1_Temp> result = null;

//            if (!string.IsNullOrWhiteSpace(currentHospitalId))
//            {
//                filter = filter.And(x => x.HospitalId == currentHospitalId);
//            }

//            var query = _dbSet.AsNoTracking().AsSplitQuery().Where(filter).Include(i => i.XML2s).Include(i1 => i1.XML3s);

//            //if (includedErrors)
//            //{
//            //    result = await query.Include(i2 => i2.XML1Errors).ToListAsync();
//            //}
//            //else
//            //{
//            //    result = await query.ToListAsync();
//            //}

//            return null;
//        }
//        #endregion

//        public async Task<IEnumerable<XML1_Temp>> GetXML1HaveXML4ErrorsOnly(List<int?> xml1Id)
//        {
//            //chi lay xml1 co xml4 errors
//            var query = _dbSet.AsNoTracking().AsSplitQuery().Where(x => xml1Id.Contains(x.XML1ID))
//                .Include(i2 => i2.XML4s);
//                //.Include(i4 => i4.XML1Errors);

//            var result = await query.ToListAsync();
//            return result;
//        }
//        //public async Task<IEnumerable<XML1>> GetListAsync(Expression<Func<XML1, bool>> filter = null)
//        //{
//        //    //var query = _dbSet.AsNoTracking();
//        //    //var query = _dbSet.Where(filter);

//        //    return await query.ToListAsync();
//        //}

//        //Linhist added 07/10/22
//        public async Task<IEnumerable<XML1_Temp>> GetXML1AndErrorsOnly(string currentHospitalId, Expression<Func<XML1_Temp, bool>> filter = null)
//        {
//            //chi lay xml1 co xml4 errors
//            var query = _dbSet.AsNoTracking().AsSplitQuery().Where(filter);
//                //.Include(i4 => i4.XML1Errors);

//            //var query = _dbSet.Where(x => x.HospitalId == currentHospitalId).Where(filter)
//            //    .IncludeOptimized(i => i.XML2s)
//            //    .IncludeOptimized(i1 => i1.XML3s)
//            //    .IncludeOptimized(i2 => i2.XML4s)
//            //    .IncludeOptimized(i3 => i3.XML5s)
//            //    .IncludeOptimized(i4 => i4.XML1Errors);

//            var result = await query.ToListAsync();
//            return result;
//        }
//    }
//}
