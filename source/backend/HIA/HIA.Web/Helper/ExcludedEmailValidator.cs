namespace HIA.Web.Helper;

/// <summary>
/// Danh sach email bi loai tru dang nhap, doc tu cau hinh AppSettings:ExcludedEmails.
/// </summary>
public sealed class ExcludedEmailValidator
{
    private readonly HashSet<string> _excludedEmails;

    public ExcludedEmailValidator(IEnumerable<string> excludedEmails)
    {
        _excludedEmails = new HashSet<string>(excludedEmails, StringComparer.Ordinal);
    }

    public bool IsExcluded(string? email) => email is not null && _excludedEmails.Contains(email);
}
