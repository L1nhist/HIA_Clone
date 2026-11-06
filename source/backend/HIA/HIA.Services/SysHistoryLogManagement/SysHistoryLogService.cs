using HIA.Core.Models.Token;
using HIA.Data.Reports;
using HIA.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

namespace HIA.Services.SysHistoryLogManagement;

public class SysHistoryLogService : ISysHistoryLogService
{
    private readonly IBaseRepository<SysHistoryLog> _sysHistoryLogRepository;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<SysHistoryLogService> _logger;

    public SysHistoryLogService(
        IBaseRepository<SysHistoryLog> sysHistoryLogRepository,
        IHttpContextAccessor httpContext,
        ILogger<SysHistoryLogService> logger
        )
    {
        _sysHistoryLogRepository = sysHistoryLogRepository;
        _httpContext = httpContext;
        _logger = logger;
    }

    public async Task AddSysLog(string functionName, string email = "")
    {
        try
        {
            var context = _httpContext.HttpContext ?? throw new InvalidOperationException("No HttpContext");
            var ipArray = Dns.GetHostAddresses(Dns.GetHostName())
                                .Where(x => x.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork))
                                .Select(x => Convert.ToString(x))
                                .ToArray();
            await _sysHistoryLogRepository.InsertAsync(new SysHistoryLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = context.User.FindFirst(ClaimTypes.Sid)?.Value ?? "",
                Email = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? email,
                HospitalId = context.User.FindFirst(ClaimKey.HOSPITAL_ID)?.Value ?? "",
                HospitalName = context.User.FindFirst(ClaimKey.HOSPITAL_NAME)?.Value ?? "",
                IpAddress = ipArray.Length > 0 ? string.Join(",", ipArray) : "",
                BrowserOsInfo = context.Request.Headers["User-Agent"].ToString(),
                FunctionName = functionName,
                ExecutionTime = DateTime.Now,
            });
            await _sysHistoryLogRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(SysHistoryLogService)} - CANNOT write log func to DB: {ex.Message}");
        }
    }
}
