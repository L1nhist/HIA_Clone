namespace HIA.Services.SysHistoryLogManagement;

public interface ISysHistoryLogService
{
    Task AddSysLog(string functionName, string email = "");
}
