using System;
using System.Collections.Generic;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class SysHistoryLog
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string HospitalId { get; set; }
        public string HospitalName { get; set; }
        public string IpAddress { get; set; }
        public string BrowserOsInfo { get; set; }
        public string FunctionName { get; set; }
        public DateTime? ExecutionTime { get; set; }
    }
}
