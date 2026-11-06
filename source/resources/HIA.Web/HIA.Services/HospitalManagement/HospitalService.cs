using AutoMapper;
using HIA.Core.Base;
using HIA.Core.Languages.Message;
using HIA.Core.Models.Base;
using HIA.Core.Models.Diagram;
using HIA.Core.Models.Hospital;
using HIA.Core.Models.Pagination;
using HIA.Core.Models.Token;
using HIA.DatabaseModel.Entites;
using HIA.Repositories.Base;
using HIA.Repositories.Files;
using HIA.Services.SysHistoryLogManagement;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;


namespace HIA.Services.HospitalManagement
{
    public class HospitalService : IHospitalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBaseRepository<Hospital> _hospitalRepository;
        //private readonly IXml1Repository _xml1Repository;
        private readonly IXml01Repository _xml1Repository;
        private readonly ISysHistoryLogService _sysHistoryLogService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;
        private readonly ILogger<HospitalService> _logger;

        public HospitalService(
            IUnitOfWork unitOfWork,
            IBaseRepository<Hospital> hospitalRepository,
            //IXml1Repository xml1Repository,
            IXml01Repository xml1Repository,
            ISysHistoryLogService sysHistoryLogService,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<HospitalService> logger
        )
        {
            _unitOfWork = unitOfWork;
            _hospitalRepository = hospitalRepository;
            _xml1Repository = xml1Repository;
            _sysHistoryLogService = sysHistoryLogService;
            _httpContext = httpContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveHospitals()
        {
            var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                
                List<SelectListItem> hospitals;
                if (currentRole.Contains(RoleConstant.MASTER) || currentMaCSKCB == "BOYTE")
                {
                    hospitals = (await _hospitalRepository.GetListAsync(
                            x => !!x.IsActivated && !x.IsDeleted,
                            o => o.OrderBy(o1 => o1.Name)))?
                            .Select(s => new SelectListItem
                            {
                                Text = s.Name,
                                //Linhist changed 27/02/23
                                //Value = s.Id
                                Value = s.MA_CSKCB,
                            }).ToList();
                }
                else
                {
                   

                    hospitals = (await _hospitalRepository.GetListAsync(
                           x => !!x.IsActivated && !x.IsDeleted &&x.SuperHospitalId== _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value,
                           o => o.OrderBy(o1 => o1.Name)))?
                           .Select(s => new SelectListItem
                           {
                               Text = s.Name,
                               //Linhist changed 27/02/23
                               //Value = s.Id
                               Value = s.MA_CSKCB,
                           }).ToList();

                    hospitals.Add(new SelectListItem
                    {
                        Text = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_NAME).Value,
                        //Linhist changed 27/02/23
                        //Value = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value
                        Value = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value,
                    });
                   
                }
                res.Data = hospitals;
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<IEnumerable<SelectListItem>>(
                    ex, new SelectList(new List<SelectListItem>()), _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetActiveHospitals)}");
            }

            return res;
        }

        public async Task<ResponseModel<IEnumerable<SelectListItem>>> GetActiveHospitalsByID()
        {
            var res = new ResponseModel<IEnumerable<SelectListItem>>(HttpStatusCode.OK, "", new List<SelectListItem>());
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                List<SelectListItem> hospitals;
                if (currentRole.Contains(RoleConstant.MASTER)|| currentMaCSKCB =="BOYTE")
                {
                    hospitals = (await _hospitalRepository.GetListAsync(
                            x => !!x.IsActivated && !x.IsDeleted,
                            o => o.OrderBy(o1 => o1.Name)))?
                            .Select(s => new SelectListItem
                            {
                                Text = s.Name,
                                Value = s.MA_CSKCB,
                            }).ToList();
                }
                else
                {

                    hospitals = (await _hospitalRepository.GetListAsync(
                           x => !!x.IsActivated && !x.IsDeleted && x.SuperHospitalId == _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value,
                           o => o.OrderBy(o1 => o1.Name)))?
                           .Select(s => new SelectListItem
                           {
                               Text = s.Name,
                               //Linhist changed 27/02/23
                               //Value = s.Id
                               Value = s.MA_CSKCB,
                           }).ToList();
                    hospitals.Add(new SelectListItem
                        {
                            Text = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_NAME).Value,
                            //Linhist changed 27/02/23
                            //Value = _httpContext.HttpContext.User.FindFirst(ClaimKey.HOSPITAL_ID).Value
                            Value = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value,
                        }
                    );
                }
                res.Data = hospitals;
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<IEnumerable<SelectListItem>>(
                    ex, new SelectList(new List<SelectListItem>()), _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetActiveHospitals)}");
            }

            return res;
        }

        public async Task<ResponseModel<IEnumerable<HospitalInfoModel>>> GetActiveByOwner()
        {
            var res = new ResponseModel<IEnumerable<HospitalInfoModel>>(HttpStatusCode.OK, "", new List<HospitalInfoModel>());
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                IEnumerable<Hospital> items;
                if (currentRole.Contains(RoleConstant.MASTER) || currentMaCSKCB == "BOYTE" || currentMaCSKCB == "00")
                {
                    items = await _hospitalRepository.GetListAsync(
                            x => !!x.IsActivated && !x.IsDeleted && x.MA_CSKCB.Length == 5,
                            o => o.OrderBy(o1 => o1.MA_CSKCB));
                }
                else if (currentMaCSKCB.Length < 5)
                {
                    items = await _hospitalRepository.GetListAsync(
                            x => !!x.IsActivated && !x.IsDeleted && x.MA_CSKCB.Length == 5 && x.MA_CSKCB.StartsWith(currentMaCSKCB),
                            o => o.OrderBy(o1 => o1.MA_CSKCB));
                }    
                else
                {
                    items = await _hospitalRepository.GetListAsync(
                           x => !!x.IsActivated && !x.IsDeleted && x.MA_CSKCB.Length == 5 && x.MA_CSKCB == currentMaCSKCB,
                           o => o.OrderBy(o1 => o1.MA_CSKCB));
                }

                res.Data = items?.Select(s => new HospitalInfoModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsActivated = s.IsActivated,
                    Address = s.Address,
                    MA_CSKCB = s.MA_CSKCB,
                    Loai_Tuyen = s.Loai_Tuyen.Value,
                    Username = s.Username,
                    Password = s.Password,
                    MaxFilePerDate = s.MaxFilePerDate,
                    SuperHospitalId = s.SuperHospital?.Name,
                    HospitalCons = string.Join(',', s.HospitalCons?.Select(x => x.Name).ToList()),
                    LevelId = s.LevelId
                }).ToList();
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<IEnumerable<HospitalInfoModel>>(
                    ex, new List<HospitalInfoModel>(), _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetActiveHospitals)}");
            }

            return res;
        }

        public async Task<ResponseModel<PaginationResponseModel<HospitalInfoModel>>> GetItems(PaginationRequestModel request)
        {
            var res = new ResponseModel<PaginationResponseModel<HospitalInfoModel>>(HttpStatusCode.OK, "", new PaginationResponseModel<HospitalInfoModel>());

            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                #region Validation
                if (string.IsNullOrWhiteSpace(currentRole) || !(currentRole.Equals(RoleConstant.MASTER) || currentMaCSKCB.Equals("BOYTE")))
                {
                    res.Code = HttpStatusCode.Forbidden;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                #endregion

                #region Build Filter Query
                Expression<Func<Hospital, bool>> filter = PredicateBuilder.New<Hospital>(x => !x.IsDeleted);
                // add filter when has searchKey
                if (!string.IsNullOrWhiteSpace(request.SearchKey))
                {
                    var key = request.SearchKey.ToLower();
                    filter = filter.And(x =>
                            x.Name.ToLower().Contains(key)
                        );
                }
                #endregion
                #region Build Sort Query
                IList<Func<IOrderedQueryable<Hospital>, IOrderedQueryable<Hospital>>> sortFuncList = new List<Func<IOrderedQueryable<Hospital>, IOrderedQueryable<Hospital>>>();

                var columnName = Uti.SetUpperCaseFirstChar(request.OrderByColumn);
                // dynamic sort by column
                if (!string.IsNullOrWhiteSpace(columnName) && !string.IsNullOrWhiteSpace(request.OrderType))
                {
                    switch (columnName)
                    {
                        default:
                            sortFuncList.Add(x => x.ThenBy(columnName + " " + request.OrderType));
                            break;
                    }
                }
                #endregion
                #region Get User
                //filter = filter.And(x => x.SuperHospitalId != null);
                PaginationResponseModel<Hospital> items = await _hospitalRepository.GetListPaginationCustomSortAsync(request.PageIndex, request.PageNumber, filter, sortFuncList, y => y.Include(t => t.SuperHospital).Include(s=>s.HospitalCons));

                res.Data.Items = items?.Items?.Select(s => new HospitalInfoModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsActivated = s.IsActivated,
                    Address = s.Address,
                    MA_CSKCB = s.MA_CSKCB,
                    Loai_Tuyen = s.Loai_Tuyen.Value,
                    Username = s.Username,
                    Password = s.Password,
                    MaxFilePerDate = s.MaxFilePerDate,
                    SuperHospitalId = s.SuperHospital?.Name,
                    HospitalCons = string.Join(',', s.HospitalCons?.Select(x => x.Name).ToList()),
                    LevelId = s.LevelId
                }).ToList();
                res.Data.TotalItems = items.TotalItems;
                #endregion

                // write log to db
                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_GET_FILTER_LIST);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, new PaginationResponseModel<HospitalInfoModel>(), _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetItems)}");
            }
            return res;
        }
        public async Task<ResponseModel<ModifyHospitalModel>> GetCurrent()
        {
            var res = new ResponseModel<ModifyHospitalModel>(HttpStatusCode.OK, "", new ModifyHospitalModel());

            try
            {
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;
                #region Validation
                if (string.IsNullOrWhiteSpace(currentMaCSKCB))
                {
                    res.Code = HttpStatusCode.Forbidden;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    return res;
                }
                #endregion

                var item = await _hospitalRepository.GetItem(
                    x => x.MA_CSKCB.Equals(currentMaCSKCB),
                    y => y.Include(t => t.SuperHospital));

                var itemData = _mapper.Map<ModifyHospitalModel>(item);
                itemData.SuperHospitalId = item.SuperHospital?.MA_CSKCB;
                res.Data = itemData;
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, new ModifyHospitalModel(), _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetCurrent)}");
            }

            return res;
        }

        public async Task<ResponseModel<ModifyHospitalModel>> GetItemById(string id)
        {
            var res = new ResponseModel<ModifyHospitalModel>(HttpStatusCode.OK, "", new ModifyHospitalModel());

            try
            {
                #region Validation
                if (string.IsNullOrWhiteSpace(id))
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    return res;
                }
                #endregion

                var item = await _hospitalRepository.GetItem(
                    x => x.Id.Equals(id),
                    y => y.Include(t => t.SuperHospital));

                var itemData = _mapper.Map<ModifyHospitalModel>(item);
                itemData.SuperHospitalId = item.SuperHospital?.MA_CSKCB;
                res.Data = itemData;
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, new ModifyHospitalModel(), _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetItemById)}");
            }

            return res;
        }

        public async Task<ResponseModel<bool>> CreateNewItem(ModifyHospitalModel request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;

                #region Validtion
                res = ValidateModifyItem(request, currentRole);
                if (res.Data == false) return res;

                res = await CheckHospitalNameExist(request.MA_CSKCB, request.Name);
                if (res.Data == false) return res;
                #endregion

                request.Name = request.Name.Trim();
                var savedModel = _mapper.Map<Hospital>(request);

                await _hospitalRepository.InsertAsBaseEntityAsync(savedModel);

                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_CREATE_NEW);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.CreateNewItem)}");
            }
            return res;
        }

        public async Task<ResponseModel<bool>> UpdateItem(ModifyHospitalModel request)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                Hospital item;
                #region Validtion
                res = ValidateModifyItem(request, currentRole);
                if (res.Data == false) return res;


                item = await _hospitalRepository.GetItem(
                    x => !string.IsNullOrWhiteSpace(request.Id) && x.Id.Equals(request.Id), null);
                if (item == null)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    res.Data = false;
                    return res;
                }
                #endregion
                Hospital itemId = await _hospitalRepository.GetItem(
                    x => x.MA_CSKCB== request.SuperHospitalId);
                if (itemId != null) { item.SuperHospitalId = itemId.Id; }

                item.Name = request.Name?.Trim();
                item.IsActivated = request.IsActivated;
                item.Address = request.Address;
                item.Username = request.Username;
                item.Password = request.Password;
                item.MA_CSKCB = request.MA_CSKCB;
                item.Loai_Tuyen = request.Loai_Tuyen;
                item.MaxFilePerDate = request.MaxFilePerDate;
                item.OfficeTime = request.OfficeTime;
                item.LevelId = request.LevelId;

                _hospitalRepository.UpdateAsBaseEntity(item);

                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_UPDATE);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.CreateNewItem)}");
            }
            return res;
        }

        //Linhist added 14/08/24
        public async Task<ResponseModel<bool>> UpdateForVerify(Hospital hospital)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                _hospitalRepository.UpdateAsBaseEntity(hospital);

                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_UPDATE);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.CreateNewItem)}");
            }
            return res;
        }

        private ResponseModel<bool> ValidateModifyItem(ModifyHospitalModel request, string currentRole)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

            var isMaster = RoleConstant.MASTER.Equals(currentRole);
            var isBoYT = "BOYTE".Equals(currentMaCSKCB);
            var isLocalAdmin = RoleConstant.ADMIN.Equals(currentRole) && currentMaCSKCB?.Equals(request?.MA_CSKCB) == true;
            
            if (!(isMaster || isBoYT || isLocalAdmin))
            {
                res.Code = HttpStatusCode.Forbidden;
                res.Message = MessageResource.ERR_403_FORBIDDEN;
                res.Data = false;
                return res;
            }
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_REQUIRED_EMAIL_PASSWORD;
                res.Data = false;
                return res;
            }
            return res;
        }

        //Linhist changed 20/02/25
        //private async Task<ResponseModel<bool>> CheckHospitalNameExist(string name)
        private async Task<ResponseModel<bool>> CheckHospitalNameExist(string maCSKCB, string name)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            //Linhist changed 20/02/25
            //var isExist = await _hospitalRepository.AnyAsync(x => x.Name.ToLower().Equals(name.Trim().ToLower()));
            var isExist = await _hospitalRepository.AnyAsync(x => x.MA_CSKCB == maCSKCB || x.Name.ToLower().Equals(name.Trim().ToLower()));
            if (isExist)
            {
                res.Code = HttpStatusCode.BadRequest;
                res.Message = MessageResource.ERR_400_EXIST_HOSPITAL;
                res.Data = false;
                return res;
            }
            return res;
        }

        public async Task<ResponseModel<bool>> DeleteItem(string id)
        {
            var res = new ResponseModel<bool>(HttpStatusCode.OK, "", true);
            try
            {
                Hospital item;
                #region Validtion

                item = await _hospitalRepository.GetById(id);
                if (item == null)
                {
                    res.Code = HttpStatusCode.BadRequest;
                    res.Message = MessageResource.ERR_400_INVALID_ID;
                    res.Data = false;
                    return res;
                }
                #endregion

                item.IsActivated = false;
                item.IsDeleted = true;
                _hospitalRepository.UpdateAsBaseEntity(item);

                await _unitOfWork.CommitAsync();

                // write log to db
                await _sysHistoryLogService.AddSysLog(Constant.HOSPITAL_SOFT_DELETE);
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError(
                    ex, false, _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.CreateNewItem)}");
            }
            return res;
        }

        public async Task<bool> HasIncludedChild(string id, string maCSKCB)
        {
            var item = await _hospitalRepository.GetItem(
                x => x.Id.Equals(id),
                y => y.Include(t => t.SuperHospital));

            foreach (var h in item.HospitalCons)
            {
                if (h.MA_CSKCB == maCSKCB) return true;
            }

            return false;
        }
        public async Task<Dictionary<string, int>> CountFilesToday()
        {
            try
            {
                return await _xml1Repository.CountTotalUploads(DateTime.Now.Date);
            }
            catch //(Exception ex)
            { }

            return null;
        }

        public async Task<ResponseModel<TotalizeReport>> CountFileUploaded(DateTime createDate)
        {
            var res = new ResponseModel<TotalizeReport>(HttpStatusCode.OK, "", new TotalizeReport()
            {
                SelectedDate = createDate,
            });

            try
            {
                var currentRole = _httpContext.HttpContext.User.FindFirst(ClaimTypes.Role).Value;
                var currentMaCSKCB = _httpContext.HttpContext.User.FindFirst(ClaimKey.MA_CSKCB).Value;

                if (!currentRole.Contains(RoleConstant.MASTER))
                {
                    res.Code = HttpStatusCode.Forbidden;
                    res.Message = MessageResource.ERR_403_FORBIDDEN;
                    res.Data = null;
                    return res;
                }

                var hospitals = await _hospitalRepository.GetListAsync(h => !string.IsNullOrEmpty(h.MA_CSKCB) && h.MA_CSKCB.ToLower() != "boyte");
                var totals = await _xml1Repository.CountTotalUploads(createDate);
                foreach (var h in hospitals)
                {
                    var item = new TotalizeItem()
                    {
                        Hospital = h.Name,
                        MaCSKCB = h.MA_CSKCB,
                        TotalFiles = 0,
                    };
                    res.Data.Add(item);

                    if (totals.ContainsKey(item.MaCSKCB))
                    {
                        item.TotalFiles = totals[item.MaCSKCB];
                    }
                }
            }
            catch (Exception ex)
            {
                res.Message += ex.ToString();
                res.Code = HttpStatusCode.BadRequest;
                res.Data = null;
            }

            return res;
        }

        public async Task<int> RemoveDublicated(string maCSKCB, DateTime tuNgay, DateTime denNgay)
        {
            var parameters = new Microsoft.Data.SqlClient.SqlParameter[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("MA_CSKCB", maCSKCB),
                new Microsoft.Data.SqlClient.SqlParameter("TU_NGAY", tuNgay),
                new Microsoft.Data.SqlClient.SqlParameter("DEN_NGAY", denNgay)
            };
            return await _hospitalRepository.CallProcedure("EXEC [dbo].[XMLDubRemove] @p0, @p1, @p2", new object[] { maCSKCB, tuNgay, denNgay });
        }

        //public async Task<int> RemoveDublicated130(string maCSKCB, DateTime tuNgay, DateTime denNgay)
        public async Task<int> RemoveDublicated130(string maCSKCB)
        {
            //var parameters = new SqlParameter[]
            //{
            //    new SqlParameter("MA_CSKCB", maCSKCB),
                
            //};
            //6.20.2025 haimaster update ham xóa trùng
            int updatecount = 0;
            try
            {


                //var rescount = await _hospitalRepository.CallProcedure("EXEC [dbo].[MarkDuplicateCount]   @p0", new object[] { maCSKCB });
                //var rescount = await _hospitalRepository.CallProcedure(
                //    "EXEC [dbo].[MarkDuplicateCount] @p0",
                //    new Microsoft.Data.SqlClient.SqlParameter("@p0", SqlDbType.VarChar, 10) { Value = maCSKCB }
                //);


                //Uti.HandleCronJobLog(_logger, $"Số bản ghi trùng cho mã CSKCB {maCSKCB} là: {rescount}", nameof(HospitalManagement));
                
                //if (rescount > 0)
                //{
                    updatecount = await _hospitalRepository.CallProcedure("EXEC [dbo].[XML130DubRemove] @p0", new object[] { maCSKCB });
                    

                Uti.HandleCronJobLog(_logger, $"Số bản ghi trùng bị xóa cho mã CSKCB {maCSKCB} đã xóa trùng:{updatecount}", nameof(HospitalManagement));

                //}
            }
            catch(Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Lỗi khi xóa trùng trùng cho mã CSKCB {maCSKCB} là: {ex.ToString()}", nameof(HospitalManagement));
            }
            

            return updatecount;

        }

        //Linhist added 190826
        public async Task<int> RemoveDubSingle(string maCSKCB, string maLK)
        {
            int updatecount = 0;
            try
            {
                updatecount = await _hospitalRepository.CallProcedure("EXEC [dbo].[XML130DubRemove_Single] @p0, @p1", new object[] { maCSKCB, maLK });
                Uti.HandleCronJobLog(_logger, $"Số bản ghi trùng bị xóa cho mã CSKCB {maCSKCB}, mã LK {maLK} đã xóa trùng:{updatecount}", nameof(HospitalManagement));
            }
            catch (Exception ex)
            {
                Uti.HandleCronJobLog(_logger, $"Lỗi khi xóa trùng trùng cho mã CSKCB {maCSKCB}, mã LK {maLK} là: {ex.ToString()}", nameof(HospitalManagement));
            }

            return updatecount;
        }

        public async Task<int> UpdateCategoryPrice(string maCSKCB, int hangBV)
        {
            var parameters = new Microsoft.Data.SqlClient.SqlParameter[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("MA_CSKCB", maCSKCB),
                new Microsoft.Data.SqlClient.SqlParameter("HANG_BV", hangBV)
            };
            return await _hospitalRepository.CallProcedure("EXEC [dbo].[Cap_Nhat_Gia_Kham_Giuong] @p0, @p1", new object[] { maCSKCB, hangBV });
        }

        //Linhist added 17/07/26
        public async Task<ResponseModel<IEnumerable<HospitalAIInfo>>> GetAll()
        {
            var res = new ResponseModel<IEnumerable<HospitalAIInfo>>(HttpStatusCode.OK, "", new List<HospitalAIInfo>());
            try
            {
                var hps = await _hospitalRepository.GetListAsync(x => x.MA_CSKCB != null && x.MA_CSKCB.Length == 5, o => o.OrderBy(o1 => o1.MA_CSKCB));
                if (hps == null || !hps.Any())
                {
                    res.Code = HttpStatusCode.NotFound;
                    res.Message = "Không có thông tin bệnh viện";
                    res.Data = null;
                    return res;
                }

                res.Data = hps.Select(e => new HospitalAIInfo(e)).ToList();
            }
            catch (Exception ex)
            {
                res = Uti.HandleInternalServerError<IEnumerable<HospitalAIInfo>>(
                    ex, null, _logger, $"{nameof(HospitalService)} - {nameof(HospitalService.GetActiveHospitals)}");
            }

            return res;
        }
    }
}