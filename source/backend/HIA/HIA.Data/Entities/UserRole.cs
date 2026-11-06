

namespace HIA.Data.Entities
{
    public partial class UserRole
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;

        public virtual Role Role { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
