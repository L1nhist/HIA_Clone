using System;

#nullable disable

namespace HIA.DatabaseModel.Entites
{
    public partial class UserLogs
    {
        public long LogID { get; set; }
        public string UserID { get; set; }
        public string Email { get; set; }
        public string MA_CSKCB { get; set; }
        public string Action { get; set; }
        public string UrlAccess { get; set; }
        public DateTime LogTime { get; set; }
    }
}