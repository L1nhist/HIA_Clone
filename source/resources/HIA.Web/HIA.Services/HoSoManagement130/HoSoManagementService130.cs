using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Models.Base;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.CheckThe;
using HIA.Repositories.Files;
using HIA.Repositories.HDSD;
using HIA.Services.Categories;
using HIA.Services.SysHistoryLogManagement;
using HIA.Services.Validate130;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HIA.Services.HosoManagement130
{
    public class HoSoManagementService130 : IHoSoManagementService130
    {
        #region Constructions
        //Linhist changed 02/03/26
        //private bool fullLoadXML = true;
        //private string MA_CSKCB_DANGGD = "";
        //private string MA_CSKCB_DANGGD1 = "";
        //private string MA_CSKCB_DANGTT = "";
        private List<string> lstMA_CSKCB_DANGTT = new List<string>();
        private readonly IMapper _mapper;
        private readonly ILogger<HoSoManagementService130> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IDMErrorService130 _dmErrorService;

        private readonly IXml01Repository _hosoRepository;
        //private readonly IBaseRepository<XMLState> _stateRepository;
        private readonly IBaseRepository<XMLState> _stateRepository;
        private readonly IBaseRepository<XMLError> _errorRepository;
        private readonly IBaseRepository<XML02> _xml02Repository;
        private readonly IBaseRepository<XML03> _xml03Repository;
        //private readonly IBaseRepository<XMLResult> _resultRepository;
        //private readonly IBaseRepository<CheckChanges> _checkChangesRepository;
        private readonly IBaseRepository<UserLogs> _userlogRepository;
        private readonly IXMLValidateRepository _validateRepository;
        //private readonly IXMLVerifyRepository _verifyRepository;

        private readonly IHDSDThuocRepository _HDSDThuocRepository;
        private readonly IHDSDDichVuRepository _HDSDDichVuRepository;
        private readonly IBaseRepository<ChiDinhDongThoi> _ChiDinhDongThoiRepository;
        private readonly IBaseRepository<PhacDoDieuTri> _PhacDoDieuTriRepository;
        //Linhist removed 29/03/25
        //private readonly IBaseRepository<ImportError> _ImportErrorRepository;
        //private readonly IBaseRepository<DMError> _dmErrorRepository;
        private readonly IBaseRepository<DMErrorMHTC> _dmErrorMHTCRepo;
        private readonly IBaseRepository<Hospital> _hospitalRepository;
        private readonly IBaseRepository<DMThuoc> _DMThuocRepository;
        private readonly IBaseRepository<DMVTYT> _DMVTYTRepository;
        private readonly IBaseRepository<DMDichVu> _DMDichVuRepository;
        private readonly IBaseRepository<DMDVThuCong> _DMThuCongRepository;
        private readonly IBaseRepository<DMBacSi> _DMBacSiRepository;
        private readonly IBaseRepository<DMKhoa> _DMKhoaRepository;
        private readonly IBaseRepository<DMKhoaBV> _DMKhoaBVRepository;
        private readonly IBaseRepository<DMDacBiet> _DMDacBietRepository;
        private readonly IBaseRepository<DMYHDTICD10> _DMYHDTRepository;
        private readonly IBaseRepository<DMPhauThuat> _DMPhauThuatRepository;
        private readonly IBaseRepository<DMGiuong> _DMGiuongRepository;
        private readonly IBaseRepository<DMThietBi> _DMThietBiRepository;
        private readonly IBaseRepository<DMBVTuyenTinh> _DMBVTuyenTinhRepository;
        private readonly ICheckThe130Repository _checkTheRepository;

        private readonly IBaseRepository<DMChung130> _DMChungRepository;
        private readonly IBaseRepository<DichVuMaMay> _dvuMaMayRepository;

        public HoSoManagementService130(

            IMapper mapper,
            ILogger<HoSoManagementService130> logger,
            IUnitOfWork unitOfWork,
            IOptions<AppSettings> appSettings,
            IHttpContextAccessor httpContext,
            ISysHistoryLogService sysHistoryLogService,
            IDMErrorService130 dmErrorService,

            IXml01Repository hosoRepository,
            //IBaseRepository<XMLState> stateRepository,
            IBaseRepository<XMLState> xml00Repository,
            IBaseRepository<XMLError> errorRepository,
            IBaseRepository<XML02> xml02Repository,
            IBaseRepository<XML03> xml03Repository,
            //IBaseRepository<XMLResult> resultRepository,
            IXMLValidateRepository validateRepository,
            //IXMLVerifyRepository verifyRepository,
            //IBaseRepository<CheckChanges> checkChangesRepository,
            IBaseRepository<UserLogs> userlogRepository,

            IHDSDThuocRepository HDSDThuocRepository,
            IHDSDDichVuRepository HDSDDichVuRepository,
            IBaseRepository<ChiDinhDongThoi> ChiDinhDongThoiRepository,
            IBaseRepository<PhacDoDieuTri> PhacDoDieuTriRepository,
            //Linhist removed 29/03/25
            //IBaseRepository<ImportError> ImportErrorRepository,
            //IBaseRepository<DMError> dmErrorRepository,
            IBaseRepository<DMErrorMHTC> dmErrorMHTCRepo,
            IBaseRepository<Hospital> hospitalRepository,
            IBaseRepository<DMThuoc> DMThuocRepository,
            IBaseRepository<DMVTYT> DMVTYTRepository,
            IBaseRepository<DMDichVu> DMDichVuRepository,
            IBaseRepository<DMDVThuCong> DMThuCongRepository,
            IBaseRepository<DMBacSi> DMBacSiRepository,
            IBaseRepository<DMKhoa> DMKhoaRepository,
            IBaseRepository<DMKhoaBV> DMKhoaBVRepository,
            IBaseRepository<DMDacBiet> DMDacBietRepository,
            IBaseRepository<DMYHDTICD10> DMYHDTRepository,
            IBaseRepository<DMPhauThuat> DMPhauThuatRepository,
            IBaseRepository<DMGiuong> DMGiuongRepository,
            IBaseRepository<DMThietBi> DMThietBiRepository,
            IBaseRepository<DMBVTuyenTinh> DMBVTuyenTinhRepository,
            ICheckThe130Repository checkTheRepository,
            IBaseRepository<DMChung130> dmChungRepository,
            IBaseRepository<DichVuMaMay> dvuMaMayRepository

        )
        {
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _appSettings = appSettings.Value;
            _httpContext = httpContext;
            _sysHistoryLogService = sysHistoryLogService;

            _hosoRepository = hosoRepository;
            //_stateRepository = stateRepository;
            _stateRepository = xml00Repository;
            _errorRepository = errorRepository;
            _xml02Repository = xml02Repository;
            _xml03Repository = xml03Repository;
            //_resultRepository = resultRepository;
            _validateRepository = validateRepository;
            //_verifyRepository = verifyRepository;
            //_checkChangesRepository = checkChangesRepository;
            _userlogRepository = userlogRepository;
            _dmErrorService = dmErrorService;

            _HDSDThuocRepository = HDSDThuocRepository;
            _HDSDDichVuRepository = HDSDDichVuRepository;
            _ChiDinhDongThoiRepository = ChiDinhDongThoiRepository;
            _PhacDoDieuTriRepository = PhacDoDieuTriRepository;
            //Linhist removed 29/03/25
            //_ImportErrorRepository = ImportErrorRepository;
            //_dmErrorRepository = dmErrorRepository;
            _dmErrorMHTCRepo = dmErrorMHTCRepo;
            _hospitalRepository = hospitalRepository;
            _DMThuocRepository = DMThuocRepository;
            _DMVTYTRepository = DMVTYTRepository;
            _DMDichVuRepository = DMDichVuRepository;
            _DMThuCongRepository  = DMThuCongRepository;
            _DMBacSiRepository = DMBacSiRepository;
            _DMKhoaRepository = DMKhoaRepository;
            _DMKhoaBVRepository = DMKhoaBVRepository; 
            _DMDacBietRepository = DMDacBietRepository;
            _DMYHDTRepository = DMYHDTRepository;
            _DMPhauThuatRepository = DMPhauThuatRepository;
            _DMGiuongRepository = DMGiuongRepository;
            _DMThietBiRepository = DMThietBiRepository;
            _DMBVTuyenTinhRepository = DMBVTuyenTinhRepository;
            _checkTheRepository = checkTheRepository;
            _DMChungRepository = dmChungRepository;
            _dvuMaMayRepository = dvuMaMayRepository;
        }
        #endregion
        public static class CSKCBValidateing
        {
            // Key: Mã hồ sơ (MA_LK hoặc ID), Value: đang xử lý hay không
            public static ConcurrentDictionary<string, bool> DangXuLy = new ConcurrentDictionary<string, bool>();
        }

        #region CronJob function
        public async Task ValidateHosoFrom4210()
        {
            //if (Debugger.IsAttached) return;
            return;

            //Khoi dong sau Validate 4210 cu 5s
            //await Task.Delay(1000);
            //Uti.HandleCronJobLog(_logger, $"Validate130-4210: Bat dau giam dinh [#THREAD {Thread.CurrentThread.ManagedThreadId}]", nameof(ValidateHoso));

            while (true)
            {
                try
                {
                    //12.7.2024 Haimaster update lên lấy 30 viện giám định, tránh trường hợp ưu tiên các viện đẩy nhiều
                    //Uti.HandleCronJobLog(_logger, $"Validate130: bat dau lay danh sach MA_CSKCB có số Hồ sơ nhiều nhất", nameof(ValidateHoso));
                    

                    var lstCSKCBD = await _validateRepository.GetLatestMaCSKCBDesc(10);
                    //long ramused1 = GC.GetTotalMemory(forceFullCollection: true);
                   // Uti.HandleCronJobLog(_logger, $"Validate130 Thread 1: RAM trước Giám định Thread1. RAM: {ramused1}:time:{DateTime.Now:dd/MM/yyyy HH:mm:ss}", nameof(ValidateHoso));
                    //Uti.HandleCronJobLog(_logger, $"Validate130 Thread 1:{lstCSKCBD.Count()}", nameof(ValidateHoso));

                    //if (lstCSKCBD != null || lstCSKCBD.Count() != 0)
                    //{

                        foreach (string maCSKCBD in lstCSKCBD)
                        {
                            try
                            {
                                //if (CSKCBValidateing.DangXuLy.ContainsKey(maCSKCBD)) continue;
                                if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCBD, true)) continue;


                                var lstIds = await _validateRepository.GetListIDToValidate(maCSKCBD, 50);
                                Uti.HandleCronJobLog(_logger, $"Validate130 Thread 1: Lay {lstIds.Count()} ho so moi nhat tu viện nhiều hồ sơ nhất {maCSKCBD} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                                DateTime dteStart = DateTime.Now;
                                await ValidateXML(maCSKCB: maCSKCBD, xmlIds: lstIds.ToArray());
                                TimeSpan execCounter = DateTime.Now - dteStart;
                                Uti.HandleCronJobLog(_logger, $"Validate130 Thread 1: Giam dinh xong viện nhiều hồ sơ nhất {lstIds.Count()} file cua {maCSKCBD}. Thoi gian thuc hien {execCounter.TotalMilliseconds}", nameof(ValidateHoso));
                                await Task.Delay(500);
                            }
                            catch (Exception ex)
                            {
                                Uti.HandleCronJobLog(_logger, $"Validate130 Thread 1: Giam dinh loi, MACSKCB: {maCSKCBD} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                            }
                            finally
                            {
                                CSKCBValidateing.DangXuLy.TryRemove(maCSKCBD, out _);
                            }
                        }
                    //}
                    
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130 Thread 1: Giam dinh loi [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                }
                

                //Uti.HandleCronJobLog(_logger, "Validate130: Ket thuc giam dinh, nghi 1s de tiep tuc", nameof(ValidateHoso));
                await Task.Delay(200);
            }
        }

        public async Task ValidateHoso()
        {
            #region unit test master only 
            //Unit Test 3.6.2025
            //string maCSKCBTest = "75009";
            //long xmlIdTest = 18815379;

            //await ValidateXML(maCSKCBTest, false, false, xmlIdTest);

            //return;
            #endregion
            //if (Debugger.IsAttached) return;
            //return;
            //Khoi dong sau Validate 4210 cu 2s
            //await Task.Delay(1000);
            //Uti.HandleCronJobLog(_logger, $"Validate130: Bat dau giam dinh [#THREAD {Thread.CurrentThread.ManagedThreadId}]", nameof(ValidateHoso));

            while (true)
            {
                try
                {
                    //12.7.2024 Haimaster update lên lấy 30 viện giám định, tránh trường hợp ưu tiên các viện đẩy ít
                    //Uti.HandleCronJobLog(_logger, $"Validate130: bat dau lay danh sach 3 MA_CSKCB số hồ sơ ít nhất", nameof(ValidateHoso));
                    //long ramused2 = GC.GetTotalMemory(forceFullCollection: true);
                    //Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: RAM trước Giám định Thread2. RAM: {ramused2}:time:{DateTime.Now:dd/MM/yyyy HH:mm:ss}", nameof(ValidateHoso));
                    var lstCSKCB = new string[] { "25001" }; await _validateRepository.GetLatestMaCSKCB(10);
                    foreach (string maCSKCB in lstCSKCB)
                    {
                        try
                        {
                            //if (CSKCBValidateing.DangXuLy.ContainsKey(maCSKCB)) continue;
                            if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;


                            var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 50);
                            Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Lay {lstIds.Count()} ho so moi nhat tu viện ít hồ sơ nhất {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                            DateTime dteStart = DateTime.Now;
                            await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                            TimeSpan execCounter = DateTime.Now - dteStart;
                            Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Giam dinh xong Viện ít hồ sơ nhất {lstIds.Count()} file cua {maCSKCB}. Thoi gian thuc hien {execCounter.TotalMilliseconds}", nameof(ValidateHoso));
                            await Task.Delay(100);
                        }
                        catch(Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Giam dinh loi, MACSKCB: {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                        }
                        finally
                        {
                            CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);

                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130 Thread 2: Giam dinh loi [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

                }

                //Uti.HandleCronJobLog(_logger, "Validate130: Ket thuc giam dinh, nghi 1s de tiep tuc", nameof(ValidateHoso));
                await Task.Delay(100);
            }
        }

        private const string VIP1 = "25001;27009;31333;34001;34016;340001";
        private const string VIP2 = "01235;01258;08101;10007;10061;14002;14011;14279;15901;19012;25011;20007;24276;25003;25012;26030;31300;31313;31336;31338;34003;34020";
        //private const string CheckchangeVIP = "25001;31333;34001;34016;08101;26030;75009";

        private readonly Stopwatch _watch1 = Stopwatch.StartNew();
        private readonly Stopwatch _watch2 = Stopwatch.StartNew();
        private readonly Stopwatch _watch3 = Stopwatch.StartNew();

        public async Task ValidateHosoVIP1()
        {
            if (Debugger.IsAttached) return;

            while (true)
            {
                try
                {
                    //Bắt đầu lấy 10 viện có số hồ sơ ít nhất > 15 hồ sơ để giám định
                    //var lstCSKCB = "25001".Split(';');
                    var lstCSKCB = VIP1.Split(';');
                    foreach (string maCSKCB in lstCSKCB)
                    {
                        try
                        {
                            if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                            //var lstIds = new long[] { 19154295 };
                            var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 10);
                            if (lstIds == null || !lstIds.Any())
                            {
                                //Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Không còn hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHosoVIP1));
                                continue;
                            }

                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Lấy {lstIds.Count()} hồ sơ cũ nhất từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                            _watch1.Restart();
                            await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                            _watch1.Stop();
                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Giám định xong {lstIds.Count()} hồ sơ từ viện {maCSKCB}. Thời gian thực hiện {_watch1.Elapsed.TotalMilliseconds}", nameof(ValidateHoso));
                            await Task.Delay(50);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Lỗi giám định từ viện {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                        }
                        finally
                        {
                            _watch1.Stop();
                            CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130 VIP 1: Lỗi giám định [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

                }

                await Task.Delay(100);
            }
        }

        public async Task ValidateHosoVIP2()
        {
            if (Debugger.IsAttached) return;

            while (true)
            {
                try
                {
                    //Bắt đầu lấy 10 viện có số hồ sơ nhiều nhất > 15 hồ sơ để giám định
                    var lstCSKCB = VIP2.Split(';');
                    foreach (string maCSKCB in lstCSKCB)
                    {
                        try
                        {
                            if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                            var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 20);
                            if (lstIds == null || !lstIds.Any())
                            {
                               // Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Không còn hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHosoVIP1));
                                continue;
                            }

                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Lấy {lstIds.Count()} hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                            _watch2.Restart();
                            await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                            _watch2.Stop();
                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Giám định xong {lstIds.Count()} hồ sơ từ viện {maCSKCB}. Thời gian thực hiện {_watch2.Elapsed.TotalMilliseconds}", nameof(ValidateHoso));
                            await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Lỗi giám định từ viện {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                        }
                        finally
                        {
                            _watch2.Stop();
                            CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130 VIP 2: Lỗi giám định [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

                }

                await Task.Delay(500);
            }
        }

        public async Task ValidateHosoVIP3()
        {
            if (Debugger.IsAttached) return;

            while (true)
            {
                try
                {
                    //Bắt đầu lấy 10 viện có số hồ sơ < 15 để giám định (Ưu tiên các viện ít hồ sơ đi trước & sử dụng giám định nhanh)
                    var lstCSKCB = await _validateRepository.GetLessCSKCB($"{VIP1};{VIP2}");
                    foreach (string maCSKCB in lstCSKCB)
                    {
                        try
                        {
                            if (!CSKCBValidateing.DangXuLy.TryAdd(maCSKCB, true)) continue;

                            var lstIds = await _validateRepository.GetListIDToValidate(maCSKCB, 30);
                            if (lstIds == null || !lstIds.Any())
                            {
                               // Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Không còn hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHosoVIP1));
                                continue;
                            }

                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Lấy {lstIds.Count()} hồ sơ từ viện {maCSKCB} [{DateTime.Now:dd/MM hh:mm:ss}]", nameof(ValidateHoso));
                            _watch3.Restart();
                            await ValidateXML(maCSKCB: maCSKCB, xmlIds: lstIds.ToArray());
                            _watch3.Stop();
                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Giám định xong {lstIds.Count()} hồ sơ từ viện {maCSKCB}. Thời gian thực hiện {_watch3.Elapsed.TotalMilliseconds}", nameof(ValidateHoso));
                            await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Giám định lỗi từ viện {maCSKCB} [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));
                        }
                        finally
                        {
                            _watch3.Stop();
                            CSKCBValidateing.DangXuLy.TryRemove(maCSKCB, out _);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130 VIP 3: Giám định lỗi [#THREAD {Thread.CurrentThread.ManagedThreadId}] {ex}{Environment.NewLine}{ex.StackTrace}", nameof(ValidateHoso));

                }

                await Task.Delay(1000);
            }
        }

        #region Linhist removed 29/03/25
        //2.8.2024 Haimaster update 1 đoạn code cho Client chạy thông tuyến từ Client

        //    public async Task checkTT()
        //    {
        //        while (true)
        //        {
        //            try
        //            {
        //                HttpClient client = new HttpClient();
        //                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //                var maCSKCBCHK = "";
        //                var allBVs = await GetBV_AllowVerify();
        //                IEnumerable<XMLVerify> allChecks = null;
        //                // lấy toàn bộ mã CSKCB và xml01id trong bảng xmlverify
        //                allChecks = await GetThe_Info_NeedVerify(maCSKCBCHK);

        //                if (allChecks == null)
        //                {
        //                        await Task.Delay(60000);
        //                        continue;
        //                }
        //                Uti.HandleCronJobLog(_logger, $"TT.3 Số lượng hồ sơ cần check thông tuyến {allChecks.Count()} to Verify.", nameof(HoSoManagementService.VerifyXML1));
        //                Hospital hospital = allBVs.First(x => x.MA_CSKCB == maCSKCBCHK);
        //                    // lấy XML theo MA_CSKCB
        //                    var xmlIds = allChecks.Select(x => x.XML01ID).ToArray();
        //                    var xml01_TTs = await _hosoRepository.GetAllIncludedStateByListID(maCSKCBCHK, xmlIds);

        //                    foreach (var x01 in xml01_TTs)
        //                    {
        //                        //Khong tim thay BV thi bo qua The nay
        //                        if (hospital == null || hospital.FileAPICount >= hospital.MaxFilePerDate) continue;
        //                        if (string.IsNullOrEmpty(x01.MA_THE_BHYT)) continue;

        //                        //Neu BV chua tung Login thi tien hanh login, sau do goi API check The
        //                        bool calledSuccess = false;
        //                        _count_Login_fail = 0;
        //                        if (string.IsNullOrWhiteSpace(hospital.AccessToken) && string.IsNullOrWhiteSpace(hospital.IdToken))
        //                        {
        //                            calledSuccess = await LoginApi_ThongTuyen(x01, hospital, client);
        //                        }
        //                        else
        //                        {
        //                            calledSuccess = await CallApi_ThongTuyen(x01, hospital, client);
        //                        }

        //                        //Neu check The loi thi bo qua The nay
        //                        if (!calledSuccess) continue;


        //                        //Kiem tra thong tin The, bao loi
        //                        VerifyAfter_ThongTuyen(x01);

        //                        await UpdateAfter_CheckThe(hospital, x01);
        //                        //MA_CSKCB_DANGTT = "";
        //                        //Uti.HandleCronJobLog(_logger, $"TT.4 kết thúc thông tuyến thông tuyến {allChecks.Count()} thời gian:.{(DateTime.Now - dtt).Milliseconds}", nameof(HoSoManagementService.VerifyXML1));

        //                        //Check xong 1 Ho so thi nghi 1s
        //                    }
        //                    await Task.Delay(1000);

        //            }
        //            catch (Exception ex)
        //            {
        //                Uti.HandleInternalServerError(ex, "Check thong tuyen job failed", _logger, $"{nameof(HoSoManagementService)} - {nameof(HoSoManagementService.VerifyXML1)}");
        //                Uti.HandleCronJobLog(_logger, $"Check thong tuyen job failed: Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //            }

        //            //Check xong 1 batch thi nghi 2s
        //            await Task.Delay(5000);
        //        }
        //    }
        //    //haimaster update viết thêm 1 hàm check từng hồ sơ một sau khi đã giám định xong

        //    public async Task VerifydataXMLafterValidate(XML01 xml01)
        //    {
        //        if (xml01 is null) return;
        //        Uti.HandleCronJobLog(_logger, $"TT. Kiểm tra thông tuyến: Time {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.ValidateFiles));
        //        try
        //        {
        //            List<XMLVerify> allVerifies = new();
        //            List<XMLError> allErrors = new();
        //            List<XMLState> oldStates = new();
        //            List<XMLState> newStates = new();

        //            var checkthe130 = await _checkTheRepository.GetListAsync(c => c.MA_CSKCB==xml01.MA_CSKCB);
        //            var checkThe = checkthe130.Where(c => c.MA_CSKCB == xml01.MA_CSKCB && c.MA_BN == xml01.MA_BN && c.MA_THE_BHYT == xml01.MA_THE_BHYT).OrderByDescending(c => c.TG_CHECK_THE).FirstOrDefault();
        //            if (checkThe == null)
        //            {
        //                return;
        //            }


        //            //Linhist added 14/11/24
        //            var state = await _stateRepository.GetItem(x => x.XML01ID == xml01.XML01ID && x.MA_CSKCB == xml01.MA_CSKCB);   
        //            XMLState newStt = null;
        //            if (state != null)
        //            {
        //                oldStates.Add(state);
        //                newStt = new XMLState
        //                {
        //                    XML01ID = state.XML01ID,
        //                    MA_CSKCB = state.MA_CSKCB,
        //                    VerifyResult = checkThe.ERROR_CODE,
        //                    TotalErrors = state.TotalErrors,
        //                    TotalAlerts = state.TotalAlerts,
        //                    LastValidate = DateTime.Now,
        //                    LastVerify = checkThe.TG_CHECK_THE
        //                };
        //                newStates.Add(newStt);
        //            }

        //            if (checkThe.ERROR_CODE == "000")
        //            {
        //                allVerifies.Add(verify);
        //                continue;
        //            }

        //            // Gán mã lỗi cho XML
        //            if (int.TryParse(checkThe.ERROR_CODE, out int errCode))
        //            {
        //                allVerifies.Add(verify);
        //                allErrors.Add(new XMLError
        //                {
        //                    XML01ID = xml01.XML01ID,
        //                    MA_CSKCB = xml01.MA_CSKCB,
        //                    MA_LOI = errCode,
        //                    STT = xml01.STT,
        //                });

        //                //Linhist added 14/11/24
        //                if (newStt != null)
        //                {
        //                    newStt.TotalErrors += (errCodes.Contains(errCode) ? 1 : 0);
        //                    newStt.TotalAlerts += (altCodes.Contains(errCode) ? 1 : 0);
        //                }
        //            }
        //            if (allVerifies.Any())
        //            {
        //                _verifyRepository.DeleteList(allVerifies);
        //                await _verifyRepository.SaveChangesAsync();
        //            }

        //        //Lưu lại Error
        //        if (allErrors.Any())
        //        {
        //            await _errorRepository.InsertListAsync(allErrors);
        //            await _errorRepository.SaveChangesAsync();
        //        }

        //        //14.11.2024 Nhờ Linh thêm đoạn code check XML Statte ở đây
        //        //1. Lấy XMLState đó ra
        //        //2. Đêm lỗi vào biến
        //        //3. Xóa XML State
        //        //4. Insert XML state mới vào

        //        //Linhist added 14/11/24
        //        if (newStates.Any())
        //        {
        //            _stateRepository.DeleteList(oldStates);
        //            await _stateRepository.SaveChangesAsync();

        //            await _stateRepository.InsertListAsync(newStates);
        //            await _stateRepository.SaveChangesAsync();
        //        }



        //    }
        //        catch(Exception ex)
        //        {

        //        }
        //}
        #endregion

        //Linhist added 08/04/25 ham verify ngay khi validate ho so
        public async Task<bool> VerifyXML(List<XMLError> allErrors, IEnumerable<int?> errCodes, IEnumerable<int?> altCodes, XML01 xml01)//, XMLState state)
        {
            try
            {
                var checkThe = await _checkTheRepository.GetItemOrderBy(c => c.MA_CSKCB == xml01.MA_CSKCB && c.MA_BN == xml01.MA_BN && c.MA_THE_BHYT == xml01.MA_THE_BHYT, query => query.OrderByDescending(c => c.TG_CHECK_THE));
                if (checkThe == null) return false;
                if(checkThe.MA_DKBD != xml01.MA_DKBD || checkThe.NGAY_SINH != xml01.NGAY_SINH || checkThe.GIOI_TINH != xml01.GIOI_TINH|| checkThe.GT_THE_DEN!=xml01.GT_THE_DEN || checkThe.GT_THE_TU!=xml01.GT_THE_TU||checkThe.GIOI_TINH!=xml01.GIOI_TINH)
                {
                    //Nếu mã đăng ký ban đầu, ngày sinh, giới tính không khớp thì không cần verify
                    return false;
                }
                //state.VerifyResult = checkThe.ERROR_CODE;
                //state.LastValidate = DateTime.Now;

                if (checkThe.ERROR_CODE == "000") return true;

                // Gán mã lỗi cho XML
                if (int.TryParse(checkThe.ERROR_CODE, out int errCode))
                {
                    allErrors.Add(new XMLError
                    {
                        XML01ID = xml01.XML01ID,
                        MA_CSKCB = xml01.MA_CSKCB,
                        MA_LOI = errCode,
                        STT = xml01.STT,
                    });
                    
                    //state.TotalErrors += (errCodes.Contains(errCode) ? 1 : 0);
                    //state.TotalAlerts += (altCodes.Contains(errCode) ? 1 : 0);
                }
                return true;
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(ex, "Check thong tuyen job failed", _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.VerifyXML)}");
                Uti.HandleCronJobLog(_logger, $"Check thong tuyen job failed: Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML));
            }
            return false;
        }
        
        //haimaster update 22.7.2025 get XML01ID

        //Đây là hàm check thông tuyến 130 đang chạy
        //public async Task VerifyHoso()
        //{
        //    int countFail = 0;
        //    if (Debugger.IsAttached) return;
        //    //return;

        //    Uti.HandleCronJobLog(_logger, $"TT. Thong tuyen job start: Time {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyHoso));

        //    while (true)
        //    {
        //        try
        //        {
        //            var lstCSKCBD = await _verifyRepository.GetLastMaCSKCBtoVerify();
        //            //Linhist changed 18/01/25
        //            //var checkthe130 = await _checkTheRepository.GetListAsync();
        //            var checkthe130 = await _checkTheRepository.GetListAsync(c => lstCSKCBD.Contains(c.MA_CSKCB));

        //            foreach (string maCSKCBD in lstCSKCBD)
        //            {
        //                Uti.HandleCronJobLog(_logger, $"TT1. Bắt đầu thông tuyến cho: {maCSKCBD}, thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyHoso));
        //                // Lúc nào data cũng có trong bảng check thẻ (Tạm thời để 50, sau đó đổi cách lấy khác)
        //                var dataverify = await _verifyRepository.GetListToVerify(maCSKCBD, 500);

        //                //Khong con MA_CSKCB nao thi tam nghi 10m roi chay lai
        //                if (dataverify == null || !dataverify.Any())
        //                {
        //                    Uti.HandleCronJobLog(_logger, $"TT. Không có thông tin cần check thẻ, thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyHoso));

        //                    await Task.Delay(60000);
        //                    continue;
        //                }

        //                //var topCSKCBs=dataverify.Items.Select(x => x.MA_CSKCB).Distinct().ToArray();
        //                List<XMLVerify> allVerifies = new();
        //                List<XMLError> allErrors = new();
        //                //List<XMLState> oldStates = new();
        //                //List<XMLState> newStates = new();

        //                //Linhist added 14/11/24
        //                var errFilters = await _dmErrorMHTCRepo.GetListAsync(e => e.MA_CSKCB == maCSKCBD);
        //                var errCodes = errFilters.Select(e => e.EMErrorCode);
        //                var altCodes = errFilters.Where(e => e.EMEnabled == false).Select(e => e.EMErrorCode);
        //                foreach (var verify in dataverify)
        //                {
        //                    //var allChecks = await GetThe_NeedVerify(allCSKCBs.ToArray());
                           
        //                    //Uti.HandleCronJobLog(_logger, $"TT.1 Không có thông tin thông tuyến CSKCB {verify.MA_CSKCB} - Mã thẻ: {verify.XML01ID} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.VerifyXML1));

        //                    //Uti.HandleCronJobLog(_logger, $"TT.2 Thông tuyến lấy danh sách hồ sơ cần thông tuyến cho CSKCB {verify.MA_CSKCB} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.VerifyXML1));
        //                    //gán cơ sở kcb đang check thông tuyến
                                

        //                    var xml01 = await _hosoRepository.GetItem(x => x.XML01ID == verify.XML01ID && x.MA_CSKCB == verify.MA_CSKCB && x.IsDelete == false);
        //                    if (xml01 == null) continue;

        //                    //var checkThe = await _checkTheRepository.GetItemOrderBy(c => c.MA_CSKCB == xml01.MA_CSKCB && c.MA_BN == xml01.MA_BN && c.MA_THE_BHYT == xml01.MA_THE_BHYT && c.MA_DKBD==xml01.MA_DKBD && c.NGAY_SINH==xml01.NGAY_SINH && c.GIOI_TINH==xml01.GIOI_TINH && c.GT_THE_DEN==xml01.GT_THE_DEN && c.GT_THE_TU==xml01.GT_THE_TU, q => q.OrderByDescending(c => c.TG_CHECK_THE));
        //                    var checkThe = checkthe130.Where(c => c.MA_CSKCB == xml01.MA_CSKCB && c.MA_BN == xml01.MA_BN && c.MA_THE_BHYT == xml01.MA_THE_BHYT).OrderByDescending(c=>c.TG_CHECK_THE).FirstOrDefault();
        //                    if (checkThe == null)
        //                    {
        //                        countFail++;
        //                        continue;
        //                    }

        //                    Uti.HandleCronJobLog(_logger, $"TT.2 Thông tuyến thành công cho CSKCB {verify.MA_CSKCB} - Mã thẻ: {checkThe.MA_THE_BHYT} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyHoso));

        //                    //Linhist added 14/11/24
        //                    //var state = await _stateRepository.GetItem(x => x.XML01ID == verify.XML01ID && x.MA_CSKCB == verify.MA_CSKCB);
        //                    //XMLState newStt = null;
        //                    //if (state != null)
        //                    //{
        //                    //    oldStates.Add(state);
        //                    //    newStt = new XMLState
        //                    //    {
        //                    //        XML01ID = state.XML01ID,
        //                    //        MA_CSKCB = state.MA_CSKCB,
        //                    //        VerifyResult = checkThe.ERROR_CODE,
        //                    //        TotalErrors = state.TotalErrors,
        //                    //        TotalAlerts = state.TotalAlerts,
        //                    //        LastValidate = DateTime.Now,
        //                    //        LastVerify = checkThe.TG_CHECK_THE
        //                    //    };
        //                    //    newStates.Add(newStt);
        //                    //}

        //                    if (checkThe.ERROR_CODE == "000")
        //                    {
        //                        allVerifies.Add(verify);
        //                        continue;
        //                    }

        //                    // Gán mã lỗi cho XML
        //                    if (int.TryParse(checkThe.ERROR_CODE, out int errCode))
        //                    {
        //                        allVerifies.Add(verify);
        //                        allErrors.Add(new XMLError
        //                        {
        //                            XML01ID = xml01.XML01ID,
        //                            MA_CSKCB = xml01.MA_CSKCB,
        //                            MA_LOI = errCode,
        //                            STT = xml01.STT,
        //                        });

        //                        //Linhist added 14/11/24
        //                        //if (newStt != null)
        //                        //{
        //                        //    newStt.TotalErrors += (errCodes.Contains(errCode) ? 1 : 0);
        //                        //    newStt.TotalAlerts += (altCodes.Contains(errCode) ? 1 : 0);
        //                        //}
        //                    }

        //                }

        //                if (allVerifies.Any())
        //                {
        //                    _verifyRepository.DeleteList(allVerifies);
        //                    await _verifyRepository.SaveChangesAsync();
        //                }

        //                //Lưu lại Error
        //                if (allErrors.Any())
        //                {
        //                    await _errorRepository.InsertListAsync(allErrors);
        //                    await _errorRepository.SaveChangesAsync();
        //                }

        //                //14.11.2024 Nhờ Linh thêm đoạn code check XML Statte ở đây
        //                //1. Lấy XMLState đó ra
        //                //2. Đêm lỗi vào biến
        //                //3. Xóa XML State
        //                //4. Insert XML state mới vào

        //                //Linhist added 14/11/24
        //                //if (newStates.Any())
        //                //{
        //                //    _stateRepository.DeleteList(oldStates);
        //                //    await _stateRepository.SaveChangesAsync();

        //                //    await _stateRepository.InsertListAsync(newStates);
        //                //    await _stateRepository.SaveChangesAsync();
        //                //}    
        //            }

        //            await Task.Delay(1000).ConfigureAwait(false);
        //        }
        //        catch (Exception ex)
        //        {
        //            Uti.HandleInternalServerError(ex, "Check thong tuyen job failed", _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.VerifyHoso)}");
        //            Uti.HandleCronJobLog(_logger, $"Check thong tuyen job failed: Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyHoso));
        //        }
        //        if(countFail>=1000)
        //        {
        //            countFail = 0;
        //            await Task.Delay(60000).ConfigureAwait(false);
        //        }
        //        //Check xong 1 batch thi nghi 2s
        //        await Task.Delay(60000).ConfigureAwait(false);
        //    }

        //    #region Linhist removed 07/10/24
        //    //while (true)
        //    //{
        //    //    try
        //    //    {
        //    //        HttpClient client = new HttpClient();
        //    //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    //        var allBVs = await GetBV_AllowVerify();
        //    //        if (allBVs == null) return;

        //    //        //Kiem tra BV da check the qua nhieu
        //    //        DateTime dateNow = DateTime.Now.Date;
        //    //        DateTime dtt = DateTime.Now;
        //    //        List<string> overCheckeds = new List<string>();

        //    //        //Lay danh sach toan bo MA_CSKCB cua cac Hospital van con so luot check Thong tuyen
        //    //        var allCSKCBs = allBVs.Where(h => h.MaxFilePerDate > h.FileAPICount).Select(x => x.MA_CSKCB).Distinct().ToArray();
        //    //        //Khong con MA_CSKCB nao thi tam nghi 10m roi chay lai
        //    //        if (allCSKCBs == null || !allCSKCBs.Any())
        //    //        {
        //    //            await Task.Delay(60000);
        //    //            continue;
        //    //        }

        //    //        IEnumerable<XMLResult> allResults = null;
        //    //        foreach (var maCSKCBCHK in allCSKCBs)
        //    //        {
        //    //            //var allChecks = await GetThe_NeedVerify(allCSKCBs.ToArray());
        //    //            if (MA_CSKCB_DANGGD == "" || MA_CSKCB_DANGGD != maCSKCBCHK)
        //    //            {
        //    //                Uti.HandleCronJobLog(_logger, $"TT.2 Thông tuyến lấy danh sách hồ sơ cần thông tuyến cho CSKCB {maCSKCBCHK} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.VerifyXML1));
        //    //                //gán cơ sở kcb đang check thông tuyến
        //    //                MA_CSKCB_DANGTT = maCSKCBCHK;
        //    //                // lấy toàn bộ mã CSKCB và xml01id trong bảng xmlverify
        //    //                allResults = await GetResult_NeedVerify(maCSKCBCHK);
        //    //            }

        //    //            //Khong co ban ghi CheckThe nao thi tam nghi 10m roi chay lai
        //    //            if (allResults == null)
        //    //            {
        //    //                Uti.HandleCronJobLog(_logger, $"TT.2.1 Không có thông tin cần thông tuyến cho CSKCB {maCSKCBCHK} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.VerifyXML1));
        //    //                MA_CSKCB_DANGTT = "";
        //    //                await Task.Delay(60000);
        //    //                continue;
        //    //            }

        //    //            Uti.HandleCronJobLog(_logger, $"TT.3 Số lượng hồ sơ cần check thông tuyến {allResults.Count()} to Verify.", nameof(HoSoManagementService.VerifyXML1));

        //    //            Hospital hospital = allBVs.First(x => x.MA_CSKCB == maCSKCBCHK);
        //    //            // lấy XML theo MA_CSKCB
        //    //            var xmlIds = allResults.Select(x => x.XML01ID).ToArray();
        //    //            var xml01_TTs = await _hosoRepository.GetAllIncludedStateByListID(maCSKCBCHK, xmlIds);

        //    //            foreach (var x01 in xml01_TTs)
        //    //            {
        //    //                //Khong tim thay BV thi bo qua The nay
        //    //                //if (hospital == null || hospital.FileAPICount >= hospital.MaxFilePerDate) continue;
        //    //                //if(string.IsNullOrEmpty(x01.MA_THE_BHYT)) continue;

        //    //                //Neu BV chua tung Login thi tien hanh login, sau do goi API check The
        //    //                bool calledSuccess = false;
        //    //                _count_Login_fail = 0;
        //    //                if (string.IsNullOrWhiteSpace(hospital.AccessToken) && string.IsNullOrWhiteSpace(hospital.IdToken))
        //    //                {
        //    //                    calledSuccess = await LoginApi_ThongTuyen(x01, hospital, client);
        //    //                }
        //    //                else
        //    //                {
        //    //                    calledSuccess = await CallApi_ThongTuyen(x01, hospital, client);
        //    //                }

        //    //                //Neu check The loi thi bo qua The nay
        //    //                if (!calledSuccess) continue;

        //    //                //Kiem tra thong tin The, bao loi
        //    //                var result = allResults.FirstOrDefault(r => r.XML01ID == x01.XML01ID);
        //    //                if (result != null && x01.State != null) x01.State.VerifyResult = result.KET_QUA;

        //    //                VerifyAfter_ThongTuyen(x01);

        //    //                await UpdateAfter_CheckThe(hospital, x01);
        //    //                MA_CSKCB_DANGTT = "";
        //    //                Uti.HandleCronJobLog(_logger, $"TT.4 kết thúc thông tuyến thông tuyến {allResults.Count()} thời gian:.{(DateTime.Now - dtt).Milliseconds}", nameof(HoSoManagementService.VerifyXML1));

        //    //                //Check xong 1 Ho so thi nghi 1s
        //    //            }
        //    //            await Task.Delay(1000);
        //    //        }
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        Uti.HandleInternalServerError(ex, "Check thong tuyen job failed", _logger, $"{nameof(HoSoManagementService)} - {nameof(HoSoManagementService.VerifyXML1)}");
        //    //        Uti.HandleCronJobLog(_logger, $"Check thong tuyen job failed: Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //    //    }

        //    //    //Check xong 1 batch thi nghi 2s
        //    //    await Task.Delay(5000);
        //    //}
        //    #endregion
        //}
        #endregion

        public async Task AppendHosoToValidate(string maCSKCB = "", params long[] xmlIds)
        {
            var vals = xmlIds.Select(id => new XMLValidate() { XML01ID = id, MA_CSKCB = maCSKCB }).ToList();
            _validateRepository.InsertList(vals);
            await _validateRepository.SaveChangesAsync();
        }

        //public async Task AppendHosoToVerify(Hospital hospital, params long[] xmlIds)
        //{
        //    if (string.IsNullOrEmpty(hospital.Username) || string.IsNullOrEmpty(hospital.Password)) return;

        //    var vers = xmlIds.Select(id => new XMLVerify() { XML01ID = id, MA_CSKCB = hospital.MA_CSKCB }).ToList();
        //    _verifyRepository.InsertList(vers);
        //    await _verifyRepository.SaveChangesAsync();
        //}

        public async Task RemoveHosoFromValidate(params long[] xmlIds)
        {
            await _validateRepository.DeleteListByFilterAsync(x => xmlIds.Contains(x.XML01ID));
            await _validateRepository.SaveChangesAsync();
        }

        //public async Task RemoveHosoFromVerify(params long[] xmlIds)
        //{
        //    await _verifyRepository.DeleteListByFilterAsync(x => xmlIds.Contains(x.XML01ID));
        //    await _verifyRepository.SaveChangesAsync();
        //}

        public async Task<ResponseModel<bool>> CreateNewXML(List<XML01> xmls, string maCSKCB = null, string email = null, bool fromHIS = false)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                await _hosoRepository.InsertListAsync(xmls);
                await _hosoRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"CreateNewXML130 fails: {ex.Message}{Environment.NewLine}{ex.StackTrace}", nameof(CreateNewXML));
                res = Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(CreateNewXML)}");
                return res;
            }

            try
            {
                var xmlIds = xmls.Select(x => x.XML01ID).ToArray();
                if (fromHIS)
                {
                    await ValidateXML(maCSKCB, email, fromHIS, xmlIds: xmlIds);
                }
                else
                {
                    await AppendHosoToValidate(maCSKCB, xmlIds);
                }
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"CreateNewXML130 fails: {ex.Message}{Environment.NewLine}{ex.StackTrace}", nameof(CreateNewXML));
                res = Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(CreateNewXML)}");
            }

            return res;
        }

        public async Task ValidateXML(string maCSKCB = null, string email = null, bool fromAPI = false, bool fromHIS = false, params long[] xmlIds)
        {
            #region Get XML & DM
            //Linhist removed 02/03/26
            //fullLoadXML = true;//haimaster add 15/7/2025
            var userEmail = email ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userEmail) && _httpContext?.HttpContext?.User != null)
            {
                userEmail = _httpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    userEmail = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                }
            }

            if (xmlIds == null || xmlIds.Length < 1)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: khong co danh sach XML01ID de kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(maCSKCB))
                {
                    maCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                }
            }
            catch
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: MA_CSKCB de trong, khong the kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;

            }

            if (string.IsNullOrEmpty(maCSKCB))
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: MA_CSKCB de trong, khong the kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            //Linhist changed 20/03/26 - move down
            //Hospital hospital = null;
            //IEnumerable<XML01> xml01s = null;
            //TimePeriod gio_Hanh_Chinh_BV = null;
            //try
            //{
            //    hospital = await _hospitalRepository.GetItem(x => x.MA_CSKCB == maCSKCB);
            //    gio_Hanh_Chinh_BV = new(hospital.OfficeTime);
            //}
            //catch
            //{
            //    Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai benh vien co MA_CSKCB {maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            //    return;
            //}

            IEnumerable<XML01> xml01s = null;
            //Uti.HandleCronJobLog(_logger, $"Validate130: bat dau lay danh sach XML theo danh sach XML01ID truyen vao de kiem tra", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            try
            {
                //Linhist changed 30/08/24
                //xml01s = await _hosoRepository.GetAllIncludedStateByListID(maCSKCB, xmlIds);
                xml01s = await _hosoRepository.GetAllByListID(maCSKCB, xmlIds);

                //Linhist added 03/03/26: xóa hết id khỏi bảng XMLValidate nếu bên trên ko select được XML01 nào
                if (xml01s == null || !xml01s.Any())
                {
                    await RemoveHosoFromValidate(xmlIds);
                    Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai XML theo danh sach XML01ID gui vao, da xoa khoi XMLValidate", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    return;
                }

                //Linhist removed 060826
                //Linhist changed 02/03/26
                //await AttachInnerXML(xml01s);
                //await AttachXMLState(xml01s);
                //if (fullLoadXML==false)
                //if (!await AttachInnerXML(xml01s) || !await AttachXMLState(xml01s))
                //{
                //    Uti.HandleCronJobLog(_logger, $"Validate130: Co loi khi lay cac XML 2,3,4... Khong giam dinh ho so, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                //    return;
                //}
            }
            catch
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: Khong lay duoc XML, khong the kiem tra, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            //Linhist added 060826
            try
            {
                if (!await AttachInnerXML(xml01s))
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: Co loi khi lay cac XML 2,3,4... Khong giam dinh ho so, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    return;
                }
            }
            catch
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: Khong lay duoc XML, khong the kiem tra, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            try
            {
                if (!await AttachXMLState(xml01s))
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: Co loi khi lay XMLState Khong giam dinh ho so, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    return;
                }
            }
            catch
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: Khong lay duoc XMLState, MA_CSKCB{maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            if (xml01s == null || !xml01s.Any())
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai XML theo danh sach XML01ID gui vao", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }
            // haimastervn 23.10.2025 updatecode test
            //var ngayThYlMin = xml01s
            //     .Where(x => x.XML03s != null)
            //     .SelectMany(x => x.XML03s)
            //     .Where(y => y.NGAY_TH_YL != null)
            //     .Min(y => y.NGAY_TH_YL);
            //var ngayThYlMax = xml01s
            //     .Where(x => x.XML03s != null)
            //     .SelectMany(x => x.XML03s)
            //     .Where(y => y.NGAY_TH_YL != null)
            //     .Max(y => y.NGAY_TH_YL);
            //List<XML01History> listXML1LichSu = new List<XML01History>();
            //if (ngayThYlMax.HasValue && ngayThYlMin.HasValue && (ngayThYlMax - ngayThYlMin).Value.TotalDays < 30)
            //{
            //    listXML1LichSu =  _hosoRepository.DbSet.AsNoTracking().Where(x => x.MA_CSKCB == maCSKCB && x.NGAY_VAO != null && x.NGAY_VAO > ngayThYlMin.Value && x.NGAY_VAO < ngayThYlMax.Value && x.IsDelete!=true).Select(x => new XML01History{XML01ID=x.XML01ID, MA_BN= x.MA_BN, HO_TEN= x.HO_TEN,NGAY_VAO= x.NGAY_VAO,NGAY_RA= x.NGAY_RA }).ToList();
            //}


            //Uti.HandleCronJobLog(_logger, $"Validate130: bat dau lay danh sach cac DM phu tro", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            //Linhist changed 20/03/26
            //IEnumerable<Hospital> _hospital = new List<Hospital>();
            Hospital _hospital = null;
            TimePeriod _officialTime = null;
            try
            {
                _hospital = await _hospitalRepository.GetItem(x => x.MA_CSKCB == maCSKCB);
                _officialTime = new(_hospital.OfficeTime);
            }
            catch
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: khong ton tai benh vien co MA_CSKCB {maCSKCB}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            IEnumerable<HDSDThuoc> _ruleThuocs = new List<HDSDThuoc>();
            IEnumerable<HDSDDichVu> _ruleDichVus = new List<HDSDDichVu>();
            IEnumerable<DMThuoc> _dmThuoc = new List<DMThuoc>();
            IEnumerable<DMVTYT> _dmVTYT = new List<DMVTYT>();
            IEnumerable<DMDichVu> _dmDichVu = new List<DMDichVu>();
            IEnumerable<DMDVThuCong> _dmThuCong = new List<DMDVThuCong>();
            IEnumerable<DMBacSi> _dmBacSi = new List<DMBacSi>();
            IEnumerable<DMKhoa> _dmKhoa = new List<DMKhoa>();
            IEnumerable<DMKhoaBV> _dmKhoaBV = new List<DMKhoaBV>();
            IEnumerable<DMYHDTICD10> _dmYHCTs = new List<DMYHDTICD10>();
            IEnumerable<DMThietBi> _dmThietBi = new List<DMThietBi>();
            IEnumerable<DMBVTuyenTinh> _dmTuyenTinhs = new List<DMBVTuyenTinh>();
            IEnumerable<DMErrorMHTC> _userErrList = new List<DMErrorMHTC>();
            IEnumerable<ChiDinhDongThoi> _ruleDongThois = new List<ChiDinhDongThoi>();
            IEnumerable<PhacDoDieuTri> _rulePhacDos = new List<PhacDoDieuTri>();
            IEnumerable<DMChung130> _dmChung = new List<DMChung130>();
            IEnumerable<DMError130> _dmError130 = new List<DMError130>();
            IEnumerable<DMErrorMHTC> _dMErrorMHTCs = new List<DMErrorMHTC>();
            IEnumerable<DichVuMaMay> _dvuMamays = new List<DichVuMaMay>();

            //1250	1	
            //	MA_DICH_VU	Bệnh nhân Ngoại trú nhưng kê giường nội trú
            //1251  1   XML3    MA_DICH_VU  Kê giường ngoại khoa sai TT39/ 2018
            Dictionary<byte, string> _dicPhauThuats = await GetDMPhauThuat();
            Dictionary<string, string> _dicDMDB = await GetDMDacBiet();
            Dictionary<string, string> _dicGiuongs = await GetDMGiuong(maCSKCB);

            try
            {
                //Linhist changed 20/03/26 - selected before
                //_hospital = await _hospitalRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);

                _ruleThuocs = await _HDSDThuocRepository.GetActiveRules(maCSKCB);
                _ruleDichVus = await _HDSDDichVuRepository.GetActiveRules(maCSKCB);
                _dmBacSi = await _DMBacSiRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dmKhoa = await _DMKhoaRepository.GetListAsync();
                _dmKhoaBV = await _DMKhoaBVRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dmYHCTs = await _DMYHDTRepository.GetListAsync();
                _dmTuyenTinhs = await _DMBVTuyenTinhRepository.GetListAsync();
                _userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(maCSKCB), null, null);
                _ruleDongThois = await _ChiDinhDongThoiRepository.GetListAsync(x => true.Equals(x.HIEU_LUC));
                _rulePhacDos = await _PhacDoDieuTriRepository.GetListAsync();
                _dmChung = await _DMChungRepository.GetListAsync();
                _dmThuCong = await _DMThuCongRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dmThietBi = await _DMThietBiRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dmError130 = await _dmErrorService.GetAll();
                _dMErrorMHTCs = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                _dvuMamays = await _dvuMaMayRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);

                //Linhist added 110926 - loại bỏ lỗi đã xóa trên hệ thống
                _dMErrorMHTCs = _dMErrorMHTCs.Where(e => _dmError130.Any(x => e.EMErrorCode == x.ErrorCode && x.IsDelete != true)).ToList();

                if (fromAPI || fromHIS)
                {
                    var maThuocNeed = xml01s.SelectMany(y => y.XML02s).Select(y => y.MA_THUOC).ToList();
                    _dmThuoc = await _DMThuocRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && maThuocNeed.Contains(x.MA_THUOC));

                    var maDMVTYTNeed = xml01s.SelectMany(y => y.XML03s).Select(y => y.MA_VAT_TU).ToList();
                    _dmVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.MA_VAT_TU != null && x.MA_VAT_TU != "" && maDMVTYTNeed.Contains(x.MA_VAT_TU));

                    var maDichVuNeed = xml01s.SelectMany(y => y.XML03s).Select(y => y.MA_DICH_VU).ToList();
                    _dmDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && x.MA_DICH_VU != null && x.MA_DICH_VU != "" && maDichVuNeed.Contains(x.MA_DICH_VU));
                }
                else
                {
                    _dmThuoc = await _DMThuocRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                    _dmVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                    _dmDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB);
                }
            }
            catch
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: khong lay duoc danh sach cac DM phu tro", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return;
            }

            DateTime dv = DateTime.Now;

            //Note Chưa hiểu sao lại khao báo mấy phần này
            string allMaDVRuleDTs = string.Join(';', _ruleDongThois.Select(x => x.MA_DICH_VU).ToArray());
            string allMaDVKhacRuleDTs = string.Join(';', _ruleDongThois.Select(x => x.DICH_VU_KHAC).ToArray());
            string allMAVTRuleDTs = string.Join(';', _ruleDongThois.Select(x => x.VAT_TU_BHYT).ToArray());

            var allDichVuDTs = new List<XML03>();
            var allDvKhacDTs = new List<XML03>();
            var allVatTuDTs = new List<XML03>();
            var xml3CungNgays = new List<XML03>();
            var dicErrors = _dmError130.ToDictionary(x => x.ErrorCode.Value, x => x.Message);
            #endregion

            try
            {
                //var x01Validator = new ValidateXML01(_officialTime, _hosoRepository, _dicDMDB, _dmYHCTs, _dmBacSi, _dmKhoaBV, _dmChung,_dmError130, _dMErrorMHTCs, _logger);
                var x01Validator = new ValidateXML01(_officialTime, _hosoRepository, _dmYHCTs, _dmBacSi, _dmKhoaBV, _dmChung, _dicDMDB, dicErrors, _logger);
                var x23Validator = new ValidateAllXML23(_officialTime, _dmBacSi, _userErrList, _dmYHCTs, _dicDMDB, _dicGiuongs, _dicPhauThuats, _ruleDongThois, _dmThuoc, _dmKhoaBV, _dmDichVu, _dmVTYT, _dmBacSi, _dmThuCong, _dmThietBi, _ruleDichVus, _ruleThuocs,_dmChung,_dmError130, _hosoRepository,_xml03Repository, _xml02Repository, _dMErrorMHTCs, _dvuMamays, _logger);
                var x04Validator = new ValidateXML04(_officialTime, _dmDichVu, _dmBacSi,_dmError130,_dmChung, _logger);

                var x05Validator = new ValidateXML05(_dmBacSi, _dmError130, _dmChung, _logger);
                var x06Validator = new ValidateXML06(_dmBacSi, _logger);
                var x07Validator = new ValidateXML07(_dmBacSi, _logger);
                var x08Validator = new ValidateXML08(_dmBacSi, _logger);
                var x09Validator = new ValidateXML09(_dmBacSi, _logger);
                var x10Validator = new ValidateXML10(_dmBacSi, _logger);
                var x11Validator = new ValidateXML11(_dmBacSi, _logger);
                var x13Validator = new ValidateXML13(_dmYHCTs, _dmBacSi, _logger);
                var x14Validator = new ValidateXML14(_dmYHCTs, _dmBacSi, _logger);
                var x15Validator = new ValidateXML15(_dmBacSi, _logger);

                //Linhist added 15/05/26
                var ttValidator = new ValidateXMLTT(_checkTheRepository, _logger);

                var dte130To3176 = new DateTime(2026, 2, 1);
                //Uti.HandleCronJobLog(_logger, $"Validate130: Giám định start B1. CS: {maCSKCB} Số lượng: {xml01s.Count()} thời gian: {DateTime.Now.ToString()}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                foreach (var x01 in xml01s)
                {
                    if (x01.IsDelete == true) continue;

                    string[] maThes = new string[0];
                    DateTime gttheTU = DateTime.MinValue;
                    DateTime gttheTU2 = DateTime.MinValue;
                    DateTime gttheDEN = DateTime.MinValue;
                    DateTime gttheDEN2 = DateTime.MinValue;

                    byte?[] mucHuongs = new byte?[] { null, null, null, null };

                    var xmlCommon = new XMLCommonInfo(x01);

                    #region XML01
                    try
                    {
                        if (!await x01Validator.Validate(x01, xmlCommon)) continue;
                        //Qualify_XML01(x01);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML01 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                    #endregion

                    #region XML02
                    //var sortedX02s = x01.XML02s.OrderBy(x => x.MA_THUOC);
                    //foreach (var x02 in sortedX02s)
                    //{
                    //    try
                    //    {
                    //        x02Validator.Validate(x02, xmlCommon);
                    //        //Qualify_XML02(x02);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML02 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //    }
                    //}
                    #endregion

                    #region XML03
                    //var sortedX03s = x01.XML03s.OrderBy(x => x.MA_DICH_VU).ThenBy(x => x.MA_VAT_TU);
                    //foreach (var x03 in sortedX03s)
                    //{
                    //    try
                    //    {
                    //        x03Validator.Validate(x03);
                    //        //Qualify_XML03(x03);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML03 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //    }
                    //}
                    try
                    {
                        //await Task.Delay(100);

                        await x23Validator.Validate(x01, xmlCommon);
                        //Qualify_XML03(x03);

                        /// Kiểm tra thông tin HDSD Tối thiểu phải chạy ở đây, ko check trong ValidateXML23 được vì trong ValidateXML23 ko có Repository để select vào DB
                      //  await Validate_Thoigian_Toithieu(x23Validator.All_XML03_ToiThieus, x23Validator.All_HDSD_ToiThieus);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML23 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                    #endregion

                    #region XML04
                    var sortedX04s = x01.XML04s.OrderBy(x => x.MA_DICH_VU);
                    foreach (var x04 in sortedX04s)
                    {
                        try
                        {
                            await x04Validator.Validate(x04);
                            //Qualify_XML04(x04);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML04 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }

                    Validate_DacBiet_XML4(x01);
                    #endregion

                    #region XML05
                    foreach (var x05 in x01.XML05s)
                    {
                        try
                        {
                            await x05Validator.Validate(x05);
                            //Qualify_XML05(x05);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML05 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML06
                    foreach (var x06 in x01.XML06s)
                    {
                        try
                        {
                            await x06Validator.Validate(x06);
                            //Qualify_XML06(x06);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML06 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML07
                    foreach (var x07 in x01.XML07s)
                    {
                        if (x07 is null)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML07 loi null HOSO {x01.MA_LK}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                        else
                        {
                            try
                            {
                                //await Task.Delay(100);
                                await x07Validator.Validate(x07);

                                //Qualify_XML07(x07);
                            }
                            catch (Exception ex)
                            {
                                Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML07 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                            }
                        }
                    }
                    #endregion

                    #region XML08
                    foreach (var x08 in x01.XML08s)
                    {
                        try
                        {
                            await x08Validator.Validate(x08);
                            //Qualify_XML08(x08);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML08 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML09
                    foreach (var x09 in x01.XML09s)
                    {
                        try
                        {
                            await x09Validator.Validate(x09);
                            //Qualify_XML09(x09);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML09 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML10
                    foreach (var x10 in x01.XML10s)
                    {
                        try
                        {
                            await x10Validator.Validate(x10);
                            //Qualify_XML10(x10);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML10 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML11
                    foreach (var x11 in x01.XML11s)
                    {
                        try
                        {
                            await x11Validator.Validate(x11);
                            //Qualify_XML11(x11);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML11 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML13
                    foreach (var x13 in x01.XML13s)
                    {
                        try
                        {
                            await x13Validator.Validate(x13);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML13 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML14
                    foreach (var x14 in x01.XML14s)
                    {
                        try
                        {
                            await x14Validator.Validate(x14);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML14 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion

                    #region XML15
                    foreach (var x15 in x01.XML15s)
                    {
                        try
                        {
                            await x15Validator.Validate(x15);
                        }
                        catch (Exception ex)
                        {
                            Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so XML15 loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        }
                    }
                    #endregion


                    //Linhist added 15/05/26
                    #region CHECKTHE THONG TUYEN
                    try
                    {
                        await ttValidator.Validate(x01);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: giam dinh ho so CHECK THE THONG TUYEN loi: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }
                    #endregion

                    //Linhist added 09/02/26
                    //Trước ngày 01/02/2026 bỏ mã lỗi 3176
                    if (x01.NGAY_VAO != null && x01.NGAY_VAO < dte130To3176)
                    {
                        x01.RemoveErrorsByCodes(2673, 2674, 2692, 2693, 2698, 2699, 2702, 2703, 2704, 2865, 2868, 2869, 2876, 2877, 2881, 2882, 2883, 2884);
                    }
                    //Từ ngày 01/02/2026 bỏ mã lỗi 130
                    else
                    {
                        x01.RemoveErrorsByCodes(1852, 1869, 1910, 1924, 1948, 2023, 2024, 2093, 2096, 2159, 2165, 2171, 2190, 2324, 2402, 2403, 2404, 2405, 2406, 2407, 2408, 2409, 2410, 2411, 2412, 2413);
                    }
                }

                //Uti.HandleCronJobLog(_logger, $"Validate130: Giám định start B2. CS: {maCSKCB} Số lượng: {xml01s.Count()} thời gian: {DateTime.Now.ToString()}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(ex, false, _logger, "Validate130");

                var LineNumber = 0;
                if ((new StackTrace(ex, true)).GetFrames().Any())
                {
                    LineNumber = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                }
                //Linhist removed 29/03/25
                //await _ImportErrorRepository.InsertAsync(
                //new ImportError
                //{
                //    ImportErrorFile = "Validate XML130",
                //    ImportErrorAt = ex.GetType().ToString(),
                //    ImportErrorMessage = $"{ex.Message} Validate XML130",
                //    ImportErrorDate = DateTime.Now,
                //    ImportErrorMHTCID = LineNumber

                //});
                await _unitOfWork.CommitAsync();
            }

            try
            {
                //13.2.2025 checkchange haimaster
                //var checkchanges = new List<CheckChanges>();
                //var userlog = new List<UserLogs>();
                var errors = new List<XMLError>();
                //var states = new List<XMLState>();
                var lstX0s = new List<XMLState>();
                //Linhist removed 22/05/25 - ko check thong tuyen lai nua
                //var verifys = new List<XMLVerify>();
                //var errFilters = await _dmErrorMHTCRepo.GetListAsync(e => e.MA_CSKCB == maCSKCB);
                var errCodes = _dMErrorMHTCs.Select(e => e.EMErrorCode).ToHashSet();
                var altCodes = _dMErrorMHTCs.Where(e => e.EMEnabled == false).Select(e => e.EMErrorCode).ToList();

                DateTime? lastChecked = DateTime.Now;
               // var mustVerify = !string.IsNullOrEmpty(hospital.Username) && !string.IsNullOrEmpty(hospital.Password);
                foreach (var x01 in xml01s)
                {
                    if (x01.IsDelete == true) continue;

                    var allErrs = "";
                    var e01s = x01.Errors.Where(e => e.MA_LOI.HasValue &&  errCodes.Contains(e.MA_LOI)).OrderBy(e => e.MA_LOI).Distinct().ToList();
                    foreach (var e in e01s)
                    {
                        allErrs += $"{e.MA_LOI};";
                        e.XML01 = null;
                        //checkchange only 25001 giờ update hết
                        //if ("34341;25001".Contains(x01.MA_CSKCB))
                        //{
                            //checkchanges.Add(new CheckChanges()
                            //{
                            //    XML01ID = x01.XML01ID,
                            //    MA_CSKCB = x01.MA_CSKCB,
                            //    MA_LK = x01.MA_LK,
                            //    MA_BN = x01.MA_BN,
                            //    MA_THE_BHYT = x01.MA_THE_BHYT,
                            //    HO_TEN = x01.HO_TEN,
                            //    MA_KHOA = x01.MA_KHOA,
                            //    STT = e.STT,
                            //    MA_LOI = e.MA_LOI,
                            //    NGAY_TAO = lastChecked,
                            //});
                        //}
                    }

                    //Linhist changed 30/07/26
                    //var state = x01.State ?? new XMLState()
                    //{
                    //    XML01ID = x01.XML01ID,
                    //    MA_CSKCB = x01.MA_CSKCB,
                    //    VerifyResult = null,//x01.State?.VerifyResult;
                    //    TotalErrors = e01s.Count,
                    //    TotalAlerts = e01s.Count(e => altCodes.Contains(e.MA_LOI)),
                    //    LastValidate = DateTime.Now,
                    //    LastVerify = null,//x01.State?.LastVerify
                    //};

                    //var state = x01.State ?? new XMLState()
                    //{
                    //    XML01ID = x01.XML01ID,
                    //    MA_CSKCB = x01.MA_CSKCB
                    //};

                    //state.XML01 = null;
                    //state.LastValidate = DateTime.Now;
                    //state.TotalErrors = e01s.Count;
                    //state.TotalAlerts = e01s.Count(e => altCodes.Contains(e.MA_LOI));
                    var x00 = new XMLState
                    {
                        XML01ID = x01.XML01ID,
                        MA_CSKCB = x01.MA_CSKCB,
                        MA_LK = x01.MA_LK,
                        XML01 = null,
                        DS_LOI = allErrs,
                        TONG_SO_LOI = e01s.Count,
                        CANH_BAO = e01s.Count(e => altCodes.Contains(e.MA_LOI)),
                        Counter = x01.State == null ? 1 : x01.State.Counter + 1,
                        CheckDate = DateTime.Now,
                        VerifyDate = x01.State?.VerifyDate ?? DateTime.Now,
                        Checker = userEmail,
                        CreatedDate = x01.CreateDate ?? DateTime.Now,
                    };
                    x01.State = null;
                    lstX0s.Add(x00);

                    //if (mustVerify)
                    //{
                    //Linhist changed 08/04/25
                    //if (x01.State?.LastVerify == null && !string.IsNullOrEmpty(x01.MA_THE_BHYT?.Trim()))
                    //{
                    //    verifys.Add(new XMLVerify()
                    //    {
                    //        XML01ID = x01.XML01ID,
                    //        MA_CSKCB = x01.MA_CSKCB,
                    //    });
                    //}

                    //Linhist removed 15/05/26
                    //var verified = await VerifyXML(errors, errCodes, altCodes, x01, state);

                    //Linhist removed 22/05/25 - ko check thong tuyen lai nua
                    //if (!verified && x01.State?.LastVerify == null && !string.IsNullOrEmpty(x01.MA_THE_BHYT?.Trim()))
                    //{
                    //    verifys.Add(new XMLVerify()
                    //    {
                    //        XML01ID = x01.XML01ID,
                    //        MA_CSKCB = x01.MA_CSKCB,
                    //    });
                    //}
                    //}

                    errors.AddRange(e01s);

                    //Linhist changed 08/04/25
                    //states.Add(new XMLState()
                    //{
                    //    XML01ID = x01.XML01ID,
                    //    MA_CSKCB = x01.MA_CSKCB,
                    //    VerifyResult = null,//x01.State?.VerifyResult,
                    //    TotalErrors = e01s.Count(),
                    //    TotalAlerts = e01s.Count(e => altCodes.Contains(e.MA_LOI)),
                    //    LastValidate = DateTime.Now,
                    //    LastVerify = null,//x01.State?.LastVerify
                    //});
                    //states.Add(state);
                    //x01.State = null;
                }
                try
                {

                    try
                    {
                        await _errorRepository.DeleteListByFilterAsync(x => x.MA_CSKCB == maCSKCB && xmlIds.Contains(x.XML01ID));
                    //}
                    //catch (Exception ex)
                    //{
                    //    Uti.HandleCronJobLog(_logger, $"Validate130: Xoa XMLError loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //}
                    //try
                    //{
                        await _unitOfWork.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: Commit xoa XML Error :{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }

                    //try
                    //{
                    //    await _stateRepository.DeleteListByFilterAsync(x => x.MA_CSKCB == maCSKCB && xmlIds.Contains(x.XML01ID));
                    //}
                    //catch(Exception ex)
                    //{
                    //    Uti.HandleCronJobLog(_logger, $"Validate130: Xoa XMLState:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //}

                    //try
                    //{
                    //    await _unitOfWork.CommitAsync();
                    //}
                    //catch (Exception ex)
                    //{
                    //    Uti.HandleCronJobLog(_logger, $"Validate130: Commit xoa XML State:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //}

                    try
                    {
                        await _stateRepository.DeleteListByFilterAsync(x => x.MA_CSKCB == maCSKCB && xmlIds.Contains(x.XML01ID));
                    //}
                    //catch (Exception ex)
                    //{
                    //    Uti.HandleCronJobLog(_logger, $"Validate130: Xoa XML00:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //}

                    //try
                    //{
                        await _unitOfWork.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: Commit xoa XML00:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }

                    //await RemoveHosoFromVerify(xmlIds);
                    try
                    {
                        //var es = errors.Where(e => e.ErrorDesc?.Length > 4000).ToList();
                        //var er = errors.Where(e => e.ErrorDesc?.Length > 2000).ToList();
                        await _errorRepository.InsertListAsync(errors);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Uti.HandleCronJobLog(_logger, $"Validate130: Luu XMLError loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //}
                    //try
                    //{
                        await _unitOfWork.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: Commit luu XMLError loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }

                    //try
                    //{
                    //    await _stateRepository.InsertListAsync(states);
                    //    //}catch(Exception ex)
                    //    //{
                    //    //    Uti.HandleCronJobLog(_logger, $"Validate130: Luu XMLState loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //    //}
                    //    //try
                    //    //{
                    //    await _unitOfWork.CommitAsync();
                    //}
                    //catch (Exception ex)
                    //{
                    //    Uti.HandleCronJobLog(_logger, $"Validate130: Commit luu XMLState:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //}

                    try
                    {
                        await _stateRepository.InsertListAsync(lstX0s);
                        //}catch(Exception ex)
                        //{
                        //    Uti.HandleCronJobLog(_logger, $"Validate130: Luu XMLState loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //}
                        //try
                        //{
                        await _unitOfWork.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: Commit luu XML00:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    }

                    //checkchange
                    //if (CheckchangeVIP.Contains(maCSKCB))
                    //{
                    //    try
                    //    {

                    //        await _checkChangesRepository.InsertListAsync(checkchanges);

                    //    //}
                    //    //catch (Exception ex)
                    //    //{
                    //    //    Uti.HandleCronJobLog(_logger, $"Validate130: Luu CheckChanges loi:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //    //}
                    //    ////await _userlogRepository.InsertListAsync(userlog);
                    //    //try
                    //    //{
                    //            await _unitOfWork.CommitAsync();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Uti.HandleCronJobLog(_logger, $"Validate130: Commit luu loi va XML State:{maCSKCB}- {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //    }
                    //}

                    //Linhist removed 22/05/25 - ko check thong tuyen lai nua
                    //await AppendHosoToVerify(hospital, xmlIds);
                    try
                    {
                        await RemoveHosoFromValidate(xmlIds);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, $"Validate130: Lỗi Bước xóa XML đã giám định tại Validate {maCSKCB}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Validate130: Luu ket qua giam dinh loi loi {maCSKCB}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            catch(Exception ex)
            { 
                _logger.LogError(ex, $"Validate130: Luu ket qua giam dinh loi loi {maCSKCB}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }


        //Mac dinh includes se lay tat ca XML tu 02-11: "2;3;4;5;6;7;8;9;10;11;13;14;15"
        //Neu chi dinh ro [inners] thi se lay XML theo so chi dinh vd: "2;3;5;7"
        public async Task<bool> AttachInnerXML(IEnumerable<XML01> xmls, string innerList = null)
        {
            //Linhist changed 02/03/26
            //innerList = innerList?.Trim();
            //string[] inners = "2;3;4;5;6;7;8;9;10;11;13;14;15".Split(';');
            //if (!string.IsNullOrEmpty(innerList))
            //{
            //    var lst = innerList.Split(';', StringSplitOptions.RemoveEmptyEntries);
            //    if (lst.Length > 0)
            //    {
            //        inners = lst;
            //    }
            //}
            //fullLoadXML = true;

            if (xmls == null || !xmls.Any()) return false;
            var inners = (string.IsNullOrWhiteSpace(innerList) ? "2;3;4;5;6;7;8;9;10;11;13;14;15" : innerList).Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (inners.Length == 0)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: List attached XML not exists", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return false;
            }

            try
            {
                //Linhist changed 02/03/26
                //string maCSKCB = "";
                //if (xmls == null || !xmls.Any())
                //{
                //    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML xmls null or empty", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                //    fullLoadXML = false;
                //    return;
                //}
                //try
                //{
                //    lstIds = xmls.Select(x => x.XML01ID).ToList();
                //    maCSKCB = xmls.Select(x=>x.MA_CSKCB).FirstOrDefault();
                //}
                //catch(Exception ex)
                //{
                //    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML GetInfor XML01IDLIST & ma_CSKCB xmls{xmls.Count()}: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                //    fullLoadXML = false;
                //}

                var maCSKCB = xmls.FirstOrDefault()?.MA_CSKCB;
                var lstIds = xmls.Select(x => x.XML01ID).ToArray();

                IEnumerable<XML02> xml02s = null;
                IEnumerable<XML03> xml03s = null;
                IEnumerable<XML04> xml04s = null;
                IEnumerable<XML05> xml05s = null;
                IEnumerable<XML06> xml06s = null;
                IEnumerable<XML07> xml07s = null;
                IEnumerable<XML08> xml08s = null;
                IEnumerable<XML09> xml09s = null;
                IEnumerable<XML10> xml10s = null;
                IEnumerable<XML11> xml11s = null;
                IEnumerable<XML13> xml13s = null;
                IEnumerable<XML14> xml14s = null;
                IEnumerable<XML15> xml15s = null;

                try
                {
                    xml02s = !inners.Contains("2") ? null : await _hosoRepository.GetInnersByListID<XML02>(maCSKCB, lstIds);
                }
                catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML02: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml03s = !inners.Contains("3") ? null : await _hosoRepository.GetInnersByListID<XML03>(maCSKCB, lstIds);
                }
                catch(Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML03: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                } 
                try
                {
                    xml04s = !inners.Contains("4") ? null : await _hosoRepository.GetInnersByListID<XML04>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML04: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml05s = !inners.Contains("5") ? null : await _hosoRepository.GetInnersByListID<XML05>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML05: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml06s = !inners.Contains("6") ? null : await _hosoRepository.GetInnersByListID<XML06>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML06: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml07s = !inners.Contains("7") ? null : await _hosoRepository.GetInnersByListID<XML07>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML07: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml08s = !inners.Contains("8") ? null : await _hosoRepository.GetInnersByListID<XML08>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML08: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml09s = !inners.Contains("9") ? null : await _hosoRepository.GetInnersByListID<XML09>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML09: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml10s = !inners.Contains("10") ? null : await _hosoRepository.GetInnersByListID<XML10>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML10: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml11s = !inners.Contains("11") ? null : await _hosoRepository.GetInnersByListID<XML11>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML11: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml13s = !inners.Contains("13") ? null : await _hosoRepository.GetInnersByListID<XML13>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML13: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml14s = !inners.Contains("14") ? null : await _hosoRepository.GetInnersByListID<XML14>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML14: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }
                try
                {
                    xml15s = !inners.Contains("15") ? null : await _hosoRepository.GetInnersByListID<XML15>(maCSKCB, lstIds);

                }
                catch (Exception ex)
                {
                    Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML XML15: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                    //fullLoadXML = false;
                    return false;
                }

                foreach (var xml01 in xmls)
                {
                    try
                    {
                        var x02 = xml02s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x02 != null) xml01.AddRange(x02);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML02: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x03 = xml03s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x03 != null) xml01.AddRange(x03);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML03: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }

                    try
                    {
                        var x04 = xml04s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x04 != null) xml01.AddRange(x04);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML04: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x05 = xml05s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x05 != null) xml01.AddRange(x05);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML05: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x06 = xml06s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x06 != null) xml01.AddRange(x06);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML06: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x07 = xml07s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x07 != null) xml01.AddRange(x07);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML07: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x08 = xml08s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x08 != null) xml01.AddRange(x08);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML08: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x09 = xml09s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x09 != null) xml01.AddRange(x09);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML09: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x10 = xml10s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x10 != null) xml01.AddRange(x10);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML10: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x11 = xml11s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x11 != null) xml01.AddRange(x11);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML11: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x13 = xml13s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x13 != null) xml01.AddRange(x13);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML13: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x14 = xml14s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x14 != null) xml01.AddRange(x14);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML14: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                    try
                    {
                        var x15 = xml15s?.Where(x => x.XML01ID == xml01.XML01ID)?.ToList();
                        if (x15 != null) xml01.AddRange(x15);
                    }
                    catch (Exception ex)
                    {
                        Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML - AttachXML15: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                        //fullLoadXML = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Validate130: AttachInnerXML: {ex.Message}{Environment.NewLine}{ex.StackTrace}", $"{nameof(HoSoManagementService130)} - {nameof(ValidateXML)}");
                return false;
            }

            return true;
        }

        //Mac dinh includes se lay tat ca XML tu 02-11: "2;3;4;5;6;7;8;9;10;11;13;14;15"
        //Neu chi dinh ro [inners] thi se lay XML theo so chi dinh vd: "2;3;5;7"
        public async Task<bool> AttachXMLState(IEnumerable<XML01> xmls)
        {
            var lastID = 0L;
            try
            {
                var maCSKCB = xmls.FirstOrDefault().MA_CSKCB;
                var lstIds = xmls.Select(x => x.XML01ID).ToList();
                var xml00s = await _stateRepository.GetListAsync(x => x.MA_CSKCB == maCSKCB && lstIds.Contains(x.XML01ID));

                foreach (var xml01 in xmls)
                {
                    lastID = xml01.XML01ID;
                    xml01.State = xml00s.FirstOrDefault(x => x.XML01ID == xml01.XML01ID) ?? new XMLState()
                    {
                        XML01 = xml01,
                        XML01ID = xml01.XML01ID,
                        MA_CSKCB = xml01.MA_CSKCB,
                        MA_LK = xml01.MA_LK,
                        DS_LOI = string.Empty,
                        TONG_SO_LOI = 0,
                        CANH_BAO = 0,
                        CheckDate = DateTime.Now,
                        Checker = string.Empty,
                        Counter = 0,
                        IsUpload = xml01.IsUpload,
                        VerifyDate = null,
                    };
                }
                return true;
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(AttachInnerXML)}");
                _logger.LogError(ex, $"Validate130: Lỗi AttachXML00 - tới ID <{lastID}>: {xmls.Count()}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            return false;
        }

        #region Get List All Categories
        private async Task<Dictionary<string, string>> GetDMDacBiet()
        {
            var _DMDacBiet = await _DMDacBietRepository.GetListAsync();
            var dic = new Dictionary<string, string>();
            foreach (var db in _DMDacBiet)
            {
                if (db.DMDacBietName == null && db.DMDacBietName.Contains("-bak")) continue;
                if (char.IsDigit(db.DMDacBietName[0]))
                {
                    dic.Add(db.DMDacBietValue, db.DMDacBietName);
                }
                else
                {
                    dic.Add(db.DMDacBietName?.Trim().ToLower(), db.DMDacBietValue);
                }
            }

            if (!dic.ContainsKey("congkham")) dic.Add("congkham", "");
            return dic;
        }

        private async Task<Dictionary<string, string>> GetDMGiuong(string maCSKCB)
        {
            var dic = new Dictionary<string, string>();
            var dmGiuongs = await _DMGiuongRepository.GetListAsync(g => !string.IsNullOrEmpty(g.GiuongCode) && g.GiuongType != null && g.MA_CSKCB == maCSKCB);

            //DM Giuong Ngoai tru
            var giuongNgoais = dmGiuongs.Where(g => g.GiuongType == 1);
            var codeNgoais = giuongNgoais.Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
            dic.Add($"GiuongNgoaiAll", string.Join(";", codeNgoais));

            var clsNgoais = giuongNgoais.Select(g => g.GiuongClass).Distinct().OrderBy(c => c);
            foreach (var cls in clsNgoais)
            {
                var codes = giuongNgoais.Where(g => g.GiuongClass == cls).Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
                dic.Add($"GiuongNgoai{cls}", string.Join(";", codes));
            }

            //DM Giuong Noi tru
            var giuongNois = dmGiuongs.Where(g => g.GiuongType == 2);
            var codeNois = giuongNois.Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
            dic.Add($"GiuongNoiAll", string.Join(";", codeNois));

            var clsNois = giuongNois.Select(g => g.GiuongClass).Distinct().OrderBy(c => c);
            foreach (var cls in clsNois)
            {
                var codes = giuongNois.Where(g => g.GiuongClass == cls).Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
                dic.Add($"GiuongNoi{cls}", string.Join(";", codes));
            }

            //DM Giuong Cap Cuu
            var codeCapCuus = dmGiuongs.Where(g => g.GiuongType == 3).Select(g => g.GiuongCode).Distinct().OrderBy(c => c);
            dic.Add($"GiuongCapCuu", string.Join(";", codeCapCuus));


            return dic;
        }

        private async Task<Dictionary<byte, string>> GetDMPhauThuat()
        {
            //DM Phhau thuat Giuong Loai
            var dic = new Dictionary<byte, string>();
            var dmPhauThuats = await _DMPhauThuatRepository.GetListAsync(p => p.PhauThuatGiuongLoai != null);
            var giuongLoais = dmPhauThuats.Select(p => p.PhauThuatGiuongLoai).Distinct().OrderBy(l => l);
            foreach (var l in giuongLoais)
            {
                if (l == null) continue;
                var codes = dmPhauThuats.Where(p => p.PhauThuatGiuongLoai == l).Select(p => p.PhauThuatCode).Distinct().OrderBy(c => c);
                dic.Add(l.Value, string.Join(";", codes));
            }
            return dic;
        }
        #endregion

        private const string strDVXml4 = "20.0059.0140;20.0059.0140;02.0295.0498;02.0295.0498;02.0295.0498;02.0295.0498;02.0286.0497;02.0286.0497;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0296.0500;02.0282.0318;02.0282.0318;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0290.0500;02.0271.0140;02.0271.0140;20.0076.0140;01.0232.0140;01.0232.0140;02.0307.0136;02.0307.0136;02.0307.0136;02.0307.0136;02.0307.0136;02.0262.0136;02.0262.0136;20.0053.0105;20.0053.0105;02.0217.0183;02.0369.0185;01.0101.0125;02.0038.0125;02.0283.0141;02.0284.0141;02.0275.0141;02.0275.0141;02.0275.0141;02.0275.0141;20.0055.0496;20.0056.0141;02.0036.0127;02.0036.0127;02.0036.0128;02.0036.0128;02.0045.0130;02.0045.0130;02.0045.0132;02.0045.0132;02.0043.0127;02.0043.0127;02.0304.0134;02.0304.0134;02.0304.0134;02.0304.0134;02.0304.0134;02.0309.0138;02.0309.0138;02.0309.0138;20.0072.0191;20.0044.0503;03.2340.0157;03.2340.0157";
        
        private void Validate_DacBiet_XML4(XML01 x01)
        {
            //431 	1 	XML4 	KET_LUAN 	Dịch vụ Giải phẫu bệnh nhuộm hai màu không có kết luận Trợt, loét, polyp, U hoặc Khối
            var allX04s = x01.XML04s.Where(x => Uti.CodeContain(x.MA_DICH_VU, strDVXml4));
            if (!allX04s.Any()) return;

            if (x01.XML05s.Any(x => Uti.StrExists(x.DIEN_BIEN_LS, true, "Trợt", "loét", "polyp", "U hoặc Khối"))) return;

            foreach (var x04 in allX04s)
            {
                x04.RaiseError(431);
            }
        }

        private async Task Validate_Thoigian_Toithieu(List<XML03> xml3ToiThieu, List<HDSDDichVu> hdsdToiThieu)
        {
            //Lay ngay toi thieu & ngay toi da can kiem tra
            XML03 xml3 = null;
            HDSDDichVu rule = null;
            DateTime dMin = DateTime.MaxValue;
            //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
            //DateTime dMax = DateTime.MinValue;

            for (int i = 0; i < xml3ToiThieu.Count; i++)
            {
                xml3 = xml3ToiThieu[i];
                rule = hdsdToiThieu[i];

                DateTime dTmp = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);
                if (dTmp < dMin)
                {
                    dMin = dTmp;
                }
            }

            try
            {
                //Lay list XML1 lich su trong khoang thoi gian can kiem tra
                var xml1Lichsu = await _hosoRepository.GetListAsync(x => x.XML01ID != xml3.XML01ID && x.IsDelete != true && x.MA_BN == xml3.XML01.MA_BN && x.MA_CSKCB == xml3.XML01.MA_CSKCB && x.MA_LK != xml3.MA_LK && x.NGAY_VAO != null && x.NGAY_VAO >= dMin && x.NGAY_RA != null && x.NGAY_RA <= xml3.NGAY_YL.Value);
                List<long> xml1IDs = xml1Lichsu.Select(x => x.XML01ID).ToList();

                //Lay list XML3 lich su trong khoang thoi gian can kiem tra
                var xml3Lichsu = await _xml03Repository.GetListAsync(x => x.MA_LK != xml3.MA_LK && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= xml3.NGAY_YL.Value && xml1IDs.Contains(x.XML01ID) && x.MA_CSKCB == xml3.MA_CSKCB);
                if (xml3Lichsu == null || !xml3Lichsu.Any()) return;

                for (int i = 0; i < xml3ToiThieu.Count; i++)
                {
                    xml3 = xml3ToiThieu[i];
                    rule = hdsdToiThieu[i];
                    dMin = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);

                    //Kiem tra voi all xml3 lich su
                    var xml3GanNhat = xml3Lichsu.Where(x => x.NGAY_YL >= dMin && x.NGAY_YL <= xml3.NGAY_YL.Value);
                    if (true.Equals(rule.CHI_DINH) != xml3GanNhat.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU)))
                    {
                        xml3.RaiseError(int.Parse(rule.MA_LOI));
                    }
                }
            }
            catch
            { }
        }

        //private async Task<IEnumerable<XMLResult>> GetResult_NeedVerify(string MA_CSKCB)
        //{
        //    //Tinh toan so ban ghi moi lan chay
        //    int maxVerifyInTime = _appSettings.BackgroundServices.VerifyXML.MinuteVerifyTime * 60;
        //    maxVerifyInTime = (int)Math.Floor((double)maxVerifyInTime - (maxVerifyInTime * _appSettings.BackgroundServices.VerifyXML.PercentItemInTime / 100));

        //    //Tam thoi fix so ban ghi 1 lan chay
        //    maxVerifyInTime = 30;
        //    //var thes = await _checkTheRepository.GetListPaginationAsync(0, maxVerifyInTime, c => c.MA_CSKCB == MA_CSKCB && c.TG_CHECK_THE == null, t => t.OrderByDescending(c => c.XML1ID));
        //    var results = await _resultRepository.GetListPaginationAsync(0, maxVerifyInTime, c => c.MA_CSKCB == MA_CSKCB, t => t.OrderByDescending(c => c.XML01ID));
        //    if (results == null || !results.Items.Any()) return null;
        //    return results.Items;
        //}

        //private async Task<IEnumerable<XMLVerify>> GetThe_Info_NeedVerify(string MA_CSKCB)
        //{
        //    //Tinh toan so ban ghi moi lan chay
        //    int maxVerifyInTime = _appSettings.BackgroundServices.VerifyXML.MinuteVerifyTime * 60;
        //    maxVerifyInTime = (int)Math.Floor((double)maxVerifyInTime - (maxVerifyInTime * _appSettings.BackgroundServices.VerifyXML.PercentItemInTime / 100));

        //    //Tam thoi fix so ban ghi 1 lan chay
        //    maxVerifyInTime = 30;
        //    //var thes = await _checkTheRepository.GetListPaginationAsync(0, maxVerifyInTime, c => c.MA_CSKCB == MA_CSKCB && c.TG_CHECK_THE == null, t => t.OrderByDescending(c => c.XML1ID));
        //    var thes = await _verifyRepository.GetListPaginationAsync(0, maxVerifyInTime, c => c.MA_CSKCB == MA_CSKCB , t => t.OrderByDescending(c => c.XML01ID));
        //    if (thes == null || !thes.Items.Any()) return null;
        //    return thes.Items;
        //}

        //private int _count_Login_fail = 0;
        
        //public async Task<XML01> GetLastXMLNeedVerify(string maCSKCB)
        //{
        //    var xmlVerify = await _verifyRepository.GetItemOrderBy(c => c.MA_CSKCB == maCSKCB, t => t.OrderByDescending(c => c.XML01ID));
        //    if (xmlVerify == null) return null;

        //    await RemoveHosoFromVerify(xmlVerify.XML01ID);
        //    return await _hosoRepository.GetById(xmlVerify.XML01ID);
        //}

        //// To remove later
        #region Linhist removed old codde 29/03/25

        //#region Linhist added 28/02/22
        ////Tach case verify cac thong tin chinh dung chung cho XML1, XML2, XML3
        //private void Verify_STT_MaLK(List<XML1Error> errors, XML1 file, int XML1ID, int? STT, string MA_LK, int fromXml)
        //{
        //    int iErrStart = fromXml == 1 ? 13 : fromXml == 2 ? 81 : 109;

        //    //13  1   XML1 MA_LK   Mã liên kết không được để trống
        //    //14  1   XML1 MA_LK   Mã liên kết không được lớn hơn 100 ký tự
        //    //81  1   XML2 MA_LK   Mã liên kết không được để trống
        //    //82  1   XML2 MA_LK   Mã liên kết không được lớn hơn 100 ký tự
        //    //109 1   XML3 MA_LK   Mã liên kết không được để trống
        //    //110 1   XML3 MA_LK   Mã liên kết không được lớn hơn 100 ký tự

        //    if (string.IsNullOrEmpty(MA_LK?.Trim()))
        //    {
        //        file.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =XML1ID,
        //            XEErrorCode = iErrStart,
        //            XEErrorRow = STT
        //        });
        //    }
        //    else if (MA_LK.Length > 100)
        //    {
        //        file.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =XML1ID,
        //            XEErrorCode = iErrStart + 1,
        //            XEErrorRow = STT
        //        });
        //    }

        //    //15  1   XML1 STT Số thứ tự phải có giá trị từ 0 đến 10 ký tự số
        //    if (fromXml == 1)
        //    {
        //        if (STT == null || STT < 0 || STT > 9999999999)
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = 15,
        //                XEErrorRow = STT
        //            });
        //        }
        //    }
        //    else
        //    {
        //        //83  1   XML2 STT Số thứ tự phải có giá trị từ 0 đến 999999 hoặc để trống
        //        //111 1   XML3 STT Số thứ tự phải có giá trị từ 0 - 999999 hoặc để trống

        //        if (STT != null && (STT < 0 || STT > 999999))
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = iErrStart + 2,
        //                XEErrorRow = STT
        //            });
        //        }
        //    }
        //}

        //private bool Verify_BacSi(List<XML1Error> errors, IEnumerable<DMBacSi> dmBacSi, XML1 file, int XML1ID, int? STT, string MA_BAC_SI, byte? MA_NHOM, DateTime? NGAY_YL, bool fromXml2)
        //{
        //    #region Linhist removed 23/06/22
        //    //if (string.IsNullOrEmpty(MA_BAC_SI))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =XML1ID,
        //    //        XEErrorCode = fromXml2 ? 358 : 359,
        //    //        XEErrorRow = STT
        //    //    });
        //    //    return false;
        //    //}
        //    ////return;

        //    ////105 1   XML2 MA_BAC_SI   Mã bác sĩ không được lớn hơn 255 ký tự
        //    ////358 1   XML2 MA_BAC_SI   Sai thông tin mã bác sĩ, không nằm trong mã CCHN được cấp

        //    ////133 1   XML3 MA_BAC_SI   Mã bác sĩ không được lớn hơn 255 ký tự hoặc để trống
        //    ////359 1   XML3 MA_BAC_SI   Sai thông tin mã bác sĩ, không nằm trong mã CCHN được cấp
        //    ////752 1   XML3 MA_BAC_SI   Chỉ bác sĩ chuyên khoa Nội mới được thực hiện KCB
        //    ////753 1   XML3 MA_BAC_SI   Chỉ bác sĩ chuyên khoa Răng Hàm Mặt và Đa Khoa được thực hiện KCB

        //    //if (string.IsNullOrEmpty(MA_BAC_SI) || MA_BAC_SI.Length > 255)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =XML1ID,
        //    //        XEErrorCode = fromXml2 ? 105 : 133,
        //    //        XEErrorRow = STT
        //    //    });
        //    //    return false;
        //    //}

        //    ////773 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
        //    ////774 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
        //    //if (MA_BAC_SI.Trim() == ";" || MA_BAC_SI.Replace(" ", "").Contains(";;"))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =XML1ID,
        //    //        XEErrorCode = fromXml2 ? 773 : 774,
        //    //        XEErrorRow = STT
        //    //    });
        //    //    return false;
        //    //}

        //    //var allBacSi = dmBacSi.Where(x => Uti.CodeContain(x.MACCHN, MA_BAC_SI) && !string.IsNullOrEmpty(x.LOAI));
        //    //if (!allBacSi.Any())
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =XML1ID,
        //    //        XEErrorCode = fromXml2 ? 358 : 359,
        //    //        XEErrorRow = STT
        //    //    });
        //    //    return false;
        //    //}

        //    //string[] bacsis = MA_BAC_SI.Split(';');
        //    //if (bacsis.Any(bs => string.IsNullOrWhiteSpace(bs)))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =XML1ID,
        //    //        XEErrorCode = fromXml2 ? 773 : 774,
        //    //        XEErrorRow = STT
        //    //    });
        //    //    return false;
        //    //}
        //    ////haimaster comment 22/6/2022 (Doan nay bi sai)
        //    //if (!allBacSi.Any(x => Uti.CodeMatch(MA_BAC_SI, x.MACCHN)))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =XML1ID,
        //    //        XEErrorCode = fromXml2 ? 358 : 359,
        //    //        XEErrorRow = STT
        //    //    });
        //    //    return false;
        //    //}

        //    //Linhist removed 16/05/22
        //    //if (!fromXml2 && Uti.NumExists(MA_NHOM, 10, 11, 14, 15)) return;
        //    #endregion

        //    #region Linhist added 23/06/22
        //    //105 1   XML2 MA_BAC_SI   Mã bác sĩ không được lớn hơn 255 ký tự hoặc để trống
        //    //133 1   XML3 MA_BAC_SI   Mã bác sĩ không được lớn hơn 255 ký tự hoặc để trống
        //    if (string.IsNullOrEmpty(MA_BAC_SI) || MA_BAC_SI.Length > 255)
        //    {
        //        file.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =XML1ID,
        //            XEErrorCode = fromXml2 ? 105 : 133,
        //            XEErrorRow = STT
        //        });
        //        return false;
        //    }

        //    //773 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
        //    //774 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
        //    if (MA_BAC_SI.Trim() == ";")
        //    {
        //        file.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =XML1ID,
        //            XEErrorCode = fromXml2 ? 773 : 774,
        //            XEErrorRow = STT
        //        });

        //        return false;
        //    }

        //    string[] bacsis = MA_BAC_SI.Split(';');
        //    var allBacSi = dmBacSi.Where(x => !string.IsNullOrEmpty(x.MACCHN)); // && !string.IsNullOrEmpty(x.LOAI));

        //    foreach (var bs in bacsis)
        //    {
        //        //773 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
        //        //774 1   XML3 MA_BAC_SI   Trống mã BS, hiển thị cách nhau bằng hai ký tự dấu chẩm phẩy
        //        if (bs.Trim() == "")
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = fromXml2 ? 773 : 774,
        //                XEErrorRow = STT
        //            });
        //        }
        //        //358 1   XML2 MA_BAC_SI   Sai thông tin mã bác sĩ, không nằm trong mã CCHN được cấp
        //        //359 1   XML3 MA_BAC_SI   Sai thông tin mã bác sĩ, không nằm trong mã CCHN được cấp
        //        else if (!allBacSi.Any(x => x.MACCHN.Contains(bs.Trim())))
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = fromXml2 ? 358 : 359,
        //                XEErrorRow = STT
        //            });
        //        }
        //    }

        //    //Linhist added 03/12/22
        //    //1295	1	XML3	MA_BAC_SI	Bác sĩ y lệnh ngoài khung thời gian làm việc gửi lên BH phê duyệt
        //    //1296    1   XML2 MA_BAC_SI   Bác sĩ y lệnh ngoài khung thời gian làm việc gửi lên BH phê duyệt
        //    //string maBS = $";{MA_BAC_SI};";
        //    string[] maBS = MA_BAC_SI.Split(';');
        //    var allCCHNs = dmBacSi.Where(x => !string.IsNullOrEmpty(x.MACCHN) && maBS.Any(b => x.MACCHN.Contains(b)));
        //    foreach (var bs in allCCHNs)
        //    {
        //        DateTime dTuNgay = DateTime.MinValue;
        //        DateTime dDenNgay = DateTime.MinValue;

        //        bool bHasTuNgay = !string.IsNullOrEmpty(bs.TUNGAY) && DateTime.TryParseExact(bs.TUNGAY, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dTuNgay);
        //        bool bHasDenNgay = !string.IsNullOrEmpty(bs.DENNGAY) && DateTime.TryParseExact(bs.DENNGAY, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dDenNgay);

        //        if (bHasTuNgay && bHasDenNgay)
        //        {
        //            if (NGAY_YL < dTuNgay || NGAY_YL > dDenNgay)
        //            {
        //                file.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =XML1ID,
        //                    XEErrorCode = fromXml2 ? 1296 : 1295,
        //                    XEErrorRow = STT
        //                });
        //            }
        //        }
        //        else if (bHasTuNgay)
        //        {
        //            if (NGAY_YL < dTuNgay)
        //            {
        //                file.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =XML1ID,
        //                    XEErrorCode = fromXml2 ? 1296 : 1295,
        //                    XEErrorRow = STT
        //                });
        //            }
        //        }
        //        else if (bHasDenNgay)
        //        {
        //            if (NGAY_YL > dDenNgay)
        //            {
        //                file.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =XML1ID,
        //                    XEErrorCode = fromXml2 ? 1296 : 1295,
        //                    XEErrorRow = STT
        //                });
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Linhist added 10/05/22
        //    //766 1   XML3 MA_BAC_SI   Phải có đủ CCHN của Bác sĩ chỉ định dịch vụ và người thực hiện dịch vụ => Chi bat cho HP
        //    //haimaster: 26/5/2022 chua thay phai la BS
        //    //if (Uti.NumExists(MA_NHOM, 1, 2, 3, 8, 18))
        //    if (!fromXml2 && Uti.NumExists(MA_NHOM, 1, 2, 3, 8, 18))
        //    {
        //        //Linhist changed 15/02/23
        //        //if (allBacSi.Count() < 2)
        //        if (allCCHNs.Count() < 2)
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = 766,
        //                XEErrorRow = STT
        //            });
        //            //Linhist addef 21/11/22
        //            //1255	1	XML3	MA_BAC_SI	ĐK Tỉnh Quảng Ninh: Phải có đủ CCHN của Bác sĩ  chỉ định dịch vụ và người thực hiện dịch vụ
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = 1255,
        //                XEErrorRow = STT
        //            });
        //        }
        //        //Linhist added 23/06/22 them kiem tra ko co bac si cung bao loi nay
        //        //Linhist changed 15/02/23
        //        //else if (!allBacSi.Any(x => Uti.StrExists(x.LOAI, true, "BS")))
        //        else if (!allCCHNs.Any(x => Uti.IsNumEqual(x.CHUCDANH_NN, 1)))
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = 766,
        //                XEErrorRow = STT
        //            });
        //        }
        //    }
        //    #endregion

        //    //468 1   XML2 MA_BAC_SI   Chỉ bác sĩ mới được thực hiện Khám chữa bệnh và ra chỉ định
        //    //469 1   XML3 MA_BAC_SI   Chỉ bác sĩ mới được thực hiện Khám chữa bệnh và ra chỉ định
        //    //haimaster: 26/5/2022 sua lai logic
        //    if (fromXml2)
        //    {
        //        //Linhist changed 15/02/23
        //        //if (!allBacSi.Any(x => Uti.StrExists(x.LOAI, true, "BS")))
        //        if (!allCCHNs.Any(x => Uti.IsNumEqual(x.CHUCDANH_NN, 1)))
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = 468,
        //                XEErrorRow = STT
        //            });
        //        }
        //    }
        //    //Linhist changed 11/09/23 - them ma nhom 18
        //    else if (Uti.NumExists(MA_NHOM, 8, 13, 18))
        //    {
        //        //Linhist changed 15/02/23
        //        //if (!allBacSi.Any(x => Uti.StrExists(x.LOAI, true, "BS")))
        //        if (!allBacSi.Any(x => Uti.IsNumEqual(x.CHUCDANH_NN, 1)))
        //        {
        //            file.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =XML1ID,
        //                XEErrorCode = 469,
        //                XEErrorRow = STT
        //            });
        //            return false;
        //        }
        //    }
        //    //if (fromXml2 || Uti.NumExists(MA_NHOM, 8, 13))
        //    //{
        //    //   if (!allBacSi.Any(x => Uti.StrExists(x.LOAI, true, "BS")))
        //    //    {
        //    //       file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =XML1ID,
        //    //            XEErrorCode = 468,
        //    //            XEErrorRow = STT
        //    //        });
        //    //        return false;
        //    //    }
        //    //}
        //    //else if (Uti.NumExists(MA_NHOM, 8, 13))
        //    //{
        //    //    if (!allBacSi.Any(x => Uti.StrExists(x.LOAI, true, "BS")))
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =XML1ID,
        //    //            XEErrorCode = 469,
        //    //            XEErrorRow = STT
        //    //        });
        //    //        return false;
        //    //    }
        //    //}

        //    return true;
        //}

        //#region Linhist added 25/05/22
        //private void Verify_HDSDThuoc(List<XML1Error> errors, IEnumerable<HDSDThuoc> ruleThuocs, XML2 xml2, double tuoiBN, double ngayTuoi, string maBenhICD10, bool coThai, bool choConBu)
        //{
        //    foreach (HDSDThuoc rule in ruleThuocs)
        //    {
        //        if (rule == null) continue;

        //        try
        //        {
        //            bool hasErr = false;

        //            //Kiem tra DUONG_DUNG neu ko phu hop thi bo qua rule nay
        //            if (!string.IsNullOrEmpty(rule.DUONG_DUNG) && !Uti.CodeContain(xml2.DUONG_DUNG, rule.DUONG_DUNG)) continue;

        //            // Case chi dinh:
        //            if (rule.CHI_DINH == true)
        //            {
        //                //Kiem tra MA_BENH: neu khong co MA_BENH theo ICD10 tuong ung --> bao loi
        //                //haimaster 6/8/2022 update chi co ma benh, khong co thuoc khac
        //                if (!string.IsNullOrEmpty(rule.MA_BENH) && !Uti.CodeMatch(maBenhICD10, rule.MA_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }

        //                //Kiem tra THUOC_KHAC: neu khong co it nhat 1 MA_THUOC trong cung ho so tuong ung voi THUOC_KHAC --> bao loi
        //                //haimaster 6/8/2022 update chi co thuoc khac, khong co ma benh
        //                if (!string.IsNullOrEmpty(rule.THUOC_KHAC) && xml2.XML1 != null && !xml2.XML1.XML2s.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)) && string.IsNullOrEmpty(rule.MA_BENH))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }
        //                // haimaster 26/05/2022: Kiem tra case cu ke thuoc la bao 25.11.2022 them DICHVU
        //                if (String.IsNullOrEmpty(rule.MA_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC)&&string.IsNullOrEmpty(rule.DICH_VU))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;

        //                }
        //                //haimaster 6/8/2022: them truoc hop co ca thuoc khac va ma benh
        //                if (!string.IsNullOrEmpty(rule.MA_BENH) && !string.IsNullOrEmpty(rule.THUOC_KHAC) && (!Uti.CodeMatch(maBenhICD10, rule.MA_BENH) && !xml2.XML1.XML2s.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC))))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }
        //                //haimaster 25.11.2022 them Dich Vu, Chi dich thuoc phai co dich vu
        //                if (!string.IsNullOrEmpty(rule.DICH_VU) && !xml2.XML1.XML3s.Any(y=>Uti.CodeMatch(y.MA_DICH_VU,rule.DICH_VU)))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }
        //            }
        //            // Case chong chi dinh:
        //            else
        //            {
        //                if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        //                {
        //                    //Kiem tra TRE_SO_SINH: neu so ngay tuoi cua BN < dieu kien ngay tuoi TRE_SO_SINH ma ke THUOC_KHAC thi bao loi   
        //                    if (rule.TRE_SO_SINH != null && rule.TRE_SO_SINH != 0d)
        //                    {
        //                        if (ngayTuoi < rule.TRE_SO_SINH)
        //                        {
        //                            hasErr = xml2.XML1.XML2s.Any(x => x.XML2ID != xml2.XML2ID && Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC));
        //                        }
        //                        goto RAISE_ERROR_HDSD_THUOC;
        //                    }

        //                    //Kiem tra THOI_GIAN_TOI_THIEU neu trong thoi gian toi thieu ke dong thoi voi THUOC_KHAC thi bao loi
        //                    else if (rule.THOI_GIAN_TOI_THIEU != null)
        //                    {
        //                        if (xml2.NGAY_YL != null)
        //                        {
        //                            if (rule.THOI_GIAN_TOI_THIEU == 0d)
        //                            {
        //                                hasErr = xml2.XML1.XML2s.Any(x => x.XML2ID != xml2.XML2ID && Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC) && x.NGAY_YL == xml2.NGAY_YL);
        //                            }
        //                            else
        //                            {
        //                                DateTime dMin = xml2.NGAY_YL.Value.AddHours(-rule.THOI_GIAN_TOI_THIEU.Value);
        //                                DateTime dMax = xml2.NGAY_YL.Value.AddHours(rule.THOI_GIAN_TOI_THIEU.Value);
        //                                if (xml2.XML1 != null)
        //                                {
        //                                    hasErr = xml2.XML1.XML2s.Any(x => x.XML2ID != xml2.XML2ID && Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC) && x.NGAY_YL != null && x.NGAY_YL.Value > dMin && x.NGAY_YL.Value < dMax);
        //                                }
        //                            }
        //                        }
        //                        goto RAISE_ERROR_HDSD_THUOC;
        //                    }

        //                    //Kiem tra THUOC_KHAC: neu co it nhat 1 MA_THUOC trong cung ho so tuong ung voi THUOC_KHAC --> bao loi
        //                    else if (xml2.XML1 != null && xml2.XML1.XML2s.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_THUOC;
        //                    }
        //                }


        //                //Kiem tra DO_TUOI: tuoi cua benh nhan phu hop voi dieu kien DO_TUOI --> bao loi 
        //                if (!string.IsNullOrEmpty(rule.TUOI) && Uti.NumMatch(tuoiBN, rule.TUOI))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }

        //                //Kiem tra CO_THAI: neu co dieu kien CO_THAI (= 1) ma benh nhan la phu nu co thai --> bao loi
        //                if (rule.CO_THAI == true && coThai == true)
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }

        //                //Kiem tra CHO_CON_BU: neu co dieu kien CHO_CON_BU (= 1) ma benh nhan la phu nu dang cho con bu --> bao loi
        //                if (rule.CHO_CON_BU == true && choConBu == true)
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }

        //                //Kiem tra MA_BENH: neu co MA_BENH theo ICD10 tuong ung --> bao loi
        //                if (!string.IsNullOrEmpty(rule.MA_BENH) && Uti.CodeMatch(maBenhICD10, rule.MA_BENH))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_THUOC;
        //                }

        //                //Linhist removed 30/05/22 chuyen len tren, kiem tra cung case khac
        //                //Kiem tra THUOC_KHAC: neu co it nhat 1 MA_THUOC trong cung ho so tuong ung voi THUOC_KHAC --> bao loi
        //                //if (!string.IsNullOrEmpty(rule.THUOC_KHAC) && xml2.XML1.XML2s.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        //                //{
        //                //    hasErr = true;
        //                //    goto RAISE_ERROR_HDSD_THUOC;
        //                //}

        //                //Kiem tra TYLE_TT: neu xml2 co ty le thanh toan khong phu hop dieu kien --> bao loi
        //                //haimaster: 26/5/2022 chinh sua lai dieu kien: Bo dau ! khi so sanh
        //                //if (string.IsNullOrEmpty(rule.TYLE_TT) && xml2.TYLE_TT != null)
        //                //Linhist changed 08/11/22
                        
        //                if (!string.IsNullOrEmpty(rule.TYLE_TT) && xml2.TYLE_TT != null)
        //                {
        //                    //27/5/2022 them dieu kien cho bệnh nhân viêm gan C dùng thuốc "Myhep 400mg" => Se them doan code kiem tra xem BN nay co phai la BN HIV bi viem gan C khong => Ko phai van bao loi sai ty le
        //                    if ((xml2.TYLE_TT == 0 && xml2.T_NGUONKHAC == xml2.THANH_TIEN) && string.Compare(xml2.MA_THUOC.ToString(), "40.30.272") == 0 || string.Compare(xml2.MA_THUOC.ToString(), "40.30.273") == 0)
        //                    {

                        
        //                    }
        //                    //29.11.2022 haimaster upate khong kiem tra cho Cong An va QUan Nhan
        //                    else if ((xml2.XML1.MA_THE.ToString().Substring(0,2)=="CA")|| (xml2.XML1.MA_THE.ToString().Substring(0, 2) == "QN"))
        //                    { 
                            
        //                    }
        //                    //Linhist changed 08/11/22: neu xml2 co ty le thanh toan phu hop dieu kien --> bao loi
        //                    //else if (!Uti.NumMatch(xml2.TYLE_TT.Value, rule.TYLE_TT))
        //                    else if (Uti.NumMatch(xml2.TYLE_TT.Value, rule.TYLE_TT))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_THUOC;
        //                    }
        //                }
        //            }

        //            RAISE_ERROR_HDSD_THUOC:
        //            if (hasErr)
        //            {
        //                xml2.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml2.XML1ID,
        //                    XEErrorCode = rule.MA_LOI,
        //                    XEErrorRow = xml2.STT
        //                });
        //            }
        //        }
        //        catch { }
        //    }
        //}
        //#endregion

        //#region Linhist removed 25/05/22
        ////Tach case verify HDSDThuoc rieng cho XML2
        ////Linhist 25/03/22 them list benh theo ICD10
        ////private void Verify_HDSDThuoc_OLD(List<XML1Error> errors, IEnumerable<HDSDThuoc> ruleThuocs, XML2 xml2, double tuoiBN, string maBenhICD10, bool coThai, bool choConBu)
        ////{
        ////    //Haimaster comment: Neu phan check HDSD Thuoc bi loi, no se bo khong check dong thuoc do tren XML2 nua, phai them Try Catch vao doan nay
        ////    //1. Thieu case chong chi dinh voi tien su benh
        ////    //2. Thieu case chong chi dinh voi thuoc khac
        ////    //3. Thieu case chong chi dinh voi Dich vu khac
        ////    foreach (HDSDThuoc rule in ruleThuocs)
        ////    {
        ////        if (rule == null) continue;

        ////        try
        ////        {
        ////            bool hasErr = false;
        ////            bool chiDinh = true.Equals(rule.CHI_DINH);

        ////            //Linhist removed 11/05/22
        ////            ////Kiem tra CO_THAI hoac CHO_CON_BU (??? cai nay can dieu kien kiem tra them)
        ////            //bool coThai = false; // Can dieu kien kiem tra
        ////            //bool choConBu = false; // Can dieu kien kiem tra
        ////            //if (coThai != true.Equals(rule.CO_THAI) || choConBu != true.Equals(rule.CHO_CON_BU)) continue;

        ////            //Kiem tra DUONG_DUNG neu ko match thi bo qua rule nay
        ////            if (!string.IsNullOrEmpty(rule.DUONG_DUNG) && !Uti.CodeContain(xml2.DUONG_DUNG, rule.DUONG_DUNG)) continue;

        ////            //Linhist changed 11/05/22
        ////            if ((chiDinh && true.Equals(rule.CO_THAI)) != coThai)
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_THUOC;
        ////            }
        ////            if ((chiDinh && true.Equals(rule.CHO_CON_BU)) != choConBu)
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_THUOC;
        ////            }

        ////            //Linhist added 12/05/22 kiem tra rule khong co them dieu kien khac ngoai chi dinh bao loi ngay
        ////            if (string.IsNullOrEmpty($"{rule.TUOI}{rule.MA_BENH}{rule.DICH_VU}{rule.TYLE_TT}{rule.TIEN_SU_BENH}".Trim()))
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_THUOC;
        ////            }

        ////            //Kiem tra DO_TUOI: co chiDinh (== true) thi dieu kien DO_TUOI phai dung, khong chiDinh (== false) thi dieu kien DO_TUOI cung phai khong dung
        ////            if (!string.IsNullOrEmpty(rule.TUOI) && chiDinh != Uti.NumMatch(tuoiBN, rule.TUOI))
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_THUOC;
        ////            }

        ////            //Kiem tra MA_BENH theo ICD10 (theo chuoi tuong ung da xu ly MA_BENH voi ma anh xa ICD10 tu XML1)
        ////            if (!string.IsNullOrEmpty(rule.MA_BENH) && chiDinh != Uti.CodeMatch(maBenhICD10, rule.MA_BENH))
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_THUOC;
        ////            }

        ////            //Kiem tra TYLE_TT
        ////            if (!string.IsNullOrEmpty(rule.TYLE_TT))
        ////            {
        ////                double tyleTT = xml2.TYLE_TT == null ? 0d : xml2.TYLE_TT.Value;
        ////                if (chiDinh != Uti.NumMatch(tyleTT, rule.TYLE_TT))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_THUOC;
        ////                }
        ////            }

        ////            //Kiem tra THUOC_KHAC trong xml cung thoi diem (??? cai nay co the phai tach ra kiem tra rieng)
        ////            if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        ////            {
        ////                var allThuocs = xml2.XML1.XML2s.Where(x => x.MA_LK == xml2.MA_LK);
        ////                foreach (var thuoc in allThuocs)
        ////                {
        ////                    if (chiDinh != Uti.CodeMatch(thuoc.MA_THUOC, rule.THUOC_KHAC))
        ////                    {
        ////                        hasErr = true;
        ////                        goto RAISE_ERROR_HDSD_THUOC;
        ////                    }
        ////                }
        ////            }

        ////        #region Linhist removed 21/04/22 - dieu kien kiem tra dich vu dang co van de
        ////        ////Kiem tra DICH_VU khac trong xml cung thoi diem (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //if (!string.IsNullOrEmpty(rule.DICH_VU))
        ////        //{
        ////        //    var allDichvus = xml2.XML1.XML3s.Where(x => x.MA_LK == xml2.MA_LK && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));
        ////        //    if (chiDinh != allDichvus.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU)))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_THUOC;
        ////        //    }
        ////        //}
        ////        #endregion

        ////        RAISE_ERROR_HDSD_THUOC:
        ////            if (hasErr)
        ////            {
        ////                file.XML1Errors.Add(new XML1Error()
        ////                {
        ////                    //XEXML1ID =xml2.XML1ID,
        ////                    XEErrorCode = Int32.Parse(rule.MA_LOI),
        ////                    XEErrorRow = xml2.STT
        ////                });
        ////            }
        ////        }
        ////        catch { }
        ////    }
        ////}
        //#endregion

        ////Linhist added 06/04/22 - kiem tra MANHOM_5937 moi tu ngay 01/04/2022
        //private DateTime ngay_NHOM_5937 = new DateTime(2022, 4, 1, 0, 0, 0);

        ////Tach case verify Thong tin Thuoc rieng cho XML2
        //private bool Verify_TTThuoc(List<XML1Error> errors, IEnumerable<DMThuoc> dmThuocs, XML2 xml2, bool isNhom5937)
        //{
        //    bool hasPassed = true;

        //    #region Linhist removed 10/05/22
        //    ////84  1   XML2 MA_NHOM Mã nhóm không được để trống
        //    ////85  1   XML2 MA_NHOM Mã nhóm phải là 4 5 6 hoặc 7
        //    ////160 1   XML2 MA_NHOM Mã nhóm không khớp với mã nhóm trong danh mục phê duyệt
        //    //if (xml2.MA_NHOM == null)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 84,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //    hasPassed = false;
        //    //}
        //    #endregion

        //    #region Linhist removed 06/04/22 - kiem tra theo nhom moi MANHOM_5937
        //    //else if (xml2.MA_NHOM < 4 || xml2.MA_NHOM > 7)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 85,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //    hasPassed = false;
        //    //}
        //    //else if (xml2.MA_NHOM == 4)
        //    //{
        //    //    filterThuocs = filterThuocs.Where(x => x.MANHOM_5937 == "4" || (string.IsNullOrEmpty(x.MANHOM_5937) && x.MANHOM_9324 == "4"));

        //    //    if (!filterThuocs.Any())
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml2.XML1ID,
        //    //            XEErrorCode = 160,
        //    //            XEErrorRow = xml2.STT
        //    //        });
        //    //    }
        //    //    hasPassed = false;
        //    //}
        //    #endregion


        //    IEnumerable<DMThuoc> filterThuocs = dmThuocs;//.Where(x => x.HIEULUC == "Có");
        //    //86  1   XML2 MA_THUOC    Mã thuốc không được để trống
        //    //87  1   XML2 MA_THUOC    Mã thuốc không được lớn hơn 255 ký tự
        //    //200 1   XML2 MA_THUOC    Mã thuốc không được tìm thấy trong danh mục phê duyệt
        //    //201 1   XML2 MA_THUOC    Mã thuốc không có hiệu lực trong danh mục phê duyệt
        //    if (string.IsNullOrEmpty(xml2.MA_THUOC))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 86,
        //            XEErrorRow = xml2.STT
        //        });
        //        hasPassed = false;
        //    }
        //    else if (xml2.MA_THUOC.Length > 255)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 87,
        //            XEErrorRow = xml2.STT
        //        });
        //        hasPassed = false;
        //    }
        //    else if (Uti.NumExists(xml2.MA_NHOM, 7, 17))
        //    {
        //        return false;
        //    }
        //    else if (!dmThuocs.Any())
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 200,
        //            XEErrorRow = xml2.STT
        //        });
        //        hasPassed = false;
        //        return false;
        //    }
        //    else if (!filterThuocs.Any())
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 201,
        //            XEErrorRow = xml2.STT
        //        });

        //        hasPassed = false;
        //        return false;
        //    }

        //    if (xml2.MA_NHOM != 4) return false;

        //    #region Linhist removed 10/05/22
        //    //bool hasNhom = true;
        //    //#region Linhist added 06/04/22 - kiem tra theo nhom moi MANHOM_5937
        //    ////Linhist changed 12/04/22 lay ngay vao vien de check MA_NHOM_5937
        //    ////if (xml2.NGAY_YL != null && xml2.NGAY_YL.Value >= ngay_NHOM_5937)
        //    //if (isNhom5937)
        //    //{
        //    //    //Linhist removed 10/05/22
        //    //    //filterThuocs = filterThuocs.Where(x => x.MANHOM_5937 == "4");
        //    //    if (!filterThuocs.Any(x => x.MANHOM_5937 == "4"))
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml2.XML1ID,
        //    //            XEErrorCode = 160,
        //    //            XEErrorRow = xml2.STT
        //    //        });
        //    //        //hasPassed = false;
        //    //        //hasNhom = false;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    //Linhist removed 10/05/22
        //    //    //filterThuocs = filterThuocs.Where(x => x.MANHOM_9324 == "4");
        //    //    if (!filterThuocs.Any(x => x.MANHOM_9324 == "4"))
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml2.XML1ID,
        //    //            XEErrorCode = 160,
        //    //            XEErrorRow = xml2.STT
        //    //        });
        //    //        //hasPassed = false;
        //    //        //hasNhom = false;
        //    //    }
        //    //}
        //    //#endregion


        //    ////88  1   XML2 TEN_THUOC   Tên thuốc không được để trống
        //    ////89  1   XML2 TEN_THUOC   Tên thuốc không được lớn hơn 255 ký tự
        //    ////161 1   XML2 TEN_THUOC   Tên thuốc không khớp với tên thuốc trong danh mục phê duyệt
        //    ////162 1   XML2 TEN_THUOC   Tên thuốc phải nằm trong thẻ<![CDATA[...]] >
        //    //if (string.IsNullOrEmpty(xml2.TEN_THUOC))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 88,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //}
        //    //else if (xml2.TEN_THUOC.Length > 255)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 89,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //}
        //    //else
        //    //{
        //    //    //Linhist removed 31/03/22
        //    //    //Khong filter doan nay, chi kiem tra
        //    //    //filterThuocs = filterThuocs.Where(x => Uti.IsEqual(xml2.TEN_THUOC, x.TEN_THUOC, ignoreQuote: true));

        //    //    if (!filterThuocs.Any(x => Uti.IsEqual(xml2.TEN_THUOC, x.TEN_THUOC, ignoreQuote: true)))
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml2.XML1ID,
        //    //            XEErrorCode = 161,
        //    //            XEErrorRow = xml2.STT
        //    //        });
        //    //    }
        //    //}
        //    #endregion

        //    //94  1   XML2 SO_DANG_KY  Số đăng ký không được khoảng trống(space) và không được lớn hơn 255 ký tự
        //    //172 1   XML2 SO_DANG_KY  Số đăng ký không khớp với Số đăng ký trong danh mục phê duyệt(phân biệt chữ hoa, chữ thường)
        //    //173 1   XML2 SO_DANG_KY  Số đăng ký không khớp với Số đăng ký trong danh mục phê duyệt(không phân biệt chữ hoa, chữ thường)
        //    //241 1   XML2 SO_DANG_KY  Số đăng ký không đúng với danh mục được phê duyệt
        //    if (string.IsNullOrEmpty(xml2.SO_DANG_KY))
        //    {
        //        //Linhist bo qua truong hop Thuoc Dong Y khong co SO_DANG_KY
        //        if (!xml2.MA_THUOC.Contains("05V."))
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 94,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }
        //    else if (xml2.SO_DANG_KY.Length > 255)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 94,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => x.SO_DANG_KY == xml2.SO_DANG_KY))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 172,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else
        //    {
        //        filterThuocs = filterThuocs.Where(x => Uti.IsEqual(x.SO_DANG_KY, xml2.SO_DANG_KY, ignoreCase: true));
        //        if (!filterThuocs.Any())
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 173,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }

        //    #region Linhist removed 29/03/2023
        //    //239 1   XML2 TT_THAU Thông tin thầu phải có độ dài từ 1 đến 50 ký tự
        //    //327 1   XML2 TT_THAU Thông tin thầu không khớp với danh mục được phê duyệt(Số quyết định; Gói thầu; Nhóm thầu)
        //    //771 1   XML2 TT_THAU Số quyết định sai định dạng mã hóa Unicode
        //    //string SO_QUYET_DINH = "";
        //    //string GOI_THAU = "";
        //    //string NHOM_THAU = "";
        //    //if (string.IsNullOrEmpty(xml2.TT_THAU) || xml2.TT_THAU.Length > 50)
        //    //{
        //    //    xml2.XML1.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 239,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //}
        //    //else
        //    //{
        //    //    string[] maThaus = xml2.TT_THAU.Split(';');
        //    //    SO_QUYET_DINH = maThaus[0];
        //    //    if (maThaus.Length > 2)
        //    //    {
        //    //        GOI_THAU = maThaus[1];
        //    //        NHOM_THAU = maThaus[2];
        //    //    }
        //    //    else if (maThaus.Length == 2)
        //    //    {
        //    //        NHOM_THAU = maThaus[1];
        //    //    }
        //    //}

        //    //if (!filterThuocs.Any(x => Uti.IsEqual(x.NHOM_THAU, NHOM_THAU, ignoreEmpty: true) && Uti.IsEqual(x.GOI_THAU, GOI_THAU, ignoreEmpty: true) && x.QUYET_DINH.Contains(SO_QUYET_DINH, StringComparison.CurrentCulture)))
        //    //{
        //    //    xml2.XML1.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 771,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //}

        //    ////Linhist removed 15/05/22 bo filter SO_QD
        //    ////filterThuocs = filterThuocs.Where(x => Uti.IsEqual(x.NHOM_THAU, NHOM_THAU, ignoreEmpty: true) && Uti.IsEqual(x.GOI_THAU, GOI_THAU, ignoreEmpty: true) && Encoding.ASCII.GetBytes(x.QUYET_DINH).SequenceEqual(Encoding.ASCII.GetBytes(SO_QUYET_DINH)));
        //    //if (!filterThuocs.Any(x => Uti.IsEqual(x.NHOM_THAU, NHOM_THAU, ignoreEmpty: true) && Uti.IsEqual(x.GOI_THAU, GOI_THAU, ignoreEmpty: true) && Encoding.ASCII.GetBytes(x.QUYET_DINH).SequenceEqual(Encoding.ASCII.GetBytes(SO_QUYET_DINH))))
        //    //{
        //    //    xml2.XML1.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 327,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //}
        //    #endregion

        //    //Linhist added 29/03/2023
        //    //239 1   XML2 TT_THAU Thông tin thầu phải có độ dài từ 1 đến 50 ký tự
        //    //327 1   XML2 TT_THAU Thông tin thầu không khớp với danh mục được phê duyệt(Số quyết định; Gói thầu; Nhóm thầu)
        //    //771 1   XML2 TT_THAU Số quyết định sai định dạng mã hóa Unicode
        //    if (string.IsNullOrEmpty(xml2.TT_THAU) || xml2.TT_THAU.Length > 50)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 239,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    //Linhist changed 18/05/23
        //    //else if(!filterThuocs.Any(x => Uti.IsEqual(x.TT_THAU, xml2.TT_THAU, ignoreEmpty: true)))
        //    //{
        //    //    xml2.XML1.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml2.XML1ID,
        //    //        XEErrorCode = 327,
        //    //        XEErrorRow = xml2.STT
        //    //    });
        //    //}
        //    //Linhist changed 24/07/23 - kiem tra thong tin thuoc theo 3 key: MA_THUOC, SO_DANG_KY, TT_THAU
        //    //else if (!filterThuocs.Any(th => Check_TTTHAU(xml2.TT_THAU, th.TT_THAU)))
        //    else
        //    {
        //        filterThuocs = filterThuocs.Where(th => Check_TTTHAU(xml2.TT_THAU, th.TT_THAU));
        //        if (!filterThuocs.Any())
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 327,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }

        //    #region Linhist added 10/05/22
        //    //84  1   XML2 MA_NHOM Mã nhóm không được để trống
        //    //85  1   XML2 MA_NHOM Mã nhóm phải là 4 5 6 hoặc 7
        //    //160 1   XML2 MA_NHOM Mã nhóm không khớp với mã nhóm trong danh mục phê duyệt
        //    if (xml2.MA_NHOM == null)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 84,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (isNhom5937)
        //    {
        //        if (!filterThuocs.Any(x => x.MANHOM_5937 == 4))
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 160,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }
        //    else
        //    {
        //        if (!filterThuocs.Any(x => x.MANHOM_9324 == 4))
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 160,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }


        //    //88  1   XML2 TEN_THUOC   Tên thuốc không được để trống
        //    //89  1   XML2 TEN_THUOC   Tên thuốc không được lớn hơn 255 ký tự
        //    //161 1   XML2 TEN_THUOC   Tên thuốc không khớp với tên thuốc trong danh mục phê duyệt
        //    //162 1   XML2 TEN_THUOC   Tên thuốc phải nằm trong thẻ<![CDATA[...]] >
        //    if (string.IsNullOrEmpty(xml2.TEN_THUOC))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 88,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (xml2.TEN_THUOC.Length > 255)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 89,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else
        //    {
        //        if (!filterThuocs.Any(x => Uti.IsEqual(xml2.TEN_THUOC, x.TEN_THUOC, ignoreBracket: true)))
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 161,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }
        //    #endregion

        //    //97  1   XML2 DON_GIA Đơn giá không được để trống
        //    //98  1   XML2 DON_GIA Đơn giá phải là số thập phân lớn hơn 0 và tối đa 15 chữ số nguyên và tối đa 2 chữ số thập phân cách nhau bởi dâu '.'
        //    //174 1   XML2 DON_GIA Đơn giá không khớp với đơn giá trong danh mục phê duyệt
        //    //175 1   XML2 DON_GIA Đơn giá nhỏ hơn đơn giá trong danh mục phê duyệt
        //    //202 1   XML2 DON_GIA Đơn giá lớn hơn đơn giá trong danh mục phê duyệt
        //    if (xml2.DON_GIA == null)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 97,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => x.DON_GIA_BH == xml2.DON_GIA))
        //    {
        //        //Linhist added 27/05/22 them case don gia lon hon DM phe duyet
        //        double? maxGia = filterThuocs.Where(x => x.DON_GIA_BH != null && Uti.IsEqual(x.TEN_THUOC, xml2.TEN_THUOC, ignoreCase: true) &&
        //                                                    Uti.IsEqual(x.MA_DUONG_DUNG, xml2.DUONG_DUNG, ignoreCase: true) &&
        //                                                    Uti.IsEqual(x.DON_VI_TINH, xml2.DON_VI_TINH, ignoreCase: true)).Max(x => x.DON_GIA_BH);

        //        if (maxGia != null && xml2.DON_GIA > maxGia)
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 202,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //        else
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 174,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }

        //    //163 1   XML2 DON_VI_TINH Đơn vị tính không được để trống
        //    //164 1   XML2 DON_VI_TINH Đơn vị tính không khớp với đơn vị tính trong danh mục phê duyệt(Không phân biệt chữ hoa, chữ thường)
        //    //165 1   XML2 DON_VI_TINH Đơn vị tính không khớ với đơn vị tính trong danh mục phê duyệt(phân biệt chữ hoa, chữ thường)
        //    //90  1   XML2 DON_VI_TINH Đơn vị tính không được lớn hơn 50 ký tự
        //    if (string.IsNullOrEmpty(xml2.DON_VI_TINH))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 163,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (xml2.DON_VI_TINH.Length > 50)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 90,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => x.DON_VI_TINH == xml2.DON_VI_TINH))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 165,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => Uti.IsEqual(x.DON_VI_TINH, xml2.DON_VI_TINH, ignoreCase: true)))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 164,
        //            XEErrorRow = xml2.STT
        //        });
        //    }

        //    //92  1   XML2 DUONG_DUNG  Mã đường dùng không được lớn hơn 50 ký tự
        //    //169 1   XML2 DUONG_DUNG  Mã đường dùng không được để trống
        //    //170 1   XML2 DUONG_DUNG  Mã đường dùng không khớp với danh mục phê duyệt
        //    if (string.IsNullOrEmpty(xml2.DUONG_DUNG))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 169,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (xml2.DUONG_DUNG?.Length > 50)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 92,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => Uti.IsEqual(x.MA_DUONG_DUNG, xml2.DUONG_DUNG, ignoreCase: true)))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 170,
        //            XEErrorRow = xml2.STT
        //        });
        //    }

        //    //166 1   XML2 HAM_LUONG   Hàm lượng không khớp với hàm lượng trong danh mục phê duyệt(So sánh tuyệt đối)
        //    //167 1   XML2 HAM_LUONG   Hàm lượng không khớp với hàm lượng trong danh mục phê duyệt(So sánh tương đối)
        //    //168 1   XML2 HAM_LUONG   Hàm lượng phải nằm trong thẻ<![CDATA[...]] >
        //    //91  1   XML2 HAM_LUONG   Hàm lượng không được lớn hơn 1024 ký tự
        //    if (xml2.HAM_LUONG?.Length > 1024)
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 91,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => x.HAM_LUONG == xml2.HAM_LUONG))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 166,
        //            XEErrorRow = xml2.STT
        //        });
        //    }
        //    else if (!filterThuocs.Any(x => Uti.IsEqual(x.HAM_LUONG, xml2.HAM_LUONG, ignoreCase: true)))
        //    {
        //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml2.XML1ID,
        //            XEErrorCode = 167,
        //            XEErrorRow = xml2.STT
        //        });
        //    }

        //    //93  1   XML2 LIEU_DUNG   Liều dùng không được lớn hơn 255 ký tự
        //    //171 1   XML2 LIEU_DUNG   Liều dùng phải nằm trong thẻ<![CDATA[...]] >
        //    //357 1   XML2 LIEU_DUNG   Liều dùng không đúng định dạng (xem công văn 4210)
        //    //884  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        //    //885  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán

        //    //Linhist changed 21/04/22 chi check lieu dung cho BN noi tru
        //    //if (xml2.XML1?.MA_LOAI_KCB == 3)
        //    //{

        //        //Linhist added 06/06/22 bo kiem tra voi ma thuoc YHCT
        //        //Hiện tại các thuốc có chứa ký tự (A,B,C...) là thuốc đông y, không cần phải check phần liều dùng
        //        bool isDongY = false;
        //        foreach (char c in xml2.MA_THUOC)
        //        {
        //            if (char.IsLetter(c))
        //            {
        //                isDongY = true;
        //                break;
        //            }
        //        }

        //        //Linhist added 07/07/22 them kiem tra ma thuoc trong lieu dung
        //        //Linhist changed 26/04/23 bo nhung ma thuoc lieu dung dac biet
        //        //if (!isDongY && !Uti.CodeMatch(xml2.MA_THUOC, "40.980;40.982;40.974;40.803;40.805;40.806;40.44"))40.394
        //        if (!isDongY && !Uti.CodeMatch(xml2.MA_THUOC, "40.347;40.394;40.44;40.803;40.804.1;40.805;40.806;40.815;40.973;40.974;40.980;40.982"))
        //        {
        //            //Linhist changed 11/09/23
        //            //int errCheckLD = Verify_LieuDung(xml2.LIEU_DUNG, xml2.DON_VI_TINH ?? "", xml2.SO_LUONG, xml2.MA_THUOC, xml2.XML1.MA_LOAI_KCB, xml2.MA_KHOA);
        //            int errCheckLD = Verify_LieuDung(xml2);

        //            //Linhist added 16/08/23 - MA_LOI 1636 chi tinh Noi tru
        //            //Linhist removed 31/08/23 (da check ben trong ham Verify_LieuDung)
        //            //if (errCheckLD == 1636)
        //            //{
        //            //    //Linhist added 30/08/23 - MA_LOI 1636 chi tinh trg hop Noi tru (Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán)
        //            //    //if (Uti.NumExists(xml2.XML1.MA_LOAI_KCB, 2, 3))
        //            //    if (Uti.NumExists(xml2.XML1.MA_LOAI_KCB, 3))
        //            //    {
        //            //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //            //        {
        //            //            //XEXML1ID =xml2.XML1ID,
        //            //            XEErrorCode = errCheckLD,
        //            //            XEErrorRow = xml2.STT
        //            //        });
        //            //    }
        //            //}
        //            //else
        //            if (errCheckLD > 0)
        //            {
        //                xml2.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml2.XML1ID,
        //                    XEErrorCode = errCheckLD,
        //                    XEErrorRow = xml2.STT
        //                });
        //            }
        //        }
        //    //}

        //    //Linhist changed 07/03/24 - Thay CONG_BO bang NGAY_DE_NGHI 
        //    //Linhist added 29/04/22
        //    //328 1   XML2 NGAY_YL Ngày y lệnh không được phép nhỏ hơn ngày công bố
        //    //if (filterThuocs.Any() && xml2.NGAY_YL != null && xml2.NGAY_YL.HasValue)
        //    //{
        //    //    bool hasErr = true;
        //    //    int ngTH = 0;
        //    //    int ngYL = xml2.NGAY_YL.Value.Year * 10000 + xml2.NGAY_YL.Value.Month * 100 + xml2.NGAY_YL.Value.Day;
        //    //    foreach (var th in filterThuocs)
        //    //    {
        //    //        if (!string.IsNullOrEmpty(th.CONG_BO))
        //    //        {
        //    //            int.TryParse(th.CONG_BO, out ngTH);
        //    //            if (ngTH < ngYL)
        //    //            {
        //    //                hasErr = false;
        //    //                break;
        //    //            }
        //    //        }
        //    //    }
        //    //    if (hasErr)
        //    //    {
        //    //        xml2.XML1.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml2.XML1ID,
        //    //            XEErrorCode = 328,
        //    //            XEErrorRow = xml2.STT
        //    //        });
        //    //    }
        //    //}
        //    DateTime ngYL = xml2.NGAY_YL?.Date ?? DateTime.MinValue;
        //    foreach (var th in filterThuocs)
        //    {
        //        if (string.IsNullOrEmpty(th.NGAY_DE_NGHI)) continue;
        //        if (!DateTime.TryParseExact(th.NGAY_DE_NGHI, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngTH)) continue;

        //        if (ngTH.Date < ngYL)
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 328,
        //                XEErrorRow = xml2.STT
        //            });
        //            break;
        //        }
        //    }

        //    return hasPassed;
        //}

        ////Linhist added 10/04/22 tach case kiem tra lieu dung theo dinh dang
        ////private int Verify_LieuDung(string lieuDung, string donvi, double soluong, string maThuoc)
        ////Linhist changed 11/09/23
        ////private int Verify_LieuDung(string lieuDung, string donvi, double? soluong, string maThuoc, byte? loaiKCB, string maKhoa)
        //private int Verify_LieuDung(XML2 xml2)
        //{
        //    string lieuDung = xml2.LIEU_DUNG;
        //    string donvi = xml2.DON_VI_TINH ?? "";
        //    double? soluong = xml2.SO_LUONG;

        //    //Linhist added 21/04/22 bo kiem tra voi DON_VI_TINH la "Tub, Tuýp, Chai, Chai nhựa PPKB, Lọ, Chai/lọ, Bình xịt, Bình hít, Bơm tiêm"
        //    // if (Uti.StrExists(donvi, true, "Tub", "Tuýp", "Chai", "Chai nhựa PPKB", "Lọ", "Bình xịt", "Bình hít", "Bơm tiêm", "UI", "Đơn vị")) return 0;
        //    //haimaster update 17.11.2022
        //    //haimaster update 21.11.2022
        //    if (Uti.StrExists(donvi, true, "Chai nhựa PPKB", "Bình xịt", "Bình hít", "Bơm tiêm", "UI", "Đơn vị", "Bút tiêm")) return 0;
        //    //93  1   XML2 LIEU_DUNG   Liều dùng không được lớn hơn 255 ký tự
        //    //171 1   XML2 LIEU_DUNG   Liều dùng phải nằm trong thẻ<![CDATA[...]] >
        //    //357 1   XML2 LIEU_DUNG   Liều dùng không đúng định dạng (xem công văn 4210)
        //    if (string.IsNullOrEmpty(lieuDung?.Trim())) return 0;
        //    if (lieuDung.Length > 255) return 93;

        //    //Linhist added 07/07/22 bo kiem tra lieu dung voi thuoc 40.805
        //    //if (Uti.CodeContain(maThuoc, "40.805")) return 0;

        //    lieuDung = lieuDung.ToLower().Replace(donvi.Trim().ToLower(), "").Replace(" ", "");

        //    //format_LIEU_DUNG = { "donvi/lần*lần/ngày", "donvi/lầnxlần/ngày", "donvi*thang*ngày", "donvixthangxngày" };
        //    //Linhist changed 31/08/23
        //    //bool check_SOLUONG = true;
        //    bool check_SOLUONG = (soluong != null && soluong.HasValue);

        //    string[] lieus = null;
        //    if (lieuDung.EndsWith("ngày"))
        //    {
        //        if (lieuDung.Contains("lần"))
        //        {
        //            lieuDung = lieuDung.Replace("lần/ngày", "").Replace("/lần", "");
        //            lieus = lieuDung.Split("x*".ToCharArray());
        //            if (lieus.Length != 2) return 357;

        //            int idx = lieus[0].Length - 1;
        //            for (; idx > -1; idx--)
        //            {
        //                if (char.IsDigit(lieus[0][idx])) break;
        //            }

        //            if (idx < lieus[0].Length - 1)
        //            {
        //                check_SOLUONG = false;
        //                lieus[0] = lieus[0][0..(idx + 1)];
        //            }
        //        }
        //        else if (lieuDung.Contains("thang"))
        //        {
        //            lieuDung = lieuDung.Replace("ngày", "").Replace("thang", "");
        //            lieus = lieuDung.Split("x*".ToCharArray());
        //            if (lieus.Length != 3) return 357;
        //        }
        //    }

        //    if (lieus == null) return 357;

        //    //886  1   XML2 LIEU_DUNG   Liều dùng không được để bằng 0 (ví dụ: 0 viên, 0 lần)
        //    //887  1   XML2 LIEU_DUNG   Tổng lượng ghi trong liều dùng cao hơn với số lượng thanh toán
        //    try
        //    {
        //        double soLuongCalc = double.Parse(lieus[0], CultureInfo.InvariantCulture) * double.Parse(lieus[1], CultureInfo.InvariantCulture);
        //        if (soLuongCalc == 0d) return 886;
        //        if (check_SOLUONG)
        //        {
        //            if (Math.Round(soLuongCalc, 2) > Math.Round(soluong ?? 0d, 2)) return 887;

        //            //Linhist added 14/08/23
        //            //1636	1	XML2	LIEU_DUNG	Tổng lượng ghi trong liều dùng thấp hơn với số lượng thanh toán
        //            //Linhist added 31/08/23 - chi check cho loai KCB = 3 (Noi tru)
        //            //Linhist added 11/09/23 - ko check cho khoa K18
        //            if (Uti.NumExists(xml2.XML1.MA_LOAI_KCB, 3) && xml2.MA_KHOA != "K18")
        //            {
        //                if (Math.Round(soLuongCalc, 2) < Math.Round(soluong ?? 0d, 2)) return 1636;
        //            }

        //            //Linhist added 31/08/23
        //            //1638	1	XML2	SO_LUONG	Kê thuốc quá 30 ngày
        //            //Chi check loai KCB = 1, 2 (Kham benh, Ngoai tru)
        //            if (Uti.NumExists(xml2.XML1.MA_LOAI_KCB, 1, 2))
        //            {
        //                if (soluong / soLuongCalc > 30d) return 1638;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return 357;
        //    }

        //    return 0;
        //}

        //#region Linhist removed 30/05/22 thua code
        //////Tach case verify HDSDDichvu rieng cho XML3
        //////Linhist 25/03/22 them list benh theo ICD10 & sua lai cac dieu kien kiem tra
        ////private void Verify_HDSDDichvu_OLD(List<XML1Error> errors, IEnumerable<HDSDDichVu> ruleDichvus, List<XML3> xml3ToiThieus, List<HDSDDichVu> hdsdToiThieus, XML3 xml3, double tuoiBN, string maBenhICD10)
        ////{
        ////    //Linhist added 15/04/22 loc rule neu co MA_BENH tuong ung thi chi kiem tra cac rule co MA_BENH
        ////    //neu khong thi chi can kiem tra cac rule khong co MA_BENH bo qua cac rule co MA_BENH
        ////    var ruleTheoBENH = ruleDichvus.Where(r => !string.IsNullOrEmpty(r.MA_BENH) && Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", r.MA_BENH));
        ////    if (ruleTheoBENH.Any())
        ////    {
        ////        ruleDichvus = ruleTheoBENH;
        ////    }
        ////    else
        ////    {
        ////        ruleDichvus = ruleDichvus.Where(r => string.IsNullOrEmpty(r.MA_BENH));
        ////    }

        ////    foreach (HDSDDichVu rule in ruleDichvus)
        ////    {
        ////        if (rule == null) continue;

        ////        try
        ////        {
        ////            bool hasErr = false;
        ////            bool chiDinh = true.Equals(rule.CHI_DINH);

        ////            //Kiem tra CO_THAI hoac CHO_CON_BU (??? cai nay can dieu kien kiem tra them)
        ////            bool coThai = false; // Can dieu kien kiem tra
        ////            bool choConBu = false; // Can dieu kien kiem tra
        ////            if (coThai != true.Equals(rule.CO_THAI) || choConBu != true.Equals(rule.CHO_CON_BU)) continue;

        ////            //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua
        ////            if (rule.MA_LOAI_KCB != null && rule.MA_LOAI_KCB != 0 && xml3.XML1.MA_LOAI_KCB != null)
        ////            {
        ////                if (rule.MA_LOAI_KCB.Value != (int)xml3.XML1.MA_LOAI_KCB.Value) continue;
        ////            }

        ////            //Kiem tra DO_TUOI: co chiDinh (== true) thi dieu kien DO_TUOI phai dung, khong chiDinh (== false) thi dieu kien DO_TUOI cung phai khong dung
        ////            if (!string.IsNullOrEmpty(rule.TUOI) && chiDinh != Uti.NumMatch(tuoiBN, rule.TUOI))
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_DICHVU;
        ////            }

        ////            //Kiem tra TYLE_TT
        ////            if (!string.IsNullOrEmpty(rule.TYLE_TT))
        ////            {
        ////                double tyleTT = xml3.TYLE_TT == null ? 0d : xml3.TYLE_TT.Value;
        ////                if (chiDinh != Uti.NumMatch(tyleTT, rule.TYLE_TT))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }

        ////            //Kiem tra THOI_GIAN_THUC_HIEN
        ////            if (!string.IsNullOrEmpty(rule.THOI_GIAN_THUC_HIEN) && xml3.NGAY_KQ != null && xml3.NGAY_YL != null)
        ////            {
        ////                double thucHien = 0d;
        ////                string ruleThucHien = rule.THOI_GIAN_THUC_HIEN;
        ////                TimeSpan tmeDim = (xml3.NGAY_KQ.Value - xml3.NGAY_YL.Value);

        ////                if (ruleThucHien.EndsWith("p"))
        ////                {
        ////                    ruleThucHien = ruleThucHien[0..^1];
        ////                    thucHien = tmeDim.TotalMinutes;
        ////                }
        ////                else if (ruleThucHien.EndsWith("h"))
        ////                {
        ////                    ruleThucHien = ruleThucHien[0..^1];
        ////                    thucHien = tmeDim.TotalHours;
        ////                }
        ////                else
        ////                {
        ////                    thucHien = tmeDim.TotalDays;
        ////                }

        ////                if (chiDinh != Uti.NumMatch(thucHien, ruleThucHien))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }

        ////            #region Linhist added 15/04/22 tach case kiem tra theo THOI_GIAN_TOI_THIEU rieng
        ////            bool hasTOITHIEU = (rule.THOI_GIAN_TOI_THIEU != null && rule.THOI_GIAN_TOI_THIEU != 0d);

        ////            DateTime dMin = DateTime.Now;
        ////            DateTime dMax = DateTime.Now;
        ////            if (hasTOITHIEU && xml3.NGAY_YL != null)
        ////            {
        ////                dMax = xml3.NGAY_YL.Value.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);
        ////                dMin = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);
        ////            }

        ////            //Kiem tra THUOC_KHAC theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////            if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        ////            {
        ////                IEnumerable<XML2> allThuocs = xml3.XML1.XML2s;
        ////                if (rule.THOI_GIAN_TOI_THIEU == 0d)
        ////                {
        ////                    allThuocs = allThuocs.Where(x => x.NGAY_YL != null && x.NGAY_YL == xml3.NGAY_YL);
        ////                }
        ////                else if (rule.THOI_GIAN_TOI_THIEU != null)
        ////                {
        ////                    allThuocs = allThuocs.Where(x => x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);
        ////                }

        ////                if (chiDinh != allThuocs.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }

        ////            var allDichvus = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));
        ////            if (rule.THOI_GIAN_TOI_THIEU == 0d)
        ////            {
        ////                allDichvus = allDichvus.Where(x => x.NGAY_YL != null && x.NGAY_YL == xml3.NGAY_YL);
        ////            }
        ////            else if (rule.THOI_GIAN_TOI_THIEU != null)
        ////            {
        ////                allDichvus = allDichvus.Where(x => x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);
        ////            }

        ////            //Kiem tra DICH_VU_KHAC theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////            if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC) && chiDinh != allDichvus.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_DICHVU;
        ////            }
        ////            else if (hasTOITHIEU)
        ////            {
        ////                hdsdToiThieus.Add(rule);
        ////                xml3ToiThieus.Add(xml3);
        ////            }
        ////        #endregion

        ////        #region Linhist removed 07/04/22 bo qua, viet lai kiem tra ben tren
        ////        //// haimaster comment THOI_GIAN_TOI_THIEU luon bang 0 (Update lai)
        ////        //if (rule.THOI_GIAN_TOI_THIEU == null || rule.THOI_GIAN_TOI_THIEU == 0d)
        ////        //{
        ////        //    //Kiem tra MA_BENH theo ICD10 (theo chuoi tuong ung da xu ly MA_BENH voi ma anh xa ICD10 tu XML1)
        ////        //    if (!string.IsNullOrEmpty(rule.MA_BENH) && chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_DICHVU;
        ////        //    }

        ////        //    //Kiem tra THUOC_KHAC (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //    if (!string.IsNullOrEmpty(rule.THUOC_KHAC) && chiDinh != xml3.XML1.XML2s.Any(x => x.STT != xml3.STT && Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_DICHVU;
        ////        //    }

        ////        //    //Kiem tra DiCH_VU_KHAC (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //    if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC) && chiDinh != xml3.XML1.XML3s.Any(x => x.STT != xml3.STT && Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_DICHVU;
        ////        //    }
        ////        //}
        ////        //else if (xml3.NGAY_YL != null)
        ////        //{
        ////        //    DateTime dMax = xml3.NGAY_YL.Value.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);
        ////        //    DateTime dMin = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);

        ////        //    //Kiem tra THUOC_KHAC theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //    if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        ////        //    {
        ////        //        var allThuocs = xml3.XML1.XML2s.Where(x => x.STT != xml3.STT && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);
        ////        //        if (chiDinh != allThuocs.Any(x => x.STT < xml3.STT && Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        ////        //        {
        ////        //            hasErr = true;
        ////        //            goto RAISE_ERROR_HDSD_DICHVU;
        ////        //        }
        ////        //    }

        ////        //    var allDichvus = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && x.NGAY_YL != null && !string.IsNullOrWhiteSpace(x.MA_DICH_VU) && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);

        ////        //    //Kiem tra MA_BENH theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //    if (!string.IsNullOrEmpty(rule.MA_BENH) && chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////        //    {
        ////        //        if (allDichvus.Any(x => x.STT < xml3.STT && x.MA_DICH_VU == xml3.MA_DICH_VU))
        ////        //        {
        ////        //            hasErr = true;
        ////        //            goto RAISE_ERROR_HDSD_DICHVU;
        ////        //        }
        ////        //    }

        ////        //    //Kiem tra DICH_VU_KHAC theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //    if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC) && chiDinh != allDichvus.Any(x => x.STT < xml3.STT && Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_DICHVU;
        ////        //    }
        ////        //}

        ////        ////Kiem tra MA_BENH truong hop rieng ko co THOI_GIAN_TOI_THIEU
        ////        //if (rule.THOI_GIAN_TOI_THIEU == null && !string.IsNullOrEmpty(rule.MA_BENH) && chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////        //{
        ////        //    hasErr = true;
        ////        //    goto RAISE_ERROR_HDSD_DICHVU;
        ////        //}

        ////        //DateTime dMin = DateTime.Now;
        ////        //DateTime dMax = DateTime.Now;
        ////        //if (rule.THOI_GIAN_TOI_THIEU != null && rule.THOI_GIAN_TOI_THIEU != 0d)
        ////        //{
        ////        //    dMax = xml3.NGAY_YL.Value.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);
        ////        //    dMin = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);
        ////        //}


        ////        ////Kiem tra THUOC_KHAC theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        ////        //{
        ////        //    var allThuocs = xml3.XML1.XML2s.Where(x => x.STT != xml3.STT);
        ////        //    if (rule.THOI_GIAN_TOI_THIEU == 0d)
        ////        //    {
        ////        //        allThuocs = allThuocs.Where(x => x.NGAY_YL != null && x.NGAY_YL == xml3.NGAY_YL);
        ////        //    }
        ////        //    else if (rule.THOI_GIAN_TOI_THIEU != null)
        ////        //    {
        ////        //        allThuocs = allThuocs.Where(x => x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);
        ////        //    }

        ////        //    if (chiDinh != allThuocs.Any(x => x.STT < xml3.STT && Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_DICHVU;
        ////        //    }
        ////        //}

        ////        //var allDichvus = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));
        ////        //if (rule.THOI_GIAN_TOI_THIEU == 0d)
        ////        //{
        ////        //    allDichvus = allDichvus.Where(x => x.NGAY_YL != null && x.NGAY_YL == xml3.NGAY_YL);
        ////        //}
        ////        //else if (rule.THOI_GIAN_TOI_THIEU != null)
        ////        //{
        ////        //    allDichvus = allDichvus.Where(x => x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);
        ////        //}

        ////        ////Kiem tra MA_BENH theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //if (!string.IsNullOrEmpty(rule.MA_BENH) && chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////        //{
        ////        //    if (rule.THOI_GIAN_TOI_THIEU == null || allDichvus.Any(x => x.MA_DICH_VU == xml3.MA_DICH_VU))
        ////        //    {
        ////        //        hasErr = true;
        ////        //        goto RAISE_ERROR_HDSD_DICHVU;
        ////        //    }
        ////        //}

        ////        ////Kiem tra DICH_VU_KHAC theo THOI_GIAN_TOI_THIEU cung luc (??? cai nay co the phai tach ra kiem tra rieng)
        ////        //if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC) && chiDinh != allDichvus.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        ////        //{
        ////        //    hasErr = true;
        ////        //    goto RAISE_ERROR_HDSD_DICHVU;
        ////        //}
        ////        #endregion

        ////        #region Linhist removed 17/03/22
        ////        //Tam thoi remove doan nay de kiem tra perfomance
        ////        //if (rule.THOI_GIAN_TOI_THIEU != null && xml3.NGAY_YL != null)
        ////        //{
        ////        //    if (!allDichvus.Any(x => x.NGAY_YL != null && chiDinh != (x.NGAY_YL.Value - xml3.NGAY_YL.Value).TotalHours <= rule.THOI_GIAN_TOI_THIEU.Value))
        ////        //    {
        ////        //        file.XML1Errors.Add(new XML1Error()
        ////        //        {
        ////        //            //XEXML1ID =xml3.XML1ID,
        ////        //            XEErrorCode = Int32.Parse(rule.MA_LOI),
        ////        //            XEErrorRow = xml3.STT
        ////        //        });
        ////        //        break;
        ////        //    }

        ////        //    IEnumerable<int> xml1LIds =
        ////        //        (await _Xml1Repository.GetListAsync(x => x.MA_BN == xml3.XML1.MA_BN && x.MA_LK != xml3.MA_LK && x.FileName != xml3.XML1.FileName)).Select(y => y.XML1ID);

        ////        //    XML3 dvCuGanNhat = (await _Xml3Repository.GetListAsync(x => x.MA_DICH_VU == rule.MA_DICH_VU && x.NGAY_YL < xml3.NGAY_YL && xml1LIds.Contains(x.XML1ID.Value))).OrderByDescending(y => y.NGAY_YL).FirstOrDefault();
        ////        //    if (dvCuGanNhat != null && chiDinh != (xml3.NGAY_YL.Value - dvCuGanNhat.NGAY_YL.Value).TotalHours <= rule.THOI_GIAN_TOI_THIEU.Value)
        ////        //    {
        ////        //        file.XML1Errors.Add(new XML1Error()
        ////        //        {
        ////        //            //XEXML1ID =xml3.XML1ID,
        ////        //            XEErrorCode = Int32.Parse(rule.MA_LOI),
        ////        //            XEErrorRow = xml3.STT
        ////        //        });
        ////        //        break;
        ////        //    }

        ////        //    XML3 dvMoiGanNhat = (await _Xml3Repository.GetListAsync(x => x.MA_DICH_VU == rule.MA_DICH_VU && x.NGAY_YL > xml3.NGAY_YL && xml1LIds.Contains(x.XML1ID.Value))).OrderBy(y => y.NGAY_YL).FirstOrDefault();
        ////        //    if (dvMoiGanNhat != null && chiDinh != (dvMoiGanNhat.NGAY_YL.Value - xml3.NGAY_YL.Value).TotalHours <= rule.THOI_GIAN_TOI_THIEU.Value)
        ////        //    {
        ////        //        file.XML1Errors.Add(new XML1Error()
        ////        //        {
        ////        //            //XEXML1ID =xml3.XML1ID,
        ////        //            XEErrorCode = Int32.Parse(rule.MA_LOI),
        ////        //            XEErrorRow = xml3.STT
        ////        //        });
        ////        //        break;
        ////        //    }
        ////        //}
        ////        #endregion

        ////        RAISE_ERROR_HDSD_DICHVU:
        ////            if (hasErr)
        ////            {
        ////                file.XML1Errors.Add(new XML1Error()
        ////                {
        ////                    //XEXML1ID =xml3.XML1ID,
        ////                    XEErrorCode = Int32.Parse(rule.MA_LOI),
        ////                    XEErrorRow = xml3.STT
        ////                });
        ////            }
        ////        }
        ////        catch
        ////        { }
        ////    }

        ////}
        //#endregion

        ////Tach case verify Thong tin Dich vu rieng cho XML3
        //private bool Verify_TTDichVu(List<XML1Error> errors, IEnumerable<DMDichVu> dmDichvus, XML3 xml3, double tuoiBN, bool isNhom5937)
        //{
        //    bool hasPassed = true;
        //    var filterDichvus = dmDichvus;//.Where(x => x.HIEULUC == "Có");

        //    //114 1   XML3 MA_DICH_VU  Mã dịch vụ không được để trống
        //    //115 1   XML3 MA_DICH_VU  Mã dịch vụ không được lớn hơn 255 ký tự
        //    //181 1   XML3 MA_DICH_VU  Mã dịch vụ ngày giường không đúng cấu trúc mã khoa. Mã dịch vụ dùng chung
        //    //182 1   XML3 MA_DICH_VU  Mã dịch vụ vẩn chuyển không đúng cấu trúc công văn 5238
        //    //203 1   XML3 MA_DICH_VU  Mã dịch vụ không được tìm thấy trong danh mục phê duyệt
        //    //204 1   XML3 MA_DICH_VU  Mã dịch vụ không có hiệu lực trong danh mục phê duyệt
        //    if (string.IsNullOrEmpty(xml3.MA_DICH_VU))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 114,
        //            XEErrorRow = xml3.STT
        //        });
        //        hasPassed = false;
        //    }
        //    else if (xml3.MA_DICH_VU.Length > 255)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 115,
        //            XEErrorRow = xml3.STT
        //        });
        //        hasPassed = false;
        //    }
        //    else if (!dmDichvus.Any())
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 203,
        //            XEErrorRow = xml3.STT
        //        });
        //        hasPassed = false;
        //    }
        //    else if (!filterDichvus.Any())
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 204,
        //            XEErrorRow = xml3.STT
        //        });
        //        hasPassed = false;
        //    }
        //    #region Linhist removed 17/11/22
        //    ////Linhist added 04/05/22
        //    ////243 1   XML3 MA_DICH_VU  Dịch vụ trẻ em không được chỉ định cho người lớn(Sử dụng mã dịch vụ Khoa Nhi dành cho bệnh nhân lớn tuổi, sai TT 39 / 2018 / TT - BYT)
        //    ////haimaster 26/5/2022 update them Ma Dich VU
        //    //else if (xml3.MA_DICH_VU.StartsWith("03."))
        //    //{
        //    //    //Linhist changed 07/07/22 them ma 03.0645.0280
        //    //    //Linhist changed 17/11/22 bo kiem tra ma dich vu, kiem tra theo ten dich vu trong bang DMDichVu
        //    //    //if (tuoiBN > 16d && !Uti.CodeContain(xml3.MA_DICH_VU, "03.0645.0280;03.0001.0391;03.0003.0292;03.0006.1774;03.0007.0391;03.0008.0193;03.0011.0196;03.0015.0008;03.0018.0081;03.0019.1798;03.0022.0192;03.0023.0192;03.0024.0192;03.0025.0192;03.0040.0081;03.0043.0004;03.0054.0297;03.0058.0209;03.0065.1888;03.0066.1888;03.0067.0186;03.0069.0001;03.0076.0114;03.0078.0120;03.0079.0077;03.0080.0079;03.0081.0071;03.0082.0209;03.0083.0209;03.0084.0077;03.0085.0094;03.0088.1791;03.0089.0898;03.0090.0898;03.0091.0300;03.0092.0299;03.0096.0120;03.0102.0200;03.0113.0074;03.0114.0118;03.0115.0118;03.0116.0119;03.0118.0117;03.0121.0110;03.0124.0148;03.0130.0262;03.0133.0210;03.0138.1777;03.0144.1775;03.0145.1775;03.0146.0083;03.0155.0140;03.0157.0140;03.0158.0137;03.0159.0140;03.0160.0184;03.0161.0136;03.0162.0139;03.0164.0077;03.0191.1510;03.0218.1769;03.0233.1814;03.0234.1814;03.0237.1809;03.0238.1809;03.0239.1808;03.0240.1814;03.0353.0230;03.0354.0230;03.0355.0230;03.0356.0230;03.0357.0230;03.0358.0230;03.0360.0230;03.0364.0230;03.0365.0230;03.0366.0230;03.0368.0230;03.0369.0230;03.0370.0230;03.0371.0230;03.0372.0230;03.0373.0230;03.0374.0230;03.0377.0230;03.0381.0230;03.0382.0230;03.0383.0230;03.0385.0230;03.0387.0230;03.0388.0230;03.0389.0230;03.0390.0230;03.0391.0230;03.0392.0230;03.0394.0230;03.0397.0230;03.0399.0230;03.0400.0230;03.0401.0230;03.0402.0230;03.0404.0227;03.0407.0227;03.0408.0227;03.0409.0227;03.0410.0227;03.0411.0227;03.0412.0227;03.0413.0227;03.0414.0227;03.0415.0227;03.0416.0227;03.0420.0227;03.0422.0227;03.0425.0227;03.0426.0227;03.0427.0227;03.0429.0227;03.0430.0227;03.0432.0227;03.0433.0227;03.0438.0227;03.0439.0227;03.0441.0227;03.0442.0227;03.0443.0227;03.0444.0227;03.0445.0227;03.0447.0227;03.0450.0227;03.0451.0227;03.0452.0227;03.0454.0227;03.0455.0227;03.0456.0227;03.0457.0227;03.0459.0227;03.0460.0227;03.0461.0230;03.0463.0230;03.0464.0230;03.0465.0230;03.0466.0230;03.0467.0230;03.0468.0230;03.0469.0230;03.0470.0230;03.0472.0230;03.0477.0230;03.0478.0230;03.0479.0230;03.0480.0230;03.0481.0230;03.0482.0230;03.0483.0230;03.0484.0230;03.0486.0230;03.0487.0230;03.0490.0230;03.0491.0230;03.0493.0230;03.0494.0230;03.0496.0230;03.0497.0230;03.0499.0230;03.0500.0230;03.0501.0230;03.0502.0230;03.0503.0230;03.0504.0230;03.0505.0230;03.0506.0230;03.0508.0230;03.0509.0230;03.0511.0230;03.0512.0230;03.0514.0230;03.0519.0230;03.0520.0230;03.0522.0230;03.0523.0230;03.0524.0230;03.0525.0230;03.0526.0230;03.0527.0230;03.0528.0230;03.0529.0230;03.0530.0230;03.0531.0230;03.0532.0271;03.0534.0271;03.0535.0271;03.0536.0271;03.0537.0271;03.0538.0271;03.0539.0271;03.0540.0271;03.0541.0271;03.0542.0271;03.0543.0271;03.0548.0271;03.0551.0271;03.0552.0271;03.0553.0271;03.0554.0271;03.0555.0271;03.0557.0271;03.0559.0271;03.0563.0271;03.0564.0271;03.0565.0271;03.0567.0271;03.0569.0271;03.0570.0271;03.0571.0271;03.0574.0271;03.0575.0271;03.0576.0271;03.0577.0271;03.0578.0271;03.0579.0271;03.0581.0271;03.0583.0271;03.0584.0271;03.0586.0271;03.0587.0271;03.0588.0271;03.0589.0271;03.0590.0271;03.0591.0271;03.0593.0271;03.0594.0271;03.0596.0271;03.0597.0271;03.0598.0271;03.0599.0271;03.0600.0271;03.0603.0280;03.0606.0280;03.0607.0280;03.0609.0280;03.0612.0280;03.0613.0280;03.0617.0280;03.0618.0280;03.0622.0280;03.0623.0280;03.0626.0280;03.0627.0280;03.0633.0280;03.0634.0280;03.0639.0280;03.0640.0280;03.0641.0280;03.0643.0280;03.0644.0280;03.0647.0280;03.0649.0280;03.0650.0280;03.0652.0280;03.0655.0280;03.0656.0280;03.0659.0280;03.0660.0280;03.0663.0280;03.0667.0280;03.0669.0280;03.0670.0280;03.0672.0228;03.0673.0228;03.0674.0228;03.0677.0228;03.0681.0228;03.0682.0228;03.0683.0228;03.0684.0228;03.0690.0228;03.0691.0228;03.0692.0228;03.0701.0245;03.0705.0254;03.0708.0253;03.0715.0226;03.0716.1783;03.0743.0281;03.0749.0265;03.0767.0272;03.0768.0272;03.0772.0231;03.0782.0242;03.0807.0282;03.0808.0283;03.0892.0266;03.0894.0267;03.0901.0261;03.0902.0269;03.0907.0239;03.0989.0374;03.0990.0374;03.0992.0868;03.0993.0869;03.0995.1005;03.0998.0990;03.1001.2048;03.1002.2048;03.1003.2048;03.1004.0133;03.1005.0883;03.1016.0883;03.1026.0123;03.1032.0503;03.1034.0157;03.1035.0496;03.1041.0502;03.1045.0145;03.1046.0141;03.1057.0140;03.1059.0500;03.1060.0145;03.1067.0498;03.1069.0141;03.1070.0140;03.1071.0139;03.1074.0104;03.1077.0115;03.1080.0151;03.1082.0152;03.1087.0149;03.1091.1830;03.1092.1830;03.1093.1830;03.1122.1851;03.1125.1850;03.1126.1850;03.1127.1850;03.1133.1839;03.1141.1841;03.1142.1847;03.1144.1843;03.1148.1843;03.1169.1864;03.1189.1827;0.1826;03.1223.1879;03.1227.1880;03.1228.1885;03.1231.1882;03.1232.1882;03.1523.0858;03.1525.0806;03.1526.0815;03.1527.0815;03.1529.0806;03.1535.0733;03.1539.0733;03.1543.0806;03.1550.0749;03.1553.0748;03.1559.0815;03.1560.0812;03.1562.0821;03.1564.0733;03.1565.0812;03.1574.0802;03.1580.0850;03.1586.0828;03.1597.0828;03.1600.0827;03.1630.0775;03.1632.0731;03.1633.0731;03.1634.0805;03.1635.0841;03.1636.0805;03.1642.0786;03.1646.0775;03.1648.0575;03.1673.0740;03.1674.0774;03.1689.0785;03.1691.0759;03.1693.0738;03.1703.0075;03.1718.1037;03.1837.1031;03.1930.1018;03.1938.1035;03.1953.1035;03.1954.1019;03.1957.1033;03.1970.1031;03.1972.1031;03.1976.1067;03.1977.1067;03.1978.1067;03.1979.1067;03.1980.1067;03.1981.1069;03.1982.1069;03.1983.1069;03.1984.1069;03.1985.1069;03.1986.1069;03.1997.1064;03.2003.1056;03.2006.1054;03.2007.1054;03.2008.1054;03.2016.1084;03.2018.1068;03.2019.1068;03.2020.1068;03.2021.1068;03.2028.1066;03.2029.1066;03.2030.1066;03.2032.1069;03.2033.1069;03.2034.1069;03.2059.1068;03.2078.0986;03.2079.0981;03.2081.0950;03.2082.0986;03.2083.0983;03.2087.0984;03.2091.0983;03.2092.0949;03.2103.0911;03.2111.0981;03.2112.0984;03.2113.0936;03.2116.0992;03.2118.0882;03.2119.0505;03.2126.0884;03.2148.0912;03.2152.0867;03.2154.0897;03.2155.0869;03.2156.0917;03.2157.0876;03.2160.0938;03.2161.0948;03.2164.0442;03.2176.0892;03.2177.0965;03.2182.0895;03.2183.0893;03.2184.0899;03.2197.0963;03.2198.0982;03.2199.0979;03.2205.0955;03.2212.0912;03.2217.0896;03.2218.0876;03.2222.0966;03.2224.0946;03.2229.0945;03.2233.0980;03.2236.1085;03.2238.0894;03.2239.0893;03.2240.0914;03.2241.0871;03.2255.0616;03.2265.0618;03.2266.0391;03.2267.0391;03.2275.0054;03.2276.0054;03.2277.0054;03.2279.0054;03.2280.0055;03.2282.0054;03.2283.0054;03.2285.0167;03.2286.0054;03.2287.0055;03.2290.0391;03.2293.0054;03.2294.0055;03.2295.0055;03.2299.0054;03.2300.0054;03.2301.0055;03.2302.0054;03.2305.0055;03.2308.0054;03.2309.0054;03.2313.0054;03.2318.0058;03.2319.0058;03.2320.0057;03.2324.0109;03.2325.0096;03.2326.0095;03.2327.0096;03.2329.0095;03.2331.0164;03.2332.0078;03.2334.0499;03.2337.0165;03.2340.0157;03.2342.0169;03.2344.0166;03.2350.0061;03.2352.0087;03.2354.0077;03.2355.0077;03.2356.0505;03.2361.0054;03.2363.0169;03.2367.0112;03.2380.0302;03.2381.0305;03.2382.0313;03.2384.0307;03.2387.0212;03.2388.0212;03.2389.0212;03.2390.0212;03.2391.0215;03.2441.1059;03.2442.1045;03.2443.1045;03.2444.1045;03.2445.0562;03.2447.1181;03.2448.1181;03.2449.0834;03.2450.0945;03.2451.1049;03.2453.1093;03.2455.1045;03.2456.1044;03.2457.1044;03.2458.1044;03.2459.1174;03.2460.0379;03.2508.1049;03.2510.1059;03.2534.1047;03.2538.1060;03.2548.0737;03.2568.0983;03.2573.0940;03.2583.0488;03.2584.0488;03.2594.0944;03.2596.0940;03.2619.0408;03.2625.0408;03.2626.0408;03.2627.0408;03.2628.1059;03.2629.0407;03.2640.0407;03.2648.0446;03.2650.0448;03.2651.0449;03.2652.0449;03.2654.0454;03.2656.0460;03.2660.0448;03.2661.0448;03.2664.0454;03.2669.0417;03.2670.0458;03.2671.0491;03.2688.0464;03.2699.0484;03.2708.0416;03.2709.0424;03.2713.0416;03.2714.0416;03.2727.0692;03.2728.0661;03.2737.1181;03.2743.1185;03.2747.0534;03.2748.0534;03.2749.0534;03.2754.0345;03.2762.1059;03.2764.0562;03.2780.1180;03.2781.1180;03.2782.1179;03.2785.1870;03.2789.1165;03.2790.1171;03.2791.1171;03.2792.1170;03.2802.1870;03.2803.1870;03.2804.1871;03.2820.0004;03.2821.1164;03.2822.1166;03.2824.1162;03.2825.1167;03.2904.0561;03.2905.0561;03.2907.1064;03.2909.1064;03.2910.1064;03.2913.0337;03.2919.1136;03.2923.0772;03.2924.1086;03.2925.1087;03.2932.1136;03.2933.1136;03.2948.0437;03.2952.1136;03.2953.1137;03.2955.1134;03.2983.1135;03.2988.1134;03.2998.0323;03.3002.0324;03.3007.0076;03.3008.0333;03.3009.0333;03.3010.0333;03.3011.0331;03.3012.0331;03.3019.0334;03.3020.0334;03.3021.0348;03.3025.1149;03.3026.1150;03.3033.0340;03.3034.0339;03.3035.0329;03.3036.0329;03.3037.0329;03.3038.0329;03.3039.0329;03.3040.0329;03.3041.0329;03.3042.0329;03.3043.0329;03.3044.0329;03.3045.0329;03.3046.0329;03.3047.0329;03.3049.0561;03.3052.0387;03.3054.0566;03.3059.0369;03.3060.0384;03.3062.0373;03.3063.0373;03.3064.0372;03.3065.0377;03.3068.0370;03.3070.0386;03.3071.0370;03.3072.0370;03.3073.0369;03.3077.0572;03.3079.0570;03.3080.0377;03.3081.0377;03.3083.0576;03.3085.0406;03.3086.0403;03.3087.0405;03.3088.0403;03.3089.0403;03.3090.0394;03.3091.0403;03.3092.0403;03.3093.0403;03.3094.0403;03.3095.0403;03.3096.0403;03.3097.0403;03.3098.0403;03.3099.0403;03.3100.0403;03.3101.0403;03.3102.0403;03.3103.0403;03.3104.0403;03.3105.0403;03.3106.0403;03.3107.0403;03.3108.0403;03.3109.0403;03.3110.0403;03.3111.0403;03.3112.0403;03.3113.0403;03.3114.0403;03.3115.0403;03.3116.0403;03.3117.0403;03.3118.0406;03.3119.0391;03.3121.0403;03.3122.0403;03.3123.0403;03.3124.0395;03.3127.0403;03.3129.0403;03.3130.1206;03.3131.0403;03.3132.0403;03.3133.0394;03.3134.0394;03.3136.0404;03.3138.0403;03.3141.0405;03.3142.0396;03.3143.0402;03.3144.0392;03.3145.0402;03.3146.0402;03.3147.0402;03.3148.0402;03.3149.0393;03.3150.0403;03.3151.0403;03.3152.0403;03.3153.0393;03.3155.0403;03.3156.0402;03.3157.0392;03.3158.0402;03.3159.0402;03.3160.0402;03.3162.0403;03.3163.0397;03.3164.0401;03.3165.0395;03.3166.0402;03.3167.0402;03.3168.0402;03.3169.0397;03.3170.0402;03.3171.0393;03.3172.0393;03.3173.0393;03.3174.0393;03.3175.0393;03.3176.0393;03.3177.0393;03.3178.0393;03.3179.0393;03.3181.0403;03.3182.0401;03.3183.0393;03.3185.0402;03.3186.0402;03.3187.0393;03.3188.0393;03.3196.0397;03.3197.0397;03.3199.0393;03.3200.0393;03.3202.0395;03.3216.0399;03.3219.1187;03.3223.0406;03.3224.0406;03.3225.0406;03.3228.0408;03.3229.0408;03.3230.0408;03.3231.0411;03.3232.0408;03.3233.0411;03.3234.0400;03.3236.0411;03.3237.0411;03.3238.0442;03.3240.0411;03.3241.0411;03.3242.0408;03.3246.0411;03.3247.0094;03.3250.0411;03.3251.0411;03.3252.0411;03.3253.0408;03.3259.0583;03.3260.0414;03.3264.0411;03.3269.0446;03.3270.0446;03.3273.0446;03.3274.0446;03.3275.0446;03.3279.0449;03.3280.0449;03.3282.0493;03.3283.0493;03.3284.0448;03.3285.0448;03.3286.0449;03.3289.0491;03.3292.0491;03.3293.0456;03.3294.0448;03.3295.0465;03.3298.0465;03.3299.0454;03.3300.0456;03.3301.0458;03.3302.0458;03.3303.0465;03.3305.0456;03.3306.0456;03.3307.0456;03.3308.0456;03.3309.0465;03.3310.0465;03.3312.0458;03.3313.0455;03.3314.0456;03.3315.0491;03.3316.0491;03.3317.0583;03.3318.0458;03.3319.0454;03.3320.0454;03.3321.0456;03.3322.0454;03.3323.0453;03.3326.0506;03.3327.0459;03.3328.0686;03.3330.0493;03.3331.0458;03.3333.0461;03.3342.0456;03.3343.0461;03.3346.0663;03.3348.0494;03.3349.0494;03.3350.0494;03.3351.0460;03.3352.0461;03.3356.0669;03.3359.0494;03.3364.0494;03.3365.0494;03.3368.0494;03.3369.0494;03.3370.0494;03.3371.0494;03.3377.0494;03.3378.0494;03.3379.0494;03.3380.0498;03.3381.0492;03.3382.0489;03.3383.0584;03.3384.0492;03.3385.0493;03.3386.0686;03.3387.0489;03.3388.0489;03.3389.0456;03.3390.0487;03.3392.0417;03.3393.0489;03.3394.0464;03.3395.0492;03.3396.0492;03.3397.0492;03.3398.0465;03.3399.0600;03.3400.0632;03.3401.0492;03.3409.0466;03.3410.0466;03.3411.0466;03.3412.0466;03.3413.0466;03.3415.0471;03.3416.0493;03.3417.0481;03.3418.0481;03.3420.0466;03.3421.0481;03.3422.0474;03.3423.0469;03.3424.0469;03.3425.0466;03.3426.0469;03.3428.0474;03.3429.0474;03.3430.0469;03.3433.0466;03.3434.0475;03.3436.0481;03.3437.0481;03.3438.0464;03.3442.0481;03.3443.0464;03.3444.0464;03.3446.0499;03.3447.0482;03.3448.0486;03.3449.0481;03.3450.0481;03.3451.0486;03.3452.0486;03.3453.0484;03.3454.0464;03.3455.0481;03.3456.0486;03.3457.0486;03.3458.0493;03.3460.0464;03.3461.0484;03.3463.0484;03.3466.0439;03.3469.0416;03.3472.0416;03.3474.0422;03.3492.0421;03.3498.0464;03.3501.0422;03.3503.0424;03.3510.0424;03.3514.0424;03.3516.0429;03.3530.0429;03.3531.0421;03.3532.0121;03.3556.0705;03.3559.0705;03.3565.0491;03.3566.0705;03.3587.0435;03.3589.0492;03.3590.0492;03.3593.0603;03.3594.0218;03.3595.0662;03.3598.0491;03.3599.0492;03.3608.0505;03.3609.0553;03.3610.0553;03.3612.0566;03.3613.0566;03.3616.0567;03.3617.0553;03.3619.0565;03.3620.0565;03.3621.0553;03.3622.0565;03.3623.0565;03.3624.0565;03.3625.0565;03.3627.0567;03.3631.0567;03.3632.0567;03.3635.0369;03.3636.0369;03.3641.0567;03.3642.0567;03.3645.0550;03.3646.0556;03.3647.0556;03.3648.0534;03.3649.0556;03.3650.0553;03.3651.0558;03.3656.0557;03.3660.0555;03.3661.0548;03.3662.0556;03.3663.0556;03.3664.0548;03.3665.0556;03.3666.0550;03.3668.0534;03.3669.0548;03.3670.0550;03.3672.0551;03.3673.0556;03.3675.0556;03.3676.0556;03.3679.0556;03.3680.0534;03.3681.0534;03.3682.0534;03.3683.0534;03.3684.0556;03.3685.0571;03.3686.0571;03.3687.0571;03.3688.0556;03.3689.0556;03.3690.0556;03.3691.0577;03.3692.0577;03.3694.0556;03.3695.0571;03.3698.0535;03.3699.0555;03.3700.0550;03.3701.0550;03.3703.0556;03.3708.0552;03.3710.0571;03.3711.0571;03.3713.0543;03.3714.0556;03.3715.0556;03.3716.0550;03.3717.0556;03.3718.0556;03.3719.0555;03.3722.0548;03.3723.0534;03.3724.0549;03.3725.0556;03.3726.0534;03.3727.0556;03.3728.0548;03.3729.0571;03.3730.0543;03.3731.0556;03.3732.0556;03.3734.0555;03.3737.0557;03.3738.0556;03.3740.0534;03.3741.0571;03.3742.0550;03.3743.0556;03.3744.0556;03.3746.0540;03.3747.0540;03.3748.0550;03.3750.0550;03.3751.0540;03.3752.0550;03.3753.0550;03.3754.0556;03.3755.0534;03.3758.0556;03.3759.0556;03.3760.0556;03.3761.0556;03.3762.0556;03.3763.0559;03.3764.0555;03.3765.0556;03.3766.0556;03.3768.0538;03.3769.0538;03.3773.0556;03.3775.0534;03.3776.0571;03.3777.0571;03.3778.0556;03.3779.0556;03.3780.0537;03.3781.0556;03.3782.0556;03.3783.0575;03.3784.0556;03.3785.0556;03.3786.0556;03.3787.0556;03.3788.0556;03.3789.0556;03.3790.0537;03.3791.0537;03.3792.0534;03.3794.0556;03.3795.0534;03.3796.0534;03.3797.0571;03.3798.0571;03.3800.0577;03.3801.0573;03.3802.0573;03.3804.0559;03.3805.0572;03.3807.0574;03.3808.0573;03.3809.1052;03.3811.0571;03.3813.0551;03.3815.0493;03.3816.0571;03.3817.0505;03.3818.0218;03.3820.0573;03.3821.0216;03.3824.0575;03.3871.0532;03.3879.0407;03.3880.0548;03.3882.0568;03.3884.0573;03.3886.0553;03.3887.0556;03.3889.0556;03.3892.0553;03.3894.0573;03.3896.0344;03.3900.0563;03.3901.0563;03.3905.0563;03.3907.0573;03.3908.0573;03.3909.0505;03.3910.0505;03.3917.0980;03.3942.0365;03.3946.0961;03.3947.0963;03.3951.0873;03.3956.0969;03.3957.0975;03.3958.0969;03.3959.0918;03.3961.0958;03.3969.0413;03.3970.0413;03.3974.0447;03.3975.0412;03.3979.0443;03.3980.0447;03.3981.0443;03.3999.0445;03.4000.0443;03.4003.0450;03.4004.0457;03.4005.0457;03.4007.0457;03.4009.0457;03.4011.0490;03.4012.0467;03.4013.0470;03.4014.0470;03.4020.0477;03.4022.0476;03.4023.0478;03.4024.0477;03.4027.0452;03.4028.0445;03.4034.0450;03.4035.0450;03.4037.0463;03.4038.0457;03.4039.0457;03.4041.0457;03.4042.0457;03.4045.0457;03.4046.0490;03.4047.0443;03.4048.0457;03.4049.0457;03.4050.0457;03.4051.0457;03.4052.0457;03.4054.0457;03.4055.0457;03.4056.0457;03.4057.0457;03.4059.0457;03.4060.0463;03.4061.0457;03.4062.0461;03.4064.0462;03.4065.0462;03.4074.0457;03.4075.0457;03.4076.0451;03.4077.0457;03.4078.0451;03.4080.0457;03.4083.0419;03.4085.0419;03.4086.0419;03.4087.0419;03.4088.0420;03.4089.0419;03.4090.0419;03.4095.0418;03.4096.0420;03.4097.0420;03.4098.0418;03.4103.0440;03.4106.0436;03.4107.0152;03.4108.0440;03.4109.0440;03.4112.0427;03.4114.0426;03.4115.0426;03.4116.0418;03.4119.0440;03.4120.0423;03.4122.0435;03.4123.0691;03.4131.0691;03.4135.0690;03.4136.0689;03.4138.0148;03.4139.0689;03.4140.0689;03.4142.0344;03.4143.0541;03.4144.0541;03.4145.0542;03.4146.0541;03.4149.0550;03.4151.0541;03.4153.0541;03.4154.0541;03.4155.0542;03.4156.0541;03.4160.0965;03.4161.0968;03.4162.0965;03.4175.0292;03.4176.0292;03.4177.0292;03.4178.0230;03.4179.0230;03.4180.0230;03.4181.0227;03.4182.0230;03.4183.0271;03.4185.1894;03.4186.1894;03.4190.0192;03.4194.0312;03.4195.0312;03.4196.0312;03.4207.0302;03.4210.0302;03.4211.0168;03.4212.0076;03.4215.0754;03.4223.0379;03.4224.0380;03.4225.0378;03.4226.0374;03.4227.0437;03.4230.0373;03.4236.0387;03.4237.0374;03.4238.0374;03.4239.0951;03.4241.0561;03.4246.0198;03.4248.0004;03.4249.0004;03.4250.0008;03.4252.0004;03.4253.0003"))
        //    //    if (tuoiBN > 16d && dmDichvus.Any(d => d.MA_AX != xml3.MA_DICH_VU && d.TEN_AX == xml3.TEN_DICH_VU))
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml3.XML1ID,
        //    //            XEErrorCode = 243,
        //    //            XEErrorRow = xml3.STT
        //    //        });
        //    //        //hasPassed = false;
        //    //    }
        //    //}
        //    #endregion

        //    //112 1   XML3 MA_NHOM Mã nhóm không được để trống
        //    //113 1   XML3 MA_NHOM Mã nhóm phải khác là 4 5 6 hoặc 7
        //    //180 1   XML3 MA_NHOM Mã nhóm không khớp với mã nhóm trong danh mục phê duyệt
        //    string xml3MANHOM = $"{xml3.MA_NHOM}";

        //    if (xml3.MA_NHOM == null)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 112,
        //            XEErrorRow = xml3.STT
        //        });
        //        hasPassed = false;
        //    }
        //    #region Linhist changed 06/04/22 - kiem tra theo nhom moi MANHOM_5937
        //    //else if (xml3.MA_NHOM > 3 && xml3.MA_NHOM < 8)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml3.XML1ID,
        //    //        XEErrorCode = 113,
        //    //        XEErrorRow = xml3.STT
        //    //    });
        //    //    hasPassed = false;
        //    //}
        //    //else if (!filterDichvus.Any(x => x.MANHOM_5937 == xml3MANHOM || (string.IsNullOrEmpty(x.MANHOM_5937) && x.MANHOM_9324 == xml3MANHOM)))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml3.XML1ID,
        //    //        XEErrorCode = 180,
        //    //        XEErrorRow = xml3.STT
        //    //    });
        //    //    hasPassed = false;
        //    //}

        //    //Linhist changed 12/04/22 lay ngay vao vien de check MA_NHOM_5937
        //    //else if (xml3.NGAY_YL != null && xml3.NGAY_YL.Value >= ngay_NHOM_5937)
        //    else if (isNhom5937)
        //    {
        //        if (!filterDichvus.Any(x => x.MANHOM_5937?.ToString() == xml3MANHOM))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 180,
        //                XEErrorRow = xml3.STT
        //            });
        //            hasPassed = false;
        //        }
        //    }
        //    else
        //    {
        //        if (!filterDichvus.Any(x => x.MANHOM_9324?.ToString() == xml3MANHOM))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 180,
        //                XEErrorRow = xml3.STT
        //            });
        //            hasPassed = false;
        //        }
        //    }
        //    #endregion

        //    //118 1   XML3 TEN_DICH_VU Tên dịch vụ không được để trống
        //    //119 1   XML3 TEN_DICH_VU Tên dịch vụ không được lớn hơn 255 ký tự
        //    //183 1   XML3 TEN_DICH_VU Tên dịch vụ không khớp với tên dịch vụ trong danh mục phê duyệt
        //    //184 1   XML3 TEN_DICH_VU Tên dịch vụ phải nằm trong thẻ < ![CDATA[...]] >
        //    if (string.IsNullOrEmpty(xml3.TEN_DICH_VU))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 118,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (xml3.TEN_DICH_VU.Length > 255)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 119,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterDichvus.Any(x => Uti.IsEqual(xml3.TEN_DICH_VU, x.TEN_DICH_VU, ignoreBracket: true)))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 183,
        //            XEErrorRow = xml3.STT
        //        });
        //    }

        //    //245 1   XML3 MA_DICH_VU  Định Lượng Cholesterol Toàn Phần(Máu) không được chỉ định quá 1 lần trong 3 tháng và không quá 1 lần trong 30 ngày với mã bệnh E78
        //    //246 1   XML3 MA_DICH_VU  Định Lượng Hdl - C[Máu] Không được chỉ định quá 1 lần trong tháng với bệnh nhân nội trú, không quá 1 lần trong 3 tháng và không quá 1 lần trong 30 ngày với mã bệnh E78
        //    //247 1   XML3 MA_DICH_VU  Định Lượng Ldl - C[Máu] Không được chỉ định quá 1 lần trong tháng với bệnh nhân nội trú, không quá 1 lần trong 3 tháng và không quá 1 lần trong 30 ngày với mã bệnh E78
        //    //248 1   XML3 MA_DICH_VU  Định Lượng Triglycerid(Máu)[Máu] Không được chỉ định quá 1 lần trong tháng với bệnh nhân nội trú, không quá 1 lần trong 3 tháng và không quá 1 lần trong 30 ngày với mã bệnh E78
        //    //249 1   XML3 MA_DICH_VU  Chỉ định sai phác đồ điều trị của Bộ Y Tế và Công văn 5162 / BHXH - CSYT của BHXH
        //    //260 1   XML3 MA_DICH_VU  HBsAg Test Nhanh không được phép chỉ định cho bệnh nhân sau phẫu thuật/ thủ thuật
        //    //261 1   XML3 MA_DICH_VU  Định lượng HbA1c[Máu] không được chỉ định quá 1 lần trong 90 ngày
        //    //262 1   XML3 MA_DICH_VU  Định lượng HbA1c[Máu] chỉ được thực hiện cho Bệnh nhân Đái tháo đường[TT13 / 2020 / TT - BYT]
        //    //263 1   XML3 MA_DICH_VU  Định lượng Acid Uric[Máu] không được phép chỉ định cho bệnh này(tài liệu các xét nghiệm thường qui áp dụng trong thực hành lâm sàng NXB Y học năm 2013)	
        //    //264 1   XML3 MA_DICH_VU  Định lượng Urê máu[Máu] không được phép chỉ định cho bệnh này(tài liệu các xét nghiệm thường qui áp dụng trong thực hành lâm sàng NXB Y học năm 2013)	
        //    //265 1   XML3 MA_DICH_VU  Đo hoạt độ CK(Creatine kinase) [Máu] không được phép chỉ định cho bệnh này(tài liệu các xét nghiệm thường qui áp dụng trong thực hành lâm sàng NXB Y học năm 2013)	
        //    //266 1   XML3 MA_DICH_VU  Đo hoạt độ CK-MB(Isozym MB of Creatine kinase)[Máu] không được phép chỉ định cho bệnh này(tài liệu các xét nghiệm thường qui áp dụng trong thực hành lâm sàng NXB Y học năm 2013)	
        //    //267 2   XML3 MA_DICH_VU  Mô bệnh học sau 1 ngày mới được chỉ định tiếp Nhuộm PAS
        //    //269 1   XML3 MA_DICH_VU  Xét nghiệm đường máu mao mạch tại giường sai với hướng dẫn Công văn 5388 / BYT - BH của Bộ Y Tế
        //    //270 1   XML3 MA_DICH_VU  Chụp động mạch vành không được phép chỉ định cho bệnh này(Chuyên đề Bảo hiểm)
        //    //329 1   XML3 MA_DICH_VU  Định Lượng Cholesterol Toàn Phần(máu) không được phép chỉ định cho bệnh này
        //    //330 1   XML3 MA_DICH_VU  Định Lượng Hdl - C[Máu] Không được phép chỉ định cho bệnh này
        //    //331 1   XML3 MA_DICH_VU  Định Lượng Ldl - C[Máu] Không được phép chỉ định cho bệnh này
        //    //332 1   XML3 MA_DICH_VU  Định Lượng Triglycerid[Máu] Không được phép chỉ định cho bệnh này
        //    //333 1   XML3 MA_DICH_VU  Fibrinogen không được phép hỉ định cho bệnh này
        //    //334 1   XML3 MA_DICH_VU  Đo hoạt độ ALT không được phép chỉ định cho bệnh này
        //    //335 1   XML3 MA_DICH_VU  Định lượng Creatinin chỉ áp dụng cho Bệnh nhân Đái Tháo Đường và Tăng Huyến Áp
        //    //336 1   XML3 MA_DICH_VU  Định lượng Glucose chỉ áp dụng cho Bệnh nhân Đái Tháo Đường
        //    //337 1   XML3 MA_DICH_VU  Tổng phân tích nước Tiểu chỉ áp dụng cho Bệnh nhân Đái Tháo Đường hoặc Tăng Huyến Áp
        //    //338 1   XML3 MA_DICH_VU  Tổng phân tích tế bào máu chỉ áp dụng cho Bệnh nhân Đái tháo Đường hoặc Tăng Huyến Áp
        //    //339 1   XML3 MA_DICH_VU  Tổng phân tích tế bào máu không được phép chỉ định quá 1 lần trong 6 tháng
        //    //340 1   XML3 MA_DICH_VU  Điện tim không được phép chỉ định cho bệnh này
        //    //341 1   XML3 MA_DICH_VU  Điện tim không được phép chỉ định quá 1 lần trong 3 tháng cho bệnh nhân tiểu đường hoặc cao huyết áp
        //    //342 1   XML3 MA_DICH_VU  Siêu âm ổ bụng không được phép chỉ định cho bệnh này
        //    //343 1   XML3 MA_DICH_VU  Siêu âm ổ bụng không được phép chỉ định quá 1 lần trong 6 tháng
        //    //275 1   XML3 MA_DICH_VU  Đo chức năng hô hấp không được chỉ định cho bệnh này(Chuyên đề Bảo hiểm)
        //    //276 1   XML3 MA_DICH_VU  Điều trị tủy răng bắt buộc phải chụp Xquang răng
        //    //277 1   XML3 MA_DICH_VU  Dịch vụ Vi khuẩn nhuộm soi không được kê kèm các dịch vụ: Vi khuẩn nuôi cấy(Chuyên đề Bảo hiểm)
        //    //278 1   XML3 MA_DICH_VU  Xét nghiệm máu ABO và Rh(D) ko phù hợp(Chuyên đề Bảo hiểm)
        //    //345 1   XML3 MA_DICH_VU  Siêu Âm Tim không được phép chỉ định quá 1 lần trong 6 tháng cho các bệnh nhân có Bệnh Tiểu Đường hoặc Cao huyết Áp
        //    //346 1   XML3 MA_DICH_VU  Có dịch vụ xét nghiệm nhưng không kê thuốc
        //    //347 1   XML3 MA_DICH_VU  Đo chức năng hô hấp không được phép chỉ định cùng dịch vụ Test giãn phế quản
        //    //348 1   XML3 MA_DICH_VU  Phẫu thuật Phaco chỉ dành cho mổ đục TTT
        //    //349 1   XML3 MA_DICH_VU  Ghi điện não đồ không được phép chỉ định cho bệnh này
        //    //350 1   XML3 MA_DICH_VU  Định lượng BNP không được phép chỉ định cho bệnh này
        //    //320 1   XML3 MA_DICH_VU  Đo hoạt độ ALT không được phép chỉ định quá 1 lần trong 3 tháng
        //    //321 1   XML3 MA_DICH_VU  Định lượng Creatinin không được phép chỉ định quá 1 lần trong 3 tháng với bệnh E10, E11, E12, E13, E14 và không quá 1 lần trong 30 ngày nếu có thêm bệnh N17, N18, N04, N08, I12, I13
        //    //322 1   XML3 MA_DICH_VU  Định lượng Urê máu không được phép chỉ định quá 1 lần trong 3 tháng với bệnh E10, E11, E12, E13, E14 và không quá 1 lần trong 30 ngày nếu có thêm bệnh N17, N18, N04, N08, I12, I13
        //    //323 1   XML3 MA_DICH_VU  Định lượng Glucose máu không được phép chỉ định quá 1 lần trong 6 tháng
        //    //324 1   XML3 MA_DICH_VU  Bơm Thông Lệ Đạo chỉ được dùng khi có bệnh về mắt
        //    //325 1   XML3 MA_DICH_VU  Tổng phân tích nước Tiểu chỉ áp dụng cho Bệnh nhân Đái Tháo Đường hoặc Tăng Huyến Áp
        //    //326 1   XML3 MA_DICH_VU  Tổng phân tích nước Tiểu không được phép chỉ định quá 1 lần trong 6 tháng
        //    //442 1   XML3 MA_DICH_VU  Định lượng Urê máu máu chưa phù hợp theo Quyết định số 3319 / QĐ - BYT và Quyết định số 3879 / QĐ - BYT ngày 30 / 9 / 2014 của Bộ Y tế
        //    //470 2   XML3 MA_DICH_VU  Chỉ định khám Nhi sai với CCHN Bác Sĩ
        //    //471 2   XML3 MA_DICH_VU  Chỉ định nội soi TMH sai CCHN Bác Sĩ
        //    //462 1   XML3 MA_DICH_VU  Dịch vụ phẫu thuật chưa có biên bản hội chẩn trong hồ sơ XML đề nghị thanh toán
        //    //463 1   XML3 MA_DICH_VU  Không có Biên bản tường trình  cho Bệnh nhân Phẫu Thuật
        //    //464 1   XML3 MA_DICH_VU  Chụp CT, MRI thiếu biên bản Hội chẩn trong hồ sơ XML đề nghị thanh toán
        //    //754 1   XML3 MA_DICH_VU  Định lượng Pro - calcitonin[Máu] không thực hiện đồng thời với DVKT Định lượng CRP/ CRP hs NULL
        //    //750 1   XML3 MA_DICH_VU  Trùng công khám chữa bệnh trong một lần đi khám
        //    //751 1   XML3 MA_DICH_VU  Không thực hiện các dịch vụ chuyên khoa Điện Quang trong cùng một thời điểm
        //    //477 1   XML3 MA_DICH_VU  Không có Biên bản tường trình cho bệnh nhân thực hiện thủ thật
        //    //458 1   XML3 MA_DICH_VU  Dịch vụ Siêu âm ổ bụng không được chỉ định cùng thời điểm với Siêu âm các tạng trong ổ bụng / Siêu âm hệ tiết niệu/ Siêu âm tử cung, phần phụ
        //    //459 1   XML3 MA_DICH_VU  Dịch vụ Chụp, nong động mạch vành bằng bóng đã bao gồm vật tư bơm áp lực các loại…	
        //    //460 1   XML3 MA_DICH_VU  Các dịch vụ xét nghiệm đông máu chỉ áp dụng với Bệnh nhân Phẫu Thuật
        //    //437 1   XML3 MA_DICH_VU  Định lượng Creatinin máu chưa phù hợp theo Quyết định số 3319 / QĐ - BYT và Quyết định số 3879 / QĐ - BYT ngày 30 / 9 / 2014 của Bộ Y tế
        //    //438 1   XML3 MA_DICH_VU  Tổng phân tích nước tiểu chưa phù hợp  theo Quyết định số 3319 / QĐ - BYT và Quyết định số 3879 / QĐ - BYT ngày 30 / 9 / 2014 của Bộ Y tế
        //    //312 1   XML3 MA_DICH_VU  Siêu âm Doppler Tim chỉ được phép chỉ định cho bệnh nhân Tăng Huyến Áp hoặc Đái Tháo Đường
        //    //355 1   XML3 MA_DICH_VU  Siêu âm dopler và điện tim thường không được chỉ định đồng thời theo qđ 3983 / 2014 / qđ - byt
        //    //367 1   XML3 MA_DICH_VU  Dịch vụ kê ngoài phác đồ điều trị bệnh Sỏi Niệu Quản
        //    //365 1   XML3 MA_DICH_VU  Dịch vụ kê ngoài phác đồ điều trị bệnh Viêm Kết Giác Mạc
        //    //363 1   XML3 MA_DICH_VU  Dịch vụ kê ngoài phác đồ điều trị bệnh Đục Thuỷ Tinh Thể
        //    //371 1   XML3 MA_DICH_VU  Dịch vụ kê ngoài phác đồ điều trị bệnh Chưa ngoài tử cung
        //    //369 1   XML3 MA_DICH_VU  Dịch vụ kê ngoài phác đồ điều trị bệnh Cứng khớp sau mổ đứt dây chằng chéo khớp gối
        //    //409 1   XML3 MA_DICH_VU  Đo chức năng hô hấp không được phép chỉ định cho bệnh này
        //    //410 1   XML3 MA_DICH_VU  Không được phép chỉ định đồng thời các dịch vụ: Siêu âm tuyến giáp, Siêu âm hạch vùng cổ, Siêu ấm tuyến nước bọt
        //    //411 1   XML3 MA_DICH_VU  Không được phép chỉ định đồng thời các dịch vụ: Nội soithực quản dạ dày tá tràng có sinh thiết, Nội soithực quản dạ dày tá tràng không sinh thiết, Nội soi thực quản -Dạ dày - Tá tràng qua đường mũi, Nội soi can thiệp - tiêm cầm máu, Nội soi can thiệp - cắt polyp ống tiêu hóa  1cm hoặc nhiều polyp
        //    //412 1   XML3 MA_DICH_VU  Không được phép chỉ định đồng thời các dịch vụ: Làm thuốc tai, Lấy nút biểu bì ống tai ngoài
        //    //413 1   XML3 MA_DICH_VU  Khám nội chỉ được phép thực hiện 22 ngày 1 lần với các bệnh:  Đái tháo đường và Huyết áp
        //    //414 1   XML3 MA_DICH_VU  Khám Răng hàm mặt chỉ được phép thực hiện 30 ngày 1 lần
        //    //415 1   XML3 MA_DICH_VU  Khám YHCT chỉ được phép thực hiện 30 ngày 1 lần
        //    //416 1   XML3 MA_DICH_VU  Khám Phục hồi chức năng chỉ được phép thực hiện 30 ngày 1 lần
        //    //417 1   XML3 MA_DICH_VU  Rửa Cùng đồ không được phép chỉ định cho bện viêm kết mạc(QĐ 40 / BYT)

        //    //122 1   XML3 DON_VI_TINH Đơn vị tính không được lớn hơn 50 ký tự
        //    //187 1   XML3 DON_VI_TINH Đơn vị tính không được để trống
        //    if (string.IsNullOrEmpty(xml3.DON_VI_TINH))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 187,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (xml3.DON_VI_TINH.Length > 50)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 122,
        //            XEErrorRow = xml3.STT
        //        });
        //    }

        //    //125 1   XML3 DON_GIA Đơn giá không được để trống
        //    //126 1   XML3 DON_GIA Đơn giá phải là số thập phân lớn hơn 0 , làm tròn đến đơn vị đồng
        //    //190 1   XML3 DON_GIA Đơn giá không khớp với đơn giá trong danh mục phê duyệt
        //    //191 1   XML3 DON_GIA Đơn giá nhỏ hơn đơn giá trong danh mục phê duyệt
        //    //207 1   XML3 DON_GIA Đơn giá lớn hơn đơn giá trong danh mục phê duyệt
        //    if (xml3.DON_GIA == null)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 125,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterDichvus.Any(x => x.DON_GIA == xml3.DON_GIA))
        //    {
        //        //Linhist changed 27/05/22 them case don gia lon hon DM phe duyet
        //        double? maxGia = filterDichvus.Where(x => x.DON_GIA != null && Uti.IsEqual(xml3.TEN_DICH_VU, x.TEN_DICH_VU, ignoreBracket: true)).Max(x => x.DON_GIA);
        //        if (maxGia != null && xml3.DON_GIA > maxGia)
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 207,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //        else
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 190,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //    }
        //    //else
        //    //{
        //    //    bool isBigger = false;
        //    //    bool isSmaller = false;
        //    //    foreach (DMDichVu dv in dichvus)
        //    //    {
        //    //        if (dv.GIA_AX > xml3.DON_GIA)
        //    //        {
        //    //            isBigger = true;
        //    //        }
        //    //        else if (dv.GIA_AX < xml3.DON_GIA)
        //    //        {
        //    //            isSmaller = true;
        //    //        }
        //    //    }
        //    //    if (isBigger != isSmaller)
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml3.XML1ID,
        //    //            XEErrorCode = isBigger ? 191 : 207,
        //    //            XEErrorRow = xml3.STT
        //    //        });
        //    //    }
        //    //}

        //    return hasPassed;
        //}

        ////linhist added 18/05/2023
        //private bool Check_TTTHAU(string TT_THAU, string DM_THAU)
        //{
        //    if (string.IsNullOrWhiteSpace(DM_THAU)) return true;
        //    if (string.IsNullOrWhiteSpace(TT_THAU)) return false;

        //    //linhist added 07/06/2023
        //    if (DM_THAU == TT_THAU) return true;

        //    string[] mts = DM_THAU.Split(';');
        //    if (mts.Length < 3) return TT_THAU == DM_THAU;
        //    return (TT_THAU == $"{mts[0]};{mts[1]};{mts[2]}");
        //}

        ////Tach case verify Thong tin Vat tu rieng cho XML3
        //private void Verify_TTVatTu(List<XML1Error> errors, IEnumerable<DMVTYT> dmVattus, XML3 xml3, bool isNhom5937)
        //{
        //    var filterVattus = dmVattus;

        //    //116 1   XML3 MA_VAT_TU   Mã vật tư không được để trống
        //    //117 1   XML3 MA_VAT_TU   Mã vật tư không được lớn hơn 255 ký tự
        //    //205 1   XML3 MA_VAT_TU   Mã vật tư không tìm thấy trong danh mục phê duyệt
        //    //206 1   XML3 MA_VAT_TU   Mã vật tư không có hiệu lực trong danh mục phê duyệt
        //    if (string.IsNullOrEmpty(xml3.MA_VAT_TU))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 116,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (xml3.MA_VAT_TU.Length > 255)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 117,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!dmVattus.Any())
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 205,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterVattus.Any())
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 206,
        //            XEErrorRow = xml3.STT
        //        });
        //    }

        //    //112 1   XML3 MA_NHOM Mã nhóm không được để trống
        //    //113 1   XML3 MA_NHOM Mã nhóm phải khác là 4 5 6 hoặc 7
        //    //180 1   XML3 MA_NHOM Mã nhóm không khớp với mã nhóm trong danh mục phê duyệt
        //    string xml3MANHOM = $"{xml3.MA_NHOM}";
        //    if (xml3.MA_NHOM == null)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 112,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    #region Linhist changed 06/04/22 - kiem tra theo nhom moi MANHOM_5937
        //    //else if (xml3.MA_NHOM > 3 && xml3.MA_NHOM < 8)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml3.XML1ID,
        //    //        XEErrorCode = 113,
        //    //        XEErrorRow = xml3.STT
        //    //    });
        //    //    hasPassed = false;
        //    //}
        //    //else if (!filterDichvus.Any(x => x.MANHOM_5937 == xml3MANHOM || (string.IsNullOrEmpty(x.MANHOM_5937) && x.MANHOM_9324 == xml3MANHOM)))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml3.XML1ID,
        //    //        XEErrorCode = 180,
        //    //        XEErrorRow = xml3.STT
        //    //    });
        //    //    hasPassed = false;
        //    //}

        //    //Linhist changed 12/04/22 lay ngay vao vien de check MA_NHOM_5937
        //    //else if (xml3.NGAY_YL != null && xml3.NGAY_YL.Value >= ngay_NHOM_5937)
        //    else if (isNhom5937)
        //    {
        //        if (!filterVattus.Any(x => x.MANHOM_5937?.ToString() == xml3MANHOM))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 180,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //    }
        //    else
        //    {
        //        if (!filterVattus.Any(x => x.MANHOM_9324?.ToString() == xml3MANHOM))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 180,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //    }
        //    #endregion

        //    ////1134    1   XML3 TT_THAU Sai thông tin thầu của Vật Tư Y Tế
        //    ////Linhist added 08/07/22
        //    //if (!filterVattus.Any(x => xml3.TT_THAU.EndsWith(x.QUYET_DINH)))
        //    //{
        //    //    xml3.XML1.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml3.XML1ID,
        //    //        XEErrorCode = 1134,
        //    //        XEErrorRow = xml3.STT
        //    //    });
        //    //}
        //    //Linhist added 18/05/23
        //    if (!filterVattus.Any(vt => Check_TTTHAU(xml3.TT_THAU, vt.TT_THAU)))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 1134,
        //            XEErrorRow = xml3.STT
        //        });
        //    }

        //    //120 1   XML3 TEN_VAT_TU  Tên vật tư không được để trống
        //    //121 1   XML3 TEN_VAT_TU  Tên vật tư không được lớn hơn 255 ký tự
        //    //185 1   XML3 TEN_VAT_TU  Tên vật tư không khớp với tên vật tư trong danh mục phê duyệt
        //    //186 1   XML3 TEN_VAT_TU  Tên vật tư phải nằm trong thẻ < ![CDATA[...]] >
        //    if (string.IsNullOrEmpty(xml3.TEN_VAT_TU))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 120,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (xml3.TEN_VAT_TU.Length > 255)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 121,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterVattus.Any(x => Uti.IsEqual(xml3.TEN_VAT_TU, x.TEN_VAT_TU, ignoreBracket: true)))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 185,
        //            XEErrorRow = xml3.STT
        //        });
        //    }

        //    //122 1   XML3 DON_VI_TINH Đơn vị tính không được lớn hơn 50 ký tự
        //    //187 1   XML3 DON_VI_TINH Đơn vị tính không được để trống
        //    //188 1   XML3 DON_VI_TINH Đơn vị tính không khớp với đơn vị tính trong danh mục phê duyệt(Phân biệt chữ hoa, chữ thường)
        //    //189 1   XML3 DON_VI_TINH Đơn vị tính không khớp với đơn vị tính trong danh mục phê duyệt(Không phân biệt chữ hoa, chữ thường)
        //    if (string.IsNullOrEmpty(xml3.DON_VI_TINH))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 187,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (xml3.DON_VI_TINH.Length > 50)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 122,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterVattus.Any(x => x.DON_VI_TINH == xml3.DON_VI_TINH))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 188,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterVattus.Any(x => Uti.IsEqual(x.DON_VI_TINH, xml3.DON_VI_TINH, ignoreCase: true)))
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 189,
        //            XEErrorRow = xml3.STT
        //        });
        //    }

        //    //125 1   XML3 DON_GIA Đơn giá không được để trống
        //    //126 1   XML3 DON_GIA Đơn giá phải là số thập phân lớn hơn 0 , làm tròn đến đơn vị đồng
        //    //190 1   XML3 DON_GIA Đơn giá không khớp với đơn giá trong danh mục phê duyệt
        //    //191 1   XML3 DON_GIA Đơn giá nhỏ hơn đơn giá trong danh mục phê duyệt
        //    //207 1   XML3 DON_GIA Đơn giá lớn hơn đơn giá trong danh mục phê duyệt
        //    if (xml3.DON_GIA == null)
        //    {
        //        xml3.XML1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml3.XML1ID,
        //            XEErrorCode = 125,
        //            XEErrorRow = xml3.STT
        //        });
        //    }
        //    else if (!filterVattus.Any(x => x.DON_GIA == xml3.DON_GIA))
        //    {
        //        //Linhist changed 06/04/23 - khong con su dung QUYET_DINH trong DMVTYT nua
        //        //Linhist added 27/05/22 them case don gia lon hon DM phe duyet
        //        //double? maxGia = filterVattus.Where(x => x.DON_GIA != null && Uti.IsEqual(xml3.TEN_VAT_TU, x.TEN_VAT_TU, ignoreQuote: true) &&
        //        //                                                Uti.IsEqual(x.DON_VI_TINH, xml3.DON_VI_TINH, ignoreCase: true) &&
        //        //                                                xml3.TT_THAU != null && xml3.TT_THAU.ToLower().Contains(x.QUYET_DINH)).Max(x => x.DON_GIA);
        //        double? maxGia = filterVattus.Where(x => x.DON_GIA != null && Uti.IsEqual(xml3.TEN_VAT_TU, x.TEN_VAT_TU, ignoreBracket: true) &&
        //                                                        Uti.IsEqual(x.DON_VI_TINH, xml3.DON_VI_TINH, ignoreCase: true)).Max(x => x.DON_GIA);

        //        if (maxGia != null && xml3.DON_GIA > maxGia)
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 207,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //        else
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 190,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //    }
        //}

        //#endregion

        //#region Linhist Added 17/03/22
        ////Tach case verify Ma Benh rieng cho XML1, tra ve list MA_BENH gop ca ICD10
        //private string Verify_TTBenh(List<XML1Error> errors, IEnumerable<DMYHDTICD10> dmYhdt, XML1 xml1, Hospital currentHospital, IEnumerable<DMBVTuyenTinh> dmTuyenTinhs)
        //{
        //    string allMaBenhs = $"{xml1.MA_BENH};{xml1.MA_BENHKHAC}";

        //    //37  1   XML1 MA_BENH Mã bệnh không được để trống
        //    //38  1   XML1 MA_BENH Mã bệnh phải có độ dài từ 3 đến 8 ký tự, không có khoảng trắng ở giữa
        //    if (string.IsNullOrEmpty(xml1.MA_BENH))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 37,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.MA_BENH?.Length > 8)
        //    {
        //        var benhYhDt = dmYhdt.FirstOrDefault(x => x.YHDTCode == xml1.MA_BENH);
        //        if (benhYhDt == null)
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 38,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //        else
        //        {
        //            allMaBenhs += $";{benhYhDt.ICD10Code}";
        //        }
        //    }
        //    else
        //    {
        //        //418 2   XML1 MA_BENH Hồ sơ khám sức khỏe không được thanh toán BHYT
        //        if (allMaBenhs.Contains("Z") && Uti.CodeMatch(allMaBenhs, "Z00.*;Z01.*;Z02.*;Z03.*;Z04.*;Z05.*;Z06.*;Z07.*;Z08.*;Z09.*;Z10.*;Z11.*;Z12.*;Z13.*"))
        //        {
        //            //Linhist added 15/06/22 bo qua ma benh Z08.8 ko bao loi
        //            if (!Uti.CodeContain(allMaBenhs, "Z08.8"))
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 418,
        //                    XEErrorRow = xml1.STT
        //                });
        //            }
        //        }
        //    }

        //    //39  1   XML1 MA_BENHKHAC Mã bệnh khác không được lớn hơn 255 ký tự
        //    if (xml1.MA_BENHKHAC?.Trim().Length > 255)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 39,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (!string.IsNullOrEmpty(xml1.MA_BENHKHAC))
        //    {
        //        var benhIC10s = dmYhdt.Where(x => Uti.CodeMatch(x.YHDTCode, xml1.MA_BENHKHAC)).Select(x => x.ICD10Code);
        //        allMaBenhs += ";" + string.Join(";", benhIC10s);
        //    }

        //    //144 1   XML1 TEN_BENH    Tên bệnh không đúng định dạng<![CDATA[...]] >
        //    //36  1   XML1 TEN_BENH    Tên bệnh không được lớn hơn 2000 ký tự
        //    if (xml1.TEN_BENH?.Length > 2000)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 36,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //73  1   XML1 MA_LOAI_KCB Mã loại KCB không được để trống
        //    //74  1   XML1 MA_LOAI_KCB Mã loại KCB phải có giá trị 1 2 hoặc 3
        //    //313 1   XML1 MA_LOAI_KCB Bệnh nhân ngoại trú phải có công khám
        //    if (xml1.MA_LOAI_KCB == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 73,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    //10/12/2022 haimaster update
        //    else if (xml1.MA_LOAI_KCB != 1 && xml1.MA_LOAI_KCB != 3 && xml1.MA_LOAI_KCB != 2 && xml1.MA_LOAI_KCB != 9)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 74,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //75  1   XML1 MA_KHOA Mã khoa không có độ dài lớn hơn 255 ký tự
        //    //426 1   XML1 MA_KHOA Mã Khoa không nằm trong danh mục được phê duyệt
        //    if (xml1.MA_KHOA?.Length > 255)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 75,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //40  1   XML1 MA_LYDO_VVIEN   Mã lý do vào viện không được để trống
        //    //41  1   XML1 MA_LYDO_VVIEN   Mã lý do vào viện phải có giá trị 1 2 hoặc 3
        //    //219 1   XML1 MA_LYDO_VVIEN   Sai mã lý do vào viện
        //    if (xml1.MA_LYDO_VVIEN == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 40,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (!Uti.NumExists(xml1.MA_LYDO_VVIEN, 1, 2, 3, 4))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 41,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else
        //    {
        //        //Linhist changed 30/12/22
        //        //Linhist added 11/06/22
        //        //219 Sai ma ly do vao vien bo qua = 2 (cap cuu) & chi kiem tra BV tuyen huyen (Loai_Tuyen = 2)
        //        //Linhist changed 07/07/22 tuyen huyen = 3
        //        //if (xml1.MA_LYDO_VVIEN != 2 && currentHospital.Loai_Tuyen == 3)
        //        //{
        //        //    string fstDKBD = string.IsNullOrEmpty(xml1.MA_DKBD) ? "" : xml1.MA_DKBD.Trim().Split(';')[0];
        //        //    bool isThongTuyen = fstDKBD == "" ? false : (fstDKBD.Substring(0, 2) == currentHospital.MA_CSKCB.Substring(0, 2));

        //        //    if (xml1.MA_LYDO_VVIEN == 1)
        //        //    {
        //        //        if (currentHospital.MA_CSKCB != fstDKBD)
        //        //        {
        //        //            file.XML1Errors.Add(new XML1Error()
        //        //            {
        //        //                //XEXML1ID =xml1.XML1ID,
        //        //                XEErrorCode = 219,
        //        //                XEErrorRow = xml1.STT
        //        //            });
        //        //        }
        //        //    }
        //        //    else if ((xml1.MA_LYDO_VVIEN == 4 && !isThongTuyen) || (xml1.MA_LYDO_VVIEN == 3 && isThongTuyen))
        //        //    {
        //        //        file.XML1Errors.Add(new XML1Error()
        //        //        {
        //        //            //XEXML1ID =xml1.XML1ID,
        //        //            XEErrorCode = 219,
        //        //            XEErrorRow = xml1.STT
        //        //        });
        //        //    }
        //        //}

        //        //Linhist changed 30/12/22
        //        //Vao vien Dung tuyen (MA_LYDO_VVIEN = 1 && MA_DKBD == MA_CSKCB)
        //        //haimater update 3.1.2023 chi check tuyen Huyen
        //        if (currentHospital.Loai_Tuyen == 3)
        //        {
        //            if (xml1.MA_LYDO_VVIEN != 2)
        //            {
        //                if (xml1.MA_DKBD == xml1.MA_CSKCB)
        //                {
        //                    if (xml1.MA_LYDO_VVIEN != 1)
        //                    {
        //                        xml1.XML1Errors.Add(new XML1Error()
        //                        {
        //                            //XEXML1ID =xml1.XML1ID,
        //                            XEErrorCode = 219,
        //                            XEErrorRow = xml1.STT
        //                        });
        //                    }
        //                }
        //                else
        //                {
        //                    string maBD = xml1.MA_DKBD?[..2];
        //                    string maCS = xml1.MA_CSKCB?[..2];
        //                    if (maBD != maCS)
        //                    {
        //                        if (xml1.MA_LYDO_VVIEN != 3)
        //                        {
        //                            xml1.XML1Errors.Add(new XML1Error()
        //                            {
        //                                //XEXML1ID =xml1.XML1ID,
        //                                XEErrorCode = 219,
        //                                XEErrorRow = xml1.STT
        //                            });
        //                        }
        //                    }
        //                    else if (currentHospital.Loai_Tuyen == 3)
        //                    {
        //                        if (dmTuyenTinhs.Any(b => b.BVTCode == xml1.MA_DKBD))
        //                        {
        //                            if (xml1.MA_LYDO_VVIEN != 3)
        //                            {
        //                                xml1.XML1Errors.Add(new XML1Error()
        //                                {
        //                                    //XEXML1ID =xml1.XML1ID,
        //                                    XEErrorCode = 219,
        //                                    XEErrorRow = xml1.STT
        //                                });
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (xml1.MA_LYDO_VVIEN != 4)
        //                            {
        //                                xml1.XML1Errors.Add(new XML1Error()
        //                                {
        //                                    //XEXML1ID =xml1.XML1ID,
        //                                    XEErrorCode = 219,
        //                                    XEErrorRow = xml1.STT
        //                                });
        //                            }
        //                        }
        //                    }
        //                    else if (xml1.MA_LYDO_VVIEN != 4)
        //                    {
        //                        xml1.XML1Errors.Add(new XML1Error()
        //                        {
        //                            //XEXML1ID =xml1.XML1ID,
        //                            XEErrorCode = 219,
        //                            XEErrorRow = xml1.STT
        //                        });
        //                    }
        //                }
        //            }
        //        }


        //        //Linhist added 07/12/22
        //        //Chi kiem tra BV tuyen TW & Tinh (Loai_Tuyen = 1 hoac 2)
        //        if (Uti.NumExists(currentHospital.Loai_Tuyen, 1, 2))
        //        {
        //            //1300	1	XML1	MA_LYDO_VVIEN	Bệnh nhân nội trú vào viện sai quyết định 1351/QĐ BHXH)
        //            if (Uti.StrExists(xml1.MA_KHUVUC, true, "K1", "K2", "K3") && !Uti.NumExists(xml1.MA_LYDO_VVIEN, 2, 4))
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 1300,
        //                    XEErrorRow = xml1.STT
        //                });
        //            }
        //            //1301	1	XML1	MA_LYDO_VVIEN	Bệnh nhân ngoại trú lựa chọn thông tuyến (Điều 4 QĐ 1351/QĐ BHXH)
        //            if (xml1.MA_LYDO_VVIEN == 4 && Uti.NumExists(xml1.MA_LOAI_KCB, 1, 2))
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 1301,
        //                    XEErrorRow = xml1.STT
        //                });
        //            }
        //        }
        //    }

        //    //42  1   XML1 MA_NOI_CHUYEN   Mã nơi chuyển không quá 5 ký tự hoặc để trống
        //    //145 1   XML1 MA_NOI_CHUYEN   Mã nơi chuyển không được trùng với mã bệnh viện
        //    if (xml1.MA_NOI_CHUYEN?.Length > 5)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 42,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.MA_NOI_CHUYEN != null && xml1.MA_NOI_CHUYEN == xml1.MA_CSKCB)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 145,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //79  1   XML1 MA_PTTT_QT  Mã phẫu thuật thủ thuật phải có gồm các chuỗi cách nhau bởi dấu ';' và nhỏ hơn 255 ký tự
        //    if (xml1.MA_PTTT_QT?.Length > 255)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 79,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //146 1   XML1 MA_TAI_NAN  Mã tai nạn phải có giá trị từ 0 đến 8 hoặc để trống
        //    if (xml1.MA_TAI_NAN != null && (xml1.MA_TAI_NAN < 0 || xml1.MA_TAI_NAN > 8))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 146,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //50  1   XML1 KET_QUA_DTRI    Kết quả điểu trị phải có giá trị từ 1 đến 5 hoặc để trống
        //    if (xml1.KET_QUA_DTRI != null && (xml1.KET_QUA_DTRI < 0 || xml1.KET_QUA_DTRI > 5))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 50,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //51  1   XML1 TINH_TRANG_RV   Tình trạng ra viện phải có giá trị từ 1 đến 4 hoặc để trống
        //    //476 1   XML1 TINH_TRANG_RV   Bệnh nhân chuyển viện không có Biên bản hội chẩn
        //    if (xml1.TINH_TRANG_RV != null && (xml1.TINH_TRANG_RV < 1 && xml1.TINH_TRANG_RV > 4))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 51,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    //Nếu XML 1 Tình trạng ra viện =2 (chuyển viện) =>XML 5 bắt buộc phải có hội chẩn, không có báo lỗi
        //    //Nội dung hội chẩn phải có "chuyển viện", "Chuyển BV" hoặc "Chuyển Bệnh viện" hoặc "chuyển"
        //    //Linhist changed 30/05/22 kiem tra lai theo dk LOAI_KCB= 3 & TINH_TRANG_RV = 2
        //    //else if (Uti.NumExists(xml1.TINH_TRANG_RV, 2, 3))
        //    else if (xml1.MA_LOAI_KCB != null && xml1.MA_LOAI_KCB == 3 && xml1.TINH_TRANG_RV == 2)
        //    {
        //        if (!xml1.XML5s.Any(x => x.HOI_CHAN != null && Uti.StrExists(x.HOI_CHAN, true, "chuyển viện", "Chuyển BV", "Chuyển Bệnh viện", "chuyển")))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 476,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    return allMaBenhs;
        //}

        //const string strBenh401013 = "E48.*;K00.*;K01.*;K02.*;K03.*;K04.*;K05.*;K06.*;K07.*;K08.*;K09.*;K10.*;K11.*;K12.*;K13.*;K14.*;K15.*;K16.*;K17.*;K18.*;K19.*;K20.*;K21.*;K22.*;K23.*;K24.*;K25.*;K26.*;K27.*;K28.*;K29.*;K30.*;K31.*;K32.*;K33.*;K34.*;K35.*;K36.*;K37.*;K38.*;K39.*;K40.*;K41.*;K42.*;K43.*;K44.*;K45.*;K46.*;K47.*;K48.*;K49.*;K50.*;K51.*;K52.*;K53.*;K54.*;K55.*;K56.*;K57.*;K58.*;K59.*;K60.*;K61.*;K62.*;K63.*;K64.*;K65.*;K66.*;K67.*;K68.*;K69.*;K70.*;K71.*;K72.*;K73.*;K74.*;K75.*;K76.*;K77.*;K78.*;K79.*;K80.*;K81.*;K82.*;K83.*;K84.*;K85.*;K86.*;K87.*;K88.*;K89.*;K90.*;K91.*;K92.*;K93.*;K94.*;K95.*;K96.*;K97.*;K98.*;K99.*;C00.*;C01.*;C02.*;C03.*;C04.*;C05.*;C06.*;C07.*;C08.*;C09.*;C10.*;C11.*;C12.*;C13.*;C14.*;C15.*;C16.*;C17.*;C18.*;C19.*;C20.*;C21.*;C22.*;C23.*;C24.*;C25.*;C26.*;C27.*;C28.*;C29.*;C30.*;C31.*;C32.*;C33.*;C34.*;C35.*;C36.*;C37.*;C38.*;C39.*;C40.*;C41.*;C42.*;C43.*;C44.*;C45.*;C46.*;C47.*;C48.*;C49.*;C50.*;C51.*;C52.*;C53.*;C54.*;C55.*;C56.*;C57.*;C58.*;C59.*;C60.*;C61.*;C62.*;C63.*;C64.*;C65.*;C66.*;C67.*;C68.*;C69.*;C70.*;C71.*;C72.*;C73.*;C74.*;C75.*;C76.*;C77.*;C78.*;C79.*;C80.*;C81.*;C82.*;C83.*;C84.*;C85.*;C86.*;C87.*;C88.*;C89.*;C90.*;C91.*;C92.*;C93.*;C94.*;C95.*;C96.*;C97.*";
        
        ////Linhist changed 08/08/23
        //private void Verify_DacBiet_XML2(List<XML1Error> errors, XML2 xml2, Dictionary<string, string> dmDacbiet)// string thuocvuottuyen)
        //{
        //    //392    1   XML2 MA_THUOC    Chỉ định Acid amin +glucose + lipid(*) sai với hướng dẫn trong TT30/ 2018 và TT01/ 2020
        //    //Mã thuốc  chỉ được phép chỉ định cho các bệnh có mã  "E48.*;........"
        //    //hoặc các bệnh nhân trong khoa Cấp cứu(K02, K48) hoặc các khoa ghép của K0248, K4802) Chỉ thanh toán 50 %
        //    if (xml2.MA_THUOC == "40.1013")
        //    {
        //        if ((!Uti.CodeMatch(xml2.MA_BENH, strBenh401013) && !Uti.CodeMatch(xml2.MA_KHOA, "K02;K48;K0248*;K4802*")) || xml2.TYLE_TT > 50)
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 392,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }

        //    string thuocVT = dmDacbiet.ContainsKey("thuocvuottuyen") ? dmDacbiet["thuocvuottuyen"] : "";
        //    //465 1   XML2 MA_THUOC    Chỉ định thuốc vượt tuyến thiếu biên bản hội chẩn
        //    if (xml2.NGAY_YL != null && Uti.CodeContain(xml2.MA_THUOC, thuocVT))
        //    {
        //        var allHoiChans = xml2.XML1?.XML5s.Where(x => x.NGAY_YL != null && x.NGAY_YL <= xml2.NGAY_YL);
        //        if (!allHoiChans.Any(x => Uti.StrExists(x.HOI_CHAN, true, "Tuyến", "Vượt Tuyến", "Invanz", "Nimedine", "Tiepanem", "Colistimetato", "Fosmicin", "Linezolid", "Kidmin", "Amiparen", "MG - Tan", "Combilipid", "Doripenem", "Ertapenem", "Impienem", "Meropenem", "Colistin", "Fosfomycin", "Linezolid", "Teicoplanin", "Amphotericin", "Acid amin", "Hội chẩn thuốc", "Dùng thuốc", "Thuốc")))
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 465,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }
        //}

        ////Linhist changed 08/08/23
        //private void Verify_DacBiet_XML3(List<XML1Error> errors, List<XML3> lstPhauThuat, List<XML3> lstDienTim, List<XML3> lstSieuAm, XML3 xml3, Dictionary<string, string> dmDacBiet)//, string ctrmi, string xetnghiem, string phauthuat, string thuthuat)
        //{
        //    //Linhist added 13/04/22
        //    //889 1   XML3 MA_DICH_VU  Tạo thông Động tĩnh mạch áp sai giá theo CV 1044/BYT
        //    if (xml3.MA_DICH_VU == "01.0175.0196")
        //    {
        //        if (xml3.XML1.XML3s.Any(x => Uti.CodeContain(x.MA_DICH_VU, "10.0260.0399;10.0260.0399_G")))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 889,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //        return;
        //    }

        //    //Linhist added 17/03/22
        //    //Kiem tra MA_DICH_VU lien quan den sieu am tim & dien tim
        //    if (xml3.NGAY_YL != null && xml3.NGAY_YL.HasValue)
        //    {
        //        if (Uti.CodeContain(xml3.MA_DICH_VU, "02.0085.1778;21.0014.1778"))
        //        {
        //            lstDienTim.Add(xml3);
        //            return;
        //        }

        //        if (Uti.CodeContain(xml3.MA_DICH_VU, "01.0018.0004;18.0052.0004;02.0113.0004;02.0458.0006"))
        //        {
        //            lstSieuAm.Add(xml3);
        //            return;
        //        }
        //    }

        //    //356     1   XML3 NGAY_KQ     Bệnh nhân chưa thực hiện xét nghiệm(Không có kết quả xét nghiệm tại XML4)
        //    //Neu MA_NHOM = 1 kiem tra XML4.GIA_TRI == null || "" --> Bao loi
        //    //haimaster update 2.3.2023
        //    if (Uti.NumExists(xml3.MA_NHOM, 1))
        //    {
        //        //Linhist changed 23/03/23
        //        //if (!xml3.XML1.XML4s.Any(x => x.MA_DICH_VU == xml3.MA_DICH_VU && !string.IsNullOrEmpty(x.GIA_TRI)))
        //        if (!xml3.XML1.XML4s.Any(x => x.MA_DICH_VU == xml3.MA_DICH_VU && (!string.IsNullOrEmpty(x.GIA_TRI) || !(string.IsNullOrEmpty(x.MO_TA) && string.IsNullOrEmpty(x.KET_LUAN)))))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 356,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //    }
        //    //Neu MA_NHOM = 2,3 kiem tra (XML4.MO_TA || XML4.KET_LUAN) == null || "" --> Bao loi
        //    else if (Uti.NumExists(xml3.MA_NHOM, 2, 3))
        //    {
        //        //if (!xml3.XML1.XML4s.Any(x => x.MA_DICH_VU.Contains(xml3.MA_DICH_VU) && !(string.IsNullOrEmpty(x.MA_MAY) && string.IsNullOrEmpty(x.MO_TA) && string.IsNullOrEmpty(x.KET_LUAN))))
        //        if (!xml3.XML1.XML4s.Any(x => x.MA_DICH_VU == xml3.MA_DICH_VU && (!string.IsNullOrEmpty(x.GIA_TRI) || (x.MO_TA ?? "").Length >= 7 || (x.KET_LUAN ?? "").Length >= 7)))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 356,
        //                XEErrorRow = xml3.STT
        //            });
        //        }
        //    }

        //    string ctrmi = dmDacBiet.ContainsKey("ctrmi") ? dmDacBiet["ctrmi"] : "";
        //    //461    1   XML5 HOI_CHAN    Chụp CT, MRI thiếu Biên bản Hội chẩn
        //    //464     1   XML3 MA_DICH_VU  Chụp CT, MRI thiếu biên bản Hội chẩn trong hồ sơ XML đề nghị thanh toán
        //    if (Uti.CodeMatch(xml3.MA_DICH_VU, ctrmi))
        //    {
        //        //haimaster 06/06/2022 update them dau bang
        //        var xml5TruocNgay = xml3.XML1.XML5s.Where(x => x.NGAY_YL <= xml3.NGAY_YL);
        //        if (!xml5TruocNgay.Any(x => Uti.StrExists(x.HOI_CHAN, true, "Chụp", "Chụp", "CT", "MRI")))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = 464,
        //                XEErrorRow = xml3.STT
        //            });

        //            foreach (var m in xml5TruocNgay)
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = 461,
        //                    XEErrorRow = m.STT
        //                });
        //            }
        //        }
        //        //return;
        //    }
            
        //    //Linhist changed 23/12/22
        //    //bool isPhauThuat = Uti.CodeMatch(xml3.MA_DICH_VU, phauthuat);
        //    //bool isThuThuat = Uti.CodeMatch(xml3.MA_DICH_VU, thuthuat);
        //    bool isPhauThuat = (xml3.MA_NHOM == 8);
        //    bool isThuThuat = (xml3.MA_NHOM == 18);

        //    //435 	1 	XML5 	PHAU_THUAT 	Không có tường trình phẫu thuật, thủ thuật trong hồ sơ XML đề nghị thanh toán
        //    //463 	1 	XML3 	MA_DICH_VU 	Không có Biên bản tường trình cho Bệnh nhân Phẫu Thuật
        //    //477 	1 	XML3 	MA_DICH_VU 	Không có Biên bản tường trình cho bệnh nhân thực hiện thủ thật
        //    if (isPhauThuat || isThuThuat)
        //    {
        //        var xml5CungNgay = xml3.XML1.XML5s.Where(x => x.NGAY_YL == xml3.NGAY_YL);
        //        if (!xml5CungNgay.Any(x => !string.IsNullOrEmpty(x.PHAU_THUAT)))
        //        {
        //            xml3.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml3.XML1ID,
        //                XEErrorCode = isPhauThuat ? 463 : 477,
        //                XEErrorRow = xml3.STT
        //            });

        //            if (isThuThuat && !Uti.CodeContain(xml3.MA_KHOA, "K16;K31") && !Uti.CodeContain(xml3.MA_DICH_VU, "02.0264.0140;02.0265.0140;02.0266.0157;02.0267.0140;02.0269.0318;02.0271.0140;02.0272.0134;02.0272.2044;02.0276.0140;02.0277.0502;02.0282.0318;02.0285.0140;02.0286.0497;02.0290.0500;02.0295.0498;02.0296.0500;02.0298.0140"))
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = 923,
        //                    XEErrorRow = xml3.STT
        //                });
        //            }

        //            if (isPhauThuat && !Uti.CodeContain(xml3.MA_DICH_VU, "03.1944.1016;03.1944.1017;16.0044.1012;16.0044.1013;16.0044.1014;16.0044.1015;16.0045.1012;16.0045.1013;16.0045.1014;16.0045.1015;16.0046.1012;16.0046.1013;16.0046.1014;16.0046.1015;16.0047.1012;16.0047.1013;16.0047.1014;16.0047.1015;16.0050.1012;16.0050.1013;16.0050.1014;16.0050.1015;16.0051.1012;16.0051.1013;16.0051.1014;16.0051.1015;16.0052.1012;16.0052.1013;16.0052.1014;16.0052.1015;16.0053.1012;16.0053.1013;16.0053.1014;16.0053.1015;16.0054.1012;16.0054.1013;16.0054.1014;16.0054.1015;16.0055.1012;16.0055.1013;16.0055.1014;16.0055.1015;16.0232.1016;16.0232.1017"))
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = 924,
        //                    XEErrorRow = xml3.STT
        //                });
        //            }

        //            foreach (var m in xml5CungNgay)
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = 435,
        //                    XEErrorRow = m.STT
        //                });
        //            }
        //        }
        //    }

        //    //462 	1 	XML3 	MA_DICH_VU 	Dịch vụ phẫu thuật chưa có biên bản hội chẩn trong hồ sơ XML đề nghị thanh toán
        //    if (isPhauThuat)
        //    {
        //        //Linhist added 13/04/22 kiem tra dich vu phau thuat tinh tien 2 lan
        //        if (xml3.TYLE_TT != null && xml3.TYLE_TT == 100 && xml3.NGAY_YL != null && xml3.NGAY_KQ != null && (xml3.NGAY_KQ.Value - xml3.NGAY_YL.Value).TotalMinutes <= 30d)
        //        {
        //            lstPhauThuat.Add(xml3);
        //        }
        //        if (!Uti.CodeMatch(xml3.XML1.MA_CSKCB.ToString().Trim(), "40544") && !Uti.CodeMatch(xml3.MA_KHOA.ToString().Trim(), "K01"))
        //        {

        //            if (!xml3.XML1.XML5s.Any(x => x.NGAY_YL <= xml3.NGAY_YL && Uti.StrExists(x.HOI_CHAN, true, "Phẫu Thuật", "Phương pháp vô cảm:", "Kết luận", "Mổ cấp cứu", "Cắt", "Cắt", "Mổ")))
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = 462,
        //                    XEErrorRow = xml3.STT
        //                });

        //                if (!Uti.CodeContain(xml3.MA_DICH_VU, "03.1944.1016;03.1944.1017;16.0044.1012;16.0044.1013;16.0044.1014;16.0044.1015;16.0045.1012;16.0045.1013;16.0045.1014;16.0045.1015;16.0046.1012;16.0046.1013;16.0046.1014;16.0046.1015;16.0047.1012;16.0047.1013;16.0047.1014;16.0047.1015;16.0050.1012;16.0050.1013;16.0050.1014;16.0050.1015;16.0051.1012;16.0051.1013;16.0051.1014;16.0051.1015;16.0052.1012;16.0052.1013;16.0052.1014;16.0052.1015;16.0053.1012;16.0053.1013;16.0053.1014;16.0053.1015;16.0054.1012;16.0054.1013;16.0054.1014;16.0054.1015;16.0055.1012;16.0055.1013;16.0055.1014;16.0055.1015;16.0232.1016;16.0232.1017"))
        //                {
        //                    xml3.XML1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml3.XML1ID,
        //                        XEErrorCode = 925,
        //                        XEErrorRow = xml3.STT
        //                    });
        //                }
        //            }
        //        }
        //    }
        //}

        ////Linhist removed 25/04/22 thay the bang function Verify_TTNgayXml1
        ////private void Verify_DacBiet_NgayGiuong(List<XML1Error> errors, XML1 xml1)
        ////{
        ////    //150 1   XML1 SO_NGAY_DTRI    Số ngày điều trị không khớp với công thưc tính của TT37 và CV bổ sung 7117
        ////    //238 1   XML1 SO_NGAY_DTRI    Số ngày điều trị không đúng(ngày ra -ngày vào + 1) ---> bỏ lỗi này thừa
        ////    double tongGio = Math.Round((xml1.NGAY_RA.Value - xml1.NGAY_VAO.Value).TotalHours, 1);
        ////    int tongNgay = (int)(xml1.NGAY_RA.Value.Date - xml1.NGAY_VAO.Value.Date).TotalDays;

        ////    int ngayDTri = 0;
        ////    if (tongGio > 4d && tongGio <= 24d)
        ////    {
        ////        ngayDTri = 1;
        ////    }
        ////    else if ((xml1.TINH_TRANG_RV == 2 && xml1.KET_QUA_DTRI > 1 && xml1.KET_QUA_DTRI < 5) || (xml1.TINH_TRANG_RV == 4 && xml1.KET_QUA_DTRI > 2 && xml1.KET_QUA_DTRI < 5))
        ////    {
        ////        ngayDTri = tongNgay + 1;
        ////    }
        ////    else
        ////    {
        ////        ngayDTri = tongNgay;
        ////    }

        ////    if (xml1.SO_NGAY_DTRI != ngayDTri)
        ////    {
        ////        file.XML1Errors.Add(new XML1Error()
        ////        {
        ////            //XEXML1ID =xml1.XML1ID,
        ////            XEErrorCode = 150,
        ////            XEErrorRow = xml1.STT
        ////        });
        ////    }

        ////    //439 1   XML1 SO_NGAY_DTRI    Tổng ngày giường lớn hơn hướng dẫn của BYT(TT39)
        ////    //440 1   XML1 SO_NGAY_DTRI    Tổng ngày giường nhỏ hơn hướng dẫn của BYT(TT39)
        ////    var allGiuongs = xml1.XML3s.Where(x => x.MA_NHOM == 14 || x.MA_NHOM == 15);
        ////    double? sumGiuongs = allGiuongs.Sum(x => x.SO_LUONG);
        ////    int sumNgayGiuong = sumGiuongs == null || !sumGiuongs.HasValue ? 0 : (int)Math.Round(sumGiuongs.Value, 0);
        ////    if (sumNgayGiuong != ngayDTri)
        ////    {
        ////        file.XML1Errors.Add(new XML1Error()
        ////        {
        ////            //XEXML1ID =xml1.XML1ID,
        ////            XEErrorCode = sumNgayGiuong > ngayDTri ? 439 : 440,
        ////            XEErrorRow = xml1.STT
        ////        });
        ////    }
        ////}

        //private void Verify_DacBiet_DienTim(List<XML1Error> errors, List<XML3> xml3DienTims, List<XML3> xml3SieuAmTims)
        //{
        //    //355     1   XML3 MA_DICH_VU  Siêu âm dopler và điện tim thường không được chỉ định đồng thời theo qđ 3983 / 2014 / qđ - byt
        //    //Trong 1 ngày nếu đã chỉ định các mã 02.0085.1778, 21.0014.1778 (Điện tim) trước, sau đó đến các mã 01.0018.0004,18.0052.0004,02.0113.0004,02.0458.0006(Siêu âm tim) thì được,
        //    //ngược lại là sai thông báo lỗi(Nếu đã có chỉ định điện tim trước, siêu âm tim sau, thì các lần điện tim tiếp theo không báo lỗi)
        //    var dienTims = xml3DienTims.OrderBy(x => x.NGAY_YL);

        //    DateTime d = DateTime.MinValue;
        //    foreach (var dt in dienTims)
        //    {
        //        if (d == dt.NGAY_YL.Value.Date) continue;

        //        d = dt.NGAY_YL.Value.Date;
        //        if (xml3SieuAmTims.Any(x => x.NGAY_YL != null && x.NGAY_YL.Value.Date == d && x.NGAY_YL < dt.NGAY_YL))
        //        {
        //            dt.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =dt.XML1.XML1ID,
        //                XEErrorCode = 355,
        //                XEErrorRow = dt.STT
        //            });
        //        }
        //    }
        //}
        //#endregion

        //#region Linhist Added 24/03/22
        //private double Verify_TTBenhNhan(List<XML1Error> errors, XML1 xml1)
        //{
        //    //16  1   XML1 MA_BN   Mã bệnh nhân không được để trống
        //    //17  1   XML1 MA_BN   Mã bệnh nhân không được lớn hơn 50 ký tự
        //    if (string.IsNullOrEmpty(xml1.MA_BN?.Trim()))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 16,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.MA_BN.Length > 50)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 17,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //215 1   XML1 HO_TEN  Thẻ sai họ tên  LOI_THE
        //    //138 1   XML1 HO_TEN  Họ tên không đúng định dạng<![CDATA[...]] >
        //    //18  1   XML1 HO_TEN  Họ tên không được để trống
        //    //19  1   XML1 HO_TEN  Họ tên không được lớn hơn 255 ký tự
        //    if (string.IsNullOrEmpty(xml1.HO_TEN?.Trim()))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 18,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.HO_TEN.Length > 255)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 19,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    double ngayTuoiBN = 0d;
        //    //20  1   XML1 NGAY_SINH   Ngày sinh không được để trống
        //    //21  1   XML1 NGAY_SINH   Ngày sinh không đúng định dạng
        //    //139 1   XML1 NGAY_SINH   Ngày sinh không được lớn hơn thời gian hiện tại
        //    //140 1   XML1 NGAY_SINH   Ngày sinh không được lớn hơn thời gian vào viện
        //    if (xml1.NGAY_SINH == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 20,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.NGAY_SINH.Value > DateTime.Now)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 139,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else
        //    {
        //        //Linhist changed 30/05/22 lay ngay tuoi BN, ko lay tuoi
        //        //tuoiBN = (double)((DateTime.Now - xml1.NGAY_SINH)?.Days / 365.242199);
        //        ngayTuoiBN = (DateTime.Now - xml1.NGAY_SINH.Value).TotalDays;

        //        if (xml1.NGAY_VAO != null && xml1.NGAY_SINH > xml1.NGAY_VAO)
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 140,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    //24  1   XML1 DIA_CHI Địa chỉ không được để trống
        //    //141 1   XML1 DIA_CHI Địa chỉ không đúng định dạng<![CDATA[...]] >
        //    if (string.IsNullOrEmpty(xml1.DIA_CHI?.Trim()))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 24,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.DIA_CHI.Length > 1024)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 141,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //Linhist changed 17/07/23
        //    //80  2   XML1 CAN_NANG    Chỉ thu thập cân nặng với các bệnh nhân là trẻ em dưới 1 tuổi
        //    //if (ngayTuoiBN >= 365.242199d && xml1.CAN_NANG != null && xml1.CAN_NANG != 0)
        //    //{
        //    //    xml1.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml1.XML1ID,
        //    //        XEErrorCode = 80,
        //    //        XEErrorRow = xml1.STT
        //    //    });
        //    //}
        //    if (xml1.CAN_NANG == null || xml1.CAN_NANG == 0)
        //    {
        //        //1606	1	XML1	CAN_NANG	Cân nặng không được để trống khi BN chưa đến 1 tuổi tại thời điểm vào viện
        //        if (ngayTuoiBN < 365.242199d)
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1606,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    else
        //    {
        //        //80  2   XML1 CAN_NANG    Chỉ thu thập cân nặng với các bệnh nhân là trẻ em dưới 1 tuổi
        //        if (ngayTuoiBN >= 365.242199d)
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 80,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    //22  1   XML1 GIOI_TINH   Giới tính không được để trống
        //    //23  1   XML1 GIOI_TINH   Giới tính phải nằm trong khoảng giá trị 1 đến 3
        //    //217 1   XML1 GIOI_TINH   Thẻ sai giới tính   LOI_THE
        //    if (xml1.GIOI_TINH == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 217,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.GIOI_TINH != 1 && xml1.GIOI_TINH != 2 && xml1.GIOI_TINH != 3)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 23,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //78  1   XML1 MA_KHUVUC   Mã khu vực phải có giá trị K1 K2 K3 hoặc để trống
        //    if (!string.IsNullOrEmpty(xml1.MA_KHUVUC) && !Uti.CodeContain(xml1.MA_KHUVUC, "K1;K2;K3"))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 78,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    return ngayTuoiBN;
        //}

        //private string[] Verify_TTTheXml1(List<XML1Error> errors, XML1 xml1, string currCSKCB)
        //{
        //    string[] maThes = null;
        //    //1   1   HOSO Lỗi mở file hồ sơ XML
        //    //11  1   HOSO LOAIHOSO    Lỗi mở file hồ sơ XML
        //    //12  1   HOSO MA_COSO_KCB MACSKCB không khớp với mã KCBBĐ trong cấu hình
        //    //2   1   HOSO MACSKCB MACSKCB không được để trống
        //    //3   1   HOSO MACSKCB MACSKCB không khớp với mã KCBBĐ trong cấu hình

        //    //---Doan nay thua, ko dung den
        //    //if (string.IsNullOrEmpty(xml1.MA_CSKCB?.Trim()))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml1.XML1ID,
        //    //        XEErrorCode = 2,
        //    //        XEErrorRow = xml1.STT
        //    //    });
        //    //}
        //    //else if (xml1.MA_CSKCB != currCSKCB)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml1.XML1ID,
        //    //        XEErrorCode = 12,
        //    //        XEErrorRow = xml1.STT
        //    //    });
        //    //}

        //    //76  1   XML1 MA_CSKCB    Mã MA_CSKCB không được để trống
        //    //77  1   XML1 MA_CSKCB    Mã MA_CSKCB phải có 5 ký tự, không tồn tại khoảng trắng ở giữa
        //    //159 1   XML1 MA_CSKCB    Mã KCB không trùng khớp với mã KCB trong cấu hình
        //    if (string.IsNullOrEmpty(xml1.MA_CSKCB?.Trim()))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 76,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.MA_CSKCB.Length != 5)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 77,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.MA_CSKCB != currCSKCB)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 159,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //25  1   XML1 MA_THE  Mã thẻ không được để trống
        //    //26  1   XML1 MA_THE  Mã thẻ phải có 15 ký tự, không tồn tại khoảng trắng ở giữa
        //    if (string.IsNullOrWhiteSpace(xml1.MA_THE))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 25,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else
        //    {
        //        bool hasErr = false;
        //        List<string> mts = xml1.MA_THE.Split(';').ToList();
        //        for (int i = 0; i < mts.Count; i++)
        //        {
        //            //Linhist added 08/04/22 case MA_THE 10 hoac 15 ky tu
        //            if ((mts[i].Length != 10 && mts[i].Length != 15) || mts[i].Contains(" "))
        //            {
        //                hasErr = true;
        //                mts.RemoveAt(i);
        //                i--;
        //            }
        //        }

        //        if (hasErr || mts.Count < 1)
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 26,
        //                XEErrorRow = xml1.STT
        //            });
        //        }

        //        maThes = mts.ToArray();
        //    }

        //    //4   1   HOSO NGAYLAP Ngày lập không được để trống
        //    //5   1   HOSO NGAYLAP Ngày lập không đúng
        //    //9   1   HOSO NOIDUNGFILE Nội dung file không được để trống
        //    //10  1   HOSO NOIDUNGFILE Lỗi mở file hồ sơ XML
        //    //6   1   HOSO SOLUONGHOSO Số lượng hồ sơ không được để trống
        //    //7   1   HOSO SOLUONGHOSO Số lượng hồ sơ không đúng
        //    //8   1   HOSO SOLUONGHOSO Số lượng hồ sơ không khớp

        //    //---Doan nay thua, ko dung den
        //    //if (xml1.NGAYLAP == null)
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml1.XML1ID,
        //    //        XEErrorCode = 4,
        //    //        XEErrorRow = xml1.STT
        //    //    });
        //    //}

        //    string[] maDKBDs = null;
        //    //27  1   XML1 MA_DKBD Mã đăng ký ban đầu không được để trống
        //    //28  1   XML1 MA_DKBD Mã đăng ký ban đầu phải có 5 ký tự, không tồn tại khoảng trắng ở giữa
        //    if (string.IsNullOrWhiteSpace(xml1.MA_DKBD))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 27,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else
        //    {
        //        maDKBDs = xml1.MA_DKBD.Split(';');
        //        foreach (string mdk in maDKBDs)
        //        {
        //            if (mdk.Length != 5 || mdk.Contains(" "))
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 28,
        //                    XEErrorRow = xml1.STT
        //                });
        //            }
        //        }
        //    }

        //    // Linhist added 31/10/22
        //    // 1240	1	XML3	MA_THE	Sai thông tin thẻ Trẻ em (Sai mã Tỉnh)
        //    if (maThes != null && maThes.Length > 0 && maDKBDs != null && maDKBDs.Length > 0)
        //    {
        //        for (int i = 0; i < maThes.Length; i++)
        //        {
        //            if (Check_TheTE_ChuaDiHoc(maThes[i], xml1.NGAY_SINH))
        //            {
        //                if (maDKBDs.Length <= i || maDKBDs[i][0] != maThes[i][3] || maDKBDs[i][1] != maThes[i][4])
        //                {
        //                    xml1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml1.XML1ID,
        //                        XEErrorCode = 1240,
        //                        XEErrorRow = xml1.STT
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return maThes;
        //}

        //private DateTime[] Verify_TTNgayXml1(List<XML1Error> errors, XML1 xml1, TimePeriod gio_Hanh_Chinh_BV)
        //{
        //    DateTime[] allDates = new DateTime[] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };

        //    //Them kiem tra GT_THE_TU
        //    //29  1   XML1 GT_THE_TU   Giá trị thẻ không được để trống
        //    //30  1   XML1 GT_THE_TU   Giá trị thẻ từ phải bao gồm 4 ký tự số từ 1000 đến 2999 + 2 ký tự số từ 01 - 12 + 2 ký tự số từ 01 - 31
        //    //31  1   XML1 GT_THE_TU   Giá trị thẻ từ không tồn tại thời gian này
        //    //32  1   XML1 GT_THE_TU   Giá trị thẻ từ không được nhỏ hơn ngày tháng năm sinh
        //    //142 1   XML1 GT_THE_TU   Giá trị thẻ từ không được lớn hơn thời gian hiện tại
        //    if (string.IsNullOrEmpty(xml1.GT_THE_TU))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 29,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else
        //    {
        //        int idx = 0;
        //        string[] ngayThangs = xml1.GT_THE_TU.Split(';');
        //        foreach (string nt in ngayThangs)
        //        {
        //            if (nt.Length != 8)
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 30,
        //                    XEErrorRow = xml1.STT
        //                });
        //                break;
        //            }

        //            int yyyy = int.Parse(nt.Substring(0, 4));
        //            int mm = int.Parse(nt.Substring(4, 2));
        //            int dd = int.Parse(nt.Substring(6, 2));

        //            if (yyyy < 1000 || yyyy > 2999 || mm < 1 || mm > 12 || dd < 1 || dd > 31)
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 30,
        //                    XEErrorRow = xml1.STT
        //                });
        //                break;
        //            }

        //            try
        //            {
        //                allDates[idx] = new DateTime(yyyy, mm, dd);

        //                if (allDates[idx] > DateTime.Now)
        //                {
        //                    xml1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml1.XML1ID,
        //                        XEErrorCode = 142,
        //                        XEErrorRow = xml1.STT
        //                    });
        //                    break;
        //                }
        //                else if (xml1.NGAY_SINH != null && allDates[idx] < xml1.NGAY_SINH.Value)
        //                {
        //                    xml1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml1.XML1ID,
        //                        XEErrorCode = 32,
        //                        XEErrorRow = xml1.STT
        //                    });
        //                    break;
        //                }

        //                idx++;
        //            }
        //            catch
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 31,
        //                    XEErrorRow = xml1.STT
        //                });
        //                break;
        //            }
        //        }
        //    }

        //    //33  1   XML1 GT_THE_DEN  Giá trị thẻ đến không được để trống
        //    //34  1   XML1 GT_THE_DEN  Giá trị thẻ đến phải bao gồm 4 ký tự số từ 1000 đến 2999 + 2 ký tự số từ 01 - 12 + 2 ký tự số từ 01 - 31
        //    //35  1   XML1 GT_THE_DEN  Giá trị thẻ đến không tồn tại thời gian này
        //    //143 1   XML1 GT_THE_DEN  Giá trị thẻ đến không được nhỏ hơn giá trị thẻ từ
        //    if (string.IsNullOrEmpty(xml1.GT_THE_DEN))
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 33,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else
        //    {
        //        int idx = 2;
        //        string[] ngayThangs = xml1.GT_THE_DEN.Split(';');
        //        foreach (string nt in ngayThangs)
        //        {
        //            if (nt.Length != 8)
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 34,
        //                    XEErrorRow = xml1.STT
        //                });
        //                break;
        //            }

        //            int yyyy = int.Parse(nt.Substring(0, 4));
        //            int mm = int.Parse(nt.Substring(4, 2));
        //            int dd = int.Parse(nt.Substring(6, 2));

        //            if (yyyy < 1000 || yyyy > 2999 || mm < 1 || mm > 12 || dd < 1 || dd > 31)
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 34,
        //                    XEErrorRow = xml1.STT
        //                });
        //                break;
        //            }

        //            try
        //            {
        //                allDates[idx] = new DateTime(yyyy, mm, dd);

        //                if (allDates[idx] < allDates[idx - 2])
        //                {
        //                    xml1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml1.XML1ID,
        //                        XEErrorCode = 143,
        //                        XEErrorRow = xml1.STT
        //                    });
        //                    break;
        //                }

        //                idx++;
        //            }
        //            catch
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 35,
        //                    XEErrorRow = xml1.STT
        //                });
        //                break;
        //            }
        //        }
        //    }

        //    bool ngay_RAVAO_Correct = true;
        //    //147 1   XML1 NGAY_VAO    Ngày vào không được lớn hơn thời gian hiện tại
        //    //43  1   XML1 NGAY_VAO    Ngày vào không được để trống
        //    //44  1   XML1 NGAY_VAO    Ngày vào phải bao gồm 4 ký tự số từ 1000 đến 2999 + 2 ký tự số từ 01 - 12 + 2 ký tự số từ 01 - 31
        //    //45  1   XML1 NGAY_VAO    Ngày vào không tồn tại thời gian này
        //    if (xml1.NGAY_VAO == null)
        //    {
        //        ngay_RAVAO_Correct = false;
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 46,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.NGAY_VAO > DateTime.Now)
        //    {
        //        ngay_RAVAO_Correct = false;
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 147,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    //Linhist added 12/04/22 lay ngay vao vien de check MA_NHOM_5937
        //    else
        //    {
        //        //Linhist added 08/12/22
        //        //1299	1	XML1	NGAY_VAO	Bệnh nhân không cấp cứu vào Viện ngoài giờ hành chính
        //        //Giờ hành chính phải theo từng Viện
        //        // Giờ có giờ mùa đông và giờ mùa hè, Giờ mùa hè từ 16 / 4 đến 15 / 10 mùa đông từ  16 / 10 đến 15 / 4 năm sau.
        //        //Giờ của Lào Cai, Bac Ninh Mùa Đông: 7h đến 12h chiều từ 13h đến 16h30
        //        //Giờ của Lòa Cai, Bac ninh Mùa Hè: 6h30 đến 11h30, chiều từ 13h30 đến 17h

        //        //Hai Phong gio mua He 1.5 den 30.9 mua dong tu 1.10 den 30.4
        //        //buoi sang mua he 7h den 11h30
        //        //buoi chieu mua he 13h30 den 17h
        //        //buoi sang mua dong la 7h30 den 12h
        //        //buoi chieu mua he la 13h30 den 17h


                


        //        //TimePeriod per3 = new TimePeriod("[7:00-12:00;13:00-16:30][6:30-11:30;13:30-17:00]16/04-15/10");

        //        //TimePeriod per2 = new TimePeriod("[7:30-12:00;13:30-17:00][7:00-11:30-13:00-16:30]01/05-30/09");

        //        //TimePeriod per2 = new TimePeriod("[7:00-12:00-13:00-16:30][6:30-11:30;13:30-17:00]01/05-30/09");

        //        if (Uti.NumExists(xml1.MA_LOAI_KCB, 1, 2, 3) && xml1.MA_KHOA != "K35")
        //        {
        //            if (xml1.MA_LYDO_VVIEN != 2)
        //            {
        //                DateTime ngayVao = xml1.NGAY_VAO.Value;
        //                //haimaster update
        //                //10061 DK Tinh Lao Cai

        //                //Linhist changed 05/04/23
        //                //if (Uti.NumExists(ngayVao.Month, 1, 2, 3, 11, 12))
        //                //{
        //                //    if (ngayVao.Hour < 7 || ngayVao.Hour > 17)
        //                //    {
        //                //        xml1.XML1Errors.Add(new XML1Error()
        //                //        {
        //                //            //XEXML1ID =xml1.XML1ID,
        //                //            XEErrorCode = 1299,
        //                //            XEErrorRow = 1
        //                //        });
        //                //    }
        //                //}


        //                //if (Uti.NumExists(ngayVao.Month, 1, 2, 3, 11, 12))
        //                //{
        //                //    ngayVao = ngayVao.AddMinutes(-30);
        //                //}
        //                //if (ngayVao.Hour < 7 || ngayVao.Hour > 17)
        //                //{
        //                //    xml1.XML1Errors.Add(new XML1Error()
        //                //    {
        //                //        //XEXML1ID =xml1.XML1ID,
        //                //        XEErrorCode = 1299,
        //                //        XEErrorRow = 1
        //                //    });
        //                //}

        //                if (gio_Hanh_Chinh_BV.IsValid && !gio_Hanh_Chinh_BV.InRange(xml1.NGAY_VAO))
        //                {
        //                    xml1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =file.XML1ID,
        //                        XEErrorCode = 1299,
        //                        XEErrorRow = xml1.STT
        //                    });
        //                }
        //            }

        //        }

        //        allDates[4] = xml1.NGAY_VAO.Value;
        //    }

        //    //46  1   XML1 NGAY_RA Ngày ra không được để trống
        //    //47  1   XML1 NGAY_RA Ngày ra  phải bao gồm 4 ký tự số từ 1000 đến 2999 + 2 ký tự số từ 01 - 12 + 2 ký tự số từ 01 - 31
        //    //48  1   XML1 NGAY_RA Ngày ra không tồn tại thời gian này
        //    //148 1   XML1 NGAY_RA Ngày ra không lớn hơn thời gian hiện tại
        //    //149 1   XML1 NGAY_RA Ngày ra không được nhở hơn thời gian vào
        //    //739 1   XML1 NGAY_RA Thời gian khám chữa bệnh quá nhanh
        //    if (xml1.NGAY_RA == null)
        //    {
        //        ngay_RAVAO_Correct = false;
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 46,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.NGAY_RA > DateTime.Now)
        //    {
        //        ngay_RAVAO_Correct = false;
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 148,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (ngay_RAVAO_Correct)
        //    {
        //        if (xml1.NGAY_RA < xml1.NGAY_VAO)
        //        {
        //            ngay_RAVAO_Correct = false;
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 149,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //        else if ((xml1.NGAY_RA - xml1.NGAY_VAO)?.TotalMinutes < 8d)
        //        {
        //            ngay_RAVAO_Correct = false;
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 739,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    //52  1   XML1 NGAY_TTOAN  Ngày thanh toán không được để trống
        //    //53  1   XML1 NGAY_TTOAN  Ngày thanh toán phải bao gồm 4 ký tự số từ 1000 đến 2999 + 2 ký tự số từ 01 - 12 + 2 ký tự số từ 01 - 31
        //    //54  1   XML1 NGAY_TTOAN  Ngày thanh toán không tồn tại thời gian này
        //    //152 1   XML1 NGAY_TTOAN  Ngày thanh toán không được nhỏ hơn ngày ra viện
        //    //153 1   XML1 NGAY_TTOAN  Ngày thanh toán không được lớn hơn thời gian hiện tại
        //    if (xml1.NGAY_TTOAN == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 52,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.NGAY_TTOAN > DateTime.Now)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 153,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (ngay_RAVAO_Correct && xml1.NGAY_TTOAN < xml1.NGAY_RA)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 152,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //69  1   XML1 NAM_QT  Năm quyết toán không được để trống
        //    //70  1   XML1 NAM_QT  Năm quyết toán không đúng định dạng
        //    if (xml1.NAM_QT == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 69,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.NAM_QT < 1000 || xml1.NAM_QT > 3000)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 70,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    //71  1   XML1 THANG_QT    Tháng quyết toán không được để trống
        //    //72  1   XML1 THANG_QT    Tháng quyết toán không đúng định dạng
        //    if (xml1.THANG_QT == null)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 71,
        //            XEErrorRow = xml1.STT
        //        });
        //    }
        //    else if (xml1.THANG_QT < 1 || xml1.THANG_QT > 12)
        //    {
        //        xml1.XML1Errors.Add(new XML1Error()
        //        {
        //            //XEXML1ID =xml1.XML1ID,
        //            XEErrorCode = 72,
        //            XEErrorRow = xml1.STT
        //        });
        //    }

        //    #region Linhst removed 10/05/22
        //    ////49  1   XML1 SO_NGAY_DTRI    Số ngày điều trị phải có giá trị từ 0 đến 999 hoặc để trống
        //    ////150 1   XML1 SO_NGAY_DTRI    Số ngày điều trị không khớp với công thưc tính của TT37 và CV bổ sung 7117
        //    ////151 1   XML1 SO_NGAY_DTRI    Tổng Số ngày điều trị trong file XML1 không khớp với tổng số ngày giường trong file XML3
        //    ////238 1   XML1 SO_NGAY_DTRI    Số ngày điều trị không đúng(ngày ra - ngày vào + 1)
        //    //if (xml1.SO_NGAY_DTRI != null && (xml1.SO_NGAY_DTRI < 0 || xml1.SO_NGAY_DTRI > 999))
        //    //{
        //    //    file.XML1Errors.Add(new XML1Error()
        //    //    {
        //    //        //XEXML1ID =xml1.XML1ID,
        //    //        XEErrorCode = 49,
        //    //        XEErrorRow = xml1.STT
        //    //    });
        //    //}

        //    //if (ngay_RAVAO_Correct)
        //    //{
        //    //    double tongGioDtri = (xml1.NGAY_RA.Value - xml1.NGAY_VAO.Value).TotalHours;
        //    //    int tongNgayDtri = (int)(xml1.NGAY_RA.Value.Date - xml1.NGAY_VAO.Value.Date).TotalDays;

        //    //    if (xml1.SO_NGAY_DTRI != null && xml1.MA_LOAI_KCB == null)
        //    //    {
        //    //        //Linhist changed 06 / 05 / 22
        //    //        //int tongNgayDTri = (int)(xml1.NGAY_RA.Value.Date - xml1.NGAY_VAO.Value.Date).TotalDays + 1;
        //    //        //if (xml1.SO_NGAY_DTRI.Value != tongNgayDTri)
        //    //        //{
        //    //        //    file.XML1Errors.Add(new XML1Error()
        //    //        //    {
        //    //        //        //XEXML1ID =xml1.XML1ID,
        //    //        //        XEErrorCode = 238,
        //    //        //        XEErrorRow = xml1.STT
        //    //        //    });
        //    //        //}
        //    //        if (xml1.MA_LOAI_KCB == 3)
        //    //        {
        //    //            tongNgayDtri = (tongGioDtri < 8d ? 1 : tongNgayDtri + 1);
        //    //        }
        //    //        else if (xml1.MA_LOAI_KCB != 2)
        //    //        {
        //    //            tongNgayDtri = 0;
        //    //        }

        //    //        if (xml1.SO_NGAY_DTRI.Value != tongNgayDtri)
        //    //        {
        //    //            file.XML1Errors.Add(new XML1Error()
        //    //            {
        //    //                //XEXML1ID =xml1.XML1ID,
        //    //                XEErrorCode = 238,
        //    //                XEErrorRow = xml1.STT
        //    //            });
        //    //        }
        //    //    }

        //    //    //439 1   XML3   SO_LUONG    Tổng ngày giường lớn hơn hướng dẫn của BYT(TT39)
        //    //    //440 1   XML3   SO_LUONG    Tổng ngày giường nhỏ hơn hướng dẫn của BYT(TT39)
        //    //    //884 1   XML3   SO_LUONG    Không thanh toán tiền giường điều trị nội trú của BN nằm viện < 4h
        //    //    if (xml1.MA_LOAI_KCB == 3)
        //    //    {
        //    //        int ngayGiuong = 0;
        //    //        if (tongGioDtri >= 4d && tongGioDtri <= 24d)
        //    //        {
        //    //            ngayGiuong = 1;
        //    //        }
        //    //        else if ((xml1.TINH_TRANG_RV == 2 && xml1.KET_QUA_DTRI > 1 && xml1.KET_QUA_DTRI < 5) || (xml1.TINH_TRANG_RV == 4 && xml1.KET_QUA_DTRI > 2 && xml1.KET_QUA_DTRI < 5))
        //    //        {
        //    //            ngayGiuong = tongNgayDtri + 1;
        //    //        }
        //    //        else
        //    //        {
        //    //            ngayGiuong = tongNgayDtri;
        //    //        }

        //    //        //if (xml1.SO_NGAY_DTRI != ngayGiuong)
        //    //        //{
        //    //        //    file.XML1Errors.Add(new XML1Error()
        //    //        //    {
        //    //        //        //XEXML1ID =xml1.XML1ID,
        //    //        //        XEErrorCode = 150,
        //    //        //        XEErrorRow = xml1.STT
        //    //        //    });
        //    //        //}

        //    //        var allGiuongs = xml1.XML3s.Where(x => x.MA_NHOM == 14 || x.MA_NHOM == 15);
        //    //        //Linhist changed 06/05/22
        //    //        double? sumGiuongs = allGiuongs.Sum(x => x.SO_LUONG);
        //    //        int sumNgayGiuong = sumGiuongs == null || !sumGiuongs.HasValue ? 0 : (int)Math.Round(sumGiuongs.Value, 0);
        //    //        if (sumNgayGiuong != ngayGiuong)
        //    //        {
        //    //            foreach (var xml3 in allGiuongs)
        //    //            {
        //    //                file.XML1Errors.Add(new XML1Error()
        //    //                {
        //    //                    //XEXML1ID =xml1.XML1ID,
        //    //                    XEErrorCode = sumNgayGiuong > ngayGiuong ? 439 : 440,
        //    //                    XEErrorRow = xml3.STT
        //    //                });
        //    //            }
        //    //        }

        //    //        //Removed vi thua, chi can kiem tra theo sum()
        //    //        //if (tongGioDtri < 4d)
        //    //        //{
        //    //        //    foreach (var xml3 in allGiuongs)
        //    //        //    {
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 884,
        //    //        //            XEErrorRow = xml3.STT
        //    //        //        });
        //    //        //    }
        //    //        //}
        //    //        //else
        //    //        //{
        //    //        //    double? sumGiuongs = allGiuongs.Sum(x => x.SO_LUONG);
        //    //        //    int sumNgayGiuong = sumGiuongs == null || !sumGiuongs.HasValue ? 0 : (int)Math.Round(sumGiuongs.Value, 0);
        //    //        //    if (sumNgayGiuong != ngayGiuong)
        //    //        //    {
        //    //        //        foreach (var xml3 in allGiuongs)
        //    //        //        {
        //    //        //            file.XML1Errors.Add(new XML1Error()
        //    //        //            {
        //    //        //                //XEXML1ID =xml1.XML1ID,
        //    //        //                XEErrorCode = sumNgayGiuong > ngayGiuong ? 439 : 440,
        //    //        //                XEErrorRow = xml3.STT
        //    //        //            });
        //    //        //        }
        //    //        //    }
        //    //        //}
        //    //    }
        //    //}
        //    #endregion

        //    #region Linhist added 10/05/22
        //    double tongGioDtri = ngay_RAVAO_Correct ? (xml1.NGAY_RA.Value - xml1.NGAY_VAO.Value).TotalHours : 0d;
        //    int tongNgayDtri = ngay_RAVAO_Correct ? (int)(xml1.NGAY_RA.Value.Date - xml1.NGAY_VAO.Value.Date).TotalDays : 0;

        //    //49  1   XML1 SO_NGAY_DTRI    Số ngày điều trị phải có giá trị từ 0 đến 999 hoặc để trống
        //    //150 1   XML1 SO_NGAY_DTRI    Số ngày điều trị không khớp với công thưc tính của TT37 và CV bổ sung 7117
        //    //151 1   XML1 SO_NGAY_DTRI    Tổng Số ngày điều trị trong file XML1 không khớp với tổng số ngày giường trong file XML3
        //    //238 1   XML1 SO_NGAY_DTRI    Số ngày điều trị không đúng (ngày ra - ngày vào + 1)
        //    if (xml1.SO_NGAY_DTRI != null)
        //    {
        //        if (xml1.SO_NGAY_DTRI < 0 || xml1.SO_NGAY_DTRI > 999)
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 49,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //        else if (ngay_RAVAO_Correct)
        //        {
        //            int ngayDtri = tongNgayDtri;

        //            if (xml1.MA_LOAI_KCB == 3)
        //            {
        //                ngayDtri = (tongGioDtri < 8d ? 1 : tongNgayDtri + 1);
        //            }
        //            else if (xml1.MA_LOAI_KCB == 1)
        //            {
        //                ngayDtri = 0;
        //            }
        //            //haimaster update 30/11/2022 => voi loai KCB =2 ngoai tru khong can kiem tra
        //            if (xml1.SO_NGAY_DTRI.Value != ngayDtri && (xml1.MA_LOAI_KCB==1 || xml1.MA_LOAI_KCB==3))
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = 238,
        //                    XEErrorRow = xml1.STT
        //                });
        //            }
        //        }
        //    }

        //    //439 1   XML3   SO_LUONG    Tổng ngày giường lớn hơn hướng dẫn của BYT(TT39)
        //    //440 1   XML3   SO_LUONG    Tổng ngày giường nhỏ hơn hướng dẫn của BYT(TT39)
        //    //884 1   XML3   SO_LUONG    Không thanh toán tiền giường điều trị nội trú của BN nằm viện < 4h
        //    if (ngay_RAVAO_Correct && xml1.MA_LOAI_KCB == 3)
        //    {
        //        int ngayGiuong = 0;
        //        if (tongGioDtri >= 4d && tongGioDtri <= 24d)
        //        {
        //            ngayGiuong = 1;
        //        }
        //        //Linhist changed 12/01/23
        //        //else if ((xml1.TINH_TRANG_RV == 2 && xml1.KET_QUA_DTRI > 1 && xml1.KET_QUA_DTRI < 5) || (xml1.TINH_TRANG_RV == 4 && xml1.KET_QUA_DTRI > 2 && xml1.KET_QUA_DTRI < 5))
        //        else if (
        //                    (xml1.TINH_TRANG_RV == 1 && (xml1.KET_QUA_DTRI == 4 || xml1.KET_QUA_DTRI == 5)) ||
        //                    (xml1.TINH_TRANG_RV == 2 && xml1.KET_QUA_DTRI > 1 && xml1.KET_QUA_DTRI < 5) ||
        //                    (xml1.TINH_TRANG_RV == 4 && xml1.KET_QUA_DTRI > 2 && xml1.KET_QUA_DTRI < 5)
        //            )
        //        {
        //            ngayGiuong = tongNgayDtri + 1;
        //        }
        //        else
        //        {
        //            ngayGiuong = tongNgayDtri;
        //        }

        //        var allGiuongs = xml1.XML3s.Where(x => x.MA_NHOM == 14 || x.MA_NHOM == 15);
        //        double? sumGiuongs = allGiuongs.Sum(x => x.SO_LUONG);
        //        int sumNgayGiuong = sumGiuongs == null || !sumGiuongs.HasValue ? 0 : (int)Math.Round(sumGiuongs.Value, 0);
        //        if (sumNgayGiuong != ngayGiuong)
        //        {
        //            foreach (var xml3 in allGiuongs)
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = sumNgayGiuong > ngayGiuong ? 439 : 440,
        //                    XEErrorRow = xml3.STT
        //                });
        //            }
        //        }
        //        //Linhist removed 11/05/22 bo loi nay ko kiem tra nua
        //        //else if (sumNgayGiuong != xml1.SO_NGAY_DTRI)
        //        //{
        //        //    file.XML1Errors.Add(new XML1Error()
        //        //    {
        //        //        //XEXML1ID =xml1.XML1ID,
        //        //        XEErrorCode = 151,
        //        //        XEErrorRow = xml1.STT
        //        //    });
        //        //}
        //    }
        //    #endregion

        //    return allDates;
        //}
        //#endregion

        //#region Linhist added 13/04/22
        //private void VerifyDacBiet_ThanhToan_PhauThuat(List<XML1Error> errors, List<XML3> lstPhauThuat)
        //{
        //    //Linhist removed 13/04/23
        //    //891   1   XML3 TYLE_TT   Phẫu thuật lần 2 trong ngày thanh toán 100% sai với CV 824-BYT
        //    //var sortPhauThuat = lstPhauThuat.OrderBy(x => x.NGAY_YL);
        //    //DateTime sameDate = DateTime.MinValue;
        //    //List<int?> lstSTTs = new List<int?>();

        //    //foreach (XML3 pt in sortPhauThuat)
        //    //{
        //    //    if (pt.NGAY_YL.Value.Date != sameDate)
        //    //    {
        //    //        if (lstSTTs.Count > 0)
        //    //        {
        //    //            foreach (int stt in lstSTTs)
        //    //            {
        //    //                pt.XML1.XML1Errors.Add(new XML1Error()
        //    //                {
        //    //                    //XEXML1ID =pt.XML1.XML1ID,
        //    //                    XEErrorCode = 891,
        //    //                    XEErrorRow = stt
        //    //                });
        //    //            }
        //    //        }

        //    //        sameDate = pt.NGAY_YL.Value.Date;
        //    //        lstSTTs = new List<int?>();
        //    //    }

        //    //    lstSTTs.Add(pt.STT);
        //    //}

        //    //Linhist added 13/04/23
        //    //891   1   XML3 TYLE_TT   Phẫu thuật lần 2 trong ngày thanh toán 100% sai với CV 824-BYT
        //    var grpPT = lstPhauThuat.GroupBy(x => x.NGAY_YL.Value.Date).Where(g => g.Count() > 1);
        //    foreach (var g in grpPT)
        //    {
        //        if (g.Count() < 2) continue;
        //        foreach (XML3 pt in g)
        //        {
        //            pt.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =pt.XML1.XML1ID,
        //                XEErrorCode = 891,
        //                XEErrorRow = pt.STT
        //            });

        //        }
        //    }
        //}
        //#endregion

        //#region Linhist added 31/05/22
        ////Viet lai HDSD Dich vu theo case
        //private void Verify_HDSDDichvu(List<XML1Error> errors, IEnumerable<HDSDDichVu> ruleDichvus, List<XML3> xml3ToiThieus, List<HDSDDichVu> hdsdToiThieus, XML3 xml3, double tuoiBN, string maBenhICD10, bool coThai, bool choConBu)
        //{
        //    //Linhist added 14/06/22
        //    //Kiem tra rieng case CHONG CHI DINH, co THOI GIAN TOI THIEU:
        //    var dichvuKhacs = xml3.XML1?.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));

        //    //Filter rieng cac rule CHONG CHI DINH co THOI GIAN TOI THIEU:
        //    //Neu co it nhat 1 rule match MA_KHOA BENH trong xml1 ma co THOI GIAN TOI THIEU thi chi kiem tra theo rule do, cac rule con lai bo qua
        //    //Neu khong co rule nao match MA_KHOA BENH trong xml1 thi chi kiem tra nhung rule co THOI GIAN TOI THIEU khong co MA_KHOA BENH, cac rule co MA_KHOA BENH bo qua
        //    var ruleToiThieus = ruleDichvus.Where(x => x.CHI_DINH == false && x.THOI_GIAN_TOI_THIEU != null && x.THOI_GIAN_TOI_THIEU != 0 && !string.IsNullOrEmpty(x.MA_BENH) && Uti.CodeMatch(maBenhICD10, x.MA_BENH));
        //    if (!ruleToiThieus.Any())
        //    {
        //        ruleToiThieus = ruleDichvus.Where(x => x.CHI_DINH == false && x.THOI_GIAN_TOI_THIEU != null && x.THOI_GIAN_TOI_THIEU != 0 && string.IsNullOrEmpty(x.MA_BENH));
        //    }

        //    if (ruleToiThieus.Any() && xml3.NGAY_YL != null)
        //    {
        //        foreach (var rule in ruleToiThieus)
        //        {
        //            if (rule == null) continue;

        //            try
        //            {
        //                //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua
        //                if (rule.MA_LOAI_KCB != null && rule.MA_LOAI_KCB != 0 && xml3.XML1.MA_LOAI_KCB != null)
        //                {
        //                    if (rule.MA_LOAI_KCB.Value != (int)xml3.XML1.MA_LOAI_KCB.Value) continue;
        //                }

        //                //Kiem tra cac MA_KHOA DICH VU khac trong cung ho so xem co chi dinh 2 lan trong THOI_GIAN TOI THIEU khong:
        //                DateTime dMin = xml3.NGAY_YL.Value.AddDays(-rule.THOI_GIAN_TOI_THIEU.Value);
        //                if (dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL < xml3.NGAY_YL.Value))
        //                {
        //                    xml3.XML1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml3.XML1ID,
        //                        XEErrorCode = int.Parse(rule.MA_LOI),
        //                        XEErrorRow = xml3.STT
        //                    });
        //                }
        //                //Neu khong chi dinh dich vu trong cung ho so thi vao case kiem tra THOI_GIAN_TOI_THIEU rieng
        //                else
        //                {
        //                    xml3ToiThieus.Add(xml3);
        //                    hdsdToiThieus.Add(rule);
        //                }
        //            }
        //            catch { }
        //        }
        //    }

        //    //Linhist added 14/06/22
        //    //Filter cac case con lai, bo qua het nhung rule CHONG CHI DINH co THOI GIAN TOI THIEU (co hoac khong co MA_KHOA BENH) da kiem tra o tren
        //    ruleDichvus = ruleDichvus.Where(x => !(x.CHI_DINH == false && x.THOI_GIAN_TOI_THIEU != null && x.THOI_GIAN_TOI_THIEU != 0));

        //    foreach (HDSDDichVu rule in ruleDichvus)
        //    {
        //        if (rule == null) continue;

        //        try
        //        {
        //            bool hasErr = false;

        //            //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua
        //            if (rule.MA_LOAI_KCB != null && rule.MA_LOAI_KCB != 0 && xml3.XML1.MA_LOAI_KCB != null)
        //            {
        //                if (rule.MA_LOAI_KCB.Value != (int)xml3.XML1.MA_LOAI_KCB.Value) continue;
        //            }

        //            bool hasDoTuoi = !string.IsNullOrEmpty(rule.TUOI);
        //            bool hasMaBenh = !string.IsNullOrEmpty(rule.MA_BENH);
        //            bool hasMaNhom = !string.IsNullOrEmpty(rule.MA_NHOM);
        //            bool hasDichVu = !string.IsNullOrEmpty(rule.DICH_VU_KHAC);
        //            //26/12/2022 haimaster update
        //            bool hasThuocKhac = !string.IsNullOrEmpty(rule.THUOC_KHAC);

        //            //Linhist removed 14/06/22
        //            //var dichvuKhacs = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU));

        //            //Case CHI DINH:
        //            if (rule.CHI_DINH == true)
        //            {
        //                //Kiem tra DO_TUOI: neu tuoi BN ko phu hop dieu kien --> bao loi
        //                if (hasDoTuoi && !Uti.NumMatch(tuoiBN, rule.TUOI))
        //                {
        //                    hasErr = true;
        //                    goto RAISE_ERROR_HDSD_DICHVU;
        //                }

        //                //Linhist added 04/11/22
        //                //Trg hop co MA_NHOM:
        //                if (hasMaBenh && hasMaNhom)
        //                {
        //                    //Neu xml1 ko co MA_BENH tuong ung hoac xml3 khong co MA_DICH_VU tuong ung --> bao loi
        //                    if (!Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH) && !xml3.XML1.XML3s.Any(x => $";{rule.MA_NHOM};".Contains($";{x.MA_NHOM};")))
        //                    {
                               
        //                            hasErr = true;
        //                            goto RAISE_ERROR_HDSD_DICHVU;
                                
        //                    }
        //                }
        //                else if (hasMaNhom)
        //                {
        //                    if (!xml3.XML1.XML3s.Any(x => $";{rule.MA_NHOM};".Contains($";{x.MA_NHOM};")))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_DICHVU;
        //                    }
        //                }

        //                //Kiem tra MA_BENH & DICH_VU_KHAC:
        //                //Trg hop co MA_BENH & co DICH_VU_KHAC:
        //                if (hasMaBenh && hasDichVu)
        //                {
        //                    //Neu xml1 ko co MA_BENH tuong ung hoac xml3 khong co MA_DICH_VU tuong ung --> bao loi
        //                    if (!Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH) && !dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_DICHVU;
        //                    }
        //                }
        //                //Trg hop chi co DICH_VU_KHAC:
        //                else if (hasDichVu)
        //                {
        //                    //Neu xml3 khong co MA_DICH_VU tuong ung --> bao loi
        //                    if (!dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_DICHVU;
        //                    }
        //                }


        //                //Trg hop chi co MA_BENH:
        //                //haimaster 6/6/22 bo doan check thua MA_DICH_VU_KHAC
        //                if (hasMaBenh && !hasMaNhom && !hasDichVu)
        //                {
        //                    //Neu xml1 ko co MA_BENH tuong ung --> bao loi
        //                    if (!Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        //                    {
        //                        //haimaster update code 15.2.2023
        //                        if (!(xml3.XML1.MA_CSKCB == "10061" && (xml3.MA_KHOA.Trim().ToString().ToUpper() == "K03" || xml3.MA_KHOA.Trim().ToUpper().ToString() == "K11") && (rule.HDSDDVID.ToString() == "131" || rule.HDSDDVID.ToString() == "132" || rule.HDSDDVID.ToString() == "133")))
        //                        {
        //                            hasErr = true;
        //                            goto RAISE_ERROR_HDSD_DICHVU;
        //                        }
        //                    }
        //                }

        //                //26/12/2022 Case co THUOC_KHAC
        //                if(hasThuocKhac)
        //                {
        //                    //neu co dich vu trong HDSD Dich vu & co thuoc khac
        //                    //kiem tra neu xml2 khong co thuoc khac => bao loi
        //                    if (!xml3.XML1.XML2s.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC))) {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_DICHVU;
        //                    }
        //                }
        //            }
        //            //Case CHONG CHI DINH:
        //            else
        //            {
        //                //Kiem tra DO_TUOI: neu tuoi BN phu hop dieu kien --> bao loi
        //                if (hasDoTuoi)
        //                {
        //                    hasErr = Uti.NumMatch(tuoiBN, rule.TUOI);
        //                    goto RAISE_ERROR_HDSD_DICHVU;
        //                }

        //                //Kiem tra THOI_GIAN_THUC_HIEN: neu thoi gian thuc hien phu hop dieu kien --> bao loi
        //                if (!string.IsNullOrEmpty(rule.THOI_GIAN_THUC_HIEN))
        //                {
        //                    if (xml3.NGAY_KQ != null && xml3.NGAY_YL != null)
        //                    {
        //                        double thucHien = 0d;
        //                        string ruleThucHien = rule.THOI_GIAN_THUC_HIEN;
        //                        TimeSpan tmeDim = (xml3.NGAY_KQ.Value - xml3.NGAY_YL.Value);

        //                        if (ruleThucHien.EndsWith("p"))
        //                        {
        //                            ruleThucHien = ruleThucHien[0..^1];
        //                            thucHien = tmeDim.TotalMinutes;
        //                        }
        //                        else if (ruleThucHien.EndsWith("h"))
        //                        {
        //                            ruleThucHien = ruleThucHien[0..^1];
        //                            thucHien = tmeDim.TotalHours;
        //                        }
        //                        else
        //                        {
        //                            thucHien = tmeDim.TotalDays;
        //                        }

        //                        hasErr = Uti.NumMatch(thucHien, ruleThucHien);
        //                    }
        //                    goto RAISE_ERROR_HDSD_DICHVU;
        //                }
        //                //30/6/23 update dich vu khac
        //                if(!string.IsNullOrEmpty(rule.DICH_VU_KHAC) && string.IsNullOrEmpty(rule.MA_BENH))
        //                {
        //                    if (dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_DICHVU;
        //                    }
        //                }
                        

        //                //Kiem tra TYLE_TT
        //                //if (!string.IsNullOrEmpty(rule.TYLE_TT) && xml3.TYLE_TT != null)
        //                //{
        //                //    hasErr = Uti.NumMatch(xml3.TYLE_TT.Value, rule.TYLE_TT);
        //                //    goto RAISE_ERROR_HDSD_DICHVU;
        //                //}

        //                //Linhist changed 14/06/22 tach case nay kiem tra rieng, de filter dieu kien MA_KHOA BENH
        //                //Kiem tra THOI_GIAN_TOI_THIEU:
        //                //if (rule.THOI_GIAN_TOI_THIEU != null && rule.THOI_GIAN_TOI_THIEU != 0)
        //                //{
        //                //    if (xml3.NGAY_YL != null)
        //                //    {
        //                //        DateTime dMin = xml3.NGAY_YL.Value.AddDays(-rule.THOI_GIAN_TOI_THIEU.Value);
        //                //        DateTime dMax = xml3.NGAY_YL.Value.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);
        //                //        if (dichvuKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax))
        //                //        {
        //                //            hasErr = true;
        //                //            goto RAISE_ERROR_HDSD_DICHVU;
        //                //        }

        //                //        //Trg hop ko co MA_BENH: vao case kiem tra THOI_GIAN_TOI_THIEU rieng
        //                //        if (!hasMaBenh)
        //                //        {
        //                //            xml3ToiThieus.Add(xml3);
        //                //            hdsdToiThieus.Add(rule);
        //                //        }
        //                //        //Trg hop co MA_BENH: neu xml1 co MA_BENH tuong ung thi vao case kiem tra THOI_GIAN_TOI_THIEU rieng
        //                //        else if (Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        //                //        {
        //                //            xml3ToiThieus.Add(xml3);
        //                //            hdsdToiThieus.Add(rule);
        //                //        }
        //                //    }
        //                //}

        //                //Trg hop chi co MA_BENH:
        //                if (hasMaBenh)
        //                {
        //                    //Neu xml1 co MA_BENH tuong ung --> bao loi
        //                    if (Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        //                    {
        //                        hasErr = true;
        //                        goto RAISE_ERROR_HDSD_DICHVU;
        //                    }
        //                }
        //            }

        //            RAISE_ERROR_HDSD_DICHVU:
        //            if (hasErr)
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = int.Parse(rule.MA_LOI),
        //                    XEErrorRow = xml3.STT
        //                });
        //            }
        //        }
        //        catch
        //        { }
        //    }
        //}
        //#endregion

        //#region Linhist added 15/04/22
        //////Tach case verify HDSDDichvu rieng cho XML3
        //////Linhist 25/03/22 them list benh theo ICD10 & sua lai cac dieu kien kiem tra
        ////private void Verify_HDSDDichvu_OLD(List<XML1Error> errors, IEnumerable<HDSDDichVu> ruleDichvus, List<XML3> xml3ToiThieus, List<HDSDDichVu> hdsdToiThieus, XML3 xml3, double tuoiBN, string maBenhICD10, bool coThai, bool choConBu)
        ////{
        ////    //Linhist changed 14/05/22 kiem tra dong thoi ca ca rule THOI_GIAN_TOI THIEU, ko tach rieng nua
        ////    ////Linhist check rule khong co THOI_GIAN_TOI THIEU
        ////    //var ruleCungNgay = ruleDichvus.Where(r => r.THOI_GIAN_TOI_THIEU == null || r.THOI_GIAN_TOI_THIEU == 0d);
        ////    foreach (HDSDDichVu rule in ruleDichvus)
        ////    {
        ////        if (rule == null) continue;

        ////        try
        ////        {
        ////            bool hasErr = false;
        ////            bool chiDinh = true.Equals(rule.CHI_DINH);

        ////            //Linhist removed 14/05/22
        ////            ////Kiem tra CO_THAI hoac CHO_CON_BU (??? cai nay can dieu kien kiem tra them)
        ////            //bool coThai = false; // Can dieu kien kiem tra
        ////            //bool choConBu = false; // Can dieu kien kiem tra
        ////            //if (coThai != true.Equals(rule.CO_THAI) || choConBu != true.Equals(rule.CHO_CON_BU)) continue;

        ////            //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua
        ////            if (rule.MA_LOAI_KCB != null && rule.MA_LOAI_KCB != 0 && xml3.XML1.MA_LOAI_KCB != null)
        ////            {
        ////                if (rule.MA_LOAI_KCB.Value != (int)xml3.XML1.MA_LOAI_KCB.Value) continue;
        ////            }

        ////            //Linhist changed 14/05/22
        ////            if ((chiDinh && true.Equals(rule.CO_THAI)) != coThai)
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_DICHVU;
        ////            }
        ////            if ((chiDinh && true.Equals(rule.CHO_CON_BU)) != choConBu)
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_DICHVU;
        ////            }

        ////            //Kiem tra TYLE_TT
        ////            if (!string.IsNullOrEmpty(rule.TYLE_TT))
        ////            {
        ////                double tyleTT = xml3.TYLE_TT == null ? 0d : xml3.TYLE_TT.Value;
        ////                if (chiDinh != Uti.NumMatch(tyleTT, rule.TYLE_TT))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }

        ////            //Kiem tra THOI_GIAN_THUC_HIEN
        ////            if (!string.IsNullOrEmpty(rule.THOI_GIAN_THUC_HIEN) && xml3.NGAY_KQ != null && xml3.NGAY_YL != null)
        ////            {
        ////                double thucHien = 0d;
        ////                string ruleThucHien = rule.THOI_GIAN_THUC_HIEN;
        ////                TimeSpan tmeDim = (xml3.NGAY_KQ.Value - xml3.NGAY_YL.Value);

        ////                if (ruleThucHien.EndsWith("p"))
        ////                {
        ////                    ruleThucHien = ruleThucHien[0..^1];
        ////                    thucHien = tmeDim.TotalMinutes;
        ////                }
        ////                else if (ruleThucHien.EndsWith("h"))
        ////                {
        ////                    ruleThucHien = ruleThucHien[0..^1];
        ////                    thucHien = tmeDim.TotalHours;
        ////                }
        ////                else
        ////                {
        ////                    thucHien = tmeDim.TotalDays;
        ////                }

        ////                if (chiDinh != Uti.NumMatch(thucHien, ruleThucHien))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }

        ////            //Kiem tra DO_TUOI: co chiDinh (== true) thi dieu kien DO_TUOI phai dung, khong chiDinh (== false) thi dieu kien DO_TUOI cung phai khong dung
        ////            if (!string.IsNullOrEmpty(rule.TUOI) && chiDinh != Uti.NumMatch(tuoiBN, rule.TUOI))
        ////            {
        ////                hasErr = true;
        ////                goto RAISE_ERROR_HDSD_DICHVU;
        ////            }

        ////            #region Linhist removed 14/05/22 kiem tra lai dieu kien rang buoc MA_BENH, THUOC_KHAC, DICH_VU_KHAC
        ////            //if (!string.IsNullOrEmpty(rule.MA_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC) && string.IsNullOrEmpty(rule.DICH_VU_KHAC))
        ////            //{
        ////            //    if (chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////            //    {
        ////            //        hasErr = true;
        ////            //        goto RAISE_ERROR_HDSD_DICHVU;
        ////            //    }
        ////            //    return;
        ////            //}

        ////            ////Kiem tra MA_BENH theo ICD10 (theo chuoi tuong ung da xu ly MA_BENH voi ma anh xa ICD10 tu XML1)
        ////            //bool passBENH = (string.IsNullOrEmpty(rule.MA_BENH) || Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH));

        ////            ////Kiem tra THUOC_KHAC theo THOI_GIAN_TOI_THIEU cung luc
        ////            //if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        ////            //{
        ////            //    var allThuocs = xml3.XML1.XML2s.Where(x => x.NGAY_YL != null && (rule.THOI_GIAN_TOI_THIEU == null || x.NGAY_YL == xml3.NGAY_YL));
        ////            //    if (chiDinh != allThuocs.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        ////            //    {
        ////            //        hasErr = !passBENH;
        ////            //        goto RAISE_ERROR_HDSD_DICHVU;
        ////            //    }
        ////            //}

        ////            ////Kiem tra DICH_VU_KHAC theo THOI_GIAN_TOI_THIEU cung luc
        ////            //if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC))
        ////            //{
        ////            //    var allDichvus = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU) && (rule.THOI_GIAN_TOI_THIEU == null || x.NGAY_YL == xml3.NGAY_YL));
        ////            //    if (chiDinh != allDichvus.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        ////            //    {
        ////            //        hasErr = !passBENH;
        ////            //        goto RAISE_ERROR_HDSD_DICHVU;
        ////            //    }
        ////            //}
        ////            #endregion

        ////            if (!string.IsNullOrEmpty(rule.MA_BENH) && string.IsNullOrEmpty(rule.THUOC_KHAC) && string.IsNullOrEmpty(rule.DICH_VU_KHAC))
        ////            {
        ////                if (chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }

        ////            //Kiem tra MA_BENH theo ICD10 (theo chuoi tuong ung da xu ly MA_BENH voi ma anh xa ICD10 tu XML1)
        ////            //khong co rule MA_BENH hoac xml3 MA_BENH sai so voi rule thi kiem tra tiep THUOC_KHAC & DICH_VU_KHAC
        ////            if (string.IsNullOrEmpty(rule.MA_BENH) || chiDinh != Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", rule.MA_BENH))
        ////            {
        ////                //Kiem tra THUOC_KHAC theo THOI_GIAN_TOI_THIEU cung luc
        ////                if (!string.IsNullOrEmpty(rule.THUOC_KHAC))
        ////                {
        ////                    var allThuocs = xml3.XML1.XML2s.Where(x => x.NGAY_YL != null && (rule.THOI_GIAN_TOI_THIEU == null || x.NGAY_YL == xml3.NGAY_YL));
        ////                    if (chiDinh != allThuocs.Any(x => Uti.CodeMatch(x.MA_THUOC, rule.THUOC_KHAC)))
        ////                    {
        ////                        hasErr = true;
        ////                        goto RAISE_ERROR_HDSD_DICHVU;
        ////                    }
        ////                }

        ////                //Kiem tra DICH_VU_KHAC theo THOI_GIAN_TOI_THIEU cung luc
        ////                if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC))
        ////                {
        ////                    var allDichvus = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU) && (rule.THOI_GIAN_TOI_THIEU == null || x.NGAY_YL == xml3.NGAY_YL));
        ////                    if (chiDinh != allDichvus.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC)))
        ////                    {
        ////                        hasErr = true;
        ////                        goto RAISE_ERROR_HDSD_DICHVU;
        ////                    }
        ////                }
        ////            }

        ////            //Kiem tra MA_DICH_VU dong thoi trong cung ho so
        ////            if (rule.THOI_GIAN_TOI_THIEU == 0d)
        ////            {
        ////                if (true.Equals(rule.CHI_DINH) != xml3.XML1.XML3s.Any(x => x.STT != xml3.STT && Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU)))
        ////                {
        ////                    hasErr = true;
        ////                    goto RAISE_ERROR_HDSD_DICHVU;
        ////                }
        ////            }
        ////            //Kiem tra thoi gian toi thieu
        ////            else if (rule.THOI_GIAN_TOI_THIEU != null && xml3.NGAY_YL != null)
        ////            {
        ////                hdsdToiThieus.Add(rule);
        ////                xml3ToiThieus.Add(xml3);
        ////            }

        ////        RAISE_ERROR_HDSD_DICHVU:
        ////            if (hasErr)
        ////            {
        ////                file.XML1Errors.Add(new XML1Error()
        ////                {
        ////                    //XEXML1ID =xml3.XML1ID,
        ////                    XEErrorCode = int.Parse(rule.MA_LOI),
        ////                    XEErrorRow = xml3.STT
        ////                });
        ////            }
        ////        }
        ////        catch
        ////        { }
        ////    }

        ////    #region Linhist removed 14/05/22 bo qua doan nay da kiem tra gop o tren
        ////    //if (xml3.NGAY_YL == null) return;

        ////    ////Linhist check rule co THOI_GIAN_TOI_THIEU
        ////    ////Neu co MA_BENH tuong ung trong XML3 thi chi check nhung rule co MA_BENH
        ////    ////Neu khong thi chi check nhung rule khong co MA_BENH bo qua rule co MA_BENH
        ////    //ruleDichvus = ruleDichvus.Where(r => r.THOI_GIAN_TOI_THIEU != null && r.THOI_GIAN_TOI_THIEU != 0d);
        ////    //var ruleToiThieu = ruleDichvus.Where(r => !string.IsNullOrEmpty(r.MA_BENH) && Uti.CodeMatch($"{maBenhICD10};{xml3.MA_BENH}", r.MA_BENH));
        ////    //if (ruleToiThieu.Any())
        ////    //{
        ////    //    ruleDichvus = ruleToiThieu;
        ////    //}
        ////    //else
        ////    //{
        ////    //    ruleDichvus = ruleDichvus.Where(r => string.IsNullOrEmpty(r.MA_BENH));
        ////    //}

        ////    //foreach (HDSDDichVu rule in ruleDichvus)
        ////    //{
        ////    //    if (rule == null) continue;

        ////    //    try
        ////    //    {
        ////    //        bool chiDinh = true.Equals(rule.CHI_DINH);

        ////    //        //Linhist removed 14/05/22
        ////    //        ////Kiem tra CO_THAI hoac CHO_CON_BU (??? cai nay can dieu kien kiem tra them)
        ////    //        //bool coThai = false; // Can dieu kien kiem tra
        ////    //        //bool choConBu = false; // Can dieu kien kiem tra
        ////    //        //if (coThai != true.Equals(rule.CO_THAI) || choConBu != true.Equals(rule.CHO_CON_BU)) continue;

        ////    //        //Kiem tra MA_LOAI_KCB, neu khong dung thi bo qua
        ////    //        if (rule.MA_LOAI_KCB != null && rule.MA_LOAI_KCB != 0 && xml3.XML1.MA_LOAI_KCB != null)
        ////    //        {
        ////    //            if (rule.MA_LOAI_KCB.Value != (int)xml3.XML1.MA_LOAI_KCB.Value) continue;
        ////    //        }

        ////    //        DateTime dMin = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);
        ////    //        DateTime dMax = xml3.NGAY_YL.Value.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);

        ////    //        //Linhist bo qua kiem tra cung thoi diem, chi kiem tra trong thoi gian toi thieu
        ////    //        //Kiem tra DICH_VU_KHAC theo THOI_GIAN_TOI_THIEU trong cung nhom XML3
        ////    //        //var allDichvus = xml3.XML1.XML3s.Where(x => x.STT != xml3.STT && !string.IsNullOrWhiteSpace(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax);
        ////    //        //if (chiDinh != allDichvus.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU)))
        ////    //        //{
        ////    //        //    file.XML1Errors.Add(new XML1Error()
        ////    //        //    {
        ////    //        //        //XEXML1ID =xml3.XML1ID,
        ////    //        //        XEErrorCode = int.Parse(rule.MA_LOI),
        ////    //        //        XEErrorRow = xml3.STT
        ////    //        //    });
        ////    //        //}
        ////    //        ////Neu khong danh dau de kiem tra them trong du lieu lich su cua benh nhan trong database
        ////    //        //else
        ////    //        //{
        ////    //        hdsdToiThieus.Add(rule);
        ////    //        xml3ToiThieus.Add(xml3);
        ////    //        //}
        ////    //    }
        ////    //    catch
        ////    //    { }
        ////    //}
        ////    #endregion
        ////}
        //#endregion

        //#region Linhist added 15/04/22
        ////Tach case verify HDSDDichvu lich su theo thoi gian toi thieu
        //private async Task Verify_Thoigian_Toithieu(List<XML1Error> errors, List<XML3> xml3ToiThieu, List<HDSDDichVu> hdsdToiThieu)
        //{
        //    //Lay ngay toi thieu & ngay toi da can kiem tra
        //    XML3 xml3 = null;
        //    HDSDDichVu rule = null;
        //    DateTime dMin = DateTime.MaxValue;
        //    //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
        //    //DateTime dMax = DateTime.MinValue;

        //    for (int i = 0; i < xml3ToiThieu.Count; i++)
        //    {
        //        xml3 = xml3ToiThieu[i];
        //        rule = hdsdToiThieu[i];

        //        DateTime dTmp = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);
        //        if (dTmp < dMin)
        //        {
        //            dMin = dTmp;
        //        }

        //        //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
        //        //dTmp = xml3.NGAY_YL.Value;//.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);
        //        //if (dTmp > dMax)
        //        //{
        //        //    dMax = dTmp;
        //        //}
        //    }

        //    try
        //    {
        //        //Linhist changed 16/05/22 lay list XML1ID de loai bo file trung, hoac da bi xoa, hoac khong phai cung vien...
        //        //Lay list XML1 lich su trong khoang thoi gian can kiem tra
        //        //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
        //        //var xml1Lichsu = await _Xml1Repository.GetListAsync(x => x.XML1ID != xml3.XML1ID && x.isDelete != true && x.MA_BN == xml3.XML1.MA_BN && x.MA_CSKCB == xml3.XML1.MA_CSKCB && x.MA_LK != xml3.MA_LK && x.NGAY_VAO != null && x.NGAY_VAO >= dMin && x.NGAY_RA != null && x.NGAY_RA <= dMax);
        //        var xml1Lichsu = await _hosoRepository.GetListAsync(x => x.XML01ID != xml3.XML1ID && x.IsDelete != true && x.MA_BN == xml3.XML1.MA_BN && x.MA_CSKCB == xml3.XML1.MA_CSKCB && x.MA_LK != xml3.MA_LK && x.NGAY_VAO != null && x.NGAY_VAO >= dMin && x.NGAY_RA != null && x.NGAY_RA <= xml3.NGAY_YL.Value);
        //        List<long> xml1IDs = xml1Lichsu.Select(x => x.XML01ID).ToList();

        //        //Lay list XML3 lich su trong khoang thoi gian can kiem tra
        //        //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
        //        //var xml3Lichsu = await _Xml3Repository.GetListAsync(x => x.XML3ID != xml3.XML3ID && x.MA_LK != xml3.MA_LK && !string.IsNullOrEmpty(x.MA_DICH_VU) && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= dMax && x.XML1ID != null && xml1IDs.Contains(x.XML1ID.Value));
        //        var xml3Lichsu = await _xml03Repository.GetListAsync(x => x.MA_LK != xml3.MA_LK && x.NGAY_YL != null && x.NGAY_YL >= dMin && x.NGAY_YL <= xml3.NGAY_YL.Value && xml1IDs.Contains(x.XML01ID) && x.MA_CSKCB == xml3.MA_CSKCB);
        //        if (xml3Lichsu == null || !xml3Lichsu.Any()) return;

        //        for (int i = 0; i < xml3ToiThieu.Count; i++)
        //        {
        //            xml3 = xml3ToiThieu[i];
        //            rule = hdsdToiThieu[i];
        //            dMin = xml3.NGAY_YL.Value.AddDays(0 - rule.THOI_GIAN_TOI_THIEU.Value);
        //            //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
        //            //dMax = xml3.NGAY_YL.Value;//.AddDays(rule.THOI_GIAN_TOI_THIEU.Value);

        //            //Kiem tra voi all xml3 lich su
        //            //Linhist changed 14/06/22 chi lay ngay lich su, ko lay thoi gian tuong lai
        //            var xml3GanNhat = xml3Lichsu.Where(x => x.NGAY_YL >= dMin && x.NGAY_YL <= xml3.NGAY_YL.Value);
        //            if (true.Equals(rule.CHI_DINH) != xml3GanNhat.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU)))
        //            {
        //                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml3.XML1ID,
        //                    XEErrorCode = int.Parse(rule.MA_LOI),
        //                    XEErrorRow = xml3.STT
        //                });
        //            }
        //        }
        //    }
        //    catch
        //    { }
        //}
        //#endregion

        //#region Linhist added 11/05/22
        //private void Verify_Thong_Tuyen(XML1 xml1, List<XML1Error> errors)
        //{
        //    if (!Uti.NumExists(xml1.CHECKED, FileStatusVerification.VERIFIED, FileStatusVerification.RECALL_VALIDATE_CHECK) && string.IsNullOrEmpty(xml1.DanhSachLoi)) return;

        //    //Linhist removed 06/07/22 bo kiem tra the tre em vi co truong hop the ko ton tai
        //    ////Linhist added 21/05/22 kiem tra neu la the tre em thi bo qua ko can check thong tuyen
        //    ////vi kiem tra thong tuyen co kiem tra ca the tre em nen dua doan nay len dau
        //    //string[] thes = (xml1.MA_THE ?? "").Split(';');
        //    //foreach (string t in thes)
        //    //{
        //    //    if (Check_TheTE_ChuaDiHoc(t, xml1.NGAY_SINH)) return;
        //    //}

        //    //Linhist removed log 21/05/22
        //    //Uti.HandleCronJobLog(_logger, $"B10.1: 20/05/2022 Verify_Thong_Tuyen kiem tra co qua dieu kien cua Linh  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //    //haimaster 3/11/2022 update bo vao try cach
        //    try
        //    {

        //        if (xml1.DanhSachLoi == "000")
        //        {
        //            #region Linhist removed 05/08/22 chuyen vao kiem tra thong tin phu khi check the
        //            ////218 1   XML1 MA_DKBD Thẻ sai nơi đăng ký KCB ban đầu
        //            //if (!string.IsNullOrEmpty(xml1.MHTCID) && !string.IsNullOrEmpty(xml1.MA_DKBD))
        //            //{
        //            //    #region Linhist removed 21/05/22 kiem tra theo MA_CSKCB ko phai theo MA_THE
        //            //    //string[] mas = xml1.MHTCID.Split(';');
        //            //    //if (mas.Length < 2) return;

        //            //    ////Linhist removed 21/05/22
        //            //    ////string[] thes = (xml1.MA_DKBD ?? "").Split(';');
        //            //    //for (int i = 0; i < thes.Length; i++)
        //            //    //{
        //            //    //    if (Check_TheTE_ChuaDiHoc(thes[i], xml1.NGAY_SINH)) continue;

        //            //    //    if (i + 1 < mas.Length && !Uti.IsEqual(thes[i], mas[i + 1], ignoreEmpty: true))
        //            //    //    {
        //            //    //        file.XML1Errors.Add(new XML1Error()
        //            //    //        {
        //            //    //            //XEXML1ID =xml1.XML1ID,
        //            //    //            XEErrorCode = 218,
        //            //    //            XEErrorRow = xml1.STT,
        //            //    //        });
        //            //    //    }
        //            //    //}
        //            //    #endregion

        //            //    //Linhist added 21/05/22

        //            //    string[] mas = xml1.MHTCID.Split(';');
        //            //    if (mas.Length < 2) return;

        //            //    string[] dkbds = xml1.MA_DKBD.Split(';');
        //            //    for (int i = 0; i < dkbds.Length; i++)
        //            //    {
        //            //        if (i + 1 >= mas.Length) return;

        //            //        //Linhist changed 03/06/22
        //            //        //if (dkbds[i] != xml1.MA_CSKCB)
        //            //        if (dkbds[i] != mas[i + 1])
        //            //        {
        //            //            file.XML1Errors.Add(new XML1Error()
        //            //            {
        //            //                //XEXML1ID =xml1.XML1ID,
        //            //                XEErrorCode = 218,
        //            //                XEErrorRow = xml1.STT,
        //            //            });
        //            //            return;
        //            //        }
        //            //    }
        //            //}

        //            ////Linhist removed log 21/05/22
        //            ////Uti.HandleCronJobLog(_logger, $"B10.2: 20/05/2022 Verify_Thong_Tuyen -> Qua Doan DANHSACHLOI == 000  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //            #endregion
        //        }
        //        else
        //        {
        //            //Linhist removed 21/05/22
        //            //string[] thes = (xml1.MA_THE ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries);
        //            //foreach (string t in thes)
        //            //{
        //            //    if (Check_TheTE_ChuaDiHoc(t, xml1.NGAY_SINH)) return;
        //            //}


        //            //Linhist added 04/07/22 kiem tra truong hop nhieu ma the co sai sot
        //            string[] dsLois = xml1.DanhSachLoi.Split(';').Distinct().ToArray();
        //            foreach (string maLoi in dsLois)
        //            {
        //                int errCode = 0;
        //                switch (maLoi)
        //                {
        //                    //222   2   XML1    MA_THE  Thẻ do BHXH Bộ Quốc Phòng quản lý, đề nghị kiểm tra thẻ và thông tin giấy tờ tùy thân
        //                    case "001": errCode = 222; break;

        //                    //223   2   XML1    MA_THE  Thẻ do BHXH Bộ Công An quản lý, đề nghị kiểm tra thẻ và thông tin giấy tờ tùy thân
        //                    case "002": errCode = 223; break;

        //                    //361   1   XML1    MA_THE  Thẻ cũ hết giá trị sử dụng nhưng đã được cấp thẻ mới
        //                    case "003": errCode = 361; break;

        //                    //362   2   XML1    MA_THE  Thẻ cũ còn giá trị sử dụng nhưng đã được cấp thẻ mới
        //                    case "004": errCode = 362; break;

        //                    //210   1   XML1    GT_THE_DEN  Thẻ hết giá trị sử dụng
        //                    case "010": errCode = 210; break;

        //                    //226   1   XML1    MA_THE  Không thấy thông tin thẻ BHYT
        //                    case "050": errCode = 226; break;

        //                    //209	1	XML1	MA_THE	Mã thẻ không đúng
        //                    case "051": errCode = 209; break;

        //                    //224   1   XML1    MA_THE  Mã tỉnh cấp thẻ(kí tự thứ 4,5 của mã thẻ) không đúng
        //                    case "052": errCode = 224; break;

        //                    //225   1   XML1    MA_THE  Mã quyền lợi thẻ(ký tự thứ 3 của mã thẻ) không đúng
        //                    case "053": errCode = 225; break;

        //                    //215   1   XML1    HO_TEN  Thẻ sai họ tên
        //                    case "060": errCode = 215; break;

        //                    //227   1   XML1    MA_THE  Thẻ sai họ tên(đúng kí tự đầu)
        //                    case "061": errCode = 277; break;

        //                    //216   1   XML1    NGAY_SINH   Thẻ sai ngày sinh
        //                    case "070": errCode = 216; break;

        //                    //221   1   XML1    MA_THE  Lỗi phát sinh trong quá trình kiểm tra thẻ
        //                    case "100": errCode = 221; break;

        //                    //229   1   XML1    MA_THE  Không có kết nối đến Máy chủ
        //                    case "101": errCode = 229; break;

        //                    //230   1   XML1    MA_THE  Thẻ đã thu hồi
        //                    case "110": errCode = 230; break;

        //                    //231   1   XML1    GT_THE_DEN  Thẻ đã báo giảm
        //                    case "120": errCode = 231; break;

        //                    //232   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Giảm chuyển ngoại tỉnh
        //                    case "121": errCode = 232; break;

        //                    //233   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Giảm chuyển nội tỉnh
        //                    case "122": errCode = 233; break;

        //                    //234   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Thu hồi do tăng lại cùng đơn vị
        //                    case "123": errCode = 234; break;

        //                    //235   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Ngừng tham gia
        //                    case "124": errCode = 235; break;

        //                    //236   2   XML1    MA_THE  Trẻ em không xuất trình thẻ
        //                    case "130": errCode = 236; break;

        //                    //221   1   XML1    MA_THE  Lỗi phát sinh trong quá trình kiểm tra thẻ
        //                    case "205": errCode = 221; break;

        //                    //228   1   XML1    MA_THE  Lỗi khi lấy dữ liệu sổ thẻ
        //                    case "401": errCode = 228; break;

        //                    //217 217 1   XML1 GIOI_TINH   Thẻ sai giới tính   LOI_THE
        //                    case "x217": errCode = 217; break;

        //                    //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
        //                    case "x211": errCode = 211; break;

        //                    //212 212 1   XML1 GT_THE_DEN  Hết hạn thẻ khi chưa ra viện LOI_THE
        //                    case "x212": errCode = 212; break;

        //                    //218 1   XML1 MA_DKBD Thẻ sai nơi đăng ký KCB ban đầu
        //                    case "x218": errCode = 218; break;

        //                    //1183	1183	2	XML1	GT_THE_TU	Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về
        //                    case "x1183": errCode = 1183; break;

        //                    //1184	1184	2	XML1	GT_THE_DEN	Sai hạn thẻ: Giá trị thẻ đến sai so với giá trị trả về
        //                    case "x1184": errCode = 1184; break;

        //                    //213 213 1   XML1 GT_THE_TU   Thẻ có giá trị khi đang nằm viện    LOI_THE
        //                    //214 214 1   XML1 MA_THE  Mã thẻ không có trong dữ liệu thẻ   LOI_THE

        //                    default: errCode = 0; break;
        //                }

        //                //Linhist removed 21/05/22
        //                //Uti.HandleCronJobLog(_logger, $"B10.3: 20/05/2022 Verify_Thong_Tuyen -> Qua doan DANHSACHLOI != 000  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //                if (errCode != 0)
        //                {
        //                    xml1.XML1Errors.Add(new XML1Error()
        //                    {
        //                        //XEXML1ID =xml1.XML1ID,
        //                        XEErrorCode = errCode,
        //                        XEErrorRow = xml1.STT,
        //                        MA_CSKCB = xml1.MA_CSKCB
        //                    });


        //                }


        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Uti.HandleCronJobLog(_logger, $"Loi khi phien xml1.DanhsachLoi vao XML1Error" + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyXML1));
        //    }



        //    //Linhist removed 17/10/22
        //    //Chuyen sang UpdateXml1AfterVerified kiem tra loi sai, sau khi loc error theo vien
        //    //if (errors.Count > 0) xml1.Creator = 2;
        //}
        //#endregion

        //#region Linhist added 16/05/22
        //private bool Check_TheTE_ChuaDiHoc(string maThe, DateTime? ngaySinh)
        //{
        //    if (string.IsNullOrEmpty(maThe) || !maThe.StartsWith("TE1")) return false;
        //    if (ngaySinh == null || !ngaySinh.HasValue) return false;

        //    double tuoiBN = (double)((DateTime.Now - ngaySinh.Value).Days / 365.242199);
        //    DateTime dMax = new DateTime(DateTime.Now.Year, 9, 30).AddDays(1);

        //    if (tuoiBN < 6 || (tuoiBN < 7 && DateTime.Now < dMax)) return true;
        //    return false;
        //}
        //#endregion

        //#region Linhist added 13/05/22
        //private void VerifyDacBiet_VuotTuyen2(List<XML1Error> errors, IEnumerable<XML2> allThuocs, IEnumerable<XML3> allDichvus)
        //{
        //    //Linhist changed bo loi 930, chi kiem tra theo rang buoc ma thuoc
        //    //if (!allDichvus.Any()) return;

        //    ////930	1	XML3	MA_DICH_VU	Chỉ định dịch vụ vượt tuyến nhưng không kê thuốc vượt tuyến
        //    //if (!allThuocs.Any())
        //    //{
        //    //    foreach (var xml3 in allDichvus)
        //    //    {
        //    //        file.XML1Errors.Add(new XML1Error()
        //    //        {
        //    //            //XEXML1ID =xml3.XML1ID,
        //    //            XEErrorCode = 930,
        //    //            XEErrorRow = xml3.STT
        //    //        });
        //    //    }
        //    //}
        //    if (!allThuocs.Any()) return;

        //    //873	1	XML2	MA_THUOC	Chỉ định thuốc vượt tuyến sai hướng dẫn của BYT
        //    if (!allDichvus.Any())
        //    {
        //        foreach (var xml2 in allThuocs)
        //        {
        //            xml2.XML1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml2.XML1ID,
        //                XEErrorCode = 873,
        //                XEErrorRow = xml2.STT
        //            });
        //        }
        //    }
        //    else
        //    {
        //        DateTime minNgayYL = allDichvus.Min(x => (x.NGAY_YL == null ? DateTime.MaxValue : x.NGAY_YL.Value));
        //        foreach (XML2 xml2 in allThuocs)
        //        {
        //            if (xml2.NGAY_YL != null && xml2.NGAY_YL.Value <= minNgayYL)
        //            {
        //                xml2.XML1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml2.XML1ID,
        //                    XEErrorCode = 873,
        //                    XEErrorRow = xml2.STT
        //                });
        //            }
        //        }
        //    }
        //}
        //#endregion

        //#region Linhist added 27/05/22
        //private void Verify_ChiDinh_DongThoi(List<XML1Error> errors, IEnumerable<ChiDinhDongThoi> ruleDongThois, List<XML3> allDichVus, List<XML3> allDvKhacs, List<XML3> allVatTus, string allDVsHaimaster)
        //{

        //    //Linhist added 04 / 06 / 22
        //    //Them mang chuoi kiem tra, neu xml3 da bao loi roi thi se ko bao lai nua
        //    List<string> errX3IDs = new List<string>();

        //    foreach (var rule in ruleDongThois)
        //    {
        //        //Linhist added 04/06/22
        //        //rule == null return la sai & them kiem tra MA_LOI, de gan 
        //        //if (rule == null) return;
        //        //if (string.IsNullOrEmpty(rule.MA_DICH_VU)) continue;
        //        if (rule == null || string.IsNullOrEmpty(rule.MA_DICH_VU) || rule.MA_LOI == null) return;

        //        var matchDVs = allDichVus.Where(x => Uti.CodeMatch(x.MA_DICH_VU, rule.MA_DICH_VU));
        //        // var matchVTs = allDichVus.Where(x => Uti.CodeContain(x.MA_VAT_TU, rule.VAT_TU_BHYT));
        //        // 20.7/2022 haimaster sua oode => Tach check dich vu rieng, vat tu ring
        //        //Case co CHI DINH cho dich vu:
        //        if (rule.CHI_DINH == true)
        //        {
        //            //Kiem tra DVKT vs VTBH
        //            if (!string.IsNullOrEmpty(rule.VAT_TU_BHYT))
        //            {
        //                if (rule.CUNG_HO_SO == true)
        //                {
        //                    var matchVTs = allVatTus.Where(x => Uti.CodeMatch(x.MA_VAT_TU, rule.VAT_TU_BHYT));

        //                    //Linhist changed 16/07/22 thay doi kiem tra chi co 1 trong 2 DV hoac VT (ko the co ca hai)
        //                    //haimaster doi lai
        //                    if (!matchDVs.Any() || !matchVTs.Any())

        //                    //if (matchDVs.Any() != matchVTs.Any())

        //                    {
        //                        foreach (var xml3 in matchDVs)
        //                        {
        //                            if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
        //                            {
        //                                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                                {
        //                                    //XEXML1ID =xml3.XML1ID,
        //                                    XEErrorCode = rule.MA_LOI,
        //                                    XEErrorRow = xml3.STT
        //                                });
        //                            }
        //                        }
        //                        //20.7.2022 haimaster them code
        //                        //var haimasterGetDVtheoVT = ruleDongThois.Where(x=> Uti.CodeMatch(x.VAT_TU_BHYT,a)
        //                        foreach (var xml3 in allVatTus)
        //                        {
        //                            var haimasterGetDVtheoVT = ruleDongThois.Where(x => Uti.CodeMatch(x.VAT_TU_BHYT, xml3.MA_VAT_TU) && Uti.CodeMatch(x.MA_DICH_VU, allDVsHaimaster));
        //                            if (!haimasterGetDVtheoVT.Any())
        //                            {

        //                                if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
        //                                {
        //                                    xml3.XML1.XML1Errors.Add(new XML1Error()
        //                                    {
        //                                        //XEXML1ID =xml3.XML1ID,
        //                                        XEErrorCode = rule.MA_LOI,
        //                                        XEErrorRow = xml3.STT
        //                                    });
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            //Kiem tra DVKT vs DVKT khac
        //            if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC))
        //            {
        //                if (rule.CUNG_NGAY_YL == true)
        //                {
        //                    var dates = matchDVs.Select(x => x.NGAY_YL == null ? (DateTime?)null : x.NGAY_YL.Value.Date).Distinct();

        //                    foreach (var dte in dates)
        //                    {
        //                        if (dte == null) continue;

        //                        if (!allDvKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
        //                        {
        //                            foreach (var xml3 in matchDVs.Where(x => x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
        //                            {
        //                                if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
        //                                {
        //                                    xml3.XML1.XML1Errors.Add(new XML1Error()
        //                                    {
        //                                        //XEXML1ID =xml3.XML1ID,
        //                                        XEErrorCode = rule.MA_LOI,
        //                                        XEErrorRow = xml3.STT
        //                                    });
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        //Case chong CHI DINH:
        //        else
        //        {
        //            //Kiem tra DVKT vs DVKT khac cung thoi diem
        //            if (!string.IsNullOrEmpty(rule.DICH_VU_KHAC))
        //            {
        //                if (rule.CUNG_THOI_DIEM == true)
        //                {
        //                    foreach (var xml3 in matchDVs)
        //                    {
        //                        if (xml3.NGAY_YL == null) continue;

        //                        if (allDvKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null && x.NGAY_YL == xml3.NGAY_YL))
        //                        {
        //                            if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
        //                            {
        //                                xml3.XML1.XML1Errors.Add(new XML1Error()
        //                                {
        //                                    //XEXML1ID =xml3.XML1ID,
        //                                    XEErrorCode = rule.MA_LOI,
        //                                    XEErrorRow = xml3.STT
        //                                });
        //                            }
        //                        }
        //                    }
        //                }

        //                //Linhist added 04/06/22
        //                //Kiem tra DVKT vs DVKT khac cung ngay YL
        //                if (rule.CUNG_NGAY_YL == true)
        //                {
        //                    var dates = matchDVs.Select(x => x.NGAY_YL == null ? (DateTime?)null : x.NGAY_YL.Value.Date).Distinct();

        //                    foreach (var dte in dates)
        //                    {
        //                        if (dte == null) continue;

        //                        if (allDvKhacs.Any(x => Uti.CodeMatch(x.MA_DICH_VU, rule.DICH_VU_KHAC) && x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
        //                        {
        //                            foreach (var xml3 in matchDVs.Where(x => x.NGAY_YL != null && x.NGAY_YL.Value.Date == dte))
        //                            {
        //                                if (!errors.Any(x => x.XEXML1ID == xml3.XML1ID && x.XEErrorCode == rule.MA_LOI && x.XEErrorRow == xml3.STT))
        //                                {
        //                                    xml3.XML1.XML1Errors.Add(new XML1Error()
        //                                    {
        //                                        //XEXML1ID =xml3.XML1ID,
        //                                        XEErrorCode = rule.MA_LOI,
        //                                        XEErrorRow = xml3.STT
        //                                    });
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //#endregion

        //#region Linhist added 08/06/22
        //private void Verify_PhacDo_DieuTri(List<XML1Error> errors, IEnumerable<PhacDoDieuTri> rulePhacDos, XML1 xml1)
        //{
        //    foreach (var rule in rulePhacDos)
        //    {
        //        if (rule == null) continue;

        //        try
        //        {
        //            bool hasErr = false;
        //            int? errCode = null;

        //            if (!string.IsNullOrEmpty(rule.PhacDoMaThuoc))
        //            {
        //                string[] allThuocs = rule.PhacDoMaThuoc.Split(";");
        //                foreach (string mt in allThuocs)
        //                {
        //                    if (!xml1.XML2s.Any(x => x.MA_THUOC == mt))
        //                    {
        //                        hasErr = true;
        //                        errCode = rule.PhacDoMaLoiThuoc;
        //                        goto RAISE_ERROR_PHACDO_DIEUTRI;
        //                    }
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(rule.PhacDoMaDichVu))
        //            {
        //                string[] allDichVus = rule.PhacDoMaDichVu.Split(";");
        //                foreach (string mt in allDichVus)
        //                {
        //                    if (!xml1.XML3s.Any(x => !string.IsNullOrEmpty(x.MA_DICH_VU) && x.MA_DICH_VU == mt))
        //                    {
        //                        hasErr = true;
        //                        errCode = rule.PhacDoMaLoiDichVu;
        //                        goto RAISE_ERROR_PHACDO_DIEUTRI;
        //                    }
        //                }
        //            }

        //            RAISE_ERROR_PHACDO_DIEUTRI:
        //            if (hasErr)
        //            {
        //                xml1.XML1Errors.Add(new XML1Error()
        //                {
        //                    //XEXML1ID =xml1.XML1ID,
        //                    XEErrorCode = errCode,
        //                    XEErrorRow = xml1.STT
        //                });
        //            }
        //        }
        //        catch
        //        { }
        //    }
        //}
        //#endregion

        //#region Linhist added 08/08/23
        //private void Verify_DacBiet_MaBenh(List<XML1Error> errors, XML1 xml1, Dictionary<string, string> dmDacBiet, string allMaBenhs, double tuoiBN, double ngayTuoi)
        //{
        //    //1600	1	XML1	GIOI_TINH	Mã Bệnh Nam Giới chỉ phù hợp với Bệnh nhân Nam (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (xml1.GIOI_TINH != 1 && dmDacBiet.ContainsKey("benhnamgioi"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhnamgioi"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1600,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    //1635	1	XML1	GIOI_TINH	Mã Bệnh Nữ Giới chỉ phù hợp với Bệnh nhân Nữ (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (xml1.GIOI_TINH != 2 && dmDacBiet.ContainsKey("benhphunu"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhphunu"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1635,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1601	1	XML1	GIOI_TINH	Mã Bệnh Sản Phụ Khoa chỉ phù hợp với Phụ nữ từ 9 - 60 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)	
        //    if (!(xml1.GIOI_TINH == 2 && tuoiBN >= 9d && tuoiBN <= 60d) && dmDacBiet.ContainsKey("benhsanphu"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhsanphu"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1601,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    //1626	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Sơ Sinh chỉ phù hợp với Trẻ em từ 0 - 365 ngày tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (ngayTuoi > 365d && dmDacBiet.ContainsKey("benhtreduoi1"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhtreduoi1"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1626,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1630	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Sơ Sinh chỉ phù hợp với Trẻ em trên 27 ngày tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (ngayTuoi < 27d && dmDacBiet.ContainsKey("benhtretren27n"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhtretren27n"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1630,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //    //1627	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Nhỏ chỉ phù hợp với Trẻ em từ 0 - 2 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (tuoiBN > 2d && dmDacBiet.ContainsKey("benhtreduoi2"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhtreduoi2"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1627,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1628	1	XML1	GIOI_TINH	Mã Bệnh Trẻ Lớn chỉ phù hợp với Trẻ em từ 0 - 10 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (tuoiBN > 10d && dmDacBiet.ContainsKey("benhtreduoi10"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhtreduoi10"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1628,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1629	1	XML1	GIOI_TINH	Mã Bệnh Thanh Thiếu Niên chỉ phù hợp với Thiếu niên từ 0 - 19 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (tuoiBN > 19d && dmDacBiet.ContainsKey("benhtreduoi19"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhtreduoi19"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1629,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1632	1	XML1	GIOI_TINH	Mã Bệnh Tuổi Dậy Thì chỉ phù hợp với Thiếu niên từ 8 - 19 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if ((tuoiBN < 8d || tuoiBN > 19d) && dmDacBiet.ContainsKey("benhdaythi"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhdaythi"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1632,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1631	1	XML7	GIOI_TINH	Mã Bệnh Trẻ em chỉ phù hợp với Trẻ em trên 1 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (tuoiBN < 1d && dmDacBiet.ContainsKey("benhtretren1"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhtretren1"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1631,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1633	1	XML9	GIOI_TINH	Mã Bệnh Người Trưởng Thành chỉ phù hợp với Bệnh nhân trên 15 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (tuoiBN < 15d && dmDacBiet.ContainsKey("benhlontren15"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhlontren15"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1633,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }
        //    //1634	1	XML10	GIOI_TINH	Mã Bệnh Người Lớn chỉ phù hợp với Bệnh nhân trên 30 tuổi (Căn cứ QĐ 4400/QĐ-BYT ngày 23/10/2020)
        //    if (tuoiBN < 30d && dmDacBiet.ContainsKey("benhlontren30"))
        //    {
        //        if (Uti.CodeMatch(allMaBenhs, dmDacBiet["benhlontren30"]))
        //        {
        //            xml1.XML1Errors.Add(new XML1Error()
        //            {
        //                //XEXML1ID =xml1.XML1ID,
        //                XEErrorCode = 1634,
        //                XEErrorRow = xml1.STT
        //            });
        //        }
        //    }

        //}
        //#endregion

        ////public async Task ValidateFiles(string currentHospitalId = "", string hospitalNameAPI = "", string currentUserId = "", string currentMaCSKCB = "", string xml1IdsLe = "", string fileNames = "", bool isAPICheck = false, bool isFromHis = false)
        ////{
        ////    //Linhist added 28/03/22 han che upload voi email (Haimaster comment 29.2.2022 => Bo doan code nay
        ////    //string currEmail = _httpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ////    //if (currEmail == "khoakhambenh3@gmail.com") return;

        ////    string currentRole = "";
        ////    Hospital currentHospital = new Hospital();
        ////    List<XML1Error> errors = new List<XML1Error>();
        ////    if (string.IsNullOrEmpty(currentMaCSKCB))
        ////    {
        ////        currentHospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;
        ////        currentUserId = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Sid).Value;
        ////        currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
        ////        currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
        ////    }

        ////    if (!string.IsNullOrEmpty(currentHospitalId))
        ////    {
        ////        currentHospital = await _hospitalRepository.GetItem(x => x.Id == currentHospitalId);
        ////    }
        ////    else if (!string.IsNullOrEmpty(currentMaCSKCB))
        ////    {
        ////        currentHospital = await _hospitalRepository.GetItem(x => x.MA_CSKCB == currentMaCSKCB);
        ////    }

        ////    TimePeriod gio_Hanh_Chinh_BV = new TimePeriod(currentHospital.OfficeTime);

        ////    #region Get xml
        ////    IEnumerable<XML01> files = new List<XML01>();
        ////    if (xml1IdsLe == "" && fileNames == "")
        ////    {
        ////        files = await _hosoRepository.GetAllListXmlReference(currentHospitalId);
        ////    }
        ////    else if (fileNames != "")
        ////    {
        ////        if (isAPICheck)
        ////        {
        ////            //List<string> allIdLe = new List<string>();
        ////            //foreach (string id in fileNames.Split(',').ToList())
        ////            //{
        ////            //    allIdLe.Add(id);
        ////            //}
        ////            //var files1 = await _hosoAPIRepository.GetAllListXmlReferenceLeString(allIdLe);
        ////            //XML1_API xml1 = files1.FirstOrDefault();
        ////            //XML1 itemData = _mapper.Map<XML1>(xml1);
        ////            //foreach (XML2_API xml2 in xml1.XML2_APIs)
        ////            //{
        ////            //    XML2 itemData2 = _mapper.Map<XML2>(xml2);
        ////            //    itemData2.XML1 = itemData;
        ////            //    itemData.XML2s.Add(itemData2);
        ////            //}
        ////            //foreach (XML3_API xml3 in xml1.XML3_APIs)
        ////            //{
        ////            //    XML3 itemData3 = _mapper.Map<XML3>(xml3);
        ////            //    itemData3.XML1 = itemData;
        ////            //    itemData.XML3s.Add(itemData3);
        ////            //}
        ////            //List<XML1> itemDataList = new List<XML1>();
        ////            //itemDataList.Add(itemData);
        ////            //files = (IEnumerable<XML1>)itemDataList;
        ////        }
        ////        else
        ////        {
        ////            List<string> allIdLe = new List<string>();
        ////            foreach (string id in fileNames.Split(',').ToList())
        ////            {
        ////                allIdLe.Add(id);
        ////            }
        ////            files = await _hosoRepository.GetAllListXmlReferenceLeString(allIdLe);
        ////        }

        ////    }
        ////    else
        ////    {
        ////        //List<int> allIdLe = new List<int>();
        ////        //foreach (string id in xml1IdsLe.Split(',').ToList())
        ////        //{
        ////        //    allIdLe.Add(int.Parse(id));
        ////        //}
                
        ////        try
        ////        {
        ////            Uti.HandleCronJobLog(_logger, $"cua1:"    + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.ValidateFiles)); ;


                    
        ////            //DateTime d0 = DateTime.Now;
        ////            //IEnumerable<XML1_Temp> itemData2 = await _hosoRepository_Temp.GetAllListXmlReferenceLe(300, currentHospital.MA_CSKCB);
        ////            //TimeSpan tsb = DateTime.Now - d0;
        ////            //Uti.HandleCronJobLog(_logger, $"Cuacua. GetAllListXmlReferenceLe Files: {itemData2.Count()}  MA_CSKCB {currentHospital.MA_CSKCB} Validated - Total time: {tsb.TotalMilliseconds}", nameof(HoSoManagementService130.ValidateFiles));


        ////            //foreach (var item in itemData2)
        ////            //{
        ////            //    XML1 itemData = _mapper.Map<XML1>(item);
        ////            //    ((List<XML1>)files).Add(itemData);

        ////            //    foreach (var x2 in itemData.XML2s)
        ////            //    {
        ////            //        x2.MA_CSKCB = itemData.MA_CSKCB;
        ////            //    }
        ////            //    foreach (var x3 in itemData.XML3s)
        ////            //    {
        ////            //        x3.MA_CSKCB = itemData.MA_CSKCB;
        ////            //    }
        ////            //    foreach (var x4 in itemData.XML4s)
        ////            //    {
        ////            //        x4.MA_CSKCB = itemData.MA_CSKCB;
        ////            //    }
        ////            //    foreach (var x5 in itemData.XML5s)
        ////            //    {
        ////            //        x5.MA_CSKCB = itemData.MA_CSKCB;
        ////            //    }
        ////            //}

        ////            ////files = 
                    
        ////            //Uti.HandleCronJobLog(_logger, $"cua2"    + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.ValidateFiles)); ;

        ////        }
        ////        catch (Exception ex)
        ////        {

        ////            Uti.HandleInternalServerError(
        ////           ex, "GetAllListXmlReferenceLe failed cua:", _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.ValidateFiles)}");
        ////            Uti.HandleCronJobLog(_logger, $"Validate job faled cua:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.ValidateFiles));

        ////        }

        ////    }

        ////    IEnumerable<HDSDThuoc> validateRuleThuoc = new List<HDSDThuoc>();
        ////    IEnumerable<HDSDDichVu> validateRuleDichVu = new List<HDSDDichVu>();
        ////    IEnumerable<DMThuoc> _DMThuoc = new List<DMThuoc>();
        ////    IEnumerable<DMVTYT> _DMVTYT = new List<DMVTYT>();
        ////    IEnumerable<DMDichVu> _DMDichVu = new List<DMDichVu>();
        ////    IEnumerable<DMBacSi> _DMBacSi = new List<DMBacSi>();
        ////    IEnumerable<DMYHDTICD10> _DMYhdt = new List<DMYHDTICD10>();
        ////    IEnumerable<DMDacBiet> _DMDacBiet = new List<DMDacBiet>();
        ////    IEnumerable<DMBVTuyenTinh> _DMTuyenTinhs = new List<DMBVTuyenTinh>();
        ////    Dictionary<string, string> _dicDMDB = new Dictionary<string, string>();

        ////    try
        ////    {
        ////        if (!isAPICheck && !isFromHis)
        ////        {

        ////            DateTime dd = DateTime.Now;
        ////            validateRuleThuoc = await _HDSDThuocRepository.GetActiveRules(currentMaCSKCB, currentRole.Equals(RoleConstant.MASTER));
        ////            validateRuleDichVu = await _HDSDDichVuRepository.GetListAsync(x => true.Equals(x.HIEU_LUC));

        ////            //getdanh muc 4 cai
        ////            _DMThuoc = await _DMThuocRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB);
        ////            _DMVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB);
        ////            _DMDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB);
        ////            _DMBacSi = await _DMBacSiRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB);

        ////            //Linhist them get danh muc Benh YHDT map ICD10
        ////            _DMYhdt = await _DMYHDTRepository.GetListAsync();

        ////            //Linhist added 17/03/22
        ////            //Lay thong tin DM Dac biet
        ////            _DMDacBiet = await _DMDacBietRepository.GetListAsync();

        ////            //Linhist added 30/12/22
        ////            _DMTuyenTinhs = await _DMBVTuyenTinhRepository.GetListAsync();

        ////            TimeSpan tsb = DateTime.Now - dd;
        ////            Uti.HandleCronJobLog(_logger, $"S110. Haimaster Check API get Data:{currentMaCSKCB} Validated - Total time: {tsb.TotalMilliseconds}", nameof(HoSoManagementService130.ValidateFiles));
        ////        }
        ////        else
        ////        {
        ////            DateTime dd1 = DateTime.Now;
        ////            validateRuleThuoc = await _HDSDThuocRepository.GetActiveRules(currentMaCSKCB, currentRole.Equals(RoleConstant.MASTER));
        ////            validateRuleDichVu = await _HDSDDichVuRepository.GetListAsync(x => true.Equals(x.HIEU_LUC));
        ////            //var alo = files.SelectMany(y => y.XML2s).Select(y => y.MA_THUOC);//    .SelectMany(y => y.MA_THUOC).ToList();

        ////            var maThuocNeed = files.SelectMany(y => y.XML02s).Select(y => y.MA_THUOC).ToList();
        ////            _DMThuoc = await _DMThuocRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && maThuocNeed.Contains(x.MA_THUOC));

        ////            //Linhist changed 03/01/23
        ////            //Fix loi filter truong hop MA_AX & MA_NHOM_AX rong, MA_DV lay tu xml3 ko phai xml4, lay MA_VT trong xml3 ko phai MA_DV
        ////            //var maDMVTYTNeed = files.SelectMany(y => y.XML3s).Select(y => y.MA_DICH_VU).ToList();
        ////            //_DMVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && maDMVTYTNeed.Contains(x.MA_NHOM_AX));
                    
                    
        ////            //var maDichVuNeed = files.SelectMany(y => y.XML4s).Select(y => y.MA_DICH_VU).ToList();
        ////            //_DMDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && maDichVuNeed.Contains(x.MA_AX));

        ////            var maDMVTYTNeed = files.SelectMany(y => y.XML03s).Select(y => y.MA_VAT_TU).ToList();
        ////            _DMVTYT = await _DMVTYTRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.MA_VAT_TU != null && x.MA_VAT_TU != "" && maDMVTYTNeed.Contains(x.MA_VAT_TU));


        ////            var maDichVuNeed = files.SelectMany(y => y.XML03s).Select(y => y.MA_DICH_VU).ToList();
        ////            _DMDichVu = await _DMDichVuRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && x.MA_DICH_VU != null && x.MA_DICH_VU != "" && maDichVuNeed.Contains(x.MA_DICH_VU));

        ////            //Cua- chir la
        ////            //var maBacSiNeed = files.SelectMany(y => y.XML2s).Select(y => y.MA_BAC_SI).ToList();
        ////            //_DMBacSi = await _DMBacSiRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB && maBacSiNeed.Contains(x.MACCHN));

        ////            _DMBacSi = await _DMBacSiRepository.GetListAsync(x => x.MA_CSKCB == currentMaCSKCB);


        ////            _DMYhdt = await _DMYHDTRepository.GetListAsync();


        ////            _DMDacBiet = await _DMDacBietRepository.GetListAsync();

        ////            //Linhist added 30/12/22
        ////            _DMTuyenTinhs = await _DMBVTuyenTinhRepository.GetListAsync();

        ////            TimeSpan tsa = DateTime.Now - dd1;
        ////            Uti.HandleCronJobLog(_logger, $"S120. Haimaster Check API get Data{currentMaCSKCB}: Validated - Total time: {tsa.TotalMilliseconds}", nameof(HoSoManagementService130.ValidateFiles));
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        //request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        ////        Uti.HandleInternalServerError(
        ////            ex, "Validate job failed:", _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.ValidateFiles)}");
        ////        Uti.HandleCronJobLog(_logger, $"Validate job faled:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.ValidateFiles));

        ////        //Console.WriteLine("Validate job failed :" + ex.ToString());

        ////    }

        ////    //string _dbCTRMI = "";
        ////    //string _dbPhauThuat = "";
        ////    //string _dbThuocVT = "";
        ////    string _dbCongKham = "";
        ////    //string _dbXetNghiem = "";
        ////    //string _dbThuThuat = "";

        ////    ////Linhist added 12/05/22 them thuoc & dv vuottuyen2
        ////    //string _dbThuocVT2 = "";
        ////    //string _dbDvuVT2 = "";

        ////    ////Linhist added 17/05/22 them giuong kham ngoai
        ////    //string _dbGiuongNgoai = "";

        ////    ////Linhist added 04/11/22 them giuong hoi suc cap cuu
        ////    //string _dbGiuongCapCuu = "";

        ////    //Linhist added 24/05/22 tao list cho DM Kham Chuyen Nganh
        ////    Dictionary<string, string> dmKhamCNs = new Dictionary<string, string>();
        ////    DateTime dv = DateTime.Now;

        ////    foreach (var db in _DMDacBiet)
        ////    {
        ////        //Linhist changed 08/08/23 chuyen DMDacBiet thanh dictionary
        ////        //switch (db.DMDacBietName?.ToLower())
        ////        //{
        ////        //    case "ctrmi": _dbCTRMI = db.DMDacBietValue; break;
        ////        //    case "phauthuat": _dbPhauThuat = db.DMDacBietValue; break;
        ////        //    case "thuocvuottuyen": _dbThuocVT = db.DMDacBietValue; break;
        ////        //    case "congkham": _dbCongKham = db.DMDacBietValue; break;
        ////        //    case "xetnghiem": _dbXetNghiem = db.DMDacBietValue; break;
        ////        //    case "thuthuat": _dbThuThuat = db.DMDacBietValue; break;
        ////        //    case "thuocvuottuyen2": _dbThuocVT2 = db.DMDacBietValue; break;
        ////        //    case "dvthuocvuottuyen2": _dbDvuVT2 = db.DMDacBietValue; break;
        ////        //    case "giuongkhamngoai": _dbGiuongNgoai = db.DMDacBietValue; break;
        ////        //    case "giuonghoisuccapcuu": _dbGiuongCapCuu = db.DMDacBietValue; break;
        ////        //    case "dmtreemkocheck": break;

        ////        //    //Linhist added 24/05/22 tao list cho DM Kham Chuyen Nganh 
        ////        //    default:
        ////        //        //Linhist changed 06/10/22 kiem tra ro hon DMDacBietName bat dau bang so thi moi add vao mang kiem tra
        ////        //        //if (!db.DMDacBietName.Contains("-bak"))
        ////        //        if (db.DMDacBietName != null && !db.DMDacBietName.Contains("-bak") && char.IsDigit(db.DMDacBietName[0]))
        ////        //        {
        ////        //            dmKhamCNs.Add(db.DMDacBietValue, db.DMDacBietName);
        ////        //        }
        ////        //        break;
        ////        //}

        ////        if (db.DMDacBietName == null && db.DMDacBietName.Contains("-bak")) continue;
        ////        if (char.IsDigit(db.DMDacBietName[0]))
        ////        {
        ////            dmKhamCNs.Add(db.DMDacBietValue, db.DMDacBietName);
        ////        }
        ////        else
        ////        {
        ////            _dicDMDB.Add(db.DMDacBietName?.Trim().ToLower(), db.DMDacBietValue);
        ////        }
        ////    }
        ////    _DMDacBiet = null;
        ////    _dbCongKham = _dicDMDB.ContainsKey("congkham") ? _dicDMDB["congkham"] : "";

        ////    IEnumerable<DMErrorMHTC> userErrList = await _dmErrorMHTCRepo.GetListAsync(x => x.MA_CSKCB.Equals(currentMaCSKCB), null, null);


        ////    //Linhist added 27/05/22
        ////    IEnumerable<ChiDinhDongThoi> validateRuleDongThoi = await _ChiDinhDongThoiRepository.GetListAsync(x => true.Equals(x.HIEU_LUC));
        ////    string allMaDVRuleDTs = string.Join(';', validateRuleDongThoi.Select(x => x.MA_DICH_VU).ToArray());
        ////    string allMaDVKhacRuleDTs = string.Join(';', validateRuleDongThoi.Select(x => x.DICH_VU_KHAC).ToArray());
        ////    string allMAVTRuleDTs = string.Join(';', validateRuleDongThoi.Select(x => x.VAT_TU_BHYT).ToArray());

        ////    List<XML3> allDichVuDTs = new List<XML3>();
        ////    List<XML3> allDvKhacDTs = new List<XML3>();
        ////    List<XML3> allVatTuDTs = new List<XML3>();
        ////    string allDVhaimaster = "";

        ////    //Linhist added 08/06/22
        ////    IEnumerable<PhacDoDieuTri> validateRulePhacDo = await _PhacDoDieuTriRepository.GetListAsync();

        ////    IEnumerable<XML1> xml3CungNgay = new List<XML1>();


        ////    //Linhist added 14/11/22
        ////    //1250	1	XML3	MA_DICH_VU	Bệnh nhân Ngoại trú nhưng kê giường nội trú
        ////    //1251  1   XML3    MA_DICH_VU  Kê giường ngoại khoa sai TT39/ 2018
        ////    string[] allPhauThuats = await GetDMPhauThuat();
        ////    string[] allGiuongNgoais = null;// await GetDMGiuong(currentMaCSKCB);
        ////    string allGiuongNois = allGiuongNgoais[4];
        ////    string allGiuongCapCuu = allGiuongNgoais[5];
        ////    System.Array.Resize(ref allGiuongNgoais, 4);

        ////    List<int> allPhauThuatGiuongLoais = new List<int>();
        ////    TimeSpan tsv = DateTime.Now - dv;
        ////    Uti.HandleCronJobLog(_logger, $"S130. Haimaster Check API get Data{currentMaCSKCB}: Get All Data - Total time: {tsv.TotalMilliseconds}", nameof(HoSoManagementService130.ValidateFiles));
        ////    #endregion

        ////    #region Validate XML
        ////    try
        ////    {
        ////        //Linhis removed 23/06/23 - Khong su dung nua vi ham CheckThe_InValidate o tren da save The moi roi
        ////        //Linhist added 20/02/23 - Insert thong tin CheckThe de kiem tra thong tuyen
        ////        if (!isAPICheck)
        ////        {
        ////            await SaveXML1_CheckThe((List<XML1>)files);
        ////        }

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        Uti.HandleInternalServerError(ex, false, _logger, "Validate");

        ////        var LineNumber = 0;
        ////        if ((new StackTrace(ex, true)).GetFrames().Any())
        ////        {
        ////            LineNumber = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
        ////        }
        ////        await _ImportErrorRepository.InsertAsync(
        ////        new ImportError
        ////        {
        ////            ImportErrorFile = "Validate XML",
        ////            ImportErrorAt = ex.GetType().ToString(),
        ////            ImportErrorMessage = ex.Message.ToString() + "Validate XML",
        ////            ImportErrorDate = System.DateTime.Now,
        ////            ImportErrorMHTCID = LineNumber

        ////        }); ;
        ////        await _unitOfWork.CommitAsync();
        ////    }
        ////    #endregion
        ////}

        #endregion

        public async Task DeleteFiles()
        {
            var currentHospitalId = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value;
            try
            {
                _hosoRepository.DeleteListByFilter(x => x.HospitalID.Equals(currentHospitalId) && x.CreateDate.HasValue && x.CreateDate.Value.Date == System.DateTime.Now.Date);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(
                       ex, false, _logger, "Delete");
                //await _ImportErrorRepository.InsertAsync(
                //    new ImportError
                //    {
                //        ImportErrorFile = "Delete",
                //        ImportErrorAt = ex.GetType().ToString(),
                //        ImportErrorMessage = ex.Message.ToString(),
                //        ImportErrorDate = System.DateTime.Now

                //    });
                await _unitOfWork.CommitAsync();

            }
        }

        public string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public string GetFirstMinus2(string source)
        {
            if (2 >= source.Length)
                return source;
            return source.Substring(0, source.Length - 2);
        }

        #region Verify XML - Linhist removed old code 29/03/25
        //private async Task<ResponseModel<XML1VerifyModel>> GetXML1NeedVerify()
        //{
        //    var res = new ResponseModel<XML1VerifyModel>(HttpStatusCode.OK, "", new XML1VerifyModel());
        //    //try
        //    //{
        //    //    //set all date to todate
        //    //    var hospitalListAll = (await _hospitalRepository.GetListAsync()).Where(x => 
        //    //    x.DateTimeNow == null || x.DateTimeNow?.Date != System.DateTime.Now.Date 
        //    //    //x.MA_CSKCB=="27009"
        //    //    ).ToList();
        //    //    //HAIMASTER
        //    //    if (hospitalListAll != null && hospitalListAll.Count() > 0)
        //    //    {
        //    //        foreach (Hospital h in hospitalListAll)
        //    //        {
        //    //            h.DateTimeNow = System.DateTime.Now;
        //    //            h.FileAPICount = 0;

        //    //        }
        //    //    }

        //    //    if (hospitalListAll != null && hospitalListAll.Count() > 0)
        //    //    {
        //    //        _hospitalRepository.UpdateList(hospitalListAll);
        //    //        await _hospitalRepository.SaveChangesAsync();
        //    //    }
        //    //    var hospitalList = (await _hospitalRepository.GetListAsync())
        //    //        .Where(x => 
        //    //        !string.IsNullOrWhiteSpace(x.Username) && !string.IsNullOrWhiteSpace(x.Password) && x.MaxFilePerDate > x.FileAPICount
        //    //        //x.MA_CSKCB == "27009" || x.MA_CSKCB == "31313" || x.MA_CSKCB == "31338" || x.MA_CSKCB == "31336"
        //    //        ).ToList();
        //    //    var maCSKCBList = hospitalList.Select(x => x.MA_CSKCB);

        //    //    var xml1ListUpdate = new List<XML1>();
        //    //    var xml1ListVerify = new List<XML1>();
        //    //    // filter by MA_CSKCB valid
        //    //    var maxItemCanVerifyInTime = _appSettings.BackgroundServices.VerifyXML.MinuteVerifyTime * 60;
        //    //    var pageSizeTemp = maxItemCanVerifyInTime - (maxItemCanVerifyInTime * _appSettings.BackgroundServices.VerifyXML.PercentItemInTime / 100);
        //    //    var pageSize = (int)Math.Floor((decimal)pageSizeTemp);

        //    //    //haimaster: Doan nay phai xem lai, vi 20/5/2022 van con recode moi co MHTCID is null
        //    //    var allXML1 = await _hosoRepository.GetListPaginationAsync(0, pageSize, x => maCSKCBList.Contains(x.MA_CSKCB) && (x.CHECKED == 0 || x.CHECKED == 2) && x.MHTCID != null && x.CreateDate < DateTime.Now.AddHours(-1) , x => x.OrderByDescending(o => o.NGAYLAP));
        //    //    if (!allXML1.Items.Any()) return res;
        //    //    // filter by fileName and Checked
        //    //    var groupByFileNameList = allXML1.Items.GroupBy(x => x.FileName);
        //    //    foreach (var groupByFileName in groupByFileNameList)
        //    //    {
        //    //        /**
        //    //         * 1/ check group only 1 item and has in NEED_VERIFIED_LIST -> add verify list
        //    //         * 2/ group more 1 and at least checked = VERIFIED -> add update list
        //    //         * 3/ group more 1 and don't have in IGNORE_VERIFIED_LIST and has in NEED_VERIFIED_LIST -> add verify list
        //    //         **/
        //    //        //haimaster: Doan nay kiem tra neu ten la la duy nhat, va check =0 => cho ham list xml1ListVerify
        //    //        if (groupByFileName.Count() == 1 && groupByFileName.Any(y => FileStatusVerification.NEED_VERIFIED_LIST.Contains(y.CHECKED.Value)))
        //    //        {
        //    //            xml1ListVerify.Add(groupByFileName.FirstOrDefault());
        //    //        }
        //    //        //haimaster: Checked =2 nghia la gi???  khong thay check trang thai =2     public const int EXTERNAL_API_HAS_PROBLEM = 2;
        //    //        //doan nay kiem tra neu nhieu hon 1 files name (Trung ten files) => kiem tra xem check =1 va da kiem tra file nao chua
        //    //        else if (groupByFileName.Count() > 1 && groupByFileName.Any(y => y.CHECKED.Value.Equals(FileStatusVerification.VERIFIED)))
        //    //        {
        //    //            //haimaster: chi lay nhung file chua kiem tra, cho vao listUpdate
        //    //            var listUpdate = groupByFileName.Where(x => FileStatusVerification.NEED_VERIFIED_LIST.Contains(x.CHECKED.Value));

        //    //            //Linhist removed log 21/05/22
        //    //            //string x1IDUpdateshaimaster = string.Join(",", listUpdate.Select(x => x.XML1ID).ToArray());
        //    //            //Uti.HandleCronJobLog(_logger, $"All list Update: {x1IDUpdateshaimaster}", nameof(HoSoManagementService.GetXML1NeedVerify));

        //    //            if (!listUpdate.Any()) continue;
        //    //            foreach (var item in listUpdate)
        //    //            {
        //    //                // gan trang thai check =3 (khong co trong CSDL => Chua bao gio chay vao day)         public const int DUPLICATE_VERIFIED_NAME = 3;
        //    //                item.CHECKED = FileStatusVerification.DUPLICATE_VERIFIED_NAME;
        //    //                xml1ListUpdate.Add(item);
        //    //            }
        //    //        }
        //    //        //haimaster: Doan nay van kiem tra co files trung ten va co checked =0 va khong chua gia tri check =1
        //    //        else if (groupByFileName.Count() > 1 && groupByFileName.Any(y => FileStatusVerification.NEED_VERIFIED_LIST.Contains(y.CHECKED.Value) && !FileStatusVerification.IGNORE_VERIFIED_LIST.Contains(y.CHECKED.Value)))
        //    //        {
        //    //            ////Linhist removed log 21/05/22
        //    //            //: chi add nhung files kiem tra co check =0
        //    //            xml1ListVerify.Add(groupByFileName.FirstOrDefault(x => FileStatusVerification.NEED_VERIFIED_LIST.Contains(x.CHECKED.Value)));
        //    //        }
        //    //    }

        //    //    //Linhist removed log 21/05/22
        //    //    //string x1IDVerifys = string.Join(",", xml1ListVerify.Select(x => x.XML1ID).ToArray());
        //    //    //string x1IDUpdates = string.Join(",", xml1ListUpdate.Select(x => x.XML1ID).ToArray());

        //    //    //Linhist removed log 21/05/22
        //    //    //// Khong he co log nay, chua bao gio chay den day??? Hay LInh moi Viet
        //    //    //Uti.HandleCronJobLog(_logger, $"All list Verify: {x1IDVerifys}", nameof(HoSoManagementService.GetXML1NeedVerify));
        //    //    //Uti.HandleCronJobLog(_logger, $"All list Update: {x1IDUpdates}", nameof(HoSoManagementService.GetXML1NeedVerify));


        //    //    res.Data = new XML1VerifyModel
        //    //    {
        //    //        XML1s = xml1ListVerify.OrderBy(x => x.MA_CSKCB).ToList(),
        //    //        Hospitals = hospitalList
        //    //    };
        //    //    //haimaster: Neu list update <> rong =>chay update truoc // khong bao gio chay ham nay
        //    //    if (xml1ListUpdate != null && xml1ListUpdate.Any()) _ = UpdateXml1AfterVerified(xml1ListUpdate);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    res = Uti.HandleInternalServerError(
        //    //        ex, new XML1VerifyModel(), _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.GetXML1NeedVerify)}");
        //    //    Uti.HandleCronJobLog(_logger, $"Validate job faled: " + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.GetXML1NeedVerify));

        //    //}

        //    return res;
        //}
        //private async Task UpdateXml1AfterVerified(IEnumerable<XML1> xml1s)
        //{
        //    //var khoi = "";
        //    //try
        //    //{
        //    //    if (xml1s == null) return;

        //    //    //haimaser teo nho LInh check duplicate xml1id o cho nay xem ntn


        //    //    //Linhist removed log 21/05/22
        //    //    // Uti.HandleCronJobLog(_logger, $"B8 UpdateXml1AfterVerified 20/05/2022 vao ham Update XML1 After Verified : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));

        //    //    //_hosoRepository.UpdateList(xml1s); - cua rem

        //    //    //Linhist added 04/08/22 xoa toan bo error thong tuyen da co truoc khi insert moi
        //    //    //List<int?> xml1Ids = xml1s.Select(x => (int?)x.XML1ID).ToList();

        //    //    //var erThongtuyen = await _dmErrorRepository.GetListAsync(e => e.ErrorType == 2);
        //    //    //List<int?> errCodes = erThongtuyen.Select(e => e.ErrorCode).ToList();

        //    //    //await _xml1ErrorRepository.DeleteListByFilterAsync(x => xml1Ids.Contains(x.XEXML1ID.Value) && errCodes.Contains(x.XEErrorCode));

        //    //    //haimaster
        //    //    //Linhist removed log 21/05/22
        //    //    //Uti.HandleCronJobLog(_logger, $"B9 UpdateXml1AfterVerified 20/05/202220/05/2022 Chay qua ham UpdateList : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));

        //    //    List<XML1Error> errors = new List<XML1Error>();
        //    //    var allMaCSKCBs = xml1s.Select(x => x.MA_CSKCB).ToArray();

        //    //    IEnumerable<DMErrorMHTC> userErrList = await _dmErrorMHTCRepo.GetListAsync(x => allMaCSKCBs.Contains(x.MA_CSKCB), null, null);

        //    //    //Linhist added 03/11/22
        //    //    //Ghi log danh sach XML1 bi trung ID trong list xml1s
        //    //    string idDuds = "";
        //    //    var dublicates = xml1s.GroupBy(x => x.XML1ID, (k, g) => new { XML1ID = k, Count = g.Count() }).Where(g => g.Count > 1);
        //    //    if (dublicates.Any())
        //    //    {
        //    //        foreach (var g in dublicates)
        //    //        {
        //    //            idDuds += $"{g.XML1ID}, ";
        //    //        }
        //    //        Uti.HandleCronJobLog(_logger, $"UpdateXml1AfterVerified 03/11: Co {dublicates.Count()} ban ghi XML1 bi trung [XML1ID] truoc khi check thong tuyen: {idDuds}", nameof(HoSoManagementService130.UpdateXml1AfterVerified));
        //    //    }

        //    //    //Linhist added 03/11/22
        //    //    //Loai bo XML1 bi trung ID trong list xml1s
        //    //    //var skipDubs = xml1s.GroupBy(x => x.XML1ID, (k, g) => g.FirstOrDefault());
        //    //    //int icntDubs = xml1s.Count() - skipDubs.Count();
        //    //    //if (icntDubs > 0)
        //    //    //{
        //    //    //    Uti.HandleCronJobLog(_logger, $"UpdateXml1AfterVerified 03/11: Co {icntDubs} ban ghi XML1 bi trung [XML1ID] truoc khi check thong tuyen.", nameof(HoSoManagementService.UpdateXml1AfterVerified));
        //    //    //    //Day la doan de xoa trung xml1
        //    //    //    //xml1s = skipDubs.ToList();
        //    //    //}

        //    //    foreach (XML1 xml1 in xml1s)
        //    //    {
        //    //        //Linhist removed log 21/05/22
        //    //        //Uti.HandleCronJobLog(_logger, $"B10 UpdateXml1AfterVerified 20/05/202220/05/2022 truoc khi chay verify thong tuyen cua Linh : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));
        //    //        Verify_Thong_Tuyen(xml1, errors);
        //    //        //Linhist removed log 21/05/22
        //    //        //Uti.HandleCronJobLog(_logger, $"B11 UpdateXml1AfterVerified 20/05/202220/05/2022 sau khi chay verify thong tuyen cua Linh : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));

        //    //        //Linhist changed 17/04/23
        //    //        ////Linhist added 17/10/22
        //    //        ////Kiem tra so loi sai theo so luong error
        //    //        //if (errors.Any(e => e.XEXML1ID == xml1.XML1ID))
        //    //        //{
        //    //        //    xml1.Creator = 2;
        //    //        //}

        //    //        var errCodes = userErrList.Where(e => e.MA_CSKCB == xml1.MA_CSKCB).Select(e => new { e.EMErrorCode, e.EMEnabled });
        //    //        errors = errors.Where(x => errCodes.Any(e => e.EMErrorCode == x.XEErrorCode)).ToList();

        //    //        //Filter chi error ko canh bao theo vien
        //    //        int fileErrorsCount = errors.Count(x => errCodes.Any(e => e.EMErrorCode == x.XEErrorCode && e.EMEnabled == true));

        //    //        //Kiem tra so loi sai theo so luong error
        //    //        //Neu Ho So co loi thi kiem tra so luong loi: --> 2: Ho So co ca loi & canh bao, 3: Ho So chi co canh bao ko phai loi
        //    //        if (xml1.Creator == 2 || fileErrorsCount > 0)
        //    //        {
        //    //            xml1.Creator = 2;
        //    //        }
        //    //        else if (errors.Any())
        //    //        {
        //    //            xml1.Creator = 3;
        //    //        }

        //    //        #region Linhist removed 11/05/22 thay the bang function kiem tra thong tuyen
        //    //        ////Linhist added 29/03/22
        //    //        ////218 218 1   XML1 MA_DKBD Thẻ sai nơi đăng ký KCB ban đầu LOI_THE
        //    //        //if (!string.IsNullOrEmpty(xml1.MA_DKBD) && !string.IsNullOrEmpty(xml1.MHTCID) && !Uti.IsEqual(xml1.MA_DKBD, xml1.MHTCID))
        //    //        //{
        //    //        //    file.XML1Errors.Add(new XML1Error()
        //    //        //    {
        //    //        //        //XEXML1ID =xml1.XML1ID,
        //    //        //        XEErrorCode = 218,
        //    //        //        XEErrorRow = xml1.STT,

        //    //        //    });
        //    //        //}

        //    //        //switch (xml1.DanhSachLoi)
        //    //        //{
        //    //        //    case "000":

        //    //        //        break;
        //    //        //    case "001":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 222,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "002":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 223,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "051":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 209,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "010":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 210,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "070":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 216,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "100":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 221,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "052":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 224,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "053":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 225,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "050":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 226,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "060":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 227,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "401":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 228,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "101":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 229,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "110":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 230,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "120":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 231,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "121":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 232,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "123":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 234,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "124":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 235,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "130":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 236,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "061":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 360,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "003":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 361,
        //    //        //            XEErrorRow = xml1.STT,

        //    //        //        });
        //    //        //        break;
        //    //        //    case "004":
        //    //        //        file.XML1Errors.Add(new XML1Error()
        //    //        //        {
        //    //        //            //XEXML1ID =xml1.XML1ID,
        //    //        //            XEErrorCode = 362,
        //    //        //            XEErrorRow = xml1.STT,


        //    //        //        });
        //    //        //        break;
        //    //        //    default:

        //    //        //        break;
        //    //        //}
        //    //        #endregion
        //    //    }

        //    //    //LOI THE CAN CHECK TAY
        //    //    //213 213 1   XML1 GT_THE_TU   Thẻ có giá trị khi đang nằm viện    LOI_THE
        //    //    //214 214 1   XML1 MA_THE  Mã thẻ không có trong dữ liệu thẻ   LOI_THE
        //    //    //380 380 2   XML1 GT_THE_DEN  Thẻ đã báo giảm. Ngừng tham gia trong thời gian nằm viện    LOI_THE


        //    //    //Linhist added 03/11/22
        //    //    //Ghi log danh sach XML1 bi trung ID trong list xml1s
        //    //    idDuds = "";
        //    //    dublicates = xml1s.GroupBy(x => x.XML1ID, (k, g) => new { XML1ID = k, Count = g.Count() }).Where(g => g.Count > 1);
        //    //    if (dublicates.Any())
        //    //    {
        //    //        foreach (var g in dublicates)
        //    //        {
        //    //            idDuds += $"{g.XML1ID}, ";
        //    //        }
        //    //        Uti.HandleCronJobLog(_logger, $"UpdateXml1AfterVerified 03/11: Co {dublicates.Count()} ban ghi XML1 bi trung [XML1ID] sau khi check thong tuyen: {idDuds}", nameof(HoSoManagementService130.UpdateXml1AfterVerified));
        //    //    }

        //    //    //Linhist added 03/11/22
        //    //    //Loai bo XML1 bi trung ID trong list xml1s
        //    //    //var skipDubs1 = xml1s.GroupBy(x => x.XML1ID, (k, g) => g.FirstOrDefault());
        //    //    //int icntDubs1 = xml1s.Count() - skipDubs1.Count();
        //    //    //if (icntDubs1 > 0)
        //    //    //{
        //    //    //    Uti.HandleCronJobLog(_logger, $"UpdateXml1AfterVerified 03/11: Co {icntDubs1} ban ghi XML1 bi trung [XML1ID] sau khi check thong tuyen.", nameof(HoSoManagementService.UpdateXml1AfterVerified));
        //    //    //    //Day la doan de xoa trung xml1
        //    //    //    //xml1s = skipDubs.ToList();
        //    //    //}
        //    //     khoi = string.Join(",", xml1s.Select(x => x.XML1ID).ToList()); 
        //    //    _hosoRepository.UpdateList(xml1s);

        //    //    await _hosoRepository.SaveChangesAsyncNoBulk();



        //    //    _xmlErrorRepository.InsertList(errors);
        //    //    await _xml1ErrorRepository.SaveChangesAsyncNoBulk();

        //    //    //haimaster
        //    //    //Linhist removed log 21/05/22
        //    //    //Uti.HandleCronJobLog(_logger, $"B12 UpdateXml1AfterVerified 20/05/2022 sau khi Chay qua ham insert Error : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));

        //    //    //khoikhoi rem 3-11
        //    //    //await _unitOfWork.CommitAsync();

        //    //    //haimaster
        //    //    //Linhist removed log 21/05/22
        //    //    //Uti.HandleCronJobLog(_logger, $"B12 UpdateXml1AfterVerified 20/05/2022 Chay qua ham commit Async : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));

        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    Uti.HandleInternalServerError(
        //    //        ex, false, _logger, $"Trung id - {nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.UpdateXml1AfterVerified) + khoi}"  );
        //    //    Uti.HandleCronJobLog(_logger, $"Validate job faled: " + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.UpdateXml1AfterVerified));

        //    //}
        //}
        //private async Task UpdateHospitalAfterVerified(IEnumerable<Hospital> hospitals)
        //{
        //    try
        //    {
        //        if (hospitals == null) return;
        //        var updatedHospital = (await _hospitalRepository.GetListAsync()).Where(x => hospitals.Select(x => x.MA_CSKCB).ToList().Contains(x.MA_CSKCB)).ToList();
        //        bool flag = false;
        //        foreach (Hospital hos in updatedHospital)
        //        {
        //            var updatedHos = hospitals.Where(x => x.MA_CSKCB == hos.MA_CSKCB).FirstOrDefault();
        //            if (hos.AccessToken != updatedHos.AccessToken || hos.IdToken != updatedHos.IdToken)
        //            {
        //                flag = true;
        //                hos.AccessToken = updatedHos.AccessToken;
        //                hos.IdToken = updatedHos.IdToken;
        //            }


        //        }
        //        if (flag)
        //        {
        //            await _hospitalRepository.UpdateListAsync(updatedHospital);
        //            await _hospitalRepository.SaveChangesAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Uti.HandleInternalServerError(
        //            ex, false, _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.UpdateHospitalAfterVerified)}");
        //        Uti.HandleCronJobLog(_logger, $"Validate job faled: " + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.UpdateHospitalAfterVerified));

        //    }
        //}
        //private async Task<Hospital> CallApiLoginExternal(HttpClient client, string serviceUrl, Hospital hospital)
        //{
        //    //Linhist removed log 21/05/22
        //    //Uti.HandleCronJobLog(_logger, $"B3.2.1 CallExternalApiValidateXML1 20/05/2022 sau khi vao ham CallApiLoginExternal   {hospital.MA_CSKCB}    ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //    try
        //    {
        //        string hashString = EasyMD5.Hash(hospital.Password ?? "xxx");
        //        var loginBody = new StringContent(JsonConvert.SerializeObject(new
        //        {
        //            username = hospital.Username,
        //            password = hashString
        //        }), Encoding.UTF8, "application/json");
        //        Uti.HandleCronJobLog(_logger, $"Tien hanh login: " + hospital.Username.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApiLoginExternal));


        //        var signInResult = await client.PostAsync($"{serviceUrl}/api/token/take", loginBody);
        //        if (!signInResult.IsSuccessStatusCode)
        //        {
        //            Uti.HandleCronJobLog(_logger, $"Login failed: " + hospital.Username.ToString() + " " + signInResult.StatusCode.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApiLoginExternal));

        //            return null;
        //        }

        //        string loginData = await signInResult.Content.ReadAsStringAsync();
        //        var plv = JsonConvert.DeserializeObject<KQPhienLamViec>(loginData);
        //        var key = plv.APIKey;
        //        Uti.HandleCronJobLog(_logger, $"Login oke. UserName: " + hospital.Username.ToString() + "New key: " + key.access_token + " Status code:" + signInResult.StatusCode.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApiLoginExternal));


        //        hospital.AccessToken = key.access_token;
        //        hospital.IdToken = key.id_token;
        //        return hospital;
        //    }
        //    catch (Exception ex)
        //    {
        //        Uti.HandleInternalServerError(
        //            ex, hospital, _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.CallApiLoginExternal)}");
        //        Uti.HandleCronJobLog(_logger, $"CallApiLoginExternal failed: " + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApiLoginExternal));

        //        return null;
        //    }
        //}

        //private async Task<XML1> LoginExternalHandler(HttpClient client, string serviceUrl, XML1 request, Hospital hospital, List<Hospital> hospitals)
        //{
        //    try
        //    {
        //        hospital = await CallApiLoginExternal(client, serviceUrl, hospital);

        //        //Linhist removed log 21/05/22
        //        //Uti.HandleCronJobLog(_logger, $"B3.2 CallExternalApiValidateXML1 20/05/2022 sau khi goi CallApiLoginExternal   " + request.XML1ID.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //        if (hospital == null)
        //        {
        //            request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //            return request;
        //        }
        //        var indexHospital = hospitals.FindIndex(x => x.Id == hospital.Id);
        //        hospitals[indexHospital] = hospital;
        //        var item = await CallApiValidateXML1(client, serviceUrl, request, hospital);
        //        if (item == null)
        //        {
        //            request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //            return request;
        //        }
        //        return request;
        //    }
        //    catch (Exception ex)
        //    {
        //        Uti.HandleInternalServerError(
        //                ex, request, _logger, $"Loi LoginExternalHandler 2-11 { request.MA_THE} - {nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.CallExternalApiValidateXML1)}");
        //        return null;
        //    }
        //}

        //private async Task<XML1> CallApiValidateXML1(HttpClient client, string serviceUrl, XML1 request, Hospital hospital)
        //{
        //    if (request.MA_LK == "TN.2210.0063888")
        //    {
        //        var khoi = "tesst faile";

        //    }
        //    //Linhist changed 04/07/22 set lai DanhSachLoi
        //    request.DanhSachLoi = "";
        //    //Linhist changed 05/07/22 set lai MHTCID
        //    request.MHTCID = (request.MHTCID ?? "").Split(';')[0];

        //    //Linhist changed 03/08/22 kiem tra them cac thong tin GT_THE
        //    string[] maThes = request.MA_THE.Split(';');
        //    string[] gtTus = request.GT_THE_TU.Split(';');
        //    string[] gtDens = request.GT_THE_DEN.Split(';');
        //    string[] dkbds = request.MA_DKBD.Split(';');
        //    DateTime minTheTu = DateTime.MinValue;
        //    DateTime maxTheDen = DateTime.MinValue;

        //    //Linhist changed 16/05/22 sua lai try catch vao trong foreach
        //    //khoikhoi  - A linh check foreach string dum em cho request.MA_THE co 2 string thi for each keu 2 lan
        //    //Linhist changed 03/08/22 kiem tra them cac thong tin GT_THE
        //    //foreach (string maThe in request.MA_THE.Split(';').ToList())
        //    for (int i = 0; i < maThes.Length; i++)
        //    {
        //        //haimaster
        //        //Linhist removed log 21/05/22
        //        //Uti.HandleCronJobLog(_logger, $"B4.1 CallApiValidateXML1 20/05/2022 tach ma the  " + maThe.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //        //Linhist added 05/08/22 kiem tra thong tin phu ben ngoai try catch
        //        NhanLichSuKCB2018 verifyData = null;

        //        try
        //        {
        //            //Linhist removed 21/05/22 van kiem tra thong tuyen the tre em
        //            ////Linhist added 16/05/22 kiem tra the Tre em & chua di hoc
        //            //if (Check_TheTE_ChuaDiHoc(maThe, request.NGAY_SINH))
        //            //{
        //            //    request.CHECKED = FileStatusVerification.VERIFIED;
        //            //    return request;
        //            //}

        //            //haimaster
        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"VB4.2 CallApiValidateXML1 20/05/2022 qua doan kiem tra the TE  " + " " + maThe.ToString() + " " + maThe.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            string hashString = EasyMD5.Hash(hospital.Password ?? "xxx");
        //            string verifyBody = string.Format($"" +
        //                $"username={hospital.Username}" +
        //                $"&password={hashString}" +
        //                $"&token={hospital.AccessToken}" +
        //                $"&id_token={hospital.IdToken}");

        //            //haimaster
        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B4.3 CallApiValidateXML1 20/05/2022 Truoc khi goi ham kiem tra  " + " " + maThe.ToString() + " " + maThe.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //            //http://egw.baohiemxahoi.gov.vn/api/egw/KQNhanLichSuKCB2019

        //            var verifyResult = await client.PostAsJsonAsync($"{serviceUrl}/api/egw/NhanLichSuKCB2018?{verifyBody}", new theBHYT
        //            //khoikhoi link api moi
        //            //var verifyResult = await client.PostAsJsonAsync($"{serviceUrl}/api/egw/KQNhanLichSuKCB2019?{verifyBody}", new theBHYT
        //            {
        //                maThe = maThes[i],
        //                hoTen = request.HO_TEN,
        //                ngaySinh = request.NGAY_SINH?.ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo),
        //                gioiTinh = (int)request.GIOI_TINH,
        //                maCSKCB = request.MA_CSKCB,
        //                ngayBD = request.NGAY_VAO?.ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo),
        //                ngayKT = request.NGAY_RA?.ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo)
        //            });

        //            Uti.HandleCronJobLog(_logger, $"Submit len API:  Thread " + " " + maThes[i].ToString() + " " + Thread.CurrentThread.ManagedThreadId.ToString() + verifyResult.StatusCode.ToString() + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));


        //            if (verifyResult.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                Uti.HandleCronJobLog(_logger, $"Submit len API Unauthorized:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + verifyResult.StatusCode.ToString() + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));
        //                return null;
        //            }

        //            if (!verifyResult.IsSuccessStatusCode)
        //            {
        //                request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //                return request;
        //            }

        //            string result = await verifyResult.Content.ReadAsStringAsync();
        //            verifyData = JsonConvert.DeserializeObject<NhanLichSuKCB2018>(result);
        //            if (verifyData.ghiChu == "Token không đúng." && verifyData.maKetQua == "401")
        //            {
        //                Uti.HandleCronJobLog(_logger, $"Submit len API failed:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + verifyData.ghiChu.ToString() + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));
        //                return null;
        //            }

        //            #region Linhist removed 05/08/22 kiem tra lai ben ngoai try catch
        //            ////Linhist added 04/07/22 them kiem tra loi GIOI_TINH (217)
        //            ////Linhist changed 03/08/22 them kiem tra GT_THE
        //            //string sKetQua = verifyData.maKetQua + ";";

        //            ////217 217 1   XML1 GIOI_TINH   Thẻ sai giới tính   LOI_THE
        //            //if (!string.IsNullOrEmpty(verifyData.gioiTinh) && !Uti.IsMatchGender(verifyData.gioiTinh, request.GIOI_TINH))
        //            //{
        //            //    if (!request.DanhSachLoi.Contains("x217;"))
        //            //    {
        //            //        sKetQua += "x217;";
        //            //    }
        //            //}

        //            //bool fail1 = string.IsNullOrEmpty(verifyData.gtTheTuMoi);
        //            //DateTime d1 = DateTime.MinValue;
        //            //fail1 = fail1 || !DateTime.TryParseExact(verifyData.gtTheTuMoi, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1);
        //            //if (fail1 && !string.IsNullOrEmpty(verifyData.gtTheTu))
        //            //{
        //            //    fail1 = !DateTime.TryParseExact(verifyData.gtTheTu, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1);
        //            //}

        //            //if (!fail1)
        //            //{
        //            //    //1183	1183	2	XML1	GT_THE_TU	Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về
        //            //    if (d1.ToString("yyyyMMdd") != gtTus[i] && !request.DanhSachLoi.Contains("x1183;"))
        //            //    {
        //            //        sKetQua += "x1183;";
        //            //    }
        //            //    //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
        //            //    if (minTheTu == DateTime.MinValue || d1 < minTheTu)
        //            //    {
        //            //        minTheTu = d1;
        //            //    }
        //            //}

        //            //bool fail2 = string.IsNullOrEmpty(verifyData.gtTheDenMoi);
        //            //DateTime d2 = DateTime.MinValue;
        //            //fail2 = fail2 || !DateTime.TryParseExact(verifyData.gtTheDenMoi, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d2);
        //            //if (fail2 && !string.IsNullOrEmpty(verifyData.gtTheDen))
        //            //{
        //            //    fail2 = DateTime.TryParseExact(verifyData.gtTheDen, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d2);
        //            //}

        //            //if (!fail2)
        //            //{
        //            //    //1184    1184    2   XML1 GT_THE_DEN  Sai hạn thẻ: Giá trị thẻ đến sai so với giá trị trả về
        //            //    if (d2.ToString("yyyyMMdd") != gtDens[i] && !request.DanhSachLoi.Contains("x1184;"))
        //            //    {
        //            //        sKetQua += "x1184;";
        //            //    }
        //            //    //212 212 1   XML1 GT_THE_DEN  Hết hạn thẻ khi chưa ra viện LOI_THE
        //            //    if (maxTheDen == DateTime.MinValue || d2 > maxTheDen)
        //            //    {
        //            //        maxTheDen = d2;
        //            //    }
        //            //}


        //            ////213 213 1   XML1 GT_THE_TU   Thẻ có giá trị khi đang nằm viện    LOI_THE
        //            ////214 214 1   XML1 MA_THE  Mã thẻ không có trong dữ liệu thẻ   LOI_THE

        //            ////Kiem tra ket qua tra ve, neu ko co loi thi set luon DanhSachLoi la "000"
        //            //if (sKetQua == "000;" && (string.IsNullOrEmpty(request.DanhSachLoi) || request.DanhSachLoi == "000"))
        //            //{
        //            //    if (string.IsNullOrEmpty(request.DanhSachLoi))
        //            //    {
        //            //        request.DanhSachLoi = "000";
        //            //    }
        //            //}
        //            //// Neu co loi thi them ma loi vao DanhSachLoi
        //            //else
        //            //{
        //            //    request.DanhSachLoi += sKetQua;
        //            //}

        //            ////Linhist removed 04/07/22
        //            //////Linhist added 22/05/22 them truong hop kiem tra 2 the ma co loi xay ra o 1 trong 2 the
        //            //////Neu la the dau tien (DanhSachLoi = ""), ghi lai ma loi
        //            //////Neu la the tiep theo, ma da kiem tra dung the dau tien (DanhSachLoi = "000"), ghi lai loi cua the sau
        //            //////Neu the dau tien da loi (DanhSachLoi != "000") thi giu nguyen DanhSachLoi ko can ghi lai nua
        //            ////if (string.IsNullOrEmpty(request.DanhSachLoi) || request.DanhSachLoi == "000")
        //            ////{
        //            ////    request.DanhSachLoi = verifyData.maKetQua;
        //            ////}

        //            //Uti.HandleCronJobLog(_logger, $"Submit len API ngon:  Thread " + " " + maThes[i].ToString() + " " + Thread.CurrentThread.ManagedThreadId.ToString() + verifyData.ghiChu.ToString() + "Ma ket qua:" + verifyData.maKetQua + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            ////Linhist changed 16/05/22
        //            ////request.MHTCID = string.IsNullOrEmpty(verifyData.maDKBD) ? "" : verifyData.maDKBD;
        //            ////Linhist changed 05/07/22
        //            ////request.MHTCID = (string.IsNullOrEmpty(request.MHTCID) ? "" : request.MHTCID) + ";" + (string.IsNullOrEmpty(verifyData.maDKBD) ? "" : verifyData.maDKBD);
        //            //request.MHTCID += ";" + (verifyData.maDKBD ?? "");
        //            //request.CHECKED = FileStatusVerification.VERIFIED;

        //            //Linhist removed 16/05/22
        //            //return request;
        //            #endregion
        //        }
        //        catch (Exception ex)
        //        {
        //            request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //            Uti.HandleInternalServerError(
        //                ex, request, _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.CallApiValidateXML1)}");
        //            Uti.HandleCronJobLog(_logger, $"Validate job faled CallApiValidateXML1:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));

        //            //return request;
        //        }

        //        if (verifyData == null) continue;

        //        #region Linhist added 05/08/22 kiem tra cac thong tin phu
        //        try
        //        {
        //            //Linhist added 04/07/22 them kiem tra loi GIOI_TINH (217)
        //            //Linhist changed 03/08/22 them kiem tra GT_THE
        //            string sKetQua = verifyData.maKetQua + ";";

        //            DateTime d1 = DateTime.MinValue;
        //            if (!string.IsNullOrEmpty(verifyData.gtTheTu) && DateTime.TryParseExact(verifyData.gtTheTu, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1))
        //            {
        //                //1183	1183	2	XML1	GT_THE_TU	Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về
        //                if (d1.ToString("yyyyMMdd") != gtTus[i] && !request.DanhSachLoi.Contains("x1183;"))
        //                {
        //                    sKetQua += "x1183;";
        //                }
        //                //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
        //                if (minTheTu == DateTime.MinValue || d1 < minTheTu)
        //                {
        //                    minTheTu = d1;
        //                }
        //            }

        //            DateTime d2 = DateTime.MinValue;
        //            if (!string.IsNullOrEmpty(verifyData.gtTheDen) && DateTime.TryParseExact(verifyData.gtTheDen, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d2))
        //            {
        //                //1184    1184    2   XML1 GT_THE_DEN  Sai hạn thẻ: Giá trị thẻ đến sai so với giá trị trả về
        //                if (d2.ToString("yyyyMMdd") != gtDens[i] && !request.DanhSachLoi.Contains("x1184;"))
        //                {
        //                    sKetQua += "x1184;";
        //                }
        //                //212 212 1   XML1 GT_THE_DEN  Hết hạn thẻ khi chưa ra viện LOI_THE
        //                if (maxTheDen == DateTime.MinValue || d2 > maxTheDen)
        //                {
        //                    maxTheDen = d2;
        //                }
        //            }

        //            //217 217 1   XML1 GIOI_TINH   Thẻ sai giới tính   LOI_THE
        //            if (!string.IsNullOrEmpty(verifyData.gioiTinh) && !Uti.IsMatchGender(verifyData.gioiTinh, request.GIOI_TINH))
        //            {
        //                if (!request.DanhSachLoi.Contains("x217;"))
        //                {
        //                    sKetQua += "x217;";
        //                }
        //            }

        //            //218 1   XML1 MA_DKBD Thẻ sai nơi đăng ký KCB ban đầu
        //            if (!string.IsNullOrEmpty(verifyData.maDKBD))
        //            {
        //                if (verifyData.maDKBD != dkbds[i] && !request.DanhSachLoi.Contains("x218;"))
        //                {
        //                    sKetQua += "x218;";
        //                }
        //            }

        //            //213 213 1   XML1 GT_THE_TU   Thẻ có giá trị khi đang nằm viện    LOI_THE
        //            //214 214 1   XML1 MA_THE  Mã thẻ không có trong dữ liệu thẻ   LOI_THE

        //            //Kiem tra ket qua tra ve, neu ko co loi thi set luon DanhSachLoi la "000"
        //            if (sKetQua == "000;" && (string.IsNullOrEmpty(request.DanhSachLoi) || request.DanhSachLoi == "000"))
        //            {
        //                if (string.IsNullOrEmpty(request.DanhSachLoi))
        //                {
        //                    request.DanhSachLoi = "000";
        //                }
        //            }
        //            // Neu co loi thi them ma loi vao DanhSachLoi
        //            else
        //            {
        //                request.DanhSachLoi += sKetQua;
        //            }

        //            //Linhist added 03/01/2023
        //            //1309	2	XML1	MA_THE	Bệnh nhân đã thực hiện KCB tại cơ sở khác, đề nghị kiểm tra thông tuyến cho BN
        //            if (request.MA_LYDO_VVIEN != 2 && verifyData.dsLichSuKCB2018 != null && verifyData.dsLichSuKCB2018.Any())
        //            {
        //                DateTime kqNGAYVAO = DateTime.MinValue;
        //                bool ngVao = DateTime.TryParseExact(verifyData.dsLichSuKCB2018[0].ngayVao, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out kqNGAYVAO);
        //                DateTime kqNGAYRA = DateTime.MinValue;
        //                bool ngRa = DateTime.TryParseExact(verifyData.dsLichSuKCB2018[0].ngayRa, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out kqNGAYRA);

        //                kqNGAYVAO = kqNGAYVAO.Date;
        //                kqNGAYRA = kqNGAYRA.Date;

        //                DateTime xml1NGAYVAO = request.NGAY_VAO == null ? DateTime.MinValue : request.NGAY_VAO.Value.Date;
        //                DateTime xml1NGAYRA = request.NGAY_RA == null ? DateTime.MinValue : request.NGAY_RA.Value.Date;

        //                if (ngVao && ngRa && xml1NGAYVAO != DateTime.MinValue && xml1NGAYRA != DateTime.MinValue && request.MA_CSKCB != verifyData.dsLichSuKCB2018[0].maCSKCB)
        //                {
        //                    if (!((xml1NGAYVAO < kqNGAYVAO && xml1NGAYRA < kqNGAYVAO) || (xml1NGAYVAO > kqNGAYRA && xml1NGAYRA > kqNGAYRA)))
        //                    {
        //                        request.DanhSachLoi += "x1309;";
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Uti.HandleCronJobLog(_logger, $"Verify card fail in CallApiValidateXML1:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));
        //            Uti.HandleInternalServerError(
        //                ex, request, _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.CallApiValidateXML1)}");
        //        }

        //        Uti.HandleCronJobLog(_logger, $"Submit len API ngon:  Thread " + " " + maThes[i].ToString() + " " + Thread.CurrentThread.ManagedThreadId.ToString() + verifyData.ghiChu.ToString() + "Ma ket qua:" + verifyData.maKetQua + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));

        //        //Linhist changed 16/05/22
        //        //request.MHTCID = string.IsNullOrEmpty(verifyData.maDKBD) ? "" : verifyData.maDKBD;
        //        //Linhist changed 05/07/22
        //        //request.MHTCID = (string.IsNullOrEmpty(request.MHTCID) ? "" : request.MHTCID) + ";" + (string.IsNullOrEmpty(verifyData.maDKBD) ? "" : verifyData.maDKBD);
        //        request.MHTCID += ";" + (verifyData.maDKBD ?? "");
        //        request.CHECKED = FileStatusVerification.VERIFIED;
        //        #endregion
        //    }

        //    //Linhist added 03/08/22 kiem tra gia tri the voi ngay ra vao vien
        //    //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
        //    if (minTheTu != DateTime.MinValue && request.NGAY_VAO < minTheTu)
        //    {
        //        request.DanhSachLoi += "x211;";
        //    }
        //    //212 212 1   XML1 GT_THE_DEN  Hết hạn thẻ khi chưa ra viện LOI_THE
        //    if (maxTheDen != DateTime.MinValue)
        //    {
        //        //Linhist add 03/03/23
        //        //Neu Noi tru thi gioi han the + 15 ngay
        //        if (request.MA_LOAI_KCB == 3 && request.NGAY_RA >= maxTheDen.AddDays(16))
        //        {
        //            request.DanhSachLoi += "x212;";
        //        }
        //        else if (request.NGAY_RA >= maxTheDen.AddDays(1))
        //        {
        //            request.DanhSachLoi += "x212;";
        //        }
        //    }

        //    //Linhist removed 16/05/22
        //    //request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;

        //    //Linhist changed 21/05/22
        //    if (request.CHECKED != FileStatusVerification.VERIFIED)
        //    {
        //        request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //    }

        //    return request;
        //}

        //public async Task CallExternalApiValidateXML1(XML1 request, List<Hospital> hospitals)
        //{

        //    try
        //    {
        //        //haimaster
        //        //Linhist removed log 21/05/22
        //        //Uti.HandleCronJobLog(_logger, $"B3. CallExternalApiValidateXML1 20/05/2022 truoc khi vao get hospital  " + request.XML1ID.ToString() + " " + request.MA_CSKCB.ToString() + "  "+ $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //        var hospital = hospitals.Where(x => x.MA_CSKCB.Equals(request.MA_CSKCB)).FirstOrDefault();

        //        var updateHospitalCal = await _hospitalRepository.GetById(hospital.Id);
        //        updateHospitalCal.FileAPICount++;
        //        _hospitalRepository.Update(updateHospitalCal);
        //        await _hospitalRepository.SaveChangesAsync();

        //        string serviceUrl = "http://egw.baohiemxahoi.gov.vn";

        //        HttpClient client = new HttpClient();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        if (!string.IsNullOrWhiteSpace(hospital.AccessToken) && !string.IsNullOrWhiteSpace(hospital.IdToken))
        //        {
        //            //haimaster
        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B4. CallExternalApiValidateXML1 20/05/2022 kiem tra token OK thi chay den day  " + request.XML1ID.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            var item = await CallApiValidateXML1(client, serviceUrl, request, hospital);

        //            //haimaster
        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B5. CallExternalApiValidateXML1 20/05/2022 Goij API Validate XML1 OK thi chay den day  " + request.XML1ID.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            if (item == null)
        //            {
        //                item = await LoginExternalHandler(client, serviceUrl, request, hospital, hospitals);
        //                //khoikhoi tat login lai khi token failed
        //                if (item == null)
        //                {
        //                    //haimaster 20/5/2022
        //                    //Uti.HandleCronJobLog(_logger, $"Validate Thong tuyen failed: " + item.MA_THE.ToString() + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //                    Uti.HandleCronJobLog(_logger, $"Validate Thong tuyen failed: " + request.XML1ID.ToString() + " " + hospital.Name + "  " + hospital.Username + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));
        //                    request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //                    return;
        //                }
        //            }
        //            request = item;

        //            //haimaster
        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B6.CallExternalApiValidateXML1 20/05/2022 Gan Requets = item  " + request.XML1ID.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //        }
        //        else
        //        {

        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B3.1 CallExternalApiValidateXML1 20/05/2022 truoc khi vao LoginExternalHandler   " + request.XML1ID.ToString() + " " + request.MA_CSKCB.ToString() + "  " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            request = await LoginExternalHandler(client, serviceUrl, request, hospital, hospitals);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //        Uti.HandleInternalServerError(
        //            ex, request, _logger, $"Kiem tra thong tin 20/5/2022: MA_THE: {request.MA_THE} - XML1ID: {request.XML1ID} - {nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.CallExternalApiValidateXML1)}");
        //        return;
        //    }
        //}

        //public async Task VerifyXML1()
        //{
        //    //Linhist added 11/07/22 tam dung thong tuyen
        //    //return;

        //    //Linhist added 23/09/22 chan thong tuyen tu 6h sang den 12h trua




        //    //Linhist added to debug
        //   if (Debugger.IsAttached) return;

        //    Uti.HandleCronJobLog(_logger, $"Thong tuyen job start: Thread " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyXML1));

        //    while (true)
        //    {
        //        //var hour = DateTime.Now.Hour;
        //        //if (hour >= 21 || hour < 5)
        //        //{
        //        //    Uti.HandleCronJobLog(_logger, $"Thong Tuyen job Stop: 7h-18h " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.VerifyXML1));
        //        //    return;
        //        //}

        //        Uti.HandleCronJobLog(_logger, $"Thong tuyen chay " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.VerifyXML1));
        //        try
        //        {
        //            var xml1sResponse = await GetXML1NeedVerify();
        //            if (xml1sResponse == null) return;
        //            if (xml1sResponse.Data == null) return;
        //            if (xml1sResponse.Data.XML1s == null) return;
        //            var xml1s = xml1sResponse.Data.XML1s;
        //            if (xml1sResponse.Data.Hospitals == null) return;
        //            var hospitals = xml1sResponse.Data.Hospitals;

        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B2: Thong tuyen start in loop:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + xml1s.Count().ToString() + $" File  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //            //Console.WriteLine($"Thong tuyen start: " + xml1s.Count().ToString() + $" File  ({DateTime.Now:hh:mm:ss}).");

        //            foreach (var item in xml1s)
        //            {
        //                // ---- Tam thoi check ca the tre em 23/05/23 ------
        //                // Linhist added 31/10/22 khong check thong tuyen voi the tre em co ma duoi la 000
        //                //if (Check_TheTE_ChuaDiHoc(item.MA_THE, item.NGAY_SINH) && item.MA_THE.EndsWith("000"))
        //                //{
        //                //    continue;
        //                //}

        //                await CallExternalApiValidateXML1(item, hospitals);
        //                await Task.Delay(3000);

        //                //Linhist removed log 21/05/22
        //                //Uti.HandleCronJobLog(_logger, $"B7: Validate Thong tuyen thanh cong:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + item.MA_THE.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //                //Console.WriteLine($"Validate Thong tuyen: " + item.MA_THE.ToString() + $"  ({DateTime.Now:hh:mm:ss}).");
        //            }




        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"B7.1: Bat dau update loi:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() +  $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));
        //            //string x1IDUpdateshaimaster = string.Join(",", xml1s.Select(x => x.XML1ID).ToArray());
        //            //Uti.HandleCronJobLog(_logger, $"B7.1.1: Bat dau update loi:  Thread " + x1IDUpdateshaimaster + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            await UpdateXml1AfterVerified(xml1s);

        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"Bxxx: Bat dau update thong tin Benh vien :  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.VerifyXML1));

        //            await UpdateHospitalAfterVerified(hospitals);

        //            //haimaster
        //            //Linhist removed log 21/05/22
        //            //Uti.HandleCronJobLog(_logger, $"Finnal Step 20/05/2022 Ket thuc Verify XML1 : " + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService.UpdateXml1AfterVerified));

        //            await Task.Delay(3000);
        //        }
        //        catch (Exception ex)
        //        {
        //            //request.CHECKED = FileStatusVerification.EXTERNAL_API_HAS_PROBLEM;
        //            Uti.HandleInternalServerError(
        //                ex, "Check thong tuyen job failed :", _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.VerifyXML1)}");
        //            Uti.HandleCronJobLog(_logger, $"Validate job faled:  Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.VerifyXML1));
        //            await Task.Delay(1000);//validate xong 1 vien thi nghi 1s
        //            //Console.WriteLine("Check thong tuyen job failed :" + ex.ToString());

        //        }
        //    }

        //}
        #endregion


        #region NEW Verify XML 08/02/2023
        //Linhist added 23/06/23
        //Luu thong tin CheckThe tu list XML1 vua upload
        //private async Task SaveXML1_CheckThe(List<XML1> list)
        //{
        //    List<CheckThe> newCheckThes = _mapper.Map<List<CheckThe>>(list);
        //    _checkTheRepository.UpdateList(newCheckThes);
        //    await _checkTheRepository.SaveChangesAsync();
        //}

        //public async Task CheckThe_InValidate(XML1 xml1)
        //{
        //    CheckThe the = null; // await _checkTheRepository.GetItem(c => c.XML1ID == xml1.XML1ID);
        //    if (the == null)
        //    {
        //        the = _mapper.Map<CheckThe>(xml1);
        //        the.CheckID = -1;
        //    }

        //    //Kiem tra The hien tai da check Thong tuyen chua? Neu chua check thi se kiem tra vao DB
        //    if (the.TG_CHECK_THE == null)
        //    {
        //        //Kiem tra xem co The nao trung ma da check trong DB chua?
        //        CheckThe theTrung = await _checkTheRepository.GetItem(c => c.XML1ID != xml1.XML1ID && c.MA_CSKCB == xml1.MA_CSKCB && c.TG_CHECK_THE != null &&
        //                                                                    c.MA_LK == xml1.MA_LK && c.MA_THE == xml1.MA_THE && c.MA_DKBD == xml1.MA_DKBD &&
        //                                                                    c.HO_TEN == xml1.HO_TEN && c.DIA_CHI == xml1.DIA_CHI && c.GIOI_TINH == xml1.GIOI_TINH &&
        //                                                                    c.NGAY_SINH == xml1.NGAY_SINH && c.GT_THE_TU == xml1.GT_THE_TU && c.GT_THE_DEN == xml1.GT_THE_DEN, null);

        //        //Neu da check thi clone the moi theo thong tin da check va cap nhat vao CSDL
        //        if (theTrung != null && theTrung.XML1ID != xml1.XML1ID)
        //        {
        //            the.GT_TRA_VE = theTrung.GT_TRA_VE;
        //            the.KQ_TRA_VE = theTrung.KQ_TRA_VE;
        //            the.TG_CHECK_THE = theTrung.TG_CHECK_THE;

        //            //Cap nhat thong tin the vao DB
        //            await UpdateAfter_CheckThe(null, the, null);
        //        }
        //    }

        //    //Kiem tra thong tin The & bao loi
        //    VerifyAfter_ThongTuyen(the, xml1.XML1Errors);
        //}

        #region Linhist removed 090826
        //public async Task Verify_TheBH()
        //{
        //    return;

        //    Debug thi stop thong tuyen ko chay
        //    if (Debugger.IsAttached) return;

        //    Uti.HandleCronJobLog(_logger, $"Thong tuyen job start: Thread " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService130.Verify_TheBH));

        //    while (true)
        //    {
        //        //Stop thong tuyen tu 7h sang den 18h
        //        //var hour = DateTime.Now.Hour;
        //        //if (hour > 6 && hour < 12)
        //        //{
        //        //    Uti.HandleCronJobLog(_logger, $"Thong Tuyen job Stop: 7h-18h " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.Verify_TheBH));
        //        //    return;
        //        //}

        //        try
        //        {
        //            HttpClient client = new HttpClient();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //            var allBVs = await GetBV_AllowVerify();
        //            if (allBVs == null) return;

        //            //Kiem tra BV da check the qua nhieu
        //            DateTime dateNow = DateTime.Now.Date;
        //            List<string> overCheckeds = new List<string>();
        //            foreach (var h in allBVs)
        //            {
        //                int iCnt = _checkTheRepository.GetCount(c => c.MA_CSKCB == h.MA_CSKCB && c.TG_CHECK_THE != null && c.TG_CHECK_THE.Value >= dateNow).Count();
        //                if (iCnt > h.MaxFilePerDate)
        //                {
        //                    h.FileAPICount = iCnt;
        //                }
        //            }

        //            //Lay danh sach toan bo MA_CSKCB cua cac Hospital van con so luot check Thong tuyen
        //            var allCSKCBs = allBVs.Where(h => h.MaxFilePerDate > h.FileAPICount).Select(x => x.MA_CSKCB).Distinct().ToArray();
        //            //Khong con MA_CSKCB nao thi tam nghi 1h roi chay lai
        //            if (allCSKCBs == null || !allCSKCBs.Any())
        //            {
        //                await Task.Delay(3600000);
        //                continue;
        //            }
        //            #region Sửa Code check thẻ theo kiểu mới
        //            IEnumerable<CheckThe> allChecks = null;
        //            MA_CSKCB_DANGTT = "";
        //            foreach (var maCSKCB in allCSKCBs)
        //            {
        //                //var allChecks = await GetThe_NeedVerify(allCSKCBs.ToArray());
        //                if (MA_CSKCB_DANGGD == "" || MA_CSKCB_DANGGD != maCSKCB)
        //                {
        //                    //Uti.HandleCronJobLog(_logger, $"B3.2 Thời gian lấy DS XML cần giám định: {itemData2.Count()}  MA_CSKCB {currentHospital.MA_CSKCB} Validated - Total time: {tsb.TotalMilliseconds}", nameof(HoSoManagementService.ValidateFiles));

        //                    Uti.HandleCronJobLog(_logger, $"TT.2 Thông tuyến lấy danh sách hồ sơ cần thông tuyến cho CSKCB {maCSKCB} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.Verify_TheBH));
        //                    MA_CSKCB_DANGTT = maCSKCB;
        //                    //Đoạn này cần sửa lấy từ bảng XMLState
        //                    //allChecks = await GetThe_CSKCB_NeedVerify(maCSKCB);
        //                }

        //                //Khong co ban ghi CheckThe nao thi tam nghi 10m roi chay lai
        //                if (allChecks == null)
        //                {
        //                    Uti.HandleCronJobLog(_logger, $"TT.2.1 Không có thông tin cần thông tuyến cho CSKCB {maCSKCB} thời gian {DateTime.Now} " + Thread.CurrentThread.ManagedThreadId.ToString(), nameof(HoSoManagementService.Verify_TheBH));
        //                    MA_CSKCB_DANGTT = "";
        //                    await Task.Delay(60000);
        //                    continue;
        //                }

        //                Uti.HandleCronJobLog(_logger, $"TT.3 Số lượng hồ sơ cần check thông tuyến {allChecks.Count()} to Verify.", nameof(HoSoManagementService.Verify_TheBH));
        //                await Task.Delay(1000);
        //            }
        //            #endregion
        //            //var allChecks = await GetThe_NeedVerify(allCSKCBs.ToArray());
        //            //Khong co ban ghi CheckThe nao thi tam nghi 10m roi chay lai
        //            if (allChecks == null)
        //            {
        //                await Task.Delay(600000);
        //                continue;
        //            }

        //            Uti.HandleCronJobLog(_logger, $"Get {allChecks.Count()} to Verify.", nameof(HoSoManagementService130.Verify_TheBH));

        //            //Lay danh sach MA_LOI theo CSKCB
        //            IEnumerable<DMErrorMHTC> allUserErrors = await _dmErrorMHTCRepo.GetListAsync(x => allCSKCBs.Contains(x.MA_CSKCB));

        //            Hospital hospital = allBVs.First();
        //            IEnumerable<int?> bvErrors = allUserErrors.Where(e => e.MA_CSKCB == hospital.MA_CSKCB).Select(x => x.EMErrorCode);

        //            //Bat dau kiem tra tung The
        //            foreach (var the in allChecks)
        //            {
        //                if (hospital?.MA_CSKCB != the.MA_CSKCB)
        //                {
        //                    hospital = allBVs.Where(h => h.MA_CSKCB.Equals(the.MA_CSKCB)).FirstOrDefault();
        //                    if (hospital != null)
        //                    {
        //                        bvErrors = allUserErrors.Where(e => e.MA_CSKCB == hospital.MA_CSKCB).Select(x => x.EMErrorCode);
        //                    }
        //                }

        //                //Khong tim thay BV thi bo qua The nay
        //                if (hospital == null) continue;

        //                bool khongTrung = true;
        //                List<XML1Error> errors = new List<XML1Error>();

        //                //Kiem tra The da chek chua trong so the dang check?
        //                CheckThe theTrung = allChecks.FirstOrDefault(c => c.CheckID != the.CheckID && c.MA_CSKCB == the.MA_CSKCB && c.TG_CHECK_THE != null &&
        //                                                                            c.MA_LK == the.MA_LK && c.MA_THE == the.MA_THE && c.MA_DKBD == the.MA_DKBD &&
        //                                                                            c.HO_TEN == the.HO_TEN && c.DIA_CHI == the.DIA_CHI && c.GIOI_TINH == the.GIOI_TINH &&
        //                                                                            c.NGAY_SINH == the.NGAY_SINH && c.GT_THE_TU == the.GT_THE_TU && c.GT_THE_DEN == the.GT_THE_DEN);

        //                //Neu da check thi clone the moi theo thong tin da check va cap nhat vao CSDL
        //                if (theTrung != null)
        //                {
        //                    khongTrung = false;
        //                    the.GT_TRA_VE = theTrung.GT_TRA_VE;
        //                    the.KQ_TRA_VE = theTrung.KQ_TRA_VE;
        //                    the.TG_CHECK_THE = theTrung.TG_CHECK_THE;
        //                }
        //                else
        //                {
        //                    //Kiem tra The da chek chua trong Db?
        //                    theTrung = await _checkTheRepository.GetItem(c => c.CheckID != the.CheckID && c.MA_CSKCB == the.MA_CSKCB && c.TG_CHECK_THE != null &&
        //                                                                                c.MA_LK == the.MA_LK && c.MA_THE == the.MA_THE && c.MA_DKBD == the.MA_DKBD &&
        //                                                                                c.HO_TEN == the.HO_TEN && c.DIA_CHI == the.DIA_CHI && c.GIOI_TINH == the.GIOI_TINH &&
        //                                                                                c.NGAY_SINH == the.NGAY_SINH && c.GT_THE_TU == the.GT_THE_TU && c.GT_THE_DEN == the.GT_THE_DEN);

        //                    //Neu da check thi clone the moi theo thong tin da check va cap nhat vao CSDL
        //                    if (theTrung != null)
        //                    {
        //                        khongTrung = false;
        //                        the.GT_TRA_VE = theTrung.GT_TRA_VE;
        //                        the.KQ_TRA_VE = theTrung.KQ_TRA_VE;
        //                        the.TG_CHECK_THE = theTrung.TG_CHECK_THE;
        //                    }
        //                }

        //                //Neu khong co The trung thi tien hanh check thong tuyen qua API
        //                if (khongTrung)
        //                {
        //                    //Neu BV chua tung Login thi tien hanh login, sau do goi API check The
        //                    bool calledSuccess = false;
        //                    if (string.IsNullOrWhiteSpace(hospital.AccessToken) && string.IsNullOrWhiteSpace(hospital.IdToken))
        //                    {
        //                        calledSuccess = await LoginApi_ThongTuyen(the, hospital, client);
        //                    }
        //                    else
        //                    {
        //                        calledSuccess = await CallApi_ThongTuyen(the, hospital, client);
        //                    }

        //                    //Neu check The loi thi bo qua The nay
        //                    if (!calledSuccess) continue;
        //                }

        //                //Kiem tra thong tin The, bao loi
        //                VerifyAfter_ThongTuyen(the, errors);

        //                //Xoa MA_LOI, chi lay MA_LOI do BV thiet lap sau do cap nhat vao DB
        //                errors = errors.Where(x => bvErrors.Contains(x.XEErrorCode)).ToList();
        //                await UpdateAfter_CheckThe(hospital, the, errors);

        //                //Check xong 1 Ho so thi nghi 1s
        //                await Task.Delay(1000);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Uti.HandleInternalServerError(ex, "Check thong tuyen job failed", _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.Verify_TheBH)}");
        //            Uti.HandleCronJobLog(_logger, $"Check thong tuyen job failed: Thread " + Thread.CurrentThread.ManagedThreadId.ToString() + ex.ToString() + $"  ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.Verify_TheBH));
        //        }

        //        //Check xong 1 batch thi nghi 2s
        //        await Task.Delay(5000);
        //    }
        //}
        #endregion

        //Lay danh sach Benh vien co cung cap username & password de call api
        private async Task<IEnumerable<Hospital>> GetBV_AllowVerify()
        {
            //dat lai ngay DateTimeNow = Now.Date cua nhung Hospital chua chay Thong tuyen (neu co)
            var notTodays = (await _hospitalRepository.GetListAsync()).Where(h => !string.IsNullOrWhiteSpace(h.Username) && !string.IsNullOrWhiteSpace(h.Password) && (h.DateTimeNow == null || h.DateTimeNow?.Date != DateTime.Now.Date)).ToList();
            if (notTodays != null && notTodays.Count() > 0)
            {
                foreach (Hospital h in notTodays)
                {
                    h.DateTimeNow = DateTime.Now;
                    h.FileAPICount = 0;
                }

                _hospitalRepository.UpdateList(notTodays);
                await _hospitalRepository.SaveChangesAsyncNoBulk();
            }

            //Lay danh sach toan bo Hospital co user & password check Thong tuyen
            var hospitals = (await _hospitalRepository.GetListAsync()).Where(h => !string.IsNullOrWhiteSpace(h.Username) && !string.IsNullOrWhiteSpace(h.Password)).ToList();
            if (hospitals == null || !hospitals.Any()) return null;
            return hospitals;
        }

        #region Linhist removed 07/10/24
        //Lay danh sach The chua check (TG_CHECK_THE == null) theo thu tu Old -> New
        //private async Task<IEnumerable<CheckThe>> GetThe_NeedVerify(string[] lst_MA_CSKCB)
        //{
        //    //Tinh toan so ban ghi moi lan chay
        //    int maxVerifyInTime = _appSettings.BackgroundServices.VerifyXML.MinuteVerifyTime * 60;
        //    maxVerifyInTime = (int)Math.Floor((double)maxVerifyInTime - (maxVerifyInTime * _appSettings.BackgroundServices.VerifyXML.PercentItemInTime / 100));

        //    //Tam thoi fix so ban ghi 1 lan chay
        //    maxVerifyInTime = 30;
        //    var thes = await _checkTheRepository.GetListPaginationAsync(0, maxVerifyInTime, c => lst_MA_CSKCB.Contains(c.MA_CSKCB) && c.TG_CHECK_THE == null, t => t.OrderByDescending(c => c.XML1ID));
        //    if (thes == null || !thes.Items.Any()) return null;
        //    return thes.Items;
        //}

        //Kiem tra co ton tai thong tin the tuong tu da check hay chua?
        //private async Task<CheckThe> GetThe_IfChecked(string maCSKCB, string maLK, string maThe, string maDKBD, string hoTen, string diaChi, byte? gioiTinh, DateTime? ngaySinh, string gtTheTu, string gtTheDen)
        //{
        //    //Tim kiem The co thong tin tuong tu trong CSDL
        //    return await _checkTheRepository.GetItem(c => c.MA_CSKCB == maCSKCB && c.TG_CHECK_THE != null && c.MA_LK == maLK && c.MA_THE == maThe &&
        //                                                        c.MA_DKBD == maDKBD && c.HO_TEN == hoTen && c.DIA_CHI == diaChi && c.GIOI_TINH == gioiTinh &&
        //                                                        c.NGAY_SINH == ngaySinh && c.GT_THE_TU == gtTheTu && c.GT_THE_DEN == gtTheDen);
        //}
        #endregion

        //Goi API Thong tuyen 1 lan va luu du lieu vao The dang check
        private async Task<bool> CallApi_ThongTuyen(XML01 x01, Hospital hospital, HttpClient client)
        {
            string serviceUrl = "http://egw.baohiemxahoi.gov.vn";

            //Set thong tin The tra ve la null, chua check
            //x01.State.VerifyResult = "";
            //x01.State.LastVerify = null;
            //x01.State.VerifyResult = "";
            x01.State.VerifyDate = null;
            NhanLichSuKCB2018 verifyData = null;

            //Dam bao kiem tra tung MA_THE le trong danh sach MA_THE truyen vao check
            string[] maThes = x01.MA_THE_BHYT.Split(';');
            string kqTraVe = "";
            string gtTraVe = "";

            try
            {
                string maTheTrc = "";
                string kqTruoc = "";
                string gtTruoc = "";

                foreach (string mt in maThes)
                {
                    if (string.IsNullOrWhiteSpace(mt)) continue;

                    //Kiem tra truong hop 2 ma The tro len bi trung nhau thi chi check Thong tuyen 1 lan
                    if (mt == maTheTrc)
                    {
                        gtTraVe += "," + gtTruoc;
                        kqTraVe += ";" + kqTruoc;
                        continue;
                    }
                    else
                    {
                        maTheTrc = mt;
                    }
                    string hashPass = EasyMD5.Hash(hospital.Password ?? "xxx");
                    string urlQuery = $"username={hospital.Username}&password={hashPass}&token={hospital.AccessToken}&id_token={hospital.IdToken}";

                    //Goi API thong tuyen
                    theBHYT thePost = new theBHYT
                    {
                        maThe = mt,
                        hoTen = x01.HO_TEN,
                        ngaySinh = x01.NGAY_SINH?.ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo),
                        gioiTinh = (int)x01.GIOI_TINH,
                        maCSKCB = x01.MA_CSKCB
                    };
                    var verifyResult = await client.PostAsJsonAsync($"{serviceUrl}/api/egw/NhanLichSuKCB2018?{urlQuery}", thePost);

                    //Goi API ko thanh cong do loi mang, sai yeu cau,... bo qua ko check The nay nua
                    if (verifyResult.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Uti.HandleCronJobLog(_logger, $"Submit len API fail (unauthorized connect to API): {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApi_ThongTuyen));
                        return false;
                    }
                    if (!verifyResult.IsSuccessStatusCode)
                    {
                        Uti.HandleCronJobLog(_logger, $"Submit len API fail (not connect to API): {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApi_ThongTuyen));
                        return false;
                    }

                    //Goi API thong tuyen thanh cong
                    string result = await verifyResult.Content.ReadAsStringAsync();
                    verifyData = JsonConvert.DeserializeObject<NhanLichSuKCB2018>(result);

                    //Bao loi chua dang nhap, thuc hien dang nhap lai va goi lai API
                    if (verifyData.ghiChu == "Token không đúng." || verifyData.maKetQua == "401")
                    {
                        Uti.HandleCronJobLog(_logger, $"Submit len API fail (login to reset token): {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApi_ThongTuyen));
                        return await LoginApi_ThongTuyen(x01, hospital, client);
                    }

                    //Ket qua kiem tra thong tuyen OK, cap nhat va chay tiep
                    gtTruoc = result;
                    kqTruoc = verifyData.maKetQua;
                    gtTraVe += (gtTraVe == "" ? "" : ",") + result;
                    kqTraVe += (kqTraVe == "" ? "" : ";") + verifyData.maKetQua;
                    hospital.FileAPICount++;
                    Uti.HandleCronJobLog(_logger, $"Submit len API ngon: {x01.MA_THE_BHYT}. KQ tra ve: {verifyData.maKetQua}, BV: {hospital.Name}.", nameof(HoSoManagementService130.CallApi_ThongTuyen));
                }

                //Luu ket qua kiem tra thong tuyen cua tat ca cac the vao CheckThe
                //KQ_TRA_VE = kqTraVe;
                //x01.State.VerifyResult = "[" + gtTraVe + "]";
                //x01.State.LastVerify = DateTime.Now;
                x01.State.VerifyDate = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(ex, x01, _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.CallApi_ThongTuyen)}");
                Uti.HandleCronJobLog(_logger, $"Submit len API fail (login to reset token): {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.CallApi_ThongTuyen));
            }
            return false;
        }

        //Thuc hien login lay token & goi lai API thong tuyen de check The
        private async Task<bool> LoginApi_ThongTuyen(XML01 x01, Hospital hospital, HttpClient client)
        {
            string serviceUrl = "http://egw.baohiemxahoi.gov.vn";

            try
            {
                //Set thong tin login cho BV
                string hashString = EasyMD5.Hash(hospital.Password ?? "xxx");
                var loginPost = new StringContent(JsonConvert.SerializeObject(new
                {
                    username = hospital.Username,
                    password = hashString
                }), Encoding.UTF8, "application/json");
                Uti.HandleCronJobLog(_logger, $"Thuc hien login: {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.LoginApi_ThongTuyen));

                //Thuc hien login, neu co loi ko the truy cap thi bo qua The nay
                var signInResult = await client.PostAsync($"{serviceUrl}/api/token/take", loginPost);
                if (!signInResult.IsSuccessStatusCode)
                {
                    Uti.HandleCronJobLog(_logger, $"Khong the login: {x01.MA_THE_BHYT} (error HTTP{signInResult.StatusCode}). BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.LoginApi_ThongTuyen));
                    return false;
                }

                //Login thanh cong
                string loginData = await signInResult.Content.ReadAsStringAsync();
                var plv = JsonConvert.DeserializeObject<KQPhienLamViec>(loginData);
                var key = plv.APIKey;
                Uti.HandleCronJobLog(_logger, $"Login ok: {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username}, toekn: {key.access_token} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.LoginApi_ThongTuyen));

                //kiem tra thong tin va set lai Token cho BV, sau do chay lai API thong tuyen de check The
                if (!string.IsNullOrEmpty(key.access_token) && !string.IsNullOrEmpty(key.id_token))
                {
                    hospital.AccessToken = key.access_token;
                    hospital.IdToken = key.id_token;

                    return await CallApi_ThongTuyen(x01, hospital, client);
                }
            }
            catch (Exception ex)
            {
                Uti.HandleInternalServerError(ex, hospital, _logger, $"{nameof(HoSoManagementService130)} - {nameof(HoSoManagementService130.LoginApi_ThongTuyen)}");
                Uti.HandleCronJobLog(_logger, $"Khong the login: {x01.MA_THE_BHYT}. BV: {hospital.Name}, user: {hospital.Username} ({DateTime.Now:hh:mm:ss}).", nameof(HoSoManagementService130.LoginApi_ThongTuyen));
            }

            return false;
        }

        //Update lai thong tin BV, CheckThe & danh sach loi vao DB
        private async Task UpdateAfter_CheckThe(Hospital hospital, XML01 x01)
        {
            DateTime d = DateTime.Now;
            Uti.HandleCronJobLog(_logger, $"Bat datu Save Thong tuyen data to Db: {x01.MA_THE_BHYT}.", nameof(HoSoManagementService130.CallApi_ThongTuyen));

            if (x01 == null || x01.IsDelete == true) return;

            try
            {
                var errors = new List<XMLError>();
                //var states = new List<XMLState>();
                var lstX0s = new List<XMLState>();
                var errFilters = await _dmErrorMHTCRepo.GetListAsync(e => e.MA_CSKCB == hospital.MA_CSKCB);
                var errCodes = errFilters.Select(e => e.EMErrorCode);
                var altCodes = errFilters.Where(e => e.EMEnabled == false).Select(e => e.EMErrorCode);

                foreach (var e in x01.Errors)
                {
                    e.XML01 = null;
                    errors.Add(e);
                }

                //x01.State.VerifyResult = x01.State?.VerifyResult;
                //x01.State.TotalErrors += errors.Count();
                //x01.State.TotalAlerts += errors.Count(e => altCodes.Contains(e.MA_LOI));
                //x01.State.LastVerify = x01.State?.LastVerify;
                //x01.State.XML01 = null;
                //states.Add(x01.State);
                x01.State.DS_LOI += string.Join("", errors.Select(e => $"{e.MA_LOI};").ToArray());
                x01.State.TONG_SO_LOI += errors.Count();
                x01.State.CANH_BAO += errors.Count(e => altCodes.Contains(e.MA_LOI));
                x01.State.VerifyDate = x01.State?.VerifyDate;
                x01.State.XML01 = null;
                lstX0s.Add(x01.State);

                //await _resultRepository.DeleteListByFilterAsync(x => x.XML01ID == x01.XML01ID);
                await _errorRepository.InsertListAsync(errors);
                //_stateRepository.Update(x01.State);
                _stateRepository.Update(x01.State);
                await _unitOfWork.CommitAsync();
            }
            catch
            { }

            //if (hospital != null)
            //{
            //    try
            //    {
            //        _hospitalRepository.Update(hospital);
            //        await _hospitalRepository.SaveChangesAsync();
            //    }
            //    catch
            //    {
            //        Uti.HandleCronJobLog(_logger, $"[VERYFY_THE]: Save Hospital Info to Db fails: {x01.MA_THE_BHYT}, MA_CSKCB: {hospital.MA_CSKCB}", nameof(HoSoManagementService130.CallApi_ThongTuyen));
            //    }
            //}

            //await _unitOfWork.CommitAsync();

            TimeSpan ts = DateTime.Now - d;
            Uti.HandleCronJobLog(_logger, $"Save thanh cong Thong tuyen data to Db: {x01.MA_THE_BHYT}, thoi gian: {ts.TotalSeconds}", nameof(HoSoManagementService130.CallApi_ThongTuyen));
        }

        //Kiem tra du lieu thong tuyen da lay duoc voi XML1
        private void VerifyAfter_ThongTuyen(XML01 x01)
        {
            //if (x01 == null || x01.State.LastVerify == null || string.IsNullOrEmpty(x01.State.VerifyResult)) return;

            //DateTime minTheTu = DateTime.MaxValue;
            //DateTime maxTheDen = DateTime.MinValue;

            ////Dam bao kiem tra tung MA_THE le trong danh sach MA_THE truyen vao check
            //int idx = -1;
            //NhanLichSuKCB2018[] verifyData = JsonConvert.DeserializeObject<NhanLichSuKCB2018[]>(x01.State.VerifyResult);
            //string[] maThes = (x01.MA_THE_BHYT ?? "").Split(';');
            //string[] gtTheTus = (x01.GT_THE_TU ?? "").Split(';');
            //string[] gtTheDens = (x01.GT_THE_DEN ?? "").Split(';');
            //string[] kqTraVes = verifyData?.Select(x => x.maKetQua).ToArray();
            //string[] maDKBDs = (x01.MA_DKBD ?? "").Split(';');

            ////Kiem tra thong tin The voi thong tin thong tuyen & bao loi
            //foreach (var v in verifyData)
            //{
            //    idx++;

            //    //Linhist added 11/04/23
            //    //haimaster 11.4.2023 => Neu the la TE bo qua ma loi 050 (Errorcode 226)
            //    //haimaster update 24.5.2023 bo qua cho Tuyen Quang
               
            //    //if (maThes[idx].ToLower().StartsWith("te") && (kqTraVes[idx] == "000" || kqTraVes[idx] == "050")) continue;

            //    int errCode = Get_MALOI(kqTraVes[idx]);
            //    if (errCode != 0)
            //    {
            //        x01.RaiseError(errCode);
            //    }

            //    DateTime d1 = DateTime.MinValue;
            //    if (!string.IsNullOrEmpty(v.gtTheTu) && DateTime.TryParseExact(v.gtTheTu, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d1))
            //    {
            //        //1183	1183	2	XML1	GT_THE_TU	Sai hạn thẻ: Giá trị thẻ từ sai so với giá trị trả về
            //        if (d1.ToString("yyyyMMdd") != gtTheTus[idx])
            //        {
            //            x01.RaiseError(1183);
            //        }
            //        //211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
            //        if (d1 < minTheTu)
            //        {
            //            minTheTu = d1;
            //        }
            //    }

            //    DateTime d2 = DateTime.MinValue;
            //    if (!string.IsNullOrEmpty(v.gtTheDen) && DateTime.TryParseExact(v.gtTheDen, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d2))
            //    {
            //        //1184    1184    2   XML1 GT_THE_DEN  Sai hạn thẻ: Giá trị thẻ đến sai so với giá trị trả về
            //        if (d2.ToString("yyyyMMdd") != gtTheDens[idx])
            //        {
            //            x01.RaiseError(1184);
            //        }
            //    }

            //    //217 217 1   XML1 GIOI_TINH   Thẻ sai giới tính   LOI_THE
            //    if (!string.IsNullOrEmpty(v.gioiTinh) && !Uti.IsMatchGender(v.gioiTinh, x01.GIOI_TINH))
            //    {
            //        x01.RaiseError(217);
            //        //212 212 1   XML1 GT_THE_DEN  Hết hạn thẻ khi chưa ra viện LOI_THE
            //        if (d2 > maxTheDen)
            //        {
            //            maxTheDen = d2;
            //        }
            //    }

            //    //218 1   XML1 MA_DKBD Thẻ sai nơi đăng ký KCB ban đầu
            //    if (!string.IsNullOrEmpty(v.maDKBD) && !Uti.IsEqual(v.maDKBD, maDKBDs[idx]))
            //    {
            //        x01.RaiseError(218);
            //    }
            //}

            ////211 211 1   XML1 GT_THE_TU   KCB khi chưa đến hạn LOI_THE
            //if (minTheTu != DateTime.MaxValue && x01.NGAY_VAO < minTheTu)
            //{
            //    x01.RaiseError(211);
            //}
            ////212 212 1   XML1 GT_THE_DEN  Hết hạn thẻ khi chưa ra viện LOI_THE
            //if (maxTheDen != DateTime.MinValue && x01.NGAY_RA >= maxTheDen.AddDays(1))
            //{
            //    x01.RaiseError(218);
            //}
        }

        //Tra ve MA_LOI tuong ung voi loi thong tuyen
        private int Get_MALOI(string KQ_TRA_VE)
        {
            switch (KQ_TRA_VE)
            {
                //213 213 1   XML1 GT_THE_TU   Thẻ có giá trị khi đang nằm viện    LOI_THE
                //214 214 1   XML1 MA_THE  Mã thẻ không có trong dữ liệu thẻ   LOI_THE

                //222   2   XML1    MA_THE  Thẻ do BHXH Bộ Quốc Phòng quản lý, đề nghị kiểm tra thẻ và thông tin giấy tờ tùy thân
                case "001": return 222;

                //223   2   XML1    MA_THE  Thẻ do BHXH Bộ Công An quản lý, đề nghị kiểm tra thẻ và thông tin giấy tờ tùy thân
                case "002": return 223;

                //361   1   XML1    MA_THE  Thẻ cũ hết giá trị sử dụng nhưng đã được cấp thẻ mới
                case "003": return 361;

                //362   2   XML1    MA_THE  Thẻ cũ còn giá trị sử dụng nhưng đã được cấp thẻ mới
                case "004": return 362;

                //210   1   XML1    GT_THE_DEN  Thẻ hết giá trị sử dụng
                case "010": return 210;

                //226   1   XML1    MA_THE  Không thấy thông tin thẻ BHYT
                case "050": return 226;

                //209	1	XML1	MA_THE	Mã thẻ không đúng
                case "051": return 209;

                //224   1   XML1    MA_THE  Mã tỉnh cấp thẻ(kí tự thứ 4,5 của mã thẻ) không đúng
                case "052": return 224;

                //225   1   XML1    MA_THE  Mã quyền lợi thẻ(ký tự thứ 3 của mã thẻ) không đúng
                case "053": return 225;

                //215   1   XML1    HO_TEN  Thẻ sai họ tên
                case "060": return 215;

                //227   1   XML1    MA_THE  Thẻ sai họ tên(đúng kí tự đầu)
                case "061": return 277;

                //216   1   XML1    NGAY_SINH   Thẻ sai ngày sinh
                case "070": return 216;

                //221   1   XML1    MA_THE  Lỗi phát sinh trong quá trình kiểm tra thẻ
                case "100": return 221;

                //229   1   XML1    MA_THE  Không có kết nối đến Máy chủ
                case "101": return 229;

                //230   1   XML1    MA_THE  Thẻ đã thu hồi
                case "110": return 230;

                //231   1   XML1    GT_THE_DEN  Thẻ đã báo giảm
                case "120": return 231;

                //232   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Giảm chuyển ngoại tỉnh
                case "121": return 232;

                //233   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Giảm chuyển nội tỉnh
                case "122": return 233;

                //234   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Thu hồi do tăng lại cùng đơn vị
                case "123": return 234;

                //235   1   XML1    GT_THE_DEN  Thẻ đã báo giảm. Ngừng tham gia
                case "124": return 235;

                //236   2   XML1    MA_THE  Trẻ em không xuất trình thẻ
                case "130": return 236;

                //221   1   XML1    MA_THE  Lỗi phát sinh trong quá trình kiểm tra thẻ
                case "205": return 221;

                //228   1   XML1    MA_THE  Lỗi khi lấy dữ liệu sổ thẻ
                case "401": return 228;
            }

            return 0;
        }
        #endregion

        #region Linhist added 11/05/23 - removed 090826
        //public async Task<List<string>> ListFilesByCreateDate(DateTime fromDate, string maCSKCB)
        //{
        //    DateTime d1 = fromDate.Date;
        //    DateTime d2 = fromDate.Date.AddDays(1);
        //    Expression<Func<XML1, bool>> filter = PredicateBuilder.New<XML1>(x => x.isDelete != true);
        //    filter = filter.And(x => x.CreateDate >= d1 && x.CreateDate < d2);
        //    if (!string.IsNullOrWhiteSpace(maCSKCB))
        //    {
        //        filter = filter.And(x => x.MA_CSKCB == maCSKCB);
        //    }

        //    return await _hosoRepository.GetFileNames(filter);
        //    return null;
        //}
        #endregion
    }


    //public class ApiToken
    //{

    //    [DataMember]
    //    public string username { get; set; }

    //    [DataMember]
    //    public string password { get; set; }

    //}

    public class KQPhienLamViec
    {
        public string maketQua { get; set; }
        public ApiKey APIKey { get; set; }

    }
    public class ApiKey
    {
        [DataMember]
        public string access_token { get; set; }
        [DataMember]

        public string id_token { get; set; }
        [DataMember]
        public string token_type { get; set; }
        [DataMember]
        public string username { get; set; }
        [DataMember]
        public DateTime expires_in { get; set; }
    }
    //public class KQWhanThongTincsKcs
    //{
    //    public string maKetQua { get; set; }
    //    public string ghiChu { get; set; }
    //    public string maThe { get; set; }
    //    public string hoTen { get; set; }
    //    public string ngaySinh { get; set; }
    //    public string gioiTinh { get; set; }
    //    public string diaChi { get; set; }
    //    public string maDKBD { get; set; }
    //    public string cqBHXH { get; set; }
    //    public string gtTheTu { get; set; }
    //    public string gtTheDen { get; set; }
    //    public string maKV { get; set; }
    //    public string ngayDu5Nam { get; set; }
    //    public string maSoBHXH { get; set; }
    //    public string maTheCu { get; set; }
    //    public string maTheMoi { get; set; }
    //    public string gtTheTuMoi { get; set; }
    //    public string gtTheDenMoi { get; set; }
    //    public string maDKBDMoi { get; set; }
    //    public string tenDKBDMoi { get; set; }
    //}
    public class theBHYT
    {
        public string maThe { get; set; }
        public string hoTen { get; set; }
        public string ngaySinh { get; set; }
        public int gioiTinh { get; set; }
        public string maCSKCB { get; set; }
        public string ngayBD { get; set; }
        public string ngayKT { get; set; }


    }

    public class NhanLichSuKCB2024
    {
        public string maKetQua { get; set; }
        public string ghiChu { get; set; }
        public string maThe { get; set; }
        public string hoTen { get; set; }
        public string ngaySinh { get; set; }
        public string gioiTinh { get; set; }
        public string diaChi { get; set; }
        public string maDKBD { get; set; }
        public string cqBHXH { get; set; }
        public string gtTheTu { get; set; }
        public string gtTheDen { get; set; }
        public string maKV { get; set; }
        public string ngayDu5Nam { get; set; }
        public string maSoBHXH { get; set; }
        public string maTheCu { get; set; }
        public string maTheMoi { get; set; }
        public string gtTheTuMoi { get; set; }
        public string gtTheDenMoi { get; set; }
        public string maDKBDMoi { get; set; }
        public string tenDKBDMoi { get; set; }
        public List<LichSuKhamChuaBenh> dsLichSuKCB2018 { get; set; }
        public List<LichSuKT2018> dsLichSuKT2018 { get; set; }
        public string maHoSo { get; set; }
        public string maCSKCB { get; set; }

    }
    public class LichSuKhamChuaBenh
    {
        public string maHoSo { get; set; }
        public string maCSKCB { get; set; }
        public string ngayVao { get; set; }
        public string ngayRa { get; set; }
        public string tenBenh { get; set; }
        public string tinhTrang { get; set; }
        public string kqDieuTri { get; set; }
        public string lyDoVV { get; set; }
        public string TEMP1 { get; set; }
        public string TEMP2 { get; set; }
        public string TEMP3 { get; set; }
        public string TEMP4 { get; set; }
        public string TEMP5 { get; set; }
    }
    public class LichSuKT2018
    {
        public string userKT { get; set; }
        public string thoiGianKT { get; set; }
        public string thongBao { get; set; }
        public string maLoi { get; set; }
    }
    public class NhanLichSuKCB2018
    {
        public string maKetQua { get; set; }
        public string ghiChu { get; set; }
        public string maThe { get; set; }
        public string hoTen { get; set; }
        public string ngaySinh { get; set; }
        public string gioiTinh { get; set; }
        public string diaChi { get; set; }
        public string maDKBD { get; set; }
        public string cqBHXH { get; set; }
        public string gtTheTu { get; set; }
        public string gtTheDen { get; set; }
        public string maKV { get; set; }
        public string ngayDu5Nam { get; set; }
        public string maSoBHXH { get; set; }
        public string maTheCu { get; set; }
        public string maTheMoi { get; set; }
        public string gtTheTuMoi { get; set; }
        public string gtTheDenMoi { get; set; }
        public string maDKBDMoi { get; set; }
        public string tenDKBDMoi { get; set; }
        //public string dsLichSuKCB2018 { get; set; }
        //public string dsLichSuKT2018 { get; set; }

        public List<LichSuKhamChuaBenh> dsLichSuKCB2018 { get; set; }
        public List<LichSuKT2018> dsLichSuKT2018 { get; set; }
    }
    public class NhanLichSuKCB2025
    {
        public string maKetQua { get; set; }
        public string ghiChu { get; set; }
        public string maThe { get; set; }
        public string hoTen { get; set; }
        public string ngaySinh { get; set; }
        public string gioiTinh { get; set; }
        public string diaChi { get; set; }
        public string maDKBD { get; set; }
        public string cqBHXH { get; set; }
        public string gtTheTu { get; set; }
        public string gtTheDen { get; set; }
        public string maKV { get; set; }
        public string ngayDu5Nam { get; set; }
        public string maSoBHXH { get; set; }
        public string maTheCu { get; set; }
        public string maTheMoi { get; set; }
        public string gtTheTuMoi { get; set; }
        public string gtTheDenMoi { get; set; }
        public string maDKBDMoi { get; set; }
        public string tenDKBDMoi { get; set; }
        public List<LichSuKhamChuaBenh> dsLichSuKCB2018 { get; set; }
        public List<LichSuKhamChuaBenh> dsLichSuKCB2025 { get; set; }
        public List<LichSuKT2018> dsLichSuKT2018 { get; set; }
        public string maHoSo { get; set; }
        public string maCSKCB { get; set; }

    }

    class EasyMD5
    {

        private static string GetMd5Hash(byte[] data)
        {
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            return sBuilder.ToString();
        }

        private static bool VerifyMd5Hash(byte[] data, string hash)
        {
            return 0 == StringComparer.OrdinalIgnoreCase.Compare(GetMd5Hash(data), hash);
        }

        public static string Hash(string data)
        {
            using (var md5 = MD5.Create())
                return GetMd5Hash(md5.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }
        public static string Hash(FileStream data)
        {
            using (var md5 = MD5.Create())
                return GetMd5Hash(md5.ComputeHash(data));
        }

        public static bool Verify(string data, string hash)
        {
            using (var md5 = MD5.Create())
                return VerifyMd5Hash(md5.ComputeHash(Encoding.UTF8.GetBytes(data)), hash);
        }

        public static bool Verify(FileStream data, string hash)
        {
            using (var md5 = MD5.Create())
                return VerifyMd5Hash(md5.ComputeHash(data), hash);
        }
    }
}