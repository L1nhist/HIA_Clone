

namespace HIA.Data.Entities
{
    public partial class SysHistoryLog
    {
        public string Id { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? HospitalId { get; set; }
        public string? HospitalName { get; set; }
        public string? IpAddress { get; set; }
        public string? BrowserOsInfo { get; set; }
        public string? FunctionName { get; set; }
        public DateTime? ExecutionTime { get; set; }
    }
}
