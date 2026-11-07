using System.Globalization;

namespace HIA.Core.Base;

/// <summary>
/// Dinh dang ngay trong file Excel (xuat/nhap). Khong phu thuoc NPOI: o ngay goc cua Excel
/// duoc doi thanh DateTime o tang Services roi dung <see cref="Format"/>.
/// </summary>
public static class ExcelDate
{
    /// <summary>Dinh dang xuat ra, invariant: "HH:mm:ss dd/MM/yyyy".</summary>
    public const string OutputFormat = "HH:mm:ss dd/MM/yyyy";

    /// <summary>
    /// Cac dinh dang nhap, theo thu tu uu tien khi mo ho (TryParseExact lay dinh dang dau tien khop):
    /// dd/MM truoc MM/dd; yyyyMMdd truoc ddMMyyyy (vi du "11072026" -> 11/07/2026 vi yyyyMMdd khong hop le).
    /// Muc 1-9: dinh dang PO. Muc 10-12 (ISO, uu tien thap nhat): chi de doc du lieu import cu, khong dung de xuat.
    /// </summary>
    private static readonly string[] ParseFormats =
    {
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "HH:mm dd/MM/yyyy",
        "HH:mm:ss dd/MM/yyyy",
        "HH:mm MM/dd/yyyy",
        "HH:mm:ss MM/dd/yyyy",
        "yyyyMMdd",
        "ddMMyyyy",
        "HHmmddMMyyyy",
        "yyyy-MM-dd",
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
    };

    public static string Format(DateTime value) => value.ToString(OutputFormat, CultureInfo.InvariantCulture);

    /// <summary>Tra false (khong nem ngoai le) khi text rong hoac khong khop dinh dang nao.</summary>
    public static bool TryParse(string? text, out DateTime value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return DateTime.TryParseExact(text.Trim(), ParseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }
}
