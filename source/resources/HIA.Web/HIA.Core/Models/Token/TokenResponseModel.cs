using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace HIA.Core.Models.Token
{
    public class TokenResponseModel
    {
        public string Token { get; set; }
        public List<Claim> ClaimList { get; set; }
        public DateTime ExpireAt { get; set; }
    }

    public class TokenRequestModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string FullName { get; set; }
        public string HospitalId { get; set; }
        public string HospitalName { get; set; }
        public string MA_CSKCB { get; set; }
        public string Type { get; set; }

        public DateTime ExpireAt { get; set; }
    }

    public class TokenResultModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string HospitalName { get; set; }
        public string MA_CSKCB { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
