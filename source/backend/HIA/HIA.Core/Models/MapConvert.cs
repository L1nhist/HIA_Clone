namespace HIA.Core.Models;

// Chuyen kieu dung chung cho MapFrom/MapTo; chuoi khong parse duoc -> null.
internal static class MapConvert
{
    public static DateTime? ToDate(string? value) => DateTime.TryParse(value, out var d) ? d : null;

    public static byte? ToByte(string? value) => byte.TryParse(value, out var b) ? b : null;

    public static int? ToInt(string? value) => int.TryParse(value, out var i) ? i : null;
}
