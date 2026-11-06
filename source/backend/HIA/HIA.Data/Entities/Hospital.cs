

namespace HIA.Data.Entities
{
    public partial class Hospital
    {
        public Hospital()
        {
            Users = new HashSet<User>();
            //XML1_APIs = new HashSet<XML1_API>();
            //XML1s = new HashSet<XML1>();
            HospitalCons = new HashSet<Hospital>();
        }

        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public bool IsActivated { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Address { get; set; }
        public string? MA_CSKCB { get; set; }
        public int? Loai_Tuyen { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? AccessToken { get; set; }
        public string? IdToken { get; set; }
        public DateTime? DateTimeNow { get; set; }
        public int? FileAPICount { get; set; }
        public int? LevelId { get; set; }
        
        public int? MaxFilePerDate { get; set; }
        public string? OfficeTime { get; set; }

        public string? SuperHospitalId { get; set; }

        public Hospital? SuperHospital { get; set; }
        public virtual ICollection<User> Users { get; set; }
        //public virtual ICollection<XML1_API> XML1_APIs { get; set; }
        //public virtual ICollection<XML1> XML1s { get; set; }

        public virtual ICollection<Hospital> HospitalCons { get; set; }
    }
}
