namespace HIA.Data.Auths;

public partial class Role
{
    public Role()
    {
        UserRoles = new HashSet<UserRole>();
    }

    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActivated { get; set; }
    public bool IsDeleted { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }
}
