namespace HIA.Data.Auths;

public partial class User
{
    public User()
    {
        UserRoles = new HashSet<UserRole>();
    }

    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool IsFirstTimeLogin { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? HospitalId { get; set; }
    public bool IsActivated { get; set; }
    public bool IsDeleted { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public virtual Hospital? Hospital { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}
