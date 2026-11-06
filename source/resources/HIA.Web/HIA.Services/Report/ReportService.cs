using AutoMapper;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;


namespace HIA.Services.Report
{
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IXml01Repository _xml1Repository;
        //Linhist removed 29/03/25
        //private readonly IBaseRepository<DMError> _dMErrorRepository;
        //private readonly IBaseRepository<XML1Error> _xml1ErrorRepository;
        private readonly IBaseRepository<DMBenh> _DMBenhRepository;
        private readonly IBaseRepository<UserLogs> _userlogRepository;
        private readonly IBaseRepository<ThongKeNgay> _thongkeRepository;

        public ReportService(
            IMapper mapper,
            ILogger<ReportService> logger,
            IHttpContextAccessor httpContext,
            IUnitOfWork unitOfWork,
            ISysHistoryLogService sysHistoryLogService,
            IXml01Repository xml1Repository,
            //Linhist removed 29/03/25
            //IBaseRepository<DMError> dMErrorRepository,
            //IBaseRepository<XML1Error> xml1ErrorRepository,
            IBaseRepository<DMBenh> DMBenhRepository,
            IBaseRepository<UserLogs> userlogRepository,
            IBaseRepository<ThongKeNgay> thongkeRepository
            )
        {
            _mapper = mapper;
            _logger = logger;
            _httpContext = httpContext;
            _unitOfWork = unitOfWork;
            _sysHistoryLogService = sysHistoryLogService;
            _xml1Repository = xml1Repository;
            //Linhist removed 29/03/25
            //_xml1ErrorRepository = xml1ErrorRepository;
            //_dMErrorRepository = dMErrorRepository;
            _DMBenhRepository = DMBenhRepository;
            _userlogRepository = userlogRepository;
            _thongkeRepository = thongkeRepository;
        }

        public async Task<DMBenh> GetTenBen(string code) 
        {

            return await _DMBenhRepository.GetItem(x => x.BenhCode == code);


        }
        public async Task<IEnumerable<XML01>> GetAllXML1()
        {

            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            IEnumerable<XML01> AllXml = await _xml1Repository.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB));
            return AllXml;
        }
        public  int TotalFileDate()
        {
            int total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue && x.CreateDate.Value.Date == System.DateTime.Now.Date).Count();
            if (maCSKCB == "00")
            {
                total = _xml1Repository.GetCount(x => x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date== DateTime.Now.Date && !true.Equals(x.IsDelete)).Count();
            }
            else
            {
                total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Count();
            }
            return total;
        }

        public int TotalFileMonth()
        {
            int total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue && x.CreateDate.Value.Month == System.DateTime.Now.Month).Count();
            if(maCSKCB=="00")
            {
                total = _xml1Repository.GetCount(x=>x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Count();
            }    
            else total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Count();
            return total;
        }

        public int TotalFileDieuTriNoiTruDate()
        {
            int total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue &&x.MA_LOAI_KCB.HasValue && x.MA_LOAI_KCB == 3 && x.CreateDate.Value.Date == System.DateTime.Now.Date).Count();
            if (maCSKCB == "00")
            {
                total = _xml1Repository.GetCount(x=>x.MA_LOAI_KCB == "03" && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Count();
            }
            else
            {
                total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && x.MA_LOAI_KCB == "03" && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Count();
            }
            return total;
        }

        public int TotalFileDieuTriNoiTruMonth()
        {
            int total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue && x.MA_LOAI_KCB.HasValue && x.MA_LOAI_KCB == 3 && x.CreateDate.Value.Month == System.DateTime.Now.Month).Count();
            if (maCSKCB == "00")
            {
                total = _xml1Repository.GetCount(x=>x.MA_LOAI_KCB == "03" && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Count();
            }
             else   total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && x.MA_LOAI_KCB == "03" && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Count();
            
            return total;
        }

        public int TotalFileRaVienDate()
        {
            int total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue &&x.TINH_TRANG_RV.HasValue && x.TINH_TRANG_RV==2 && x.CreateDate.Value.Date == System.DateTime.Now.Date).Count();
            if(maCSKCB=="00")
            {
                total = _xml1Repository.GetCount(x => (x.MA_LOAI_RV == 2 || x.MA_LOAI_RV == 5) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Count();
            }
            else total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && (x.MA_LOAI_RV == 2 || x.MA_LOAI_RV == 5) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Count();
            return total;
        }

        public int TotalFileRavienMonth()
        {
            int total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue && x.TINH_TRANG_RV.HasValue && x.TINH_TRANG_RV == 2 && x.CreateDate.Value.Month == System.DateTime.Now.Month).Count();
            if (maCSKCB == "00")
            {
                total= _xml1Repository.GetCount(x=> (x.MA_LOAI_RV == 2 || x.MA_LOAI_RV == 5) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Count();
            }       
            else total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && (x.MA_LOAI_RV == 2 || x.MA_LOAI_RV == 5) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Count();
            return total;
        }

        public double? TotalFileTongTienDate()
        {
            double? total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue && x.CreateDate.Value.Date == System.DateTime.Now.Date).Sum(y=>y.T_TONGCHI);
            if(maCSKCB=="00")
            {
                //total = _xml1Repository.GetCount(x => x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Sum(y => y.T_TONGCHI_BV)/1000;
                //var totalTien = _xml1Repository.GetCount(x => x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Select(y =>  new { y.T_TONGCHI_BV }).ToList();
                //total=totalTien.Sum(x => x.T_TONGCHI_BV) / 1000000;
                total = 0;
            }
            else total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Sum(y => y.T_TONGCHI_BV) / 1000;
            if (total < 0) return (total * -1);
            return total;
        }

        public double? TotalFileTongTienMonth()
        {
            double? total = 0;
            var maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
            //Linhist changed 22/05/22 chuyen thanh NGAY_THANH_TOAN
            //total = _xml1Repository.GetCount(x => x.HospitalId.Equals(currentHospitalId) && x.CreateDate.HasValue && x.CreateDate.Value.Month == System.DateTime.Now.Month).Sum(y => y.T_TONGCHI);
            try
            { 
                if(maCSKCB=="00")
                {
                    //var totalTien = _xml1Repository.GetCount(x => x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Date == DateTime.Now.Date && !true.Equals(x.IsDelete)).Select(y => new { y.T_TONGCHI_BV }).ToList();
                    //total = totalTien.Sum(x => x.T_TONGCHI_BV) / 1000000;
                    //total = _xml1Repository.GetCount(x => x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Sum(y => y.T_TONGCHI_BV)/1000;
                    total = 0;
                } else total = _xml1Repository.GetCount(x => x.MA_CSKCB.Equals(maCSKCB) && x.CreateDate != null && x.CreateDate.HasValue && x.CreateDate.Value.Year == DateTime.Now.Year && x.CreateDate.Value.Month == DateTime.Now.Month && !true.Equals(x.IsDelete)).Sum(y => y.T_TONGCHI_BV) / 1000;
                if (total < 0) return (total * -1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TotalFileTongTienMonth");
                return 0;
            }
            return total;
        }

        public async Task<TotalizeReport> GetThongKe(DateTime? byDate = null)
        {
            var where = PredicateBuilder.New<ThongKeNgay>(e => e.TheLoai == 2);
            var dte = byDate ?? DateTime.Now;
            var dFrom = dte.Year * 10000 + dte.Month * 100 + dte.Day;

            dte = dte.AddDays(-30);
            var dTo = dte.Year * 10000 + dte.Month * 100 + dte.Day;
            where = where.And(e => e.TrongNgay >= dTo && e.TrongNgay <= dFrom);

            var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            if (currentRole.Contains(RoleConstant.MASTER) || currentMaCSKCB == "BOYTE" || currentMaCSKCB == "00")
            { }
            else if (currentMaCSKCB.Length < 5)
            {
                where = where.And(e => e.MA_CSKCB.StartsWith(currentMaCSKCB));
            }
            else
            {
                where = where.And(e => e.MA_CSKCB == currentMaCSKCB);
            }

            var thongKes = await _thongkeRepository.GetListAsync(where);
            var lst = new TotalizeReport();
            foreach (var tk in thongKes)
            {
                var dd = tk.TrongNgay ?? 0;
                lst.Add(new TotalizeItem
                {
                    MaCSKCB = tk.MA_CSKCB,
                    Hospital = "",
                    TotalFiles = (int)(tk.TongHop ?? 0),
                    FromDate = new DateTime(dd / 10000, (dd % 10000) / 100, dd % 100)
                });
            }

            return lst;
        }

        public async Task<Summary> GetSummary(DateTime? byDate = null)
        {
            var where = PredicateBuilder.New<ThongKeNgay>(e => e.TheLoai == 2);
            var dte = byDate ?? DateTime.Now;
            var dFrom = dte.Year * 10000 + dte.Month * 100 + dte.Day;

            dte = dte.AddDays(-7);
            var dTo = dte.Year * 10000 + dte.Month * 100 + dte.Day;
            where = where.And(e => e.TrongNgay >= dTo && e.TrongNgay <= dFrom);

            var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            if (currentRole.Contains(RoleConstant.MASTER) || currentMaCSKCB == "BOYTE" || currentMaCSKCB == "00")
            { }
            else if (currentMaCSKCB.Length < 5)
            {
                where = where.And(e => e.MA_CSKCB.StartsWith(currentMaCSKCB));
            }
            else
            {
                where = where.And(e => e.MA_CSKCB == currentMaCSKCB);
            }

            var thongKes = await _thongkeRepository.GetListAsync(where);
            var lst = new Summary();
            foreach (var tk in thongKes)
            {
                var dd = tk.TrongNgay ?? 0;
                lst.Add(new Summaryitem
                {
                    MaCSKCB = tk.MA_CSKCB,
                    Hospital = "",
                    TotalUploads = (int) (tk.TongHop ?? 0f),
                    isUploadFiles = tk.TongHop != null && tk.TongHop > 0,
                    FromDate = new DateTime(dd / 10000, (dd % 10000) / 100, dd % 100)
                });
            }

            return lst;
        }

        public async Task<Summary> GetDaily(DateTime dte)
        {
            var where = PredicateBuilder.New<ThongKeNgay>(e => e.TheLoai == 2);
            where = where.And(e => e.TrongNgay == dte.Year * 10000 + dte.Month * 100 + dte.Day);

            var thongKes = await _thongkeRepository.GetListAsync(where);
            var lst = new Summary();
            var maCSKCBs = new List<string>();
            foreach (var tk in thongKes)
            {
                if (!maCSKCBs.Contains(tk.MA_CSKCB))
                {
                    maCSKCBs.Add(tk.MA_CSKCB);
                }

                lst.Add(new Summaryitem
                {
                    MaCSKCB = tk.MA_CSKCB,
                    Hospital = "",
                    TotalUploads = (int)(tk.TongHop ?? 0f),
                    isUploadFiles = tk.TongHop != null && tk.TongHop > 0,
                    FromDate = dte
                });
            }
            if (lst.Count == 0) return lst;

            var actives = await _userlogRepository.GetListAsync(x => maCSKCBs.Contains(x.MA_CSKCB) && x.LogTime.Date == dte && x.Action.ToLower().Contains("báo cáo"));
            foreach (var r in lst)
            {
                r.isActive = actives.Any(e => e.MA_CSKCB == r.MaCSKCB);
            }

            return lst;
        }
    }
}