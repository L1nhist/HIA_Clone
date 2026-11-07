namespace HIA.Core.Base;

/// <summary>
/// Danh sach email bi loai tru dang nhap, doc tu cau hinh AppSettings:ExcludedEmails.
/// So sanh khong phan biet hoa thuong, cat khoang trang hai dau.
/// </summary>
public sealed class ExcludedEmailValidator
{
    private readonly HashSet<string> _excludedEmails;

    public ExcludedEmailValidator(IEnumerable<string> excludedEmails)
    {
        _excludedEmails = new HashSet<string>(
            excludedEmails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()),
            StringComparer.OrdinalIgnoreCase);
    }

    public bool IsExcluded(string? email)
    {
        var value = email?.Trim();
        return !string.IsNullOrEmpty(value) && _excludedEmails.Contains(value);
    }
}
